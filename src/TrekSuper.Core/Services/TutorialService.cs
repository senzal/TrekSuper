using TrekSuper.Core.Models;

namespace TrekSuper.Core.Services;

/// <summary>
/// Manages interactive tutorial progression and hints.
/// </summary>
public class TutorialService
{
    private readonly GameEngine _engine;
    public bool IsTutorialActive { get; private set; }
    public bool IsBeginnerMode { get; set; }
    public int CurrentStep { get; private set; }
    public List<TutorialStep> Steps { get; private set; }

    public TutorialService(GameEngine engine)
    {
        _engine = engine;
        Steps = new List<TutorialStep>();
        CurrentStep = 0;
    }

    public void StartTutorial()
    {
        IsTutorialActive = true;
        IsBeginnerMode = true;
        CurrentStep = 0;
        InitializeTutorialSteps();
        ShowCurrentStep();
    }

    public void EndTutorial()
    {
        IsTutorialActive = false;
        _engine.Message("\n*** TUTORIAL COMPLETE! ***");
        _engine.Message("You're ready to play Super Star Trek!");
        _engine.Message("Good luck, Captain!\n");
    }

    public void OnCommandExecuted(string commandName, bool success)
    {
        if (!IsTutorialActive || CurrentStep >= Steps.Count)
            return;

        var step = Steps[CurrentStep];

        // Check if this is the expected command
        if (step.ExpectedCommands.Contains(commandName.ToUpperInvariant()))
        {
            if (success)
            {
                _engine.Message($"\n✅ Great! {step.SuccessMessage}");
                CurrentStep++;

                if (CurrentStep >= Steps.Count)
                {
                    EndTutorial();
                }
                else
                {
                    ShowCurrentStep();
                }
            }
        }
        else if (IsBeginnerMode)
        {
            // Show a hint if they're in beginner mode but did wrong command
            _engine.Message($"\n💡 Hint: Try the {step.ExpectedCommands[0]} command instead.");
        }
    }

    public void ShowCurrentStep()
    {
        if (!IsTutorialActive || CurrentStep >= Steps.Count)
            return;

        var step = Steps[CurrentStep];
        _engine.Message("\n" + "=".PadRight(60, '='));
        _engine.Message($"📚 TUTORIAL STEP {CurrentStep + 1}/{Steps.Count}: {step.Title}");
        _engine.Message("=".PadRight(60, '='));
        _engine.Message(step.Description);
        _engine.Message($"\n▶️  YOUR TASK: {step.Task}");

        if (!string.IsNullOrEmpty(step.Hint))
        {
            _engine.Message($"💡 Hint: {step.Hint}");
        }

        _engine.Message("");
    }

    public string GetContextualHint(string lastCommand, GameState state)
    {
        if (!IsBeginnerMode)
            return string.Empty;

        // Provide context-sensitive hints based on game state
        var hints = new List<string>();

        // Check for dangerous situations
        if (state.CurrentQuadrant?.Enemies.Any() == true && !state.Ship.ShieldsUp)
        {
            hints.Add("💡 Tip: Enemies detected! Raise shields with 'SHIELDS UP' before they attack.");
        }

        if (state.Ship.Energy < 500 && state.Ship.Shield > 100)
        {
            hints.Add("💡 Tip: Energy is low. Consider transferring shield energy back: 'SHIELDS TRANSFER -100'");
        }

        if (state.Ship.Energy < 200)
        {
            hints.Add("💡 Tip: Energy critical! Find a starbase and DOCK to recharge.");
        }

        if (lastCommand.ToUpperInvariant() == "SRSCAN" && state.CurrentQuadrant?.Enemies.Any() == true)
        {
            hints.Add("💡 Next: Try 'PHASERS 500' to attack enemies, or 'IMPULSE' to move away.");
        }

        if (lastCommand.ToUpperInvariant() == "LRSCAN")
        {
            hints.Add("💡 Next: Use 'CHART' to see the galaxy map and plan your route.");
        }

        return hints.Any() ? "\n" + string.Join("\n", hints) : string.Empty;
    }

