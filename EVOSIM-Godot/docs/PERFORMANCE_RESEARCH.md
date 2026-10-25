# FPS research and iteration 48 evidence

25 October 2026. Hardware: NVIDIA RTX 5090 Laptop GPU, local Linux, Godot 4.7.2 .NET Forward+.

## What was measured

The view is a 48×48 region, not a rendering of the whole map. Results are local short tests, not universal frame-rate guarantees. Do not equate a 256-cell map test with 256×256 terrain visible at once.

| Controlled renderer run | Baseline | Final |
|---|---:|---:|
| Map / founders / seed | 128 / 500 / 42 | same |
| Prescribed simulation delta | 1/60 s × 600 frames | same |
| Requested simulation speed | 20× | 20× |
| End tick / region agents | 3999 / 19 | same |
| Measured frame samples after warmup | 480 | 480 |
| Median frame duration | 24.327 ms | 23.810 ms |
| Equivalent median FPS | 41.1 | 42.0 |
| p95 frame duration | 47.034 ms | 45.962 ms |
| Full biological state hash | 635171377bc932b2343428ef2cc1d6ee2a012d3e9d04bed8beeb93618f6dfebb | identical |

This benchmark deliberately supplies identical deltas to compare identical simulation work; the requested 20× is not a claim of 20× wall-clock throughput in this harness. Final median stage costs: advance 7.875 ms, visible agent updates 2.915 ms, remaining world process 0.296 ms. Stage medians do not add to total median because intermittent work and correlations differ. A separate natural-clock large-world sample reached ~20×, but had a different surviving population and must not be used as a controlled FPS comparison.

Terrain cold preparation fell from a previous 1398 ms local measurement to 407.663 ms (~71% shorter); cached revisit 2.027 ms. Three regions, 76,800 height samples and 22,188 sampled shore pixels match exactly. These are generation/camera transition gains, not steady-frame gains.

## Applied solutions

1. Move field/height arithmetic to a read-only C# sampler; keep the same noise, water classification and 0.25-unit geometry resolution. One bulk shoreline image replaces per-pixel script calls. Geometry and biological hashes verified.
2. Preserve the three-region resource cache; tree growth does not invalidate terrain topology.
3. Skip off-screen skeletal pose calculations with a screen margin, resetting foot anchors on re-entry. Keep every animal's biological ticks and position data current.
4. Redraw the population dial only when its counts change.
5. Keep exact-order spatial indexes already implemented for forest neighbors and nearest shoreline.
6. Export self-contained Release application; isolate compiler export flags from the simulation model identity.

## Official sources and interpretation

