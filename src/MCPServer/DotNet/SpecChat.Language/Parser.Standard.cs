using SpecChat.Language.Ast;

namespace SpecChat.Language;

/// <summary>
/// Partial class containing parse methods for The Standard extension
/// constructs: architecture, layer_contract, layer-prefixed declarations,
/// vocabulary, and realize directives.
/// </summary>
public sealed partial class Parser
{
    // ── Architecture declaration ────────────────────────────────────

    private ArchitectureDecl ParseArchitectureDecl()
    {
        var loc = CurrentLocation();
        Expect(TokenKind.KwArchitecture);
        string name = ExpectName();
        Expect(TokenKind.LBrace);

        string? version = null;
        var enforceRules = new List<string>();
        VocabularyDecl? vocabulary = null;
        var realizations = new List<RealizeDecl>();
        var rationales = new List<RationaleDecl>();

        while (!Check(TokenKind.RBrace) && !IsAtEnd)
        {
            if (Check(TokenKind.KwVersion))
            {
                Advance();
                Expect(TokenKind.Colon);
                version = ExpectStringContent();
                Expect(TokenKind.Semicolon);
            }
            else if (Check(TokenKind.KwEnforce))
            {
                Advance();
                Expect(TokenKind.Colon);
                Expect(TokenKind.LBracket);
                enforceRules = ParseEnforceList();
                Expect(TokenKind.RBracket);
                Expect(TokenKind.Semicolon);
            }
            else if (Check(TokenKind.KwVocabulary))
            {
                vocabulary = ParseVocabularyDecl();
            }
            else if (Check(TokenKind.KwRealize))
            {
                realizations.Add(ParseRealizeDecl());
            }
            else if (Check(TokenKind.KwRationale))
            {
                rationales.Add(ParseRationaleDecl());
            }
            else
            {
                ReportError($"Unexpected token in architecture body: {Peek().Kind}");
                Synchronize();
            }
        }

        Expect(TokenKind.RBrace);
        return new ArchitectureDecl(name, version, enforceRules, vocabulary,
            realizations, rationales, loc);
    }

    // ── Layer contract declaration ──────────────────────────────────

    private LayerContractDecl ParseLayerContractDecl()
    {
        var loc = CurrentLocation();
        Expect(TokenKind.KwLayerContract);
        string name = ExpectName();
        Expect(TokenKind.LBrace);

        string layerName = "";
        var clauses = new List<ContractClause>();
        var rationales = new List<RationaleDecl>();

        while (!Check(TokenKind.RBrace) && !IsAtEnd)
        {
            if (Check(TokenKind.KwLayer))
            {
                Advance();
                Expect(TokenKind.Colon);
                layerName = ParseLayerKeywordOrIdent();
                Expect(TokenKind.Semicolon);
            }
            else if (Check(TokenKind.KwRequires) || Check(TokenKind.KwEnsures)
                     || Check(TokenKind.KwGuarantees))
            {
                var parsed = ParseContractClauses();
                clauses.AddRange(parsed);
            }
            else if (Check(TokenKind.KwRationale))
            {
                rationales.Add(ParseRationaleDecl());
            }
            else
            {
                ReportError($"Unexpected token in layer_contract body: {Peek().Kind}");
                Synchronize();
            }
        }

        Expect(TokenKind.RBrace);
        return new LayerContractDecl(name, layerName, clauses, rationales, loc);
    }

    // ── Layer-prefixed declarations (desugar to ComponentDecl) ──────

    /// <summary>
    /// Parses broker/exposer/test Name { ... } and desugars to ComponentDecl.
    /// </summary>
    private ComponentDecl ParseLayerPrefixedDecl(string layer)
    {
        var loc = CurrentLocation();
        Advance(); // consume the layer keyword (broker/exposer/test)
        string name = ExpectNameOrLayerKeyword();
        Expect(TokenKind.LBrace);

        return ParseLayerPrefixedBody(name, layer, loc);
    }

    /// <summary>
    /// Parses foundation/processing/orchestration/coordination/aggregation service Name { ... }.
    /// </summary>
    private ComponentDecl ParseServiceDecl(string layer)
    {
        var loc = CurrentLocation();
        Advance(); // consume the layer keyword
        Expect(TokenKind.KwService); // consume "service"
        string name = ExpectNameOrLayerKeyword();
        Expect(TokenKind.LBrace);

        return ParseLayerPrefixedBody(name, layer, loc);
    }

