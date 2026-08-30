namespace Dnet.Blazor.BrowserTests.VisualBaseline;

public enum VisualState
{
    Default,
    Hover,
    Focus,
    Selected,
    Open,
}

public sealed record ComponentVariant(string Label, string Selector);

/// <summary>
/// Registry of visual-baseline scenarios for STY-002. One entry per component
/// family: the sample route where it is demoed, the element to wait for and
/// capture, and the states that are reachable on that page.
///
/// Keep goldens deterministic: state interactions only, no timed waits beyond
/// the harness settle; data-driven pages resolve <c>sample-data/person_500.json</c>
/// through the fixture route.
/// </summary>
public sealed record ComponentScenario(
    string Name,
    string Route,
    string RootSelector,
    VisualState[] States,
    string? OpenSelector = null,
    string? SelectedSelector = null,
    string? SelectedCaptureSelector = null,
    string? HoverSelector = null,
    string? FocusSelector = null,
    string? StateAssertion = null,
    bool SelectedPreconfigured = false,
    VisualState[]? PageCaptureStates = null,
    ComponentVariant[]? Variants = null,
    int MaxDiffPixels = 0,
    int ChannelThreshold = 8,
    bool SkipMobile = false,
    string? ReadyFunction = null)
{
    /// <summary>States captured as a full-page screenshot (open panels live in the overlay portal).</summary>
    public VisualState[] PageCaptureStates { get; } = PageCaptureStates ?? [];

    public ComponentVariant[] Variants { get; } = Variants ?? [];
}

