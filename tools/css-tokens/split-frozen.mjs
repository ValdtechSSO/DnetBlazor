#!/usr/bin/env node
/**
 * split-frozen.mjs — trocea el monolito congelado en la estructura del plan §16.
 *
 * Método: los límites NO se adivinan. Se recompila el árbol Sass previo a la
 * migración (commit 10bd903) componente a componente y se busca cada salida
 * literalmente dentro del monolito. 26 de 29 casan byte a byte; los 3 que no
 * (AdminLayout, Grid, Button) son exactamente los que se editaron a mano y se
 * recortan por offset explícito, igual que PickList y la capa de tokens.
 *
 * Garantía: reconcatenar los trozos en orden debe reproducir el monolito byte a
 * byte. Si no, el script aborta y no escribe nada.
 *
 * Uso:
 *   node split-frozen.mjs --plan     (por defecto: solo informa, no escribe)
 *   node split-frozen.mjs --apply    (escribe los ficheros y reescribe la entrada)
 */

import { readFileSync, writeFileSync, mkdirSync, rmSync, existsSync } from "node:fs";
import { join, dirname, resolve } from "node:path";
import { execFileSync } from "node:child_process";

const repo = process.env.DNET_REPO ?? resolve(process.cwd());
const componentsRoot = join(repo, "src/Dnet.Blazor/Components");
const stylesRoot = join(componentsRoot, "Assets/styles");
const frozenPath = join(stylesRoot, "dnet-blazor-styles.css");
const apply = process.argv.includes("--apply");
const partsDir = process.env.DNET_PARTS ?? "/tmp/parts";

/** Orden exacto de los @use del entry Sass original (10bd903). */
const sassOrder = [
    "shared", "AdminLayout", "Autocomplete", "Checkbox", "Chips", "ConnectedPanel",
    "DatePicker", "Dialog", "ExpansionPanel", "FloatingPanel", "Grid", "ImageEditor",
    "List", "Overlay", "Paginator", "RadioButton", "Spinner", "Stepper", "Tabs",
    "Toast", "Tooltip", "Tree", "DatePickerWeek", "DatePickerWeekRaw",
    "FloatingDoubleList", "Form", "Select", "Button", "DynamicStepper",
];

/** Destino de cada trozo. Los que no salen de Sass se marcan con `manual`. */
const destination = {
    shared: "Assets/styles/base/shared.css",
    AdminLayout: "AdminLayout/dnet-admin-layout.css",
    Autocomplete: "Autocomplete/dnet-autocomplete.css",
    Checkbox: "Checkbox/dnet-checkbox.css",
    Chips: "Chips/dnet-chip-list.css",
    ConnectedPanel: "ConnectedPanel/dnet-connected-panel.css",
    DatePicker: "DatePicker/dnet-datepicker.css",
    Dialog: "Dialog/dnet-dialog.css",
    ExpansionPanel: "ExpansionPanel/dnet-expansion-panel.css",
    FloatingPanel: "FloatingPanel/dnet-floating-panel.css",
    Grid: "Grid/dnet-blgrid.css",
    ImageEditor: "ImageEditor/dnet-image-editor.css",
    List: "List/dnet-list.css",
    Overlay: "Overlay/dnet-overlay-prebuilt.css",
    Paginator: "Paginator/dnet-paginator.css",
    PickList: "PickList/dnet-pick-list.css",
    RadioButton: "RadioButton/dnet-radio-button.css",
    Spinner: "Spinner/dnet-spinner.css",
    Stepper: "Stepper/dnet-stepper.css",
    Tabs: "Tabs/dnet-tabs.css",
    Toast: "Toast/dnet-toast.css",
    Tooltip: "Tooltip/dnet-tooltip.css",
    Tree: "Tree/dnet-tree.css",
    DatePickerWeek: "DatePickerWeek/dnet-datepicker-week.css",
    DatePickerWeekRaw: "DatePickerWeekRaw/dnet-datepicker-week-raw.css",
    FloatingDoubleList: "FloatingDoubleList/dnet-floating-double-list.css",
    Form: "Form/dnet-form-field-plain-cmp.css",
    Select: "Select/dnet-select.css",
    Button: "Button/dnet-button.css",
    DynamicStepper: "DynamicStepper/dnet-dynamic-stepper.css",
};

const frozen = readFileSync(frozenPath, "utf8");

// ---------------------------------------------------------------- localizar
const anchors = [];
let cursor = 0;
const handEdited = [];

let pending = [];   // nombres saltados desde el ancla anterior
for (const name of sassOrder) {
    const partPath = join(partsDir, `${name}.css`);
    if (!existsSync(partPath)) {
        console.error(`falta ${partPath} — ejecuta antes la recompilación del árbol antiguo`);
        process.exit(2);
    }
    const part = readFileSync(partPath, "utf8").trim();
    const at = frozen.indexOf(part, cursor);
    if (at < 0) { handEdited.push(name); pending.push(name); continue; }
    anchors.push({ name, start: at, end: at + part.length, exact: true, skipped: pending });
    pending = [];
    cursor = at + part.length;
}
const trailingSkipped = pending;

