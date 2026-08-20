# Developing Dnet.Blazor

This guide is for changing the library itself. For *using* it in an application,
see the README and `docs/styling/theming-guide.md`.

A Blazor component library with its own styling system: **hand-written plain CSS,
no frameworks**, built on three layers of custom properties. The promise is that
changing the look — or building a whole theme — means editing tokens, never
component rules.

That promise is fragile. It breaks through mistakes that **pass the linter and the
build without complaint**. That's why this skill exists.

## Before touching any component

Run the component fact sheet. It reads from source on every run, so it is never
stale:

```bash
node tools/component-info.mjs Tooltip     # full fact sheet
node tools/component-info.mjs --list      # every component
```

You get the files, the public token API, the **real fallback chain** behind each
private token, which semantic tokens it consumes, its CSS classes, its public
parameters — and a **WARNINGS** section listing the traps detected in that
specific component.

**Read the WARNINGS before writing anything.** Each maps to a failure that has
already cost time in this repo: C# parameters that silently override tokens,
variant classes in the wrong cascade order, API declared but never wired up,
rule violations.

If the script can't find the repo, point it there: `DNET_REPO=/path node scripts/…`.

## The rule that matters most

A component **declares private tokens** (`--_x`) and **reads public ones**
(`--dnet-<comp>-*`). It never declares a public one.

```css
/* ✔ the default lives in the fallback; a consumer's :root can still win */
.dnet-tooltip {
  --_radius: var(--dnet-tooltip-border-radius, var(--dnet-sys-radius-lg));
  border-radius: var(--_radius);
}

/* ❌ shields the component: :root { --dnet-tooltip-border-radius: 0 } no longer wins */
.dnet-tooltip { --dnet-tooltip-border-radius: 10px; }
```

`Components/Button/dnet-button.css` is the canonical reference. Copy its shape.

## Workflow

1. **Fact sheet** via the script. Read the WARNINGS.
2. **Read `docs/ai/architecture.md`** unless you already know the three layers,
   the legacy read-chain rule, and the overlay portal problem.
3. **Read `docs/ai/pitfalls.md`** before any non-trivial change. Eight concrete
   failures that already happened here, each with its cause.
4. **One component per PR.** No batches.
5. **Verify:**
   ```bash
   npm run buildDnetBlazor
   npm run generate:css-tokens   # if you touched system.css
   npm run lint:css              # 0 violations, 0 new
   ```
6. **Check it by eye in the sample app.** The linter cannot see appearance. This
   step is not optional.

## Minimum manual check

For any component you touch, in `samples/`:

- The public token works in **all three scopes**: `:root`, an intermediate
  container, and a single instance's `style` attribute.
- With `data-dnet-theme="dark"` it looks right **without any theme-specific
  rule**. If one is needed, the tokenisation is incomplete.
- Legacy token names still take effect.
- Variants (sizes, states) are visually distinct from one another.

## Constraints

- **Don't touch `system.css` or `reference.css`** without explicit confirmation.
  They are the contract every theme depends on, and they grow by accretion. If a
  value doesn't fit an existing role, say so and wait rather than inventing a
  token.
- **Don't change default appearance** without declaring it, with before/after
  screenshots.
- **≤ 12 public tokens per component**, no minimum. A token exists when someone is
  going to change it, not when they might.
- **No new `!important`.**
- C# signature changes go in `Changelog.md`: this is 6.0.0 and the styling API is
  public.

## When the visual change is deliberate

Regenerate the goldens under `tests/Dnet.Blazor.BrowserTests/VisualBaseline` and
**justify it in writing in the PR**, with before/after screenshots. A golden
regenerated without justification turns the suite into a rubber stamp that
approves anything.

## These files

- `docs/ai/architecture.md` — the three layers, rules R1–R10, themes, the
  overlay portal, legacy compatibility, commands.
- `docs/ai/pitfalls.md` — the eight failures that already happened, with causes
  and how to spot them. Read it in full the first time.
- `tools/component-info.mjs` — fact sheet for any component, extracted from
  source on the spot.

The full plan, including the reasoning behind each decision, lives in the repo at
`docs/implementation-plans/dnet-blazor-styling-architecture-plan.md`.
