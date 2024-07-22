using System.Reflection;

namespace SquiggleCop.Tool.Tests;

public class PackageTests
{
    private static readonly FileInfo Package =
        new FileInfo(Assembly.GetExecutingAssembly().Location)
            .Directory!
            .GetFiles("SquiggleCop.Tool.*.nupkg")
            .OrderByDescending(f => f.LastWriteTimeUtc)
            .First();

    [Fact]
    public async Task AssertPackageContents()
    {
        await VerifyFile(Package).ScrubNuspec();
    }
}
