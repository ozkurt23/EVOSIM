# Experimental forest-aware CoreV2 — not active in Godot

This separate assembly EvoSim.Core.V2 starts from the frozen v1sources. No original source/binary/user world was replaced. It references ForestEcology and now connects actual corpse decay to capped soil deposits. Eaten mass is already removed before the decay hook; only decay loss and once-only expiry remainder are deposited. River cells reject deposits. The forest advances every20world ticks, matching1model day.

The new EVOSIM-FOREST-WORLD-1checkpoint contains the original biological records plus forest origin day and complete forest snapshot under the existing SHA256payload envelope. StateHash includes ecology and its epoch. Settings persist. Forest/world clock mismatch is rejected on save/load. World.Load refuses v1checkpoints; explicit ImportV1Origin accepts only a modern v1origin, creates new ecology and retains animal state/tick. LegacyBridge must first materialize a historical origin under the old engine. This is not permission to replay a v1timeline in v2. No full v2timeline/UI import is built yet.

An overload accepts ForestSettings for new worlds. Default constructor uses the draft settings. Imported baseline moisture is regenerated from the unchanged seed/config rather than double-attenuating already-dry current moisture.

**Revision43:** forest state now synchronizes actual Map.Trees and fruit. Persistent forest IDs map to resource objects; newborn trees have no fruit until maturation, fruit energy/regrowth is health/fertility/competition-limited, death removes fruit and expired trunks reopen ground. Seed-derived permanent terrain excludes original tree trunks while retaining rocks; dynamic occupancy refreshes land components/shore/path caches. Saved paths are preserved on load, invalidated only on live topology changes. New seedlings cannot occupy creature footprints. Life-cycle changes enter the journal.

**Still incomplete:** composite v2timeline and explicit GUI import/model routing, graphics/setup, large-world spatial indexing/performance and coupled long-run balancing. Do not switch the active Godot reference yet. V2 model fingerprint must cover both CoreV2 and ForestEcology assemblies, not just the former.

corev2-verification42.json:16checks including explicit import, exact composite save/load and future replay across repeated reloads/weather changes, backwards rejection by v1, corruption/truncation rejection, real corpse-decay/expiry/water hooks, daily cadence, clock validation and custom settings. Initial test used invalid CorpseDecay2and lifetime.05; corrected to existing validated limits rather than changing the model bounds. Existing v1 hash remains unchanged.

corev2-verification43.json adds real fruit/death/removal/rock protection, seedling occupancy, maturation, stable identity, saved path, forward replay and lifecycle journal checks;28total pass. The actual Godot application still runs v1.
