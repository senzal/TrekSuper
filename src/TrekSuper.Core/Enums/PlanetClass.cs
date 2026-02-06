namespace TrekSuper.Core.Enums;

/// <summary>
/// Planet classifications.
/// </summary>
public enum PlanetClass
{
    Unknown = 0,
    M = 1,  // Class M - Earth-like, suitable for humanoid life
    N = 2,  // Class N - Sulfuric, Venus-like
    O = 3   // Class O - Pelagic, water world
}

public static class PlanetClassExtensions
{
    public static string GetDisplayName(this PlanetClass planetClass) => planetClass switch
    {
        PlanetClass.M => "M",
        PlanetClass.N => "N",
        PlanetClass.O => "O",
        _ => "?"
    };

    public static string GetDescription(this PlanetClass planetClass) => planetClass switch
    {
        PlanetClass.M => "Class M (Earth-like)",
        PlanetClass.N => "Class N (Sulfuric)",
        PlanetClass.O => "Class O (Water world)",
        _ => "Unknown class"
    };
}
