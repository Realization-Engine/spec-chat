using System.ComponentModel;
using System.Text.Json;
using ModelContextProtocol.Server;
using SpecChat.Language;
using SpecChat.Language.Ast;

namespace SpecChat.Mcp.Tools;

[McpServerToolType]
public static class StandardValidationTools
{
    [McpServerTool, Description("Validate Standard layer assignments and hierarchy.")]
    public static string CheckStandardLayers(
        [Description("Absolute path to the .spec.md file")] string filePath)
    {
        return RunCheck(filePath, "layers", (analyzer, doc, arch) =>
            analyzer.CheckLayers(doc, arch));
    }

    [McpServerTool, Description("Validate Standard Flow Forward topology rules.")]
    public static string CheckFlowForward(
        [Description("Absolute path to the .spec.md file")] string filePath)
    {
        return RunCheck(filePath, "flow_forward", (analyzer, doc, _) =>
            analyzer.CheckFlowForward(doc));
    }

    [McpServerTool, Description("Validate Standard Florance Pattern constraints.")]
    public static string CheckFlorance(
        [Description("Absolute path to the .spec.md file")] string filePath)
    {
        return RunCheck(filePath, "florance", (analyzer, doc, _) =>
            analyzer.CheckFlorance(doc));
    }

    [McpServerTool, Description("Validate Standard entity ownership rules.")]
    public static string CheckEntityOwnership(
        [Description("Absolute path to the .spec.md file")] string filePath)
    {
        return RunCheck(filePath, "entity_ownership", (analyzer, doc, _) =>
            analyzer.CheckEntityOwnership(doc));
    }

    [McpServerTool, Description("Validate Standard autonomy constraints.")]
    public static string CheckAutonomy(
        [Description("Absolute path to the .spec.md file")] string filePath)
    {
        return RunCheck(filePath, "autonomy", (analyzer, doc, _) =>
            analyzer.CheckAutonomy(doc));
    }

    [McpServerTool, Description("Validate Standard vocabulary usage.")]
    public static string CheckStandardVocabulary(
        [Description("Absolute path to the .spec.md file")] string filePath)
    {
        return RunCheck(filePath, "vocabulary", (analyzer, doc, arch) =>
            analyzer.CheckVocabulary(doc, arch));
    }

    private static string RunCheck(string filePath, string ruleName,
        Action<StandardSemanticAnalyzer, SpecDocument, ArchitectureDecl> check)
    {
        if (!File.Exists(filePath))
            return SpecFileHelper.ErrorJson($"File not found: {filePath}");

        try
        {
            var (document, parseDiagnostics) = SpecFileHelper.ParseFile(filePath);

            var archDecl = document.Declarations.OfType<ArchitectureDecl>().FirstOrDefault();
            if (archDecl is null)
            {
                return SpecFileHelper.ErrorJson(
                    $"No architecture declaration found. The '{ruleName}' check requires an active architecture.");
            }

            var diagnostics = new DiagnosticBag();
            var analyzer = new StandardSemanticAnalyzer(diagnostics);
            check(analyzer, document, archDecl);

            var allDiagnostics = new List<object>();
            allDiagnostics.AddRange(SpecFileHelper.SerializeDiagnostics(parseDiagnostics));
            allDiagnostics.AddRange(SpecFileHelper.SerializeDiagnostics(diagnostics));

            var result = new Dictionary<string, object>
            {
                ["file"] = filePath,
                ["rule"] = ruleName,
                ["diagnostics"] = allDiagnostics,
                ["hasErrors"] = parseDiagnostics.HasErrors || diagnostics.HasErrors,
                ["diagnosticCount"] = allDiagnostics.Count,
            };

            return JsonSerializer.Serialize(result, SpecFileHelper.JsonOptions);
        }
        catch (Exception ex)
        {
            return SpecFileHelper.ErrorJson($"Failed to run {ruleName} check: {ex.Message}");
        }
    }
}
