using SpecChat.Language.Ast;

namespace SpecChat.Language;

/// <summary>
/// Recursive-descent parser that consumes a token list (from <see cref="Lexer"/>)
/// and produces a <see cref="SpecDocument"/> AST.
/// </summary>
public sealed partial class Parser
{
    private readonly List<Token> _tokens;
    private readonly string _filePath;
    private readonly DiagnosticBag _diagnostics;
    private int _pos;

    public Parser(List<Token> tokens, string filePath, DiagnosticBag? diagnostics = null)
    {
        _tokens = tokens;
        _filePath = filePath;
        _diagnostics = diagnostics ?? new DiagnosticBag();
    }

    /// <summary>All diagnostics accumulated during parsing.</summary>
    public DiagnosticBag Diagnostics => _diagnostics;

    // ── Navigation helpers ──────────────────────────────────────────

    private bool IsAtEnd => _pos >= _tokens.Count || Peek().Kind == TokenKind.Eof;

    private Token Peek()
    {
        if (_pos >= _tokens.Count)
            return new Token(TokenKind.Eof, "", 0, 0);
        return _tokens[_pos];
    }

    private Token PeekAhead(int offset)
    {
        int idx = _pos + offset;
        if (idx >= _tokens.Count)
            return new Token(TokenKind.Eof, "", 0, 0);
        return _tokens[idx];
    }

    private Token Advance()
    {
        Token t = Peek();
        if (_pos < _tokens.Count) _pos++;
        return t;
    }

    private bool Check(TokenKind kind) => Peek().Kind == kind;

    private bool Match(TokenKind kind)
    {
        if (!Check(kind)) return false;
        Advance();
        return true;
    }

    private Token Expect(TokenKind kind)
    {
        if (Check(kind)) return Advance();

        ReportError($"Expected {kind}, got {Peek().Kind} ('{Peek().Text}')");
        // Return a synthetic token so the caller can continue.
        return new Token(kind, "", Peek().Line, Peek().Column);
    }

    private SourceLocation CurrentLocation()
    {
        Token t = Peek();
        return new SourceLocation(_filePath, t.Line, t.Column, 0);
    }

    private SourceLocation LocationOf(Token t) =>
        new(_filePath, t.Line, t.Column, 0);

    private void ReportError(string message) =>
        _diagnostics.ReportError(CurrentLocation(), message);

    /// <summary>
    /// Skip tokens until we find a likely recovery point (closing brace at depth,
    /// semicolon, or a top-level keyword).
    /// </summary>
    /// <summary>
    /// Skip tokens until we find a likely recovery point. Guarantees
    /// at least one token is consumed to prevent infinite loops in
    /// body-parsing while loops.
    /// </summary>
    private void Synchronize()
    {
        // Always advance at least once to guarantee progress.
        if (!IsAtEnd) Advance();

        while (!IsAtEnd)
        {
            if (Check(TokenKind.Semicolon)) { Advance(); return; }
            if (Check(TokenKind.RBrace)) return; // don't consume; caller owns it
            if (IsTopLevelKeyword(Peek().Kind)) return;
            Advance();
        }
    }

    private static bool IsTopLevelKeyword(TokenKind kind) => kind switch
    {
        TokenKind.KwEntity or TokenKind.KwEnum or TokenKind.KwContract
            or TokenKind.KwRefines or TokenKind.KwPerson or TokenKind.KwExternal
            or TokenKind.KwSystem or TokenKind.KwTopology
            or TokenKind.KwPhase or TokenKind.KwTrace or TokenKind.KwConstraint
            or TokenKind.KwPackagePolicy or TokenKind.KwDotnet
            or TokenKind.KwDeployment or TokenKind.KwView or TokenKind.KwDynamic
            or TokenKind.KwPage or TokenKind.KwVisualization
            or TokenKind.KwArchitecture or TokenKind.KwLayerContract => true,
        _ => false,
    };

    // ── Contextual keyword helpers ──────────────────────────────────

    /// <summary>
    /// Returns true when the token is a keyword that may appear as a field name,
    /// enum member name, or annotation name (grammar sec 4, ContextualKeyword).
    /// </summary>
    private static bool IsContextualKeyword(TokenKind kind) => kind switch
    {
        TokenKind.KwSource or TokenKind.KwVersion or TokenKind.KwTarget
            or TokenKind.KwKind or TokenKind.KwResponsibility
            or TokenKind.KwRule or TokenKind.KwFormat or TokenKind.KwCommand
            or TokenKind.KwScope or TokenKind.KwContext or TokenKind.KwDecision
            or TokenKind.KwConsequence or TokenKind.KwSupersedes
            or TokenKind.KwStartup or TokenKind.KwFolder or TokenKind.KwProjects
            or TokenKind.KwGate or TokenKind.KwPhase or TokenKind.KwTrace
            or TokenKind.KwConstraint or TokenKind.KwComponent
            or TokenKind.KwPath or TokenKind.KwStatus or TokenKind.KwExisting
            or TokenKind.KwNew or TokenKind.KwPage or TokenKind.KwHost
            or TokenKind.KwRoute or TokenKind.KwConcepts or TokenKind.KwRole
            or TokenKind.KwCrossLinks or TokenKind.KwVisualization
            or TokenKind.KwParameters or TokenKind.KwSliders
            // Context, deployment, view, dynamic keywords (contextual in field-name positions)
            or TokenKind.KwPerson or TokenKind.KwExternal or TokenKind.KwDescription
            or TokenKind.KwTechnology or TokenKind.KwDeployment or TokenKind.KwNode
            or TokenKind.KwInstance or TokenKind.KwView or TokenKind.KwInclude
            or TokenKind.KwExclude or TokenKind.KwAutoLayout or TokenKind.KwDynamic
            or TokenKind.KwTag
            // Additional keywords that appear as parameter names or field names
            // in real-world specs (not in the grammar's ContextualKeyword list
            // but needed to prevent parse failures in ParameterBinding positions).
            or TokenKind.KwSystem or TokenKind.KwDefault or TokenKind.KwRequires
            or TokenKind.KwProduces or TokenKind.KwExpects
            or TokenKind.KwTopology or TokenKind.KwDotnet
            or TokenKind.KwCategory or TokenKind.KwIncludes
            or TokenKind.KwAllow or TokenKind.KwDeny
            // The Standard extension keywords (contextual in field-name positions)
            or TokenKind.KwArchitecture or TokenKind.KwEnforce or TokenKind.KwVocabulary
            or TokenKind.KwLayer or TokenKind.KwOwns or TokenKind.KwBroker
            or TokenKind.KwFoundation or TokenKind.KwProcessing or TokenKind.KwOrchestration
            or TokenKind.KwCoordination or TokenKind.KwAggregation or TokenKind.KwExposer
            or TokenKind.KwTest or TokenKind.KwService or TokenKind.KwLayerContract
            or TokenKind.KwRealize or TokenKind.KwValidation or TokenKind.KwSuppress => true,
        _ => false,
    };

    /// <summary>
    /// Returns true when the token is a scope word (IDENT, KwAuthored, KwConsumed).
    /// </summary>
    private static bool IsScopeWord(TokenKind kind) =>
        kind == TokenKind.Ident || kind == TokenKind.KwAuthored || kind == TokenKind.KwConsumed;

    /// <summary>
    /// Returns true when the token can be treated as an identifier in field-name
    /// or enum-member positions (IDENT or contextual keyword).
    /// </summary>
    private static bool IsFieldNameToken(TokenKind kind) =>
        kind == TokenKind.Ident || IsContextualKeyword(kind);

    /// <summary>
    /// Consume a field name (IDENT or contextual keyword) and return its text.
    /// </summary>
    private string ExpectFieldName()
    {
        if (IsFieldNameToken(Peek().Kind))
            return Advance().Text;

        ReportError($"Expected identifier or contextual keyword as field name, got {Peek().Kind}");
        return "<missing>";
    }

    /// <summary>
    /// Consume an identifier or dotted identifier and return its text.
    /// </summary>
    private string ExpectName()
    {
        if (Check(TokenKind.Ident) || Check(TokenKind.DottedIdent))
            return Advance().Text;

        ReportError($"Expected identifier, got {Peek().Kind} ('{Peek().Text}')");
        return "<missing>";
    }

    /// <summary>
    /// Consume a STRING token and return its content without surrounding quotes.
    /// </summary>
    private string ExpectStringContent()
    {
        Token t = Expect(TokenKind.String);
        return StripQuotes(t.Text);
    }

    /// <summary>
    /// Strip surrounding double-quote characters from a string token's raw text.
    /// </summary>
    private static string StripQuotes(string text)
    {
        if (text.Length >= 2 && text[0] == '"' && text[^1] == '"')
            return text[1..^1];
        return text;
    }

    // ═══════════════════════════════════════════════════════════════
    //  TOP-LEVEL PARSING
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Parse the full token stream into a <see cref="SpecDocument"/>.
    /// </summary>
    public SpecDocument Parse()
    {
        var loc = CurrentLocation();
        var decls = new List<TopLevelDecl>();

        while (!IsAtEnd)
        {
            TopLevelDecl? decl = ParseTopLevelDecl();
            if (decl is not null)
                decls.Add(decl);
            else
            {
                // Unrecognized token at top level; skip and try again.
                if (!IsAtEnd)
                {
                    ReportError($"Unexpected token at top level: {Peek().Kind} ('{Peek().Text}')");
                    Advance();
                }
            }
        }

        return new SpecDocument(decls, loc);
    }

