# Dnet.Blazor styling architecture

Full context lives in the repo at
`docs/implementation-plans/dnet-blazor-styling-architecture-plan.md`.
This file is the working summary.

## The three layers

```
--dnet-ref-*      raw primitives: palette, scales. Nothing consumes these directly.
      ↓
--dnet-sys-*      semantic roles. A THEME IS THIS LAYER AND NOTHING ELSE.
      ↓
--dnet-<comp>-*   the component's public API. Consumers write here.
      ↓
--_x              file-private. The component reads this.
```

Files: `Components/Assets/styles/tokens/{reference,system}.css`,
`Components/<X>/dnet-<x>.css`, themes in `wwwroot/styles/theme/`.
Bundle entry: `Components/Assets/styles/dnet-blazor-styles.css`, which contains
only `@import` statements **in a meaningful order**.

## The pattern, as one rule

A component **declares** private tokens and **reads** public ones. Never the
other way round.

```css
.dnet-tooltip {
  --_radius: var(--dnet-tooltip-border-radius, var(--dnet-sys-radius-lg));
  border-radius: var(--_radius);
}
```

This is what lets a consumer set the token on `:root`, on an intermediate
container, or in a single instance's `style` attribute — and have all three work
through plain inheritance, with no extra rules in the library.

**The opposite mistake breaks the whole system:**

```css
/* ❌ the component shields itself: :root { --dnet-tooltip-border-radius: 0 } no longer wins */
.dnet-tooltip { --dnet-tooltip-border-radius: 10px; }
```

That failure **passes every visual test** — the component still looks right —
while breaking the entire promise. Hence the linter rule (R10).

## Backwards compatibility with old token names

Legacy names go **in the read chain**, never as a reverse definition:

```css
/* ✔ */ --_bg: var(--dnet-btn-background, var(--dnet-button-background-color, var(--dnet-sys-transparent)));
/* ❌ */ :root { --dnet-button-background-color: var(--dnet-btn-background); }
```

The old consumer **writes** the old name; it never reads it. Defining it from the
new name accomplishes nothing and silently breaks their styling.

## Rules the linter enforces (R1–R10)

| # | Rule |
|---|---|
| R1 | No colour literals in component files, including `fill` inside data-URI SVGs |
| R2 | No `:root` outside `tokens/` and `theme/` |
| R3 | Every declared token is used |
| R4 | Every used token is declared or has a fallback |
| R5 | Public `--dnet-(ref\|sys\|<comp>)-*`; private `--_*`, owned by the component's own file |
| R6 | Components never read `--dnet-ref-*` |
| R7 | No new `!important` |
| R8 | A theme file = one scope selector containing only custom properties |
| R9 | Monochrome icons use `mask-image` + `background-color`, never a hardcoded `fill` |
| R10 | A component never declares a public token |

```bash
npm run buildDnetBlazor      # rebuilds wwwroot/dnet-blazor-styles.css
npm run generate:css-tokens  # regenerates docs/styling/tokens.md
npm run lint:css             # 0 violations, 0 new
```

`lint:css` passes `--check-tokens-doc`, so it **also fails when `tokens.md` is out
of sync**, even with zero violations. If you touch `system.css`, regenerate.

## Themes

A theme redefines only the `system` layer, and is scopeable:

```css
[data-dnet-theme="dark"] { --dnet-sys-surface: #1e1f22; --dnet-sys-on-surface: #e3e5e8; }
```

Activate with `document.documentElement.dataset.dnetTheme = "dark"`.

**State layers are derived with `color-mix` from `--dnet-sys-on-surface`**, so
they invert automatically and a theme never needs to redeclare them. Lean on that
when tokenising: deriving beats enumerating.

## Overlays: themes don't cross the portal on their own

`DnetOverlay` renders dialogs, tooltips, toasts and panels into a global host in
`MainLayout` — **outside the DOM subtree where they were invoked**. Custom
property inheritance follows the DOM, so an overlay opened inside a themed
container **does not inherit that theme**.

The fix already in place: the `DnetThemeScope` component sets `data-dnet-theme` on
a `<div>` and publishes the name through a `CascadingValue`; components that open
overlays write it into `OverlayConfig.ThemeScope`, and the host emits it.

Arbitrary custom-property overrides **do not cross the portal** — a deliberate,
documented limitation. Use `OverlayConfig.PanelStyle` for those.

## Visual safety net

`tests/Dnet.Blazor.BrowserTests/VisualBaseline` holds reference screenshots
(*goldens*) per component, state and viewport. Any pixel change fails the test.

When an appearance change is **deliberate**, regenerate the goldens and justify it
in writing in the PR, with before/after screenshots. Regenerating without
justification is what turns the suite into a rubber stamp that approves anything.
