using System.Text.Json;
using SpecChat.Mcp.Tools;

namespace SpecChat.Mcp.Tests;

public class SpecParsingToolsTests
{
    private static string FixturePath(string filename)
    {
        string dir = AppContext.BaseDirectory;
        while (dir != null && !File.Exists(Path.Combine(dir, "SpecChat.slnx")))
            dir = Path.GetDirectoryName(dir)!;
        return Path.Combine(dir!, "..", "..", "..", "tests", "fixtures", filename);
    }

    [Fact]
    public void ParseSpec_BlazerHarness_ReturnsConstructs()
    {
        string path = FixturePath("blazor-harness.spec.md");
        string result = SpecParsingTools.ParseSpec(path);

        using var doc = JsonDocument.Parse(result);
        var root = doc.RootElement;

        // Should have constructs
        int constructCount = root.GetProperty("constructCount").GetInt32();
        Assert.True(constructCount > 0, "Expected at least one construct");

        // Should have a constructs array with entries
        var constructs = root.GetProperty("constructs");
        Assert.True(constructs.GetArrayLength() > 0);

        // The blazor-harness spec has complex visualization parameter bindings
        // that produce parse diagnostics. The key assertion is that constructs
        // were successfully extracted.
        Assert.True(constructCount >= 10,
            $"Expected at least 10 constructs but got {constructCount}");
    }

    [Fact]
    public void ParseSpec_NonexistentFile_ReturnsError()
    {
        string result = SpecParsingTools.ParseSpec("/nonexistent/path/to/file.spec.md");

        Assert.Contains("error", result);
        Assert.Contains("File not found", result);
    }

    [Fact]
    public void ListConstructs_BlazerHarness_CategorizedOutput()
    {
        string path = FixturePath("blazor-harness.spec.md");
        string result = SpecParsingTools.ListConstructs(path);

        using var doc = JsonDocument.Parse(result);
        var root = doc.RootElement;

        // Verify expected category arrays exist
        Assert.True(root.GetProperty("entities").GetArrayLength() > 0, "Expected entities");
        Assert.True(root.GetProperty("systems").GetArrayLength() > 0, "Expected systems");
        Assert.True(root.GetProperty("topologies").GetArrayLength() > 0, "Expected topologies");
        Assert.True(root.GetProperty("phases").GetArrayLength() > 0, "Expected phases");
        Assert.True(root.GetProperty("traces").GetArrayLength() > 0, "Expected traces");
        Assert.True(root.GetProperty("constraints").GetArrayLength() > 0, "Expected constraints");
    }

    [Fact]
    public void ParseSpecBlock_SimpleEntity_Parses()
    {
        string specBlock = "entity Foo { bar: string; }";
        string result = SpecParsingTools.ParseSpecBlock(specBlock);

        using var doc = JsonDocument.Parse(result);
        var root = doc.RootElement;

        // Should parse without errors
        bool hasErrors = root.GetProperty("hasErrors").GetBoolean();
        Assert.False(hasErrors, "Expected no parse errors for simple entity");

        // Should have one node
        var nodes = root.GetProperty("nodes");
        Assert.Equal(1, nodes.GetArrayLength());

        // Node should be an Entity type
        string nodeType = nodes[0].GetProperty("type").GetString()!;
        Assert.Equal("Entity", nodeType);
    }
}