    private void InitializeTutorialSteps()
    {
        Steps = new List<TutorialStep>
        {
            new TutorialStep
            {
                Title = "Welcome to Star Trek",
                Description = @"Welcome, Captain! You are in command of the USS Enterprise.
Your mission: Destroy all Klingon ships in the galaxy before time runs out.

The galaxy is divided into 8×8 QUADRANTS, each containing 10×10 SECTORS.
Stars, enemies, and starbases are scattered throughout.",
                Task = "Type STATUS to see your ship's current status",
                ExpectedCommands = new List<string> { "STATUS", "ST" },
                SuccessMessage = "You can see your ship's energy, shields, and mission status.",
                Hint = "Type: STATUS (or just ST)"
            },
            new TutorialStep
            {
                Title = "Short Range Scan",
                Description = @"The short-range scan shows your immediate surroundings in the current sector.
You'll see the Enterprise (E or 🚀) and any nearby objects:
  • Stars (*)
  • Klingons (K or 👾)
  • Starbases (B or 🏰)",
                Task = "Type SRSCAN to scan the current sector",
                ExpectedCommands = new List<string> { "SRSCAN", "SR" },
                SuccessMessage = "You can now see your local area!",
                Hint = "Type: SRSCAN (or SR)"
            },
            new TutorialStep
            {
                Title = "Long Range Scan",
                Description = @"The long-range scan shows neighboring quadrants.
Each number shows what's in that quadrant:
  • First digit = number of Klingons
  • Second digit = number of Starbases
  • Third digit = number of Stars

Example: '205' means 2 Klingons, 0 Bases, 5 Stars",
                Task = "Type LRSCAN to scan nearby quadrants",
                ExpectedCommands = new List<string> { "LRSCAN", "LR" },
                SuccessMessage = "Now you know what's around you!",
                Hint = "Type: LRSCAN (or LR)"
            },
            new TutorialStep
            {
                Title = "Raising Shields",
                Description = @"Shields protect your ship from enemy fire and collisions.
IMPORTANT: After any movement, enemies will attack!

Always raise shields before moving if enemies are nearby.",
                Task = "Type 'SHIELDS UP' to raise shields to maximum",
                ExpectedCommands = new List<string> { "SHIELDS" },
                SuccessMessage = "Shields are up! You're now protected.",
                Hint = "Type: SHIELDS UP"
            },
            new TutorialStep
            {
                Title = "Moving with Impulse",
                Description = @"Impulse engines move you within the current quadrant.
Directions use a clock face (12=up, 3=right, 6=down, 9=left).
Distance is in quadrants: 0.1 = 1 sector, 0.5 = 5 sectors.

        12  1
    11        2
  10            3
 9      E       4
  8            5
    7        6",
                Task = "Move 2 sectors to the right: Type 'IM 3 0.2'",
                ExpectedCommands = new List<string> { "IMPULSE", "IM" },
                SuccessMessage = "You've moved! Notice enemies attacked after your move.",
                Hint = "Type: IM 3 0.2"
            },
            new TutorialStep
            {
                Title = "Viewing the Galaxy Chart",
                Description = @"The galaxy chart shows all quadrants you've scanned.
It helps you plan long-range travel and find starbases.

Quadrants you haven't visited show as '...'",
                Task = "Type CHART to see the galaxy map",
                ExpectedCommands = new List<string> { "CHART", "CH" },
                SuccessMessage = "You can now see the explored galaxy!",
                Hint = "Type: CHART"
            },
            new TutorialStep
            {
                Title = "Getting Help",
                Description = @"You can get help on any command by typing HELP <command>.

For example: HELP IMPULSE shows detailed movement instructions.
Just HELP shows all available commands.",
                Task = "Type 'HELP PHASERS' to learn about the phaser weapon",
                ExpectedCommands = new List<string> { "HELP" },
                SuccessMessage = "Now you know how to learn any command!",
                Hint = "Type: HELP PHASERS"
            },
            new TutorialStep
            {
                Title = "Tutorial Complete!",
                Description = @"Congratulations, Captain! You've learned the basics:
  ✅ STATUS - Check ship status
  ✅ SRSCAN - View current sector
  ✅ LRSCAN - Scan nearby quadrants
  ✅ SHIELDS - Raise/lower shields
  ✅ IMPULSE - Move within quadrant
  ✅ CHART - View galaxy map
  ✅ HELP - Get command help

Additional important commands:
  • PHASERS - Fire phasers at enemies
  • TORPEDO - Fire photon torpedoes
  • WARP - Long-range travel between quadrants
  • DOCK - Dock at starbase for repairs/recharge
  • DAMAGE - Check system damage

Type HELP <command> to learn more about any command!",
                Task = "You're ready! Type any command to continue playing.",
                ExpectedCommands = new List<string> { }, // Any command completes
                SuccessMessage = "Good luck on your mission, Captain!",
                Hint = ""
            }
        };
    }
}

public class TutorialStep
{
    public required string Title { get; init; }
    public required string Description { get; init; }
    public required string Task { get; init; }
    public required List<string> ExpectedCommands { get; init; }
    public required string SuccessMessage { get; init; }
    public string Hint { get; init; } = string.Empty;
}
