namespace SpecChat.Language.Ast;

// ── Design specification declarations ────────────────────────────────

/// <summary>
/// Page declaration: binds a route to a host component and declares
/// the domain concepts it presents.
/// </summary>
public sealed record PageDecl(
    string Name,
    string Host,
    string Route,
    List<string> Concepts,
    string? Role,
    List<string> CrossLinks,
    List<RationaleDecl> Rationales,
    List<ProseIntent> ProseIntents,
    SourceLocation Location) : TopLevelDecl(Location);

/// <summary>
/// Visualization declaration: binds chart parameters to computation
/// methods and declares interactive controls.
/// </summary>
public sealed record VisualizationDecl(
    string Name,
    string? PageRef,
    string? ComponentRef,
    List<VisualizationParam> Parameters,
    List<string> SliderNames,
    List<RationaleDecl> Rationales,
    List<ProseIntent> ProseIntents,
    SourceLocation Location) : TopLevelDecl(Location);

/// <summary>
/// A single parameter binding within a visualization: maps a chart
/// parameter name to a value expression.
/// </summary>
public sealed record VisualizationParam(
    string Name,
    Expr Value,
    SourceLocation Location) : AstNode(Location);

/// <summary>
/// Prose intent: natural-language design rationale collected from
/// markdown context surrounding page or visualization declarations.
/// Not a grammar production; assembled by the markdown extractor.
/// </summary>
public sealed record ProseIntent(
    string Text,
    SourceLocation Location) : AstNode(Location);
