namespace TrekSuper.Core.Models;

/// <summary>
/// Represents a position in the galaxy (quadrant coordinates 1-8).
/// </summary>
public readonly record struct QuadrantCoordinate(int X, int Y)
{
    public static QuadrantCoordinate Invalid => new(-1, -1);

    public bool IsValid => X >= 1 && X <= 8 && Y >= 1 && Y <= 8;

    public override string ToString() => $"{X} - {Y}";

    public static QuadrantCoordinate Parse(int x, int y) => new(x, y);

    public double DistanceTo(QuadrantCoordinate other) =>
        Math.Sqrt(Math.Pow(X - other.X, 2) + Math.Pow(Y - other.Y, 2));
}

/// <summary>
/// Represents a position within a quadrant (sector coordinates 1-10).
/// </summary>
public readonly record struct SectorCoordinate(int X, int Y)
{
    public static SectorCoordinate Invalid => new(-1, -1);

    public bool IsValid => X >= 1 && X <= 10 && Y >= 1 && Y <= 10;

    public override string ToString() => $"{X} - {Y}";

    public static SectorCoordinate Parse(int x, int y) => new(x, y);

    public double DistanceTo(SectorCoordinate other) =>
        Math.Sqrt(Math.Pow(X - other.X, 2) + Math.Pow(Y - other.Y, 2));
}

/// <summary>
/// Full galactic position combining quadrant and sector.
/// </summary>
public readonly record struct GalacticPosition(QuadrantCoordinate Quadrant, SectorCoordinate Sector)
{
    public bool IsValid => Quadrant.IsValid && Sector.IsValid;

    public override string ToString() => $"Quadrant {Quadrant}, Sector {Sector}";

    /// <summary>
    /// Calculate distance in quadrant units (sectors = 0.1 quadrants).
    /// </summary>
    public double DistanceTo(GalacticPosition other)
    {
        double dx = (Quadrant.X - other.Quadrant.X) + (Sector.X - other.Sector.X) / 10.0;
        double dy = (Quadrant.Y - other.Quadrant.Y) + (Sector.Y - other.Sector.Y) / 10.0;
        return Math.Sqrt(dx * dx + dy * dy);
    }
}
