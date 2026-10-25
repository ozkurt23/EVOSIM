# Foliage revision36

`Grove-foliage36.blend` records the deterministic generator output. Only oak, oak_b, alder, alder_b, pine and shrub were installed from this revision. The other objects in that Blender file are generator candidates, not the current runtime rocks or creatures. Original `Grove-library.blend` is retained. Runtime asset-manifest merges only those six installed vegetation entries. Do not overwrite the articulated deer/wolf assets with legacy herbivore/predator candidates.

Generation changes random-number consumption, so a complete generator run also changes later non-vegetation candidates. Export to a staging directory and explicitly select reviewed assets. Biological world RNG is separate and never used.
