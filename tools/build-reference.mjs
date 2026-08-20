#!/usr/bin/env node
/**
 * build-reference.mjs — MAINTAINER TOOL, not shipped behaviour.
 *
 * Generates the agent-facing API reference under docs/ai/reference/ from the
 * component source, plus a versioned zip in artifacts/ for the GitHub release.
 *
 *   node tools/build-reference.mjs            # from the repo root
 *   node tools/build-reference.mjs --no-zip   # skip the archive (CI staleness check)
 *   node tools/build-reference.mjs /path/to/repo /custom/out
 *
 * Re-run for each release and commit docs/ai/reference — consumers read it
 * without cloning, and the diff makes API changes visible in review.
 */

import { readFileSync, readdirSync, writeFileSync, mkdirSync, existsSync, rmSync } from "node:fs";
import { join, basename } from "node:path";
import { execFileSync } from "node:child_process";

const flags = process.argv.slice(2).filter((a) => a.startsWith("--"));
const positional = process.argv.slice(2).filter((a) => !a.startsWith("--"));
const repo = positional[0] ?? process.cwd();
const outRoot = positional[1] ?? join(repo, "docs/ai/reference");

if (!existsSync(join(repo, "src/Dnet.Blazor/Components"))) {
    console.error("Run from the DnetBlazor repo root, or: node build-reference.mjs /path/to/repo [outDir]");
    process.exit(2);
}

const componentsRoot = join(repo, "src/Dnet.Blazor/Components");
const outDir = join(outRoot, "components");
mkdirSync(outDir, { recursive: true });

const walk = (d) =>
    readdirSync(d, { withFileTypes: true }).flatMap((e) =>
        e.isDirectory() ? walk(join(d, e.name)) : [join(d, e.name)]
    );

const uniq = (a) => [...new Set(a)].sort();
const grab = (re, t) => [...t.matchAll(re)].map((m) => m[1]);
const clean = (s) => s.replace(/\s+/g, " ").trim();
const stripDoc = (s) => clean(
    (s ?? "")
        .replace(/\/\/\/\s?/g, "")
        .replace(/<see\s+cref="([^"]+)"\s*\/>/gi, "$1")
        .replace(/<\/?[a-z]+[^>]*>/gi, "")
);
const cell = (s) => s.replace(/\|/g, "\\|");
const words = (name) => name.replace(/([a-z0-9])([A-Z])/g, "$1 $2").toLowerCase();

/**
 * Source summaries are the preferred documentation. Older components predate
 * XML docs, so keep the distributable reference useful while they are brought
 * up to the same standard instead of emitting an empty description cell.
 */
