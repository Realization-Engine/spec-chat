namespace SpecChat.Language.Ast;

// ── Architecture declaration ────────────────────────────────────────

/// <summary>
/// Top-level architecture declaration that activates The Standard
/// extension and controls which rule sets are enforced.
/// </summary>
public sealed record ArchitectureDecl(
    string Name,
    string? Version,
    List<string> EnforceRules,
    VocabularyDecl? Vocabulary,
    List<RealizeDecl> Realizations,
    List<RationaleDecl> Rationales,
    SourceLocation Location) : TopLevelDecl(Location);

/// <summary>
/// Vocabulary block within an architecture declaration, mapping
/// layer names to their permitted operation verbs.
/// </summary>
public sealed record VocabularyDecl(
    List<VocabularyMapping> Mappings,
    SourceLocation Location) : AstNode(Location);

/// <summary>
/// A single layer-to-verbs mapping within a vocabulary block.
/// </summary>
public sealed record VocabularyMapping(
    string LayerName,
    List<string> Verbs,
    SourceLocation Location) : AstNode(Location);

/// <summary>
/// Realization directive within an architecture declaration.
/// Contains advisory prose for code generation at a specific layer.
/// </summary>
public sealed record RealizeDecl(
    string LayerName,
    List<string> Directives,
    SourceLocation Location) : AstNode(Location);

// ── Layer contract declaration ──────────────────────────────────────

/// <summary>
/// Top-level layer contract declaration defining behavioral
/// commitments for an entire architectural layer.
/// </summary>
public sealed record LayerContractDecl(
    string Name,
    string LayerName,
    List<ContractClause> Clauses,
    List<RationaleDecl> Rationales,
    SourceLocation Location) : TopLevelDecl(Location);
