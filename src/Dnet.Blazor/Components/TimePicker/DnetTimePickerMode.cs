namespace Dnet.Blazor.Components.TimePicker;

/// <summary>Defines how a <see cref="DnetTimePicker"/> lets the user choose a time.</summary>
public enum DnetTimePickerMode
{
    /// <summary>A list of times, generated from the interval or given as options.</summary>
    List,

    /// <summary>
    /// A clock face in a centred dialog: the hours, then the minutes, with the
    /// minutes between the five-minute marks reached through an arc outside
    /// the dial. The clock is 12-hour when the format uses <c>h</c>, and
    /// 24-hour otherwise. The interval and the options do not apply.
    /// </summary>
    Dial
}
