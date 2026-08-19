# Changelog for Blazor Library

## Version 6.0.0 (August 2026)

- Introduced runtime reference, semantic and component CSS token layers.
- Added `DnetThemeScope`, scoped themes and explicit overlay `PanelStyle` support.
- Added the CSS token audit to CI and documented the component token contract.
- Began the 6.x compatibility window for legacy styling tokens; new overrides use
  `--dnet-sys-*` and documented component tokens.
- Breaking: `DnetChip` color parameters (`BackgroungColor`, `Color`,
  `BackgroungColorSelected`, `ColorSelected`) lost their hard-coded defaults and
  are now `null` unless explicitly set. The previous defaults moved to the CSS
  fallback chain (`--dnet-chips-background`, `--dnet-chips-foreground`,
  `--dnet-chips-background-selected`, `--dnet-chips-foreground-selected`, backed
  by the new `--dnet-sys-surface-variant`, `--dnet-sys-on-surface-emphasis` and
  `--dnet-sys-primary-emphasis` roles), so the default look is unchanged and the
  tokens/themes now take effect. Chip size variants moved from inline styles to
  `.dnet-chip-size-{lg,md,sm,xs}` classes; the selected state is styled via
  `[aria-pressed="true"]`.
- `Dialog` migrated to the token architecture: the `:root` block was removed
  (`--dnet-dialog-*` names remain as read-chain fallbacks), color literals were
  replaced by `--dnet-sys-*` roles, and the close icon moved to a mask-based
  monochrome icon. The ghost token `--dnet-dialog-padding` (never declared; its
  negative-margin + re-pad pattern was inert) was restored via
  `--dnet-dialog-padding-left-right`. Two small, deliberate visual changes,
  captured before/after with the STY-002 suite: the dialog shadow now derives
  from `--dnet-sys-shadow-color` (softer than the old hard-coded 20/14/12 %
  layers) and the close icon renders in `--dnet-sys-on-surface` instead of a
  hard-coded black fill.
- `List`, `Select` and `Autocomplete` now share one public token namespace,
  `--dnet-list-*` (ADR-04a): a single override affects the three families, and
  each component keeps its own default in the private indirection (radii
  10/5/4 px, hover radii, headline and supporting-text differences). The old
  names `--dnet-select-list-*` and `--dnet-autocomplete-list-*` remain as
  read-chain links through 6.x. The six structural constants
  (`padding-*`, `header-footer-height`, `check-width`, `prefix-suffix-min-width`,
  `wrapper-horizontal-padding`) are no longer public API (still readable).
  The `* { box-sizing: border-box }` global rule that lived in the List
  stylesheet is scoped to each component subtree. Measured with STY-002:
  Select and Autocomplete are pixel-identical; the List search icon moved to a
  monochrome mask icon (R9) and renders in `--dnet-sys-on-surface`.
- `Paginator` migrated to the token architecture: the `:root` block was
  removed and the dead tokens (declared but never read, 14 of them) were
  dropped — they had no effect, so overrides never worked. The 8 live tokens
  remain as read-chain fallbacks. Icons now render in `--dnet-sys-on-surface`
  instead of hard-coded black, and the hover uses `--dnet-sys-state-hover`;
  the focus ring follows `--dnet-primary-color` with a `--dnet-sys-focus-ring`
  fallback. The List sample page now enables pagination so the paginator is
  demoed and covered by the STY-002 goldens.
- `Tooltip` migrated to the token architecture: the `:root` block was removed
  and its colors are now derived from system roles (`--dnet-sys-on-surface`
  at 90 % for the background, `--dnet-sys-surface` for the text), so the
  tooltip inverts automatically in dark themes. New public tokens
  `--dnet-tooltip-background` and `--dnet-tooltip-foreground`; the dead
  `--dnet-tooltip-margin` token was dropped. Measured with STY-002: the
  tooltip goldens are pixel-identical (0 px diff).
