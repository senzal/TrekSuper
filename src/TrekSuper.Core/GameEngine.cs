using TrekSuper.Core.Commands;
using TrekSuper.Core.Enums;
using TrekSuper.Core.Models;
using TrekSuper.Core.Models.Entities;
using TrekSuper.Core.Models.Ship;
using TrekSuper.Core.Services;

namespace TrekSuper.Core;

/// <summary>
/// Main game engine - orchestrates game logic.
/// </summary>
public class GameEngine
{
    private readonly Random _random = new();

    public GameState State { get; private set; } = new();

    // Services
    public CombatService Combat { get; }
    public NavigationService Navigation { get; }
    public EventService Events { get; }
    public TutorialService Tutorial { get; }

    // Events for UI
    public event Action<string>? OnMessage;
    public event Action<string>? OnWarning;
    public event Action<string>? OnError;

    public GameEngine()
    {
        Combat = new CombatService(this);
        Navigation = new NavigationService(this);
        Events = new EventService(this);
        Tutorial = new TutorialService(this);
    }

    /// <summary>
    /// Initializes a new game.
    /// </summary>
    public void NewGame(SkillLevel skill, GameLength length, int? tournamentSeed = null)
    {
        State = new GameState
        {
            Skill = skill,
            Length = length,
            TournamentNumber = tournamentSeed ?? 0,
            Random = tournamentSeed.HasValue ? new Random(tournamentSeed.Value) : new Random()
        };

        InitializeGalaxy();
        InitializeShip();
        InitializeTime();

        // Enter starting quadrant
        EnterQuadrant(State.Ship.Quadrant, true);

        Message($"Your orders, Captain {GetPlayerTitle()}:\n");
        Message($"  Destroy the {State.InitialKlingons} Klingon warships which have invaded");
        Message($"  the galaxy before they can attack Federation Headquarters");
        Message($"  on stardate {State.InitialStardate + State.InitialTime:F1}.");
        Message($"  This gives you {State.InitialTime:F1} days.");
        Message($"  There {(State.InitialBases == 1 ? "is" : "are")} {State.InitialBases} starbase{(State.InitialBases == 1 ? "" : "s")} in the galaxy");
        Message($"  for resupply of your ship.\n");
    }

    private string GetPlayerTitle() => State.Skill switch
    {
        SkillLevel.Novice => "Novice",
        SkillLevel.Fair => "Fair",
        SkillLevel.Good => "",
        SkillLevel.Expert => "Expert",
        SkillLevel.Emeritus => "Commodore",
        _ => ""
    };

    private void InitializeGalaxy()
    {
        var random = State.Random;

        // Calculate number of Klingons based on skill and game length
        int skillFactor = (int)State.Skill;
        int lengthFactor = (int)State.Length;

        State.InitialKlingons = 2 * skillFactor * lengthFactor + (int)(random.NextDouble() * (skillFactor + 1) * lengthFactor) + 1;
        State.InitialCommanders = Math.Min(skillFactor + 1, State.InitialKlingons / 4 + 1);

        if (State.Skill >= SkillLevel.Good)
        {
            State.RemainingSuperCommanders = 1;
        }

        State.InitialBases = Math.Max(2, random.Next(3, 6));
        State.InitialStars = random.Next(200, 400);
        State.InitialPlanets = random.Next(4, Galaxy.MaxPlanets + 1);

        State.RemainingKlingons = State.InitialKlingons - State.InitialCommanders;
        State.RemainingCommanders = State.InitialCommanders;
        State.RemainingBases = State.InitialBases;

        // Place starbases
        for (int i = 0; i < State.InitialBases; i++)
        {
            QuadrantCoordinate baseQuad;
            do
            {
                baseQuad = Galaxy.GetRandomQuadrant(random);
            } while (State.Galaxy.StarbaseLocations.Contains(baseQuad));

            State.Galaxy.StarbaseLocations.Add(baseQuad);
            var currentData = State.Galaxy.GetQuadrantData(baseQuad);
            State.Galaxy.SetQuadrantData(baseQuad, currentData + 10); // Add base
        }

        // Place stars
        for (int i = 0; i < State.InitialStars; i++)
        {
            var quad = Galaxy.GetRandomQuadrant(random);
            var currentData = State.Galaxy.GetQuadrantData(quad);
            if (currentData % 10 < 9) // Max 9 stars per quadrant
            {
                State.Galaxy.SetQuadrantData(quad, currentData + 1);
            }
            else
            {
                i--; // Try again
            }
        }

        // Place Klingons
        for (int i = 0; i < State.RemainingKlingons; i++)
        {
            var quad = Galaxy.GetRandomQuadrant(random);
            State.Galaxy.AddKlingon(quad);
        }

        // Place Commanders
        for (int i = 0; i < State.InitialCommanders; i++)
        {
            QuadrantCoordinate cmdQuad;
            do
            {
                cmdQuad = Galaxy.GetRandomQuadrant(random);
            } while (State.Galaxy.CommanderLocations.Contains(cmdQuad));

            State.Galaxy.CommanderLocations.Add(cmdQuad);
            State.Galaxy.AddKlingon(cmdQuad);
        }

        // Place Super-Commander (if applicable)
        if (State.RemainingSuperCommanders > 0)
        {
            QuadrantCoordinate scQuad;
            do
            {
                scQuad = Galaxy.GetRandomQuadrant(random);
            } while (State.Galaxy.StarbaseLocations.Contains(scQuad));

            State.Galaxy.SuperCommanderLocation = scQuad;
            State.Galaxy.AddKlingon(scQuad);
        }

        // Place planets
        for (int i = 0; i < State.InitialPlanets; i++)
        {
            var quad = Galaxy.GetRandomQuadrant(random);
            var planetClass = (PlanetClass)(random.Next(1, 4));
            var hasCrystals = random.NextDouble() < 0.25;

            var planet = new Planet(SectorCoordinate.Invalid, planetClass, hasCrystals)
            {
                QuadrantLocation = quad
            };
            State.Galaxy.Planets.Add(planet);
        }

        // Place the mysterious "Thing"
        State.Galaxy.ThingLocation = Galaxy.GetRandomQuadrant(random);

        // Calculate initial resources
        State.InitialResources = (State.InitialKlingons + 4 * State.InitialCommanders) * lengthFactor;
        State.RemainingResources = State.InitialResources;
    }

