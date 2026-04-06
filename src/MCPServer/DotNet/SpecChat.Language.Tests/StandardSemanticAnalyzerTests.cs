using SpecChat.Language;
using SpecChat.Language.Ast;

namespace SpecChat.Language.Tests;

public class StandardSemanticAnalyzerTests
{
    private static (SpecDocument Document, DiagnosticBag Diagnostics) ParseAndAnalyze(string source)
    {
        var diag = new DiagnosticBag();
        var lexer = new Lexer(source, "test.spec.md", diagnostics: diag);
        var tokens = lexer.Tokenize();
        var parser = new Parser(tokens, "test.spec.md", diag);
        var doc = parser.Parse();

        var analyzer = new SemanticAnalyzer(diag);
        analyzer.Analyze(doc);

        return (doc, diag);
    }

    [Fact]
    public void CheckLayers_MissingLayer_ReportsError()
    {
        var (_, diag) = ParseAndAnalyze("""
            architecture TheStandard {
                enforce: [layers];
            }
            system TestSystem {
                target: "net10.0";
                responsibility: "Test.";
                authored component NoLayer {
                    kind: library;
                    responsibility: "Missing layer.";
                }
            }
            """);

        Assert.Contains(diag.Diagnostics,
            d => d.Severity == DiagnosticSeverity.Error && d.Message.Contains("no layer property"));
    }

    [Fact]
    public void CheckLayers_ValidLayer_NoError()
    {
        var (_, diag) = ParseAndAnalyze("""
            architecture TheStandard {
                enforce: [layers];
            }
            system TestSystem {
                target: "net10.0";
                responsibility: "Test.";
                broker TestBroker {
                    kind: library;
                    responsibility: "Has layer.";
                }
            }
            """);

        var layerErrors = diag.Diagnostics
            .Where(d => d.Message.Contains("no layer property"))
            .ToList();
        Assert.Empty(layerErrors);
    }

    [Fact]
    public void CheckEntityOwnership_MissingOwns_ReportsWarning()
    {
        var (_, diag) = ParseAndAnalyze("""
            architecture TheStandard {
                enforce: [entity_ownership];
            }
            system TestSystem {
                target: "net10.0";
                responsibility: "Test.";
                foundation service NoOwns {
                    kind: library;
                    responsibility: "No owns.";
                }
            }
            """);

        Assert.Contains(diag.Diagnostics,
            d => d.Message.Contains("does not declare entity ownership"));
    }

    [Fact]
    public void CheckEntityOwnership_DuplicateOwns_ReportsError()
    {
        var (_, diag) = ParseAndAnalyze("""
            architecture TheStandard {
                enforce: [entity_ownership];
            }
            system TestSystem {
                target: "net10.0";
                responsibility: "Test.";
                foundation service ServiceA {
                    kind: library;
                    owns: Order;
                    responsibility: "A.";
                }
                foundation service ServiceB {
                    kind: library;
                    owns: Order;
                    responsibility: "B.";
                }
            }
            """);

        Assert.Contains(diag.Diagnostics,
            d => d.Severity == DiagnosticSeverity.Error && d.Message.Contains("already owned by"));
    }

    [Fact]
    public void EnforceGating_OnlyActivatedRulesFire()
    {
        // Only enforce entity_ownership; layers check should NOT fire
        var (_, diag) = ParseAndAnalyze("""
            architecture TheStandard {
                enforce: [entity_ownership];
            }
            system TestSystem {
                target: "net10.0";
                responsibility: "Test.";
                authored component NoLayerNoOwns {
                    kind: library;
                    responsibility: "No layer, no owns.";
                }
            }
            """);

        // Should NOT have "no layer property" error (layers not enforced)
        Assert.DoesNotContain(diag.Diagnostics,
            d => d.Message.Contains("no layer property"));
        // Should have entity_ownership warning (but only for foundation components)
        // Since this component has no layer, entity_ownership skips it
    }

    [Fact]
    public void WarnLayerPrefixedWithoutArchitecture_WarnsOnLayerProperty()
    {
        var (_, diag) = ParseAndAnalyze("""
            system TestSystem {
                target: "net10.0";
                responsibility: "Test.";
                authored component HasLayer {
                    kind: library;
                    layer: broker;
                    responsibility: "Has layer but no architecture.";
                }
            }
            """);

        Assert.Contains(diag.Diagnostics,
            d => d.Message.Contains("no architecture declaration is active"));
    }

    [Fact]
    public void Suppress_SkipsRule()
    {
        var (_, diag) = ParseAndAnalyze("""
            architecture TheStandard {
                enforce: [layers];
            }
            system TestSystem {
                target: "net10.0";
                responsibility: "Test.";
                authored component Suppressed {
                    kind: library;
                    @suppress(layers);
                    responsibility: "Suppressed.";
                }
            }
            """);

        var layerErrors = diag.Diagnostics
            .Where(d => d.Message.Contains("no layer property") && d.Message.Contains("Suppressed"))
            .ToList();
        Assert.Empty(layerErrors);
    }
}
