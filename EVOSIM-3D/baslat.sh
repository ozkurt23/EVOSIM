#!/usr/bin/env bash
set -eu
EVOSIM_PREVIEW_DIR="$(cd -- "$(dirname -- "$0")" && pwd)"
exec "$EVOSIM_PREVIEW_DIR/Build/EVOSIM-3D.x86_64" -screen-fullscreen 0 -screen-width 1600 -screen-height 1000 "$@"