function describeParameter(name, type) {
    const descriptions = {
        AdditionalAttributes: "Gets or sets unmatched HTML attributes applied to the rendered element.",
        AllowedFormats: "Gets or sets the image file formats that may be selected.",
        BaseZindex: "Gets or sets the base z-index used by overlays.",
        ButtonType: "Gets or sets the HTML button type rendered by the component.",
        ChildContent: "Gets or sets the child content rendered by the component.",
        Class: "Gets or sets additional CSS classes for the component root.",
        ComponentType: "Gets or sets the component type rendered dynamically.",
        ContentChild: "Gets or sets custom content rendered by the component.",
        Culture: "Gets or sets the culture used to format and parse values.",
        DateFormat: "Gets or sets the format used to display date values.",
        DebounceTime: "Gets or sets the delay before a debounced input action is raised.",
        Disabled: "Gets or sets whether user interaction with the component is disabled.",
        DisplayValueConverter: "Gets or sets the function that converts an item to its display text.",
        EqualityComparer: "Gets or sets the comparer used to match item values.",
        ErrorContent: "Gets or sets content displayed when validation reports an error.",
        FooterContent: "Gets or sets content rendered in the component footer.",
        Format: "Gets or sets the display and parsing format for the value.",
        Height: "Gets or sets the component height.",
        HintContent: "Gets or sets supporting content displayed with the control.",
        IsRequired: "Gets or sets whether a value is required for validation.",
        InitialFocus: "Gets or sets whether the component receives focus after it is rendered.",
        Items: "Gets or sets the collection of items rendered by the component.",
        ItemTemplate: "Gets or sets the template used to render each item.",
        Label: "Gets or sets the label displayed for the component.",
        MaxHeight: "Gets or sets the maximum component height.",
        MaxWidth: "Gets or sets the maximum component width.",
        MinHeight: "Gets or sets the minimum component height.",
        MinWidth: "Gets or sets the minimum component width.",
        NodeContent: "Gets or sets the template used to render a tree node.",
        OnClick: "Raised when the user clicks the component.",
        OnClearInput: "Raised when the user clears the input.",
        OnFocus: "Raised when the component receives focus.",
        OnFocusin: "Raised when focus enters the component.",
        OnItemSelected: "Raised when the user selects an item.",
        OnSelectionChange: "Raised when the selection changes.",
        OnSelectionChanged: "Raised when the selection changes.",
        OnStopTyping: "Raised after the user stops typing.",
        OverscanCount: "Gets or sets the number of additional items rendered outside the visible viewport.",
        PageSize: "Gets or sets the number of items displayed on each page.",
        Parameters: "Gets or sets parameters passed to the dynamically rendered component.",
        PlaceHolder: "Gets or sets placeholder text displayed when the input is empty.",
        PrefixContent: "Gets or sets content rendered before the main control content.",
        SearchText: "Gets or sets the text used to filter items.",
        SelectedItems: "Gets or sets the currently selected items.",
        SelectedKeys: "Gets or sets the stable keys of the selected items.",
        SelectedStepId: "Gets or sets the identifier of the selected step.",
        SelectedTabId: "Gets or sets the identifier of the selected tab.",
        ShowMask: "Gets or sets whether a loading mask is displayed.",
        Strings: "Gets or sets the localized strings used by the component.",
        SufixContent: "Gets or sets content rendered after the main control content.",
        Theme: "Gets or sets the theme applied to the component subtree.",
        Title: "Gets or sets the title displayed by the component.",
        Value: "Gets or sets the component value.",
        ValueChanged: "Raised when the component value changes.",
        ValueExpression: "Gets or sets the expression that identifies the bound value for validation.",
        Width: "Gets or sets the component width.",
    };
    if (descriptions[name]) return descriptions[name];
    if (/^On[A-Z]/.test(name)) {
        return `Raised when ${words(name.slice(2))} occurs.`;
    }
    if (/Changed$/.test(name) || /^EventCallback/.test(type)) {
        return `Raised when ${words(name.replace(/Changed$/, ""))} changes.`;
    }
    if (/Template$/.test(name)) return `Gets or sets the template used to render ${words(name.replace(/Template$/, ""))}.`;
    if (/Content$/.test(name)) return `Gets or sets content rendered for ${words(name.replace(/Content$/, ""))}.`;
    if (/^Is[A-Z]/.test(name)) return `Gets or sets whether ${words(name.slice(2))}.`;
    if (/^(Show|Enable|Allow|Use|Hide|Confirm|Editable|Linear|Selectable|Removable|Multi|Fixed)[A-Z]/.test(name)) {
        return `Gets or sets whether the component ${words(name)}.`;
    }
    return `Gets or sets the ${words(name)} used by this component.`;
}

// Panels, hosts and probes are internal plumbing — a consumer never places them.
const INTERNAL = /(Panel|Host|Pane|Content|PropertyDisplay|Probe)\.razor$/;

const version =
    (readFileSync(join(repo, "README.md"), "utf8").match(/\*\*Current version:\*\*\s*(\d+\.\d+\.\d+)/) ?? [])[1]
    ?? "unknown";

