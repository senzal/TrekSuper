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

try
{
    var client = new GameClient();
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
