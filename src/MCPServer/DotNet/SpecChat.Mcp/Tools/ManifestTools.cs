using System.ComponentModel;
using System.Text.Json;
using ModelContextProtocol.Server;
using SpecChat.Language;

namespace SpecChat.Mcp.Tools;

[McpServerToolType]
public static class ManifestTools
{
    [McpServerTool, Description("Parse a manifest file and return the full spec inventory with lifecycle states.")]
    public static string ReadManifest(
        [Description("Absolute path to the .manifest.md file")] string filePath)
    {
        if (!File.Exists(filePath))
            return SpecFileHelper.ErrorJson($"File not found: {filePath}");

        try
        {
            string content = File.ReadAllText(filePath);
            var diagnostics = new DiagnosticBag();
            var parser = new ManifestParser(diagnostics);
            var manifest = parser.Parse(content, filePath);

            var inventory = new List<Dictionary<string, object>>();
            foreach (var entry in manifest.Inventory)
            {
                inventory.Add(new Dictionary<string, object>
                {
                    ["filename"] = entry.Filename,
                    ["type"] = entry.Type,
                    ["state"] = entry.State,
                    ["tier"] = entry.Tier,
                    ["dependencies"] = entry.Dependencies ?? "",
                });
            }

            var executionOrder = new List<Dictionary<string, object>>();
            foreach (var tier in manifest.ExecutionOrder)
            {
                executionOrder.Add(new Dictionary<string, object>
                {
                    ["tierNumber"] = tier.TierNumber,
                    ["description"] = tier.Description,
                    ["specs"] = tier.Specs,
                });
            }

            var lifecycleStates = new List<Dictionary<string, object>>();
            foreach (var ls in manifest.LifecycleStates)
            {
                lifecycleStates.Add(new Dictionary<string, object>
                {
                    ["name"] = ls.Name,
                    ["meaning"] = ls.Meaning,
                    ["trackedBy"] = ls.TrackedBy,
                });
            }

            var result = new Dictionary<string, object>
            {
                ["file"] = filePath,
                ["system"] = manifest.System,
                ["baseSpec"] = manifest.BaseSpec,
                ["target"] = manifest.Target,
                ["specCount"] = manifest.SpecCount,
                ["lifecycleStates"] = lifecycleStates,
                ["inventory"] = inventory,
                ["executionOrder"] = executionOrder,
                ["diagnostics"] = SpecFileHelper.SerializeDiagnostics(diagnostics),
            };

            return JsonSerializer.Serialize(result, SpecFileHelper.JsonOptions);
        }
        catch (Exception ex)
        {
            return SpecFileHelper.ErrorJson($"Failed to read manifest: {ex.Message}");
        }
    }

