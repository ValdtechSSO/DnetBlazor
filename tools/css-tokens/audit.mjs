import { existsSync, readdirSync, readFileSync, writeFileSync } from "node:fs";
import { basename, extname, join, relative, resolve } from "node:path";

const repositoryRoot = resolve(import.meta.dirname, "../..");
const stylesRoot = join(repositoryRoot, "src/Dnet.Blazor/Components");
const themeRoot = join(repositoryRoot, "src/Dnet.Blazor/wwwroot/styles/theme");
const baselinePath = join(import.meta.dirname, "baseline.json");
const systemTokenPath = join(repositoryRoot, "src/Dnet.Blazor/Components/Assets/styles/tokens/system.css");
const tokenDocumentationPath = join(repositoryRoot, "docs/styling/tokens.md");
const ci = process.argv.includes("--ci");
const writeBaseline = process.argv.includes("--write-baseline");
const writeTokenDocumentation = process.argv.includes("--write-tokens-doc");
const checkTokenDocumentation = process.argv.includes("--check-tokens-doc");

const styleExtensions = new Set([".css", ".scss"]);
const declarationPattern = /^\s*(--[-_a-zA-Z0-9]+)\s*:/gm;
const variablePattern = /var\(\s*(--[-_a-zA-Z0-9]+)(?:\s*,[^)]*)?\s*\)/g;
const publicTokenPattern = /^--dnet-(?:ref|sys|[a-z0-9-]+)-[a-z0-9-]+$/;
const privateTokenPattern = /^--_[a-z0-9-]+$/;
const colorPattern = /#[0-9a-fA-F]{3,8}\b|\brgba?\(|\b(?:white|black)\b(?!-)/i;

function findStyleFiles(directory) {
    return readdirSync(directory, { withFileTypes: true }).flatMap((entry) => {
        const entryPath = join(directory, entry.name);
        if (entry.isDirectory()) {
            return findStyleFiles(entryPath);
        }

        // La entrada solo contiene @import: no hay nada que lintar en ella, pero
        // NO se excluye ninguna otra cosa. Excluir el fuente real vaciaba el linter.
        if (entryPath.endsWith(`${join("Assets", "styles", "dnet-blazor-styles.css")}`)) {
            return [];
        }

        return styleExtensions.has(extname(entry.name)) ? [entryPath] : [];
    });
}

function readMatches(pattern, content) {
    return [...content.matchAll(pattern)];
}

function addViolation(violations, rule, file, token = "") {
    violations.push({ rule, file: relative(repositoryRoot, file), token });
}

function hasFallback(match) {
    return match[0].includes(",");
}

