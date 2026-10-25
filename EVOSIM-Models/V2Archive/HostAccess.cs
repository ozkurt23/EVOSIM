using System.Runtime.CompilerServices;
// Shared Godot observer sources use archive internals without recompiling them into
// the rendering assembly. UI/art edits must not alter the biological fingerprint.
[assembly: InternalsVisibleTo("EvoSimGodot")]
