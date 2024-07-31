using DiffPlex.DiffBuilder;
using DiffPlex.DiffBuilder.Model;

namespace SquiggleCop.Common;

/// <summary>
/// A class that can diff two baselines.
/// </summary>
/// <remarks>
/// The differ ignores line endings (i.e. `\n` vs `\r\n) to avoid issues related to git and line ending normalization.
/// </remarks>
public class BaselineDiffer
{
    private readonly SideBySideDiffBuilder _diffBuilder = SideBySideDiffBuilder.Instance;

    /// <summary>
    /// Diffs two baselines.
    /// </summary>
    /// <param name="expected">The expected text value.</param>
    /// <param name="actual">The actual text value.</param>
    /// <returns>A <see cref="BaselineDiff"/> that holds the results of the diff.</returns>
    public BaselineDiff Diff(string expected, string actual)
    {
        SideBySideDiffModel diff = _diffBuilder.BuildDiffModel(expected, actual, ignoreWhitespace: false, ignoreCase: false);
        return new BaselineDiff(diff);
    }
}
