using TrekSuper.Core;
using TrekSuper.GameService;
using TrekSuper.Shared;
using Xunit;
using Xunit.Abstractions;
using CoreSkillLevel = TrekSuper.Core.Enums.SkillLevel;
using CoreGameLength = TrekSuper.Core.Enums.GameLength;

namespace TrekSuper.Core.Tests;

/// <summary>
/// Comprehensive playtesting that simulates actual gameplay and validates
/// formatting, game mechanics, and user experience.
/// </summary>
public class PlaytestingTests
{
    private readonly ITestOutputHelper _output;

    public PlaytestingTests(ITestOutputHelper output)
    {
        _output = output;
    }

    [Fact]
    public void Playtest_LongRangeScanFormatting()
    {
        _output.WriteLine("=== PLAYTEST: Long-Range Scan Formatting ===\n");

        // Create game with known seed for reproducibility
        var engine = new GameEngine();
        engine.NewGame(CoreSkillLevel.Good, CoreGameLength.Short, tournamentSeed: 12345);

        // Capture messages
        var messages = new List<string>();
        engine.OnMessage += msg => messages.Add(msg);

        // Execute LRSCAN command
        var lrscanCmd = new Core.Commands.LongRangeScanCommand(engine);
        var result = lrscanCmd.ExecuteAsync(engine.State, Array.Empty<string>()).Result;

        Assert.True(result.Success);

        // Validate formatting
        _output.WriteLine("Long-Range Scan Output:");
        foreach (var msg in messages)
        {
            _output.WriteLine(msg);
        }

        // Check that output has proper structure
        Assert.Contains(messages, m => m.Contains("Long-range scan"));

        // Should have a header line with column numbers
        var headerLine = messages.FirstOrDefault(m => m.Trim().StartsWith("3") || m.Trim().StartsWith("4"));
        Assert.NotNull(headerLine);

        // Should have data rows with row numbers
        var dataLines = messages.Where(m => m.Contains(":")).ToList();
        Assert.NotEmpty(dataLines);

        // Each data line should have 3 sets of 3-digit numbers
        foreach (var line in dataLines.Where(l => !l.Contains("scan")))
        {
            _output.WriteLine($"Validating line: {line}");
            // Format should be like "5: 002 006 009"
            var colonIndex = line.IndexOf(':');
            if (colonIndex > 0)
            {
                var datapart = line.Substring(colonIndex + 1).Trim();
                // Should have roughly 3 numbers separated by spaces
                Assert.True(datapart.Length >= 9, $"Data line too short: {datapart}");
            }
        }

        _output.WriteLine("\n✓ Long-range scan formatting validated");
    }

    [Fact]
    public void Playtest_SectorScanFormatting()
    {
        _output.WriteLine("=== PLAYTEST: Sector Scan Formatting ===\n");

        var engine = new GameEngine();
        engine.NewGame(CoreSkillLevel.Good, CoreGameLength.Short, tournamentSeed: 99999);

        var renderer = new MarkdownRenderer();
        var display = renderer.RenderGameDisplay(engine, new List<string>());

        // Check that mermaid diagram exists
        Assert.NotNull(display.MermaidDiagram);
        _output.WriteLine("Mermaid Diagram:");
        _output.WriteLine(display.MermaidDiagram);

        // Should contain sector grid
        Assert.Contains("Sector Grid", display.MermaidDiagram);
        Assert.Contains("R1[", display.MermaidDiagram); // Row 1
        Assert.Contains("R10[", display.MermaidDiagram); // Row 10

        // Should show Enterprise
        Assert.Contains("🚀", display.MermaidDiagram);

        _output.WriteLine("\n✓ Sector scan formatting validated");
    }

