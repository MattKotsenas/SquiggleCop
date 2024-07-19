using System.Reflection;

using Microsoft.Build.Utilities.ProjectCreation;

namespace SquiggleCop.Tasks.Tests;

public abstract class TestBase : MSBuildTestBase, IDisposable
{
    protected TestBase()
    {
        TestRootPath = Directory.CreateDirectory(Path.Combine(Path.GetTempPath(), Path.GetRandomFileName())).FullName;

        Uri feed = new(new FileInfo(Assembly.GetExecutingAssembly().Location).Directory!.FullName);

        Repository = PackageRepository.Create(TestRootPath, new Uri("https://api.nuget.org/v3/index.json"), feed);
    }

    protected string TestRootPath { get; }
    protected PackageRepository Repository { get; }

    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Roslynator", "RCS1075:Avoid empty catch clause that catches System.Exception", Justification = "Test cleanup that can be impacted by other factors like AV software.")]
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

    protected string GetTempFileName(string? extension = null)
    {
        return Path.Combine(TestRootPath, $"{Path.GetRandomFileName()}{extension ?? string.Empty}");
    }

    protected string GetTempProjectPath(string? extension = null)
    {
        DirectoryInfo tempDirectoryInfo = Directory.CreateDirectory(Path.Combine(TestRootPath, Path.GetRandomFileName()));

        return Path.Combine(tempDirectoryInfo.FullName, $"{Path.GetRandomFileName()}{extension ?? string.Empty}");
    }
}
