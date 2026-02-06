using TrekSuper.Core;
using TrekSuper.Core.Enums;
using Xunit;

namespace TrekSuper.Core.Tests;

public class GameEngineTests
{
    [Fact]
    public void NewGame_InitializesCorrectly()
    {
        // Arrange
        var engine = new GameEngine();

        // Act
        engine.NewGame(SkillLevel.Good, GameLength.Short);

        // Assert
        Assert.NotNull(engine.State);
        Assert.Equal(SkillLevel.Good, engine.State.Skill);
        Assert.Equal(GameLength.Short, engine.State.Length);
        Assert.True(engine.State.InitialKlingons > 0);
        Assert.True(engine.State.InitialBases > 0);
        Assert.True(engine.State.Ship.Energy > 0);
        Assert.Equal(10, engine.State.Ship.Torpedoes);
        Assert.False(engine.State.IsGameOver);
    }

    [Fact]
    public void NewGame_WithTournamentSeed_IsReproducible()
    {
        // Arrange
        var engine1 = new GameEngine();
        var engine2 = new GameEngine();
        int seed = 12345;

        // Act
        engine1.NewGame(SkillLevel.Expert, GameLength.Medium, seed);
        engine2.NewGame(SkillLevel.Expert, GameLength.Medium, seed);

        // Assert - both games should have identical initial state
        Assert.Equal(engine1.State.InitialKlingons, engine2.State.InitialKlingons);
        Assert.Equal(engine1.State.InitialBases, engine2.State.InitialBases);
        Assert.Equal(engine1.State.Ship.Quadrant, engine2.State.Ship.Quadrant);
        Assert.Equal(engine1.State.Ship.Sector, engine2.State.Ship.Sector);
        Assert.Equal(engine1.State.SelfDestructPassword, engine2.State.SelfDestructPassword);
    }

    [Theory]
    [InlineData(SkillLevel.Novice, GameLength.Short)]
    [InlineData(SkillLevel.Fair, GameLength.Medium)]
    [InlineData(SkillLevel.Good, GameLength.Long)]
    [InlineData(SkillLevel.Expert, GameLength.Medium)]
    [InlineData(SkillLevel.Emeritus, GameLength.Long)]
    public void NewGame_DifferentSkillsAndLengths_ScalesAppropriately(SkillLevel skill, GameLength length)
    {
        // Arrange
        var engine = new GameEngine();

        // Act
        engine.NewGame(skill, length);

        // Assert
        Assert.True(engine.State.InitialKlingons > 0);
        Assert.True(engine.State.TimeRemaining > 0);

        // Higher skill/length should have more enemies
        if (skill == SkillLevel.Emeritus && length == GameLength.Long)
        {
            Assert.True(engine.State.InitialKlingons >= 8, "Emeritus/Long should have many Klingons");
        }
    }

    [Fact]
    public void SuperCommander_OnlyAppearsAtGoodOrHigher()
    {
        // Arrange & Act
        var noviceEngine = new GameEngine();
        noviceEngine.NewGame(SkillLevel.Novice, GameLength.Short);

        var expertEngine = new GameEngine();
        expertEngine.NewGame(SkillLevel.Expert, GameLength.Short);

        // Assert
        Assert.Equal(0, noviceEngine.State.RemainingSuperCommanders);
        Assert.True(expertEngine.State.RemainingSuperCommanders >= 0); // May be 1
    }

    [Fact]
    public void GetSectorGrid_ReturnsCorrectDimensions()
    {
        // Arrange
        var engine = new GameEngine();
        engine.NewGame(SkillLevel.Good, GameLength.Short);

        // Act
        var grid = engine.GetSectorGrid();

        // Assert
        Assert.Equal(11, grid.GetLength(0)); // 0-10 (we use 1-10)
        Assert.Equal(11, grid.GetLength(1));

        // Enterprise should be placed
        bool foundEnterprise = false;
        for (int x = 1; x <= 10; x++)
        {
            for (int y = 1; y <= 10; y++)
            {
                if (grid[x, y] == 'E' || grid[x, y] == 'F')
                {
                    foundEnterprise = true;
                }
            }
        }
        Assert.True(foundEnterprise, "Enterprise should be on the grid");
    }

    [Fact]
    public void GameState_CalculateScore_WorksCorrectly()
    {
        // Arrange
        var engine = new GameEngine();
        engine.NewGame(SkillLevel.Good, GameLength.Short);

        // Simulate killing some Klingons
        engine.State.KlingonsKilled = 5;
        engine.State.CommandersKilled = 2;

        // Act
        var score = engine.State.CalculateScore();

        // Assert
        // 5 Klingons * 10 + 2 Commanders * 50 = 150
        // Multiplied by skill (3) = 450
        Assert.True(score >= 450, $"Score {score} should be at least 450");
    }

    [Fact]
    public void UpdateCondition_SetsCorrectConditions()
    {
        // Arrange
        var engine = new GameEngine();
        engine.NewGame(SkillLevel.Good, GameLength.Short);

        // Act & Assert - Initially should not be docked
        Assert.NotEqual(Condition.Docked, engine.State.Ship.Condition);

        // Simulate low energy
        engine.State.Ship.Energy = 500;
        engine.UpdateCondition();
        Assert.Equal(Condition.Yellow, engine.State.Ship.Condition);
    }

    [Fact]
    public void EnterQuadrant_PopulatesEntities()
    {
        // Arrange
        var engine = new GameEngine();
        engine.NewGame(SkillLevel.Good, GameLength.Short);
        var targetQuad = engine.State.Ship.Quadrant;

        // Act
        engine.EnterQuadrant(targetQuad);

        // Assert
        Assert.NotNull(engine.State.CurrentQuadrant);
        Assert.Equal(targetQuad, engine.State.CurrentQuadrant.Coordinate);
    }
}
