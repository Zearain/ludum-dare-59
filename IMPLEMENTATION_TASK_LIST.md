# Implementation Task List

This document is the dependency-ordered implementation task list for `Ghost Frequency: Storm Relay`.

It is intended to translate the design and architecture documents into a practical build sequence.

Related documents:

- Design: `GHOST_FREQUENCY_STORM_RELAY_MINI_GDD.md`
- Architecture: `ARCHITECTURE_OVERVIEW.md`

This is a living document. Update it when the project architecture, scope, or implementation order changes.

## Usage

The list is ordered by dependency, not by discipline.

That means:

- Earlier tasks should unlock later tasks.
- If a task is blocked, do not skip too far ahead unless the blocked work is intentionally deferred.
- Optional polish work should remain below the MVP path.

## Milestone Definitions

### Milestone 1: Project boots

The project opens, builds, and loads the main scene without errors.

### Milestone 2: First playable

The player can move through a generated sector, read a signal, activate a relay, and reach a win or fail state.

### Milestone 3: Full run

The game supports the complete 5-relay-plus-final-sector structure with procedural variation.

### Milestone 4: Submission-ready

The game has basic juice, clear feedback, a score screen, and stable run flow.

## Dependency-Ordered Task List

### 1. Establish project structure

Dependencies: none

Tasks:

- Create the `scenes/` folder structure from `ARCHITECTURE_OVERVIEW.md`.
- Create the `scripts/` folder structure from `ARCHITECTURE_OVERVIEW.md`.
- Add placeholder scenes for the main runtime pieces.
- Add placeholder C# files for the core systems, entities, components, and data classes.
- Confirm the project still loads and `dotnet build` succeeds.

Why first:

- Everything else depends on stable file locations and compilable types.

### 2. Create the composition root

Dependencies:

- `1. Establish project structure`

Tasks:

- Create `scenes/main.tscn`.
- Add top-level nodes for `GameManager`, `RunController`, `SectorRoot`, `PlayerShip`, `Camera2D`, and `HUD`.
- Attach the matching scripts.
- Ensure the scene can run even if most child behavior is still placeholder.

Definition of done:

- The project starts into `main.tscn` without missing script or scene errors.

### 3. Implement bootstrap and run state ownership

Dependencies:

- `2. Create the composition root`

Tasks:

- Implement `GameManager` bootstrap responsibilities.
- Decide whether input actions will be created at runtime and wire that in consistently.
- Implement the base `RunController`.
- Add run-level state fields: seed, sector index, run state, active objective, elapsed time.
- Add restart support.

Definition of done:

- A run can be initialized, reset, and restarted without sector gameplay yet existing.

### 4. Implement the player ship shell

Dependencies:

- `2. Create the composition root`
- `3. Implement bootstrap and run state ownership`

Tasks:

- Create `PlayerShip` as a thin orchestrator entity.
- Add `ShipMovementComponent`.
- Add `ShipHullComponent`.
- Cache required child nodes in `_Ready()`.
- Expose hull and death events for controller/UI use.

Definition of done:

- The player ship exists in the scene, moves, and has a hull value that can change.

### 5. Implement basic camera and playable movement feel

Dependencies:

- `4. Implement the player ship shell`

Tasks:

- Make the camera follow the player.
- Tune movement until navigation feels readable and responsive.
- Add bounds behavior if needed to prevent the player from leaving the playable area.

Definition of done:

- Moving around an empty sector already feels acceptable for the jam target.

### 6. Create the sector container and lifecycle

Dependencies:

- `3. Implement bootstrap and run state ownership`

Tasks:

- Create the `SectorRoot` scene/node contract.
- Define how generated hazards and objectives are added and removed.
- Add sector cleanup between transitions.
- Make `RunController` own active sector lifetime.

Definition of done:

- The game can spawn, clear, and replace sector content safely.

### 7. Implement objective entities

Dependencies:

- `6. Create the sector container and lifecycle`

Tasks:

- Implement `RelayBuoy`.
- Implement `PirateWreck`.
- Add activation zones or proximity checks.
- Add `RelayActivationComponent` for hold-to-activate behavior.
- Expose completion events.

Definition of done:

- A test objective can be placed in the world and completed by the player.

### 8. Implement run progression through objectives

Dependencies:

- `3. Implement bootstrap and run state ownership`
- `7. Implement objective entities`

Tasks:

