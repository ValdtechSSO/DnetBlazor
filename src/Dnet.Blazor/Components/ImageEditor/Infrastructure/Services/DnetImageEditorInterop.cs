using Dnet.Blazor.Components.ImageEditor.Infrastructure.Models;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace Dnet.Blazor.Components.ImageEditor.Infrastructure.Services
{
    /// <summary>
    /// Talks to the image editor browser module. Every call carries the editor id so
    /// that several editors can coexist on one page without sharing state.
    /// </summary>
    public class DnetImageEditorInterop : IAsyncDisposable
    {
        private const string JsFunctionsPrefix = "dnetimageeditor";

        private readonly IDragAndDropJsCallbacks _owner;

        private readonly IJSRuntime _jsRuntime;

        private readonly string _editorId;

        private DotNetObjectReference<DnetImageEditorInterop>? _selfReference;


        public DnetImageEditorInterop(IDragAndDropJsCallbacks owner, IJSRuntime jsRuntime, string editorId)
        {
            _owner = owner;
            _jsRuntime = jsRuntime;
            _editorId = editorId;
            _selfReference = DotNetObjectReference.Create(this);
        }

        public ValueTask<FlexibleConnectedPositionStrategyOrigin> GetBoundingClientRect(ElementReference element)
        {
            return _jsRuntime.InvokeAsync<FlexibleConnectedPositionStrategyOrigin>($"{JsFunctionsPrefix}.getBoundingClientRect", element);
        }

        /// <summary>Decodes the image in the browser and draws it on the editor canvas.</summary>
        public ValueTask<ImageEditorSourceData> InitializeSource(DotNetStreamReference streamReference, ElementReference canvas, ElementReference preview, ElementReference viewport, ImageEditorOptions options)
        {
            return _jsRuntime.InvokeAsync<ImageEditorSourceData>(
                $"{JsFunctionsPrefix}.initializeSource",
                _selfReference,
                _editorId,
                streamReference,
                canvas,
                preview,
                viewport,
                options);
        }

        /// <summary>Wires the crop rectangle to the rendered overlay and reports the initial selection.</summary>
        public ValueTask<DraggedData> AttachCrop(ElementReference board, ElementReference cropContainer, ElementReference cropBox, ElementReference selectionBadge)
        {
            return _jsRuntime.InvokeAsync<DraggedData>(
                $"{JsFunctionsPrefix}.attachCrop",
                _editorId,
                board,
                cropContainer,
                cropBox,
                selectionBadge);
        }

        /// <summary>Sets the zoom factor and reports the resulting view.</summary>
        public ValueTask<ImageEditorViewState> SetZoom(double factor)
        {
            return _jsRuntime.InvokeAsync<ImageEditorViewState>($"{JsFunctionsPrefix}.setZoom", _editorId, factor);
        }

        /// <summary>Locks the selection to a width/height ratio, or releases it with a null or zero value.</summary>
        public ValueTask<DraggedData> SetAspectRatio(double ratio)
        {
            return _jsRuntime.InvokeAsync<DraggedData>($"{JsFunctionsPrefix}.setAspectRatio", _editorId, ratio);
        }

        /// <summary>Moves the selection to an explicit rectangle given in source pixels.</summary>
        public ValueTask<DraggedData> SetCropInSource(double left, double top, double width, double height)
        {
            return _jsRuntime.InvokeAsync<DraggedData>(
                $"{JsFunctionsPrefix}.setCropInSource",
                _editorId,
                left,
                top,
                width,
                height);
        }

        /// <summary>Turns the picture a quarter turn, swapping its dimensions.</summary>
        public ValueTask<ImageEditorViewState> Rotate(bool clockwise)
        {
            return _jsRuntime.InvokeAsync<ImageEditorViewState>($"{JsFunctionsPrefix}.rotate", _editorId, clockwise);
        }

        /// <summary>Applies the selection to the working image, without re-encoding it.</summary>
        public ValueTask<ImageEditorViewState> CropToSelection()
        {
            return _jsRuntime.InvokeAsync<ImageEditorViewState>($"{JsFunctionsPrefix}.cropToSelection", _editorId);
        }

        /// <summary>Returns to the untouched picture and a centred selection.</summary>
        public ValueTask<ImageEditorViewState> ResetEditor()
        {
            return _jsRuntime.InvokeAsync<ImageEditorViewState>($"{JsFunctionsPrefix}.reset", _editorId);
        }

        /// <summary>Mirrors the working image on the canvas, without re-encoding it.</summary>
        public ValueTask Flip(bool horizontal)
        {
            return _jsRuntime.InvokeVoidAsync($"{JsFunctionsPrefix}.flip", _editorId, horizontal);
        }

        /// <summary>Encodes the selection at its native resolution and keeps it ready for the caller.</summary>
        public ValueTask<ImageEditorExportResult> ExportImage(bool useCrop)
        {
            return _jsRuntime.InvokeAsync<ImageEditorExportResult>($"{JsFunctionsPrefix}.exportImage", _editorId, useCrop);
        }

        /// <summary>Opens the encoded result as a .NET stream, without going through base64.</summary>
        public ValueTask<IJSStreamReference> GetResultStream()
        {
            return _jsRuntime.InvokeAsync<IJSStreamReference>($"{JsFunctionsPrefix}.getResultStream", _editorId);
        }

        /// <summary>Starts loading the Photon WASM module in the background, without waiting for it.</summary>
        public ValueTask PreloadPhotonWasm()
        {
            return _jsRuntime.InvokeVoidAsync("photonInit.init");
        }

        [JSInvokable]
        public void OnDragEnd(DraggedData draggedData)
        {
            _owner.OnDragEnd(draggedData);
        }

        [JSInvokable]
        public void OnDragStart()
        {
            _owner.OnDragStart();
        }

        [JSInvokable]
        public void OnDrag(DraggedData draggedData)
        {
            _owner.OnDrag(draggedData);
        }

        [JSInvokable]
        public void OnResizeEnd(DraggedData draggedData)
        {
            _owner.OnResizeEnd(draggedData);
        }

        [JSInvokable]
        public void OnResizeStart()
        {
            _owner.OnResizeStart();
        }

        [JSInvokable]
        public void OnResize(DraggedData draggedData)
        {
            _owner.OnResize(draggedData);
        }

        public async ValueTask DisposeAsync()
        {
            var reference = _selfReference;

            if (reference != null)
            {
                _selfReference = null;

                try
                {
                    await _jsRuntime.InvokeVoidAsync($"{JsFunctionsPrefix}.dispose", _editorId);
                }
                catch (JSDisconnectedException)
                {
                    // The circuit is already gone; there is nothing left to clean up.
                }
                finally
                {
                    reference.Dispose();
                }
            }
        }
    }
}
