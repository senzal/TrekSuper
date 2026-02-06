using TrekSuper.Core;
using TrekSuper.Core.Enums;
using TrekSuper.GameService;
using Xunit;
using Xunit.Abstractions;

namespace TrekSuper.Core.Tests;

public class SectorScanTests
{
    private readonly ITestOutputHelper _output;

    public SectorScanTests(ITestOutputHelper output)
    {
        _output = output;
    }

    [Fact]
    public void GetSectorGrid_ShowsEnterprise()
    {
        // Arrange
        var engine = new GameEngine();
        engine.NewGame(SkillLevel.Good, GameLength.Short, 12345);

        // Act
        var grid = engine.GetSectorGrid();

        // Assert - Enterprise should be at its sector position
        var enterpriseChar = grid[engine.State.Ship.Sector.X, engine.State.Ship.Sector.Y];

        _output.WriteLine($"Enterprise position: ({engine.State.Ship.Sector.X}, {engine.State.Ship.Sector.Y})");
        _output.WriteLine($"Character at position: '{enterpriseChar}'");
        _output.WriteLine($"CurrentQuadrant is null: {engine.State.CurrentQuadrant == null}");

        // Print the grid
        _output.WriteLine("\nSector Grid:");
        _output.WriteLine("   1 2 3 4 5 6 7 8 9 10");
        for (int y = 1; y <= 10; y++)
        {
            var line = $"{y,2} ";
            for (int x = 1; x <= 10; x++)
            {
                line += grid[x, y] + " ";
            }
            _output.WriteLine(line);
        }

        Assert.True(enterpriseChar == 'E' || enterpriseChar == 'F',
            $"Enterprise should be 'E' or 'F' but was '{enterpriseChar}'");
    }

    [Fact]
    public void MermaidDiagram_IncludesEnterprise()
    {
        // Arrange
        var engine = new GameEngine();
        engine.NewGame(SkillLevel.Good, GameLength.Short, 12345);
        var renderer = new MarkdownRenderer();

        // Act
        var mermaid = renderer.RenderSectorMermaid(engine);

        // Assert
        Assert.NotNull(mermaid);
        _output.WriteLine("Mermaid diagram:");
        _output.WriteLine(mermaid);

        // Should contain the Enterprise emoji
        Assert.Contains("🚀", mermaid);
    }

    [Fact]
    public void InitialGameDisplay_IncludesSectorScan()
    {
        // Arrange
        var engine = new GameEngine();
        engine.NewGame(SkillLevel.Good, GameLength.Short, 12345);
        var renderer = new MarkdownRenderer();
        var messages = new List<string> { "INFO: Game started" };

        // Act
        var display = renderer.RenderGameDisplay(engine, messages);

        // Assert
        Assert.NotNull(display);
        Assert.NotNull(display.MermaidDiagram);

        _output.WriteLine("Mermaid Diagram included: " + (display.MermaidDiagram != null));
        if (display.MermaidDiagram != null)
        {
            _output.WriteLine("First 500 chars:");
            _output.WriteLine(display.MermaidDiagram.Substring(0, Math.Min(500, display.MermaidDiagram.Length)));
        }

        Assert.Contains("🚀", display.MermaidDiagram);
    }
}
