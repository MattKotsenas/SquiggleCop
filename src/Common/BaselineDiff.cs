using DiffPlex.DiffBuilder.Model;

namespace SquiggleCop.Common;

/// <summary>
/// A record that represents the differences between two baselines.
/// </summary>
public record class BaselineDiff
{
    /// <summary>
    /// Creates a new instance of <see cref="BaselineDiff"/>.
    /// </summary>
    /// <param name="model">
    /// The <see cref="SideBySideDiffModel"/> model that contains the differences.
    /// </param>
    public BaselineDiff(SideBySideDiffModel model)
    {
        HasDifferences = model.OldText.HasDifferences || model.NewText.HasDifferences;
    }

    /// <summary>
    /// <see langword="true"/> if the two baselines have differences; otherwise, <see langword="false"/>.
    /// </summary>
    public bool HasDifferences { get; }
}
