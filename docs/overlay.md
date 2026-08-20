# Overlay

`DnetOverlay` is the shared host for floating components: Dialog, Select,
Autocomplete, DatePicker, Tooltip, ConnectedPanel, FloatingPanel and Toast.
Render one host in the application layout:

```razor
<DnetOverlay BaseZindex="1100" />
```

The host uses one viewport listener and only registers document keyboard,
pointer and scroll listeners while an attached overlay needs them. It also
follows the active Fullscreen API element when the browser enters fullscreen.

## OverlayConfig

The existing positioning, backdrop, size and theme options remain available.
The following optional settings define interaction and accessibility behavior:

| Property | Purpose |
| --- | --- |
| `ScrollStrategy` | `Noop`, `Reposition`, `Close` or `Block`. `Close` ignores its own panel's scroll by default; set `CloseOnOverlayScroll` to change that. |
| `CloseOnEscape` | Lets the topmost eligible overlay close on Escape. |
| `CloseOnOutsidePointer` | Lets a non-modal overlay close when a pointer gesture begins and ends outside it. |
| `Role`, `AriaLabel`, `AriaLabelledBy`, `AriaDescribedBy`, `AriaModal`, `Direction` | Pane semantics and language direction. |
| `TrapFocus`, `RestoreFocus`, `InitialFocusSelector` | Opt-in modal focus management. |

The built-in consumers use sensible defaults: dialogs block document scroll and
trap focus; connected controls such as Select, Autocomplete and DatePicker
reposition on scroll and close with Escape; Tooltip repositions and closes on
outside interaction. Applications can override any option through the
corresponding component configuration.

## Lifetime and manual updates

Every `IOverlayService.Attach` call returns an `OverlayReference`. It has an
idempotent `Detach`/`Dispose` lifecycle, exposes `IsAttached`, and can request
a new connected position through `RequestPositionUpdate()`.

For a live overlay, update constraints through `UpdateSize`:

```csharp
overlayReference.UpdateSize(new OverlaySize
{
    Width = "32rem",
    MaxHeight = "min(80dvh, 42rem)"
});
```

Requests from resize, scroll and explicit updates are coalesced per overlay;
only the latest calculated position is applied.