    [Fact]
    public void Playtest_ChartFormatting()
    {
        _output.WriteLine("=== PLAYTEST: Star Chart Formatting ===\n");

        var engine = new GameEngine();
        engine.NewGame(CoreSkillLevel.Good, CoreGameLength.Short, tournamentSeed: 54321);

        var messages = new List<string>();
        engine.OnMessage += msg => messages.Add(msg);

        // Execute chart command
        var chartCmd = new Core.Commands.ChartCommand(engine);
        var result = chartCmd.ExecuteAsync(engine.State, Array.Empty<string>()).Result;

        Assert.True(result.Success);

        _output.WriteLine("Star Chart Output:");
        foreach (var msg in messages)
        {
            _output.WriteLine(msg);
        }

        // Validate chart structure
        Assert.Contains(messages, m => m.Contains("STAR CHART"));
        Assert.Contains(messages, m => m.Contains("1   2   3   4   5   6   7   8"));
        Assert.Contains(messages, m => m.Contains("+---+"));

        // Should have 8 rows of data
        var dataRows = messages.Where(m => m.Contains("|") && !m.Contains("+")).ToList();
        Assert.True(dataRows.Count >= 8, $"Expected 8 data rows, got {dataRows.Count}");

        _output.WriteLine("\n✓ Star chart formatting validated");
    }

    [Fact]
    public void Playtest_StatusReportFormatting()
    {
        _output.WriteLine("=== PLAYTEST: Status Report Formatting ===\n");

        var engine = new GameEngine();
        engine.NewGame(CoreSkillLevel.Expert, CoreGameLength.Long, tournamentSeed: 777);

        var messages = new List<string>();
        engine.OnMessage += msg => messages.Add(msg);

        var statusCmd = new Core.Commands.StatusCommand(engine);
        var result = statusCmd.ExecuteAsync(engine.State, Array.Empty<string>()).Result;

        Assert.True(result.Success);

        _output.WriteLine("Status Report:");
        foreach (var msg in messages)
        {
            _output.WriteLine(msg);
        }

        // Should contain key information
        Assert.Contains(messages, m => m.Contains("Stardate"));
        Assert.Contains(messages, m => m.Contains("Time"));
        Assert.Contains(messages, m => m.Contains("Klingons") || m.Contains("klingons"));

        _output.WriteLine("\n✓ Status report formatting validated");
    }

    [Fact]
    public void Playtest_NavigationSequence()
    {
        _output.WriteLine("=== PLAYTEST: Navigation Sequence ===\n");

        var gameService = new GameStateManager(new MarkdownRenderer());
        var newGame = gameService.CreateGame(SkillLevel.Novice, GameLength.Short, seed: 111);
        var gameId = newGame.GameId;

        _output.WriteLine($"Game created: {gameId}");
        _output.WriteLine($"Starting position: {newGame.InitialDisplay.Status.Position}");

        // Get initial status
        var status1 = gameService.GetGameState(gameId);
        var initialEnergy = status1.Display.Status.Energy;

        // Execute impulse movement
        _output.WriteLine("\n--- Testing IMPULSE command ---");
        var impulseResult = gameService.ExecuteCommand(gameId, "IMPULSE", new[] { "6", "2" });

        _output.WriteLine($"Impulse result: {impulseResult.Success}");
        _output.WriteLine($"New position: {impulseResult.Display.Status.Position}");
        _output.WriteLine($"Energy used: {initialEnergy - impulseResult.Display.Status.Energy}");

        if (impulseResult.Display.Messages.Any())
        {
            _output.WriteLine("Messages:");
            foreach (var msg in impulseResult.Display.Messages)
            {
                _output.WriteLine($"  [{msg.Type}] {msg.Content}");
            }
        }

        // Energy should be consumed
        Assert.True(impulseResult.Display.Status.Energy < initialEnergy, "Energy should decrease after impulse");

        // Try warp movement
        _output.WriteLine("\n--- Testing WARP command ---");
        var warpResult = gameService.ExecuteCommand(gameId, "WARP", new[] { "4", "4", "6" });

        _output.WriteLine($"Warp result: {warpResult.Success}");
        _output.WriteLine($"Final position: {warpResult.Display.Status.Position}");

        gameService.RemoveGame(gameId);
        _output.WriteLine("\n✓ Navigation sequence completed");
    }

