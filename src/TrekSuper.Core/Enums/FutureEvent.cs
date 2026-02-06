namespace TrekSuper.Core.Enums;

/// <summary>
/// Types of scheduled future events in the game.
/// </summary>
public enum FutureEventType
{
    /// <summary>Spy event - can cause SC to tractor beam Enterprise.</summary>
    Spy = 0,

    /// <summary>Supernova occurs somewhere in galaxy.</summary>
    Supernova = 1,

    /// <summary>Commander tractor beams Enterprise.</summary>
    TractorBeam = 2,

    /// <summary>Snapshot taken for time warp.</summary>
    Snapshot = 3,

    /// <summary>Commander attacks a starbase.</summary>
    BaseAttack = 4,

    /// <summary>Commander destroys a starbase.</summary>
    CommanderDestroysBase = 5,

    /// <summary>Super-Commander moves (might attack base).</summary>
    SuperCommanderMoves = 6,

    /// <summary>Super-Commander destroys base.</summary>
    SuperCommanderDestroysBase = 7,

    /// <summary>Deep space probe moves.</summary>
    DeepSpaceProbeMove = 8
}
