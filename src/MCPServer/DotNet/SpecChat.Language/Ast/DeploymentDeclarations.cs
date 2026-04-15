namespace SpecChat.Language.Ast;

// ── Deployment specification declarations ───────────────────────────

/// <summary>
/// Deployment declaration: a named environment (Production, Staging, etc.)
/// containing infrastructure nodes.
/// </summary>
public sealed record DeploymentDecl(
    string Name,
    List<DeploymentNodeDecl> Nodes,
    List<RationaleDecl> Rationales,
    SourceLocation Location) : TopLevelDecl(Location);

/// <summary>
/// Deployment node: an infrastructure element (server, cloud service,
/// pod, cluster) that may contain child nodes and component instances.
/// Nodes nest recursively to model infrastructure hierarchy.
/// </summary>
public sealed record DeploymentNodeDecl(
    string Name,
    string? Technology,
    string? Instance,
    List<DeploymentNodeDecl> ChildNodes,
    List<string> Tags,
    List<RationaleDecl> Rationales,
    SourceLocation Location) : AstNode(Location);