    private void InitializeShip()
    {
        var random = State.Random;

        // Start in a random quadrant
        State.Ship.Quadrant = Galaxy.GetRandomQuadrant(random);

        // Random sector within quadrant
        State.Ship.Sector = new SectorCoordinate(random.Next(1, 11), random.Next(1, 11));

        // Full energy and supplies
        State.Ship.Energy = State.Ship.MaxEnergy;
        State.Ship.Shield = 0; // Shields down initially
        State.Ship.ShieldsUp = false;
        State.Ship.Torpedoes = State.Ship.MaxTorpedoes;
        State.Ship.LifeSupportReserves = State.Ship.MaxLifeSupportReserves;
        State.Ship.Probes = State.Ship.MaxProbes;
        State.Ship.WarpFactor = 5.0;
        State.Ship.Condition = Condition.Green;

        // Generate self-destruct password
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
        State.SelfDestructPassword = new string(Enumerable.Range(0, 6)
            .Select(_ => chars[random.Next(chars.Length)]).ToArray());
    }

    private void InitializeTime()
    {
        // Initial stardate
        State.InitialStardate = 100 * (31 * State.Random.NextDouble() + 20);
        State.Stardate = State.InitialStardate;

        // Time limit based on game length
        State.InitialTime = 7.0 * (int)State.Length;
        State.TimeRemaining = State.InitialTime;

        // Schedule future events
        ScheduleInitialEvents();
    }

    private void ScheduleInitialEvents()
    {
        var random = State.Random;

        // Schedule supernova
        State.FutureEvents[(int)FutureEventType.Supernova] =
            State.Stardate + ExpRand(0.5 * State.InitialTime);

        // Schedule tractor beam (if commanders exist)
        if (State.RemainingCommanders > 0)
        {
            State.FutureEvents[(int)FutureEventType.TractorBeam] =
                State.Stardate + ExpRand(1.5 * State.InitialTime / State.RemainingCommanders);
        }

        // Schedule snapshot
        State.FutureEvents[(int)FutureEventType.Snapshot] =
            State.Stardate + ExpRand(0.5 * State.InitialTime);

        // Schedule base attack (if commanders exist)
        if (State.RemainingCommanders > 0)
        {
            State.FutureEvents[(int)FutureEventType.BaseAttack] =
                State.Stardate + ExpRand(0.3 * State.InitialTime);
        }

        // Schedule super-commander move
        if (State.RemainingSuperCommanders > 0)
        {
            State.FutureEvents[(int)FutureEventType.SuperCommanderMoves] =
                State.Stardate + 0.2777;
        }
    }

    private double ExpRand(double mean) =>
        -mean * Math.Log(1.0 - State.Random.NextDouble());

