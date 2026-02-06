using TrekSuper.Core.Enums;
using TrekSuper.Core.Models;
using TrekSuper.Core.Models.Entities;

namespace TrekSuper.Core.Services;

/// <summary>
/// Handles all combat-related operations.
/// </summary>
public class CombatService
{
    private readonly GameEngine _engine;
    private const double PhaserFactor = 2.0;

    public CombatService(GameEngine engine)
    {
        _engine = engine;
    }

    private GameState State => _engine.State;
    private Random Random => State.Random;

    /// <summary>
    /// Fire phasers at enemies.
    /// </summary>
    public bool FirePhasers(double energyUsed, bool automatic = true)
    {
        if (State.CurrentQuadrant == null) return false;

        var ship = State.Ship;
        var enemies = State.CurrentQuadrant.Enemies.Where(e => !e.IsDestroyed).ToList();

        if (enemies.Count == 0)
        {
            _engine.Message("No enemies in this quadrant.");
            return false;
        }

        if (!ship.IsDeviceOperational(DeviceType.Phasers))
        {
            _engine.Error("Phasers damaged and inoperative.");
            return false;
        }

        if (ship.IsCloaked)
        {
            _engine.Error("Cannot fire phasers while cloaked!");
            return false;
        }

        if (energyUsed > ship.Energy)
        {
            _engine.Error($"Insufficient energy. You have {ship.Energy:F2} units.");
            return false;
        }

        // Use energy
        ship.Energy -= energyUsed;
        State.ActionTaken = true;

        // Check if shields are changing (affects phaser efficiency)
        if (ship.ShieldsChanging)
        {
            energyUsed *= 0.5;
            _engine.Warning("Phaser efficiency reduced while shields are changing.");
        }

        // Distribute energy among enemies
        if (automatic)
        {
            AutomaticPhaser(energyUsed, enemies);
        }

        // Remove destroyed enemies
        RemoveDestroyedEnemies();

        return true;
    }

    private void AutomaticPhaser(double energy, List<Enemy> enemies)
    {
        double totalInverseDistance = enemies.Sum(e => 1.0 / e.Distance);

        foreach (var enemy in enemies)
        {
            // Distribute energy inversely proportional to distance
            double portion = (1.0 / enemy.Distance) / totalInverseDistance;
            double energyForEnemy = energy * portion;

            // Calculate damage
            double hit = energyForEnemy * PhaserFactor / enemy.Distance;

            // Add some randomness
            hit *= 0.8 + 0.4 * Random.NextDouble();

            ApplyDamageToEnemy(enemy, hit);
        }
    }

    /// <summary>
    /// Fire a photon torpedo.
    /// </summary>
    public bool FireTorpedo(double direction, double deltaX = 0, double deltaY = 0)
    {
        if (State.CurrentQuadrant == null) return false;

        var ship = State.Ship;

        if (!ship.IsDeviceOperational(DeviceType.PhotonTubes))
        {
            _engine.Error("Photon tubes damaged and inoperative.");
            return false;
        }

        if (ship.Torpedoes <= 0)
        {
            _engine.Error("No photon torpedoes remaining.");
            return false;
        }

        if (ship.IsCloaked)
        {
            _engine.Error("Cannot fire torpedoes while cloaked!");
            return false;
        }

        ship.Torpedoes--;
        State.ActionTaken = true;

        _engine.Message("Torpedo track:");

        // Calculate trajectory
        double angle = (15.0 - direction) * Math.PI / 8.0;
        double dx = -Math.Sin(angle);
        double dy = Math.Cos(angle);

        // Add delta for spread
        dx += deltaX;
        dy += deltaY;

        double x = ship.Sector.X;
        double y = ship.Sector.Y;

        // Track the torpedo
        while (true)
        {
            x += dx;
            y += dy;

            int ix = (int)Math.Round(x);
            int iy = (int)Math.Round(y);

            if (ix < 1 || ix > 10 || iy < 1 || iy > 10)
            {
                _engine.Message($"Torpedo missed - left quadrant at {ix}, {iy}");
                return true;
            }

            _engine.Message($"  {ix} - {iy}");

            var sector = new SectorCoordinate(ix, iy);
            var entity = State.CurrentQuadrant.GetEntityAt(sector);

            if (entity != null)
            {
                return HandleTorpedoHit(entity, sector);
            }

            // Check for Tholian web
            if (State.CurrentQuadrant.WebPositions.Contains(sector))
            {
                _engine.Message($"*** Torpedo absorbed by Tholian web at {sector}! ***");
                State.CurrentQuadrant.WebPositions.Remove(sector);
                return true;
            }
        }
    }

