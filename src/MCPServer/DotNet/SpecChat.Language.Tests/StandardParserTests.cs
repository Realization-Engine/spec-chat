using SpecChat.Language;
using SpecChat.Language.Ast;

namespace SpecChat.Language.Tests;

public class StandardParserTests
{
    private static SpecDocument ParseSource(string source)
    {
        var diag = new DiagnosticBag();
        var lexer = new Lexer(source, "test.spec.md", diagnostics: diag);
        var tokens = lexer.Tokenize();
        var parser = new Parser(tokens, "test.spec.md", diag);
        return parser.Parse();
    }

    [Fact]
    public void Parse_ArchitectureDecl_BasicFields()
    {
        var doc = ParseSource("""
            architecture TheStandard {
                version: "1.0";
                enforce: [layers, flow_forward, vocabulary];
            }
            """);

        Assert.Single(doc.Declarations);
        var arch = Assert.IsType<ArchitectureDecl>(doc.Declarations[0]);
        Assert.Equal("TheStandard", arch.Name);
        Assert.Equal("1.0", arch.Version);
        Assert.Equal(3, arch.EnforceRules.Count);
        Assert.Contains("layers", arch.EnforceRules);
        Assert.Contains("flow_forward", arch.EnforceRules);
        Assert.Contains("vocabulary", arch.EnforceRules);
    }

    [Fact]
    public void Parse_ArchitectureDecl_WithVocabulary()
    {
        var doc = ParseSource("""
            architecture TheStandard {
                vocabulary {
                    broker: [Insert, Select, Update, Delete];
                    foundation: [Add, Retrieve, Modify, Remove];
                }
            }
            """);

        var arch = Assert.IsType<ArchitectureDecl>(doc.Declarations[0]);
        Assert.NotNull(arch.Vocabulary);
        Assert.Equal(2, arch.Vocabulary!.Mappings.Count);
        Assert.Equal("broker", arch.Vocabulary.Mappings[0].LayerName);
        Assert.Equal(4, arch.Vocabulary.Mappings[0].Verbs.Count);
        Assert.Equal("foundation", arch.Vocabulary.Mappings[1].LayerName);
    }

    [Fact]
    public void Parse_ArchitectureDecl_WithRealize()
    {
        var doc = ParseSource("""
            architecture TheStandard {
                realize broker {
                    "Partial classes per entity.";
                    "Support brokers single-file.";
                }
            }
            """);

        var arch = Assert.IsType<ArchitectureDecl>(doc.Declarations[0]);
        Assert.Single(arch.Realizations);
        Assert.Equal("broker", arch.Realizations[0].LayerName);
        Assert.Equal(2, arch.Realizations[0].Directives.Count);
    }

    [Fact]
    public void Parse_ArchitectureDecl_WithRationale()
    {
        var doc = ParseSource("""
            architecture TheStandard {
                rationale {
                    context "Test system.";
                    decision "All components declare layer.";
                    consequence "Semantic analysis enforces rules.";
                }
            }
            """);

        var arch = Assert.IsType<ArchitectureDecl>(doc.Declarations[0]);
        Assert.Single(arch.Rationales);
        Assert.True(arch.Rationales[0].IsStructured);
    }

    [Fact]
    public void Parse_LayerContractDecl_ParsesClauses()
    {
        var doc = ParseSource("""
            layer_contract FoundationContract {
                layer: foundation;
                guarantees "TryCatch wrapping on every public method.";
                guarantees "Single broker call per method.";
            }
            """);

        Assert.Single(doc.Declarations);
        var lc = Assert.IsType<LayerContractDecl>(doc.Declarations[0]);
        Assert.Equal("FoundationContract", lc.Name);
        Assert.Equal("foundation", lc.LayerName);
        Assert.Equal(2, lc.Clauses.Count);
    }

    [Fact]
    public void Parse_BrokerDecl_DesugarsToComponentDecl()
    {
        var doc = ParseSource("""
            system TestSystem {
                target: "net10.0";
                responsibility: "Test.";

                broker StorageBroker {
                    kind: library;
                    path: "src/Brokers/Storage";
                    responsibility: "Wraps storage.";
                }
            }
            """);

        var system = Assert.IsType<SystemDecl>(doc.Declarations[0]);
        Assert.Single(system.Components);
        var comp = system.Components[0];
        Assert.Equal("StorageBroker", comp.Name);
        Assert.Equal(ComponentDisposition.Authored, comp.Disposition);
        Assert.Equal("broker", comp.Layer);
    }

    [Fact]
    public void Parse_FoundationService_DesugarsToComponentDecl()
    {
        var doc = ParseSource("""
            system TestSystem {
                target: "net10.0";
                responsibility: "Test.";

                foundation service OrderService {
                    kind: library;
                    owns: Order;
                    responsibility: "Foundation service.";
                }
            }
            """);

        var system = Assert.IsType<SystemDecl>(doc.Declarations[0]);
        var comp = system.Components[0];
        Assert.Equal("OrderService", comp.Name);
        Assert.Equal("foundation", comp.Layer);
        Assert.Equal("Order", comp.Owns);
    }

