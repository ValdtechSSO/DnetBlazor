# Chips

Components: `<DnetChip>`, `<DnetChipList>`

## `<DnetChip>`

| Parameter | Type | Default |
|---|---|---|
| `OnSelectionChange` | `EventCallback<string>` | — |
| `OnRemoved` | `EventCallback<DnetChip>` | — |
| `ChildContent` | `RenderFragment?` | — |
| `BackgroungColor` | `string?` | — |
| `Color` | `string?` | — |
| `BackgroungColorSelected` | `string?` | — |
| `ColorSelected` | `string?` | — |
| `Selectable` | `bool` | `true` |
| `Selected` | `bool` | — |
| `Removable` | `bool` | `true` |
| `Value` | `string?` | — |
| `ChipSize` | `ChipSize` | `ChipSize.Small` |
| `Disabled` | `bool` | — |
| `RemoveIcon` | `string?` | — |
| `AvatarIcon` | `string?` | — |

## `<DnetChipList>`

| Parameter | Type | Default |
|---|---|---|
| `OnSelectionChange` | `EventCallback<List<DnetChip>>` | — |
| `ChildContent` | `RenderFragment?` | — |
| `Multi` | `bool` | — |

## Minimal usage

```razor
<DnetChip
    BackgroungColor="..."
    Color="..."
    BackgroungColorSelected="..."
/>
```

## Styling tokens

Override these anywhere in the DOM above the component — `:root`, a
container, or the element's own `style`. Nothing else is needed.

| Token | Falls back to |
|---|---|
| `--dnet-chips-background` | `var(--dnet-sys-surface-variant)` |
| `--dnet-chips-background-hover` | — |
| `--dnet-chips-background-selected` | `var(--dnet-sys-primary-emphasis)` |
| `--dnet-chips-border-radius` | — |
| `--dnet-chips-font-size` | `var(--dnet-chips-sm-font-size, var(--dnet-sys-text-md))` |
| `--dnet-chips-foreground` | `var(--dnet-sys-on-surface-emphasis)` |
| `--dnet-chips-foreground-selected` | `var(--dnet-sys-on-primary)` |
| `--dnet-chips-icon-color` | `var(--dnet-sys-on-surface)` |
| `--dnet-chips-icon-size` | `20px` |
| `--dnet-chips-min-height` | `32px` |
| `--dnet-chips-padding` | `7px 12px` |
| `--dnet-chips-radius` | `var(--dnet-chips-border-radius, var(--dnet-sys-radius-sm))` |
| `--dnet-chips-sm-font-size` | — |

```css
:root { --dnet-chips-background: /* your value */; }
```
