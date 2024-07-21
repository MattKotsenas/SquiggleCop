namespace SquiggleCop.Tasks.Tests;

// TODO: Move all of the test asserts into this type so that Verify can see them all at once
// TODO: Do we care about the contents for baselining tests?

internal record struct BaselineFileResults(IEnumerable<BuildLogMessage> BuildLogMessages, string? BaselineFileContents);
