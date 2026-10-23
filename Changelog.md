# Changelog for Blazor Library

## Unreleased

## Version 6.0.20-rc.2 (October 2026)

- Grid measures each instance's own header, keeping the horizontal scrollbar
  aligned when another grid is open in a floating panel.
- `BlgHeader.HeaderContainerId` allows the owning grid to identify its header;
  direct header consumers retain the previous default.
- Grid preserves measurable horizontal overflow when classic scrollbars occupy
  the full height of the scrollbar viewport.

## Version 6.0.19-rc.2 (October 2026)

- PickList searching and clearing do not mark a surrounding EditForm modified
  or trigger validation for an internal search field.
- Checkboxes and radio buttons leave 10px between their indicator and label,
  preserving the existing geometry of indicators without a label.
- List uses a common 20px inset and 20px between search, header, items and
  pagination, with aligned avatars and checkboxes. Its responsive paginator and
  count-only footer follow the same spacing, including in composed double lists.
- The linear Stepper example keeps its action buttons on the same surface,
  aligned to the right with padded edges and 10px between actions.
- Tabs add 24px of inner spacing, reduced to 20px on mobile, around navigation
  and content. The `--dnet-tabs-padding` token customizes this inset.
- Shared form fields use an outlined circle with an X in a subdued theme color
  for the clear action.
- PickList inherits the List background and uses the shared form field and text
  input for searching, including its floating label and clear action.
- PickList selection actions use the shared 40px button size with 10px spacing.
  Search, actions, rows and pagination share aligned insets, with 20px between
  sections.

## Version 6.0.18-rc.2 (October 2026)

- Reduced the sample header spacing between user details and the account icon.

- Localized example screens and their opened content with separate Spanish and
  English application resources. Navigation menu labels remain unchanged.
  Existing values, selections and validation state survive language changes.
- Added optional `TextLocalizer` callbacks to DialogConfig/DnetDialog and
  ToastConfig/DnetToast for live application-owned titles, messages and actions.
- Step and tab headers refresh when their supplied labels change, preserving
  the active step/tab and completion state during language switches.

- Added shared control localization with Spanish as the default and English as
  the second language, including regional formats, live updates in open panels,
  accessible labels and parsing messages. Explicit text overrides and per-user
  scope are preserved. Added a language selector and `/Localization` sample.

- The sample header is 65px tall and uses circular library buttons with connected
  panels for language and theme. The triggers show the active language code and
  theme icon, and the panels mark the current selection.

- Refreshed the FloatingDoubleList sample with a responsive assignment workspace,
  local searches, a live roster, a protected lead, read-only preview and reset.
  Transfers persist when reopening the panel. FloatingDoubleList now forwards its
  existing row-template/search-label options and supports keyboard opening,
  Escape dismissal and focus restoration. Both lists share the full popup width;
  the mobile list fits its panel without relying on viewport-based widths.

- Grid advanced filters now use DnetFormField, DnetSelect and library text,
  numeric and date inputs. The panel adapts to the viewport and validates active
  conditions before applying a filter.
- Select emits explicit true/false ARIA values for its expanded and disabled
  states, including when used inside the grid's advanced filter panel.

- Fixed the FilteringExample sample's empty filter row by enabling simple filters
  on every column and declaring the data types for IsActive and Phone.

- Added `DnetButtonVariant`: Filled is now the default primary action and Outlined
  distinguishes secondary actions. Existing size, pill-radius and token overrides
  remain supported; `--dnet-btn-border-color` customizes the outline.
- Changed the default primary color to `#5B5CE0`, with contrasting primary text
  and a theme-adapted outlined-action color. Added visible keyboard
  focus and disabled action states, and applied the hierarchy across the samples
  and internal commands. `--dnet-sys-primary-content` keeps colored text legible
  on normal surfaces while primary fills retain their contrasting foreground.

- Increased the expanded left navigation column default width from 250 to
  270 px, including its minifier and submenu widths.

- Added 10 px of extra text spacing inside the expanded desktop and responsive
  navigation menus. Icon links start at the same inset as text-only links,
  with 8 px between their icon and label.

