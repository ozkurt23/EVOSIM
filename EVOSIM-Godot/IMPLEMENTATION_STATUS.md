# EVOSIM — active completion work

Updated24October2026, afternoon Europe/Istanbul. **User away for several hours, explicitly authorized implementing the remaining work. Task is not finished.**

## Implemented in the current work session

- [x] First articulated locomotion revision: two-segment knees, world-space stance anchors, swing arcs, backwards motion, turn replant, ground-height contacts, faster diagonal gait, paddling, exact paused joints. Existing interaction poses remain. Real Core motion captured; further aesthetic refinement remains.
- [x] Godot main menu/world library: atomic existing WorldStore format, save/open/rename/recoverable delete, restore, two-minute and exit/menu autosave. Deleted active worlds cannot silently resurrect; menu resume is disabled until restore/new world.
- [x] Setup: all annotated Core settings plus24gene bounds, descriptions, normalized sliders, numeric values/years,4presets; lifespan0, reproductive drive, trees, resources, map size/regions. Optional seasonal dry/rain cycle saved as metadata.
- [x] Population graph, species-labelled events, creature parent/child/grandchild information, four independent screen positions and header stacks. Fifth/ninth selection goes behind first position.25panels tested within viewport. Older graph remains open.
- [x] Real timeline seek, exact historical replay, return to head and independent branches, using original TimelineSession/Archive.
- [x] Original SDL world import and JSON/CSV observation export. Four original worlds copied into **test storage**, original bytes unchanged. User-facing old-world import is available in menu; no actual user's original file overwritten.
- [x] Regional navigation across128/192/256worlds, minimap and double-click creature follow; manual panning stops follow. Same48×48rendering window is reconstructed as needed.
- [x] Performance pass: use unchanged optimized original ReleaseCore; region-filter creature/fruit snapshots before Godot dictionary transfer; bounded adaptive tick budget. Small78-founder test reached20×,500-founder test~11.8×. Speed tooltip shows achieved speed/debt.
- [x] Original SDL observer and Core regressions, Godot persistence/UI/locomotion/import/export tests, actual climate/extinction captures.

## Current acceptance and important limits

**Art target still NOT accepted.** Actual R14 reference re-opened and compared: our oversized/fine-noise tree crowns, generic understory and animals still need refinement; shoreline improved but overall composition is not yet comparable. HUD style uses measured references. New menu/setup layout is EvoSim-specific, not a reverse-engineered Terra Nil main menu. Collect genuine main-menu/settings references before claiming that match.

**Large-map region changes are synchronous and can pause rendering.** Terrain heights and biological tree identity are world-coordinate stable; ornamental grass/flowers/reeds now use stable world-cell seeding with exact overlap tests. Terrain mesh construction is approximately2.8× faster on three tested regions with identical geometry; full rebuild still blocks. At the boundary a whole region rebuilds. This is navigation, not seamless streaming.

**500-founder20× and universal60FPS are unmet.** No fictitious speedup/benchmark claim. Core logic/hash unchanged. A worker-owned simulation plus immutable snapshot publication may help, but preserve timeline/save ownership and determinism; do not parallelize World.Step casually.

**Fire/flood/disease/construction/material carrying do not exist yet as complete mechanics.** Keep them separate/versioned/opt-in when implementing, with explicit checkpoint schema and migration tests. Never add hardcoded flee/chase/food-seeking or increase mutation to fake emergence. Existing Core has modest shelter factor, not a construction system. Full feature inventory remains in ../EVOSIM/docs/FEATURE-BACKLOG.md and ../plan.md.

Godot now includes gene histograms, whole-world resource inventory and recorded creature life traces. Remaining SDL research views include the complete ancestry graph and individual resource inspector/map layers. Current family counts are factual; no lifetime-learning claim.

Packaging: local Linux source launcher works; EVOSIM.desktop provided inside project. Standalone Windows/macOS/Linux export packages are not built. No export templates installed. No publish/push performed.

## Evidence

`validation/verification-20261024.json`: consolidated raw reports.

