using System.Globalization;

using Spectre.Console;
using Cocona.Filters;
using Cocona.Command.Binder;

namespace SquiggleCop.Tool;

internal class SpectreParameterBindExceptionFilterAttribute : CommandFilterAttribute
{
    private readonly IAnsiConsole _console;

    public SpectreParameterBindExceptionFilterAttribute(IAnsiConsole console)
    {
        _console = console;
    }

    public override async ValueTask<int> OnCommandExecutionAsync(CoconaCommandExecutingContext ctx, CommandExecutionDelegate next)
    {
        try
        {
            return await next(ctx).ConfigureAwait(false);
        }
        catch (ParameterBinderException paramEx) when (paramEx.Result == ParameterBinderResult.InsufficientArgument)
        {
            _console.MarkupLine(CultureInfo.InvariantCulture, "[red]Error:[/] Argument '{0}' is required. See '--help' for usage.", paramEx.Argument!.Name);
        }
        catch (ParameterBinderException paramEx) when (paramEx.Result == ParameterBinderResult.InsufficientOption)
        {
            _console.MarkupLine(CultureInfo.InvariantCulture, "[red]Error:[/] Option '{0}' is required. See '--help' for usage.", paramEx.Option!.Name);
        }
        catch (ParameterBinderException paramEx) when (paramEx.Result == ParameterBinderResult.InsufficientOptionValue)
        {
            _console.MarkupLine(CultureInfo.InvariantCulture, "[red]Error:[/] Option '{0}' requires a value. See '--help' for usage.", paramEx.Option!.Name);
        }
        catch (ParameterBinderException paramEx) when (paramEx.Result == ParameterBinderResult.TypeNotSupported || paramEx.Result == ParameterBinderResult.ValidationFailed)
        {
            _console.MarkupLine(CultureInfo.InvariantCulture, "[red]Error:[/] {0}", paramEx.Message);
        }

        return ExitCodes.UnknownError;
    }
}
