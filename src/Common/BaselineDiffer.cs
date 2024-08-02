using DiffPlex;
using DiffPlex.Chunkers;
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
    private readonly InlineDiffBuilder _diffBuilder = InlineDiffBuilder.Instance;
    private readonly IChunker _chunker = new LineChunker();

    /// <summary>
    /// Diffs two baselines.
    /// </summary>
    /// <param name="expected">The expected text value.</param>
    /// <param name="actual">The actual text value.</param>
    /// <returns>A <see cref="BaselineDiff"/> that holds the results of the diff.</returns>
    public BaselineDiff Diff(string expected, string actual)
    {
        DiffPaneModel diff = _diffBuilder.BuildDiffModel(expected, actual, ignoreWhitespace: false, ignoreCase: false, _chunker);
        return new BaselineDiff(diff);
    }
}
