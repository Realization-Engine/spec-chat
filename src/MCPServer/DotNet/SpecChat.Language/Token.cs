namespace SpecChat.Language;

/// <summary>
/// A single token produced by the lexer.
/// </summary>
/// <param name="Kind">The classification of this token.</param>
/// <param name="Text">The raw lexeme as it appeared in source.</param>
/// <param name="Line">1-based line number in the source.</param>
/// <param name="Column">1-based column number in the source.</param>
public readonly record struct Token(
    TokenKind Kind,
    string Text,
    int Line,
    int Column)
{
    public override string ToString() =>
        $"{Kind}(\"{Text}\") at {Line}:{Column}";
}
