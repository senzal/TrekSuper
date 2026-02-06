using TrekSuper.Core.Enums;

namespace TrekSuper.Core.Models.Ship;

/// <summary>
/// Manages ship device damage and repair state.
/// </summary>
public class ShipDevices
{
    private readonly Dictionary<DeviceType, double> _damage = new();

    public ShipDevices()
    {
        // Initialize all devices with no damage
        foreach (var device in DeviceTypeExtensions.All)
        {
            _damage[device] = 0.0;
        }
    }

    /// <summary>
    /// Gets the damage level for a device. 0 = working, positive = damaged.
    /// </summary>
    public double GetDamage(DeviceType device) =>
        _damage.TryGetValue(device, out var damage) ? damage : 0.0;

    /// <summary>
    /// Sets the damage level for a device.
    /// </summary>
    public void SetDamage(DeviceType device, double damage)
    {
        _damage[device] = Math.Max(0, damage);
    }

    /// <summary>
    /// Adds damage to a device.
    /// </summary>
    public void AddDamage(DeviceType device, double damage)
    {
        _damage[device] = Math.Max(0, GetDamage(device) + damage);
    }

    /// <summary>
    /// Repairs a device by reducing damage.
    /// </summary>
    public void Repair(DeviceType device, double amount)
    {
        _damage[device] = Math.Max(0, GetDamage(device) - amount);
    }

    /// <summary>
    /// Checks if a device is operational (no damage).
    /// </summary>
    public bool IsOperational(DeviceType device) => GetDamage(device) <= 0;

    /// <summary>
    /// Checks if a device is damaged.
    /// </summary>
    public bool IsDamaged(DeviceType device) => GetDamage(device) > 0;

    /// <summary>
    /// Gets all damaged devices and their damage levels.
    /// </summary>
    public IEnumerable<(DeviceType Device, double Damage)> GetDamagedDevices() =>
        _damage.Where(kvp => kvp.Value > 0)
               .Select(kvp => (kvp.Key, kvp.Value))
               .OrderByDescending(x => x.Value);

    /// <summary>
    /// Gets all device states for status display.
    /// </summary>
    public IEnumerable<(DeviceType Device, double Damage)> GetAllDevices() =>
        _damage.Select(kvp => (kvp.Key, kvp.Value))
               .OrderBy(x => (int)x.Key);

    /// <summary>
    /// Repairs all devices (for docking).
    /// </summary>
    public void RepairAll()
    {
        foreach (var device in _damage.Keys.ToList())
        {
            _damage[device] = 0.0;
        }
    }

    /// <summary>
    /// Counts the number of damaged devices.
    /// </summary>
    public int DamagedCount => _damage.Count(kvp => kvp.Value > 0);
}
