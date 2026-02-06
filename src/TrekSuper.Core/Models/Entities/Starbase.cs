using TrekSuper.Core.Enums;

namespace TrekSuper.Core.Models.Entities;

/// <summary>
/// A Federation starbase for docking, repairs, and resupply.
/// </summary>
public class Starbase : Entity
{
    public override EntityType Type => EntityType.Starbase;

    /// <summary>The quadrant where this starbase is located.</summary>
    public QuadrantCoordinate QuadrantLocation { get; set; }

    /// <summary>Whether this base is under attack.</summary>
    public bool IsUnderAttack { get; set; }

    /// <summary>Stardate when base will be destroyed if not defended.</summary>
    public double DestructionDate { get; set; }

    public Starbase(SectorCoordinate position) : base(position) { }

    public Starbase(SectorCoordinate position, QuadrantCoordinate quadrant) : base(position)
    {
        QuadrantLocation = quadrant;
    }
}
