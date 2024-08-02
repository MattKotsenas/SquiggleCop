using DiffPlex.DiffBuilder.Model;

using Spectre.Console;

using SquiggleCop.Common;

namespace SquiggleCop.Tool.Rendering;

internal class DiffRenderer
{
    private readonly IAnsiConsole _console;

    public DiffRenderer(IAnsiConsole console)
    {
        _console = console;
    }

    public void Render(BaselineDiff diff, int context)
    {
        int i = 0;
        while (i < diff.Lines.Count)
        {
            if (diff.Lines[i].Type == ChangeType.Unchanged)
            {
                i++;
                continue;
            }

            int begin = Math.Max(0, i - context + 1);
            int end = Math.Min(diff.Lines.Count - 1, diff.Lines.SearchForEndOfDiffBlock(i) + context - 1);

            for (int j = begin; j < end; j++)
            {
                RenderLine(diff.Lines[j]);
            }

            _console.WriteLine();
            i = end + 2;
        }
    }

    private void RenderLine(DiffPiece line)
    {
        if (line.Type == ChangeType.Inserted)
        {
            _console.MarkupLine($"[green]+{line.Text.EscapeMarkup()}[/]");
        }
        else if (line.Type == ChangeType.Deleted)
        {
            _console.MarkupLine($"[red]-{line.Text.EscapeMarkup()}[/]");
        }
        else
        {
            _console.MarkupLine($"[grey]{line.Text.EscapeMarkup()}[/]");
        }
    }
}
