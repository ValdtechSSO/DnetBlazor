---
core: dotnet-test/run-tests
binding-revision: "1"
mode: extend
---

# Running DnetBlazor tests

The SDK-style test projects target `net11.0` and use xUnit v2 on VSTest.
Use a positional project or solution path and VSTest flags such as `--filter`
and `--logger trx`.

Run unit tests with:

```bash
dotnet test tests/Dnet.Blazor.UnitTests/Dnet.Blazor.UnitTests.csproj -c Release
```

For a full run, follow [.github/workflows/build-test.yml](../../../.github/workflows/build-test.yml).
Build the frontend assets and the Release solution, install Playwright Chromium
with `tests/Dnet.Blazor.BrowserTests/install-playwright.sh chromium`, and leave the
WebAssembly sample running in a separate terminal:

```bash
dotnet run --project samples/Dnet.ClientSide/Dnet.App.ClientSide.csproj \
  -c Release --no-build --no-launch-profile --urls http://127.0.0.1:5101
```

Once the sample is listening, run the solution with the browser and visual
checks enabled:

```bash
DNET_BLAZOR_BASE_URL=http://127.0.0.1:5101 \
DNET_BLAZOR_BROWSER_TESTS=true DNET_BLAZOR_VISUAL_TESTS=true \
dotnet test DnetBlazorComponents.sln -c Release --no-build --no-restore
```

Browser and visual tests can return early when their flags are absent. Report
which checks actually ran. For a browser-only run, target
`tests/Dnet.Blazor.BrowserTests/Dnet.Blazor.BrowserTests.csproj` and enable only
the applicable flags. For a single visual scenario, set `DNET_VISUAL_SCENARIO`
and filter on `FullyQualifiedName~VisualBaselineTests`. Linux is the CI
reference platform. See the [visual baseline guide](../../../tests/Dnet.Blazor.BrowserTests/VisualBaseline/README.md)
for platform-specific captures and diagnostics.
