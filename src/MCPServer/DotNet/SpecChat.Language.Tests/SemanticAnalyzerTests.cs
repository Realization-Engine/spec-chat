using SpecChat.Language;
using SpecChat.Language.Ast;

namespace SpecChat.Language.Tests;

public class SemanticAnalyzerTests
{
    private static string FixturePath(string filename)
    {
        string dir = AppContext.BaseDirectory;
        while (dir != null && !File.Exists(Path.Combine(dir, "SpecChat.slnx")))
            dir = Path.GetDirectoryName(dir)!;
        return Path.Combine(dir!, "..", "..", "..", "tests", "fixtures", filename);
    }

    private static SpecDocument ParseFixture(string filename, DiagnosticBag diagnostics)
    {
        string content = File.ReadAllText(FixturePath(filename));
        var extractor = new SpecBlockExtractor();
        var blocks = extractor.Extract(content, filename);
        var allDecls = new List<TopLevelDecl>();

        foreach (var block in blocks)
        {
            var lexer = new Lexer(block.Content, block.FilePath, block.StartLine, diagnostics);
            var tokens = lexer.Tokenize();
            var parser = new Parser(tokens, block.FilePath, diagnostics);
            var doc = parser.Parse();
            allDecls.AddRange(doc.Declarations);
        }

        return new SpecDocument(allDecls,
            new SourceLocation(filename, 1, 1, 0));
    }

    [Fact]
    public void CheckTopology_ValidTopology_NoDiagnostics()
    {
        var parseDiag = new DiagnosticBag();
        var doc = ParseFixture("blazor-harness.spec.md", parseDiag);

        var semanticDiag = new DiagnosticBag();
        var analyzer = new SemanticAnalyzer(semanticDiag);
        analyzer.CheckTopology(doc);

        var errors = semanticDiag.Diagnostics
            .Where(d => d.Severity == DiagnosticSeverity.Error)
            .ToList();
        Assert.Empty(errors);
    }

    [Fact]
    public void CheckTopology_UndeclaredComponent_ReportsError()
    {
        var parseDiag = new DiagnosticBag();
        var doc = ParseFixture("broken-topology.spec.md", parseDiag);

        var semanticDiag = new DiagnosticBag();
        var analyzer = new SemanticAnalyzer(semanticDiag);
        analyzer.CheckTopology(doc);

        Assert.True(semanticDiag.HasErrors);
        Assert.Contains(semanticDiag.Diagnostics,
            d => d.Message.Contains("Gamma") && d.Severity == DiagnosticSeverity.Error);
    }

    [Fact]
    public void CheckTopology_Contradiction_ReportsError()
    {
        var parseDiag = new DiagnosticBag();
        var doc = ParseFixture("broken-topology.spec.md", parseDiag);

        var semanticDiag = new DiagnosticBag();
        var analyzer = new SemanticAnalyzer(semanticDiag);
        analyzer.CheckTopology(doc);

        Assert.Contains(semanticDiag.Diagnostics,
            d => d.Message.Contains("contradicts") && d.Severity == DiagnosticSeverity.Error);
    }

    [Fact]
    public void CheckTraces_DuplicateSource_ReportsError()
    {
        var parseDiag = new DiagnosticBag();
        var doc = ParseFixture("broken-traces.spec.md", parseDiag);

        var semanticDiag = new DiagnosticBag();
        var analyzer = new SemanticAnalyzer(semanticDiag);
        analyzer.CheckTraces(doc);

        Assert.True(semanticDiag.HasErrors);
        Assert.Contains(semanticDiag.Diagnostics,
            d => d.Message.Contains("Concept1") && d.Message.Contains("Duplicate")
                 && d.Severity == DiagnosticSeverity.Error);
    }

    [Fact]
    public void CheckTraces_EmptyTargets_ReportsWarning()
    {
        var parseDiag = new DiagnosticBag();
        var doc = ParseFixture("broken-traces.spec.md", parseDiag);

        var semanticDiag = new DiagnosticBag();
        var analyzer = new SemanticAnalyzer(semanticDiag);
        analyzer.CheckTraces(doc);

        Assert.Contains(semanticDiag.Diagnostics,
            d => d.Message.Contains("Concept2") && d.Message.Contains("no targets")
                 && d.Severity == DiagnosticSeverity.Warning);
    }

    [Fact]
    public void CheckPhaseOrdering_ValidPhases_NoDiagnostics()
    {
        var parseDiag = new DiagnosticBag();
        var doc = ParseFixture("blazor-harness.spec.md", parseDiag);

        var semanticDiag = new DiagnosticBag();
        var analyzer = new SemanticAnalyzer(semanticDiag);
        analyzer.CheckPhaseOrdering(doc);

        var errors = semanticDiag.Diagnostics
            .Where(d => d.Severity == DiagnosticSeverity.Error)
            .ToList();
        Assert.Empty(errors);
    }

    [Fact]
    public void Analyze_BlazerHarness_NoDiagnostics()
    {
        var parseDiag = new DiagnosticBag();
        var doc = ParseFixture("blazor-harness.spec.md", parseDiag);

        var semanticDiag = new DiagnosticBag();
        var analyzer = new SemanticAnalyzer(semanticDiag);
        analyzer.Analyze(doc);

        var errors = semanticDiag.Diagnostics
            .Where(d => d.Severity == DiagnosticSeverity.Error)
            .ToList();
        Assert.Empty(errors);
    }
}
