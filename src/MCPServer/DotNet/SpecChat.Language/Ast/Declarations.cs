namespace SpecChat.Language.Ast;

/// <summary>
/// Discriminates contract clause kinds: requires (precondition),
/// ensures (postcondition), or guarantees (prose commitment).
/// </summary>
public enum ContractClauseKind
{
    Requires,
    Ensures,
    Guarantees,
}

// ── Document root ────────────────────────────────────────────────────

/// <summary>
/// Root node of a parsed .spec.md file. Contains all top-level
/// declarations found across every spec block in the document.
/// </summary>
public sealed record SpecDocument(
    List<TopLevelDecl> Declarations,
    SourceLocation Location) : AstNode(Location);

// ── Top-level declaration base ───────────────────────────────────────

/// <summary>
/// Base type for all top-level declarations (entity, enum, contract,
/// refinement, system, topology, phase, trace, constraint,
/// package_policy, dotnet solution, page, visualization).
/// </summary>
public abstract record TopLevelDecl(SourceLocation Location) : AstNode(Location);

// ── Data specification declarations ──────────────────────────────────

/// <summary>
/// Entity declaration with fields, invariants, rationale, and
/// optional inline contracts.
/// </summary>
public sealed record EntityDecl(
    string Name,
    List<FieldDecl> Fields,
    List<InvariantDecl> Invariants,
    List<RationaleDecl> Rationales,
    SourceLocation Location) : TopLevelDecl(Location);

/// <summary>
/// Field declaration within an entity or refinement.
/// </summary>
public sealed record FieldDecl(
    string Name,
    TypeRef Type,
    bool IsOptional,
    List<Annotation> Annotations,
    SourceLocation Location) : AstNode(Location);

/// <summary>
/// Type reference for a field. Name is the base type name (primitive
/// or named type). IsArray indicates the [] suffix.
/// </summary>
public sealed record TypeRef(
    string Name,
    bool IsArray,
    SourceLocation Location) : AstNode(Location);

/// <summary>
/// Field annotation such as @confidence(high), @default(medium),
/// @range(1..10), or @pattern("regex").
/// For range annotations, RangeMin and RangeMax hold the bounds.
/// For simple value annotations, Value holds the text.
/// </summary>
public sealed record Annotation(
    string Name,
    string? Value,
    Expr? RangeMin,
    Expr? RangeMax,
    SourceLocation Location) : AstNode(Location);

/// <summary>
/// Enum declaration with named members.
/// </summary>
public sealed record EnumDecl(
    string Name,
    List<EnumMemberDecl> Members,
    SourceLocation Location) : TopLevelDecl(Location);

/// <summary>
/// A single member of an enum: name and description string.
/// </summary>
public sealed record EnumMemberDecl(
    string Name,
    string Description,
    SourceLocation Location) : AstNode(Location);

/// <summary>
/// Named or anonymous contract declaration containing requires,
/// ensures, and guarantees clauses.
/// </summary>
public sealed record ContractDecl(
    string? Name,
    List<ContractClause> Clauses,
    SourceLocation Location) : TopLevelDecl(Location);

/// <summary>
/// A single clause within a contract.
/// For Requires/Ensures, Expression holds the condition.
/// For Guarantees, ProseGuarantee holds the prose commitment.
/// </summary>
public sealed record ContractClause(
    ContractClauseKind Kind,
    Expr? Expression,
    string? ProseGuarantee,
    string? ValidationCategory,
    SourceLocation Location) : AstNode(Location);

/// <summary>
/// Invariant declaration: a named boolean condition that must hold.
/// </summary>
public sealed record InvariantDecl(
    string Name,
    Expr Condition,
    SourceLocation Location) : AstNode(Location);

/// <summary>
/// Rationale declaration in either simple (prose-only) or structured
/// (ADR-style) form.
/// Simple form: Text is set; FieldName, Context, Decision,
/// Consequence, Supersedes are null.
/// Structured form: Context, Decision, Consequence are set;
/// FieldName and Supersedes are optional.
/// </summary>
public sealed record RationaleDecl(
    string? Text,
    string? FieldName,
    string? Context,
    string? Decision,
    string? Consequence,
    string? Supersedes,
    SourceLocation Location) : AstNode(Location)
{
    /// <summary>True when this is a Tier 1 (simple) rationale.</summary>
    public bool IsSimple => Text is not null;

    /// <summary>True when this is a Tier 2 (structured/ADR) rationale.</summary>
    public bool IsStructured => Context is not null;
}

/// <summary>
/// Refinement declaration: extends an existing entity with additional
/// fields and invariants.
/// </summary>
public sealed record RefinementDecl(
    string OriginalEntity,
    string RefinedName,
    List<FieldDecl> Fields,
    List<InvariantDecl> Invariants,
    List<RationaleDecl> Rationales,
    SourceLocation Location) : TopLevelDecl(Location);
