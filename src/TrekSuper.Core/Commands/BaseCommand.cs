using TrekSuper.Core.Models;

namespace TrekSuper.Core.Commands;

/// <summary>
/// Base class for game commands with common functionality.
/// </summary>
public abstract class BaseCommand : IGameCommand
{
    protected readonly GameEngine Engine;

    public abstract string Name { get; }
    public abstract string Abbreviation { get; }
    public abstract string HelpText { get; }

    /// <summary>
    /// Detailed help text shown when user types "HELP commandname".
    /// Override to provide comprehensive usage instructions.
    /// </summary>
    public virtual string? DetailedHelpText => null;

    protected BaseCommand(GameEngine engine)
    {
        Engine = engine;
    }

    public abstract Task<CommandResult> ExecuteAsync(GameState state, string[] args);

    public virtual bool CanExecute(GameState state) => !state.IsGameOver;

    protected bool TryParseDouble(string s, out double value) =>
        double.TryParse(s, out value);

    protected bool TryParseInt(string s, out int value) =>
        int.TryParse(s, out value);

    protected void Message(string text) => Engine.Message(text);
    protected void Warning(string text) => Engine.Warning(text);
    protected void Error(string text) => Engine.Error(text);
}
