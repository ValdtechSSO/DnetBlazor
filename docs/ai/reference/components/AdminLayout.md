# AdminLayout

## `<DesktopFooter>`

```razor
<DesktopFooter
/>
```

| Parameter | Type | Default | Description |
|---|---|---|---|
| `ChildContent` | `RenderFragment?` | — | Gets or sets the child content rendered by the component. |

## `<DesktopHeader>`

```razor
<DesktopHeader
/>
```

| Parameter | Type | Default | Description |
|---|---|---|---|
| `ChildContent` | `RenderFragment?` | — | Gets or sets the child content rendered by the component. |

## `<DesktopLayout>`

```razor
<DesktopLayout
    IsHeaderFixed="..."
    IsLeftColumnFixed="..."
    ShowMinifier="..."
    IsMinified="..."
/>
```

| Parameter | Type | Default | Description |
|---|---|---|---|
| `ChildContent` | `RenderFragment?` | — | Gets or sets the child content rendered by the component. |
| `AdditionalAttributes` | `IReadOnlyDictionary<string, object>?` | — | Gets or sets unmatched HTML attributes applied to the rendered element. |
| `IsHeaderFixed` | `bool` | — | Gets or sets whether header fixed. |
| `IsLeftColumnFixed` | `bool` | — | Gets or sets whether left column fixed. |
| `ShowMinifier` | `bool` | — | Gets or sets whether the component show minifier. |
| `IsMinified` | `bool` | — | Gets or sets whether minified. |
| `IsHeaderHidden` | `bool` | — | Gets or sets whether header hidden. |
| `IsFooterHidden` | `bool` | — | Gets or sets whether footer hidden. |
| `IsLeftColumnHidden` | `bool` | — | Gets or sets whether left column hidden. |
| `IsRightColumnHidden` | `bool` | — | Gets or sets whether right column hidden. |
| `IsDesktopMode` | `bool` | — | Gets or sets whether desktop mode. |

## `<DesktopLeftColumn>`

```razor
<DesktopLeftColumn
/>
```

| Parameter | Type | Default | Description |
|---|---|---|---|
| `ChildContent` | `RenderFragment?` | — | Gets or sets the child content rendered by the component. |

## `<DesktopMinifyMenu>`

```razor
<DesktopMinifyMenu
    AllowMinified="..."
/>
```

| Parameter | Type | Default | Description |
|---|---|---|---|
| `AllowMinified` | `bool` | — | Gets or sets whether the component allow minified. |

## `<DesktopNavigation>`

```razor
<DesktopNavigation
/>
```

| Parameter | Type | Default | Description |
|---|---|---|---|
| `CompanyChildContent` | `RenderFragment?` | — | Gets or sets content rendered for company child. |
| `MenuChildContent` | `RenderFragment?` | — | Gets or sets content rendered for menu child. |
| `MinifierChildContent` | `RenderFragment?` | — | Gets or sets content rendered for minifier child. |

## `<DesktopNavigationMenu>`

```razor
<DesktopNavigationMenu
/>
```

| Parameter | Type | Default | Description |
|---|---|---|---|
| `Menus` | `List<NavigationMenu>` | `new()` | Gets or sets the menus used by this component. |

## `<DesktopRightColumn>`

```razor
<DesktopRightColumn
/>
```

| Parameter | Type | Default | Description |
|---|---|---|---|
| `ChildContent` | `RenderFragment?` | — | Gets or sets the child content rendered by the component. |

## `<MenuTree>`

```razor
<MenuTree
/>
```

| Parameter | Type | Default | Description |
|---|---|---|---|
| `Menu` | `NavigationMenu?` | — | Gets or sets the menu used by this component. |

## `<MenuTreeLink>`

```razor
<MenuTreeLink
/>
```

| Parameter | Type | Default | Description |
|---|---|---|---|
| `AdditionalAttributes` | `Dictionary<string, object>` | `new()` | Gets or sets unmatched HTML attributes applied to the rendered element. |
| `Menu` | `NavigationMenu?` | — | Gets or sets the menu used by this component. |
| `OnLinkNodeClick` | `EventCallback<bool>` | — | Raised when link node click occurs. |

## `<MenuTreeLinkHelper>`

```razor
<MenuTreeLinkHelper
/>
```

| Parameter | Type | Default | Description |
|---|---|---|---|
| `AdditionalAttributes` | `Dictionary<string, object>?` | — | Gets or sets unmatched HTML attributes applied to the rendered element. |
| `Menu` | `NavigationMenu?` | — | Gets or sets the menu used by this component. |
| `OnLinkNodeClick` | `EventCallback<bool>` | — | Raised when link node click occurs. |

## Styling tokens

Set any of these above the component in the DOM — `:root`, a container, or
the element's own `style`. Nothing else is needed.

