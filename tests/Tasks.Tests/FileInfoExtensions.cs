namespace SquiggleCop.Tasks.Tests;

internal static class FileInfoExtensions
{
    public static bool WasWritten(this FileInfo fileInfo, DateTime anchor)
    {
        return fileInfo.LastWriteTimeUtc.IsCloseTo(anchor, TimeSpan.FromHours(1));
    }

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
        catch (DirectoryNotFoundException)
        {
            return defaultValue;
        }
    }
}
