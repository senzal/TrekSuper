using TrekSuper.Core.Enums;

namespace TrekSuper.Core.Models.Entities;

/// <summary>
/// Base class for enemy ships.
/// </summary>
public abstract class Enemy : Entity
{
    /// <summary>Current energy/power level.</summary>
    public double Power { get; set; }

    /// <summary>Distance from the Enterprise.</summary>
    public double Distance { get; set; }

    /// <summary>Average distance (for combat calculations).</summary>
    public double AverageDistance { get; set; }

    /// <summary>Whether this enemy has been destroyed.</summary>
    public bool IsDestroyed => Power <= 0;

    protected Enemy(SectorCoordinate position, double power) : base(position)
    {
        Power = power;
    }

    public abstract double GetAttackPower(Random random);
}

/// <summary>
/// Standard Klingon warship.
/// </summary>
public class Klingon : Enemy
{
    public override EntityType Type => EntityType.Klingon;

    public Klingon(SectorCoordinate position, double power) : base(position, power) { }

    public override double GetAttackPower(Random random)
    {
        // Klingons hit with random portion of their power, modified by distance
        double hit = Power * (2.0 + random.NextDouble()) / Distance;
        return hit;
    }
}

/// <summary>
/// Klingon Commander - stronger and smarter than regular Klingons.
/// </summary>
public class Commander : Enemy
{
    public override EntityType Type => EntityType.Commander;

    public Commander(SectorCoordinate position, double power) : base(position, power) { }

    public override double GetAttackPower(Random random)
    {
        // Commanders are more accurate
        double hit = Power * (2.5 + random.NextDouble()) / Distance;
        return hit;
    }
}

/// <summary>
/// Super-Commander - the most dangerous enemy.
/// </summary>
public class SuperCommander : Enemy
{
    public override EntityType Type => EntityType.SuperCommander;

    /// <summary>Quadrant location of the Super-Commander.</summary>
    public QuadrantCoordinate QuadrantLocation { get; set; }

    /// <summary>Whether the SC is currently attacking a starbase.</summary>
    public bool IsAttackingBase { get; set; }

    public SuperCommander(SectorCoordinate position, double power) : base(position, power) { }

    public override double GetAttackPower(Random random)
    {
        // Super-Commanders are the most dangerous
        double hit = Power * (3.0 + random.NextDouble()) / Distance;
        return hit;
    }
}

/// <summary>
/// Romulan warbird - neutral zone violations possible.
/// </summary>
public class Romulan : Enemy
{
    public override EntityType Type => EntityType.Romulan;

    public Romulan(SectorCoordinate position, double power) : base(position, power) { }

    public override double GetAttackPower(Random random)
    {
        double hit = Power * (2.0 + random.NextDouble()) / Distance;
        return hit;
    }
}

/// <summary>
/// Tholian - creates web barriers.
/// </summary>
public class Tholian : Enemy
{
    public override EntityType Type => EntityType.Tholian;

    public Tholian(SectorCoordinate position, double power) : base(position, power) { }

    public override double GetAttackPower(Random random)
    {
        double hit = Power * (1.5 + random.NextDouble()) / Distance;
        return hit;
    }
}
