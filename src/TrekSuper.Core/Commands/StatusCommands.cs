using TrekSuper.Core.Enums;
using TrekSuper.Core.Models;

namespace TrekSuper.Core.Commands;

/// <summary>
/// Status/report command.
/// </summary>
public class StatusCommand : BaseCommand
{
    public override string Name => "STATUS";
    public override string Abbreviation => "ST";
    public override string HelpText => "Display ship and mission status.";

    public StatusCommand(GameEngine engine) : base(engine) { }

    public override Task<CommandResult> ExecuteAsync(GameState state, string[] args)
    {
        var ship = state.Ship;

        Message("\n*** STATUS REPORT ***\n");
        Message($"Stardate:           {state.Stardate:F1}");
        Message($"Time remaining:     {state.TimeRemaining:F2}");
        Message($"Condition:          {ship.Condition.GetDisplayName()}");
        Message($"Position:           Quadrant {ship.Quadrant}, Sector {ship.Sector}");
        Message($"Life Support:       {(ship.IsDeviceOperational(DeviceType.LifeSupport) ? "ACTIVE" : "DAMAGED")}");

        if (!ship.IsDeviceOperational(DeviceType.LifeSupport))
        {
            Message($"  Reserves:         {ship.LifeSupportReserves:F2}");
        }

        Message($"Warp Factor:        {ship.WarpFactor:F1}");
        Message($"Energy:             {ship.Energy:F0}");
        Message($"Shields:            {(ship.ShieldsUp ? "UP" : "DOWN")}, {ship.Shield:F0}");
        Message($"Photon Torpedoes:   {ship.Torpedoes}");
        Message($"Probes:             {ship.Probes}");

        if (ship.HasCrystals)
        {
            Message($"Dilithium Crystals: ABOARD");
        }

        Message($"\nKlingons remaining: {state.RemainingKlingons + state.RemainingCommanders}");
        Message($"Starbases:          {state.RemainingBases}");

        if (state.RemainingSuperCommanders > 0)
        {
            Message($"Super-Commanders:   {state.RemainingSuperCommanders}");
        }

        return Task.FromResult(CommandResult.Ok());
    }
}

/// <summary>
/// Damage report command.
/// </summary>
public class DamageCommand : BaseCommand
{
    public override string Name => "DAMAGES";
    public override string Abbreviation => "DA";
    public override string HelpText => "Display damage report for all ship systems.";

    public DamageCommand(GameEngine engine) : base(engine) { }

    public override Task<CommandResult> ExecuteAsync(GameState state, string[] args)
    {
        var ship = state.Ship;

        Message("\n*** DAMAGE REPORT ***\n");
        Message("DEVICE              STATUS");
        Message("------              ------");

        bool anyDamage = false;

        foreach (var (device, damage) in ship.Devices.GetAllDevices())
        {
            string status;
            if (damage <= 0)
            {
                status = "Operational";
            }
            else
            {
                status = $"DAMAGED ({damage:F2} stardates to repair)";
                anyDamage = true;
            }

            Message($"{device.GetDisplayName(),-18}  {status}");
        }

        if (!anyDamage)
        {
            Message("\nAll systems operational.");
        }
        else if (ship.IsDocked)
        {
            Message("\nRepairs proceeding at starbase.");
        }
        else
        {
            Message("\nDock at a starbase for faster repairs.");
        }

        return Task.FromResult(CommandResult.Ok());
    }
}

/// <summary>
/// Computer command for calculating courses.
/// </summary>
public class ComputerCommand : BaseCommand
{
    public override string Name => "COMPUTER";
    public override string Abbreviation => "CO";
    public override string HelpText => "Use ship's computer. Usage: CO <dest-quadrant-x> <dest-quadrant-y>";

    public ComputerCommand(GameEngine engine) : base(engine) { }

