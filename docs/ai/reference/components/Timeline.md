# Timeline

## `<DnetTimeline>`

```razor
<DnetTimeline
/>
```

| Parameter | Type | Default | Description |
|---|---|---|---|
| `Alignment` | `DnetTimelineAlignment` | `DnetTimelineAlignment.Start` | Gets or sets the position of the rail. Start and End follow the text direction. |
| `AriaLabel` | `string?` | Computed | Gets or sets the accessible name of the ordered list. |
| `ChildContent` | `RenderFragment?` | — | Gets or sets the DnetTimelineItem components in chronological or process order. |
| `AdditionalAttributes` | `IReadOnlyDictionary<string, object>?` | — | Gets or sets unmatched attributes, including class, style, id and dir, on the ordered list. |

## `<DnetTimelineItem>`

```razor
<DnetTimelineItem
/>
```

| Parameter | Type | Default | Description |
|---|---|---|---|
| `Title` | `string?` | — | Gets or sets the event title. |
| `Timestamp` | `string?` | — | Gets or sets a human-readable date or time shown after the event content. |
| `DateTime` | `string?` | — | Gets or sets an optional ISO date or time. When provided, Timestamp renders as a time element. |
| `Status` | `DnetTimelineItemStatus` | `DnetTimelineItemStatus.Pending` | Gets or sets the explicit event status. It does not depend on sibling order. |
| `StatusLabel` | `string?` | — | Gets or sets the localized status announced to assistive technology. Defaults to the status name. |
| `BulletContent` | `RenderFragment?` | — | Gets or sets a decorative custom bullet. Put meaningful information in the title or content. |
| `OppositeContent` | `RenderFragment?` | — | Gets or sets secondary metadata opposite alternating events, or below other layouts. |
| `ChildContent` | `RenderFragment?` | — | Gets or sets the rich event content, including links or other controls. |
| `AdditionalAttributes` | `IReadOnlyDictionary<string, object>?` | — | Gets or sets unmatched attributes, including class, style and id, on this list item. |

## Styling tokens

Set any of these above the component in the DOM — `:root`, a container, or
the element's own `style`. Nothing else is needed.

| Token | Effective default |
|---|---|
| `--dnet-timeline-accent-color` | `color-mix(in srgb, var(--dnet-sys-primary) 80%, var(--dnet-ref-neutral-1000))` <br><sub>via `--dnet-sys-primary-emphasis`</sub> |
| `--dnet-timeline-accent-foreground` | `#ffffff` <br><sub>via `--dnet-sys-on-primary`</sub> |
| `--dnet-timeline-bullet-background` | `#ffffff` <br><sub>via `--dnet-sys-surface`</sub> |
| `--dnet-timeline-bullet-size` | `calc(var(--dnet-sys-space-unit) * 6)` |
| `--dnet-timeline-content-gap` | `calc(var(--dnet-sys-space-unit) * 4)` <br><sub>via `--dnet-sys-space-4`</sub> |
| `--dnet-timeline-font-size` | `0.875rem` <br><sub>via `--dnet-sys-text-md`</sub> |
| `--dnet-timeline-foreground` | `#5f6368` <br><sub>via `--dnet-sys-on-surface`</sub> |
| `--dnet-timeline-gap` | `calc(var(--dnet-sys-space-unit) * 7)` |
| `--dnet-timeline-line-color` | `#e1e3e1` <br><sub>via `--dnet-sys-border-strong`</sub> |
| `--dnet-timeline-line-width` | `2px` |
| `--dnet-timeline-muted-foreground` | `#666666` <br><sub>via `--dnet-sys-on-surface-muted`</sub> |
| `--dnet-timeline-title-font-size` | `1rem` <br><sub>via `--dnet-sys-text-lg`</sub> |

```css
:root { --dnet-timeline-accent-color: /* your value */; }
```
