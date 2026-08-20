

window.dnetoverlay = (function () {
    var viewportSubscriptions = new Map();
    var nextViewportSubscriptionId = 1;
    var interactionSubscriptions = new Map();
    var nextInteractionSubscriptionId = 1;
    var focusTraps = new Map();
    var nextFocusTrapId = 1;
    var sizeObservers = new Map();
    var nextSizeObserverId = 1;
    var fullscreenContainers = new Map();
    var nextFullscreenContainerId = 1;
    var scrollBlockState = null;

    function getViewportSize() {
        var viewport = window.visualViewport;

        return {
            Width: viewport ? viewport.width : window.innerWidth,
            Height: viewport ? viewport.height : window.innerHeight
        };
    }

    function removeViewportSubscription(subscriptionId) {
        var subscription = viewportSubscriptions.get(subscriptionId);
        if (!subscription) {
            return;
        }

        subscription.disposed = true;
        window.removeEventListener('resize', subscription.notify, { passive: true });
        window.removeEventListener('orientationchange', subscription.notify, { passive: true });

        if (window.visualViewport) {
            window.visualViewport.removeEventListener('resize', subscription.notify, { passive: true });
            window.visualViewport.removeEventListener('scroll', subscription.notify, { passive: true });
        }

        if (subscription.frameId) {
            window.cancelAnimationFrame(subscription.frameId);
        }

        viewportSubscriptions.delete(subscriptionId);
    }

    function findOverlayId(event) {
        var path = typeof event.composedPath === 'function' ? event.composedPath() : [event.target];

        for (var index = 0; index < path.length; index++) {
            var element = path[index];
            if (element && element.dataset && element.dataset.dnetOverlayId) {
                var overlayId = Number(element.dataset.dnetOverlayId);
                return Number.isSafeInteger(overlayId) ? overlayId : null;
            }
        }

        return null;
    }

    function removeInteractionSubscription(subscriptionId) {
        var subscription = interactionSubscriptions.get(subscriptionId);
        if (!subscription) {
            return;
        }

        subscription.disposed = true;
        document.removeEventListener('keydown', subscription.onKeyDown, true);
        document.removeEventListener('pointerdown', subscription.onPointerDown, true);
        document.removeEventListener('click', subscription.onClick, true);
        document.removeEventListener('auxclick', subscription.onClick, true);
        document.removeEventListener('contextmenu', subscription.onClick, true);
        document.removeEventListener('scroll', subscription.onScroll, true);

        if (subscription.scrollFrameId) {
            window.cancelAnimationFrame(subscription.scrollFrameId);
        }

        interactionSubscriptions.delete(subscriptionId);
    }

    function setDocumentScrollBlocked(blocked) {
        var documentElement = document.documentElement;
        var body = document.body;

        if (blocked && !scrollBlockState) {
            var scrollbarWidth = Math.max(0, window.innerWidth - documentElement.clientWidth);
            scrollBlockState = {
                documentElementOverflow: documentElement.style.overflow,
                bodyOverflow: body.style.overflow,
                bodyPaddingRight: body.style.paddingRight
            };
            documentElement.style.overflow = 'hidden';
            body.style.overflow = 'hidden';
            if (scrollbarWidth) {
                body.style.paddingRight = scrollbarWidth + 'px';
            }
        } else if (!blocked && scrollBlockState) {
            documentElement.style.overflow = scrollBlockState.documentElementOverflow;
            body.style.overflow = scrollBlockState.bodyOverflow;
            body.style.paddingRight = scrollBlockState.bodyPaddingRight;
            scrollBlockState = null;
        }
    }

    function getFocusableElements(element) {
        return Array.prototype.slice.call(element.querySelectorAll(
            'a[href], area[href], input:not([disabled]):not([type="hidden"]), select:not([disabled]), textarea:not([disabled]), button:not([disabled]), iframe, object, embed, [contenteditable], [tabindex]:not([tabindex="-1"])'))
            .filter(function (candidate) {
                return !candidate.hasAttribute('hidden') && candidate.getClientRects().length > 0;
            });
    }

    function deactivateFocusTrap(focusTrapId) {
        var trap = focusTraps.get(focusTrapId);
        if (!trap) {
            return;
        }

        trap.element.removeEventListener('keydown', trap.onKeyDown, true);
        focusTraps.delete(focusTrapId);

        if (trap.restoreFocus && trap.previousFocus && trap.previousFocus.isConnected && typeof trap.previousFocus.focus === 'function') {
            trap.previousFocus.focus({ preventScroll: true });
        }
    }

    function stopObservingOverlaySize(observerId) {
        var subscription = sizeObservers.get(observerId);
        if (!subscription) {
            return;
        }

        subscription.disposed = true;
        subscription.observer.disconnect();
        if (subscription.frameId) {
            window.cancelAnimationFrame(subscription.frameId);
        }
        sizeObservers.delete(observerId);
    }

    function disposeFullscreenContainer(subscriptionId) {
        var subscription = fullscreenContainers.get(subscriptionId);
        if (!subscription) {
            return;
        }

        document.removeEventListener('fullscreenchange', subscription.adjustParent);
        document.removeEventListener('webkitfullscreenchange', subscription.adjustParent);
        if (subscription.originalParent && subscription.originalParent.isConnected) {
            subscription.originalParent.insertBefore(
                subscription.container,
                subscription.originalNextSibling && subscription.originalNextSibling.parentNode === subscription.originalParent
                    ? subscription.originalNextSibling
                    : null);
        }
        fullscreenContainers.delete(subscriptionId);
    }

    return {

        addWindowEventListeners: function (dotnetClass) {
            var subscriptionId = nextViewportSubscriptionId++;
            var subscription = {
                disposed: false,
                frameId: 0,
                notify: null
            };

            subscription.notify = function () {
                if (subscription.disposed || subscription.frameId) {
                    return;
                }

                subscription.frameId = window.requestAnimationFrame(function () {
                    subscription.frameId = 0;
                    if (subscription.disposed) {
                        return;
                    }

                    dotnetClass.invokeMethodAsync('OnWindowResized', getViewportSize())
                        .catch(function () {
                            // The .NET instance may have been disposed between event delivery
                            // and invocation. Remove this listener immediately in that case.
                            removeViewportSubscription(subscriptionId);
                        });
                });
            };

            window.addEventListener('resize', subscription.notify, { passive: true });
            window.addEventListener('orientationchange', subscription.notify, { passive: true });

            if (window.visualViewport) {
                window.visualViewport.addEventListener('resize', subscription.notify, { passive: true });
                window.visualViewport.addEventListener('scroll', subscription.notify, { passive: true });
            }

            viewportSubscriptions.set(subscriptionId, subscription);
            return subscriptionId;
        },

        removeWindowEventListeners: function (subscriptionId) {
            removeViewportSubscription(subscriptionId);
        },

        addInteractionEventListeners: function (dotnetClass) {
            var subscriptionId = nextInteractionSubscriptionId++;
            var subscription = {
                disposed: false,
                pointerDownOverlayId: null,
                scrollFrameId: 0,
                onKeyDown: null,
                onPointerDown: null,
                onClick: null,
                onScroll: null
            };

            function invoke(method, args) {
                if (subscription.disposed) {
                    return;
                }

                dotnetClass.invokeMethodAsync(method, args).catch(function () {
                    removeInteractionSubscription(subscriptionId);
                });
            }

            subscription.onKeyDown = function (event) {
                invoke('OnDocumentKeyDown', { Key: event.key, DefaultPrevented: event.defaultPrevented });
            };

            subscription.onPointerDown = function (event) {
                subscription.pointerDownOverlayId = findOverlayId(event);
            };

            subscription.onClick = function (event) {
                var pointerDownOverlayId = subscription.pointerDownOverlayId;
                subscription.pointerDownOverlayId = null;
                invoke('OnOutsidePointer', {
                    PointerDownOverlayId: pointerDownOverlayId,
                    TargetOverlayId: findOverlayId(event)
                });
            };

            subscription.onScroll = function () {
                if (subscription.disposed || subscription.scrollFrameId) {
                    return;
                }

                subscription.scrollFrameId = window.requestAnimationFrame(function () {
                    subscription.scrollFrameId = 0;
                    invoke('OnDocumentScrolled', { SourceOverlayId: findOverlayId(event) });
                });
            };

            document.addEventListener('keydown', subscription.onKeyDown, true);
            document.addEventListener('pointerdown', subscription.onPointerDown, true);
            document.addEventListener('click', subscription.onClick, true);
            document.addEventListener('auxclick', subscription.onClick, true);
            document.addEventListener('contextmenu', subscription.onClick, true);
            document.addEventListener('scroll', subscription.onScroll, { capture: true, passive: true });
            interactionSubscriptions.set(subscriptionId, subscription);
            return subscriptionId;
        },

        removeInteractionEventListeners: function (subscriptionId) {
            removeInteractionSubscription(subscriptionId);
        },

        setDocumentScrollBlocked: function (blocked) {
            setDocumentScrollBlocked(blocked);
        },

        activateFocusTrap: function (element, initialFocusSelector, restoreFocus) {
            if (!element) {
                return 0;
            }

            var focusTrapId = nextFocusTrapId++;
            var trap = {
                element: element,
                previousFocus: document.activeElement,
                restoreFocus: restoreFocus,
                onKeyDown: null
            };

            trap.onKeyDown = function (event) {
                if (event.key !== 'Tab') {
                    return;
                }

                var focusable = getFocusableElements(element);
                if (!focusable.length) {
                    event.preventDefault();
                    element.focus({ preventScroll: true });
                    return;
                }

                var currentIndex = focusable.indexOf(document.activeElement);
                if (event.shiftKey && (currentIndex <= 0 || !element.contains(document.activeElement))) {
                    event.preventDefault();
                    focusable[focusable.length - 1].focus();
                } else if (!event.shiftKey && (currentIndex === -1 || currentIndex === focusable.length - 1)) {
                    event.preventDefault();
                    focusable[0].focus();
                }
            };

            element.addEventListener('keydown', trap.onKeyDown, true);
            focusTraps.set(focusTrapId, trap);

            window.requestAnimationFrame(function () {
                if (!focusTraps.has(focusTrapId) || !element.isConnected) {
                    return;
                }

                var initialFocus = initialFocusSelector ? element.querySelector(initialFocusSelector) : null;
                var focusTarget = initialFocus || getFocusableElements(element)[0] || element;
                if (typeof focusTarget.focus === 'function') {
                    focusTarget.focus({ preventScroll: true });
                }
            });

            return focusTrapId;
        },

        deactivateFocusTrap: function (focusTrapId) {
            deactivateFocusTrap(focusTrapId);
        },

        observeOverlaySize: function (pane, origin, dotnetClass) {
            if (!pane || typeof ResizeObserver === 'undefined') {
                return 0;
            }

            var observerId = nextSizeObserverId++;
            var subscription = {
                disposed: false,
                frameId: 0,
                observer: null
            };
            subscription.observer = new ResizeObserver(function () {
                if (subscription.disposed || subscription.frameId) {
                    return;
                }

                subscription.frameId = window.requestAnimationFrame(function () {
                    subscription.frameId = 0;
                    if (subscription.disposed) {
                        return;
                    }

                    dotnetClass.invokeMethodAsync('OnOverlayResized').catch(function () {
                        stopObservingOverlaySize(observerId);
                    });
                });
            });
            subscription.observer.observe(pane);
            if (origin && origin !== pane) {
                subscription.observer.observe(origin);
            }
            sizeObservers.set(observerId, subscription);
            return observerId;
        },

        stopObservingOverlaySize: function (observerId) {
            stopObservingOverlaySize(observerId);
        },

        initializeFullscreenContainer: function (container) {
            if (!container) {
                return 0;
            }

            var subscriptionId = nextFullscreenContainerId++;
            var subscription = {
                container: container,
                originalParent: container.parentNode,
                originalNextSibling: container.nextSibling,
                adjustParent: function () {
                    var fullscreenElement = document.fullscreenElement || document.webkitFullscreenElement;
                    if (fullscreenElement) {
                        fullscreenElement.appendChild(container);
                    } else if (subscription.originalParent && subscription.originalParent.isConnected) {
                        subscription.originalParent.insertBefore(
                            container,
                            subscription.originalNextSibling && subscription.originalNextSibling.parentNode === subscription.originalParent
                                ? subscription.originalNextSibling
                                : null);
                    }
                }
            };

            subscription.adjustParent();
            document.addEventListener('fullscreenchange', subscription.adjustParent);
            document.addEventListener('webkitfullscreenchange', subscription.adjustParent);
            fullscreenContainers.set(subscriptionId, subscription);
            return subscriptionId;
        },

        disposeFullscreenContainer: function (subscriptionId) {
            disposeFullscreenContainer(subscriptionId);
        },

        getViewportScrollPosition: function () {

            var documentElement = document.documentElement;

            var documentRect = documentElement.getBoundingClientRect();

            var top = -documentRect.top || document.body.scrollTop || window.scrollY ||
                documentElement.scrollTop || 0;

            var left = -documentRect.left || document.body.scrollLeft || window.scrollX || documentElement.scrollLeft || 0;

            return { Top: top, Left: left };
        },

        getViewportSize: function () {

            return getViewportSize();
        },

        getViewportSizeNoScroll: function () {

            return { Width: document.documentElement.clientWidth, Height: document.documentElement.clientHeight };
        },

        getBoundingClientRect: function (elementRef) {

            if (!elementRef) return null;

            var tt = elementRef.getBoundingClientRect();

            return tt;
        },

        getDocumentBoundingClientRect: function () {

            var documentElement = document.documentElement;

            var documentRect = documentElement.getBoundingClientRect();

            return documentRect;
        },

        getDocumentClientHeight: function () {

            return document.documentElement.clientHeight;
        },

        getDocumentClientWidth: function () {

            return document.documentElement.clientWidth;
        },

        getWindowWidth: function () {

            return window.innerWidth;
        },

        getContainerWidth: function (containerId) {
            const container = document.getElementById(containerId);
            if (container) {
                return container.offsetWidth;
            }
            return 0;
        }

    };
})();