- Refreshed the PillTabs sample with a themed request inbox, live approval and
  count updates, synchronized selections, full-width settings and separate
  scrolling examples in LTR and RTL. Reset restores the inbox while keeping
  the other examples independent.
- PillTabs keeps the selected option visible after browser resizing while
  preserving manual scrolling within its container.

- Refreshed the ImageEditor sample with a responsive upload workspace, a local
  demo image, editing guidance and a downloadable result with file details.
  Added validation, cancel/reopen/reset flows and disposal of upload streams
  after both edited and unchanged exports.
- ImageEditor stacks its stage and editing panel on small screens, preserving
  a usable image area and scroll access to all controls. Images that cannot
  be decoded display an alert, and icon actions have accessible names.

- Refreshed the Spoiler sample with responsive themed cards for editorial
  content, shared expansion controls, dynamic descriptions, short content and
  an initially expanded RTL story. An editable note demonstrates that content
  and input state survive collapsing and reopening the details.

- Refreshed the Timeline sample with themed example cards, responsive order
  tracking, project milestones, team activity and right-to-left reading. The
  order example can advance through delivery and reset, keeping event status,
  accessible labels, dates and the progress summary in sync.

- Refreshed the Double List sample with a responsive team assignment workspace,
  keyboard-accessible transfer actions, local sample data, display preferences
  and a live project roster. Fixed project leads cannot be transferred; moving
  people resets searches and pending selections together.
- Local List searches with no matches now remain empty, including after sorting,
  rather than falling back to the full collection. Pressing Enter in the search
  field no longer submits its form or reloads the page.

- Refreshed the PickList sample with themed, responsive cards for local and
  paged team selection, readable selection summaries, custom team templates
  and a localized example with a protected selection.

- Refreshed the Chips sample with topic filters, an optional project priority,
  removable teammates, a four-size gallery and themed icon/disabled examples.
  Selection summaries, clear filters and restore actions reflect the live state.

- Refreshed eighteen component samples with shared page headers, themed example
  cards, responsive layouts and live results: Dialog, Select, Connected Panel,
  Floating Panel, DatePicker, TimePicker, Tabs, Stepper, DynamicStepper, Toast,
  Tooltip, Spinner, List View, Tree View, Expansion Panel, Radio Button, Checkbox
  and Slide Toggle. Profile editors and filters now return usable results, and
  the loading example finishes cleanly and cancels pending work on navigation.
  Tooltips retain their trigger positioning on mobile and tablet instead of
  appearing at the top-left of the viewport.

- Autocomplete and Select panels with an omitted width now follow the field's
  width instead of resizing with virtualized results. Sizing remains stable
  during filtering and scrolling, follows field resizing on desktop/tablet and
  preserves explicit widths, size constraints and the mobile layout.

- Redesigned the Autocomplete sample with separate example cards, live value
  previews, richer result templates and a responsive usage guide. The basic and
  detailed examples retain their shared value; the decorated example is independent.
  External value changes now synchronize floating labels and clear actions
  without remounting the autocomplete or emitting an extra value change.

- Redesigned the Forms sample with grouped contact, date, numeric and address
  fields, an integrated appearance picker and a responsive reference gallery.
  Filled and transparent modes preserve the same input instances and values.

- Redesigned the FormValidation sample with grouped employee details, address
  cards, inline feedback, validation progress and clear save/reset actions.
  Required-field validation now associates nested address errors with their
  actual inputs and validates employee details consistently.

- Added `DnetButton.Size` (`Small`, `Medium`, `Large`) and a decorative leading
  `IconContent` slot. Heights are 32/40/56px, horizontal padding 12/16/24px,
  icons 20/20/24px and icon-to-text gaps 4/8/8px for every button shape.
  Medium (40px) is the default; explicit sizing and legacy tokens remain valid.

- Buttons use a pill shape by default in every theme. Explicit radius tokens,
  including the legacy button-radius token, still override the default.

- Grid headers and rows reserve the same native scrollbar gutter, keeping
  pinned columns aligned across scrollbar preferences. Horizontal scrolling
  includes column borders so the final column can be revealed completely.

