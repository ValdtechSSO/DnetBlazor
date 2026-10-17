using System.Text;
using System.Text.RegularExpressions;
using Microsoft.Playwright;
using Xunit;

namespace Dnet.Blazor.BrowserTests;

public sealed class ImageEditorShowcaseTests
{
    [Theory]
    [InlineData("", 1440, "image/png")]
    [InlineData("dark", 1440, "image/jpeg")]
    [InlineData("", 390, "image/jpeg")]
    [InlineData("dark", 390, "image/png")]
    public async Task Local_images_can_be_saved_downloaded_edited_and_cleared_while_invalid_inputs_keep_the_last_result(
        string theme, int width, string mime)
    {
        if (!BrowserTestsEnabled) return;
        using var playwright = await Playwright.CreateAsync();
        await using var browser = await playwright.Chromium.LaunchAsync(new() { Headless = true });
        var (page, errors) = await OpenAsync(browser, theme, width);
        var preview = page.Locator("#image-editor-result-image");
        var input = page.Locator("#image-editor-file");
        await Assertions.Expect(page.Locator("#image-editor-result-empty")).ToBeVisibleAsync();
        await Assertions.Expect(page.Locator("#image-editor-download")).ToHaveCountAsync(0);
        await CheckLayoutAsync(page);

        await page.Locator("#image-editor-demo").FocusAsync();
        await page.Locator("#image-editor-demo").PressAsync("Enter");
        var editor = await ReadyEditorAsync(page);
        await CheckLayoutAsync(page);
        await editor.GetByRole(AriaRole.Button, new() { Name = "Apply", Exact = true }).ClickAsync();
        await WaitForPreviewAsync(page, "image/png", 186, 186);
        await Assertions.Expect(page.Locator("#image-editor-result-name")).ToHaveTextAsync("blazor-logo-editada.png");
        var demoBytes = await DownloadAsync(page);
        var source = await page.APIRequest.GetAsync($"{BaseUrl}/_content/Dnet.App.Shared/img/blazor-logo.png");
        Assert.True(source.Ok, $"Local image fixture returned HTTP {source.Status}.");
        Assert.Equal(await source.BodyAsync(), demoBytes);
        var savedDemo = await preview.GetAttributeAsync("src");

        await page.Locator("#image-editor-reopen").ClickAsync();
        editor = await ReadyEditorAsync(page);
        await editor.GetByRole(AriaRole.Button, new() { Name = "Cancel", Exact = true }).ClickAsync();
        await Assertions.Expect(page.Locator(".dnet-image-editor")).ToHaveCountAsync(0);
        await Assertions.Expect(preview).ToHaveAttributeAsync("src", savedDemo!);
        await Assertions.Expect(page.Locator("#image-editor-status")).ToContainTextAsync("Edición cancelada");

        await input.SetInputFilesAsync(new FilePayload
        {
            Name = "unsupported.svg", MimeType = "image/svg+xml", Buffer = Encoding.UTF8.GetBytes("<svg xmlns='http://www.w3.org/2000/svg'/>")
        });
        await Assertions.Expect(page.Locator("#image-editor-error")).ToContainTextAsync("formato no está admitido");
        await Assertions.Expect(preview).ToHaveAttributeAsync("src", savedDemo!);
        await Assertions.Expect(page.Locator(".dnet-image-editor")).ToHaveCountAsync(0);
        await input.SetInputFilesAsync(new FilePayload
        {
            Name = "invalid.png", MimeType = "image/png", Buffer = Encoding.UTF8.GetBytes("This is not an image.")
        });
        await Assertions.Expect(page.Locator("#image-editor-error")).ToContainTextAsync("no contiene una imagen");
        await Assertions.Expect(preview).ToHaveAttributeAsync("src", savedDemo!);

        // A valid signature does not imply the browser can decode the image.
        await input.SetInputFilesAsync(new FilePayload
        {
            Name = "truncated.png", MimeType = "image/png", Buffer = [137, 80, 78, 71, 13, 10, 26, 10]
        });
        editor = page.Locator(".dnet-image-editor");
        await Assertions.Expect(editor.GetByRole(AriaRole.Alert)).ToBeVisibleAsync();
        await Assertions.Expect(editor.GetByRole(AriaRole.Button, new() { Name = "Apply", Exact = true })).ToBeDisabledAsync();
        await editor.GetByRole(AriaRole.Button, new() { Name = "Cancel", Exact = true }).ClickAsync();
        await Assertions.Expect(page.Locator(".dnet-image-editor")).ToHaveCountAsync(0);
        await Assertions.Expect(preview).ToHaveAttributeAsync("src", savedDemo!);

        var fixture = await CreateFixtureAsync(page, mime);
        await input.SetInputFilesAsync(fixture);
        editor = await ReadyEditorAsync(page);
        await Assertions.Expect(editor.Locator(".dnet-image-editor-subtitle")).ToContainTextAsync("640 × 400 px");
        var apply = editor.GetByRole(AriaRole.Button, new() { Name = "Apply", Exact = true });
        await apply.FocusAsync();
        await apply.PressAsync("Enter");
        await WaitForPreviewAsync(page, mime, 640, 400);
        var extension = mime == "image/png" ? "png" : "jpg";
        await Assertions.Expect(page.Locator("#image-editor-result-name")).ToHaveTextAsync($"workspace-fixture-editada.{extension}");
        await Assertions.Expect(page.Locator("#image-editor-result-format")).ToHaveTextAsync(mime == "image/png" ? "PNG" : "JPEG");
        await Assertions.Expect(page.Locator("#image-editor-error")).ToHaveCountAsync(0);
        Assert.Equal(fixture.Buffer, await DownloadAsync(page));

        await page.Locator("#image-editor-reopen").ClickAsync();
        editor = await ReadyEditorAsync(page);
        await editor.GetByRole(AriaRole.Button, new() { Name = "Rotate right", Exact = true }).ClickAsync();
        await Assertions.Expect(editor.Locator(".dnet-image-editor-subtitle")).ToContainTextAsync("400 × 640 px");
        await editor.GetByRole(AriaRole.Button, new() { Name = "Apply", Exact = true }).ClickAsync();
        await WaitForPreviewAsync(page, mime, 400, 640);
        await Assertions.Expect(page.Locator("#image-editor-result-name")).ToHaveTextAsync($"workspace-fixture-editada.{extension}");
        var editedBytes = await DownloadAsync(page);
        Assert.False(fixture.Buffer.SequenceEqual(editedBytes));
        Assert.True(editedBytes.Length > 0);
        var corner = await preview.EvaluateAsync<int[]>("""
            image => {
                const canvas = document.createElement('canvas');
                canvas.width = image.naturalWidth; canvas.height = image.naturalHeight;
                const context = canvas.getContext('2d'); context.drawImage(image, 0, 0);
                return Array.from(context.getImageData(20, 20, 1, 1).data);
            }
            """);
        Assert.True(corner[1] > corner[0] + 50 && corner[1] > corner[2] + 40,
            $"The rotated top-left corner should come from the green bottom-left quadrant: {string.Join(",", corner)}.");
        await CheckLayoutAsync(page);

        await page.Locator("#image-editor-clear").ClickAsync();
        await Assertions.Expect(page.Locator("#image-editor-result-empty")).ToBeVisibleAsync();
        await Assertions.Expect(preview).ToHaveCountAsync(0);
        await Assertions.Expect(page.Locator("#image-editor-download")).ToHaveCountAsync(0);
        await Assertions.Expect(page.Locator("#image-editor-status")).ToContainTextAsync("Resultado eliminado");
        await page.Locator("#image-editor-demo").ClickAsync();
        editor = await ReadyEditorAsync(page);
        await editor.GetByRole(AriaRole.Button, new() { Name = "Cancel", Exact = true }).ClickAsync();
        await Assertions.Expect(page.Locator("#image-editor-result-empty")).ToBeVisibleAsync();
        Assert.True(errors.Count == 0, string.Join(Environment.NewLine, errors));
    }

