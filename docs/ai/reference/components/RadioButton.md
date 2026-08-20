# RadioButton

An input component for editing bool values.

## `<DnetInputRadioButton>`

```razor
<DnetInputRadioButton
    Disabled="..."
/>
```

| Parameter | Type | Default | Description |
|---|---|---|---|
| `AdditionalAttributes` | `IReadOnlyDictionary<string, object>?` | — | Gets or sets a collection of additional attributes that will be applied to the input element. |
| `Value` | `TValue?` | — | Gets or sets the value of this input. |
| `Name` | `string?` | — | Gets or sets the name of the parent input radio group. |
| `TextPlacedBefore` | `bool` | `false` | Gets or sets the name of the parent input radio group. |
| `ChildContent` | `RenderFragment` | — | Gets or sets the child content rendered by the component. |
| `Disabled` | `bool` | — | Gets or sets whether user interaction with the component is disabled. |

## `<DnetInputRadioGroup>`

```razor
<DnetInputRadioGroup
/>
```

| Parameter | Type | Default | Description |
|---|---|---|---|
| `ChildContent` | `RenderFragment?` | — | Gets or sets the child content to be rendering inside the InputRadioGroup{TValue}. |
| `Name` | `string?` | — | Gets or sets the name of the group. |

## Styling tokens

Set any of these above the component in the DOM — `:root`, a container, or
the element's own `style`. Nothing else is needed.

| Token | Effective default |
|---|---|
| `--dnet-radio-button-background` | `#42b0d5` <br><sub>via `--dnet-sys-primary-strong`</sub> |
| `--dnet-radio-button-border-width` | `1px` |
| `--dnet-radio-button-font-size` | `0.875rem` <br><sub>via `--dnet-sys-text-md`</sub> |
| `--dnet-radio-button-size` | `16px` |

```css
:root { --dnet-radio-button-background: /* your value */; }
```
