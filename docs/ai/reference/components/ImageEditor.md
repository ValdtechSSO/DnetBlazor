# ImageEditor

## `<DnetImageEditor>`

```razor
<DnetImageEditor
    MaxFileSizes="..."
/>
```

| Parameter | Type | Default | Description |
|---|---|---|---|
| `OnImageSelected` | `EventCallback<MemoryStream>` | — | Raised when image selected occurs. |
| `OnStarLoadingImage` | `EventCallback` | — | Raised when star loading image occurs. |
| `OnEndLoadingImage` | `EventCallback` | — | Raised when end loading image occurs. |
| `OnCancel` | `EventCallback` | — | Raised when cancel occurs. |
| `ImageContainerHeight` | `int` | `480` | Gets or sets the image container height used by this component. |
| `ImageContainerWidth` | `int` | `640` | Gets or sets the image container width used by this component. |
| `ImagePreviewHeight` | `int` | `170` | Gets or sets the image preview height used by this component. |
| `ImagePreviewWidth` | `int` | `170` | Gets or sets the image preview width used by this component. |
| `ModalDialogHeight` | `int` | `668` | Gets or sets the modal dialog height used by this component. |
| `ModalDialogWidth` | `int` | `1024` | Gets or sets the modal dialog width used by this component. |
| `MaxFileSizes` | `long` | — | Gets or sets the max file sizes used by this component. |
| `AllowedFormats` | `List<string>` | `new()` | Gets or sets the image file formats that may be selected. |
| `ImageEditingControls` | `List<ImageEditorControlType>` | `new()` | Gets or sets the image editing controls used by this component. |
| `ImageFile` | `IBrowserFile?` | — | Gets or sets the image file used by this component. |
