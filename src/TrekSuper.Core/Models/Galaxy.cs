using TrekSuper.Core.Enums;
using TrekSuper.Core.Models.Entities;

namespace TrekSuper.Core.Models;

/// <summary>
/// The 8x8 galaxy containing all quadrants.
/// </summary>
public class Galaxy
{
    public const int Size = 8;
    public const int MaxPlanets = 10;

    // Galaxy map data: encoded as KKK (Klingons*100 + Bases*10 + Stars)
    private readonly int[,] _map = new int[Size + 1, Size + 1];

    // Star chart - what the player has discovered
    private readonly int[,] _chart = new int[Size + 1, Size + 1];

    // Extended info flags (Romulans, Super-Commander, etc.)
    private readonly int[,] _extendedInfo = new int[Size + 1, Size + 1];

    // Commander locations
    public List<QuadrantCoordinate> CommanderLocations { get; } = [];

    // Super-Commander location
    public QuadrantCoordinate SuperCommanderLocation { get; set; } = QuadrantCoordinate.Invalid;

    // Starbase locations
    public List<QuadrantCoordinate> StarbaseLocations { get; } = [];

    // Planet data
    public List<Planet> Planets { get; } = [];

    // The mysterious "Thing" location
    public QuadrantCoordinate ThingLocation { get; set; } = QuadrantCoordinate.Invalid;

    /// <summary>
    /// Gets the encoded galaxy data for a quadrant (KKK format).
    /// </summary>
    public int GetQuadrantData(QuadrantCoordinate coord)
    {
        if (!coord.IsValid) return 0;
        return _map[coord.X, coord.Y];
    }

    /// <summary>
    /// Gets the encoded galaxy data for a quadrant.
    /// </summary>
    public int GetQuadrantData(int x, int y) => GetQuadrantData(new QuadrantCoordinate(x, y));

    /// <summary>
    /// Sets the encoded galaxy data for a quadrant.
    /// </summary>
    public void SetQuadrantData(QuadrantCoordinate coord, int data)
    {
        if (coord.IsValid)
        {
            _map[coord.X, coord.Y] = data;
        }
    }

    /// <summary>
    /// Sets the encoded galaxy data for a quadrant.
    /// </summary>
    public void SetQuadrantData(int x, int y, int data) =>
        SetQuadrantData(new QuadrantCoordinate(x, y), data);

    /// <summary>
    /// Gets the number of Klingons in a quadrant from galaxy data.
    /// </summary>
    public int GetKlingonCount(QuadrantCoordinate coord) => GetQuadrantData(coord) / 100;

    /// <summary>
    /// Gets the number of starbases in a quadrant from galaxy data.
    /// </summary>
    public int GetStarbaseCount(QuadrantCoordinate coord) => (GetQuadrantData(coord) % 100) / 10;

    /// <summary>
    /// Gets the number of stars in a quadrant from galaxy data.
    /// </summary>
    public int GetStarCount(QuadrantCoordinate coord) => GetQuadrantData(coord) % 10;

    /// <summary>
    /// Updates the star chart with current quadrant data.
    /// </summary>
    public void UpdateChart(QuadrantCoordinate coord)
    {
        if (coord.IsValid)
        {
            _chart[coord.X, coord.Y] = _map[coord.X, coord.Y] + 1; // +1 to distinguish from unknown
        }
    }

    /// <summary>
    /// Gets the charted data for a quadrant (0 = unknown).
    /// </summary>
    public int GetChartData(QuadrantCoordinate coord)
    {
        if (!coord.IsValid) return 0;
        return _chart[coord.X, coord.Y];
    }

    /// <summary>
    /// Checks if a quadrant has been charted.
    /// </summary>
    public bool IsCharted(QuadrantCoordinate coord) => GetChartData(coord) > 0;

    /// <summary>
    /// Sets extended info flags for a quadrant.
    /// </summary>
    public void SetExtendedInfo(QuadrantCoordinate coord, int info)
    {
        if (coord.IsValid)
        {
            _extendedInfo[coord.X, coord.Y] = info;
        }
    }

    /// <summary>
    /// Gets extended info flags for a quadrant.
    /// </summary>
    public int GetExtendedInfo(QuadrantCoordinate coord)
    {
        if (!coord.IsValid) return 0;
        return _extendedInfo[coord.X, coord.Y];
    }

    /// <summary>
    /// Adds a Klingon to a quadrant.
    /// </summary>
    public void AddKlingon(QuadrantCoordinate coord)
    {
        if (coord.IsValid)
        {
            _map[coord.X, coord.Y] += 100;
        }
    }

    /// <summary>
    /// Removes a Klingon from a quadrant.
    /// </summary>
    public void RemoveKlingon(QuadrantCoordinate coord)
    {
        if (coord.IsValid && GetKlingonCount(coord) > 0)
        {
            _map[coord.X, coord.Y] -= 100;
        }
    }

    /// <summary>
    /// Checks if coordinates are within galaxy bounds.
    /// </summary>
    public static bool IsValidCoordinate(int x, int y) => x >= 1 && x <= Size && y >= 1 && y <= Size;

    /// <summary>
    /// Gets a random valid quadrant coordinate.
    /// </summary>
    public static QuadrantCoordinate GetRandomQuadrant(Random random) =>
        new(random.Next(1, Size + 1), random.Next(1, Size + 1));

    /// <summary>
    /// Gets total Klingons remaining in the galaxy.
    /// </summary>
    public int TotalKlingons
    {
        get
        {
            int total = 0;
            for (int x = 1; x <= Size; x++)
            {
                for (int y = 1; y <= Size; y++)
                {
                    total += _map[x, y] / 100;
                }
            }
            return total;
        }
    }

    /// <summary>
    /// Gets a planet by its quadrant location.
    /// </summary>
    public Planet? GetPlanetAt(QuadrantCoordinate coord) =>
        Planets.FirstOrDefault(p => p.QuadrantLocation == coord);

    /// <summary>
    /// Marks a supernova in a quadrant - destroys everything.
    /// </summary>
    public void Supernova(QuadrantCoordinate coord)
    {
        if (!coord.IsValid) return;

        // Remove any klingons, bases, stars
        _map[coord.X, coord.Y] = 1000; // Mark as supernova (special value)
        _chart[coord.X, coord.Y] = 1001;

        // Remove starbase if present
        StarbaseLocations.Remove(coord);

        // Remove commander if present
        CommanderLocations.Remove(coord);

        // Check super-commander
        if (SuperCommanderLocation == coord)
        {
            SuperCommanderLocation = QuadrantCoordinate.Invalid;
        }

        // Remove planet if present
        var planet = GetPlanetAt(coord);
        if (planet != null)
        {
            Planets.Remove(planet);
        }
    }
}
