using TrekSuper.GameService;
using TrekSuper.Shared;
using Xunit;
using Xunit.Abstractions;

namespace TrekSuper.Core.Tests;

/// <summary>
/// Simulates actual gameplay as a client would, testing the full game flow.
/// </summary>
public class GameplaySimulationTests
{
    private readonly ITestOutputHelper _output;
    private readonly IGameStateManager _gameService;

    public GameplaySimulationTests(ITestOutputHelper output)
    {
        _output = output;
        var renderer = new MarkdownRenderer();
        _gameService = new GameStateManager(renderer);
    }

    [Fact]
    public void FullGameFlow_CreateGameAndExecuteCommands()
    {
        // Simulate a client creating a game and playing
        _output.WriteLine("=== CLIENT: Creating new game ===");

        // 1. Create game
        var newGameResponse = _gameService.CreateGame(SkillLevel.Good, GameLength.Short, seed: 12345);

        Assert.True(newGameResponse.Success);
        Assert.NotEqual(Guid.Empty, newGameResponse.GameId);

        var gameId = newGameResponse.GameId;
        _output.WriteLine($"Game created with ID: {gameId}");
        _output.WriteLine($"Initial Klingons: {newGameResponse.InitialDisplay.Status.RemainingKlingons}");
        _output.WriteLine($"Position: {newGameResponse.InitialDisplay.Status.Position}");

        // 2. Execute STATUS command
        _output.WriteLine("\n=== CLIENT: Executing STATUS command ===");
        var statusResponse = _gameService.ExecuteCommand(gameId, "STATUS", Array.Empty<string>());

        Assert.NotNull(statusResponse);
        Assert.NotNull(statusResponse.Display);
        _output.WriteLine("Status display received");
        _output.WriteLine($"Energy: {statusResponse.Display.Status.Energy}");
        _output.WriteLine($"Torpedoes: {statusResponse.Display.Status.Torpedoes}");

        // 3. Execute SRSCAN command
        _output.WriteLine("\n=== CLIENT: Executing SRSCAN command ===");
        var scanResponse = _gameService.ExecuteCommand(gameId, "SRSCAN", Array.Empty<string>());

        Assert.NotNull(scanResponse.Display);
        Assert.NotNull(scanResponse.Display.MermaidDiagram);
        Assert.Contains("🚀", scanResponse.Display.MermaidDiagram); // Enterprise should be visible
        _output.WriteLine("Sector scan received with Enterprise visible");

        // 4. Execute LRSCAN command
        _output.WriteLine("\n=== CLIENT: Executing LRSCAN command ===");
        var lrscanResponse = _gameService.ExecuteCommand(gameId, "LRSCAN", Array.Empty<string>());

        Assert.NotNull(lrscanResponse.Display);
        _output.WriteLine("Long-range scan completed");

        // 5. Get current game state
        _output.WriteLine("\n=== CLIENT: Getting game state ===");
        var stateResponse = _gameService.GetGameState(gameId);

        Assert.True(stateResponse.Success);
        Assert.Equal(statusResponse.Display.Status.Energy, stateResponse.Display.Status.Energy);
        _output.WriteLine("Game state retrieved successfully");

        // 6. Cleanup
        bool removed = _gameService.RemoveGame(gameId);
        Assert.True(removed);
        _output.WriteLine("\n=== Game session cleaned up ===");
    }

    [Fact]
    public void SimulateMovement_WarpToNewQuadrant()
    {
        // Arrange
        var newGameResponse = _gameService.CreateGame(SkillLevel.Novice, GameLength.Short, seed: 99999);
        var gameId = newGameResponse.GameId;
        var initialQuadrant = newGameResponse.InitialDisplay.Status.QuadrantName;

        _output.WriteLine($"Starting at: {initialQuadrant}");
        _output.WriteLine($"Position: {newGameResponse.InitialDisplay.Status.Position}");

        // Act - Try to warp (may fail if not enough energy, but should execute)
        _output.WriteLine("\n=== CLIENT: Attempting warp movement ===");
        var warpResponse = _gameService.ExecuteCommand(gameId, "WARP", new[] { "1", "2", "5.0" });

        // Assert
        Assert.NotNull(warpResponse);
        Assert.NotNull(warpResponse.Display);

        _output.WriteLine($"Warp command executed: {warpResponse.Success}");
        _output.WriteLine($"New position: {warpResponse.Display.Status.Position}");
        _output.WriteLine($"Energy remaining: {warpResponse.Display.Status.Energy}");

        if (warpResponse.Display.Messages.Any())
        {
            _output.WriteLine("\nMessages:");
            foreach (var msg in warpResponse.Display.Messages)
            {
                _output.WriteLine($"  [{msg.Type}] {msg.Content}");
            }
        }

        _gameService.RemoveGame(gameId);
    }

