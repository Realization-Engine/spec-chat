using System.Collections.Frozen;
using System.Text;

namespace SpecChat.Language;

/// <summary>
/// Converts raw spec-block text into a stream of <see cref="Token"/> values.
/// </summary>
public sealed class Lexer
{
    private readonly string _source;
    private readonly string _filePath;
    private readonly int _baseLineOffset;
    private readonly DiagnosticBag _diagnostics;

    private int _pos;
    private int _line;
    private int _column;

    private static readonly FrozenDictionary<string, TokenKind> Keywords =
        new Dictionary<string, TokenKind>
        {
            // Data specification keywords
            ["entity"] = TokenKind.KwEntity,
            ["enum"] = TokenKind.KwEnum,
            ["invariant"] = TokenKind.KwInvariant,
            ["contract"] = TokenKind.KwContract,
            ["requires"] = TokenKind.KwRequires,
            ["ensures"] = TokenKind.KwEnsures,
            ["guarantees"] = TokenKind.KwGuarantees,
            ["refines"] = TokenKind.KwRefines,
            ["as"] = TokenKind.KwAs,
            ["rationale"] = TokenKind.KwRationale,
            ["context"] = TokenKind.KwContext,
            ["decision"] = TokenKind.KwDecision,
            ["consequence"] = TokenKind.KwConsequence,
            ["supersedes"] = TokenKind.KwSupersedes,

            // Expression keywords
            ["implies"] = TokenKind.KwImplies,
            ["and"] = TokenKind.KwAnd,
            ["or"] = TokenKind.KwOr,
            ["not"] = TokenKind.KwNot,
            ["contains"] = TokenKind.KwContains,
            ["excludes"] = TokenKind.KwExcludes,
            ["in"] = TokenKind.KwIn,
            ["all"] = TokenKind.KwAll,
            ["exists"] = TokenKind.KwExists,
            ["have"] = TokenKind.KwHave,
            ["satisfy"] = TokenKind.KwSatisfy,
            ["count"] = TokenKind.KwCount,

            // Systems specification keywords
            ["system"] = TokenKind.KwSystem,
            ["target"] = TokenKind.KwTarget,
            ["responsibility"] = TokenKind.KwResponsibility,
            ["authored"] = TokenKind.KwAuthored,
            ["consumed"] = TokenKind.KwConsumed,
            ["component"] = TokenKind.KwComponent,
            ["kind"] = TokenKind.KwKind,
            ["source"] = TokenKind.KwSource,
            ["version"] = TokenKind.KwVersion,
            ["used_by"] = TokenKind.KwUsedBy,
            ["topology"] = TokenKind.KwTopology,
            ["allow"] = TokenKind.KwAllow,
            ["deny"] = TokenKind.KwDeny,
            ["phase"] = TokenKind.KwPhase,
            ["produces"] = TokenKind.KwProduces,
            ["gate"] = TokenKind.KwGate,
            ["command"] = TokenKind.KwCommand,
            ["expects"] = TokenKind.KwExpects,
            ["trace"] = TokenKind.KwTrace,
            ["constraint"] = TokenKind.KwConstraint,
            ["scope"] = TokenKind.KwScope,
            ["rule"] = TokenKind.KwRule,
            ["package_policy"] = TokenKind.KwPackagePolicy,
            ["category"] = TokenKind.KwCategory,
            ["includes"] = TokenKind.KwIncludes,
            ["default"] = TokenKind.KwDefault,
            ["dotnet"] = TokenKind.KwDotnet,
            ["solution"] = TokenKind.KwSolution,
            ["format"] = TokenKind.KwFormat,
            ["startup"] = TokenKind.KwStartup,
            ["folder"] = TokenKind.KwFolder,
            ["projects"] = TokenKind.KwProjects,
            ["path"] = TokenKind.KwPath,
            ["status"] = TokenKind.KwStatus,
            ["existing"] = TokenKind.KwExisting,
            ["new"] = TokenKind.KwNew,

            // Design specification keywords
            ["page"] = TokenKind.KwPage,
            ["host"] = TokenKind.KwHost,
            ["route"] = TokenKind.KwRoute,
            ["concepts"] = TokenKind.KwConcepts,
            ["role"] = TokenKind.KwRole,
            ["cross_links"] = TokenKind.KwCrossLinks,
            ["visualization"] = TokenKind.KwVisualization,
            ["parameters"] = TokenKind.KwParameters,
            ["sliders"] = TokenKind.KwSliders,

            // Primitive type keywords
            ["string"] = TokenKind.KwString,
            ["int"] = TokenKind.KwInt,
            ["double"] = TokenKind.KwDouble,
            ["bool"] = TokenKind.KwBool,
            ["unknown"] = TokenKind.KwUnknown,
        }.ToFrozenDictionary();

