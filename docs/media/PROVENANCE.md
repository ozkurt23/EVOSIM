# EVOSIM introduction — 25 October 2026

12-second silent montage, 1920×1080,30fps,H.264. Original EVOSIM visuals; no reference-game footage. First and final four seconds from validation/teaser49.gd in the actual Godot application; middle four seconds from the iteration49 live simulation recording (10 captured frames/second repeated into30fps output). Edited titles and entry/exit fades; no audio. The recording is a presentation, not a real-time FPS benchmark. The GIF is a640px,8fps derivative for GitHub README compatibility.

QA: reviewed frames at2,6,10seconds; source scene rendered360frames in Godot movie mode. The main Linux release was independently tested with a clean HOME. Cold source build exposed a ResolveReferences ordering issue; corrected model build to run before ResolveAssemblyReferences, then rebuilt with missing model bin/obj directories: zero warnings/errors.
