# EvoSim visual contract — reference-derived revision 2026-10-23

This supersedes the initial generic style brief. Evidence: [reference decomposition](research/REFERENCE_ANALYSIS.md), [source manifest](research/reference-manifest.json), [pixel measurements](research/UI_MEASUREMENTS.json). R12–R15 are official Terra Nil gameplay screenshots; R09 is the user's close-up; R19–R22 are public artbook previews, not the complete paid book. Exact hidden renderer settings are unknown. **All numeric implementation values below are chosen EvoSim settings**, with visible evidence and a test; they are not asserted Terra Nil settings.

## UI: structural contract

| Rule | Visible evidence | Implementation | Acceptance |
|---|---|---|---|
| UI01 Corner instrument | R12–R15 top left, circular disc about 180px at native 1080p | 168px disc at (8,8); inset rings, two population sectors, pale original leaf/paw glyphs and live counts | No long brand/header strip. Disc reads as dominant left instrument; count split reflects live data |
| UI02 Attached counter | R14 counter roughly x171–332, y23–89 | Total living count on dark rectangular plate behind disc at (154,26), 156×52; calendar below | Numeric emphasis, no unrelated logo/world subtitle |
| UI03 Playback | R12–R15 small buttons attached below counter | Pause and speed controls at (181,92), 36px high; orange active/pause action; one 36px quality utility beside counter | All three work; tooltips explain; no speculative construction buttons |
| UI04 Right meters | R12 two rows, R13 three, R14 one; narrow dark inset bars and bold pale numbers | 294px right panel, two 12px bordered bars for real mean energy and hydration; leaf/drop original icons, percentages | Values computed from alive creatures, never fake climate data; energy normalized per creature |
| UI05 Surfaces | R12 enlarged crops; R14 sample regions | Navy #19333e, inset #10242e, slate outline #557985; 2px border; 2px corners; amber #b85a20 actions; pale #f3f2df text | Compact angular instrument appearance, no pills or oversized soft shadows |
| UI06 Hierarchy | Bold counts and simple glyphs in R12–R15 | 24px population total, 20px species counts, 13px calendar, 14px secondary labels; filled original glyphs where useful | At native 1080p numbers readable without large text blocks |
| UI07 Selection | No directly equivalent creature inspector in supplied set | Adapt UI04 surface/bars to right card below world meters; species/id heading, age/generation and two values | Explicit semantic adaptation, not claimed copy of a reference inspector; does not cover world meters |
| UI08 Controls scope | R14 bottom square tools represent construction actions | Do not invent equivalent construction actions. Reserve square tile grammar for actual future observation tools | Permanent controls limited to pause, speed, quality; details appear on selection/hover |

Hover/disabled styles are not established by still references. EvoSim uses a brighter border on hover/focus, never claims these are Terra Nil interactions. All hit targets >=32px. Reference icons, typeface files and thumbnails are not copied.

## Assets, terrain, camera and atmosphere