    private bool HandleTorpedoHit(Entity entity, SectorCoordinate sector)
    {
        switch (entity)
        {
            case Enemy enemy:
                _engine.Message($"*** Direct hit on {enemy.Type.GetDisplayName()} at {sector}! ***");
                double damage = 500 + 200 * Random.NextDouble();
                ApplyDamageToEnemy(enemy, damage);
                RemoveDestroyedEnemies();
                return true;

            case Star:
                _engine.Message($"*** Star at {sector} goes nova! ***");
                TriggerNova(sector);
                return true;

            case Starbase:
                _engine.Error($"*** STARBASE AT {sector} DESTROYED! ***");
                DestroyStarbase(sector);
                return true;

            case Planet planet:
                _engine.Warning($"*** Torpedo impacts planet at {sector}! ***");
                if (Random.NextDouble() < 0.5)
                {
                    _engine.Message("Planet destroyed!");
                    DestroyPlanet(planet);
                }
                else
                {
                    _engine.Message("Planet absorbed the blast.");
                }
                return true;

            default:
                _engine.Message($"Torpedo hit something at {sector}.");
                return true;
        }
    }

    private void ApplyDamageToEnemy(Enemy enemy, double damage)
    {
        enemy.Power -= damage;

        if (enemy.IsDestroyed)
        {
            _engine.Message($"*** {enemy.Type.GetDisplayName()} at {enemy.Position} destroyed! ***");

            switch (enemy)
            {
                case SuperCommander:
                    State.SuperCommandersKilled++;
                    State.RemainingSuperCommanders--;
                    break;
                case Commander:
                    State.CommandersKilled++;
                    State.RemainingCommanders--;
                    State.Galaxy.CommanderLocations.Remove(State.Ship.Quadrant);
                    break;
                case Klingon:
                    State.KlingonsKilled++;
                    State.RemainingKlingons--;
                    break;
                case Romulan:
                    State.RomulansKilled++;
                    State.RemainingRomulans--;
                    break;
            }

            // Update galaxy map
            State.Galaxy.RemoveKlingon(State.Ship.Quadrant);

            // Check for victory
            State.CheckVictory();
        }
        else
        {
            double percent = 100 * damage / (damage + enemy.Power);
            _engine.Message($"  {enemy.Type.GetDisplayName()} at {enemy.Position} hit with {damage:F2} units ({percent:F1}% damage)");
        }
    }

    private void RemoveDestroyedEnemies()
    {
        if (State.CurrentQuadrant == null) return;

        var destroyed = State.CurrentQuadrant.Enemies.Where(e => e.IsDestroyed).ToList();
        foreach (var enemy in destroyed)
        {
            State.CurrentQuadrant.RemoveEnemy(enemy);
        }

        _engine.UpdateCondition();
    }

    /// <summary>
    /// Enemies attack the Enterprise.
    /// </summary>
    public void EnemiesAttack()
    {
        if (State.CurrentQuadrant == null) return;
        if (State.Ship.IsCloaked) return; // Enemies can't see us

        var ship = State.Ship;
        var enemies = State.CurrentQuadrant.Enemies.Where(e => !e.IsDestroyed).ToList();

        if (enemies.Count == 0) return;

        double totalDamage = 0;

        foreach (var enemy in enemies)
        {
            double hit = enemy.GetAttackPower(Random);

            // Reduce hit by distance
            hit *= (enemy.AverageDistance + enemy.Distance) / 2.0;
            hit /= enemy.Distance;

            // Skill modifier
            hit *= State.Skill.GetDamageFactor();

            if (hit <= 0) continue;

            _engine.Message($"{enemy.Type.GetDisplayName()} at {enemy.Position} fires at {ship.Name}!");

            // Shields absorb damage
            if (ship.ShieldsUp && ship.Shield > 0)
            {
                double absorbed = Math.Min(hit, ship.Shield);
                ship.Shield -= absorbed;
                hit -= absorbed;

                if (absorbed > 0)
                {
                    _engine.Message($"  Shields absorb {absorbed:F2} units, shields now at {ship.Shield:F2}");
                }
            }

            if (hit > 0)
            {
                // Damage to ship
                ship.Energy -= hit;
                totalDamage += hit;
                _engine.Warning($"  {hit:F2} units damage to {ship.Name}!");

                // Possible device damage
                if (hit > 200 + 50 * Random.NextDouble())
                {
                    CauseDeviceDamage(hit);
                }

                // Possible casualties
                if (hit > 400 && Random.NextDouble() < 0.2)
                {
                    int casualties = (int)(hit / 100);
                    ship.Casualties += casualties;
                    _engine.Warning($"  {casualties} crew casualties!");
                }
            }
        }

        // Check for death
        if (ship.Energy <= 0)
        {
            ship.Energy = 0;
            _engine.Error("*** ENTERPRISE DESTROYED ***");
            State.EndGame(GameOutcome.KilledInBattle);
        }

        if (totalDamage > 0)
        {
            _engine.Message($"Total damage: {totalDamage:F2} units.");
        }
    }

