using Microsoft.Extensions.DependencyInjection;
using Cocona.Filters;

namespace SquiggleCop.Tool;

internal class CommandFilterFactory<TCommandFilterAttribute> : IFilterFactory where TCommandFilterAttribute : ICommandFilter
{
    public IFilterMetadata CreateInstance(IServiceProvider serviceProvider)
    {
        return serviceProvider.GetRequiredService<TCommandFilterAttribute>();
    }
}
