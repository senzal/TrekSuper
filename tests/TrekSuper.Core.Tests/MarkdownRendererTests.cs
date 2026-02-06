using TrekSuper.Core;
using TrekSuper.Core.Enums;
using TrekSuper.GameService;
using Xunit;

namespace TrekSuper.Core.Tests;

public class MarkdownRendererTests
{
    [Fact]
    public void RenderGameDisplay_ReturnsValidMarkdown()
    {
        // Arrange
        var engine = new GameEngine();
        engine.NewGame(SkillLevel.Good, GameLength.Short);
        var renderer = new MarkdownRenderer();
        var messages = new List<string> { "INFO: Test message" };

        // Act
        var display = renderer.RenderGameDisplay(engine, messages);

        // Assert
        Assert.NotNull(display);
        Assert.NotNull(display.MarkdownContent);
        Assert.NotEmpty(display.MarkdownContent);
        Assert.Contains("USS Enterprise", display.MarkdownContent);
        Assert.NotNull(display.Status);
        Assert.True(display.Status.Energy > 0);
        Assert.NotEmpty(display.AvailableCommands);
    }

    [Fact]
    public void RenderSectorScan_ContainsGrid()
    {
        // Arrange
        var engine = new GameEngine();
        engine.NewGame(SkillLevel.Good, GameLength.Short);
        var renderer = new MarkdownRenderer();

        // Act
        var scanMarkdown = renderer.RenderSectorScan(engine);

        // Assert
        Assert.NotNull(scanMarkdown);
        Assert.Contains("Sector Scan", scanMarkdown);
        Assert.Contains("```", scanMarkdown); // Code block for grid
        Assert.Contains("Legend", scanMarkdown);
    }

    [Fact]
    public void RenderGalaxyMap_ContainsChart()
    {
        // Arrange
        var engine = new GameEngine();
        engine.NewGame(SkillLevel.Good, GameLength.Short);
        var renderer = new MarkdownRenderer();

        // Act
        var mapMarkdown = renderer.RenderGalaxyMap(engine);

        // Assert
        Assert.NotNull(mapMarkdown);
        Assert.Contains("Galaxy Map", mapMarkdown);
        Assert.Contains("Format:", mapMarkdown);
        Assert.Contains("...", mapMarkdown); // Unknown quadrants
    }

    [Fact]
    public void RenderStatus_ContainsShipInfo()
    {
        // Arrange
        var engine = new GameEngine();
        engine.NewGame(SkillLevel.Good, GameLength.Short);
        var renderer = new MarkdownRenderer();

        // Act
        var statusMarkdown = renderer.RenderStatus(engine);

        // Assert
        Assert.NotNull(statusMarkdown);
        Assert.Contains("Status Report", statusMarkdown);
        Assert.Contains("Energy", statusMarkdown);
        Assert.Contains("Shields", statusMarkdown);
        Assert.Contains("Torpedoes", statusMarkdown);
        Assert.Contains("Mission Progress", statusMarkdown);
        Assert.Contains("Position", statusMarkdown);
    }

    [Fact]
    public void RenderDamageReport_AllOperational_ShowsOK()
    {
        // Arrange
        var engine = new GameEngine();
        engine.NewGame(SkillLevel.Good, GameLength.Short);
        var renderer = new MarkdownRenderer();

        // Act
        var damageMarkdown = renderer.RenderDamageReport(engine);

        // Assert
        Assert.NotNull(damageMarkdown);
        Assert.Contains("Damage Report", damageMarkdown);
        Assert.Contains("operational", damageMarkdown, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void RenderSectorMermaid_GeneratesDiagram()
    {
        // Arrange
        var engine = new GameEngine();
        engine.NewGame(SkillLevel.Good, GameLength.Short);
        var renderer = new MarkdownRenderer();

        // Act
        var mermaid = renderer.RenderSectorMermaid(engine);

        // Assert
        Assert.NotNull(mermaid);
        Assert.Contains("```mermaid", mermaid);
        Assert.Contains("graph LR", mermaid);
        Assert.Contains("Sector Grid", mermaid);
    }

    [Fact]
    public void GetAvailableCommands_ReturnsStandardCommands()
    {
        // Arrange
        var engine = new GameEngine();
        engine.NewGame(SkillLevel.Good, GameLength.Short);
        var renderer = new MarkdownRenderer();

        // Act
        var commands = renderer.GetAvailableCommands(engine);

        // Assert
        Assert.NotNull(commands);
        Assert.NotEmpty(commands);
        Assert.Contains(commands, c => c.Command == "SRSCAN");
        Assert.Contains(commands, c => c.Command == "WARP");
        Assert.Contains(commands, c => c.Command == "PHASERS");
        Assert.Contains(commands, c => c.Command == "TORPEDO");
        Assert.Contains(commands, c => c.Command == "SHIELDS");
        Assert.Contains(commands, c => c.Command == "STATUS");
    }

    [Fact]
    public void RenderGameDisplay_HandlesMessages()
    {
        // Arrange
        var engine = new GameEngine();
        engine.NewGame(SkillLevel.Good, GameLength.Short);
        var renderer = new MarkdownRenderer();
        var messages = new List<string>
        {
            "INFO: Game started",
            "WARN: Low energy",
            "ERROR: System failure"
        };

        // Act
        var display = renderer.RenderGameDisplay(engine, messages);

        // Assert
        Assert.NotNull(display.Messages);
        Assert.Equal(3, display.Messages.Count);
        Assert.Contains(display.Messages, m => m.Type == Shared.MessageType.Info);
        Assert.Contains(display.Messages, m => m.Type == Shared.MessageType.Warning);
        Assert.Contains(display.Messages, m => m.Type == Shared.MessageType.Error);
    }
}