    private static bool BrowserTestsEnabled => string.Equals(
        Environment.GetEnvironmentVariable("DNET_BLAZOR_BROWSER_TESTS"), "true", StringComparison.OrdinalIgnoreCase);
    private static string BaseUrl => (Environment.GetEnvironmentVariable("DNET_BLAZOR_BASE_URL") ?? "http://127.0.0.1:5101").TrimEnd('/');

    private static async Task<ILocator> ReadyEditorAsync(IPage page)
    {
        var editor = page.Locator(".dnet-image-editor");
        await Assertions.Expect(editor).ToBeVisibleAsync();
        await Assertions.Expect(editor.GetByRole(AriaRole.Button, new() { Name = "Apply", Exact = true })).ToBeEnabledAsync();
        await Assertions.Expect(editor.Locator(".dnet-crop-container")).ToBeVisibleAsync();
        await page.WaitForFunctionAsync("() => document.querySelector('.dnet-editing-image-canvas')?.width > 1");
        return editor;
    }

    private static async Task WaitForPreviewAsync(IPage page, string mime, int width, int height)
    {
        await Assertions.Expect(page.Locator(".dnet-image-editor")).ToHaveCountAsync(0);
        await Assertions.Expect(page.Locator("#image-editor-result-image")).ToHaveAttributeAsync("src", new Regex($"^data:{Regex.Escape(mime)};base64,"));
        await page.WaitForFunctionAsync("""
            expected => {
                const image = document.getElementById('image-editor-result-image');
                return image?.complete && image.naturalWidth === expected.width && image.naturalHeight === expected.height;
            }
            """, new { width, height });
    }

