using Dnet.Blazor.Components.DatePicker.Infrastructure.Enums;

namespace Dnet.Blazor.Components.DatePicker.Infrastructure.Models;

/// <summary>Provides state and commands to a custom calendar header.</summary>
public sealed record DnetCalendarHeaderContext(
    string PeriodLabel,
    DnetCalendarView View,
    bool PreviousDisabled,
    bool NextDisabled,
    Func<Task> Previous,
    Func<Task> Next,
    Func<Task> ChangeView);
