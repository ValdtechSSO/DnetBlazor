using Dnet.Blazor.Components.Overlay.Infrastructure.Interfaces;

namespace Dnet.Blazor.Components.Overlay.Infrastructure.Services;

/// <summary>
/// Keeps the latest position request for each overlay and serializes its calculations.
/// Browser events are already frame-coalesced in JavaScript; this scheduler additionally
/// protects against overlapping interop calculations and stale results in .NET.
/// </summary>
public sealed class OverlayPositionScheduler : IOverlayPositionScheduler
{
    private sealed class PendingUpdate
    {
        public long Version { get; set; }

        public bool IsRunning { get; set; }

        public Func<long, Task>? ApplyAsync { get; set; }
    }

    private readonly object _syncRoot = new();
    private readonly Dictionary<int, PendingUpdate> _pendingUpdates = new();

    public void Schedule(int overlayReferenceId, Func<long, Task> applyAsync)
    {
        ArgumentNullException.ThrowIfNull(applyAsync);

        var shouldRun = false;
        lock (_syncRoot)
        {
            if (!_pendingUpdates.TryGetValue(overlayReferenceId, out var pending))
            {
                pending = new PendingUpdate();
                _pendingUpdates.Add(overlayReferenceId, pending);
            }

            pending.Version++;
            pending.ApplyAsync = applyAsync;

            if (!pending.IsRunning)
            {
                pending.IsRunning = true;
                shouldRun = true;
            }
        }

        if (shouldRun)
        {
            _ = RunAsync(overlayReferenceId);
        }
    }

    public bool IsCurrent(int overlayReferenceId, long requestVersion)
    {
        lock (_syncRoot)
        {
            return _pendingUpdates.TryGetValue(overlayReferenceId, out var pending)
                   && pending.Version == requestVersion;
        }
    }

    public void Remove(int overlayReferenceId)
    {
        lock (_syncRoot)
        {
            _pendingUpdates.Remove(overlayReferenceId);
        }
    }

    private async Task RunAsync(int overlayReferenceId)
    {
        // Let all events raised in the same Blazor turn contribute their final request.
        await Task.Yield();

        while (true)
        {
            long version;
            Func<long, Task>? applyAsync;
            lock (_syncRoot)
            {
                if (!_pendingUpdates.TryGetValue(overlayReferenceId, out var pending))
                {
                    return;
                }

                version = pending.Version;
                applyAsync = pending.ApplyAsync;
            }

            if (applyAsync is not null)
            {
                try
                {
                    await applyAsync(version).ConfigureAwait(false);
                }
                catch (ObjectDisposedException)
                {
                    // The pane was removed while its in-flight request completed.
                }
            }

            lock (_syncRoot)
            {
                if (!_pendingUpdates.TryGetValue(overlayReferenceId, out var pending))
                {
                    return;
                }

                if (pending.Version != version)
                {
                    continue;
                }

                pending.IsRunning = false;
                _pendingUpdates.Remove(overlayReferenceId);
                return;
            }
        }
    }
}
