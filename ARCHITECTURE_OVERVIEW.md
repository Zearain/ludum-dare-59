# Architecture Overview

This document is the living technical architecture for `Ghost Frequency: Storm Relay`.

It should evolve as the implementation changes. Update it when scene ownership, system responsibilities, runtime flow, or folder structure change in a meaningful way.

Related documents:

- Design: `GHOST_FREQUENCY_STORM_RELAY_MINI_GDD.md`
- Architecture: `ARCHITECTURE_OVERVIEW.md`

## Purpose

This document translates the mini-GDD into a practical Godot 4.6 + C# project structure that is realistic for a 48-hour Ludum Dare build.

The architecture is intentionally small:

- One composition root scene
- One run controller
- One procedural sector generator
- One player ship
- A small set of hazards and objectives
- Passive UI that reads from gameplay systems

The main goals are:

- Ship a complete 12-15 minute run
- Keep responsibilities separated enough to stay editable under jam pressure
- Preserve replayability through deterministic sector generation
- Avoid over-engineering features that do not support the core signal-tracing loop

## Design Reference

The gameplay and scope assumptions in this document come from `GHOST_FREQUENCY_STORM_RELAY_MINI_GDD.md`.

Key assumptions inherited from the GDD:

- The game is a top-down 2D neon space-tracing game
- A full run should take about 12-15 minutes when played well
- Progression is built around 5 relay sectors and 1 final source sector
- Replayability comes from procedural sector generation, not a large handcrafted world
- Scoring is based on time used and damage taken
- The final objective is the recovered black box of the pirate ship `Night Saint`

If any of those assumptions change, update both the GDD and this document.

## Architectural Principles

### 1. Keep the composition root simple

`main.tscn` should own the top-level runtime assembly:

- global flow
- current run
- current sector container
- player ship
- camera
- HUD

### 2. Use thin scene entities

Scene-bound nodes such as the player ship, relays, and hazards should primarily orchestrate child components and Godot lifecycle hooks.

Avoid putting unrelated game rules directly inside entity nodes.

### 3. Keep rules in focused systems or components

Procedural generation, signal interpretation, run progression, and scoring should live in small C# types with clear ownership.

### 4. Generate one sector at a time

The game should not attempt to manage a giant continuous world. Each run advances through a sequence of generated sectors.

This is safer for jam scope, simpler to debug, and easier to tune.

### 5. Prefer deterministic generation

Sector generation should be reproducible from a run seed plus sector index.

This helps with:

- debugging
- balancing
- comparing runs
- replayability with predictable structure

## Recommended Project Layout

```text
scenes/
  main.tscn
  game/
	game_manager.tscn
	run_controller.tscn
	sector_root.tscn
  player/
	player_ship.tscn
	camera_rig.tscn
  objectives/
	relay_buoy.tscn
	pirate_wreck.tscn
  hazards/
	static_cloud.tscn
	debris_field.tscn
	lightning_arc.tscn
  ui/
	hud.tscn
	score_screen.tscn

scripts/
  core/
	StateMachine.cs
	RunSeed.cs
	RunTimer.cs
  data/
	SectorDefinition.cs
	HazardSpawnData.cs
	RunResult.cs
	SignalReading.cs
  systems/
	GameManager.cs
	RunController.cs
	SectorGenerator.cs
	SignalSystem.cs
	ScoreSystem.cs
  entities/
	PlayerShip.cs
	RelayBuoy.cs
	PirateWreck.cs
  components/
	ShipMovementComponent.cs
	ShipHullComponent.cs
	ScannerComponent.cs
	SignalEmitterComponent.cs
	RelayActivationComponent.cs
	HazardDamageComponent.cs
	ObjectiveMarkerComponent.cs
  interfaces/
	IDamageSource.cs
	ISignalSource.cs
	IActivatableObjective.cs
```

This layout is a target structure, not a requirement to create every file immediately.

For jam speed, create only what is needed for the current milestone.

## Composition Root

`scenes/main.tscn` should be the runtime assembly point for the game.

