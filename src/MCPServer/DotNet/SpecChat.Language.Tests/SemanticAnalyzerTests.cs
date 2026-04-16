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

    // ═══════════════════════════════════════════════════════════════
    //  DIAGRAM COMPANION CHECKS
    // ═══════════════════════════════════════════════════════════════

    private static (SpecDocument Doc, List<SpecBlock> SpecBlocks, List<MermaidBlock> MermaidBlocks)
        ParseMarkdown(string markdown, DiagnosticBag diagnostics)
    {
        var extractor = new SpecBlockExtractor();
        var specBlocks = extractor.Extract(markdown, "test.spec.md", diagnostics);
        var mermaidBlocks = extractor.ExtractMermaidBlocks(markdown, "test.spec.md");
        var allDecls = new List<TopLevelDecl>();

        foreach (var block in specBlocks)
        {
            var lexer = new Lexer(block.Content, block.FilePath, block.StartLine, diagnostics);
            var tokens = lexer.Tokenize();
            var parser = new Parser(tokens, block.FilePath, diagnostics);
            var doc = parser.Parse();
            allDecls.AddRange(doc.Declarations);
        }

        var document = new SpecDocument(allDecls, new SourceLocation("test.spec.md", 1, 1, 0));
        return (document, specBlocks, mermaidBlocks);
    }

    [Fact]
    public void CheckDiagramCompanions_ViewWithMermaid_NoWarning()
    {
        string markdown = @"# Views

```spec
view systemContext of App ContextView {
    include: all;
    description: ""The system context."";
}
```

Rendered system context:

```mermaid
C4Context
    title System Context
```
";
        var parseDiag = new DiagnosticBag();
        var (doc, specBlocks, mermaidBlocks) = ParseMarkdown(markdown, parseDiag);

        var semanticDiag = new DiagnosticBag();
        var analyzer = new SemanticAnalyzer(semanticDiag);
        analyzer.CheckDiagramCompanions(doc, specBlocks, mermaidBlocks);

        var warnings = semanticDiag.Diagnostics
            .Where(d => d.Message.Contains("companion mermaid"))
            .ToList();
        Assert.Empty(warnings);
    }

    [Fact]
    public void CheckDiagramCompanions_ViewWithoutMermaid_Warning()
    {
        string markdown = @"# Views

```spec
view systemContext of App ContextView {
    include: all;
    description: ""The system context."";
}
```

No diagram here, just prose.
";
        var parseDiag = new DiagnosticBag();
        var (doc, specBlocks, mermaidBlocks) = ParseMarkdown(markdown, parseDiag);

        var semanticDiag = new DiagnosticBag();
        var analyzer = new SemanticAnalyzer(semanticDiag);
        analyzer.CheckDiagramCompanions(doc, specBlocks, mermaidBlocks);

        var warnings = semanticDiag.Diagnostics
            .Where(d => d.Message.Contains("companion mermaid"))
            .ToList();
        Assert.Single(warnings);
        Assert.Contains("ContextView", warnings[0].Message);
    }

    [Fact]
    public void CheckDiagramCompanions_DynamicWithoutMermaid_Warning()
    {
        string markdown = @"# Dynamic

```spec
dynamic PlaceOrder {
    1: User -> App : ""Opens page."";
    2: App -> User : ""Shows result."";
}
```
";
        var parseDiag = new DiagnosticBag();
        var (doc, specBlocks, mermaidBlocks) = ParseMarkdown(markdown, parseDiag);

        var semanticDiag = new DiagnosticBag();
        var analyzer = new SemanticAnalyzer(semanticDiag);
        analyzer.CheckDiagramCompanions(doc, specBlocks, mermaidBlocks);

        var warnings = semanticDiag.Diagnostics
            .Where(d => d.Message.Contains("companion mermaid"))
            .ToList();
        Assert.Single(warnings);
        Assert.Contains("PlaceOrder", warnings[0].Message);
    }

    [Fact]
    public void CheckDiagramCompanions_TopologyWithoutMermaid_Warning()
    {
        string markdown = @"# Topology

```spec
topology Deps {
    allow App -> Engine;
    deny Engine -> App;
}
```
";
        var parseDiag = new DiagnosticBag();
        var (doc, specBlocks, mermaidBlocks) = ParseMarkdown(markdown, parseDiag);

        var semanticDiag = new DiagnosticBag();
        var analyzer = new SemanticAnalyzer(semanticDiag);
        analyzer.CheckDiagramCompanions(doc, specBlocks, mermaidBlocks);

        var warnings = semanticDiag.Diagnostics
            .Where(d => d.Message.Contains("companion mermaid"))
            .ToList();
        Assert.Single(warnings);
        Assert.Contains("Deps", warnings[0].Message);
    }
}
