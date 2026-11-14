window.dnetinterop = (function () {

    return {

        get: key => key in localStorage ? JSON.parse(localStorage[key]) : null,

        set: (key, value) => { localStorage[key] = JSON.stringify(value); },

        delete: key => { delete localStorage[key]; },

        getElementScrollLeft: function (elementRef) {
            if (elementRef != null) {
                return elementRef.scrollLeft;
            } else {
                return 0;
            }
        },

        getBoundingClientRect: function (elementRef) {
            if (elementRef != null) {
                return elementRef.getBoundingClientRect();
            } else {
                return null;
            }
        },

        getElementScrollWidth: function (elementRef) {
            if (elementRef != null) {
                return elementRef.scrollWidth;
            } else {
                return 0;
            }
        },

        getElementSOffsets: function (elementRef) {

            if (elementRef == null) {
                return {
                    offsetWidth: 0,
                    offsetHeight: 0,
                    offsetTop: 0,
                    offsetLeft: 0
                };
            } else {
                return {
                    offsetWidth: elementRef.offsetWidth,
                    offsetHeight: elementRef.offsetHeight,
                    offsetTop: elementRef.offsetTop,
                    offsetLeft: elementRef.offsetLeft
                };
            }
        },

        setStyleProperty: function (name, value) {
            document.documentElement.style.setProperty(name, value);
        },

        setTheme: function (theme) {
            document.documentElement.dataset.dnetTheme = theme;
        },

        addAriaDescribedBy: function (elementRef, tooltipId) {
            if (!elementRef || !tooltipId) {
                return;
            }

            const describedBy = new Set((elementRef.getAttribute("aria-describedby") || "").split(/\s+/).filter(Boolean));
            describedBy.add(tooltipId);
            elementRef.setAttribute("aria-describedby", [...describedBy].join(" "));
        },

        removeAriaDescribedBy: function (elementRef, tooltipId) {
            if (!elementRef || !tooltipId) {
                return;
            }

            const describedBy = (elementRef.getAttribute("aria-describedby") || "").split(/\s+/).filter(id => id && id !== tooltipId);
            if (describedBy.length) {
                elementRef.setAttribute("aria-describedby", describedBy.join(" "));
            } else {
                elementRef.removeAttribute("aria-describedby");
            }
        },

        scrollElementIntoViewById: function (elementId) {
            const element = document.getElementById(elementId);
            if (element) {
                element.scrollIntoView({ block: "nearest" });
            }
        },

        focusElementById: function (elementId) {
            const element = document.getElementById(elementId);
            if (element) {
                element.focus({ preventScroll: true });
            }
        },

        focusElementByIdAfterRender: function (elementId) {
            window.requestAnimationFrame(function () {
                window.requestAnimationFrame(function () {
                    const element = document.getElementById(elementId);
                    if (element) {
                        element.focus({ preventScroll: true });
                    }
                });
            });
        },

        matchesMedia: function (query) {
            return window.matchMedia(query).matches;
        },

        // Calls back once the element can scroll. An overlay renders its content
        // before it gets its size, so scrolling the list to an item has to wait.
        // Gives up after a few seconds, or when the element leaves the page.
        whenScrollable: function (element, dotNetRef, methodName) {
            if (!element || !dotNetRef) {
                return;
            }

            let done = false;
            let observer = null;
            let timeout = null;
            const finish = function (notify) {
                if (done) {
                    return;
                }
                done = true;
                if (observer) {
                    observer.disconnect();
                }
                window.clearTimeout(timeout);
                if (notify && element.isConnected) {
                    dotNetRef.invokeMethodAsync(methodName).catch(function () { });
                }
            };
            const check = function () {
                if (!element.isConnected) {
                    finish(false);
                } else if (element.scrollHeight > element.clientHeight + 1) {
                    finish(true);
                }
            };

            timeout = window.setTimeout(function () { finish(false); }, 3000);
            if (typeof ResizeObserver !== "undefined") {
                observer = new ResizeObserver(check);
                observer.observe(element);
            }
            check();
        },

        copyText: async function (text) {
            await navigator.clipboard.writeText(text);
        }
    };
})();
