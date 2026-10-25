# Visual QA — ongoing, art acceptance NOT passed

Date: 2026-10-23. Actual Godot 4.7.2 .NET / Vulkan Forward+ viewport captures, not Blender renders or composited mockups. User feedback explicitly rejects iterations 1–5 as sufficiently close to the requested Terra Nil quality. Do not label these a completed visual transformation.

## Comparison procedure

Open the actual viewport PNG alongside the user's supplied Terra Nil reference #1 (temperate river) and #9 (close vegetation/animal/shore). Judge silhouette scale, material detail, tonal hierarchy and ground-water transitions. The references belong to their owners and are not game assets. The rubric weights are artistic cohesion 25%, environment 20%, lighting/color 20%, vegetation/detail 15%, camera/composition 10%, animation/atmosphere 10%. Ratings below are subjective internal development ratings out of 5, not image similarity or user acceptance.

## Iterations actually viewed

| Capture | Three largest deficiencies | Subsequent changes | Art gate |
|---|---|---|---|
| iteration-01.png | Land invisible from reversed mesh winding; washed mint lighting; coarse bubble crowns | Reverse terrain winding, remove fog from background, rebalance key/ambient | Fail |
| iteration-02.png | Smooth oversized crown lumps; pale repetitive ground; cliff-like drinking slope and exposed slab seam | Lower shore height, close exterior edge, darker material palette, original branched-river preset | Fail |
| iteration-03.png | Entire-tree fading produces ghost forest; leaf detail still coarse; river surface too uniform | Replace whole-tree transparency with screen-local leaf reveal, add flowers/reeds, instrument render times | Fail |
| iteration-04.png | Close camera shows crude crown cores; overly regular ripple stripes; dense foreground foliage hides composition | Capture mode now excludes external unhandled camera input; smoother bank geometry; independent camera/selection smoke checks | Fail |
| iteration-05.png | Crown model is structurally unlike reference; coarse grass tufts; water still resembles lined plastic | Rebuild tree assets with individual folded leaves and branching twigs; finer grass, spatial tree instancing, irregular water noise | Fail; user explicitly rejected visual target |

Iteration 04 was unexpectedly zoomed during the live run; its JSON predates recording camera state. It is not an equivalent composition benchmark. Iteration 05 locks capture input and records camera size 54, focus [24,0,24].

Scores at iteration 05: cohesion 2/5, environment 2/5, lighting/color 2.5/5, vegetation/detail 1.5/5, camera/composition 2.5/5, animation/atmosphere 2/5. Weighted 41.5/100. This is a rejection record, not an achievement score.

## Measured baseline and limits

Actual device: NVIDIA GeForce RTX 5090 Laptop GPU, Ubuntu 24.04, Vulkan Forward+. The previous Unity Intel-device observation does not describe this session. Capture 05 at 1920×1080: 1102 frame samples after the first two seconds, median frame interval 8.33 ms, p95 13.33 ms; median measured GPU rendering 1.71 ms, rendering CPU 0.35 ms, main script 0.852 ms. These are short small-scene samples, not a broad hardware or large-population benchmark. Frame interval includes pacing and is not interchangeable with GPU time.

CPU/GPU counters use Godot's [RenderingServer instrumentation](https://docs.godotengine.org/en/stable/classes/class_renderingserver.html). Timing JSON is stored next to each PNG. Core hash equality passed through tick 240 against a fresh renderer-free replay. Camera zoom, rotation, pause, resume, quality switch and creature picking smoke checks passed in run 05. The second motion capture retains the same paused simulation tick; image-difference inspection is still required before attributing motion to specific shaders.

## Remaining acceptance work

- Re-evaluate newly generated leaf geometry in the real renderer, both overview and close view.
- Replace any remaining coarse asset silhouettes, reduce visual repetition and tune natural color variation.
- Evaluate water, dark soil seam, shallow drinking access and readable creature shapes together.
- Record latest render budget after foliage changes, then validate the 20× clock separately.
- Do not migrate full saves/UI or declare visual completion until this scene meets the artistic gate.

