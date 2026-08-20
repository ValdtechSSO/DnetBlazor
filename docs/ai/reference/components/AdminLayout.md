# AdminLayout

Components: `<DesktopFooter>`, `<DesktopHeader>`, `<DesktopLayout>`, `<DesktopLeftColumn>`, `<DesktopMinifyMenu>`, `<DesktopNavigation>`, `<DesktopNavigationMenu>`, `<DesktopRightColumn>`, `<MenuTree>`, `<MenuTreeLink>`, `<MenuTreeLinkHelper>`

## `<DesktopFooter>`

| Parameter | Type | Default |
|---|---|---|
| `ChildContent` | `RenderFragment?` | — |

## `<DesktopHeader>`

| Parameter | Type | Default |
|---|---|---|
| `ChildContent` | `RenderFragment?` | — |

## `<DesktopLayout>`

| Parameter | Type | Default |
|---|---|---|
| `ChildContent` | `RenderFragment?` | — |
| `IsHeaderFixed` | `bool` | — |
| `IsLeftColumnFixed` | `bool` | — |
| `ShowMinifier` | `bool` | — |
| `IsMinified` | `bool` | — |
| `IsHeaderHidden` | `bool` | — |
| `IsFooterHidden` | `bool` | — |
| `IsLeftColumnHidden` | `bool` | — |
| `IsRightColumnHidden` | `bool` | — |
| `IsDesktopMode` | `bool` | — |

## `<DesktopLeftColumn>`

| Parameter | Type | Default |
|---|---|---|
| `ChildContent` | `RenderFragment?` | — |

## `<DesktopMinifyMenu>`

| Parameter | Type | Default |
|---|---|---|
| `AllowMinified` | `bool` | — |

## `<DesktopNavigation>`

| Parameter | Type | Default |
|---|---|---|
| `CompanyChildContent` | `RenderFragment?` | — |
| `MenuChildContent` | `RenderFragment?` | — |
| `MinifierChildContent` | `RenderFragment?` | — |

## `<DesktopNavigationMenu>`

| Parameter | Type | Default |
|---|---|---|
| `Menus` | `List<NavigationMenu>` | `new()` |

## `<DesktopRightColumn>`

| Parameter | Type | Default |
|---|---|---|
| `ChildContent` | `RenderFragment?` | — |

## `<MenuTree>`

| Parameter | Type | Default |
|---|---|---|
| `Menu` | `NavigationMenu?` | — |

## `<MenuTreeLink>`

| Parameter | Type | Default |
|---|---|---|
| `Menu` | `NavigationMenu?` | — |
| `OnLinkNodeClick` | `EventCallback<bool>` | — |

## `<MenuTreeLinkHelper>`

| Parameter | Type | Default |
|---|---|---|
| `Menu` | `NavigationMenu?` | — |
| `OnLinkNodeClick` | `EventCallback<bool>` | — |

## Minimal usage

```razor
<DesktopFooter />
```

## Styling tokens

Override these anywhere in the DOM above the component — `:root`, a
container, or the element's own `style`. Nothing else is needed.

| Token | Falls back to |
|---|---|
| `--dnet-dash-aside` | — |
| `--dnet-dash-aside-width` | — |
| `--dnet-dash-aside-width-minified` | — |
| `--dnet-dash-body-background-color` | — |
| `--dnet-dash-default-font-color` | — |
| `--dnet-dash-floating-menu-left` | — |
| `--dnet-dash-font-family-` | — |
| `--dnet-dash-font-family-headline` | — |
| `--dnet-dash-font-family-text` | — |
| `--dnet-dash-font-rem-reference` | — |
| `--dnet-dash-footer-height` | — |
| `--dnet-dash-gray-dark` | — |
| `--dnet-dash-header-height` | — |
| `--dnet-dash-html-background-color` | — |
| `--dnet-dash-indicator-bgcolor` | — |
| `--dnet-dash-indicator-width` | — |
| `--dnet-dash-left-column-width` | — |
| `--dnet-dash-link-color` | — |
| `--dnet-dash-menu-item-parent-color` | — |
| `--dnet-dash-menu-left-padding-firts-level` | — |
| `--dnet-dash-menu-left-padding-second-level` | — |
| `--dnet-dash-menu-left-padding-third-level` | — |
| `--dnet-dash-menu-text-size` | — |
| `--dnet-dash-minifyme-bg-color` | — |
| `--dnet-dash-minifyme-foreground` | — |
| `--dnet-dash-navbar-height` | — |
| `--dnet-dash-navigation-color` | — |
| `--dnet-dash-positioning-helper-color` | — |
| `--dnet-dash-primary-dark` | — |
| `--dnet-dash-primary-reset` | — |
| `--dnet-dash-red` | — |
| `--dnet-dash-right-column-width` | — |
| `--dnet-dash-scrollbar-background-color` | — |
| `--dnet-dash-scrollbar-foreground-color` | — |
| `--dnet-dash-scrollbar-size` | — |
| `--dnet-footer-border-color` | — |
| `--dnet-header-border-color` | — |
| `--dnet-layout-divider-color` | — |
| `--dnet-layout-divider-style` | `solid` |
| `--dnet-layout-divider-width` | `0` |
| `--dnet-left-column-border-color` | — |
| `--dnet-menu-container-bgcolor` | — |
| `--dnet-menu-container-open-bgcolor` | — |
| `--dnet-right-column-border-color` | — |

```css
:root { --dnet-dash-aside: /* your value */; }
```
