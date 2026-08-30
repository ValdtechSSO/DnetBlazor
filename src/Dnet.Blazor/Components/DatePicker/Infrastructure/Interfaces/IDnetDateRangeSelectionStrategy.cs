using Dnet.Blazor.Components.DatePicker.Infrastructure.Models;

namespace Dnet.Blazor.Components.DatePicker.Infrastructure.Interfaces;

/// <summary>Controls how date clicks and pointer previews construct a range.</summary>
public interface IDnetDateRangeSelectionStrategy
{
    DnetDateRange SelectionFinished(DateOnly? date, DnetDateRange currentRange);

    DnetDateRange CreatePreview(DateOnly? activeDate, DnetDateRange currentRange);
}
