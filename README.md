# DnetBlazor

Blazor component library for .NET 10, compatible with Blazor WebAssembly and
interactive server-side applications.

**Current version:** 6.0.2. See the [changelog](Changelog.md) for release
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
dotnet add package Dnet.Blazor --version 6.0.2
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
- [NuGet publishing guide](docs/nuget-publishing.md)
- [Changelog](Changelog.md)
- [Sample application](samples/)

## License

See [LICENSE.txt](LICENSE.txt).