    private TopLevelDecl? ParseTopLevelDecl()
    {
        // Check for bare relationship: DOTTED_IDENT -> ... at top level (§5.17)
        if ((Peek().Kind == TokenKind.Ident || Peek().Kind == TokenKind.DottedIdent)
            && PeekAhead(1).Kind == TokenKind.Arrow)
        {
            return ParseRelationshipDecl();
        }

        return Peek().Kind switch
        {
            TokenKind.KwEntity => ParseEntityDecl(),
            TokenKind.KwEnum => ParseEnumDecl(),
            TokenKind.KwContract => ParseContractDecl(),
            TokenKind.KwRefines => ParseRefinementDecl(),
            TokenKind.KwPerson => ParsePersonDecl(),
            TokenKind.KwExternal when PeekAhead(1).Kind == TokenKind.KwSystem
                => ParseExternalSystemDecl(),
            TokenKind.KwSystem => ParseSystemDecl(),
            TokenKind.KwTopology => ParseTopologyDecl(),
            TokenKind.KwPhase => ParsePhaseDecl(),
            TokenKind.KwTrace => ParseTraceDecl(),
            TokenKind.KwConstraint => ParseConstraintDecl(),
            TokenKind.KwPackagePolicy => ParsePackagePolicyDecl(),
            TokenKind.KwDotnet when PeekAhead(1).Kind == TokenKind.KwSolution
                => ParseDotNetSolutionDecl(),
            TokenKind.KwDeployment => ParseDeploymentDecl(),
            TokenKind.KwView => ParseViewDecl(),
            TokenKind.KwDynamic => ParseDynamicDecl(),
            TokenKind.KwPage => ParsePageDecl(),
            TokenKind.KwVisualization => ParseVisualizationDecl(),
            TokenKind.KwArchitecture => ParseArchitectureDecl(),
            TokenKind.KwLayerContract => ParseLayerContractDecl(),
            _ => null,
        };
    }

    // ═══════════════════════════════════════════════════════════════
    //  DATA SPECIFICATION PRODUCTIONS
    // ═══════════════════════════════════════════════════════════════

    // ── Entity ──────────────────────────────────────────────────────

    private EntityDecl ParseEntityDecl()
    {
        var loc = CurrentLocation();
        Expect(TokenKind.KwEntity);
        string name = ExpectName();
        Expect(TokenKind.LBrace);

        var fields = new List<FieldDecl>();
        var invariants = new List<InvariantDecl>();
        var rationales = new List<RationaleDecl>();

        while (!Check(TokenKind.RBrace) && !IsAtEnd)
        {
            if (Check(TokenKind.KwInvariant))
                invariants.Add(ParseInvariantDecl());
            else if (Check(TokenKind.KwRationale))
                rationales.Add(ParseRationaleDecl());
            else if (IsFieldNameToken(Peek().Kind))
                fields.Add(ParseFieldDecl());
            else
            {
                ReportError($"Unexpected token in entity body: {Peek().Kind}");
                Synchronize();
            }
        }

        Expect(TokenKind.RBrace);
        return new EntityDecl(name, fields, invariants, rationales, loc);
    }

    // ── Field ───────────────────────────────────────────────────────

    private FieldDecl ParseFieldDecl()
    {
        var loc = CurrentLocation();
        string name = ExpectFieldName();
        Expect(TokenKind.Colon);
        var (typeRef, isOptional) = ParseTypeExpr();

        var annotations = new List<Annotation>();
        while (Check(TokenKind.At))
            annotations.Add(ParseAnnotation());

        Expect(TokenKind.Semicolon);
        return new FieldDecl(name, typeRef, isOptional, annotations, loc);
    }

    private (TypeRef Type, bool IsOptional) ParseTypeExpr()
    {
        var loc = CurrentLocation();
        string baseName = ParseBaseType();

        bool isOptional = Match(TokenKind.Question);
        bool isArray = false;
        if (Match(TokenKind.LBracket))
        {
            Expect(TokenKind.RBracket);
            isArray = true;
        }

        return (new TypeRef(baseName, isArray, loc), isOptional);
    }

    private string ParseBaseType()
    {
        // Primitive type keywords
        if (Check(TokenKind.KwString)) return Advance().Text;
        if (Check(TokenKind.KwInt)) return Advance().Text;
        if (Check(TokenKind.KwDouble)) return Advance().Text;
        if (Check(TokenKind.KwBool)) return Advance().Text;
        if (Check(TokenKind.KwUnknown)) return Advance().Text;

        // Named type (IDENT or DOTTED_IDENT)
        if (Check(TokenKind.Ident) || Check(TokenKind.DottedIdent))
            return Advance().Text;

        ReportError($"Expected type name, got {Peek().Kind}");
        return "<unknown>";
    }

    // ── Annotation ──────────────────────────────────────────────────

    private Annotation ParseAnnotation()
    {
        var loc = CurrentLocation();
        Expect(TokenKind.At);

        // AnnotationName: IDENT or specific keyword tokens
        string annoName;
        if (Check(TokenKind.Ident) || Check(TokenKind.KwDefault) || Check(TokenKind.KwSource)
            || Check(TokenKind.KwVersion) || Check(TokenKind.KwTarget))
            annoName = Advance().Text;
        else
        {
            ReportError($"Expected annotation name, got {Peek().Kind}");
            annoName = "<unknown>";
        }

        Expect(TokenKind.LParen);

        string? simpleValue = null;
        Expr? rangeMin = null;
        Expr? rangeMax = null;

        // Check for range: INTEGER ".." INTEGER
        if ((Check(TokenKind.Integer) || Check(TokenKind.Float))
            && PeekAhead(1).Kind == TokenKind.DotDot)
        {
            rangeMin = ParsePrimaryExpr();
            Expect(TokenKind.DotDot);
            rangeMax = ParsePrimaryExpr();
        }
        else if (Check(TokenKind.String))
        {
            simpleValue = StripQuotes(Advance().Text);
        }
        else if (Check(TokenKind.Boolean))
        {
            simpleValue = Advance().Text;
        }
        else if (Check(TokenKind.Integer))
        {
            simpleValue = Advance().Text;
        }
        else if (Check(TokenKind.Float))
        {
            simpleValue = Advance().Text;
        }
        else if (Check(TokenKind.Ident))
        {
            simpleValue = Advance().Text;
        }
        else
        {
            ReportError($"Expected annotation value, got {Peek().Kind}");
        }

        Expect(TokenKind.RParen);
        return new Annotation(annoName, simpleValue, rangeMin, rangeMax, loc);
    }

    // ── Enum ────────────────────────────────────────────────────────

    private EnumDecl ParseEnumDecl()
    {
        var loc = CurrentLocation();
        Expect(TokenKind.KwEnum);
        string name = ExpectName();
        Expect(TokenKind.LBrace);

        var members = new List<EnumMemberDecl>();
        while (!Check(TokenKind.RBrace) && !IsAtEnd)
        {
            if (Check(TokenKind.Comma))
            {
                Advance(); // optional trailing/separating comma
                continue;
            }

            if (IsFieldNameToken(Peek().Kind))
            {
                var mLoc = CurrentLocation();
                string mName = ExpectFieldName();
                Expect(TokenKind.Colon);
                string desc = ExpectStringContent();
                members.Add(new EnumMemberDecl(mName, desc, mLoc));
            }
            else
            {
                ReportError($"Unexpected token in enum body: {Peek().Kind}");
                Synchronize();
            }
        }

        Expect(TokenKind.RBrace);
        return new EnumDecl(name, members, loc);
    }

    // ── Contract ────────────────────────────────────────────────────

    private ContractDecl ParseContractDecl()
    {
        var loc = CurrentLocation();
        Expect(TokenKind.KwContract);
        // Top-level contracts require a name (grammar sec 3.2, sec 5.8).
        string name = ExpectName();

        Expect(TokenKind.LBrace);
        var clauses = ParseContractClauses();
        Expect(TokenKind.RBrace);
        return new ContractDecl(name, clauses, loc);
    }

    private ContractDecl ParseInlineContractDecl()
    {
        var loc = CurrentLocation();
        Expect(TokenKind.KwContract);
        // Inline contracts have no name (grammar sec 5.8)
        Expect(TokenKind.LBrace);
        var clauses = ParseContractClauses();
        Expect(TokenKind.RBrace);
        return new ContractDecl(null, clauses, loc);
    }

    private List<ContractClause> ParseContractClauses()
    {
        var clauses = new List<ContractClause>();
        while (!Check(TokenKind.RBrace) && !IsAtEnd)
        {
            var cLoc = CurrentLocation();
            if (Match(TokenKind.KwRequires))
            {
                Expr expr = ParseExpr();
                string? validationCategory = null;
                if (Check(TokenKind.At) && PeekAhead(1).Kind == TokenKind.KwValidation)
                {
                    Advance(); // @
                    Advance(); // validation
                    Expect(TokenKind.LParen);
                    validationCategory = Advance().Text;
                    Expect(TokenKind.RParen);
                }
                Expect(TokenKind.Semicolon);
                clauses.Add(new ContractClause(ContractClauseKind.Requires, expr, null, validationCategory, cLoc));
            }
            else if (Match(TokenKind.KwEnsures))
            {
                Expr expr = ParseExpr();
                Expect(TokenKind.Semicolon);
                clauses.Add(new ContractClause(ContractClauseKind.Ensures, expr, null, null, cLoc));
            }
            else if (Match(TokenKind.KwGuarantees))
            {
                string prose = ExpectStringContent();
                Expect(TokenKind.Semicolon);
                clauses.Add(new ContractClause(ContractClauseKind.Guarantees, null, prose, null, cLoc));
            }
            else
            {
                ReportError($"Unexpected token in contract body: {Peek().Kind}");
                Synchronize();
            }
        }
        return clauses;
    }

    // ── Invariant ───────────────────────────────────────────────────

    private InvariantDecl ParseInvariantDecl()
    {
        var loc = CurrentLocation();
        Expect(TokenKind.KwInvariant);
        string name = ExpectStringContent();
        Expect(TokenKind.Colon);
        Expr condition = ParseExpr();
        Expect(TokenKind.Semicolon);
        return new InvariantDecl(name, condition, loc);
    }

    // ── Rationale ───────────────────────────────────────────────────

