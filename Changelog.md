# Changelog for Blazor Library

## Unreleased

- Grid: the header's sort, filter and group actions take a pill radius and
  the material's ghost glass, with its sheen, rim of light and focus halo.
  `--dnet-grid-header-button-padding`, `--dnet-grid-header-actions-gap` and
  `--dnet-grid-header-button-hover-background` set their padding, the space
  between them and their fill under the pointer. Without visual change by
  default.
- The Liquid Glass themes render the grid header's sort, filter and group
  actions as the paginator's round glass buttons: 28px, 6px apart, tinted
  in the accent under the pointer, with the focus halo.
- Grid: `GridOptions.EnableMaximize` puts a button in the grid's top-right
  corner, a quarter of a disc drawn from its top edge to its side in a soft
  tint of the accent, that makes the grid fill the window, on desktop and
  on phones, and brings it back. Escape also brings it back, unless a popup
  such as the advanced filter is open, and the popups still open above the
  maximized grid. The grid keeps a band as tall as the button above its
  header, so the button covers none of the header's actions.
  `BlgGrid.IsMaximized` says whether it is maximized. Tokens:
  `--dnet-grid-maximize-size`, `--dnet-grid-maximize-clearance`,
  `--dnet-grid-maximize-background`, `--dnet-grid-maximize-hover-background`,
  `--dnet-grid-maximize-color` and `--dnet-grid-maximized-z-index` (900,
  under the overlays). New strings: `Grid.Maximize` and `Grid.Restore`. Off
  by default. The maximized grid is `position: fixed`, so an ancestor with a
  `transform`, `filter`, `backdrop-filter` or `contain` confines it to that
  ancestor.
- The Liquid Glass themes fit the grid's maximize button in the corner
  their header row already leaves free, with no band above the header.
- FloatingPanel: `--dnet-floating-panel-background` sets the panel's fill.
  Without visual change by default.
- The Liquid Glass themes give the floating panel the dialog's glass: the
  translucent card material instead of the nearly opaque popup one.
- DatePickerWeekRaw: a theme can draw the chosen week as one band across
  its row, with round ends and no seams between days
  (`--dnet-calendar-week-band-color`), the week under the pointer the same
  way from any of its days (`--dnet-calendar-week-hover-band-color`), and
  then clear the days' own fill (`--dnet-calendar-week-day-background`).
  Today takes a mark drawn in its number's colour
  (`--dnet-calendar-today-mark`), a ring (`--dnet-calendar-today-ring`) and
  a colour (`--dnet-calendar-today-color`). `--dnet-calendar-nav-size` and
  `--dnet-calendar-nav-gap` size and space the month buttons, and
  `--dnet-calendar-inline-background` fills the calendar, as it does the
  date picker's calendar shown in the page. The calendar and its month
  buttons take the material's sheen, rim and focus halo. Without visual
  change by default.
- DatePickerWeek: the same week band, today and month button tokens as
  DatePickerWeekRaw, and `--dnet-datepickerweek-panel-background` for the
  popup's fill. Its month buttons take the material's sheen, rim and focus
  halo, and its calendar the date picker popup's inner padding
  (`--dnet-datepicker-panel-padding`). Without visual change by default,
  except its Reset: it is now the date picker's, a small outlined button
  that the keyboard reaches, in the date picker popup's footer, instead of
  a text link that only took clicks.
- DatePicker: the calendar shown in the page takes the material's sheen,
  rim and blur, with `--dnet-calendar-inline-background` for its own fill;
  in the popup it draws neither. Its month buttons are round buttons in
  the ghost material, sized and spaced by `--dnet-calendar-nav-size` and
  `--dnet-calendar-nav-gap`, and the chosen day takes the accent
  material's sheen and glow. Today's ring and colour follow
  `--dnet-calendar-today-ring` and `--dnet-calendar-today-color`. Without
  visual change by default.
- The Liquid Glass themes give the calendars glass: those shown in the
  page, of days and of weeks, are a pane of the controls' glass, and the
  week picker's popup is the date picker's glass and spacing. Their month
  buttons are round ghost glass buttons, 32px across and 8px apart. The
  chosen day is an accent circle and the chosen week one accent pill
  across its row, both with the accent's sheen and glow; the week under
  the pointer is a faint tint of the accent, and today a soft circle
  without a ring, in the accent outside the chosen week and white inside.
- Publishes the 6.1.0 changes to NuGet. 6.1.0 was released on GitHub only:
  its package was not published because the Linux visual baselines had not
  been updated for that version's intended changes. No library changes.
