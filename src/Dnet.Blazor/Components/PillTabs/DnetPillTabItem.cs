namespace Dnet.Blazor.Components.PillTabs;

/// <summary>Describes a pill tab with a stable value, a label and an optional count.</summary>
/// <param name="Value">The unique value used for selection.</param>
/// <param name="Label">The text displayed in the tab.</param>
/// <param name="Count">The optional count; zero is displayed.</param>
/// <param name="Disabled">Whether the tab can be selected.</param>
public sealed record DnetPillTabItem<TValue>(TValue Value, string Label, int? Count = null, bool Disabled = false)
    where TValue : notnull;
