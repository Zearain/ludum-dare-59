# Ghost Frequency: Storm Relay

## Overview

`Ghost Frequency: Storm Relay` is a top-down 2D neon space-tracing game for Ludum Dare built around the theme `Signal`.

The player pilots a small hunter ship through procedurally generated storm sectors, restoring drifting relay buoys to reconstruct the last transmission of the infamous pirate dreadnought `Night Saint`. Each run is a compact 12-15 minute chase through hazards, interference, and false readings, ending at the pirate wreck's black box.

The game is designed for a 48-hour implementation scope:

- One player ship
- One main objective chain
- One core scan-and-navigate loop
- Chunk-based procedural generation for replayability
- Score based on time used and damage taken

## Pillars

- Trace a signal through hostile space instead of following a simple waypoint.
- Make storm interference and hazard navigation the core challenge.
- Deliver a full run with a clear ending in about 15 minutes.
- Keep the scope small enough to build and polish in 48 hours.
- Use procedural sector remixing to create replayability without huge content demands.

## One-Sentence Pitch

Restore ancient relays inside a deadly ion storm to follow the final transmission of a legendary pirate ship, then race to the wreck with the least damage and fastest time.

## Player Fantasy

You are a skilled signal hunter threading a fragile neon ship through impossible weather, piecing together a lost pirate transmission while surviving the storm long enough to claim the final black box.

## Core Loop

1. Enter a storm sector.
2. Read distorted signal feedback and move toward the current objective.
3. Dodge hazards while using scan pulses to regain clarity.
4. Reach and activate the relay buoy.
5. Receive the next transmission fragment and continue to the next sector.
6. Reach the final source sector and recover the pirate black box.
7. Receive a final score based on time and damage taken.

## Run Structure

- Total ideal run length: 12-15 minutes
- Relay sectors: 5
- Final source sector: 1
- Total sectors per run: 6
- Average time per sector: 1.5-2.5 minutes

Pacing by phase:

- Early run: light hazards, simple readings, teaches scanning and relay activation.
- Mid run: more debris, stronger interference, false echoes, tighter safe routes.
- Final run: heaviest storm conditions and a direct push to the pirate wreck.

## Narrative Frame

Years ago, the pirate dreadnought `Night Saint` vanished inside the storm belt while fleeing pursuit. A fragmented transmission has resurfaced across abandoned relay buoys in the sector. The player follows the broken chain, sector by sector, reconstructing the pirate ship's final route before the storm swallows the signal again.

Narrative delivery should stay light:

- Short transmission lines after each relay activation
- A stronger final reveal at the wreck
- Minimal exposition outside the run

## Moment-to-Moment Gameplay

The player flies freely in a top-down arena using simple arcade controls rather than full Newtonian physics. The ship is guided by a signal indicator that points toward the current relay or final source, but storm interference makes it unreliable.

The player must:

- Steer through safe openings
- Avoid damage from storm hazards
- Use scan pulses to reveal clearer information
- Commit to routes even when the signal is partially scrambled
- Hold position at relays long enough to activate them

This keeps the game focused on interpretation, movement, and risk management rather than combat or inventory systems.

## MVP Mechanics

- Top-down ship movement
- Hull or health meter
- Run timer
- Signal direction and strength indicator
- Scan pulse with cooldown
- Relay activation objective
- Final source objective
- Procedural sector generation
- Hazard collisions and damage
- Win screen and score summary

## Procedural Generation

The game should use lightweight procedural generation based on sequential sectors instead of a giant open map.

Each run generates a linear chain of six sectors. Each sector defines:

- Arena size within a small range
- Entry point
- Relay position or final source position
- Hazard density
- One or two signal distortion modifiers

Generation rules:

- Always guarantee at least one viable path from spawn to objective.
- Scatter hazards around the route and partially across it, but never fully block completion.
- Increase density and distortion from sector to sector.
- Reuse a small set of hazard and layout patterns with shuffled parameters.

This approach gives replayability while staying realistic for a jam scope.

## Hazards

Keep hazards limited to three readable types.

### 1. Static Clouds

Storm zones that deal damage while the player remains inside them.

- Easy to implement
- Easy to read visually
- Good for shaping safe lanes

### 2. Debris Fields

Physical obstacles such as wreckage, rocks, or satellite fragments.

- Can be static in MVP
- Force steering choices
- Pair well with narrow passages

### 3. Lightning Arcs

Periodic electric bursts across a line or area.

