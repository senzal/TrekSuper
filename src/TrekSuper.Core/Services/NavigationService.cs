using TrekSuper.Core.Enums;
using TrekSuper.Core.Models;
using TrekSuper.Core.Models.Entities;

namespace TrekSuper.Core.Services;

/// <summary>
/// Handles ship movement and navigation.
/// </summary>
public class NavigationService
{
    private readonly GameEngine _engine;

    public NavigationService(GameEngine engine)
    {
        _engine = engine;
    }

    private GameState State => _engine.State;
    private Random Random => State.Random;

    /// <summary>
    /// Move using warp drive.
    /// </summary>
    public bool Warp(double direction, double distance)
    {
        var ship = State.Ship;

        if (!ship.IsDeviceOperational(DeviceType.WarpEngines))
        {
            _engine.Error("Warp engines are damaged.");

            // Can we limp along?
            if (ship.Devices.GetDamage(DeviceType.WarpEngines) > 2.0)
            {
                _engine.Error("Cannot move - warp engines are too heavily damaged.");
                return false;
            }

            if (ship.WarpFactor > 4.0)
            {
                _engine.Warning("Maximum warp reduced to 4.0 due to damage.");
                ship.WarpFactor = 4.0;
            }
        }

        // Calculate energy required
        double power = (distance + 0.05) * ship.WarpFactorSquared;

        if (power >= ship.Energy)
        {
            _engine.Error($"Insufficient energy. Available: {ship.Energy:F2}, Required: {power:F2}");

            // Can we lower warp factor?
            double maxWarp = Math.Sqrt(ship.Energy / (distance + 0.05));
            if (maxWarp >= 1.0)
            {
                _engine.Message($"Maximum achievable warp: {maxWarp:F2}");
            }
            return false;
        }

        // Check trajectory for obstacles BEFORE moving
        var obstacles = CheckTrajectory(direction, distance);
        if (obstacles.Count > 0)
        {
            _engine.Warning("*** NAVIGATION ALERT ***");
            _engine.Warning($"Projected course intercepts {obstacles.Count} object(s):");
            foreach (var (obstacleType, sector, distance_to) in obstacles)
            {
                string warning = obstacleType switch
                {
                    "Star" => $"  ⚠️  STAR at {sector} (distance {distance_to:F1}) - COLLISION WILL DESTROY SHIP!",
                    "BlackHole" => $"  ⚠️  BLACK HOLE at {sector} (distance {distance_to:F1}) - WILL DESTROY SHIP!",
                    "Enemy" => $"  ⚠️  Enemy at {sector} (distance {distance_to:F1}) - collision will cause damage",
                    "Starbase" => $"  ⚠️  Starbase at {sector} (distance {distance_to:F1}) - collision will cause damage",
                    _ => $"  ⚠️  Object at {sector} (distance {distance_to:F1})"
                };
                _engine.Warning(warning);
            }
            _engine.Error("Course aborted for safety. Adjust direction or distance.");
            return false;
        }

        // Calculate time
        double time = 10.0 * distance / ship.WarpFactorSquared;

        // Perform movement
        return Move(direction, distance, power, time, isWarp: true);
    }

