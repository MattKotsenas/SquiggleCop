using Microsoft.Extensions.DependencyInjection;

namespace SquiggleCop.Tool.Rendering;

internal static class ServiceCollectionExtensions
{
    public static IServiceCollection AddRenderers(this IServiceCollection services)
    {
        services.AddSingleton<DiffRenderer>();
        services.AddSingleton<ReportRenderer>();

        return services;
    }
}
