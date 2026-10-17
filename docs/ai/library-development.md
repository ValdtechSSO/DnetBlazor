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

## Sample pages

Component showcase pages share `DemoPage` and `DemoCard` from
`samples/Dnet.Shared/Shared/Showcase`. Keep one page heading, short descriptions
and working examples with visible results. The helpers provide themed surfaces,
spacing and responsive grids through `demo-grid`, `demo-stack`, `demo-actions`
and `demo-preview`; page-specific scoped CSS should style the example content.
Do not override library geometry to make a sample fit. Use public sizing APIs
or a labelled local scroll region when a control needs more horizontal space.

Action buttons use `DnetButtonVariant.Filled` by default, with the theme's
primary color (`#5B5CE0` in the default theme). Use `Outlined` for secondary
commands such as Cancel, Back, Reset and Clear. Its foreground adapts to the
surface through `--dnet-sys-primary-content` for contrast. Keep primary-colored
text on that role; primary fills continue pairing primary/strong with on-primary.
Keep raw calendar/tab navigation separate: those controls
share base styles but do not carry action-variant classes. Public and legacy
button token overrides apply to both variants.

Preserve existing routes and stable interaction IDs when refreshing samples.
Check all cards and opened overlays at desktop and mobile widths in both themes,
including selected, disabled and empty states. Sample changes can intentionally
change visual baselines; record before/after evidence and update the official
Linux goldens in their matching environment.

The `/DoubleList` sample composes two `DnetList` instances; run the `List` fact
sheet for it. It is separate from `/FloatingDoubleList`, whose fact sheet and
visual baseline cover a different control.
The FloatingDoubleList sample keeps snapshots of both lists from
`OnSelectionChanged` so closing and reopening preserves transfers. Treat that
event as the assignment source; the legacy string `Value` alone does not track
transferred items. Local searches filter each side without replacing its data.
The sample's reset recreates the field to clear panel state, and the first
editable trigger has the stable ID `floating-doublelist-field`.

Grid filter panels compose `DnetFormField` and the library's Select, text,
numeric and date controls. Keep enum models behind integer Select option IDs:
the Select formatter does not support enum values directly. Dates display the
column format and store invariant ISO values for the filtering engine; numeric
advanced filters currently use integers. A simple filter needs both the grid's
`EnableFilter` flag and the column's `EnableSimpleFilter` plus `CellDataType`.

The `/ImageEditor` sample validates PNG/JPEG uploads, offers the bundled logo
and keeps the input stream alive until the dialog closes. An unchanged export
may return that same stream; an edited export may return a different one. Dispose
both owned streams after consuming the result, without losing the previous
confirmed image when an edit is cancelled.

## Localization

Control-owned text uses `Dnet.Blazor.Localization.IDnetLocalizer`, automatically
registered scoped by `AddDnetBlazor`. Spanish (`es-ES`) is the default; English
and regional variants are supported. Resources live in `Localization/DnetStrings`
(`.resx` Spanish neutral, `.en.resx` English). Keep both catalogs complete.

Razor controls resolve the service via `DnetLocalization.Get(IServiceProvider)`
to preserve no-DI use of presentational controls. A `DnetLocalizationObserver`
calls the owner's refresh callback without rendering DOM. Dispose subscriptions;
do not reset values, selection or search when the culture changes. Open overlay
content must observe localization itself: passing a resolved label/culture at
creation freezes it. Pass explicit overrides through and resolve defaults at the
rendering component. Refresh the overlay's ARIA metadata when it changes.

Explicit labels win over translations. Preserve partial `Strings`/`Labels`
overrides by tracking assignment per property, never by comparing text with an
English default. PickList/Toast retain instance → DI → catalog precedence.
Use scoped `Localizer.Culture` for human-visible formats; keep HTML number/date
wire values and grid filter/sort serialization invariant. Do not set global or
ambient cultures to change a user's language. Caller-owned business content stays
in the application's resources. See `docs/localization.md` for public usage.

## Reference files

- `docs/ai/architecture.md` — the three layers, rules R1–R10, themes, the
  overlay portal, legacy compatibility, commands.
- `docs/ai/pitfalls.md` — the eight failures that already happened, with causes
  and how to spot them. Read it in full the first time.
- `tools/component-info.mjs` — fact sheet for any component, extracted from
  source on the spot.
