using TrekSuper.Core.Enums;
using TrekSuper.Core.Models;

namespace TrekSuper.Core.Commands;

/// <summary>
/// Warp command.
/// </summary>
public class WarpCommand : BaseCommand
{
    public override string Name => "WARP";
    public override string Abbreviation => "W";
    public override string HelpText => "Warp to a new location. Usage: W <direction> <distance>";

    public WarpCommand(GameEngine engine) : base(engine) { }

    public override Task<CommandResult> ExecuteAsync(GameState state, string[] args)
    {
        if (args.Length < 2)
        {
            Error("Specify direction (1-12) and distance. Usage: W <direction> <distance>");
            return Task.FromResult(CommandResult.Fail("Invalid arguments."));
        }

        if (!TryParseDouble(args[0], out double direction) || direction < 1 || direction > 12)
        {
            Error("Direction must be between 1 and 12.");
            return Task.FromResult(CommandResult.Fail("Invalid direction."));
        }

        if (!TryParseDouble(args[1], out double distance) || distance <= 0)
        {
            Error("Distance must be positive.");
            return Task.FromResult(CommandResult.Fail("Invalid distance."));
        }

        bool success = Engine.Navigation.Warp(direction, distance);

        if (success && !state.IsGameOver)
        {
            Engine.Events.ProcessEvents();
            Engine.Combat.EnemiesAttack();
        }

        return Task.FromResult(new CommandResult
        {
            Success = success,
            ActionTaken = success
        });
    }
}

/// <summary>
/// Impulse command.
/// </summary>
public class ImpulseCommand : BaseCommand
{
    public override string Name => "IMPULSE";
    public override string Abbreviation => "IM";
    public override string HelpText => "Move using impulse engines. Usage: IM <direction> <distance>";

    public ImpulseCommand(GameEngine engine) : base(engine) { }

    public override Task<CommandResult> ExecuteAsync(GameState state, string[] args)
    {
        if (args.Length < 2)
        {
            Error("Specify direction (1-12) and distance. Usage: IM <direction> <distance>");
            return Task.FromResult(CommandResult.Fail("Invalid arguments."));
        }

        if (!TryParseDouble(args[0], out double direction) || direction < 1 || direction > 12)
        {
            Error("Direction must be between 1 and 12.");
            return Task.FromResult(CommandResult.Fail("Invalid direction."));
        }

        if (!TryParseDouble(args[1], out double distance) || distance <= 0)
        {
            Error("Distance must be positive.");
            return Task.FromResult(CommandResult.Fail("Invalid distance."));
        }

        bool success = Engine.Navigation.Impulse(direction, distance);

        if (success && !state.IsGameOver)
        {
            Engine.Combat.EnemiesAttack();
        }

        return Task.FromResult(new CommandResult
        {
            Success = success,
            ActionTaken = success
        });
    }
}

/// <summary>
/// Dock command.
/// </summary>
public class DockCommand : BaseCommand
{
    public override string Name => "DOCK";
    public override string Abbreviation => "DO";
    public override string HelpText => "Dock at a nearby starbase for resupply and repairs.";

    public DockCommand(GameEngine engine) : base(engine) { }

    public override Task<CommandResult> ExecuteAsync(GameState state, string[] args)
    {
        bool success = Engine.Navigation.Dock();
        return Task.FromResult(new CommandResult { Success = success });
    }
}

/// <summary>
/// Set warp factor command.
/// </summary>
public class SetWarpCommand : BaseCommand
{
    public override string Name => "SETWARP";
    public override string Abbreviation => "SE";
    public override string HelpText => "Set warp factor (1.0-10.0). Usage: SE <factor>";

    public SetWarpCommand(GameEngine engine) : base(engine) { }

    public override Task<CommandResult> ExecuteAsync(GameState state, string[] args)
    {
        if (args.Length < 1)
        {
            Message($"Current warp factor: {state.Ship.WarpFactor:F1}");
            return Task.FromResult(CommandResult.Ok());
        }

        if (!TryParseDouble(args[0], out double warp))
        {
            Error("Invalid warp factor.");
            return Task.FromResult(CommandResult.Fail("Invalid warp factor."));
        }

        Engine.Navigation.SetWarpFactor(warp);
        return Task.FromResult(CommandResult.Ok());
    }
}

/// <summary>
/// Rest command.
/// </summary>
public class RestCommand : BaseCommand
{
    public override string Name => "REST";
    public override string Abbreviation => "RE";
    public override string HelpText => "Rest for repairs. Usage: RE <time>";

    public RestCommand(GameEngine engine) : base(engine) { }

    public override Task<CommandResult> ExecuteAsync(GameState state, string[] args)
    {
        if (args.Length < 1)
        {
            Error("Specify rest time. Usage: RE <time>");
            return Task.FromResult(CommandResult.Fail("No time specified."));
        }

        if (!TryParseDouble(args[0], out double time) || time <= 0)
        {
            Error("Invalid rest time.");
            return Task.FromResult(CommandResult.Fail("Invalid time."));
        }

        Engine.Navigation.Rest(time);
        Engine.Events.ProcessEvents();

        return Task.FromResult(CommandResult.Ok(actionTaken: true, timeUsed: time));
    }
}
