using System.Text.Json;
using Microsoft.Playwright;
using Xunit;

namespace Dnet.Blazor.BrowserTests;

// The week calendars, in the page and in the week picker's popup. In the Liquid Glass
// themes the chosen week is one accent pill across its row, the week under the pointer
// a tint of it, today a circle in its number's colour and the month buttons round
// glass buttons. Other themes keep the days' own fills.
public sealed class WeekCalendarGlassTests
{
    private const string Inline = "#date-picker-week-inline .dnet-datepickerweek-raw-content";
    private const string Popup = ".dnet-datepickerweek-content";

    [Theory]
    [InlineData("liquid-glass", false)]
    [InlineData("liquid-glass-dark", false)]
    [InlineData("liquid-glass", true)]
    public async Task The_chosen_week_is_one_pill_and_today_a_circle(string theme, bool popup)
    {
        if (!Enabled) return;
        await using var session = await OpenAsync(theme);
        var page = session.Page;
        var calendar = await ShowCalendarAsync(page, popup);
        var day = popup ? "dnet-datepickerweekca-day" : "dnet-datepickerweekca-raw-day";

        // One band behind the chosen row, as wide as the row, in the accent with its glow;
        // the days themselves paint nothing.
        var band = await calendar.EvaluateAsync<Band>($$"""
            (calendar, day) => {
                const selected = [...calendar.querySelectorAll(`.${day}-content.${day}-selected`)];
                const first = selected[0].parentElement;
                const last = selected[selected.length - 1].parentElement;
                const style = getComputedStyle(first, '::before');
                const box = first.getBoundingClientRect();
                return {
                    days: selected.length,
                    width: parseFloat(style.width),
                    row: last.getBoundingClientRect().right - box.left,
                    color: style.backgroundColor,
                    shadow: style.boxShadow,
                    radius: parseFloat(style.borderTopLeftRadius),
                    dayFills: [...new Set(selected.map(element => getComputedStyle(element).backgroundColor))]
                };
            }
            """, day);
        Assert.Equal(7, band.Days);
        Assert.Equal(band.Row, band.Width, 1);
        Assert.NotEqual("rgba(0, 0, 0, 0)", band.Color);
        Assert.NotEqual("none", band.Shadow);
        Assert.True(band.Radius > 100, "The band has round ends.");
        Assert.Equal(["rgba(0, 0, 0, 0)"], band.DayFills);

        // The week under the pointer: the band shows from any of its days.
        var other = calendar.Locator($".{day}:has(> .{day}-content:not(.{day}-selected))").Nth(20);
        await other.HoverAsync();
        var hovered = await other.EvaluateAsync<string>($$"""
            (element, day) => {
                const days = [...element.parentElement.children];
                const monday = days[days.indexOf(element) - days.indexOf(element) % 7];
                return getComputedStyle(monday, '::before').backgroundColor;
            }
            """, day);
        Assert.NotEqual("rgba(0, 0, 0, 0)", hovered);

        // The month buttons are round glass buttons, 8px apart.
        var buttons = await calendar.EvaluateAsync<string[]>(
            "calendar => { const next = calendar.querySelector('[class*=next-button]'); const style = getComputedStyle(next); return [style.backgroundColor, style.boxShadow, String(Math.round(next.getBoundingClientRect().left - next.previousElementSibling.getBoundingClientRect().right)) + 'px']; }");
        Assert.NotEqual("rgba(0, 0, 0, 0)", buttons[0]);
        Assert.Contains("inset", buttons[1]);
        Assert.Equal("8px", buttons[2]);

        // Today, outside the chosen week, is a circle drawn in its number's colour,
        // without the ring. (A click in the popup chooses the week and closes it.)
        if (!popup)
        {
            await other.ClickAsync();
            var today = calendar.Locator($".{day}-content.{day}-today");
            if (await today.CountAsync() == 1 && !await today.EvaluateAsync<bool>("(element, day) => element.classList.contains(day + '-selected')", day))
            {
                var mark = await today.EvaluateAsync<string[]>(
                    "element => { const style = getComputedStyle(element); return [style.backgroundImage, style.boxShadow]; }");
                Assert.StartsWith("radial-gradient", mark[0]);
                Assert.Equal("none", mark[1]);
            }
        }
        Assert.Empty(session.Errors);
    }