- The Linux visual goldens are updated for those changes. The *Build and
  test* workflow can regenerate them on its Linux runner with the
  `update_goldens` option, which uploads them for review without committing.

## Version 6.1.0 (November 2026)

- Forms: messages and accessible names use the name the model gives a field
  with `[Display(Name = ...)]` or `[DisplayName(...)]`, as the framework's
  inputs do since .NET 11, instead of the property name. The parsing errors
  of InputNumber, InputDate and Select and the radio group's invalid-value
  message say "El campo Edad…" rather than "El campo Age…", and Select,
  Autocomplete and FloatingDoubleList take that name as their `aria-label`
  when they have none, before falling back to their generic one. The
  `DisplayName` parameter, and `ResponsiveLabel` on Select, still win. A
  name from resources (`ResourceType`) is read in the library's language,
  without changing the thread's culture, and cached per member and
  language. `DnetInputBase` gains `GetModelDisplayName` and
  `GetFieldDisplayName` for derived inputs.
- Forms: a field shows when an asynchronous validator is checking it. While
  the check runs, a small spinner in the accent colour sits beside the
  field's actions, the hint says "Comprobando…" ("Checking…") and the input
  carries `aria-busy`. When the check itself fails (it throws), the hint says
  in amber, with a warning icon, that it could not be checked, and the field
  is not marked as wrong. Both messages are announced (`role="status"`) and
  follow the language. The inputs report the state through `DnetInputBase`
  and `IFormEventService.RaiseValidationState` with the new
  `FieldValidationState`; the new interface members have defaults that do
  nothing, so existing implementations keep compiling.
  `--dnet-form-field-pending-color` and
  `--dnet-form-field-faulted-color` set the colours. New strings:
  `Form.ValidationChecking` and `Form.ValidationFaulted`. Fields without an
  asynchronous validator do not change.
- Forms: a field shows the error that an asynchronous validator reports
  after the change. Validators registered with .NET 11's
  `EditContext.RegisterAsyncFieldValidator` deliver their messages through
  `OnValidationStateChanged`, and the fields only looked at errors when the
  value changed, so the form field stayed without its error until the next
  edit. The text, text area, number and date inputs, Select, Autocomplete,
  FloatingDoubleList, DatePicker, DateRangePicker and TimePicker now update
  it when the validation state changes. `DnetInputBase` has a new protected
  `OnFieldValidationStateChanged` hook for inputs that show their errors.
- Virtualize: `Dnet.Blazor.Components.Virtualize.Virtualize<TItem>` and its
  `ItemsProviderDelegate`, `ItemsProviderRequest`, `ItemsProviderResult` and
  `PlaceholderContext` are obsolete. They are a 2021 copy of the framework's
  component that the library does not use, and it still calls the
  framework's internal script with the signature of that time. Use
  `Microsoft.AspNetCore.Components.Web.Virtualization`, as List, Select,
  Autocomplete, Chips and FloatingDoubleList do. They will be removed in the
  next major version. The Grid's own virtualization is not affected.
- Select: the panel opens scrolled to the selection, the selected option or
  with `MultiSelect` the first selected one, instead of always at the top,
  so in a long list it is in view. It waits until the overlay has sized the
  list and then uses the framework's `Virtualize.ScrollToItemAsync`.
  `DnetSelectPanel` takes the new `InitialItemIndex` parameter. Without a
  selection, or with the first option selected, it opens at the top as
  before.
- Tooltip: takes the material's blur, `--dnet-sys-material-backdrop`, so a
  glass theme frosts what lies behind it. Without visual change by default.
- PickList: the paginator's arrows take the paginator's own style, as in
  List and Grid. A PickList rule left over from its earlier plain buttons
  made them transparent, with muted icons, 25×32 and rounded like the rows.
  Visible in every theme: the arrows are 25px circles in the text colour and
  the footer is 7px shorter, so the `pick-list-*` goldens change and the
  Linux ones need regenerating on CI. In the Liquid Glass themes they are
  the same white glass buttons as the other paginators', and fade like a
  disabled white button. `--dnet-picklist-primary`, which only coloured the
  arrows' focus outline, is removed: they take the theme's focus ring.
- PickList: its paginator places the range by the list's
  `--dnet-list-paginator-range-basis`, `-range-order` and
  `-range-compact-display`, and new `--dnet-picklist-footer-overflow` lets
  a theme stop the footer clipping the shadow and focus halo of its
  buttons. The paginator now takes the footer's whole width, with nothing
  visible changing. Without visual change by default.