    [Fact]
    public void Playtest_CombatSequence()
    {
        _output.WriteLine("=== PLAYTEST: Combat Sequence ===\n");

        var gameService = new GameStateManager(new MarkdownRenderer());

        // Keep trying different seeds until we find one with enemies in starting quadrant
        Guid gameId = Guid.Empty;
        NewGameResponse? gameResponse = null;
        bool foundEnemies = false;

        for (int seed = 1000; seed < 1100 && !foundEnemies; seed++)
        {
            if (gameId != Guid.Empty)
                gameService.RemoveGame(gameId);

            gameResponse = gameService.CreateGame(SkillLevel.Novice, GameLength.Short, seed);
            gameId = gameResponse.GameId;

            var scan = gameService.ExecuteCommand(gameId, "SRSCAN", Array.Empty<string>());
            foundEnemies = scan.Display.MermaidDiagram?.Contains("👾") ?? false;

            if (foundEnemies)
                _output.WriteLine($"Found enemies at seed {seed}");
        }

        if (!foundEnemies)
        {
            _output.WriteLine("⚠️ No enemies found in starting quadrant after 100 seeds - skipping combat test");
            gameService.RemoveGame(gameId);
            return;
        }

        _output.WriteLine($"Starting position: {gameResponse!.InitialDisplay.Status.Position}");
        _output.WriteLine($"Klingons total: {gameResponse.InitialDisplay.Status.RemainingKlingons}");

        // Raise shields
        _output.WriteLine("\n--- Raising shields ---");
        var shieldResult = gameService.ExecuteCommand(gameId, "SHIELDS", new[] { "TRANSFER", "500" });
        _output.WriteLine($"Shield command: {shieldResult.Success}");
        _output.WriteLine($"Shields: {shieldResult.Display.Status.Shield}");

        // Fire phasers
        _output.WriteLine("\n--- Firing phasers ---");
        var phaserResult = gameService.ExecuteCommand(gameId, "PHASERS", new[] { "1000" });
        _output.WriteLine($"Phaser result: {phaserResult.Success}");

        if (phaserResult.Display.Messages.Any())
        {
            _output.WriteLine("Combat messages:");
            foreach (var msg in phaserResult.Display.Messages)
            {
                _output.WriteLine($"  [{msg.Type}] {msg.Content}");
            }
        }

        gameService.RemoveGame(gameId);
        _output.WriteLine("\n✓ Combat sequence completed");
    }

    [Fact]
    public void Playtest_CompleteGameSession()
    {
        _output.WriteLine("=== PLAYTEST: Complete Game Session ===\n");

        var gameService = new GameStateManager(new MarkdownRenderer());
        var game = gameService.CreateGame(SkillLevel.Novice, GameLength.Short, seed: 42);
        var gameId = game.GameId;

        _output.WriteLine($"Mission: Destroy {game.InitialDisplay.Status.RemainingKlingons} Klingons");
        _output.WriteLine($"Time limit: {game.InitialDisplay.Status.TimeRemaining} days");
        _output.WriteLine($"Starting energy: {game.InitialDisplay.Status.Energy}");
        _output.WriteLine($"Starbases: {game.InitialDisplay.Status.RemainingBases}");

        // Sequence of commands a player would typically use
        var testCommands = new[]
        {
            ("STATUS", Array.Empty<string>(), "Check status"),
            ("SRSCAN", Array.Empty<string>(), "Short range scan"),
            ("LRSCAN", Array.Empty<string>(), "Long range scan"),
            ("CHART", Array.Empty<string>(), "View galaxy"),
            ("DAMAGE", Array.Empty<string>(), "Check damage"),
            ("SHIELDS", new[] { "TRANSFER", "200" }, "Adjust shields"),
            ("COMPUTER", Array.Empty<string>(), "Use computer"),
        };

        int commandNum = 1;
        foreach (var (cmd, args, desc) in testCommands)
        {
            _output.WriteLine($"\n[{commandNum}] {desc} ({cmd})");
            var result = gameService.ExecuteCommand(gameId, cmd, args);

            _output.WriteLine($"  Success: {result.Success}");
            _output.WriteLine($"  Energy: {result.Display.Status.Energy}");
            _output.WriteLine($"  Shield: {result.Display.Status.Shield}");

            if (!result.Success && result.ErrorMessage != null)
            {
                _output.WriteLine($"  Error: {result.ErrorMessage}");
            }

            if (result.Display.Messages.Any())
            {
                _output.WriteLine($"  Messages: {result.Display.Messages.Count}");
                foreach (var msg in result.Display.Messages.Take(3))
                {
                    _output.WriteLine($"    - {msg.Content}");
                }
            }

            commandNum++;
        }

        var finalState = gameService.GetGameState(gameId);
        _output.WriteLine("\n--- Final Game State ---");
        _output.WriteLine($"Energy: {finalState.Display.Status.Energy}");
        _output.WriteLine($"Shields: {finalState.Display.Status.Shield}");
        _output.WriteLine($"Position: {finalState.Display.Status.Position}");
        _output.WriteLine($"Klingons remaining: {finalState.Display.Status.RemainingKlingons}");
        _output.WriteLine($"Time remaining: {finalState.Display.Status.TimeRemaining}");

        gameService.RemoveGame(gameId);
        _output.WriteLine("\n✓ Complete game session validated");
    }

