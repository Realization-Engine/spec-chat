using System.ComponentModel;
using System.Text;
using System.Text.Json;
using ModelContextProtocol.Server;
using SpecChat.Language.Ast;

namespace SpecChat.Mcp.Tools;

[McpServerToolType]
public static class RealizationTools
{
    [McpServerTool, Description("Extract all contracts from a spec for code generation.")]
    public static string ExtractContracts(
        [Description("Absolute path to the .spec.md file")] string filePath)
    {
        if (!File.Exists(filePath))
            return SpecFileHelper.ErrorJson($"File not found: {filePath}");

        try
        {
            var (document, diagnostics) = SpecFileHelper.ParseFile(filePath);

            var contracts = new List<Dictionary<string, object>>();

            foreach (var decl in document.Declarations)
            {
                // Top-level contracts
                if (decl is ContractDecl contract)
                {
                    contracts.Add(SerializeContract(contract));
                }

                // Contracts embedded within system components
                if (decl is SystemDecl system)
                {
                    foreach (var comp in system.Components)
                    {
                        foreach (var cc in comp.Contracts)
                        {
                            var serialized = SerializeContract(cc);
                            serialized["parentSystem"] = system.Name;
                            serialized["parentComponent"] = comp.Name;
                            contracts.Add(serialized);
                        }
                    }
                }
            }

            var result = new Dictionary<string, object>
            {
                ["file"] = filePath,
                ["contracts"] = contracts,
                ["contractCount"] = contracts.Count,
                ["diagnostics"] = SpecFileHelper.SerializeDiagnostics(diagnostics),
            };

            return JsonSerializer.Serialize(result, SpecFileHelper.JsonOptions);
        }
        catch (Exception ex)
        {
            return SpecFileHelper.ErrorJson($"Failed to extract contracts: {ex.Message}");
        }
    }

    [McpServerTool, Description("Extract entity definitions with invariants and annotations.")]
    public static string ExtractEntities(
        [Description("Absolute path to the .spec.md file")] string filePath)
    {
        if (!File.Exists(filePath))
            return SpecFileHelper.ErrorJson($"File not found: {filePath}");

        try
        {
            var (document, diagnostics) = SpecFileHelper.ParseFile(filePath);

            var entities = new List<Dictionary<string, object>>();

            foreach (var decl in document.Declarations)
            {
                if (decl is not EntityDecl entity)
                    continue;

                var fields = new List<Dictionary<string, object>>();
                foreach (var field in entity.Fields)
                {
                    var fieldDict = new Dictionary<string, object>
                    {
                        ["name"] = field.Name,
                        ["type"] = field.Type.Name + (field.Type.IsArray ? "[]" : ""),
                        ["isOptional"] = field.IsOptional,
                    };

                    if (field.Annotations.Count > 0)
                    {
                        var annotations = new List<Dictionary<string, object>>();
                        foreach (var ann in field.Annotations)
                        {
                            var annDict = new Dictionary<string, object>
                            {
                                ["name"] = ann.Name,
                            };
                            if (ann.Value is not null)
                                annDict["value"] = ann.Value;
                            annotations.Add(annDict);
                        }
                        fieldDict["annotations"] = annotations;
                    }

                    fields.Add(fieldDict);
                }

                var invariants = new List<Dictionary<string, object>>();
                foreach (var inv in entity.Invariants)
                {
                    invariants.Add(new Dictionary<string, object>
                    {
                        ["name"] = inv.Name,
                        ["location"] = inv.Location.ToString(),
                    });
                }

                var rationales = new List<Dictionary<string, object>>();
                foreach (var rat in entity.Rationales)
                {
                    var ratDict = new Dictionary<string, object>
                    {
                        ["isSimple"] = rat.IsSimple,
                        ["isStructured"] = rat.IsStructured,
                    };
                    if (rat.Text is not null)
                        ratDict["text"] = rat.Text;
                    if (rat.Context is not null)
                        ratDict["context"] = rat.Context;
                    if (rat.Decision is not null)
                        ratDict["decision"] = rat.Decision;
                    if (rat.Consequence is not null)
                        ratDict["consequence"] = rat.Consequence;
                    rationales.Add(ratDict);
                }

                entities.Add(new Dictionary<string, object>
                {
                    ["name"] = entity.Name,
                    ["fields"] = fields,
                    ["invariants"] = invariants,
                    ["rationales"] = rationales,
                    ["rationaleCount"] = entity.Rationales.Count,
                });
            }

            var result = new Dictionary<string, object>
            {
                ["file"] = filePath,
                ["entities"] = entities,
                ["entityCount"] = entities.Count,
                ["diagnostics"] = SpecFileHelper.SerializeDiagnostics(diagnostics),
            };

            return JsonSerializer.Serialize(result, SpecFileHelper.JsonOptions);
        }
        catch (Exception ex)
        {
            return SpecFileHelper.ErrorJson($"Failed to extract entities: {ex.Message}");
        }
    }