- List: new `--dnet-list-footer-height` sets the footer's height apart from
  the header's. `--dnet-list-paginator-range-basis` and
  `--dnet-list-paginator-range-order` let a theme bring the range from its
  own line under the controls to the start of their line, where it stays
  while there is room and wraps where the footer is too narrow, and
  `--dnet-list-paginator-range-compact-display` hides the range on phones.
  The paginator now takes the footer's whole width, with nothing visible
  changing. Without visual change by default.
- Paginator: new `--dnet-paginator-padding` sets the space around it and
  `--dnet-paginator-input-margin` the page field's margin. Without visual
  change by default.
- Paginator: new `--dnet-paginator-controls-gap` sets the space between the
  arrows. Without visual change by default.
- Paginator: new tokens let a theme restyle it and lay it out on one line.
  `--dnet-paginator-button-hover-background` tints the arrows under the
  pointer, which also take the material's ghost glass; the page field gets
  `--dnet-paginator-input-background`, `-input-border-color`,
  `-input-radius`, `-input-width` and `-input-height` and the material's well
  shadow. `--dnet-paginator-rows`, `-columns` and `-width` set the layout,
  and `--dnet-paginator-range-area`, `-range-justify` and
  `-range-font-size` place the range; on a phone it goes back under the
  controls. Without visual change by default.
- TimePicker: new `Mode` parameter. `DnetTimePickerMode.Dial` chooses the
  time on a clock face in a centred modal dialog, with Cancel and OK: first
  the hour, then the minutes. It is 24-hour by default, with 1 to 12 on the
  outer ring and 13 to 23 and 00 on the inner one; with a 12-hour `Format`
  (`h`) it shows one ring and an AM/PM switch. The minutes show 00 to 55;
  dots between the marks open an arc 4px outside the dial with the four
  minutes of their gap. `Min` and `Max` disable what falls outside them;
  `Interval` and `TimeOptions` do not apply. The dial is a slider for the
  keyboard: arrows move by one, PageUp and PageDown by five, Enter goes from
  the hours to the minutes. It takes its look from the theme's materials, so
  a glass theme makes it glass with no tokens of its own. `List` stays the
  default, unchanged. New strings: `Common.Ok`, `TimePicker.Hours` and
  `TimePicker.Minutes`.
- Overlay: the TimePicker's list is centred on tablet and phone widths, as
  the DatePicker's calendar is, instead of sitting in the top-left corner.
  Visible in every theme from 601px to 1024px; at 600px and below the
  TimePicker opens its touch dialog, which was already centred.
- Grid: the column filter panel is centred on tablet and phone widths
  instead of sitting in the top-left corner, visible in every theme there.
  New `--dnet-grid-filter-scrim` and `--dnet-grid-filter-scrim-backdrop`
  let a theme dim and blur the page behind the panel on desktop, where its
  backdrop is transparent. Without visual change on desktop by default.
- Overlay: on tablet and phone widths, an overlay without a component type
  of its own that brings a global position strategy is placed by it.
  Before, it fell to the top-left corner of the screen.
- Grid: new `--dnet-grid-filter-background`, `--dnet-grid-filter-padding`
  and `--dnet-grid-filter-gap` lay out the column filter panel, and
  `--dnet-grid-filter-button-height` and `--dnet-grid-filter-button-padding`
  size its buttons; unset, they keep their small size and whatever
  `--dnet-btn-*` values they inherit. Without visual change by default.
- PillTabs: a tab under the pointer takes the material's rim, the selected
  tab the accent's ring and glow like a filled button, and a tab with
  keyboard focus the material's focus halo, since the global `:focus` reset
  hides its outline. Themes that leave those materials unset see no change.
- Tabs: the labels sit in the middle of their row, level with the scroll
  arrows. An inline `display:block` on each tab overrode the stylesheet's
  centring and kept them at the top of the row. Visible in every theme: the
  `tabs-default` goldens change, and the Linux ones need regenerating on CI.
- Tabs: new `--dnet-tabs-radius`, `--dnet-tabs-shadow` and
  `--dnet-tabs-gap` shape the group and space the tab row from the view, and
  `--dnet-tabs-background` now takes layered backgrounds.
  `--dnet-tabs-active-foreground` and `--dnet-tabs-active-opacity` set the
  active label. `--dnet-tabs-label-hover-background` washes a lens behind
  the label under the pointer, which rises and carries the focus halo with
  the material's tokens, and `--dnet-tabs-pagination-size` sets each arrow
  in a round ghost glass button. Without visual change by default.
- ExpansionPanel: the header reports `aria-expanded="true"` or `"false"`;
  it rendered an empty attribute when open and none when closed. The
  toggle's rotation moved from an inline style to the stylesheet.
