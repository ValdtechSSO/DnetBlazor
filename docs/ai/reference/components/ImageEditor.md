# ImageEditor

## `<DnetImageEditor>`

```razor
<DnetImageEditor
    MaxFileSizes="..."
    MaxOutputDimension="..."
    OutputFormat="..."
/>
```

| Parameter | Type | Default | Description |
|---|---|---|---|
| `OnImageSelected` | `EventCallback<MemoryStream>` | — | Raised when image selected occurs. |
| `OnStarLoadingImage` | `EventCallback` | — | Raised when star loading image occurs. |
| `OnEndLoadingImage` | `EventCallback` | — | Raised when end loading image occurs. |
| `OnCancel` | `EventCallback` | — | Raised when cancel occurs. |
| `ImageContainerHeight` | `int` | `480` | Gets or sets the image container height used by this component. It bounds the size the image is displayed at. |
| `ImageContainerWidth` | `int` | `640` | Gets or sets the image container width used by this component. It bounds the size the image is displayed at. |
| `ImagePreviewHeight` | `int` | `170` | Gets or sets the image preview height used by this component. |
| `ImagePreviewWidth` | `int` | `170` | Gets or sets the image preview width used by this component. |
| `ModalDialogHeight` | `int` | `780` | Gets or sets the modal dialog height used by this component. |
| `ModalDialogWidth` | `int` | `1024` | Gets or sets the modal dialog width used by this component. |
| `MaxFileSizes` | `long` | — | Gets or sets the maximum size accepted for the image, in bytes. A value of zero or less uses DefaultMaxFileSize. |
| `MaxOutputDimension` | `int` | — | Gets or sets the longest edge of the exported image. Zero keeps the native resolution of the selection. |
| `OutputFormat` | `string` | — | Gets or sets the format the editor exports, such as ImageFormat.PNG. A null or empty value keeps the format of the selected image. |
| `OutputQuality` | `int` | `95` | Gets or sets the quality used by lossy output formats, from 1 to 100. |
| `AllowedFormats` | `List<string>` | `new()` | Gets or sets the image file formats that may be selected. |
| `ImageEditingControls` | `List<ImageEditorControlType>` | `new()` | Gets or sets the image editing controls used by this component. |
| `ImageFile` | `IBrowserFile?` | — | Gets or sets the image file used by this component. |

## `<ImageEditorTooltipText>`

```razor
<ImageEditorTooltipText
/>
```

| Parameter | Type | Default | Description |
|---|---|---|---|
| `ResourceKey` | `string` | `string.Empty` | Gets or sets the resource key used by this component. |

## Styling tokens

Set any of these above the component in the DOM — `:root`, a container, or
the element's own `style`. Nothing else is needed.

| Token | Effective default |
|---|---|
| `--dnet-image-editor-footer-border-width` | `1px` |
| `--dnet-image-editor-footer-padding-top` | `calc(var(--dnet-sys-space-unit) * 3)` <br><sub>via `--dnet-sys-space-3`</sub> |
| `--dnet-image-editor-zoom-padding` | `0 var(--dnet-sys-space-1)` |
| `--dnet-image-editor-zoom-radius` | `5px` <br><sub>via `--dnet-sys-radius-md`</sub> |

```css
:root { --dnet-image-editor-zoom-radius: /* your value */; }
```