    [McpServerTool, Description("Extract phase gate commands for execution.")]
    public static string ExtractPhaseGates(
        [Description("Absolute path to the .spec.md file")] string filePath,
        [Description("Name of the phase to extract gates from")] string phaseName)
    {
        if (!File.Exists(filePath))
            return SpecFileHelper.ErrorJson($"File not found: {filePath}");

        try
        {
            var (document, diagnostics) = SpecFileHelper.ParseFile(filePath);

            PhaseDecl? targetPhase = null;
            foreach (var decl in document.Declarations)
            {
                if (decl is PhaseDecl phase &&
                    string.Equals(phase.Name, phaseName, StringComparison.OrdinalIgnoreCase))
                {
                    targetPhase = phase;
                    break;
                }
            }

            if (targetPhase is null)
            {
                return SpecFileHelper.ErrorJson($"Phase '{phaseName}' not found in spec file.");
            }

            var gates = new List<Dictionary<string, object>>();
            foreach (var gate in targetPhase.Gates)
            {
                var expectations = new List<Dictionary<string, object>>();
                foreach (var exp in gate.Expectations)
                {
                    var expDict = new Dictionary<string, object>();
                    if (exp.ProseExpectation is not null)
                        expDict["prose"] = exp.ProseExpectation;
                    if (exp.Expression is not null)
                        expDict["hasExpression"] = true;
                    expectations.Add(expDict);
                }

                gates.Add(new Dictionary<string, object>
                {
                    ["name"] = gate.Name,
                    ["command"] = gate.Command,
                    ["expectations"] = expectations,
                });
            }

            var result = new Dictionary<string, object>
            {
                ["file"] = filePath,
                ["phase"] = targetPhase.Name,
                ["requiresPhases"] = targetPhase.RequiresPhases,
                ["produces"] = targetPhase.Produces,
                ["gates"] = gates,
                ["gateCount"] = gates.Count,
                ["diagnostics"] = SpecFileHelper.SerializeDiagnostics(diagnostics),
            };

            return JsonSerializer.Serialize(result, SpecFileHelper.JsonOptions);
        }
        catch (Exception ex)
        {
            return SpecFileHelper.ErrorJson($"Failed to extract phase gates: {ex.Message}");
        }
    }

    [McpServerTool, Description("Generate project/solution scaffold from system and platform realization declarations.")]
    public static string GenerateScaffold(
        [Description("Absolute path to the .spec.md file")] string filePath)
    {
        if (!File.Exists(filePath))
            return SpecFileHelper.ErrorJson($"File not found: {filePath}");

        try
        {
            var (document, diagnostics) = SpecFileHelper.ParseFile(filePath);

            SystemDecl? system = null;
            DotNetSolutionDecl? solution = null;

            foreach (var decl in document.Declarations)
            {
                if (decl is SystemDecl s && system is null)
                    system = s;
                if (decl is DotNetSolutionDecl d && solution is null)
                    solution = d;
            }

            if (system is null)
            {
                return SpecFileHelper.ErrorJson("No system declaration found in spec file.");
            }

            var scaffold = new Dictionary<string, object>
            {
                ["systemName"] = system.Name,
                ["target"] = system.Target,
            };

            // Generate component project entries from the system tree.
            var projects = new List<Dictionary<string, object>>();
            foreach (var comp in system.Components)
            {
                if (comp.Disposition == ComponentDisposition.Consumed)
                    continue; // consumed packages are not authored projects

                var project = new Dictionary<string, object>
                {
                    ["name"] = comp.Name,
                    ["kind"] = comp.Kind ?? "classlib",
                    ["path"] = comp.Path ?? $"src/{comp.Name}",
                };

                if (comp.Status is not null)
                    project["status"] = comp.Status;
                if (comp.Layer is not null)
                    project["layer"] = comp.Layer;

                projects.Add(project);
            }
            scaffold["projects"] = projects;

            // If a .NET solution declaration exists, include the workspace layout.
            if (solution is not null)
            {
                var folders = new List<Dictionary<string, object>>();
                foreach (var folder in solution.Folders)
                {
                    folders.Add(new Dictionary<string, object>
                    {
                        ["name"] = folder.Name,
                        ["projects"] = folder.Projects,
                    });
                }

                scaffold["solution"] = new Dictionary<string, object>
                {
                    ["name"] = solution.Name,
                    ["format"] = solution.Format,
                    ["startup"] = solution.Startup ?? "(none)",
                    ["folders"] = folders,
                };
            }

            // Generate a file tree description.
            var fileTree = new List<string>();
            if (solution is not null)
            {
                fileTree.Add($"{solution.Name}.slnx");
                foreach (var folder in solution.Folders)
                {
                    foreach (var proj in folder.Projects)
                    {
                        fileTree.Add($"{proj}/{proj}.csproj");
                    }
                }
            }
            else
            {
                foreach (var proj in projects)
                {
                    string path = (string)proj["path"];
                    string name = (string)proj["name"];
                    fileTree.Add($"{path}/{name}.csproj");
                }
            }
            scaffold["fileTree"] = fileTree;

            var result = new Dictionary<string, object>
            {
                ["file"] = filePath,
                ["scaffold"] = scaffold,
                ["diagnostics"] = SpecFileHelper.SerializeDiagnostics(diagnostics),
            };

            return JsonSerializer.Serialize(result, SpecFileHelper.JsonOptions);
        }
        catch (Exception ex)
        {
            return SpecFileHelper.ErrorJson($"Failed to generate scaffold: {ex.Message}");
        }
    }

