namespace Dnet.Blazor.Components.Overlay.Infrastructure.Interfaces;

/// <summary>
/// Coalesces position updates so an overlay never runs more than one calculation at a time.
/// </summary>
public interface IOverlayPositionScheduler
{
    void Schedule(int overlayReferenceId, Func<long, Task> applyAsync);

    bool IsCurrent(int overlayReferenceId, long requestVersion);

    void Remove(int overlayReferenceId);
}
