namespace SpecChat.Language;

/// <summary>
/// The context in which a spec block appears within the markdown document.
/// </summary>
public enum SpecBlockContext
{
    /// <summary>Top-level spec block, not inside a page or visualization context.</summary>
    TopLevel,

    /// <summary>Spec block that opens or belongs to a page context.</summary>
    Page,

    /// <summary>Spec block that opens or belongs to a visualization context.</summary>
    Visualization,
}

/// <summary>
/// A single extracted spec fenced code block from a .spec.md file.
/// </summary>
/// <param name="Content">The text content inside the fence, excluding the fence markers.</param>
/// <param name="StartLine">1-based line number of the opening fence marker.</param>
/// <param name="EndLine">1-based line number of the closing fence marker.</param>
/// <param name="FilePath">Path to the source file.</param>
/// <param name="Context">Whether this block is top-level, page, or visualization.</param>
public sealed record SpecBlock(
    string Content,
    int StartLine,
    int EndLine,
    string FilePath,
    SpecBlockContext Context);

/// <summary>
/// A mermaid fenced code block found in a .spec.md file, tracked by line range.
/// Used by quality checks to verify that view, topology, and dynamic declarations
/// have companion rendered diagrams.
/// </summary>
/// <param name="StartLine">1-based line number of the opening ```mermaid fence.</param>
/// <param name="EndLine">1-based line number of the closing ``` fence.</param>
/// <param name="FilePath">Path to the source file.</param>
public sealed record MermaidBlock(
    int StartLine,
    int EndLine,
    string FilePath);

/// <summary>
/// Prose text that falls within a page or visualization context, preserved
/// for association with the parent PageDecl or VisualizationDecl in the AST.
/// </summary>
/// <param name="Text">The markdown prose text.</param>
/// <param name="StartLine">1-based line number where the prose begins.</param>
/// <param name="EndLine">1-based line number where the prose ends.</param>
/// <param name="ParentContext">Name of the parent page or visualization context.</param>
public sealed record ProseIntentBlock(
    string Text,
    int StartLine,
    int EndLine,
    string ParentContext);

/// <summary>
/// Extracts spec fenced code blocks and prose intent blocks from .spec.md markdown files.
/// A spec block starts with a line containing exactly "```spec" and ends with a line
/// containing exactly "```". The extractor also tracks heading-based page and visualization
/// contexts to classify blocks and capture prose intents.
/// </summary>
public sealed class SpecBlockExtractor
{
    private const string SpecFenceOpen = "```spec";
    private const string FenceClose = "```";

    /// <summary>
    /// Extract all spec fenced code blocks from the given markdown content.
    /// </summary>
    /// <param name="markdownContent">The full text of a .spec.md file.</param>
    /// <param name="filePath">The file path, used for diagnostics and SpecBlock metadata.</param>
    /// <param name="diagnostics">Optional bag to receive error diagnostics (e.g. unclosed fences).</param>
    /// <returns>A list of extracted spec blocks in document order.</returns>
    public List<SpecBlock> Extract(string markdownContent, string filePath, DiagnosticBag? diagnostics = null)
    {
        var blocks = new List<SpecBlock>();
        var lines = SplitLines(markdownContent);

        bool insideFence = false;
        int fenceStartLine = 0;
        var contentBuilder = new System.Text.StringBuilder();

        // Context tracking: heading level and name of the active page/visualization context.
        int contextHeadingLevel = 0;
        string? contextName = null;
        SpecBlockContext activeContext = SpecBlockContext.TopLevel;

        for (int i = 0; i < lines.Length; i++)
        {
            int lineNumber = i + 1; // 1-based
            string line = lines[i];
            string trimmed = line.Trim();

            if (insideFence)
            {
                if (trimmed == FenceClose)
                {
                    string content = contentBuilder.ToString();
                    SpecBlockContext blockContext = DetermineBlockContext(content, activeContext);

                    // If this block opens a new context, update the tracker.
                    if (blockContext == SpecBlockContext.Page)
                    {
                        // The heading that preceded this block sets the context level.
                        // contextHeadingLevel was set when we last saw a heading.
                        activeContext = SpecBlockContext.Page;
                        contextName = ExtractContextName(content);
                    }
                    else if (blockContext == SpecBlockContext.Visualization && activeContext == SpecBlockContext.Page)
                    {
                        // Visualization within a page context; keep the page context active
                        // but this block is classified as Visualization.
                    }
                    else if (blockContext == SpecBlockContext.Visualization && activeContext == SpecBlockContext.TopLevel)
                    {
                        activeContext = SpecBlockContext.Visualization;
                        contextName = ExtractContextName(content);
                    }

                    blocks.Add(new SpecBlock(content, fenceStartLine, lineNumber, filePath, blockContext));
                    insideFence = false;
                    contentBuilder.Clear();
                }
                else
                {
                    if (contentBuilder.Length > 0)
                        contentBuilder.Append('\n');
                    contentBuilder.Append(line);
                }
            }
            else
            {
                // Check for heading to track context boundaries.
                int headingLevel = GetHeadingLevel(trimmed);
                if (headingLevel > 0)
                {
                    // If we are inside a context and encounter a heading at the same
                    // or higher (numerically smaller or equal) level, close the context.
                    if (contextHeadingLevel > 0 && headingLevel <= contextHeadingLevel)
                    {
                        activeContext = SpecBlockContext.TopLevel;
                        contextName = null;
                        contextHeadingLevel = 0;
                    }

                    // Record the heading level for potential context opening by
                    // the next spec block.
                    if (activeContext == SpecBlockContext.TopLevel)
                    {
                        contextHeadingLevel = headingLevel;
                    }
                }

                if (trimmed == SpecFenceOpen)
                {
                    insideFence = true;
                    fenceStartLine = lineNumber;
                }
            }
        }

        // If we ended while still inside a fence, report the error.
        if (insideFence)
        {
            diagnostics?.ReportError(
                new SourceLocation(filePath, fenceStartLine, 1, 0),
                $"Spec block opened at line {fenceStartLine} was never closed.");
        }

        return blocks;
    }

