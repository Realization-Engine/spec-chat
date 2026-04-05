using System.ComponentModel;
using System.Text.Json;
using ModelContextProtocol.Server;
using SpecChat.Language;

namespace SpecChat.Mcp.Tools;

[McpServerToolType]
public static class SpecValidationTools
{
    [McpServerTool, Description("Run full semantic validation on a spec file.")]
    public static string ValidateSpec(
        [Description("Absolute path to the .spec.md file")] string filePath)
    {
        if (!File.Exists(filePath))
            return SpecFileHelper.ErrorJson($"File not found: {filePath}");

        try
        {
            var (document, diagnostics) = SpecFileHelper.ParseFile(filePath);

            var analyzer = new SemanticAnalyzer(diagnostics);
            analyzer.Analyze(document);

            var result = new Dictionary<string, object>
            {
                ["file"] = filePath,
                ["diagnostics"] = SpecFileHelper.SerializeDiagnostics(diagnostics),
                ["hasErrors"] = diagnostics.HasErrors,
                ["diagnosticCount"] = diagnostics.Diagnostics.Count,
            };

            return JsonSerializer.Serialize(result, SpecFileHelper.JsonOptions);
        }
        catch (Exception ex)
        {
            return SpecFileHelper.ErrorJson($"Validation failed: {ex.Message}");
        }
    }

    [McpServerTool, Description("Validate topology rules against declared components.")]
    public static string CheckTopology(
        [Description("Absolute path to the .spec.md file")] string filePath)
    {
        if (!File.Exists(filePath))
            return SpecFileHelper.ErrorJson($"File not found: {filePath}");

        try
        {
            var (document, diagnostics) = SpecFileHelper.ParseFile(filePath);

            var analyzer = new SemanticAnalyzer(diagnostics);
            analyzer.CheckTopology(document);

            var result = new Dictionary<string, object>
            {
                ["file"] = filePath,
                ["check"] = "topology",
                ["diagnostics"] = SpecFileHelper.SerializeDiagnostics(diagnostics),
                ["hasErrors"] = diagnostics.HasErrors,
            };

            return JsonSerializer.Serialize(result, SpecFileHelper.JsonOptions);
        }
        catch (Exception ex)
        {
            return SpecFileHelper.ErrorJson($"Topology check failed: {ex.Message}");
        }
    }

    [McpServerTool, Description("Verify trace coverage invariants.")]
    public static string CheckTraces(
        [Description("Absolute path to the .spec.md file")] string filePath)
    {
        if (!File.Exists(filePath))
            return SpecFileHelper.ErrorJson($"File not found: {filePath}");

        try
        {
            var (document, diagnostics) = SpecFileHelper.ParseFile(filePath);

            var analyzer = new SemanticAnalyzer(diagnostics);
            analyzer.CheckTraces(document);

            var result = new Dictionary<string, object>
            {
                ["file"] = filePath,
                ["check"] = "traces",
                ["diagnostics"] = SpecFileHelper.SerializeDiagnostics(diagnostics),
                ["hasErrors"] = diagnostics.HasErrors,
            };

            return JsonSerializer.Serialize(result, SpecFileHelper.JsonOptions);
        }
        catch (Exception ex)
        {
            return SpecFileHelper.ErrorJson($"Trace check failed: {ex.Message}");
        }
    }

    [McpServerTool, Description("Validate phase ordering and gate consistency.")]
    public static string CheckPhaseGates(
        [Description("Absolute path to the .spec.md file")] string filePath)
    {
        if (!File.Exists(filePath))
            return SpecFileHelper.ErrorJson($"File not found: {filePath}");

        try
        {
            var (document, diagnostics) = SpecFileHelper.ParseFile(filePath);

            var analyzer = new SemanticAnalyzer(diagnostics);
            analyzer.CheckPhaseOrdering(document);

            var result = new Dictionary<string, object>
            {
                ["file"] = filePath,
                ["check"] = "phaseGates",
                ["diagnostics"] = SpecFileHelper.SerializeDiagnostics(diagnostics),
                ["hasErrors"] = diagnostics.HasErrors,
            };

            return JsonSerializer.Serialize(result, SpecFileHelper.JsonOptions);
        }
        catch (Exception ex)
        {
            return SpecFileHelper.ErrorJson($"Phase gate check failed: {ex.Message}");
        }
    }

    [McpServerTool, Description("Validate consumed components against package policy.")]
    public static string CheckPackagePolicy(
        [Description("Absolute path to the .spec.md file")] string filePath)
    {
        if (!File.Exists(filePath))
            return SpecFileHelper.ErrorJson($"File not found: {filePath}");

        try
        {
            var (document, diagnostics) = SpecFileHelper.ParseFile(filePath);

            var analyzer = new SemanticAnalyzer(diagnostics);
            analyzer.CheckPackagePolicy(document);

            var result = new Dictionary<string, object>
            {
                ["file"] = filePath,
                ["check"] = "packagePolicy",
                ["diagnostics"] = SpecFileHelper.SerializeDiagnostics(diagnostics),
                ["hasErrors"] = diagnostics.HasErrors,
            };

            return JsonSerializer.Serialize(result, SpecFileHelper.JsonOptions);
        }
        catch (Exception ex)
        {
            return SpecFileHelper.ErrorJson($"Package policy check failed: {ex.Message}");
        }
    }
}
