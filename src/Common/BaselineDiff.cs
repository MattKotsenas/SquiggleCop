using DiffPlex.DiffBuilder.Model;

namespace SquiggleCop.Common;

/// <summary>
/// A class that represents the differences between two baselines.
/// </summary>
public class BaselineDiff
{
    private readonly DiffPaneModel _model;

    /// <summary>
    /// Creates a new instance of <see cref="BaselineDiff"/>.
    /// </summary>
    /// <param name="model">
    /// The <see cref="DiffPaneModel"/> model that contains the differences.
    /// </param>
    public BaselineDiff(DiffPaneModel model)
    {
        _model = model;
    }

    /// <summary>
    /// <see langword="true"/> if the two baselines have differences; otherwise, <see langword="false"/>.
    /// </summary>
    public bool HasDifferences => _model.HasDifferences;

    /// <summary>
    /// The lines that represent the differences between the two baselines.
    /// </summary>
    public IReadOnlyList<DiffPiece> Lines => _model.Lines;
}
