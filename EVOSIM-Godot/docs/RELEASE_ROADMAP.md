# Release work — 25 October 2026

## Current assessment
The living-forest v2 graphical host is implemented and is the default launcher. An isolated Linux x86_64 release preview now launches without an editor or SDK. Explicit v1 import is available from the library. Rendering, archives and ecology have reproducible checks; artistic acceptance, sustained large-population 60 FPS, animal population balance and cross-platform validation remain open. Existing worlds remain recoverable with their original model.

## Execution order and acceptance
1. **Living forest host:** share rendering/UI source with the original host; isolate assemblies, cache and world storage. Connect seven forest settings, persistent tree IDs, fruit, growth/health/death views and timeline. Keep the original launcher usable; never silently migrate historical worlds.
2. **Ecology performance:** replace full-forest neighbour scans with an index without changing evaluation order or RNG. Compare exact state hashes before/after, then measure representative large forests. Test determinism, replay, fruit/obstacles and corpse accounting.
3. **Locomotion:** inspect walking, turning, stopping, backwards motion and swimming at normal speed. Fix unreachable limb endpoints and abrupt replanting; preserve biological motion. Capture actual rendered motion and track remaining defects.
4. **Render performance:** update forest instances without rebuilding terrain; cull by region, preserve selection by stable ID. Profile simulation and scene generation separately. Do not promise universal 20x or 60 FPS without measurements.
5. **Release gate:** integrated save/open/seek/branch tests, drought/recovery and long-running population scenarios, visual screenshots, export smoke tests. Keep unresolved items explicit.

## Work policy
No change to animal decision policies. No implicit replacement of the original model. Model changes require explicit version boundaries. A passing compiler is not visual acceptance. Source checkpoints are not distributable desktop builds.

## Completed in this pass
- Isolated generated Godot v2 host using shared source; default launcher routes to it, `--legacy` preserves v1.
- Seven settings/tooltips, rapid-turnover preset, stable tree inspection, seedlings/health/death/fruit rendering; no terrain rebuild for daily growth.
- Exact-order forest neighbour index and shoreline index; whole-state equivalence before/after optimizations.
- Forest save/load/seek/branch, climate mortality/recovery and existing library operations exercised in real host.
- Recovery steps, settled stop, fixed-length IK endpoints, explicit pause, separate paws/hooves, material update deduplication.
- Fruit instancing and water-only terrain cache identity.

## Still required before calling this a finished release
- Sustained large-population 20x / approximately 60 FPS; current performance numbers are short local scenarios, not guarantees.
- First-time region construction still synchronous. A warm terrain cache does not fix cold-region navigation stalls.
- Explicit old-world import UX into v2 (retain provenance/new history); currently old worlds open in preserved v1 host.
- Longer coupled animal/forest balance across seeds and extreme settings. Century forest test alone cannot establish evolutionary population balance.
- Animal silhouettes/materials and locomotion still need art review; current procedural articulation is not a final authored animation library.
- Desktop exports and clean-machine launch/install tests, licenses/provenance bundle, release repository cleanup.

## Iteration 48 — implemented and measured
- Compiled read-only terrain sampler and bulk shoreline mask: three sampled regions produced exactly matching heights and sampled mask bytes. Cold region preparation 407.7 ms (previous local measurement 1398 ms); cache revisit 2.0 ms.
- Frame comparison at identical seed/configuration, 600 prescribed frames, ending at tick 3999 with the same complete biological hash. Baseline median 24.327 ms, final 23.810 ms. This is a modest ~2% FPS gain, not a large-world 60 FPS pass. Renderer stages identify simulation stepping as the largest remaining CPU cost.
- Animation blends walk/trot phases, differentiates predator steps and uses visual time for bite motion. Off-screen rigs skip pose work while biology and positions stay current. Existing twelve locomotion invariants pass.
- Library offers explicit `.evoworld` import through the pinned v1 subprocess. Original file preserved, new forest archive begins at imported head; no invented historical forest. Nine import checks pass, including reopen.
- Three coupled 10-year scenarios: save/load and resumed determinism pass. All three animal populations go extinct at days 1003/925/750; forests remain. This is an observed balance concern, not a successful coexistence test.
- Linux export uses a solution file and self-contained .NET runtime. Export properties cannot bleed into model Release builds. Clean-home launch with DOTNET_ROOT=/nonexistent passes app smoke and six clock checks. The packaged legacy helper passes seven checks. No Windows/macOS runtime validation.
- Actual viewport screenshot: previews/package-smoke48.png. Motion capture: previews/locomotion48.mp4 (60 captured frames encoded at 10 FPS; screenshot overhead means this is not a real-time benchmark).