- Core23cases, unchanged release hash50e63b1ef6fe985e8849d5ac55b27da9db0428f51bd3ca008f79785277459033.
- `validation/library-checks-19.json` now23persistence/settings/replay/export cases. Stable filename overwritten by final run24. Negative invalid-create/deleted-save warnings intentional.
- `validation/ui-checks-19.json` now18cases including25panels, real region change, small-map reset and biological immutability; final run33. Stable screenshot names `library-19`, `world-setup-19`, `observation-stacks-19`, `timeline-19`, `distant-region-19` were overwritten by later corrections.
- `validation/locomotion-checks-20.json`:8fixture checks,431stance samples, sampled foot slip~6e-8, slope contact. Bounded evidence, not universal gait perfection.
- `validation/import-checks-23.json`:4original imports passed; `work/original-worlds-after.txt` source hash confirmations.
- `previews/final-controls-23.png.json`:1920×1080 normal, median13.895ms/p9520.375;6fixed-clock cases and10control checks pass.
- `previews/speed20-22.png.json`:1920×1080,78founders,3999ticks in~10sec,20.003×last window;median18.739/p9528.733ms.
- `previews/large-world-22.png.json`: overwritten by optimized run23,500founders/256map,2271ticks in~10sec,11.804×last window;median19.444/p9521.329ms. Prior run22was1524ticks/~6.96×.
- `previews/{drought,rain,empty}-24.png(.json)`: real states and replay/corpse/classification pass.
- `previews/live-motion-24.gif`:40actual frames of Core creature20,tick60→165,~5.25sec, actual follow-camera motion, not synthetic. Companion metadata `validation/live-motion-24.json`; frames overwritten by corrected run25with region-follow.
- Old SDL smoke: `work/observer-regression24b.log` passed after setting bundled SDL library path. First run24failed missing SDL_ttf; not a pass. Build0warnings/0errors.

## Architecture and changed shared files

Godot C# `SimulationBridge.cs` plus new partial `SimulationLibrary.cs` owns original Core and TimelineSession. `EvoSimGodot.csproj` links original WorldStore, TimelineSession, TimelineArchive, WorldCalendar, SettingHelp, ObservationExporter, PopulationHealth source files. References original Core with Configuration=Release. Embedded release DLL resource supplies actual byte SHA256 because Godot's assembly.Location is empty; no duplicate simulation.

Only original SDL helper changes: WorldStore accepts an optional externally provided Core fingerprint and AutoClimate metadata; ObservationExporter accepts optional fingerprint. Existing caller defaults preserved and original SDL suite passes. **Core source untouched.**

New GDScript: library_ui, observation_ui, population_graph, world_minimap. Mainworld.gd integrates menus and regional rebuilding. `creature_view.gd` cosmetic IK. `world_surface/terrain_view` coordinate-stable height and shader offset. Water broken-bank highlight. `previews/.gdignore` prevents captured footage import into runtime.

Toolchain: `work/toolchain/dotnet/dotnet` (.NET10.0.401), Godot4.7.2mono in `work/toolchain/godot`, Blender4.5.9in `work/toolchain`.

## Continue next

1. Read recent task steering; do not repeat already passing tests without changed behavior. No subagents authorized.
2. Refine locomotion/body silhouettes and tree/ground cohesion against genuine references. Collect Terra Nil menu/settings references; use visual-reverse-engineering skill. Actual captures, three largest differences, update VISUAL_QA. Do not call compiling art completion.
3. Make regional generation stable/smoother; examine rendering-time block on region change. Preserve direct Core state equivalence.
4. Complete ancestry navigation and individual resource inspection in the existing four-stack UI. Genes, aggregate resources and life traces are implemented; no permanent button proliferation.
5. Implement new ecology mechanics only as carefully versioned optional extensions with real consequences and replay/save tests; prioritize coherent limited behavior over many decorative fake options.
6. Desktop packaging and longer benchmarks. Finish with truthful implemented vs remaining list.

User requested ongoing work while away. Existing heartbeat `evosim-geli-tirme-sabaha-kadar` was updated to continue this task every20minutes until24October18:00Istanbul, then stop safely and delete itself; honor user return/steering. Notify only meaningful milestones or issues; occasional actual screenshots. Do not duplicate a currently running process. Current tests completed; check processes before launching more.

