using System.Collections.Concurrent;
using TrekSuper.Core;
using TrekSuper.Core.Models;
using TrekSuper.Shared;

namespace TrekSuper.GameService;

/// <summary>
/// Manages multiple concurrent game sessions with in-memory state.
/// </summary>
public class GameStateManager : IGameStateManager
{
    private readonly ConcurrentDictionary<Guid, GameSession> _games = new();
    private readonly IMarkdownRenderer _markdownRenderer;

    public int ActiveGameCount => _games.Count;

    public GameStateManager(IMarkdownRenderer markdownRenderer)
    {
        _markdownRenderer = markdownRenderer;
    }

    public NewGameResponse CreateGame(SkillLevel skill, GameLength length, int? seed = null)
    {
        try
        {
            var gameId = Guid.NewGuid();
            var engine = new GameEngine();

            // Convert from Shared enums to Core enums
            var coreSkill = (Core.Enums.SkillLevel)(int)skill;
            var coreLength = (Core.Enums.GameLength)(int)length;

            engine.NewGame(coreSkill, coreLength, seed);

            var session = new GameSession
            {
                Engine = engine,
                LastAccessed = DateTime.UtcNow,
                MessageHistory = new List<string>()
            };

            // Capture engine messages
            engine.OnMessage += msg => session.MessageHistory.Add($"INFO: {msg}");
            engine.OnWarning += msg => session.MessageHistory.Add($"WARN: {msg}");
            engine.OnError += msg => session.MessageHistory.Add($"ERROR: {msg}");

            _games[gameId] = session;

            var display = _markdownRenderer.RenderGameDisplay(engine, session.MessageHistory);

            return new NewGameResponse(gameId, display, Success: true);
        }
        catch (Exception ex)
        {
            return new NewGameResponse(
                Guid.Empty,
                new GameDisplayData(),
                Success: false,
                ErrorMessage: $"Failed to create game: {ex.Message}");
        }
    }

    public ExecuteCommandResponse ExecuteCommand(Guid gameId, string command, string[] args)
    {
        if (!_games.TryGetValue(gameId, out var session))
        {
            return new ExecuteCommandResponse(
                new GameDisplayData(),
                Success: false,
                IsGameOver: false,
                ErrorMessage: "Game not found");
        }

        session.LastAccessed = DateTime.UtcNow;

        try
        {
            // Clear message history for this turn
            session.MessageHistory.Clear();

            // Execute command via the command registry
            var commandRegistry = new Core.Commands.CommandRegistry();
            RegisterCommands(commandRegistry, session.Engine);

            var gameCommand = commandRegistry.GetCommand(command.ToUpperInvariant());

            if (gameCommand == null)
            {
                return new ExecuteCommandResponse(
                    _markdownRenderer.RenderGameDisplay(session.Engine, session.MessageHistory),
                    Success: false,
                    IsGameOver: session.Engine.State.IsGameOver,
                    ErrorMessage: $"Unknown command: {command}");
            }

            var result = gameCommand.ExecuteAsync(session.Engine.State, args).Result;

            var display = _markdownRenderer.RenderGameDisplay(session.Engine, session.MessageHistory);

            var outcome = session.Engine.State.IsGameOver
                ? (GameOutcome)(int)session.Engine.State.Outcome
                : (GameOutcome?)null;

            return new ExecuteCommandResponse(
                display,
                Success: result.Success,
                IsGameOver: session.Engine.State.IsGameOver,
                Outcome: outcome,
                ErrorMessage: result.Success ? null : result.Message);
        }
        catch (Exception ex)
        {
            return new ExecuteCommandResponse(
                _markdownRenderer.RenderGameDisplay(session.Engine, session.MessageHistory),
                Success: false,
                IsGameOver: session.Engine.State.IsGameOver,
                ErrorMessage: $"Error executing command: {ex.Message}");
        }
    }

    public GetGameStateResponse GetGameState(Guid gameId)
    {
        if (!_games.TryGetValue(gameId, out var session))
        {
            return new GetGameStateResponse(
                new GameDisplayData(),
                Success: false,
                ErrorMessage: "Game not found");
        }

        session.LastAccessed = DateTime.UtcNow;

        var display = _markdownRenderer.RenderGameDisplay(session.Engine, session.MessageHistory);

        return new GetGameStateResponse(display, Success: true);
    }

    public bool RemoveGame(Guid gameId)
    {
        return _games.TryRemove(gameId, out _);
    }

    public void CleanupInactiveGames()
    {
        var cutoff = DateTime.UtcNow.AddHours(-1);
        var inactiveGames = _games
            .Where(kvp => kvp.Value.LastAccessed < cutoff)
            .Select(kvp => kvp.Key)
            .ToList();

        foreach (var gameId in inactiveGames)
        {
            _games.TryRemove(gameId, out _);
        }
    }

    private void RegisterCommands(Core.Commands.CommandRegistry registry, GameEngine engine)
    {
        // Register all available commands
        registry.Register(new Core.Commands.ShortRangeScanCommand(engine));
        registry.Register(new Core.Commands.LongRangeScanCommand(engine));
        registry.Register(new Core.Commands.ChartCommand(engine));
        registry.Register(new Core.Commands.PhaserCommand(engine));
        registry.Register(new Core.Commands.TorpedoCommand(engine));
        registry.Register(new Core.Commands.DeathRayCommand(engine));
        registry.Register(new Core.Commands.WarpCommand(engine));
        registry.Register(new Core.Commands.ImpulseCommand(engine));
        registry.Register(new Core.Commands.DockCommand(engine));
        registry.Register(new Core.Commands.SetWarpCommand(engine));
        registry.Register(new Core.Commands.RestCommand(engine));
        registry.Register(new Core.Commands.ShieldCommand(engine));
        registry.Register(new Core.Commands.StatusCommand(engine));
        registry.Register(new Core.Commands.DamageCommand(engine));
        registry.Register(new Core.Commands.ComputerCommand(engine));
        registry.Register(new Core.Commands.ScoreCommand(engine));
        registry.Register(new Core.Commands.HelpCommand(engine, registry));
        registry.Register(new Core.Commands.QuitCommand(engine));
    }

    private class GameSession
    {
        public GameEngine Engine { get; set; } = null!;
        public DateTime LastAccessed { get; set; }
        public List<string> MessageHistory { get; set; } = new();
    }
}
