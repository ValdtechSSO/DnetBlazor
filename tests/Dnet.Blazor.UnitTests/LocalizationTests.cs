using System.Globalization;
using System.Resources;
using Dnet.Blazor.Localization;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Dnet.Blazor.UnitTests;

public sealed class LocalizationTests
{
    [Fact]
    public void Spanish_is_the_default_and_regional_English_uses_the_English_catalog()
    {
        using var provider = CreateProvider();
        using var scope = provider.CreateScope();
        var localizer = scope.ServiceProvider.GetRequiredService<IDnetLocalizer>();
        Assert.Equal("es-ES", localizer.Culture.Name);
        Assert.Equal("Aplicar", localizer["Common.Apply"]);
        localizer.SetCulture("en-GB");
        Assert.Equal("Apply", localizer["Common.Apply"]);
        localizer.SetCulture("es-MX");
        Assert.Equal("Aplicar", localizer["Common.Apply"]);
    }

    [Fact]
    public void Changing_one_user_does_not_change_another_or_the_ambient_cultures()
    {
        using var provider = CreateProvider();
        using var first = provider.CreateScope();
        using var second = provider.CreateScope();
        var english = first.ServiceProvider.GetRequiredService<IDnetLocalizer>();
        var spanish = second.ServiceProvider.GetRequiredService<IDnetLocalizer>();
        var culture = CultureInfo.CurrentCulture;
        var uiCulture = CultureInfo.CurrentUICulture;
        english.SetCulture("en-US");
        Assert.Equal("Apply", english["Common.Apply"]);
        Assert.Equal("Aplicar", spanish["Common.Apply"]);
        Assert.Same(culture, CultureInfo.CurrentCulture);
        Assert.Same(uiCulture, CultureInfo.CurrentUICulture);
    }

    [Fact]
    public void Host_resource_folder_does_not_change_library_resource_lookup()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddLocalization(options => options.ResourcesPath = "AppResources");
        services.AddDnetLocalization();
        using var provider = services.BuildServiceProvider();
        using var scope = provider.CreateScope();
        Assert.Equal("Aplicar", scope.ServiceProvider.GetRequiredService<IDnetLocalizer>()["Common.Apply"]);
    }

    [Fact]
    public void Applications_can_choose_English_initially_and_duplicate_registration_keeps_configuration()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddDnetLocalization(options => options.DefaultCulture = "en-US");
        services.AddDnetLocalization();
        using var provider = services.BuildServiceProvider();
        using var scope = provider.CreateScope();
        Assert.Equal("Apply", scope.ServiceProvider.GetRequiredService<IDnetLocalizer>()["Common.Apply"]);
    }

    [Fact]
    public void Notifications_only_fire_when_culture_changes_and_invalid_languages_keep_the_selection()
    {
        using var provider = CreateProvider();
        using var scope = provider.CreateScope();
        var localizer = scope.ServiceProvider.GetRequiredService<IDnetLocalizer>();
        var changes = 0;
        localizer.Changed += () => changes++;
        localizer.SetCulture("es-ES");
        localizer.SetCulture("en-US");
        localizer.SetCulture("en-US");
        Assert.Equal(1, changes);
        Assert.Throws<ArgumentException>(() => localizer.SetCulture("fr-FR"));
        Assert.Equal("en-US", localizer.Culture.Name);
        Assert.Equal("Missing.Resource", localizer["Missing.Resource"]);
    }

    [Fact]
    public void Spanish_and_English_catalogs_have_identical_keys_and_nonempty_translations()
    {
        var manager = new ResourceManager("Dnet.Blazor.Localization.DnetStrings", typeof(IDnetLocalizer).Assembly);
        var spanish = manager.GetResourceSet(CultureInfo.InvariantCulture, true, false)!;
        var englishManager = new ResourceManager("Dnet.Blazor.Localization.DnetEnglishStrings", typeof(IDnetLocalizer).Assembly);
        var english = englishManager.GetResourceSet(CultureInfo.InvariantCulture, true, false)!;
        var keys = spanish.Cast<System.Collections.DictionaryEntry>().Select(entry => (string)entry.Key).Order().ToArray();
        Assert.Equal(keys, english.Cast<System.Collections.DictionaryEntry>().Select(entry => (string)entry.Key).Order().ToArray());
        foreach (var key in keys)
        {
            Assert.False(string.IsNullOrWhiteSpace(spanish.GetString(key)), key);
            Assert.False(string.IsNullOrWhiteSpace(english.GetString(key)), key);
        }
    }

    private static ServiceProvider CreateProvider()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddDnetLocalization();
        return services.BuildServiceProvider();
    }
}
