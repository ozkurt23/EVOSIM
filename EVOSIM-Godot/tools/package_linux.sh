#!/usr/bin/env bash
set -euo pipefail
project_dir="$(cd -- "$(dirname -- "${BASH_SOURCE[0]}")/.." && pwd)"
workspace_dir="$(cd -- "$project_dir/../.." && pwd)"
export DOTNET_ROOT="${DOTNET_ROOT:-$workspace_dir/work/toolchain/dotnet}"
export PATH="$DOTNET_ROOT:$PATH"
engine="${EVOSIM_GODOT:-$workspace_dir/work/toolchain/godot/Godot_v4.7.2-stable_mono_linux_x86_64/Godot_v4.7.2-stable_mono_linux.x86_64}"
forest_dir="$(python3 "$project_dir/tools/prepare_forest_host.py")"
output_dir="$project_dir/../EVOSIM-Linux"
mkdir -p "$output_dir"
"$engine" --headless --path "$forest_dir" --editor --import > "$output_dir/import.log" 2>&1
"$engine" --headless --path "$forest_dir" --export-release 'Linux Desktop' > "$output_dir/export.log" 2>&1
# Godot can exit zero after a failed C# export, so check its log and actual runtime files.
if rg -q '^ERROR:' "$output_dir/export.log"; then cat "$output_dir/export.log"; exit 1; fi
test -f "$output_dir/data_EvoSimGodot_linuxbsd_x86_64/EvoSimGodot.dll"
dotnet publish "$project_dir/../EVOSIM-Models/LegacyBridge/LegacyBridge.csproj" -c Release -r linux-x64 --self-contained true -o "$output_dir/legacy" --nologo --verbosity quiet
"$engine" --headless --path "$forest_dir" --script res://validation/licenses48.gd
mkdir -p "$output_dir/licenses"
cp "$forest_dir/validation/Godot-LICENSE.txt" "$forest_dir/validation/Godot-THIRD-PARTY.txt" "$output_dir/licenses/"
cp "$project_dir/assets/fonts/LICENSE-DejaVu.txt" "$output_dir/licenses/"
cp "$DOTNET_ROOT/LICENSE.txt" "$output_dir/licenses/Dotnet-LICENSE.txt"
cp "$DOTNET_ROOT/ThirdPartyNotices.txt" "$output_dir/licenses/Dotnet-THIRD-PARTY.txt"
cp "$project_dir/packaging/linux/BENI-OKU.txt" "$project_dir/packaging/linux/baslat.sh" "$output_dir/"
printf '%s\n' 'Build complete. Run the isolated smoke test before distributing.'
