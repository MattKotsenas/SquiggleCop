namespace SquiggleCop.Common;

/// <summary>
/// A class that writes a baseline to a file.
/// </summary>
/// <remarks>
/// This class has very little logic. The primary purpose is to ensure that files are always written with the same encoding.
/// </remarks>
public class BaselineWriter
{
    /// <summary>
    /// Writes the baseline to the specified path.
    /// </summary>
    /// <param name="path">The path to the file to write.</param>
    /// <param name="contents">The contents of the file.</param>
    public void Write(string path, string contents)
    {
        EnsureDirectory(path);
        File.WriteAllText(path, contents);
    }

#if NET6_0_OR_GREATER
    /// <summary>
    /// Writes the baseline to the specified path.
    /// </summary>
    /// <param name="path">The path to the file to write.</param>
    /// <param name="contents">The contents of the file.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    public async Task WriteAsync(string path, string contents)
    {
        EnsureDirectory(path);
        await File.WriteAllTextAsync(path, contents).ConfigureAwait(false);
    }
#endif

    private static void EnsureDirectory(string path)
    {
        string? parent = Directory.GetParent(path)?.FullName;
        if (parent is not null && !Directory.Exists(parent))
        {
            Directory.CreateDirectory(parent);
        }
    }
}
