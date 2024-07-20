using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

using Microsoft.CodeAnalysis.Sarif;

namespace SquiggleCop.Common;

internal static class Guard
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    [SuppressMessage("Roslynator", "RCS1256:Invalid argument null check", Justification = "Guard class specifically for null checks")]
    public static void ThrowIfNull([NotNull] object? argument, [CallerArgumentExpression(nameof(argument))] string? paramName = null)
    {
#if NET70_OR_GREATER
        ArgumentNullException.ThrowIfNull(argument);
#else
        if (argument is null) { throw new ArgumentNullException(paramName); }
#endif
    }
}
