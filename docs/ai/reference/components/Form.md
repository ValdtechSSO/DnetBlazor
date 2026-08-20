# Form

Components: `<DnetFormField>`, `<DnetInputDate>`, `<DnetInputNumber>`, `<DnetInputText>`, `<DnetInputTextArea>`

## `<DnetFormField>`

| Parameter | Type | Default |
|---|---|---|
| `ChildContent` | `RenderFragment?` | — |
| `PrefixContent` | `RenderFragment?` | — |
| `SufixContent` | `RenderFragment?` | — |
| `HintContent` | `RenderFragment?` | — |
| `ErrorContent` | `RenderFragment?` | — |
| `UseClearButton` | `bool` | `false` |
| `EmptyIconClass` | `string?` | — |
| `Label` | `string?` | — |
| `IsRequired` | `bool` | — |

## `<DnetInputDate>` — generic over TValue

| Parameter | Type | Default |
|---|---|---|
| `OnClearInput` | `EventCallback<bool>` | — |
| `OnStopTyping` | `EventCallback<string>` | — |
| `Disabled` | `bool` | — |
| `ImmediateResponse` | `bool` | `true` |
| `DebounceTime` | `int` | `300` |
| `Type` | `InputDateType` | `InputDateType.Date` |
| `ParsingErrorMessage` | `string` | `string.Empty` |
| `IsRequired` | `bool` | — |
| `PlaceHolder` | `string?` | — |

## `<DnetInputNumber>` — generic over TValue

| Parameter | Type | Default |
|---|---|---|
| `OnClearInput` | `EventCallback<bool>` | — |
| `OnStopTyping` | `EventCallback<string>` | — |
| `Disabled` | `bool` | — |
| `ImmediateResponse` | `bool` | `false` |
| `ParsingErrorMessage` | `string` | `"The {0} field must be a number."` |
| `DebounceTime` | `int` | `300` |
| `IsRequired` | `bool` | — |
| `PlaceHolder` | `string?` | — |
| `Id` | `string` | `string.Empty` |
| `Min` | `string` | `string.Empty` |
| `Max` | `string` | `string.Empty` |
| `Step` | `string` | `"any"` |

## `<DnetInputText>`

| Parameter | Type | Default |
|---|---|---|
| `OnClearInput` | `EventCallback<bool>` | — |
| `OnStopTyping` | `EventCallback<string>` | — |
| `Disabled` | `bool` | — |
| `ImmediateResponse` | `bool` | `true` |
| `DebounceTime` | `int` | `300` |
| `PlaceHolder` | `string?` | — |

## `<DnetInputTextArea>`

| Parameter | Type | Default |
|---|---|---|
| `OnStopTyping` | `EventCallback<string>` | — |
| `OnClearInput` | `EventCallback<bool>` | — |
| `Disabled` | `bool` | — |
| `Label` | `string?` | — |
| `DebounceTime` | `int` | `300` |
| `ImmediateResponse` | `bool` | `true` |
| `IsRequired` | `bool` | — |
| `PlaceHolder` | `string?` | — |
| `MaxCharacters` | `int` | — |

## Minimal usage

```razor
<DnetFormField
    EmptyIconClass="..."
    Label="..."
    IsRequired="..."
/>
```

## Styling tokens

Override these anywhere in the DOM above the component — `:root`, a
container, or the element's own `style`. Nothing else is needed.

| Token | Falls back to |
|---|---|
| `--dnet-form-field-hint-color` | — |
| `--dnet-form-field-hint-font-size` | — |
| `--dnet-form-field-plain-label-color` | — |
| `--dnet-form-field-plain-label-font-size` | — |

```css
:root { --dnet-form-field-hint-color: /* your value */; }
```
