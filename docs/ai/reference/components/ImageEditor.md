# ImageEditor

Components: `<DnetImageEditor>`

## `<DnetImageEditor>`

| Parameter | Type | Default |
|---|---|---|
| `OnImageSelected` | `EventCallback<MemoryStream>` | — |
| `OnStarLoadingImage` | `EventCallback` | — |
| `OnEndLoadingImage` | `EventCallback` | — |
| `OnCancel` | `EventCallback` | — |
| `ImageContainerHeight` | `int` | `480` |
| `ImageContainerWidth` | `int` | `640` |
| `ImagePreviewHeight` | `int` | `170` |
| `ImagePreviewWidth` | `int` | `170` |
| `ModalDialogHeight` | `int` | `668` |
| `ModalDialogWidth` | `int` | `1024` |
| `MaxFileSizes` | `long` | — |
| `AllowedFormats` | `List<string>` | `new()` |
| `ImageEditingControls` | `List<ImageEditorControlType>` | `new()` |
| `ImageFile` | `IBrowserFile?` | — |

## Minimal usage

```razor
<DnetImageEditor
    MaxFileSizes="..."
    ImageFile="..."
/>
```
