using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace TabTip.Avalonia;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddTabTipProviders(this IServiceCollection services)
    {
        services.TryAddSingleton<ITabTipFactory>();

        return services;
    }
}