- Make `RunController` respond to relay completion.
- Advance sector index on relay activation.
- Switch from relay objectives to the final pirate wreck objective on the last sector.
- End the run on final recovery.
- End the run on player death.

Definition of done:

- The controller can drive a complete run flow using manually placed objectives.

### 9. Implement the signal model

Dependencies:

- `7. Implement objective entities`
- `8. Implement run progression through objectives`

Tasks:

- Create `SignalReading` data.
- Implement `SignalSystem` as the single source of truth for signal output.
- Compute direction toward the active objective.
- Compute signal strength.
- Define hooks for distortion and scanner effects.

Definition of done:

- The game can produce a signal reading for the current objective even before distortion exists.

### 10. Implement the scanner

Dependencies:

- `4. Implement the player ship shell`
- `9. Implement the signal model`

Tasks:

- Add `ScannerComponent` to the player ship.
- Add scan pulse input.
- Add cooldown tracking.
- Make scan pulses temporarily improve clarity, reveal signal strength, or counter attenuation.

Definition of done:

- The player can actively interact with the signal system rather than passively reading it.

### 11. Build the HUD foundation

Dependencies:

- `3. Implement bootstrap and run state ownership`
- `4. Implement the player ship shell`
- `9. Implement the signal model`
- `10. Implement the scanner`

Tasks:

- Create `HUD` scene.
- Display timer.
- Display hull.
- Display scan cooldown.
- Display signal direction and strength.
- Display current objective text.

Definition of done:

- The main gameplay loop is readable without debug prints.

### 12. Implement sector generation data types

Dependencies:

- `6. Create the sector container and lifecycle`

Tasks:

- Add `SectorDefinition`.
- Add `HazardSpawnData`.
- Define the data needed to describe one sector.
- Add a deterministic seed derivation strategy from run seed plus sector index.

Definition of done:

- A sector can be described as generated data before any hazard scenes are created.

### 13. Implement the sector generator skeleton

Dependencies:

- `6. Create the sector container and lifecycle`
- `12. Implement sector generation data types`

Tasks:

- Implement `SectorGenerator`.
- Generate arena bounds.
- Generate spawn and objective positions.
- Reserve a viable corridor between them.
- Spawn only the objective initially.

Definition of done:

- Each sector can be generated and entered with a reachable objective in an otherwise simple arena.

### 14. Connect generation to run flow

Dependencies:

- `8. Implement run progression through objectives`
- `13. Implement the sector generator skeleton`

Tasks:

- Make `RunController` request a fresh sector at run start.
- Request a new generated sector after each objective completion.
- Generate 5 relay sectors followed by 1 final sector.
- Reset player position on sector entry.

Definition of done:

- A complete end-to-end run exists with generated sectors and no hazards yet.

### 15. Implement the first hazard type

Dependencies:

- `13. Implement the sector generator skeleton`
- `4. Implement the player ship shell`

Tasks:

- Implement `StaticCloud`.
- Add `HazardDamageComponent` or equivalent shared damage path.
- Spawn clouds from generated hazard data.
- Apply damage over time while inside the hazard.

Definition of done:

- The player can now fail from environmental damage during a generated run.

### 16. Tune the first playable loop

Dependencies:

- `10. Implement the scanner`
- `11. Build the HUD foundation`
- `14. Connect generation to run flow`
- `15. Implement the first hazard type`

Tasks:

- Balance movement, scan cooldown, and relay activation time.
- Verify the first sector teaches the loop clearly.
- Verify a run can be won or lost.
- Trim anything non-essential that blocks a playable build.

Definition of done:

- Milestone 2 reached: the game is first playable.

### 17. Add distortion rules

Dependencies:

- `9. Implement the signal model`
- `10. Implement the scanner`
- `14. Connect generation to run flow`

Tasks:

- Implement `Noise` signal jitter.
- Implement `Echo` false-direction behavior.
- Bind distortion types to generated sector data.
- Make scanner use interact cleanly with distortion.

Definition of done:

- Sectors meaningfully differ in how the signal is interpreted.

### 18. Add remaining hazard types

Dependencies:

- `15. Implement the first hazard type`
- `13. Implement the sector generator skeleton`

Tasks:

- Implement `DebrisField`.
- Implement `LightningArc`.
- Spawn both from generated data.
- Scale hazard variety and density by sector index.

Definition of done:

- The full MVP hazard set is available.

### 19. Implement scoring and run results