    /// <summary>
    /// Move using impulse engines.
    /// </summary>
    public bool Impulse(double direction, double distance)
    {
        var ship = State.Ship;

        if (!ship.IsDeviceOperational(DeviceType.ImpulseEngines))
        {
            _engine.Error("Impulse engines are damaged and inoperative.");
            return false;
        }

        // Impulse is limited to intra-quadrant movement
        if (distance > 1.0)
        {
            _engine.Warning("Impulse engines limited to short range movement.");
            _engine.Warning("Maximum distance at impulse: 1.0 quadrant.");
            distance = 1.0;
        }

        // Calculate energy (lower than warp)
        double power = 20.0 + 100.0 * distance;

        if (power >= ship.Energy)
        {
            _engine.Error($"Insufficient energy. Available: {ship.Energy:F2}, Required: {power:F2}");
            return false;
        }

        // Check trajectory for obstacles BEFORE moving
        var obstacles = CheckTrajectory(direction, distance);
        if (obstacles.Count > 0)
        {
            _engine.Warning("*** NAVIGATION ALERT ***");
            _engine.Warning($"Projected course intercepts {obstacles.Count} object(s):");
            foreach (var (obstacleType, sector, distance_to) in obstacles)
            {
                string warning = obstacleType switch
                {
                    "Star" => $"  ⚠️  STAR at {sector} (distance {distance_to:F1}) - COLLISION WILL DESTROY SHIP!",
                    "BlackHole" => $"  ⚠️  BLACK HOLE at {sector} (distance {distance_to:F1}) - WILL DESTROY SHIP!",
                    "Enemy" => $"  ⚠️  Enemy at {sector} (distance {distance_to:F1}) - collision will cause damage",
                    "Starbase" => $"  ⚠️  Starbase at {sector} (distance {distance_to:F1}) - collision will cause damage",
                    _ => $"  ⚠️  Object at {sector} (distance {distance_to:F1})"
                };
                _engine.Warning(warning);
            }
            _engine.Error("Course aborted for safety. Adjust direction or distance.");
            return false;
        }

        // Impulse is slower
        double time = distance / 0.095;

        return Move(direction, distance, power, time, isWarp: false);
    }

    private List<(string Type, SectorCoordinate Sector, double Distance)> CheckTrajectory(double direction, double distance)
    {
        var obstacles = new List<(string, SectorCoordinate, double)>();
        var ship = State.Ship;

        // Calculate trajectory
        double angle = (15.0 - direction) * Math.PI / 8.0;
        double dx = -Math.Sin(angle);
        double dy = Math.Cos(angle);

        // Scale to sectors
        double totalSectors = distance * 10.0;

        double x = ship.Sector.X;
        double y = ship.Sector.Y;
        double distanceMoved = 0;
        double stepSize = 0.1;

        while (distanceMoved < totalSectors)
        {
            x += dx * stepSize;
            y += dy * stepSize;
            distanceMoved += stepSize;

            int checkX = (int)Math.Round(x);
            int checkY = (int)Math.Round(y);

            if (checkX >= 1 && checkX <= 10 && checkY >= 1 && checkY <= 10)
            {
                var sector = new SectorCoordinate(checkX, checkY);

                if (sector != ship.Sector)
                {
                    var entity = State.CurrentQuadrant?.GetEntityAt(sector);

                    if (entity != null)
                    {
                        string entityType = entity switch
                        {
                            Star => "Star",
                            BlackHole => "BlackHole",
                            Enemy => "Enemy",
                            Starbase => "Starbase",
                            _ => "Object"
                        };

                        // Check if we already reported this obstacle
                        if (!obstacles.Any(o => o.Item2 == sector))
                        {
                            obstacles.Add((entityType, sector, distanceMoved / 10.0));
                        }
                    }
                }
            }
        }

        return obstacles;
    }

