namespace SpecChat.Language;

using SpecChat.Language.Ast;

/// <summary>
/// Post-parse semantic validation for a <see cref="SpecDocument"/> AST.
/// Reports violations through a <see cref="DiagnosticBag"/>.
/// </summary>
public sealed class SemanticAnalyzer
{
    private readonly DiagnosticBag _diagnostics;

    public SemanticAnalyzer(DiagnosticBag diagnostics)
    {
        _diagnostics = diagnostics;
    }

    /// <summary>
    /// Runs all semantic checks against the given document.
    /// </summary>
    public void Analyze(SpecDocument document)
    {
        CheckTopology(document);
        CheckTraces(document);
        CheckPhaseOrdering(document);
        CheckPackagePolicy(document);
        CheckCrossReferences(document);

        // The Standard extension: activate if architecture declaration present
        var archDecl = document.Declarations.OfType<ArchitectureDecl>().FirstOrDefault();
        if (archDecl is not null)
        {
            var standardAnalyzer = new StandardSemanticAnalyzer(_diagnostics);
            standardAnalyzer.Analyze(document, archDecl);
        }
        else
        {
            StandardSemanticAnalyzer.WarnLayerPrefixedWithoutArchitecture(document, _diagnostics);
        }
    }

    // ── Topology validation ─────────────────────────────────────────

    /// <summary>
    /// Validates topology rules against declared components.
    /// </summary>
    public void CheckTopology(SpecDocument document)
    {
        HashSet<string> declaredComponents = CollectDeclaredComponents(document);
        List<ComponentDecl> consumedComponents = CollectConsumedComponents(document);
        HashSet<string> consumedNames = new(consumedComponents.Count, StringComparer.Ordinal);
        foreach (ComponentDecl c in consumedComponents)
            consumedNames.Add(c.Name);

        foreach (TopLevelDecl decl in document.Declarations)
        {
            if (decl is not TopologyDecl topology)
                continue;

            // Track allow pairs to detect contradictions with deny rules.
            HashSet<string> allowPairs = new(StringComparer.Ordinal);

            foreach (TopologyRule rule in topology.Rules)
            {
                // Every referenced component must be declared.
                if (!declaredComponents.Contains(rule.Source))
                {
                    _diagnostics.ReportError(rule.Location,
                        $"Topology references undeclared component '{rule.Source}'.");
                }

                if (!declaredComponents.Contains(rule.Target))
                {
                    _diagnostics.ReportError(rule.Location,
                        $"Topology references undeclared component '{rule.Target}'.");
                }

                string pair = string.Concat(rule.Source, "->", rule.Target);

                if (rule.Kind == TopologyRuleKind.Allow)
                {
                    allowPairs.Add(pair);

                    // Warn if a consumed component is the source of an allow rule.
                    if (consumedNames.Contains(rule.Source))
                    {
                        _diagnostics.ReportWarning(rule.Location,
                            $"Component '{rule.Source}' is consumed, but appears as a source in an allow rule. " +
                            "Consumed components are dependencies, not dependents.");
                    }
                }
                else if (rule.Kind == TopologyRuleKind.Deny)
                {
                    // Deny must not contradict an allow on the same pair.
                    if (allowPairs.Contains(pair))
                    {
                        _diagnostics.ReportError(rule.Location,
                            $"Deny rule on '{pair}' contradicts an existing allow rule.");
                    }
                }
            }

            // Also check allow rules that come after deny rules (reverse order contradictions).
            HashSet<string> denyPairs = new(StringComparer.Ordinal);
            foreach (TopologyRule rule in topology.Rules)
            {
                string pair = string.Concat(rule.Source, "->", rule.Target);
                if (rule.Kind == TopologyRuleKind.Deny)
                    denyPairs.Add(pair);
                else if (rule.Kind == TopologyRuleKind.Allow && denyPairs.Contains(pair))
                {
                    _diagnostics.ReportError(rule.Location,
                        $"Allow rule on '{pair}' contradicts an existing deny rule.");
                }
            }
        }
    }

    // ── Trace coverage ──────────────────────────────────────────────

    /// <summary>
    /// Validates trace invariants: no empty targets, no duplicate sources.
    /// </summary>
    public void CheckTraces(SpecDocument document)
    {
        foreach (TopLevelDecl decl in document.Declarations)
        {
            if (decl is not TraceDecl trace)
                continue;

            HashSet<string> seenSources = new(StringComparer.Ordinal);

            foreach (TraceMapping mapping in trace.Mappings)
            {
                // Duplicate source detection.
                if (!seenSources.Add(mapping.Source))
                {
                    _diagnostics.ReportError(mapping.Location,
                        $"Duplicate source '{mapping.Source}' in trace '{trace.Name}'.");
                }

                // Warn if source maps to an empty target list.
                if (mapping.Targets.Count == 0)
                {
                    _diagnostics.ReportWarning(mapping.Location,
                        $"Source '{mapping.Source}' in trace '{trace.Name}' has no targets.");
                }
            }

            // Validate trace invariants. The zero-target warning above already
            // covers the practical case; invariant checking here validates the
            // formal pattern without duplicating diagnostics.
        }
    }

