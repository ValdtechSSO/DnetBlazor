# Migrating styling from 5.x to 6.0

Version 6 moves the styling API to semantic and component token layers. Load the
DnetBlazor stylesheet as the library styling bundle. New code uses `--dnet-sys-*`
and documented component tokens. Legacy names remain fallback links through 6.x;
migrate overrides before 7.0.

## Stylesheet distribution

The library publishes one base stylesheet:

```html
<link href="_content/Dnet.Blazor/dnet-blazor-styles.css" rel="stylesheet" />
```

`PickList` is part of that bundle. Applications no longer need their generated
`{App}.styles.css` file to receive library component styles.

## Runtime themes

Load any theme files that the application exposes to users after the base
stylesheet. They can be selected without rebuilding the application:

```html
<link href="_content/Dnet.Blazor/styles/theme/dark.css" rel="stylesheet" />
<link href="_content/Dnet.Blazor/styles/theme/compact.css" rel="stylesheet" />
<link href="_content/Dnet.Blazor/styles/theme/high-contrast.css" rel="stylesheet" />
```

```js
document.documentElement.dataset.dnetTheme = "dark";
```

Use `DnetThemeScope` when a theme applies to only part of a Razor tree. It also
transports the theme name to component-owned overlays. For overlays opened via
services, set `ThemeScope` and optional `PanelStyle` in the service
configuration; arbitrary inherited custom properties cannot cross the portal.

## Token migration

Prefer semantic roles for application-wide policy and component tokens for a
single control. For example:

```css
:root {
	--dnet-sys-primary: #0f6cbd;
	--dnet-btn-radius: 0;
}
```

The stylesheet still accepts documented legacy component names where a migrated
component exposes a fallback chain. Treat those names as compatibility-only:
keep existing overrides during 6.x, move new overrides to the documented public
tokens, and remove legacy names before upgrading to 7.0.