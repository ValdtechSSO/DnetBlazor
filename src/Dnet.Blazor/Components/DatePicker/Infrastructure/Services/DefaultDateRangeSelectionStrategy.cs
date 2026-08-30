using Dnet.Blazor.Components.DatePicker.Infrastructure.Interfaces;
using Dnet.Blazor.Components.DatePicker.Infrastructure.Models;

namespace Dnet.Blazor.Components.DatePicker.Infrastructure.Services;

/// <summary>Selects the start first and completes the range with the next date.</summary>
public sealed class DefaultDateRangeSelectionStrategy : IDnetDateRangeSelectionStrategy
{
    public DnetDateRange SelectionFinished(DateOnly? date, DnetDateRange currentRange)
    {
        if (!date.HasValue)
        {
            return new DnetDateRange();
        }

        if (!currentRange.Start.HasValue || currentRange.End.HasValue)
        {
            return new DnetDateRange(date, null);
        }

        return date < currentRange.Start
            ? new DnetDateRange(date, currentRange.Start)
            : new DnetDateRange(currentRange.Start, date);
    }

    public DnetDateRange CreatePreview(DateOnly? activeDate, DnetDateRange currentRange)
    {
        if (!activeDate.HasValue || !currentRange.Start.HasValue || currentRange.End.HasValue)
        {
            return new DnetDateRange();
        }

        return activeDate < currentRange.Start
            ? new DnetDateRange(activeDate, currentRange.Start)
            : new DnetDateRange(currentRange.Start, activeDate);
    }
}
