namespace SpecChat.Language.Ast;

// ── View specification declarations ─────────────────────────────────

/// <summary>
/// View kind: determines the abstraction level rendered.
/// </summary>
public enum ViewKind
{
    SystemLandscape,
    SystemContext,
    Container,
    Component,
    Deployment,
}

/// <summary>
/// View filter kind for include/exclude.
/// </summary>
public enum ViewFilterKind
{
    /// <summary>Include/exclude all elements in scope.</summary>
    All,
    /// <summary>Include/exclude elements carrying a specific tag.</summary>
    Tagged,
    /// <summary>Include/exclude a named list of elements.</summary>
    Explicit,
}

/// <summary>
/// Layout direction hint for diagram rendering.
/// </summary>
public enum LayoutDirection
{
    TopDown,
    LeftRight,
    BottomUp,
    RightLeft,
}

/// <summary>
/// View declaration: selects a subset of the model for diagram rendering
/// at a specific zoom level.
/// </summary>
public sealed record ViewDecl(
    ViewKind Kind,
    string? Scope,
    string Name,
    ViewFilter? Include,
    ViewFilter? Exclude,
    LayoutDirection? AutoLayout,
    string? Description,
    List<string> Tags,
    List<RationaleDecl> Rationales,
    SourceLocation Location) : TopLevelDecl(Location);

/// <summary>
/// View filter: selects elements for inclusion or exclusion.
/// </summary>
public sealed record ViewFilter(
    ViewFilterKind Kind,
    string? TagValue,
    List<string> ExplicitElements,
    SourceLocation Location) : AstNode(Location)
{
    public static ViewFilter AllFilter(SourceLocation loc) =>
        new(ViewFilterKind.All, null, [], loc);

    public static ViewFilter TaggedFilter(string tag, SourceLocation loc) =>
        new(ViewFilterKind.Tagged, tag, [], loc);

    public static ViewFilter ExplicitFilter(List<string> elements, SourceLocation loc) =>
        new(ViewFilterKind.Explicit, null, elements, loc);
}
