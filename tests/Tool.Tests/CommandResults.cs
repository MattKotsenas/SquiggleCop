namespace SquiggleCop.Tool.Tests;

internal record class CommandResults(IReadOnlyCollection<string> Lines, int ExitCode);