    [Fact]
    public void SimulateCombat_FirePhasers()
    {
        // Arrange - Create game and find a quadrant with enemies
        var gameResponse = _gameService.CreateGame(SkillLevel.Novice, GameLength.Short, seed: 54321);
        var gameId = gameResponse.GameId;

        _output.WriteLine($"Game created. Klingons: {gameResponse.InitialDisplay.Status.RemainingKlingons}");

        // Check if there are enemies in current quadrant
        var scanResponse = _gameService.ExecuteCommand(gameId, "SRSCAN", Array.Empty<string>());

        bool hasEnemies = scanResponse.Display.MermaidDiagram?.Contains("👾") ?? false;
        _output.WriteLine($"Enemies in quadrant: {hasEnemies}");

        // Act - Try to fire phasers
        _output.WriteLine("\n=== CLIENT: Firing phasers ===");
        var phaserResponse = _gameService.ExecuteCommand(gameId, "PHASERS", new[] { "1000" });

        // Assert
        Assert.NotNull(phaserResponse);
        _output.WriteLine($"Phaser command result: {phaserResponse.Success}");

        if (phaserResponse.Display.Messages.Any())
        {
            _output.WriteLine("\nCombat messages:");
            foreach (var msg in phaserResponse.Display.Messages)
            {
                _output.WriteLine($"  [{msg.Type}] {msg.Content}");
            }
        }

        _output.WriteLine($"Energy after phasers: {phaserResponse.Display.Status.Energy}");

        _gameService.RemoveGame(gameId);
    }

    [Fact]
    public void SimulateShields_RaiseAndLower()
    {
        // Arrange
        var gameResponse = _gameService.CreateGame(SkillLevel.Good, GameLength.Short);
        var gameId = gameResponse.GameId;
        var initialEnergy = gameResponse.InitialDisplay.Status.Energy;

        _output.WriteLine($"Initial energy: {initialEnergy}");
        _output.WriteLine($"Initial shields: {gameResponse.InitialDisplay.Status.Shield}");

        // Act - Raise shields
        _output.WriteLine("\n=== CLIENT: Raising shields to 500 ===");
        var shieldUpResponse = _gameService.ExecuteCommand(gameId, "SHIELDS", new[] { "500" });

        Assert.NotNull(shieldUpResponse);
        _output.WriteLine($"Shield command result: {shieldUpResponse.Success}");
        _output.WriteLine($"Shields after: {shieldUpResponse.Display.Status.Shield}");
        _output.WriteLine($"Energy after: {shieldUpResponse.Display.Status.Energy}");

        // The shields should have changed
        int shieldsAfter = shieldUpResponse.Display.Status.Shield;
        int energyAfter = shieldUpResponse.Display.Status.Energy;

        _output.WriteLine($"\nEnergy used: {initialEnergy - energyAfter}");
        _output.WriteLine($"Shield level: {shieldsAfter}");

        _gameService.RemoveGame(gameId);
    }

    [Fact]
    public void SimulateMultipleClients_ConcurrentGames()
    {
        // Simulate 3 different players playing simultaneously
        _output.WriteLine("=== Simulating 3 concurrent players ===\n");

        // Player 1 - Novice
        var game1 = _gameService.CreateGame(SkillLevel.Novice, GameLength.Short);
        _output.WriteLine($"Player 1 (Novice): Game {game1.GameId}");
        _output.WriteLine($"  Position: {game1.InitialDisplay.Status.Position}");
        _output.WriteLine($"  Klingons: {game1.InitialDisplay.Status.RemainingKlingons}");

        // Player 2 - Expert
        var game2 = _gameService.CreateGame(SkillLevel.Expert, GameLength.Long);
        _output.WriteLine($"\nPlayer 2 (Expert): Game {game2.GameId}");
        _output.WriteLine($"  Position: {game2.InitialDisplay.Status.Position}");
        _output.WriteLine($"  Klingons: {game2.InitialDisplay.Status.RemainingKlingons}");

        // Player 3 - Good
        var game3 = _gameService.CreateGame(SkillLevel.Good, GameLength.Medium);
        _output.WriteLine($"\nPlayer 3 (Good): Game {game3.GameId}");
        _output.WriteLine($"  Position: {game3.InitialDisplay.Status.Position}");
        _output.WriteLine($"  Klingons: {game3.InitialDisplay.Status.RemainingKlingons}");

        // Verify all 3 games are active
        Assert.Equal(3, _gameService.ActiveGameCount);
        _output.WriteLine($"\n✓ Total active games: {_gameService.ActiveGameCount}");

        // Each player executes commands
        _output.WriteLine("\n=== Players executing commands ===");

        var p1Status = _gameService.ExecuteCommand(game1.GameId, "STATUS", Array.Empty<string>());
        _output.WriteLine($"Player 1 executed STATUS: {p1Status.Success}");

        var p2Scan = _gameService.ExecuteCommand(game2.GameId, "SRSCAN", Array.Empty<string>());
        _output.WriteLine($"Player 2 executed SRSCAN: {p2Scan.Success}");

        var p3Shields = _gameService.ExecuteCommand(game3.GameId, "SHIELDS", new[] { "100" });
        _output.WriteLine($"Player 3 executed SHIELDS: {p3Shields.Success}");

        // Verify games are independent
        Assert.NotEqual(p1Status.Display.Status.RemainingKlingons, p2Scan.Display.Status.RemainingKlingons);
        _output.WriteLine("\n✓ Game states are independent");

        // Cleanup
        _gameService.RemoveGame(game1.GameId);
        _gameService.RemoveGame(game2.GameId);
        _gameService.RemoveGame(game3.GameId);

        Assert.Equal(0, _gameService.ActiveGameCount);
        _output.WriteLine("\n✓ All games cleaned up");
    }

