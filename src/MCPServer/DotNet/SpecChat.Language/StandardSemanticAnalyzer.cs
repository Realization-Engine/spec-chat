namespace SpecChat.Language;

using SpecChat.Language.Ast;

/// <summary>
/// Semantic validation for The Standard extension rule sets.
/// Only runs when an <see cref="ArchitectureDecl"/> is present.
/// </summary>
public sealed class StandardSemanticAnalyzer
{
    private readonly DiagnosticBag _diagnostics;

    private static readonly Dictionary<string, int> LayerOrder = new(StringComparer.OrdinalIgnoreCase)
    {
        ["broker"] = 0,
        ["foundation"] = 1,
        ["processing"] = 2,
        ["orchestration"] = 3,
        ["coordination"] = 4,
        ["aggregation"] = 5,
        ["exposer"] = 6,
    };

    public StandardSemanticAnalyzer(DiagnosticBag diagnostics)
    {
        _diagnostics = diagnostics;
    }

    /// <summary>
    /// Runs all activated rule sets based on the enforce list.
    /// Empty enforce list means all rules are active (All-In default).
    /// </summary>
    public void Analyze(SpecDocument document, ArchitectureDecl architecture)
    {
        var enforced = new HashSet<string>(architecture.EnforceRules, StringComparer.OrdinalIgnoreCase);
        bool allRules = enforced.Count == 0;

        if (allRules || enforced.Contains("layers")) CheckLayers(document, architecture);
        if (allRules || enforced.Contains("flow_forward")) CheckFlowForward(document);
        if (allRules || enforced.Contains("florance")) CheckFlorance(document);
        if (allRules || enforced.Contains("entity_ownership")) CheckEntityOwnership(document);
        if (allRules || enforced.Contains("autonomy")) CheckAutonomy(document);
        if (allRules || enforced.Contains("vocabulary")) CheckVocabulary(document, architecture);
    }

    /// <summary>
    /// Every authored component must have a valid layer property.
    /// </summary>
    public void CheckLayers(SpecDocument document, ArchitectureDecl architecture)
    {
        foreach (ComponentDecl comp in GetAuthoredComponents(document))
        {
            if (comp.Suppressions.Contains("layers", StringComparer.OrdinalIgnoreCase))
                continue;

            if (comp.Layer is null)
            {
                _diagnostics.ReportError(comp.Location,
                    $"Authored component '{comp.Name}' has no layer property. " +
                    "All components must declare their layer when an architecture is active.");
                continue;
            }

            if (!LayerOrder.ContainsKey(comp.Layer)
                && !string.Equals(comp.Layer, "test", StringComparison.OrdinalIgnoreCase))
            {
                _diagnostics.ReportError(comp.Location,
                    $"Component '{comp.Name}' has unrecognized layer '{comp.Layer}'. " +
                    "Expected: broker, foundation, processing, orchestration, coordination, aggregation, exposer, or test.");
            }
        }
    }

    /// <summary>
    /// Services at the same layer cannot depend on each other.
    /// Exposers cannot depend on brokers directly.
    /// Brokers cannot depend on services or exposers.
    /// </summary>
    public void CheckFlowForward(SpecDocument document)
    {
        var components = BuildComponentMap(document);
        var topology = GetTopologyRules(document);

        foreach (TopologyRule rule in topology)
        {
            if (rule.Kind != TopologyRuleKind.Allow) continue;

            if (!components.TryGetValue(rule.Source, out ComponentDecl? source) ||
                !components.TryGetValue(rule.Target, out ComponentDecl? target))
                continue;

            if (source.Layer is null || target.Layer is null) continue;
            if (string.Equals(source.Layer, "test", StringComparison.OrdinalIgnoreCase)) continue;
            if (source.Suppressions.Contains("flow_forward", StringComparer.OrdinalIgnoreCase)) continue;

            if (source.Disposition == ComponentDisposition.Consumed ||
                target.Disposition == ComponentDisposition.Consumed)
                continue;

            if (LayerOrder.TryGetValue(source.Layer, out int sourceOrder) &&
                LayerOrder.TryGetValue(target.Layer, out int targetOrder))
            {
                // Same layer: lateral call (forbidden)
                if (sourceOrder == targetOrder)
                {
                    _diagnostics.ReportWarning(rule.Location,
                        $"Flow Forward violation: '{rule.Source}' and '{rule.Target}' " +
                        $"are both at layer '{source.Layer}'. Same-layer dependencies are not allowed.");
                }
                // Upward call (higher number depending on lower): allowed
                // Downward call (lower number depending on higher): forbidden
                else if (sourceOrder < targetOrder)
                {
                    _diagnostics.ReportWarning(rule.Location,
                        $"Flow Forward violation: '{rule.Source}' (layer '{source.Layer}') " +
                        $"depends on '{rule.Target}' (layer '{target.Layer}'). " +
                        "Dependencies must flow from higher layers to lower layers.");
                }
            }
        }
    }