// Los huecos entre anclas son: la capa de tokens (cabecera), los componentes
// editados a mano, PickList, y la cola. Se atribuyen por su contenido.
const segments = [];
let prev = 0;
/**
 * Atribuye un hueco. El orden manda: los nombres que no casaron por Sass se
 * consumen en la misma secuencia en que aparecen en el entry original, así que
 * un hueco pertenece al siguiente nombre saltado pendiente. Solo los casos que
 * no vienen de Sass (capa de tokens, PickList) se detectan por contenido.
 */
const gapOwner = (text, skippedQueue) => {
    if (/--dnet-ref-/.test(text)) return "__tokens__";
    if (/\.dnet-pick-list\b/.test(text)) return "PickList";
    if (skippedQueue.length) return skippedQueue.shift();
    return "__tail__";
};

for (const a of anchors) {
    if (a.start > prev) {
        const text = frozen.slice(prev, a.start);
        if (text.trim()) {
            segments.push({ name: gapOwner(text, a.skipped), start: prev, end: a.start, exact: false });
        } else if (segments.length) {
            // hueco solo de espacios: se anexa al trozo anterior para no perder bytes
            segments[segments.length - 1].end = a.start;
        } else {
            segments.push({ name: "__tokens__", start: prev, end: a.start, exact: false });
        }
    }
    segments.push(a);
    prev = a.end;
}
if (prev < frozen.length) {
    const tail = frozen.slice(prev);
    if (tail.trim()) segments.push({ name: gapOwner(tail, trailingSkipped), start: prev, end: frozen.length, exact: false });
    else segments[segments.length - 1].end = frozen.length;
}

// -------------------------------------------------- verificación byte a byte
const reassembled = segments.map((s) => frozen.slice(s.start, s.end)).join("");
const covered = segments.reduce((a, s) => a + (s.end - s.start), 0);
if (reassembled !== frozen) {
    console.error("ABORTADO: la reconcatenación no reproduce el monolito byte a byte.");
    console.error(`  cubierto ${covered} de ${frozen.length} bytes`);
    process.exit(1);
}

// ------------------------------------------------------------------ informe
console.log(`monolito: ${frozen.length} bytes → ${segments.length} trozos`);
console.log(`reconcatenación byte a byte: OK\n`);
console.log("origen        trozo                                          bytes");
for (const s of segments) {
    const dest = s.name === "__tokens__" ? "Assets/styles/tokens/*.css"
        : s.name === "__tail__" ? "(descartado: comentario huérfano)"
        : destination[s.name] ?? `??? ${s.name}`;
    const how = s.exact ? "sass  " : "manual";
    console.log(`${how}  ${dest.padEnd(46)} ${String(s.end - s.start).padStart(7)}`);
}
if (handEdited.length) {
    console.log(`\neditados a mano tras la migración (recorte por offset): ${handEdited.join(", ")}`);
}

if (!apply) {
    console.log("\n(modo informe — nada escrito. Añade --apply para escribir)");
    process.exit(0);
}

// ------------------------------------------------------------------ escribir
/* Cabecera de una línea: la procedencia importa, pero cada byte de comentario
   viaja al bundle (webpack no minifica CSS aquí, ver optimization.minimize). */
const header = (origin) => `/*! ${origin} — generado una vez; a partir de aquí se edita a mano. */\n`;

const write = (rel, body, origin, prologue = "") => {
    const path = join(componentsRoot, rel);
    mkdirSync(dirname(path), { recursive: true });
    writeFileSync(path, prologue + header(origin) + body.trim() + "\n", "utf8");
};

const imports = [];
for (const s of segments) {
    const body = frozen.slice(s.start, s.end);
    if (s.name === "__tail__") continue;
    if (s.name === "__tokens__") {
        const blocks = body.split(/(?=^:root)/m).filter((b) => b.includes(":root"));
        const ref = blocks.find((b) => /--dnet-ref-/.test(b)) ?? "";
        const sys = blocks.find((b) => /--dnet-sys-/.test(b)) ?? "";
        write("Assets/styles/tokens/reference.css", ref, "capa de primitivas del monolito", '@charset "UTF-8";\n/* Primer @import del bundle. ORDEN: tokens -> base -> componentes.\n   Los temas los enlaza el consumidor DESPUES del bundle. */\n');
        write("Assets/styles/tokens/system.css", sys, "capa semántica del monolito");
        imports.push("./tokens/reference.css", "./tokens/system.css");
        continue;
    }
    const rel = destination[s.name];
    write(rel, body, s.exact ? `Sass ${s.name} (casó byte a byte)` : `${s.name} (recorte por offset; editado a mano en 6.0)`);
    imports.push(rel.startsWith("Assets/") ? `.${rel.slice("Assets/styles".length)}` : `../../${rel}`);
}

// La entrada contiene SOLO @import: cualquier comentario propio de la entrada
// lo emite webpack AL FINAL del bundle, no al principio. La procedencia y el
// orden se documentan en tokens/reference.css, que es el primer @import.
const entry = imports.map((i) => `@import "${i}";`).join("\n") + "\n";
writeFileSync(frozenPath, entry, "utf8");

console.log(`\nescritos ${imports.length} ficheros; entrada reescrita con ${imports.length} @import`);
