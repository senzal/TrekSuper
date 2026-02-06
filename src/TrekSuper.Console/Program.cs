using Spectre.Console;
using TrekSuper.Console;

AnsiConsole.Clear();

AnsiConsole.Write(
    new FigletText("SUPER STAR TREK")
        .Centered()
        .Color(Color.Cyan));

AnsiConsole.MarkupLine("[dim]C# .NET 10 Edition - Multi-Client Architecture[/]");
AnsiConsole.MarkupLine("[dim]Based on the classic 1978 game[/]");
AnsiConsole.WriteLine();

// Check for command-line arguments to force emoji mode
var displaySettings = DisplaySettings.AutoDetect();

if (args.Contains("--emojis") || args.Contains("-e"))
{
    displaySettings = DisplaySettings.WithEmojis();
    AnsiConsole.MarkupLine("[green]✓ Emoji mode enabled[/]");
}
else if (args.Contains("--ascii") || args.Contains("-a"))
{
    displaySettings = DisplaySettings.AsciiOnly();
    AnsiConsole.MarkupLine("[yellow]✓ ASCII mode enabled[/]");
}
else
{
    var mode = displaySettings.UseEmojis ? "emoji" : "ASCII";
    AnsiConsole.MarkupLine($"[dim]Display mode: {mode} (auto-detected)[/]");
    AnsiConsole.MarkupLine($"[dim]Use --emojis or --ascii to override[/]");
}

AnsiConsole.WriteLine();

try
{
    var client = new GameClient(displaySettings);
    await client.RunAsync();
}
catch (Exception ex)
{
    AnsiConsole.MarkupLine($"[red]Fatal error: {ex.Message}[/]");
    AnsiConsole.WriteException(ex);
}

AnsiConsole.WriteLine();
AnsiConsole.MarkupLine("[dim]Press any key to exit...[/]");
Console.ReadKey();
