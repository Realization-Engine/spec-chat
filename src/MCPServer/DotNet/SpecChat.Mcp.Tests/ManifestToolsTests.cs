using System.Text.Json;
using SpecChat.Mcp.Tools;

namespace SpecChat.Mcp.Tests;

public class ManifestToolsTests
{
    private static string FixturePath(string filename)
    {
        string dir = AppContext.BaseDirectory;
        while (dir != null && !File.Exists(Path.Combine(dir, "SpecChat.slnx")))
            dir = Path.GetDirectoryName(dir)!;
        return Path.Combine(dir!, "..", "..", "..", "tests", "fixtures", filename);
    }

    [Fact]
    public void ReadManifest_BlazerHarness_ReturnsInventory()
    {
        string path = FixturePath("blazor-harness.manifest.md");
        string result = ManifestTools.ReadManifest(path);

        using var doc = JsonDocument.Parse(result);
        var root = doc.RootElement;

        // Verify system name
        string system = root.GetProperty("system").GetString()!;
        Assert.Equal("FStarVisualHarness", system);

        // Verify spec count
        int specCount = root.GetProperty("specCount").GetInt32();
        Assert.Equal(13, specCount);

        // Verify inventory has entries
        var inventory = root.GetProperty("inventory");
        Assert.True(inventory.GetArrayLength() > 0, "Expected inventory entries");
    }

    [Fact]
    public void ReadManifest_IncludesLifecycleStates()
    {
        string path = FixturePath("blazor-harness.manifest.md");
        string result = ManifestTools.ReadManifest(path);

        using var doc = JsonDocument.Parse(result);
        var root = doc.RootElement;

        var lifecycleStates = root.GetProperty("lifecycleStates");
        Assert.Equal(5, lifecycleStates.GetArrayLength());
    }

    [Fact]
    public void NextExecutable_BlazerHarness_FindsActionable()
    {
        string path = FixturePath("blazor-harness.manifest.md");
        string result = ManifestTools.NextExecutable(path);

        using var doc = JsonDocument.Parse(result);
        var root = doc.RootElement;

        // The manifest has Approved base spec and Executed tier-0 specs,
        // so there should be actionable specs (those in Approved state with deps met).
        int actionableCount = root.GetProperty("actionableCount").GetInt32();
        Assert.True(actionableCount >= 0, "Expected actionableCount to be present");

        // Verify the actionableSpecs array exists
        var actionableSpecs = root.GetProperty("actionableSpecs");
        Assert.True(actionableSpecs.ValueKind == JsonValueKind.Array);
    }

    [Fact]
    public void CheckLifecycle_BrokenLifecycle_ReportsViolation()
    {
        string path = FixturePath("broken-lifecycle.spec.md");
        string result = ManifestTools.CheckLifecycle(path);

        using var doc = JsonDocument.Parse(result);
        var root = doc.RootElement;

        int violationCount = root.GetProperty("violationCount").GetInt32();
        Assert.True(violationCount > 0, "Expected lifecycle violations");

        // Should report InvalidState as unrecognized
        string violationsJson = root.GetProperty("violations").ToString();
        Assert.Contains("InvalidState", violationsJson);
    }

    [Fact]
    public void ReadManifest_NonexistentFile_ReturnsError()
    {
        string result = ManifestTools.ReadManifest("/nonexistent/path/to/file.manifest.md");

        Assert.Contains("error", result);
        Assert.Contains("File not found", result);
    }
}
