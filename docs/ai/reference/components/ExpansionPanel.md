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
| `--dnet-accordion-border-radius` | `4px` <br><sub>via `--dnet-sys-radius-sm`</sub> |
| `--dnet-accordion-box-shadow` | `0 2px 1px -1px var(--dnet-sys-shadow-color), 0 1px 2px 0 var(--dnet-sys-shadow-color), 0 1px 10px 0 var(--dnet-sys-shadow-color)` <br><sub>via `--dnet-sys-elevation-1`</sub> |
| `--dnet-accordion-title-font-size` | `0.875rem` <br><sub>via `--dnet-sys-text-md`</sub> |

```css
:root { --dnet-accordion-border-radius: /* your value */; }
```