    /// <summary>
    /// Extract prose intent blocks from the given markdown content. Prose intents
    /// are markdown text that falls within a page or visualization context (between
    /// spec blocks or after spec blocks, at a deeper heading level).
    /// </summary>
    /// <param name="markdownContent">The full text of a .spec.md file.</param>
    /// <param name="filePath">The file path (used for context; not stored in ProseIntentBlock).</param>
    /// <returns>A list of prose intent blocks in document order.</returns>
    public List<ProseIntentBlock> ExtractProseIntents(string markdownContent, string filePath)
    {
        var proseBlocks = new List<ProseIntentBlock>();
        var lines = SplitLines(markdownContent);

        bool insideFence = false;
        int contextHeadingLevel = 0;
        string? contextName = null;
        SpecBlockContext activeContext = SpecBlockContext.TopLevel;

        // Accumulate prose lines within a context.
        var proseBuilder = new System.Text.StringBuilder();
        int proseStartLine = 0;

        for (int i = 0; i < lines.Length; i++)
        {
            int lineNumber = i + 1;
            string line = lines[i];
            string trimmed = line.Trim();

            if (insideFence)
            {
                if (trimmed == FenceClose)
                {
                    insideFence = false;
                }
                continue;
            }

            // Check for heading.
            int headingLevel = GetHeadingLevel(trimmed);
            if (headingLevel > 0)
            {
                // Flush any accumulated prose before changing context.
                FlushProse(proseBlocks, proseBuilder, ref proseStartLine, lineNumber - 1, contextName, activeContext);

                if (contextHeadingLevel > 0 && headingLevel <= contextHeadingLevel)
                {
                    activeContext = SpecBlockContext.TopLevel;
                    contextName = null;
                    contextHeadingLevel = 0;
                }

                if (activeContext == SpecBlockContext.TopLevel)
                {
                    contextHeadingLevel = headingLevel;
                }
            }

            if (trimmed == SpecFenceOpen)
            {
                // Flush prose before entering a fence.
                FlushProse(proseBlocks, proseBuilder, ref proseStartLine, lineNumber - 1, contextName, activeContext);
                insideFence = true;

                // We need to peek at the block content to determine context.
                // Scan ahead to find the closing fence and peek at the first keyword.
                string blockContent = PeekBlockContent(lines, i + 1);
                SpecBlockContext blockContext = DetermineBlockContext(blockContent, activeContext);

                if (blockContext == SpecBlockContext.Page)
                {
                    activeContext = SpecBlockContext.Page;
                    contextName = ExtractContextName(blockContent);
                }
                else if (blockContext == SpecBlockContext.Visualization && activeContext != SpecBlockContext.Page)
                {
                    activeContext = SpecBlockContext.Visualization;
                    contextName = ExtractContextName(blockContent);
                }
                else if (blockContext == SpecBlockContext.Visualization && activeContext == SpecBlockContext.Page)
                {
                    // Sub-visualization within page; contextName stays as the page name
                    // but we can refine to visualization name for prose association.
                    contextName = ExtractContextName(blockContent);
                }

                continue;
            }

            // If we are inside a page or visualization context, accumulate prose.
            if (activeContext != SpecBlockContext.TopLevel && trimmed.Length > 0)
            {
                if (proseBuilder.Length == 0)
                    proseStartLine = lineNumber;

                if (proseBuilder.Length > 0)
                    proseBuilder.Append('\n');
                proseBuilder.Append(line);
            }
        }

        // Flush any trailing prose.
        FlushProse(proseBlocks, proseBuilder, ref proseStartLine, lines.Length, contextName, activeContext);

        return proseBlocks;
    }

    private static void FlushProse(
        List<ProseIntentBlock> proseBlocks,
        System.Text.StringBuilder builder,
        ref int startLine,
        int endLine,
        string? contextName,
        SpecBlockContext activeContext)
    {
        if (builder.Length > 0 && activeContext != SpecBlockContext.TopLevel && contextName is not null)
        {
            proseBlocks.Add(new ProseIntentBlock(builder.ToString(), startLine, endLine, contextName));
            builder.Clear();
        }
        else
        {
            builder.Clear();
        }
        startLine = 0;
    }

