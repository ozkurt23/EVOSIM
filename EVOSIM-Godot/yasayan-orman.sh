#!/usr/bin/env bash
set -euo pipefail
project_dir="$(cd -- "$(dirname -- "${BASH_SOURCE[0]}")" && pwd)"
workspace_dir="$(cd -- "$project_dir/../.." && pwd)"
if [[ -x "$workspace_dir/work/toolchain/dotnet/dotnet" ]]; then
    export DOTNET_ROOT="$workspace_dir/work/toolchain/dotnet"
    export PATH="$DOTNET_ROOT:$PATH"
fi
engine="${EVOSIM_GODOT:-$workspace_dir/work/toolchain/godot/Godot_v4.7.2-stable_mono_linux_x86_64/Godot_v4.7.2-stable_mono_linux.x86_64}"
if [[ ! -x "$engine" ]]; then engine="$(command -v godot-mono || command -v godot || true)"; fi
if [[ -z "$engine" ]] || ! command -v dotnet >/dev/null || ! command -v python3 >/dev/null; then
    printf '%s\n' 'Godot 4.7.2 .NET, .NET 10 SDK ve Python 3 gerekli.' >&2; exit 1
fi
forest_dir="$(python3 "$project_dir/tools/prepare_forest_host.py")"
dotnet build "$project_dir/../EVOSIM-Models/LegacyBridge/LegacyBridge.csproj" -c Release --nologo --verbosity quiet
dotnet build "$forest_dir/EvoSimGodot.csproj" --nologo --verbosity quiet
"$engine" --headless --path "$forest_dir" --editor --import
exec "$engine" --path "$forest_dir" "$@"