| Rule | Evidence | Implementation | Acceptance |
|---|---|---|---|
| ENV01 World composition | R14/R18 terrain extends beyond image; river divides dense forest from clearing | Default orthographic size 30, diagonal yaw .66rad, elevation 42°; overview zoom remains available | Default frame primarily terrain; record both opening crop and whole-slice overview. Camera values are choices, not reverse-engineered engine values |
| ENV02 Crown silhouette | R09/R14 broad rounded deciduous, narrow deciduous and layered conifers, dense crown interiors | Blender oak/alder fans overlap vertically; reduce leaf length to .075–.15 units, pack sprays 220 leaves, compact branch spread and taller spray volume; preserve three species silhouettes | At comparable tree pixel size crowns read as coherent masses before individual leaves; no exposed wire-cage center |
| ENV03 Crown color | R14 green-dominant forest with grouped pale/ochre/burgundy patches; R18 quieter green field | Low-frequency position-driven warmth; keep most trees green, choose subdued warm outliers within palette; reduce per-leaf palette contrast | Neighboring trees share color character; no random rainbow or uniform lime grove |
| ENV04 Ground hierarchy | R04/R14/R18 quiet grass clearings, dense forest-edge undergrowth and flowers | Core food patches remain authoritative; ornamental grass/shrubs clustered by existing deterministic spatial fields; preserve quiet open land | Distinguish resource fruit/grass from shrubs; no uniform noise carpet |
| ENV05 Shore | R09 dark brown exposed bank; R14 moss lip and thin broken edge line | .25-unit terrain sampling, shallow drinkable edge, brown soil band below green lip, lighter water edge | No water rocks caused by land/water mismatch; 0 misclassified cell centers; do not recreate high inaccessible cliffs |
| ENV06 Rocks | R14 flattened slate masses with broad top face, smaller chipped bevels | Irregular 7-sided rings, buried base, narrow upper bevel and broad top; color variation across stone clusters | Avoid repeated octagonal token silhouettes; test close view, not only overview |
| ENV07 Surface hierarchy | R18 broad water paint patches + tiny glints; R09 fibrous grasses | Opaque blue water with low-frequency tone blocks and sparse slow glints, matte foliage/land, fine restrained ground grain | No glitter blanket, no smooth untextured green slab, no requirement for costly refraction |
| ENV08 Light | R09/R14 warm upper foliage, dark cool interiors, soft contact shadows | Sun energy1.35, ambient .45, warm key/cool fill, SSAO, ACES, no bloom; 4xMSAA/TAA | Animal silhouettes remain readable; crown interiors retain detail; avoid black shadows and neon leaves |
| ENV09 Motion | Screenshots cannot establish timing | Chosen wind .8rad/s amplitude .065units; water independent of tick speed; cosmetic animal motion tied to simulation ticks | Two captures at identical paused tick show water/foliage motion while animal position stays fixed |
| ENV10 Observation | R04/R09 animals framed by vegetation; user requires forest visibility | Depth-aware local canopy reveal around actual animals, subtle ground ring for selected creature | No full ghost-tree forest; selected animal remains readable in close view |

## Technical gate and art gate

Preserve C# EvoSim.Core. Render through immutable snapshots; fixed ticks, smooth visual movement, original .glb models and chunked MultiMeshes. Reference images are research only (`research/.gdignore`), never scene textures. Budget: profile 1080p on actual device, target ~60FPS; record CPU/GPU/frame p95 and concurrent load. No clean-machine or cross-platform claim from this host.

Every iteration must record actual screenshot paths, references/regions, three largest differences, corresponding changes and remaining defects in VISUAL_QA.md. Compare UI crops at **native equal pixel scale**, environment crops at comparable object scale. No numerical image-similarity claim. Rubric: cohesion25%, environment20%, light/color20%, vegetation15%, camera10%, motion10%; weights structure review, not objective scores. Passing tests does not pass art. User has rejected iterations through 10; final visual acceptance remains pending. Do not expand migration until this gate is reviewed.

## Iteration 12 refinements

UI05/UI06: unmodified licensed DejaVu Sans Bold for 17px+ values; filled original leaf/paw/drop glyphs. Orange pause action and round orange quality utility derive from R14. ENV03: spatially grouped ochre-to-muted-burgundy foliage, green majority. ENV05: brown grain extends to 0.47 visual height with a soft moss transition; no geometry/depth change. These address measured/observed differences in comparison-11, not new unrelated features.

Iteration13 ENV02/ENV06: two independently generated deterministic oak/alder meshes, slight height/branch variations, two flattened stone variants with .075-unit two-segment bevel. The same biological tree positions are retained; extra visual variants do not create food sources. Asset variants raise actual visible primitives/draw calls; see the measured13 budget, not the initial aspirational budget.

## Ecosystem state extension — v14–16

| Rule | Evidence / reason | Executable presentation rule | Validation |
|---|---|---|---|
| STATE01 Drought | R03/R05 dry earth vs R14 healthy green; Core WaterLevel and moisture | Global dryness=clamp((1-WaterLevel)/.88); terrain mixes toward muted ochre, foliage toward dry warm tint; food instances retain actual resource growth | drought fixture, water≈.12, no terrain collision changes |
| STATE02 Rain recovery | User request; still references do not establish rain motion | Existing SetDrought(false) recovery,1200 instanced rain streaks/500 balanced; sparse surface rings, sun1.35→1.0; fog limited .001 | rain fixture, water rises gradually; rain present even while land remains dry |
| STATE03 Animal silhouettes | R04/R09 distinct readable animal profiles; EvoSim original species | Deer: upright neck, long ears, horns, pale belly. Wolf: low long muzzle, pointed ears, dark saddle/ruff, longer tail | Native renderer pose gallery, no proprietary character assets |
| STATE04 Locomotion | User rejects whole-body wobble | Four separate leg hinges; diagonal pairs; stride phase follows DistanceTravelled; head feeding rotation-1.1rad; exact joint freeze at same tick |8pose checks and24-frame actual render sequence |
| STATE05 Lifecycle | Actual Radius/Maturity/Aging/Corpses | Radius scales body continuously, age reduces saturation, corpse rests sideways and disappears with authoritative resource | empty/extinction fixture, corpse synchronization, synthetic juvenile/old/dead poses |
| STATE06 Consistent HUD | UI01–08 + state observability | Compact framed climate line beneath world meters; inspector shows actual Core action and juvenile/injury status |1080p/720p, inspect/empty and climate states |