    private bool Move(double direction, double distance, double power, double time, bool isWarp)
    {
        var ship = State.Ship;

        // Use energy
        ship.Energy -= power;
        State.ActionTaken = true;

        // Calculate trajectory
        double angle = (15.0 - direction) * Math.PI / 8.0;
        double dx = -Math.Sin(angle);
        double dy = Math.Cos(angle);

        // Scale to sectors (10 sectors per quadrant)
        double totalSectors = distance * 10.0;

        // Current position in continuous coordinates
        double x = ship.Sector.X;
        double y = ship.Sector.Y;
        int qx = ship.Quadrant.X;
        int qy = ship.Quadrant.Y;

        // Step through movement
        double distanceMoved = 0;
        double stepSize = 0.1; // Check every 0.1 sectors

        while (distanceMoved < totalSectors)
        {
            x += dx * stepSize;
            y += dy * stepSize;
            distanceMoved += stepSize;

            // Check for quadrant boundary crossing
            bool changedQuadrant = false;

            while (x < 0.5)
            {
                x += 10.0;
                qx--;
                changedQuadrant = true;
            }
            while (x > 10.5)
            {
                x -= 10.0;
                qx++;
                changedQuadrant = true;
            }
            while (y < 0.5)
            {
                y += 10.0;
                qy--;
                changedQuadrant = true;
            }
            while (y > 10.5)
            {
                y -= 10.0;
                qy++;
                changedQuadrant = true;
            }

            // Check for galaxy boundary
            if (qx < 1 || qx > 8 || qy < 1 || qy > 8)
            {
                _engine.Warning("*** LEAVING GALAXY - NEGATIVE ENERGY BARRIER ***");
                _engine.Warning("Ship reflected back into galaxy.");

                // Bounce back
                if (qx < 1) { qx = 1; x = 1; dx = -dx; }
                if (qx > 8) { qx = 8; x = 10; dx = -dx; }
                if (qy < 1) { qy = 1; y = 1; dy = -dy; }
                if (qy > 8) { qy = 8; y = 10; dy = -dy; }
            }

            // If we changed quadrants, enter new quadrant
            if (changedQuadrant)
            {
                int ix = (int)Math.Round(x);
                int iy = (int)Math.Round(y);
                ix = Math.Clamp(ix, 1, 10);
                iy = Math.Clamp(iy, 1, 10);

                ship.Sector = new SectorCoordinate(ix, iy);
                _engine.EnterQuadrant(new QuadrantCoordinate(qx, qy));

                // Advance time for remaining distance
                State.AdvanceTime(time * (totalSectors - distanceMoved) / totalSectors);
                return true;
            }

            // Check for collision
            int checkX = (int)Math.Round(x);
            int checkY = (int)Math.Round(y);

            if (checkX >= 1 && checkX <= 10 && checkY >= 1 && checkY <= 10)
            {
                var sector = new SectorCoordinate(checkX, checkY);

                // Don't collide with ourselves
                if (sector != ship.Sector)
                {
                    var entity = State.CurrentQuadrant?.GetEntityAt(sector);

                    if (entity != null)
                    {
                        return HandleCollision(entity, sector, x, y, dx, dy);
                    }

                    // Check Tholian web
                    if (State.CurrentQuadrant?.WebPositions.Contains(sector) == true)
                    {
                        _engine.Warning($"Ship intersects Tholian web at {sector}!");
                        // Take some damage
                        double webDamage = 50 + 50 * Random.NextDouble();
                        ship.Energy -= webDamage;
                        _engine.Warning($"Web causes {webDamage:F2} units damage.");
                    }
                }
            }
        }

        // Final position
        int finalX = (int)Math.Round(x);
        int finalY = (int)Math.Round(y);
        finalX = Math.Clamp(finalX, 1, 10);
        finalY = Math.Clamp(finalY, 1, 10);

        // Make sure final position is empty
        var finalSector = new SectorCoordinate(finalX, finalY);
        if (State.CurrentQuadrant?.IsSectorEmpty(finalSector) == false && finalSector != ship.Sector)
        {
            // Find nearest empty sector
            finalSector = FindNearestEmptySector(finalX, finalY) ?? ship.Sector;
        }

        ship.Sector = finalSector;
        ship.IsDocked = false;

        // Advance time
        State.AdvanceTime(time);

        // Update distances
        _engine.SortEnemiesByDistance();
        _engine.UpdateCondition();

        return true;
    }

    private bool HandleCollision(Entity entity, SectorCoordinate sector, double x, double y, double dx, double dy)
    {
        switch (entity)
        {
            case Star:
                _engine.Error($"*** COLLISION WITH STAR AT {sector}! ***");
                _engine.Error("Ship destroyed!");
                State.EndGame(GameOutcome.Supernova);
                return false;

            case BlackHole:
                _engine.Error($"*** SHIP FALLS INTO BLACK HOLE AT {sector}! ***");
                State.EndGame(GameOutcome.BlackHole);
                return false;

            case Enemy enemy:
                _engine.Error($"*** COLLISION WITH {enemy.Type.GetDisplayName()} AT {sector}! ***");

                // Ramming damage
                double damage = 400 + 200 * Random.NextDouble();
                State.Ship.Energy -= damage;
                enemy.Power -= damage * 2; // We do more damage to them

                _engine.Warning($"Ship takes {damage:F2} damage from collision.");

                if (enemy.IsDestroyed)
                {
                    _engine.Message($"{enemy.Type.GetDisplayName()} destroyed in collision!");
                }

                if (State.Ship.Energy <= 0)
                {
                    State.EndGame(GameOutcome.KilledInBattle);
                    return false;
                }

                // Bounce back
                int newX = (int)Math.Round(x - dx * 2);
                int newY = (int)Math.Round(y - dy * 2);
                newX = Math.Clamp(newX, 1, 10);
                newY = Math.Clamp(newY, 1, 10);
                State.Ship.Sector = new SectorCoordinate(newX, newY);
                return true;

            case Starbase:
                // Dock automatically
                _engine.Message($"Ship docked at starbase in sector {sector}.");
                DockAtStarbase();
                return true;

            default:
                // Stop before the obstacle
                int stopX = (int)Math.Round(x - dx);
                int stopY = (int)Math.Round(y - dy);
                stopX = Math.Clamp(stopX, 1, 10);
                stopY = Math.Clamp(stopY, 1, 10);
                State.Ship.Sector = new SectorCoordinate(stopX, stopY);
                return true;
        }
    }

