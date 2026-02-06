using TrekSuper.Core.Models;

namespace TrekSuper.Core.Commands;

/// <summary>
/// Phaser command.
/// </summary>
public class PhaserCommand : BaseCommand
{
    public override string Name => "PHASERS";
    public override string Abbreviation => "PH";
    public override string HelpText => "Fire phasers at enemy ships. Usage: PH <energy>";

    public PhaserCommand(GameEngine engine) : base(engine) { }

    public override Task<CommandResult> ExecuteAsync(GameState state, string[] args)
    {
        if (args.Length < 1)
        {
            Error("Specify phaser energy to fire. Usage: PH <energy>");
            return Task.FromResult(CommandResult.Fail("No energy specified."));
        }

        if (!TryParseDouble(args[0], out double energy) || energy <= 0)
        {
            Error("Invalid energy amount.");
            return Task.FromResult(CommandResult.Fail("Invalid energy."));
        }

        bool success = Engine.Combat.FirePhasers(energy);

        if (success)
        {
            // Allow enemies to counter-attack
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
/// Torpedo command.
/// </summary>
public class TorpedoCommand : BaseCommand
{
    public override string Name => "TORPEDO";
    public override string Abbreviation => "TO";
    public override string HelpText => "Fire photon torpedo. Usage: TO <direction> or TO <x> <y>";

    public TorpedoCommand(GameEngine engine) : base(engine) { }

    public override Task<CommandResult> ExecuteAsync(GameState state, string[] args)
    {
        double direction;

        if (args.Length == 0)
        {
            Error("Specify torpedo direction (1-12) or target coordinates.");
            return Task.FromResult(CommandResult.Fail("No direction specified."));
        }

        if (args.Length == 1)
        {
            if (!TryParseDouble(args[0], out direction))
            {
                Error("Invalid direction.");
                return Task.FromResult(CommandResult.Fail("Invalid direction."));
            }
        }
        else
        {
            // Target coordinates specified
            if (!TryParseInt(args[0], out int tx) || !TryParseInt(args[1], out int ty))
            {
                Error("Invalid target coordinates.");
                return Task.FromResult(CommandResult.Fail("Invalid coordinates."));
            }

            // Calculate direction from ship to target
            var ship = state.Ship;
            double dx = tx - ship.Sector.X;
            double dy = ty - ship.Sector.Y;

            if (dx == 0 && dy == 0)
            {
                Error("Cannot fire at own position!");
                return Task.FromResult(CommandResult.Fail("Invalid target."));
            }

            // Convert to course (1-12)
            double angle = Math.Atan2(-dx, dy);
            direction = 8.0 - 8.0 * angle / Math.PI;
            if (direction < 1) direction += 12;
            if (direction > 12) direction -= 12;
        }

        bool success = Engine.Combat.FireTorpedo(direction);

        if (success)
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
/// Death ray command.
/// </summary>
public class DeathRayCommand : BaseCommand
{
    public override string Name => "DEATHRAY";
    public override string Abbreviation => "DE";
    public override string HelpText => "Fire the experimental death ray (DANGEROUS!).";

    public DeathRayCommand(GameEngine engine) : base(engine) { }

    public override Task<CommandResult> ExecuteAsync(GameState state, string[] args)
    {
        Warning("The death ray is experimental and DANGEROUS!");
        Warning("There is a significant chance of malfunction.");

        Engine.Combat.FireDeathRay();

        if (!state.IsGameOver)
        {
            Engine.Combat.EnemiesAttack();
        }

        return Task.FromResult(CommandResult.Ok(actionTaken: true));
    }
}
