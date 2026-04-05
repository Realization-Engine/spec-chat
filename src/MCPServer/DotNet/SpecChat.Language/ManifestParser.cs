namespace SpecChat.Language;

/// <summary>
/// A parsed .manifest.md document containing system metadata,
/// lifecycle states, spec inventory, and execution ordering.
/// </summary>
public sealed record ManifestDocument(
    string System,
    string BaseSpec,
    string Target,
    int SpecCount,
    List<LifecycleState> LifecycleStates,
    List<SpecEntry> Inventory,
    List<ExecutionTier> ExecutionOrder,
    string FilePath);

/// <summary>
/// An entry in the spec inventory table.
/// </summary>
public sealed record SpecEntry(
    string Filename,
    string Type,
    string State,
    string Tier,
    string Dependencies);

/// <summary>
/// An execution tier grouping specs that can be executed in parallel.
/// </summary>
public sealed record ExecutionTier(
    int TierNumber,
    string Description,
    List<string> Specs);

/// <summary>
/// A lifecycle state definition from the manifest.
/// </summary>
public sealed record LifecycleState(
    string Name,
    string Meaning,
    string TrackedBy);

/// <summary>
/// Parses .manifest.md files (structured markdown with tables and headings)
/// into a <see cref="ManifestDocument"/>.
/// </summary>
public sealed class ManifestParser
{
    private static readonly string[] ValidStates =
        ["Draft", "Reviewed", "Approved", "Executed", "Verified"];

    private readonly DiagnosticBag? _diagnostics;

    public ManifestParser(DiagnosticBag? diagnostics = null)
    {
        _diagnostics = diagnostics;
    }

    /// <summary>
    /// Parses the given markdown content as a manifest document.
    /// </summary>
    public ManifestDocument Parse(string markdownContent, string filePath)
    {
        string[] lines = markdownContent.Split('\n');

        Dictionary<string, string> systemFields = ParseSystemTable(lines);
        List<LifecycleState> lifecycleStates = ParseLifecycleStatesTable(lines);
        List<SpecEntry> inventory = ParseInventoryTable(lines, filePath);
        List<ExecutionTier> executionOrder = ParseExecutionOrder(lines);

        string system = systemFields.GetValueOrDefault("System", "");
        string baseSpec = systemFields.GetValueOrDefault("Base spec", "");
        string target = systemFields.GetValueOrDefault("Target", "");
        _ = int.TryParse(systemFields.GetValueOrDefault("Spec count", "0"), out int specCount);

        ValidateStates(inventory, filePath);
        ValidateDependencies(inventory, filePath);

        return new ManifestDocument(
            system, baseSpec, target, specCount,
            lifecycleStates, inventory, executionOrder, filePath);
    }

    // ── System table ────────────────────────────────────────────────

    private static Dictionary<string, string> ParseSystemTable(string[] lines)
    {
        // Find the "## System" heading, then parse the key-value table that follows.
        int start = FindHeading(lines, "## System");
        if (start < 0)
            return new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        return ParseKeyValueTable(lines, start + 1);
    }

    // ── Lifecycle States table ──────────────────────────────────────

    private List<LifecycleState> ParseLifecycleStatesTable(string[] lines)
    {
        int start = FindHeading(lines, "## Lifecycle States");
        if (start < 0)
            return [];

        List<Dictionary<string, string>> rows = ParseColumnarTable(lines, start + 1);
        List<LifecycleState> states = new(rows.Count);

        foreach (Dictionary<string, string> row in rows)
        {
            states.Add(new LifecycleState(
                row.GetValueOrDefault("State", ""),
                row.GetValueOrDefault("Meaning", ""),
                row.GetValueOrDefault("Tracked by", "")));
        }

        return states;
    }

    // ── Spec Inventory table ────────────────────────────────────────

    private static List<SpecEntry> ParseInventoryTable(string[] lines, string filePath)
    {
        int start = FindHeading(lines, "## Spec Inventory");
        if (start < 0)
            return [];

        List<Dictionary<string, string>> rows = ParseColumnarTable(lines, start + 1);
        List<SpecEntry> entries = new(rows.Count);

        foreach (Dictionary<string, string> row in rows)
        {
            entries.Add(new SpecEntry(
                row.GetValueOrDefault("Filename", ""),
                row.GetValueOrDefault("Type", ""),
                row.GetValueOrDefault("State", ""),
                row.GetValueOrDefault("Tier", ""),
                row.GetValueOrDefault("Dependencies", "")));
        }

        return entries;
    }

