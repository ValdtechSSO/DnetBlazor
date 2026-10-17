# DnetButton

`DnetButton` provides three sizes with consistent padding and icon spacing.
Import `Dnet.Blazor.Components.Button`. The default is a filled primary action,
`Medium` size and pill corners. The default primary color is `#5B5CE0`.

```razor
@using Dnet.Blazor.Components.Button

<DnetButton OnClick="Save">Save changes</DnetButton>
<DnetButton Variant="DnetButtonVariant.Outlined" OnClick="Cancel">Cancel</DnetButton>

<DnetButton Size="DnetButtonSize.Large" OnClick="Add">
    <IconContent>
        <svg viewBox="0 0 24 24" fill="none" aria-hidden="true" focusable="false">
            <path d="M12 5v14M5 12h14"
                  stroke="currentColor" stroke-width="1.8" stroke-linecap="round" />
        </svg>
    </IconContent>
    <ChildContent>Common button</ChildContent>
</DnetButton>
```

## Action hierarchy

Use `Filled` for the main task, such as Save, Apply or Continue. Use `Outlined`
for secondary actions, such as Cancel, Back, Reset or Clear. Both have a visible
button surface or border, with distinct hover, keyboard-focus and disabled states.
There is no text-only action variant. Structural controls such as calendar
navigation and tabs keep their own component presentation.

Filled buttons read `--dnet-sys-primary` and `--dnet-sys-on-primary`. Outlined
buttons use `--dnet-sys-primary-content`, a brand-colored foreground that stays
legible on ordinary surfaces in both themes. Explicit component token overrides remain effective
for either variant.

## Sizes

| `Size` | Height | Padding on each side | Icon size | Icon/text gap |
| --- | --- | --- | --- | --- |
| `DnetButtonSize.Small` | 32 px | 12 px | 20 × 20 px | 4 px |
| `DnetButtonSize.Medium` | 40 px | 16 px | 20 × 20 px | 8 px |
| `DnetButtonSize.Large` | 56 px | 24 px | 24 × 24 px | 8 px |

The same measurements apply to pill, softly rounded and square buttons. The
icon/text gap is present when an icon accompanies the label. `IconContent`
provides a leading icon slot; direct SVG or image elements in `ChildContent`
also adopt the selected icon size and spacing. Give an SVG a `viewBox` and use
`currentColor` so it follows the button foreground color.

## Parameters and native attributes

| Parameter | Default | Behavior |
| --- | --- | --- |
| `Variant` | `DnetButtonVariant.Filled` | Selects `Filled` or `Outlined`. |
| `Size` | `DnetButtonSize.Medium` | Selects `Small`, `Medium` or `Large` spacing. |
| `IconContent` | `null` | Optional leading decorative icon. |
| `ChildContent` | `null` | Visible button label or other content. |
| `OnClick` | — | Callback invoked when the native button is activated. |
| `ButtonType` | `"button"` | Native button type, such as `"submit"` or `"reset"`. |
| `InitialFocus` | `false` | Applies the library's initial-focus marker. |
| `AdditionalAttributes` | `null` | Native attributes including `id`, `class`, `style`, `disabled` and ARIA attributes. |

```razor
<DnetButton Size="DnetButtonSize.Small" disabled="@_saving" OnClick="Save">
    Save changes
</DnetButton>
```

The underlying element is a native `<button>`, so Enter and Space activate it.
Native `disabled` prevents activation. `IconContent` is decorative and excluded
from the accessibility tree; retain a visible label or supply an accessible name
when creating an icon-only action.

## Corner shapes and styling

Pill corners use `--dnet-sys-radius-pill`, which defaults to `9999px`. Override
`--dnet-btn-radius` to select another shape without changing the size:

```razor
<DnetButton Size="DnetButtonSize.Small">Pill</DnetButton>
<DnetButton Size="DnetButtonSize.Medium" style="--dnet-btn-radius:6px;">Soft</DnetButton>
<DnetButton Size="DnetButtonSize.Large" style="--dnet-btn-radius:0;">Square</DnetButton>
```

Public styling tokens inherit from `:root`, an intermediate container or the
button's `style` attribute. Explicit `--dnet-btn-height` and `--dnet-btn-padding`
overrides take precedence over the selected size. Existing legacy tokens such
as `--dnet-button-line-height`, `--dnet-button-padding` and
`--dnet-button-border-radius` remain supported in the fallback chain.

The color tokens are `--dnet-btn-background`, `--dnet-btn-background-hover` and
`--dnet-btn-foreground`; `--dnet-btn-border-color` customizes the border, and
`--dnet-btn-font-size` controls the label's text size.
Use semantic theme tokens for colors so the same button works in scoped themes.

See `/Buttons` for all three sizes in pill, soft and square shapes, with and
without icons, plus disabled and keyboard interaction examples.
