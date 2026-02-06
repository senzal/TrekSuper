using TrekSuper.Core.Enums;
using TrekSuper.Core.Models;

namespace TrekSuper.Core.Services;

/// <summary>
/// Handles scheduled game events (supernovas, attacks, etc.).
/// </summary>
public class EventService
{
    private readonly GameEngine _engine;

    public EventService(GameEngine engine)
    {
        _engine = engine;
    }

    private GameState State => _engine.State;
    private Random Random => State.Random;

    /// <summary>
    /// Process all events that should occur up to the current stardate.
    /// </summary>
    public void ProcessEvents()
    {
        // Check each event type
        for (int i = 1; i <= 8; i++)
        {
            var eventType = (FutureEventType)i;
            double scheduledDate = State.FutureEvents[i];

            if (scheduledDate > 0 && scheduledDate <= State.Stardate)
            {
                HandleEvent(eventType);
            }
        }
    }

    private void HandleEvent(FutureEventType eventType)
    {
        switch (eventType)
        {
            case FutureEventType.Supernova:
                HandleSupernova();
                break;

            case FutureEventType.TractorBeam:
                HandleTractorBeam();
                break;

            case FutureEventType.Snapshot:
                HandleSnapshot();
                break;

            case FutureEventType.BaseAttack:
                HandleBaseAttack();
                break;

            case FutureEventType.CommanderDestroysBase:
                HandleBaseDestruction();
                break;

            case FutureEventType.SuperCommanderMoves:
                HandleSuperCommanderMove();
                break;

            case FutureEventType.SuperCommanderDestroysBase:
                HandleSuperCommanderDestroysBase();
                break;

            case FutureEventType.DeepSpaceProbeMove:
                HandleProbeMove();
                break;
        }
    }

    private void HandleSupernova()
    {
        // Pick a random quadrant for supernova
        var quad = Galaxy.GetRandomQuadrant(Random);

        bool isPlayerQuadrant = quad == State.Ship.Quadrant;

        if (isPlayerQuadrant)
        {
            _engine.Error("*** RED ALERT! SUPERNOVA DETECTED! ***");
            _engine.Error($"*** SUPERNOVA IN QUADRANT {quad}! ***");

            // Player might escape if they're fast
            if (State.Ship.IsDeviceOperational(DeviceType.WarpEngines))
            {
                _engine.Warning("Emergency warp engaged!");

                // Move to adjacent quadrant
                int newX = State.Ship.Quadrant.X + (Random.Next(3) - 1);
                int newY = State.Ship.Quadrant.Y + (Random.Next(3) - 1);
                newX = Math.Clamp(newX, 1, 8);
                newY = Math.Clamp(newY, 1, 8);

                if (newX == quad.X && newY == quad.Y)
                {
                    newX = (newX % 8) + 1; // Ensure different quadrant
                }

                var newQuad = new QuadrantCoordinate(newX, newY);
                _engine.EnterQuadrant(newQuad);
                _engine.Message($"Escaped to {GameEngine.GetQuadrantName(newQuad)}.");
            }
            else
            {
                _engine.Error("Cannot escape - warp engines are damaged!");
                State.EndGame(GameOutcome.Supernova);
                return;
            }
        }
        else if (CanReceiveRadio())
        {
            _engine.Warning($"Message from Starfleet: Supernova in {GameEngine.GetQuadrantName(quad)}!");
        }

        // Mark quadrant as destroyed
        State.Galaxy.Supernova(quad);

        // Schedule next supernova
        State.FutureEvents[(int)FutureEventType.Supernova] =
            State.Stardate + ExpRand(0.5 * State.InitialTime);
    }

    private void HandleTractorBeam()
    {
        if (State.RemainingCommanders == 0)
        {
            State.FutureEvents[(int)FutureEventType.TractorBeam] = 0;
            return;
        }

        // Commander attempts to tractor beam the Enterprise
        if (State.Ship.IsCloaked)
        {
            // Can't be tractored while cloaked
            RescheduleTractorBeam();
            return;
        }

        // Pick a random commander
        if (State.Galaxy.CommanderLocations.Count == 0)
        {
            RescheduleTractorBeam();
            return;
        }

        var cmdQuad = State.Galaxy.CommanderLocations[Random.Next(State.Galaxy.CommanderLocations.Count)];

        _engine.Warning("*** COMMANDER TRACTOR BEAM! ***");
        _engine.Warning($"Enterprise pulled to quadrant {cmdQuad}!");

        // Move the Enterprise
        _engine.EnterQuadrant(cmdQuad);

        // This uses time
        State.AdvanceTime(Random.NextDouble() * 2.0);

        RescheduleTractorBeam();
    }

    private void RescheduleTractorBeam()
    {
        if (State.RemainingCommanders > 0)
        {
            State.FutureEvents[(int)FutureEventType.TractorBeam] =
                State.Stardate + ExpRand(1.5 * State.InitialTime / State.RemainingCommanders);
        }
        else
        {
            State.FutureEvents[(int)FutureEventType.TractorBeam] = 0;
        }
    }

