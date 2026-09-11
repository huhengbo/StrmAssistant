#!/usr/bin/env bash

set -euo pipefail

repo_root="$(CDPATH= cd -- "$(dirname -- "$0")/.." && pwd)"
python_cmd="${PYTHON_CMD:-python3}"

exec "$python_cmd" "$repo_root/scripts/build_plugin.py" "$@"