### Safe checkpoint at end of first work block

Source/art-runtime snapshot: `outputs/EVOSIM-source-checkpoint-20261024.zip` (~9.6MB; source snapshot, not a standalone executable distribution). Contains Godot runtime assets and original Core/Observer sources, excludes engine caches, paid/reference imagery and bulky captured frames. Local live application launched successfully at the main menu: PID136909, command Godot `--path .../outputs/EVOSIM-Godot --resolution1600x900`, log `work/live-evosim-20261024.log`, EVOSIM_READY, no startup errors. Inspect process state before benchmarking; do not run concurrent GUI benchmark windows. No user-requested biological behavior changed.

## Heartbeat28 — regional stability fixes (24October,14:23–14:35Istanbul)

Implemented `decoration_sampler.gd`: deterministic world-cell candidate streams for ornamental grass/flowers/reeds. Density fields now sample absolute coordinates; fern rotations consume a complete per-tree stream before view clipping, edible grass tufts seed per cell, rock type/scale/rotation seed from global cells. Foliage wind/variation and rock surface grain use a shared region-offset shader parameter, so rebuilding a neighboring window does not restart their spatial phase. This keeps decorative placement independent of window position; no biological RNG consumed.

Fixed a newly discovered crash path: navigating a256map near(230,230), then creating/opening128map retained a region origin beyond128. `_rebuild_world(reset_view=true)` now resets the view origin to(42,20) before reading map arrays.

Tests: sampler6checks compare exact overlap candidates and repeatability; actual Godot UI suite now18checks, including256→128transition, valid reset origin, and exact biological state hash before/after renderer reconstruction. `work/ui28b.log` has no script/shader errors and all checks pass. Actual `previews/distant-region-28.png` viewed; natural patch distribution retained. New UI screenshot runner suffix28 prevents overwriting older19captures in subsequent captures of this revision. Art gate remains open; synchronous region generation still causes a pause. No GUI FPS benchmark made while the user's existing paused application is still open(PID136909); that process was not interrupted.

New issue to address next: edible grass instances currently build only for cells with currentFood>.01 and normalize against current stock. Returning to a depleted/recovered region can therefore change apparent fullness, and fully depleted cells at build may never regain a visible instance. Use a stable reference/potential mask and actual current stock, without modifying Core biology or faking resources. Terrain's true `_capacity` is private; prefer a clean read-only approach preserving checkpoint/model identity. Do not accidentally replace the core assembly just to expose a getter without planning archival fingerprint compatibility.

## Iterations29–33 — food continuity, research panels, terrain construction (24October afternoon)

Completed stable edible-grass reference stock in `GroundFoodReference.cs` and `world.gd`. Reference is the deterministic *initial stock*, not private carrying capacity. Potential cells exist even when currently depleted; rebuilding does not redefine fullness. Separate terrain RNG does not mutate Core. Initial headless visual test29 was invalid because the dummy renderer did not retain MultiMesh custom data; actual Forward+ test30 passes11 checks, including every patch's growth vs actual stock. Test now explicitly refuses headless execution. Two close-up fixture screenshots exist, but tree occlusion makes them weak visual evidence; no art-quality claim from them.

New compact observer panels, inside the existing observation menu and four-stack layout:
- Six gene distributions using the original SDL `PopulationInsights` implementation, ten shared-count bins, separate diet colors, median and empty-population dash. One gene selector rather than six oversized charts.
- Read-only whole-world resources: tree count, available fruit/slots, fruit and ground energy, actual water-access coefficient, current corpse count. Snapshot caches invalidate on world replacement/time rewind and never mutate Core.
- Creature card opens a life-trace panel: energy percentage, hydration and health over recorded time. Reuses original `LifeTraceJob`/`BackgroundLifeTrace` on a private replay worker. At most4trace jobs; closing clears results/cancels work; world replacement invalidates jobs. Historical queries end at the displayed tick, not the future head. Snapshot is labeled as recorded history, not a continuously updated live chart. Maximum1200days comes from the original helper.