## Protocol correction and iterations 06–13

The user rejected the invented UI direction, not just foliage. Iterations 06–10 are retained as exploration, **not accepted results**. Iteration10 has been re-inspected against official R14 and its long rounded header is explicitly rejected. The earlier subjective 41.5 score is not reused or presented as progress. A formal evidence set now contains20 distinct visual references (16 gameplay/world-map views +4 public artbook previews), plus2 aliases; see research/REFERENCE_ANALYSIS.md. R12 enlarged UI regions and R14 native image establish the HUD grammar.

| Actual capture | Three largest differences observed | Correction / disposition |
|---|---|---|
| iteration-10.png | Invented header hierarchy; bright disconnected leaf shards; isolated diamond against empty background | Replace with left circular instrument/attached counter and right meters; compact crown fans and lower leaf contrast; orthographic size30 |
| iteration-11.png | Thin UI glyphs/type and missing orange action emphasis; near-uniform crown colors; green slope instead of readable soil seam | Filled original icons and licensed bold type; spatial ochre/burgundy tree color groups; brown grain/moss transition |
| iteration-12.png | Repeating crown/branch arrangement; stones look like regular faceted tokens; animals lack convincing surface/animation detail | Add deterministic second oak/alder variants and beveled flattened stone variants in13; animal artistry remains open |

Comparison boards `previews/comparison-11.jpg`, `comparison-12.jpg` compare actual full views with official R14 using aspect-preserving downscaling. They expose composition differences; they are not exact object-scale matches. `UI-comparison-11.png` / `UI-comparison-12.png` compare native equal-pixel UI crops. No reference artwork is embedded into the runtime. Changes are mapped to UI01–08/ENV01–10 in VISUAL_SPEC.md.

### Technical results (actual measured)

Iteration11: 1920×1080, camera30, median19.05ms, p95 27.78ms, GPU11.28ms,252draw calls,1.52M primitives. Iteration12: same camera and tick240, median16.67ms, p95 22.22ms, GPU8.35ms,252draw calls. Hash matches direct Core replay in both; zero misclassified terrain cells. These sequential samples are under variable concurrent host load; timing differences do not prove optimization from the visual edits.

Close12 at requested20×: tick2546 after roughly12simulation-running wall seconds; actual approximately10.6×, with113.37simulation-seconds of pending work. Frame median19.74ms, p95 33.33ms; GPU7.39ms, main script10.19ms. **Fails sustained20× throughput target**, despite passing behavioral hash equality. Debt is retained, not discarded. Alive71, visible13. Fixed tick adapter tests at30/60/144FPS ×1/20 all pass when debt is drained. Actual UI pause/speed/quality signals also pass; inspector sits below world meters. See `close-12-20x.png.json`.

### Motion evidence

Iteration12 and iteration12-motion captured0.75s apart at the same paused tick. Pixel-region analysis (`motion-analysis-12.json`) shows mean absolute RGB difference2.25 in the river,10.17 in a canopy,0 in the counter. This confirms changing rendered environment while the tick/counter is held; temporal antialiasing also contributes, so this is not a pure wind-amplitude or speed measurement. Camera and animal simulation state are held; environmental clock continues.

### Remaining visual gate

Cohesion: UI now shares the reference's instrument grammar, but materials still need stronger common painterly treatment. Environment: smaller slice and sparse understory variety remain unlike the larger reference composition. Light/color: warm groups improve hierarchy but some leaves/animals are too bright. Vegetation: original silhouette variants are being checked; fine leaf noise persists. Camera: size30 improves map coverage but corner background remains. Motion: wind/water verified, no skeletal creature locomotion. These are rubric categories with the original weights25/20/20/15/10/10, not a similarity score. **No final art acceptance or full migration claim.**