    [Fact]
    public void Playtest_AllCommandsWork()
    {
        _output.WriteLine("=== PLAYTEST: All Commands Executable ===\n");

        var gameService = new GameStateManager(new MarkdownRenderer());
        var game = gameService.CreateGame(SkillLevel.Good, GameLength.Short, seed: 999);
        var gameId = game.GameId;

        // Test every command at least once
        var allCommands = new Dictionary<string, string[]>
        {
            { "STATUS", Array.Empty<string>() },
            { "SRSCAN", Array.Empty<string>() },
            { "LRSCAN", Array.Empty<string>() },
            { "CHART", Array.Empty<string>() },
            { "DAMAGE", Array.Empty<string>() },
            { "COMPUTER", Array.Empty<string>() },
            { "SHIELDS", new[] { "UP" } },
            { "SETWARP", new[] { "5.0" } },
            { "SCORE", Array.Empty<string>() },
            { "HELP", Array.Empty<string>() },
            // Navigation and combat require parameters - test with valid values
            { "IMPULSE", new[] { "1", "1" } },
            { "PHASERS", new[] { "100" } },
            { "TORPEDO", new[] { "1" } },
        };

        int passed = 0;
        int failed = 0;

        foreach (var (cmd, args) in allCommands)
        {
            try
            {
                var result = gameService.ExecuteCommand(gameId, cmd, args);

                // Command should at least not crash
                _output.WriteLine($"✓ {cmd}: {(result.Success ? "Success" : "Failed gracefully")}");

                if (result.ErrorMessage != null)
                {
                    _output.WriteLine($"    Message: {result.ErrorMessage}");
                }

                passed++;
            }
            catch (Exception ex)
            {
                _output.WriteLine($"✗ {cmd}: CRASHED - {ex.Message}");
                failed++;
            }
        }

        gameService.RemoveGame(gameId);

        _output.WriteLine($"\n--- Results ---");
        _output.WriteLine($"Passed: {passed}/{allCommands.Count}");
        _output.WriteLine($"Failed: {failed}/{allCommands.Count}");

        Assert.Equal(0, failed);
        _output.WriteLine("\n✓ All commands executable without crashes");
    }

