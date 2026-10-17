// A light-DOM custom element keeps observers tied to each viewport's lifetime,
// including Blazor navigation, conditional rendering and item replacement.
class DnetPillTabsViewport extends HTMLElement {
    connectedCallback() {
        this.observedTabs = new Set();
        this.refreshTargets = true;
        this.revealSelection = true;
        this.schedule = () => {
            if (!this.frame) this.frame = requestAnimationFrame(() => this.update());
        };
        this.resizeObserver = new ResizeObserver(this.schedule);
        this.resizeObserver.observe(this);
        this.mutationObserver = new MutationObserver(records => {
            this.refreshTargets = true;
            this.revealSelection ||= records.some(record =>
                ["aria-selected", "aria-pressed"].includes(record.attributeName) &&
                record.target.getAttribute(record.attributeName) === "true");
            this.schedule();
        });
        this.mutationObserver.observe(this, {
            subtree: true, childList: true, characterData: true,
            attributes: true, attributeFilter: ["aria-selected", "aria-pressed", "dir"]
        });
        this.mutationObserver.observe(this.parentElement, {
            attributes: true, attributeFilter: ["dir"]
        });
        this.onFocus = event => {
            if (event.target.matches(".dnet-pill-tabs-tab")) {
                this.revealSelection = true;
                this.schedule();
            }
        };
        this.onWindowResize = () => {
            this.revealSelection = true;
            this.schedule();
        };
        this.addEventListener("scroll", this.schedule, { capture: true, passive: true });
        this.addEventListener("focusin", this.onFocus);
        window.addEventListener("resize", this.onWindowResize, { passive: true });
        this.schedule();
    }

    disconnectedCallback() {
        this.resizeObserver.disconnect();
        this.mutationObserver.disconnect();
        this.removeEventListener("scroll", this.schedule, true);
        this.removeEventListener("focusin", this.onFocus);
        window.removeEventListener("resize", this.onWindowResize);
        cancelAnimationFrame(this.frame);
        this.frame = 0;
        this.observedTabs.clear();
    }

    update() {
        this.frame = 0;
        const list = this.querySelector(":scope > .dnet-pill-tabs-list");
        if (!list) return;
        if (this.refreshTargets) {
            const tabs = new Set(list.querySelectorAll(":scope > .dnet-pill-tabs-tab"));
            for (const tab of this.observedTabs) {
                if (!tabs.has(tab)) this.resizeObserver.unobserve(tab);
            }
            for (const tab of tabs) {
                if (!this.observedTabs.has(tab)) this.resizeObserver.observe(tab);
            }
            this.observedTabs = tabs;
            this.refreshTargets = false;
        }

        const bounds = list.getBoundingClientRect();
        if (this.revealSelection && list.clientWidth > 0) {
            const tab = list.contains(document.activeElement) ? document.activeElement
                : list.querySelector('[aria-selected="true"], [aria-pressed="true"]');
            if (tab) {
                const rect = tab.getBoundingClientRect();
                const inset = parseFloat(getComputedStyle(list).scrollPaddingLeft) || 0;
                const delta = rect.left < bounds.left + inset ? rect.left - bounds.left - inset
                    : rect.right > bounds.right - inset ? rect.right - bounds.right + inset : 0;
                if (delta) list.scrollBy({ left: delta, behavior: "instant" });
            }
            this.revealSelection = false;
        }

        // Physical bounds avoid scrollLeft's different sign in RTL, and ignore
        // subpixel rounding at either end of the scroll range.
        const first = list.firstElementChild?.getBoundingClientRect();
        const last = list.lastElementChild?.getBoundingClientRect();
        const overflow = first && last && list.clientWidth > 0 && list.scrollWidth > list.clientWidth + 1;
        this.classList.toggle("dnet-pill-tabs-overflow-left", Boolean(overflow &&
            Math.min(first.left, last.left) < bounds.left - 1));
        this.classList.toggle("dnet-pill-tabs-overflow-right", Boolean(overflow &&
            Math.max(first.right, last.right) > bounds.right + 1));
    }
}

if (!customElements.get("dnet-pill-tabs-viewport")) {
    customElements.define("dnet-pill-tabs-viewport", DnetPillTabsViewport);
}

// Native clicks keep selection in the Blazor binding.
document.addEventListener("keydown", event => {
    if (event.altKey || event.ctrlKey || event.metaKey ||
        !["ArrowLeft", "ArrowRight", "Home", "End"].includes(event.key) ||
        !(event.target instanceof Element)) return;

    const current = event.target.closest("button.dnet-pill-tabs-tab");
    const list = current?.parentElement;
    if (!list?.classList.contains("dnet-pill-tabs-list") || current.disabled) return;

    const tabs = Array.from(list.children).filter(tab =>
        tab.matches("button.dnet-pill-tabs-tab:not(:disabled)"));
    const index = tabs.indexOf(current);
    if (index < 0) return;

    const rtl = getComputedStyle(list).direction === "rtl";
    const forward = event.key === "ArrowRight" ? !rtl : rtl;
    const next = event.key === "Home" ? tabs[0]
        : event.key === "End" ? tabs[tabs.length - 1]
        : tabs[(index + (forward ? 1 : -1) + tabs.length) % tabs.length];

    event.preventDefault();
    next.focus();
    next.click();
});
