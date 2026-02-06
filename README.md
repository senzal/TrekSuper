# TrekSuper - Modern C# Super Star Trek

A modern, multi-client architecture implementation of the classic 1978 Super Star Trek game in C# .NET 10.

## Architecture

TrekSuper uses a clean separation between game logic and presentation to support multiple client types:

```
┌─────────────────────────────────────┐
│         Clients                     │
│  Console | Web | Mobile | Desktop   │
└────────────────┬────────────────────┘
                 │
         ┌───────▼────────┐
         │  GameService   │  Multi-game state manager
         │  - Manages     │  Returns markdown/mermaid
         │    concurrent  │  for cross-platform
         │    games       │  rendering
         └───────┬────────┘
                 │
         ┌───────▼────────┐
         │  Core Engine   │  Pure game logic
         │  - No UI deps  │  Fully testable
         │  - Services    │
         └────────────────┘
```

## Projects

### TrekSuper.Core
Pure game logic with no UI dependencies:
- **GameEngine**: Orchestrates game flow
- **Services**: Combat, Navigation, Events, AI, Planets
- **Models**: Galaxy, Quadrant, Ship, Entities
- **Commands**: All player commands

### TrekSuper.GameService
Multi-game session management:
- **GameStateManager**: Manages concurrent games with unique IDs
- **MarkdownRenderer**: Renders game state as Markdown + Mermaid diagrams
- Server-side state prevents cheating

### TrekSuper.Shared
Communication contracts between service and clients:
- **Request/Response DTOs**: NewGame, ExecuteCommand, GetState
- **GameDisplayData**: Markdown content, Mermaid diagrams, status
- **Enums**: Shared enumerations

### TrekSuper.Console
Console client using Spectre.Console:
- Interactive terminal UI
- Markdown rendering
- Real-time status display

### TrekSuper.Core.Tests
Comprehensive unit tests:
- GameEngine tests
- GameStateManager tests
- MarkdownRenderer tests
- 30+ tests with 100% pass rate

## Features

### Currently Implemented ✅
- **Core Gameplay**: Full game loop with victory/defeat conditions
- **Combat**: Phasers, photon torpedoes, shields, death ray
- **Navigation**: Warp drive, impulse engines, docking
- **Scanning**: Short-range, long-range, star charts
- **Events**: Supernovas, tractor beams, base attacks
- **Multi-Game Support**: Concurrent games with unique IDs
- **Markdown Output**: Cross-platform compatible rendering
- **Unit Tests**: Comprehensive test coverage

### Planned Features 🚧
- **Enemy AI**: Tactical movement and combat decisions
- **Planet Interactions**: Beam down, mine dilithium, shuttle craft
- **Save/Load**: Game state persistence
- **Advanced Enemies**: Tholians with web, Romulans with cloaking
- **Web Client**: Blazor/ASP.NET Core implementation
- **Mobile Client**: Cross-platform mobile app
- **Multiplayer**: Spectating, chat, leaderboards

## Getting Started

### Prerequisites
- .NET 10.0 SDK or later

### Build
```bash
cd TrekSuper
dotnet build
```

### Run Console Client
```bash
cd src/TrekSuper.Console
dotnet run
```

### Run Tests
```bash
dotnet test
```

## Game Commands

| Command | Abbreviation | Description |
|---------|--------------|-------------|
| SRSCAN | SR | Short-range sensor scan |
| LRSCAN | LR | Long-range sensor scan |
| CHART | CH | Display galaxy star chart |
| STATUS | ST | Ship status report |
| DAMAGE | DA | Damage report |
| WARP | W | Warp drive movement |
| IMPULSE | I | Impulse engine movement |
| PHASERS | PH | Fire phasers |
| TORPEDO | TO | Fire photon torpedoes |
| SHIELDS | SH | Shield control |
| DOCK | DO | Dock at starbase |
| REST | R | Rest and repair |
| COMPUTER | CO | Computer functions |
| SCORE | SC | Show current score |
| HELP | H | Show command help |
| QUIT | Q | Quit game |

## Architecture Benefits

### For Development
- ✅ Clean separation enables easy testing
- ✅ Swap clients without changing game logic
- ✅ Markdown generation reusable across platforms
- ✅ Service can be in-process or remote API

### For Gameplay
- ✅ Server-side state prevents cheating
- ✅ Support for multiplayer features
- ✅ Game sessions can be persisted
- ✅ Multiple concurrent games

### For Deployment
- ✅ Console: Direct in-process service
- ✅ Web: Service as ASP.NET API
- ✅ Mobile: Same API endpoints
- ✅ Desktop: In-process or remote

## Technology Stack

- **.NET 10.0**: Latest framework features
- **C# 13**: Modern language features, nullable reference types
- **xUnit**: Unit testing
- **Spectre.Console**: Rich console UI
- **Markdig**: Markdown parsing (future web client)

## Contributing

This is a personal project, but suggestions and feedback are welcome!

## Original Game

Based on the classic Super Star Trek game from 1978, originally written in BASIC.

## License

See LICENSE file for details.

## Credits

- Original Super Star Trek game (1978)
- C implementation contributors
- Modern C# port by Claude AI and Steve

## Screenshots

### Console Client
```
╭────────────────────────────────────────────────────────────╮
│ Condition │ GREEN    │ Energy    │ 3000                    │
│ Stardate  │ 2453.7   │ Shield    │ 0                       │
│ Time Left │ 27.3     │ Torpedoes │ 10                      │
│ Klingons  │ 15       │ Bases     │ 3                       │
╰────────────────────────────────────────────────────────────╯

ℹ️ Entering Antares I Quadrant...

🚀 USS Enterprise - Antares I

 Sector Scan:
   1 2 3 4 5 6 7 8 9 10
 1 ・ ・ ・ ⭐ ・ ・ ・ ・ ・ ・
 2 ・ ・ ・ ・ ・ ・ ・ ・ ・ ・
 3 ・ ・ ・ ・ ・ 👾 ・ ・ ・ ・
 4 ・ ・ ・ ・ ・ ・ ・ ・ ・ ・
 5 ・ ・ ・ 🚀 ・ ・ ・ ・ ・ ・
 6 ・ ・ ・ ・ ・ ・ ⭐ ・ ・ ・
 7 ・ ・ ・ ・ ・ ・ ・ ・ ・ ・
 8 ・ ・ ・ ・ ・ ・ ・ ・ ・ 🏰
 9 ・ ・ ・ ・ ・ ・ ・ ・ ・ ・
10 ⭐ ・ ・ ・ ・ ・ ・ ・ ・ ・

COMMAND> _
```

## Future Vision

TrekSuper is designed to evolve into a full multi-platform game:

1. **Phase 1 (Current)**: Console client with core gameplay ✅
2. **Phase 2**: Add missing features (AI, planets, save/load)
3. **Phase 3**: Web client with Blazor
4. **Phase 4**: Mobile apps
5. **Phase 5**: Multiplayer features

The clean architecture makes each phase independent and testable!
