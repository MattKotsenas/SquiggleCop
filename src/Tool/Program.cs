using Cocona;

CoconaApp app = CoconaApp.CreateBuilder().Build();

app.AddCommand("generate", () => Console.WriteLine("Hello, World!"));

await app.RunAsync().ConfigureAwait(false);