`research-checks-31.json`:17 checks passed at1280×720, including diet boundary, dead exclusion, exact upper gene bin, empty population, source/GUI state immutability, resource inventory vs actual fruit objects, world-change cache invalidation and four panels onscreen. First attempts failed due invalid zero-founder test setup and PackedInt32Array test helper misuse; corrected test creates one creature and uses actual extinction scenario. No biological validation loosened.
`trace-checks-32.json`:12checks passed, actual creature-card button opens trace, replay endpoint matches independently observed vitals, no main-world time/state movement, historical no-future leak, cancellation and world switch. Build0warnings/0errors.

Terrain construction caches repeated land-vertex normals/positions and shared edge fields only for the build, then frees caches. Three original/cached region meshes have byte-identical vertex+normal hashes including banks/skirt; biological hash unchanged. CPU build times under current host load:4159→1490ms,4024→1400ms,3811→1343ms (~2.8×). This measures terrain generation only, not FPS or total region load. Synchronous loading still remains; do not call this seamless streaming.

User returned, received real30second4K showcase and requested continuing all remaining work. Video artifacts are in `../EVOSIM-Showcase`;1080p was explicitly uploaded to user's Drive, ID1qae-b_Pshn5bRP3_uFfnLnBlWkjlGYpa. Video is offline real-app capture, not a performance benchmark. Art gate is still open. Existing live desktop process136909 predates these changes; do not assume it hot-reloaded C#.

## Iterations33–34 — minimap and canopy visibility

Minimap markers now use a packed array containing only live position/diet, refreshed5Hz while expanded instead of transferring full creature dictionaries every draw. Research validation checks marker population and diet counts. This is a transfer/cadence improvement, not a new measured FPS claim.

Canopy reveal selects only on-screen creatures, prioritizes the selected animal then proximity to screen center, and keeps the existing16-hole shader budget. Previously offscreen early IDs could consume that budget. `canopy-checks-34.json` passes5checks including on-screen bounds, selected priority and untouched Core hash. Actual `previews/canopy-34.png` inspected; dither visibility and the16-target limit remain visual limitations.

## Iterations35–37 — foliage and family navigation

Original Blender vegetation revised after a rejected intermediate iteration; current foliage retains leaf silhouettes with lower local contrast and small internal masses. Actual before/after comparisons recorded in VISUAL_QA. Six vegetation GLBs installed; original rocks/creature models retained. Full art acceptance remains open.

Creature cards now open a scrollable family panel with direct-parent and direct-child links to independent creature cards. Uses only displayed-world records, deduplicates parents, includes deceased offspring and shows no fake#0parent. This is contextual family navigation, not a complete multi-generation graph. `family-checks-37.json` verifies8data cases and5UI/state cases at720p.

Latest build:0warnings/0errors. UI regression37 passes18cases after adding the family controls; rain/drought36 actual captures inspected and scenario checks pass. Unchanged Core DLL SHA256 rechecked. Source checkpoint `../EVOSIM-source-checkpoint-20261024-observers.zip` includes these additions and revised assets, excludes personal worlds and runtime caches; it is not a standalone game distribution. Existing desktop process136909 still predates this block; launch through baslat.sh to load current code.

## Heartbeat38 — bounded region revisit cache

Terrain rendering now retains at most3recent region resource sets (land/bank/skirt/water meshes, materials and shoreline texture). Nodes remain independent. A SHA256 of actual tile topology+size invalidates all cached regions when terrain changes; no simulation snapshots, creatures, fruit or food are cached. Least-recently-used eviction bounds retained resources. Minimap texture is retained during navigation but rebuilt on world replacement.

`region-cache-38.json`:10checks pass, exact mesh arrays/transforms/shore texture, separate nodes, capacity3, eviction, topology invalidation, read-only biology. Cold terrain1900ms vs revisited0.156ms in this local short fixture. `region-revisit-38.json`:5integration checks pass, actual distant navigation1968ms versus returning to a cached region263ms. These are different region paths, not an apples-to-apples whole-engine benchmark. New regions still block; foliage/food/agents still rebuild, so this is not seamless streaming.