// ------------------------------------------------- global token resolution
const readIf = (p) => (existsSync(p) ? readFileSync(p, "utf8") : "");
const globalTokens = {};
for (const t of [
    readIf(join(componentsRoot, "Assets/styles/tokens/reference.css")),
    readIf(join(componentsRoot, "Assets/styles/tokens/system.css")),
]) {
    for (const m of t.matchAll(/^\s*(--dnet-(?:ref|sys)-[a-z0-9-]+)\s*:\s*([^;]+);/gm)) {
        globalTokens[m[1]] = clean(m[2]);
    }
}
const resolveGlobal = (expr, depth = 0) => {
    const m = expr.match(/^var\(\s*(--dnet-(?:ref|sys)-[a-z0-9-]+)\s*\)$/);
    if (m && globalTokens[m[1]] && depth < 5) return resolveGlobal(globalTokens[m[1]], depth + 1);
    return expr;
};

/** Extract every top-level `var(...)` with balanced parentheses. */
function extractVars(text) {
    const out = [];
    for (let i = text.indexOf("var("); i !== -1; i = text.indexOf("var(", i + 1)) {
        let depth = 0;
        for (let j = i + 3; j < text.length; j++) {
            if (text[j] === "(") depth++;
            else if (text[j] === ")" && --depth === 0) { out.push(text.slice(i, j + 1)); break; }
        }
    }
    return out;
}

/** `var(--a, var(--b, 10px))` -> { links: ["--a","--b"], final: "10px" } */
function parseChain(expr) {
    const links = [];
    let rest = clean(expr);
    for (;;) {
        const m = rest.match(/^var\(\s*(--[a-zA-Z0-9-]+)\s*(?:,\s*([\s\S]*))?\)$/);
        if (!m) break;
        links.push(m[1]);
        if (!m[2]) return { links, final: null };
        rest = m[2].trim();
    }
    return { links, final: rest || null };
}

const components = readdirSync(componentsRoot, { withFileTypes: true })
    .filter((e) => e.isDirectory() && e.name !== "Assets")
    .map((e) => e.name)
    .sort();

const index = [];

