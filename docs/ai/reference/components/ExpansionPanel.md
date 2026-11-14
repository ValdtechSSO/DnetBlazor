# ExpansionPanel

## `<DnetExpansionPanelItem>`

```razor
<DnetExpansionPanelItem
/>
```

| Parameter | Type | Default | Description |
|---|---|---|---|
| `ChildContent` | `RenderFragment?` | — | Gets or sets the child content rendered by the component. |
| `ItemHeaderTemplate` | `RenderFragment?` | — | Gets or sets the template used to render item header. |
| `Title` | `string?` | — | Gets or sets the title displayed by the component. |
| `TitleContent` | `RenderFragment?` | — | Gets or sets content rendered for title. |
| `Description` | `string?` | — | Gets or sets the description used by this component. |
| `DescriptionContent` | `RenderFragment?` | — | Gets or sets content rendered for description. |
| `Order` | `int` | `0` | Gets or sets the order used by this component. |
| `Disabled` | `bool` | `false` | Gets or sets whether user interaction with the component is disabled. |
| `HideToggle` | `bool` | `false` | Gets or sets whether the component hide toggle. |
| `AccordionShadow` | `bool` | `true` | Gets or sets the accordion shadow used by this component. |

## Styling tokens

Set any of these above the component in the DOM — `:root`, a container, or
the element's own `style`. Nothing else is needed.

| Token | Effective default |
|---|---|
| `--dnet-accordion-body-padding` | `0 24px 16px` |
| `--dnet-accordion-border-radius` | `4px` <br><sub>via `--dnet-sys-radius-sm`</sub> |
| `--dnet-accordion-box-shadow` | `0 2px 1px -1px var(--dnet-sys-shadow-color), 0 1px 2px 0 var(--dnet-sys-shadow-color), 0 1px 10px 0 var(--dnet-sys-shadow-color)` <br><sub>via `--dnet-sys-elevation-1`</sub> |
| `--dnet-accordion-box-shadow-expanded` | `var(--_shadow)` |
| `--dnet-accordion-chevron-offset` | `0px` |
| `--dnet-accordion-chevron-rotate-expanded` | `none` |
| `--dnet-accordion-description-font-size` | `0.625rem` |
| `--dnet-accordion-gap` | `0px` |
| `--dnet-accordion-header-height` | `48px` |
| `--dnet-accordion-header-hover-background` | `transparent` <br><sub>via `--dnet-sys-transparent`</sub> |
| `--dnet-accordion-header-padding` | `0 24px` |
| `--dnet-accordion-indicator-background` | `transparent` <br><sub>via `--dnet-sys-transparent`</sub> |
| `--dnet-accordion-indicator-background-expanded` | `var(--_indicator-background)` |
| `--dnet-accordion-indicator-color` | `#757575` <br><sub>via `--dnet-sys-on-surface-subtle`</sub> |
| `--dnet-accordion-indicator-color-expanded` | `var(--_indicator-color)` |
| `--dnet-accordion-indicator-disabled-opacity` | `1` |
| `--dnet-accordion-indicator-focus-outline` | `none` |
| `--dnet-accordion-indicator-padding` | `0px` |
| `--dnet-accordion-indicator-radius` | `0px` |
| `--dnet-accordion-indicator-rotate-expanded` | `rotate(180deg)` |
| `--dnet-accordion-indicator-shadow` | `none` |
| `--dnet-accordion-indicator-shadow-expanded` | `var(--_indicator-shadow)` |
| `--dnet-accordion-indicator-transition` | `none` |
| `--dnet-accordion-joined-radius` | `0px` |
| `--dnet-accordion-title-font-size` | `0.875rem` <br><sub>via `--dnet-sys-text-md`</sub> |
| `--dnet-accordion-title-font-weight` | `400` |

```css
:root { --dnet-accordion-border-radius: /* your value */; }
```
