namespace SquiggleCop.Common.Tests;

public class SquiggleBaselineFileTests
{
    // TODO: Use MSBuildProjectCreator with a custom implementation of CoreCompile (or similar)
    // to fake the compilation and set an output file.

    [Theory]
    [CombinatorialData]
    public void NoBaselineFile(bool automaticBaseline, bool explicitFile)
    {
        // TODO: Implement; when auto is on, create the file
        // TODO: Implement; when auto is off, do what? Give message to run tool?
    }

    [Theory]
    [CombinatorialData]
    public void BaselineUpToDate(bool automaticBaseline, bool explicitFile)
    {
        // TODO: Implement; in both cases, ensure to not set the modified date
    }

    [Theory]
    [CombinatorialData]
    public void BaselineOutOfDate(bool automaticBaseline, bool explicitFile)
    {
        // TODO: Implement; when auto is on, update the file
        // TODO: Implement; when auto is off, pop Verify? Give message to run tool?
    }
}