    [Theory]
    [InlineData("", false)]
    [InlineData("dark", true)]
    public async Task Other_themes_keep_the_days_own_fills(string theme, bool popup)
    {
        if (!Enabled) return;
        await using var session = await OpenAsync(theme);
        var calendar = await ShowCalendarAsync(session.Page, popup);
        var day = popup ? "dnet-datepickerweekca-day" : "dnet-datepickerweekca-raw-day";

        var fills = await calendar.EvaluateAsync<string[]>($$"""
            (calendar, day) => {
                const selected = calendar.querySelector(`.${day}-content.${day}-selected`);
                const band = getComputedStyle(selected.parentElement, '::before');
                const next = getComputedStyle(calendar.querySelector('[class*=next-button]'));
                return [getComputedStyle(selected).backgroundColor, band.backgroundColor, band.backgroundImage,
                    next.backgroundColor, String(Math.round(calendar.querySelector('[class*=next-button]').getBoundingClientRect().left - calendar.querySelector('[class*=previous-button]').getBoundingClientRect().right)) + 'px'];
            }
            """, day);
        Assert.NotEqual("rgba(0, 0, 0, 0)", fills[0]);
        Assert.Equal("rgba(0, 0, 0, 0)", fills[1]);
        Assert.Equal("none, none", fills[2]);
        Assert.Equal("rgba(0, 0, 0, 0)", fills[3]);
        Assert.Equal("2px", fills[4]);
        Assert.Empty(session.Errors);
    }

    // The week picker's Reset is the date picker's: a real button, in every theme.
    [Theory]
    [InlineData("")]
    [InlineData("liquid-glass")]
    public async Task The_week_popup_resets_with_the_date_pickers_button(string theme)
    {
        if (!Enabled) return;
        await using var session = await OpenAsync(theme);
        var panel = await ShowCalendarAsync(session.Page, popup: true);

        var reset = panel.Locator("button.dnet-datepicker-reset-button-wrapper");
        await Assertions.Expect(reset).ToHaveTextAsync("Reset");
        await reset.FocusAsync();
        await session.Page.Keyboard.PressAsync("Enter");
        await Assertions.Expect(panel).ToBeHiddenAsync();
        Assert.Empty(session.Errors);
    }

