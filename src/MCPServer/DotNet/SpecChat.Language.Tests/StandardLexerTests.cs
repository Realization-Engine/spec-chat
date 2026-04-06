using SpecChat.Language;

namespace SpecChat.Language.Tests;

public class StandardLexerTests
{
    [Theory]
    [InlineData("architecture", TokenKind.KwArchitecture)]
    [InlineData("enforce", TokenKind.KwEnforce)]
    [InlineData("vocabulary", TokenKind.KwVocabulary)]
    [InlineData("layer", TokenKind.KwLayer)]
    [InlineData("owns", TokenKind.KwOwns)]
    [InlineData("broker", TokenKind.KwBroker)]
    [InlineData("foundation", TokenKind.KwFoundation)]
    [InlineData("processing", TokenKind.KwProcessing)]
    [InlineData("orchestration", TokenKind.KwOrchestration)]
    [InlineData("coordination", TokenKind.KwCoordination)]
    [InlineData("aggregation", TokenKind.KwAggregation)]
    [InlineData("exposer", TokenKind.KwExposer)]
    [InlineData("test", TokenKind.KwTest)]
    [InlineData("service", TokenKind.KwService)]
    [InlineData("layer_contract", TokenKind.KwLayerContract)]
    [InlineData("realize", TokenKind.KwRealize)]
    [InlineData("validation", TokenKind.KwValidation)]
    [InlineData("suppress", TokenKind.KwSuppress)]
    public void Lex_StandardKeyword_ReturnsCorrectKind(string text, TokenKind expected)
    {
        var lexer = new Lexer(text, "test.spec.md");
        var tokens = lexer.Tokenize();

        Assert.Equal(2, tokens.Count); // keyword + EOF
        Assert.Equal(expected, tokens[0].Kind);
        Assert.Equal(text, tokens[0].Text);
    }

    [Fact]
    public void Lex_KeywordsInsideStrings_NotTokenizedAsKeywords()
    {
        var lexer = new Lexer("\"broker foundation test\"", "test.spec.md");
        var tokens = lexer.Tokenize();

        Assert.Equal(2, tokens.Count); // string + EOF
        Assert.Equal(TokenKind.String, tokens[0].Kind);
    }

    [Fact]
    public void Lex_StandardKeywordsInSequence_AllRecognized()
    {
        var lexer = new Lexer("architecture broker foundation", "test.spec.md");
        var tokens = lexer.Tokenize();

        Assert.Equal(4, tokens.Count);
        Assert.Equal(TokenKind.KwArchitecture, tokens[0].Kind);
        Assert.Equal(TokenKind.KwBroker, tokens[1].Kind);
        Assert.Equal(TokenKind.KwFoundation, tokens[2].Kind);
        Assert.Equal(TokenKind.Eof, tokens[3].Kind);
    }
}
