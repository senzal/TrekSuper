using TrekSuper.Shared;

namespace TrekSuper.GameService;

/// <summary>
/// Manages multiple concurrent game sessions.
/// </summary>
public interface IGameStateManager
{
    /// <summary>
    /// Creates a new game and returns its unique ID.
    /// </summary>
    NewGameResponse CreateGame(SkillLevel skill, GameLength length, int? seed = null);

    /// <summary>
    /// Executes a command in the specified game.
    /// </summary>
    ExecuteCommandResponse ExecuteCommand(Guid gameId, string command, string[] args);

    /// <summary>
    /// Gets the current display data for a game.
    /// </summary>
    GetGameStateResponse GetGameState(Guid gameId);

    /// <summary>
    /// Removes a game session (after game over or quit).
    /// </summary>
    bool RemoveGame(Guid gameId);

    /// <summary>
    /// Number of active game sessions.
    /// </summary>
    int ActiveGameCount { get; }

    /// <summary>
    /// Cleanup inactive games (not accessed in over 1 hour).
    /// </summary>
    void CleanupInactiveGames();
}
