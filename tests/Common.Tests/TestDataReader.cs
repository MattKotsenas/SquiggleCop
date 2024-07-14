namespace SquiggleCop.Common.Tests;

internal class TestDataReader
{
    public Stream Read(string path)
    {
        var assembly = typeof(TestDataReader).Assembly;
        var resourceName = $"{assembly.GetName().Name}.TestData.{path}";

        Stream? stream = assembly.GetManifestResourceStream(resourceName);
        if (stream is null)
        {
            throw new FileNotFoundException($"Resource '{resourceName}' not found in assembly '{assembly.FullName}'");
        }

        return stream;
    }
}