    // ── Execution Order ─────────────────────────────────────────────

    private static List<ExecutionTier> ParseExecutionOrder(string[] lines)
    {
        int start = FindHeading(lines, "## Execution Order");
        if (start < 0)
            return [];

        List<ExecutionTier> tiers = [];
        int tierNumber = -1;
        string tierDescription = "";
        List<string> tierSpecs = [];

        for (int i = start + 1; i < lines.Length; i++)
        {
            string line = lines[i].TrimEnd('\r');

            // Stop at the next ## heading.
            if (line.StartsWith("## ", StringComparison.Ordinal) && !line.StartsWith("### ", StringComparison.Ordinal))
                break;

            // Detect ### Tier N: headings.
            if (line.StartsWith("### Tier ", StringComparison.Ordinal))
            {
                // Flush previous tier.
                if (tierNumber >= 0)
                    tiers.Add(new ExecutionTier(tierNumber, tierDescription, tierSpecs));

                (tierNumber, tierDescription) = ParseTierHeading(line);
                tierSpecs = [];
                continue;
            }

            // Detect numbered spec entries: "1. filename.spec.md" or "1. filename.spec.md (notes)"
            string trimmed = line.TrimStart();
            if (trimmed.Length > 0 && char.IsDigit(trimmed[0]))
            {
                int dotIdx = trimmed.IndexOf(". ", StringComparison.Ordinal);
                if (dotIdx >= 0)
                {
                    string specPart = trimmed[(dotIdx + 2)..].Trim();
                    // Extract just the filename (before any parenthetical).
                    int parenIdx = specPart.IndexOf('(');
                    string specFilename = parenIdx >= 0
                        ? specPart[..parenIdx].Trim()
                        : specPart.Trim();
                    if (specFilename.Length > 0)
                        tierSpecs.Add(specFilename);
                }
            }
        }

        // Flush last tier.
        if (tierNumber >= 0)
            tiers.Add(new ExecutionTier(tierNumber, tierDescription, tierSpecs));

        return tiers;
    }

    // ── Validation ──────────────────────────────────────────────────

    private void ValidateStates(List<SpecEntry> inventory, string filePath)
    {
        if (_diagnostics is null)
            return;

        foreach (SpecEntry entry in inventory)
        {
            if (entry.State.Length == 0)
                continue;

            bool valid = false;
            for (int i = 0; i < ValidStates.Length; i++)
            {
                if (string.Equals(entry.State, ValidStates[i], StringComparison.OrdinalIgnoreCase))
                {
                    valid = true;
                    break;
                }
            }

            if (!valid)
            {
                _diagnostics.ReportError(
                    new SourceLocation(filePath, 0, 0, 0),
                    $"Spec '{entry.Filename}' has unrecognized state '{entry.State}'. " +
                    "Valid states are: Draft, Reviewed, Approved, Executed, Verified.");
            }
        }
    }

    private void ValidateDependencies(List<SpecEntry> inventory, string filePath)
    {
        if (_diagnostics is null)
            return;

        HashSet<string> knownFilenames = new(inventory.Count, StringComparer.OrdinalIgnoreCase);
        foreach (SpecEntry entry in inventory)
        {
            if (entry.Filename.Length > 0)
                knownFilenames.Add(entry.Filename);
        }

        foreach (SpecEntry entry in inventory)
        {
            if (string.IsNullOrWhiteSpace(entry.Dependencies) ||
                string.Equals(entry.Dependencies, "None", StringComparison.OrdinalIgnoreCase))
                continue;

            string[] deps = entry.Dependencies.Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
            foreach (string dep in deps)
            {
                if (!knownFilenames.Contains(dep))
                {
                    _diagnostics.ReportWarning(
                        new SourceLocation(filePath, 0, 0, 0),
                        $"Spec '{entry.Filename}' lists dependency '{dep}', " +
                        "which is not in the inventory.");
                }
            }
        }
    }

    // ── Table parsing helpers ───────────────────────────────────────

    /// <summary>
    /// Finds the line index of a heading (e.g., "## System").
    /// Returns -1 if not found.
    /// </summary>
    private static int FindHeading(string[] lines, string heading)
    {
        for (int i = 0; i < lines.Length; i++)
        {
            string trimmed = lines[i].TrimEnd('\r').Trim();
            if (trimmed.Equals(heading, StringComparison.OrdinalIgnoreCase))
                return i;

            // Also match headings like "## Lifecycle States" that may have trailing text.
            if (trimmed.StartsWith(heading, StringComparison.OrdinalIgnoreCase))
            {
                // Exact match or followed by whitespace only.
                if (trimmed.Length == heading.Length ||
                    char.IsWhiteSpace(trimmed[heading.Length]))
                    return i;
            }
        }
        return -1;
    }

