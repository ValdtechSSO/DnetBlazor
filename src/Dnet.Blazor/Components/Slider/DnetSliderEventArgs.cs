namespace Dnet.Blazor.Components.Slider;

/// <summary>Provides data for slider input and drag events.</summary>
public sealed class DnetSliderEventArgs : EventArgs
{
    /// <summary>Initializes a slider event.</summary>
    public DnetSliderEventArgs(DnetSliderThumb thumb, double value)
    {
        Thumb = thumb;
        Value = value;
    }

    /// <summary>Gets the thumb that originated the event.</summary>
    public DnetSliderThumb Thumb { get; }

    /// <summary>Gets the value of the originating thumb.</summary>
    public double Value { get; }
}
