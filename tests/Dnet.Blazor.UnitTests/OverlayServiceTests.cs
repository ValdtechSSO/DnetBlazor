using Dnet.Blazor.Components.Overlay.Infrastructure.Models;
using Dnet.Blazor.Components.Overlay.Infrastructure.Services;
using Microsoft.AspNetCore.Components;
using Xunit;

namespace Dnet.Blazor.UnitTests;

public sealed class OverlayServiceTests
{
    [Fact]
    public void Attach_generates_unique_monotonic_ids()
    {
        var service = new OverlayService();
        var ids = new List<int>();
        service.OnAttach += (_, config) => ids.Add(config.OverlayReferenceId);

        for (var index = 0; index < 1_500; index++)
        {
            service.Attach(static builder => builder.AddContent(0, "overlay"), new OverlayConfig());
        }

        Assert.Equal(1_500, ids.Distinct().Count());
        Assert.Equal(Enumerable.Range(1, 1_500), ids);
    }

    [Fact]
    public void Detach_of_an_unknown_overlay_does_not_emit_or_change_active_overlay()
    {
        var service = new OverlayService();
        var detached = 0;
        service.OnDetach += _ => detached++;

        var reference = service.Attach(static builder => builder.AddContent(0, "overlay"), new OverlayConfig());
        service.Detach(new OverlayResult { OverlayReferenceId = int.MaxValue });
        service.Detach(new OverlayResult { OverlayReferenceId = reference.GetOverlayReferenceId() });

        Assert.Equal(1, detached);
    }
}
