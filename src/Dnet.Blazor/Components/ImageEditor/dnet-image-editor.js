// Dnet ImageEditor interop.
//
// Design notes (read before changing anything here):
//
//  * The source of truth for the pixels is the <canvas> rendered by the
//    component, not the on-screen <img> that used to be re-read on every
//    operation. The image is decoded once, drawn once, and every later
//    operation works on that canvas, so nothing is re-encoded until the
//    user actually exports.
//  * The crop geometry lives in the DOM: the four --_crop-* custom
//    properties written on the crop container. JavaScript updates them
//    during a gesture and Blazor only commits the final rectangle, so a
//    drag or a resize never costs a JS -> .NET call per frame.
//  * Exports keep the resolution of the crop in source pixels: the preview
//    box is a preview, never the output size. Only the optional
//    MaxOutputDimension cap shrinks the result.
//  * Nothing crosses the boundary as base64: the image comes in as a
//    DotNetStreamReference (ArrayBuffer) and goes out as a Blob. The blob is
//    returned as it is: the Blazor runtime wraps the returned value into a
//    stream reference, and calling DotNet.createJSStreamReference here would
//    hand it a value it can no longer convert.

window.dnetimageeditor = (function () {
    'use strict';

    var editors = new Map();

    var DEFAULT_MIN_CROP = 50;
    var DEFAULT_PREVIEW_SIZE = 170;
    var NOTIFY_INTERVAL = 120;

    var JPEG = 'image/jpeg';
    var PNG = 'image/png';
    var WEBP = 'image/webp';

    var RESIZER_TYPES = [
        'top-left',
        'top-center',
        'top-right',
        'left-center',
        'right-center',
        'bottom-left',
        'bottom-center',
        'bottom-right'
    ];

    function clamp(value, min, max) {
        return Math.min(Math.max(value, min), max);
    }

    function round(value) {
        return Math.round(value);
    }

    function getState(id) {
        return editors.get(id) || null;
    }

    // ---------------------------------------------------------------- format

    function sniffFormat(bytes) {
        if (bytes.length > 8 &&
            bytes[0] === 0x89 && bytes[1] === 0x50 && bytes[2] === 0x4E && bytes[3] === 0x47) {
            return PNG;
        }

        if (bytes.length > 3 && bytes[0] === 0xFF && bytes[1] === 0xD8) {
            return JPEG;
        }

        if (bytes.length > 12 &&
            bytes[0] === 0x52 && bytes[1] === 0x49 && bytes[2] === 0x46 && bytes[3] === 0x46 &&
            bytes[8] === 0x57 && bytes[9] === 0x45 && bytes[10] === 0x42 && bytes[11] === 0x50) {
            return WEBP;
        }

        // Anything else (gif, bmp, avif, ...) is exported losslessly, which is
        // the only safe choice: it keeps transparency and never re-encodes
        // pixels the editor was not asked to change.
        return PNG;
    }

    function normalizeFormat(format) {
        if (!format) return null;

        var value = String(format).toLowerCase();

        if (value === 'jpeg' || value === 'jpg' || value === 'image/jpg') return JPEG;
        if (value === 'png' || value === 'image/png') return PNG;
        if (value === 'webp' || value === 'image/webp') return WEBP;

        return null;
    }

    function hasAlpha(format) {
        return format === PNG || format === WEBP;
    }

    // ---------------------------------------------------------------- decode

    async function decodeImage(bytes, format) {
        var blob = new Blob([bytes], { type: format });

        if (typeof createImageBitmap === 'function') {
            try {
                var bitmap = await createImageBitmap(blob);

                return {
                    image: bitmap,
                    width: bitmap.width,
                    height: bitmap.height,
                    release: function () { if (bitmap.close) bitmap.close(); }
                };
            } catch (error) {
                // Falls through to the <img> path below.
            }
        }

        var url = URL.createObjectURL(blob);

        try {
            var image = await new Promise(function (resolve, reject) {
                var element = new Image();
                element.onload = function () { resolve(element); };
                element.onerror = function () { reject(new Error('The selected image could not be decoded.')); };
                element.src = url;
            });

            return {
                image: image,
                width: image.naturalWidth || image.width,
                height: image.naturalHeight || image.height,
                release: function () { URL.revokeObjectURL(url); }
            };
        } catch (error) {
            URL.revokeObjectURL(url);
            throw error;
        }
    }

    // --------------------------------------------------------------- geometry

    // Crop rectangle in layout pixels, read straight from the DOM so it stays
    // correct through dialog animations, window resizes and reflows.
    function readCropRect(state) {
        var box = state.box;

        if (!box || !box.offsetWidth) {
            return null;
        }

        return {
            left: box.offsetLeft,
            top: box.offsetTop,
            width: box.offsetWidth,
            height: box.offsetHeight
        };
    }

    function writeCropVars(state, rect) {
        var container = state.container;

        if (!container) return;

        container.style.setProperty('--_crop-left', rect.left + 'px');
        container.style.setProperty('--_crop-top', rect.top + 'px');
        container.style.setProperty('--_crop-width', rect.width + 'px');
        container.style.setProperty('--_crop-height', rect.height + 'px');
    }

    function displaySize(state) {
        var canvas = state.canvas;

        return {
            width: canvas.offsetWidth || canvas.width,
            height: canvas.offsetHeight || canvas.height
        };
    }

    // Maps the current selection (or the whole image) to source pixels.
    function sourceRect(state, useCrop) {
        var canvas = state.canvas;
        var display = displaySize(state);
        var rect = useCrop ? readCropRect(state) : null;

        if (!rect) {
            rect = { left: 0, top: 0, width: display.width, height: display.height };
        }

        var scaleX = canvas.width / Math.max(1, display.width);
        var scaleY = canvas.height / Math.max(1, display.height);

        var x = clamp(round(rect.left * scaleX), 0, Math.max(0, canvas.width - 1));
        var y = clamp(round(rect.top * scaleY), 0, Math.max(0, canvas.height - 1));

        return {
            x: x,
            y: y,
            width: clamp(round(rect.width * scaleX), 1, canvas.width - x),
            height: clamp(round(rect.height * scaleY), 1, canvas.height - y)
        };
    }

    function toNotifyPayload(state) {
        var rect = readCropRect(state);
        var crop = sourceRect(state, true);

        return {
            left: rect ? rect.left : 0,
            top: rect ? rect.top : 0,
            width: rect ? rect.width : 0,
            height: rect ? rect.height : 0,
            outputWidth: crop.width,
            outputHeight: crop.height
        };
    }

    function notify(state, method, payload) {
        if (!state.dotNetHelper) return;

        state.dotNetHelper.invokeMethodAsync(method, payload).catch(function () {
            // The circuit can be gone while a gesture is still in flight.
        });
    }

    // ---------------------------------------------------------------- preview

    // Fits the given source rectangle into the preview canvas without
    // allocating anything, so it is safe to run on every pointer frame. The
    // rectangle carries its own origin: drawing from (0, 0) would show the
    // top-left corner of the image instead of the selection.
    function drawPreview(state, source, rect) {
        var canvas = state.preview;

        if (!canvas || !rect.width || !rect.height) return;

        var box = state.previewBox;
        var ratio = Math.min(1, box.width / rect.width, box.height / rect.height);

        canvas.width = Math.max(1, round(rect.width * ratio));
        canvas.height = Math.max(1, round(rect.height * ratio));

        var context = canvas.getContext('2d');
        context.imageSmoothingEnabled = true;
        context.imageSmoothingQuality = 'high';
        context.clearRect(0, 0, canvas.width, canvas.height);
        context.drawImage(
            source,
            rect.x, rect.y, rect.width, rect.height,
            0, 0, canvas.width, canvas.height);
    }

    // The preview always shows the selection the crop box is framing. The box is
    // visible on screen, so previewing anything else reads as a mismatch.
    function refreshPreview(state) {
        if (!state.preview || !state.canvas.width) return;

        drawPreview(state, state.canvas, sourceRect(state, true));
    }

    // ------------------------------------------------------------------- i/o

    function canvasToBlob(canvas, type, quality) {
        return new Promise(function (resolve, reject) {
            canvas.toBlob(function (blob) {
                if (blob) {
                    resolve(blob);
                } else {
                    reject(new Error('The image could not be encoded as ' + type + '.'));
                }
            }, type, quality);
        });
    }

    async function encodeWorkCanvas(work, format, quality, maxDimension) {
        var width = work.width;
        var height = work.height;

        if (maxDimension > 0) {
            var longest = Math.max(width, height);

            if (longest > maxDimension) {
                var ratio = maxDimension / longest;
                width = Math.max(1, round(width * ratio));
                height = Math.max(1, round(height * ratio));
            }
        }

        var lossy = !hasAlpha(format);
        var qualityValue = clamp(round(quality), 1, 100);

        if (width === work.width && height === work.height) {
            var blob = await canvasToBlob(work, format, lossy ? qualityValue / 100 : undefined);
            return { blob: blob, width: width, height: height };
        }

        // Only the cropped region enters WASM, and only when a resize is
        // actually needed. Lanczos3 is the best filter Photon exposes for
        // photographic downscales.
        var photon = await window.photonInit.get();
        var image = photon.open_image(work, work.getContext('2d'));

        try {
            var resized = photon.resize(image, width, height, photon.SamplingFilter.Lanczos3);

            try {
                var bytes;

                if (format === PNG) {
                    bytes = resized.get_bytes();
                } else if (format === WEBP) {
                    bytes = resized.get_bytes_webp();
                } else {
                    bytes = resized.get_bytes_jpeg(qualityValue);
                }

                return { blob: new Blob([bytes], { type: format }), width: width, height: height };
            } finally {
                resized.free();
            }
        } finally {
            image.free();
        }
    }

    // --------------------------------------------------------------- gestures

    function clampToBoard(rect, board) {
        return {
            left: clamp(rect.left, 0, Math.max(0, board.width - rect.width)),
            top: clamp(rect.top, 0, Math.max(0, board.height - rect.height)),
            width: rect.width,
            height: rect.height
        };
    }

    function computeRect(mode, start, deltaX, deltaY, minWidth, minHeight, board) {
        var left = start.left;
        var top = start.top;
        var width = start.width;
        var height = start.height;

        var right = start.left + start.width;
        var bottom = start.top + start.height;

        switch (mode) {
            case 'move':
                return clampToBoard({
                    left: start.left + deltaX,
                    top: start.top + deltaY,
                    width: width,
                    height: height
                }, board);
            case 'top-left':
                left = clamp(start.left + deltaX, 0, Math.max(0, right - minWidth));
                top = clamp(start.top + deltaY, 0, Math.max(0, bottom - minHeight));
                width = right - left;
                height = bottom - top;
                break;
            case 'top-center':
                top = clamp(start.top + deltaY, 0, Math.max(0, bottom - minHeight));
                height = bottom - top;
                break;
            case 'top-right':
                top = clamp(start.top + deltaY, 0, Math.max(0, bottom - minHeight));
                height = bottom - top;
                width = clamp(start.width + deltaX, Math.min(minWidth, board.width), board.width - start.left);
                break;
            case 'left-center':
                left = clamp(start.left + deltaX, 0, Math.max(0, right - minWidth));
                width = right - left;
                break;
            case 'right-center':
                width = clamp(start.width + deltaX, Math.min(minWidth, board.width), board.width - start.left);
                break;
            case 'bottom-left':
                left = clamp(start.left + deltaX, 0, Math.max(0, right - minWidth));
                width = right - left;
                height = clamp(start.height + deltaY, Math.min(minHeight, board.height), board.height - start.top);
                break;
            case 'bottom-center':
                height = clamp(start.height + deltaY, Math.min(minHeight, board.height), board.height - start.top);
                break;
            case 'bottom-right':
                width = clamp(start.width + deltaX, Math.min(minWidth, board.width), board.width - start.left);
                height = clamp(start.height + deltaY, Math.min(minHeight, board.height), board.height - start.top);
                break;
            default:
                break;
        }

        return { left: left, top: top, width: width, height: height };
    }

    function startGesture(state, event, mode) {
        if (event.pointerType === 'mouse' && event.button !== 0) return;

        event.preventDefault();

        var target = event.currentTarget;
        var start = readCropRect(state);

        if (!start) return;

        var startX = event.clientX;
        var startY = event.clientY;
        var pointerId = event.pointerId;
        var current = start;
        var started = false;
        var frame = null;
        var lastNotify = 0;
        var latest = { x: startX, y: startY };

        var isMove = mode === 'move';
        var startMethod = isMove ? 'OnDragStart' : 'OnResizeStart';
        var moveMethod = isMove ? 'OnDrag' : 'OnResize';
        var endMethod = isMove ? 'OnDragEnd' : 'OnResizeEnd';

        if (target.setPointerCapture) {
            try { target.setPointerCapture(pointerId); } catch (error) { /* capture is optional */ }
        }

        function apply() {
            var board = { width: state.board.clientWidth, height: state.board.clientHeight };

            current = computeRect(
                mode,
                start,
                latest.x - startX,
                latest.y - startY,
                state.minCropWidth,
                state.minCropHeight,
                board);

            writeCropVars(state, current);

            if (!started) {
                started = true;
                notify(state, startMethod);
            }

            var now = Date.now();

            if (now - lastNotify >= NOTIFY_INTERVAL) {
                lastNotify = now;
                notify(state, moveMethod, toNotifyPayload(state));
            }
        }

        function onPointerMove(moveEvent) {
            if (moveEvent.pointerId !== pointerId) return;

            moveEvent.preventDefault();

            latest = { x: moveEvent.clientX, y: moveEvent.clientY };

            if (frame !== null) return;

            frame = window.requestAnimationFrame(function () {
                frame = null;
                apply();
                refreshPreview(state);
            });
        }

        function onPointerUp(upEvent) {
            if (upEvent.pointerId !== pointerId) return;

            target.removeEventListener('pointermove', onPointerMove);
            target.removeEventListener('pointerup', onPointerUp);
            target.removeEventListener('pointercancel', onPointerUp);

            if (frame !== null) {
                window.cancelAnimationFrame(frame);
                frame = null;
                apply();
            }

            if (!started) return;

            refreshPreview(state);
            notify(state, endMethod, toNotifyPayload(state));
        }

        target.addEventListener('pointermove', onPointerMove);
        target.addEventListener('pointerup', onPointerUp);
        target.addEventListener('pointercancel', onPointerUp);
    }

    function attachGesture(state, element, mode) {
        var handler = function (event) { startGesture(state, event, mode); };

        element.addEventListener('pointerdown', handler);

        state.cleanup.push(function () {
            element.removeEventListener('pointerdown', handler);
        });
    }

    function handleViewportResize(state) {
        if (state.resizeFrame !== null) return;

        state.resizeFrame = window.requestAnimationFrame(function () {
            state.resizeFrame = null;

            var current = readCropRect(state);

            if (!current) return;

            var write = clampToBoard(current, displaySize(state));

            if (write.left !== current.left || write.top !== current.top) {
                writeCropVars(state, write);
            }

            refreshPreview(state);
        });
    }

    // ---------------------------------------------------------------- public

    return {

        setFocus: function (element) {
            if (element) element.focus();
        },

        getBoundingClientRect: function (elementRef) {
            return elementRef.getBoundingClientRect();
        },

        /**
         * Decodes the source image into the editor canvas and returns the
         * geometry the component needs to render its chrome.
         */
        initializeSource: async function (dotNetHelper, id, streamReference, canvas, preview, options) {
            if (getState(id)) {
                window.dnetimageeditor.dispose(id);
            }

            options = options || {};

            var buffer = await streamReference.arrayBuffer();
            var bytes = new Uint8Array(buffer);
            var format = sniffFormat(bytes);
            var decoded = await decodeImage(bytes, format);

            // Assigning width/height also clears the canvas, so a re-opened
            // dialog can never show stale pixels.
            canvas.width = decoded.width;
            canvas.height = decoded.height;
            canvas.getContext('2d').drawImage(decoded.image, 0, 0, decoded.width, decoded.height);
            decoded.release();

            var state = {
                dotNetHelper: dotNetHelper,
                canvas: canvas,
                preview: preview,
                previewBox: {
                    width: options.previewWidth > 0 ? options.previewWidth : DEFAULT_PREVIEW_SIZE,
                    height: options.previewHeight > 0 ? options.previewHeight : DEFAULT_PREVIEW_SIZE
                },
                board: null,
                container: null,
                box: null,
                sourceFormat: format,
                minCropWidth: options.minCropWidth > 0 ? options.minCropWidth : DEFAULT_MIN_CROP,
                minCropHeight: options.minCropHeight > 0 ? options.minCropHeight : DEFAULT_MIN_CROP,
                maxOutputDimension: options.maxOutputDimension > 0 ? options.maxOutputDimension : 0,
                outputFormat: normalizeFormat(options.outputFormat),
                outputQuality: options.outputQuality > 0 ? options.outputQuality : 95,
                originalBlob: new Blob([bytes], { type: format }),
                pendingBlob: null,
                scratch: document.createElement('canvas'),
                cleanup: [],
                resizeFrame: null
            };

            editors.set(id, state);

            var display = displaySize(state);
            var cropWidth = round(Math.max(state.minCropWidth, Math.min(100, display.width)));
            var cropHeight = round(Math.max(state.minCropHeight, Math.min(100, display.height)));

            state.crop = {
                left: round(Math.max(0, (display.width - cropWidth) / 2)),
                top: round(Math.max(0, (display.height - cropHeight) / 2)),
                width: cropWidth,
                height: cropHeight
            };

            return {
                sourceWidth: decoded.width,
                sourceHeight: decoded.height,
                sourceFormat: format,
                displayWidth: display.width,
                displayHeight: display.height,
                cropLeft: state.crop.left,
                cropTop: state.crop.top,
                cropWidth: state.crop.width,
                cropHeight: state.crop.height
            };
        },

        /**
         * Wires the crop rectangle to the rendered overlay. Called once the
         * component has rendered the crop chrome.
         */
        attachCrop: function (id, board, container, box) {
            var state = getState(id);

            if (!state) throw new Error('The image editor is not initialized.');

            state.board = board;
            state.container = container;
            state.box = box;

            writeCropVars(state, state.crop);
            attachGesture(state, box, 'move');

            for (var index = 0; index < RESIZER_TYPES.length; index++) {
                var element = container.querySelector('.dnet-crop-box-resizer-' + RESIZER_TYPES[index]);

                if (element) {
                    attachGesture(state, element, RESIZER_TYPES[index]);
                }
            }

            var onViewportResize = function () { handleViewportResize(state); };
            window.addEventListener('resize', onViewportResize);
            state.cleanup.push(function () { window.removeEventListener('resize', onViewportResize); });

            refreshPreview(state);

            return toNotifyPayload(state);
        },

        /**
         * Mirrors the working image. The flip is applied to the raw pixels of
         * the canvas, so any number of flips survives without the
         * generational quality loss of re-encoding on every click.
         */
        flip: function (id, horizontal) {
            var state = getState(id);

            if (!state) throw new Error('The image editor is not initialized.');

            var canvas = state.canvas;
            var width = canvas.width;
            var height = canvas.height;
            var scratch = state.scratch;

            scratch.width = width;
            scratch.height = height;

            var scratchContext = scratch.getContext('2d');
            scratchContext.clearRect(0, 0, width, height);
            scratchContext.drawImage(canvas, 0, 0);

            var context = canvas.getContext('2d');
            context.save();
            context.setTransform(1, 0, 0, 1, 0, 0);
            context.clearRect(0, 0, width, height);
            context.translate(horizontal ? width : 0, horizontal ? 0 : height);
            context.scale(horizontal ? -1 : 1, horizontal ? 1 : -1);
            context.drawImage(scratch, 0, 0);
            context.restore();

            state.pendingBlob = null;
            refreshPreview(state);
        },

        /**
         * Encodes the current selection (or the whole image) at its native
         * resolution and keeps the result until the component asks for it.
         */
        exportImage: async function (id, useCrop) {
            var state = getState(id);

            if (!state) throw new Error('The image editor is not initialized.');

            var source = sourceRect(state, useCrop !== false);
            var format = state.outputFormat || state.sourceFormat;
            var work = document.createElement('canvas');

            work.width = source.width;
            work.height = source.height;

            var context = work.getContext('2d', { willReadFrequently: true });

            // JPEG has no alpha channel: without this, transparent pixels come
            // out black.
            if (!hasAlpha(format)) {
                context.fillStyle = '#ffffff';
                context.fillRect(0, 0, work.width, work.height);
            }

            context.drawImage(state.canvas, source.x, source.y, source.width, source.height, 0, 0, source.width, source.height);

            var result = await encodeWorkCanvas(work, format, state.outputQuality, state.maxOutputDimension);

            state.pendingBlob = result.blob;

            // `work` already holds the selection, so the preview takes all of it.
            // The optional size cap keeps the aspect ratio, so previewing the
            // uncapped canvas shows the same picture.
            drawPreview(state, work, { x: 0, y: 0, width: work.width, height: work.height });

            return {
                width: result.width,
                height: result.height,
                format: format,
                size: result.blob.size
            };
        },

        /**
         * Hands the encoded result (or the untouched original bytes when the
         * user changed nothing) back to .NET as a stream.
         */
        getResultStream: function (id) {
            var state = getState(id);

            if (!state) throw new Error('The image editor is not initialized.');

            return state.pendingBlob || state.originalBlob;
        },

        dispose: function (id) {
            var state = getState(id);

            if (!state) return;

            state.cleanup.forEach(function (remove) { remove(); });

            if (state.resizeFrame !== null) {
                window.cancelAnimationFrame(state.resizeFrame);
            }

            state.pendingBlob = null;
            state.originalBlob = null;
            state.canvas = null;
            state.box = null;
            state.board = null;
            state.container = null;
            state.preview = null;
            state.dotNetHelper = null;

            editors.delete(id);
        }
    };
})();
