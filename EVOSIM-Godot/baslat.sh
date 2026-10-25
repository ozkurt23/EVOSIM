#!/usr/bin/env bash
set -euo pipefail
project_dir="$(cd -- "$(dirname -- "${BASH_SOURCE[0]}")" && pwd)"
workspace_dir="$(cd -- "$project_dir/../.." && pwd)"
# Default to the living-forest model; the original model remains explicit/recoverable.
if [[ "${1:-}" != '--legacy' && "${1:-}" != '--engine-only' ]]; then
    exec "$project_dir/yasayan-orman.sh" "$@"
fi
if [[ "${1:-}" == '--legacy' ]]; then shift; fi
local_sdk="$workspace_dir/work/toolchain/dotnet"
if [[ -x "$local_sdk/dotnet" ]]; then
    export DOTNET_ROOT="$local_sdk"
    export PATH="$local_sdk:$PATH"
fi
local_engine="$workspace_dir/work/toolchain/godot/Godot_v4.7.2-stable_mono_linux_x86_64/Godot_v4.7.2-stable_mono_linux.x86_64"
engine="${EVOSIM_GODOT:-$local_engine}"
if [[ ! -x "$engine" ]]; then
    engine="$(command -v godot-mono || command -v godot || true)"
fi
if [[ -z "$engine" ]] || ! command -v dotnet >/dev/null; then
    printf '%s\n' 'Godot 4.7.2 .NET ve .NET 10 SDK gerekli. README.md dosyasındaki kuruluma bakın.' >&2
    exit 1
fi
if [[ "${1:-}" == '--engine-only' ]]; then shift; exec "$engine" --path "$project_dir" "$@"; fi
dotnet build "$project_dir/EvoSimGodot.csproj" --nologo --verbosity quiet
"$engine" --headless --path "$project_dir" --editor --import
exec "$engine" --path "$project_dir" "$@"