function audit() {
    const declarations = new Map();
    const usages = new Map();
    const violations = [];

    for (const file of [...findStyleFiles(stylesRoot), ...findStyleFiles(themeRoot)]) {
        const content = readFileSync(file, "utf8");
        const fileName = basename(file);
        const isTokenLayer = file.includes(`${join("Assets", "styles", "tokens")}/`) ||
            file.includes(`${join("Assets", "styles", "theme")}/`) ||
            file.includes(`${join("Assets", "styles", "tokens")}/`) ||
            file.includes(`${join("wwwroot", "styles", "theme")}/`);
        const isComponentFile = !isTokenLayer;
        const declaredHere = new Set();

        for (const match of readMatches(declarationPattern, content)) {
            const token = match[1];
            declaredHere.add(token);
            if (!declarations.has(token)) {
                declarations.set(token, new Set());
            }
            declarations.get(token).add({ file, isTokenLayer });

            if (token.startsWith("--_") && !privateTokenPattern.test(token)) {
                addViolation(violations, "R5-private-name", file, token);
            }
            if (token.startsWith("--dnet-") && !publicTokenPattern.test(token)) {
                addViolation(violations, "R5-public-name", file, token);
            }
            if (isComponentFile && token.startsWith("--dnet-")) {
                addViolation(violations, "R10-public-declaration", file, token);
            }
        }

        for (const match of readMatches(variablePattern, content)) {
            const token = match[1];
            if (!usages.has(token)) {
                usages.set(token, []);
            }
            usages.get(token).push({ file, hasFallback: hasFallback(match) });

            if (isComponentFile && token.startsWith("--dnet-ref-")) {
                addViolation(violations, "R6-reference-use", file, token);
            }
            if (token.startsWith("--_") && !declaredHere.has(token)) {
                addViolation(violations, "R5-private-ownership", file, token);
            }
        }

        if (isComponentFile && /:root\b/.test(content)) {
            addViolation(violations, "R2-root", file);
        }
        if (isComponentFile && colorPattern.test(content)) {
            addViolation(violations, "R1-color-literal", file);
        }
        if (file.includes(`${join("Assets", "styles", "theme")}/`)) {
            const scopes = readMatches(/(^|})\s*([^@][^{]+)\{/gm, content)
                .map((match) => match[2].trim())
                .filter((scope) => scope && !scope.startsWith("/*"));
            if (scopes.length !== 1 || !scopes[0].startsWith("[data-dnet-theme=")) {
                addViolation(violations, "R8-theme-shape", file);
            }
        }
    }

    for (const [token, declarationsForToken] of declarations) {
        if (!usages.has(token)) {
            for (const declaration of declarationsForToken) {
                if (!declaration.isTokenLayer) {
                    addViolation(violations, "R3-dead-token", declaration.file, token);
                }
            }
        }
    }
    for (const [token, uses] of usages) {
        if (!declarations.has(token) && uses.some((use) => !use.hasFallback)) {
            for (const use of uses.filter((candidate) => !candidate.hasFallback)) {
                addViolation(violations, "R4-ghost-token", use.file, token);
            }
        }
    }

    return violations.sort((left, right) =>
        `${left.rule}:${left.file}:${left.token}`.localeCompare(`${right.rule}:${right.file}:${right.token}`));
}

function generateTokenDocumentation() {
    const stylesheet = readFileSync(systemTokenPath, "utf8");
    const tokens = [...stylesheet.matchAll(/^\s*(--dnet-sys-[a-z0-9-]+):\s*([^;]+);/gm)]
        .map((match) => [match[1], match[2].trim()])
        .sort(([left], [right]) => left.localeCompare(right));
    const rows = tokens.map(([token, value]) => `| \`${token}\` | \`${value.replace(/`/g, "\\`")}\` |`).join("\n");

    return `# Semantic tokens\n\nGenerated by \`node tools/css-tokens/audit.mjs --write-tokens-doc\`. Do not edit manually.\n\n| Token | Default value |\n| --- | --- |\n${rows}\n`;
}

const violations = audit();
if (writeBaseline) {
    writeFileSync(baselinePath, `${JSON.stringify({ violations }, null, 2)}\n`);
    console.log(`Wrote baseline with ${violations.length} violation(s).`);
    process.exit(0);
}

const tokenDocumentation = generateTokenDocumentation();
if (writeTokenDocumentation) {
    writeFileSync(tokenDocumentationPath, tokenDocumentation);
    console.log(`Wrote semantic token documentation.`);
    process.exit(0);
}

if (checkTokenDocumentation && (!existsSync(tokenDocumentationPath) || readFileSync(tokenDocumentationPath, "utf8") !== tokenDocumentation)) {
    console.log("Semantic token documentation is out of date. Run `node tools/css-tokens/audit.mjs --write-tokens-doc`.");
    process.exitCode = 1;
}

const baseline = existsSync(baselinePath)
    ? JSON.parse(readFileSync(baselinePath, "utf8")).violations
    : [];
const baselineKeys = new Set(baseline.map((entry) => `${entry.rule}:${entry.file}:${entry.token}`));
const newViolations = violations.filter((entry) => !baselineKeys.has(`${entry.rule}:${entry.file}:${entry.token}`));

console.log(`CSS token audit: ${violations.length} violation(s), ${newViolations.length} new.`);
for (const violation of newViolations) {
    console.log(`${violation.rule}: ${violation.file}${violation.token ? ` (${violation.token})` : ""}`);
}

if (ci && newViolations.length > 0) {
    process.exitCode = 1;
}