- Checkbox controls are 18px and radio buttons 20px by default. Grid selection
  checkboxes also use 18px; existing component size tokens remain supported.

- Fixed overlapping labels and week values in DatePickerWeek fields. The weekly
  selector uses the shared select layout and keeps the form's floating label in
  sync with initial, selected and programmatically updated values.

- Inline calendars, including the weekly calendar, use the same raised theme
  background as popup panels and honor `--dnet-calendar-background` throughout.
  Calendar borders match their background, including custom background colors.
  The inline weekly calendar's outer background follows the calendar radius.

- Redesigned the Theming sample with grouped appearance controls, live component
  previews, color presets, shape options and a CSS export preview. Added reset,
  accurate theme defaults and synchronization with the application theme picker.

- Opened panels share the TimePicker surface: raised theme background, large
  rounded corners and elevation 3. Applied to selects, autocomplete, calendars,
  connected/floating panels, floating double lists, dialogs and Grid filters.
  Explicit styling tokens remain supported.

- Grid data rows are 56px high by default, with matching sample configurations
  and text spacing. Explicit row-height settings remain supported.

- Grid filter rows reserve the complete input height plus vertical spacing,
  keeping pinned headers and the body aligned when form spacing is customized.

- Form fields follow Material spacing: 56px minimum height, 16px text insets,
  24px icon slots with 12px edge insets and 16px text gaps. Fill floats its label
  inside the control; supporting text and character counts share a row 4px below
  it. Added geometry tokens and a reference gallery in `/Forms`.
- Added `DnetFormField.Appearance`, with a filled background by default and
  transparent `Standard`/`Legacy` options. Fill adapts to the active theme,
  including disabled fields. Added the `--dnet-form-field-background` styling
  token, HTML attribute forwarding and a live appearance selector in `/Forms`.
- Filled fields use the same color for their border and background, including
  disabled states and custom background tokens.
- Replaced the Select's filled triangle with the Bootstrap Icons down chevron,
  preserving the foreground styling token and theme colors.
- Fixed the calendar icon mask in DatePicker and DateRangePicker, and prevented
  legacy sample styles from overriding the icon color in dark themes.
- Added `DnetTimeline` and `DnetTimelineItem` to display activity with rich content,
  status markers, timestamps, custom bullets, RTL and responsive layouts.
- Added `DnetSkeleton` for animated loading placeholders, circular variants and
  preserving wrapped content while loading. Includes reduced-motion support.
- Added `/Timeline` and `/Skeleton` samples and component documentation.
- Added `DnetSpoiler` with a configurable preview height, automatic overflow
  detection, controlled or internal expansion, localized labels and accessible
  keyboard controls. Hidden interactive content is removed from keyboard focus.
- Added five styling tokens, reduced-motion support and the `/Spoiler` sample.

## Version 6.0.17-rc.2 (October 2026)

- PillTabs hides horizontal scrollbars and fades only the edges with hidden
  options, updating on scroll, resize and content changes in LTR and RTL.
- Added `DnetPillTabs<TValue>` and `DnetPillTabItem<TValue>`: rounded tabs with
  optional counts, two-way selection binding, disabled items, custom label and
  panel templates, full-width layout and horizontal overflow. Keyboard navigation
  supports Left/Right, Home and End, including inherited RTL direction.
- Added the `/PillTabs` sample and twelve public styling tokens that inherit
  correctly from root, container and instance scopes and adapt to existing themes.

- Added VS Code launch profiles for ClientSide and KestrelServer, including a
  compound launch that builds once and starts both projects. The development
  Kestrel host now respects the configured local port.

## Version 6.0.16-rc.2 (October 2026)

- Migrated the library, samples and test projects to .NET 11 RC2 (`net11.0`),
  with ASP.NET Core and .NET packages at `11.0.0-rc.2.26504.105`.
- Pinned SDK `11.0.100-rc.2.26504.105` in `global.json` and aligned GitHub
  Actions, Docker images, the WebAssembly publish profile and the Playwright
  installer with RC2. Consumers of this build must target .NET 11.