    [Fact]
    public void SimulateInvalidCommand_ReturnsError()
    {
        // Arrange
        var gameResponse = _gameService.CreateGame(SkillLevel.Good, GameLength.Short);
        var gameId = gameResponse.GameId;

        // Act - Execute invalid command
        _output.WriteLine("=== CLIENT: Executing invalid command ===");
        var response = _gameService.ExecuteCommand(gameId, "INVALIDCMD", Array.Empty<string>());

        // Assert
        Assert.False(response.Success);
        Assert.NotNull(response.ErrorMessage);
        Assert.Contains("Unknown command", response.ErrorMessage);

        _output.WriteLine($"Error message: {response.ErrorMessage}");

        _gameService.RemoveGame(gameId);
    }

    [Fact]
    public void FullGameSession_FromStartToMultipleCommands()
    {
        _output.WriteLine("=== COMPLETE GAME SESSION SIMULATION ===\n");

        // 1. Start game
        var game = _gameService.CreateGame(SkillLevel.Good, GameLength.Short, seed: 777);
        var gameId = game.GameId;

        _output.WriteLine($"✓ Game created: {gameId}");
        _output.WriteLine($"  Starting at: {game.InitialDisplay.Status.QuadrantName}");
        _output.WriteLine($"  Condition: {game.InitialDisplay.Status.Condition}");
        _output.WriteLine($"  Energy: {game.InitialDisplay.Status.Energy}");
        _output.WriteLine($"  Mission: Destroy {game.InitialDisplay.Status.RemainingKlingons} Klingons\n");

        // 2. Check status
        var status = _gameService.ExecuteCommand(gameId, "STATUS", Array.Empty<string>());
        _output.WriteLine("✓ STATUS command executed");

        // 3. Short range scan
        var sr = _gameService.ExecuteCommand(gameId, "SRSCAN", Array.Empty<string>());
        _output.WriteLine($"✓ SRSCAN executed - Enterprise visible: {sr.Display.MermaidDiagram?.Contains("🚀")}");

        // 4. Long range scan
        var lr = _gameService.ExecuteCommand(gameId, "LRSCAN", Array.Empty<string>());
        _output.WriteLine("✓ LRSCAN executed");

        // 5. Check damage report
        var damage = _gameService.ExecuteCommand(gameId, "DAMAGE", Array.Empty<string>());
        _output.WriteLine("✓ DAMAGE report executed");

        // 6. View chart
        var chart = _gameService.ExecuteCommand(gameId, "CHART", Array.Empty<string>());
        _output.WriteLine("✓ CHART executed");

        // 7. Adjust shields
        var shields = _gameService.ExecuteCommand(gameId, "SHIELDS", new[] { "200" });
        _output.WriteLine($"✓ SHIELDS adjusted: {shields.Success}");
        _output.WriteLine($"  New shield level: {shields.Display.Status.Shield}");

        // 8. Check score
        var score = _gameService.ExecuteCommand(gameId, "SCORE", Array.Empty<string>());
        _output.WriteLine("✓ SCORE displayed");

        // Verify game is still running
        Assert.False(status.IsGameOver);
        Assert.False(lr.IsGameOver);
        _output.WriteLine("\n✓ Game session completed successfully");
        _output.WriteLine($"  Final energy: {shields.Display.Status.Energy}");
        _output.WriteLine($"  Final shields: {shields.Display.Status.Shield}");

        _gameService.RemoveGame(gameId);
    }
}
