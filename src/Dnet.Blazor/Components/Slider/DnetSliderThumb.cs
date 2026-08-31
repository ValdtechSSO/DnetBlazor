namespace Dnet.Blazor.Components.Slider;

/// <summary>Identifies the thumb that originated a slider event.</summary>
public enum DnetSliderThumb
{
    /// <summary>The only thumb of a single-value slider.</summary>
    Single,

    /// <summary>The lower-value thumb of a range slider.</summary>
    Start,

    /// <summary>The upper-value thumb of a range slider.</summary>
    End
}