Dependencies:

- `8. Implement run progression through objectives`
- `15. Implement the first hazard type`
- `11. Build the HUD foundation`

Tasks:

- Add `RunResult`.
- Track damage taken, hull remaining, elapsed time, and sectors cleared.
- Implement `ScoreSystem`.
- Choose the first-pass score model.

Definition of done:

- The game can produce a stable result object for both success and failure.

### 20. Create the score screen and end flow

Dependencies:

- `19. Implement scoring and run results`

Tasks:

- Create `score_screen.tscn`.
- Display final time.
- Display damage or hull result.
- Display score or rank.
- Add restart action.

Definition of done:

- The run ends cleanly in a user-readable summary screen.

### 21. Add transmission fragments and narrative wrapper

Dependencies:

- `8. Implement run progression through objectives`
- `20. Create the score screen and end flow`

Tasks:

- Add short relay-completion transmission lines.
- Add the final `Night Saint` recovery line.
- Hook narrative delivery into relay and final recovery events.

Definition of done:

- The pirate trail has enough narrative framing to support the fantasy.

### 22. Add first-pass visual identity

Dependencies:

- `11. Build the HUD foundation`
- `18. Add remaining hazard types`

Tasks:

- Apply neon colors to HUD and objective feedback.
- Make hazards visually distinct and readable.
- Add signal pulse and interference feedback.
- Add basic transition polish between sectors.

Definition of done:

- The game reads as a coherent neon storm-tracing experience, not just gray prototypes.

### 23. Add first-pass audio feedback

Dependencies:

- `10. Implement the scanner`
- `15. Implement the first hazard type`
- `20. Create the score screen and end flow`

Tasks:

- Add scan pulse sound.
- Add damage feedback sound.
- Add relay activation sound.
- Add run complete or fail stingers.

Definition of done:

- Core actions and states have audible feedback.

### 24. Balance full-run pacing

Dependencies:

- `17. Add distortion rules`
- `18. Add remaining hazard types`
- `19. Implement scoring and run results`
- `22. Add first-pass visual identity`

Tasks:

- Tune sector length and density so a strong run lasts about 12-15 minutes.
- Check that early sectors teach and late sectors escalate.
- Check that damage and time scoring both matter.
- Remove frustration spikes caused by unfair generation.

Definition of done:

- Milestone 3 reached: full-run pacing is functional.

### 25. Stabilize and verify the build

Dependencies:

- All MVP tasks above

Tasks:

- Run `dotnet build` and fix compile issues.
- Test repeated runs for progression bugs.
- Verify restart always resets state cleanly.
- Verify every generated sector is traversable.
- Fix null references, race conditions, and bad teardown paths.

Definition of done:

- The game is reliable enough to submit.

### 26. Final submission pass

Dependencies:

- `25. Stabilize and verify the build`

Tasks:

- Trim unfinished extras.
- Improve clarity in HUD labels and score text.
- Improve menu or score screen readability.
- Validate that the game communicates the objective immediately.
- Package a final jam-safe build.

Definition of done:

- Milestone 4 reached: submission-ready build.

## MVP Critical Path

If time is short, prioritize this exact chain:

1. `1. Establish project structure`
2. `2. Create the composition root`
3. `3. Implement bootstrap and run state ownership`
4. `4. Implement the player ship shell`
5. `6. Create the sector container and lifecycle`
6. `7. Implement objective entities`
7. `8. Implement run progression through objectives`
8. `9. Implement the signal model`
9. `10. Implement the scanner`
10. `11. Build the HUD foundation`
11. `12. Implement sector generation data types`
12. `13. Implement the sector generator skeleton`
13. `14. Connect generation to run flow`
14. `15. Implement the first hazard type`
15. `16. Tune the first playable loop`
16. `19. Implement scoring and run results`
17. `20. Create the score screen and end flow`
18. `25. Stabilize and verify the build`

Everything else is secondary to shipping a full playable run.

## Fast Cut Strategy

If implementation slips, cut in this order:

1. Remove `LightningArc`.
2. Simplify `Echo` behavior.
3. Reduce total sectors from 6 to 4.
4. Remove boost entirely if it was added.
5. Simplify score to `time + hull remaining` display.
6. Reduce narrative text to a minimal intro and final recovery line.

Do not cut:

- signal reading
- scanning
- relay progression
- final sector ending
- score or result screen

Those systems are the identity of the game.
