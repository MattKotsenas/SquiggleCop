using Cocona;
using Cocona.Builder;

using Microsoft.Extensions.DependencyInjection;

using Spectre.Console;

CoconaAppBuilder builder = CoconaApp.CreateBuilder();

builder.Services.AddSingleton(AnsiConsole.Console);

CoconaApp app = builder.Build();

app.AddCommand("generate", (IAnsiConsole console) => console.MarkupLine("Hello, World! :globe_showing_americas:"));

await app.RunAsync().ConfigureAwait(false);