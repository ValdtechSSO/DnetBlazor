# Setup

Dnet.Blazor 6.0.2. Targets .NET 10, works in Blazor WebAssembly and interactive
server-side.

## Install

```bash
dotnet add package Dnet.Blazor --version 6.0.2
```

Register services in `Program.cs`:

```csharp
using Dnet.Blazor.Infrastructure.Services;

builder.Services.AddDnetBlazor();
```

Add the assets to `wwwroot/index.html` (WebAssembly) or the host page
(server-side):

```html
<link href="_content/Dnet.Blazor/dnet-blazor-styles.css" rel="stylesheet" />

<script src="_content/Dnet.Blazor/rxjs.min.js"></script>
<script src="_content/Dnet.Blazor/dnet-blazor.js"></script>
```

That single stylesheet is the whole library. Themes are separate CSS files loaded
**after** it.

## The overlay host — required for floating components

Dialogs, tooltips, toasts, connected panels and floating panels all render into
one global host. Place it once in `MainLayout.razor`:

```razor
<DnetOverlay BaseZindex="1100" />
```

`BaseZindex` is the starting z-index for library overlays — pick a value that sits
above your own stacking layers.

**Without this, those components silently render nothing.** It's the most common
setup mistake.

## Imports

Add to `_Imports.razor` so you don't qualify every tag:

```razor
@using Dnet.Blazor.Components.Button
@using Dnet.Blazor.Components.Chips
@* …and the namespaces for whatever else you use *@
```

Namespaces follow the folder layout: a component in `Components/Tooltip` lives in
`Dnet.Blazor.Components.Tooltip`.

## Upgrading from 5.x

6.0.0 is a breaking release for styling. The library moved to a CSS custom-property
token architecture, and several token names changed.

- **Old token names still work.** They're kept as intermediate links in each
  component's fallback chain, so existing overrides keep taking effect. No extra
  file to link. That compatibility window closes in 7.0.
- **`PickList` no longer uses CSS isolation.** It ships in the main stylesheet now,
  so you no longer need `{App}.styles.css` for library styles.
- **Some component parameters that had hardcoded appearance defaults are now
  nullable.** Rendering is unchanged — the default moved into the CSS — but reading
  such a property in C# now returns `null` instead of a colour string.

See `Changelog.md` in the repository for the full list.

## Verifying it works

Render a `<DnetButton>` and open devtools. In the Styles panel you should see
`--dnet-sys-*` tokens resolving on `:root`. If they're missing, the stylesheet
link is wrong. If the button renders but a dialog doesn't appear, the
`<DnetOverlay />` host is missing.
