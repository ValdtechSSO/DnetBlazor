namespace Dnet.Blazor.Components.Timeline;

/// <summary>Describes an event independently of its position in the timeline.</summary>
public enum DnetTimelineItemStatus
{
    /// <summary>The event has finished.</summary>
    Completed,

    /// <summary>The event is the current step.</summary>
    Current,

    /// <summary>The event has not started.</summary>
    Pending
}
