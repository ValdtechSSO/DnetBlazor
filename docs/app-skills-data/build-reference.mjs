#!/usr/bin/env node
/**
 * build-reference.mjs — MAINTAINER TOOL, not shipped behaviour.
 *
 * Run this against a clone of the DnetBlazor repo to regenerate the bundled
 * reference files under references/components/. Consumers of the library never
 * run this: they just read the generated markdown, which is why the skill works
 * without the repo.
 *
 *   node build-reference.mjs /path/to/DnetBlazor ./references
 *
 * Re-run it for each library release so the snapshot matches the shipped version.
 */

import { readFileSync, readdirSync, writeFileSync, mkdirSync, existsSync } from "node:fs";
import { join, basename } from "node:path";

const repo = process.argv[2];
const outRoot = process.argv[3] ?? "./references";
if (!repo || !existsSync(join(repo, "src/Dnet.Blazor/Components"))) {
    console.error("Usage: node build-reference.mjs /path/to/DnetBlazor [outDir]");
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

// Panels and hosts are internal plumbing — a consumer never places them by hand.
const INTERNAL = /(Panel|Host|Pane|Content|PropertyDisplay|Probe)\.razor$/;

const version =
    (readFileSync(join(repo, "README.md"), "utf8").match(/\*\*Current version:\*\*\s*([\d]+\.[\d]+\.[\d]+)/) ?? [])[1]
    ?? "unknown";

const components = readdirSync(componentsRoot, { withFileTypes: true })
    .filter((e) => e.isDirectory() && e.name !== "Assets")
    .map((e) => e.name)
    .sort();

const index = [];

for (const name of components) {
    const dir = join(componentsRoot, name);
    const files = walk(dir);
    const css = files.filter((f) => f.endsWith(".css")).map((f) => readFileSync(f, "utf8")).join("\n");
    const razorFiles = files.filter((f) => f.endsWith(".razor"));
    const publicRazor = razorFiles.filter((f) => !INTERNAL.test(f));

    // Public styling API: tokens the component READS but never declares.
    const tokens = uniq(grab(/var\(\s*(--dnet-(?!sys-|ref-)[a-z0-9-]+)/g, css));

    // What each token falls back to, so the consumer knows the current value.
    const defaults = {};
    for (const m of css.matchAll(/--_[a-z0-9-]+\s*:\s*var\(\s*(--dnet-(?!sys-|ref-)[a-z0-9-]+)\s*,\s*([^;]+?)\)\s*;/g)) {
        defaults[m[1]] ??= m[2].replace(/\s+/g, " ").trim();
    }

    // Public parameters, per public component.
    const api = [];
    for (const f of publicRazor) {
        const t = readFileSync(f, "utf8");
        const ps = [];
        for (const m of t.matchAll(
            /\[Parameter\][\s\S]{0,140}?public\s+([\w?<>,\s\[\]]+?)\s+(\w+)\s*\{\s*get;\s*set;\s*\}(?:\s*=\s*([^;]+);)?/g
        )) {
            ps.push({ type: m[1].replace(/\s+/g, " ").trim(), name: m[2], def: m[3]?.trim() ?? null });
        }
        const generics = (t.match(/@typeparam\s+(\w+)/g) ?? []).map((g) => g.split(/\s+/)[1]);
        if (ps.length) api.push({ tag: basename(f, ".razor"), generics, params: ps });
    }

    if (!api.length && !tokens.length) continue;

    const lines = [];
    lines.push(`# ${name}`, "");
    if (api.length) {
        lines.push(`Components: ${api.map((a) => "`<" + a.tag + ">`").join(", ")}`, "");
    }

    for (const a of api) {
        lines.push(`## \`<${a.tag}>\`${a.generics.length ? ` — generic over ${a.generics.join(", ")}` : ""}`, "");
        lines.push("| Parameter | Type | Default |", "|---|---|---|");
        for (const p of a.params) {
            lines.push(`| \`${p.name}\` | \`${p.type}\` | ${p.def ? "`" + p.def + "`" : "—"} |`);
        }
        lines.push("");
    }

    if (api.length) {
        const main = api[0];
        const req = main.params.filter((p) => !p.def && !/^(EventCallback|RenderFragment)/.test(p.type)).slice(0, 3);
        lines.push("## Minimal usage", "", "```razor",
            `<${main.tag}${main.generics.length ? " T" + main.generics.join(" T") + "=\"...\"" : ""}` +
            (req.length ? "\n" + req.map((p) => `    ${p.name}="..."`).join("\n") + "\n/>" : " />"),
            "```", "");
    }

    if (tokens.length) {
        lines.push(`## Styling tokens`, "");
        lines.push("Override these anywhere in the DOM above the component — `:root`, a", "container, or the element's own `style`. Nothing else is needed.", "");
        lines.push("| Token | Falls back to |", "|---|---|");
        for (const t of tokens) lines.push(`| \`${t}\` | ${defaults[t] ? "`" + defaults[t] + "`" : "—"} |`);
        lines.push("");
        lines.push("```css", `:root { ${tokens[0]}: /* your value */; }`, "```", "");
    }

    writeFileSync(join(outDir, `${name}.md`), lines.join("\n"), "utf8");
    index.push({ name, tags: api.map((a) => a.tag), tokens: tokens.length });
}

// ------------------------------------------------------------------- index
const idx = [
    `# Component index`,
    ``,
    `Dnet.Blazor ${version}. One file per component under \`references/components/\`.`,
    `Read only the one you need.`,
    ``,
    `| Component | Use in markup | Styling tokens |`,
    `|---|---|---:|`,
    ...index.map(
        (c) => `| [${c.name}](components/${c.name}.md) | ${c.tags.map((t) => "`<" + t + ">`").join(", ") || "—"} | ${c.tokens} |`
    ),
    ``,
];
writeFileSync(join(outRoot, "component-index.md"), idx.join("\n"), "utf8");

console.log(`Dnet.Blazor ${version}: wrote ${index.length} component files + index to ${outRoot}`);