    /// <summary>
    /// Orchestration-layer components must have 2-3 service-layer dependencies.
    /// </summary>
    public void CheckFlorance(SpecDocument document)
    {
        var components = BuildComponentMap(document);
        var topology = GetTopologyRules(document);

        foreach (ComponentDecl comp in GetAuthoredComponents(document))
        {
            if (comp.Layer is null) continue;
            if (!string.Equals(comp.Layer, "orchestration", StringComparison.OrdinalIgnoreCase)) continue;
            if (comp.Suppressions.Contains("florance", StringComparer.OrdinalIgnoreCase)) continue;

            // Count service-layer dependencies (foundation/processing)
            int serviceDeps = 0;
            foreach (TopologyRule rule in topology)
            {
                if (rule.Kind != TopologyRuleKind.Allow) continue;
                if (!string.Equals(rule.Source, comp.Name, StringComparison.OrdinalIgnoreCase)) continue;
                if (components.TryGetValue(rule.Target, out ComponentDecl? target) &&
                    target.Layer is not null &&
                    (string.Equals(target.Layer, "foundation", StringComparison.OrdinalIgnoreCase) ||
                     string.Equals(target.Layer, "processing", StringComparison.OrdinalIgnoreCase)))
                {
                    serviceDeps++;
                }
            }

            if (serviceDeps < 2 || serviceDeps > 3)
            {
                _diagnostics.ReportWarning(comp.Location,
                    $"Florance Pattern: orchestration component '{comp.Name}' has {serviceDeps} " +
                    "service-layer dependencies. Expected 2-3.");
            }
        }
    }

    /// <summary>
    /// Foundation services must declare exactly one entity via owns.
    /// </summary>
    public void CheckEntityOwnership(SpecDocument document)
    {
        var ownedEntities = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        foreach (ComponentDecl comp in GetAuthoredComponents(document))
        {
            if (comp.Layer is null) continue;
            if (!string.Equals(comp.Layer, "foundation", StringComparison.OrdinalIgnoreCase)) continue;
            if (comp.Suppressions.Contains("entity_ownership", StringComparer.OrdinalIgnoreCase)) continue;

            if (comp.Owns is null)
            {
                _diagnostics.ReportWarning(comp.Location,
                    $"Foundation component '{comp.Name}' does not declare entity ownership via 'owns'.");
                continue;
            }

            if (ownedEntities.TryGetValue(comp.Owns, out string? existingOwner))
            {
                _diagnostics.ReportError(comp.Location,
                    $"Entity '{comp.Owns}' is already owned by '{existingOwner}'. " +
                    $"Foundation component '{comp.Name}' cannot also own it.");
            }
            else
            {
                ownedEntities[comp.Owns] = comp.Name;
            }
        }
    }

