# Theming guide

Load the base stylesheet and an optional theme stylesheet after it:

```html
<link href="_content/Dnet.Blazor/dnet-blazor-styles.css" rel="stylesheet" />
<link href="_content/Dnet.Blazor/styles/theme/dark.css" rel="stylesheet" />
```

Apply a theme globally with `document.documentElement.dataset.dnetTheme = "dark"`,
or scope it to a subtree with `data-dnet-theme="dark"`.

Global override:

```css
:root { --dnet-btn-radius: 0; }
```

Subtree override:

```css
.danger-zone { --dnet-btn-background: crimson; --dnet-btn-foreground: white; }
```

One component:

```razor
<DnetButton style="--dnet-btn-background: crimson; --dnet-btn-foreground: white;">Delete</DnetButton>
```

`DnetThemeScope` renders `data-dnet-theme` for its subtree and component-owned
overlays (`Select`, `Autocomplete`, date pickers, floating double lists and Grid
advanced filters) carry the current scope to the global overlay host. Services
opened directly by application code use the explicit `ThemeScope` property on
their public configuration.

Arbitrary inherited custom properties do not cross the portal. Set
overlay-specific properties with `OverlayConfig.PanelStyle`.