    private RationaleDecl ParseRationaleDecl()
    {
        var loc = CurrentLocation();
        Expect(TokenKind.KwRationale);

        // Disambiguation (grammar sec 5.5):
        // STRING ";" -> simple
        // STRING "{" -> structured with target name
        // "{" -> structured without target name

        if (Check(TokenKind.String))
        {
            string text = StripQuotes(Advance().Text);
            if (Check(TokenKind.Semicolon))
            {
                Advance();
                return new RationaleDecl(text, null, null, null, null, null, loc);
            }
            // STRING followed by "{" -> structured with field/target name
            if (Check(TokenKind.LBrace))
            {
                return ParseStructuredRationale(text, loc);
            }
            // Fallback: treat as simple if neither ";" nor "{" follows
            ReportError("Expected ';' or '{' after rationale string");
            return new RationaleDecl(text, null, null, null, null, null, loc);
        }

        if (Check(TokenKind.LBrace))
        {
            return ParseStructuredRationale(null, loc);
        }

        ReportError("Expected string or '{' after 'rationale'");
        Synchronize();
        return new RationaleDecl(null, null, null, null, null, null, loc);
    }

    private RationaleDecl ParseStructuredRationale(string? fieldName, SourceLocation loc)
    {
        Expect(TokenKind.LBrace);

        string? context = null;
        string? decision = null;
        string? consequence = null;
        string? supersedes = null;

        while (!Check(TokenKind.RBrace) && !IsAtEnd)
        {
            if (Match(TokenKind.KwContext))
            {
                context = ExpectStringContent();
                Expect(TokenKind.Semicolon);
            }
            else if (Match(TokenKind.KwDecision))
            {
                decision = ExpectStringContent();
                Expect(TokenKind.Semicolon);
            }
            else if (Match(TokenKind.KwConsequence))
            {
                consequence = ExpectStringContent();
                Expect(TokenKind.Semicolon);
            }
            else if (Match(TokenKind.KwSupersedes))
            {
                supersedes = ExpectStringContent();
                Expect(TokenKind.Semicolon);
            }
            else
            {
                ReportError($"Unexpected token in structured rationale: {Peek().Kind}");
                Synchronize();
            }
        }

        Expect(TokenKind.RBrace);
        return new RationaleDecl(null, fieldName, context, decision, consequence, supersedes, loc);
    }

    // ── Refinement ──────────────────────────────────────────────────

    private RefinementDecl ParseRefinementDecl()
    {
        var loc = CurrentLocation();
        Expect(TokenKind.KwRefines);
        string originalEntity = ExpectName();
        Expect(TokenKind.KwAs);
        string refinedName = ExpectName();
        Expect(TokenKind.LBrace);

        var fields = new List<FieldDecl>();
        var invariants = new List<InvariantDecl>();
        var rationales = new List<RationaleDecl>();

        while (!Check(TokenKind.RBrace) && !IsAtEnd)
        {
            if (Check(TokenKind.KwInvariant))
                invariants.Add(ParseInvariantDecl());
            else if (Check(TokenKind.KwRationale))
                rationales.Add(ParseRationaleDecl());
            else if (IsFieldNameToken(Peek().Kind))
                fields.Add(ParseFieldDecl());
            else
            {
                ReportError($"Unexpected token in refinement body: {Peek().Kind}");
                Synchronize();
            }
        }

        Expect(TokenKind.RBrace);
        return new RefinementDecl(originalEntity, refinedName, fields, invariants, rationales, loc);
    }

    // ═══════════════════════════════════════════════════════════════
    //  SYSTEMS SPECIFICATION PRODUCTIONS
    // ═══════════════════════════════════════════════════════════════

    // ── System ──────────────────────────────────────────────────────

    private SystemDecl ParseSystemDecl()
    {
        var loc = CurrentLocation();
        Expect(TokenKind.KwSystem);
        string name = ExpectName();
        Expect(TokenKind.LBrace);

        string target = "";
        string responsibility = "";
        var components = new List<ComponentDecl>();

        while (!Check(TokenKind.RBrace) && !IsAtEnd)
        {
            if (Check(TokenKind.KwTarget))
            {
                Advance();
                Expect(TokenKind.Colon);
                target = ParseTargetValue();
                Expect(TokenKind.Semicolon);
            }
            else if (Check(TokenKind.KwResponsibility))
            {
                Advance();
                Expect(TokenKind.Colon);
                responsibility = ExpectStringContent();
                Expect(TokenKind.Semicolon);
            }
            else if (Check(TokenKind.KwAuthored) && PeekAhead(1).Kind == TokenKind.KwComponent)
            {
                components.Add(ParseAuthoredComponentDecl());
            }
            else if (Check(TokenKind.KwConsumed) && PeekAhead(1).Kind == TokenKind.KwComponent)
            {
                components.Add(ParseConsumedComponentDecl());
            }
            // The Standard extension: layer-prefixed declarations
            else if (Check(TokenKind.KwBroker) && IsNameToken(PeekAhead(1).Kind))
            {
                components.Add(ParseLayerPrefixedDecl("broker"));
            }
            else if (Check(TokenKind.KwExposer) && IsNameToken(PeekAhead(1).Kind))
            {
                components.Add(ParseLayerPrefixedDecl("exposer"));
            }
            else if (Check(TokenKind.KwTest) && IsNameToken(PeekAhead(1).Kind))
            {
                components.Add(ParseLayerPrefixedDecl("test"));
            }
            else if (Check(TokenKind.KwFoundation) && PeekAhead(1).Kind == TokenKind.KwService)
            {
                components.Add(ParseServiceDecl("foundation"));
            }
            else if (Check(TokenKind.KwProcessing) && PeekAhead(1).Kind == TokenKind.KwService)
            {
                components.Add(ParseServiceDecl("processing"));
            }
            else if (Check(TokenKind.KwOrchestration) && PeekAhead(1).Kind == TokenKind.KwService)
            {
                components.Add(ParseServiceDecl("orchestration"));
            }
            else if (Check(TokenKind.KwCoordination) && PeekAhead(1).Kind == TokenKind.KwService)
            {
                components.Add(ParseServiceDecl("coordination"));
            }
            else if (Check(TokenKind.KwAggregation) && PeekAhead(1).Kind == TokenKind.KwService)
            {
                components.Add(ParseServiceDecl("aggregation"));
            }
            else
            {
                ReportError($"Unexpected token in system body: {Peek().Kind}");
                Synchronize();
            }
        }

        Expect(TokenKind.RBrace);
        return new SystemDecl(name, target, responsibility, components, loc);
    }

    private string ParseTargetValue()
    {
        // TargetValue = DOTTED_IDENT | STRING
        if (Check(TokenKind.String)) return StripQuotes(Advance().Text);
        if (Check(TokenKind.Ident) || Check(TokenKind.DottedIdent)) return Advance().Text;
        ReportError($"Expected target value (identifier or string), got {Peek().Kind}");
        return "<unknown>";
    }

    // ── Authored component ──────────────────────────────────────────

    private ComponentDecl ParseAuthoredComponentDecl()
    {
        var loc = CurrentLocation();
        Expect(TokenKind.KwAuthored);
        Expect(TokenKind.KwComponent);
        string name = ExpectName();
        Expect(TokenKind.LBrace);

        string? kind = null;
        string? path = null;
        string? status = null;
        string? responsibility = null;
        string? layer = null;
        string? owns = null;
        var suppressions = new List<string>();
        var contracts = new List<ContractDecl>();
        var rationales = new List<RationaleDecl>();

        while (!Check(TokenKind.RBrace) && !IsAtEnd)
        {
            if (Check(TokenKind.KwKind))
            {
                Advance();
                Expect(TokenKind.Colon);
                kind = ParseKindValue();
                Expect(TokenKind.Semicolon);
            }
            else if (Check(TokenKind.KwPath))
            {
                Advance();
                Expect(TokenKind.Colon);
                path = ExpectStringContent();
                Expect(TokenKind.Semicolon);
            }
            else if (Check(TokenKind.KwStatus))
            {
                Advance();
                Expect(TokenKind.Colon);
                status = ParseStatusValue();
                Expect(TokenKind.Semicolon);
            }
            else if (Check(TokenKind.KwResponsibility))
            {
                Advance();
                Expect(TokenKind.Colon);
                responsibility = ExpectStringContent();
                Expect(TokenKind.Semicolon);
            }
            else if (Check(TokenKind.KwTarget))
            {
                // authored components can also have target: value;
                Advance();
                Expect(TokenKind.Colon);
                ParseTargetValue(); // consume but ComponentDecl AST has no separate target field
                Expect(TokenKind.Semicolon);
            }
            else if (Check(TokenKind.KwLayer))
            {
                Advance();
                Expect(TokenKind.Colon);
                layer = ParseLayerKeywordOrIdent();
                Expect(TokenKind.Semicolon);
            }
            else if (Check(TokenKind.KwOwns))
            {
                Advance();
                Expect(TokenKind.Colon);
                owns = ExpectName();
                Expect(TokenKind.Semicolon);
            }
            else if (Check(TokenKind.At) && PeekAhead(1).Kind == TokenKind.KwSuppress)
            {
                Advance(); // @
                Advance(); // suppress
                Expect(TokenKind.LParen);
                suppressions.Add(Advance().Text);
                Expect(TokenKind.RParen);
                Expect(TokenKind.Semicolon);
            }
            else if (Check(TokenKind.KwContract))
            {
                contracts.Add(ParseInlineContractDecl());
            }
            else if (Check(TokenKind.KwRationale))
            {
                rationales.Add(ParseRationaleDecl());
            }
            else
            {
                // Tolerate unknown properties by skipping to next semicolon or brace
                ReportError($"Unexpected token in authored component body: {Peek().Kind}");
                Synchronize();
            }
        }

        Expect(TokenKind.RBrace);
        return new ComponentDecl(
            name, ComponentDisposition.Authored, kind, path, status,
            responsibility, [], null, null, contracts, rationales,
            layer, owns, suppressions, loc);
    }

