using TrekSuper.Core.Enums;
using TrekSuper.Core.Models;

namespace TrekSuper.Core.Commands;

/// <summary>
/// Shield control command.
/// </summary>
public class ShieldCommand : BaseCommand
{
    public override string Name => "SHIELDS";
    public override string Abbreviation => "SH";
    public override string HelpText => "Control shields. Usage: SH UP/DOWN or SH TRANSFER <amount>";

    public override string DetailedHelpText => @"
USAGE:
  SHIELDS                    - Show current shield status
  SHIELDS UP                 - Raise shields to maximum
  SHIELDS DOWN               - Lower shields completely
  SHIELDS TRANSFER <amount>  - Transfer specific energy to shields

  SH UP                      - Abbreviated form
  SH DOWN                    - Abbreviated form
  SH TRANSFER <amount>       - Abbreviated form

DESCRIPTION:
  Control the Enterprise's defensive shields. Shields protect against
  enemy weapons fire and collisions, but consume energy.

PARAMETERS:
  <amount>  - Energy to transfer to/from shields (positive or negative)

EXAMPLES:
  SHIELDS                 - Check current shield status
  SHIELDS UP              - Raise shields to maximum protection
  SHIELDS DOWN            - Lower shields (saves energy but dangerous!)
  SHIELDS TRANSFER 500    - Transfer 500 energy units to shields
  SHIELDS TRANSFER -200   - Transfer 200 energy FROM shields back to ship

SHIELD MECHANICS:
  • Shields absorb damage from enemy weapons
  • Each hit reduces shield energy
  • When shields reach 0, hull takes direct damage
  • Shields use ship's energy reserves
  • Maximum shield energy equals ship's max energy

IMPORTANT NOTES:
  • ALWAYS raise shields before combat or movement near enemies!
  • After any IMPULSE or WARP movement, enemies will attack
  • Shields don't regenerate - you must transfer energy manually
  • When docked at a starbase, shields are automatically replenished
  • Running out of energy with shields up can leave you vulnerable

ENERGY MANAGEMENT:
  • Total energy = Ship Energy + Shield Energy
  • Transfer between ship and shields as needed
  • Keep enough ship energy for weapons and movement
  • Balance defense (shields) vs offense (phasers/torpedoes)

TACTICAL TIPS:
  • Raise shields BEFORE entering combat
  • Use SRSCAN to detect nearby enemies
  • If energy is low, consider lowering shields to free up power
  • Dock at starbases to fully recharge

SEE ALSO:
  STATUS    - View complete ship status including shields
  DAMAGE    - Check if shield control is damaged
  DOCK      - Dock at starbase for repairs and recharge";

    public ShieldCommand(GameEngine engine) : base(engine) { }

    public override Task<CommandResult> ExecuteAsync(GameState state, string[] args)
    {
        if (!state.Ship.IsDeviceOperational(DeviceType.Shields))
        {
            Error("Shield control is damaged.");
            return Task.FromResult(CommandResult.Fail("Shields damaged."));
        }

        if (args.Length == 0)
        {
            // Report shield status
            Message($"Shield status: {(state.Ship.ShieldsUp ? "UP" : "DOWN")}");
            Message($"Shield energy: {state.Ship.Shield:F0} / {state.Ship.MaxShield:F0}");
            return Task.FromResult(CommandResult.Ok());
        }

        string action = args[0].ToUpperInvariant();

        switch (action)
        {
            case "UP":
            case "U":
                if (state.Ship.ShieldsUp)
                {
                    Message("Shields are already up.");
                }
                else
                {
                    state.Ship.ShieldsUp = true;
                    state.Ship.ShieldsChanging = true;
                    Message("Shields raised.");
                }
                return Task.FromResult(CommandResult.Ok(actionTaken: true));

            case "DOWN":
            case "D":
                if (!state.Ship.ShieldsUp)
                {
                    Message("Shields are already down.");
                }
                else
                {
                    state.Ship.ShieldsUp = false;
                    state.Ship.ShieldsChanging = true;
                    Message("Shields lowered.");
                }
                return Task.FromResult(CommandResult.Ok(actionTaken: true));

            case "TRANSFER":
            case "T":
                if (args.Length < 2)
                {
                    Error("Specify energy to transfer. Usage: SH T <amount>");
                    return Task.FromResult(CommandResult.Fail("No amount specified."));
                }

                if (!TryParseDouble(args[1], out double amount))
                {
                    Error("Invalid energy amount.");
                    return Task.FromResult(CommandResult.Fail("Invalid amount."));
                }

                return TransferEnergy(state, amount);

            default:
                Error("Unknown shield command. Use UP, DOWN, or TRANSFER.");
                return Task.FromResult(CommandResult.Fail("Unknown command."));
        }
    }

    private Task<CommandResult> TransferEnergy(GameState state, double amount)
    {
        var ship = state.Ship;

        if (amount > 0)
        {
            // Transfer from main energy to shields
            if (amount > ship.Energy)
            {
                Error($"Insufficient energy. Available: {ship.Energy:F0}");
                return Task.FromResult(CommandResult.Fail("Insufficient energy."));
            }

            double space = ship.MaxShield - ship.Shield;
            if (amount > space)
            {
                Warning($"Shields can only accept {space:F0} more units.");
                amount = space;
            }

            ship.Energy -= amount;
            ship.Shield += amount;
            Message($"Transferred {amount:F0} to shields.");
            Message($"Energy: {ship.Energy:F0}, Shields: {ship.Shield:F0}");
        }
        else
        {
            // Transfer from shields to main energy
            amount = Math.Abs(amount);

            if (amount > ship.Shield)
            {
                Error($"Insufficient shield energy. Available: {ship.Shield:F0}");
                return Task.FromResult(CommandResult.Fail("Insufficient shield energy."));
            }

            double space = ship.MaxEnergy - ship.Energy;
            if (amount > space)
            {
                Warning($"Energy banks can only accept {space:F0} more units.");
                amount = space;
            }

            ship.Shield -= amount;
            ship.Energy += amount;
            Message($"Transferred {amount:F0} from shields to energy banks.");
            Message($"Energy: {ship.Energy:F0}, Shields: {ship.Shield:F0}");
        }

        return Task.FromResult(CommandResult.Ok(actionTaken: true));
    }
}
