using System.Reflection;

namespace SquiggleCop.Common.Tests;

internal class TestDataReader
{
    private readonly string _namespace;

    public TestDataReader()
    {
        Assembly assembly = typeof(TestDataReader).Assembly;
        _namespace = assembly.GetName().Name ?? throw new InvalidOperationException("Assembly name not found");
    }

    public TestDataReader(string @namespace)
    {
        _namespace = @namespace;
    }

    public Stream Read(string path)
    {
        Assembly assembly = typeof(TestDataReader).Assembly;
        string resourceName = $"{_namespace}.TestData.{path}";

        return assembly.GetManifestResourceStream(resourceName) ?? throw new FileNotFoundException($"Resource '{resourceName}' not found in assembly '{assembly.FullName}'");
    }
}