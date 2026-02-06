using TrekSuper.Core;
using TrekSuper.Core.Enums;
using TrekSuper.GameService;

var engine = new GameEngine();
engine.NewGame(SkillLevel.Good, GameLength.Short, 12345);

var grid = engine.GetSectorGrid();

Console.WriteLine("Sector Grid (10x10):");
Console.WriteLine("   1 2 3 4 5 6 7 8 9 10");
for (int y = 1; y <= 10; y++)
{
    Console.Write($"{y,2} ");
    for (int x = 1; x <= 10; x++)
    {
        Console.Write(grid[x, y]);
        Console.Write(' ');
    }
    Console.WriteLine();
}

Console.WriteLine($"\nEnterprise position: ({engine.State.Ship.Sector.X}, {engine.State.Ship.Sector.Y})");
Console.WriteLine($"Enterprise character on grid: '{grid[engine.State.Ship.Sector.X, engine.State.Ship.Sector.Y]}'");

var renderer = new MarkdownRenderer();
var display = renderer.RenderGameDisplay(engine, new List<string>());

Console.WriteLine("\n=== Mermaid Diagram ===");
Console.WriteLine(display.MermaidDiagram ?? "NULL");