Recommended top-level children:

- `GameManager`
- `RunController`
- `SectorRoot`
- `PlayerShip`
- `Camera2D`
- `HUD`

Responsibilities at this level:

- boot the run
- hold the active generated sector
- keep the player and UI persistent across sector transitions
- make restart and end-of-run flow straightforward

## Runtime Flow

Expected run sequence:

1. `GameManager` performs basic bootstrap work such as runtime input setup if needed.
2. `RunController` creates the run seed and resets per-run state.
3. `RunController` requests the first sector from `SectorGenerator`.
4. `SectorGenerator` spawns the objective and hazards into `SectorRoot`.
5. `SignalSystem` produces signal feedback based on the active objective and sector distortion.
6. The player navigates, scans, and reaches the relay.
7. Relay activation completes and `RunController` advances to the next sector.
8. After the fifth relay sector, the final source sector is generated.
9. The player reaches the pirate wreck and recovers the black box.
10. `ScoreSystem` builds the final result and the UI switches to the score screen.

Failure flow:

1. Player hull reaches zero.
2. `RunController` marks the run as failed.
3. `ScoreSystem` produces a failed run result.
4. The UI offers restart.

## Core Systems

### GameManager

Purpose:

- Own app-level bootstrap work
- Create input actions at runtime if the project continues the pattern noted in `AGENTS.md`
- Expose only the small amount of global state that truly needs to be globally reachable

Should not own:

- sector generation
- scoring
- per-objective logic
- player movement

### RunController

Purpose:

- Own the run lifecycle
- Track current sector index
- Track active objective
- Decide whether the run is active, completed, or failed
- Coordinate transitions between sectors

This is the main flow controller and likely the most important gameplay system.

Suggested run states:

- `Intro`
- `InSector`
- `ActivatingRelay`
- `Transition`
- `FinalRecovery`
- `RunComplete`
- `RunFailed`

### SectorGenerator

Purpose:

- Generate one playable sector at a time
- Place spawn, objective, hazards, and distortion data
- Guarantee a traversable route from entry to objective
- Scale density and difficulty by sector index

Important constraint:

The generator should build compact arenas with safe-enough paths, not open-ended maps.

### SignalSystem

Purpose:

- Centralize all gameplay signal calculations
- Convert world state into player-facing signal feedback
- Apply distortion rules consistently for HUD, scanner, and audio

Inputs:

- player position
- active objective position
- current scan state
- current sector distortion modifiers

Outputs:

- displayed direction
- signal strength
- jitter amount
- false reading flags

### ScoreSystem

Purpose:

- Convert the final run state into a result summary
- Score primarily from elapsed time and damage taken
- Keep scoring logic out of UI code

Recommended minimum scoring inputs:

- completion state
- total time
- damage taken
- hull remaining
- sectors cleared

## Entities

### PlayerShip

Purpose:

- Represent the controllable ship in the world
- Orchestrate movement, hull, and scanner components
- Forward Godot lifecycle callbacks into the relevant components

The ship should remain a thin entity node rather than becoming the owner of all game logic.

### RelayBuoy

Purpose:

- Represent the sector objective in sectors 1-5
- Expose range checks and activation completion
- Notify `RunController` when activated

### PirateWreck

Purpose:

- Represent the final objective in the last sector
- Trigger end-of-run recovery flow
- Provide the narrative endpoint of the pirate transmission trail

## Components

### ShipMovementComponent

- Handles top-down ship movement
- Keeps movement arcade-like and readable
- Optional future extension point for boost

### ShipHullComponent

- Tracks current hull
- Applies damage
- Emits death or depleted-hull events

### ScannerComponent

- Manages scan pulse cooldown
- Requests fresh or stronger readings from `SignalSystem`
- Owns temporary scan-state behavior if needed

### RelayActivationComponent

- Handles hold-to-activate timing
- Reports completion to the relay or wreck owner

### HazardDamageComponent

- Shared damage behavior for clouds, debris, or lightning interactions
- Keeps damage rules reusable across hazard scenes

