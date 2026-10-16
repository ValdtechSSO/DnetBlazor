# DnetSkeleton

`DnetSkeleton` displays a loading placeholder, either by itself or over existing
content. Import `Dnet.Blazor.Components.Skeleton`.

```razor
@using Dnet.Blazor.Components.Skeleton

<DnetSkeleton Circle="true" Height="48px" />
<DnetSkeleton Height="12px" Width="70%" />

<DnetSkeleton Visible="_loading">
    <ProfileEditor Model="_profile" />
</DnetSkeleton>
```

The component needs no JavaScript. When wrapping content, its height follows the
content unless you explicitly set `Height` or the height token. Child components
stay mounted throughout loading changes, so their local state is preserved.

| Parameter | Default | Behavior |
| --- | --- | --- |
| `Visible` | `true` | Displays the placeholder and hides wrapped content. |
| `Animate` | `true` | Pulses between the background and highlight colors. |
| `Circle` | `false` | Uses `Height` as a reference size, maintains a square aspect ratio and a circular radius; ignores `Width`. |
| `Width` | `null` | Optional CSS width, such as `70%`, `12rem` or `120px`. |
| `Height` | `null` | Optional CSS height. Standalone and circular placeholders default to `1rem`. |
| `ChildContent` | `null` | Optional content kept mounted behind the placeholder. |
| `AdditionalAttributes` | `null` | HTML attributes, including `id`, `class`, `style` and `dir`, applied to the root. |

Use explicit dimensions for avatar or image placeholders. For circles, `Height`
is the reference size for both dimensions even when `Width` is provided.
Percentage values resolve against the container's width: `Height="50%"` in a
128px-wide container creates a 64px circle. Wrapped content keeps this geometry
when revealed; provide appropriately sized avatar content. Loading is controlled
by the parent; the component does not fetch data or emit loading changes.

## Accessibility

Standalone placeholders are decorative and have `aria-hidden="true"`. If a
loading announcement is useful, put one status message on the surrounding
section rather than announcing every placeholder.

Wrapped content sets `aria-busy` on the root. During loading, the content wrapper
is hidden visually, excluded from the accessibility tree and marked `inert`, so
its inputs, links and buttons cannot receive focus or pointer interaction.
These restrictions are removed when `Visible` becomes `false`. Descendant
components retain their own disabled or inert state.

If loading starts while focus is inside the content, the browser removes that
focus when applying `inert`. Applications that need a specific focus destination
should move focus to an appropriate control before changing `Visible`. The
component does not restore focus automatically after loading.

Animations stop automatically when the user requests reduced motion. Set
`Animate="false"` to disable them for an individual placeholder. Logical sizing
supports RTL without changing the appearance.

## Styling

Public tokens inherit from `:root`, an intermediate container or an individual
instance. The component only declares private properties; nullable geometry
parameters leave these public defaults available. Explicit `Width` and `Height`
parameters take precedence over their corresponding tokens.

| Public token | Default |
| --- | --- |
| `--dnet-skeleton-background` | 12% `--dnet-sys-on-surface` mixed with the current surface |
| `--dnet-skeleton-highlight` | 20% `--dnet-sys-on-surface` mixed with the current surface |
| `--dnet-skeleton-border-radius` | `--dnet-sys-radius-sm` |
| `--dnet-skeleton-width` | `100%` |
| `--dnet-skeleton-height` | `1rem`, or `auto` for noncircular wrapped content |
| `--dnet-skeleton-animation-duration` | `1.5s` |

The semantic color defaults adapt to scoped dark themes without separate rules.
The radius token does not alter circles. Without wrapped content, setting
`Visible="false"` leaves an empty element with the requested dimensions.

See the `/Skeleton` sample for profile placeholders, controlled loading,
preserved input state and static RTL examples.