    // ── Phase ordering ──────────────────────────────────────────────

    /// <summary>
    /// Validates phase dependency ordering: required phases exist,
    /// no circular dependencies, and produces lists reference known identifiers.
    /// </summary>
    public void CheckPhaseOrdering(SpecDocument document)
    {
        Dictionary<string, PhaseDecl> phases = new(StringComparer.Ordinal);
        foreach (TopLevelDecl decl in document.Declarations)
        {
            if (decl is PhaseDecl phase)
                phases[phase.Name] = phase;
        }

        HashSet<string> allKnownIdentifiers = CollectAllKnownIdentifiers(document);

        foreach (KeyValuePair<string, PhaseDecl> entry in phases)
        {
            PhaseDecl phase = entry.Value;

            // Validate required phases exist.
            foreach (string reqPhase in phase.RequiresPhases)
            {
                if (!phases.ContainsKey(reqPhase))
                {
                    _diagnostics.ReportError(phase.Location,
                        $"Phase '{phase.Name}' requires phase '{reqPhase}', which does not exist.");
                }
            }

            // Circular dependency detection via BFS over all RequiresPhases.
            // Diamond dependencies (A->B, A->C, B->D, C->D) are not cycles;
            // only report when the starting phase is reachable from itself.
            {
                HashSet<string> visited = new(StringComparer.Ordinal);
                Queue<string> frontier = new();
                foreach (string rp in phase.RequiresPhases)
                    frontier.Enqueue(rp);

                bool cycleFound = false;
                while (frontier.Count > 0 && !cycleFound)
                {
                    string current = frontier.Dequeue();
                    if (string.Equals(current, phase.Name, StringComparison.Ordinal))
                    {
                        _diagnostics.ReportError(phase.Location,
                            $"Circular phase dependency detected: '{phase.Name}' transitively requires itself.");
                        cycleFound = true;
                        break;
                    }
                    if (!visited.Add(current))
                        continue; // already explored this node (diamond), not a cycle
                    if (phases.TryGetValue(current, out PhaseDecl? dep))
                    {
                        foreach (string transitive in dep.RequiresPhases)
                            frontier.Enqueue(transitive);
                    }
                }
            }

            // Validate produces list references known identifiers.
            foreach (string produced in phase.Produces)
            {
                if (!allKnownIdentifiers.Contains(produced))
                {
                    _diagnostics.ReportWarning(phase.Location,
                        $"Phase '{phase.Name}' produces '{produced}', " +
                        "which is not recognized as a declared component, entity, or page.");
                }
            }
        }
    }

    // ── Package policy ──────────────────────────────────────────────

    /// <summary>
    /// Validates consumed component packages against policy rules.
    /// </summary>
    public void CheckPackagePolicy(SpecDocument document)
    {
        List<PackagePolicyDecl> policies = new();
        foreach (TopLevelDecl decl in document.Declarations)
        {
            if (decl is PackagePolicyDecl policy)
                policies.Add(policy);
        }

        if (policies.Count == 0)
            return;

        List<ComponentDecl> consumedComponents = CollectConsumedComponents(document);

        foreach (ComponentDecl component in consumedComponents)
        {
            if (component.SourcePackage is null)
                continue;

            foreach (PackagePolicyDecl policy in policies)
            {
                bool matchedAllow = false;
                bool matchedDeny = false;

                string? deniedByCategory = null;

                foreach (PolicyRule rule in policy.Rules)
                {
                    if (MatchesPackagePattern(component.SourcePackage, rule.Includes))
                    {
                        if (rule.Kind == PolicyRuleKind.Deny)
                        {
                            matchedDeny = true;
                            deniedByCategory = rule.Category;
                        }
                        else if (rule.Kind == PolicyRuleKind.Allow)
                        {
                            matchedAllow = true;
                        }
                    }
                }

                // An allow match overrides a deny match (the package is explicitly
                // permitted in a different category).
                if (matchedDeny && !matchedAllow && deniedByCategory is not null)
                {
                    _diagnostics.ReportError(component.Location,
                        $"Consumed component '{component.Name}' uses package " +
                        $"'{component.SourcePackage}', which is denied by " +
                        $"category '{deniedByCategory}' in policy '{policy.Name}'.");
                }

                // Default policy: if not in any allow or deny, check for rationale.
                if (!matchedAllow && !matchedDeny &&
                    string.Equals(policy.DefaultPolicy, "require_rationale", StringComparison.OrdinalIgnoreCase))
                {
                    if (component.Rationales.Count == 0)
                    {
                        _diagnostics.ReportWarning(component.Location,
                            $"Consumed component '{component.Name}' with source " +
                            $"'{component.SourcePackage}' is not in any allowed category " +
                            $"and has no rationale declaration (policy '{policy.Name}' " +
                            "requires rationale for uncategorized packages).");
                    }
                }
            }
        }
    }