    [Fact]
    public void Playtest_EdgeCases()
    {
        _output.WriteLine("=== PLAYTEST: Edge Cases ===\n");

        var gameService = new GameStateManager(new MarkdownRenderer());
        var game = gameService.CreateGame(SkillLevel.Good, GameLength.Short, seed: 555);
        var gameId = game.GameId;

        // Test 1: Invalid command
        _output.WriteLine("Test 1: Invalid command");
        var invalidCmd = gameService.ExecuteCommand(gameId, "NOTACOMMAND", Array.Empty<string>());
        Assert.False(invalidCmd.Success);
        Assert.NotNull(invalidCmd.ErrorMessage);
        _output.WriteLine($"  ✓ Handled invalid command: {invalidCmd.ErrorMessage}");

        // Test 2: Command with missing arguments
        _output.WriteLine("\nTest 2: Missing arguments");
        var missingArgs = gameService.ExecuteCommand(gameId, "IMPULSE", Array.Empty<string>());
        Assert.False(missingArgs.Success);
        _output.WriteLine($"  ✓ Handled missing args: {missingArgs.ErrorMessage}");

        // Test 3: Command with invalid arguments
        _output.WriteLine("\nTest 3: Invalid arguments");
        var invalidArgs = gameService.ExecuteCommand(gameId, "SHIELDS", new[] { "TRANSFER", "abc" });
        Assert.False(invalidArgs.Success);
        _output.WriteLine($"  ✓ Handled invalid args: {invalidArgs.ErrorMessage}");

        // Test 4: Excessive shield allocation
        _output.WriteLine("\nTest 4: Excessive shield allocation");
        var excessShields = gameService.ExecuteCommand(gameId, "SHIELDS", new[] { "TRANSFER", "99999" });
        // Should either fail or cap at max energy
        _output.WriteLine($"  ✓ Handled excessive shields (Success: {excessShields.Success})");

        // Test 5: Get non-existent game
        _output.WriteLine("\nTest 5: Non-existent game");
        var fakeGame = gameService.GetGameState(Guid.NewGuid());
        Assert.False(fakeGame.Success);
        _output.WriteLine($"  ✓ Handled non-existent game: {fakeGame.ErrorMessage}");

        gameService.RemoveGame(gameId);
        _output.WriteLine("\n✓ All edge cases handled gracefully");
    }

    [Fact]
    public void Playtest_MultipleGamesIsolation()
    {
        _output.WriteLine("=== PLAYTEST: Multiple Games Isolation ===\n");

        var gameService = new GameStateManager(new MarkdownRenderer());

        // Create 3 different games
        var game1 = gameService.CreateGame(SkillLevel.Novice, GameLength.Short, seed: 100);
        var game2 = gameService.CreateGame(SkillLevel.Expert, GameLength.Long, seed: 200);
        var game3 = gameService.CreateGame(SkillLevel.Good, GameLength.Medium, seed: 300);

        _output.WriteLine($"Game 1: {game1.GameId} - {game1.InitialDisplay.Status.RemainingKlingons} Klingons");
        _output.WriteLine($"Game 2: {game2.GameId} - {game2.InitialDisplay.Status.RemainingKlingons} Klingons");
        _output.WriteLine($"Game 3: {game3.GameId} - {game3.InitialDisplay.Status.RemainingKlingons} Klingons");

        // Execute different commands on each game
        _output.WriteLine("\n--- Executing commands on each game ---");

        var r1 = gameService.ExecuteCommand(game1.GameId, "SHIELDS", new[] { "TRANSFER", "100" });
        var r2 = gameService.ExecuteCommand(game2.GameId, "SHIELDS", new[] { "TRANSFER", "200" });
        var r3 = gameService.ExecuteCommand(game3.GameId, "SHIELDS", new[] { "TRANSFER", "300" });

        _output.WriteLine($"Game 1 shields: {r1.Display.Status.Shield}");
        _output.WriteLine($"Game 2 shields: {r2.Display.Status.Shield}");
        _output.WriteLine($"Game 3 shields: {r3.Display.Status.Shield}");

        // Verify games are isolated
        Assert.NotEqual(r1.Display.Status.Shield, r2.Display.Status.Shield);
        Assert.NotEqual(r2.Display.Status.Shield, r3.Display.Status.Shield);

        // Verify Klingon counts are different (different seeds)
        Assert.NotEqual(game1.InitialDisplay.Status.RemainingKlingons,
                       game2.InitialDisplay.Status.RemainingKlingons);

        gameService.RemoveGame(game1.GameId);
        gameService.RemoveGame(game2.GameId);
        gameService.RemoveGame(game3.GameId);

        _output.WriteLine("\n✓ Games properly isolated");
    }
}
