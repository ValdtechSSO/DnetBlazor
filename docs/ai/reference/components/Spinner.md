# Spinner

Components: `<DnetSpinner>`

## `<DnetSpinner>`

| Parameter | Type | Default |
|---|---|---|
| `CanRun` | `bool` | — |
| `BindToView` | `bool` | — |
| `ShowMask` | `bool` | — |
| `DebounceTime` | `int` | `250` |

## Minimal usage

```razor
<DnetSpinner
    CanRun="..."
    BindToView="..."
    ShowMask="..."
/>
```

## Styling tokens

Override these anywhere in the DOM above the component — `:root`, a
container, or the element's own `style`. Nothing else is needed.

| Token | Falls back to |
|---|---|
| `--dnet-spinner-color` | `var(--dnet-sys-primary-strong)` |
| `--dnet-spinner-mask-background` | `color-mix(in srgb, var(--dnet-sys-on-surface) 16%, transparent)` |

```css
:root { --dnet-spinner-color: /* your value */; }
```
