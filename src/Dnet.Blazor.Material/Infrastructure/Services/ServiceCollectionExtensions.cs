using Dnet.Blazor.Material.Components.FormField;
using Microsoft.Extensions.DependencyInjection;

namespace Dnet.Blazor.Material.Infrastructure.Services;

public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registers the services required by Dnet.Blazor.Material components.
    /// </summary>
    public static IServiceCollection AddDnetBlazorMaterial(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);
        services.AddTransient<IFormEventService, FormEventService>();
        return services;
    }
}
