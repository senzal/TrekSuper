namespace TrekSuper.Core.Enums;

/// <summary>
/// Player skill level - affects difficulty, scoring, and game parameters.
/// </summary>
public enum SkillLevel
{
    Novice = 1,
    Fair = 2,
    Good = 3,
    Expert = 4,
    Emeritus = 5
}

public static class SkillLevelExtensions
{
    public static string GetDisplayName(this SkillLevel skill) => skill switch
    {
        SkillLevel.Novice => "Novice",
        SkillLevel.Fair => "Fair",
        SkillLevel.Good => "Good",
        SkillLevel.Expert => "Expert",
        SkillLevel.Emeritus => "Emeritus",
        _ => skill.ToString()
    };

    /// <summary>
    /// Gets the damage factor multiplier for this skill level.
    /// Higher skill = more damage taken.
    /// </summary>
    public static double GetDamageFactor(this SkillLevel skill) => skill switch
    {
        SkillLevel.Novice => 0.5,
        SkillLevel.Fair => 0.7,
        SkillLevel.Good => 1.0,
        SkillLevel.Expert => 1.3,
        SkillLevel.Emeritus => 1.5,
        _ => 1.0
    };
}
