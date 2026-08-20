# Dnet.Blazor — AI agent reference

Machine-readable reference for **Dnet.Blazor 6.0.0**, aimed at coding agents
(Claude, Codex, Cursor, Gemini CLI, or any tool that reads project docs).

Point your agent at this folder and it will know the component API and the
styling tokens without guessing. Works with any LLM — these are plain markdown
files, no vendor-specific format.

## Using it in your project

Copy this folder into your own repository, then reference it from whatever file
your agent reads at startup (`AGENTS.md`, `CLAUDE.md`, `.cursorrules`, …). See
`AGENTS-snippet.md` for text you can paste.

```bash
# copy just this folder out of the library repo
npx degit ValdtechSSO/DnetBlazor/docs/ai/reference docs/dnet-blazor
```

Pin it to the library version you actually use: this reference is a snapshot of
6.0.0. If you upgrade the package, refresh this folder too.

## What's here

| File | Read it when |
|---|---|
| `component-index.md` | You need to know which components exist, or find the file for one |
| `components/<Name>.md` | Working with a specific component — every parameter, every token |
| `setup.md` | Installing, registering services, the overlay host, upgrading from 5.x |
| `theming.md` | Changing colours, spacing, radius, fonts; building a theme or dark mode |
| `design-tokens.css` | The full list of semantic tokens with their current values |
| `theme-dark-example.css` | A complete working theme, as a template |

The component files are independent — read only the one you need.

## The one idea worth knowing up front

Every component reads its appearance from CSS custom properties with a fallback
chain:

```css
--_radius: var(--dnet-tooltip-border-radius, var(--dnet-sys-radius-lg));
```

So you restyle by **setting tokens**, never by overriding library CSS. No
`!important`, no `::deep`, no specificity fights. Override at whatever scope you
need and inheritance does the rest:

```css
:root         { --dnet-btn-radius: 0; }            /* whole app  */
.admin-panel  { --dnet-list-item-height: 32px; }   /* one region */
```
```razor
<DnetButton style="--dnet-btn-background: crimson;">Delete</DnetButton>  @* one instance *@
```

Two layers are yours to write: `--dnet-<component>-*` for one component, and
`--dnet-sys-*` for everything at once.

## Two things that trip people up

**Floating components need a host.** Dialogs, tooltips, toasts and panels render
into one global `<DnetOverlay BaseZindex="1100" />` that belongs in
`MainLayout.razor`. Without it they silently render nothing.

**Themes don't reach inside overlays by default.** Overlays render outside the DOM
subtree where you opened them, so inheritance doesn't carry a theme in. Wrap the
region in `<DnetThemeScope Theme="dark">`. Details in `theming.md`.

## A note for agents

When a parameter or token isn't listed in these files, say so rather than
inventing one. A plausible-looking CSS token that doesn't exist fails silently —
no error, just no effect — which is much harder to debug than a compile error.

---

Generated from the library source. Runnable examples for every component live in
the repository's `samples/` folder, and there's a live demo at datalnet.com.