- Updated bUnit to 2.11.3 and AngleSharp to 1.8.1 to support the .NET 11
  virtualization callbacks in component tests.

## Version 6.0.15 (September 2026)

- Form: the trailing icon of a field with a clear button - the clear mark, or the
  empty-state glyph a consumer passes in `EmptyIconClass`, such as the grid's
  search magnifier - now sits 5px away from the right border instead of against
  it.

- FloatingDoubleList: the selected values shown by the control stay on one line
  and end in an ellipsis when they no longer fit its width, instead of wrapping
  onto a second line that the fixed control height cut in half.

- List: fixed drag and drop between connected lists. The fallback row markup,
  which is the one the double list renders, made items draggable without ever
  marking them as the drag payload, so every drop carried an empty list and the
  connected list ignored it. Dragging now marks the item, and the connection
  guard is covered by tests.
- FloatingDoubleList: the right container was named "RghtContainer" on both
  sides of the connection contract.

- ImageEditor: the X/Y/W/H labels of the selection fields start on the field's own
  padding instead of pressed against its border, with a gap before the value.

- ImageEditor: rebuilt the dialog around the library's own components, in the
  layout of a two-column editor: a header with the file name and source size, a
  toolbar with Crop, rotate left/right, flip horizontal/vertical and Reset, the
  picture on a bounded stage with a zoom control, a side panel with the live
  preview, the ratio presets (`DnetChipList`) and the selection fields
  (`DnetFormField` + `DnetInputNumber`), and a footer with Cancel and Apply. The
  buttons, chips, fields and tooltips come from Button, Chips, Form and Tooltip;
  the editor only places them and paints the crop overlay.
- ImageEditor: added a selection lock by ratio (Free, 1:1, 4:3, 16:9), editable
  X/Y/W/H fields in source pixels, a zoom control that opens fitted to the stage
  and scrolls when the picture no longer fits, quarter turns in both directions
  - wiring the `RotateClockWise` and `RotateCounterClockWise` values that the
  control enum declared but never used - a size badge on the crop box, a
  rule-of-thirds grid and tooltips on the icon-only actions.
- ImageEditor: Crop now applies the selection to the working image instead of
  only staging a result, and Reset is a real undo: it re-decodes the original
  bytes, clears the quarter turns and returns to the fitted, centred selection.
- ImageEditor: the working image is bounded by `ImageContainerHeight` and
  `ImageContainerWidth`, which now size the stage instead of the overlay, and the
  default `ModalDialogHeight` grew from 668 to 780 for the two-column layout.
  Button roles (primary, outlined, square icon) are set per instance through
  Button's public tokens, so no component stylesheet declares another
  component's tokens.

- ImageEditor: fixed the preview, which drew the top-left corner of the picture
  at the size of the selection instead of the selected pixels, so it never
  matched the crop box while dragging. It now mirrors the selection live, and it
  shows the selection from the moment the dialog opens instead of the whole
  image.
- ImageEditor: the exported image now keeps the resolution of the selection in
  source pixels instead of always being resized to the preview box (170 x 170),
  it preserves the aspect ratio of the crop and the format of the source image
  (PNG and WebP keep their transparency), and it no longer turns transparent
  pixels black when the output is JPEG.
- ImageEditor: an untouched image is returned byte for byte, and every other
  edit is encoded exactly once. Flipping now mirrors the working pixels instead
  of re-encoding a JPEG on every click, so both repeated flips and a crop
  followed by a flip keep their full quality.
- ImageEditor: added `MaxOutputDimension` (caps the longest edge of the result;
  zero keeps the native resolution), `OutputFormat` (forces the output format)
  and `OutputQuality` (lossy quality, 95 by default).
- ImageEditor: fixed the crop overlay, which was pinned to
  `ImageContainerHeight`/`ImageContainerWidth` and therefore misaligned with the
  image for any picture whose natural size differs from those values; the
  overlay and its masks and handles now track the displayed image exactly.
