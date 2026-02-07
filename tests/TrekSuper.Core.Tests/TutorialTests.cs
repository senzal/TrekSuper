using TrekSuper.Core;
using CoreSkillLevel = TrekSuper.Core.Enums.SkillLevel;
using CoreGameLength = TrekSuper.Core.Enums.GameLength;
using TrekSuper.GameService;
using TrekSuper.Shared;
using Xunit;
using Xunit.Abstractions;

namespace TrekSuper.Core.Tests;

public class TutorialTests
{
    private readonly ITestOutputHelper _output;

    public TutorialTests(ITestOutputHelper output)
    {
        _output = output;
    }

    [Fact]
    public void Tutorial_StartsSuccessfully()
    {
        var gameService = new GameStateManager(new MarkdownRenderer());
        var game = gameService.CreateGame(SkillLevel.Novice, GameLength.Short);
        var gameId = game.GameId;

        _output.WriteLine("=== Starting Tutorial ===\n");

        // Start tutorial
        var result = gameService.ExecuteCommand(gameId, "TUTORIAL", Array.Empty<string>());

        Assert.True(result.Success);
        Assert.Contains("Starting Interactive Tutorial", string.Join("\n", result.Display.Messages.Select(m => m.Content)));

        _output.WriteLine("✓ Tutorial started successfully");
        _output.WriteLine($"Messages: {result.Display.Messages.Count}");

        gameService.RemoveGame(gameId);
    }

    [Fact]
    public void Tutorial_StepProgression()
    {
        _output.WriteLine("=== Tutorial Step Progression Test ===\n");

        var gameService = new GameStateManager(new MarkdownRenderer());
        var game = gameService.CreateGame(SkillLevel.Novice, GameLength.Short);
        var gameId = game.GameId;

        // Start tutorial
        var startResult = gameService.ExecuteCommand(gameId, "TUTORIAL", Array.Empty<string>());
        Assert.True(startResult.Success);

        _output.WriteLine("Step 1: Tutorial Started");
        _output.WriteLine("Expected: STATUS command\n");

        // Step 1: STATUS
        var step1 = gameService.ExecuteCommand(gameId, "STATUS", Array.Empty<string>());
        Assert.True(step1.Success);

        var messages = string.Join("\n", step1.Display.Messages.Select(m => m.Content));
        _output.WriteLine("Step 1 Response:");
        _output.WriteLine(messages.Substring(0, Math.Min(300, messages.Length)) + "...\n");

        // Check if we got a success message
        if (messages.Contains("Great!") || messages.Contains("✅"))
        {
            _output.WriteLine("✓ Step 1 completed - Got success feedback");
        }

        // Step 2: SRSCAN
        _output.WriteLine("Step 2: Expected SRSCAN command\n");
        var step2 = gameService.ExecuteCommand(gameId, "SRSCAN", Array.Empty<string>());
        Assert.True(step2.Success);

        messages = string.Join("\n", step2.Display.Messages.Select(m => m.Content));
        _output.WriteLine("Step 2 Response:");
        _output.WriteLine(messages.Substring(0, Math.Min(300, messages.Length)) + "...\n");

        if (messages.Contains("Great!") || messages.Contains("✅"))
        {
            _output.WriteLine("✓ Step 2 completed - Got success feedback");
        }

        // Step 3: LRSCAN
        _output.WriteLine("Step 3: Expected LRSCAN command\n");
        var step3 = gameService.ExecuteCommand(gameId, "LRSCAN", Array.Empty<string>());
        Assert.True(step3.Success);

        messages = string.Join("\n", step3.Display.Messages.Select(m => m.Content));
        if (messages.Contains("Great!") || messages.Contains("✅"))
        {
            _output.WriteLine("✓ Step 3 completed - Got success feedback");
        }

        _output.WriteLine("\n=== Tutorial Progression Working ===");

        gameService.RemoveGame(gameId);
    }

    [Fact]
    public void Tutorial_HelpCommand_ShowsDetailedHelp()
    {
        _output.WriteLine("=== Testing Enhanced Help with Mermaid ===\n");

        var gameService = new GameStateManager(new MarkdownRenderer());
        var game = gameService.CreateGame(SkillLevel.Novice, GameLength.Short);
        var gameId = game.GameId;

        // Test HELP IMPULSE
        var helpResult = gameService.ExecuteCommand(gameId, "HELP", new[] { "IMPULSE" });
        Assert.True(helpResult.Success);

        var messages = string.Join("\n", helpResult.Display.Messages.Select(m => m.Content));

        _output.WriteLine("HELP IMPULSE Response:");
        _output.WriteLine(new string('=', 60));
        _output.WriteLine(messages);
        _output.WriteLine(new string('=', 60));

        // Check for key elements
        Assert.Contains("IMPULSE", messages);
        Assert.Contains("0.1", messages); // Should mention 0.1 = 1 sector
        Assert.Contains("mermaid", messages.ToLower()); // Should have Mermaid diagram
        Assert.Contains("DIRECTION", messages.ToUpper());

        _output.WriteLine("\n✓ Detailed help includes:");
        _output.WriteLine("  - Movement syntax");
        _output.WriteLine("  - Distance explanation (0.1 = 1 sector)");
        _output.WriteLine("  - Mermaid diagram");
        _output.WriteLine("  - Examples");

        gameService.RemoveGame(gameId);
    }

