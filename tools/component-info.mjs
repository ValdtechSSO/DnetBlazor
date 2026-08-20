#!/usr/bin/env node
/**
 * component-info.mjs — fact sheet for a Dnet.Blazor component, extracted from source.
 *
 * Re-read from source on every run, so it never goes stale.
 *
 *   node tools/component-info.mjs Tooltip
 *   node tools/component-info.mjs --list
 *
 * Run from the repo root, or set DNET_REPO=/path/to/repo.
 */

import { readFileSync, readdirSync, existsSync, statSync } from "node:fs";
import { join, basename, relative } from "node:path";

const repo = process.env.DNET_REPO ?? process.cwd();
const componentsRoot = join(repo, "src/Dnet.Blazor/Components");

if (!existsSync(componentsRoot)) {
    console.error(`Can't find ${componentsRoot}. Run from the repo root or set DNET_REPO.`);
    process.exit(2);
}

const walk = (dir) =>
    readdirSync(dir, { withFileTypes: true }).flatMap((e) => {
        const p = join(dir, e.name);
        return e.isDirectory() ? walk(p) : [p];
    });

const listComponents = () =>
    readdirSync(componentsRoot, { withFileTypes: true })
        .filter((e) => e.isDirectory() && !["Assets"].includes(e.name))
        .map((e) => e.name)
        .sort();

// ----------------------------------------------------------------- arguments
const arg = process.argv[2];
if (!arg || arg === "--list") {
    console.log("Available components:\n");
    for (const c of listComponents()) {
        const dir = join(componentsRoot, c);
        const css = walk(dir).filter((f) => f.endsWith(".css")).length;
        const razor = walk(dir).filter((f) => f.endsWith(".razor")).length;
        console.log(`  ${c.padEnd(22)} ${css} css, ${razor} razor`);
    }
    process.exit(0);
}

const all = listComponents();
const name = all.find((c) => c.toLowerCase() === arg.toLowerCase())
    ?? all.find((c) => c.toLowerCase().includes(arg.toLowerCase()));

if (!name) {
    console.error(`No component named "${arg}". Try --list.`);
    process.exit(1);
}

const dir = join(componentsRoot, name);
const files = walk(dir);
const cssFiles = files.filter((f) => f.endsWith(".css"));
const razorFiles = files.filter((f) => f.endsWith(".razor"));
const csFiles = files.filter((f) => f.endsWith(".cs"));
const rel = (f) => relative(repo, f);

const cssText = cssFiles.map((f) => readFileSync(f, "utf8")).join("\n");
const razorText = razorFiles.concat(csFiles).map((f) => readFileSync(f, "utf8")).join("\n");

const uniq = (a) => [...new Set(a)].sort();
const matches = (re, t) => [...t.matchAll(re)].map((m) => m[1]);

