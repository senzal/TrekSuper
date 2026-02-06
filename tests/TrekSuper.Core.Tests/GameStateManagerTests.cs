using TrekSuper.GameService;
using TrekSuper.Shared;
using Xunit;

namespace TrekSuper.Core.Tests;

public class GameStateManagerTests
{
    [Fact]
    public void CreateGame_ReturnsValidGameId()
    {
        // Arrange
        var renderer = new MarkdownRenderer();
        var manager = new GameStateManager(renderer);

        // Act
        var response = manager.CreateGame(SkillLevel.Good, GameLength.Short);

        // Assert
        Assert.True(response.Success);
        Assert.NotEqual(Guid.Empty, response.GameId);
        Assert.NotNull(response.InitialDisplay);
        Assert.Equal(1, manager.ActiveGameCount);
    }

    [Fact]
    public void CreateGame_MultipleConcurrentGames_AllUnique()
    {
        // Arrange
        var renderer = new MarkdownRenderer();
        var manager = new GameStateManager(renderer);

        // Act
        var game1 = manager.CreateGame(SkillLevel.Novice, GameLength.Short);
        var game2 = manager.CreateGame(SkillLevel.Expert, GameLength.Long);
        var game3 = manager.CreateGame(SkillLevel.Good, GameLength.Medium);

        // Assert
        Assert.True(game1.Success);
        Assert.True(game2.Success);
        Assert.True(game3.Success);
        Assert.NotEqual(game1.GameId, game2.GameId);
        Assert.NotEqual(game1.GameId, game3.GameId);
        Assert.NotEqual(game2.GameId, game3.GameId);
        Assert.Equal(3, manager.ActiveGameCount);
    }

    [Fact]
    public void ExecuteCommand_ValidCommand_ReturnsSuccess()
    {
        // Arrange
        var renderer = new MarkdownRenderer();
        var manager = new GameStateManager(renderer);
        var gameResponse = manager.CreateGame(SkillLevel.Good, GameLength.Short);
        var gameId = gameResponse.GameId;

        // Act - Execute status command
        var cmdResponse = manager.ExecuteCommand(gameId, "STATUS", Array.Empty<string>());

        // Assert
        Assert.NotNull(cmdResponse);
        Assert.NotNull(cmdResponse.Display);
    }

    [Fact]
    public void ExecuteCommand_InvalidGameId_ReturnsError()
    {
        // Arrange
        var renderer = new MarkdownRenderer();
        var manager = new GameStateManager(renderer);

        // Act
        var response = manager.ExecuteCommand(Guid.NewGuid(), "STATUS", Array.Empty<string>());

        // Assert
        Assert.False(response.Success);
        Assert.Contains("not found", response.ErrorMessage, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void ExecuteCommand_UnknownCommand_ReturnsError()
    {
        // Arrange
        var renderer = new MarkdownRenderer();
        var manager = new GameStateManager(renderer);
        var gameResponse = manager.CreateGame(SkillLevel.Good, GameLength.Short);

        // Act
        var response = manager.ExecuteCommand(gameResponse.GameId, "INVALIDCMD", Array.Empty<string>());

        // Assert
        Assert.False(response.Success);
        Assert.Contains("Unknown command", response.ErrorMessage, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void GetGameState_ValidGameId_ReturnsDisplay()
    {
        // Arrange
        var renderer = new MarkdownRenderer();
        var manager = new GameStateManager(renderer);
        var gameResponse = manager.CreateGame(SkillLevel.Good, GameLength.Short);

        // Act
        var stateResponse = manager.GetGameState(gameResponse.GameId);

        // Assert
        Assert.True(stateResponse.Success);
        Assert.NotNull(stateResponse.Display);
        Assert.NotNull(stateResponse.Display.Status);
    }

    [Fact]
    public void GetGameState_InvalidGameId_ReturnsError()
    {
        // Arrange
        var renderer = new MarkdownRenderer();
        var manager = new GameStateManager(renderer);

        // Act
        var response = manager.GetGameState(Guid.NewGuid());

        // Assert
        Assert.False(response.Success);
        Assert.Contains("not found", response.ErrorMessage, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void RemoveGame_ExistingGame_RemovesSuccessfully()
    {
        // Arrange
        var renderer = new MarkdownRenderer();
        var manager = new GameStateManager(renderer);
        var gameResponse = manager.CreateGame(SkillLevel.Good, GameLength.Short);

        // Act
        bool removed = manager.RemoveGame(gameResponse.GameId);

        // Assert
        Assert.True(removed);
        Assert.Equal(0, manager.ActiveGameCount);
    }

    [Fact]
    public void RemoveGame_NonExistentGame_ReturnsFalse()
    {
        // Arrange
        var renderer = new MarkdownRenderer();
        var manager = new GameStateManager(renderer);

        // Act
        bool removed = manager.RemoveGame(Guid.NewGuid());

        // Assert
        Assert.False(removed);
    }

    [Fact]
    public void MultipleGames_IndependentState()
    {
        // Arrange
        var renderer = new MarkdownRenderer();
        var manager = new GameStateManager(renderer);

        var game1 = manager.CreateGame(SkillLevel.Novice, GameLength.Short);
        var game2 = manager.CreateGame(SkillLevel.Emeritus, GameLength.Long);

        // Act - Get state for both
        var state1 = manager.GetGameState(game1.GameId);
        var state2 = manager.GetGameState(game2.GameId);

        // Assert - States should be independent
        Assert.NotNull(state1.Display);
        Assert.NotNull(state2.Display);

        // Game 1 is easier, Game 2 is harder
        // State should reflect different configurations
        Assert.NotEqual(state1.Display.Status.RemainingKlingons,
                       state2.Display.Status.RemainingKlingons);
    }
}
