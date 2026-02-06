namespace TrekSuper.Console;

/// <summary>
/// Display settings for the console client.
/// </summary>
public class DisplaySettings
{
    /// <summary>
    /// Whether to use emoji characters for display.
    /// Auto-detected based on terminal capabilities.
    /// </summary>
    public bool UseEmojis { get; set; }

    /// <summary>
    /// Auto-detect if terminal supports emojis.
    /// Windows Terminal, VS Code terminal, and most modern terminals support emojis.
    /// cmd.exe and older terminals do not.
    /// </summary>
    public static DisplaySettings AutoDetect()
    {
        // Check if running in Windows Terminal, VS Code, or other modern terminals
        bool supportsEmojis = DetectEmojiSupport();

        return new DisplaySettings
        {
            UseEmojis = supportsEmojis
        };
    }

    private static bool DetectEmojiSupport()
    {
        // Check environment variables that indicate modern terminal
        var wtSession = Environment.GetEnvironmentVariable("WT_SESSION");
        var termProgram = Environment.GetEnvironmentVariable("TERM_PROGRAM");
        var terminalEmulator = Environment.GetEnvironmentVariable("TERMINAL_EMULATOR");

        // Windows Terminal
        if (!string.IsNullOrEmpty(wtSession))
            return true;

        // VS Code integrated terminal
        if (termProgram?.Contains("vscode", StringComparison.OrdinalIgnoreCase) == true)
            return true;

        // Other modern terminals
        if (terminalEmulator?.Contains("JetBrains", StringComparison.OrdinalIgnoreCase) == true)
            return true;

        // Check if console supports UTF-8
        try
        {
            var encoding = System.Console.OutputEncoding;
            if (encoding.CodePage == 65001) // UTF-8
            {
                // Could be Windows Terminal or other UTF-8 capable terminal
                // But cmd.exe also reports UTF-8, so we need additional checks

                // cmd.exe typically doesn't have these environment variables
                var colorTerm = Environment.GetEnvironmentVariable("COLORTERM");
                if (!string.IsNullOrEmpty(colorTerm))
                    return true;
            }
        }
        catch
        {
            // If we can't detect, default to false (safe fallback)
        }

        // Default to ASCII for maximum compatibility (cmd.exe, PowerShell 5.1, etc.)
        return false;
    }

    /// <summary>
    /// Create settings with emojis explicitly enabled.
    /// Use this if you know your terminal supports emojis (Windows Terminal, etc.)
    /// </summary>
    public static DisplaySettings WithEmojis() => new() { UseEmojis = true };

    /// <summary>
    /// Create settings with ASCII characters only.
    /// Use this for maximum compatibility (cmd.exe, older terminals).
    /// </summary>
    public static DisplaySettings AsciiOnly() => new() { UseEmojis = false };
}