    private string ParseKindValue()
    {
        // KindValue = IDENT | STRING
        if (Check(TokenKind.Ident)) return Advance().Text;
        if (Check(TokenKind.String)) return StripQuotes(Advance().Text);
        ReportError($"Expected kind value, got {Peek().Kind}");
        return "<unknown>";
    }

    private string ParseStatusValue()
    {
        // StatusValue = "existing" | "new"
        if (Check(TokenKind.KwExisting)) return Advance().Text;
        if (Check(TokenKind.KwNew)) return Advance().Text;
        if (Check(TokenKind.Ident)) return Advance().Text; // tolerate unrecognized
        ReportError($"Expected 'existing' or 'new', got {Peek().Kind}");
        return "<unknown>";
    }

    // ── Consumed component ──────────────────────────────────────────

    private ComponentDecl ParseConsumedComponentDecl()
    {
        var loc = CurrentLocation();
        Expect(TokenKind.KwConsumed);
        Expect(TokenKind.KwComponent);
        string name = ExpectName();
        Expect(TokenKind.LBrace);

        string? sourcePackage = null;
        string? version = null;
        string? responsibility = null;
        var usedBy = new List<string>();
        var contracts = new List<ContractDecl>();
        var rationales = new List<RationaleDecl>();

        while (!Check(TokenKind.RBrace) && !IsAtEnd)
        {
            if (Check(TokenKind.KwSource))
            {
                Advance();
                Expect(TokenKind.Colon);
                sourcePackage = ParseSourceExpr();
                Expect(TokenKind.Semicolon);
            }
            else if (Check(TokenKind.KwVersion))
            {
                Advance();
                Expect(TokenKind.Colon);
                version = ExpectStringContent();
                Expect(TokenKind.Semicolon);
            }
            else if (Check(TokenKind.KwResponsibility))
            {
                Advance();
                Expect(TokenKind.Colon);
                responsibility = ExpectStringContent();
                Expect(TokenKind.Semicolon);
            }
            else if (Check(TokenKind.KwUsedBy))
            {
                Advance();
                Expect(TokenKind.Colon);
                Expect(TokenKind.LBracket);
                usedBy = ParseIdentList();
                Expect(TokenKind.RBracket);
                Expect(TokenKind.Semicolon);
            }
            else if (Check(TokenKind.KwContract))
            {
                contracts.Add(ParseInlineContractDecl());
            }
            else if (Check(TokenKind.KwRationale))
            {
                rationales.Add(ParseRationaleDecl());
            }
            else
            {
                ReportError($"Unexpected token in consumed component body: {Peek().Kind}");
                Synchronize();
            }
        }

        Expect(TokenKind.RBrace);
        return new ComponentDecl(
            name, ComponentDisposition.Consumed, null, null, null,
            responsibility, usedBy, sourcePackage, version, contracts, rationales,
            null, null, [], loc);
    }

    /// <summary>
    /// SourceExpr = IDENT "(" STRING ")"
    /// Returns the combined string like "nuget(package-name)".
    /// </summary>
    private string ParseSourceExpr()
    {
        // source: nuget("Microsoft.AspNetCore.Components.WebAssembly")
        // We store just the package name (stripped of quotes) so that
        // SemanticAnalyzer.MatchesPackagePattern can compare it directly
        // against PolicyRule.Includes entries.
        if (Check(TokenKind.Ident))
            Advance(); // consume registry name (e.g., "nuget"); not stored
        else
            ReportError($"Expected registry identifier, got {Peek().Kind}");

        Expect(TokenKind.LParen);
        string pkg = ExpectStringContent();
        Expect(TokenKind.RParen);
        return pkg;
    }

    /// <summary>
    /// IdentList = DOTTED_IDENT { "," DOTTED_IDENT }
    /// Returns the list of identifier strings.
    /// </summary>
    private List<string> ParseIdentList()
    {
        var list = new List<string>();

        if (Check(TokenKind.Ident) || Check(TokenKind.DottedIdent))
        {
            list.Add(Advance().Text);
            while (Match(TokenKind.Comma))
            {
                if (Check(TokenKind.RBracket)) break; // trailing comma
                if (Check(TokenKind.Ident) || Check(TokenKind.DottedIdent))
                    list.Add(Advance().Text);
                else
                {
                    ReportError($"Expected identifier in list, got {Peek().Kind}");
                    break;
                }
            }
        }

        return list;
    }

    /// <summary>
    /// StringList = STRING { "," STRING }
    /// </summary>
    private List<string> ParseStringList()
    {
        var list = new List<string>();
        if (Check(TokenKind.String))
        {
            list.Add(StripQuotes(Advance().Text));
            while (Match(TokenKind.Comma))
            {
                if (Check(TokenKind.RBracket)) break; // trailing comma
                if (Check(TokenKind.String))
                    list.Add(StripQuotes(Advance().Text));
                else
                {
                    ReportError($"Expected string in list, got {Peek().Kind}");
                    break;
                }
            }
        }
        return list;
    }

    // ── Topology ────────────────────────────────────────────────────

    private TopologyDecl ParseTopologyDecl()
    {
        var loc = CurrentLocation();
        Expect(TokenKind.KwTopology);
        string name = ExpectName();
        Expect(TokenKind.LBrace);

        var rules = new List<TopologyRule>();
        var invariants = new List<InvariantDecl>();
        var rationales = new List<RationaleDecl>();

        while (!Check(TokenKind.RBrace) && !IsAtEnd)
        {
            if (Check(TokenKind.KwAllow) || Check(TokenKind.KwDeny))
            {
                var rLoc = CurrentLocation();
                var ruleKind = Advance().Kind == TokenKind.KwAllow
                    ? TopologyRuleKind.Allow : TopologyRuleKind.Deny;
                string source = ExpectName();
                Expect(TokenKind.Arrow);
                string target = ExpectName();

                // Simple form (;) or block form ({) for enriched edges (§5.18)
                string? edgeDesc = null;
                string? edgeTech = null;
                var edgeRationales = new List<RationaleDecl>();

                if (Check(TokenKind.LBrace))
                {
                    Advance(); // consume {
                    while (!Check(TokenKind.RBrace) && !IsAtEnd)
                    {
                        if (Check(TokenKind.KwDescription))
                        {
                            Advance();
                            Expect(TokenKind.Colon);
                            edgeDesc = ExpectStringContent();
                            Expect(TokenKind.Semicolon);
                        }
                        else if (Check(TokenKind.KwTechnology))
                        {
                            Advance();
                            Expect(TokenKind.Colon);
                            edgeTech = ExpectStringContent();
                            Expect(TokenKind.Semicolon);
                        }
                        else if (Check(TokenKind.KwRationale))
                        {
                            edgeRationales.Add(ParseRationaleDecl());
                        }
                        else
                        {
                            ReportError($"Unexpected token in topology edge body: {Peek().Kind}");
                            Synchronize();
                        }
                    }
                    Expect(TokenKind.RBrace);
                }
                else
                {
                    Expect(TokenKind.Semicolon);
                }

                rules.Add(new TopologyRule(ruleKind, source, target, edgeDesc, edgeTech, edgeRationales, rLoc));
            }
            else if (Check(TokenKind.KwInvariant))
            {
                invariants.Add(ParseInvariantDecl());
            }
            else if (Check(TokenKind.KwRationale))
            {
                rationales.Add(ParseRationaleDecl());
            }
            else
            {
                ReportError($"Unexpected token in topology body: {Peek().Kind}");
                Synchronize();
            }
        }

        Expect(TokenKind.RBrace);
        return new TopologyDecl(name, rules, invariants, rationales, loc);
    }

    // ── Phase ───────────────────────────────────────────────────────

    private PhaseDecl ParsePhaseDecl()
    {
        var loc = CurrentLocation();
        Expect(TokenKind.KwPhase);
        string name = ExpectName();
        Expect(TokenKind.LBrace);

        var requiresPhases = new List<string>();
        var produces = new List<string>();
        var gates = new List<GateDecl>();
        var rationales = new List<RationaleDecl>();

        while (!Check(TokenKind.RBrace) && !IsAtEnd)
        {
            if (Check(TokenKind.KwRequires))
            {
                // In phase context, "requires" is followed by ":" (grammar sec 5.2)
                Advance();
                Expect(TokenKind.Colon);
                requiresPhases.Add(ExpectName());
                while (Match(TokenKind.Comma))
                {
                    requiresPhases.Add(ExpectName());
                }
                Expect(TokenKind.Semicolon);
            }
            else if (Check(TokenKind.KwProduces))
            {
                Advance();
                Expect(TokenKind.Colon);
                Expect(TokenKind.LBracket);
                produces = ParseIdentList();
                Expect(TokenKind.RBracket);
                Expect(TokenKind.Semicolon);
            }
            else if (Check(TokenKind.KwGate))
            {
                gates.Add(ParseGateDecl());
            }
            else if (Check(TokenKind.KwRationale))
            {
                rationales.Add(ParseRationaleDecl());
            }
            else
            {
                ReportError($"Unexpected token in phase body: {Peek().Kind}");
                Synchronize();
            }
        }

        Expect(TokenKind.RBrace);
        return new PhaseDecl(name, requiresPhases, produces, gates, rationales, loc);
    }

    private GateDecl ParseGateDecl()
    {
        var loc = CurrentLocation();
        Expect(TokenKind.KwGate);
        string name = ExpectName();
        Expect(TokenKind.LBrace);

        string command = "";
        var expectations = new List<GateExpectation>();

        while (!Check(TokenKind.RBrace) && !IsAtEnd)
        {
            if (Check(TokenKind.KwCommand))
            {
                Advance();
                Expect(TokenKind.Colon);
                command = ExpectStringContent();
                Expect(TokenKind.Semicolon);
            }
            else if (Check(TokenKind.KwExpects))
            {
                Advance();
                Expect(TokenKind.Colon);
                expectations = ParseGateExpectsList();
                Expect(TokenKind.Semicolon);
            }
            else
            {
                ReportError($"Unexpected token in gate body: {Peek().Kind}");
                Synchronize();
            }
        }

        Expect(TokenKind.RBrace);
        return new GateDecl(name, command, expectations, loc);
    }

