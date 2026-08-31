# DnetBlazor

Blazor component library for .NET 10, compatible with Blazor WebAssembly and
interactive server-side applications.

**Current version:** 6.0.14. See the [changelog](Changelog.md) for release
details.

## Components

The library includes data-entry, navigation, overlay and data-display
components, including Grid, PickList, List, DoubleList, Select, Autocomplete,
Form, DatePicker, TimePicker, Slider, Badge, Tree, Tabs, Toast, Tooltip and
Overlay.

Runnable examples for every component live in [`samples/`](samples/). The demo
application is available at [datalnet.com](https://www.datalnet.com).

## Installation

Install the package:

```bash
dotnet add package Dnet.Blazor --version 6.0.14
```

Register the services in `Program.cs`:

```csharp
using Dnet.Blazor.Infrastructure.Services;

builder.Services.AddDnetBlazor();
```

Add the library assets to `wwwroot/index.html` (WebAssembly) or the host page
(server-side Blazor):

```html
<link href="_content/Dnet.Blazor/dnet-blazor-styles.css" rel="stylesheet" />

<script src="_content/Dnet.Blazor/dnet-blazor.js"></script>
```

`DnetOverlay` has no RxJS dependency. Applications using `ImageEditor` must
also load `_content/Dnet.Blazor/rxjs.min.js` before `dnet-blazor.js`.

Use the base stylesheet as the single library styling bundle. Optional themes
are regular CSS assets loaded after it; see the [styling guide](docs/styling/theming-guide.md).

Add one overlay host in `MainLayout.razor` when using floating components such
as dialogs, connected panels, floating panels, tooltips or toasts:

```razor
<DnetOverlay BaseZindex="1100" />
```

`BaseZindex` establishes the starting z-index for library overlays; choose a
value that fits the application's own stacking layers.

## Using Dnet.Blazor with an AI coding agent

Each release ships a machine-readable reference: every component's parameters,
every styling token, plus setup and theming guides. Plain markdown, so it works
with Claude, Codex, Cursor, Gemini CLI or anything else that reads project docs.

**[Download the agent reference](https://github.com/ValdtechSSO/DnetBlazor/releases/latest/download/dnet-blazor-agent-reference-6.0.14.zip)**
— attached to every release.

```bash
unzip dnet-blazor-agent-reference-6.0.14.zip -d docs/
```

Then point your agent at it. The archive's `AGENTS-snippet.md` has text you can
paste into your own `AGENTS.md`, `CLAUDE.md` or `.cursorrules`.

Prefer to browse it first, or pull it straight from source?

```bash
npx degit ValdtechSSO/DnetBlazor/docs/ai/reference docs/dnet-blazor
```

The reference is a snapshot of the version it ships with — grab the matching one
when you upgrade the package.

## What's new in 6.0.14

- Increased the Toast title and message sizes to 14px and 12px for better
  readability while preserving their hierarchy.
- Replaced the fixed light scrollbar thumb with theme-derived normal and hover
  colors suited to dark surfaces.

## What's new in 6.0.13

- Redesigned `DnetToast` with elevated theme-aware surfaces, severity accents,
  progress timing, actions, persistent notifications and an accessible
  four-item stack with queueing.
- Made Toast automatic dismissal reliable and synchronized it with hover and
  focus pauses.
- Aligned Grid group expand arrows with their labels and themed the
  AdminLayout minifier surface.

## What's new in 6.0.12

- Added an adaptive modal presentation to `DnetTimePicker`, with a fixed header,
  localizable cancel action and safe focus restoration after closing or choosing
  a time.
- Fixed Grid selection checkbox appearance and alignment on responsive layouts.
- Placed FloatingPanel scrollbars flush with the panel edge without changing its
  content spacing.
- Aligned Tree expand controls, selection checkboxes and labels consistently.

## What's new in 6.0.11

- Added the form-aware `DnetSlider`, with single and range modes, ticks,
  formatted discrete labels, semantic colors and RTL support.
- Added `DnetBadge`, `DnetTimePicker` and `DnetSlideToggle`, all aligned with
  the Dnet visual and token contracts.
- Expanded `DnetDatePicker` with typed ranges, inline calendars, comparison and
  preview APIs, configurable selection strategies and adaptive presentation.
- Integrated `DnetSlideToggle` into PickList items while preserving disabled
  selections and controlled state.

## What's new in 6.0.10

- PickList can now keep inherited selections disabled through
  `IsItemDisabled`.

## What's new in 6.0.9

- PickList items preserve their configured height when the component fills a
  taller panel.

## What's new in 6.0.8

- The Grid can now size responsive columns from their loaded content with a
  per-column cap, while keeping headers and cells aligned.

## What's new in 6.0.7

- Grouped Grid rows and the grouping placeholder column now respect
  `GridOptions.RowHeight`.

## What's new in 6.0.6

- `FloatingPanelConfig` supports accessible complementary-region semantics, live
  announcements and updating an open panel's dynamic content.
- Grid row nodes expose `FocusAsync` so consumers can restore keyboard focus
  after a contextual interaction closes.

## What's new in 6.0.4

- Fixed the Grid selection checkbox column so it respects `GridOptions.RowHeight`.
- Improved `FloatingDoubleList` service initialization, sorting callbacks and
  selection state handling.

## What's new in 6.0.3

- Added the Modern semantic theme and made it available through the persisted
  theme selector in the sample application.
- Added XML documentation for public component parameters and refreshed the
  bundled agent reference.

## What's new in 6.0.2

- Fixed row-span hover synchronization in `BlgGrid`, so a cell spanning several
  rows remains highlighted whichever covered row the pointer is over.
- Added a browser regression test for row-span hover behavior.
- Simplified the sample PickList styles to remove an unnecessary browser CSS
  preload warning.

## Documentation

- [PickList guide](docs/pick-list.md)
- [Overlay guide](docs/overlay.md)
- [Styling guide](docs/styling/README.md)
- [AI agent reference](docs/ai/reference/README.md)
- [NuGet publishing guide](docs/nuget-publishing.md)
- [Changelog](Changelog.md)
- [Sample application](samples/)

## License

See [LICENSE.txt](LICENSE.txt).
