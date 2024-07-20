namespace SquiggleCop.Common.Tests;

internal class TestDataReader
{
    public Stream Read(string path)
    {
        var assembly = typeof(TestDataReader).Assembly;
        var resourceName = $"{assembly.GetName().Name}.TestData.{path}";

        return assembly.GetManifestResourceStream(resourceName) ?? throw new FileNotFoundException($"Resource '{resourceName}' not found in assembly '{assembly.FullName}'");
    }
}