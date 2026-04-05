namespace SpecChat.Language;

/// <summary>
/// Pinpoints a position in a spec block for diagnostic reporting.
/// </summary>
/// <param name="FilePath">Path to the file containing the spec block.</param>
/// <param name="Line">1-based line number.</param>
/// <param name="Column">1-based column number.</param>
/// <param name="Offset">Character offset from the start of the spec block.</param>
public readonly record struct SourceLocation(
    string FilePath,
    int Line,
    int Column,
    int Offset)
{
    public override string ToString() =>
        $"{FilePath}({Line},{Column})";
}
