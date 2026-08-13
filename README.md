# DnetBlazor

Blazor component library for .NET 10, compatible with Blazor WebAssembly and
interactive server-side applications.

**Current version:** 5.0.4. See the [changelog](Changelog.md) for release
details.

## Components

The library includes data-entry, navigation, overlay and data-display
components, including Grid, PickList, List, DoubleList, Select, Autocomplete,
Form, DatePicker, Tree, Tabs, Toast, Tooltip and Overlay.

Runnable examples for every component live in [`samples/`](samples/). The demo
application is available at [datalnet.com](https://www.datalnet.com).

## Installation

Install the package:

```bash
dotnet add package Dnet.Blazor --version 5.0.4
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

<script src="_content/Dnet.Blazor/rxjs.min.js"></script>
<script src="_content/Dnet.Blazor/dnet-blazor.js"></script>
```

If the application uses isolated component CSS, include its generated stylesheet
too (for example, `<link href="MyApp.styles.css" rel="stylesheet" />`). This is
required for `PickList` and any other component that uses CSS isolation.

Add one overlay host in `MainLayout.razor` when using floating components such
as dialogs, connected panels, floating panels, tooltips or toasts:

```razor
<DnetOverlay BaseZindex="1100" />
```

`BaseZindex` establishes the starting z-index for library overlays; choose a
value that fits the application's own stacking layers.

## What's new in 5.0.4

- Added `PickList<TItem, TKey>`, a controlled, key-based multi-selector for
  local collections and paged server-side data.
- Added cancellable provider requests, retained selection across search and
  pages, localization support and shared paginator styling for PickList.
- Added unit and browser regression coverage for the sample application.
- Added a trusted-publishing workflow for NuGet.org releases.

## Documentation

- [PickList guide](docs/pick-list.md)
- [NuGet publishing guide](docs/nuget-publishing.md)
- [Changelog](Changelog.md)
- [Sample application](samples/)

## License

See [LICENSE.txt](LICENSE.txt).
