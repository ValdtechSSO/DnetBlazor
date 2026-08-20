# ExpansionPanel

Components: `<DnetExpansionPanelItem>`

## `<DnetExpansionPanelItem>`

| Parameter | Type | Default |
|---|---|---|
| `ChildContent` | `RenderFragment?` | — |
| `ItemHeaderTemplate` | `RenderFragment?` | — |
| `Title` | `string?` | — |
| `TitleContent` | `RenderFragment?` | — |
| `Description` | `string?` | — |
| `DescriptionContent` | `RenderFragment?` | — |
| `Order` | `int` | `0` |
| `Disabled` | `bool` | `false` |
| `HideToggle` | `bool` | `false` |
| `AccordionShadow` | `bool` | `true` |

## Minimal usage

```razor
<DnetExpansionPanelItem
    Title="..."
    Description="..."
/>
```

## Styling tokens

Override these anywhere in the DOM above the component — `:root`, a
container, or the element's own `style`. Nothing else is needed.

| Token | Falls back to |
|---|---|
| `--dnet-accordion-border-radius` | — |
| `--dnet-accordion-box-shadow` | — |
| `--dnet-accordion-title-font-size` | — |

```css
:root { --dnet-accordion-border-radius: /* your value */; }
```
