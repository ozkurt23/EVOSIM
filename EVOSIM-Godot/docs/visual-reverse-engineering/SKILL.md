---
name: visual-reverse-engineering
description: Derive and implement a game's art direction and UI from visual references using source-linked decomposition, measurable rules, a small vertical slice, and real-render comparisons. Use when a user asks to match a reference game's visual style; do not substitute generic aesthetic redesign.
---

# Visual reverse engineering

Build an original implementation of the reference's visual construction. Do not treat a matching palette or a compiling renderer as style fidelity.

## Evidence before redesign

1. Inspect the real project and preserve working simulation/business logic. Do not assume the language named in a copied prompt matches the files.
2. Build a representative reference set: typically 15–20 distinct scenes for a broad game-style task, scaled to the user's scope. Include UI-visible gameplay, close-ups, multiple environmental conditions and official art-direction material when available. Track source URL/user attachment, resolution, crop/duplicate relationships and actual visual inspection. Do not count enlarged duplicates as new evidence or imply access to a paid artbook from its marketing page.
3. Decompose each reference into camera, silhouette/geometry, palette, surface detail, lighting/shadows, spatial density, motion evidence and UI structure. Separate observation from inference. Screenshots cannot prove animation speed or hidden renderer settings.
4. Analyze UI explicitly: screen anchors, component bounding boxes and proportions, panel contour, border/opacity, icon treatment, typography hierarchy, active/hover/disabled states where observed. A generic rounded dashboard with similar colors is not a substitute. Record unobserved states as unknown.

## Contract and mapping

Write one VISUAL_SPEC with rule IDs. Each rule links a reference region to a concrete implementation choice and an acceptance check. Pixel measurements may have a stated measurement tolerance; chosen implementation values must be executable. Never label inferred engine settings as measured facts.

Map reference functions to real product data. Preserve the visual grammar while adapting semantics; do not invent fake temperature, progress or resources to fill copied widgets. Keep the user's constraints (e.g. minimal controls) above reference fidelity.

Keep exact third-party artwork and proprietary assets out of the shipped implementation. Analysis references are not runtime assets. Use original or appropriately licensed production assets.

## Implement and verify

Build a small vertical slice first. Trace each art/UI change to a rule or a named discrepancy. Capture the real application at reproducible overview and close-view cameras. Compare adjacent crops at comparable visual scale; do not use stretched screenshots or arbitrary whole-image similarity scores.

For every iteration record: reference IDs/regions, actual capture, three largest discrepancies, implemented correction, remaining defects. Separate technical checks, subjective art review and user acceptance. Use the user's rubric when provided. Reject an iteration that matches colors but not geometry, density, composition or UI hierarchy. Do not expand the full migration before the agreed visual gate.

Validate behavior independently from pixels, profile real hardware, and label limitations. Reuse a successful rule set, not project-specific colors or one game's layout, in future tasks.