for (const name of components) {
    const dir = join(componentsRoot, name);
    const files = walk(dir);
    const css = files.filter((f) => f.endsWith(".css")).map((f) => readFileSync(f, "utf8")).join("\n");
    // Public entry points. Two wrinkles:
    //  - excluding "…Panel.razor" would wipe out components whose only public
    //    component happens to end in Panel, so only apply it when something
    //    survives;
    //  - some components (Checkbox, RadioButton) are pure C# with no .razor.
    const allRazor = files.filter((f) => f.endsWith(".razor"));
    const filtered = allRazor.filter((f) => !INTERNAL.test(f));
    let publicRazor = filtered.length ? filtered : allRazor;
    if (!allRazor.length) {
        publicRazor = files.filter(
            (f) => f.endsWith(".cs") && !f.endsWith(".razor.cs") && /\[Parameter\]/.test(readFileSync(f, "utf8"))
        );
    }

    // ---------------------------------------------------------------- tokens
    // The FIRST link of a chain is this component's own token. Later links are
    // legacy aliases or shared family names — listing them as its API misleads.
    const owned = new Map();
    const alias = new Set();
    const addChain = (expr) => {
        const { links, final } = parseChain(expr);
        if (!links.length) return;
        if (!owned.has(links[0])) owned.set(links[0], { rest: links.slice(1), final });
        links.slice(1).forEach((l) => alias.add(l));
    };
    for (const v of extractVars(css)) {
        if (/^var\(\s*--dnet-(?!sys-|ref-)/.test(v)) addChain(v);
    }
    for (const t of uniq(grab(/var\(\s*(--dnet-(?!sys-|ref-)[a-z0-9-]+)/g, css))) {
        if (!owned.has(t) && !alias.has(t)) owned.set(t, { rest: [], final: null });
    }
    for (const a of alias) owned.delete(a);
    const tokens = [...owned.keys()].sort();

    // ----------------------------------------------------- parameters + docs
    // Parameters commonly live in a .razor.cs code-behind, not the .razor.
    const api = [];
    for (const rf of publicRazor) {
        const isRazor = rf.endsWith(".razor");
        const tag = basename(rf, isRazor ? ".razor" : ".cs");
        const sources = uniq([rf, `${rf}.cs`, join(dir, `${tag}.cs`)]).filter(existsSync);
        const text = sources.map((f) => readFileSync(f, "utf8")).join("\n");
        const generics = isRazor
            ? uniq(grab(/@typeparam\s+(\w+)/g, readFileSync(rf, "utf8")))
            : uniq(grab(/class\s+\w+<([\w,\s]+)>/g, text).flatMap((g) => g.split(",").map((x) => x.trim())));

        const summary = stripDoc(
            (text.match(/\/\/\/\s*<summary>([\s\S]*?)<\/summary>[\s\S]{0,300}?public\s+(?:partial\s+)?class/) ?? [])[1]
        );

        // Line scan: a /// <summary> only counts when it sits immediately above
        // the [Parameter] attribute. Regex across the whole file swallows the
        // class body into the first parameter's description.
        const params = [];
        const lines = text.split(/\r?\n/);
        for (let i = 0; i < lines.length; i++) {
            if (!/^\s*\[Parameter(?:\s*(?:\(|,|\]))/.test(lines[i])) continue;

            let doc = "";
            for (let k = i - 1; k >= 0 && k >= i - 6; k--) {
                const l = lines[k].trim();
                if (l.startsWith("///")) { doc = l.replace(/^\/\/\/\s?/, "") + " " + doc; continue; }
                if (l === "" || l.startsWith("[")) continue;
                break;
            }

            const decl = lines.slice(i, i + 4).join(" ");
            const m = decl.match(/public\s+([\w?<>,\s\[\]]+?)\s+(\w+)\s*\{\s*get;\s*set;\s*\}(?:\s*=\s*([^;]+);)?/);
            if (!m) continue;
            const parameterType = clean(m[1]);
            params.push({
                doc: stripDoc(doc) || describeParameter(m[2], parameterType),
                type: parameterType,
                name: m[2],
                def: m[3]?.trim() ?? null,
            });
        }

        if (params.length) api.push({ tag, generics, params, summary });
    }

    if (!api.length && !tokens.length) continue;

    // ---------------------------------------------------------------- render
    const L = [`# ${name}`, ""];
    const topSummary = api.find((a) => a.summary)?.summary;
    if (topSummary) L.push(topSummary, "");

    for (const a of api) {
        L.push(`## \`<${a.tag}>\`${a.generics.length ? ` — generic over ${a.generics.join(", ")}` : ""}`, "");
        if (a.summary && a.summary !== topSummary) L.push(a.summary, "");

        const req = a.params.filter(
            (p) => !p.def && !/^(EventCallback|RenderFragment)/.test(p.type) && !p.type.includes("?")
        );
        L.push("```razor", `<${a.tag}${a.generics.map((g) => ` ${g}="..."`).join("")}`);
        for (const p of req.slice(0, 4)) L.push(`    ${p.name}="..."`);
        L.push("/>", "```", "");

        L.push("| Parameter | Type | Default | Description |", "|---|---|---|---|");
        for (const p of a.params) {
            L.push(`| \`${p.name}\` | \`${cell(p.type)}\` | ${p.def ? "`" + cell(p.def) + "`" : "—"} | ${cell(p.doc)} |`);
        }
        L.push("");
    }

    if (tokens.length) {
        L.push(
            "## Styling tokens",
            "",
            "Set any of these above the component in the DOM — `:root`, a container, or",
            "the element's own `style`. Nothing else is needed.",
            "",
            "| Token | Effective default |",
            "|---|---|"
        );
        for (const t of tokens) {
            const { rest, final } = owned.get(t);
            // A chain usually bottoms out in a semantic token with no literal
            // fallback — resolve it so the reader sees a real value.
            const via = rest.find((r) => r.startsWith("--dnet-sys-"));
            let literal = final;
            if (!literal && via && globalTokens[via]) literal = globalTokens[via];
            let value = "—";
            if (literal) {
                value = "`" + cell(resolveGlobal(literal)) + "`";
                if (via) value += ` <br><sub>via \`${via}\`</sub>`;
            }
            L.push(`| \`${t}\` | ${value} |`);
        }
        L.push("");

        const legacy = [...alias].filter((a) => !a.startsWith("--dnet-sys-") && !a.startsWith("--dnet-ref-")).sort();
        if (legacy.length) {
            L.push(
                "<details><summary>Legacy token names still honoured</summary>",
                "",
                "Kept as intermediate links in the fallback chains so 5.x overrides keep",
                "working. Prefer the names above for new code; these go away in 7.0.",
                "",
                legacy.map((a) => "`" + a + "`").join(", "),
                "",
                "</details>",
                ""
            );
        }

        const example = tokens.find((t) => /radius|background|color/.test(t)) ?? tokens[0];
        L.push("```css", `:root { ${example}: /* your value */; }`, "```", "");
    }

    writeFileSync(join(outDir, `${name}.md`), L.join("\n"), "utf8");
    index.push({
        name,
        tags: api.map((a) => a.tag),
        tokens: tokens.length,
        params: api.reduce((n, a) => n + a.params.length, 0),
    });
}

// -------------------------------------------------------------------- index
writeFileSync(
    join(outRoot, "component-index.md"),
    [
        `# Component index`,
        ``,
        `Dnet.Blazor ${version}. One file per component under \`components/\`.`,
        `Open only the one you need.`,
        ``,
        `| Component | Use in markup | Parameters | Styling tokens |`,
        `|---|---|---:|---:|`,
        ...index.map(
            (c) =>
                `| [${c.name}](components/${c.name}.md) | ${c.tags.map((t) => "`<" + t + ">`").join(", ") || "—"} | ${c.params || "—"} | ${c.tokens || "—"} |`
        ),
        ``,
    ].join("\n"),
    "utf8"
);

// ------------------------------------------------------------------- assets
let copied = 0;
for (const [from, to] of [
    ["src/Dnet.Blazor/Components/Assets/styles/tokens/system.css", "design-tokens.css"],
    ["src/Dnet.Blazor/wwwroot/styles/theme/dark.css", "theme-dark-example.css"],
]) {
    const src = join(repo, from);
    if (existsSync(src)) { writeFileSync(join(outRoot, to), readFileSync(src, "utf8"), "utf8"); copied++; }
    else console.warn(`  ! missing ${from}`);
}

console.log(`Dnet.Blazor ${version}: ${index.length} component files + index + ${copied} assets -> ${outRoot}`);
console.log(`Commit ${outRoot} — it makes API changes visible in PR diffs.`);

// --------------------------------------------------------- distributable zip
if (!flags.includes("--no-zip")) {
    const artifacts = join(repo, "artifacts");
    const zipName = `dnet-blazor-agent-reference-${version}.zip`;
    const zipPath = join(artifacts, zipName);
    mkdirSync(artifacts, { recursive: true });
    rmSync(zipPath, { force: true });
    try {
        const stageRoot = join(artifacts, ".stage");
        const staged = join(stageRoot, "dnet-blazor-agent-reference");
        rmSync(stageRoot, { recursive: true, force: true });
        mkdirSync(staged, { recursive: true });
        const copyTree = (from, to) => {
            mkdirSync(to, { recursive: true });
            for (const e of readdirSync(from, { withFileTypes: true })) {
                e.isDirectory()
                    ? copyTree(join(from, e.name), join(to, e.name))
                    : writeFileSync(join(to, e.name), readFileSync(join(from, e.name)));
            }
        };
        copyTree(outRoot, staged);
        execFileSync("zip", ["-rq", zipPath, "dnet-blazor-agent-reference"], { cwd: stageRoot });
        rmSync(stageRoot, { recursive: true, force: true });
        console.log(`\nPackaged ${zipName} (${Math.round(readFileSync(zipPath).length / 1024)} KB) -> artifacts/`);
        console.log(`Attach it to the v${version} GitHub release. Do not commit it.`);
    } catch {
        console.warn(`\n! Could not create the zip (is the 'zip' command available?).`);
        console.warn(`  The reference folder is still written; only the archive was skipped.`);
    }
}
