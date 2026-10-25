# PROJECT AUDIT — 23 October 2026

## Evidence inspected

`../EVOSIM/src/EvoSim.Core/{Simulation,Brain,Genetics,Terrain,WorldConfig,Checkpoint,ResourceIndex}.cs`; `EvoSim.Checks/Program.cs`; Unity `EvoSimBridge`, `CoreTerrainView`, `CoreResourcesView`, `CanopyVisibility`, `SimulationClock`; Blender generator and saved assets; last publication `6d61cb4`.

## Existing architecture

- C# Core, .NET Standard 2.1, no engine dependency. Fixed `World.Dt = 0.05f`; displayed Time = Tick × 0.05. Sequential owner, separated PCG streams. Renderers must not consume these streams.
- SDL2 C# observer, mature world library/timeline/checkpoint panels; unpublished observer candidate also retained. Unity 6000.3 / URP preview, C# adapters, 3D FBX assets. Unity library and build caches are not source.
- Blender 4.5.9 procedural originals: 3 tree types, shrub, reeds, rocks, two creatures. Existing long trees have useful silhouettes but repetitive pointed foliage and limited art polish.
- `EvoSim.Checks` is an executable invariant suite, net10.0. Existing Unity capture asserts independent Core state equality, view count, scale and height.

## Biology, not a new implementation

30 senses → 8 recurrent hidden units → 5 motors; 357 inherited weights. Six genes: body, perception, diet, aquatic adaptation, boldness, social bond. Per-locus inheritance samples the parental interval; rare mutation defaults to 0.001 per birth. No lifetime weight learning. No renderer-authored chase/flee or reward policy is permitted.

Every step moves agents, resolves shared interactions, spends energy/hydration, applies injury/age, resolves death before healing/reproduction, then decays corpses. Age affects phenotype. Corpse lifetime is configurable. Sexual/asexual/disabled reproduction, founder distributions, nutrition and mutation are configurable. Actual ecological sustainability/learned strategy quality is not established by rendering tests.

## Terrain / resources

Seeded grid maps (128, 192 or 256 only); river/branches/ponds, optional land regions, rock/trunk occupied cells, land connectivity and swimming constraints. Moisture influences tree placement; edible grass grows in meadow patches. Trees own fruit slots/regrow timers. No full biome simulation exists: drought, moisture, resource state and substrate may drive appearance, but an art biome must not be presented as new biology.

## Risks and baseline

Core depends on floating-point behavior and same-runtime determinism; do not assert cross-platform bit identity without tests. Large world history and many agents can cost CPU/memory. Previous measured graphics device was Intel ARL, not a functioning NVIDIA 5090. 60 FPS at 1080p is a target, not an achieved baseline. Previous Unity view is recoverable and remains runnable.

## Decision

Use Godot 4 .NET, Forward+, with the unchanged Core source linked as a project reference. A small C# bridge owns fixed ticks and exports batched read-only state to GDScript. GDScript owns camera, procedural terrain, MultiMesh vegetation, HUD and visual interpolation. Shaders own surface response and ambient motion. No Python simulation, no IPC server, no speculative full GDScript port. This minimizes behavioral divergence and avoids serializing a large state every frame.

References: [Godot cross-language scripting](https://docs.godotengine.org/en/stable/tutorials/scripting/cross_language_scripting.html), [MultiMesh](https://docs.godotengine.org/en/stable/tutorials/performance/using_multimesh.html).

## Scope gate

First a bounded rendered valley window into an existing 128-cell world. Visual state follows Core data where practical. No library migration, no evolutionary behavior change, no complete product claim until visual quality and integration are validated. New work lives in EVOSIM-Godot; existing Unity/SDL releases and user saves are untouched.
