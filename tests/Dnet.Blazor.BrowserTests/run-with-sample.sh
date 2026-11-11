#!/usr/bin/env bash
set -euo pipefail

sample_url=http://127.0.0.1:5101
sample_log=/tmp/dnetblazor-sample.log
dotnet run --project samples/Dnet.ClientSide/Dnet.App.ClientSide.csproj \
    --configuration Release --no-build --no-launch-profile --urls "$sample_url" \
    > "$sample_log" 2>&1 &
sample_pid=$!
trap 'kill "$sample_pid" 2>/dev/null || true' EXIT

for attempt in {1..60}; do
    if ! kill -0 "$sample_pid" 2>/dev/null; then
        break
    fi
    if curl --fail --silent --max-time 2 "$sample_url/" > /dev/null; then
        DNET_BLAZOR_BASE_URL="$sample_url" DNET_BLAZOR_BROWSER_TESTS=true DNET_BLAZOR_VISUAL_TESTS=true \
            dotnet test DnetBlazorComponents.sln --configuration Release --no-build --no-restore --verbosity normal "$@"
        exit 0
    fi
    sleep 1
done

cat "$sample_log" >&2
exit 1
