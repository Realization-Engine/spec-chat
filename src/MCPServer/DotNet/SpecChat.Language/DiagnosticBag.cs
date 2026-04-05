namespace SpecChat.Language;

/// <summary>
/// Severity level for a diagnostic message.
/// </summary>
public enum DiagnosticSeverity
{
    Info,
    Warning,
    Error,
}

/// <summary>
/// A single diagnostic produced during lexing or parsing.
/// </summary>
/// <param name="Severity">How severe the issue is.</param>
/// <param name="Message">Human-readable description of the problem.</param>
/// <param name="Location">Where in the source the problem was found.</param>
public sealed record Diagnostic(
    DiagnosticSeverity Severity,
    string Message,
    SourceLocation Location);

/// <summary>
/// Accumulates diagnostics produced during lexing or parsing.
/// </summary>
public sealed class DiagnosticBag
{
    private readonly List<Diagnostic> _diagnostics = [];

    /// <summary>All diagnostics reported so far.</summary>
    public IReadOnlyList<Diagnostic> Diagnostics => _diagnostics;

    /// <summary>True if any Error-severity diagnostic has been reported.</summary>
    public bool HasErrors
    {
        get
        {
            for (int i = 0; i < _diagnostics.Count; i++)
            {
                if (_diagnostics[i].Severity == DiagnosticSeverity.Error)
                    return true;
            }
            return false;
        }
    }

    /// <summary>Report an error at the given location.</summary>
    public void ReportError(SourceLocation location, string message) =>
        _diagnostics.Add(new Diagnostic(DiagnosticSeverity.Error, message, location));

    /// <summary>Report a warning at the given location.</summary>
    public void ReportWarning(SourceLocation location, string message) =>
        _diagnostics.Add(new Diagnostic(DiagnosticSeverity.Warning, message, location));

    /// <summary>Remove all previously reported diagnostics.</summary>
    public void Clear() => _diagnostics.Clear();
}
