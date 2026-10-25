# Reference → actual EvoSim architecture

The copied protocol mentions Pygame. The inspected working simulation is C# netstandard2.1, with an SDL client and a separate Unity visualization. The independent Godot 4.7.2 .NET slice directly references EvoSim.Core; there is no Python IPC server and no biological port. Both prior clients remain recoverable.

| Layer | Implementation | Allowed visual changes |
|---|---|---|
| Simulation | ../EVOSIM/src/EvoSim.Core | None for this visual task |
| Read-only bridge | scripts/SimulationBridge.cs | Snapshot fields and derived UI statistics; no new rewards or behavior |
| World representation | scripts/world_surface.gd | Bounded terrain interpolation preserving map cell classification |
| Generation | terrain_view.gd, art/build_assets.py, procedural_meshes.gd | Original geometry, deterministic seeds, glTF export |
| Rendering | world.gd, shaders/*.gdshader | Orthographic camera, MultiMesh instances, surface shading, local canopy reveal |
| UI | observer_ui.gd, population_dial.gd | Reference-derived layout adapted to actual population/energy/water data |

Blender Python is an offline asset authoring tool, not the simulation. New renderer/UI cannot yet replace the mature SDL world library, saves, configuration or panel-stack system. Those stay available in the existing client and are deferred until the visual slice is accepted. Reference bottom construction tools are not implemented because EvoSim does not have those semantics.

Behavior equivalence: replay from seed to current tick and compare StateHash; test actual adapter at 30/60/144 render calls per second and 1×/20×. Terrain checks compare all 2304 slice cell centers. Visual capture uses the real Forward+ application, not a mockup. Benchmark results apply only to detected hardware and workload.

## State integration revision

`SimulationBridge` now exports actual actions, distance, maturity, health/damage, water occupancy and corpses. `creature_view.gd` assembles eight original Blender component assets into two articulated cosmetic rigs. No controller output is written by animation. `weather_view.gd` updates global shader uniforms and a rain MultiMesh; rain mesh/material share a batch rather than a node per drop. Implementation API references: [Godot global shader uniforms](https://docs.godotengine.org/en/stable/tutorials/shaders/shader_reference/shading_language.html), [MultiMesh](https://docs.godotengine.org/en/stable/classes/class_multimesh.html).

A240day demonstrator climate cycle issues recorded SetDrought commands at80/160days. These are environment inputs, not Core source changes. `Validate` replays the same input log; its comparison is therefore behavioral equivalence **under identical climate inputs**, not equivalence to a perpetual-temperate world. `--scenario` fixtures include dry warmup, recovering rain, real extinction and full climate-cycle replay. Invalid scenarios now return failure and terminate initialization instead of continuing with the old world after a C# exception.