Initial integration test failed because it requested origin42via a four-cell snapped navigation grid, which correctly chose44. Corrected fixture starts at aligned(40,20), moves away, returns; no runtime navigation semantics altered to satisfy test. Initial failed log retained in work/revisit38.log. Actual `previews/region-revisit-38.png` inspected: river/foliage/HUD retained, observation panel remains. Current source checkpoint observers.zip predates this cache; files are saved locally and a small revision38delta is archived separately.

Full UI regression38 also passes18checks including large→small world replacement with cached terrain enabled. No active validation process remains.

## Heartbeat39 — individual tree inspector

Click a rendered tree trunk to inspect that actual tree's available fruit/slots, total remaining energy, configured full-fruit energy, radius and world water access. Actual source-list ID is added to read-only map metadata. No new permanent button. Creature picking takes priority; empty clicks create no panel; the selected tree has a ground ring. Existing population/other panels stay open. Tree panel queries displayed world, so no stale cached resource stock. World replacement clears panels/selection.

`tree-checks-39.json`:17checks pass in actual Forward+ at1280×720, including data immutability, invalid IDs, depleted slot, real EcologyStep regrowth, click path, ring, creature priority, panel bounds, treeless-world replacement. Build0warnings/0errors; Core release hash unchanged. Actual `tree-inspector-39.png` opened and inspected. No estimated regrowth deadline shown because water changes affect it. Ground-cell/corpse inspection and map layers remain unported. Revision39delta archive supplements the observers checkpoint and delta38; no active test process remains.

## New user priority — living forest ecology

User explicitly requests finite tree lives, drought death and balanced tree regeneration around creature deaths. Accepted design contract: docs/FOREST_LIFECYCLE.md. This authorizes a versioned biological extension; earlier blanket keep-Core-unchanged applies to preserving v1behavior, not forbidding this requested v2feature. Existing Core/archive remain untouched today. First prerequisite is v1archive/model preservation and a separately versioned v2branch; do not silently change fingerprints or replay old commands under new rules. No graphical-only fake ecology.7essential controls plus enabled option; corpse decomposition boosts capped temporary soil fertility, never guaranteed one-tree-per-corpse. Tree age/health, seeds, competition, real fruit consequences and saved RNG all required. Status: designed after actual code audit, not implemented or balanced. Prioritize this request over optional cosmetic polishing; no agents authorized.

## Heartbeat40 — implemented v1 model isolation prerequisite

`../EVOSIM-Models/v1` pins original CoreDLLand matching core/archive sources with per-file SHA256 manifest. Separate-process `LegacyBridge` loads/validates a v1world and can export a verified historical v1origin plus provenance into a new directory; it never upgrades/replays the history as v2. Existing destination rejection, atomic staging, exact historic checkpoint, source/head immutability and bounds pass7selftests. All4original .evoworld files inspected successfully with hashes unchanged (`legacy-verification40.json`). No actual user migration/extraction performed. Active Godot Core remains unchanged.

This completes the first technical boundary for the living-forest request, not its integration. Next: renderer-independent deterministic forest ecology module/state, finite nutrient deposits and seed/age/health transitions, then explicit v2import/new-history writer, dynamic resource/collision integration and Godot views. Do not weaken fingerprint guards. Existing bridge CLI is developer tooling; GUI routing and a complete v2model are pending. No new screenshots because this pass changes backend tooling only. Preserved model/tool sources archived in EVOSIM-model-boundary40.zip.

## Heartbeat41 — deterministic forest kernel implemented, not yet integrated

`../EVOSIM-Models/ForestEcology` now implements the seven-control forest model independently from legacy biology/rendering: stable IDs, saved separate RNG, finite seeds, age/health/growth, drought death/recovery, seedling fruit restriction, dead-trunk expiry, capped decaying mineral bonus, spatial spacing/competition and occupancy veto. Save/load includes all module state. No changes to active Godot Core or user worlds.

