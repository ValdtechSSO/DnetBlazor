const themeWatchers = new Map();
let nextThemeWatcherId = 1;

function toHexColor(value) {
    const color = value.trim();
    const shortHex = /^#([\da-f]{3})(?:[\da-f])?$/i.exec(color);
    if (shortHex) {
        return `#${[...shortHex[1]].map(channel => channel + channel).join("")}`.toLowerCase();
    }

    const fullHex = /^#([\da-f]{6})(?:[\da-f]{2})?$/i.exec(color);
    if (fullHex) {
        return `#${fullHex[1]}`.toLowerCase();
    }

    const probe = document.createElement("span");
    probe.style.color = color;
    probe.style.display = "none";
    document.documentElement.appendChild(probe);
    const resolved = getComputedStyle(probe).color;
    probe.remove();

    const rgb = /^rgba?\(([^)]+)\)$/i.exec(resolved);
    if (rgb) {
        const channels = rgb[1].split(/[\s,\/]+/).slice(0, 3);
        return `#${channels.map(channel => {
            const number = parseFloat(channel) * (channel.endsWith("%") ? 2.55 : 1);
            return Math.round(Math.max(0, Math.min(255, number))).toString(16).padStart(2, "0");
        }).join("")}`;
    }

    // CSS Color 4 values can remain in their original space after computation.
    const canvas = document.createElement("canvas");
    canvas.width = canvas.height = 1;
    const context = canvas.getContext("2d", { willReadFrequently: true });
    context.fillStyle = resolved;
    context.fillRect(0, 0, 1, 1);
    return `#${Array.from(context.getImageData(0, 0, 1, 1).data).slice(0, 3)
        .map(channel => channel.toString(16).padStart(2, "0")).join("")}`;
}

export function readSettings() {
    const root = document.documentElement;
    const styles = getComputedStyle(root);
    const radius = styles.getPropertyValue("--dnet-btn-radius").trim()
        || styles.getPropertyValue("--dnet-button-border-radius").trim()
        || styles.getPropertyValue("--dnet-sys-radius-pill").trim();

    return {
        theme: root.dataset.dnetTheme || "",
        primaryColor: toHexColor(styles.getPropertyValue("--dnet-sys-primary")),
        spaceUnit: parseInt(styles.getPropertyValue("--dnet-sys-space-unit"), 10),
        radius: parseInt(radius, 10)
    };
}

export function resetOverrides() {
    const style = document.documentElement.style;
    style.removeProperty("--dnet-sys-primary");
    style.removeProperty("--dnet-sys-space-unit");
    style.removeProperty("--dnet-btn-radius");
}

export function watchTheme(dotNetRef) {
    const root = document.documentElement;
    const id = nextThemeWatcherId++;
    let previousTheme = root.dataset.dnetTheme || "";
    const observer = new MutationObserver(() => {
        const theme = root.dataset.dnetTheme || "";
        if (theme === previousTheme) {
            return;
        }

        previousTheme = theme;
        try {
            Promise.resolve(dotNetRef.invokeMethodAsync("OnThemeChanged", theme))
                .catch(() => unwatchTheme(id));
        } catch {
            unwatchTheme(id);
        }
    });

    observer.observe(root, { attributes: true, attributeFilter: ["data-dnet-theme"] });
    themeWatchers.set(id, observer);
    return id;
}

export function unwatchTheme(id) {
    const observer = themeWatchers.get(id);
    if (observer) {
        observer.disconnect();
        themeWatchers.delete(id);
    }
}
