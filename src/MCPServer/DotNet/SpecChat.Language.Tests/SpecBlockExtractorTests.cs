using SpecChat.Language;

namespace SpecChat.Language.Tests;

public class SpecBlockExtractorTests
{
    private static string FixturePath(string filename)
    {
        string dir = AppContext.BaseDirectory;
        while (dir != null && !File.Exists(Path.Combine(dir, "SpecChat.slnx")))
            dir = Path.GetDirectoryName(dir)!;
        return Path.Combine(dir!, "..", "..", "..", "tests", "fixtures", filename);
    }

    [Fact]
    public void Extract_SingleBlock_ReturnsOneBlock()
    {
        string markdown = @"# Title

Some text.

```spec
entity Foo {
    name: string;
}
```

More text.
";
        var extractor = new SpecBlockExtractor();
        var blocks = extractor.Extract(markdown, "test.spec.md");

        Assert.Single(blocks);
        Assert.Contains("entity Foo", blocks[0].Content);
    }

    [Fact]
    public void Extract_MultipleBlocks_ReturnsAll()
    {
        string markdown = @"# Title

```spec
entity A { name: string; }
```

Some prose.

```spec
entity B { value: int; }
```

More prose.

```spec
enum Status { active: ""Active""; }
```
";
        var extractor = new SpecBlockExtractor();
        var blocks = extractor.Extract(markdown, "test.spec.md");

        Assert.Equal(3, blocks.Count);
    }

    [Fact]
    public void Extract_UnclosedBlock_ReportsDiagnostic()
    {
        string markdown = @"# Title

```spec
entity Foo {
    name: string;
}
";
        var extractor = new SpecBlockExtractor();
        var diagnostics = new DiagnosticBag();
        var blocks = extractor.Extract(markdown, "test.spec.md", diagnostics);

        Assert.Empty(blocks);
        Assert.True(diagnostics.HasErrors);
        Assert.Contains(diagnostics.Diagnostics,
            d => d.Message.Contains("never closed"));
    }

    [Fact]
    public void Extract_NoBlocks_ReturnsEmpty()
    {
        string markdown = @"# Title

This is plain markdown with no spec fences.

## Section

More text here.
";
        var extractor = new SpecBlockExtractor();
        var blocks = extractor.Extract(markdown, "test.spec.md");

        Assert.Empty(blocks);
    }

    [Fact]
    public void ExtractMermaidBlocks_FindsMermaidFences()
    {
        string markdown = """
            # Title

            ```spec
            entity Foo { id: int; }
            ```

            Rendered diagram:

            ```mermaid
            flowchart LR
                A --> B
            ```

            ## Next Section

            ```mermaid
            sequenceDiagram
                A->>B: hello
            ```
            """;
        var extractor = new SpecBlockExtractor();
        var blocks = extractor.ExtractMermaidBlocks(markdown, "test.spec.md");

        Assert.Equal(2, blocks.Count);
        Assert.True(blocks[0].StartLine < blocks[1].StartLine);
    }

    [Fact]
    public void ExtractMermaidBlocks_IgnoresOtherFences()
    {
        string markdown = """
            # Title

            ```spec
            entity Foo { id: int; }
            ```

            ```csharp
            var x = 1;
            ```

            ```json
            { "key": "value" }
            ```
            """;
        var extractor = new SpecBlockExtractor();
        var blocks = extractor.ExtractMermaidBlocks(markdown, "test.spec.md");

        Assert.Empty(blocks);
    }

    [Fact]
    public void Extract_BlazerHarness_ExtractsAllBlocks()
    {
        string content = File.ReadAllText(FixturePath("blazor-harness.spec.md"));
        var extractor = new SpecBlockExtractor();
        var diagnostics = new DiagnosticBag();
        var blocks = extractor.Extract(content, "blazor-harness.spec.md", diagnostics);

        // The file has 70 spec blocks covering system, dotnet solution, package policy,
        // topology, phases, traces, constraints, entities, pages, and visualizations.
        Assert.True(blocks.Count >= 7,
            $"Expected at least 7 blocks but got {blocks.Count}");
        Assert.False(diagnostics.HasErrors,
            $"Extraction produced errors: {string.Join("; ", diagnostics.Diagnostics.Select(d => d.Message))}");
    }
}