    /// <summary>
    /// Creates a new lexer for the given spec-block source text.
    /// </summary>
    /// <param name="source">The text content of a single spec block.</param>
    /// <param name="filePath">Path to the file, used for source location tracking.</param>
    /// <param name="baseLineOffset">
    /// The 0-based line number in the original .spec.md file where this spec block starts,
    /// so token locations are accurate relative to the full file.
    /// </param>
    /// <param name="diagnostics">Optional diagnostic bag for reporting lexer errors.</param>
    public Lexer(string source, string filePath, int baseLineOffset = 0, DiagnosticBag? diagnostics = null)
    {
        _source = source;
        _filePath = filePath;
        _baseLineOffset = baseLineOffset;
        _diagnostics = diagnostics ?? new DiagnosticBag();
        _pos = 0;
        _line = 1;
        _column = 1;
    }

    /// <summary>
    /// Tokenizes the entire source and returns the token list, always ending with an EOF token.
    /// </summary>
    public List<Token> Tokenize()
    {
        var tokens = new List<Token>();

        while (true)
        {
            SkipWhitespaceAndComments();

            if (_pos >= _source.Length)
            {
                tokens.Add(MakeToken(TokenKind.Eof, "", _line, _column));
                break;
            }

            var token = ReadToken();
            tokens.Add(token);
        }

        return tokens;
    }

    private Token ReadToken()
    {
        char c = _source[_pos];

        // Numbers: INTEGER or FLOAT
        if (char.IsAsciiDigit(c))
            return ReadNumber();

        // Identifiers, keywords, booleans, DOTTED_IDENT
        if (char.IsAsciiLetter(c))
            return ReadIdentifierOrKeyword();

        // String literals
        if (c == '"')
            return ReadString();

        // Multi-character symbols and single-character symbols
        return ReadSymbol();
    }

    // ─── Numbers ────────────────────────────────────────────────────

    private Token ReadNumber()
    {
        int startLine = _line;
        int startCol = _column;
        int start = _pos;

        // Consume integer digits
        while (_pos < _source.Length && char.IsAsciiDigit(_source[_pos]))
            Advance();

        // Check for FLOAT vs DOTDOT (rule 1.4.4a)
        if (_pos < _source.Length && _source[_pos] == '.')
        {
            // Look ahead: if next char after '.' is also '.', this is INTEGER followed by DOTDOT
            if (_pos + 1 < _source.Length && _source[_pos + 1] == '.')
            {
                // Emit just the INTEGER; the DOTDOT will be consumed on the next call
                return MakeToken(TokenKind.Integer, _source[start.._pos], startLine, startCol);
            }

            // If next char after '.' is a digit, consume as FLOAT
            if (_pos + 1 < _source.Length && char.IsAsciiDigit(_source[_pos + 1]))
            {
                Advance(); // consume '.'
                while (_pos < _source.Length && char.IsAsciiDigit(_source[_pos]))
                    Advance();
                return MakeToken(TokenKind.Float, _source[start.._pos], startLine, startCol);
            }

            // If next char after '.' is a letter, emit INTEGER (the dot starts a new token)
            if (_pos + 1 < _source.Length && char.IsAsciiLetter(_source[_pos + 1]))
            {
                return MakeToken(TokenKind.Integer, _source[start.._pos], startLine, startCol);
            }

            // Lone dot after digits with no digit or second dot following:
            // emit INTEGER, let the dot be handled separately
            return MakeToken(TokenKind.Integer, _source[start.._pos], startLine, startCol);
        }

        return MakeToken(TokenKind.Integer, _source[start.._pos], startLine, startCol);
    }

