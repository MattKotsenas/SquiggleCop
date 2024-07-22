using Cocona;
using Cocona.Application;
using Cocona.Builder;

using Microsoft.Extensions.DependencyInjection;

using Spectre.Console;

using SquiggleCop.Common;
using SquiggleCop.Tool;

using YamlDotNet.Serialization.NamingConventions;
using YamlDotNet.Serialization;

CoconaAppBuilder builder = CoconaApp.CreateBuilder();
builder.Services.AddSingleton<ICoconaApplicationMetadataProvider, DotnetToolApplicationMetadataProvider>();
builder.Services.AddSingleton<SpectreErrorHandlerFilterAttribute>();
builder.Services.AddSingleton<SpectreParameterBindExceptionFilterAttribute>();
builder.Services.AddSingleton(AnsiConsole.Console);
builder.Services.AddTransient<SarifParser>();

CoconaApp app = builder.Build();

app.UseFilter(new CommandFilterFactory<SpectreErrorHandlerFilterAttribute>());
app.UseFilter(new CommandFilterFactory<SpectreParameterBindExceptionFilterAttribute>());

app.AddCommand("generate", GenerateAsync);

await app.RunAsync(app.Lifetime.ApplicationStopping).ConfigureAwait(false);

static async Task<int> GenerateAsync(
    SarifParser parser,
    [Option('a', Description = "Automatically update baseline if necessary")] bool autoBaseline,
    [Argument(Description = "The SARIF log to generate a baseline for")] string sarif,
    [Option('o', Description = "The output path for the baseline file")] string? output)
{
    if (!File.Exists(sarif))
    {
        throw new SarifNotFoundException(sarif);
    }

    output = ValidateOutputPath(output);

    if (!autoBaseline)
    {
        throw new NotImplementedException("Manual baseline creation is not yet implemented. Use `--auto-baseline` and your source control system to review changes for now.");
    }

    IReadOnlyCollection<DiagnosticConfig> configs;
    Stream stream = File.OpenRead(sarif);
    await using (stream.ConfigureAwait(false))
    {
        configs = await parser.ParseAsync(stream).ConfigureAwait(false);
    }

    // TODO: Extract into common class
    ISerializer serializer = new SerializerBuilder()
        .WithNamingConvention(PascalCaseNamingConvention.Instance)
        .Build();

    string newBaseline = serializer.Serialize(configs);
    await File.WriteAllTextAsync(output, newBaseline).ConfigureAwait(false);

    return ExitCodes.Success;
}

static string ValidateOutputPath(string? path)
{
    path ??= Directory.GetCurrentDirectory();

    if (!Path.HasExtension(path))
    {
        // Doesn't appear to be a file, so append the default filename
        path = Path.Combine(path, "SquiggleCop.Baseline.yaml");
    }

    return path;
}
