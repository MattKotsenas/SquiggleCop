using System.Diagnostics.CodeAnalysis;

using Microsoft.CodeAnalysis.Sarif;

namespace SquiggleCop.Common;

internal static class ToolComponentExtensions
{
    public static bool IsCSharpCompiler(this ToolComponent? tool)
    {
        return string.Equals(tool?.Name, "Microsoft (R) Visual C# Compiler", StringComparison.Ordinal);
    }

    public static bool TryGetVersion(this ToolComponent? tool, [NotNullWhen(true)] out Version? version)
    {
        return Version.TryParse(tool?.SemanticVersion, out version);
    }
}
