#!/usr/bin/env node
/*
 * Contrast gate for material themes (liquid-glass*.css).
 *
 * The contrast of translucent glass depends on what is behind it, so a single
 * colour pair is not enough. For every material level this script composes the
 * fill over the worst-case backgrounds and checks WCAG AA (4.5:1) for the text
 * tokens painted on that level:
 *
 *   chrome, card  fill over every canvas stop and over the canvas base
 *   popup         the same, plus pure black and pure white (it floats over
 *                 arbitrary content)
 *   control       fill over card over the canvas
 *   ghost         the same as control
 *   card, popup,  also with the sheen peak on top
 *   ghost
 *   primary       on-primary over the primary fill, plain and under the accent
 *                 sheen at the height where a button label starts
 *
 * The backdrop-filter `saturate()` of the theme is applied to the canvas before
 * composing. Blur only mixes colours that already lie between the stops, so
 * checking the stops bounds the worst case.
 *
 * Opaque fallbacks (@supports / @media blocks) are checked over the canvas base.
 *
 * Inputs must be literals the gate can read: text tokens and material fills as
 * #hex or rgb(); canvas as gradients whose first stop is #hex plus a final #hex
 * base colour. Anything else (var(), color-mix()) fails the gate.
 *
 *   node tools/css-tokens/contrast.mjs        # or: npm run check:contrast
 */
import { readdirSync, readFileSync } from "node:fs";
import { join, resolve } from "node:path";

const repositoryRoot = resolve(import.meta.dirname, "../..");
const themeRoot = join(repositoryRoot, "src/Dnet.Blazor/wwwroot/styles/theme");
const threshold = 4.5;

const textTokens = [
    "on-surface",
    "on-surface-emphasis",
    "on-surface-muted",
    "on-surface-subtle",
    "primary-content",
    "danger",
];
const ghostTextTokens = ["on-surface", "primary-content"];
const levels = ["chrome", "card", "popup", "control", "ghost"];

// ------------------------------------------------------------------ parsing

function stripComments(css) {
    return css.replace(/\/\*[\s\S]*?\*\//g, "");
}

/** Splits CSS into rules, descending one level into at-rules. */
function parseRules(css, atRule = null) {
    const rules = [];
    let index = 0;
    while (index < css.length) {
        const open = css.indexOf("{", index);
        if (open === -1) {
            break;
        }
        let depth = 1;
        let close = open + 1;
        while (close < css.length && depth > 0) {
            if (css[close] === "{") depth++;
            if (css[close] === "}") depth--;
            close++;
        }
        const prelude = css.slice(index, open).trim();
        const body = css.slice(open + 1, close - 1);
        if (prelude.startsWith("@")) {
            rules.push(...parseRules(body, prelude));
        } else {
            rules.push({ atRule, selector: prelude, declarations: parseDeclarations(body) });
        }
        index = close;
    }
    return rules;
}

/** Splits at top-level separators, ignoring those inside parentheses. */
function splitTopLevel(text, separator) {
    const parts = [];
    let depth = 0;
    let current = "";
    for (const character of text) {
        if (character === "(") depth++;
        if (character === ")") depth--;
        if (character === separator && depth === 0) {
            parts.push(current);
            current = "";
        } else {
            current += character;
        }
    }
    parts.push(current);
    return parts.map((part) => part.trim()).filter(Boolean);
}

function parseDeclarations(body) {
    const declarations = new Map();
    for (const declaration of splitTopLevel(body, ";")) {
        const match = /^(--[-_a-zA-Z0-9]+)\s*:\s*([\s\S]+)$/.exec(declaration);
        if (match) {
            declarations.set(match[1], match[2].replace(/\s+/g, " ").trim());
        }
    }
    return declarations;
}

/** Parses #hex and rgb()/rgba() literals into { r, g, b, a } (0-255, alpha 0-1). */
function parseColor(value) {
    const text = value.trim();
    const hex = /^#([\da-f]{3,4}|[\da-f]{6}|[\da-f]{8})$/i.exec(text);
    if (hex) {
        let digits = hex[1];
        if (digits.length <= 4) {
            digits = [...digits].map((digit) => digit + digit).join("");
        }
        const channel = (offset) => parseInt(digits.slice(offset, offset + 2), 16);
        return { r: channel(0), g: channel(2), b: channel(4), a: digits.length === 8 ? channel(6) / 255 : 1 };
    }
    const rgb = /^rgba?\(\s*([^)]+)\)$/i.exec(text);
    if (rgb) {
        const [channels, alpha] = rgb[1].includes("/") ? rgb[1].split("/") : [rgb[1], null];
        const values = channels.split(/[\s,]+/).filter(Boolean);
        const legacyAlpha = values.length === 4 ? values.pop() : null;
        if (values.length !== 3) {
            return null;
        }
        const toChannel = (part) => part.endsWith("%") ? parseFloat(part) * 2.55 : parseFloat(part);
        const toAlpha = (part) => part === null ? 1 : part.trim().endsWith("%") ? parseFloat(part) / 100 : parseFloat(part);
        const [r, g, b] = values.map(toChannel);
        const a = toAlpha(alpha ?? legacyAlpha);
        if ([r, g, b, a].some(Number.isNaN)) {
            return null;
        }
        return { r, g, b, a };
    }
    return null;
}

