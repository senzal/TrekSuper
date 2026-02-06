using System.Text;
using TrekSuper.Core;
using TrekSuper.Core.Enums;
using TrekSuper.Shared;

namespace TrekSuper.GameService;

/// <summary>
/// Renders game state as markdown with Mermaid diagrams.
/// </summary>
public class MarkdownRenderer : IMarkdownRenderer
{
    public GameDisplayData RenderGameDisplay(GameEngine engine, List<string> recentMessages)
    {
        var status = new GameStatusData
        {
            Stardate = engine.State.Stardate,
            TimeRemaining = engine.State.TimeRemaining,
            Energy = (int)engine.State.Ship.Energy,
            Shield = (int)engine.State.Ship.Shield,
            Torpedoes = engine.State.Ship.Torpedoes,
            Condition = engine.State.Ship.Condition.ToString(),
            RemainingKlingons = engine.State.RemainingKlingons,
            RemainingCommanders = engine.State.RemainingCommanders,
            RemainingSuperCommanders = engine.State.RemainingSuperCommanders,
            RemainingBases = engine.State.RemainingBases,
            QuadrantName = GameEngine.GetQuadrantName(engine.State.Ship.Quadrant),
            Position = $"{engine.State.Ship.Quadrant.X},{engine.State.Ship.Quadrant.Y} - Sector {engine.State.Ship.Sector.X},{engine.State.Ship.Sector.Y}"
        };

        var messages = recentMessages.Select(msg =>
        {
            var type = msg.StartsWith("ERROR:") ? MessageType.Error :
                       msg.StartsWith("WARN:") ? MessageType.Warning :
                       MessageType.Info;
            var content = msg.Contains(":") ? msg.Substring(msg.IndexOf(":") + 1).Trim() : msg;
            return new GameMessage(type, content, DateTime.UtcNow);
        }).ToList();

        var markdown = new StringBuilder();

        // Title
        markdown.AppendLine($"# USS Enterprise - {status.QuadrantName}");
        markdown.AppendLine();

        // Messages
        if (messages.Any())
        {
            foreach (var msg in messages.TakeLast(5))
            {
                var prefix = msg.Type switch
                {
                    MessageType.Error => "❌",
                    MessageType.Warning => "⚠️",
                    MessageType.Success => "✅",
                    MessageType.Combat => "⚔️",
                    _ => "ℹ️"
                };
                markdown.AppendLine($"{prefix} {msg.Content}");
            }
            markdown.AppendLine();
        }

        // Quick status bar
        markdown.AppendLine($"**Condition:** {GetConditionEmoji(engine.State.Ship.Condition)} {status.Condition} | " +
                           $"**Stardate:** {status.Stardate:F1} | " +
                           $"**Time:** {status.TimeRemaining:F1} days");
        markdown.AppendLine();

        return new GameDisplayData
        {
            MarkdownContent = markdown.ToString(),
            MermaidDiagram = RenderSectorMermaid(engine),
            Status = status,
            Messages = messages,
            AvailableCommands = GetAvailableCommands(engine)
        };
    }

    public string RenderSectorScan(GameEngine engine)
    {
        var sb = new StringBuilder();
        var grid = engine.GetSectorGrid();

        sb.AppendLine($"## {GameEngine.GetQuadrantName(engine.State.Ship.Quadrant)} - Sector Scan");
        sb.AppendLine();
        sb.AppendLine("```");
        sb.AppendLine("   1 2 3 4 5 6 7 8 9 10");
        for (int y = 1; y <= 10; y++)
        {
            sb.Append($"{y,2} ");
            for (int x = 1; x <= 10; x++)
            {
                sb.Append(grid[x, y]);
                sb.Append(' ');
            }
            sb.AppendLine();
        }
        sb.AppendLine("```");
        sb.AppendLine();

        // Legend
        sb.AppendLine("**Legend:** E=Enterprise, K=Klingon, C=Commander, S=Super-Commander, " +
                     "B=Starbase, *=Star, @=Planet, .=Empty");

        return sb.ToString();
    }

    public string RenderGalaxyMap(GameEngine engine)
    {
        var sb = new StringBuilder();
        sb.AppendLine("## Galaxy Map");
        sb.AppendLine();
        sb.AppendLine("```");
        sb.AppendLine("     1   2   3   4   5   6   7   8");

        for (int y = 1; y <= 8; y++)
        {
            sb.Append($"  {y} ");
            for (int x = 1; x <= 8; x++)
            {
                var coord = new Core.Models.QuadrantCoordinate(x, y);
                var chartData = engine.State.Galaxy.GetChartData(coord);
                if (chartData > 0)
                {
                    sb.Append($"{chartData,3} ");
                }
                else
                {
                    sb.Append("... ");
                }
            }
            sb.AppendLine();
        }
        sb.AppendLine("```");
        sb.AppendLine();
        sb.AppendLine("**Format:** KBS where K=Klingons, B=Bases, S=Stars");
        sb.AppendLine("**Unknown quadrants:** ...");

        return sb.ToString();
    }

