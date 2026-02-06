using TrekSuper.Core.Models;
using TrekSuper.Core.Enums;

namespace TrekSuper.Core.Commands;

/// <summary>
/// Tutorial command - starts interactive tutorial for new players.
/// </summary>
public class TutorialCommand : BaseCommand
{
    public override string Name => "TUTORIAL";
    public override string Abbreviation => "TUT";
    public override string HelpText => "Start interactive tutorial for new players. Usage: TUTORIAL";

    public override string DetailedHelpText => @"
USAGE:
  TUTORIAL
  TUT

DESCRIPTION:
  Starts an interactive tutorial that teaches you how to play Super Star Trek.
  Perfect for new players who want to learn the basics.

WHAT YOU'LL LEARN:
  • How to check your ship's status
  • Reading short and long-range scans
  • Moving with impulse engines
  • Raising and lowering shields
  • Viewing the galaxy chart
  • Getting help on commands

TUTORIAL FEATURES:
  ✓ Step-by-step guidance
  ✓ Practice in a safe environment
  ✓ Hints and tips along the way
  ✓ Learn at your own pace
  ✓ Beginner mode with contextual hints

The tutorial uses a special practice scenario designed for learning.
It takes about 5-10 minutes to complete.

After completing the tutorial, you'll be ready to play a full game!

BEGINNER MODE:
  The tutorial automatically enables 'beginner mode' which provides
  helpful hints after each command. This mode can be toggled on/off
  in regular games if you want continued guidance.

SEE ALSO:
  HELP      - Get help on any command
  STATUS    - Check ship status
  SRSCAN    - Short-range scan";

    public TutorialCommand(GameEngine engine) : base(engine) { }

    public override Task<CommandResult> ExecuteAsync(GameState state, string[] args)
    {
        Message("\n🎓 Starting Interactive Tutorial...\n");
        Message("This tutorial will teach you the basics of Super Star Trek.");
        Message("Follow the instructions for each step.\n");

        // Start tutorial mode
        Engine.Tutorial.StartTutorial();

        return Task.FromResult(CommandResult.Ok());
    }
}