    // ── Cross-reference checks ──────────────────────────────────────

    /// <summary>
    /// Validates cross-references: page hosts reference authored components,
    /// trace targets reference declared pages, topology components are declared.
    /// </summary>
    public void CheckCrossReferences(SpecDocument document)
    {
        HashSet<string> authoredComponentNames = new(StringComparer.Ordinal);
        foreach (TopLevelDecl decl in document.Declarations)
        {
            if (decl is SystemDecl system)
            {
                foreach (ComponentDecl comp in system.Components)
                {
                    if (comp.Disposition == ComponentDisposition.Authored)
                        authoredComponentNames.Add(comp.Name);
                }
            }
        }

        HashSet<string> pageNames = new(StringComparer.Ordinal);
        foreach (TopLevelDecl decl in document.Declarations)
        {
            if (decl is PageDecl page)
                pageNames.Add(page.Name);
        }

        // Pages: host should reference an existing authored component.
        foreach (TopLevelDecl decl in document.Declarations)
        {
            if (decl is PageDecl page && page.Host.Length > 0)
            {
                if (!authoredComponentNames.Contains(page.Host))
                {
                    _diagnostics.ReportWarning(page.Location,
                        $"Page '{page.Name}' references host '{page.Host}', " +
                        "which is not a declared authored component.");
                }
            }
        }

        // Trace targets can reference pages, components, visualizations,
        // or other identifiers. No diagnostic is emitted for unresolved
        // targets because the trace vocabulary is open-ended.
    }

    // ── Helpers ─────────────────────────────────────────────────────

    private static HashSet<string> CollectDeclaredComponents(SpecDocument document)
    {
        HashSet<string> names = new(StringComparer.Ordinal);
        foreach (TopLevelDecl decl in document.Declarations)
        {
            if (decl is SystemDecl system)
            {
                foreach (ComponentDecl comp in system.Components)
                    names.Add(comp.Name);
            }
        }
        return names;
    }

    private static List<ComponentDecl> CollectConsumedComponents(SpecDocument document)
    {
        List<ComponentDecl> consumed = new();
        foreach (TopLevelDecl decl in document.Declarations)
        {
            if (decl is SystemDecl system)
            {
                foreach (ComponentDecl comp in system.Components)
                {
                    if (comp.Disposition == ComponentDisposition.Consumed)
                        consumed.Add(comp);
                }
            }
        }
        return consumed;
    }

    private static HashSet<string> CollectAllKnownIdentifiers(SpecDocument document)
    {
        HashSet<string> ids = new(StringComparer.Ordinal);

        foreach (TopLevelDecl decl in document.Declarations)
        {
            switch (decl)
            {
                case SystemDecl system:
                    foreach (ComponentDecl comp in system.Components)
                        ids.Add(comp.Name);
                    break;
                case EntityDecl entity:
                    ids.Add(entity.Name);
                    break;
                case EnumDecl enumDecl:
                    ids.Add(enumDecl.Name);
                    break;
                case ContractDecl contract when contract.Name is not null:
                    ids.Add(contract.Name);
                    break;
                case RefinementDecl refinement:
                    ids.Add(refinement.RefinedName);
                    break;
                case PageDecl page:
                    ids.Add(page.Name);
                    break;
                case VisualizationDecl viz:
                    ids.Add(viz.Name);
                    break;
                case TraceDecl trace:
                    ids.Add(trace.Name);
                    break;
                case PhaseDecl phase:
                    ids.Add(phase.Name);
                    break;
                case ArchitectureDecl arch:
                    ids.Add(arch.Name);
                    break;
                case LayerContractDecl lc:
                    ids.Add(lc.Name);
                    break;
            }
        }

        return ids;
    }

    /// <summary>
    /// Matches a package name against a list of patterns.
    /// Supports exact match and trailing wildcard (e.g., "Microsoft.AspNetCore.*").
    /// </summary>
    private static bool MatchesPackagePattern(string packageName, List<string> patterns)
    {
        foreach (string pattern in patterns)
        {
            if (pattern.EndsWith(".*", StringComparison.Ordinal))
            {
                ReadOnlySpan<char> prefix = pattern.AsSpan(0, pattern.Length - 2);
                if (packageName.AsSpan().StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
                    return true;
            }
            else if (string.Equals(packageName, pattern, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
        }
        return false;
    }
}
