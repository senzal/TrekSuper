namespace TrekSuper.Core.Commands;

/// <summary>
/// Result of executing a game command.
/// </summary>
public class CommandResult
{
    public bool Success { get; init; }
    public bool ActionTaken { get; init; } // If true, enemies get to attack
    public double TimeUsed { get; init; }
    public string? Message { get; init; }
    public List<string> Messages { get; init; } = [];

    public static CommandResult Ok(bool actionTaken = false, double timeUsed = 0) => new()
    {
        Success = true,
        ActionTaken = actionTaken,
        TimeUsed = timeUsed
    };

    public static CommandResult Fail(string message) => new()
    {
        Success = false,
        Message = message
    };

    public static CommandResult WithMessage(string message, bool actionTaken = false) => new()
    {
        Success = true,
        ActionTaken = actionTaken,
        Message = message
    };
}
