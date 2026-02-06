using TrekSuper.Core.Enums;

namespace TrekSuper.Core.Models.Ship;

/// <summary>
/// The player's ship - the USS Enterprise (or Faerie Queen).
/// </summary>
public class Enterprise
{
    /// <summary>Ship identifier ('E' for Enterprise, 'F' for Faerie Queen).</summary>
    public char ShipType { get; set; } = 'E';

    /// <summary>Whether this is the Enterprise (true) or Faerie Queen (false).</summary>
    public bool IsEnterprise => ShipType == 'E';

    /// <summary>Current position - quadrant.</summary>
    public QuadrantCoordinate Quadrant { get; set; }

    /// <summary>Current position - sector within quadrant.</summary>
    public SectorCoordinate Sector { get; set; }

    /// <summary>Full galactic position.</summary>
    public GalacticPosition Position => new(Quadrant, Sector);

    // Energy and Shields
    public double Energy { get; set; }
    public double MaxEnergy { get; init; } = 5000.0;
    public double Shield { get; set; }
    public double MaxShield { get; init; } = 2500.0;
    public bool ShieldsUp { get; set; }
    public bool ShieldsChanging { get; set; }

    // Weapons
    public int Torpedoes { get; set; }
    public int MaxTorpedoes { get; init; } = 10;

    // Propulsion
    public double WarpFactor { get; set; } = 5.0;
    public double WarpFactorSquared => WarpFactor * WarpFactor;

    // Life Support
    public double LifeSupportReserves { get; set; }
    public double MaxLifeSupportReserves { get; init; } = 2.5;

    // Deep Space Probes
    public int Probes { get; set; }
    public int MaxProbes { get; init; } = 3;

    // Special items
    public bool HasCrystals { get; set; }
    public double CrystalProbability { get; set; } = 1.0;

    // Crew status
    public int Casualties { get; set; }
    public bool IsAlive { get; set; } = true;

    // Shuttle craft
    public ShuttleState ShuttleState { get; set; } = ShuttleState.OnShip;

    // Cloaking (optional feature)
    public bool IsCloaked { get; set; }
    public bool IsCloaking { get; set; } // In process of cloaking
    public int TreatyViolations { get; set; }

    // Capture (optional feature)
    public int CapturedKlingons { get; set; }
    public int BrigCapacity { get; set; } = 400;
    public int BrigFreeSpace => BrigCapacity - CapturedKlingons;

    // Device status
    public ShipDevices Devices { get; } = new();

    // Docking
    public bool IsDocked { get; set; }
    public double DockingFactor { get; set; } = 0.25;

    /// <summary>Current condition based on energy, enemies, and docking status.</summary>
    public Condition Condition { get; set; } = Condition.Green;

    /// <summary>
    /// Gets the total available energy (energy + shields).
    /// </summary>
    public double TotalEnergy => Energy + Shield;

    /// <summary>
    /// Checks if ship has enough energy for an action.
    /// </summary>
    public bool HasEnergy(double amount) => Energy >= amount;

    /// <summary>
    /// Uses energy from the ship.
    /// </summary>
    public bool UseEnergy(double amount)
    {
        if (Energy < amount) return false;
        Energy -= amount;
        return true;
    }

    /// <summary>
    /// Checks if a device is operational.
    /// </summary>
    public bool IsDeviceOperational(DeviceType device) => Devices.IsOperational(device);

    /// <summary>
    /// Gets the name of the ship.
    /// </summary>
    public string Name => IsEnterprise ? "Enterprise" : "Faerie Queen";

    /// <summary>
    /// Resupply at starbase - restore energy, torpedoes, shields, etc.
    /// </summary>
    public void Resupply()
    {
        Energy = MaxEnergy;
        Shield = MaxShield;
        Torpedoes = MaxTorpedoes;
        LifeSupportReserves = MaxLifeSupportReserves;
        Probes = MaxProbes;
        Devices.RepairAll();
        IsDocked = true;
        Condition = Condition.Docked;
    }
}

/// <summary>
/// State of the shuttle craft.
/// </summary>
public enum ShuttleState
{
    OnShip = 1,
    OnPlanet = 0,
    Destroyed = -1
}