| Token | Effective default |
|---|---|
| `--dnet-dash-aside` | — |
| `--dnet-dash-aside-width` | `250px` |
| `--dnet-dash-aside-width-minified` | `60px` |
| `--dnet-dash-body-background-color` | `#ffffff` <br><sub>via `--dnet-sys-surface`</sub> |
| `--dnet-dash-default-font-color` | `#5f6368` <br><sub>via `--dnet-sys-on-surface`</sub> |
| `--dnet-dash-floating-menu-left` | `60px` |
| `--dnet-dash-font-family-` | — |
| `--dnet-dash-font-family-headline` | `"Roboto", "Helvetica Neue", Helvetica, Arial, sans-serif` <br><sub>via `--dnet-sys-font`</sub> |
| `--dnet-dash-font-family-text` | `"Roboto", "Helvetica Neue", Helvetica, Arial, sans-serif` <br><sub>via `--dnet-sys-font`</sub> |
| `--dnet-dash-font-rem-reference` | `16px` |
| `--dnet-dash-footer-height` | `50px` |
| `--dnet-dash-gray-dark` | `color-mix(in srgb, var(--dnet-ref-neutral-1000) 87%, transparent)` <br><sub>via `--dnet-sys-on-surface-emphasis`</sub> |
| `--dnet-dash-header-height` | `50px` |
| `--dnet-dash-html-background-color` | `#ffffff` <br><sub>via `--dnet-sys-surface`</sub> |
| `--dnet-dash-indicator-bgcolor` | `#42b0d5` <br><sub>via `--dnet-sys-primary-strong`</sub> |
| `--dnet-dash-indicator-width` | `3px` |
| `--dnet-dash-left-column-width` | `250px` |
| `--dnet-dash-link-color` | `color-mix(in srgb, var(--dnet-ref-neutral-1000) 87%, transparent)` <br><sub>via `--dnet-sys-on-surface-emphasis`</sub> |
| `--dnet-dash-menu-item-parent-color` | `#ffffff` <br><sub>via `--dnet-sys-surface`</sub> |
| `--dnet-dash-menu-left-padding-firts-level` | `1em` |
| `--dnet-dash-menu-left-padding-second-level` | `2em` |
| `--dnet-dash-menu-left-padding-third-level` | `4em` |
| `--dnet-dash-menu-text-size` | `0.875rem` <br><sub>via `--dnet-sys-text-md`</sub> |
| `--dnet-dash-minifyme-bg-color` | `#ebebeb` <br><sub>via `--dnet-sys-border`</sub> |
| `--dnet-dash-minifyme-foreground` | `#5f6368` <br><sub>via `--dnet-sys-on-surface`</sub> |
| `--dnet-dash-navbar-height` | `38px` |
| `--dnet-dash-positioning-helper-color` | `color-mix(in srgb, var(--dnet-ref-neutral-1000) 87%, transparent)` <br><sub>via `--dnet-sys-on-surface-emphasis`</sub> |
| `--dnet-dash-primary-dark` | `color-mix(in srgb, var(--dnet-ref-neutral-1000) 87%, transparent)` <br><sub>via `--dnet-sys-on-surface-emphasis`</sub> |
| `--dnet-dash-primary-reset` | `#42b0d5` <br><sub>via `--dnet-sys-primary-strong`</sub> |
| `--dnet-dash-red` | `#b80012` <br><sub>via `--dnet-sys-danger`</sub> |
| `--dnet-dash-right-column-width` | `250px` |
| `--dnet-dash-scrollbar-background-color` | `transparent` <br><sub>via `--dnet-sys-transparent`</sub> |
| `--dnet-dash-scrollbar-foreground-color` | `#ebebeb` <br><sub>via `--dnet-sys-border`</sub> |
| `--dnet-dash-scrollbar-size` | `.4rem` |
| `--dnet-footer-border-color` | `#e1e3e1` <br><sub>via `--dnet-sys-border-strong`</sub> |
| `--dnet-header-border-color` | `#e1e3e1` <br><sub>via `--dnet-sys-border-strong`</sub> |
| `--dnet-layout-divider-style` | `solid` |
| `--dnet-layout-divider-width` | `0` |
| `--dnet-left-column-border-color` | `#e1e3e1` <br><sub>via `--dnet-sys-border-strong`</sub> |
| `--dnet-menu-container-bgcolor` | `#ebebeb` <br><sub>via `--dnet-sys-border`</sub> |
| `--dnet-menu-container-open-bgcolor` | `#ffffff` <br><sub>via `--dnet-sys-surface`</sub> |
| `--dnet-right-column-border-color` | `#e1e3e1` <br><sub>via `--dnet-sys-border-strong`</sub> |

<details><summary>Legacy token names still honoured</summary>

Kept as intermediate links in the fallback chains so 5.x overrides keep
working. Prefer the names above for new code; these go away in 7.0.

`--dnet-dash-asideWidth`, `--dnet-dash-asideWidth-minified`, `--dnet-dash-font-family-Headline`, `--dnet-dash-navigation-color`, `--dnet-layout-divider-color`

</details>

```css
:root { --dnet-dash-body-background-color: /* your value */; }
```
