namespace SpecChat.Language.Ast;

// ── Context specification declarations ──────────────────────────────

/// <summary>
/// Person declaration: a human user or actor who interacts with the system.
/// </summary>
public sealed record PersonDecl(
    string Name,
    string Description,
    List<string> Tags,
    List<RationaleDecl> Rationales,
    SourceLocation Location) : TopLevelDecl(Location);

/// <summary>
/// External system declaration: a software system outside our boundary
/// that we interact with at runtime (not a build-time package dependency).
/// </summary>
public sealed record ExternalSystemDecl(
    string Name,
    string Description,
    string? Technology,
    List<string> Tags,
    List<RationaleDecl> Rationales,
    SourceLocation Location) : TopLevelDecl(Location);

/// <summary>
/// Relationship declaration: a labeled directional edge between two
/// elements (persons, systems, or external systems).
/// </summary>
public sealed record RelationshipDecl(
    string Source,
    string Target,
    string Description,
    string? Technology,
    List<string> Tags,
    List<RationaleDecl> Rationales,
    SourceLocation Location) : TopLevelDecl(Location);

/// <summary>
/// Tag annotation: a list of classification strings attached to a
/// declaration. Used by view filters for include/exclude by tag.
/// </summary>
public sealed record TagAnnotation(
    List<string> Values,
    SourceLocation Location) : AstNode(Location);
