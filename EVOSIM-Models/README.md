# Model version boundary — v1 preservation

The living-forest feature changes biology, so it must not replay old histories using new rules. This directory is the first implemented prerequisite, not a completed ecology update.

- `v1/bin/EvoSim.Core.dll` is the original runtime assembly with SHA256 `50e63b1ef6fe985e8849d5ac55b27da9db0428f51bd3ca008f79785277459033`.
- `v1/source/Core` and `v1/source/Archive` freeze the matching core and archive helper sources. `v1/manifest.json` hashes every preserved file. The active Godot build still uses the existing v1 project; no world or fingerprint was changed.
- `LegacyBridge` is a separate .NET10process, referencing that pinned binary rather than recompiling it. Startup rejects any changed core assembly. Keeping v1/v2 in separate processes also avoids the existing single-model archive fingerprint guard being mixed in one host.

## Commands

Build/run using the project's local .NET SDK, or installed .NET10:

```
dotnet run --project LegacyBridge -- selftest
dotnet run --project LegacyBridge -- inspect /absolute/path/ID.evoworld [tick]
dotnet run --project LegacyBridge -- extract /absolute/path/ID.evoworld /absolute/new-output-directory [tick]
```

Inspect loads and validates the head; a supplied historical tick uses only the old engine and original recorded commands. Invalid/out-of-recording times are rejected. Extract writes a verified `origin.world` plus provenance into a new directory using a sibling staging directory and rename. An existing destination is never overwritten. Source archives are read only. Legacy archives without a timeline can expose their saved head, not invented earlier history.

The output is a **v1 origin checkpoint**, not a v2 world. A future v2 importer must explicitly map biological state, initialize saved ecology state and start a separate v2history at this origin. Do not label the old replay as v2-equivalent. The graphical selection/import flow and v2 importer are still pending.

## Evidence

`legacy-selftest40.json`: exact historical origin, unchanged source/head, output overwrite rejection, past/future bounds and provenance output. `legacy-verification40.json`: all4existing original user worlds read successfully with source hashes unchanged. These checks do not modify or migrate the user's worlds.

## Experimental v2 progress

ForestEcology supplies deterministic tree/soil/seed state. CoreV2 integrates actual corpse decay, fruit and dynamic tree obstacles with a new full-world checkpoint. V2Archive provides model-fingerprinted seek/branch/save/replay. Separate test projects and JSON reports document each boundary. None is selected by the current Godot app yet. Original v1remains the active application model; the new forest feature is not user-ready until graphical routing, setup/rendering and broader coupled testing are complete.
