namespace TrekSuper.Core.Enums;

/// <summary>
/// Ship condition (alert status).
/// </summary>
public enum Condition
{
    Green = 'G',
    Yellow = 'Y',
    Red = 'R',
    Docked = 'D'
}

public static class ConditionExtensions
{
    public static string GetDisplayName(this Condition condition) => condition switch
    {
        Condition.Green => "GREEN",
        Condition.Yellow => "YELLOW",
        Condition.Red => "*RED*",
        Condition.Docked => "DOCKED",
        _ => condition.ToString()
    };

    public static ConsoleColor GetColor(this Condition condition) => condition switch
    {
        Condition.Green => ConsoleColor.Green,
        Condition.Yellow => ConsoleColor.Yellow,
        Condition.Red => ConsoleColor.Red,
        Condition.Docked => ConsoleColor.Cyan,
        _ => ConsoleColor.White
    };
}