    /// <summary>
    /// Enters a new quadrant and populates it.
    /// </summary>
    public void EnterQuadrant(QuadrantCoordinate coord, bool isInitial = false)
    {
        State.Ship.Quadrant = coord;
        State.CurrentQuadrant = new Quadrant(coord);
        State.JustEnteredQuadrant = true;

        var random = State.Random;
        var quadrant = State.CurrentQuadrant;
        var galaxyData = State.Galaxy.GetQuadrantData(coord);

        int klingons = galaxyData / 100;
        int bases = (galaxyData % 100) / 10;
        int stars = galaxyData % 10;

        // Place the Enterprise
        if (!quadrant.IsSectorEmpty(State.Ship.Sector))
        {
            State.Ship.Sector = quadrant.FindEmptySector(random);
        }

        // Place stars
        for (int i = 0; i < stars; i++)
        {
            var sector = quadrant.FindEmptySector(random);
            if (sector.IsValid)
            {
                var star = new Star(sector);
                quadrant.Stars.Add(star);
                quadrant.PlaceEntity(star);
            }
        }

        // Place starbase
        if (bases > 0)
        {
            var sector = quadrant.FindEmptySector(random);
            if (sector.IsValid)
            {
                quadrant.Starbase = new Starbase(sector, coord);
                quadrant.PlaceEntity(quadrant.Starbase);
            }
        }

        // Check for planet in this quadrant
        var planet = State.Galaxy.GetPlanetAt(coord);
        if (planet != null)
        {
            var sector = quadrant.FindEmptySector(random);
            if (sector.IsValid)
            {
                planet.Position = sector;
                quadrant.Planet = planet;
                quadrant.PlaceEntity(planet);
            }
        }

        // Place Klingons
        bool hasCommander = State.Galaxy.CommanderLocations.Contains(coord);
        bool hasSuperCommander = State.Galaxy.SuperCommanderLocation == coord;

        int regularKlingons = klingons;
        if (hasCommander) regularKlingons--;
        if (hasSuperCommander) regularKlingons--;

        // Place regular Klingons
        for (int i = 0; i < regularKlingons; i++)
        {
            var sector = quadrant.FindEmptySector(random);
            if (sector.IsValid)
            {
                double power = 300 + 200 * random.NextDouble() + 50 * (int)State.Skill;
                var klingon = new Klingon(sector, power);
                quadrant.AddEnemy(klingon);
            }
        }

        // Place Commander
        if (hasCommander)
        {
            var sector = quadrant.FindEmptySector(random);
            if (sector.IsValid)
            {
                double power = 600 + 200 * random.NextDouble() + 75 * (int)State.Skill;
                var commander = new Commander(sector, power);
                quadrant.AddEnemy(commander);
            }
        }

        // Place Super-Commander
        if (hasSuperCommander)
        {
            var sector = quadrant.FindEmptySector(random);
            if (sector.IsValid)
            {
                double power = 800 + 300 * random.NextDouble() + 100 * (int)State.Skill;
                var sc = new SuperCommander(sector, power);
                quadrant.AddEnemy(sc);
            }
        }

        // Update condition
        UpdateCondition();

        // Update star chart
        State.Galaxy.UpdateChart(coord);

        // Check for Romulans in neutral zone
        if (coord.X == 1 || coord.X == 8 || coord.Y == 1 || coord.Y == 8)
        {
            if (random.NextDouble() < 0.1)
            {
                quadrant.IsInNeutralZone = true;
                var sector = quadrant.FindEmptySector(random);
                if (sector.IsValid)
                {
                    double power = 400 + 200 * random.NextDouble();
                    var romulan = new Romulan(sector, power);
                    quadrant.AddEnemy(romulan);
                }
            }
        }

        if (!isInitial)
        {
            Message($"\nEntering {GetQuadrantName(coord)} Quadrant...");
        }

        // Sort enemies by distance
        SortEnemiesByDistance();
    }

    /// <summary>
    /// Updates enemy distances from the Enterprise.
    /// </summary>
    public void SortEnemiesByDistance()
    {
        if (State.CurrentQuadrant == null) return;

        foreach (var enemy in State.CurrentQuadrant.Enemies)
        {
            enemy.Distance = State.Ship.Sector.DistanceTo(enemy.Position);
            enemy.AverageDistance = (enemy.Distance + enemy.AverageDistance) / 2.0;
        }

        // Sort by distance
        var sorted = State.CurrentQuadrant.Enemies.OrderBy(e => e.Distance).ToList();
        State.CurrentQuadrant.Enemies.Clear();
        State.CurrentQuadrant.Enemies.AddRange(sorted);
    }

