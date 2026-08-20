# Theming Dnet.Blazor

Everything visual in the library resolves through CSS custom properties. You never
edit library CSS and never fight specificity — you set tokens and they cascade.

## Three levels of override, one mechanism

```css
/* whole app */
:root { --dnet-btn-radius: 0; }

/* one region */
.settings-panel { --dnet-list-item-height: 32px; }
```
```razor
@* one instance *@
<DnetButton style="--dnet-btn-background: crimson; --dnet-btn-foreground: white;">Delete</DnetButton>
```

All three work by plain inheritance, so the most specific placement wins. No
`!important`, no `::deep`, no wrapper classes.

## Two layers you can write to

**Component tokens** `--dnet-<component>-*` change one component. Each component's
file under `references/components/` lists its tokens and what they currently fall
back to.

**Semantic tokens** `--dnet-sys-*` change everything at once. This is the layer a
theme is made of — see `references/design-tokens.css` for the full list with
current values. Highlights:

| Token | Effect |
|---|---|
| `--dnet-sys-surface`, `--dnet-sys-on-surface` | Base background and text |
| `--dnet-sys-primary`, `--dnet-sys-on-primary` | Accent colour and text on it |
| `--dnet-sys-radius-sm/md/lg/pill` | Corner rounding across the library |
| `--dnet-sys-space-unit` | One value that rescales all spacing |
| `--dnet-sys-control-height` | Density of inputs and controls |
| `--dnet-sys-font`, `--dnet-sys-text-xs…xl` | Typography |
| `--dnet-sys-elevation-1…5` | Shadow depth |
| `--dnet-sys-motion-fast/normal/ease` | Animation timing |

## Writing a theme

A theme is one CSS file that redefines semantic tokens under a scope selector.
That's the whole format — see `references/theme-dark-example.css` for a real one
that ships with the library.

```css
/* my-theme.css */
[data-dnet-theme="brand"] {
  --dnet-sys-primary: #6366f1;
  --dnet-sys-radius-sm: 8px;
  --dnet-sys-radius-md: 12px;
  --dnet-sys-font: "Inter", system-ui, sans-serif;
  --dnet-sys-space-unit: 5px;      /* rescales every gap and padding */
  --dnet-sys-control-height: 44px; /* rescales every control */
}
```

Load it **after** the library stylesheet:

```html
<link href="_content/Dnet.Blazor/dnet-blazor-styles.css" rel="stylesheet" />
<link href="css/my-theme.css" rel="stylesheet" />
```

Activate it:

```js
document.documentElement.dataset.dnetTheme = "brand";
```

Because the selector is an attribute, you can also scope a theme to part of the
page instead of the whole document.

**State layers are derived, not enumerated.** `--dnet-sys-state-hover` and friends
are computed with `color-mix` from `--dnet-sys-on-surface`, so changing your text
colour flips hover and pressed states automatically. You rarely need to set them.

## Theming inside overlays

Dialogs, tooltips, toasts and floating panels render into a global host
(`<DnetOverlay />` in your `MainLayout`), **outside the DOM where you opened
them**. Since inheritance follows the DOM, an overlay opened inside a themed
container does not inherit that theme.

Use `DnetThemeScope`, which sets the attribute and carries the theme name through
to the overlay host:

```razor
<DnetThemeScope Theme="dark">
    @* dialogs and tooltips opened in here render dark *@
</DnetThemeScope>
```

Arbitrary token overrides do **not** cross into an overlay. To style one overlay
instance, pass custom properties through `OverlayConfig.PanelStyle`.

## Dark mode

The library ships `dark.css`, `compact.css` and `high-contrast.css` under
`_content/Dnet.Blazor/styles/theme/`. Link the one you want after the base
stylesheet and set `data-dnet-theme` to match.

To follow the OS setting, wrap it:

```css
@media (prefers-color-scheme: dark) {
  :root { /* paste the dark theme's declarations here */ }
}
```

## What tokens cannot do

Tokens change values, not structure. They will not move an element, add a divider
that doesn't exist, or change which DOM a component renders. For those, use the
component's own parameters, or `TooltipClass` / `PanelClass` style hooks where a
component exposes them.

If you find yourself needing a structural change frequently, that's worth raising
as an issue rather than working around with CSS overrides.
