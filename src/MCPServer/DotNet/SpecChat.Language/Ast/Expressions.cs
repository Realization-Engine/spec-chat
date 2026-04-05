namespace SpecChat.Language.Ast;

/// <summary>
/// Discriminates the kind of a literal value.
/// </summary>
public enum LiteralKind
{
    Integer,
    Float,
    String,
    Boolean,
}

/// <summary>
/// Discriminates the quantifier form in a QuantifierExpr.
/// </summary>
public enum Quantifier
{
    All,
    Exists,
    Count,
}

// ── Expression base ──────────────────────────────────────────────────

/// <summary>
/// Base type for all expression nodes (invariant conditions, contract
/// clauses, gate expectations, parameter bindings).
/// </summary>
public abstract record Expr(SourceLocation Location) : AstNode(Location);

// ── Concrete expression nodes ────────────────────────────────────────

/// <summary>
/// Binary expression: ==, !=, &lt;, &gt;, &lt;=, &gt;=, and, or,
/// implies, contains, excludes, in.
/// </summary>
public sealed record BinaryExpr(
    Expr Left,
    string Operator,
    Expr Right,
    SourceLocation Location) : Expr(Location);

/// <summary>
/// Unary expression: not.
/// </summary>
public sealed record UnaryExpr(
    string Operator,
    Expr Operand,
    SourceLocation Location) : Expr(Location);

/// <summary>
/// Dot member access: Target.MemberName.
/// </summary>
public sealed record AccessExpr(
    Expr Target,
    string MemberName,
    SourceLocation Location) : Expr(Location);

/// <summary>
/// Method call: Target.MemberName(Args).
/// </summary>
public sealed record CallExpr(
    Expr Target,
    string MemberName,
    List<Expr> Args,
    SourceLocation Location) : Expr(Location);

/// <summary>
/// Literal value: integer, float, string, or boolean.
/// </summary>
public sealed record LiteralExpr(
    object Value,
    LiteralKind Kind,
    SourceLocation Location) : Expr(Location);

/// <summary>
/// Bare identifier reference.
/// </summary>
public sealed record IdentifierExpr(
    string Name,
    SourceLocation Location) : Expr(Location);

/// <summary>
/// List literal: [elem1, elem2, ...].
/// </summary>
public sealed record ListExpr(
    List<Expr> Elements,
    SourceLocation Location) : Expr(Location);

/// <summary>
/// Quantifier expression: all/exists scope have/satisfy body,
/// or count(body).
/// </summary>
public sealed record QuantifierExpr(
    Quantifier Quantifier,
    List<string> ScopeWords,
    Expr Body,
    SourceLocation Location) : Expr(Location);
