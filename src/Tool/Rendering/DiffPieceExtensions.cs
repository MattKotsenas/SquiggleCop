using DiffPlex.DiffBuilder.Model;

namespace SquiggleCop.Tool.Rendering;

internal static class DiffPieceExtensions
{
    public static int SearchForEndOfDiffBlock(this IReadOnlyList<DiffPiece> lines, int index)
    {
        int i = index;
        while (i < lines.Count && lines[i].Type != ChangeType.Unchanged)
        {
            i++;
        }

        return i;
    }
}
