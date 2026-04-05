using System.Text.Json;
using SpecChat.Mcp.Tools;

namespace SpecChat.Mcp.Tests;

public class RealizationToolsTests
{
    private static string FixturePath(string filename)
    {
        string dir = AppContext.BaseDirectory;
        while (dir != null && !File.Exists(Path.Combine(dir, "SpecChat.slnx")))
            dir = Path.GetDirectoryName(dir)!;
        return Path.Combine(dir!, "..", "..", "..", "tests", "fixtures", filename);
    }

    [Fact]
    public void ExtractContracts_BlazerHarness_FindsContracts()
    {
        string path = FixturePath("blazor-harness.spec.md");
        string result = RealizationTools.ExtractContracts(path);

        using var doc = JsonDocument.Parse(result);
        var root = doc.RootElement;

        int contractCount = root.GetProperty("contractCount").GetInt32();
        Assert.True(contractCount > 0, "Expected at least one contract");
    }

    [Fact]
    public void ExtractEntities_BlazerHarness_FindsEntities()
    {
        string path = FixturePath("blazor-harness.spec.md");
        string result = RealizationTools.ExtractEntities(path);

        using var doc = JsonDocument.Parse(result);
        var root = doc.RootElement;

        int entityCount = root.GetProperty("entityCount").GetInt32();
        Assert.True(entityCount > 0, "Expected at least one entity");

        // Verify specific entity names are present
        string entitiesJson = root.GetProperty("entities").ToString();
        Assert.Contains("ChartSeries", entitiesJson);
        Assert.Contains("DataPoint", entitiesJson);
        Assert.Contains("BarItem", entitiesJson);
    }

    [Fact]
    public void ExtractPhaseGates_Scaffolding_ReturnsGates()
    {
        string path = FixturePath("blazor-harness.spec.md");
        string result = RealizationTools.ExtractPhaseGates(path, "Scaffolding");

        using var doc = JsonDocument.Parse(result);
        var root = doc.RootElement;

        // Verify phase name
        string phase = root.GetProperty("phase").GetString()!;
        Assert.Equal("Scaffolding", phase);

        // Verify gates exist with commands
        int gateCount = root.GetProperty("gateCount").GetInt32();
        Assert.True(gateCount > 0, "Expected at least one gate");

        var gates = root.GetProperty("gates");
        for (int i = 0; i < gates.GetArrayLength(); i++)
        {
            var gate = gates[i];
            Assert.True(gate.TryGetProperty("command", out _), "Each gate should have a command");
            Assert.True(gate.TryGetProperty("name", out _), "Each gate should have a name");
        }
    }

    [Fact]
    public void GenerateScaffold_BlazerHarness_ProducesFileTree()
    {
        string path = FixturePath("blazor-harness.spec.md");
        string result = RealizationTools.GenerateScaffold(path);

        using var doc = JsonDocument.Parse(result);
        var root = doc.RootElement;

        var scaffold = root.GetProperty("scaffold");

        // Verify system name
        string systemName = scaffold.GetProperty("systemName").GetString()!;
        Assert.Equal("FStarVisualHarness", systemName);

        // Verify projects array has entries (authored components)
        var projects = scaffold.GetProperty("projects");
        Assert.True(projects.GetArrayLength() > 0, "Expected authored component projects");

        // Verify file tree
        var fileTree = scaffold.GetProperty("fileTree");
        Assert.True(fileTree.GetArrayLength() > 0, "Expected file tree entries");
    }
}