- Introduce timing pressure
- Make relay activation riskier
- Increase drama in later sectors

## Signal Distortion Modifiers

These change how the signal behaves from sector to sector.

### 1. Noise

The signal indicator jitters and becomes less precise.

### 2. Echo

False readings briefly pull the player toward the wrong direction.

### 3. Attenuation

The signal weakens until the player uses a scan pulse.

For the 48-hour version, two distortion types are enough.

## Relays

Relays are the main progression checkpoints.

- Each non-final sector has one relay buoy.
- Reaching the relay begins an activation sequence.
- Activation requires the player to stay near the relay for about 2 seconds.
- Hazards remain active during activation, so approach and positioning matter.
- Successful activation reveals the next path and plays a short transmission fragment.

## Final Sector

The last sector resolves the signal into the pirate wreck's black box.

- No relay, only the final source
- Highest hazard density and interference
- Objective is to reach the wreck and hold position to recover the black box
- Success immediately ends the run and shows the score screen

This gives the game a clear ending and a satisfying final push.

## Controls

Required:

- Move
- Scan pulse

Optional if time allows:

- Boost

The safest choice is to ship without boost unless movement already feels excellent.

## UI

The UI should feel like a lightweight cockpit overlay.

- Timer
- Hull bar
- Scanner cooldown
- Signal direction indicator
- Signal strength meter
- Current objective text

Recommended objective states:

- `Locate Relay`
- `Activate Relay`
- `Recover Black Box`

## Scoring

Scoring should reward clean, fast runs.

Primary scoring factors:

- Less time is better
- Less damage taken is better

Simple implementation options:

### Option A: Rank-Based

- Primary score: total completion time
- Tiebreaker: hull remaining

### Option B: Points-Based

- Start from a fixed score value
- Subtract points for elapsed time
- Subtract more points for damage taken
- Add a small bonus per relay completed

For a jam build, the rank-based version is the safest and clearest.

## Failure States

- Hull reaches zero: run lost
- No hard time limit by default

The timer should create score pressure rather than an instant fail state.

## Audio and Visual Direction

Visual style:

- Black space background
- Cyan, magenta, and violet highlights
- Radar lines, scan pulses, and signal bars
- Storm effects with static and electric distortion

Audio style:

- Synthwave drone
- Radar pings tied to signal feedback
- Static crackle during interference
- Sharper electric hits for lightning hazards

The presentation should sell cyberpunk mood without requiring large art production.

## Content Budget

Target content for the 48-hour build:

- 1 player ship
- 1 relay buoy prefab
- 1 pirate wreck or black box endpoint prefab
- 3 hazard types
- 2 signal distortion behaviors
- 1 procedural sector generator
- 1 win or score screen
- 5-8 short transmission text lines

## Nice-to-Haves

Only attempt these if the MVP is already stable and fun.

- Boost ability
- Local best score tracking
- Rival scavenger ghost marker
- More hazard variants
- Extra transmission flavor text

## Out of Scope

- Combat system
- Inventory or upgrades
- Full hacking minigame
- Open-world galaxy map
- Large dialogue systems
- More than three base hazards

## 48-Hour Implementation Plan

### Phase 1: Core Flight

- Ship movement
- Camera
- Collision
- Hull and timer

### Phase 2: Objective Flow

- Relay objective
- Activation hold behavior
- Final source and win state

### Phase 3: Signal Systems

- Direction indicator
- Strength meter
- Scan pulse and cooldown
- Distortion effects

### Phase 4: Procedural Sectors

- Sector generation pipeline
- Spawn and objective placement
- Guaranteed traversable route
- Difficulty scaling across sectors

### Phase 5: Hazards and Scoring

- Static clouds
- Debris fields
- Lightning arcs
- Time and damage based score summary

### Phase 6: Presentation and Polish

- UI pass
- Transmission fragments
- Audio and VFX pass
- Balance and bug fixing

## Minimum Shippable Fallback

If implementation time gets tight, cut to this version:

- 4 sectors instead of 6
- 2 hazard types instead of 3
- No boost
- No moving obstacles
- Score screen based only on time and remaining hull

This still preserves the full fantasy and run structure.

## Submission-Friendly Description

`Ghost Frequency: Storm Relay` is a top-down neon space-tracing game about following the final transmission of a pirate flagship through a deadly ion storm. Restore lost relays, interpret distorted signals, dodge hazards, and race to the final wreck with the fastest time and least damage.
