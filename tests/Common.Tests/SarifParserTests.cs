namespace SquiggleCop.Common.Tests;

public class SarifParserTests
{
    private readonly TestDataReader _reader = new();
    private readonly SarifParser _parser = new();

    [Fact]
    public async Task V1LogsThrowSpecificException()
    {
        await using Stream data = _reader.Read("Log.v1.sarif");

        await _parser.Invoking(p => p.ParseAsync(data))
            .Should()
            .ThrowAsync<InvalidDataException>()
            .Where(e => e.Message.Contains("SARIF v1"));
    }
}
