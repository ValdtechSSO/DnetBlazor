#!/usr/bin/env bash
set -euo pipefail

configuration="${DNET_BLAZOR_CONFIGURATION:-Release}"
runtime_dir="$(cd "$(dirname "$0")/bin/$configuration/net10.0" && pwd)"

case "$(uname -s)-$(uname -m)" in
  Darwin-arm64) node_rid="darwin-arm64" ;;
  Darwin-x86_64) node_rid="darwin-x64" ;;
  Linux-aarch64) node_rid="linux-arm64" ;;
  Linux-x86_64) node_rid="linux-x64" ;;
  *)
    echo "Unsupported Playwright installer platform: $(uname -s)-$(uname -m)" >&2
    exit 1
    ;;
esac

exec "$runtime_dir/.playwright/node/$node_rid/node" \
  "$runtime_dir/.playwright/package/cli.js" install "$@"
