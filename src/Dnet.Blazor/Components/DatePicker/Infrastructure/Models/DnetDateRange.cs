namespace Dnet.Blazor.Components.DatePicker.Infrastructure.Models;

/// <summary>Represents a date range whose start and end may be selected independently.</summary>
public sealed record DnetDateRange(DateOnly? Start = null, DateOnly? End = null)
{
    /// <summary>Gets whether both range boundaries have been selected.</summary>
    public bool IsComplete => Start.HasValue && End.HasValue;

    /// <summary>Gets whether neither boundary has been selected.</summary>
    public bool IsEmpty => !Start.HasValue && !End.HasValue;

    /// <summary>Returns the range with its boundaries in chronological order.</summary>
    public DnetDateRange Normalize() => Start.HasValue && End.HasValue && Start > End
        ? new DnetDateRange(End, Start)
        : this;

    /// <summary>Gets whether the supplied date is inside the completed range.</summary>
    public bool Contains(DateOnly date)
    {
        var range = Normalize();
        return range.Start.HasValue && range.End.HasValue && date >= range.Start && date <= range.End;
    }
}
