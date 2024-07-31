namespace SquiggleCop.Common.Tests;

public class BaselineDifferTests
{
    private readonly BaselineDiffer _differ = new();

    [Fact]
    public void DiffEqual()
    {
        const string expected = "- {Id: CA1000, Title: Do not declare static members on generic types, Category: Design, DefaultSeverity: Note, IsEnabledByDefault: true, EffectiveSeverities: [Note], IsEverSuppressed: true}\n";
        const string actual = expected;

        _differ.Diff(expected, actual).HasDifferences.Should().BeFalse();
    }

    [Fact]
    public void DiffUnequal()
    {
        const string expected = "- {Id: CA1000, Title: Do not declare static members on generic types, Category: Design, DefaultSeverity: Note, IsEnabledByDefault: true, EffectiveSeverities: [Note], IsEverSuppressed: true}\n";
        const string actual = "- {Id: CA1000, Title: Do not declare static members on generic types, Category: Design, DefaultSeverity: Note, IsEnabledByDefault: true, EffectiveSeverities: [Warning], IsEverSuppressed: false}\n- { Id: CA1001, Title: Types that own disposable fields should be disposable, Category: Design, DefaultSeverity: Note, IsEnabledByDefault: true, EffectiveSeverities: [Note], IsEverSuppressed: true}\n";

        _differ.Diff(expected, actual).HasDifferences.Should().BeTrue();
    }

    [Fact]
    public void DiffNewlines()
    {
        const string expected = "- {Id: CA1000, Title: Do not declare static members on generic types, Category: Design, DefaultSeverity: Note, IsEnabledByDefault: true, EffectiveSeverities: [Note], IsEverSuppressed: true}\n";
        const string actual = "- {Id: CA1000, Title: Do not declare static members on generic types, Category: Design, DefaultSeverity: Note, IsEnabledByDefault: true, EffectiveSeverities: [Note], IsEverSuppressed: true}\r\n";

        _differ.Diff(expected, actual).HasDifferences.Should().BeFalse();
    }
}