    // The day calendar shown in the page: a pane of glass with round glass month
    // buttons, the chosen day in the accent with its glow and today without a ring
    // in Liquid Glass; its own fill, transparent buttons and the ring elsewhere.
    [Theory]
    [InlineData("liquid-glass", true)]
    [InlineData("liquid-glass-dark", true)]
    [InlineData("", false)]
    [InlineData("dark", false)]
    public async Task The_day_calendar_in_the_page_is_glass_only_in_Liquid_Glass(string theme, bool glass)
    {
        if (!Enabled) return;
        await using var session = await OpenAsync(theme);
        var calendar = session.Page.Locator(".date-calendar-preview .dnet-calendar").First;
        await calendar.ScrollIntoViewIfNeededAsync();
        await Assertions.Expect(calendar).ToBeVisibleAsync();

        var look = await calendar.EvaluateAsync<string[]>("""
            calendar => {
                const style = getComputedStyle(calendar);
                const previous = calendar.querySelector('.dnet-calendar-previous-button');
                const next = calendar.querySelector('.dnet-calendar-next-button');
                const selected = calendar.querySelector('.dnet-calendar-day-selected');
                return [style.backgroundColor, style.backgroundImage, getComputedStyle(next).backgroundColor,
                    String(next.getBoundingClientRect().width), String(Math.round(next.getBoundingClientRect().left - previous.getBoundingClientRect().right)),
                    getComputedStyle(selected).boxShadow];
            }
            """);

        // Today: the sample's calendar opens on August 2026, so it moves to today's month.
        var months = (DateTime.Today.Year - 2026) * 12 + DateTime.Today.Month - 8;
        var button = calendar.Locator(months > 0 ? ".dnet-calendar-next-button" : ".dnet-calendar-previous-button");
        for (var move = 0; move < Math.Abs(months); move++) await button.ClickAsync();
        var today = calendar.Locator(".dnet-calendar-day:not(.dnet-calendar-day-outside) > .dnet-calendar-day-today");
        await Assertions.Expect(today).ToHaveCountAsync(1);
        var ring = await today.EvaluateAsync<string>("element => getComputedStyle(element).boxShadow");

        if (glass)
        {
            Assert.NotEqual("none, none", look[1]);
            Assert.Matches(@"rgba\(.*, 0\.\d+\)", look[0]);
            Assert.NotEqual("rgba(0, 0, 0, 0)", look[2]);
            Assert.Equal("32", look[3]);
            Assert.Equal("8", look[4]);
            Assert.Contains("28px", look[5]);
            Assert.Equal("none", ring);
        }
        else
        {
            Assert.Equal("none, none", look[1]);
            Assert.Equal("rgba(0, 0, 0, 0)", look[2]);
            Assert.Equal("36", look[3]);
            Assert.Equal("2", look[4]);
            Assert.DoesNotContain("28px", look[5]);
            Assert.Contains("1px inset", ring);
        }
        Assert.Empty(session.Errors);
    }

    private static bool Enabled => string.Equals(
        Environment.GetEnvironmentVariable("DNET_BLAZOR_BROWSER_TESTS"), "true", StringComparison.OrdinalIgnoreCase);

    private static async Task<ILocator> ShowCalendarAsync(IPage page, bool popup)
    {
        if (!popup)
        {
            var inline = page.Locator(Inline);
            await inline.ScrollIntoViewIfNeededAsync();
            await Assertions.Expect(inline).ToBeVisibleAsync();
            return inline;
        }

        await page.Locator("#date-picker-week .dnet-plain-select").ClickAsync();
        var panel = page.Locator(Popup);
        await Assertions.Expect(panel).ToBeVisibleAsync();
        return panel;
    }

    private static async Task<Session> OpenAsync(string theme)
    {
        var baseUrl = (Environment.GetEnvironmentVariable("DNET_BLAZOR_BASE_URL") ?? "http://127.0.0.1:5101").TrimEnd('/');
        var playwright = await Playwright.CreateAsync();
        var browser = await playwright.Chromium.LaunchAsync(new() { Headless = true });
        var page = await browser.NewPageAsync(new()
        {
            IgnoreHTTPSErrors = true,
            ViewportSize = new() { Width = 1440, Height = 900 }
        });
        var session = new Session(playwright, browser, page);
        page.PageError += (_, error) => session.Errors.Add(error);
        await page.RouteAsync("https://fonts.googleapis.com/**", route => route.AbortAsync());
        await page.AddInitScriptAsync(
            $"localStorage.setItem('dnet.theme', JSON.stringify({JsonSerializer.Serialize(theme)}));");
        await BrowserLanguage.UseEnglishAsync(page);
        await BrowserReady.OpenAsync(page, $"{baseUrl}/DatePicker");
        return session;
    }

    private sealed class Session(IPlaywright playwright, IBrowser browser, IPage page) : IAsyncDisposable
    {
        public IPage Page { get; } = page;
        public List<string> Errors { get; } = [];

        public async ValueTask DisposeAsync()
        {
            await browser.DisposeAsync();
            playwright.Dispose();
        }
    }

    private sealed class Band
    {
        public int Days { get; set; }
        public double Width { get; set; }
        public double Row { get; set; }
        public string Color { get; set; } = "";
        public string Shadow { get; set; } = "";
        public double Radius { get; set; }
        public string[] DayFills { get; set; } = [];
    }
}