## Remaining release gates
1. Controlled CPU optimization for large populations; do not hide slowdowns by skipping simulation ticks. Background simulation requires immutable snapshots and a single authoritative owner, not unguarded Task.Run around World.Step.
2. Investigate reproduction/energy/water mortality and evaluate explicit reference presets over multiple seeds. Do not force survival or change inherited policies to mask extinction.
3. Further original animal anatomy/silhouette art polish, more predator and crowded-canopy visual coverage.
4. Test the portable build on another Linux installation; only then claim broader Linux compatibility. Windows/macOS exports need their own runtime tests.


## Iteration 49 — observation, creatures, frame cost (25 October 2026)
Implemented map-region inspection, one active map layer (living density / available food energy / last-30-day deaths), clickable short observation notes, population history with birth/death rates and timeline seeking, founder-versus-living genotype distributions, living descendant map tracking, gene-versus-adult-age offspring scatter, current action counts and recent death causes. Saved-world comparison shows parameters and population/resource summaries, with separate herbivore/carnivore means. Four-slot panel stacking remains. UI retains existing navy/ivory, green/copper, small orange accents; no extra always-visible toolbar.

Observation semantics: founders are retained founder genotypes; food is inventory, not path reachability; action counts are current, not historical strategy measurements; reproductive success counts children reaching maturity AGE, not inferred learned intelligence; both parents can share descendants. World comparisons use saved heads at their actual ages, not aligned experiments. No causal explanations are invented. The simulation model assemblies were not modified.

Original Blender creatures now use continuous torso meshes, narrower deer profiles, attached ears/branched antlers and lower broad-shouldered predators with triangular ears and dark saddles. Existing gait/IK contacts retained. Three MultiMesh limb batches replace twelve limb draw objects per creature. Distant unselected poses update at 30 Hz with accumulated delta; close/selected rigs update each render frame; positions and biology continue independently. Settled legs skip redundant IK. Simulation per-frame budget is 7 ms detail / 4 ms balanced; accumulated ticks are retained, so target speed can fall below requested speed.

Controlled 500-founder, 128-map, seed42, 600 prescribed-frame comparison: 480 measured frames, median 27.899→18.013 ms (~35.8→55.5 FPS), p95 60.478→35.284 ms. Both end at tick3999 and hash635171377bc932b2343428ef2cc1d6ee2a012d3e9d04bed8beeb93618f6dfebb. This short benchmark ends with189 living animals and19 rendered regional agents; it does not prove500 simultaneous visible animals at60FPS. Natural-clock balanced run: median19.444 ms (~51.4FPS), p9533.333ms, target20×, effective14.744× at capture,275 animals alive. Separate-render-thread experiment was rejected due shutdown errors.

Actual GPU captures: previews/observations49.png, comparison49.png, pose-gallery-49-12.png (synthetic poses), live-motion-49-28.png (actual simulation). Observation checks, comparison read-only checks, 19 UI checks, 12 locomotion checks, 6 limb batching checks, 8 pose checks and23 forest lifecycle/archive checks passed. Motion capture records60 actual viewport frames, ticks60→219; screenshot overhead prevents using it as a real-time benchmark.

Remaining release gates: sustained large-world60FPS/20×, synchronous cold region construction, multi-seed ecological population balance (iteration48 extinctions remain), broader Linux-machine/Windows/macOS tests, further art review. This iteration improves a playable preview; it is not proof that every roadmap goal is complete.