`forest-verification41.json`:29checks passed including repeated mid-run save/reload forward identity, old age, age-immortal drought death, no resurrection, mineral saturation/half-life, no seedless spontaneous tree, water rejection, occupied cells, invalid-state checks and three100year8x8fixtures. With fast-observation settings and45dry days/year, seeds42/73/123 peaked at15/15/16trees and ended11/10/13. This is a tiny standalone fixture, not full-system equilibrium/performance evidence.

Outstanding: actual corpse-decay accounting, real fruit and dynamic obstacle integration, composite v2world checkpoint/history and explicit legacy import, Godot setup/rendering, full-map spatial indexing and coupled long runs. DepositDecay must only receive true uneaten decay; module does not own this yet. No screenshot claim because no visual integration occurred. Source/tests archived as EVOSIM-forest-kernel41.zip.

## Heartbeat42 — experimental composite v2 checkpoint + real corpse hook

Created separately versioned `../EVOSIM-Models/CoreV2` from pinned v1sources, with assembly name EvoSim.Core.V2 and reference to ForestEcology. Original Godot project/reference/CoreDLLunchanged. Forest is stepped once per20world ticks. Actual corpse loop deposits only decay loss and once-only remaining mass at expiration; consumed meat is excluded and water rejects deposit.

EVOSIM-FOREST-WORLD-1checkpoint extends the protected biological payload with forest epoch + complete ecology snapshot. StateHash includes both. Save/load reject clock mismatches. Implicit v1load rejected; explicit ImportV1Origin establishes new ecology at old selected tick, without replaying old commands as v2. Custom forest settings accepted and roundtrip.

`corev2-verification42.json`:16checks pass, including repeated composite replay/reload/weather equivalence, original state preservation, v1/v2format isolation, corruption/truncation and actual corpse accounting. Initial test failed because its CorpseDecay2/lifetime.05were outside existing bounds; corrected fixture uses1and5, no production validator weakened. No active tests remain. Source archived in EVOSIM-corev2-stage42.zip.

Not user-ready: forest tree lifecycle does not yet synchronize actual Map.Trees/fruit/obstacles; permanent rocks vs trunks and stable tree-resource mapping, path invalidation, v2timeline, graphical import/setup/rendering remain. Do not activate experimental v2in Godot until these and coupled tests pass. Continue with real tree resource/obstacle integration next, rather than claiming soil-only coupling completes the request.

## Heartbeat43 — real tree resources and obstacles connected in experimental v2

Forest IDs now synchronize actual Tree resource objects/list order. Seedlings begin with zero fruit; maturation permits actual regrowth, scaled by health/fertility/competition. Death zeros food; thirty-day expired trunks disappear and reopen their ground cell. Permanent rock substrate is reconstructed separately from original generated trunks; new dynamic tiles refresh land-component/shore caches and invalidate live paths. Loaded paths remain intact. Creature footprint veto prevents new trunks under living agents. Tree birth/death events are journaled.

`corev2-verification43.json`:28checks pass (previous16plus dynamic-tree tests). Includes actual food, blocking/opening ground, preserved rocks, grown-tree stable IDs, saved nonempty path, repeated forward state/checkpoint equivalence, new seedling no-food, maturation and event logs. Active v1hash unchanged. This is biological integration in a separate experimental assembly, **not yet available in the Godot app**. No screenshot claimed. Archived EVOSIM-corev2-stage43.zip.

Next critical boundary: v2timeline/archive host must use a composite model fingerprint covering **both CoreV2 and ForestEcologyDLLs**; reusing CoreV2hash alone misses changes in the separate ecology library. Then GUI model routing/import + tree growth/death visuals/setup, spatial indexing and larger coupled balancing tests. Keep old v1launcher/library usable and do not silently rewrite existing worlds.

## Heartbeat44 — v2 archive/seek/branch now verified

`../EVOSIM-Models/V2Archive` exposes ForestSession using separate v2compiled timeline/store helpers. Composite model identity includes actual CoreV2, ForestEcology and V2Archive DLLs, so ecology/replay changes cannot silently reuse an old history. Default v2store hashing also uses this composite; no fingerprint guard weakened.