    // ─── Identifiers, keywords, booleans, DOTTED_IDENT ──────────────

    private Token ReadIdentifierOrKeyword()
    {
        int startLine = _line;
        int startCol = _column;
        int start = _pos;

        // Consume first segment: letter { letter | digit | '_' }
        ConsumeIdentSegment();

        // Greedily consume additional dot-separated segments (rule 1.4.2)
        int dotCount = 0;
        while (_pos < _source.Length && _source[_pos] == '.'
               && _pos + 1 < _source.Length && char.IsAsciiLetter(_source[_pos + 1]))
        {
            Advance(); // consume '.'
            dotCount++;
            ConsumeIdentSegment();
        }

        string text = _source[start.._pos];

        if (dotCount > 0)
        {
            // Multi-segment: always DOTTED_IDENT, no keyword checking
            return MakeToken(TokenKind.DottedIdent, text, startLine, startCol);
        }

        // Single segment: keyword table first (rule 1.4.1a)
        if (Keywords.TryGetValue(text, out var kwKind))
            return MakeToken(kwKind, text, startLine, startCol);

        // Then boolean literals (rule 1.4.1b)
        if (text is "true" or "false")
            return MakeToken(TokenKind.Boolean, text, startLine, startCol);

        // Otherwise plain identifier
        return MakeToken(TokenKind.Ident, text, startLine, startCol);
    }

    private void ConsumeIdentSegment()
    {
        // First char is already validated as a letter by the caller
        Advance();
        while (_pos < _source.Length && IsIdentContinuation(_source[_pos]))
            Advance();
    }

    private static bool IsIdentContinuation(char c) =>
        char.IsAsciiLetterOrDigit(c) || c == '_';

    // ─── String literals ────────────────────────────────────────────

    private Token ReadString()
    {
        int startLine = _line;
        int startCol = _column;
        int startOffset = _pos;

        Advance(); // consume opening '"'

        while (_pos < _source.Length)
        {
            char c = _source[_pos];

            if (c == '"')
            {
                Advance(); // consume closing '"'
                return MakeToken(TokenKind.String, _source[startOffset.._pos], startLine, startCol);
            }

            if (c == '\\')
            {
                Advance(); // consume '\'
                if (_pos >= _source.Length)
                {
                    ReportError(startLine, startCol, "Unterminated string literal.");
                    return MakeToken(TokenKind.String, _source[startOffset.._pos], startLine, startCol);
                }

                char escaped = _source[_pos];
                switch (escaped)
                {
                    case '"':
                    case '\\':
                    case 'n':
                    case 't':
                        Advance();
                        break;
                    default:
                        ReportError(_line, _column, $"Invalid escape sequence '\\{escaped}' in string literal.");
                        Advance();
                        break;
                }
            }
            else
            {
                if (c == '\n')
                {
                    _line++;
                    _column = 0; // Advance() will set it to 1
                }
                Advance();
            }
        }

        // Reached end of source without closing quote
        ReportError(startLine, startCol, "Unterminated string literal.");
        return MakeToken(TokenKind.String, _source[startOffset.._pos], startLine, startCol);
    }

    // ─── Symbols ────────────────────────────────────────────────────

