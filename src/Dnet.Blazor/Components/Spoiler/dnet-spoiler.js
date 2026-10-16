const focusableSelector = 'a[href], button, input, select, textarea, summary, iframe, [tabindex], [contenteditable="true"], audio[controls], video[controls]';

// Light DOM preserves Blazor event handlers. Native element lifecycle callbacks
// release observers when an instance is removed or the user navigates away.
class DnetSpoilerElement extends HTMLElement {
    static get observedAttributes() { return ["data-expanded", "data-max-height"]; }

    connectedCallback() {
        this.blockedElements = new Map();
        this.resizeObserver = new ResizeObserver(() => this.schedule());
        this.mutationObserver = new MutationObserver(() => this.schedule());
        this.mutationObserver.observe(this, { childList: true, subtree: true, characterData: true });
        this.schedule();
    }

    attributeChangedCallback() { this.schedule(); }

    disconnectedCallback() {
        this.resizeObserver.disconnect();
        this.mutationObserver.disconnect();
        cancelAnimationFrame(this.frame);
        this.frame = 0;
        this.body = null;
        this.restoreFocusability();
    }

    schedule() {
        if (this.isConnected && !this.frame) {
            this.frame = requestAnimationFrame(() => this.update());
        }
    }

    restoreFocusability() {
        for (const [element, inert] of this.blockedElements) element.inert = inert;
        this.blockedElements.clear();
    }

    update() {
        this.frame = 0;
        const viewport = this.querySelector(":scope > .dnet-spoiler-viewport");
        const body = viewport?.querySelector(":scope > .dnet-spoiler-body");
        const toggle = this.querySelector(":scope > .dnet-spoiler-toggle");
        if (!body || !toggle) return;
        if (this.body !== body) {
            this.restoreFocusability();
            this.resizeObserver.disconnect();
            this.resizeObserver.observe(body);
            this.body = body;
        }

        const maxHeight = Number(this.dataset.maxHeight);
        const fullHeight = Math.max(body.scrollHeight, body.offsetHeight);
        const overflow = fullHeight > maxHeight + 1;
        const expanded = this.dataset.expanded === "true";
        const collapsed = overflow && !expanded;
        const active = document.activeElement;
        const edge = viewport.getBoundingClientRect().top + maxHeight;

        // Offscreen links and form controls must not become invisible tab stops.
        // Preserve explicitly inert descendants when revealing the content again.
        for (const [element, inert] of this.blockedElements) {
            if (!collapsed || !body.contains(element) || element.getBoundingClientRect().bottom <= edge + 1) {
                element.inert = inert;
                this.blockedElements.delete(element);
            }
        }
        if (collapsed) {
            for (const element of body.querySelectorAll(focusableSelector)) {
                if (!(element instanceof HTMLElement) || element.getBoundingClientRect().bottom <= edge + 1) continue;
                if (element === active || element.contains(active)) toggle.focus({ preventScroll: true });
                if (!this.blockedElements.has(element)) this.blockedElements.set(element, element.inert);
                element.inert = true;
            }
        }
        if (!overflow && active === toggle) viewport.focus({ preventScroll: true });

        viewport.style.setProperty("--_height", `${collapsed ? maxHeight : fullHeight}px`);
        this.dataset.overflow = String(overflow);
    }
}

if (!customElements.get("dnet-spoiler")) {
    customElements.define("dnet-spoiler", DnetSpoilerElement);
}
