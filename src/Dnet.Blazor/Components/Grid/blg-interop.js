window.blginterop = (function () {

    const observersByDotNetId = {};

    const listenerStateByElement = new WeakMap();

    function findClosestScrollContainer(element) {

        if (!element) {
            return null;
        }

        const style = getComputedStyle(element);

        if (style.overflowY !== 'visible') {
            return element;
        }

        return findClosestScrollContainer(element.parentElement);
    };

    // e1: MouseEvent | Touch, e2: MouseEvent | Touch, pixelCount: number
    function areEventsNear(e1, e2, pixelCount) {
        // by default, we wait 4 pixels before starting the drag

        if (pixelCount === 0) { return false; }

        const diffX = Math.abs(e1.clientX - e2.clientX);
        const diffY = Math.abs(e1.clientY - e2.clientY);

        return Math.max(diffX, diffY) <= pixelCount;
    }

    function getActiveTouch(touchList, touchStart) {
        for (let i = 0; i < touchList.length; i++) {
            const matches = touchList[i].identifier === touchStart.identifier;
            if (matches) {
                return touchList[i];
            }
        }

        return null;
    }

    function addTouchListeners(elementRef, scrollElementRef, dotNetReference) {

        let startX, startY, scrollStartX, touchStart = null;
        let touching = false;
        let moved = false;
        let pendingScrollInfo = null;
        let animationFrame = null;

        function notifyTouchScroll() {
            animationFrame = null;
            if (pendingScrollInfo === null) return;

            dotNetReference.invokeMethodAsync('OnTouchMove', pendingScrollInfo);
            pendingScrollInfo = null;
        }

        const touchStartHandler = function (e) {
            if (touching || e.touches.length === 0) return;

            // On narrow viewports the complete grid canvas owns horizontal
            // scrolling and the desktop scrollbar is hidden. Let the browser
            // handle that native gesture instead of forwarding it to Blazor.
            if (scrollElementRef.clientWidth === 0 || getComputedStyle(scrollElementRef).display === 'none') return;

            startX = e.touches[0].clientX;
            startY = e.touches[0].clientY;
            scrollStartX = scrollElementRef.scrollLeft;
            touching = true;
            touchStart = e.touches[0];
            moved = false;
        };

        const touchMoveHandler = function (e) {
            if (!touching || touchStart === null) return;

            const activeTouch = getActiveTouch(e.touches, touchStart);
            if (!activeTouch) return;

            moved = moved || !areEventsNear(activeTouch, touchStart, 4);
            const deltaX = activeTouch.clientX - startX;
            const deltaY = activeTouch.clientY - startY;
            const threshold = 10;

            if (Math.abs(deltaX) <= Math.abs(deltaY) || Math.abs(deltaX) <= threshold) return;

            e.preventDefault();
            const maxScrollLeft = scrollElementRef.scrollWidth - scrollElementRef.clientWidth;
            scrollElementRef.scrollLeft = scrollStartX - deltaX;
            const elementScrollLeft = scrollElementRef.scrollLeft;

            if ((elementScrollLeft === 0 && deltaX < 0) || (elementScrollLeft >= maxScrollLeft && deltaX > 0)) return;

            pendingScrollInfo = {
                maxScrollLeft,
                deltaX,
                elementScrollLeft
            };

            if (animationFrame === null) {
                animationFrame = window.requestAnimationFrame(notifyTouchScroll);
            }
        };

        const touchEndHandler = function () {
            touching = false;
            touchStart = null;
            pendingScrollInfo = null;
        };

        elementRef.addEventListener('touchstart', touchStartHandler, { passive: true });
        elementRef.addEventListener('touchmove', touchMoveHandler, { passive: false });
        elementRef.addEventListener('touchend', touchEndHandler, { passive: true });

        const state = listenerStateByElement.get(elementRef) || {};
        state.touch = {
            touchStartHandler,
            touchMoveHandler,
            touchEndHandler,
            cancelAnimationFrame: () => {
                if (animationFrame !== null) window.cancelAnimationFrame(animationFrame);
            }
        };
        listenerStateByElement.set(elementRef, state);
    }

    return {

        addTouchListeners: function (elementRef, scrollElementRef, dotNetReference) {
            addTouchListeners(elementRef, scrollElementRef, dotNetReference);
            return true;
        },

        addWindowEventListeners: function (elementRef, dotnetClass) {
            const mouseLeaveHandler = function () {
                dotnetClass.invokeMethodAsync('MouseLeave');
            };
            let hoveredRowId = null;

            const setHoveredRow = function (rowId) {
                if (hoveredRowId === rowId) return;

                if (hoveredRowId !== null) {
                    elementRef.querySelectorAll(`[data-blg-row-id="${hoveredRowId}"]`)
                        .forEach(cell => cell.classList.remove('blg-hover-class'));
                }

                hoveredRowId = rowId;
                if (hoveredRowId !== null) {
                    elementRef.querySelectorAll(`[data-blg-row-id="${hoveredRowId}"]`)
                        .forEach(cell => cell.classList.add('blg-hover-class'));
                }
            };

            const mouseOverHandler = function (event) {
                const cell = event.target.closest('[data-blg-row-id]');
                if (cell && elementRef.contains(cell)) {
                    setHoveredRow(cell.dataset.blgRowId);
                }
            };

            const clearHoveredRow = function () {
                setHoveredRow(null);
            };

            const keyDownHandler = function (event) {
                const cell = event.target.closest('[role="gridcell"]');
                const center = elementRef.querySelector('.blg-center-cols-container');
                if (!cell || !center || !center.contains(cell)) return;

                const cells = Array.from(center.querySelectorAll('[role="gridcell"]'));
                const row = Number(cell.getAttribute('aria-rowindex'));
                const column = Number(cell.getAttribute('aria-colindex'));
                let target = null;

                const sameRow = candidate => Number(candidate.getAttribute('aria-rowindex')) === row;
                const candidatesInRow = cells.filter(sameRow);
                if (event.key === 'ArrowLeft') {
                    target = candidatesInRow.filter(candidate => Number(candidate.getAttribute('aria-colindex')) < column).pop();
                } else if (event.key === 'ArrowRight') {
                    target = candidatesInRow.find(candidate => Number(candidate.getAttribute('aria-colindex')) > column);
                } else if (event.key === 'ArrowUp') {
                    target = cells.find(candidate => Number(candidate.getAttribute('aria-rowindex')) === row - 1 && Number(candidate.getAttribute('aria-colindex')) === column);
                } else if (event.key === 'ArrowDown') {
                    target = cells.find(candidate => Number(candidate.getAttribute('aria-rowindex')) === row + 1 && Number(candidate.getAttribute('aria-colindex')) === column);
                } else if (event.key === 'Home') {
                    target = event.ctrlKey ? cells[0] : candidatesInRow[0];
                } else if (event.key === 'End') {
                    target = event.ctrlKey ? cells[cells.length - 1] : candidatesInRow[candidatesInRow.length - 1];
                } else if (event.key === 'PageUp' || event.key === 'PageDown') {
                    const visibleRows = [...new Set(cells.map(candidate => candidate.getAttribute('aria-rowindex')))];
                    const currentRow = visibleRows.indexOf(String(row));
                    const pageRow = event.key === 'PageUp'
                        ? visibleRows[Math.max(0, currentRow - Math.max(1, visibleRows.length - 1))]
                        : visibleRows[Math.min(visibleRows.length - 1, currentRow + Math.max(1, visibleRows.length - 1))];
                    target = cells.find(candidate => candidate.getAttribute('aria-rowindex') === pageRow && Number(candidate.getAttribute('aria-colindex')) === column);
                } else {
                    return;
                }

                event.preventDefault();
                if (!target) return;

                cells.forEach(candidate => candidate.setAttribute('tabindex', '-1'));
                target.setAttribute('tabindex', '0');
                target.focus();
            };

            elementRef.addEventListener("mouseleave", mouseLeaveHandler);
            elementRef.addEventListener("mouseover", mouseOverHandler);
            elementRef.addEventListener("mouseleave", clearHoveredRow);
            elementRef.addEventListener("keydown", keyDownHandler);
            const state = listenerStateByElement.get(elementRef) || {};
            state.mouseLeaveHandler = mouseLeaveHandler;
            state.mouseOverHandler = mouseOverHandler;
            state.clearHoveredRow = clearHoveredRow;
            state.keyDownHandler = keyDownHandler;
            listenerStateByElement.set(elementRef, state);

            return true;
        },

        removeEventListeners: function (elementRef) {
            const state = listenerStateByElement.get(elementRef);
            if (!state) return;

            if (state.mouseLeaveHandler) {
                elementRef.removeEventListener("mouseleave", state.mouseLeaveHandler);
            }
            if (state.mouseOverHandler) {
                elementRef.removeEventListener("mouseover", state.mouseOverHandler);
            }
            if (state.clearHoveredRow) {
                elementRef.removeEventListener("mouseleave", state.clearHoveredRow);
            }
            if (state.keyDownHandler) {
                elementRef.removeEventListener("keydown", state.keyDownHandler);
            }
            if (state.touch) {
                elementRef.removeEventListener('touchstart', state.touch.touchStartHandler);
                elementRef.removeEventListener('touchmove', state.touch.touchMoveHandler);
                elementRef.removeEventListener('touchend', state.touch.touchEndHandler);
                state.touch.cancelAnimationFrame();
            }
            listenerStateByElement.delete(elementRef);
        },

        getElementScrollLeft: function (elementRef) {
            return elementRef.scrollLeft;
        },

        getBoundingClientRect: function (elementRef) {
            return elementRef.getBoundingClientRect();
        },

        getElementScrollWidth: function (elementRef) {
            let scrollWidth = elementRef.scrollWidth;

            return scrollWidth;
        },

        getHeaderWidth: function (id) {

            let parent = document.getElementById(id);
            let elements = parent.getElementsByClassName("blg-header-cell blg-header-cell-notpinned");

            let headerWidth = 0;

            for (var i = 0; i < elements.length; i++) {
                headerWidth = headerWidth + elements[i].clientWidth;
            }

            return headerWidth;
        },

        init: function (dotNetHelper, spacerBefore, spacerAfter, rootMargin = 50) {

            const scrollContainer = findClosestScrollContainer(spacerBefore);
            (scrollContainer || document.documentElement).style.overflowAnchor = 'none';

            // The Grid passes a margin derived from half of its overscan buffer.
            // This gives its comparatively expensive row tree time to render
            // before a spacer reaches the visible viewport during fast scrolling.
            const intersectionObserver = new window.IntersectionObserver(intersectionCallback, {
                root: scrollContainer,
                rootMargin: `${rootMargin}px`,
                threshold: 0
            });

            intersectionObserver.observe(spacerBefore);

            intersectionObserver.observe(spacerAfter);

            const mutationObserverBefore = createSpacerMutationObserver(spacerBefore);

            const mutationObserverAfter = createSpacerMutationObserver(spacerAfter);

            observersByDotNetId[dotNetHelper._id] = {
                intersectionObserver,
                mutationObserverBefore,
                mutationObserverAfter
            };

            function createSpacerMutationObserver(spacer) {
                // Without the use of thresholds, IntersectionObserver only detects binary changes in visibility,
                // so if a spacer gets resized but remains visible, no additional callbacks will occur. By unobserving
                // and reobserving spacers when they get resized, the intersection callback will re-run if they remain visible.
                const mutationObserver = new window.MutationObserver(() => {
                    intersectionObserver.unobserve(spacer);
                    intersectionObserver.observe(spacer);
                });

                mutationObserver.observe(spacer, { attributes: true });

                return mutationObserver;
            }

            function intersectionCallback(entries) {

                entries.forEach((entry) => {

                    if (!entry.isIntersecting) {
                        return;
                    }

                    const spacerBeforeRect = spacerBefore.getBoundingClientRect();
                    const spacerAfterRect = spacerAfter.getBoundingClientRect();
                    const spacerSeparation = spacerAfterRect.top - spacerBeforeRect.bottom;
                    const containerSize = entry.rootBounds.height;

                    if (entry.target === spacerBefore) {
                        dotNetHelper.invokeMethodAsync('OnSpacerBeforeVisible', entry.intersectionRect.top - entry.boundingClientRect.top, spacerSeparation, containerSize);
                    }

                    else if (entry.target === spacerAfter && spacerAfter.offsetHeight > 0) {
                        // When we first start up, both the "before" and "after" spacers will be visible, but it's only relevant to raise a
                        // single event to load the initial data. To avoid raising two events, skip the one for the "after" spacer if we know
                        // it's meaningless to talk about any overlap into it.
                        dotNetHelper.invokeMethodAsync('OnSpacerAfterVisible', entry.boundingClientRect.bottom - entry.intersectionRect.bottom, spacerSeparation, containerSize);
                    }
                });
            }
        },

        dispose: function (dotNetHelper) {

            const observers = observersByDotNetId[dotNetHelper._id];

            if (observers) {

                observers.intersectionObserver.disconnect();
                observers.mutationObserverBefore.disconnect();
                observers.mutationObserverAfter.disconnect();

                dotNetHelper.dispose();

                delete observersByDotNetId[dotNetHelper._id];
            }
        }
    };
})();
