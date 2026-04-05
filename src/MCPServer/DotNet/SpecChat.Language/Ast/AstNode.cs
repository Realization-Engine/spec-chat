namespace SpecChat.Language.Ast;

/// <summary>
/// Base type for all AST nodes. Every node carries a source location
/// for diagnostic reporting.
/// </summary>
public abstract record AstNode(SourceLocation Location);