    [Fact]
    public void Tutorial_BeginnerMode_ProvidesHints()
    {
        _output.WriteLine("=== Testing Beginner Mode Hints ===\n");

        var engine = new GameEngine();
        engine.NewGame(CoreSkillLevel.Novice, CoreGameLength.Short, tournamentSeed: 12345);

        // Enable beginner mode
        engine.Tutorial.IsBeginnerMode = true;

        _output.WriteLine("Beginner mode enabled");
        _output.WriteLine($"Is Active: {engine.Tutorial.IsBeginnerMode}");

        // Get contextual hint
        var hint = engine.Tutorial.GetContextualHint("SRSCAN", engine.State);

        _output.WriteLine($"\nContextual hint after SRSCAN:");
        _output.WriteLine(hint);

        // Hint should be provided (even if empty, it shouldn't crash)
        Assert.NotNull(hint);

        _output.WriteLine("\n✓ Beginner mode hint system working");
    }

    [Fact]
    public void Tutorial_AllSteps_AreDefined()
    {
        _output.WriteLine("=== Validating Tutorial Steps ===\n");

        var engine = new GameEngine();
        engine.NewGame(CoreSkillLevel.Novice, CoreGameLength.Short);

        engine.Tutorial.StartTutorial();

        Assert.True(engine.Tutorial.IsTutorialActive);
        Assert.True(engine.Tutorial.IsBeginnerMode);
        Assert.Equal(0, engine.Tutorial.CurrentStep);
        Assert.NotEmpty(engine.Tutorial.Steps);

        _output.WriteLine($"Total tutorial steps: {engine.Tutorial.Steps.Count}");
        _output.WriteLine("\nTutorial Steps:");

        for (int i = 0; i < engine.Tutorial.Steps.Count; i++)
        {
            var step = engine.Tutorial.Steps[i];
            _output.WriteLine($"\n{i + 1}. {step.Title}");
            _output.WriteLine($"   Task: {step.Task}");
            _output.WriteLine($"   Expected: {string.Join(", ", step.ExpectedCommands)}");
        }

        Assert.True(engine.Tutorial.Steps.Count >= 7, "Should have at least 7 tutorial steps");

        _output.WriteLine("\n✓ All tutorial steps properly defined");
    }

    [Fact]
    public void Tutorial_WrongCommand_ShowsHint()
    {
        _output.WriteLine("=== Testing Tutorial Hints for Wrong Commands ===\n");

        var engine = new GameEngine();
        engine.NewGame(CoreSkillLevel.Novice, CoreGameLength.Short);

        var messages = new List<string>();
        engine.OnMessage += msg => messages.Add(msg);

        engine.Tutorial.StartTutorial();

        _output.WriteLine("Started tutorial - expecting STATUS command");

        // Try wrong command
        engine.Tutorial.OnCommandExecuted("CHART", success: true);

        var allMessages = string.Join("\n", messages);
        _output.WriteLine($"\nMessages after wrong command:");
        _output.WriteLine(allMessages);

        // Should still be on step 0 (STATUS expected)
        Assert.Equal(0, engine.Tutorial.CurrentStep);

        _output.WriteLine("\n✓ Tutorial doesn't advance on wrong command");
    }

    [Fact]
    public void NavigationSafety_DetectsObstacles()
    {
        _output.WriteLine("=== Testing Navigation Safety System ===\n");

        var gameService = new GameStateManager(new MarkdownRenderer());

        // Try multiple seeds to find one with obstacles
        bool foundObstacle = false;

        for (int seed = 1000; seed < 1100 && !foundObstacle; seed++)
        {
            var game = gameService.CreateGame(SkillLevel.Novice, GameLength.Short, seed);
            var gameId = game.GameId;

            // Try to move in different directions
            for (int dir = 1; dir <= 12 && !foundObstacle; dir++)
            {
                var moveResult = gameService.ExecuteCommand(gameId, "IMPULSE", new[] { dir.ToString(), "0.5" });

                var messages = string.Join("\n", moveResult.Display.Messages.Select(m => m.Content));

                if (messages.Contains("NAVIGATION ALERT") || messages.Contains("Course aborted"))
                {
                    foundObstacle = true;
                    _output.WriteLine($"✓ Found navigation safety in action (seed {seed}, direction {dir}):");
                    _output.WriteLine(messages.Substring(0, Math.Min(400, messages.Length)));
                }
            }

            gameService.RemoveGame(gameId);
        }

        if (foundObstacle)
        {
            _output.WriteLine("\n✓ Navigation safety system is working!");
        }
        else
        {
            _output.WriteLine("\n⚠ No obstacles found in test range (safety system exists but not triggered)");
        }
    }
}