    [Fact]
    public void Parse_ExposerDecl_DesugarsToComponentDecl()
    {
        var doc = ParseSource("""
            system TestSystem {
                target: "net10.0";
                responsibility: "Test.";

                exposer MyApp.Web {
                    kind: executable;
                    responsibility: "API controller.";
                }
            }
            """);

        var system = Assert.IsType<SystemDecl>(doc.Declarations[0]);
        var comp = system.Components[0];
        Assert.Equal("MyApp.Web", comp.Name);
        Assert.Equal("exposer", comp.Layer);
    }

    [Fact]
    public void Parse_TestDecl_DesugarsToComponentDecl()
    {
        var doc = ParseSource("""
            system TestSystem {
                target: "net10.0";
                responsibility: "Test.";

                test MyApp.Tests {
                    kind: library;
                    responsibility: "Unit tests.";
                }
            }
            """);

        var system = Assert.IsType<SystemDecl>(doc.Declarations[0]);
        var comp = system.Components[0];
        Assert.Equal("MyApp.Tests", comp.Name);
        Assert.Equal("test", comp.Layer);
    }

    [Fact]
    public void Parse_AuthoredComponent_LayerOwnsSuppress()
    {
        var doc = ParseSource("""
            system TestSystem {
                target: "net10.0";
                responsibility: "Test.";

                authored component MyService {
                    kind: library;
                    layer: foundation;
                    owns: Order;
                    @suppress(autonomy);
                    responsibility: "Service.";
                }
            }
            """);

        var system = Assert.IsType<SystemDecl>(doc.Declarations[0]);
        var comp = system.Components[0];
        Assert.Equal("foundation", comp.Layer);
        Assert.Equal("Order", comp.Owns);
        Assert.Single(comp.Suppressions);
        Assert.Equal("autonomy", comp.Suppressions[0]);
    }

    [Fact]
    public void Parse_ValidationAnnotation_OnRequiresClause()
    {
        var doc = ParseSource("""
            contract MyContract {
                requires name != "" @validation(structural);
                requires count > 0;
                ensures result != null;
            }
            """);

        var contract = Assert.IsType<ContractDecl>(doc.Declarations[0]);
        Assert.Equal(3, contract.Clauses.Count);
        Assert.Equal("structural", contract.Clauses[0].ValidationCategory);
        Assert.Null(contract.Clauses[1].ValidationCategory);
        Assert.Null(contract.Clauses[2].ValidationCategory);
    }

    [Fact]
    public void Parse_EnforceList_WithVocabularyKeyword()
    {
        // "vocabulary" is a keyword but must be accepted in the enforce list
        var doc = ParseSource("""
            architecture TheStandard {
                enforce: [layers, vocabulary, autonomy];
            }
            """);

        var arch = Assert.IsType<ArchitectureDecl>(doc.Declarations[0]);
        Assert.Equal(3, arch.EnforceRules.Count);
        Assert.Contains("vocabulary", arch.EnforceRules);
    }

    [Fact]
    public void Parse_StandardTestFixture_NoDiagnostics()
    {
        string dir = Path.GetDirectoryName(typeof(StandardParserTests).Assembly.Location)!;
        string fixturePath = Path.Combine(dir, "..", "..", "..", "tests", "fixtures", "standard-test.spec.md");
        fixturePath = Path.GetFullPath(fixturePath);

        // Fallback: try repo root relative path
        if (!File.Exists(fixturePath))
        {
            fixturePath = Path.GetFullPath(Path.Combine(dir, "..", "..", "..", "..", "..", "..",
                "tests", "fixtures", "standard-test.spec.md"));
        }

        if (!File.Exists(fixturePath))
        {
            // Skip if fixture not found in CI
            return;
        }

        var content = File.ReadAllText(fixturePath);
        var extractor = new SpecBlockExtractor();
        var blocks = extractor.Extract(content, fixturePath);

        var diag = new DiagnosticBag();
        var allDecls = new List<TopLevelDecl>();
        foreach (var block in blocks)
        {
            var lexer = new Lexer(block.Content, fixturePath, block.StartLine, diag);
            var tokens = lexer.Tokenize();
            var parser = new Parser(tokens, fixturePath, diag);
            var doc = parser.Parse();
            allDecls.AddRange(doc.Declarations);
        }

        var errors = diag.Diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error).ToList();
        Assert.Empty(errors);
        Assert.True(allDecls.Count >= 3); // architecture + system + entity
        Assert.Contains(allDecls, d => d is ArchitectureDecl);
        Assert.Contains(allDecls, d => d is SystemDecl);
        Assert.Contains(allDecls, d => d is EntityDecl);
    }
}