    private static async Task<byte[]> DownloadAsync(IPage page)
    {
        var link = page.Locator("#image-editor-download");
        var filename = await link.GetAttributeAsync("download");
        var download = await page.RunAndWaitForDownloadAsync(() => link.ClickAsync());
        Assert.Equal(filename, download.SuggestedFilename);
        Assert.Null(await download.FailureAsync());
        return await File.ReadAllBytesAsync(await download.PathAsync());
    }

    private static async Task<FilePayload> CreateFixtureAsync(IPage page, string mime)
    {
        var base64 = await page.EvaluateAsync<string>("""
            mime => {
                const canvas = document.createElement('canvas'); canvas.width = 640; canvas.height = 400;
                const context = canvas.getContext('2d');
                for (const [x, y, color] of [[0, 0, 'rgb(220,35,45)'], [320, 0, 'rgb(25,90,210)'], [0, 200, 'rgb(20,160,80)'], [320, 200, 'rgb(235,185,30)']]) {
                    context.fillStyle = color; context.fillRect(x, y, 320, 200);
                }
                return canvas.toDataURL(mime, 0.95).split(',')[1];
            }
            """, mime);
        return new FilePayload
        {
            Name = $"workspace-fixture.{(mime == "image/png" ? "png" : "jpg")}", MimeType = mime, Buffer = Convert.FromBase64String(base64)
        };
    }

    private static async Task CheckLayoutAsync(IPage page)
    {
        var issues = await page.EvaluateAsync<string[]>("""
            () => {
                const issues = [], root = document.getElementById('demo-image-editor');
                if (root.scrollWidth > root.clientWidth + 1) issues.push('Showcase horizontal overflow');
                for (const id of ['image-editor-upload-card', 'image-editor-result-card', 'image-editor-tools-card']) {
                    const card = document.getElementById(id), rect = card.getBoundingClientRect();
                    if (rect.left < -1 || rect.right > innerWidth + 1 || card.scrollWidth > card.clientWidth + 1) issues.push(`${id} overflows`);
                }
                const editor = document.querySelector('.dnet-image-editor');
                if (editor) {
                    const rect = editor.getBoundingClientRect();
                    if (rect.left < -1 || rect.right > innerWidth + 1 || editor.scrollWidth > editor.clientWidth + 1) issues.push('Editor horizontal overflow');
                }
                return issues;
            }
            """);
        Assert.True(issues.Length == 0, string.Join(Environment.NewLine, issues));
    }

    private static async Task<(IPage Page, List<string> Errors)> OpenAsync(IBrowser browser, string theme, int width)
    {
        var page = await browser.NewPageAsync(new() { AcceptDownloads = true, ViewportSize = new() { Width = width, Height = 1000 } });
        var errors = new List<string>();
        page.PageError += (_, error) => errors.Add(error);
        page.Console += (_, message) =>
        {
            if (message.Type == "error" && message.Text.Contains("Microsoft.AspNetCore.Components", StringComparison.Ordinal)) errors.Add(message.Text);
        };
        var serializedTheme = System.Text.Json.JsonSerializer.Serialize(theme);
        await page.AddInitScriptAsync($"localStorage.setItem('dnet.theme', JSON.stringify({serializedTheme}));");
        await page.GotoAsync($"{BaseUrl}/ImageEditor");
        await Assertions.Expect(page.Locator("#demo-image-editor")).ToBeVisibleAsync();
        return (page, errors);
    }
}
