using System.Diagnostics.CodeAnalysis;
using System.Reflection;

using Microsoft.Build.Utilities.ProjectCreation;

using SquiggleCop.Common.Tests;

namespace SquiggleCop.Tool.Tests;

public abstract class TestBase : IDisposable
{
    private readonly TestDataReader _testDataReader = new();

    protected string TestRootPath { get; }
    protected PackageRepository Repository { get; }

    protected TestBase()
    {
        TestRootPath = Directory.CreateDirectory(Path.Combine(Path.GetTempPath(), Path.GetRandomFileName())).FullName;

        Uri feed = new(new FileInfo(Assembly.GetExecutingAssembly().Location).Directory!.FullName);

        Repository = PackageRepository.Create(TestRootPath, /*new Uri("https://api.nuget.org/v3/index.json"),*/ feed);
    }

    protected async Task CopyTestData(string testDataName)
    {
        Stream fileStream = File.Create(Path.Combine(TestRootPath, testDataName));
        await using (fileStream.ConfigureAwait(false))
        {
            Stream dataStream = _testDataReader.Read(testDataName);
            await using (dataStream.ConfigureAwait(false))
            {
                await dataStream.CopyToAsync(fileStream).ConfigureAwait(false);
            }
        }
    }

    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

    [SuppressMessage("Roslynator", "RCS1075:Avoid empty catch clause that catches System.Exception", Justification = "Test cleanup that can be impacted by other factors like AV software.")]
    protected virtual void Dispose(bool disposing)
    {
        if (disposing)
        {
            Repository.Dispose();
        }

        if (Directory.Exists(TestRootPath))
        {
            try
            {
                Directory.Delete(TestRootPath, recursive: true);
            }
            catch (Exception)
            {
                // Ignored
            }
        }
    }
}
