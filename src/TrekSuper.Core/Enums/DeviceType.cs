namespace TrekSuper.Core.Enums;

/// <summary>
/// Ship devices that can be damaged and repaired.
/// </summary>
public enum DeviceType
{
    None = 0,
    ShortRangeSensors = 1,
    LongRangeSensors = 2,
    Phasers = 3,
    PhotonTubes = 4,
    LifeSupport = 5,
    WarpEngines = 6,
    ImpulseEngines = 7,
    Shields = 8,
    SubspaceRadio = 9,
    ShuttleCraft = 10,
    Computer = 11,
    Transporter = 12,
    ShieldControl = 13,
    DeathRay = 14,
    DeepSpaceProbe = 15,
    CloakingDevice = 16
}

public static class DeviceTypeExtensions
{
    private static readonly Dictionary<DeviceType, string> DeviceNames = new()
    {
        [DeviceType.ShortRangeSensors] = "S. R. Sensors",
        [DeviceType.LongRangeSensors] = "L. R. Sensors",
        [DeviceType.Phasers] = "Phasers",
        [DeviceType.PhotonTubes] = "Photon Tubes",
        [DeviceType.LifeSupport] = "Life Support",
        [DeviceType.WarpEngines] = "Warp Engines",
        [DeviceType.ImpulseEngines] = "Impulse Engines",
        [DeviceType.Shields] = "Shields",
        [DeviceType.SubspaceRadio] = "Subspace Radio",
        [DeviceType.ShuttleCraft] = "Shuttle Craft",
        [DeviceType.Computer] = "Computer",
        [DeviceType.Transporter] = "Transporter",
        [DeviceType.ShieldControl] = "Shield Control",
        [DeviceType.DeathRay] = "Death Ray",
        [DeviceType.DeepSpaceProbe] = "D. S. Probe",
        [DeviceType.CloakingDevice] = "Cloaking Device"
    };

    public static string GetDisplayName(this DeviceType device) =>
        DeviceNames.TryGetValue(device, out var name) ? name : device.ToString();

    public static IEnumerable<DeviceType> All =>
        Enum.GetValues<DeviceType>().Where(d => d != DeviceType.None);
}