### Iteration13 — actual inspection completed

Viewed `iteration-13.png`, `comparison-13.jpg`, and native-scale `UI-comparison-13.png`. Original second oak/alder variants now vary the branch fans; rock tops are flatter with small rounded weathered bevels, closer to R14's broad slate masses. UI structure and type remain consistent with12. Largest remaining differences: (1) forest understory/material richness and branch repetition still below R09/R14, (2) animal surface and gait remain primitive compared with readable reference animals, (3) river lacks the reference's localized bank foam and broad painterly surface detail. These remain open; art gate not passed.

Actual1920×1080 run13: median18.75ms (about53FPS), p9525ms, GPU3.70ms, main script1.628ms,301draw calls and1.99M primitives. Expanded geometry increases budget; GPU timing variance must not be interpreted as optimization. Core hash equals direct replay at tick240;0terrain mismatches; all10control checks true; six fixed-clock cases true; inspector below meters true. **Does not establish sustained60FPS.** See adjacent JSON for raw values. No source Core changes and no repository publication were performed in this visual revision.

## Holistic state pass — v14–17

### Evidence and iteration

Used existing R03/R05 dry terrain, R04/R09 animal readability, and R12–R15 UI grammar. Rain animation and animal gait timing are original decisions, not values inferred from still references. Reviewed real normal14, drought14, rain14, rain15, empty16, creatures15, gallery14/15 and the final8-state contact sheet. `climate-comparison-16.jpg` shows three actual environment states at one aspect-preserving scale. `creature-motion-16.gif` is24 actual Godot gallery frames, with **synthetic input poses** labeled in the image.

Three initial deficiencies: rain fog washed out vegetation, regular water rings dominated the river, feeding poses did not reach the ground. Iteration15 reduced fog from .002 to .001, reduced/rationalized ring density and brightness, and lowered the feeding head hinge with a connecting neck piece. Climate text lacked contrast; it now uses a narrow framed panel under the world meters. Iteration17 additionally prioritizes the selected animal in the16-slot canopy reveal list, enlarges only its local opening and supplies restrained inspection fill under deep canopy shadow.

### Test outcomes

`validation/state-tests-16.json` records eight real application runs: normal, drought, rain, extinct, full climate cycle, balanced graphics at distant zoom,20× target, close animal inspection. All eight final runs pass their applicable behavioral/input replay, cell-classification and control checks. Same-tick motion captures preserve simulation time. `pose-gallery-15.json` passes8joint/silhouette checks: freeze, moving legs, diagonal pair phase, feeding head, fruit head, juvenile scale, age saturation and side-lying corpse. These tests do not prove every interaction combination or art quality.

- Drought: water access .12,71live,2visible corpses, actual food loss and altered landscape.
- Recovery rain: water .144 after6days; it does not instantly become lush.71live,2visible corpses;1200rain instances (balanced500).
- Extinction:0live,0visible corpses,1actual death, finite0%meters. No divide-by-zero or lingering inspector.
- Full cycle: tick4901,2recorded climate changes,9births,55deaths,32live; input replay hash matches.7visible corpses match Core list.
-20×: behavior passes, but only tick1202 after roughly6wall seconds, with60.17simulation-seconds pending. **Still fails sustained20× throughput.**

Two invalid initial extinction fixtures are explicitly excluded: empty14 attempted a forbidden zero-founder configuration; the C# exception allowed the old world to continue and the raw generic capture falsely passed. empty15 correctly failed the new scenario assertion, with corpse lifetime below the legal minimum. Fixed fixture uses one real short-lived founder, valid5daycorpse lifetime and160warmup ticks. Initialization now fails fast on rejected scenarios. No Core validation rule was weakened.

Final observed frame medians range19.05–27.78ms in1080p climate runs under variable host load;720p balanced rain14.29ms. These do not establish sustained60FPS or performance at large populations. Articulated creatures add draw calls; future rig batching/LOD remains necessary before scaling.