    private List<GateExpectation> ParseGateExpectsList()
    {
        var list = new List<GateExpectation>();

        list.Add(ParseGateExpect());
        while (Match(TokenKind.Comma))
        {
            if (Check(TokenKind.Semicolon) || Check(TokenKind.RBrace)) break;
            list.Add(ParseGateExpect());
        }

        return list;
    }

    private GateExpectation ParseGateExpect()
    {
        var loc = CurrentLocation();

        // Grammar sec 5.6: STRING is prose; otherwise parse as expression.
        if (Check(TokenKind.String))
        {
            string prose = StripQuotes(Advance().Text);
            return new GateExpectation(null, prose, loc);
        }

        // Try to parse as expression
        Expr expr = ParseExpr();
        return new GateExpectation(expr, null, loc);
    }

    // ── Trace ───────────────────────────────────────────────────────

    private TraceDecl ParseTraceDecl()
    {
        var loc = CurrentLocation();
        Expect(TokenKind.KwTrace);
        string name = ExpectName();
        Expect(TokenKind.LBrace);

        var mappings = new List<TraceMapping>();
        var invariants = new List<InvariantDecl>();

        while (!Check(TokenKind.RBrace) && !IsAtEnd)
        {
            if (Check(TokenKind.KwInvariant))
            {
                invariants.Add(ParseInvariantDecl());
            }
            else if (Check(TokenKind.Ident) || Check(TokenKind.DottedIdent))
            {
                var mLoc = CurrentLocation();
                string source = Advance().Text;
                Expect(TokenKind.Arrow);
                Expect(TokenKind.LBracket);
                var targets = ParseIdentList();
                Expect(TokenKind.RBracket);
                Expect(TokenKind.Semicolon);
                mappings.Add(new TraceMapping(source, targets, mLoc));
            }
            else
            {
                ReportError($"Unexpected token in trace body: {Peek().Kind}");
                Synchronize();
            }
        }

        Expect(TokenKind.RBrace);
        return new TraceDecl(name, mappings, invariants, loc);
    }

    // ── Constraint ──────────────────────────────────────────────────

    private ConstraintDecl ParseConstraintDecl()
    {
        var loc = CurrentLocation();
        Expect(TokenKind.KwConstraint);
        string name = ExpectName();
        Expect(TokenKind.LBrace);

        Expr? scope = null;
        var scopeList = new List<string>();
        string rule = "";
        var rationales = new List<RationaleDecl>();

        while (!Check(TokenKind.RBrace) && !IsAtEnd)
        {
            if (Check(TokenKind.KwScope))
            {
                Advance();
                Expect(TokenKind.Colon);
                ParseScopeExpr(out scope, out scopeList);
                Expect(TokenKind.Semicolon);
            }
            else if (Check(TokenKind.KwRule))
            {
                Advance();
                Expect(TokenKind.Colon);
                rule = ExpectStringContent();
                Expect(TokenKind.Semicolon);
            }
            else if (Check(TokenKind.KwRationale))
            {
                rationales.Add(ParseRationaleDecl());
            }
            else
            {
                ReportError($"Unexpected token in constraint body: {Peek().Kind}");
                Synchronize();
            }
        }

        Expect(TokenKind.RBrace);
        return new ConstraintDecl(name, scope, scopeList, rule, rationales, loc);
    }

    /// <summary>
    /// ScopeExpr = "all" ScopeWord [ ScopeWord ] | "[" IdentList "]"
    /// </summary>
    private void ParseScopeExpr(out Expr? scopeExpr, out List<string> scopeList)
    {
        scopeExpr = null;
        scopeList = new List<string>();

        if (Check(TokenKind.LBracket))
        {
            // Explicit list form
            Advance();
            scopeList = ParseIdentList();
            Expect(TokenKind.RBracket);
        }
        else if (Check(TokenKind.KwAll))
        {
            // "all" ScopeWord [ ScopeWord ]
            var loc = CurrentLocation();
            Advance(); // consume "all"

            var scopeWords = new List<string>();
            if (IsScopeWord(Peek().Kind))
            {
                scopeWords.Add(Advance().Text);
                // Check if there is a second scope word (grammar sec 5.7):
                // In constraint context, if the next token is ";", scope is one word.
                if (IsScopeWord(Peek().Kind) && !Check(TokenKind.Semicolon))
                    scopeWords.Add(Advance().Text);
            }

            // Build as a QuantifierExpr to represent "all <scope>" in the AST
            // using an IdentifierExpr as a placeholder body (the constraint has a
            // separate "rule" property, not an expression body).
            scopeExpr = new QuantifierExpr(
                Quantifier.All, scopeWords,
                new LiteralExpr(true, LiteralKind.Boolean, loc), loc);
        }
        else
        {
            ReportError("Expected '[' or 'all' in scope expression");
        }
    }

    // ── Package policy ──────────────────────────────────────────────

    private PackagePolicyDecl ParsePackagePolicyDecl()
    {
        var loc = CurrentLocation();
        Expect(TokenKind.KwPackagePolicy);
        string name = ExpectName();
        Expect(TokenKind.LBrace);

        string source = "";
        var rules = new List<PolicyRule>();
        string? defaultPolicy = null;
        var rationales = new List<RationaleDecl>();

        while (!Check(TokenKind.RBrace) && !IsAtEnd)
        {
            if (Check(TokenKind.KwSource))
            {
                Advance();
                Expect(TokenKind.Colon);
                source = ParseSourceExpr();
                Expect(TokenKind.Semicolon);
            }
            else if (Check(TokenKind.KwDefault))
            {
                Advance();
                Expect(TokenKind.Colon);
                defaultPolicy = ParsePolicyDefault();
                Expect(TokenKind.Semicolon);
            }
            else if ((Check(TokenKind.KwAllow) || Check(TokenKind.KwDeny))
                     && PeekAhead(1).Kind == TokenKind.KwCategory)
            {
                rules.Add(ParsePolicyCategoryRule());
            }
            else if (Check(TokenKind.KwRationale))
            {
                rationales.Add(ParseRationaleDecl());
            }
            else
            {
                ReportError($"Unexpected token in package_policy body: {Peek().Kind}");
                Synchronize();
            }
        }

        Expect(TokenKind.RBrace);
        return new PackagePolicyDecl(name, source, rules, defaultPolicy, rationales, loc);
    }

    private string ParsePolicyDefault()
    {
        // PolicyDefault = IDENT | "allow" | "deny"
        if (Check(TokenKind.Ident)) return Advance().Text;
        if (Check(TokenKind.KwAllow)) return Advance().Text;
        if (Check(TokenKind.KwDeny)) return Advance().Text;
        ReportError($"Expected policy default value, got {Peek().Kind}");
        return "<unknown>";
    }

    private PolicyRule ParsePolicyCategoryRule()
    {
        var loc = CurrentLocation();
        var ruleKind = Advance().Kind == TokenKind.KwAllow
            ? PolicyRuleKind.Allow : PolicyRuleKind.Deny;

        Expect(TokenKind.KwCategory);
        Expect(TokenKind.LParen);
        string category = ExpectStringContent();
        Expect(TokenKind.RParen);

        Expect(TokenKind.KwIncludes);

        Expect(TokenKind.LBracket);
        var includes = ParseStringList();
        Expect(TokenKind.RBracket);

        Expect(TokenKind.Semicolon);
        return new PolicyRule(ruleKind, category, includes, loc);
    }

    // ── DotNet solution ─────────────────────────────────────────────

    private DotNetSolutionDecl ParseDotNetSolutionDecl()
    {
        var loc = CurrentLocation();
        Expect(TokenKind.KwDotnet);
        Expect(TokenKind.KwSolution);
        string name = ExpectName();
        Expect(TokenKind.LBrace);

        string format = "";
        string? startup = null;
        var folders = new List<SolutionFolderDecl>();
        var rationales = new List<RationaleDecl>();

        while (!Check(TokenKind.RBrace) && !IsAtEnd)
        {
            if (Check(TokenKind.KwFormat))
            {
                Advance();
                Expect(TokenKind.Colon);
                format = ExpectName();
                Expect(TokenKind.Semicolon);
            }
            else if (Check(TokenKind.KwStartup))
            {
                Advance();
                Expect(TokenKind.Colon);
                startup = ExpectName();
                Expect(TokenKind.Semicolon);
            }
            else if (Check(TokenKind.KwFolder))
            {
                folders.Add(ParseSolutionFolderDecl());
            }
            else if (Check(TokenKind.KwRationale))
            {
                rationales.Add(ParseRationaleDecl());
            }
            else
            {
                ReportError($"Unexpected token in dotnet solution body: {Peek().Kind}");
                Synchronize();
            }
        }

        Expect(TokenKind.RBrace);
        return new DotNetSolutionDecl(name, format, startup, folders, rationales, loc);
    }

    private SolutionFolderDecl ParseSolutionFolderDecl()
    {
        var loc = CurrentLocation();
        Expect(TokenKind.KwFolder);
        string folderName = ExpectStringContent();
        Expect(TokenKind.LBrace);

        var projects = new List<string>();
        var rationales = new List<RationaleDecl>();

        while (!Check(TokenKind.RBrace) && !IsAtEnd)
        {
            if (Check(TokenKind.KwProjects))
            {
                Advance();
                Expect(TokenKind.Colon);
                Expect(TokenKind.LBracket);
                projects = ParseIdentList();
                Expect(TokenKind.RBracket);
                Expect(TokenKind.Semicolon);
            }
            else if (Check(TokenKind.KwRationale))
            {
                rationales.Add(ParseRationaleDecl());
            }
            else
            {
                ReportError($"Unexpected token in folder body: {Peek().Kind}");
                Synchronize();
            }
        }

        Expect(TokenKind.RBrace);
        return new SolutionFolderDecl(folderName, projects, rationales, loc);
    }

    // ═══════════════════════════════════════════════════════════════
    //  CONTEXT SPECIFICATION PRODUCTIONS
    // ═══════════════════════════════════════════════════════════════