    /// <summary>
    /// Shared body parser for layer-prefixed declarations.
    /// Accepts the same properties as authored component minus "layer" (implied).
    /// </summary>
    private ComponentDecl ParseLayerPrefixedBody(string name, string layer, SourceLocation loc)
    {
        string? kind = null;
        string? path = null;
        string? status = null;
        string? responsibility = null;
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
                ReportError($"Unexpected token in {layer} component body: {Peek().Kind}");
                Synchronize();
            }
        }

        Expect(TokenKind.RBrace);
        return new ComponentDecl(
            name, ComponentDisposition.Authored, kind, path, status,
            responsibility, [], null, null, contracts, rationales,
            layer, owns, suppressions, loc);
    }

    // ── Vocabulary declaration ──────────────────────────────────────

    private VocabularyDecl ParseVocabularyDecl()
    {
        var loc = CurrentLocation();
        Expect(TokenKind.KwVocabulary);
        Expect(TokenKind.LBrace);

        var mappings = new List<VocabularyMapping>();

        while (!Check(TokenKind.RBrace) && !IsAtEnd)
        {
            var mLoc = CurrentLocation();
            string layerName = ParseLayerKeywordOrIdent();
            Expect(TokenKind.Colon);
            Expect(TokenKind.LBracket);
            var verbs = ParseEnforceList(); // reuse: accepts ident or keyword tokens
            Expect(TokenKind.RBracket);
            Expect(TokenKind.Semicolon);
            mappings.Add(new VocabularyMapping(layerName, verbs, mLoc));
        }

        Expect(TokenKind.RBrace);
        return new VocabularyDecl(mappings, loc);
    }

    // ── Realize directive ───────────────────────────────────────────

    private RealizeDecl ParseRealizeDecl()
    {
        var loc = CurrentLocation();
        Expect(TokenKind.KwRealize);
        string layerName = ParseLayerKeywordOrIdent();
        Expect(TokenKind.LBrace);

        var directives = new List<string>();

        while (!Check(TokenKind.RBrace) && !IsAtEnd)
        {
            if (Check(TokenKind.String))
            {
                directives.Add(StripQuotes(Advance().Text));
                if (Check(TokenKind.Semicolon)) Advance();
            }
            else
            {
                ReportError($"Expected string directive in realize block, got {Peek().Kind}");
                Synchronize();
            }
        }

        Expect(TokenKind.RBrace);
        return new RealizeDecl(layerName, directives, loc);
    }

    // ── Shared helpers for The Standard extension ───────────────────

    /// <summary>
    /// Accepts any layer keyword or plain IDENT and returns the text.
    /// Used in positions where a layer name is expected as a value.
    /// </summary>
    private string ParseLayerKeywordOrIdent()
    {
        TokenKind kind = Peek().Kind;
        if (kind == TokenKind.Ident || kind == TokenKind.DottedIdent
            || kind == TokenKind.KwBroker || kind == TokenKind.KwFoundation
            || kind == TokenKind.KwProcessing || kind == TokenKind.KwOrchestration
            || kind == TokenKind.KwCoordination || kind == TokenKind.KwAggregation
            || kind == TokenKind.KwExposer || kind == TokenKind.KwTest
            || kind == TokenKind.KwService)
        {
            return Advance().Text;
        }

        ReportError($"Expected layer name or identifier, got {Peek().Kind}");
        return "<missing>";
    }

    /// <summary>
    /// Parses a comma-separated list that may contain identifiers or keywords.
    /// Used for enforce lists and vocabulary verb lists where values like
    /// "vocabulary" lex as keywords rather than plain identifiers.
    /// </summary>
    private List<string> ParseEnforceList()
    {
        var list = new List<string>();

        if (IsIdentOrKeyword(Peek().Kind))
        {
            list.Add(Advance().Text);
            while (Match(TokenKind.Comma))
            {
                if (Check(TokenKind.RBracket)) break; // trailing comma
                if (IsIdentOrKeyword(Peek().Kind))
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
    /// Accepts name tokens for component names in layer-prefixed positions.
    /// Includes Ident, DottedIdent, and layer keywords that might be used as names.
    /// </summary>
    private string ExpectNameOrLayerKeyword()
    {
        if (Check(TokenKind.Ident) || Check(TokenKind.DottedIdent) || IsLayerKeyword(Peek().Kind))
            return Advance().Text;

        ReportError($"Expected component name, got {Peek().Kind}");
        return "<missing>";
    }

    /// <summary>
    /// Returns true when the token is any identifier or keyword (for list parsing).
    /// </summary>
    private static bool IsIdentOrKeyword(TokenKind kind) =>
        kind == TokenKind.Ident || kind == TokenKind.DottedIdent || IsContextualKeyword(kind);

    /// <summary>
    /// Returns true for layer-specific keywords.
    /// </summary>
    private static bool IsLayerKeyword(TokenKind kind) => kind switch
    {
        TokenKind.KwBroker or TokenKind.KwFoundation or TokenKind.KwProcessing
            or TokenKind.KwOrchestration or TokenKind.KwCoordination
            or TokenKind.KwAggregation or TokenKind.KwExposer
            or TokenKind.KwTest or TokenKind.KwService => true,
        _ => false,
    };

    /// <summary>
    /// Returns true when the token can serve as a name (Ident, DottedIdent, or layer keyword).
    /// Used for lookahead in layer-prefixed dispatch.
    /// </summary>
    private static bool IsNameToken(TokenKind kind) =>
        kind == TokenKind.Ident || kind == TokenKind.DottedIdent || IsLayerKeyword(kind);
}
