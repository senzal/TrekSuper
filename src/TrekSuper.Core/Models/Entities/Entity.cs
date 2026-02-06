using TrekSuper.Core.Enums;

namespace TrekSuper.Core.Models.Entities;

/// <summary>
/// Base class for all entities that can exist in a sector.
/// </summary>
public abstract class Entity
{
    public abstract EntityType Type { get; }
    public SectorCoordinate Position { get; set; }

    public char Symbol => Type.ToChar();

    protected Entity(SectorCoordinate position)
    {
        Position = position;
    }

    public override string ToString() => $"{Type.GetDisplayName()} at {Position}";
}

/// <summary>
/// A star in a sector.
/// </summary>
public class Star : Entity
{
    public override EntityType Type => EntityType.Star;

    public Star(SectorCoordinate position) : base(position) { }
}

/// <summary>
/// A black hole - dangerous space hazard.
/// </summary>
public class BlackHole : Entity
{
    public override EntityType Type => EntityType.BlackHole;

    public BlackHole(SectorCoordinate position) : base(position) { }
}