- ExpansionPanel: new tokens shape the panels and their toggle.
  `--dnet-accordion-gap` and `--dnet-accordion-joined-radius` set joined
  panels apart and round their inner corners, and
  `--dnet-accordion-box-shadow-expanded` lifts the open one.
  `-header-padding`, `-header-height` and `-header-hover-background` size
  the row, and `-title-font-weight` and `-description-font-size` its text.
  `-indicator-padding`, `-indicator-radius`, `-indicator-background`,
  `-indicator-shadow` and `-indicator-color`, with `-expanded` variants of
  the last three, set the chevron in a button.
  `-indicator-rotate-expanded` and `-chevron-rotate-expanded` choose whether
  the button or only the chevron turns over, `-indicator-transition`
  animates it and `-chevron-offset` centres the chevron.
  `-indicator-focus-outline` rings the button while the header has keyboard
  focus, `-indicator-disabled-opacity` fades it on a disabled panel, and
  `-body-padding` spaces the content. Without visual change by default.
- FloatingDoubleList: its lists keep the list's own padding and gap, so the
  standalone list's panel tokens do not reach them. Without visual change.
- List: new `--dnet-list-panel-background`, `-border`, `-shadow`,
  `-padding` and `-gap` style a standalone list's own box, apart from the
  `--dnet-list-*` family the dropdowns share; the shared tokens still win
  when set. Without visual change by default.
- PickList: the selection counter keeps its line. When the actions do not
  fit beside it they move under it, instead of squeezing the counter into a
  column one word per line. Visible in every theme where the box is narrow.
- PickList: new `--dnet-picklist-padding` and `--dnet-picklist-gap` set the
  space inside the box and between its sections (20px each by default), and
  `--dnet-picklist-shadow` its shadow. Without visual change by default.
- FloatingDoubleList: new `--dnet-floating-double-list-panel-background`,
  `-header-padding` (the row over the lists, which no longer sets its
  padding inline) and `-lists-padding` lay out the panel, and
  `-close-padding`, `-close-radius`, `-close-background`,
  `-close-background-hover` and `-close-shadow` paint its close button as a
  round button with the row's height unchanged. Without visual change by
  default.
- TimePicker: new `--dnet-timepicker-panel-options-padding`,
  `--dnet-timepicker-panel-touch-options-padding`,
  `--dnet-timepicker-panel-header-padding` and
  `--dnet-timepicker-panel-actions-padding` space the panel, and
  `--dnet-timepicker-button-height`, `--dnet-timepicker-button-padding` and
  `--dnet-timepicker-button-icon-gap` size the touch panel's button; unset,
  it keeps its small size and whatever `--dnet-btn-*` values it inherits.
  The touch header no longer shrinks when the list is long. Without visual
  change by default.
- DatePicker: new `--dnet-datepicker-panel-padding`,
  `--dnet-datepicker-panel-footer-padding` and
  `--dnet-datepicker-panel-footer-gap` space the popup, and
  `--dnet-datepicker-button-height`, `--dnet-datepicker-button-padding` and
  `--dnet-datepicker-button-icon-gap` size its buttons (Reset, Cancel,
  Apply); unset, the buttons keep their small size and whatever
  `--dnet-btn-*` values they inherit. Without visual change by default.
- Button: new `--dnet-btn-icon-gap` sets the space between an icon and the
  label (4px small, 8px medium and large by default). Without visual change
  by default.
- TimePicker: new `--dnet-timepicker-panel-header-background` sets the fill
  of the touch panel's header, the panel's own fill by default. Without
  visual change by default.
- DatePicker: new `--dnet-datepicker-panel-background` sets the popup's fill
  apart from calendars shown in the page. Without visual change by default.
- Autocomplete: new `--dnet-autocomplete-panel-*` tokens lay out the
  dropdown like the Select's: `-background`, `-header-padding`,
  `-header-height`, `-header-gap`, `-list-padding`, `-padding-bottom`, and
  the close button's `-close-padding`, `-close-radius`, `-close-background`,
  `-close-background-hover` and `-close-shadow`. Without visual change by
  default.
- Select: new `--dnet-select-panel-*` tokens lay out the dropdown apart
  from the `--dnet-list-*` family that inline lists share: `-background`,
  `-header-padding`, `-header-height`, `-header-gap` (space under the
  title), `-list-padding`, `-padding-bottom`, and the close button's
  `-close-padding`, `-close-radius`, `-close-background`,
  `-close-background-hover` and `-close-shadow`, which paint it as a round
  button with the header's height unchanged. Without visual change by
  default.
