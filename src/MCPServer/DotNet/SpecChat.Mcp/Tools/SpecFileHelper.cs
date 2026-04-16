using System.Text.Json;
using SpecChat.Language;
using SpecChat.Language.Ast;

namespace SpecChat.Mcp.Tools;

/// <summary>
/// Shared helpers for the common pattern of reading a .spec.md file,
/// extracting spec blocks, lexing, parsing, and returning a SpecDocument.
/// </summary>
internal static class SpecFileHelper
{
    internal static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
    };

    /// <summary>
    /// Parse a .spec.md file into a SpecDocument plus accumulated diagnostics.
    /// </summary>
    public static (SpecDocument Document, DiagnosticBag Diagnostics) ParseFile(string filePath)
    {
        var diagnostics = new DiagnosticBag();
        string content = File.ReadAllText(filePath);

        var extractor = new SpecBlockExtractor();
        var blocks = extractor.Extract(content, filePath, diagnostics);

        var allDeclarations = new List<TopLevelDecl>();

        foreach (var block in blocks)
        {
            // The baseLineOffset is the 0-based line where the block content starts
            // (the line after the opening fence marker).
            int baseLineOffset = block.StartLine; // StartLine is 1-based line of the fence opener
            var lexer = new Lexer(block.Content, filePath, baseLineOffset, diagnostics);
            var tokens = lexer.Tokenize();

            var parser = new Parser(tokens, filePath, diagnostics);
            var doc = parser.Parse();

            allDeclarations.AddRange(doc.Declarations);
        }

        var location = new SourceLocation(filePath, 1, 1, 0);
        var document = new SpecDocument(allDeclarations, location);
        return (document, diagnostics);
    }

    /// <summary>
    /// Parse a .spec.md file and also return the extracted spec and mermaid blocks,
    /// for quality checks that need markdown-level context (e.g., diagram companion checks).
    /// </summary>
    public static (SpecDocument Document, DiagnosticBag Diagnostics,
                    List<SpecBlock> SpecBlocks, List<MermaidBlock> MermaidBlocks)
        ParseFileWithBlocks(string filePath)
    {
        var diagnostics = new DiagnosticBag();
        string content = File.ReadAllText(filePath);

        var extractor = new SpecBlockExtractor();
        var specBlocks = extractor.Extract(content, filePath, diagnostics);
        var mermaidBlocks = extractor.ExtractMermaidBlocks(content, filePath);

        var allDeclarations = new List<TopLevelDecl>();

        foreach (var block in specBlocks)
        {
            int baseLineOffset = block.StartLine;
            var lexer = new Lexer(block.Content, filePath, baseLineOffset, diagnostics);
            var tokens = lexer.Tokenize();

            var parser = new Parser(tokens, filePath, diagnostics);
            var doc = parser.Parse();

            allDeclarations.AddRange(doc.Declarations);
        }

        var location = new SourceLocation(filePath, 1, 1, 0);
        var document = new SpecDocument(allDeclarations, location);
        return (document, diagnostics, specBlocks, mermaidBlocks);
    }

    /// <summary>
    /// Serialize diagnostics to a JSON-friendly list of objects.
    /// </summary>
    public static List<Dictionary<string, object>> SerializeDiagnostics(DiagnosticBag diagnostics)
    {
        var result = new List<Dictionary<string, object>>();
        foreach (var diag in diagnostics.Diagnostics)
        {
            result.Add(new Dictionary<string, object>
            {
                ["severity"] = diag.Severity.ToString(),
                ["message"] = diag.Message,
                ["location"] = diag.Location.ToString(),
            });
        }
        return result;
    }

    /// <summary>
    /// Return a JSON error response for file-not-found or other I/O failures.
    /// </summary>
    public static string ErrorJson(string error)
    {
        return JsonSerializer.Serialize(new Dictionary<string, object>
        {
            ["error"] = error,
        }, JsonOptions);
    }
}
