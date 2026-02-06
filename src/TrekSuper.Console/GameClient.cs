using Spectre.Console;
using TrekSuper.GameService;
using TrekSuper.Shared;

namespace TrekSuper.Console;

/// <summary>
/// Console client for TrekSuper game.
/// </summary>
public class GameClient
{
    private readonly IGameStateManager _gameService;
    private readonly DisplaySettings _displaySettings;
    private Guid? _currentGameId;
    private GameDisplayData? _currentDisplay;

    public GameClient(DisplaySettings? displaySettings = null)
    {
        var renderer = new MarkdownRenderer();
        _gameService = new GameStateManager(renderer);
        _displaySettings = displaySettings ?? DisplaySettings.AutoDetect();
    }

    public async Task RunAsync()
    {
        // Start new game
        var (skill, length) = await PromptForGameSettings();

        var response = _gameService.CreateGame(skill, length);

        if (!response.Success)
        {
            AnsiConsole.MarkupLine($"[red]Failed to create game: {response.ErrorMessage}[/]");
            return;
        }

        _currentGameId = response.GameId;
        _currentDisplay = response.InitialDisplay;

        // Show initial state
        RenderDisplay(_currentDisplay);

        // Game loop
        bool running = true;
        while (running && !IsGameOver())
        {
            var command = await PromptForCommand();

            if (string.IsNullOrWhiteSpace(command))
                continue;

            var parts = command.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries);
            var cmdName = parts[0].ToUpperInvariant();
            var args = parts.Length > 1 ? parts[1..] : Array.Empty<string>();

            if (cmdName == "QUIT" || cmdName == "Q")
            {
                if (AnsiConsole.Confirm("Are you sure you want to quit?"))
                {
                    running = false;
                    continue;
                }
            }

            // Execute command
            var cmdResponse = _gameService.ExecuteCommand(_currentGameId!.Value, cmdName, args);
            _currentDisplay = cmdResponse.Display;

            AnsiConsole.Clear();
            RenderDisplay(_currentDisplay);

            if (!cmdResponse.Success && cmdResponse.ErrorMessage != null)
            {
                AnsiConsole.MarkupLine($"[red]{cmdResponse.ErrorMessage}[/]");
            }

            if (cmdResponse.IsGameOver)
            {
                ShowGameOver(cmdResponse.Outcome!.Value);
                running = false;
            }
        }