    /// <summary>
    /// Parses a two-column key-value markdown table (Field | Value)
    /// starting at the given line index. Skips separator lines (|---|---|).
    /// </summary>
    private static Dictionary<string, string> ParseKeyValueTable(string[] lines, int startLine)
    {
        Dictionary<string, string> result = new(StringComparer.OrdinalIgnoreCase);
        bool foundHeader = false;
        bool skippedSeparator = false;

        for (int i = startLine; i < lines.Length; i++)
        {
            string line = lines[i].TrimEnd('\r').Trim();

            if (line.Length == 0)
            {
                if (foundHeader && skippedSeparator)
                    break;
                continue;
            }

            if (!line.Contains('|'))
            {
                if (foundHeader && skippedSeparator)
                    break;
                continue;
            }

            // Skip separator lines like |---|---|
            if (IsSeparatorLine(line))
            {
                skippedSeparator = true;
                continue;
            }

            string[] cells = SplitTableRow(line);
            if (cells.Length < 2)
                continue;

            // Skip the header row.
            if (!foundHeader)
            {
                foundHeader = true;
                continue;
            }

            result[cells[0].Trim()] = cells[1].Trim();
        }

        return result;
    }

    /// <summary>
    /// Parses a multi-column markdown table into a list of row dictionaries.
    /// Column names come from the header row.
    /// </summary>
    private static List<Dictionary<string, string>> ParseColumnarTable(string[] lines, int startLine)
    {
        List<Dictionary<string, string>> rows = [];
        string[]? headers = null;

        for (int i = startLine; i < lines.Length; i++)
        {
            string line = lines[i].TrimEnd('\r').Trim();

            if (line.Length == 0)
            {
                if (headers is not null)
                    break;
                continue;
            }

            if (!line.Contains('|'))
            {
                if (headers is not null)
                    break;
                continue;
            }

            if (IsSeparatorLine(line))
                continue;

            string[] cells = SplitTableRow(line);

            if (headers is null)
            {
                headers = new string[cells.Length];
                for (int c = 0; c < cells.Length; c++)
                    headers[c] = cells[c].Trim();
                continue;
            }

            Dictionary<string, string> row = new(StringComparer.OrdinalIgnoreCase);
            for (int c = 0; c < headers.Length && c < cells.Length; c++)
                row[headers[c]] = cells[c].Trim();

            rows.Add(row);
        }

        return rows;
    }

    /// <summary>
    /// Splits a markdown table row by pipe characters, ignoring leading/trailing pipes.
    /// </summary>
    private static string[] SplitTableRow(string line)
    {
        // Trim leading/trailing pipes and split.
        ReadOnlySpan<char> span = line.AsSpan().Trim();
        if (span.Length > 0 && span[0] == '|')
            span = span[1..];
        if (span.Length > 0 && span[^1] == '|')
            span = span[..^1];

        // Split by '|'.
        string inner = span.ToString();
        return inner.Split('|');
    }

    /// <summary>
    /// Determines whether a line is a markdown table separator (e.g., |---|---|).
    /// </summary>
    private static bool IsSeparatorLine(string line)
    {
        for (int i = 0; i < line.Length; i++)
        {
            char c = line[i];
            if (c != '|' && c != '-' && c != ':' && c != ' ' && c != '\t')
                return false;
        }
        return line.Contains('-');
    }

    /// <summary>
    /// Parses a "### Tier N: description" heading into number and description.
    /// </summary>
    private static (int Number, string Description) ParseTierHeading(string line)
    {
        // Expected: "### Tier N: description (status)"
        ReadOnlySpan<char> span = line.AsSpan();

        // Skip "### Tier "
        int tierIdx = line.IndexOf("Tier ", StringComparison.OrdinalIgnoreCase);
        if (tierIdx < 0)
            return (0, line);

        span = span[(tierIdx + 5)..];

        // Parse number.
        int numEnd = 0;
        while (numEnd < span.Length && char.IsDigit(span[numEnd]))
            numEnd++;

        int number = 0;
        if (numEnd > 0)
            _ = int.TryParse(span[..numEnd], out number);

        // Skip ": " to get description.
        span = span[numEnd..];
        if (span.Length > 0 && span[0] == ':')
            span = span[1..];
        string description = span.ToString().Trim();

        return (number, description);
    }
}
