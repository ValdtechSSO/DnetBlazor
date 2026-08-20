using Dnet.Blazor.Components.Overlay.Infrastructure.Services;
using Xunit;

namespace Dnet.Blazor.UnitTests;

public sealed class OverlayPositionSchedulerTests
{
    [Fact]
    public async Task Requests_preserve_the_latest_version_when_the_scheduler_is_already_running()
    {
        var scheduler = new OverlayPositionScheduler();
        var versions = new List<long>();
        var firstStarted = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var releaseFirst = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var applied = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);

        scheduler.Schedule(12, async version =>
        {
            versions.Add(version);
            firstStarted.TrySetResult();
            await releaseFirst.Task;
        });
        await firstStarted.Task.WaitAsync(TimeSpan.FromSeconds(2));

        scheduler.Schedule(12, version =>
        {
            versions.Add(version);
            applied.TrySetResult();
            return Task.CompletedTask;
        });
        releaseFirst.TrySetResult();

        await applied.Task.WaitAsync(TimeSpan.FromSeconds(2));

        Assert.Equal(new[] { 1L, 2L }, versions);
        Assert.Equal(2L, versions.Last());
        Assert.False(scheduler.IsCurrent(12, 2));
    }

    [Fact]
    public async Task A_request_during_an_inflight_calculation_runs_after_the_current_one()
    {
        var scheduler = new OverlayPositionScheduler();
        var firstStarted = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var releaseFirst = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var secondApplied = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);

        scheduler.Schedule(4, async _ =>
        {
            firstStarted.TrySetResult();
            await releaseFirst.Task;
        });
        await firstStarted.Task.WaitAsync(TimeSpan.FromSeconds(2));

        scheduler.Schedule(4, _ =>
        {
            secondApplied.TrySetResult();
            return Task.CompletedTask;
        });
        releaseFirst.TrySetResult();

        await secondApplied.Task.WaitAsync(TimeSpan.FromSeconds(2));
        Assert.False(scheduler.IsCurrent(4, 2));
    }

    [Fact]
    public async Task Removing_an_overlay_invalidates_its_inflight_result()
    {
        var scheduler = new OverlayPositionScheduler();
        var started = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var release = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);

        scheduler.Schedule(9, async version =>
        {
            started.TrySetResult();
            await release.Task;
            Assert.False(scheduler.IsCurrent(9, version));
        });
        await started.Task.WaitAsync(TimeSpan.FromSeconds(2));

        scheduler.Remove(9);
        release.TrySetResult();
        await Task.Yield();
    }
}
