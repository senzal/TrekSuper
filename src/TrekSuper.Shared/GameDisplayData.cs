namespace TrekSuper.Shared;

/// <summary>
/// Complete display data sent from service to client.
/// </summary>
public record GameDisplayData
{
    /// <summary>
    /// Main content to display (in Markdown format).
    /// </summary>
    public string MarkdownContent { get; init; } = string.Empty;

    /// <summary>
    /// Optional Mermaid diagram for visualizations (galaxy map, sector scan, etc.).
    /// </summary>
    public string? MermaidDiagram { get; init; }

    /// <summary>
    /// List of available commands in current context.
    /// </summary>
    public List<MenuOption> AvailableCommands { get; init; } = new();

    /// <summary>
    /// Current game status summary.
    /// </summary>
    public GameStatusData Status { get; init; } = new();

    /// <summary>
    /// Recent messages from game (combat, events, warnings).
    /// </summary>
    public List<GameMessage> Messages { get; init; } = new();
}

/// <summary>
/// A menu option/command available to the player.
/// </summary>
public record MenuOption(
    string Command,
    string Description,
    string Abbreviation);

/// <summary>
/// Summary of current game status for display.
/// </summary>
public record GameStatusData
{
    public double Stardate { get; init; }
    public double TimeRemaining { get; init; }
    public int Energy { get; init; }
    public int Shield { get; init; }
    public int Torpedoes { get; init; }
    public string Condition { get; init; } = string.Empty;
    public int RemainingKlingons { get; init; }
    public int RemainingCommanders { get; init; }
    public int RemainingSuperCommanders { get; init; }
    public int RemainingBases { get; init; }
    public string QuadrantName { get; init; } = string.Empty;
    public string Position { get; init; } = string.Empty;
}

/// <summary>
/// A message from the game.
/// </summary>
public record GameMessage(
    MessageType Type,
    string Content,
    DateTime Timestamp);

/// <summary>
/// Type of game message.
/// </summary>
public enum MessageType
{
    Info,
    Warning,
    Error,
    Combat,
    Event,
    Success
}
