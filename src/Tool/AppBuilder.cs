using Cocona;
using Cocona.Application;
using Cocona.Builder;

using Microsoft.Extensions.DependencyInjection;

using Spectre.Console;

using SquiggleCop.Common;

namespace SquiggleCop.Tool;

/// <summary>
/// Configure and create the <see cref="CoconaApp"/>.
/// </summary>
public static class AppBuilder
{
    /// <summary>
    /// Create a new instance of <see cref="CoconaApp"/>.
    /// </summary>
    /// <returns>
    /// An instance of <see cref="CoconaApp"/> that's ready to run.
    /// </returns>
    public static CoconaApp Create() => Create(_ => { });

    /// <summary>
    /// Create a new instance of <see cref="CoconaApp"/>.
    /// </summary>
    /// <param name="configure">
    /// An <see cref="Action"/> to allow additional configuration.
    /// Intented to allow overriding defaults for testing.
    /// </param>
    /// <returns>
    /// An instance of <see cref="CoconaApp"/> that's ready to run.
    /// </returns>
    public static CoconaApp Create(Action<CoconaAppBuilder> configure)
    {
        CoconaAppBuilder builder = CoconaApp.CreateBuilder();
        builder.Services.AddSingleton<ICoconaApplicationMetadataProvider, DotnetToolApplicationMetadataProvider>();
        builder.Services.AddSingleton<SpectreErrorHandlerFilterAttribute>();
        builder.Services.AddSingleton<SpectreParameterBindExceptionFilterAttribute>();
        builder.Services.AddSingleton(AnsiConsole.Console);
        builder.Services.AddTransient<SarifParser>();
        builder.Services.AddSingleton<Serializer>();
        builder.Services.AddSingleton<BaselineDiffer>();
        builder.Services.AddSingleton<BaselineWriter>();

        configure(builder);

        CoconaApp app = builder.Build();

        app.UseFilter(new CommandFilterFactory<SpectreErrorHandlerFilterAttribute>());
        app.UseFilter(new CommandFilterFactory<SpectreParameterBindExceptionFilterAttribute>());

        app.AddCommand("generate", GenerateAsync);

        return app;
    }

    private static async Task<int> GenerateAsync(
        SarifParser parser,
        Serializer serializer,
        BaselineWriter writer,
        IAnsiConsole console,
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

        try
        {
            IReadOnlyCollection<DiagnosticConfig> configs;
            Stream stream = File.OpenRead(sarif);
            await using (stream.ConfigureAwait(false))
            {
                configs = await parser.ParseAsync(stream).ConfigureAwait(false);
            }

            string newBaseline = serializer.Serialize(configs);
            await writer.WriteAsync(output, newBaseline).ConfigureAwait(false);
        }
        catch (UnsupportedVersionException ex)
        {
            throw new SarifInvalidException($"SARIF file is invalid. {ex.Message}", ex);
        }

        console.MarkupLine($"[green]Baseline generated[/] @ {output}");

        return ExitCodes.Success;
    }

    private static string ValidateOutputPath(string? path)
    {
        path ??= Directory.GetCurrentDirectory();

        if (!Path.HasExtension(path))
        {
            // Doesn't appear to be a file, so append the default filename
            path = Path.Combine(path, "SquiggleCop.Baseline.yaml");
        }

        return path;
    }
}