    private Token ReadSymbol()
    {
        int startLine = _line;
        int startCol = _column;
        char c = _source[_pos];

        switch (c)
        {
            case '{':
                Advance();
                return MakeToken(TokenKind.LBrace, "{", startLine, startCol);
            case '}':
                Advance();
                return MakeToken(TokenKind.RBrace, "}", startLine, startCol);
            case '[':
                Advance();
                return MakeToken(TokenKind.LBracket, "[", startLine, startCol);
            case ']':
                Advance();
                return MakeToken(TokenKind.RBracket, "]", startLine, startCol);
            case '(':
                Advance();
                return MakeToken(TokenKind.LParen, "(", startLine, startCol);
            case ')':
                Advance();
                return MakeToken(TokenKind.RParen, ")", startLine, startCol);
            case ':':
                Advance();
                return MakeToken(TokenKind.Colon, ":", startLine, startCol);
            case ';':
                Advance();
                return MakeToken(TokenKind.Semicolon, ";", startLine, startCol);
            case ',':
                Advance();
                return MakeToken(TokenKind.Comma, ",", startLine, startCol);
            case '?':
                Advance();
                return MakeToken(TokenKind.Question, "?", startLine, startCol);
            case '@':
                Advance();
                return MakeToken(TokenKind.At, "@", startLine, startCol);

            // ARROW: -> (rule 1.4.3)
            case '-':
                if (_pos + 1 < _source.Length && _source[_pos + 1] == '>')
                {
                    Advance();
                    Advance();
                    return MakeToken(TokenKind.Arrow, "->", startLine, startCol);
                }
                // Single '-' is not valid
                Advance();
                ReportError(startLine, startCol, "Unexpected character '-'. Did you mean '->'?");
                return MakeToken(TokenKind.Ident, "-", startLine, startCol);

            // DOT and DOTDOT (rule 1.4.4)
            case '.':
                if (_pos + 1 < _source.Length && _source[_pos + 1] == '.')
                {
                    Advance();
                    Advance();
                    return MakeToken(TokenKind.DotDot, "..", startLine, startCol);
                }
                Advance();
                return MakeToken(TokenKind.Dot, ".", startLine, startCol);

            // EQ and invalid single '=' (rule 1.4.5)
            case '=':
                if (_pos + 1 < _source.Length && _source[_pos + 1] == '=')
                {
                    Advance();
                    Advance();
                    return MakeToken(TokenKind.Eq, "==", startLine, startCol);
                }
                Advance();
                ReportError(startLine, startCol, "Unexpected character '='. Did you mean '=='?");
                return MakeToken(TokenKind.Ident, "=", startLine, startCol);

            // NEQ
            case '!':
                if (_pos + 1 < _source.Length && _source[_pos + 1] == '=')
                {
                    Advance();
                    Advance();
                    return MakeToken(TokenKind.Neq, "!=", startLine, startCol);
                }
                Advance();
                ReportError(startLine, startCol, "Unexpected character '!'. Did you mean '!='?");
                return MakeToken(TokenKind.Ident, "!", startLine, startCol);

            // LT, LTE
            case '<':
                if (_pos + 1 < _source.Length && _source[_pos + 1] == '=')
                {
                    Advance();
                    Advance();
                    return MakeToken(TokenKind.Lte, "<=", startLine, startCol);
                }
                Advance();
                return MakeToken(TokenKind.Lt, "<", startLine, startCol);

            // GT, GTE
            case '>':
                if (_pos + 1 < _source.Length && _source[_pos + 1] == '=')
                {
                    Advance();
                    Advance();
                    return MakeToken(TokenKind.Gte, ">=", startLine, startCol);
                }
                Advance();
                return MakeToken(TokenKind.Gt, ">", startLine, startCol);

            default:
                Advance();
                ReportError(startLine, startCol, $"Unexpected character '{c}'.");
                return MakeToken(TokenKind.Ident, c.ToString(), startLine, startCol);
        }
    }

    // ─── Whitespace and comments ────────────────────────────────────

    private void SkipWhitespaceAndComments()
    {
        while (_pos < _source.Length)
        {
            char c = _source[_pos];

            // Whitespace
            if (c == ' ' || c == '\t' || c == '\r')
            {
                Advance();
                continue;
            }

            if (c == '\n')
            {
                _pos++;
                _line++;
                _column = 1;
                continue;
            }

            // Line comment: // to end of line
            if (c == '/' && _pos + 1 < _source.Length && _source[_pos + 1] == '/')
            {
                // Skip until end of line (but do not consume the newline itself;
                // the next iteration will handle line counting)
                _pos += 2;
                _column += 2;
                while (_pos < _source.Length && _source[_pos] != '\n')
                {
                    _pos++;
                    _column++;
                }
                continue;
            }

            // Not whitespace or comment; stop
            break;
        }
    }

    // ─── Helpers ────────────────────────────────────────────────────

    private void Advance()
    {
        _pos++;
        _column++;
    }

    private Token MakeToken(TokenKind kind, string text, int line, int column) =>
        new(kind, text, line + _baseLineOffset, column);

    private void ReportError(int line, int column, string message) =>
        _diagnostics.ReportError(
            new SourceLocation(_filePath, line + _baseLineOffset, column, _pos),
            message);
}