function parseGradientStops(gradient) {
    const match = /^(?:linear|radial)-gradient\(([\s\S]*)\)$/.exec(gradient.trim());
    if (!match) {
        return null;
    }
    // The first argument may be a direction or a shape; stops are those that start with a colour.
    return splitTopLevel(match[1], ",")
        .map((argument) => argument.split(/\s+(?![^(]*\))/)[0])
        .filter((first) => /^(#|rgba?\(|transparent$)/i.test(first));
}

function parseCanvas(value, problems) {
    const layers = splitTopLevel(value, ",");
    const base = parseColor(layers.at(-1) ?? "");
    if (!base || base.a !== 1 || !layers.at(-1).startsWith("#")) {
        problems.push("--dnet-sys-canvas must end in an opaque #hex base colour");
        return null;
    }
    const stops = [];
    for (const layer of layers.slice(0, -1)) {
        const colors = parseGradientStops(layer);
        const first = colors?.[0];
        if (!first || !first.startsWith("#") || !parseColor(first)) {
            problems.push(`--dnet-sys-canvas layer must be a gradient whose first stop is #hex: ${layer}`);
            continue;
        }
        stops.push(parseColor(first));
    }
    return { base, stops };
}

function parseSheenPeak(value, problems) {
    if (value === undefined || value === "none") {
        return null;
    }
    const stops = parseGradientStops(value);
    if (!stops) {
        problems.push(`--dnet-sys-material-sheen must be none or a gradient of literals: ${value}`);
        return null;
    }
    const colors = stops.map(parseColor);
    if (colors.some((color) => color === null)) {
        problems.push(`--dnet-sys-material-sheen stops must be #hex or rgb(): ${value}`);
        return null;
    }
    return colors.reduce((peak, color) => (color.a > peak.a ? color : peak));
}

/**
 * Colour of a vertical gradient at `position` (0 = top, 1 = bottom). Accent
 * fills put a label near the middle of the sheen, not at its peak, so the gate
 * samples where the label's cap height starts in the smallest button (34%).
 * A gradient that is not top-to-bottom falls back to its peak.
 */
function parseGradientAt(value, position, problems, name) {
    if (value === undefined || value === "none") {
        return null;
    }
    const match = /^linear-gradient\(([\s\S]*)\)$/.exec(value.trim());
    const parts = match ? splitTopLevel(match[1], ",") : [];
    const vertical = parts.length > 0 && /^(180deg|to bottom)$/.test(parts[0]);
    const stops = parts
        .map((part) => /^(#[\da-f]+|rgba?\([^)]*\)|transparent)\s*(?:([\d.]+)%)?$/i.exec(part))
        .filter(Boolean)
        .map((stop) => ({ color: stop[1] === "transparent" ? { r: 0, g: 0, b: 0, a: 0 } : parseColor(stop[1]), at: stop[2] }));
    if (stops.length < 2 || stops.some((stop) => stop.color === null)) {
        problems.push(`${name} must be none or a linear-gradient of literals: ${value}`);
        return null;
    }
    if (!vertical) {
        return stops.map((stop) => stop.color).reduce((peak, color) => (color.a > peak.a ? color : peak));
    }
    const positions = stops.map((stop, index) =>
        stop.at !== undefined ? parseFloat(stop.at) / 100 : index / (stops.length - 1));
    if (position <= positions[0]) return stops[0].color;
    for (let index = 1; index < stops.length; index++) {
        if (position <= positions[index]) {
            const t = (position - positions[index - 1]) / (positions[index] - positions[index - 1]);
            const from = stops[index - 1].color;
            const to = stops[index].color;
            const mix = (channel) => from[channel] + (to[channel] - from[channel]) * t;
            // A fully transparent stop keeps its neighbour's colour (premultiplied interpolation).
            const rgb = (channel) => (to.a === 0 ? from[channel] : from.a === 0 ? to[channel] : mix(channel));
            return { r: rgb("r"), g: rgb("g"), b: rgb("b"), a: mix("a") };
        }
    }
    return stops.at(-1).color;
}

function parseSaturation(value) {
    const match = /saturate\(\s*([\d.]+)(%?)\s*\)/.exec(value ?? "");
    if (!match) {
        return 1;
    }
    return match[2] ? parseFloat(match[1]) / 100 : parseFloat(match[1]);
}

// ------------------------------------------------------------ colour maths

/** CSS saturate() (Filter Effects 1), applied to sRGB values as browsers do. */
function saturate(color, amount) {
    if (amount === 1) {
        return color;
    }
    const s = amount;
    const { r, g, b } = color;
    const clamp = (value) => Math.min(255, Math.max(0, value));
    return {
        r: clamp((0.213 + 0.787 * s) * r + (0.715 - 0.715 * s) * g + (0.072 - 0.072 * s) * b),
        g: clamp((0.213 - 0.213 * s) * r + (0.715 + 0.285 * s) * g + (0.072 - 0.072 * s) * b),
        b: clamp((0.213 - 0.213 * s) * r + (0.715 - 0.715 * s) * g + (0.072 + 0.928 * s) * b),
        a: color.a,
    };
}

/** Source-over composition of `top` on an opaque `bottom`, in sRGB like the browser. */
function over(top, bottom) {
    const mix = (channel) => top[channel] * top.a + bottom[channel] * (1 - top.a);
    return { r: mix("r"), g: mix("g"), b: mix("b"), a: 1 };
}

function luminance({ r, g, b }) {
    const linear = (channel) => {
        const value = channel / 255;
        return value <= 0.04045 ? value / 12.92 : ((value + 0.055) / 1.055) ** 2.4;
    };
    return 0.2126 * linear(r) + 0.7152 * linear(g) + 0.0722 * linear(b);
}

function contrast(foreground, background) {
    const text = foreground.a < 1 ? over(foreground, background) : foreground;
    const [light, dark] = [luminance(text), luminance(background)].sort((left, right) => right - left);
    return (light + 0.05) / (dark + 0.05);
}

// -------------------------------------------------------------- the gate

const black = { r: 0, g: 0, b: 0, a: 1 };
const white = { r: 255, g: 255, b: 255, a: 1 };

function readLiteral(declarations, name, problems) {
    const value = declarations.get(`--dnet-sys-${name}`);
    if (value === undefined) {
        problems.push(`--dnet-sys-${name} is not declared`);
        return null;
    }
    const color = parseColor(value);
    if (!color) {
        problems.push(`--dnet-sys-${name} must be a #hex or rgb() literal, found: ${value}`);
    }
    return color;
}

/** Worst-case backgrounds for each material level. */
function backgroundsFor(fills, canvas, saturation, sheen) {
    const backdrop = [...canvas.stops.map((stop) => over(stop, canvas.base)), canvas.base]
        .map((color) => saturate(color, saturation));
    const withSheen = (colors) => (sheen ? [...colors, ...colors.map((color) => over(sheen, color))] : colors);

    const card = backdrop.map((color) => over(fills.card, color));
    return {
        chrome: backdrop.map((color) => over(fills.chrome, color)),
        card: withSheen(card),
        popup: withSheen([...backdrop, black, white].map((color) => over(fills.popup, color))),
        control: card.map((color) => over(fills.control, color)),
        ghost: withSheen(card.map((color) => over(fills.ghost, color))),
    };
}

function checkLevel(theme, level, tokens, text, backgrounds, results) {
    for (const token of tokens) {
        const minimum = Math.min(...backgrounds.map((background) => contrast(text[token], background)));
        results.push({ theme, level, token, minimum });
    }
}

function checkTheme(file) {
    const css = stripComments(readFileSync(join(themeRoot, file), "utf8"));
    const rules = parseRules(css);
    const main = rules.find((rule) => rule.atRule === null && /^\[data-dnet-theme="[^"]+"\]$/.test(rule.selector));
    const problems = [];
    const results = [];
    if (!main) {
        return { theme: file, problems: ["no [data-dnet-theme=\"…\"] scope block"], results };
    }
    const theme = /"([^"]+)"/.exec(main.selector)[1];
    const declarations = main.declarations;

    const text = Object.fromEntries(
        [...textTokens, "on-primary", "primary"].map((token) => [token, readLiteral(declarations, token, problems)]));
    const fills = Object.fromEntries(
        levels.map((level) => [level, readLiteral(declarations, `material-${level}`, problems)]));
    const canvasValue = declarations.get("--dnet-sys-canvas");
    const canvas = canvasValue === undefined
        ? (problems.push("--dnet-sys-canvas is not declared"), null)
        : parseCanvas(canvasValue, problems);
    const sheen = parseSheenPeak(declarations.get("--dnet-sys-material-sheen"), problems);
    const saturation = parseSaturation(declarations.get("--dnet-sys-material-backdrop"));
    const accentSheen = parseGradientAt(
        declarations.get("--dnet-sys-material-accent-sheen"), 0.34, problems, "--dnet-sys-material-accent-sheen");

    if (problems.length > 0) {
        return { theme, problems, results };
    }

    const backgrounds = backgroundsFor(fills, canvas, saturation, sheen);
    for (const level of levels) {
        checkLevel(theme, level, level === "ghost" ? ghostTextTokens : textTokens, text, backgrounds[level], results);
    }
    results.push({
        theme,
        level: "primary",
        token: "on-primary",
        minimum: contrast(text["on-primary"], text.primary),
    });
    if (accentSheen) {
        results.push({
            theme,
            level: "primary (sheen)",
            token: "on-primary",
            minimum: contrast(text["on-primary"], over(accentSheen, text.primary)),
        });
    }

    // Opaque fallbacks: every block nested in @supports/@media that restates fills.
    for (const fallback of rules.filter((rule) => rule.atRule !== null && rule.selector === main.selector)) {
        for (const level of levels) {
            const value = fallback.declarations.get(`--dnet-sys-material-${level}`);
            if (value === undefined) {
                continue;
            }
            const fill = parseColor(value);
            if (!fill) {
                problems.push(`fallback --dnet-sys-material-${level} must be a literal, found: ${value}`);
                continue;
            }
            const label = fallback.atRule.startsWith("@supports") ? "no-backdrop" : "reduced";
            checkLevel(theme, `${level} (${label})`, level === "ghost" ? ghostTextTokens : textTokens,
                text, [over(fill, canvas.base)], results);
        }
    }

    return { theme, problems, results };
}

const files = readdirSync(themeRoot).filter((file) => /^liquid-glass.*\.css$/.test(file)).sort();
if (files.length === 0) {
    console.log("Contrast gate: no liquid-glass*.css theme found.");
    process.exit(1);
}

let failures = 0;
for (const file of files) {
    const { theme, problems, results } = checkTheme(file);
    console.log(`\n${theme} (${file})`);
    for (const problem of problems) {
        console.log(`  ✗ ${problem}`);
        failures++;
    }
    const byLevel = Map.groupBy(results, (result) => result.level);
    for (const [level, entries] of byLevel) {
        const cells = entries.map(({ token, minimum }) => {
            const pass = minimum >= threshold;
            if (!pass) failures++;
            return `${token} ${minimum.toFixed(2)}${pass ? "" : " ✗"}`;
        });
        console.log(`  ${level.padEnd(22)} ${cells.join("  ")}`);
    }
}

console.log(`\nContrast gate (WCAG AA ${threshold}:1, worst case): ${failures === 0 ? "pass" : `${failures} failure(s)`}.`);
process.exitCode = failures === 0 ? 0 : 1;
