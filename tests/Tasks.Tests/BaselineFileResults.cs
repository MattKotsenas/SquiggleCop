namespace SquiggleCop.Tasks.Tests;

internal record struct BaselineFileResults(IEnumerable<BuildLogMessage> BuildLogMessages, string? BaselineFileContents);
