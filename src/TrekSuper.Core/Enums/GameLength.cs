namespace TrekSuper.Core.Enums;

/// <summary>
/// Game length determines initial resources and time limits.
/// </summary>
public enum GameLength
{
    Short = 1,
    Medium = 2,
    Long = 4
}

public static class GameLengthExtensions
{
    public static string GetDisplayName(this GameLength length) => length switch
    {
        GameLength.Short => "Short",
        GameLength.Medium => "Medium",
        GameLength.Long => "Long",
        _ => length.ToString()
    };

    /// <summary>
    /// Gets the time limit multiplier for this game length.
    /// </summary>
    public static double GetTimeMultiplier(this GameLength length) => (int)length;
}
