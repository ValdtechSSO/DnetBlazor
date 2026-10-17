using Dnet.Blazor.Localization;

namespace Dnet.Blazor.Components.DatePicker.Infrastructure.Models;

/// <summary>Contains optional localizable label overrides used by date pickers and calendars.</summary>
public sealed class DnetDatePickerLabels
{
    private string? _openCalendar;
    public string OpenCalendar { get => _openCalendar ?? "Open calendar"; set => _openCalendar = value; }

    private string? _calendar;
    public string Calendar { get => _calendar ?? "Calendar"; set => _calendar = value; }

    private string? _previousPeriod;
    public string PreviousPeriod { get => _previousPeriod ?? "Previous period"; set => _previousPeriod = value; }

    private string? _nextPeriod;
    public string NextPeriod { get => _nextPeriod ?? "Next period"; set => _nextPeriod = value; }

    private string? _chooseMonthAndYear;
    public string ChooseMonthAndYear { get => _chooseMonthAndYear ?? "Choose month and year"; set => _chooseMonthAndYear = value; }

    private string? _chooseDate;
    public string ChooseDate { get => _chooseDate ?? "Choose date"; set => _chooseDate = value; }

    private string? _startDate;
    public string StartDate { get => _startDate ?? "Start date"; set => _startDate = value; }

    private string? _endDate;
    public string EndDate { get => _endDate ?? "End date"; set => _endDate = value; }

    private string? _apply;
    public string Apply { get => _apply ?? "Apply"; set => _apply = value; }

    private string? _cancel;
    public string Cancel { get => _cancel ?? "Cancel"; set => _cancel = value; }

    private string? _reset;
    public string Reset { get => _reset ?? "Reset"; set => _reset = value; }

    // Unset properties remain distinct from explicit overrides, including an explicit English label.
    internal DnetDatePickerLabels Resolve(IDnetLocalizer localizer) => new()
    {
        OpenCalendar = _openCalendar ?? localizer["DatePicker.OpenCalendar"],
        Calendar = _calendar ?? localizer["DatePicker.Calendar"],
        PreviousPeriod = _previousPeriod ?? localizer["DatePicker.PreviousPeriod"],
        NextPeriod = _nextPeriod ?? localizer["DatePicker.NextPeriod"],
        ChooseMonthAndYear = _chooseMonthAndYear ?? localizer["DatePicker.ChooseMonthAndYear"],
        ChooseDate = _chooseDate ?? localizer["DatePicker.ChooseDate"],
        StartDate = _startDate ?? localizer["DatePicker.StartDate"],
        EndDate = _endDate ?? localizer["DatePicker.EndDate"],
        Apply = _apply ?? localizer["Common.Apply"],
        Cancel = _cancel ?? localizer["Common.Cancel"],
        Reset = _reset ?? localizer["Common.Reset"],
    };
}
