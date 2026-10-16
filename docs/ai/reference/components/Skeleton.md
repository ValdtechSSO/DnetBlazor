# Skeleton

## `<DnetSkeleton>`

```razor
<DnetSkeleton
    Circle="..."
/>
```

| Parameter | Type | Default | Description |
|---|---|---|---|
| `Visible` | `bool` | `true` | Gets or sets whether the loading placeholder covers the content. |
| `Animate` | `bool` | `true` | Gets or sets whether the placeholder pulses. Reduced-motion preferences always disable the animation. |
| `Circle` | `bool` | — | Gets or sets whether the placeholder is circular. Height determines its size; percentages use the container width. Width is ignored. |
| `Width` | `string?` | — | Gets or sets an optional CSS width. When omitted, the width token controls the size. |
| `Height` | `string?` | — | Gets or sets an optional CSS height, or the reference size of a circle. When omitted, the height token controls the size. |
| `ChildContent` | `RenderFragment?` | — | Gets or sets optional content. It stays mounted and preserves its layout while hidden and inert during loading. |
| `AdditionalAttributes` | `IReadOnlyDictionary<string, object>?` | — | Gets or sets unmatched HTML attributes, including class, style, id and dir, on the component root. |

## Styling tokens

Set any of these above the component in the DOM — `:root`, a container, or
the element's own `style`. Nothing else is needed.

| Token | Effective default |
|---|---|
| `--dnet-skeleton-animation-duration` | `1.5s` |
| `--dnet-skeleton-background` | `color-mix(in srgb, var(--dnet-sys-on-surface) 12%, var(--dnet-sys-surface))` |
| `--dnet-skeleton-border-radius` | `4px` <br><sub>via `--dnet-sys-radius-sm`</sub> |
| `--dnet-skeleton-height` | `1rem` |
| `--dnet-skeleton-highlight` | `color-mix(in srgb, var(--dnet-sys-on-surface) 20%, var(--dnet-sys-surface))` |
| `--dnet-skeleton-width` | `100%` |

```css
:root { --dnet-skeleton-background: /* your value */; }
```