### Remaining limits

No physical rainfall accumulation/flood simulation, snow, fire, disease or housing mechanics were fabricated. Rain displays the existing recovery process. Swimming currently adjusts body depth and reuses limb motion, without a dedicated swimming clip. Attack pose is a cosmetic head movement, not a jaw/contact choreography. Actual newborn sizing uses Core radius; no staged birth cinematic. Full saved-world menus and stacked observation panels still belong to the original client. Animal/forest materials and occlusion need continued art review; the Terra Nil quality gate remains open.

Inspection17 actual screenshot reviewed: selected animal now has a wider local opening and slightly brighter fur; branches can still overlap its silhouette, so occlusion quality remains an art-review item. This final targeted run passes hash,10controls,6clock cases, corpse synchronization and terrain checks. Median16.67ms, p9527.04ms at1080p; short-run timing only. The8-scenario report remains the broad regression record; this is the9th targeted capture.

## 24 October — iterations18–26, observer integration and gait

### Real evidence inspected

Viewed actual `pose-gallery-18-12.png`, menu/setup and observation stack captures from `validation/ui_checks.gd`, `distant-region-19.png`, `speed20-22.png`, and `live-motion-24-15.png`. Reopened native official R14 to compare HUD grammar, vegetation scale, water and clearings. UI check captures retain the `-19` filename as the test runner's stable output, and were overwritten by later corrected runs; they are **not immutable iteration19 evidence**. Logs in `work/ui19.log` through `work/ui26.log` record the iterations. `live-motion-24.gif` combines40 actual Godot frames of real Core creature#20, tick60→165, approximately5.25seconds, with camera tracking. It is not a synthetic action fixture; the separate pose gallery is explicitly synthetic.

Three largest remaining visual differences vs R14: (1) our trees are larger on screen and fine leaf noise dominates coherent painted crown masses; (2) understory, tree proportions and animal silhouettes still lack the reference's refinement; (3) shoreline/water shading is improved but terrain composition and the narrow regional view remain simpler. Menu/setup styles use the measured HUD grammar, but actual Terra Nil main-menu/layout evidence is missing. **Art acceptance remains open.**

Corrections in this pass: single-hinge legs replaced with knees, grounded feet and observed-motion gait; feeding neck retained; subtle inherited cool/warm coat tint; broken bank highlights; coordinate-stable heights/tree variants across regions; compact functional menu/map/observation controls; properly stacked observer panels. Actual captures revealed bottom controls were initially offscreen and the minimap border filled its entire area; both were corrected before delivery. A follow-camera capture initially showed the artificial regional edge; crossing into the adjacent region now retains the selected creature.

### Verified behavior and recoverability

- Core23 invariant cases pass, unchanged ReleaseCore SHA256 `50e63b1ef6fe985e8849d5ac55b27da9db0428f51bd3ca008f79785277459033`.
- Godot persistence/settings/export checks: full saved-state roundtrip, safe historical preview saves, exact replay of recorded drought to the original future, branch, rename, trash/restore, no deleted-world autosave resurrection, invalid creation leaves active world intact, weather preference saved, JSON/CSV export read-only.
- Four original SDL world files imported into isolated test storage; all source bytes retain their original SHA256. No user world was overwritten.
- Godot UI checks cover menu pause, every setting, immortal preset, graph persistence,9panels in[3,2,2,2]stacks, foreground selection,25panels staying onscreen, region navigation and new-world reconstruction.
- Locomotion:431consecutive stance samples; maximum sampled slip≈6×10^-8world units on flat ground. Ground contact, slope contact, backwards direction, finite turn, exact pause and swimming fixtures pass. These are bounded fixtures, not a claim that every gait at20× is visually perfect.
- Actual normal23/drought24/rain24/empty24 captures pass replay hashes,0terrain-cell classification errors and corpse synchronization. Empty world correctly displays0creatures/0corpses.
- Original SDL observer smoke suite passed after setting its existing bundled SDL library path. The first attempt could not locate SDL_ttf and is not a passing run. Expected deleted-save rejection appears in the successful suite log.

