namespace Dnet.Blazor.Components.DatePicker.Infrastructure.Models;

/// <summary>Describes a day cell rendered by <c>DnetCalendar</c>.</summary>
public sealed record DnetCalendarCellContext(
    DateOnly Date,
    bool IsDisabled,
    bool IsToday,
    bool IsSelected,
    bool IsRangeStart,
    bool IsRangeEnd,
    bool IsInRange,
    bool IsInPreview,
    bool IsInComparisonRange);