- [Godot CPU optimization](https://github.com/godotengine/godot-docs/blob/master/tutorials/performance/cpu_optimization.rst): profile hot paths and avoid unnecessary scene-tree overhead. Applied through staged measurements and invisible-rig suppression. A large GPU cannot automatically accelerate C# decision loops.
- [Godot MultiMesh guidance](https://github.com/godotengine/godot-docs/blob/master/tutorials/performance/using_multimesh.rst): instancing reduces draw overhead, but instances do not individually cull. Existing vegetation is spatially grouped; future batches should preserve those regions.
- [Thread-safe APIs](https://docs.godotengine.org/en/stable/tutorials/performance/thread_safe_apis.html): scene and GPU-resource operations have thread restrictions. Legacy extraction uses a separate process with main-thread commit. Simulation itself remains on its authoritative thread until immutable render snapshots and ordered commands are designed.
- [Procedural geometry](https://github.com/godotengine/godot-docs/blob/master/tutorials/3d/procedural_geometry/index.rst): generation is CPU work. Faster sampling addresses the measured terrain preparation cost; lowering visual fidelity was unnecessary for this change.
- [Godot export workflow](https://docs.godotengine.org/en/latest/tutorials/export/exporting_projects.html): a resource pack alone is not an executable distribution. The delivered preview includes the matching template executable, resource pack, self-contained runtime and import helper. Documentation marked latest; actual export was verified against installed 4.7.2.

## Next optimization experiments, in order

1. Profile CoreV2 Step in isolation with allocation/CPU traces at 500, 1000 and 2000 founders. Preserve RNG order and compare full hashes for every proposed algorithmic optimization.
2. Measure periodic timeline snapshots separately; buffer/compress completed immutable snapshots outside the render critical path if they dominate p95. No concurrent access to mutable world collections.
3. Move entire simulation ownership to one worker only with versioned immutable render DTOs, ordered interventions, save/seek barriers and pause tests. A lock around arbitrary Step calls is insufficient.
4. Batch common limb materials/meshes and add tested distance-based animation rates if crowded visible herds dominate. Avoid foot sliding introduced by crude per-frame skipping.
5. Only if GPU timing dominates: compare shadows, SSAO and grass density through the existing quality mode. Do not blindly reduce terrain resolution or use compute shaders for deterministic biology.

## Limits

Final controlled test does not meet 60 FPS at 500 founders and prescribed 20× work. The independent clean-home default-world launch is a functional export check, not a comparable performance benchmark: first-run shader/cache behavior and visible populations differ. Windows/macOS and another physical Linux machine are not tested.


## Iteration 49 — observation, creatures, frame cost (25 October 2026)
Implemented map-region inspection, one active map layer (living density / available food energy / last-30-day deaths), clickable short observation notes, population history with birth/death rates and timeline seeking, founder-versus-living genotype distributions, living descendant map tracking, gene-versus-adult-age offspring scatter, current action counts and recent death causes. Saved-world comparison shows parameters and population/resource summaries, with separate herbivore/carnivore means. Four-slot panel stacking remains. UI retains existing navy/ivory, green/copper, small orange accents; no extra always-visible toolbar.

Observation semantics: founders are retained founder genotypes; food is inventory, not path reachability; action counts are current, not historical strategy measurements; reproductive success counts children reaching maturity AGE, not inferred learned intelligence; both parents can share descendants. World comparisons use saved heads at their actual ages, not aligned experiments. No causal explanations are invented. The simulation model assemblies were not modified.

Original Blender creatures now use continuous torso meshes, narrower deer profiles, attached ears/branched antlers and lower broad-shouldered predators with triangular ears and dark saddles. Existing gait/IK contacts retained. Three MultiMesh limb batches replace twelve limb draw objects per creature. Distant unselected poses update at 30 Hz with accumulated delta; close/selected rigs update each render frame; positions and biology continue independently. Settled legs skip redundant IK. Simulation per-frame budget is 7 ms detail / 4 ms balanced; accumulated ticks are retained, so target speed can fall below requested speed.

Controlled 500-founder, 128-map, seed42, 600 prescribed-frame comparison: 480 measured frames, median 27.899→18.013 ms (~35.8→55.5 FPS), p95 60.478→35.284 ms. Both end at tick3999 and hash635171377bc932b2343428ef2cc1d6ee2a012d3e9d04bed8beeb93618f6dfebb. This short benchmark ends with189 living animals and19 rendered regional agents; it does not prove500 simultaneous visible animals at60FPS. Natural-clock balanced run: median19.444 ms (~51.4FPS), p9533.333ms, target20×, effective14.744× at capture,275 animals alive. Separate-render-thread experiment was rejected due shutdown errors.

Actual GPU captures: previews/observations49.png, comparison49.png, pose-gallery-49-12.png (synthetic poses), live-motion-49-28.png (actual simulation). Observation checks, comparison read-only checks, 19 UI checks, 12 locomotion checks, 6 limb batching checks, 8 pose checks and23 forest lifecycle/archive checks passed. Motion capture records60 actual viewport frames, ticks60→219; screenshot overhead prevents using it as a real-time benchmark.

Remaining release gates: sustained large-world60FPS/20×, synchronous cold region construction, multi-seed ecological population balance (iteration48 extinctions remain), broader Linux-machine/Windows/macOS tests, further art review. This iteration improves a playable preview; it is not proof that every roadmap goal is complete.


### Final portable verification49
Self-contained Linux export passed clean-HOME graphical launch with DOTNET_ROOT=/nonexistent and PATH=/usr/bin:/bin. At1920×1080,78founders, target20×: median14.583ms (~68.6FPS), p9521.426ms, effective20.009× at capture, 268frames. Six fixed-clock cases (30/60/144 renderFPS ×1/20speed) and all smoke control checks passed. This is a short local RTX5090Laptop measurement, not a hardware guarantee. See validation/verification49.json and package-smoke49.json. Preview distribution: ../EVOSIM-Linux-preview49-20261025.zip. Existing user worlds were not modified or migrated by testing.