    [McpServerTool, Description("Identify specs ready for execution based on dependency tiers.")]
    public static string NextExecutable(
        [Description("Absolute path to the .manifest.md file")] string filePath)
    {
        if (!File.Exists(filePath))
            return SpecFileHelper.ErrorJson($"File not found: {filePath}");

        try
        {
            string content = File.ReadAllText(filePath);
            var diagnostics = new DiagnosticBag();
            var parser = new ManifestParser(diagnostics);
            var manifest = parser.Parse(content, filePath);

            // Build a set of specs that are already in the Executed or Verified state.
            var completedSpecs = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (var entry in manifest.Inventory)
            {
                if (string.Equals(entry.State, "Executed", StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(entry.State, "Verified", StringComparison.OrdinalIgnoreCase))
                {
                    completedSpecs.Add(entry.Filename);
                }
            }

            // Find specs whose dependencies are all completed and whose own state
            // is Approved (ready for execution).
            var actionable = new List<Dictionary<string, object>>();
            foreach (var entry in manifest.Inventory)
            {
                if (!string.Equals(entry.State, "Approved", StringComparison.OrdinalIgnoreCase))
                    continue;

                bool allDepsCompleted = true;
                var deps = ParseDependencies(entry.Dependencies);
                foreach (var dep in deps)
                {
                    if (!completedSpecs.Contains(dep))
                    {
                        allDepsCompleted = false;
                        break;
                    }
                }

                if (allDepsCompleted)
                {
                    actionable.Add(new Dictionary<string, object>
                    {
                        ["filename"] = entry.Filename,
                        ["type"] = entry.Type,
                        ["tier"] = entry.Tier,
                        ["dependencies"] = entry.Dependencies ?? "",
                    });
                }
            }

            var result = new Dictionary<string, object>
            {
                ["file"] = filePath,
                ["actionableSpecs"] = actionable,
                ["actionableCount"] = actionable.Count,
                ["diagnostics"] = SpecFileHelper.SerializeDiagnostics(diagnostics),
            };

            return JsonSerializer.Serialize(result, SpecFileHelper.JsonOptions);
        }
        catch (Exception ex)
        {
            return SpecFileHelper.ErrorJson($"Failed to determine next executable: {ex.Message}");
        }
    }

    [McpServerTool, Description("Validate lifecycle state transitions in the manifest.")]
    public static string CheckLifecycle(
        [Description("Absolute path to the .manifest.md file")] string filePath)
    {
        if (!File.Exists(filePath))
            return SpecFileHelper.ErrorJson($"File not found: {filePath}");

        try
        {
            string content = File.ReadAllText(filePath);
            var diagnostics = new DiagnosticBag();
            var parser = new ManifestParser(diagnostics);
            var manifest = parser.Parse(content, filePath);

            // Valid lifecycle transitions: Draft -> Reviewed -> Approved -> Executed -> Verified
            var validStates = new List<string> { "Draft", "Reviewed", "Approved", "Executed", "Verified" };

            var violations = new List<Dictionary<string, object>>();
            foreach (var entry in manifest.Inventory)
            {
                // Check that the state is a recognized lifecycle state.
                bool isValid = false;
                foreach (var vs in validStates)
                {
                    if (string.Equals(entry.State, vs, StringComparison.OrdinalIgnoreCase))
                    {
                        isValid = true;
                        break;
                    }
                }

                if (!isValid)
                {
                    violations.Add(new Dictionary<string, object>
                    {
                        ["filename"] = entry.Filename,
                        ["issue"] = $"Unrecognized lifecycle state: {entry.State}",
                        ["currentState"] = entry.State,
                    });
                }
            }

            var result = new Dictionary<string, object>
            {
                ["file"] = filePath,
                ["check"] = "lifecycle",
                ["violations"] = violations,
                ["violationCount"] = violations.Count,
                ["diagnostics"] = SpecFileHelper.SerializeDiagnostics(diagnostics),
            };

            return JsonSerializer.Serialize(result, SpecFileHelper.JsonOptions);
        }
        catch (Exception ex)
        {
            return SpecFileHelper.ErrorJson($"Lifecycle check failed: {ex.Message}");
        }
    }

    [McpServerTool, Description("Generate a proposed edit to advance a spec's lifecycle state.")]
    public static string UpdateSpecState(
        [Description("Absolute path to the .manifest.md file")] string manifestPath,
        [Description("Filename of the spec to update")] string specFilename,
        [Description("New lifecycle state (Reviewed, Approved, Executed, Verified)")] string newState)
    {
        if (!File.Exists(manifestPath))
            return SpecFileHelper.ErrorJson($"File not found: {manifestPath}");

        try
        {
            string content = File.ReadAllText(manifestPath);
            var diagnostics = new DiagnosticBag();
            var parser = new ManifestParser(diagnostics);
            var manifest = parser.Parse(content, manifestPath);

            // Valid lifecycle ordering for transition validation.
            var stateOrder = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase)
            {
                ["Draft"] = 0,
                ["Reviewed"] = 1,
                ["Approved"] = 2,
                ["Executed"] = 3,
                ["Verified"] = 4,
            };

            if (!stateOrder.ContainsKey(newState))
            {
                return SpecFileHelper.ErrorJson(
                    $"Invalid target state: {newState}. Must be one of: Reviewed, Approved, Executed, Verified.");
            }

            // Find the spec entry.
            SpecEntry? targetEntry = null;
            foreach (var entry in manifest.Inventory)
            {
                if (string.Equals(entry.Filename, specFilename, StringComparison.OrdinalIgnoreCase))
                {
                    targetEntry = entry;
                    break;
                }
            }

            if (targetEntry is null)
            {
                return SpecFileHelper.ErrorJson($"Spec '{specFilename}' not found in manifest.");
            }

            // Validate the transition is legal (must advance exactly one step, or from Draft to Reviewed).
            if (!stateOrder.TryGetValue(targetEntry.State, out int currentOrder))
            {
                return SpecFileHelper.ErrorJson(
                    $"Current state '{targetEntry.State}' is not a recognized lifecycle state.");
            }

            int targetOrder = stateOrder[newState];
            if (targetOrder <= currentOrder)
            {
                return SpecFileHelper.ErrorJson(
                    $"Cannot transition from {targetEntry.State} to {newState}. State must advance forward.");
            }
            if (targetOrder > currentOrder + 1)
            {
                return SpecFileHelper.ErrorJson(
                    $"Cannot skip states. Current: {targetEntry.State}, requested: {newState}. " +
                    $"Next valid state: {GetStateName(currentOrder + 1, stateOrder)}.");
            }

            string today = DateTime.UtcNow.ToString("yyyy-MM-dd");

            var result = new Dictionary<string, object>
            {
                ["file"] = manifestPath,
                ["spec"] = specFilename,
                ["previousState"] = targetEntry.State,
                ["newState"] = newState,
                ["date"] = today,
                ["proposedEdit"] = $"Change state of '{specFilename}' from '{targetEntry.State}' to '{newState}' (date: {today})",
            };

            return JsonSerializer.Serialize(result, SpecFileHelper.JsonOptions);
        }
        catch (Exception ex)
        {
            return SpecFileHelper.ErrorJson($"State update failed: {ex.Message}");
        }
    }

    private static string GetStateName(int order, Dictionary<string, int> stateOrder)
    {
        foreach (var kvp in stateOrder)
        {
            if (kvp.Value == order)
                return kvp.Key;
        }
        return "Unknown";
    }

    /// <summary>
    /// Split a comma-separated dependencies string into individual filenames,
    /// filtering out empty entries and the "None" sentinel.
    /// </summary>
    private static List<string> ParseDependencies(string? dependencies)
    {
        if (string.IsNullOrWhiteSpace(dependencies) ||
            string.Equals(dependencies, "None", StringComparison.OrdinalIgnoreCase))
        {
            return [];
        }

        var parts = dependencies.Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
        var result = new List<string>(parts.Length);
        foreach (var part in parts)
        {
            if (part.Length > 0)
                result.Add(part);
        }
        return result;
    }
}
