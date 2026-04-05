using SpecChat.Language;

namespace SpecChat.Language.Tests;

public class ManifestParserTests
{
    private static string FixturePath(string filename)
    {
        string dir = AppContext.BaseDirectory;
        while (dir != null && !File.Exists(Path.Combine(dir, "SpecChat.slnx")))
            dir = Path.GetDirectoryName(dir)!;
        return Path.Combine(dir!, "..", "..", "..", "tests", "fixtures", filename);
    }

    [Fact]
    public void Parse_BlazerHarness_ExtractsSystem()
    {
        string content = File.ReadAllText(FixturePath("blazor-harness.manifest.md"));
        var parser = new ManifestParser();
        var manifest = parser.Parse(content, "blazor-harness.manifest.md");

        Assert.Equal("FStarVisualHarness", manifest.System);
        Assert.Equal("blazor-harness.spec.md", manifest.BaseSpec);
        Assert.Equal("net10.0", manifest.Target);
        Assert.Equal(13, manifest.SpecCount);
    }

    [Fact]
    public void Parse_BlazerHarness_ExtractsInventory()
    {
        string content = File.ReadAllText(FixturePath("blazor-harness.manifest.md"));
        var parser = new ManifestParser();
        var manifest = parser.Parse(content, "blazor-harness.manifest.md");

        Assert.Equal(13, manifest.Inventory.Count);

        // Verify first and last entries
        Assert.Equal("blazor-harness.spec.md", manifest.Inventory[0].Filename);
        Assert.Equal("Base", manifest.Inventory[0].Type);
        Assert.Equal("Approved", manifest.Inventory[0].State);

        Assert.Equal("bug-timeline-crosshair-sync.spec.md", manifest.Inventory[12].Filename);
        Assert.Equal("Bug", manifest.Inventory[12].Type);
    }

    [Fact]
    public void Parse_BlazerHarness_ExtractsExecutionOrder()
    {
        string content = File.ReadAllText(FixturePath("blazor-harness.manifest.md"));
        var parser = new ManifestParser();
        var manifest = parser.Parse(content, "blazor-harness.manifest.md");

        Assert.Equal(5, manifest.ExecutionOrder.Count);
        Assert.Equal(0, manifest.ExecutionOrder[0].TierNumber);
        Assert.Equal(1, manifest.ExecutionOrder[1].TierNumber);
        Assert.Equal(2, manifest.ExecutionOrder[2].TierNumber);
        Assert.Equal(3, manifest.ExecutionOrder[3].TierNumber);
        Assert.Equal(4, manifest.ExecutionOrder[4].TierNumber);
    }

    [Fact]
    public void Parse_BrokenLifecycle_InvalidState()
    {
        string content = File.ReadAllText(FixturePath("broken-lifecycle.spec.md"));
        var diagnostics = new DiagnosticBag();
        var parser = new ManifestParser(diagnostics);
        parser.Parse(content, "broken-lifecycle.spec.md");

        Assert.True(diagnostics.HasErrors);
        Assert.Contains(diagnostics.Diagnostics,
            d => d.Message.Contains("InvalidState") && d.Severity == DiagnosticSeverity.Error);
    }

    [Fact]
    public void Parse_BrokenLifecycle_MissingDependency()
    {
        string content = File.ReadAllText(FixturePath("broken-lifecycle.spec.md"));
        var diagnostics = new DiagnosticBag();
        var parser = new ManifestParser(diagnostics);
        parser.Parse(content, "broken-lifecycle.spec.md");

        Assert.Contains(diagnostics.Diagnostics,
            d => d.Message.Contains("nonexistent-spec.spec.md"));
    }
}
