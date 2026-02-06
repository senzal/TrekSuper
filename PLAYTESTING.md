# TrekSuper Playtesting Strategy

## Overview

Comprehensive automated testing that validates both formatting and gameplay mechanics by simulating actual player behavior.

## Test Categories

### 1. Formatting Validation Tests
Tests that verify output formatting is correct and readable.

#### Long-Range Scan Formatting
- **Purpose**: Validate LRSCAN displays correctly with proper column headers
- **What it tests**:
  - Column headers match displayed data (shows only 3 columns, not all 8)
  - Row numbers are correct
  - Data format is consistent (KBS: Klingons, Bases, Stars)
  - Output is readable and aligned

#### Sector Scan Formatting
- **Purpose**: Validate short-range scan displays the Enterprise and entities
- **What it tests**:
  - Mermaid diagram generation works
  - Enterprise appears on the grid (🚀 in emoji mode, E in ASCII)
  - All 10 rows are present
  - Entities are correctly placed

#### Star Chart Formatting
- **Purpose**: Validate galaxy map displays correctly
- **What it tests**:
  - All 8x8 quadrants shown
  - Border formatting works
  - Unknown quadrants show as "..."
  - Known quadrants show data
  - Current position is clear

#### Status Report Formatting
- **Purpose**: Validate status information is complete
- **What it tests**:
  - All key stats displayed (stardate, time, energy, etc.)
  - Values are correct
  - Format is readable

### 2. Gameplay Mechanics Tests
Tests that simulate actual gameplay sequences.

#### Navigation Sequence
- **Tests**:
  - IMPULSE command moves ship correctly
  - Energy is consumed appropriately
  - WARP command works for quadrant changes
  - Position updates correctly
  - Messages are generated

#### Combat Sequence
- **Tests**:
  - Shield raising/lowering works
  - Phaser firing works
  - Torpedo firing works
  - Combat messages are generated
  - Energy/shield values update
  - Enemy detection works

#### Complete Game Session
- **Tests**: A full playthrough executing all major commands:
  1. STATUS - Check initial state
  2. SRSCAN - View sector
  3. LRSCAN - Scan surroundings
  4. CHART - View galaxy
  5. DAMAGE - Check systems
  6. SHIELDS - Adjust defense
  7. COMPUTER - Use computer functions
- **Validates**: All commands execute without crashes

#### All Commands Executable
- **Tests**: Every single command at least once
- **Commands tested**:
  - STATUS, SRSCAN, LRSCAN, CHART
  - DAMAGE, COMPUTER, SCORE, HELP
  - SHIELDS, SETWARP
  - IMPULSE, WARP, DOCK, REST
  - PHASERS, TORPEDO
  - QUIT
- **Validates**: No command causes a crash

### 3. Edge Case Tests
Tests that verify error handling and boundary conditions.

#### Edge Cases Tested:
1. **Invalid command** - "NOTACOMMAND"
   - Should return error gracefully
2. **Missing arguments** - IMPULSE with no args
   - Should return clear error message
3. **Invalid arguments** - SHIELDS abc
   - Should handle non-numeric input
4. **Excessive values** - SHIELDS 99999
   - Should cap or reject
5. **Non-existent game** - Get state for invalid GUID
   - Should return "game not found"

### 4. Multi-Game Isolation Tests
Tests that verify multiple concurrent games don't interfere.

#### Tests:
- Create 3 games with different settings
- Execute different commands on each
- Verify:
  - Game states are independent
  - Commands affect only the target game
  - Different seeds produce different games
  - Game IDs are unique

## How to Run Playtests

### Run All Playtests
```bash
cd C:\Users\steve\Documents\GitHub\TrekSuper
dotnet test --filter "FullyQualifiedName~PlaytestingTests"
```

### Run Specific Category
```bash
# Formatting tests only
dotnet test --filter "Playtest_*Formatting"

# Gameplay tests only
dotnet test --filter "Playtest_*Sequence" or "Playtest_CompleteGameSession"

# Edge cases only
dotnet test --filter "Playtest_EdgeCases"
```

### Run with Detailed Output
```bash
dotnet test --filter "FullyQualifiedName~PlaytestingTests" --logger "console;verbosity=detailed"
```

## What Gets Validated

### ✅ Formatting
- Output is readable and properly aligned
- Headers match data columns
- Entities are visible (Enterprise, Klingons, etc.)
- Tables and grids are formatted correctly

### ✅ Gameplay
- All commands execute without crashes
- Game state updates correctly
- Energy/shields/torpedoes change appropriately
- Messages are generated for player feedback
- Navigation works (impulse and warp)
- Combat works (phasers and torpedoes)

### ✅ Error Handling
- Invalid commands handled gracefully
- Missing arguments caught with helpful messages
- Invalid input (non-numeric, out of range) rejected
- Non-existent games return appropriate errors

### ✅ Multi-Game Support
- Multiple concurrent games work
- Games are isolated from each other
- Each game maintains independent state
- Game IDs are unique

## Benefits

1. **Automated Validation**: No manual testing needed for basic functionality
2. **Regression Detection**: Catch breaking changes immediately
3. **Format Verification**: Ensure output stays readable
4. **Player Experience**: Simulate real gameplay patterns
5. **CI/CD Ready**: Can run in automated build pipelines
6. **Documentation**: Tests serve as usage examples

## Future Enhancements

- [ ] Test save/load functionality when implemented
- [ ] Test enemy AI behavior when implemented
- [ ] Test planet interactions when implemented
- [ ] Add performance benchmarks
- [ ] Test with different terminal types (emoji vs ASCII)
- [ ] Add screenshot/output comparison tests
- [ ] Test tournament mode with seeded games
- [ ] Validate scoring calculations

## Example Test Output

```
=== PLAYTEST: Long-Range Scan Formatting ===

Long-Range Scan Output:
Long-range scan from quadrant 6 - 4:
    3   4   5
5: 002 006 009
6: 005 005 009
7: 009 009 006

✓ Long-range scan formatting validated

=== PLAYTEST: Complete Game Session ===

Mission: Destroy 2 Klingons
Time limit: 7.0 days
Starting energy: 5000
Starbases: 3

[1] Check status (STATUS)
  Success: True
  Energy: 5000
  Shield: 0

[2] Short range scan (SRSCAN)
  Success: True
  Enterprise visible: True

...

✓ Complete game session validated
```

## Maintenance

- Update tests when adding new commands
- Add tests for new features
- Keep expected output formats in sync with changes
- Review test coverage regularly
- Update seed values if random generation changes
