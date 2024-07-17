using System.Reflection;

namespace SquiggleCop.Tasks.Tests;

public class PackageTests
{
    private static readonly FileInfo Package =
        new FileInfo(Assembly.GetExecutingAssembly().Location)
            .Directory!
            .GetFiles("SquiggleCop*.nupkg")
            .OrderByDescending(f => f.LastWriteTimeUtc)
            .First();

    [Fact]
    public async Task AssertPackageContents()
    {
        await VerifyFile(Package).ScrubNuspec();
    }
}
