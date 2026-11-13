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

Opened panels share the TimePicker defaults: `--dnet-sys-material-popup` (which
defaults to `--dnet-sys-surface-raised`), `--dnet-sys-radius-lg` and
`--dnet-sys-elevation-3`. This applies to Select, Autocomplete, date and week
pickers, connected and floating panels, floating double lists, dialogs, toasts
and Grid advanced filters. List options use rounded rows with an inset and the
theme's hover and selected state colors.

Existing component overrides still take precedence, including
`--dnet-list-background-color`, `--dnet-list-border-radius`,
`--dnet-list-box-shadow`, `--dnet-calendar-background` and the Dialog surface
properties. When opening a panel in a scoped theme, use `DnetThemeScope` or its
configuration's `ThemeScope`; use `PanelStyle` for instance-specific tokens.

## Materials and Liquid Glass

`--dnet-sys-surface*` are colours: what focus-ring gaps, checkmarks, inverse
text and masks are cut out against. They stay opaque in every theme.
`--dnet-sys-material-*` describe how a container is painted: fill, backdrop
blur, rim highlight and top sheen.

| Token | Paints | Default |
| --- | --- | --- |
| `--dnet-sys-canvas` | The app background (`html` in `DesktopLayout`) | `surface` |
| `--dnet-sys-material-page` | Full-bleed regions: body, content, Tabs, Stepper | `surface` |
| `--dnet-sys-material-chrome` | Header, side columns and footer | `surface` |
| `--dnet-sys-material-card` | In-flow containers: ExpansionPanel, PillTabs track | `surface` |
| `--dnet-sys-material-popup` | Popup surfaces (see above) | `surface-raised` |
| `--dnet-sys-material-control` | Form fields with the `fill` appearance | the previous field fill |
| `--dnet-sys-material-ghost` | Outlined buttons | `transparent` |
| `--dnet-sys-material-backdrop` | `backdrop-filter` of chrome, card and popup | `none` |
| `--dnet-sys-material-highlight` | Inset rim shadows | an invisible inset shadow |
| `--dnet-sys-material-sheen` | Image layer over the fill | `none` |
| `--dnet-sys-scrim-backdrop` | `backdrop-filter` of the modal backdrop | `none` |
| `--dnet-sys-material-accent-sheen` | Image layer over accent fills (filled buttons) | `none` |
| `--dnet-sys-material-accent-highlight` | Rim and glow of accent fills | an invisible inset shadow |
| `--dnet-sys-material-ghost-elevation` | Drop shadow of ghost controls (outlined buttons) | an invisible shadow |
| `--dnet-sys-material-focus` | Focus ring of `fill` form fields | an invisible shadow |

Every default is what components painted before the tokens existed, so a theme
that does not set them looks exactly as it did.

The library ships a material theme in two files, loaded like any other theme:

```html
<link href="_content/Dnet.Blazor/styles/theme/liquid-glass.css" rel="stylesheet" />
<link href="_content/Dnet.Blazor/styles/theme/liquid-glass-dark.css" rel="stylesheet" />
```

Activate it with `data-dnet-theme="liquid-glass"` or `"liquid-glass-dark"`. The
dark variant is a separate theme rather than a `data-dnet-scheme`, so its name
crosses the overlay portal through `DnetThemeScope` and `ThemeScope`.

Glass has no outlines, so the themes also set a few component tokens:
`--dnet-btn-border-color` (transparent: outlined buttons become ghost pills),
`--dnet-form-field-radius`, `--dnet-form-field-hint-color`,
`--dnet-form-field-clear-icon` (a soft chip instead of the outlined circle),
`--dnet-calendar-icon-color` and `--dnet-timepicker-icon-color` (accent), and
`--dnet-menu-container-bgcolor`. Override any of them after the theme.

Replace the background by redefining the canvas; any `background` value works:

```css
[data-dnet-theme|="liquid-glass"] { --dnet-sys-canvas: url(/img/wallpaper.jpg) center / cover, #e8ecf4; }
```

Your own containers can use the same materials:

```css
[data-dnet-theme|="liquid-glass"] .my-card {
    background: var(--dnet-sys-material-sheen), var(--dnet-sys-material-card);
    box-shadow: var(--dnet-sys-material-highlight), var(--dnet-sys-elevation-1);
    backdrop-filter: var(--dnet-sys-material-backdrop);
}
```

The `/theming` sample's `DemoCard`, `DemoPage` and page stylesheets follow this
pattern. The sample also keeps its `body` transparent under these themes so the
canvas shows; an application that paints its own `body` must do the same.

**Opaque fallbacks.** Materials become opaque, without blur or sheen, when the
user asks for reduced transparency (`prefers-reduced-transparency: reduce`), in
forced-colors mode, and in browsers without `backdrop-filter`.

**Contrast.** `npm run check:contrast` composes each fill over the worst-case
backgrounds (every canvas stop and the base, black and white for popups, the
card under controls, the sheen peak on top) and requires WCAG AA. It reads the
theme's literals, so text tokens, fills and canvas stops stay `#hex` or `rgb()`.

**Limitations.**

- Only the outermost container blurs. A `backdrop-filter` nested in another only
  sees its ancestor's content, so controls, ghosts and the expanded menu
  submenu rely on their fill alone.
- A component whose shadow token is set to `none` (for example
  `--dnet-list-box-shadow: none`) loses the rim too: `box-shadow: <rim>, none` is
  invalid and falls back to `none`.
- While a filled or outlined button shows `:focus-visible`, the focus ring
  replaces its rim.
- The accent glow in `--dnet-sys-material-accent-highlight` uses the theme's
  primary colour. A button with its own `--dnet-btn-background` keeps that glow
  unless the same element also overrides the highlight token.
- Content does not scroll under a translucent header: `DesktopLayout` regions are
  adjacent grid areas.
- The Grid, Tooltip, Chips, Badge, Slider, SlideToggle, ImageEditor and inline
  calendars stay opaque on purpose.

## Theme studio sample

Open `/theming` to choose a theme and adjust its primary color, spacing unit and
button shape. The component preview inherits the same live values as the app;
separate examples demonstrate subtree, instance and nested-theme overrides.
The header and studio theme selectors stay synchronized, and the selected theme
is retained across reloads.

The CSS preview and **Copy CSS** export the values actually in use.
**Reset changes** clears those three customizations and restores the currently
selected theme's defaults. Other explicit styling properties remain intact.
