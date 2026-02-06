using TrekSuper.Core.Models;

namespace TrekSuper.Core.Commands;

/// <summary>
/// Interface for all game commands.
/// </summary>
public interface IGameCommand
{
    /// <summary>
    /// The command name (for matching user input).
    /// </summary>
    string Name { get; }

    /// <summary>
    /// Abbreviated command name.
    /// </summary>
    string Abbreviation { get; }

    /// <summary>
    /// Help text for this command.
    /// </summary>
    string HelpText { get; }

    /// <summary>
    /// Detailed help text shown when user types "HELP commandname".
    /// Returns null if no detailed help is available.
    /// </summary>
    string? DetailedHelpText { get; }

    /// <summary>
    /// Executes the command.
    /// </summary>
    Task<CommandResult> ExecuteAsync(GameState state, string[] args);

    /// <summary>
    /// Checks if this command can be executed in the current state.
    /// </summary>
    bool CanExecute(GameState state);
}