### Performance measurements, not universal guarantees

All below on the actual RTX5090LaptopGPU host, short runs, variable concurrent host load:

| Run | Resolution | Founders/map | Actual time rate | Median / p95 frame ms |
|---|---|---|---|---|
| final-controls-23 |1920×1080|78/128|~1×|13.895 /20.375|
| speed20-22 |1920×1080|78/128|20.003×last reporting window;3999ticks in~10s|18.739 /28.733|
| large-world-22 (latest overwritten by optimized run23) |1920×1080|500/256|11.804×last reporting window;2271ticks in~10s|19.444 /21.329|

The earlier large run reached1524ticks and~6.96×; removing offscreen agent dictionary transfer raised throughput. The small run meets20× over this short sample; the500-founder run **does not**. Neither establishes sustained60FPS at every population. Main improvements came from retaining the original optimized ReleaseCore (rather than debugCore), bounded12ms CPU budget at20×, and sending only region-visible fruit/creature snapshots. No biological rules were replaced for speed. Debt is retained and exposed in the speed tooltip.

### Iteration28 — world-window continuity

Maintenance of ENV04/ENV11, not a new art style. Actual `distant-region-28.png` viewed after the regional sampler change; patchy ground hierarchy and reed shore distribution remain. Before this pass, decoration sampling was tied to the current crop/RNG consumption and could reshuffle while panning. Candidate positions/angles/weights now match exactly across overlapping windows (grass, flowers, reeds:6checks). Fern/rock/edible-tuft random streams and foliage/rock spatial shader phase are also tied to world coordinates. No visual quality acceptance claimed.

The actual UI run adds a256→128world switch regression: the old large-world view origin is reset before rebuilding the smaller map, avoiding out-of-bounds resource indexing. All18UI checks passed and the biological hash is unchanged by renderer rebuilding. Existing paused desktop instance remained open, so this pass does not claim benchmark timing. Synchronous loading, grass resource normalization after reopening a region, and art refinement remain open.

### Iterations30–33 — observations and unchanged-terrain acceleration

Viewed actual `research-31.png`: gene chart and resources coexist with population/creature panels, share existing navy/ivory framing and species colors, and add no permanent toolbar buttons. This extends the existing reference-derived HUD grammar; no new Terra Nil menu/style equivalence is claimed. `life-trace-32.png` initially exposed a capture-harness issue: manually advanced world but HUD left at default0. Harness now refreshes actual snapshots before capture; runtime normal process already does this. Trace rows are real private replay samples, not illustrative curves.

Three largest remaining art differences remain crown noise/scale, creature/understory refinement, and regional composition. No blanket completion claim. Food depleted/restored fixtures are synthetic renderer-only snapshots; MultiMesh data matches actual stock but trees obscure that close view, so those images alone are not considered strong evidence of visibility.

Terrain33 cached/reference meshes match exact vertex and normal bytes in three windows. The optimization changes CPU cost, not the art or biological terrain classification. Short build timings are recorded in `validation/terrain-build-33.json`; no FPS inference is made.

## Iterations34–37 — actual canopy / foliage / research UI review

References re-opened: R14 official full gameplay, especially rounded dense grove masses; R09 user close vegetation. Actual Forward+ captures at1600×900: `before35-16/26.png`, `after35-16/26.png`, `after36-16/26.png`. Same paused world/camera and30warmup frames. Revision35 rejected: separated smooth crown balls became obvious. Revision36 reduced interior radii and restored leaf coverage; inspected both zooms, kept as a modest local improvement. It reduces fine contrast and preserves branching silhouette, but does not meet the whole Terra Nil target.