- ImageEditor: dragging and resizing run entirely in the browser. The selection
  is written as four `--_crop-*` custom properties that the stylesheet turns
  into the box, the masks and the handles, so a gesture no longer costs a
  JS -> .NET call and a component render per pointer frame; .NET receives a
  throttled progress notification and the final rectangle.
- ImageEditor: the image is transferred without base64. Bytes arrive through a
  `DotNetStreamReference` and the result is returned as a blob stream, so a
  large picture no longer becomes a multi-megabyte string on either side. The
  editor draws the picture and the preview on canvases and never re-reads the
  DOM for pixels.
- ImageEditor: Photon WASM is loaded in the background on first use and only
  resamples the selection, releasing its buffers explicitly. `MaxFileSizes` now
  falls back to `DnetImageEditor.DefaultMaxFileSize` (15 MB) when it is not set.
- ImageEditor: the measured dimensions use the `--dnet-sys-primary` token
  instead of a hardcoded colour, the preview label shows the size of the result,
  the left and right handles use the correct resize cursors, and every editor
  instance keeps its own state, so two editors on one page no longer interfere.
- ImageEditor, breaking: the default output is the native resolution of the
  selection; set `MaxOutputDimension` to cap it. `DialogData.ImageUrl` was
  removed, `IStyleService.GetResizerStyles`/`GetMaskStyles` and
  `IImageEditorService.UpdateResizersData`/`PlaceMasks` were removed together
  with the `MaskData` model, and `IStyleService.GetStyles`,
  `GetCropContainerStyles` and `GetImagePreviewStyles` now return custom
  properties instead of finished positions. The result stream handed to
  `OnImageSelected` belongs to the consumer from that moment on.
- ImageEditor sample: uploads reach the editor without being downscaled and
  re-encoded to 640 x 480 first, and the page reports the size of the result.

## Version 6.0.14 (August 2026)

- Increased Toast title typography from 12px to 14px and message typography
  from 10px to 12px while preserving their visual hierarchy.
- Replaced the fixed light scrollbar color with theme-derived thumb and hover
  colors that remain visible without overpowering dark surfaces.

## Version 6.0.13 (August 2026)

- Redesigned Toast around an accessible, theme-aware stack with elevated
  surfaces, severity accents, progress timing, hover/focus pause, optional
  actions, persistent notifications, Escape dismissal and a four-item queue.
- Preserved the existing toast service, severity, timing and customization API;
  added nullable millisecond duration, localized close text and per-stack
  visibility limits.
- Fixed Toast automatic dismissal so the notification closes reliably when its
  progress duration ends while remaining synchronized with hover and focus.
- Aligned Grid group expand arrows with their labels and applied the same
  spacing used by Tree nodes.
- Updated the AdminLayout minifier surface to derive from the active theme.

## Version 6.0.12 (August 2026)

- Fixed Grid selection checkboxes on responsive browsers by removing the native
  control chrome and centering the token-styled indicator in its column.
- Placed FloatingPanel scrollbars flush with the side edge while preserving the
  content and footer spacing.
- Added a responsive TimePicker presentation that opens as a centered,
  focus-trapped modal on small viewports while retaining the connected desktop
  overlay.
- Prevented TimePicker options from showing through the space above its sticky
  responsive header while scrolling to the selected time.
- Added an explicit, localizable cancel action to the responsive TimePicker
  dialog.
- Prevented the responsive TimePicker from reopening immediately after canceling
  or selecting a time when focus is restored to its input.
- Aligned Tree expand arrows, selection checkboxes and node labels on one
  vertical center line, with consistent spacing between controls.

## Version 6.0.11 (August 2026)

- Added `DnetSlider`, a form-aware native range control with single and dual-thumb
  modes, discrete formatted value labels, tick marks, semantic colors, RTL,
  drag lifecycle events and token-based styling.
- Added `DnetBadge`, a token-driven status indicator for arbitrary content with
  logical positions, three sizes, semantic colors, overlap, hidden and disabled
  states, RTL support and accessible descriptions.
