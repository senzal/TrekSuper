using TrekSuper.Core.Enums;
using TrekSuper.Core.Models.Ship;

namespace TrekSuper.Core.Models;

/// <summary>
/// Complete game state - everything needed to save/restore a game.
/// </summary>
public class GameState
{
    // Galaxy
    public Galaxy Galaxy { get; } = new();

    // Player ship
    public Enterprise Ship { get; } = new();

    // Current quadrant (populated when entering)
    public Quadrant? CurrentQuadrant { get; set; }

    // Time tracking
    public double Stardate { get; set; }
    public double InitialStardate { get; set; }
    public double TimeRemaining { get; set; }
    public double InitialTime { get; set; }

    // Kill tracking
    public int KlingonsKilled { get; set; }
    public int CommandersKilled { get; set; }
    public int SuperCommandersKilled { get; set; }
    public int RomulansKilled { get; set; }
    public int PlanetsDestroyed { get; set; }
    public int StarsDestroyed { get; set; }
    public int BasesDestroyed { get; set; }

    // Initial counts (for scoring)
    public int InitialKlingons { get; set; }
    public int InitialCommanders { get; set; }
    public int InitialBases { get; set; }
    public int InitialStars { get; set; }
    public int InitialPlanets { get; set; }

    // Remaining counts
    public int RemainingKlingons { get; set; }
    public int RemainingCommanders { get; set; }
    public int RemainingBases { get; set; }
    public int RemainingSuperCommanders { get; set; }
    public int RemainingRomulans { get; set; }

    // Game settings
    public SkillLevel Skill { get; set; } = SkillLevel.Good;
    public GameLength Length { get; set; } = GameLength.Medium;
    public int TournamentNumber { get; set; }
    public bool IsThawedGame { get; set; }

    // Game progress
    public GameOutcome Outcome { get; set; } = GameOutcome.InProgress;
    public bool IsGameOver => Outcome != GameOutcome.InProgress;
    public bool IsVictory => Outcome == GameOutcome.Won;
    public int HelpCalls { get; set; }

    // Self-destruct password
    public string SelfDestructPassword { get; set; } = "";

    // Resources
    public double RemainingResources { get; set; }
    public double InitialResources { get; set; }

    // Future events (scheduled stardate)
    public double[] FutureEvents { get; } = new double[9];

    // Probe tracking
    public double ProbeX { get; set; }
    public double ProbeY { get; set; }
    public double ProbeIncrementX { get; set; }
    public double ProbeIncrementY { get; set; }
    public int ProbeQuadrantX { get; set; }
    public int ProbeQuadrantY { get; set; }
    public int ProbeMovesRemaining { get; set; }
    public bool ProbeIsArmed { get; set; }

    // Base attack tracking
    public QuadrantCoordinate BaseUnderAttack { get; set; } = QuadrantCoordinate.Invalid;
    public bool HasSeenBaseAttackReport { get; set; }

    // Snapshot for time warp
    public GameStateSnapshot? Snapshot { get; set; }

    // Flags
    public bool JustEnteredQuadrant { get; set; }
    public bool ActionTaken { get; set; } // Allows enemy to attack
    public bool IsResting { get; set; }

    // Random number generator (seeded for reproducibility in tournaments)
    public Random Random { get; set; } = new();

    /// <summary>
    /// Gets the current score.
    /// </summary>
    public int CalculateScore()
    {
        // Base scoring formula from original game
        int score = 0;

        // Points for kills
        score += KlingonsKilled * 10;
        score += CommandersKilled * 50;
        score += SuperCommandersKilled * 200;
        score += RomulansKilled * 20;

        // Penalties
        score -= Ship.Casualties * 5;
        score -= HelpCalls * 50;
        score -= StarsDestroyed * 5;
        score -= PlanetsDestroyed * 10;
        score -= BasesDestroyed * 100;

        // Time bonus
        if (IsVictory)
        {
            double timeTaken = Stardate - InitialStardate;
            double timeAllowed = InitialTime;
            if (timeTaken < timeAllowed)
            {
                score += (int)((timeAllowed - timeTaken) * 10);
            }
        }

        // Skill multiplier
        score = (int)(score * (int)Skill);

        return Math.Max(0, score);
    }

    /// <summary>
    /// Checks if the player has won (all Klingons destroyed).
    /// </summary>
    public void CheckVictory()
    {
        if (RemainingKlingons <= 0 && RemainingCommanders <= 0 && RemainingSuperCommanders <= 0)
        {
            Outcome = GameOutcome.Won;
        }
    }

    /// <summary>
    /// Ends the game with a specific outcome.
    /// </summary>
    public void EndGame(GameOutcome outcome)
    {
        Outcome = outcome;
        OnGameEnded?.Invoke(outcome);
    }

    /// <summary>
    /// Event raised when the game ends. Allows engine to display appropriate messages.
    /// </summary>
    public event Action<GameOutcome>? OnGameEnded;

    /// <summary>
    /// Advances the stardate and checks for time-based events.
    /// </summary>
    public void AdvanceTime(double time)
    {
        Stardate += time;
        TimeRemaining -= time;

        // Warn when time is running low
        if (TimeRemaining <= 2.0 && TimeRemaining > 0 && !IsGameOver)
        {
            OnTimeWarning?.Invoke(TimeRemaining);
        }

        if (TimeRemaining <= 0 && !IsGameOver)
        {
            EndGame(GameOutcome.TimeExpired);
        }
    }

    /// <summary>
    /// Event raised when time is running low.
    /// </summary>
    public event Action<double>? OnTimeWarning;
}

/// <summary>
/// Snapshot of game state for time warp feature.
/// </summary>
public class GameStateSnapshot
{
    public double Stardate { get; set; }
    public int RemainingKlingons { get; set; }
    public int RemainingCommanders { get; set; }
    public int RemainingBases { get; set; }
    // Add other fields as needed for full state restoration
}