    public string RenderStatus(GameEngine engine)
    {
        var sb = new StringBuilder();
        var state = engine.State;

        sb.AppendLine("## Status Report");
        sb.AppendLine();

        // Ship Systems
        sb.AppendLine("| System | Value |");
        sb.AppendLine("|--------|-------|");
        sb.AppendLine($"| Condition | {GetConditionEmoji(state.Ship.Condition)} {state.Ship.Condition} |");
        sb.AppendLine($"| Energy | {state.Ship.Energy:F0} |");
        sb.AppendLine($"| Shields | {state.Ship.Shield:F0} {(state.Ship.ShieldsUp ? "(UP)" : "(DOWN)")} |");
        sb.AppendLine($"| Torpedoes | {state.Ship.Torpedoes} |");
        sb.AppendLine($"| Life Support | {state.Ship.LifeSupportReserves:F1} stardates |");
        sb.AppendLine($"| Warp Factor | {state.Ship.WarpFactor:F1} |");
        sb.AppendLine();

        // Mission Progress
        sb.AppendLine("### Mission Progress");
        sb.AppendLine($"- **Stardate:** {state.Stardate:F1}");
        sb.AppendLine($"- **Time Remaining:** {state.TimeRemaining:F1} days");
        sb.AppendLine($"- **Klingons Remaining:** {state.RemainingKlingons}");
        sb.AppendLine($"- **Commanders Remaining:** {state.RemainingCommanders}");
        if (state.RemainingSuperCommanders > 0)
            sb.AppendLine($"- **Super-Commanders:** {state.RemainingSuperCommanders}");
        sb.AppendLine($"- **Starbases:** {state.RemainingBases}");
        sb.AppendLine();

        // Position
        sb.AppendLine("### Current Position");
        sb.AppendLine($"- **Quadrant:** {GameEngine.GetQuadrantName(state.Ship.Quadrant)} ({state.Ship.Quadrant.X},{state.Ship.Quadrant.Y})");
        sb.AppendLine($"- **Sector:** ({state.Ship.Sector.X},{state.Ship.Sector.Y})");
        if (state.Ship.IsDocked)
            sb.AppendLine("- **Docked:** ✅ YES");

        return sb.ToString();
    }

    public string RenderDamageReport(GameEngine engine)
    {
        var sb = new StringBuilder();
        sb.AppendLine("## Damage Report");
        sb.AppendLine();

        var devices = engine.State.Ship.Devices.GetAllDevices();
        var anyDamaged = devices.Any(d => d.Damage < 0);

        if (!anyDamaged)
        {
            sb.AppendLine("✅ All systems operational");
        }
        else
        {
            sb.AppendLine("| Device | Status |");
            sb.AppendLine("|--------|--------|");
            foreach (var device in devices.OrderBy(d => d.Device))
            {
                var status = device.Damage >= 0 ? "✅ OK" : $"❌ Damaged ({Math.Abs(device.Damage):F1})";
                sb.AppendLine($"| {device.Device} | {status} |");
            }
        }

        return sb.ToString();
    }

    public string? RenderSectorMermaid(GameEngine engine)
    {
        if (engine.State.CurrentQuadrant == null)
            return null;

        var sb = new StringBuilder();
        var grid = engine.GetSectorGrid();

        sb.AppendLine("```mermaid");
        sb.AppendLine("graph LR");
        sb.AppendLine("    subgraph \"Sector Grid (10x10)\"");

        for (int y = 1; y <= 10; y++)
        {
            sb.Append($"        R{y}[\"");
            for (int x = 1; x <= 10; x++)
            {
                char c = grid[x, y];
                // Use more descriptive characters
                string cell = c switch
                {
                    'E' => "🚀",
                    'K' => "👾",
                    'C' => "💀",
                    'S' => "☠️",
                    'B' => "🏰",
                    '*' => "⭐",
                    '@' => "🪐",
                    _ => "・"
                };
                sb.Append(cell);
                if (x < 10) sb.Append(' ');
            }
            sb.AppendLine("\"]");
        }

        sb.AppendLine("    end");
        sb.AppendLine("```");

        return sb.ToString();
    }

    public List<MenuOption> GetAvailableCommands(GameEngine engine)
    {
        var commands = new List<MenuOption>
        {
            new("SRSCAN", "Short-range sensor scan", "SR"),
            new("LRSCAN", "Long-range sensor scan", "LR"),
            new("CHART", "Galaxy star chart", "CH"),
            new("STATUS", "Status report", "ST"),
            new("DAMAGE", "Damage report", "DA"),
            new("WARP", "Warp drive movement", "W"),
            new("IMPULSE", "Impulse engine movement", "I"),
            new("PHASERS", "Fire phasers", "PH"),
            new("TORPEDO", "Fire photon torpedoes", "TO"),
            new("SHIELDS", "Shield control", "SH"),
            new("DOCK", "Dock at starbase", "DO"),
            new("REST", "Rest and repair", "R"),
            new("COMPUTER", "Computer functions", "CO"),
            new("SCORE", "Show score", "SC"),
            new("HELP", "Show help", "H"),
            new("QUIT", "Quit game", "Q")
        };

        // Filter based on game state (could add logic to disable unavailable commands)
        return commands;
    }

    private string GetConditionEmoji(Condition condition) => condition switch
    {
        Condition.Docked => "🟢",
        Condition.Green => "🟢",
        Condition.Yellow => "🟡",
        Condition.Red => "🔴",
        _ => "⚪"
    };
}
