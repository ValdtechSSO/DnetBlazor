# DnetFormField

`DnetFormField` groups an input with its label, optional prefix and suffix, hint
and validation messages. Import `Dnet.Blazor.Components.Form`.

## Appearance

Fields use `FormAppearance.Fill` by default. The control has a subtle background
derived from the current theme: 12% `--dnet-sys-on-surface` mixed with
`--dnet-sys-surface`. In a dark theme this creates a lighter surface behind the
input, select, autocomplete or date picker. The border matches the background,
including when disabled. Standard and Legacy retain their contrasting outline.

```razor
@using Dnet.Blazor.Components.Form

<DnetFormField Label="Name">
    <ChildContent>
        <DnetInputText @bind-Value="_name" />
    </ChildContent>
</DnetFormField>

<DnetFormField Label="Email" Appearance="FormAppearance.Standard">
    <ChildContent>
        <DnetInputText @bind-Value="_email" />
    </ChildContent>
</DnetFormField>
```

| `Appearance` | Behavior |
| --- | --- |
| `FormAppearance.Fill` | Filled background and a label inside the control; the default. |
| `FormAppearance.Standard` | Transparent background with the label above the control. |
| `FormAppearance.Legacy` | Compatibility alias for the transparent appearance and external label. |

Change `Appearance` at any time without remounting the input or clearing its
value. Apply the same bound value to several fields for a form-wide preference.
`Fill` floats its label when the field has a value or receives focus. `Standard`
and `Legacy` keep the label outside the control. Clear buttons, validation,
prefixes and suffixes remain available in each appearance.

Disabled inputs using `Fill` have a muted, opaque background. `Standard` and
`Legacy` retain the previous disabled background. Set `Disabled` on the input
component, as before. Theme colors follow the surrounding theme, including a
scoped dark theme; no component-specific dark selector is required.

## Spacing

The default geometry follows filled Material fields:

- A single-line control is at least **56px** high, with **16px** horizontal
  padding when there is no icon. Multiline content can grow beyond this height.
- An empty, unfocused `Fill` field shows a **16px** label with a **24px** line
  height, centered vertically inside the control.
- On focus or when a value is present, the label uses **12px** text with a
  **16px** line height, **8px** from the top. The input uses **16px** text with a
  **24px** line height, with **8px** below it.
- Leading and trailing icons occupy **24px**, sit **12px** from their nearest
  edge and have a **16px** gap to the text. Prefix and suffix slots follow the
  text direction, including in RTL layouts.
- Supporting text starts **4px** below the control and is inset **16px** on each
  side. The character counter shares this row at the trailing edge.

`Standard` and `Legacy` share the 56px minimum height and horizontal spacing;
their labels remain above the control. Input values, labels and supporting text
can still be customized through the tokens below.

## Styling

`AdditionalAttributes` forwards HTML attributes such as `id`, `class` and `style`
to the field's root. Custom classes are combined with the component's classes.
Public tokens work on `:root`, an intermediate container or an individual field.

| Public token | Default |
| --- | --- |
| `--dnet-form-field-background` | The theme-derived filled color, or transparent for `Standard` and `Legacy`. |
| `--dnet-form-field-plain-label-color` | `--dnet-sys-on-surface-muted` |
| `--dnet-form-field-plain-label-font-size` | `--dnet-sys-text-lg` (16px) for the resting `Fill` label; `--dnet-sys-text-sm` (12px) for `Standard` and `Legacy`. The floating label scales to 75% (12px by default). |
| `--dnet-form-field-hint-color` | `--dnet-sys-on-surface-muted` |
| `--dnet-form-field-hint-font-size` | `--dnet-sys-text-sm` (12px) |
| `--dnet-form-field-min-height` | `56px` |
| `--dnet-form-field-padding-inline` | `16px` |
| `--dnet-form-field-icon-size` | `24px` |
| `--dnet-form-field-icon-inset` | `12px` |
| `--dnet-form-field-icon-gap` | `16px` |
| `--dnet-form-field-font-size` | `1rem` |

The background token overrides the selected appearance. To customize the default
fill across a section, define the token on its container:

```razor
<section style="--dnet-form-field-background:var(--dnet-sys-state-hover);">
    <DnetFormField Label="City">
        <ChildContent>
            <DnetInputText @bind-Value="_city" />
        </ChildContent>
    </DnetFormField>
</section>

<DnetFormField id="postal-code" class="address-field" Label="Postal code"
               style="--dnet-form-field-background:var(--dnet-sys-surface);">
    <ChildContent>
        <DnetInputText @bind-Value="_postalCode" />
    </ChildContent>
</DnetFormField>
```

The `/Forms` sample groups contact details, dates, numeric values and an address
into responsive cards. Its **Field appearance** selector compares **Filled
background** and **Transparent** on the same controls without losing entered
values. The reference gallery shows empty and populated fields, leading and
trailing icons, supporting text and a character counter. A link opens the
`/FormValidation` sample for a complete validation and save flow.
