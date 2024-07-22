using Spectre.Console;
using Cocona.Filters;

namespace SquiggleCop.Tool;

internal class SpectreErrorHandlerFilterAttribute : CommandFilterAttribute
{
    private readonly IAnsiConsole _console;

    public SpectreErrorHandlerFilterAttribute(IAnsiConsole console)
    {
        _console = console;
    }

    public override async ValueTask<int> OnCommandExecutionAsync(CoconaCommandExecutingContext ctx, CommandExecutionDelegate next)
    {
        try
        {
            return await next(ctx).ConfigureAwait(false);
        }
        catch (ExitCodeException exc)
        {
            _console.MarkupLine($"[red]Error:[/] {exc.Message}");
            return exc.ExitCode;
        }
        catch (Exception e)
        {
            _console.WriteException(e, ExceptionFormats.ShortenEverything);
            return ExitCodes.UnknownError;
        }
    }
}
