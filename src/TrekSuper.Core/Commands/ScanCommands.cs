using TrekSuper.Core.Enums;
using TrekSuper.Core.Models;

namespace TrekSuper.Core.Commands;

/// <summary>
/// Short-range scan command.
/// </summary>
public class ShortRangeScanCommand : BaseCommand
{
    public override string Name => "SRSCAN";
    public override string Abbreviation => "SR";
    public override string HelpText => "Perform a short-range scan of the current quadrant.";

    public ShortRangeScanCommand(GameEngine engine) : base(engine) { }

    public override Task<CommandResult> ExecuteAsync(GameState state, string[] args)
    {
        if (!state.Ship.IsDeviceOperational(DeviceType.ShortRangeSensors))
        {
            Warning("Short-range sensors are damaged.");
            Warning("Scan will be limited.");
        }

        // The actual rendering is handled by the UI through events
        Engine.Message("SHORT-RANGE SCAN");
        return Task.FromResult(CommandResult.Ok());
    }
}

/// <summary>
/// Long-range scan command.
/// </summary>
public class LongRangeScanCommand : BaseCommand
{
    public override string Name => "LRSCAN";
    public override string Abbreviation => "LR";
    public override string HelpText => "Perform a long-range scan of surrounding quadrants.";

    public LongRangeScanCommand(GameEngine engine) : base(engine) { }

    public override Task<CommandResult> ExecuteAsync(GameState state, string[] args)
    {
        if (!state.Ship.IsDeviceOperational(DeviceType.LongRangeSensors))
        {
            Error("Long-range sensors are damaged and inoperative.");
            return Task.FromResult(CommandResult.Fail("Long-range sensors damaged."));
        }

        var ship = state.Ship;
        Message($"\nLong-range scan from quadrant {ship.Quadrant}:");
        Message("    1   2   3   4   5   6   7   8");

        for (int x = ship.Quadrant.X - 1; x <= ship.Quadrant.X + 1; x++)
        {
            string line = $" {x}: ";
            for (int y = ship.Quadrant.Y - 1; y <= ship.Quadrant.Y + 1; y++)
            {
                if (x < 1 || x > 8 || y < 1 || y > 8)
                {
                    line += " ***";
                }
                else
                {
                    var coord = new QuadrantCoordinate(x, y);
                    int data = state.Galaxy.GetQuadrantData(coord);
                    state.Galaxy.UpdateChart(coord);

                    int klingons = data / 100;
                    int bases = (data % 100) / 10;
                    int stars = data % 10;

                    line += $" {klingons}{bases}{stars}";
                }
            }
            Message(line);
        }

        return Task.FromResult(CommandResult.Ok());
    }
}

/// <summary>
/// Star chart command.
/// </summary>
public class ChartCommand : BaseCommand
{
    public override string Name => "CHART";
    public override string Abbreviation => "CH";
    public override string HelpText => "Display the star chart of known galaxy data.";

    public ChartCommand(GameEngine engine) : base(engine) { }

    public override Task<CommandResult> ExecuteAsync(GameState state, string[] args)
    {
        Message("\n       STAR CHART");
        Message("    1   2   3   4   5   6   7   8");
        Message("  +---+---+---+---+---+---+---+---+");

        for (int x = 1; x <= 8; x++)
        {
            string line = $"{x} |";
            for (int y = 1; y <= 8; y++)
            {
                var coord = new QuadrantCoordinate(x, y);
                int chartData = state.Galaxy.GetChartData(coord);

                if (chartData == 0)
                {
                    // Unknown
                    line += "...|";
                }
                else if (chartData >= 1000)
                {
                    // Supernova
                    line += "***|";
                }
                else
                {
                    int data = chartData - 1; // Remove the +1 offset
                    int k = data / 100;
                    int b = (data % 100) / 10;
                    int s = data % 10;
                    line += $"{k}{b}{s}|";
                }
            }
            Message(line);
            Message("  +---+---+---+---+---+---+---+---+");
        }

        var ship = state.Ship;
        Message($"\nCurrent position: Quadrant {ship.Quadrant}, Sector {ship.Sector}");

        return Task.FromResult(CommandResult.Ok());
    }
}
