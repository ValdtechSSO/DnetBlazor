# DnetSpoiler

Shows a height-limited preview of long content with an expand/collapse button.
The button only appears when the content exceeds `MaxHeight`. Content remains
mounted in both states. Load the standard library CSS and JavaScript bundles.
The sample page is `/Spoiler`.

```razor
@using Dnet.Blazor.Components.Spoiler

<DnetSpoiler MaxHeight="120" ShowLabel="Leer descripción completa"
             HideLabel="Resumir descripción" AriaLabel="Descripción">
    <p>Long content here…</p>
</DnetSpoiler>
```

`MaxHeight` is a non-negative pixel height, defaulting to 100. It configures the
preview geometry through a data attribute; it does not override a styling token.
Zero hides the entire preview and leaves the expand button available. The full
height is measured again when content, images, fonts or available width change.
Short or empty content renders in full with no button, regardless of selection.

Omit `Expanded` for internal state, optionally starting with `DefaultExpanded`.
Later updates to `DefaultExpanded` do not reset an instance. Set `Expanded` to
control state and accept requested changes with `ExpandedChanged` or binding:

```razor
<DnetSpoiler @bind-Expanded="_expanded" ShowLabel="Ver detalles" HideLabel="Ocultar detalles">
    @Details
</DnetSpoiler>

@code {
    private bool _expanded;
    private RenderFragment Details => builder => builder.AddContent(0, "Long details…");
}
```

Controlled instances wait for the parent to accept a request. Content or width
changes do not emit `ExpandedChanged` or reset expansion. A native button provides
Enter/Space activation, `aria-expanded`, `aria-controls` and visible keyboard
focus. Optional `ShowAriaLabel` and `HideAriaLabel` override the accessible button
name. Set `AriaLabel` to expose a named content region; it is omitted by default
to avoid unnecessary region landmarks.

Text stays available to assistive technology even when visually clipped.
Interactive descendants outside the collapsed preview are made inert until
revealed; visible links remain usable. Existing inert elements remain inert.
Collapsing while focus is in a clipped control returns focus to the toggle.
Observer cleanup follows the component's DOM lifetime. Without JavaScript, all
content is visible and the inactive button is hidden.

`class`, `style`, `id`, `dir` and other unmatched attributes apply to the root.
The control follows inherited text direction. No legacy token aliases exist for
this new component.

## Styling

Tokens can be set at root, container or instance scope. Colors use semantic
fallbacks and adapt to scoped themes. Reduced-motion preferences disable the
height transition.

| Token | Default |
| --- | --- |
| `--dnet-spoiler-control-color` | Primary emphasis at 60%, mixed with on-surface |
| `--dnet-spoiler-control-hover-color` | Control color at 75%, mixed with on-surface |
| `--dnet-spoiler-control-font-size` | `--dnet-sys-text-md` |
| `--dnet-spoiler-gap` | `--dnet-sys-space-2` |
| `--dnet-spoiler-transition-duration` | `--dnet-sys-motion-normal` |

```razor
<DnetSpoiler style="--dnet-spoiler-control-color: #3855ff; --dnet-spoiler-transition-duration: 0ms;">
    @Details
</DnetSpoiler>
```