- `Checkbox` migrated to the token architecture: the `:root` block was
  removed and the dead `--dnet-checkbox-font-color` token was dropped. The
  border color is derived from `--dnet-sys-on-surface-subtle` via
  `color-mix` (verified to render exactly #b0b0b0), the checked background
  follows `--dnet-sys-primary` and the check/mixedmark color follows
  `--dnet-sys-surface`. The STY-002 goldens show a sub-pixel antialiasing
  shift in the label text (values verified identical) and are stable across
  two consecutive runs.
- `RadioButton` migrated to the token architecture: the `:root` block was
  removed and the dead tokens `--dnet-radio-button-border-color` (declared
  twice but never read — the ring uses `currentColor`) and
  `--dnet-radio-button-checkmark-path` were dropped. The checked color follows
  `--dnet-sys-primary-strong` (exact) and the disabled foreground is derived
  from `--dnet-sys-on-surface-subtle` via `color-mix` (renders exactly the
  old 38 % black). The STY-002 goldens show the same sub-pixel label
  antialiasing shift as Checkbox and are stable across two consecutive runs.
- `DatePicker` migrated to the token architecture: the `:root` block was
  removed and the ghost `--dnet-datepicker-input-height` (used without
  fallback in the icon wrapper, so it resolved to `auto`) was dropped by
  removing the inert declarations. The Material calendar tints are derived
  from `--dnet-sys-*` with `color-mix` percentages that render exactly the
  old black-alpha values (5 % → `state-hover`, 4 % → 6.4 % on-surface,
  10 % → 15.9 %, 38 % → 60.5 %, 54 % → `on-surface-subtle`, 87 % →
  `on-surface-emphasis`); measured with STY-002 the calendar is
  pixel-identical. The trigger icon moved to a monochrome mask icon (R9) and
  renders in `--dnet-sys-on-surface` (was a hard-coded black fill); the
  reset icon was already invisible (a zero-size span) and stays unchanged.
  The day cells now follow `--dnet-calendar-day-height` so the grid and the
  cells stay aligned when the token is overridden.
- `Toast` migrated to the token architecture. Approved extension of the
  semantic layer with status roles: `--dnet-ref-green-500`/`-yellow-500`/
  `-red-500` primitives and `--dnet-sys-success`/`-warning`/`-danger` roles
  (info reuses `--dnet-sys-primary-strong`, exact). The `:root` block was
  removed and `--dnet-toast-bg-*`, `--dnet-toast-margin` and
  `--dnet-toast-border-radius` remain as read-chain fallbacks. Toast icons
  moved to monochrome masks (R9) that follow `currentColor`, reproducing the
  old white/black fills per status. The toast-open STY-002 goldens show a
  sub-pixel rendering shift in the toast content (mask vs background-image);
  the suite is stable across four consecutive runs.
- `Stepper` migrated to the token architecture: the `:root` block was
  removed, the dead `--dnet-step-icon-height`/`-width` tokens were dropped
  and the unused `.dnet-app-stepper-icon*` rules (which read a Tree token)
  were removed. Step circles use `--dnet-sys-on-surface-subtle` (inactive)
  and `--dnet-sys-primary-emphasis` (selected/done/edit, the exact indigo),
  labels use `--dnet-sys-on-surface-emphasis`, the connector lines are
  derived with `color-mix` (exact 12 % black) and the background follows
  `--dnet-sys-surface`. Measured with STY-002: pixel-identical (0 px diff).
- `Tree` migrated to the token architecture: the `:root` block was removed,
  the dead `--dnet-tree-icon-height` token was dropped and the expand/collapse
  chevrons moved to monochrome masks (R9) rendering in
  `--dnet-sys-on-surface` (were hard-coded black). The page-global
  `li ul { background: transparent }` rule was scoped to the tree subtree.
  Measured with STY-002: only the chevron color changes (44 px).
- `Form` (form field) migrated to the token architecture: the `:root` block
  was removed and the four `--dnet-form-field-*` tokens remain as read-chain
  fallbacks. The control border maps exactly to `--dnet-sys-border` and the
  disabled background to `--dnet-sys-state-hover`; labels/hints use
  `on-surface-subtle`/`on-surface-muted` and the clear button moved to a
  monochrome mask (R9). The validation error color adopts
  `--dnet-sys-danger` (deliberate change: bright red to the danger role).
  The orphaned `--dnet-component-xs-font-size` and
  `--dnet-component-supporting-text-color` declarations were removed (their
  nested legacy links in the Autocomplete chain remain).
- `ExpansionPanel` migrated to the token architecture: the `:root` block was
  removed and the three `--dnet-accordion-*` tokens remain as read-chain
  fallbacks (radius -> `--dnet-sys-radius-sm`, shadow -> `--dnet-sys-elevation-1`,
  title font -> `--dnet-sys-text-md`). Colors map exactly (`on-surface-emphasis`
  for text, `on-surface-subtle` for the description/indicator, a 41.3 %
  `color-mix` for the disabled tint). The orphaned
  `--dnet-component-border-radius` declarations were removed (the nested
  legacy links in the Autocomplete and Chips chains remain). The goldens show
  the same sub-pixel header-text antialiasing shift and are stable.
- `ConnectedPanel` migrated to the token architecture: the `:root` block was
  removed and the two `--dnet-connected-panel-*` tokens remain as read-chain
  fallbacks (background -> `--dnet-sys-surface`, padding stays 2 px).
  Pixel-identical (0 px). The STY-002 goldens were refreshed to the current
  rendering baseline (sub-pixel antialiasing drift accumulated from the
  session's app rebuilds; verified stable across two consecutive runs).
- `FloatingPanel` migrated to the token architecture: the `:root` block was
  removed (it wrongly declared `--dnet-toast-margin`), the panel background
  maps to `--dnet-sys-surface`, the shadow to `--dnet-sys-elevation-1` and
  the demo-content colors to the closest roles (`--dnet-sys-primary-strong`
  for the link blue, `--dnet-sys-warning` for the orange — approved
  mapping). Pixel-identical in the sample (0 px).
- `Tabs` migrated to the token architecture: the four remaining color
  literals were replaced with exact derivations — the disabled label tint
  (38 % black) via a 60.5 % `color-mix`, the divider (12 % black) via a
  19.4 % `color-mix`, and the active-tab underline `#3f51b5` via
  `--dnet-sys-primary-emphasis`. Pixel-identical (0 px).
- `Spinner` migrated to the token architecture: the `:root` block was removed
  and the two unprefixed tokens (`--spinner-color`,
  `--spinner-mask-background`) were dropped per the namespace rule. The bars
  follow the new `--dnet-spinner-color` token with a
  `--dnet-sys-primary-strong` fallback and the mask is derived from
  `--dnet-sys-on-surface` via `color-mix`. The bar color shifts slightly
  from #69b8d6 to primary-strong (declared; not captured by goldens since
  the open spinner is timer-driven and excluded).
- `FloatingDoubleList` migrated to the token architecture: the remaining
  literals map exactly — shadow -> `--dnet-sys-elevation-1`, background ->
  `--dnet-sys-surface`, text -> `--dnet-sys-on-surface-emphasis`, footer
  color #42b0d5 -> `--dnet-sys-primary-strong`. Pixel-identical (0 px).
- `ImageEditor` migrated to the token architecture: the color literals map to
  the closest roles (canvas backdrop -> `surface-hover`, crop border ->
  `primary-strong`, upload tones -> `border-strong`/`on-surface-muted`,
  text -> `on-surface`/`on-surface-emphasis`) and the four toolbar icons
  moved to monochrome masks (R9). The dead `.dnet-btn-*` rules were removed.
  Declared changes; the editor is excluded from STY-002 goldens (canvas).

## Version 5.0.5 (August 2026)

- Added `PickList<TItem, TKey>` for controlled multi-selection over local and
  server-side paged data.
- Added PickList search, selection persistence, localization and browser
  regression coverage.
- Added trusted NuGet.org publishing through GitHub Actions.

## Unreleased — stabilization

- Corrected overlay identity, viewport listener ownership and flexible positioning offsets.
- Reworked toast and spinner timing around cancelable tasks, and made paginator bounds/debounce deterministic.
- Fixed selection and dynamic-parameter behavior across Grid, List, Chips, Tree, Tabs and steppers.
- Added deterministic cleanup for form, Grid and ImageEditor interop resources.
- Added `AddDnetBlazorMaterial()` and Material form-event registration.
- Improved keyboard and ARIA semantics for buttons, chips, tabs, tree, expansion panels, paginator, toast and tooltip.
- Added baseline unit tests and a CI build/test workflow.

## Version 5.0.0 (November 2025)

### Tooltip Component Improvements

#### Memory Management
- **IDisposable Implementation**: TooltipService now properly implements IDisposable pattern for automatic resource cleanup
- **Timer Management**: All show and hide timers are now properly tracked and disposed
- **Reference Tracking**: Active tooltips are tracked in dictionaries to prevent memory leaks
- **Automatic Cleanup**: Dispose() method ensures all resources are released when the service is disposed

#### Show/Hide Delays
- **ShowDelay Property**: Tooltips can now be configured to appear after a specified delay (in milliseconds)
- **HideDelay Property**: Tooltips can now be configured to hide after a specified delay when the mouse leaves
- **Smart Cancellation**: 
  - If mouse leaves before ShowDelay completes, tooltip creation is cancelled
  - If mouse re-enters during HideDelay, the hide operation is cancelled
- **Thread-Safe Operations**: All timer operations are protected with locks for concurrent access

#### Technical Improvements
- **ID Mapping System**: Placeholder IDs are mapped to real overlay IDs for proper tracking with delayed tooltips
- **Unified Show Logic**: Internal ShowInternal() method reduces code duplication
- **Improved Close Logic**: Separate CloseImmediate() method for immediate cleanup vs delayed closing

#### Usage Example
```csharp
var tooltipConfig = new TooltipConfig()
{
    Text = "This tooltip appears after 500ms",
    ShowDelay = 500,  // Wait 500ms before showing
    HideDelay = 200   // Wait 200ms before hiding
};

_tooltipReference = TooltipService.Show(tooltipConfig, _element);

// When mouse leaves
TooltipService.Close(new OverlayResult { OverlayReferenceId = _tooltipReference.GetOverlayReferenceId() });
```

## Upcoming Changes v.4.0.0
- **Default Theme Update**: Form components will now default to the 'Plain' theme.
- **Separate NuGet Package for Material Theme**: Users wishing to use the Material theme can find it in a separate NuGet package, available from January 2024.

## Recommended Migration Path
- **Version 3.2.0 as a Transition Release**: Users are advised to use version 3.2.0 as an intermediary step when migrating from version 3.2.0 to 4+.
  - This version (3.2.0) includes both Material and Plain themes in the same package, facilitating a smoother transition to later versions where these themes are separated.
