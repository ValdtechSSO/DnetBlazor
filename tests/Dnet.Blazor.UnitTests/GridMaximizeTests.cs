using Bunit;
using Dnet.Blazor.Components.Grid.BlgGrid;
using Dnet.Blazor.Components.Grid.Infrastructure.Entities;
using Dnet.Blazor.Infrastructure.Services;
using Dnet.Blazor.Localization;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Dnet.Blazor.UnitTests;

// GridOptions.EnableMaximize puts a button in the grid's corner that makes the grid
// fill the window and brings it back.
public sealed class GridMaximizeTests : BunitContext
{
    public GridMaximizeTests()
    {
        Services.AddDnetBlazor();
        JSInterop.Mode = JSRuntimeMode.Loose;
    }

    [Fact]
    public void Without_the_option_the_grid_has_no_button_and_keeps_its_classes()
    {
        var cut = RenderGrid(enableMaximize: false);

        Assert.Empty(cut.FindAll(".blg-maximize-button"));
        Assert.Equal("blg-arcadia-theme", Container(cut).GetAttribute("class"));
    }

    [Fact]
    public void The_button_maximizes_the_grid_and_brings_it_back()
    {
        var cut = RenderGrid(enableMaximize: true);
        Assert.Contains("blg-grid-maximizable", Container(cut).ClassList);
        Assert.DoesNotContain("blg-grid-maximized", Container(cut).ClassList);
        Assert.Equal("Maximizar tabla", cut.Find(".blg-maximize-button").GetAttribute("aria-label"));
        Assert.NotNull(cut.Find(".blg-maximize-button .blg-icon-maximize"));

        cut.Find(".blg-maximize-button").Click();

        Assert.True(cut.Instance.IsMaximized);
        Assert.Contains("blg-grid-maximized", Container(cut).ClassList);
        Assert.Equal("Restaurar tabla", cut.Find(".blg-maximize-button").GetAttribute("aria-label"));
        Assert.NotNull(cut.Find(".blg-maximize-button .blg-icon-restore"));

        cut.Find(".blg-maximize-button").Click();

        Assert.False(cut.Instance.IsMaximized);
        Assert.DoesNotContain("blg-grid-maximized", Container(cut).ClassList);
    }

    [Fact]
    public async Task Escape_from_the_script_restores_the_grid()
    {
        var cut = RenderGrid(enableMaximize: true);
        cut.Find(".blg-maximize-button").Click();

        await cut.InvokeAsync(cut.Instance.RestoreFromMaximized);

        Assert.False(cut.Instance.IsMaximized);
        Assert.DoesNotContain("blg-grid-maximized", Container(cut).ClassList);
    }

    [Fact]
    public async Task The_label_follows_the_language()
    {
        var cut = RenderGrid(enableMaximize: true);

        await cut.InvokeAsync(() => Services.GetRequiredService<IDnetLocalizer>().SetCulture("en-US"));

        cut.WaitForAssertion(() => Assert.Equal("Maximize table", cut.Find(".blg-maximize-button").GetAttribute("aria-label")));
        cut.Find(".blg-maximize-button").Click();
        Assert.Equal("Restore table", cut.Find(".blg-maximize-button").GetAttribute("title"));
    }

    private IRenderedComponent<BlgGrid<string>> RenderGrid(bool enableMaximize) =>
        Render<BlgGrid<string>>(parameters => parameters
            .Add(grid => grid.GridData, new[] { "Record" })
            .Add(grid => grid.GridColumns, new List<GridColumn<string>>())
            .Add(grid => grid.GridOptions, new GridOptions<string> { UseVirtualization = false, EnableMaximize = enableMaximize }));

    private static AngleSharp.Dom.IElement Container(IRenderedComponent<BlgGrid<string>> cut) =>
        cut.Find(".blg-arcadia-theme");
}
