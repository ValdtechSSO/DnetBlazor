# DnetPillTabs

Rounded horizontal tabs with optional counters, keyboard navigation and optional
content panels. Import `Dnet.Blazor.Components.PillTabs` and load the standard
library CSS and JavaScript bundles. The runnable example is at `/PillTabs` in
the sample application.

```razor
@using Dnet.Blazor.Components.PillTabs

<DnetPillTabs Items="_tabs" @bind-Value="_status" AriaLabel="Estado">
    <ChildContent Context="tab">
        <p>Contenido de @tab.Label</p>
    </ChildContent>
</DnetPillTabs>

@code {
    private string _status = "pending";
    private readonly IReadOnlyList<DnetPillTabItem<string>> _tabs =
    [
        new("pending", "Pendientes", 1),
        new("approved", "Aprobados", 2),
        new("all", "Todos", 3)
    ];
}
```

Each item has a unique, non-null `Value`, a `Label`, an optional integer `Count`
and a `Disabled` flag. A null count hides the badge; zero remains visible.
`ItemTemplate` can replace the label, and the count is still rendered separately.
Avoid interactive elements inside the label template, since the tab is a button.

Omit `Value` to let the component own selection: it starts with the first
enabled item, preserves the selected value across item reordering and count
updates, and falls back to the first enabled item if the selected item disappears
or becomes disabled. Those collection updates do not emit `ValueChanged`.

Supplying `Value` makes selection controlled. Use `@bind-Value`, or handle
`ValueChanged` and pass the accepted value back. The component does not change
the displayed selection until the parent accepts it. An unknown or disabled
controlled value leaves no selected tab; the first enabled tab remains reachable
by keyboard. Clicking the current tab does not emit another change.

`ChildContent` receives the active item. Only the selected panel's content is
mounted; changing tabs recreates its content. Omit `ChildContent` to use the
header as a selector and handle the selected value in the surrounding page.
This header-only mode exposes a named button group with `aria-pressed`; with
content, the component exposes a tab list and associated tab panels.

Set `Disabled` to disable the whole control. Set `FullWidth` to stretch the list
and distribute its available width evenly. Long lists scroll horizontally inside
their container without a visible scrollbar. A fade at the left, right or both
edges indicates hidden options, including in RTL. The fades update while
scrolling, resizing and changing items or counts, and disappear when all items
fit. Touch, trackpad and keyboard scrolling remain available; changing selection
reveals the selected option. Unmatched attributes such as `class`, `style`, `id`, `dir` and
`data-*` are applied to the root.

The selected tab is the tab list's single tab stop. Left/Right wrap around enabled
tabs and automatically select them; Home/End choose the first/last enabled item.
Enter/Space use native button activation. Tab leaves the list, and Up/Down retain
normal page scrolling. Arrow navigation follows inherited RTL direction.

## Styling tokens

Set tokens on `:root`, a parent container or the component's `style` attribute.
Defaults read the existing semantic tokens and adapt to the active theme. No
theme-layer changes are required.

| Token | Default |
| --- | --- |
| `--dnet-pill-tabs-background` | `--dnet-sys-surface` |
| `--dnet-pill-tabs-foreground` | `--dnet-sys-on-surface` |
| `--dnet-pill-tabs-selected-background` | `--dnet-sys-primary-emphasis` |
| `--dnet-pill-tabs-selected-foreground` | `--dnet-sys-on-primary` |
| `--dnet-pill-tabs-hover-background` | `--dnet-sys-state-hover` |
| `--dnet-pill-tabs-badge-background` | `--dnet-sys-surface-hover` |
| `--dnet-pill-tabs-badge-selected-background` | Current foreground at 22% opacity |
| `--dnet-pill-tabs-radius` | `--dnet-sys-radius-pill` |
| `--dnet-pill-tabs-padding` | `--dnet-sys-space-1` |
| `--dnet-pill-tabs-tab-height` | `--dnet-sys-control-height-sm` |
| `--dnet-pill-tabs-font-size` | `--dnet-sys-text-md` |
| `--dnet-pill-tabs-shadow` | `--dnet-sys-elevation-1` |

```razor
<DnetPillTabs Items="_tabs" @bind-Value="_status" AriaLabel="Estado"
              style="--dnet-pill-tabs-selected-background: #3855ff; --dnet-pill-tabs-radius: 12px;" />
```

This is a new component family; it introduces no legacy token aliases.
