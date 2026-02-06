namespace TrekSuper.Shared;

/// <summary>
/// Request to create a new game.
/// </summary>
public record NewGameRequest(
    SkillLevel Skill,
    GameLength Length,
    int? TournamentSeed = null);

/// <summary>
/// Response with new game ID and initial display.
/// </summary>
public record NewGameResponse(
    Guid GameId,
    GameDisplayData InitialDisplay,
    bool Success = true,
    string? ErrorMessage = null);

/// <summary>
/// Request to execute a command in a game.
/// </summary>
public record ExecuteCommandRequest(
    Guid GameId,
    string Command,
    string[] Args);

/// <summary>
/// Response after executing a command.
/// </summary>
public record ExecuteCommandResponse(
    GameDisplayData Display,
    bool Success,
    bool IsGameOver,
    GameOutcome? Outcome = null,
    string? ErrorMessage = null);

/// <summary>
/// Request to get current game state/display.
/// </summary>
public record GetGameStateRequest(Guid GameId);

/// <summary>
/// Response with current game display.
/// </summary>
public record GetGameStateResponse(
    GameDisplayData Display,
    bool Success = true,
    string? ErrorMessage = null);

/// <summary>
/// Skill levels.
/// </summary>
public enum SkillLevel
{
    Novice = 1,
    Fair = 2,
    Good = 3,
    Expert = 4,
    Emeritus = 5
}

/// <summary>
/// Game length options.
/// </summary>
public enum GameLength
{
    Short = 1,
    Medium = 2,
    Long = 4
}

/// <summary>
/// Game outcomes.
/// </summary>
public enum GameOutcome
{
    InProgress,
    Won,
    ShipDestroyed,
    TimeExpired,
    FederationLost,
    SupernovaWhileNotInQuadrant,
    CommanderDestroyedBase,
    Quit
}