    public override Task<CommandResult> ExecuteAsync(GameState state, string[] args)
    {
        if (!state.Ship.IsDeviceOperational(DeviceType.Computer))
        {
            Error("Computer is damaged and inoperative.");
            return Task.FromResult(CommandResult.Fail("Computer damaged."));
        }

        if (args.Length < 2)
        {
            // Show current position
            Message($"Current position: Quadrant {state.Ship.Quadrant}, Sector {state.Ship.Sector}");
            Message("To calculate course, specify destination quadrant: CO <x> <y>");
            return Task.FromResult(CommandResult.Ok());
        }

        if (!TryParseInt(args[0], out int destX) || !TryParseInt(args[1], out int destY))
        {
            Error("Invalid coordinates.");
            return Task.FromResult(CommandResult.Fail("Invalid coordinates."));
        }

        if (destX < 1 || destX > 8 || destY < 1 || destY > 8)
        {
            Error("Coordinates must be 1-8.");
            return Task.FromResult(CommandResult.Fail("Invalid coordinates."));
        }

        var dest = new QuadrantCoordinate(destX, destY);
        var ship = state.Ship;

        double dx = dest.X - ship.Quadrant.X;
        double dy = dest.Y - ship.Quadrant.Y;

        if (dx == 0 && dy == 0)
        {
            Message("You are already in that quadrant.");
            return Task.FromResult(CommandResult.Ok());
        }

        // Calculate course
        double angle = Math.Atan2(-dx, dy);
        double course = 8.0 - 8.0 * angle / Math.PI;
        if (course < 1) course += 12;
        if (course > 12) course -= 12;

        // Calculate distance
        double distance = Math.Sqrt(dx * dx + dy * dy);

        // Calculate time at current warp
        double time = 10.0 * distance / ship.WarpFactorSquared;

        // Calculate energy required
        double energy = (distance + 0.05) * ship.WarpFactorSquared;

        Message($"\nCourse to {GameEngine.GetQuadrantName(dest)} ({dest}):");
        Message($"  Direction: {course:F2}");
        Message($"  Distance:  {distance:F2} quadrants");
        Message($"  Time:      {time:F2} stardates at warp {ship.WarpFactor:F1}");
        Message($"  Energy:    {energy:F0} units at warp {ship.WarpFactor:F1}");

        return Task.FromResult(CommandResult.Ok());
    }
}

/// <summary>
/// Help command.
/// </summary>
public class HelpCommand : BaseCommand
{
    private readonly CommandRegistry _registry;

    public override string Name => "HELP";
    public override string Abbreviation => "HE";
    public override string HelpText => "Display help information.";

    public HelpCommand(GameEngine engine, CommandRegistry registry) : base(engine)
    {
        _registry = registry;
    }

    public override Task<CommandResult> ExecuteAsync(GameState state, string[] args)
    {
        if (args.Length > 0)
        {
            var cmd = _registry.GetCommand(args[0]);
            if (cmd != null)
            {
                Message($"\n{cmd.Name} ({cmd.Abbreviation})");
                Message(cmd.HelpText);
                return Task.FromResult(CommandResult.Ok());
            }
        }

        Message("\n*** AVAILABLE COMMANDS ***\n");
        Message("COMMAND     ABBREV  DESCRIPTION");
        Message("-------     ------  -----------");

        foreach (var cmd in _registry.GetAllCommands().OrderBy(c => c.Name))
        {
            Message($"{cmd.Name,-11} {cmd.Abbreviation,-6}  {cmd.HelpText}");
        }

        Message("\nDirections: 1=up-left, 3=up-right, 5=down-right, 7=down-left");
        Message("            2=up,      4=right,    6=down,       8=left");

        return Task.FromResult(CommandResult.Ok());
    }
}

/// <summary>
/// Quit/exit command.
/// </summary>
public class QuitCommand : BaseCommand
{
    public override string Name => "QUIT";
    public override string Abbreviation => "QU";
    public override string HelpText => "End the game.";

    public QuitCommand(GameEngine engine) : base(engine) { }

    public override Task<CommandResult> ExecuteAsync(GameState state, string[] args)
    {
        state.EndGame(GameOutcome.Abandoned);
        Message("\nGame ended.");
        Engine.Score();
        return Task.FromResult(CommandResult.Ok());
    }

    public override bool CanExecute(GameState state) => true; // Can always quit
}

/// <summary>
/// Score command.
/// </summary>
public class ScoreCommand : BaseCommand
{
    public override string Name => "SCORE";
    public override string Abbreviation => "SC";
    public override string HelpText => "Display current score.";

    public ScoreCommand(GameEngine engine) : base(engine) { }

    public override Task<CommandResult> ExecuteAsync(GameState state, string[] args)
    {
        Engine.Score();
        return Task.FromResult(CommandResult.Ok());
    }
}
