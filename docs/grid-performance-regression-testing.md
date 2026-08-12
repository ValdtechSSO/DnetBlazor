# Grid performance regression testing

The runtime performance work is applied one change at a time. Before changing
the Grid implementation, the browser baseline protects the three regressions
that have been observed manually:

- a transient empty-state message while a populated Grid is loading;
- increased grouping or ungrouping latency;
- uncovered vertical bands during fast virtual scrolling.

The test uses the existing complex Grid sample, so the measurement includes
templates, pinned columns, spans, selection, grouping, pagination and
virtualization.

## Reference comparison

The Grid source, grouping service and virtualization source at tag `v5.0.3`
are byte-for-byte identical to the same files at commit `4407d4c`. The observed
difference therefore isn't caused by a committed Grid change after `v5.0.3`.
The production image is published in `Release`, while the local IDE normally
runs WebAssembly in `Debug`; stale generated WebAssembly output can also retain
an implementation after its source has been rolled back.

The first isolated change after establishing this reference skips the two full
tree visibility resets that grouping previously performed when both simple and
advanced filters are empty. Active filters and server-side filter callbacks
continue through the existing paths.

## Run locally

Start the WebAssembly sample on its configured HTTPS endpoint:

```shell
dotnet run --project samples/Dnet.ClientSide/Dnet.App.ClientSide.csproj
```

In another terminal, run the opt-in browser baseline:

```shell
DNET_BLAZOR_BROWSER_TESTS=true \
DNET_BLAZOR_GRID_PERFORMANCE_TESTS=true \
DNET_BLAZOR_BASE_URL=https://127.0.0.1:5101 \
dotnet test tests/Dnet.Blazor.BrowserTests/Dnet.Blazor.BrowserTests.csproj \
  --filter GridPerformanceBaselineTests \
  --logger "console;verbosity=detailed"
```

The test fails if the populated Grid shows an empty-state message or if any
animation frame contains an uncovered vertical band. It also prints initial
render, grouping and ungrouping durations. Those timings are observations, not
absolute CI gates, because host load affects browser timings.

For every subsequent optimization:

1. record the baseline medians from at least five runs;
2. apply only one production change;
3. repeat the same runs and the manual fast-scroll check;
4. retain the change only if there are no blank frames and the relevant median
   does not regress by more than ten percent.
