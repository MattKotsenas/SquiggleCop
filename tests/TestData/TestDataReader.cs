using System.Reflection;

namespace SquiggleCop.Common.Tests;

internal sealed class TestDataReader
{
    private readonly string _namespace;

    public TestDataReader(string @namespace)
    {
        _namespace = @namespace;
    }

    public Stream Read(string path)
    {
        Assembly assembly = typeof(TestDataReader).Assembly;
        string resourceName = $"{_namespace}.{path}";

        return assembly.GetManifestResourceStream(resourceName) ?? throw new FileNotFoundException($"Resource '{resourceName}' not found in assembly '{assembly.FullName}'");
    }
}