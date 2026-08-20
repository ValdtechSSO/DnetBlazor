# Form

## `<DnetFormField>`

```razor
<DnetFormField
    IsRequired="..."
/>
```

| Parameter | Type | Default | Description |
|---|---|---|---|
| `ChildContent` | `RenderFragment?` | — | Gets or sets the child content rendered by the component. |
| `PrefixContent` | `RenderFragment?` | — | Gets or sets content rendered before the main control content. |
| `SufixContent` | `RenderFragment?` | — | Gets or sets content rendered after the main control content. |
| `HintContent` | `RenderFragment?` | — | Gets or sets supporting content displayed with the control. |
| `ErrorContent` | `RenderFragment?` | — | Gets or sets content displayed when validation reports an error. |
| `UseClearButton` | `bool` | `false` | Gets or sets whether the component use clear button. |
| `EmptyIconClass` | `string?` | — | Gets or sets the empty icon class used by this component. |
| `Label` | `string?` | — | Gets or sets the label displayed for the component. |
| `IsRequired` | `bool` | — | Gets or sets whether a value is required for validation. |

## `<DnetInputDate>` — generic over TValue

```razor
<DnetInputDate TValue="..."
    Disabled="..."
    IsRequired="..."
/>
```

| Parameter | Type | Default | Description |
|---|---|---|---|
| `OnClearInput` | `EventCallback<bool>` | — | Raised when the user clears the input. |
| `OnStopTyping` | `EventCallback<string>` | — | Raised after the user stops typing. |
| `Disabled` | `bool` | — | Gets or sets whether user interaction with the component is disabled. |
| `ImmediateResponse` | `bool` | `true` | Gets or sets the immediate response used by this component. |
| `DebounceTime` | `int` | `300` | Gets or sets the delay before a debounced input action is raised. |
| `Type` | `InputDateType` | `InputDateType.Date` | Gets or sets the type used by this component. |
| `ParsingErrorMessage` | `string` | `string.Empty` | Gets or sets the parsing error message used by this component. |
| `IsRequired` | `bool` | — | Gets or sets whether a value is required for validation. |
| `PlaceHolder` | `string?` | — | Gets or sets placeholder text displayed when the input is empty. |

## `<DnetInputNumber>` — generic over TValue

```razor
<DnetInputNumber TValue="..."
    Disabled="..."
    IsRequired="..."
/>
```

| Parameter | Type | Default | Description |
|---|---|---|---|
| `OnClearInput` | `EventCallback<bool>` | — | Raised when the user clears the input. |
| `OnStopTyping` | `EventCallback<string>` | — | Raised after the user stops typing. |
| `Disabled` | `bool` | — | Gets or sets whether user interaction with the component is disabled. |
| `ImmediateResponse` | `bool` | `false` | Gets or sets the immediate response used by this component. |
| `ParsingErrorMessage` | `string` | `"The {0} field must be a number."` | Gets or sets the parsing error message used by this component. |
| `DebounceTime` | `int` | `300` | Gets or sets the delay before a debounced input action is raised. |
| `IsRequired` | `bool` | — | Gets or sets whether a value is required for validation. |
| `PlaceHolder` | `string?` | — | Gets or sets placeholder text displayed when the input is empty. |
| `Id` | `string` | `string.Empty` | Gets or sets the id used by this component. |
| `Min` | `string` | `string.Empty` | Gets or sets the min used by this component. |
| `Max` | `string` | `string.Empty` | Gets or sets the max used by this component. |
| `Step` | `string` | `"any"` | Gets or sets the step used by this component. |

## `<DnetInputText>`

```razor
<DnetInputText
    Disabled="..."
/>
```

| Parameter | Type | Default | Description |
|---|---|---|---|
| `OnClearInput` | `EventCallback<bool>` | — | Raised when the user clears the input. |
| `OnStopTyping` | `EventCallback<string>` | — | Raised after the user stops typing. |
| `Disabled` | `bool` | — | Gets or sets whether user interaction with the component is disabled. |
| `ImmediateResponse` | `bool` | `true` | Gets or sets the immediate response used by this component. |
| `DebounceTime` | `int` | `300` | Gets or sets the delay before a debounced input action is raised. |
| `PlaceHolder` | `string?` | — | Gets or sets placeholder text displayed when the input is empty. |

## `<DnetInputTextArea>`

```razor
<DnetInputTextArea
    Disabled="..."
    IsRequired="..."
    MaxCharacters="..."
/>
```

| Parameter | Type | Default | Description |
|---|---|---|---|
| `OnStopTyping` | `EventCallback<string>` | — | Raised after the user stops typing. |
| `OnClearInput` | `EventCallback<bool>` | — | Raised when the user clears the input. |
| `Disabled` | `bool` | — | Gets or sets whether user interaction with the component is disabled. |
| `Label` | `string?` | — | Gets or sets the label displayed for the component. |
| `DebounceTime` | `int` | `300` | Gets or sets the delay before a debounced input action is raised. |
| `ImmediateResponse` | `bool` | `true` | Gets or sets the immediate response used by this component. |
| `IsRequired` | `bool` | — | Gets or sets whether a value is required for validation. |
| `PlaceHolder` | `string?` | — | Gets or sets placeholder text displayed when the input is empty. |
| `MaxCharacters` | `int` | — | Gets or sets the max characters used by this component. |

## Styling tokens

Set any of these above the component in the DOM — `:root`, a container, or
the element's own `style`. Nothing else is needed.

| Token | Effective default |
|---|---|
| `--dnet-form-field-hint-color` | `#666666` <br><sub>via `--dnet-sys-on-surface-muted`</sub> |
| `--dnet-form-field-hint-font-size` | `0.625rem` <br><sub>via `--dnet-sys-text-xs`</sub> |
| `--dnet-form-field-plain-label-color` | `#757575` <br><sub>via `--dnet-sys-on-surface-subtle`</sub> |
| `--dnet-form-field-plain-label-font-size` | `0.75rem` <br><sub>via `--dnet-sys-text-sm`</sub> |

```css
:root { --dnet-form-field-hint-color: /* your value */; }
```
