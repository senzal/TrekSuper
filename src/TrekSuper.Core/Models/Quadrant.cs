using TrekSuper.Core.Enums;
using TrekSuper.Core.Models.Entities;

namespace TrekSuper.Core.Models;

/// <summary>
/// Represents a 10x10 sector grid within a quadrant.
/// </summary>
public class Quadrant
{
    public const int Size = 10;

    public QuadrantCoordinate Coordinate { get; }

    // The sector grid (1-10, 1-10)
    private readonly Entity?[,] _sectors = new Entity?[Size + 1, Size + 1];

    // Entities in this quadrant
    public List<Enemy> Enemies { get; } = [];
    public List<Star> Stars { get; } = [];
    public Starbase? Starbase { get; set; }
    public Planet? Planet { get; set; }
    public Tholian? Tholian { get; set; }

    // Tholian web positions
    public HashSet<SectorCoordinate> WebPositions { get; } = [];

    // Quadrant state
    public bool IsInNeutralZone { get; set; }
    public bool HasSuperCommander => Enemies.Any(e => e is SuperCommander);

    public Quadrant(QuadrantCoordinate coordinate)
    {
        Coordinate = coordinate;
    }

    /// <summary>
    /// Gets the entity at a sector position.
    /// </summary>
    public Entity? GetEntityAt(SectorCoordinate sector)
    {
        if (!sector.IsValid) return null;
        return _sectors[sector.X, sector.Y];
    }

    /// <summary>
    /// Gets the entity at a sector position.
    /// </summary>
    public Entity? GetEntityAt(int x, int y) => GetEntityAt(new SectorCoordinate(x, y));

    /// <summary>
    /// Gets the entity type character at a sector position.
    /// </summary>
    public char GetCharAt(SectorCoordinate sector)
    {
        if (WebPositions.Contains(sector)) return '#';
        var entity = GetEntityAt(sector);
        return entity?.Symbol ?? '.';
    }

    /// <summary>
    /// Gets the entity type character at a sector position.
    /// </summary>
    public char GetCharAt(int x, int y) => GetCharAt(new SectorCoordinate(x, y));

    /// <summary>
    /// Places an entity at a sector position.
    /// </summary>
    public void PlaceEntity(Entity entity)
    {
        if (!entity.Position.IsValid) return;
        _sectors[entity.Position.X, entity.Position.Y] = entity;
    }

    /// <summary>
    /// Removes an entity from its sector position.
    /// </summary>
    public void RemoveEntity(Entity entity)
    {
        if (!entity.Position.IsValid) return;
        if (_sectors[entity.Position.X, entity.Position.Y] == entity)
        {
            _sectors[entity.Position.X, entity.Position.Y] = null;
        }
    }

    /// <summary>
    /// Moves an entity to a new sector position.
    /// </summary>
    public void MoveEntity(Entity entity, SectorCoordinate newPosition)
    {
        RemoveEntity(entity);
        entity.Position = newPosition;
        PlaceEntity(entity);
    }

    /// <summary>
    /// Clears a sector position.
    /// </summary>
    public void ClearSector(SectorCoordinate sector)
    {
        if (sector.IsValid)
        {
            _sectors[sector.X, sector.Y] = null;
        }
    }

    /// <summary>
    /// Checks if a sector is empty.
    /// </summary>
    public bool IsSectorEmpty(SectorCoordinate sector)
    {
        if (!sector.IsValid) return false;
        return _sectors[sector.X, sector.Y] == null && !WebPositions.Contains(sector);
    }

    /// <summary>
    /// Checks if a sector is empty.
    /// </summary>
    public bool IsSectorEmpty(int x, int y) => IsSectorEmpty(new SectorCoordinate(x, y));

    /// <summary>
    /// Finds a random empty sector.
    /// </summary>
    public SectorCoordinate FindEmptySector(Random random)
    {
        int attempts = 0;
        while (attempts < 100)
        {
            int x = random.Next(1, Size + 1);
            int y = random.Next(1, Size + 1);
            var sector = new SectorCoordinate(x, y);
            if (IsSectorEmpty(sector))
            {
                return sector;
            }
            attempts++;
        }
        return SectorCoordinate.Invalid;
    }

    /// <summary>
    /// Gets the total number of enemies in this quadrant.
    /// </summary>
    public int EnemyCount => Enemies.Count(e => !e.IsDestroyed);

    /// <summary>
    /// Gets the number of Klingons (including commanders).
    /// </summary>
    public int KlingonCount => Enemies.Count(e => !e.IsDestroyed && e is Klingon or Commander);

    /// <summary>
    /// Gets the number of commanders.
    /// </summary>
    public int CommanderCount => Enemies.Count(e => !e.IsDestroyed && e is Commander);

    /// <summary>
    /// Gets the number of Romulans.
    /// </summary>
    public int RomulanCount => Enemies.Count(e => !e.IsDestroyed && e is Romulan);

    /// <summary>
    /// Adds an enemy to this quadrant and places it in the sector grid.
    /// </summary>
    public void AddEnemy(Enemy enemy)
    {
        Enemies.Add(enemy);
        PlaceEntity(enemy);
    }

    /// <summary>
    /// Removes a destroyed enemy from the quadrant.
    /// </summary>
    public void RemoveEnemy(Enemy enemy)
    {
        RemoveEntity(enemy);
        Enemies.Remove(enemy);
    }

    /// <summary>
    /// Places the Enterprise at a sector position.
    /// </summary>
    public void PlaceEnterprise(SectorCoordinate sector)
    {
        // Enterprise is tracked separately, just need to mark the sector
        // This is handled by the game state
    }
}