- Expanded `DnetDatePicker` without breaking its string-based binding API: it
  now shares a localized, accessible calendar with configurable views,
  constraints, actions, customization hooks, lifecycle events and an adaptive
  touch dialog. Added the inline `DnetCalendar` and typed
  `DnetDateRangePicker`, including previews, comparison ranges, custom range
  strategies and responsive presentation.
- Added `DnetTimePicker`, a `TimeOnly?` control integrated with `DnetFormField`,
  with typed input, generated or explicit time options, min/max constraints,
  keyboard navigation and an accessible, responsive overlay listbox.
- Added `DnetSlideToggle`, an accessible, form-aware on/off switch with native
  keyboard behavior, label placement, disabled/required and full-width modes,
  and a token-based styling API.
- PickList item selection now uses a right-aligned `DnetSlideToggle` with
  transparent row backgrounds, while preserving controlled selection, disabled
  items and accessible item labels.

## Version 6.0.10 (August 2026)

- Added `PickList.IsItemDisabled` so callers can render inherited assignments
  as selected but unavailable for modification.

## Version 6.0.9 (August 2026)

- Fixed `PickList` layouts that fill their parent height: items retain their
  configured row height and the remaining space stays empty and scrollable.

## Version 6.0.8 (August 2026)

- Added `GridOptions.ResponsiveContentColumnMaxWidth`. On responsive
  viewports, it sizes unpinned columns from the loaded content, caps each width
  at the configured value and keeps headers aligned with their cells.

## Version 6.0.7 (August 2026)

- Fixed grouped Grid rows and the grouping placeholder column so they respect
  `GridOptions.RowHeight` instead of `GroupGridColumn.Height`.

## Version 6.0.6 (August 2026)

- The grid row node exposes `FocusAsync` for accessible focus restoration. Consumers must supply a fallback when a virtualized, filtered, or deleted row is no longer rendered.

## Version 6.0.5 (August 2026)

- `FloatingPanelConfig` now exposes accessible overlay semantics, live-region
  announcements and focus-management options. Open floating panels can replace
  their dynamic component and parameters through `OverlayReference.UpdateContent`.

## Version 6.0.4 (August 2026)
- Fixed the Grid selection checkbox column so it uses `GridOptions.RowHeight`
  instead of a fixed 40 px height.
- Improved `FloatingDoubleList` service initialization, sorting callbacks,
  selection-state handling and overlay origin fallback.
- Removed the obsolete bundled app-skills documentation artifacts.

## Version 6.0.3 (August 2026)

- Added the Modern semantic theme, including its demo asset and persisted theme
  selector support.
- Added XML summaries to component parameters and regenerated the agent-facing
  API reference.

## Version 6.0.2 (August 2026)

- Fixed `BlgGrid` hover synchronization for `RowSpanFn` cells. A cell spanning
  multiple rows now highlights correctly whichever covered visual row is hovered.
- Added browser regression coverage for row-span hover behavior.
- Simplified the PickList sample styling pipeline, removing the redundant
  CSS-isolation preload emitted by the sample application.

## Version 6.0.1 (August 2026)

- `DesktopLayout` structural dividers (header, footer and lateral columns) are
  now disabled by default. Consumers can enable and customize them through
  `--dnet-layout-divider-width`, `--dnet-layout-divider-style` and
  `--dnet-layout-divider-color`, without targeting internal selectors.
- `DesktopLayout` forwards unmatched HTML attributes, so its layout tokens can
  also be scoped directly with its `style` attribute.

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
- `Overlay` migrated to the token architecture: the dark backdrop now uses
  the new `--dnet-sys-scrim` semantic role (32 % black, derived from
  `--dnet-ref-neutral-1000` in the system layer — approved), so the scrim
  stays dark in every theme.
- `DatePickerWeek` migrated to the token architecture: the `:root` block was
  removed, the six `--dnet-datepickerweek-*` tokens remain as read-chain
  fallbacks (input container border -> `--dnet-sys-border`, selected ->
  `--dnet-sys-primary-strong`, day disabled/today tints via `color-mix`) and
  the Material calendar literals map to the same exact derivations used in
  `DatePicker`. No sample page exists, so this component is not covered by
  STY-002 goldens.
