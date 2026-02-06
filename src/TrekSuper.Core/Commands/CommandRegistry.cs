namespace TrekSuper.Core.Commands;

/// <summary>
/// Registry of all available game commands.
/// </summary>
public class CommandRegistry
{
    private readonly Dictionary<string, IGameCommand> _commands = new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<string, IGameCommand> _abbreviations = new(StringComparer.OrdinalIgnoreCase);

    public IReadOnlyDictionary<string, IGameCommand> Commands => _commands;

    public void Register(IGameCommand command)
    {
        _commands[command.Name] = command;
        if (!string.IsNullOrEmpty(command.Abbreviation))
        {
            _abbreviations[command.Abbreviation] = command;
        }
    }

    public IGameCommand? GetCommand(string name)
    {
        if (_commands.TryGetValue(name, out var cmd))
            return cmd;

        if (_abbreviations.TryGetValue(name, out cmd))
            return cmd;

        // Try partial match
        var matches = _commands.Values
            .Where(c => c.Name.StartsWith(name, StringComparison.OrdinalIgnoreCase))
            .ToList();

        if (matches.Count == 1)
            return matches[0];

        return null;
    }

    public IEnumerable<IGameCommand> GetAllCommands() => _commands.Values;
}