    // ── Person ─────────────────────────────────────────────────────

    private PersonDecl ParsePersonDecl()
    {
        var loc = CurrentLocation();
        Expect(TokenKind.KwPerson);
        string name = ExpectName();
        Expect(TokenKind.LBrace);

        string description = "";
        var tags = new List<string>();
        var rationales = new List<RationaleDecl>();

        while (!Check(TokenKind.RBrace) && !IsAtEnd)
        {
            if (Check(TokenKind.KwDescription))
            {
                Advance();
                Expect(TokenKind.Colon);
                description = ExpectStringContent();
                Expect(TokenKind.Semicolon);
            }
            else if (Check(TokenKind.At) && PeekAhead(1).Kind == TokenKind.KwTag)
            {
                tags.AddRange(ParseTagAnnotation().Values);
            }
            else if (Check(TokenKind.KwRationale))
            {
                rationales.Add(ParseRationaleDecl());
            }
            else
            {
                ReportError($"Unexpected token in person body: {Peek().Kind}");
                Synchronize();
            }
        }

        Expect(TokenKind.RBrace);
        return new PersonDecl(name, description, tags, rationales, loc);
    }

    // ── External system ────────────────────────────────────────────

    private ExternalSystemDecl ParseExternalSystemDecl()
    {
        var loc = CurrentLocation();
        Expect(TokenKind.KwExternal);
        Expect(TokenKind.KwSystem);
        string name = ExpectName();
        Expect(TokenKind.LBrace);

        string description = "";
        string? technology = null;
        var tags = new List<string>();
        var rationales = new List<RationaleDecl>();

        while (!Check(TokenKind.RBrace) && !IsAtEnd)
        {
            if (Check(TokenKind.KwDescription))
            {
                Advance();
                Expect(TokenKind.Colon);
                description = ExpectStringContent();
                Expect(TokenKind.Semicolon);
            }
            else if (Check(TokenKind.KwTechnology))
            {
                Advance();
                Expect(TokenKind.Colon);
                technology = ExpectStringContent();
                Expect(TokenKind.Semicolon);
            }
            else if (Check(TokenKind.At) && PeekAhead(1).Kind == TokenKind.KwTag)
            {
                tags.AddRange(ParseTagAnnotation().Values);
            }
            else if (Check(TokenKind.KwRationale))
            {
                rationales.Add(ParseRationaleDecl());
            }
            else
            {
                ReportError($"Unexpected token in external system body: {Peek().Kind}");
                Synchronize();
            }
        }

        Expect(TokenKind.RBrace);
        return new ExternalSystemDecl(name, description, technology, tags, rationales, loc);
    }

    // ── Relationship ───────────────────────────────────────────────

    private RelationshipDecl ParseRelationshipDecl()
    {
        var loc = CurrentLocation();
        string source = ExpectName();
        Expect(TokenKind.Arrow);
        string target = ExpectName();

        string description;
        string? technology = null;
        var tags = new List<string>();
        var rationales = new List<RationaleDecl>();

        if (Check(TokenKind.Colon))
        {
            // Short form: Source -> Target : "description";
            Advance();
            description = ExpectStringContent();
            Expect(TokenKind.Semicolon);
        }
        else if (Check(TokenKind.LBrace))
        {
            // Block form: Source -> Target { description: "..."; technology: "..."; }
            Advance();
            description = "";

            while (!Check(TokenKind.RBrace) && !IsAtEnd)
            {
                if (Check(TokenKind.KwDescription))
                {
                    Advance();
                    Expect(TokenKind.Colon);
                    description = ExpectStringContent();
                    Expect(TokenKind.Semicolon);
                }
                else if (Check(TokenKind.KwTechnology))
                {
                    Advance();
                    Expect(TokenKind.Colon);
                    technology = ExpectStringContent();
                    Expect(TokenKind.Semicolon);
                }
                else if (Check(TokenKind.At) && PeekAhead(1).Kind == TokenKind.KwTag)
                {
                    tags.AddRange(ParseTagAnnotation().Values);
                }
                else if (Check(TokenKind.KwRationale))
                {
                    rationales.Add(ParseRationaleDecl());
                }
                else
                {
                    ReportError($"Unexpected token in relationship body: {Peek().Kind}");
                    Synchronize();
                }
            }

            Expect(TokenKind.RBrace);
        }
        else
        {
            ReportError("Expected ':' or '{' after relationship target");
            description = "";
        }

        return new RelationshipDecl(source, target, description, technology, tags, rationales, loc);
    }

    // ── Tag annotation ─────────────────────────────────────────────

    private TagAnnotation ParseTagAnnotation()
    {
        var loc = CurrentLocation();
        Expect(TokenKind.At);
        Expect(TokenKind.KwTag);
        Expect(TokenKind.LParen);

        var values = new List<string>();
        values.Add(ExpectStringContent());
        while (Match(TokenKind.Comma))
        {
            values.Add(ExpectStringContent());
        }

        Expect(TokenKind.RParen);
        Expect(TokenKind.Semicolon);
        return new TagAnnotation(values, loc);
    }

    // ═══════════════════════════════════════════════════════════════
    //  DEPLOYMENT SPECIFICATION PRODUCTIONS
    // ═══════════════════════════════════════════════════════════════

    // ── Deployment ─────────────────────────────────────────────────

    private DeploymentDecl ParseDeploymentDecl()
    {
        var loc = CurrentLocation();
        Expect(TokenKind.KwDeployment);
        string name = ExpectName();
        Expect(TokenKind.LBrace);

        var nodes = new List<DeploymentNodeDecl>();
        var rationales = new List<RationaleDecl>();

        while (!Check(TokenKind.RBrace) && !IsAtEnd)
        {
            if (Check(TokenKind.KwNode))
            {
                nodes.Add(ParseDeploymentNodeDecl());
            }
            else if (Check(TokenKind.KwRationale))
            {
                rationales.Add(ParseRationaleDecl());
            }
            else
            {
                ReportError($"Unexpected token in deployment body: {Peek().Kind}");
                Synchronize();
            }
        }

        Expect(TokenKind.RBrace);
        return new DeploymentDecl(name, nodes, rationales, loc);
    }

    private DeploymentNodeDecl ParseDeploymentNodeDecl()
    {
        var loc = CurrentLocation();
        Expect(TokenKind.KwNode);
        string name = ExpectStringContent();
        Expect(TokenKind.LBrace);

        string? technology = null;
        string? instance = null;
        var childNodes = new List<DeploymentNodeDecl>();
        var tags = new List<string>();
        var rationales = new List<RationaleDecl>();

        while (!Check(TokenKind.RBrace) && !IsAtEnd)
        {
            if (Check(TokenKind.KwTechnology))
            {
                Advance();
                Expect(TokenKind.Colon);
                technology = ExpectStringContent();
                Expect(TokenKind.Semicolon);
            }
            else if (Check(TokenKind.KwInstance))
            {
                Advance();
                Expect(TokenKind.Colon);
                instance = ExpectName();
                Expect(TokenKind.Semicolon);
            }
            else if (Check(TokenKind.KwNode))
            {
                childNodes.Add(ParseDeploymentNodeDecl());
            }
            else if (Check(TokenKind.At) && PeekAhead(1).Kind == TokenKind.KwTag)
            {
                tags.AddRange(ParseTagAnnotation().Values);
            }
            else if (Check(TokenKind.KwRationale))
            {
                rationales.Add(ParseRationaleDecl());
            }
            else
            {
                ReportError($"Unexpected token in deployment node body: {Peek().Kind}");
                Synchronize();
            }
        }

        Expect(TokenKind.RBrace);
        return new DeploymentNodeDecl(name, technology, instance, childNodes, tags, rationales, loc);
    }

    // ═══════════════════════════════════════════════════════════════
    //  VIEW AND DYNAMIC SPECIFICATION PRODUCTIONS
    // ═══════════════════════════════════════════════════════════════

    // ── View ───────────────────────────────────────────────────────