    /// <summary>
    /// Updates ship condition based on current state.
    /// </summary>
    public void UpdateCondition()
    {
        if (State.Ship.IsDocked)
        {
            State.Ship.Condition = Condition.Docked;
        }
        else if (State.CurrentQuadrant?.EnemyCount > 0)
        {
            State.Ship.Condition = Condition.Red;
        }
        else if (State.Ship.Energy < 1000)
        {
            State.Ship.Condition = Condition.Yellow;
        }
        else
        {
            State.Ship.Condition = Condition.Green;
        }
    }

    /// <summary>
    /// Gets a quadrant name for display.
    /// </summary>
    public static string GetQuadrantName(QuadrantCoordinate coord)
    {
        string[] quadrantNames1 =
        {
            "", "Antares", "Rigel", "Procyon", "Vega",
            "Canopus", "Altair", "Sagittarius", "Pollux"
        };

        string[] quadrantNames2 =
        {
            "", "Sirius", "Deneb", "Capella", "Betelgeuse",
            "Aldebaran", "Regulus", "Arcturus", "Spica"
        };

        if (coord.Y <= 4)
        {
            return $"{quadrantNames1[coord.X]} {ToRoman(coord.Y)}";
        }
        else
        {
            return $"{quadrantNames2[coord.X]} {ToRoman(coord.Y - 4)}";
        }
    }

    private static string ToRoman(int n) => n switch
    {
        1 => "I",
        2 => "II",
        3 => "III",
        4 => "IV",
        _ => n.ToString()
    };

    /// <summary>
    /// Gets the sector grid for display.
    /// </summary>
    public char[,] GetSectorGrid()
    {
        var grid = new char[11, 11];

        // Initialize with empty space
        for (int x = 1; x <= 10; x++)
        {
            for (int y = 1; y <= 10; y++)
            {
                grid[x, y] = '.';
            }
        }

        if (State.CurrentQuadrant == null) return grid;

        // Place entities
        for (int x = 1; x <= 10; x++)
        {
            for (int y = 1; y <= 10; y++)
            {
                grid[x, y] = State.CurrentQuadrant.GetCharAt(x, y);
            }
        }

        // Place Enterprise
        grid[State.Ship.Sector.X, State.Ship.Sector.Y] = State.Ship.IsEnterprise ? 'E' : 'F';

        return grid;
    }

    /// <summary>
    /// Sends a message to be displayed.
    /// </summary>
    public void Message(string text) => OnMessage?.Invoke(text);

    /// <summary>
    /// Sends a warning message.
    /// </summary>
    public void Warning(string text) => OnWarning?.Invoke(text);

    /// <summary>
    /// Sends an error message.
    /// </summary>
    public void Error(string text) => OnError?.Invoke(text);

    /// <summary>
    /// Display the current score.
    /// </summary>
    public void Score()
    {
        int score = State.CalculateScore();

        Message("\n*** SCORE ***\n");
        Message($"Klingons killed:      {State.KlingonsKilled,4} x 10 = {State.KlingonsKilled * 10,5}");
        Message($"Commanders killed:    {State.CommandersKilled,4} x 50 = {State.CommandersKilled * 50,5}");
        Message($"Super-Cmdr killed:    {State.SuperCommandersKilled,4} x 200 = {State.SuperCommandersKilled * 200,5}");
        Message($"Romulans killed:      {State.RomulansKilled,4} x 20 = {State.RomulansKilled * 20,5}");
        Message($"Casualties:           {State.Ship.Casualties,4} x -5 = {State.Ship.Casualties * -5,5}");
        Message($"Stars destroyed:      {State.StarsDestroyed,4} x -5 = {State.StarsDestroyed * -5,5}");
        Message($"Planets destroyed:    {State.PlanetsDestroyed,4} x -10 = {State.PlanetsDestroyed * -10,5}");
        Message($"Bases destroyed:      {State.BasesDestroyed,4} x -100 = {State.BasesDestroyed * -100,5}");
        Message($"Calls for help:       {State.HelpCalls,4} x -50 = {State.HelpCalls * -50,5}");
        Message($"Skill multiplier:     x{(int)State.Skill}");
        Message($"                      -------");
        Message($"TOTAL SCORE:          {score,7}");

        if (State.IsVictory)
        {
            Message("\n*** VICTORY! ***");
            Message("The Federation is saved!");
        }
        else if (State.IsGameOver)
        {
            Message($"\nGame over: {State.Outcome}");
        }
    }
}