    private void HandleSnapshot()
    {
        // Take a snapshot for time warp
        State.Snapshot = new GameStateSnapshot
        {
            Stardate = State.Stardate,
            RemainingKlingons = State.RemainingKlingons,
            RemainingCommanders = State.RemainingCommanders,
            RemainingBases = State.RemainingBases
        };

        // Schedule next snapshot
        State.FutureEvents[(int)FutureEventType.Snapshot] =
            State.Stardate + ExpRand(0.5 * State.InitialTime);
    }

    private void HandleBaseAttack()
    {
        if (State.RemainingCommanders == 0 || State.RemainingBases == 0)
        {
            State.FutureEvents[(int)FutureEventType.BaseAttack] = 0;
            return;
        }

        // Find a base to attack
        if (State.Galaxy.StarbaseLocations.Count == 0)
        {
            return;
        }

        var baseQuad = State.Galaxy.StarbaseLocations[Random.Next(State.Galaxy.StarbaseLocations.Count)];

        // Check if a commander is near
        var nearbyCommander = State.Galaxy.CommanderLocations
            .FirstOrDefault(c => c.DistanceTo(baseQuad) <= 2.0);

        if (nearbyCommander == QuadrantCoordinate.Invalid)
        {
            // No commander nearby, reschedule
            State.FutureEvents[(int)FutureEventType.BaseAttack] =
                State.Stardate + ExpRand(0.3 * State.InitialTime);
            return;
        }

        State.BaseUnderAttack = baseQuad;
        State.HasSeenBaseAttackReport = false;

        if (CanReceiveRadio())
        {
            _engine.Warning($"*** STARBASE IN {GameEngine.GetQuadrantName(baseQuad)} IS UNDER ATTACK! ***");
            _engine.Warning("You have limited time to respond!");
            State.HasSeenBaseAttackReport = true;
        }

        // Schedule base destruction
        State.FutureEvents[(int)FutureEventType.CommanderDestroysBase] =
            State.Stardate + 1.0 + 2.0 * Random.NextDouble();

        // Reschedule next base attack
        State.FutureEvents[(int)FutureEventType.BaseAttack] =
            State.Stardate + ExpRand(0.3 * State.InitialTime);
    }

    private void HandleBaseDestruction()
    {
        if (State.BaseUnderAttack == QuadrantCoordinate.Invalid)
        {
            State.FutureEvents[(int)FutureEventType.CommanderDestroysBase] = 0;
            return;
        }

        // Check if player is at the base
        if (State.Ship.Quadrant == State.BaseUnderAttack)
        {
            // Player defended the base
            State.FutureEvents[(int)FutureEventType.CommanderDestroysBase] = 0;
            State.BaseUnderAttack = QuadrantCoordinate.Invalid;
            return;
        }

        // Base is destroyed
        if (CanReceiveRadio())
        {
            _engine.Error($"*** STARBASE IN {GameEngine.GetQuadrantName(State.BaseUnderAttack)} DESTROYED! ***");
        }

        State.Galaxy.StarbaseLocations.Remove(State.BaseUnderAttack);
        State.RemainingBases--;
        State.BasesDestroyed++;

        // Update galaxy data
        var data = State.Galaxy.GetQuadrantData(State.BaseUnderAttack);
        State.Galaxy.SetQuadrantData(State.BaseUnderAttack, data - 10);

        State.FutureEvents[(int)FutureEventType.CommanderDestroysBase] = 0;
        State.BaseUnderAttack = QuadrantCoordinate.Invalid;
    }

    private void HandleSuperCommanderMove()
    {
        if (State.RemainingSuperCommanders == 0)
        {
            State.FutureEvents[(int)FutureEventType.SuperCommanderMoves] = 0;
            return;
        }

        // Super-Commander moves toward nearest base or Enterprise
        var scQuad = State.Galaxy.SuperCommanderLocation;
        if (!scQuad.IsValid) return;

        // Find nearest target
        QuadrantCoordinate target = State.Ship.Quadrant;
        double minDist = scQuad.DistanceTo(target);

        foreach (var baseQuad in State.Galaxy.StarbaseLocations)
        {
            double dist = scQuad.DistanceTo(baseQuad);
            if (dist < minDist)
            {
                minDist = dist;
                target = baseQuad;
            }
        }

        // Move toward target
        int dx = Math.Sign(target.X - scQuad.X);
        int dy = Math.Sign(target.Y - scQuad.Y);

        var newQuad = new QuadrantCoordinate(scQuad.X + dx, scQuad.Y + dy);
        if (newQuad.IsValid)
        {
            // Update galaxy
            State.Galaxy.RemoveKlingon(scQuad);
            State.Galaxy.AddKlingon(newQuad);
            State.Galaxy.SuperCommanderLocation = newQuad;

            // Check if SC reached a base
            if (State.Galaxy.StarbaseLocations.Contains(newQuad))
            {
                // SC will attack base
                State.FutureEvents[(int)FutureEventType.SuperCommanderDestroysBase] =
                    State.Stardate + 0.5 + Random.NextDouble();
            }
        }

        // Schedule next move
        State.FutureEvents[(int)FutureEventType.SuperCommanderMoves] =
            State.Stardate + 0.2777;
    }