        // Cleanup
        if (_currentGameId.HasValue)
        {
            _gameService.RemoveGame(_currentGameId.Value);
        }
    }

    private async Task<(SkillLevel, GameLength)> PromptForGameSettings()
    {
        var skill = AnsiConsole.Prompt(
            new SelectionPrompt<SkillLevel>()
                .Title("Select [green]skill level[/]:")
                .AddChoices(Enum.GetValues<SkillLevel>()));

        var length = AnsiConsole.Prompt(
            new SelectionPrompt<GameLength>()
                .Title("Select [green]game length[/]:")
                .AddChoices(Enum.GetValues<GameLength>()));

        return (skill, length);
    }

    private async Task<string> PromptForCommand()
    {
        return AnsiConsole.Prompt(
            new TextPrompt<string>("[yellow]COMMAND>[/] ")
                .AllowEmpty());
    }

    private void RenderDisplay(GameDisplayData display)
    {
        // Render status header
        var statusTable = new Table()
            .Border(TableBorder.Rounded)
            .BorderColor(Color.Grey)
            .AddColumn(new TableColumn("Status").Centered())
            .AddColumn(new TableColumn("Value").Centered())
            .AddColumn(new TableColumn("Status").Centered())
            .AddColumn(new TableColumn("Value").Centered());

        var condition = display.Status.Condition;
        var conditionColor = condition switch
        {
            "Red" => "red",
            "Yellow" => "yellow",
            "Green" => "green",
            "Docked" => "cyan",
            _ => "white"
        };

        statusTable.AddRow(
            "Condition", $"[{conditionColor}]{condition}[/]",
            "Energy", $"{display.Status.Energy}");
        statusTable.AddRow(
            "Stardate", $"{display.Status.Stardate:F1}",
            "Shield", $"{display.Status.Shield}");
        statusTable.AddRow(
            "Time Left", $"{display.Status.TimeRemaining:F1}",
            "Torpedoes", $"{display.Status.Torpedoes}");
        statusTable.AddRow(
            "Klingons", $"{display.Status.RemainingKlingons}",
            "Bases", $"{display.Status.RemainingBases}");

        AnsiConsole.Write(statusTable);
        AnsiConsole.WriteLine();

        // Render main content
        if (!string.IsNullOrWhiteSpace(display.MarkdownContent))
        {
            AnsiConsole.MarkupLine(display.MarkdownContent);
        }

        // Render Mermaid diagram (simplified for console)
        if (!string.IsNullOrWhiteSpace(display.MermaidDiagram))
        {
            RenderMermaidAsText(display.MermaidDiagram);
        }

        // Show recent messages
        if (display.Messages.Any())
        {
            AnsiConsole.WriteLine();
            var messagePanel = new Panel(string.Join("\n", display.Messages.Select(m => m.Content)))
            {
                Header = new PanelHeader("Messages"),
                Border = BoxBorder.Rounded,
                BorderStyle = new Style(Color.Grey)
            };
            AnsiConsole.Write(messagePanel);
        }
    }

    private void RenderMermaidAsText(string mermaid)
    {
        // Extract the grid content from mermaid and display it
        // Use emojis if supported, otherwise fall back to ASCII

        var lines = mermaid.Split('\n', StringSplitOptions.RemoveEmptyEntries);
        var gridLines = lines.Where(l => l.Contains("R") && l.Contains("[\"")).ToList();

        if (gridLines.Any())
        {
            AnsiConsole.WriteLine();

            if (_displaySettings.UseEmojis)
            {
                AnsiConsole.MarkupLine("[bold]🌌 Sector Scan:[/]");
            }
            else
            {
                AnsiConsole.MarkupLine("[bold]Sector Scan:[/]");
            }

            AnsiConsole.MarkupLine("   1 2 3 4 5 6 7 8 9 10");

            int row = 1;
            foreach (var line in gridLines)
            {
                // Extract content between [" and "]
                var start = line.IndexOf("[\"") + 2;
                var end = line.IndexOf("\"]");
                if (start > 1 && end > start)
                {
                    var content = line.Substring(start, end - start);

                    if (_displaySettings.UseEmojis)
                    {
                        // Keep emojis as-is for modern terminals
                        content = content.Replace("・", " ");
                        AnsiConsole.MarkupLine($"{row,2} {content}");
                    }
                    else
                    {
                        // Convert emojis to colored ASCII characters for compatibility
                        content = content
                            .Replace("🚀", "[cyan]E[/]")  // Enterprise
                            .Replace("👾", "[red]K[/]")   // Klingon
                            .Replace("💀", "[red]C[/]")   // Commander
                            .Replace("☠️", "[red]S[/]")   // Super-Commander
                            .Replace("🏰", "[green]B[/]") // Starbase
                            .Replace("⭐", "[yellow]*[/]") // Star
                            .Replace("🪐", "[blue]@[/]")   // Planet
                            .Replace("・", ".");           // Empty space

                        AnsiConsole.MarkupLine($"{row,2} {content}");
                    }
                    row++;
                }
            }
            AnsiConsole.WriteLine();

            // Legend
            if (_displaySettings.UseEmojis)
            {
                AnsiConsole.MarkupLine("[dim]🚀=Enterprise 👾=Klingon 💀=Commander 🏰=Starbase ⭐=Star 🪐=Planet[/]");
            }
            else
            {
                AnsiConsole.MarkupLine("[dim]Legend: [cyan]E[/]=Enterprise [red]K[/]=Klingon [red]C[/]=Commander [green]B[/]=Starbase [yellow]*[/]=Star [blue]@[/]=Planet[/]");
            }
        }
    }

    private bool IsGameOver()
    {
        return _currentDisplay != null &&
               _currentDisplay.Status.RemainingKlingons <= 0 &&
               _currentDisplay.Status.RemainingCommanders <= 0 &&
               _currentDisplay.Status.RemainingSuperCommanders <= 0;
    }

    private void ShowGameOver(GameOutcome outcome)
    {
        AnsiConsole.WriteLine();

        var rule = new Rule("[red]GAME OVER[/]")
        {
            Style = Style.Parse("red")
        };
        AnsiConsole.Write(rule);

        var message = outcome switch
        {
            GameOutcome.Won => "[green]🎉 VICTORY! You have saved the Federation![/]",
            GameOutcome.ShipDestroyed => "[red]💥 Your ship has been destroyed![/]",
            GameOutcome.TimeExpired => "[yellow]⏰ Time has run out![/]",
            GameOutcome.FederationLost => "[red]The Federation has fallen![/]",
            GameOutcome.Quit => "[grey]Game quit[/]",
            _ => "[grey]Game ended[/]"
        };

        AnsiConsole.MarkupLine(message);
        AnsiConsole.WriteLine();
    }
}