- `DatePickerWeekRaw` migrated to the token architecture: same exact
  derivations as `DatePickerWeek`/`DatePicker`; the two dead input tokens
  (`--dnet-datepickerweek-raw-input-container-border`,
  `--dnet-datepickerweek-raw-input-height`) were dropped. Not covered by
  STY-002 goldens (no sample page).
- STY-002: the open-calendar state of `DatePicker` was excluded from the
  goldens (intermittent sub-pixel variance in the day column, 200-400 px
  between runs); only the input is captured, like the open Toast/Spinner.
- `Grid` (dnet-blgrid.css) migrated to the token architecture: both `:root`
  blocks were removed and the 37 `--blg-*` declarations became `--dnet-grid-*`
  read-chains with `--blg-*` kept as legacy links through 6.x (the sample
  app's `--blg-font-family` override keeps working). Dead tokens
  (`--blg-header-height`, `--blg-row-border-width`,
  `--blg-root-wrapper-border-color`) were dropped and the ghost
  `--foreground-color` usage was restored via `--dnet-grid-foreground-color`.
  Colors map to system roles: foreground #5f6368 -> `--dnet-sys-on-surface`
  (exact), borders -> `--dnet-sys-border`, surfaces -> `--dnet-sys-surface`,
  paginator 54 % black -> `--dnet-sys-on-surface-subtle`, elevation shadows ->
  `--dnet-sys-elevation-1`, validation red -> `--dnet-sys-danger`, filter-hover
  orange -> `--dnet-sys-warning`. The 12 inline SVG icons moved to monochrome
  masks (R9) and now render in `--dnet-sys-on-surface` instead of hard-coded
  black (`--dnet-sys-on-surface-emphasis` for the checkbox check). Declared
  visual changes: row hover/selected use a light primary tint instead of the
  old blue #EFF6FC, clicked rows use the neutral `--dnet-sys-state-pressed`
  instead of peach, disabled text uses `--dnet-sys-state-disabled-fg`. The
  grid has no STY-002 golden coverage (virtual-scroll instability); the other
  components were verified pixel-identical.
- `AdminLayout` (dnet-admin-layout.css) migrated to the token architecture:
  the four `:root` blocks were removed (156 violations cleared) and the
  `--dnet-dash-*`/`--dnet-*` tokens became read-chains with their defaults in
  the fallback, keeping the old names as legacy links. Dead tokens were
  dropped (30 declarations, e.g. `--dnet-dash-base-url`, `--dnet-dash-white`,
  `--dnet-secundary-color`) and names with uppercase letters
  (`--dnet-dash-asideWidth`, `--dnet-dash-asideWidth-minified`,
  `--dnet-dash-font-family-Headline`) were renamed to kebab-case with the old
  spelling kept as a legacy link. The five unprefixed ghost tokens
  (`--content-color`, `--header-color`, `--footer-color`,
  `--left-column-color`, `--right-column-color`) were replaced by
  `--dnet-sys-surface` (they resolved to transparent over the white body, so
  the look is unchanged) and the broken fallbacks
  `var(--dnet-*-border-color, --general-border-color)` (missing `var()`, so
  the header/footer/column borders were not painted) now resolve to
  `--dnet-sys-border-strong` (#e1e3e1, exact). Colors map to system roles:
  #5f6368 -> `--dnet-sys-on-surface` (exact), #42b0d5 ->
  `--dnet-sys-primary-strong` (exact), #000 -> `--dnet-sys-on-surface-emphasis`,
  #eee -> `--dnet-sys-border`, #717780/#62727b ->
  `--dnet-sys-on-surface-muted`. The mobile tap-highlight was restored via
  `color-mix` (the Sass `fade()` was invalid CSS and had no effect). The six
  inline `style="background-color: transparent; border-color: white"` in
  `Menu/MenuTreeLink.razor` moved to the `.dnet-menu-img` rule (default
  aspect must not be inline). The shell was confirmed visually by the user.
  The STY-002 admin-layout golden was captured from an older page state and
  is not regenerated in this commit.

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