Three largest remaining differences: (1) oversized crowns and insufficient tree-to-tree shape/density variation; (2) repetitive grass/understory and simple creature materials; (3) river/light/composition still lacks reference ecological richness. No invented rubric score. UI functional additions use existing compact frames; no claim main menu/setup was reverse-engineered from missing references.

`family-37.png` actual1280×720: population, two creature cards and family panel coexist within screen. Empty founder family is correctly empty; linked-family edge cases are separately validated with isolated biological fixtures. `life-trace-32.png` re-opened after corrected HUD refresh: recorded20day trace and actual world day are coherent. No synthetic evolved behavior presented as gameplay evidence.

Revision36 follow-up: actual `rain-36.png` and `drought-36.png` at1280×720 opened and inspected. Both state/corpse/classification reports pass. Warm drought tint and subdued rainy lighting retain canopy shape; rain still appears hazy in the static overview, and this pair does not prove animation quality. These runs overlapped the existing paused desktop app, so timings are not clean GPU benchmarks.

## Iteration38 — unchanged rendering on cached revisit

Actual1280×720 `region-revisit-38.png` captured after navigating away and returning to the aligned(40,20)region; inspected. Mesh arrays, water shore-mask bytes and transforms match cold build exactly in separate fixture. Observer panel remains visible. No intended art change, no new reference-fidelity claim. Remaining three art discrepancies remain canopy scale/variety, understory/creature surfaces, and river/composition richness.

## Iteration39 — tree inspection, existing UI grammar

Actual `tree-inspector-39.png` at1280×720 inspected after30warmup frames: compact tree stock card under retained population chart; thin amber ground ring identifies selected trunk. No new permanent control and no change to forest geometry/materials. Title ID is stable within current world. No claim of matching previously uncollected Terra Nil resource-inspector UI. Remaining overall art deficiencies unchanged from38.

## Iterations45–47 — observable living forest and articulation

References: existing ENV/creature/UI rules in VISUAL_SPEC; no new claim of measured Terra Nil animation timing. All new frames are actual Forward+ Godot captures at reproducible origin42,20, seed42. Forest test deliberately uses shortened lifespan/maturity; climate test uses age-immunity and low drought tolerance.

Three deficiencies: (1) static forest despite ecological backend; (2) lower limbs stretched to unreachable targets and snapping replants; (3) hundreds of separate fruit draw objects and repeated full-landscape shoreline queries.

Implemented: original tree meshes scale with growth; health desaturates/browns canopy, death removes leaf shader surfaces leaving trunks/branches, expiry removes object; independent tree inspection/resource summaries. Short recovery steps replace replant teleport; IK clamps endpoint as well as knee, stop settles, paused rig freezes. Paw/hoof silhouette differs. Fruit MultiMesh preserves red stock markers with per-slot transforms. Shore optimization has exact behavioral tests.

Inspected actual captures: `../EVOSIM-Godot-Forest/previews/forest-renewal45.png`, `forest-drought47.png`, `forest-recovery47.png`, and live-motion46 frame20 (plus actual captured sequence/MP4). Dry yellow canopies transition to regrowth with changed spatial distribution; dead-tree status now shows cause instead of misleading100%health after age death. UI remains map-first.

Remaining three art deficiencies: animal bodies are still simple rounded procedural forms; dry bare branches expose thin angular geometry; dense regrowth can obscure small animals despite reveal masks. Actual captured motion and technical stance tests are evidence of implementation, not final animation/art acceptance. No global Terra Nil-quality claim.


## Iteration 48 — actual exported scene and locomotion
Evidence: `previews/package-smoke48.png` (actual Linux export, 1920×1080), `previews/locomotion48.mp4` from sixty actual Godot frames, source capture metadata in validation/live-motion-48.json. Inspected exported full scene and motion frames 20 and 35. No claim of inspecting the encoded video continuously.

Preserved: organic shoreline silhouette, original layered foliage, warm water/earth palette, navy/ivory compact HUD. New terrain sample arithmetic does not change sampled height/mask values. Walk/trot phase blend removes instantaneous hind-leg phase switches; normal-motion contact tests still pass.

