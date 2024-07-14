using Newtonsoft.Json;

namespace SquiggleCop.Common.Tests;

public class UnitTest1
{
    private readonly TestDataReader _reader = new TestDataReader();
    private readonly SarifParser _parser = new SarifParser();

    [Fact]
    public async Task Test1()
    {
        await using Stream data = _reader.Read("Log.v2.sarif");

        IReadOnlyCollection<DiagnosticConfig> configs = await _parser.ParseAsync(data);

        // TODO: Add tests
        configs.Should().NotBeEmpty();
    }
}
