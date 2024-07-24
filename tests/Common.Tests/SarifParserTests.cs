namespace SquiggleCop.Common.Tests;

public class SarifParserTests
{
    private readonly TestDataReader _reader = new();
    private readonly SarifParser _parser = new();

    [Fact]
    public async Task SnapshotTest()
    {
        await using Stream data = _reader.Read("Log.v2.sarif");

        IReadOnlyCollection<DiagnosticConfig> configs = await _parser.ParseAsync(data);

        await Verify(configs); // TODO: This should assert the YAML instead
    }
}
