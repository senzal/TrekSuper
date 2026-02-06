namespace TrekSuper.Core.Enums;

/// <summary>
/// Types of entities that can appear in a sector.
/// </summary>
public enum EntityType
{
    Empty = '.',
    Star = '*',
    Planet = 'P',
    Starbase = 'B',
    Enterprise = 'E',
    Faerie = 'F',         // Faerie Queen (alternate ship)
    Klingon = 'K',
    Commander = 'C',
    SuperCommander = 'S',
    Romulan = 'R',
    Tholian = 'T',
    TholianWeb = '#',
    BlackHole = ' ',
    Unknown = '?'
}

public static class EntityTypeExtensions
{
    public static char ToChar(this EntityType entity) => (char)entity;

    public static bool IsEnemy(this EntityType entity) => entity switch
    {
        EntityType.Klingon => true,
        EntityType.Commander => true,
        EntityType.SuperCommander => true,
        EntityType.Romulan => true,
        EntityType.Tholian => true,
        _ => false
    };

    public static bool IsHostile(this EntityType entity) => entity switch
    {
        EntityType.Klingon => true,
        EntityType.Commander => true,
        EntityType.SuperCommander => true,
        _ => false
    };

    public static string GetDisplayName(this EntityType entity) => entity switch
    {
        EntityType.Empty => "Empty space",
        EntityType.Star => "Star",
        EntityType.Planet => "Planet",
        EntityType.Starbase => "Starbase",
        EntityType.Enterprise => "Enterprise",
        EntityType.Faerie => "Faerie Queen",
        EntityType.Klingon => "Klingon",
        EntityType.Commander => "Commander",
        EntityType.SuperCommander => "Super-Commander",
        EntityType.Romulan => "Romulan",
        EntityType.Tholian => "Tholian",
        EntityType.TholianWeb => "Tholian Web",
        EntityType.BlackHole => "Black Hole",
        EntityType.Unknown => "Unknown",
        _ => entity.ToString()
    };
}
