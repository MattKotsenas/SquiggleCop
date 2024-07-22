namespace SquiggleCop.Tasks.Tests;

internal record struct BaselineFileResults(
    bool BuildResult,
    IEnumerable<BuildLogMessage> BuildLogMessages,
    bool BaselineExists,
    bool BaselineWritten);
