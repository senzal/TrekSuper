using TrekSuper.Core.Enums;

namespace TrekSuper.Core.Models.Entities;

/// <summary>
/// A planet that can be orbited, landed on, and mined for crystals.
/// </summary>
public class Planet : Entity
{
    public override EntityType Type => EntityType.Planet;

    /// <summary>Planet classification (M, N, or O).</summary>
    public PlanetClass Class { get; }

    /// <summary>Whether dilithium crystals are present.</summary>
    public bool HasCrystals { get; set; }

    /// <summary>Whether the planet has been scanned/contents known.</summary>
    public bool IsKnown { get; set; }

    /// <summary>Whether the shuttle craft is currently on this planet.</summary>
    public bool HasShuttle { get; set; }

    /// <summary>The quadrant where this planet is located.</summary>
    public QuadrantCoordinate QuadrantLocation { get; set; }

    public Planet(SectorCoordinate position, PlanetClass planetClass, bool hasCrystals = false)
        : base(position)
    {
        Class = planetClass;
        HasCrystals = hasCrystals;
    }

    public override string ToString() =>
        $"Class {Class.GetDisplayName()} planet at {Position}" +
        (HasCrystals ? " (crystals)" : "") +
        (IsKnown ? " [known]" : "");
}
