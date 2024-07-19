namespace SquiggleCop.Tasks.Tests;

internal static class FileInfoExtensions
{
    public static async Task<string?> ReadAllTextAsyncOrDefault(this FileInfo file, string? defaultValue = "")
    {
        try
        {
            return await File.ReadAllTextAsync(file.FullName).ConfigureAwait(false);
        }
        catch (FileNotFoundException)
        {
            return defaultValue;
        }
    }
}
