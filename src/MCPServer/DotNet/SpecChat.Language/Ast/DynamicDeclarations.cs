namespace SpecChat.Language.Ast;

// ── Dynamic specification declarations ──────────────────────────────

/// <summary>
/// Dynamic declaration: a named behavioral interaction sequence
/// showing how elements collaborate for a specific use case.
/// </summary>
public sealed record DynamicDecl(
    string Name,
    List<DynamicStep> Steps,
    SourceLocation Location) : TopLevelDecl(Location);

/// <summary>
/// A single numbered step in a dynamic interaction sequence.
/// </summary>
public sealed record DynamicStep(
    int SequenceNumber,
    string Source,
    string Target,
    string Description,
    string? Technology,
    SourceLocation Location) : AstNode(Location);