// -------------------------------------------------------------------- tokens
// A public token is READ via var(); a private one is DECLARED and read.
const publicTokens = uniq(matches(/var\(\s*(--dnet-(?!sys-|ref-)[a-z0-9-]+)/g, cssText));
const sysUsed = uniq(matches(/var\(\s*(--dnet-sys-[a-z0-9-]+)/g, cssText));
const classes = uniq(matches(/^\s*(\.[a-z][a-z0-9-]*)/gm, cssText));

// Full fallback chain per private token — this is what reveals the real default.
const chains = [];
for (const f of cssFiles) {
    const t = readFileSync(f, "utf8");
    for (const m of t.matchAll(/^\s*(--_[a-z0-9-]+)\s*:\s*([^;]+);/gm)) {
        chains.push([m[1], m[2].replace(/\s+/g, " ").trim()]);
    }
}

// ---------------------------------------------------------- public parameters
const params = [];
for (const f of razorFiles.concat(csFiles)) {
    const t = readFileSync(f, "utf8");
    for (const m of t.matchAll(
        /\[Parameter\][\s\S]{0,120}?public\s+([\w?<>,\s]+?)\s+(\w+)\s*\{\s*get;\s*set;\s*\}(?:\s*=\s*([^;]+);)?/g
    )) {
        params.push({ type: m[1].trim(), name: m[2], def: m[3]?.trim() ?? null, file: basename(f) });
    }
}

// ------------------------------------------------------------------ warnings
const warnings = [];

// (1) appearance parameter with a non-null default = inert token
const APPEARANCE = /(color|background|padding|margin|font|width|height|size|radius|border)/i;
const COLORISH = /^"(#[0-9a-fA-F]{3,8}|rgba?\(|white|black|transparent)/;
for (const p of params) {
    if (!p.def) continue;
    if (COLORISH.test(p.def)) {
        warnings.push(`Parameter "${p.name}" has a hardcoded colour default (${p.def}). If written inline it OVERRIDES the tokens and defeats theming. See docs/ai/pitfalls.md #1.`);
    } else if (APPEARANCE.test(p.name) && p.def !== "null") {
        warnings.push(`Parameter "${p.name}" is appearance-related and has a default (${p.def}). Check whether it is written inline.`);
    }
}

// (2) inline styles
if (/StyleBuilder/.test(razorText)) {
    const props = uniq(matches(/(?:AddStyle|new StyleBuilder)\("([a-z-]+)"/g, razorText));
    warnings.push(`Uses StyleBuilder, writing inline: ${props.join(", ")}. Runtime-computed geometry is fine; default appearance is NOT — inline beats the stylesheet.`);
}
if (uniq(matches(/style="[^"]*?([a-z-]*colou?r):/g, razorText)).length) {
    warnings.push(`Literal style="" with a colour in the markup: no theme can reach it.`);
}

// (3) parameters declared but never used
for (const p of params) {
    const uses = (razorText.match(new RegExp(`\\b${p.name}\\b`, "g")) ?? []).length;
    if (uses <= 1) warnings.push(`Parameter "${p.name}" is declared but never appears to be used. Dead API?`);
}

// (4) CSS classes nobody applies.
//     Blazor markup composes class names by interpolation (class="x-size-@Foo"),
//     so a literal lookup gives false positives — also try shorter prefixes.
const appearsInMarkup = (bare) => {
    if (razorText.includes(bare)) return true;
    const parts = bare.split("-");
    for (let i = parts.length - 1; i >= 2; i--) {
        if (razorText.includes(parts.slice(0, i).join("-") + "-")) return true;
    }
    return false;
};
for (const c of classes) {
    if (!/^\.dnet-[a-z-]+$/.test(c)) continue;
    if (!appearsInMarkup(c.slice(1))) {
        warnings.push(`Class "${c}" doesn't appear in this component's markup. Could be dead CSS, or applied from elsewhere — verify before deleting.`);
    }
}

// (5) hard invariants
if (/^:root/m.test(cssText)) warnings.push(`R2 VIOLATION: a component file contains :root.`);
const declaredPublic = matches(/^\s*(--dnet-(?!sys-|ref-)[a-z0-9-]+)\s*:/gm, cssText);
if (declaredPublic.length) warnings.push(`R10 VIOLATION: this component DECLARES public tokens (${uniq(declaredPublic).join(", ")}). That breaks inheritance — a consumer's :root can no longer win.`);
const literals = uniq(matches(/(#[0-9a-fA-F]{3,8})\b/g, cssText)).filter((h) => !new RegExp(`fill=['"]?${h}`).test(cssText));
if (literals.length) warnings.push(`Likely R1 violation: colour literals in the CSS (${literals.slice(0, 6).join(", ")}).`);
if (/!important/.test(cssText)) warnings.push(`Contains !important (${(cssText.match(/!important/g) ?? []).length}). Don't add more.`);

// (6) cascade order between variants
const variantBlocks = uniq(matches(/^(\.[a-z0-9-]*(?:-size-|--)[a-z0-9-]+)\s*\{/gm, cssText));
if (variantBlocks.length) {
    warnings.push(`Variant classes present (${variantBlocks.join(", ")}). Same specificity as the base rule, so they MUST come LAST in the file or the base overwrites them.`);
}

// ---------------------------------------------------------------- rendering
const H = (s) => `\n${s}\n${"─".repeat(s.length)}`;
console.log(`COMPONENT FACT SHEET: ${name}`);
console.log(H("Files"));
for (const f of [...cssFiles, ...razorFiles, ...csFiles].slice(0, 25)) {
    console.log(`  ${rel(f)}  (${statSync(f).size} B)`);
}
if (files.length > 25) console.log(`  … and ${files.length - 25} more`);

console.log(H(`Public styling API — ${publicTokens.length} tokens`));
if (publicTokens.length) publicTokens.forEach((t) => console.log(`  ${t}`));
else console.log("  (none — this component is not themeable via tokens)");
if (publicTokens.length > 12) console.log(`\n  ⚠ More than 12. The plan asks for ≤12; check whether some are redundant.`);

console.log(H("Actual defaults (fallback chain per private token)"));
chains.forEach(([k, v]) => console.log(`  ${k}\n      = ${v}`));

console.log(H(`Semantic tokens consumed — ${sysUsed.length}`));
console.log("  " + (sysUsed.join("\n  ") || "(none)"));

console.log(H(`CSS classes — ${classes.length}`));
console.log("  " + classes.join(", "));

console.log(H(`Public parameters — ${params.length}`));
params.forEach((p) => console.log(`  ${p.type} ${p.name}${p.def ? ` = ${p.def}` : ""}   [${p.file}]`));

console.log(H(`WARNINGS — ${warnings.length}`));
if (!warnings.length) console.log("  None detected.");
warnings.forEach((w) => console.log(`  • ${w}`));

console.log(H("Before changing anything"));
console.log(`  1. Read docs/ai/architecture.md if you haven't already.
  2. Work through the WARNINGS above — each maps to a mistake that has already cost time here.
  3. Use Button/dnet-button.css as the canonical example of the pattern.
  4. When done: npm run buildDnetBlazor && npm run lint:css (must report 0 new).`);
