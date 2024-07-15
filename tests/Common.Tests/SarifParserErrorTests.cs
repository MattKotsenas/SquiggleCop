namespace SquiggleCop.Common.Tests;

public class SarifParserErrorTests
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

    [Fact]
    public async Task V2LogsCreatedByAnOldCompilerThrowException()
    {
        await using Stream data = _reader.Read("Log.old.v2.sarif");

        await _parser.Invoking(p => p.ParseAsync(data))
            .Should()
            .ThrowAsync<InvalidDataException>()
            .Where(e => e.Message.Contains("SDK 8.0.100 or later"));
    }
}