    private ViewDecl ParseViewDecl()
    {
        var loc = CurrentLocation();
        Expect(TokenKind.KwView);

        // Parse ViewKind (an identifier like "systemContext", "container", etc.)
        // "component" and "deployment" are lexed as keywords, so accept them here.
        string kindText;
        if (Check(TokenKind.Ident) || Check(TokenKind.DottedIdent))
            kindText = Advance().Text;
        else if (Check(TokenKind.KwComponent))
            kindText = Advance().Text;
        else if (Check(TokenKind.KwDeployment))
            kindText = Advance().Text;
        else
        {
            ReportError($"Expected view kind, got {Peek().Kind} ('{Peek().Text}')");
            kindText = "<missing>";
        }
        ViewKind kind = kindText switch
        {
            "systemLandscape" => ViewKind.SystemLandscape,
            "systemContext" => ViewKind.SystemContext,
            "container" => ViewKind.Container,
            "component" => ViewKind.Component,
            "deployment" => ViewKind.Deployment,
            _ => ViewKind.SystemContext, // default with diagnostic
        };
        if (kindText is not ("systemLandscape" or "systemContext" or "container" or "component" or "deployment"))
        {
            ReportError($"Unknown view kind '{kindText}'. Expected systemLandscape, systemContext, container, component, or deployment.");
        }

        // Optional "of" scope clause
        string? scope = null;
        if (Check(TokenKind.Ident) && Peek().Text == "of"
            || Check(TokenKind.DottedIdent) && Peek().Text == "of")
        {
            Advance(); // consume "of"
            scope = ExpectName();
        }

        string name = ExpectName();
        Expect(TokenKind.LBrace);

        ViewFilter? include = null;
        ViewFilter? exclude = null;
        LayoutDirection? autoLayout = null;
        string? description = null;
        var tags = new List<string>();
        var rationales = new List<RationaleDecl>();

        while (!Check(TokenKind.RBrace) && !IsAtEnd)
        {
            if (Check(TokenKind.KwInclude))
            {
                Advance();
                Expect(TokenKind.Colon);
                include = ParseViewFilter();
                Expect(TokenKind.Semicolon);
            }
            else if (Check(TokenKind.KwExclude))
            {
                Advance();
                Expect(TokenKind.Colon);
                exclude = ParseViewFilter();
                Expect(TokenKind.Semicolon);
            }
            else if (Check(TokenKind.KwAutoLayout))
            {
                Advance();
                Expect(TokenKind.Colon);
                // Layout direction values may be hyphenated (top-down) or
                // camelCase (topDown). Accept STRING, IDENT, or IDENT-Minus-IDENT.
                string dirText;
                if (Check(TokenKind.String))
                    dirText = StripQuotes(Advance().Text);
                else if (Check(TokenKind.Ident) || Check(TokenKind.DottedIdent))
                {
                    dirText = Advance().Text;
                    // Consume hyphenated form: IDENT "-" IDENT (e.g., top-down)
                    if (Check(TokenKind.Minus) && PeekAhead(1).Kind == TokenKind.Ident)
                    {
                        Advance(); // consume "-"
                        dirText += "-" + Advance().Text;
                    }
                }
                else
                {
                    ReportError("Expected layout direction (top-down, left-right, bottom-up, right-left)");
                    dirText = "top-down";
                }
                autoLayout = dirText switch
                {
                    "top-down" or "topDown" => LayoutDirection.TopDown,
                    "left-right" or "leftRight" => LayoutDirection.LeftRight,
                    "bottom-up" or "bottomUp" => LayoutDirection.BottomUp,
                    "right-left" or "rightLeft" => LayoutDirection.RightLeft,
                    _ => LayoutDirection.TopDown,
                };
                Expect(TokenKind.Semicolon);
            }
            else if (Check(TokenKind.KwDescription))
            {
                Advance();
                Expect(TokenKind.Colon);
                description = ExpectStringContent();
                Expect(TokenKind.Semicolon);
            }
            else if (Check(TokenKind.At) && PeekAhead(1).Kind == TokenKind.KwTag)
            {
                tags.AddRange(ParseTagAnnotation().Values);
            }
            else if (Check(TokenKind.KwRationale))
            {
                rationales.Add(ParseRationaleDecl());
            }
            else
            {
                ReportError($"Unexpected token in view body: {Peek().Kind}");
                Synchronize();
            }
        }

        Expect(TokenKind.RBrace);
        return new ViewDecl(kind, scope, name, include, exclude, autoLayout, description, tags, rationales, loc);
    }

    private ViewFilter ParseViewFilter()
    {
        var loc = CurrentLocation();

        // "all"
        if (Check(TokenKind.KwAll))
        {
            Advance();
            return ViewFilter.AllFilter(loc);
        }

        // "tagged" STRING
        if (Check(TokenKind.Ident) && Peek().Text == "tagged")
        {
            Advance();
            string tag = ExpectStringContent();
            return ViewFilter.TaggedFilter(tag, loc);
        }

        // "[" IdentList "]"
        if (Check(TokenKind.LBracket))
        {
            Advance();
            var elements = new List<string>();
            elements.Add(ExpectName());
            while (Match(TokenKind.Comma))
            {
                elements.Add(ExpectName());
            }
            Expect(TokenKind.RBracket);
            return ViewFilter.ExplicitFilter(elements, loc);
        }

        ReportError("Expected 'all', 'tagged \"...\"', or '[...]' in view filter");
        return ViewFilter.AllFilter(loc);
    }

    // ── Dynamic ────────────────────────────────────────────────────

    private DynamicDecl ParseDynamicDecl()
    {
        var loc = CurrentLocation();
        Expect(TokenKind.KwDynamic);
        string name = ExpectName();
        Expect(TokenKind.LBrace);

        var steps = new List<DynamicStep>();

        while (!Check(TokenKind.RBrace) && !IsAtEnd)
        {
            if (Check(TokenKind.Integer))
            {
                steps.Add(ParseDynamicStep());
            }
            else
            {
                ReportError($"Expected step number in dynamic body, got {Peek().Kind}");
                Synchronize();
            }
        }

        Expect(TokenKind.RBrace);
        return new DynamicDecl(name, steps, loc);
    }

    private DynamicStep ParseDynamicStep()
    {
        var loc = CurrentLocation();
        int seq = int.Parse(Advance().Text); // INTEGER
        Expect(TokenKind.Colon);
        string source = ExpectName();
        Expect(TokenKind.Arrow);
        string target = ExpectName();

        string description;
        string? technology = null;

        if (Check(TokenKind.Colon))
        {
            // Simple form: N: Source -> Target : "description";
            Advance();
            description = ExpectStringContent();
            Match(TokenKind.Semicolon); // optional trailing semicolon
        }
        else if (Check(TokenKind.LBrace))
        {
            // Block form: N: Source -> Target { description: "..."; technology: "..."; }
            Advance();
            description = "";

            while (!Check(TokenKind.RBrace) && !IsAtEnd)
            {
                if (Check(TokenKind.KwDescription))
                {
                    Advance();
                    Expect(TokenKind.Colon);
                    description = ExpectStringContent();
                    Expect(TokenKind.Semicolon);
                }
                else if (Check(TokenKind.KwTechnology))
                {
                    Advance();
                    Expect(TokenKind.Colon);
                    technology = ExpectStringContent();
                    Expect(TokenKind.Semicolon);
                }
                else
                {
                    ReportError($"Unexpected token in dynamic step body: {Peek().Kind}");
                    Synchronize();
                }
            }

            Expect(TokenKind.RBrace);
            Match(TokenKind.Semicolon); // optional trailing semicolon
        }
        else
        {
            ReportError("Expected ':' or '{' after dynamic step target");
            description = "";
        }

        return new DynamicStep(seq, source, target, description, technology, loc);
    }

    // ═══════════════════════════════════════════════════════════════
    //  DESIGN SPECIFICATION PRODUCTIONS
    // ═══════════════════════════════════════════════════════════════

    // ── Page ────────────────────────────────────────────────────────

    private PageDecl ParsePageDecl()
    {
        var loc = CurrentLocation();
        Expect(TokenKind.KwPage);
        string name = ExpectName();
        Expect(TokenKind.LBrace);

        string host = "";
        string route = "";
        var concepts = new List<string>();
        string? role = null;
        var crossLinks = new List<string>();
        var rationales = new List<RationaleDecl>();

        while (!Check(TokenKind.RBrace) && !IsAtEnd)
        {
            if (Check(TokenKind.KwHost))
            {
                Advance();
                Expect(TokenKind.Colon);
                host = ExpectName();
                Expect(TokenKind.Semicolon);
            }
            else if (Check(TokenKind.KwRoute))
            {
                Advance();
                Expect(TokenKind.Colon);
                route = ExpectStringContent();
                Expect(TokenKind.Semicolon);
            }
            else if (Check(TokenKind.KwConcepts))
            {
                Advance();
                Expect(TokenKind.Colon);
                Expect(TokenKind.LBracket);
                concepts = ParseIdentList();
                Expect(TokenKind.RBracket);
                Expect(TokenKind.Semicolon);
            }
            else if (Check(TokenKind.KwRole))
            {
                Advance();
                Expect(TokenKind.Colon);
                role = ExpectStringContent();
                Expect(TokenKind.Semicolon);
            }
            else if (Check(TokenKind.KwCrossLinks))
            {
                Advance();
                Expect(TokenKind.Colon);
                Expect(TokenKind.LBracket);
                crossLinks = ParseIdentList();
                Expect(TokenKind.RBracket);
                Expect(TokenKind.Semicolon);
            }
            else if (Check(TokenKind.KwRationale))
            {
                rationales.Add(ParseRationaleDecl());
            }
            else
            {
                ReportError($"Unexpected token in page body: {Peek().Kind}");
                Synchronize();
            }
        }

        Expect(TokenKind.RBrace);
        return new PageDecl(name, host, route, concepts, role, crossLinks, rationales, [], loc);
    }

    // ── Visualization ───────────────────────────────────────────────

    private VisualizationDecl ParseVisualizationDecl()
    {
        var loc = CurrentLocation();
        Expect(TokenKind.KwVisualization);
        string name = ExpectName();
        Expect(TokenKind.LBrace);

        string? pageRef = null;
        string? componentRef = null;
        var parameters = new List<VisualizationParam>();
        var sliderNames = new List<string>();
        var rationales = new List<RationaleDecl>();

        while (!Check(TokenKind.RBrace) && !IsAtEnd)
        {
            if (Check(TokenKind.KwPage))
            {
                // "page: DOTTED_IDENT;" inside visualization (grammar sec 5.16)
                Advance();
                Expect(TokenKind.Colon);
                pageRef = ExpectName();
                Expect(TokenKind.Semicolon);
            }
            else if (Check(TokenKind.KwComponent))
            {
                Advance();
                Expect(TokenKind.Colon);
                componentRef = ExpectName();
                Expect(TokenKind.Semicolon);
            }
            else if (Check(TokenKind.KwParameters))
            {
                parameters = ParseParametersBlock();
            }
            else if (Check(TokenKind.KwSliders))
            {
                Advance();
                Expect(TokenKind.Colon);
                Expect(TokenKind.LBracket);
                sliderNames = ParseIdentList();
                Expect(TokenKind.RBracket);
                Expect(TokenKind.Semicolon);
            }
            else if (Check(TokenKind.KwRationale))
            {
                rationales.Add(ParseRationaleDecl());
            }
            else
            {
                ReportError($"Unexpected token in visualization body: {Peek().Kind}");
                Synchronize();
            }
        }

        Expect(TokenKind.RBrace);
        return new VisualizationDecl(name, pageRef, componentRef, parameters, sliderNames, rationales, [], loc);
    }