    /// <summary>
    /// Detect horizontal entanglement: same-layer components sharing dependencies.
    /// </summary>
    public void CheckAutonomy(SpecDocument document)
    {
        var components = BuildComponentMap(document);
        var topology = GetTopologyRules(document);

        // Group dependencies by source layer
        var depsByLayer = new Dictionary<string, List<(string Source, string Target)>>(StringComparer.OrdinalIgnoreCase);

        foreach (TopologyRule rule in topology)
        {
            if (rule.Kind != TopologyRuleKind.Allow) continue;
            if (!components.TryGetValue(rule.Source, out ComponentDecl? source)) continue;
            if (source.Layer is null) continue;
            if (string.Equals(source.Layer, "test", StringComparison.OrdinalIgnoreCase)) continue;
            if (source.Suppressions.Contains("autonomy", StringComparer.OrdinalIgnoreCase)) continue;

            if (!depsByLayer.TryGetValue(source.Layer, out var deps))
            {
                deps = [];
                depsByLayer[source.Layer] = deps;
            }
            deps.Add((rule.Source, rule.Target));
        }

        foreach (var (layer, deps) in depsByLayer)
        {
            // Find targets depended on by multiple sources at the same layer
            var targetToSources = new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase);
            foreach (var (src, tgt) in deps)
            {
                if (!components.TryGetValue(tgt, out ComponentDecl? tgtComp)) continue;
                if (tgtComp.Layer is null) continue;
                if (!string.Equals(tgtComp.Layer, layer, StringComparison.OrdinalIgnoreCase)) continue;

                if (!targetToSources.TryGetValue(tgt, out var sources))
                {
                    sources = [];
                    targetToSources[tgt] = sources;
                }
                if (!sources.Contains(src, StringComparer.OrdinalIgnoreCase))
                    sources.Add(src);
            }

            foreach (var (target, sources) in targetToSources)
            {
                if (sources.Count > 1 && components.TryGetValue(target, out ComponentDecl? targetComp))
                {
                    _diagnostics.ReportWarning(targetComp.Location,
                        $"Autonomy: '{target}' at layer '{layer}' is depended on by " +
                        $"multiple same-layer components: {string.Join(", ", sources)}.");
                }
            }
        }
    }

    /// <summary>
    /// Validate contract verbs against layer vocabulary when defined.
    /// </summary>
    public void CheckVocabulary(SpecDocument document, ArchitectureDecl architecture)
    {
        if (architecture.Vocabulary is null) return;

        var vocabByLayer = new Dictionary<string, HashSet<string>>(StringComparer.OrdinalIgnoreCase);
        foreach (VocabularyMapping mapping in architecture.Vocabulary.Mappings)
        {
            vocabByLayer[mapping.LayerName] = new HashSet<string>(mapping.Verbs, StringComparer.OrdinalIgnoreCase);
        }

        foreach (ComponentDecl comp in GetAuthoredComponents(document))
        {
            if (comp.Layer is null) continue;
            if (comp.Suppressions.Contains("vocabulary", StringComparer.OrdinalIgnoreCase)) continue;
            if (!vocabByLayer.TryGetValue(comp.Layer, out HashSet<string>? allowedVerbs)) continue;

            foreach (ContractDecl contract in comp.Contracts)
            {
                foreach (ContractClause clause in contract.Clauses)
                {
                    if (clause.Kind != ContractClauseKind.Guarantees) continue;
                    if (clause.ProseGuarantee is null) continue;

                    // Check if the guarantee prose starts with a recognized verb
                    string firstWord = clause.ProseGuarantee.Split(' ', 2)[0];
                    if (firstWord.Length > 0 && !allowedVerbs.Contains(firstWord))
                    {
                        _diagnostics.ReportWarning(clause.Location,
                            $"Vocabulary: guarantee in component '{comp.Name}' starts with '{firstWord}', " +
                            $"which is not in the '{comp.Layer}' vocabulary: [{string.Join(", ", allowedVerbs)}].");
                    }
                }
            }
        }
    }

    /// <summary>
    /// Warns when layer-prefixed declarations are used without an architecture declaration.
    /// </summary>
    public static void WarnLayerPrefixedWithoutArchitecture(SpecDocument document, DiagnosticBag diagnostics)
    {
        foreach (TopLevelDecl decl in document.Declarations)
        {
            if (decl is not SystemDecl system) continue;
            foreach (ComponentDecl comp in system.Components)
            {
                if (comp.Layer is not null && comp.Disposition == ComponentDisposition.Authored)
                {
                    diagnostics.ReportWarning(comp.Location,
                        $"Component '{comp.Name}' has a layer property but no architecture declaration is active. " +
                        "Layer-specific rules will not be enforced.");
                }
            }
        }
    }

    // ── Helpers ─────────────────────────────────────────────────────

    private static List<ComponentDecl> GetAuthoredComponents(SpecDocument document)
    {
        var result = new List<ComponentDecl>();
        foreach (TopLevelDecl decl in document.Declarations)
        {
            if (decl is SystemDecl system)
            {
                foreach (ComponentDecl comp in system.Components)
                {
                    if (comp.Disposition == ComponentDisposition.Authored)
                        result.Add(comp);
                }
            }
        }
        return result;
    }

    private static Dictionary<string, ComponentDecl> BuildComponentMap(SpecDocument document)
    {
        var map = new Dictionary<string, ComponentDecl>(StringComparer.OrdinalIgnoreCase);
        foreach (TopLevelDecl decl in document.Declarations)
        {
            if (decl is SystemDecl system)
            {
                foreach (ComponentDecl comp in system.Components)
                    map[comp.Name] = comp;
            }
        }
        return map;
    }

    private static List<TopologyRule> GetTopologyRules(SpecDocument document)
    {
        var rules = new List<TopologyRule>();
        foreach (TopLevelDecl decl in document.Declarations)
        {
            if (decl is TopologyDecl topo)
                rules.AddRange(topo.Rules);
        }
        return rules;
    }
}
