# Theming guide

Load the base stylesheet and an optional theme stylesheet after it:

```html
<link href="_content/Dnet.Blazor/dnet-blazor-styles.css" rel="stylesheet" />
<link href="_content/Dnet.Blazor/styles/theme/dark.css" rel="stylesheet" />
```

Apply a theme globally with `document.documentElement.dataset.dnetTheme = "dark"`,
or scope it to a subtree with `data-dnet-theme="dark"`.

Global override:

Buttons use a filled primary action (`#5B5CE0` by default); choose
`Variant="DnetButtonVariant.Outlined"` for secondary actions. Both use pill corners
by default. Primary fills pair `--dnet-sys-primary` with `--dnet-sys-on-primary`;
brand-colored text on normal surfaces uses `--dnet-sys-primary-content` for
contrast without lightening the filled background. Set their radius token to choose another
shape, for example square corners:

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

## Popup surfaces

Opened panels share the TimePicker defaults: `--dnet-sys-surface-raised`,
`--dnet-sys-radius-lg` and `--dnet-sys-elevation-3`. This applies to Select,
Autocomplete, date and week pickers, connected and floating panels, floating
double lists, dialogs and Grid advanced filters. List options use rounded rows
with an inset and the theme's hover and selected state colors.

Existing component overrides still take precedence, including
`--dnet-list-background-color`, `--dnet-list-border-radius`,
`--dnet-list-box-shadow`, `--dnet-calendar-background` and the Dialog surface
properties. When opening a panel in a scoped theme, use `DnetThemeScope` or its
configuration's `ThemeScope`; use `PanelStyle` for instance-specific tokens.


## Theme studio sample

Open `/theming` to choose a theme and adjust its primary color, spacing unit and
button shape. The component preview inherits the same live values as the app;
separate examples demonstrate subtree, instance and nested-theme overrides.
The header and studio theme selectors stay synchronized, and the selected theme
is retained across reloads.

The CSS preview and **Copy CSS** export the values actually in use.
**Reset changes** clears those three customizations and restores the currently
selected theme's defaults. Other explicit styling properties remain intact.