References establish static visual language; motion timing and the240day demonstration climate cycle are original implementation decisions, not reverse-engineered Terra Nil mechanics. The climate cycle supplies explicit recorded inputs to the unchanged biological core. No new creature strategy/reward is introduced.

## 24 October — functional observer adaptation

User explicitly requested completing the remaining observer work while away. This authorizes integrating controls, but does **not** close the outstanding art-quality gate. The new menu/setup screens adapt UI05's frames, type and colors; their information layout is EvoSim-specific and is **not claimed to be a measured reproduction of Terra Nil's main menu**. Actual Terra Nil main-menu/settings references still need collection before claiming fidelity there.

- UI09: permanent bottom-left controls are compact menu, observation selector, and collapsible world map. No unimplemented construction tools.
- UI10: observation positions are left-top, left-bottom, right-top, right-bottom. Selections5/9/13 enter behind position1. Visible headers select the foreground panel; hidden bodies do not draw through it. Overflow stays within the viewport. Graphs survive creature selection.
- UI11: pre-world tabs expose every annotated Core setting and24 founder gene bounds, tooltips from the existing model descriptions, normalized sliders, numeric quantities and years. Native limits still validate in Core; `allow_greater` is not a promise that any population fits memory.
- MOTION01: two-segment procedural limbs solve feet to ground height. Stance anchors are world-space; swing arcs depend on rendered travel and observed forward/backward direction. This is a cosmetic solver, not an AI steering rule. Turning can replant a foot when reach is exceeded.
- MOTION02: walking uses four-beat offsets; faster motion uses diagonal pairs; swimming lifts/paddles feet. Feeding/head and bite poses remain driven by actual action fields. Age desaturates; boldness adds a restrained cool/rust fur tint.
- ENV11: regional rendering retains48×48 visible terrain but permits navigation across the real128/192/256 world. Surface elevation and tree identity use world coordinates. Regional generation is currently synchronous; it is an interim scaling solution, not seamless large-world streaming.

## Foliage revision36 — ENV02/03 refinement

Evidence: R14 left grove / upper-right autumn crowns and R09 red canopy. Preserve original irregular silhouette; reduce local leaf-palette contrast to38% of former variation around62% base. Broadleaf sprays retain82% leaf samples over an internal irregular crown at62% spray radii. These are chosen implementation parameters, not measured Terra Nil settings. Revision35 with84% interior /43% leaves was rejected after actual close render because obvious smooth balls replaced crown structure.

Acceptance for this bounded change: retain broken silhouettes at zoom16, avoid exposed sphere masses, reduce isolated bright leaf noise at zoom26. Overall tree scale, understory variety and animal surface treatment remain unresolved; this is not overall visual acceptance.


### Iteration49: observation and animal refinements
Continue STATE03 / ENV10 and existing UI reference rules. Animals must have continuous torso silhouette; deer slender neck/limbs, visible ear attachment and branched antlers; predators lower body, broader shoulders, dark saddle, attached triangular ears. Original assets only. Maintain warm earth fur, restrained specularity and age desaturation. At normal world scale read diet from silhouette before detail.
Map evidence uses thin ivory/amber rings and muted green/copper data, never recolors biological aggression/age colors. Keep one geographic data layer active at once. Existing four panel slots remain. History uses days/years, founder/current histograms use within-diet percentage with shared scale. Show sample counts and meaningful denominators. Empty offspring samples must not invent a one-child maximum. Distinguish inventory, association, and causal evidence. Use actual viewport captures for QA, not gallery renders alone.
