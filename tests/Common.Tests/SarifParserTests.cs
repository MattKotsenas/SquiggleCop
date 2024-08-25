namespace SquiggleCop.Common.Tests;

public class SarifParserTests
{
    private readonly TestDataReader _reader = new(typeof(SarifParserTests).Namespace!);
    private readonly SarifParser _parser = new();
    private readonly Serializer _serializer = new();

    [Fact]
    public async Task SnapshotTest()
    {
        await using Stream data = _reader.Read("Log.v2.sarif");

        IReadOnlyCollection<DiagnosticConfig> configs = await _parser.ParseAsync(data);
        string baseline = _serializer.Serialize(configs);

        await Verify(baseline);
    }
}