### ObjectiveMarkerComponent

- Optional helper for visible or hidden objective state
- Useful if the visual layer needs a separate marker behavior from gameplay logic

## Data Model

Keep procedural and scoring data in plain C# data types.

### SectorDefinition

Should contain:

- sector index
- seed
- arena bounds
- spawn point
- objective point
- hazard density
- distortion modifiers

### HazardSpawnData

Should contain:

- hazard type
- position
- size or radius
- optional timing parameters

### SignalReading

Should contain:

- direction vector
- strength from `0..1`
- jitter amount
- confidence or trustworthiness indicator

### RunResult

Should contain:

- completed flag
- final time
- damage taken
- hull remaining
- sectors cleared
- final score or rank

## Sector Generation Strategy

The generator should produce one sector per stage of the run.

Per sector process:

1. Derive a sector seed from the run seed and sector index.
2. Pick arena bounds and difficulty parameters.
3. Place entry and objective points with meaningful travel distance.
4. Reserve a rough viable corridor between them.
5. Populate hazards around that corridor.
6. Add one or two distortion modifiers.
7. Instantiate the generated content into `SectorRoot`.

Progression expectations:

- Sector 1 should teach the loop clearly.
- Sectors 2-4 should escalate interference and routing pressure.
- Sector 5 should be the hardest relay.
- Sector 6 should be the final push to the pirate wreck.

## Hazards

The architecture should support three hazard types in the MVP.

### StaticCloud

- Area damage over time
- Shapes safe navigation space
- Likely the simplest hazard to implement first

### DebrisField

- Physical obstacle cluster
- Forces route changes and piloting precision
- Can remain static for MVP scope

### LightningArc

- Periodic burst or telegraphed strike
- Adds timing pressure near routes and objectives
- Good late-sector hazard escalator

All hazards should be spawnable by the same generator pipeline.

## Signal Model

The signal is the central mechanic and should have a single source of truth.

The player should never read signal direction directly from objective transforms in UI code. All signal feedback should flow through `SignalSystem`.

That enables:

- consistent distortion rules
- scanner interactions
- UI and audio reacting to the same reading
- easier balancing and debugging

## UI Architecture

The HUD should be passive and presentation-focused.

HUD data sources:

- timer from run state
- hull from the player hull component
- scanner cooldown from the scanner component
- signal direction and strength from `SignalSystem`
- objective state text from `RunController`

Recommended HUD elements:

- timer
- hull bar
- scanner cooldown
- signal arrow
- signal strength meter
- objective label

The score screen should consume a `RunResult` and avoid recomputing gameplay values.

## State Boundaries

Recommended ownership split:

- `GameManager`: application-level bootstrap
- `RunController`: run-level game flow
- `SectorGenerator`: sector construction
- `PlayerShip`: player-facing orchestration
- `HUD`: presentation only

This boundary keeps the project understandable during fast jam iteration.

## What Not To Build Into The Architecture

The current design does not need:

- combat systems
- inventory or upgrades
- enemy AI
- persistent campaign progression
- large autoload networks
- a giant world streaming system
- a separate hacking minigame architecture

If one of these becomes necessary, update the GDD first, then update this document.

## Implementation Priorities

Recommended order of implementation:

1. `main.tscn`, `GameManager`, and `RunController`
2. `PlayerShip` with movement and hull
3. `RelayBuoy` objective loop
4. `SectorGenerator` with one hazard type
5. `SignalSystem` and HUD signal display
6. Remaining hazards and scoring
7. Final sector and score screen polish

## Maintenance Notes

This is a living document.

Update it when:

- scene ownership changes
- new systems become permanent parts of the architecture
- data models change significantly
- procedural generation changes shape
- signal rules move between systems
- the MVP scope expands or shrinks

When editing architecture, keep these two documents aligned:

- `GHOST_FREQUENCY_STORM_RELAY_MINI_GDD.md`
- `ARCHITECTURE_OVERVIEW.md`
