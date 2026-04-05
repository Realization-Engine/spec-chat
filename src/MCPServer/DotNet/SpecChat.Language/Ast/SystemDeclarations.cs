namespace SpecChat.Language.Ast;

/// <summary>
/// Whether a component is authored (we build it) or consumed
/// (we use it; someone else built it).
/// </summary>
public enum ComponentDisposition
{
    Authored,
    Consumed,
}

/// <summary>
/// Topology rule direction: allow or deny a dependency edge.
/// </summary>
public enum TopologyRuleKind
{
    Allow,
    Deny,
}

/// <summary>
/// Package policy rule direction: allow or deny a category.
/// </summary>
public enum PolicyRuleKind
{
    Allow,
    Deny,
}

// ── System tree ──────────────────────────────────────────────────────

/// <summary>
/// System declaration: the root of the component decomposition tree.
/// </summary>
public sealed record SystemDecl(
    string Name,
    string Target,
    string Responsibility,
    List<ComponentDecl> Components,
    SourceLocation Location) : TopLevelDecl(Location);

/// <summary>
/// Component declaration (authored or consumed) within a system.
/// </summary>
public sealed record ComponentDecl(
    string Name,
    ComponentDisposition Disposition,
    string? Kind,
    string? Path,
    string? Status,
    string? Responsibility,
    List<string> UsedBy,
    string? SourcePackage,
    string? Version,
    List<ContractDecl> Contracts,
    List<RationaleDecl> Rationales,
    SourceLocation Location) : AstNode(Location);

// ── Topology ─────────────────────────────────────────────────────────

/// <summary>
/// Topology declaration defining dependency allow/deny rules and
/// structural invariants.
/// </summary>
public sealed record TopologyDecl(
    string Name,
    List<TopologyRule> Rules,
    List<InvariantDecl> Invariants,
    List<RationaleDecl> Rationales,
    SourceLocation Location) : TopLevelDecl(Location);

/// <summary>
/// A single allow or deny rule in a topology: Source -> Target.
/// </summary>
public sealed record TopologyRule(
    TopologyRuleKind Kind,
    string Source,
    string Target,
    SourceLocation Location) : AstNode(Location);

// ── Phase ────────────────────────────────────────────────────────────

/// <summary>
/// Phase declaration for ordered construction with validation gates.
/// </summary>
public sealed record PhaseDecl(
    string Name,
    List<string> RequiresPhases,
    List<string> Produces,
    List<GateDecl> Gates,
    List<RationaleDecl> Rationales,
    SourceLocation Location) : TopLevelDecl(Location);

/// <summary>
/// Gate declaration within a phase: a command to run and its
/// expected outcomes.
/// </summary>
public sealed record GateDecl(
    string Name,
    string Command,
    List<GateExpectation> Expectations,
    SourceLocation Location) : AstNode(Location);

/// <summary>
/// A single expectation within a gate. Either a machine-evaluable
/// expression or a prose expectation string.
/// </summary>
public sealed record GateExpectation(
    Expr? Expression,
    string? ProseExpectation,
    SourceLocation Location) : AstNode(Location);

// ── Trace ────────────────────────────────────────────────────────────

/// <summary>
/// Trace declaration: many-to-many mappings between domain concepts
/// and implementation artifacts.
/// </summary>
public sealed record TraceDecl(
    string Name,
    List<TraceMapping> Mappings,
    List<InvariantDecl> Invariants,
    SourceLocation Location) : TopLevelDecl(Location);

/// <summary>
/// A single trace mapping: Source -> [Target1, Target2, ...].
/// </summary>
public sealed record TraceMapping(
    string Source,
    List<string> Targets,
    SourceLocation Location) : AstNode(Location);

// ── Constraint ───────────────────────────────────────────────────────

/// <summary>
/// System-level constraint applying a prose rule across a scope.
/// Scope is either an expression (all authored components) or an
/// explicit list (ScopeList).
/// </summary>
public sealed record ConstraintDecl(
    string Name,
    Expr? Scope,
    List<string> ScopeList,
    string Rule,
    List<RationaleDecl> Rationales,
    SourceLocation Location) : TopLevelDecl(Location);

// ── Package policy ───────────────────────────────────────────────────

/// <summary>
/// Package policy declaration governing dependency categories.
/// </summary>
public sealed record PackagePolicyDecl(
    string Name,
    string Source,
    List<PolicyRule> Rules,
    string? DefaultPolicy,
    List<RationaleDecl> Rationales,
    SourceLocation Location) : TopLevelDecl(Location);

/// <summary>
/// A single allow or deny rule within a package policy, specifying
/// a category and its included packages.
/// </summary>
public sealed record PolicyRule(
    PolicyRuleKind Kind,
    string Category,
    List<string> Includes,
    SourceLocation Location) : AstNode(Location);

// ── Platform realization (.NET solution) ─────────────────────────────

/// <summary>
/// .NET solution declaration: maps the system tree to a .slnx
/// workspace layout.
/// </summary>
public sealed record DotNetSolutionDecl(
    string Name,
    string Format,
    string? Startup,
    List<SolutionFolderDecl> Folders,
    List<RationaleDecl> Rationales,
    SourceLocation Location) : TopLevelDecl(Location);

/// <summary>
/// A folder within a .NET solution containing project references.
/// </summary>
public sealed record SolutionFolderDecl(
    string Name,
    List<string> Projects,
    List<RationaleDecl> Rationales,
    SourceLocation Location) : AstNode(Location);