    [McpServerTool, Description("Generate interface/type definitions from entity or contract declarations.")]
    public static string SpecToInterface(
        [Description("Absolute path to the .spec.md file")] string filePath,
        [Description("Name of the entity or contract to generate from")] string constructName)
    {
        if (!File.Exists(filePath))
            return SpecFileHelper.ErrorJson($"File not found: {filePath}");

        try
        {
            var (document, diagnostics) = SpecFileHelper.ParseFile(filePath);

            // Search for the named construct.
            foreach (var decl in document.Declarations)
            {
                if (decl is EntityDecl entity &&
                    string.Equals(entity.Name, constructName, StringComparison.OrdinalIgnoreCase))
                {
                    return GenerateEntityInterface(entity);
                }

                if (decl is ContractDecl contract &&
                    string.Equals(contract.Name, constructName, StringComparison.OrdinalIgnoreCase))
                {
                    return GenerateContractInterface(contract);
                }

                if (decl is EnumDecl enumDecl &&
                    string.Equals(enumDecl.Name, constructName, StringComparison.OrdinalIgnoreCase))
                {
                    return GenerateEnumCode(enumDecl);
                }
            }

            return SpecFileHelper.ErrorJson($"Construct '{constructName}' not found in spec file.");
        }
        catch (Exception ex)
        {
            return SpecFileHelper.ErrorJson($"Failed to generate interface: {ex.Message}");
        }
    }

    // ── Private helpers ────────────────────────────────────────────────

    private static Dictionary<string, object> SerializeContract(ContractDecl contract)
    {
        var clauses = new List<Dictionary<string, object>>();
        foreach (var clause in contract.Clauses)
        {
            var clauseDict = new Dictionary<string, object>
            {
                ["kind"] = clause.Kind.ToString(),
            };
            if (clause.ProseGuarantee is not null)
                clauseDict["prose"] = clause.ProseGuarantee;
            if (clause.Expression is not null)
                clauseDict["hasExpression"] = true;
            if (clause.ValidationCategory is not null)
                clauseDict["validationCategory"] = clause.ValidationCategory;
            clauses.Add(clauseDict);
        }

        return new Dictionary<string, object>
        {
            ["name"] = contract.Name ?? "(anonymous)",
            ["clauses"] = clauses,
            ["clauseCount"] = clauses.Count,
        };
    }

    private static string GenerateEntityInterface(EntityDecl entity)
    {
        var sb = new StringBuilder();
        sb.AppendLine($"public interface I{entity.Name}");
        sb.AppendLine("{");

        foreach (var field in entity.Fields)
        {
            string csType = MapToCSharpType(field.Type.Name, field.Type.IsArray);
            if (field.IsOptional)
                csType += "?";

            sb.AppendLine($"    {csType} {field.Name} {{ get; }}");
        }

        sb.AppendLine("}");

        return sb.ToString();
    }

    private static string GenerateContractInterface(ContractDecl contract)
    {
        var sb = new StringBuilder();
        string name = contract.Name ?? "AnonymousContract";

        sb.AppendLine($"/// <summary>");
        sb.AppendLine($"/// Contract: {name}");

        foreach (var clause in contract.Clauses)
        {
            string prefix = clause.Kind switch
            {
                ContractClauseKind.Requires => "Requires",
                ContractClauseKind.Ensures => "Ensures",
                ContractClauseKind.Guarantees => "Guarantees",
                _ => "Clause",
            };

            if (clause.ProseGuarantee is not null)
                sb.AppendLine($"/// {prefix}: {clause.ProseGuarantee}");
            else
                sb.AppendLine($"/// {prefix}: (expression-based)");
        }

        sb.AppendLine($"/// </summary>");
        sb.AppendLine($"public interface I{name}Contract");
        sb.AppendLine("{");
        sb.AppendLine("}");

        return sb.ToString();
    }

    private static string GenerateEnumCode(EnumDecl enumDecl)
    {
        var sb = new StringBuilder();
        sb.AppendLine($"public enum {enumDecl.Name}");
        sb.AppendLine("{");

        for (int i = 0; i < enumDecl.Members.Count; i++)
        {
            var member = enumDecl.Members[i];
            sb.AppendLine($"    /// <summary>{member.Description}</summary>");
            string comma = i < enumDecl.Members.Count - 1 ? "," : "";
            sb.AppendLine($"    {member.Name}{comma}");
        }

        sb.AppendLine("}");

        return sb.ToString();
    }

    private static string MapToCSharpType(string specType, bool isArray)
    {
        string baseType = specType switch
        {
            "string" => "string",
            "int" => "int",
            "double" => "double",
            "bool" => "bool",
            "unknown" => "object",
            _ => specType, // named types pass through
        };

        return isArray ? $"IReadOnlyList<{baseType}>" : baseType;
    }
}
