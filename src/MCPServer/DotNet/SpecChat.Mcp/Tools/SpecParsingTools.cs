using System.ComponentModel;
using System.Text.Json;
using ModelContextProtocol.Server;
using SpecChat.Language;
using SpecChat.Language.Ast;

namespace SpecChat.Mcp.Tools;

[McpServerToolType]
public static class SpecParsingTools
{
    [McpServerTool, Description("Parse a .spec.md file and return AST summary and diagnostics.")]
    public static string ParseSpec(
        [Description("Absolute path to the .spec.md file")] string filePath)
    {
        if (!File.Exists(filePath))
            return SpecFileHelper.ErrorJson($"File not found: {filePath}");

        try
        {
            var (document, diagnostics) = SpecFileHelper.ParseFile(filePath);

            var constructs = new List<Dictionary<string, object>>();
            foreach (var decl in document.Declarations)
            {
                constructs.Add(SummarizeDeclaration(decl));
            }

            var result = new Dictionary<string, object>
            {
                ["file"] = filePath,
                ["constructCount"] = document.Declarations.Count,
                ["constructs"] = constructs,
                ["diagnostics"] = SpecFileHelper.SerializeDiagnostics(diagnostics),
                ["hasErrors"] = diagnostics.HasErrors,
            };

            return JsonSerializer.Serialize(result, SpecFileHelper.JsonOptions);
        }
        catch (Exception ex)
        {
            return SpecFileHelper.ErrorJson($"Failed to parse file: {ex.Message}");
        }
    }

    [McpServerTool, Description("Parse a single spec block string and return the AST.")]
    public static string ParseSpecBlock(
        [Description("Raw text content of a spec block (without fence markers)")] string specBlock)
    {
        try
        {
            var diagnostics = new DiagnosticBag();
            var lexer = new Lexer(specBlock, "<inline>", 0, diagnostics);
            var tokens = lexer.Tokenize();

            var parser = new Parser(tokens, "<inline>", diagnostics);
            var doc = parser.Parse();

            var nodes = new List<Dictionary<string, object>>();
            foreach (var decl in doc.Declarations)
            {
                nodes.Add(SummarizeDeclaration(decl));
            }

            var result = new Dictionary<string, object>
            {
                ["nodes"] = nodes,
                ["diagnostics"] = SpecFileHelper.SerializeDiagnostics(diagnostics),
                ["hasErrors"] = diagnostics.HasErrors,
            };

            return JsonSerializer.Serialize(result, SpecFileHelper.JsonOptions);
        }
        catch (Exception ex)
        {
            return SpecFileHelper.ErrorJson($"Failed to parse spec block: {ex.Message}");
        }
    }

