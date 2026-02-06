namespace TrekSuper.Core.Enums;

/// <summary>
/// How the game ended - used for scoring and final messages.
/// </summary>
public enum GameOutcome
{
    /// <summary>Game is still in progress.</summary>
    InProgress,

    /// <summary>All Klingons destroyed - victory!</summary>
    Won,

    /// <summary>Ran out of resources (energy depleted with no bases).</summary>
    ResourcesDepleted,

    /// <summary>Life support failed and no reserves.</summary>
    LifeSupportFailed,

    /// <summary>Ran out of energy.</summary>
    EnergyDepleted,

    /// <summary>Killed in battle.</summary>
    KilledInBattle,

    /// <summary>Dropped below negative energy threshold.</summary>
    NegativeEnergy,

    /// <summary>Destroyed by supernova.</summary>
    Supernova,

    /// <summary>Caught in supernova while evacuating.</summary>
    SupernovaedWhileEscaping,

    /// <summary>Abandoned ship.</summary>
    Abandoned,

    /// <summary>Dilithium crystals destroyed ship.</summary>
    DilithiumExplosion,

    /// <summary>Materialized inside solid matter.</summary>
    MaterializedInRock,

    /// <summary>Killed by own phasers.</summary>
    PhaserAccident,

    /// <summary>Lost in space.</summary>
    LostInSpace,

    /// <summary>Killed in mining accident.</summary>
    MiningAccident,

    /// <summary>Destroyed along with planet.</summary>
    PlanetDestroyed,

    /// <summary>Planet went nova while on surface.</summary>
    PlanetNova,

    /// <summary>Killed by Super-Commander.</summary>
    SuperCommanderKill,

    /// <summary>Tractor beamed into supernova.</summary>
    TractorBeamIntoSupernova,

    /// <summary>Death ray backfired.</summary>
    DeathRayBackfire,

    /// <summary>Tribbles consumed all food.</summary>
    Tribbles,

    /// <summary>Fell into black hole.</summary>
    BlackHole,

    /// <summary>Destroyed while cloaked (treaty violation).</summary>
    CloakingViolation,

    /// <summary>Self-destruct activated.</summary>
    SelfDestruct,

    /// <summary>Ran out of time.</summary>
    TimeExpired
}

public static class GameOutcomeExtensions
{
    public static bool IsVictory(this GameOutcome outcome) => outcome == GameOutcome.Won;

    public static bool IsDefeat(this GameOutcome outcome) =>
        outcome != GameOutcome.InProgress && outcome != GameOutcome.Won;
}