    private List<VisualizationParam> ParseParametersBlock()
    {
        var loc = CurrentLocation();
        Expect(TokenKind.KwParameters);
        Expect(TokenKind.LBrace);

        var bindings = new List<VisualizationParam>();
        while (!Check(TokenKind.RBrace) && !IsAtEnd)
        {
            if (Check(TokenKind.Ident) || IsFieldNameToken(Peek().Kind))
            {
                var bLoc = CurrentLocation();
                string paramName = Advance().Text;
                Expect(TokenKind.Colon);

                Expr valueExpr = ParseExpr();
                Expect(TokenKind.Semicolon);
                bindings.Add(new VisualizationParam(paramName, valueExpr, bLoc));
            }
            else
            {
                ReportError($"Unexpected token in parameters block: {Peek().Kind}");
                Synchronize();
            }
        }

        Expect(TokenKind.RBrace);
        return bindings;
    }

    // ═══════════════════════════════════════════════════════════════
    //  EXPRESSION PARSER (Grammar sec 2.2)
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Entry point for expression parsing.
    /// Expr = ImpliesExpr
    /// </summary>
    private Expr ParseExpr() => ParseImpliesExpr();

    // ── Precedence 1: implication (right-associative) ───────────────

    private Expr ParseImpliesExpr()
    {
        Expr left = ParseOrExpr();
        if (Check(TokenKind.KwImplies))
        {
            var loc = CurrentLocation();
            Advance();
            Expr right = ParseImpliesExpr(); // right-associative
            return new BinaryExpr(left, "implies", right, loc);
        }
        return left;
    }

    // ── Precedence 2: disjunction (left-associative) ────────────────

    private Expr ParseOrExpr()
    {
        Expr left = ParseAndExpr();
        while (Check(TokenKind.KwOr))
        {
            var loc = CurrentLocation();
            Advance();
            Expr right = ParseAndExpr();
            left = new BinaryExpr(left, "or", right, loc);
        }
        return left;
    }

    // ── Precedence 3: conjunction (left-associative) ────────────────

    private Expr ParseAndExpr()
    {
        Expr left = ParseNotExpr();
        while (Check(TokenKind.KwAnd))
        {
            var loc = CurrentLocation();
            Advance();
            Expr right = ParseNotExpr();
            left = new BinaryExpr(left, "and", right, loc);
        }
        return left;
    }

    // ── Precedence 4: negation (prefix, unary) ──────────────────────

    private Expr ParseNotExpr()
    {
        if (Check(TokenKind.KwNot))
        {
            var loc = CurrentLocation();
            Advance();
            Expr operand = ParseNotExpr();
            return new UnaryExpr("not", operand, loc);
        }
        return ParseCompareExpr();
    }

    // ── Precedence 5: comparison (non-associative) ──────────────────

    private Expr ParseCompareExpr()
    {
        Expr left = ParseAccessExpr();

        string? op = TryParseCompareOp();
        if (op is not null)
        {
            var loc = CurrentLocation();
            // The location was before the operator token, but the operator has been consumed.
            // Use left's location for the binary node.
            Expr right = ParseAccessExpr();
            return new BinaryExpr(left, op, right, left.Location);
        }

        return left;
    }

    private string? TryParseCompareOp()
    {
        switch (Peek().Kind)
        {
            case TokenKind.Eq: Advance(); return "==";
            case TokenKind.Neq: Advance(); return "!=";
            case TokenKind.Lt: Advance(); return "<";
            case TokenKind.Gt: Advance(); return ">";
            case TokenKind.Lte: Advance(); return "<=";
            case TokenKind.Gte: Advance(); return ">=";
            case TokenKind.KwContains: Advance(); return "contains";
            case TokenKind.KwExcludes: Advance(); return "excludes";
            case TokenKind.KwIn: Advance(); return "in";
            default: return null;
        }
    }

    // ── Precedence 6: member access and function calls ──────────────

    private Expr ParseAccessExpr()
    {
        Expr expr = ParsePrimaryExpr();

        while (Check(TokenKind.Dot))
        {
            Advance(); // consume "."
            var loc = CurrentLocation();
            string member = ExpectName();

            if (Match(TokenKind.LParen))
            {
                var args = new List<Expr>();
                if (!Check(TokenKind.RParen))
                    args = ParseArgList();
                Expect(TokenKind.RParen);
                expr = new CallExpr(expr, member, args, loc);
            }
            else
            {
                expr = new AccessExpr(expr, member, loc);
            }
        }

        return expr;
    }

    // ── Primary expressions ─────────────────────────────────────────

    private Expr ParsePrimaryExpr()
    {
        var loc = CurrentLocation();

        // Quantifier expressions
        if (Check(TokenKind.KwAll) || Check(TokenKind.KwExists))
            return ParseQuantifierExpr();

        if (Check(TokenKind.KwCount))
            return ParseCountExpr();

        // DOTTED_IDENT: split into AccessExpr chain (grammar sec 5.1)
        if (Check(TokenKind.DottedIdent))
        {
            string text = Advance().Text;
            return SplitDottedIdentIntoAccessChain(text, loc);
        }

        // IDENT (possibly with function call)
        if (Check(TokenKind.Ident))
        {
            string name = Advance().Text;
            if (Match(TokenKind.LParen))
            {
                var args = new List<Expr>();
                if (!Check(TokenKind.RParen))
                    args = ParseArgList();
                Expect(TokenKind.RParen);
                // CallExpr with an implicit "self" target
                return new CallExpr(new IdentifierExpr(name, loc), name, args, loc);
            }
            return new IdentifierExpr(name, loc);
        }

        // Contextual keyword used as identifier in expression (e.g., status == Delivered)
        if (IsContextualKeyword(Peek().Kind))
        {
            string name = Advance().Text;
            return new IdentifierExpr(name, loc);
        }

        // Literals
        if (Check(TokenKind.Integer))
        {
            Token t = Advance();
            if (int.TryParse(t.Text, out int iv))
                return new LiteralExpr(iv, LiteralKind.Integer, loc);
            return new LiteralExpr(long.Parse(t.Text), LiteralKind.Integer, loc);
        }
        if (Check(TokenKind.Float))
        {
            Token t = Advance();
            return new LiteralExpr(double.Parse(t.Text, System.Globalization.CultureInfo.InvariantCulture),
                LiteralKind.Float, loc);
        }
        if (Check(TokenKind.String))
        {
            Token t = Advance();
            return new LiteralExpr(StripQuotes(t.Text), LiteralKind.String, loc);
        }
        if (Check(TokenKind.Boolean))
        {
            Token t = Advance();
            return new LiteralExpr(t.Text == "true", LiteralKind.Boolean, loc);
        }

        // List literal
        if (Check(TokenKind.LBracket))
        {
            Advance();
            var elements = new List<Expr>();
            if (!Check(TokenKind.RBracket))
            {
                elements.Add(ParseExpr());
                while (Match(TokenKind.Comma))
                {
                    if (Check(TokenKind.RBracket)) break;
                    elements.Add(ParseExpr());
                }
            }
            Expect(TokenKind.RBracket);
            return new ListExpr(elements, loc);
        }

        // Parenthesized expression
        if (Check(TokenKind.LParen))
        {
            Advance();
            Expr inner = ParseExpr();
            Expect(TokenKind.RParen);
            return inner;
        }

        ReportError($"Expected expression, got {Peek().Kind} ('{Peek().Text}')");
        Advance(); // skip the bad token
        return new IdentifierExpr("<error>", loc);
    }

    /// <summary>
    /// Split "A.B.C" into AccessExpr(AccessExpr(Ident("A"), "B"), "C").
    /// Segments are plain names, not checked against keyword table (grammar sec 5.1).
    /// </summary>
    private static Expr SplitDottedIdentIntoAccessChain(string dottedText, SourceLocation loc)
    {
        string[] segments = dottedText.Split('.');
        Expr result = new IdentifierExpr(segments[0], loc);
        for (int i = 1; i < segments.Length; i++)
            result = new AccessExpr(result, segments[i], loc);
        return result;
    }

    // ── Quantifier expressions ──────────────────────────────────────

    private Expr ParseQuantifierExpr()
    {
        var loc = CurrentLocation();
        Quantifier quantifier;

        if (Check(TokenKind.KwAll))
        {
            Advance();
            quantifier = Quantifier.All;
        }
        else
        {
            Advance(); // KwExists
            quantifier = Quantifier.Exists;
        }

        // Parse ScopeRef: one or two scope words
        var scopeWords = new List<string>();

        if (IsScopeWord(Peek().Kind))
        {
            scopeWords.Add(Advance().Text);

            // Grammar sec 5.7: peek to determine if scope is one or two words.
            // If the next token is "have" or "satisfy", scope is one word.
            if (!Check(TokenKind.KwHave) && !Check(TokenKind.KwSatisfy)
                && IsScopeWord(Peek().Kind))
            {
                scopeWords.Add(Advance().Text);
            }
        }

        // Grammar sec 2.2: "all" accepts "have" or "satisfy"; "exists" only accepts "have".
        if (quantifier == Quantifier.All)
        {
            if (!Match(TokenKind.KwHave) && !Match(TokenKind.KwSatisfy))
                ReportError("Expected 'have' or 'satisfy' after scope in 'all' quantifier");
        }
        else
        {
            if (!Match(TokenKind.KwHave))
                ReportError("Expected 'have' after scope in 'exists' quantifier");
        }

        Expr body = ParseExpr();
        return new QuantifierExpr(quantifier, scopeWords, body, loc);
    }

    private Expr ParseCountExpr()
    {
        var loc = CurrentLocation();
        Expect(TokenKind.KwCount);
        Expect(TokenKind.LParen);
        Expr body = ParseExpr();
        Expect(TokenKind.RParen);
        return new QuantifierExpr(Quantifier.Count, [], body, loc);
    }

    // ── Argument list ───────────────────────────────────────────────

    private List<Expr> ParseArgList()
    {
        var args = new List<Expr>();
        args.Add(ParseExpr());
        while (Match(TokenKind.Comma))
        {
            if (Check(TokenKind.RParen)) break;
            args.Add(ParseExpr());
        }
        return args;
    }
}
