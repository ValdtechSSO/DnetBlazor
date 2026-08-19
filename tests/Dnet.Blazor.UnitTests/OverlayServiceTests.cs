using Dnet.Blazor.Components.ConnectedPanel.Infrastructure.Models;
using Dnet.Blazor.Components.ConnectedPanel.Infrastructure.Services;
using Dnet.Blazor.Components.Dialog.Infrastructure.Models;
using Dnet.Blazor.Components.Dialog.Infrastructure.Services;
using Dnet.Blazor.Components.FloatingPanel.Infrastructure.Models;
using Dnet.Blazor.Components.FloatingPanel.Infrastructure.Services;
using Dnet.Blazor.Components.Overlay.Infrastructure.Models;
using Dnet.Blazor.Components.Overlay.Infrastructure.Services;
using Dnet.Blazor.Components.Toast.Infrastructure.Models;
using Dnet.Blazor.Components.Toast.Infrastructure.Services;
using Dnet.Blazor.Components.Tooltip.Infrastructure.Models;
using Dnet.Blazor.Components.Tooltip.Infrastructure.Services;
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

    [Fact]
    public void Attach_preserves_theme_scope_and_panel_style()
    {
        var service = new OverlayService();
        var config = new OverlayConfig
        {
            ThemeScope = "dark",
            PanelStyle = "--dnet-btn-radius: 0"
        };
        OverlayConfig? attachedConfig = null;
        service.OnAttach += (_, value) => attachedConfig = value;

        service.Attach(static builder => builder.AddContent(0, "overlay"), config);

        Assert.Same(config, attachedConfig);
        Assert.Equal("dark", attachedConfig!.ThemeScope);
        Assert.Equal("--dnet-btn-radius: 0", attachedConfig.PanelStyle);
    }

    [Fact]
    public void Overlay_services_forward_theme_scope_and_panel_style()
    {
        var overlayService = new OverlayService();
        var attachedConfigs = new List<OverlayConfig>();
        overlayService.OnAttach += (_, config) => attachedConfigs.Add(config);
        const string panelStyle = "--dnet-btn-radius: 0";
        const string themeScope = "dark";

        new DialogService(overlayService).Open(
            typeof(TestComponent),
            new Dictionary<string, object>(),
            new DialogConfig { ThemeScope = themeScope, PanelStyle = panelStyle });

        using (var tooltipService = new TooltipService(overlayService))
        {
            tooltipService.Show(
                new TooltipConfig { ThemeScope = themeScope, PanelStyle = panelStyle },
                default);
        }

        new ConnectedPanelService(overlayService).Open(
            typeof(TestComponent),
            new Dictionary<string, object>(),
            default,
            new ConnectedPanelConfig { ThemeScope = themeScope, PanelStyle = panelStyle });

        new FloatingPanelService(overlayService).Show(
            typeof(TestComponent),
            new Dictionary<string, object>(),
            new FloatingPanelConfig { ThemeScope = themeScope, PanelStyle = panelStyle });

        new ToastService(overlayService).Show(
            new ToastConfig { ThemeScope = themeScope, PanelStyle = panelStyle },
            null!,
            new Dictionary<string, object>(),
            null!);

        Assert.Equal(5, attachedConfigs.Count);
        Assert.All(attachedConfigs, config =>
        {
            Assert.Equal(themeScope, config.ThemeScope);
            Assert.Equal(panelStyle, config.PanelStyle);
        });
    }

    private sealed class TestComponent : ComponentBase
    {
    }
}