Three largest remaining visual deficiencies: (1) animal torsos/head shapes still read as assembled rounded primitives at close zoom; (2) repeated dense grass silhouettes compete with small animals; (3) crowded canopy visibility needs more predator/crowd-specific review. These are not marked visually accepted. No new numerical art score was invented from two still frames.


## Iteration 49 — observation, creatures, frame cost (25 October 2026)
Implemented map-region inspection, one active map layer (living density / available food energy / last-30-day deaths), clickable short observation notes, population history with birth/death rates and timeline seeking, founder-versus-living genotype distributions, living descendant map tracking, gene-versus-adult-age offspring scatter, current action counts and recent death causes. Saved-world comparison shows parameters and population/resource summaries, with separate herbivore/carnivore means. Four-slot panel stacking remains. UI retains existing navy/ivory, green/copper, small orange accents; no extra always-visible toolbar.

Observation semantics: founders are retained founder genotypes; food is inventory, not path reachability; action counts are current, not historical strategy measurements; reproductive success counts children reaching maturity AGE, not inferred learned intelligence; both parents can share descendants. World comparisons use saved heads at their actual ages, not aligned experiments. No causal explanations are invented. The simulation model assemblies were not modified.

Original Blender creatures now use continuous torso meshes, narrower deer profiles, attached ears/branched antlers and lower broad-shouldered predators with triangular ears and dark saddles. Existing gait/IK contacts retained. Three MultiMesh limb batches replace twelve limb draw objects per creature. Distant unselected poses update at 30 Hz with accumulated delta; close/selected rigs update each render frame; positions and biology continue independently. Settled legs skip redundant IK. Simulation per-frame budget is 7 ms detail / 4 ms balanced; accumulated ticks are retained, so target speed can fall below requested speed.

Controlled 500-founder, 128-map, seed42, 600 prescribed-frame comparison: 480 measured frames, median 27.899→18.013 ms (~35.8→55.5 FPS), p95 60.478→35.284 ms. Both end at tick3999 and hash635171377bc932b2343428ef2cc1d6ee2a012d3e9d04bed8beeb93618f6dfebb. This short benchmark ends with189 living animals and19 rendered regional agents; it does not prove500 simultaneous visible animals at60FPS. Natural-clock balanced run: median19.444 ms (~51.4FPS), p9533.333ms, target20×, effective14.744× at capture,275 animals alive. Separate-render-thread experiment was rejected due shutdown errors.

Actual GPU captures: previews/observations49.png, comparison49.png, pose-gallery-49-12.png (synthetic poses), live-motion-49-28.png (actual simulation). Observation checks, comparison read-only checks, 19 UI checks, 12 locomotion checks, 6 limb batching checks, 8 pose checks and23 forest lifecycle/archive checks passed. Motion capture records60 actual viewport frames, ticks60→219; screenshot overhead prevents using it as a real-time benchmark.

Remaining release gates: sustained large-world60FPS/20×, synchronous cold region construction, multi-seed ecological population balance (iteration48 extinctions remain), broader Linux-machine/Windows/macOS tests, further art review. This iteration improves a playable preview; it is not proof that every roadmap goal is complete.


### Final portable verification49
Self-contained Linux export passed clean-HOME graphical launch with DOTNET_ROOT=/nonexistent and PATH=/usr/bin:/bin. At1920×1080,78founders, target20×: median14.583ms (~68.6FPS), p9521.426ms, effective20.009× at capture, 268frames. Six fixed-clock cases (30/60/144 renderFPS ×1/20speed) and all smoke control checks passed. This is a short local RTX5090Laptop measurement, not a hardware guarantee. See validation/verification49.json and package-smoke49.json. Preview distribution: ../EVOSIM-Linux-preview49-20261025.zip. Existing user worlds were not modified or migrated by testing.