    [McpServerTool, Description("List all declared constructs in a spec file.")]
    public static string ListConstructs(
        [Description("Absolute path to the .spec.md file")] string filePath)
    {
        if (!File.Exists(filePath))
            return SpecFileHelper.ErrorJson($"File not found: {filePath}");

        try
        {
            var (document, diagnostics) = SpecFileHelper.ParseFile(filePath);

            var entities = new List<string>();
            var enums = new List<string>();
            var contracts = new List<string>();
            var refinements = new List<string>();
            var systems = new List<string>();
            var topologies = new List<string>();
            var phases = new List<string>();
            var traces = new List<string>();
            var constraints = new List<string>();
            var packagePolicies = new List<string>();
            var dotnetSolutions = new List<string>();
            var pages = new List<string>();
            var visualizations = new List<string>();

            foreach (var decl in document.Declarations)
            {
                switch (decl)
                {
                    case EntityDecl e:
                        entities.Add(e.Name);
                        break;
                    case EnumDecl e:
                        enums.Add(e.Name);
                        break;
                    case ContractDecl c:
                        contracts.Add(c.Name ?? "(anonymous)");
                        break;
                    case RefinementDecl r:
                        refinements.Add($"{r.OriginalEntity} -> {r.RefinedName}");
                        break;
                    case SystemDecl s:
                        systems.Add(s.Name);
                        break;
                    case TopologyDecl t:
                        topologies.Add(t.Name);
                        break;
                    case PhaseDecl p:
                        phases.Add(p.Name);
                        break;
                    case TraceDecl t:
                        traces.Add(t.Name);
                        break;
                    case ConstraintDecl c:
                        constraints.Add(c.Name);
                        break;
                    case PackagePolicyDecl p:
                        packagePolicies.Add(p.Name);
                        break;
                    case DotNetSolutionDecl d:
                        dotnetSolutions.Add(d.Name);
                        break;
                    case PageDecl p:
                        pages.Add(p.Name);
                        break;
                    case VisualizationDecl v:
                        visualizations.Add(v.Name);
                        break;
                }
            }

            var result = new Dictionary<string, object>
            {
                ["file"] = filePath,
                ["entities"] = entities,
                ["enums"] = enums,
                ["contracts"] = contracts,
                ["refinements"] = refinements,
                ["systems"] = systems,
                ["topologies"] = topologies,
                ["phases"] = phases,
                ["traces"] = traces,
                ["constraints"] = constraints,
                ["packagePolicies"] = packagePolicies,
                ["dotnetSolutions"] = dotnetSolutions,
                ["pages"] = pages,
                ["visualizations"] = visualizations,
                ["diagnostics"] = SpecFileHelper.SerializeDiagnostics(diagnostics),
            };

            return JsonSerializer.Serialize(result, SpecFileHelper.JsonOptions);
        }
        catch (Exception ex)
        {
            return SpecFileHelper.ErrorJson($"Failed to list constructs: {ex.Message}");
        }
    }

    /// <summary>
    /// Create a summary dictionary for a single top-level declaration.
    /// </summary>
    private static Dictionary<string, object> SummarizeDeclaration(TopLevelDecl decl)
    {
        var summary = new Dictionary<string, object>
        {
            ["type"] = decl.GetType().Name.Replace("Decl", ""),
            ["location"] = decl.Location.ToString(),
        };

        switch (decl)
        {
            case EntityDecl e:
                summary["name"] = e.Name;
                summary["fieldCount"] = e.Fields.Count;
                summary["invariantCount"] = e.Invariants.Count;
                summary["rationaleCount"] = e.Rationales.Count;
                break;
            case EnumDecl e:
                summary["name"] = e.Name;
                summary["memberCount"] = e.Members.Count;
                break;
            case ContractDecl c:
                summary["name"] = c.Name ?? "(anonymous)";
                summary["clauseCount"] = c.Clauses.Count;
                break;
            case RefinementDecl r:
                summary["originalEntity"] = r.OriginalEntity;
                summary["refinedName"] = r.RefinedName;
                summary["fieldCount"] = r.Fields.Count;
                break;
            case SystemDecl s:
                summary["name"] = s.Name;
                summary["target"] = s.Target;
                summary["componentCount"] = s.Components.Count;
                break;
            case TopologyDecl t:
                summary["name"] = t.Name;
                summary["ruleCount"] = t.Rules.Count;
                break;
            case PhaseDecl p:
                summary["name"] = p.Name;
                summary["requiresPhases"] = p.RequiresPhases;
                summary["gateCount"] = p.Gates.Count;
                break;
            case TraceDecl t:
                summary["name"] = t.Name;
                summary["mappingCount"] = t.Mappings.Count;
                break;
            case ConstraintDecl c:
                summary["name"] = c.Name;
                summary["rule"] = c.Rule;
                break;
            case PackagePolicyDecl p:
                summary["name"] = p.Name;
                summary["source"] = p.Source;
                summary["ruleCount"] = p.Rules.Count;
                break;
            case DotNetSolutionDecl d:
                summary["name"] = d.Name;
                summary["format"] = d.Format;
                summary["folderCount"] = d.Folders.Count;
                break;
            case PageDecl p:
                summary["name"] = p.Name;
                summary["host"] = p.Host;
                summary["route"] = p.Route;
                break;
            case VisualizationDecl v:
                summary["name"] = v.Name;
                summary["parameterCount"] = v.Parameters.Count;
                break;
        }

        return summary;
    }
}
