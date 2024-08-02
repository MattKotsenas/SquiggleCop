using Spectre.Console;

using SquiggleCop.Common;

namespace SquiggleCop.Tool.Rendering;

internal sealed class ReportRenderer
{
    private readonly IAnsiConsole _console;
    private readonly DiffRenderer _diffRenderer;

    public ReportRenderer(IAnsiConsole console, DiffRenderer diffRenderer)
    {
        _console = console;
        _diffRenderer = diffRenderer;
    }

    public void Render(BaselineDiff diff, bool showDiff, int context)
    {
        if (diff.HasDifferences)
        {
            _console.Markup("[yellow]Baseline is different[/]");

            if (!showDiff)
            {
                _console.MarkupLine("[dim] (auto baselining...)[/]");
            }
            else
            {
                _console.WriteLine();
            }
        }
        else
        {
            _console.MarkupLine("[green]Baseline is up-to-date[/]");
        }
        _console.WriteLine();

        if (showDiff)
        {
            _diffRenderer.Render(diff, context);
        }
    }
}