    private void CauseDeviceDamage(double hit)
    {
        var devices = DeviceTypeExtensions.All.ToList();
        var device = devices[Random.Next(devices.Count)];

        double damage = hit / (75 + 25 * Random.NextDouble());
        damage *= State.Skill.GetDamageFactor();

        State.Ship.Devices.AddDamage(device, damage);
        _engine.Warning($"  *** {device.GetDisplayName()} damaged! ***");
    }

    private void TriggerNova(SectorCoordinate center)
    {
        // Nova can damage nearby sectors
        for (int dx = -1; dx <= 1; dx++)
        {
            for (int dy = -1; dy <= 1; dy++)
            {
                if (dx == 0 && dy == 0) continue;

                var sector = new SectorCoordinate(center.X + dx, center.Y + dy);
                if (!sector.IsValid) continue;

                var entity = State.CurrentQuadrant?.GetEntityAt(sector);
                if (entity is Enemy enemy)
                {
                    _engine.Message($"  Nova destroys {enemy.Type.GetDisplayName()} at {sector}!");
                    enemy.Power = 0;
                }
            }
        }

        // Remove the star
        State.CurrentQuadrant?.ClearSector(center);
        State.StarsDestroyed++;
    }

    private void DestroyStarbase(SectorCoordinate sector)
    {
        if (State.CurrentQuadrant?.Starbase == null) return;

        State.CurrentQuadrant.RemoveEntity(State.CurrentQuadrant.Starbase);
        State.CurrentQuadrant.Starbase = null;

        State.Galaxy.StarbaseLocations.Remove(State.Ship.Quadrant);
        State.RemainingBases--;
        State.BasesDestroyed++;

        // Undock if we were docked there
        if (State.Ship.IsDocked)
        {
            State.Ship.IsDocked = false;
            _engine.UpdateCondition();
        }
    }

    private void DestroyPlanet(Planet planet)
    {
        if (State.CurrentQuadrant == null) return;

        State.CurrentQuadrant.RemoveEntity(planet);
        State.CurrentQuadrant.Planet = null;
        State.Galaxy.Planets.Remove(planet);
        State.PlanetsDestroyed++;
    }

    /// <summary>
    /// Activate the death ray (experimental weapon).
    /// </summary>
    public void FireDeathRay()
    {
        if (!State.Ship.IsDeviceOperational(DeviceType.DeathRay))
        {
            _engine.Error("Death ray is damaged and inoperative.");
            return;
        }

        State.ActionTaken = true;

        double chance = Random.NextDouble();

        if (chance < 0.3)
        {
            // Success! Kill all enemies
            _engine.Message("*** DEATH RAY FIRES SUCCESSFULLY! ***");

            if (State.CurrentQuadrant != null)
            {
                foreach (var enemy in State.CurrentQuadrant.Enemies)
                {
                    enemy.Power = 0;
                    _engine.Message($"  {enemy.Type.GetDisplayName()} at {enemy.Position} disintegrated!");
                }
            }

            RemoveDestroyedEnemies();
        }
        else if (chance < 0.7)
        {
            // Fizzles
            _engine.Warning("Death ray fizzles. Energy discharge harmless.");
        }
        else if (chance < 0.9)
        {
            // Damages the death ray
            _engine.Warning("Death ray overloads! Massive damage to death ray unit!");
            State.Ship.Devices.AddDamage(DeviceType.DeathRay, 10.0);
        }
        else
        {
            // Disaster!
            _engine.Error("*** DEATH RAY BACKFIRES! ***");
            _engine.Error("*** ENTERPRISE DESTROYED! ***");
            State.EndGame(GameOutcome.DeathRayBackfire);
        }
    }
}
