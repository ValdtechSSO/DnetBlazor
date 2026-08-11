namespace Dnet.Blazor.Components.Grid.Infrastructure.Models;

/// <summary>
/// Snapshot of the Grid work performed by the virtual data pipeline.
/// It is intended for tests and opt-in diagnostics, not for per-row logging.
/// </summary>
public sealed class GridDiagnostics
{
    public long VirtualRequestsStarted { get; init; }

    public long VirtualRequestsCancelled { get; init; }

    public long VirtualResponsesDiscarded { get; init; }

    public long VirtualResponsesApplied { get; init; }

    public long VirtualRefreshDurationMilliseconds { get; init; }

    public int TotalItemCount { get; init; }

    public int ItemsBefore { get; init; }

    public int VisibleItemCapacity { get; init; }

    public int RenderedItemCount { get; init; }

    public int OverscanCount { get; init; }
}