public static class ComponentScenarios
{
    public static readonly IReadOnlyList<ComponentScenario> All =
    [
        // Migrated components — the reference implementations. Button states are
        // captured on /theming because its buttons have no InitialFocus.
        new(
            "button",
            "/theming",
            ".dnet-button",
            [VisualState.Default, VisualState.Hover, VisualState.Focus]),

        new(
            "chips",
            "/Chips",
            ".dnet-chip",
            [VisualState.Default, VisualState.Hover, VisualState.Selected],
            SelectedSelector: ".dnet-chip-size-lg[aria-pressed='true']",
            SelectedCaptureSelector: ".dnet-chip-size-lg[aria-pressed='true']",
            StateAssertion: "() => document.querySelector('.dnet-chip-size-lg[aria-pressed=\"true\"]') !== null",
            SelectedPreconfigured: true,
            Variants:
            [
                new("medium", ".dnet-chip-size-md"),
                new("small", ".dnet-chip-size-sm"),
                new("extra-small", ".dnet-chip-size-xs"),
                new("custom-color", ".dnet-chip[style*=\"background-color\"]"),
            ]),

        new(
            "pick-list",
            "/PickList",
            ".dnet-pick-list",
            [VisualState.Default, VisualState.Hover]),

        // Remaining families, in the order of the debt ledger (largest first).
        new(
            "admin-layout",
            "/Dialog",
            ".dnet-aside",
            [VisualState.Default],
            PageCaptureStates: [VisualState.Default],
            SkipMobile: true), // the sidebar collapses into a drawer on mobile; the desktop golden covers the shell.
                               // Captured on /Dialog because "/" redirects to /BlGrid in OnAfterRender.

        new(
            "dialog",
            "/Dialog",
            "button:has-text(\"Show Dialog\")",
            [VisualState.Default, VisualState.Open],
            OpenSelector: "button:has-text(\"Show Dialog\")",
            PageCaptureStates: [VisualState.Open]),

        new(
            "autocomplete",
            "/Autocomplete",
            "input.mat-input-element",
            [VisualState.Default, VisualState.Open],
            OpenSelector: "input.mat-input-element",
            PageCaptureStates: [VisualState.Open]),

        new(
            "select",
            "/Select",
            ".dnet-plain-select",
            [VisualState.Default, VisualState.Hover, VisualState.Open],
            OpenSelector: ".dnet-plain-select",
            PageCaptureStates: [VisualState.Open]),

        new(
            "checkbox",
            "/CheckBox",
            ".dnet-checkbox:not(.dnet-checkbox-disabled)",
            [VisualState.Default, VisualState.Hover, VisualState.Selected],
            SelectedSelector: ".dnet-checkbox.dnet-checkbox-checked",
            SelectedCaptureSelector: ".dnet-checkbox.dnet-checkbox-checked",
            StateAssertion: "() => document.querySelector('.dnet-checkbox.dnet-checkbox-checked') !== null",
            SelectedPreconfigured: true),

        new(
            "radio-button",
            "/RadioButton",
            ".dnet-radio-button:not(.dnet-radio-disabled):not(.dnet-radio-checked)",
            [VisualState.Default, VisualState.Hover, VisualState.Selected],
            SelectedSelector: ".dnet-radio-button.dnet-radio-checked",
            SelectedCaptureSelector: ".dnet-radio-button.dnet-radio-checked",
            StateAssertion: "() => document.querySelector('.dnet-radio-button.dnet-radio-checked') !== null",
            SelectedPreconfigured: true),

        new(
            "slide-toggle",
            "/SlideToggle",
            ".dnet-slide-toggle:not(.dnet-slide-toggle-disabled):not(.dnet-slide-toggle-checked)",
            [VisualState.Default, VisualState.Hover, VisualState.Focus, VisualState.Selected],
            SelectedSelector: ".dnet-slide-toggle.dnet-slide-toggle-checked",
            SelectedCaptureSelector: ".dnet-slide-toggle.dnet-slide-toggle-checked",
            FocusSelector: ".dnet-slide-toggle:not(.dnet-slide-toggle-disabled) .dnet-slide-toggle-input",
            StateAssertion: "() => document.querySelector('.dnet-slide-toggle.dnet-slide-toggle-checked') !== null",
            SelectedPreconfigured: true,
            Variants: [new("full-width", ".dnet-slide-toggle-full-width")]),

        new(
            "datepicker",
            "/DatePicker",
            "input.mat-input-element",
            [VisualState.Default]), // the open calendar shows intermittent sub-pixel variance; not stable for goldens

        new(
            "tabs",
            "/Tabs",
            ".mat-tab-group",
            [VisualState.Default]),

        new(
            "stepper",
            "/Stepper",
            ".dnet-stepper-horizontal",
            [VisualState.Default, VisualState.Hover]),

        new(
            "dynamic-stepper",
            "/DynamicStepper",
            ".dnet-app-dstepper-container",
            [VisualState.Default, VisualState.Hover]),

        new(
            "expansion-panel",
            "/Expansion",
            ".dnet-expansion-panel",
            [VisualState.Default, VisualState.Open],
            OpenSelector: ".dnet-expansion-panel-header"),

        new(
            "floating-panel",
            "/FloatingPanel",
            "button:has-text(\"Show Panel\")",
            [VisualState.Default, VisualState.Open],
            OpenSelector: "button:has-text(\"Show Panel\")",
            PageCaptureStates: [VisualState.Open]),

        new(
            "connected-panel",
            "/ConnectedPanel",
            ".bi-filter",
            [VisualState.Default, VisualState.Open],
            OpenSelector: ".bi-filter",
            PageCaptureStates: [VisualState.Open]),

        new(
            "floating-double-list",
            "/FloatingDoubleList",
            ".dnet-plain-select",
            [VisualState.Default]),

        new(
            "spinner",
            "/Spinner",
            "button:has-text(\"RunSpinner\")",
            [VisualState.Default],
            PageCaptureStates: [VisualState.Default]), // the open spinner is timer-driven; not deterministic

        new(
            "list",
            "/List",
            ".dnet-list-wrapper",
            [VisualState.Default, VisualState.Hover]),

        // The paginator only renders inside a list with pagination enabled;
        // the List sample page enables it. Hover targets the "next" button
        // (first/previous are disabled on page one).
        new(
            "paginator",
            "/List",
            ".dnet-paginator",
            [VisualState.Default, VisualState.Hover],
            HoverSelector: ".dnet-paginator-navigation-next"),

        new(
            "toast",
            "/Toast",
            "button:has-text(\"Show Toast\")",
            [VisualState.Default],
            PageCaptureStates: [VisualState.Default]), // the open toast is timer/position-driven (counter + stack offset); not deterministic

        new(
            "tooltip",
            "/Tooltip",
            "div[style*=\"4fc3f7\"]",
            [VisualState.Default, VisualState.Hover],
            HoverSelector: "div[style*=\"4fc3f7\"]",
            PageCaptureStates: [VisualState.Hover],
            SkipMobile: true), // the sample's absolutely-positioned targets overlap on narrow viewports

        new(
            "tree",
            "/Tree",
            "ul[role=tree]",
            [VisualState.Default, VisualState.Hover]),

        // Grid (BlGrid) is deliberately absent: its virtual-scroll rendering is
        // not deterministic enough for goldens yet (visible rows differ run to
        // run). GridPerformanceBaselineTests covers its functional stability;
        // revisit the golden once the rendering settles. See README.md.

        new(
            "forms",
            "/Forms",
            ".dnet-form-field-plain-wrapper",
            [VisualState.Default, VisualState.Focus],
            FocusSelector: ".dnet-form-field-plain-wrapper input"),
    ];
}