- ImageEditor: new `--dnet-image-editor-footer-border-width` and
  `--dnet-image-editor-footer-padding-top` set the line over the footer's
  buttons and the space under it, and `--dnet-image-editor-zoom-padding`
  and `--dnet-image-editor-zoom-radius` the zoom group's inset and corners.
  Without visual change by default.
- Dialog: new `--dnet-dialog-close-padding`, `-close-radius`,
  `-close-background`, `-close-background-hover` and `-close-shadow` paint
  the close button as a round button. A negative margin cancels the
  padding, so the header keeps its height. New `--dnet-dialog-padding-bottom`
  sets the bottom padding apart from the top, and
  `--dnet-dialog-content-padding-bottom` adds space at the end of the
  scrolling content, where a button's glow is not cut. Without visual change
  by default.
- Autocomplete: the options show the pointer cursor, like other clickable
  items.
- Toast: new `--dnet-toast-action-foreground` and
  `--dnet-toast-action-border-color` colour the actions' label and outline
  (the toast's text colour by default), and `--dnet-toast-progress-inset`
  moves the progress bar in from both sides. Without visual change by
  default.
- AdminLayout: the expanded menu shows each item's `MinifiedText` as a tile
  before its label (`.dnet-menu-tile`, hidden by default), and new tokens
  shape the menu. Rows: `--dnet-dash-menu-inset`, `-item-gap`,
  `-item-radius`, `-item-padding-end`, `-item-weight`,
  `-item-weight-active`, `-item-background-hover`,
  `-item-background-active`, `-item-shadow-active`, `-text-color-hover`,
  `-text-color-active` and `-text-gap`. Tiles, which also paint the icon
  box: `--dnet-dash-menu-tile-display`, `-tile-size`, `-tile-radius`,
  `-tile-background`, `-tile-shadow`, `-tile-background-active`,
  `-tile-color-active`, `-tile-shadow-active` and `-img-offset`. Group
  chevron: `--dnet-dash-menu-chevron-size`, `-chevron-offset`,
  `-chevron-rotate` and `-chevron-rotate-open`. Keyboard focus:
  `--dnet-dash-menu-focus-ring` and `-focus-offset`, drawn as a ring
  because the global focus reset removes outlines. Open group:
  `--dnet-dash-submenu-width`, `-indent`, `-rail`, `-padding`,
  `-background`, `-shadow` and `-text-size`. Without visual change by
  default.
- DynamicStepper reads the same `--dnet-step-*` tokens as Stepper. Without
  visual change by default.
- Stepper: new `--dnet-step-*` tokens paint the icons (`-icon-background`,
  `-icon-color`, `-icon-shadow`, `-icon-font-size`, `-icon-font-weight`,
  `-icon-background-done` painted over completed steps,
  `-icon-background-current`, `-icon-shadow-current`, `-icon-scale-current`)
  and the labels (`-label-weight`, `-label-weight-current`,
  `-label-color-upcoming`), and space a horizontal stepper: the icons
  (`-icon-margin`), the labels' minimum width (`-label-min-width`) and the
  extra space between a label and the line after it (`-line-gap`).
  Completed steps are the ones before the current one. Without visual
  change by default.
- Timeline: new tokens turn each event into a card and the markers into
  glass. Cards: `--dnet-timeline-content-padding`, `-content-radius`,
  `-content-border-width`, `-content-background`,
  `-content-background-current` and `-content-shadow-current`, over the
  material sheen, highlight and lift; pending events are an outline in the
  line colour. Markers: `--dnet-timeline-bullet-offset` (lines them up with
  the card's title), `-bullet-border-width`, `-bullet-shadow`,
  `-bullet-background-completed`, `-bullet-shadow-completed`,
  `-bullet-background-current`, `-bullet-shadow-current` and
  `-current-foreground`. Line: `--dnet-timeline-line-inset` (its gap to the
  markers; negative values run it under them) and `-line-completed`.
  Without visual change by default.
- New material mechanic `--dnet-sys-material-well-shadow`, the inner shadow
  and rim of recessed tracks (slide toggle). Without effect by default.
- SlideToggle: new `Variant` parameter. `DnetSlideToggleVariant.Lens` is a
  tinted track with a clear lens handle, painted by its
  `--dnet-slide-toggle-lens-*` tokens (track height, handle size, track
  background and shadow in both states, handle background, shadow and
  backdrop filter). Each falls back to the standard value, so where no
  theme sets them it looks like `Standard`.
- SlideToggle paint tokens `--dnet-slide-toggle-track-border-width`,
  `-track-shadow-checked`, `-handle-shadow`, `-icon-color`, `-halo-size`
  (the hover and focus halo around the handle), `-focus-outline` (a keyboard
  focus outline around the whole control) and `-disabled-opacity`, with
  today's values as defaults; the track reads the well shadow material. The
  control is as tall as the taller of track and handle, and the track is
  centred in it, so a handle larger than the track overhangs it. Without
  visual change by default.
- New material mechanic `--dnet-sys-material-lift`, the drop shadow of small
  glass faces (checkbox, radio button). Without effect by default.
- Checkbox, RadioButton and Chips show a focus ring for keyboard focus in
  every theme: a 2px gap in `--dnet-sys-surface` and 2px in
  `--dnet-sys-focus-ring`. A chip's remove button shows a 2px ring in the
  chip's text colour. Visual change: before, keyboard focus was not visible.
  Pointer use looks the same.
- Checkbox and RadioButton faces paint the ghost material (`ghost`, `sheen`,
  `highlight`, `lift`). Without effect by default. The checkmark draws itself
  when the user allows motion.
- New RadioButton tokens `--dnet-radio-button-border-color`,
  `--dnet-radio-button-checked-border-color` and
  `--dnet-radio-button-disabled-border-color`, with the previous colours as
  defaults.
- Grid rows can be separate cards: `--dnet-grid-row-gap`,
  `--dnet-grid-row-radius` and `--dnet-grid-row-border` set the space between
  rows, the rounding of their ends and their outline. `--dnet-grid-radius` and
  `--dnet-grid-padding` shape the grid itself, which paints the card material.
  The first and last cell of each row carry `blg-cell-row-start` and
  `blg-cell-row-end`. `--dnet-grid-row-highlight` adds the top and bottom
  lines of a glass rim, except on cells whose `CellStyleFn` paints their
  background. Without effect by default.
- Grid selection checkboxes can take the Checkbox glass face:
  `--dnet-grid-checkbox-size`, `-radius`, `-border-color`, `-background`,
  `-background-checked`, `-mark-color`, `-mark-size` and `-mark-image`, plus
  the ghost material's sheen, rim and lift. Without effect by default. Keyboard focus shows the
  Checkbox ring in every theme (visual change: before, the global focus
  reset hid it).
- Grid: horizontal wheel and trackpad gestures, and Shift + wheel, over the
  rows scroll the columns. Before, only the grid's scrollbar (hidden until
  hovered on macOS) or a touch drag did.
- Grid: its horizontal scrollbar stays visible on desktop. An inherited
  `scrollbar-color` or `scrollbar-width` (an app's body style, for example)
  no longer switches it to the native overlay scrollbar that macOS hides
  until the columns already move. Browsers without `::-webkit-scrollbar`
  get a thin scrollbar in the grid's colours.
- New Grid token `--dnet-grid-grid-foreground-color-hover` for the scrollbar
  thumb on hover; the grid now owns that hover, so a page-wide scrollbar style
  no longer repaints it. Defaults to the usual darker grey.
- Grid: horizontally scrolled columns stop at the pinned zones instead of
  running under them, so translucent pinned cells (Liquid Glass) no longer
  show the text of the columns beneath.
- Grid: selecting all, from the header or with `SelectAll()`, leaves out the
  rows `DisableRow` disables. Before, the header did so only in the tree while
  the rendered body and `SelectAll()` selected them too. A disabled row's
  selection and grouping cells never take the selected or clicked colour,
  like its other cells. Behaviour change for `SelectAll()` callers that also
  disable rows: those rows are no longer in `GetSelectedNodes()`.
- Chips read the material mechanics like buttons: the ghost sheen, rim and
  elevation on the face, and the accent sheen and rim once selected. Without
  effect by default.
- The Liquid Glass themes render checkboxes and radio buttons as pure glass
  faces, without a border, with an accent mark and dot. `npm run check:contrast`
  gates the mark and the dot at WCAG 1.4.11 (3:1). An empty face stays below
  3:1 against its card; the gate reports that value without gating it.
- The Liquid Glass themes show the Grid as a glass card whose rows are cards
  of their own: control material, an 8px gap, rounded ends and the glass
  rim of buttons and containers instead of a grey outline. The header sits on the glass without fills or separators, and
  selected and clicked rows take an accent tint.
- The Liquid Glass themes draw the Grid's selection checkboxes like Checkbox:
  a 20px glass face with 6px corners and no border, and Checkbox's tick in
  the accent, without a fill when checked.
- The Liquid Glass themes outline accent glass (filled buttons, selected
  chips) with a 1px border of the accent slightly lighter than the fill,
  instead of a white line.
- The Liquid Glass themes render chips as glass like the buttons, keeping
  their own corners: unselected chips are ghost glass, and a selected chip
  takes the accent fill with its sheen and glow.
- The Liquid Glass themes paint the steps of Stepper and DynamicStepper as
  glass beads (from option C of the reference design), keeping their own
  layout: the label beside the bead and thin lines between steps that stop
  short of the beads, with the same space at both ends of every line. The
  current step is a larger accent drop with a glow;
  completed steps show an accent tick instead of their number; upcoming
  labels are muted and the current one is bolder.
- The Liquid Glass themes render the other dropdown panels as glass too:
  the Autocomplete's like the Select's (spaced like a dialog, with the
  round close button), and the popups of DatePicker, TimePicker and
  ConnectedPanel. Calendars shown in the page keep their own fill, and the
  time picker's touch header no longer adds a second layer. The date
  picker's popup is spaced like a dialog (its controls and buttons 24px
  from the edges, 24px between the days and the buttons, 8px between
  buttons) and its buttons take the theme's normal size. So is the time
  picker's panel: the options' text 24px from the edges with 24px above
  and below the list, and in the touch panel the title 24px in with 16px
  under it and the button 24px from the corner at the normal size.
- The Liquid Glass themes space the grid like a dialog: its header text,
  rows and paginator buttons sit 24px from the card's edges, 16px from the
  sides on phones so the paginator keeps its line.
- The Liquid Glass dark theme draws the spinner of a field that is being
  checked in its light accent, which reads on dark glass.
- The Liquid Glass themes render the tooltip in the menus' popup glass:
  frosted white with the sheen, the rim of light and the panels' soft
  shadow, and dark text, its arrow in the same fill. In glass dark it is
  the menus' dark glass with light text.
- The Liquid Glass themes space the pick list like a dialog throughout:
  its buttons 16px from the search and from the rows, and its paginator
  laid out as the list's, 24px above the bottom of the card, with the
  shadow and focus halo of its arrows no longer cut off at the footer's
  edges.
- The Liquid Glass themes lay the list's paginator out on one line on
  desktop, the range at the left and the arrows at the right, and leave the
  range out on phones. Where the card is too narrow for one line, the
  arrows wrap under the page instead of running into the card's padding.
  The footer is only as tall as the paginator, so the arrows sit 24px above
  the bottom of the card, as the header sits below its top.
- The Liquid Glass themes render the paginator with the theme's round glass
  buttons for the arrows, 6px apart, the page in a rounded glass well, and
  the range on the same line at the far left of the footer; on phones the
  range stays under the controls.
- The Liquid Glass themes dim and blur the page behind the grid's column
  filter on desktop too, as on narrower screens, so the panel reads as
  glass at every width.
- The Liquid Glass themes render the grid's column filter panel as glass
  like the other panels, spaced like a dialog: 24px around its content and
  16px between its fields, with its buttons at the theme's normal size.
- The Liquid Glass themes render PillTabs with the selected tab glossy
  accent like the filled buttons, a tinted ghost glass lens under the
  pointer, the reference design's chips as counters and 40px tabs like the
  theme's buttons; with keyboard focus a tab takes the focus halo. The
  sample keeps 16px between the bar and its view.
- The Liquid Glass themes render Tabs as a glass panel like the cards,
  with the rim, 24px around its content and 16px between the tab row and
  the view. The active label reads in the accent at full strength over its
  line, a raised glass lens shows under the pointer and the focus halo with
  keyboard focus, and the arrows are round glass buttons.
- The Liquid Glass themes render the ExpansionPanel as separate glass
  cards 8px apart and rounded on every corner, with 64px rows, a bolder
  title over a 12px description and a sheen under the pointer. The chevron
  sits in a round glass button that stays still while the chevron turns; on
  an open panel the button is the accent drop with a white chevron, and the
  card lifts with 24px under its content. With keyboard focus the button
  takes the focus ring, and on a disabled panel it fades.
- The Liquid Glass themes render a standalone List as a glass panel like
  the PickList: the rim instead of a border, 24px around its content and
  16px between its sections.
- The Liquid Glass themes render the PickList as a glass panel like the
  cards, with the rim instead of a border, 24px around its content and 16px
  between its sections.
- The Liquid Glass themes render the FloatingDoubleList panel as glass and
  space it like a dialog: the round close button 18px from the corner and
  the lists' content 24px from the panel's edges and from the close button.
- The Liquid Glass themes render the Select dropdown as glass like dialogs
  and space it like them: 24px around, the options' text in line with the
  title and 16px under it, and the theme's round close button 18px from the
  corner.
- The Liquid Glass themes render dialogs as liquid glass like the cards,
  spaced as Material 3's basic dialog: 24px padding on every side and 16px
  between title and body, with the bottom 24px inside the scrolling content
  so buttons at the end keep their rim and glow. Their close button is the
  theme's round icon button (ghost glass with the rim and a soft lift, 36px
  like the image editor's tools). The image editor drops the line over its
  buttons and leaves 24px between its content and them, and its zoom group
  keeps 4px all around its round buttons inside pill corners.
- The Liquid Glass themes render the toast as liquid glass like the cards,
  so the page shows through it blurred. Its actions are the theme's
  outlined buttons (ghost glass pills with the accent label), and the
  progress bar stops 5px short of where the corners start to curve.
- The Liquid Glass themes draw the inside of the AdminLayout menu as in
  their reference design: rows inset from the column, 44px tall, each led
  by a glass tile with the item's short text or its icon. Hovering a row
  lights it; the current page is an accent-tinted glass row with a filled
  accent tile and a bolder label. An open group hangs its rows on a hairline
  rail, its chevron points down and turns up while open, and keyboard focus
  draws an accent ring around the row.
- The Liquid Glass themes draw the timeline as glass cards (option A of the
  reference design): completed and current events are glass cards, the
  current one tinted with the accent, a ring and a glow, and pending ones
  a dashed outline. The markers are glass dots on a line that runs under
  them and fills with the accent after completed events: a bead with the
  accent tick when completed, an accent drop with a white core when
  current.
- The Liquid Glass themes paint the slide toggle as in their reference
  design. Standard (option A): a recessed glass well with a faint rim and a
  white glass bead inside it; checked, the track fills with the accent,
  lightened on the dark glass, under a sheen and a soft glow. Lens
  (option C): the glass of a checkbox face when off, translucent accent
  glass when checked, and a clear 32px lens 4px taller than the track.
  Neither shows icons or a hover halo; keyboard focus outlines the whole
  control, and disabled is at 45% opacity. Without `backdrop-filter`, or
  with reduced transparency or forced colours, the lens turns opaque. The
  sample's SlideToggle page shows the Lens variant.

- New material tokens in the system layer: `--dnet-sys-canvas`,
  `--dnet-sys-material-{page,chrome,card,popup,control,ghost}`, the mechanics
  `--dnet-sys-material-{backdrop,highlight,sheen}` and
  `--dnet-sys-scrim-backdrop`. `--dnet-sys-surface*` stay opaque colours; the
  materials describe how containers are painted. Their defaults reproduce the
  previous rendering, so existing themes do not change.
- `DesktopLayout`, popups (Select, Autocomplete, connected and floating panels,
  floating double lists, date, week and time pickers, dialogs, toasts and Grid
  advanced filters), the modal backdrop, ExpansionPanel, PillTabs, Tabs,
  Stepper, fill form fields and filled and outlined buttons consume them.
- New `liquid-glass` and `liquid-glass-dark` themes: translucent, blurred
  containers over a gradient canvas, with opaque fallbacks for reduced
  transparency, forced colors and browsers without `backdrop-filter`.
  `npm run check:contrast` verifies their worst-case WCAG AA contrast in CI.
- Form field corners read `--dnet-form-field-radius`, falling back to
  `--dnet-sys-radius-md` (5px by default). Visual change: fields in the
  `modern` theme, which sets that radius to 10px, now have 10px corners.
- The left `DesktopLayout` column carries `z-index: 200` so its menu flyouts
  stay above content that creates its own stacking context.
- The sample registers both themes in the header and the theme studio.
- Four more material mechanics, without effect by default:
  `--dnet-sys-material-accent-sheen` and `--dnet-sys-material-accent-highlight`
  (filled buttons), `--dnet-sys-material-ghost-elevation` (outlined buttons) and
  `--dnet-sys-material-focus` (focus ring of fill form fields).
- New public tokens: `--dnet-form-field-clear-icon` (mask image of the clear
  action), `--dnet-calendar-icon-color` and `--dnet-timepicker-icon-color`.
- Fill form fields draw a transparent border over their fill instead of a
  border in the fill colour. Opaque fills render the same; translucent fills
  no longer show a doubled edge.
- The Liquid Glass themes follow their reference design more closely: glass
  fills and rim, accent buttons with their own sheen and glow, outlined buttons
  as ghost pills, a chip-style clear action, accent field icons, quieter hints
  and a focus ring on fields. The dark variant keeps the same accent with
  white text.

## Version 6.0.20 (November 2026)

- Updated to .NET 11 final with SDK `11.0.100` and stable ASP.NET Core and
  Microsoft.Extensions packages at `11.0.0`.
- Updated the Alpine SDK and runtime images used to build and host the samples.

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
