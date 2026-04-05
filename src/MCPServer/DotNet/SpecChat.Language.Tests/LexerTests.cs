using SpecChat.Language;

namespace SpecChat.Language.Tests;

public class LexerTests
{
    private static List<Token> Lex(string source)
    {
        var diagnostics = new DiagnosticBag();
        var lexer = new Lexer(source, "test.spec", 0, diagnostics);
        return lexer.Tokenize();
    }

    [Fact]
    public void Tokenize_Keywords_ProducesCorrectTokenKinds()
    {
        var tokens = Lex("entity enum contract system topology phase trace constraint package_policy");

        // Last token is always Eof
        Assert.Equal(TokenKind.KwEntity, tokens[0].Kind);
        Assert.Equal(TokenKind.KwEnum, tokens[1].Kind);
        Assert.Equal(TokenKind.KwContract, tokens[2].Kind);
        Assert.Equal(TokenKind.KwSystem, tokens[3].Kind);
        Assert.Equal(TokenKind.KwTopology, tokens[4].Kind);
        Assert.Equal(TokenKind.KwPhase, tokens[5].Kind);
        Assert.Equal(TokenKind.KwTrace, tokens[6].Kind);
        Assert.Equal(TokenKind.KwConstraint, tokens[7].Kind);
        Assert.Equal(TokenKind.KwPackagePolicy, tokens[8].Kind);
        Assert.Equal(TokenKind.Eof, tokens[9].Kind);
    }

    [Fact]
    public void Tokenize_DottedIdent_EmitsSingleToken()
    {
        var tokens = Lex("Dashboard.UI");

        Assert.Equal(2, tokens.Count); // DottedIdent + Eof
        Assert.Equal(TokenKind.DottedIdent, tokens[0].Kind);
        Assert.Equal("Dashboard.UI", tokens[0].Text);
    }

    [Fact]
    public void Tokenize_DottedIdentVsKeyword_SingleSegmentIsKeyword()
    {
        var tokens = Lex("system");

        Assert.Equal(2, tokens.Count);
        Assert.Equal(TokenKind.KwSystem, tokens[0].Kind);
    }

    [Fact]
    public void Tokenize_BooleanLiterals_NotKeywords()
    {
        var tokens = Lex("true false");

        Assert.Equal(3, tokens.Count); // true, false, Eof
        Assert.Equal(TokenKind.Boolean, tokens[0].Kind);
        Assert.Equal("true", tokens[0].Text);
        Assert.Equal(TokenKind.Boolean, tokens[1].Kind);
        Assert.Equal("false", tokens[1].Text);
    }

    [Fact]
    public void Tokenize_Arrow_MatchedBeforeMinus()
    {
        var tokens = Lex("->");

        Assert.Equal(2, tokens.Count);
        Assert.Equal(TokenKind.Arrow, tokens[0].Kind);
        Assert.Equal("->", tokens[0].Text);
    }

    [Fact]
    public void Tokenize_DotDot_MatchedBeforeDot()
    {
        var tokens = Lex("1..10");

        Assert.Equal(4, tokens.Count); // Integer, DotDot, Integer, Eof
        Assert.Equal(TokenKind.Integer, tokens[0].Kind);
        Assert.Equal("1", tokens[0].Text);
        Assert.Equal(TokenKind.DotDot, tokens[1].Kind);
        Assert.Equal("..", tokens[1].Text);
        Assert.Equal(TokenKind.Integer, tokens[2].Kind);
        Assert.Equal("10", tokens[2].Text);
    }

    [Fact]
    public void Tokenize_FloatVsDotDot_Disambiguated()
    {
        // 3.14 should be a single Float token
        var floatTokens = Lex("3.14");
        Assert.Equal(2, floatTokens.Count);
        Assert.Equal(TokenKind.Float, floatTokens[0].Kind);
        Assert.Equal("3.14", floatTokens[0].Text);

        // 3..5 should be Integer DotDot Integer
        var rangeTokens = Lex("3..5");
        Assert.Equal(4, rangeTokens.Count);
        Assert.Equal(TokenKind.Integer, rangeTokens[0].Kind);
        Assert.Equal("3", rangeTokens[0].Text);
        Assert.Equal(TokenKind.DotDot, rangeTokens[1].Kind);
        Assert.Equal(TokenKind.Integer, rangeTokens[2].Kind);
        Assert.Equal("5", rangeTokens[2].Text);
    }

    [Fact]
    public void Tokenize_StringLiteral_MultiLine()
    {
        var tokens = Lex("\"line one\nline two\"");

        Assert.Equal(2, tokens.Count);
        Assert.Equal(TokenKind.String, tokens[0].Kind);
        Assert.Equal("\"line one\nline two\"", tokens[0].Text);
    }

    [Fact]
    public void Tokenize_StringEscapes_Handled()
    {
        var tokens = Lex("\"hello\\n\\t\\\"\\\\world\"");

        Assert.Equal(2, tokens.Count);
        Assert.Equal(TokenKind.String, tokens[0].Kind);
    }

    [Fact]
    public void Tokenize_Comments_Skipped()
    {
        var tokens = Lex("entity // comment\nfoo");

        Assert.Equal(3, tokens.Count); // KwEntity, Ident, Eof
        Assert.Equal(TokenKind.KwEntity, tokens[0].Kind);
        Assert.Equal(TokenKind.Ident, tokens[1].Kind);
        Assert.Equal("foo", tokens[1].Text);
    }

    [Fact]
    public void Tokenize_AllSymbols()
    {
        var tokens = Lex("{ } [ ] ( ) : ; , . ? @ -> .. == != < > <= >=");

        var expected = new[]
        {
            TokenKind.LBrace, TokenKind.RBrace,
            TokenKind.LBracket, TokenKind.RBracket,
            TokenKind.LParen, TokenKind.RParen,
            TokenKind.Colon, TokenKind.Semicolon,
            TokenKind.Comma, TokenKind.Dot,
            TokenKind.Question, TokenKind.At,
            TokenKind.Arrow, TokenKind.DotDot,
            TokenKind.Eq, TokenKind.Neq,
            TokenKind.Lt, TokenKind.Gt,
            TokenKind.Lte, TokenKind.Gte,
            TokenKind.Eof,
        };

        Assert.Equal(expected.Length, tokens.Count);
        for (int i = 0; i < expected.Length; i++)
        {
            Assert.Equal(expected[i], tokens[i].Kind);
        }
    }
}
