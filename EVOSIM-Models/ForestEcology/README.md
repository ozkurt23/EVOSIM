# Deterministic forest kernel — experimental v2 building block

Implemented in a separate .NET10library; **not connected to the active Godot/Core animal simulation yet**. One StepDay is one model day. No renderer, wall clock, animal policy or old Core reference. Settings expose the seven agreed controls.

Implements persistent independent xorshift RNG, stable increasing tree IDs, age and health, accumulated growth before fruit, drought injury/recovery, finite lifespan (0disables age death only), thirty-day dead trunks, finite365day seed viability, bounded local mineral bonus with half-life, growth consumption and spatial competition. Seed spread is at most.08attempts per adult/day times the selected0–1setting. Minimum spacing is1+2*(1-density)cells; soft competition radius2+4*(1-density). These are explicit provisional model constants, not biological measurements. New trees can be vetoed by the host's occupancy callback.

DepositDecay accepts actual decomposed, uneaten biomass only. It saturates a land cell and cannot directly create a tree. The host still must implement exactly-once corpse loss accounting and feed this method; this kernel by itself does not inspect corpses or animal consumption. Soil spreading beyond one cell, full mineral mass balance, canopy geometry/collision and fruit objects are not integrated. FruitFactor exposes health/fertility/competition scaling with zero for seedlings/dead trees, but the host must connect it to real fruit.

Save/Load includes terrain masks, moisture, bonus soil, trees, finite seed bank, time, nextID and RNG. Format EVOSIM-FOREST-1 is a module snapshot, **not a full v2world or timeline**. Validate settings/arrays/IDs/cells/numbers and reject unsupported/truncated states. The eventual full-world checkpoint envelope must add integrity checks and strict compatibility; do not accept this as completed v2 migration.

Run ../ForestChecks using .NET10. forest-verification41.json records29checks including repeated mid-run save/reload equality, drought/age/dead-trunk/seedling behavior, saturated soil/half-life, no automatic corpse-tree conversion, occupied cell rejection, malformed snapshots and three100year fixtures. The fixtures are only8x8cells and use a fast-observation profile. They are not coupled ecology validation, full-map performance or proof of universal equilibrium.

Current simple neighborhood scans are quadratic in tree count; spatial indexing will be needed before large-world integration. Active v1Core and existing user files remain untouched.
