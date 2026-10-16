namespace Dnet.Blazor.Components.Timeline;

/// <summary>Controls where timeline events appear relative to the vertical rail.</summary>
public enum DnetTimelineAlignment
{
    /// <summary>Places the rail at the logical start of the timeline.</summary>
    Start,

    /// <summary>Places the rail at the logical end of the timeline.</summary>
    End,

    /// <summary>Alternates events around a centered rail, stacking them on narrow screens.</summary>
    Alternate
}