`v2archive-verification44.json`:12cases pass: real forest death/removal, exact prior forest reconstruction, preserved head, independent branch, save during preview, origin reopen, complete replay equivalence, continued save and foreign-model rejection, plus fingerprint sensitivity to each assembly. All tests used isolated temporary worlds. No actual user migration or active Godot reference change. Combined sources/pinned model/reports saved in EVOSIM-models-checkpoint44.zip.

Remaining to expose living forest in the game: isolated Godot v2host/model routing (preserve v1opening), explicit historical-origin import flow, seven settings/help/presets, tree growth/health/death/chunk updates and stable tree inspection IDs, actual screenshots, coupled large-map balance/performance tests. Forest neighborhood queries remain quadratic and first region builds synchronous; these are unresolved. Current app still uses v1. At18:00Istanbul stop new work per scheduled boundary, save and report honestly; do not claim forest is available just because its backend tests pass.

## 25 October — living forest host, locomotion and performance (45–47)

Supersedes the earlier statement that forest is backend-only. `baslat.sh` defaults to generated `../EVOSIM-Godot-Forest`; `--legacy` keeps the original model and library. Shared rendering source is maintained here; `tools/prepare_forest_host.py` generates the separate runtime. No user worlds migrated. Seven settings and rapid-turnover preset, stable IDs, sapling scaling, health tint, dead bare branches, fruit availability, expiry, resource counts and inspector are connected to v2.

Forest local index preserves original list accumulation/RNG order; 300/1200/3000-tree benchmark hashes exactly match before/after (~10/28/33x isolated kernel speedup). Real 128-map forest century fixture peaked848 and ended733 trees from300; one seed and isolated ecology only. Shoreline lookup preserves last-equal-distance tie;60,000 comparisons across original/changed/restored128/256maps passed. Two2000-tick coupled-world benchmark hashes unchanged;78founders1968→1379ms,500founders5073→2108ms. Timings are local CPU tests, not FPS.

Actual Godot forest host checks: lifecycle, stable identity, resource instance count, save, exact rewind/head/reopen and branching. UI18checks, forest23checks, library23checks per model, climate8checks, fruit instancing6checks. Locomotion12checks include finite fixed-length limbs, stance contact, turn/backwards/swim, stop settling and pause. Captured actual forest lifecycle/climate and live motion frames. No blanket art approval.

Integration uncovered Godot's memory-loaded assemblies (empty Assembly.Location) and mixed Debug/Release transitive resolution. Fixed via explicit optimized model references plus embedded fingerprints verified against actual loaded module MVIDs. Initial failed capture/logs remain in /tmp; subsequent host tests reran successfully. Initialization now fails closed if model setup fails.

Tree growth/health updates only instances; topology changes rebuild visible forest/fruit objects, not terrain. Fruit is now one MultiMesh draw object. Terrain cache ignores land/tree distinction because geometry depends only on water. Cold region generation remains synchronous. Original v1 Core SHA unchanged:50e63b1ef6fe985e8849d5ac55b27da9db0428f51bd3ca008f79785277459033.

Final local performance captures (1920×1080, RTX5090LaptopGPU): default78founders target20x achieved19.97x, median14.829ms/p9523.141ms over the short smoke capture;500founders256map latest achieved17.10x at capture end, median22.418ms/p9534.706ms. Large-world initial result before shoreline optimization was9.23x; workloads diverge in elapsed simulated time, so the exact2000tick CPU benchmarks are the controlled speedup evidence. Large capture is still below target20x/60FPS. GPU~2–3ms; remaining bottleneck is CPU/model/render scheduling. Reports preserve this limitation rather than treating behavioral hash success as a performance pass.


## Iteration 48 — terrain, import, locomotion, export
Compiled immutable terrain sampler, bulk shore image, unchanged resolution and sample equivalence; cached revisit remains bounded. Explicit asynchronous v1 import now in library, with pinned subprocess and source preservation. Walk/trot blending, predator step lift, independent visual bite time and off-screen pose suppression; twelve gait checks pass. Linux self-contained export and clean-home graphical smoke pass; packaged legacy extractor seven checks pass. Exported runtime does not accept the editor `--script` test runner, so the full import UI was tested in the editor host and the packaged helper separately, not falsely reported as an exported UI import test.