    private void HandleSuperCommanderDestroysBase()
    {
        var scQuad = State.Galaxy.SuperCommanderLocation;
        if (!scQuad.IsValid) return;

        // Check if SC is at a base
        if (!State.Galaxy.StarbaseLocations.Contains(scQuad))
        {
            State.FutureEvents[(int)FutureEventType.SuperCommanderDestroysBase] = 0;
            return;
        }

        // Check if player is there to defend
        if (State.Ship.Quadrant == scQuad)
        {
            State.FutureEvents[(int)FutureEventType.SuperCommanderDestroysBase] = 0;
            return;
        }

        // Destroy the base
        if (CanReceiveRadio())
        {
            _engine.Error($"*** SUPER-COMMANDER DESTROYS STARBASE IN {GameEngine.GetQuadrantName(scQuad)}! ***");
        }

        State.Galaxy.StarbaseLocations.Remove(scQuad);
        State.RemainingBases--;
        State.BasesDestroyed++;

        var data = State.Galaxy.GetQuadrantData(scQuad);
        State.Galaxy.SetQuadrantData(scQuad, data - 10);

        State.FutureEvents[(int)FutureEventType.SuperCommanderDestroysBase] = 0;
    }

    private void HandleProbeMove()
    {
        if (State.ProbeMovesRemaining <= 0)
        {
            State.FutureEvents[(int)FutureEventType.DeepSpaceProbeMove] = 0;
            return;
        }

        // Move probe
        State.ProbeX += State.ProbeIncrementX;
        State.ProbeY += State.ProbeIncrementY;
        State.ProbeMovesRemaining--;

        int qx = (int)State.ProbeX;
        int qy = (int)State.ProbeY;

        // Check boundaries
        if (qx < 1 || qx > 8 || qy < 1 || qy > 8)
        {
            _engine.Message("Probe has left the galaxy.");
            State.FutureEvents[(int)FutureEventType.DeepSpaceProbeMove] = 0;
            return;
        }

        var probeQuad = new QuadrantCoordinate(qx, qy);

        // Report what probe finds
        if (CanReceiveRadio())
        {
            int data = State.Galaxy.GetQuadrantData(probeQuad);
            int klingons = data / 100;
            int bases = (data % 100) / 10;
            int stars = data % 10;

            _engine.Message($"Probe reports from {GameEngine.GetQuadrantName(probeQuad)}:");
            _engine.Message($"  Klingons: {klingons}, Bases: {bases}, Stars: {stars}");

            // Update chart
            State.Galaxy.UpdateChart(probeQuad);
        }

        // If armed and enemies present, detonate
        if (State.ProbeIsArmed && State.Galaxy.GetKlingonCount(probeQuad) > 0)
        {
            _engine.Warning($"*** PROBE DETONATES IN {GameEngine.GetQuadrantName(probeQuad)}! ***");

            // Destroy everything in quadrant
            State.Galaxy.Supernova(probeQuad);
            State.FutureEvents[(int)FutureEventType.DeepSpaceProbeMove] = 0;
            return;
        }

        // Schedule next move
        if (State.ProbeMovesRemaining > 0)
        {
            State.FutureEvents[(int)FutureEventType.DeepSpaceProbeMove] =
                State.Stardate + 0.1;
        }
        else
        {
            _engine.Message("Probe has exhausted its fuel.");
            State.FutureEvents[(int)FutureEventType.DeepSpaceProbeMove] = 0;
        }
    }

    private bool CanReceiveRadio()
    {
        return State.Ship.IsDocked ||
               (State.Ship.IsDeviceOperational(DeviceType.SubspaceRadio) && !State.Ship.IsCloaked);
    }

    private double ExpRand(double mean) =>
        -mean * Math.Log(1.0 - Random.NextDouble());

    /// <summary>
    /// Launch a deep space probe.
    /// </summary>
    public void LaunchProbe(double direction, bool armed)
    {
        if (State.Ship.Probes <= 0)
        {
            _engine.Error("No probes remaining.");
            return;
        }

        State.Ship.Probes--;

        // Set probe starting position and direction
        State.ProbeX = State.Ship.Quadrant.X;
        State.ProbeY = State.Ship.Quadrant.Y;

        double angle = (15.0 - direction) * Math.PI / 8.0;
        State.ProbeIncrementX = -Math.Sin(angle);
        State.ProbeIncrementY = Math.Cos(angle);

        State.ProbeMovesRemaining = 10;
        State.ProbeIsArmed = armed;

        _engine.Message($"Probe launched on course {direction:F1}.");
        if (armed)
        {
            _engine.Warning("Probe is ARMED.");
        }

        // Schedule first move
        State.FutureEvents[(int)FutureEventType.DeepSpaceProbeMove] = State.Stardate + 0.1;
    }
}