    /// <summary>
    /// Peek ahead from a given index to collect the content of the spec block
    /// (without consuming lines from the main loop).
    /// </summary>
    private static string PeekBlockContent(string[] lines, int startIndex)
    {
        var sb = new System.Text.StringBuilder();
        for (int j = startIndex; j < lines.Length; j++)
        {
            string t = lines[j].Trim();
            if (t == FenceClose)
                break;
            if (sb.Length > 0)
                sb.Append('\n');
            sb.Append(lines[j]);
        }
        return sb.ToString();
    }

    /// <summary>
    /// Determine the context of a spec block by peeking at its first keyword.
    /// </summary>
    private static SpecBlockContext DetermineBlockContext(string content, SpecBlockContext currentContext)
    {
        string firstKeyword = GetFirstKeyword(content);

        if (string.Equals(firstKeyword, "page", StringComparison.Ordinal))
            return SpecBlockContext.Page;

        if (string.Equals(firstKeyword, "visualization", StringComparison.Ordinal))
            return SpecBlockContext.Visualization;

        return currentContext;
    }

    /// <summary>
    /// Extract the context name from the spec block content (the identifier after the
    /// first keyword, e.g. "page ExecutiveDashboard { ... }" yields "ExecutiveDashboard").
    /// </summary>
    private static string? ExtractContextName(string content)
    {
        // Skip leading whitespace to the first keyword, then grab the next token.
        int pos = 0;
        int len = content.Length;

        // Skip whitespace.
        while (pos < len && char.IsWhiteSpace(content[pos])) pos++;

        // Skip first keyword.
        while (pos < len && !char.IsWhiteSpace(content[pos])) pos++;

        // Skip whitespace between keyword and name.
        while (pos < len && char.IsWhiteSpace(content[pos])) pos++;

        // Read the identifier.
        int nameStart = pos;
        while (pos < len && (char.IsLetterOrDigit(content[pos]) || content[pos] == '_' || content[pos] == '.'))
            pos++;

        if (pos > nameStart)
            return content.Substring(nameStart, pos - nameStart);

        return null;
    }

    /// <summary>
    /// Get the first non-whitespace word from the content, which is the first keyword
    /// of the spec block.
    /// </summary>
    private static string GetFirstKeyword(string content)
    {
        int pos = 0;
        int len = content.Length;

        // Skip leading whitespace/newlines.
        while (pos < len && char.IsWhiteSpace(content[pos])) pos++;

        int start = pos;
        while (pos < len && !char.IsWhiteSpace(content[pos]) && content[pos] != '{')
            pos++;

        if (pos > start)
            return content.Substring(start, pos - start);

        return string.Empty;
    }

    /// <summary>
    /// Determine the ATX heading level of a line (number of leading '#' characters
    /// followed by a space). Returns 0 if the line is not a heading.
    /// </summary>
    private static int GetHeadingLevel(string trimmedLine)
    {
        if (trimmedLine.Length == 0 || trimmedLine[0] != '#')
            return 0;

        int level = 0;
        while (level < trimmedLine.Length && trimmedLine[level] == '#')
            level++;

        // ATX headings require a space after the '#' characters (or end of line for empty headings).
        if (level >= trimmedLine.Length || trimmedLine[level] == ' ')
            return level;

        return 0;
    }

    /// <summary>
    /// Extract all mermaid fenced code blocks from the given markdown content.
    /// Returns only line ranges, not content. Used by quality checks to verify
    /// that view, topology, and dynamic declarations have companion diagrams.
    /// </summary>
    public List<MermaidBlock> ExtractMermaidBlocks(string markdownContent, string filePath)
    {
        const string mermaidFenceOpen = "```mermaid";

        var blocks = new List<MermaidBlock>();
        var lines = SplitLines(markdownContent);

        bool insideFence = false;
        bool insideMermaid = false;
        int fenceStartLine = 0;

        for (int i = 0; i < lines.Length; i++)
        {
            int lineNumber = i + 1; // 1-based
            string trimmed = lines[i].Trim();

            if (insideFence)
            {
                if (trimmed == FenceClose)
                {
                    if (insideMermaid)
                        blocks.Add(new MermaidBlock(fenceStartLine, lineNumber, filePath));
                    insideFence = false;
                    insideMermaid = false;
                }
            }
            else
            {
                if (trimmed == mermaidFenceOpen)
                {
                    insideFence = true;
                    insideMermaid = true;
                    fenceStartLine = lineNumber;
                }
                else if (trimmed.StartsWith("```") && trimmed.Length > 3)
                {
                    // Some other fenced block (spec, csharp, json, etc.); skip it
                    insideFence = true;
                    insideMermaid = false;
                    fenceStartLine = lineNumber;
                }
            }
        }

        return blocks;
    }

    /// <summary>
    /// Split content into lines, handling \r\n, \n, and \r line endings.
    /// </summary>
    private static string[] SplitLines(string content)
    {
        return content.Split(["\r\n", "\n", "\r"], StringSplitOptions.None);
    }
}