Controlled same-state FPS: 41.1→42.0 at 500 founders, 20× prescribed work; no broad 60 FPS claim. Cold region 1398→407.7 ms across local runs. Three ten-year coupled scenarios passed checkpoints/replay but all animal populations went extinct: days 1003, 925, 750. This remains a population-balance issue. Actual screenshots and sixty motion frames captured; package screenshot and frames 20/35 visually inspected. Animal anatomy remains simpler than the desired reference quality.


## Iteration 49 — observation, creatures, frame cost (25 October 2026)
Implemented map-region inspection, one active map layer (living density / available food energy / last-30-day deaths), clickable short observation notes, population history with birth/death rates and timeline seeking, founder-versus-living genotype distributions, living descendant map tracking, gene-versus-adult-age offspring scatter, current action counts and recent death causes. Saved-world comparison shows parameters and population/resource summaries, with separate herbivore/carnivore means. Four-slot panel stacking remains. UI retains existing navy/ivory, green/copper, small orange accents; no extra always-visible toolbar.

Observation semantics: founders are retained founder genotypes; food is inventory, not path reachability; action counts are current, not historical strategy measurements; reproductive success counts children reaching maturity AGE, not inferred learned intelligence; both parents can share descendants. World comparisons use saved heads at their actual ages, not aligned experiments. No causal explanations are invented. The simulation model assemblies were not modified.

Original Blender creatures now use continuous torso meshes, narrower deer profiles, attached ears/branched antlers and lower broad-shouldered predators with triangular ears and dark saddles. Existing gait/IK contacts retained. Three MultiMesh limb batches replace twelve limb draw objects per creature. Distant unselected poses update at 30 Hz with accumulated delta; close/selected rigs update each render frame; positions and biology continue independently. Settled legs skip redundant IK. Simulation per-frame budget is 7 ms detail / 4 ms balanced; accumulated ticks are retained, so target speed can fall below requested speed.

Controlled 500-founder, 128-map, seed42, 600 prescribed-frame comparison: 480 measured frames, median 27.899→18.013 ms (~35.8→55.5 FPS), p95 60.478→35.284 ms. Both end at tick3999 and hash635171377bc932b2343428ef2cc1d6ee2a012d3e9d04bed8beeb93618f6dfebb. This short benchmark ends with189 living animals and19 rendered regional agents; it does not prove500 simultaneous visible animals at60FPS. Natural-clock balanced run: median19.444 ms (~51.4FPS), p9533.333ms, target20×, effective14.744× at capture,275 animals alive. Separate-render-thread experiment was rejected due shutdown errors.

Actual GPU captures: previews/observations49.png, comparison49.png, pose-gallery-49-12.png (synthetic poses), live-motion-49-28.png (actual simulation). Observation checks, comparison read-only checks, 19 UI checks, 12 locomotion checks, 6 limb batching checks, 8 pose checks and23 forest lifecycle/archive checks passed. Motion capture records60 actual viewport frames, ticks60→219; screenshot overhead prevents using it as a real-time benchmark.

Remaining release gates: sustained large-world60FPS/20×, synchronous cold region construction, multi-seed ecological population balance (iteration48 extinctions remain), broader Linux-machine/Windows/macOS tests, further art review. This iteration improves a playable preview; it is not proof that every roadmap goal is complete.


### Final portable verification49
Self-contained Linux export passed clean-HOME graphical launch with DOTNET_ROOT=/nonexistent and PATH=/usr/bin:/bin. At1920×1080,78founders, target20×: median14.583ms (~68.6FPS), p9521.426ms, effective20.009× at capture, 268frames. Six fixed-clock cases (30/60/144 renderFPS ×1/20speed) and all smoke control checks passed. This is a short local RTX5090Laptop measurement, not a hardware guarantee. See validation/verification49.json and package-smoke49.json. Preview distribution: ../EVOSIM-Linux-preview49-20261025.zip. Existing user worlds were not modified or migrated by testing.