    private SectorCoordinate? FindNearestEmptySector(int x, int y)
    {
        for (int r = 1; r <= 5; r++)
        {
            for (int dx = -r; dx <= r; dx++)
            {
                for (int dy = -r; dy <= r; dy++)
                {
                    var sector = new SectorCoordinate(x + dx, y + dy);
                    if (sector.IsValid && State.CurrentQuadrant?.IsSectorEmpty(sector) == true)
                    {
                        return sector;
                    }
                }
            }
        }
        return null;
    }

    /// <summary>
    /// Dock at a starbase.
    /// </summary>
    public bool Dock()
    {
        if (State.CurrentQuadrant?.Starbase == null)
        {
            _engine.Error("No starbase in this quadrant.");
            return false;
        }

        var baseSector = State.CurrentQuadrant.Starbase.Position;
        double distance = State.Ship.Sector.DistanceTo(baseSector);

        if (distance > 1.5)
        {
            _engine.Error($"Starbase at {baseSector} is too far to dock. Move closer.");
            return false;
        }

        return DockAtStarbase();
    }

    private bool DockAtStarbase()
    {
        _engine.Message("Ship docked at starbase.");
        _engine.Message("Resupplying and repairing...");

        State.Ship.Resupply();
        State.Ship.IsDocked = true;
        State.Ship.Condition = Condition.Docked;

        _engine.Message($"  Energy: {State.Ship.Energy:F0}");
        _engine.Message($"  Shields: {State.Ship.Shield:F0}");
        _engine.Message($"  Torpedoes: {State.Ship.Torpedoes}");
        _engine.Message($"  Probes: {State.Ship.Probes}");
        _engine.Message("  All damage repaired.");

        return true;
    }

    /// <summary>
    /// Set warp factor.
    /// </summary>
    public void SetWarpFactor(double warp)
    {
        if (warp < 1.0 || warp > 10.0)
        {
            _engine.Error("Warp factor must be between 1.0 and 10.0");
            return;
        }

        if (warp > 8.0 && State.Ship.IsDeviceOperational(DeviceType.WarpEngines))
        {
            _engine.Warning("Warp factors above 8.0 risk damaging the engines.");
        }

        if (!State.Ship.IsDeviceOperational(DeviceType.WarpEngines) && warp > 4.0)
        {
            _engine.Error("Damaged warp engines limit warp to 4.0");
            warp = 4.0;
        }

        State.Ship.WarpFactor = warp;
        _engine.Message($"Warp factor set to {warp:F1}");
    }

    /// <summary>
    /// Rest to allow time to pass (for repairs).
    /// </summary>
    public void Rest(double time)
    {
        if (State.CurrentQuadrant?.EnemyCount > 0 && !State.Ship.IsDocked)
        {
            _engine.Warning("You cannot rest with enemies present unless docked!");
            return;
        }

        _engine.Message($"Resting for {time:F2} stardates...");
        State.IsResting = true;

        // Repair damage over time
        double repairRate = State.Ship.IsDocked ? 1.0 : 0.5;

        foreach (var (device, damage) in State.Ship.Devices.GetDamagedDevices().ToList())
        {
            double repair = time * repairRate;
            State.Ship.Devices.Repair(device, repair);

            if (State.Ship.Devices.IsOperational(device))
            {
                _engine.Message($"  {device.GetDisplayName()} repaired.");
            }
        }

        State.AdvanceTime(time);
        State.IsResting = false;
    }
}
