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

    public override string DetailedHelpText => @"
USAGE:
  IMPULSE <direction> <distance>
  IM <direction> <distance>

DESCRIPTION:
  Move the Enterprise using impulse engines for short-range movement
  within the current sector or nearby sectors.

PARAMETERS:
  <direction>  - Direction to move (1-12, like a clock face)
                 12=up, 3=right, 6=down, 9=left
                 Use fractional values (e.g., 6.5) for diagonal movement

  <distance>   - Distance in QUADRANTS (1 quadrant = 10 sectors)
                 0.1 = 1 sector   (precise movement)
                 0.5 = 5 sectors  (medium range)
                 1.0 = 10 sectors (maximum impulse range)

DIRECTION MAP (MERMAID):
```mermaid
graph TB
    subgraph ""Navigation Compass""
        D12[""12<br/>⬆️ UP""]
        D1[""1<br/>↗️""]
        D2[""2<br/>↗️""]
        D3[""3<br/>➡️ RIGHT""]
        D4[""4<br/>↘️""]
        D5[""5<br/>↘️""]
        D6[""6<br/>⬇️ DOWN""]
        D7[""7<br/>↙️""]
        D8[""8<br/>↙️""]
        D9[""9<br/>⬅️ LEFT""]
        D10[""10<br/>↖️""]
        D11[""11<br/>↖️""]
        E[""🚀<br/>ENTERPRISE""]

        D12 -.-> E
        D1 -.-> E
        D2 -.-> E
        D3 -.-> E
        D4 -.-> E
        D5 -.-> E
        D6 -.-> E
        D7 -.-> E
        D8 -.-> E
        D9 -.-> E
        D10 -.-> E
        D11 -.-> E
    end

    style E fill:#f9f,stroke:#333,stroke-width:4px
    style D12 fill:#bbf,stroke:#333
    style D3 fill:#bbf,stroke:#333
    style D6 fill:#bbf,stroke:#333
    style D9 fill:#bbf,stroke:#333
```

EXAMPLES:
  IM 6 0.1     - Move 1 sector straight down
  IM 3 0.2     - Move 2 sectors to the right
  IM 12 0.5    - Move 5 sectors straight up
  IM 9 0.05    - Move half a sector to the left

ENERGY COST:
  20 + (100 × distance) energy units

IMPORTANT NOTES:
  • Impulse is limited to 1.0 quadrant maximum range
  • After each impulse move, enemies will attack if present!
  • Course is automatically checked for obstacles before moving
  • If your path would hit a star or black hole, move is aborted
  • Always raise shields before moving if enemies are nearby
  • Use SRSCAN to see your surroundings before moving

SAFETY:
  The navigation computer will warn you if your projected course
  will intercept any objects (stars, enemies, starbases). The move
  will be automatically aborted to prevent accidental collisions.

SEE ALSO:
  WARP      - Long-range movement between quadrants
  SRSCAN    - View current sector
  SHIELDS   - Raise/lower shields for protection";

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
