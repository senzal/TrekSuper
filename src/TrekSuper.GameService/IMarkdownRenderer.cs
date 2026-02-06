using TrekSuper.Core;
using TrekSuper.Shared;

namespace TrekSuper.GameService;

/// <summary>
/// Renders game state as markdown for display on clients.
/// </summary>
public interface IMarkdownRenderer
{
    /// <summary>
    /// Renders complete game display including all sections.
    /// </summary>
    GameDisplayData RenderGameDisplay(GameEngine engine, List<string> recentMessages);

    /// <summary>
    /// Renders short-range sector scan.
    /// </summary>
    string RenderSectorScan(GameEngine engine);

    /// <summary>
    /// Renders galaxy map/chart.
    /// </summary>
    string RenderGalaxyMap(GameEngine engine);

    /// <summary>
    /// Renders ship status.
    /// </summary>
    string RenderStatus(GameEngine engine);

    /// <summary>
    /// Renders damage report.
    /// </summary>
    string RenderDamageReport(GameEngine engine);

    /// <summary>
    /// Generates Mermaid diagram for sector scan.
    /// </summary>
    string? RenderSectorMermaid(GameEngine engine);

    /// <summary>
    /// Gets available commands menu.
    /// </summary>
    List<MenuOption> GetAvailableCommands(GameEngine engine);
}
