using Bunit;
using Dnet.Blazor.Components.Overlay.Infrastructure.Models;
using Dnet.Blazor.Components.Overlay.Infrastructure.Services;
using Dnet.Blazor.Components.Toast.Infrastructure.Interfaces;
using Dnet.Blazor.Components.Toast.Infrastructure.Models;
using Dnet.Blazor.Components.Toast.Infrastructure.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Dnet.Blazor.UnitTests;

public sealed class ToastTests : BunitContext
{
    [Fact]
    public void Stack_limits_visible_toasts_and_keeps_permanent_live_regions()
    {
        var (service, content) = CreateStack(5);

        var cut = Render(content());

        Assert.Equal(4, cut.FindAll(".dnet-toast").Count);
        Assert.NotNull(cut.Find("[role='status'][aria-live='polite']"));
        Assert.NotNull(cut.Find("[role='alert'][aria-live='assertive']"));
        Assert.Single(cut.FindAll(".dnet-toast-stack"));
        GC.KeepAlive(service);
    }

    [Fact]
    public void Persistent_toast_omits_progress_and_uses_descriptive_state_attributes()
    {
        var (_, content) = CreateStack(1);

        var cut = Render(content());
        var toast = cut.Find(".dnet-toast");

        Assert.Equal("info", toast.GetAttribute("data-severity"));
        Assert.Equal("open", toast.GetAttribute("data-state"));
        Assert.Empty(cut.FindAll(".dnet-toast__progress"));
        Assert.Equal("Close notification", cut.Find(".dnet-toast__close").GetAttribute("aria-label"));
    }

    [Fact]
    public void Queue_promotes_the_next_toast_after_a_visible_one_closes()
    {
        var (_, content) = CreateStack(5);
        var cut = Render(content());

        Assert.DoesNotContain("Toast 5", cut.Markup);
        cut.Find(".dnet-toast__close").Click();

        cut.WaitForAssertion(() =>
        {
            Assert.Equal(4, cut.FindAll(".dnet-toast").Count);
            Assert.Contains("Toast 5", cut.Markup);
        }, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void Timed_toast_is_removed_when_its_progress_duration_finishes()
    {
        var overlayService = new OverlayService();
        var toastService = new ToastService(overlayService);
        RenderFragment? attachedContent = null;
        overlayService.OnAttach += (content, _) => attachedContent = content;
        Services.AddSingleton<IToastService>(toastService);

        toastService.Show(new ToastConfig
        {
            Title = "Timed toast",
            Duration = 50
        });

        var cut = Render(attachedContent ?? throw new InvalidOperationException("Toast stack was not attached."));

        cut.WaitForAssertion(
            () => Assert.Empty(cut.FindAll(".dnet-toast")),
            TimeSpan.FromSeconds(1));
    }

    private (ToastService Service, Func<RenderFragment> Content) CreateStack(int count)
    {
        var overlayService = new OverlayService();
        var toastService = new ToastService(overlayService);
        RenderFragment? attachedContent = null;
        overlayService.OnAttach += (content, _) => attachedContent = content;
        Services.AddSingleton<IToastService>(toastService);

        for (var index = 1; index <= count; index++)
        {
            toastService.Show(new ToastConfig
            {
                Title = $"Toast {index}",
                Text = "Saved successfully",
                Duration = null,
                MaxVisible = 4
            });
        }

        return (toastService, () => attachedContent ?? throw new InvalidOperationException("Toast stack was not attached."));
    }
}
