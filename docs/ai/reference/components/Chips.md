# Chips

## `<DnetChip>`

```razor
<DnetChip
    Selected="..."
    Disabled="..."
/>
```

| Parameter | Type | Default | Description |
|---|---|---|---|
| `OnSelectionChange` | `EventCallback<string>` | — | Raised when the selection changes. |
| `OnRemoved` | `EventCallback<DnetChip>` | — | Raised when removed occurs. |
| `ChildContent` | `RenderFragment?` | — | Gets or sets the child content rendered by the component. |
| `BackgroungColor` | `string?` | — | Gets or sets the backgroung color used by this component. |
| `Color` | `string?` | — | Gets or sets the color used by this component. |
| `BackgroungColorSelected` | `string?` | — | Gets or sets the backgroung color selected used by this component. |
| `ColorSelected` | `string?` | — | Gets or sets the color selected used by this component. |
| `Selectable` | `bool` | `true` | Gets or sets the selectable used by this component. |
| `Selected` | `bool` | — | Gets or sets the selected used by this component. |
| `Removable` | `bool` | `true` | Gets or sets the removable used by this component. |
| `Value` | `string?` | — | Gets or sets the component value. |
| `ChipSize` | `ChipSize` | `ChipSize.Small` | Gets or sets the chip size used by this component. |
| `Disabled` | `bool` | — | Gets or sets whether user interaction with the component is disabled. |
| `RemoveIcon` | `string?` | — | Gets or sets the remove icon used by this component. |
| `AvatarIcon` | `string?` | — | Gets or sets the avatar icon used by this component. |

## `<DnetChipList>`

```razor
<DnetChipList
    Multi="..."
/>
```

| Parameter | Type | Default | Description |
|---|---|---|---|
| `OnSelectionChange` | `EventCallback<List<DnetChip>>` | — | Raised when the selection changes. |
| `ChildContent` | `RenderFragment?` | — | Gets or sets the child content rendered by the component. |
| `Multi` | `bool` | — | Gets or sets the multi used by this component. |

## Styling tokens

Set any of these above the component in the DOM — `:root`, a container, or
the element's own `style`. Nothing else is needed.

| Token | Effective default |
|---|---|
| `--dnet-chips-background` | `#e0e0e0` <br><sub>via `--dnet-sys-surface-variant`</sub> |
| `--dnet-chips-background-hover` | `#f2f2f2` <br><sub>via `--dnet-sys-surface-hover`</sub> |
| `--dnet-chips-background-selected` | `#3f51b5` <br><sub>via `--dnet-sys-primary-emphasis`</sub> |
| `--dnet-chips-font-size` | `0.875rem` <br><sub>via `--dnet-sys-text-md`</sub> |
| `--dnet-chips-foreground` | `color-mix(in srgb, var(--dnet-ref-neutral-1000) 87%, transparent)` <br><sub>via `--dnet-sys-on-surface-emphasis`</sub> |
| `--dnet-chips-foreground-selected` | `#ffffff` <br><sub>via `--dnet-sys-on-primary`</sub> |
| `--dnet-chips-icon-color` | `#5f6368` <br><sub>via `--dnet-sys-on-surface`</sub> |
| `--dnet-chips-icon-size` | `20px` |
| `--dnet-chips-min-height` | `32px` |
| `--dnet-chips-padding` | `7px 12px` |
| `--dnet-chips-radius` | `4px` <br><sub>via `--dnet-sys-radius-sm`</sub> |

<details><summary>Legacy token names still honoured</summary>

Kept as intermediate links in the fallback chains so 5.x overrides keep
working. Prefer the names above for new code; these go away in 7.0.

`--dnet-chips-border-radius`, `--dnet-chips-sm-font-size`

</details>

```css
:root { --dnet-chips-background: /* your value */; }
```
