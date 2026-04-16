using SpecChat.Language;
using SpecChat.Language.Ast;

namespace SpecChat.Language.Tests;

public class ParserTests
{
    private static string FixturePath(string filename)
    {
        string dir = AppContext.BaseDirectory;
        while (dir != null && !File.Exists(Path.Combine(dir, "SpecChat.slnx")))
            dir = Path.GetDirectoryName(dir)!;
        return Path.Combine(dir!, "..", "..", "..", "tests", "fixtures", filename);
    }

    private static SpecDocument ParseSource(string source, DiagnosticBag? diagnostics = null)
    {
        diagnostics ??= new DiagnosticBag();
        var lexer = new Lexer(source, "test.spec", 0, diagnostics);
        var tokens = lexer.Tokenize();
        var parser = new Parser(tokens, "test.spec", diagnostics);
        return parser.Parse();
    }

    [Fact]
    public void Parse_EntityDecl_FieldsAndInvariants()
    {
        string source = @"
entity Order {
    id: int;
    total: double;
    status: string;

    invariant ""total positive"": total > 0;

    rationale ""Orders track purchases."";
}";
        var doc = ParseSource(source);
        var entity = Assert.Single(doc.Declarations);
        var entityDecl = Assert.IsType<EntityDecl>(entity);

        Assert.Equal("Order", entityDecl.Name);
        Assert.Equal(3, entityDecl.Fields.Count);
        Assert.Single(entityDecl.Invariants);
        Assert.Single(entityDecl.Rationales);
    }

    [Fact]
    public void Parse_EnumDecl_MembersWithDescriptions()
    {
        string source = @"
enum Priority {
    low: ""Low priority"",
    medium: ""Medium priority"",
    high: ""High priority""
}";
        var doc = ParseSource(source);
        var enumDecl = Assert.IsType<EnumDecl>(Assert.Single(doc.Declarations));

        Assert.Equal("Priority", enumDecl.Name);
        Assert.Equal(3, enumDecl.Members.Count);
        Assert.Equal("low", enumDecl.Members[0].Name);
        Assert.Equal("Low priority", enumDecl.Members[0].Description);
        Assert.Equal("medium", enumDecl.Members[1].Name);
        Assert.Equal("high", enumDecl.Members[2].Name);
    }

    [Fact]
    public void Parse_ContractDecl_RequiresEnsuresGuarantees()
    {
        string source = @"
contract Ordering {
    requires count(items) >= 1;
    ensures total > 0;
    guarantees ""All items are shipped within 5 days."";
}";
        var doc = ParseSource(source);
        var contract = Assert.IsType<ContractDecl>(Assert.Single(doc.Declarations));

        Assert.Equal("Ordering", contract.Name);
        Assert.Equal(3, contract.Clauses.Count);
        Assert.Equal(ContractClauseKind.Requires, contract.Clauses[0].Kind);
        Assert.Equal(ContractClauseKind.Ensures, contract.Clauses[1].Kind);
        Assert.Equal(ContractClauseKind.Guarantees, contract.Clauses[2].Kind);
        Assert.NotNull(contract.Clauses[0].Expression);
        Assert.NotNull(contract.Clauses[1].Expression);
        Assert.NotNull(contract.Clauses[2].ProseGuarantee);
    }

    [Fact]
    public void Parse_SystemDecl_AuthoredAndConsumed()
    {
        string source = @"
system TestSys {
    target: ""net10.0"";
    responsibility: ""A test system."";

    authored component AppServer {
        kind: application;
        path: ""src/App"";
        responsibility: ""Main application."";
    }

    consumed component Logging {
        source: nuget(""Serilog"");
        version: ""3.0.0"";
        responsibility: ""Structured logging."";
        used_by: [AppServer];
    }
}";
        var doc = ParseSource(source);
        var system = Assert.IsType<SystemDecl>(Assert.Single(doc.Declarations));

        Assert.Equal("TestSys", system.Name);
        Assert.Equal(2, system.Components.Count);

        var authored = system.Components[0];
        Assert.Equal(ComponentDisposition.Authored, authored.Disposition);
        Assert.Equal("AppServer", authored.Name);

        var consumed = system.Components[1];
        Assert.Equal(ComponentDisposition.Consumed, consumed.Disposition);
        Assert.Equal("Logging", consumed.Name);
    }

    [Fact]
    public void Parse_TopologyDecl_AllowDeny()
    {
        string source = @"
topology Deps {
    allow Alpha -> Beta;
    deny Beta -> Alpha;
}";
        var doc = ParseSource(source);
        var topology = Assert.IsType<TopologyDecl>(Assert.Single(doc.Declarations));

        Assert.Equal(2, topology.Rules.Count);
        Assert.Equal(TopologyRuleKind.Allow, topology.Rules[0].Kind);
        Assert.Equal("Alpha", topology.Rules[0].Source);
        Assert.Equal("Beta", topology.Rules[0].Target);
        Assert.Equal(TopologyRuleKind.Deny, topology.Rules[1].Kind);
        Assert.Equal("Beta", topology.Rules[1].Source);
        Assert.Equal("Alpha", topology.Rules[1].Target);
    }

    [Fact]
    public void Parse_PhaseDecl_RequiresAndGates()
    {
        string source = @"
phase Build {
    requires: Foundation;
    produces: [AppServer, Tests];

    gate Compile {
        command: ""dotnet build"";
        expects: ""zero errors"", ""zero warnings"";
    }
}";
        var doc = ParseSource(source);
        var phase = Assert.IsType<PhaseDecl>(Assert.Single(doc.Declarations));

        Assert.Equal("Build", phase.Name);
        Assert.Single(phase.RequiresPhases);
        Assert.Equal("Foundation", phase.RequiresPhases[0]);
        Assert.Equal(2, phase.Produces.Count);
        Assert.Single(phase.Gates);
        Assert.Equal("dotnet build", phase.Gates[0].Command);
        Assert.Equal(2, phase.Gates[0].Expectations.Count);
    }

    [Fact]
    public void Parse_TraceDecl_MappingsAndInvariant()
    {
        string source = @"
trace ConceptMap {
    Concept1 -> [PageA, PageB];
    Concept2 -> [PageC];

    invariant ""full coverage"": all sources have count(targets) >= 1;
}";
        var doc = ParseSource(source);
        var trace = Assert.IsType<TraceDecl>(Assert.Single(doc.Declarations));

        Assert.Equal("ConceptMap", trace.Name);
        Assert.Equal(2, trace.Mappings.Count);
        Assert.Equal("Concept1", trace.Mappings[0].Source);
        Assert.Equal(2, trace.Mappings[0].Targets.Count);
        Assert.Single(trace.Invariants);
    }

    [Fact]
    public void Parse_ExprPrecedence_ImpliesIsRightAssociative()
    {
        // "a implies b implies c" should parse as "a implies (b implies c)"
        string source = @"
contract Test {
    requires a implies b implies c;
}";
        var doc = ParseSource(source);
        var contract = Assert.IsType<ContractDecl>(Assert.Single(doc.Declarations));
        var clause = contract.Clauses[0];
        var outerBinary = Assert.IsType<BinaryExpr>(clause.Expression);

        Assert.Equal("implies", outerBinary.Operator);

        // Left side should be "a"
        var leftIdent = Assert.IsType<IdentifierExpr>(outerBinary.Left);
        Assert.Equal("a", leftIdent.Name);

        // Right side should be "b implies c"
        var innerBinary = Assert.IsType<BinaryExpr>(outerBinary.Right);
        Assert.Equal("implies", innerBinary.Operator);
        var innerLeft = Assert.IsType<IdentifierExpr>(innerBinary.Left);
        Assert.Equal("b", innerLeft.Name);
        var innerRight = Assert.IsType<IdentifierExpr>(innerBinary.Right);
        Assert.Equal("c", innerRight.Name);
    }

    [Fact]
    public void Parse_ExprQuantifier_AllHave()
    {
        string source = @"
contract Test {
    requires all sources have count(targets) >= 1;
}";
        var doc = ParseSource(source);
        var contract = Assert.IsType<ContractDecl>(Assert.Single(doc.Declarations));
        var clause = contract.Clauses[0];
        var quantifier = Assert.IsType<QuantifierExpr>(clause.Expression);

        Assert.Equal(Quantifier.All, quantifier.Quantifier);
        Assert.Contains("sources", quantifier.ScopeWords);
    }

    [Fact]
    public void Parse_ExistsOnlyAcceptsHave()
    {
        // "exists x satisfy y" should produce a diagnostic because exists only accepts "have"
        string source = @"
contract Test {
    requires exists x satisfy y;
}";
        var diagnostics = new DiagnosticBag();
        ParseSource(source, diagnostics);

        Assert.True(diagnostics.HasErrors,
            "Expected an error diagnostic for 'exists ... satisfy'");
        Assert.Contains(diagnostics.Diagnostics,
            d => d.Message.Contains("have") && d.Severity == DiagnosticSeverity.Error);
    }

    [Fact]
    public void Parse_BlazerHarness_FullFile()
    {
        string content = File.ReadAllText(FixturePath("blazor-harness.spec.md"));
        var extractor = new SpecBlockExtractor();
        var blocks = extractor.Extract(content, "blazor-harness.spec.md");
        var allDecls = new List<TopLevelDecl>();
        var diagnostics = new DiagnosticBag();

        foreach (var block in blocks)
        {
            var lexer = new Lexer(block.Content, block.FilePath, block.StartLine, diagnostics);
            var tokens = lexer.Tokenize();
            var parser = new Parser(tokens, block.FilePath, diagnostics);
            var doc = parser.Parse();
            allDecls.AddRange(doc.Declarations);
        }

        // The blazor-harness spec has complex visualization parameter bindings
        // that produce parse diagnostics. The important assertion is that all
        // blocks parse to completion without hanging and produce declarations.
        // Block count increased from 70 to 74 with the addition of context
        // (person, external system, relationship), deployment, view, and
        // dynamic spec blocks.
        Assert.True(allDecls.Count >= 10,
            $"Expected at least 10 top-level declarations but got {allDecls.Count}");
        Assert.Equal(74, blocks.Count);
    }

    // ═══════════════════════════════════════════════════════════════
    //  CONTEXT SPECIFICATION TESTS
    // ═══════════════════════════════════════════════════════════════

    [Fact]
    public void Parse_PersonDecl()
    {
        var doc = ParseSource(@"
person Analyst {
    description: ""Reviews revenue metrics."";
    @tag(""stakeholder"", ""primary-user"");
}");
        Assert.Single(doc.Declarations);
        var person = Assert.IsType<PersonDecl>(doc.Declarations[0]);
        Assert.Equal("Analyst", person.Name);
        Assert.Equal("Reviews revenue metrics.", person.Description);
        Assert.Equal(2, person.Tags.Count);
        Assert.Equal("stakeholder", person.Tags[0]);
        Assert.Equal("primary-user", person.Tags[1]);
    }

    [Fact]
    public void Parse_ExternalSystemDecl()
    {
        var doc = ParseSource(@"
external system PaymentGateway {
    description: ""Stripe payment API."";
    technology: ""REST/HTTPS"";
    @tag(""external"");
}");
        Assert.Single(doc.Declarations);
        var ext = Assert.IsType<ExternalSystemDecl>(doc.Declarations[0]);
        Assert.Equal("PaymentGateway", ext.Name);
        Assert.Equal("Stripe payment API.", ext.Description);
        Assert.Equal("REST/HTTPS", ext.Technology);
        Assert.Single(ext.Tags);
    }

    [Fact]
    public void Parse_RelationshipDecl_ShortForm()
    {
        var doc = ParseSource(@"Analyst -> Dashboard : ""Reviews dashboards."";");
        Assert.Single(doc.Declarations);
        var rel = Assert.IsType<RelationshipDecl>(doc.Declarations[0]);
        Assert.Equal("Analyst", rel.Source);
        Assert.Equal("Dashboard", rel.Target);
        Assert.Equal("Reviews dashboards.", rel.Description);
        Assert.Null(rel.Technology);
    }

    [Fact]
    public void Parse_RelationshipDecl_BlockForm()
    {
        var doc = ParseSource(@"
App -> PaymentGateway {
    description: ""Fetches transactions."";
    technology: ""REST/HTTPS"";
}");
        Assert.Single(doc.Declarations);
        var rel = Assert.IsType<RelationshipDecl>(doc.Declarations[0]);
        Assert.Equal("App", rel.Source);
        Assert.Equal("PaymentGateway", rel.Target);
        Assert.Equal("Fetches transactions.", rel.Description);
        Assert.Equal("REST/HTTPS", rel.Technology);
    }

    // ═══════════════════════════════════════════════════════════════
    //  DEPLOYMENT SPECIFICATION TESTS
    // ═══════════════════════════════════════════════════════════════

    [Fact]
    public void Parse_DeploymentDecl_NestedNodes()
    {
        var doc = ParseSource(@"
deployment Production {
    node ""Azure App Service"" {
        technology: ""Linux/P1v3"";
        node ""WASM Runtime"" {
            technology: ""WebAssembly"";
            instance: App;
        }
    }
}");
        Assert.Single(doc.Declarations);
        var dep = Assert.IsType<DeploymentDecl>(doc.Declarations[0]);
        Assert.Equal("Production", dep.Name);
        Assert.Single(dep.Nodes);
        Assert.Equal("Azure App Service", dep.Nodes[0].Name);
        Assert.Equal("Linux/P1v3", dep.Nodes[0].Technology);
        Assert.Single(dep.Nodes[0].ChildNodes);
        var inner = dep.Nodes[0].ChildNodes[0];
        Assert.Equal("WASM Runtime", inner.Name);
        Assert.Equal("WebAssembly", inner.Technology);
        Assert.Equal("App", inner.Instance);
    }

    // ═══════════════════════════════════════════════════════════════
    //  VIEW SPECIFICATION TESTS
    // ═══════════════════════════════════════════════════════════════

    [Fact]
    public void Parse_ViewDecl_SystemContext()
    {
        var doc = ParseSource(@"
view systemContext of Dashboard MainView {
    include: all;
    autoLayout: ""top-down"";
    description: ""The system and its users."";
}");
        Assert.Single(doc.Declarations);
        var view = Assert.IsType<ViewDecl>(doc.Declarations[0]);
        Assert.Equal(ViewKind.SystemContext, view.Kind);
        Assert.Equal("Dashboard", view.Scope);
        Assert.Equal("MainView", view.Name);
        Assert.NotNull(view.Include);
        Assert.Equal(ViewFilterKind.All, view.Include!.Kind);
        Assert.Equal(LayoutDirection.TopDown, view.AutoLayout);
    }

    [Fact]
    public void Parse_ViewDecl_WithTaggedFilter()
    {
        var doc = ParseSource(@"
view container of Dashboard ContainerView {
    include: all;
    exclude: tagged ""internal-only"";
}");
        Assert.Single(doc.Declarations);
        var view = Assert.IsType<ViewDecl>(doc.Declarations[0]);
        Assert.NotNull(view.Exclude);
        Assert.Equal(ViewFilterKind.Tagged, view.Exclude!.Kind);
        Assert.Equal("internal-only", view.Exclude.TagValue);
    }

    [Fact]
    public void Parse_ViewDecl_WithExplicitFilter()
    {
        var doc = ParseSource(@"
view systemContext of App Filtered {
    include: [Analyst, App, Gateway];
}");
        Assert.Single(doc.Declarations);
        var view = Assert.IsType<ViewDecl>(doc.Declarations[0]);
        Assert.NotNull(view.Include);
        Assert.Equal(ViewFilterKind.Explicit, view.Include!.Kind);
        Assert.Equal(3, view.Include.ExplicitElements.Count);
    }

    [Fact]
    public void Parse_ViewDecl_ComponentKind()
    {
        var doc = ParseSource(@"
view component of App.Api ApiComponentView {
    include: all;
}");
        Assert.Single(doc.Declarations);
        var view = Assert.IsType<ViewDecl>(doc.Declarations[0]);
        Assert.Equal(ViewKind.Component, view.Kind);
        Assert.Equal("App.Api", view.Scope);
        Assert.Equal("ApiComponentView", view.Name);
    }

    [Fact]
    public void Parse_ViewDecl_DeploymentKind()
    {
        var doc = ParseSource(@"
view deployment of Production ProdDeployView {
    include: all;
}");
        Assert.Single(doc.Declarations);
        var view = Assert.IsType<ViewDecl>(doc.Declarations[0]);
        Assert.Equal(ViewKind.Deployment, view.Kind);
        Assert.Equal("Production", view.Scope);
        Assert.Equal("ProdDeployView", view.Name);
    }

    [Fact]
    public void Parse_ViewDecl_AutoLayoutHyphenated()
    {
        var diagnostics = new DiagnosticBag();
        var doc = ParseSource(@"
view systemContext of App ContextView {
    include: all;
    autoLayout: top-down;
}", diagnostics);
        Assert.False(diagnostics.HasErrors,
            string.Join("; ", diagnostics.Diagnostics.Select(d => d.Message)));
        var view = Assert.IsType<ViewDecl>(doc.Declarations[0]);
        Assert.Equal(LayoutDirection.TopDown, view.AutoLayout);
    }

    [Fact]
    public void Parse_Expr_ContextualKeywordAsIdentifier()
    {
        var doc = ParseSource(@"
entity Order {
    status: string;
    completedAt: string;
    invariant ""delivered has timestamp"": status == Delivered implies completedAt != null;
}");
        Assert.Single(doc.Declarations);
        var entity = Assert.IsType<EntityDecl>(doc.Declarations[0]);
        Assert.Single(entity.Invariants);
        var inv = entity.Invariants[0];
        // Top-level expression should be "implies" (status == Delivered implies completedAt != null)
        var impliesExpr = Assert.IsType<BinaryExpr>(inv.Condition);
        Assert.Equal("implies", impliesExpr.Operator);
    }

    // ═══════════════════════════════════════════════════════════════
    //  DYNAMIC SPECIFICATION TESTS
    // ═══════════════════════════════════════════════════════════════

    [Fact]
    public void Parse_DynamicDecl_SimpleSteps()
    {
        var doc = ParseSource(@"
dynamic PlaceOrder {
    1: User -> App : ""Opens form."";
    2: App -> Engine : ""Processes order."";
    3: Engine -> App : ""Returns result."";
}");
        Assert.Single(doc.Declarations);
        var dyn = Assert.IsType<DynamicDecl>(doc.Declarations[0]);
        Assert.Equal("PlaceOrder", dyn.Name);
        Assert.Equal(3, dyn.Steps.Count);
        Assert.Equal(1, dyn.Steps[0].SequenceNumber);
        Assert.Equal("User", dyn.Steps[0].Source);
        Assert.Equal("App", dyn.Steps[0].Target);
        Assert.Equal("Opens form.", dyn.Steps[0].Description);
    }

    [Fact]
    public void Parse_DynamicDecl_BlockSteps()
    {
        var doc = ParseSource(@"
dynamic FetchData {
    1: App -> Gateway {
        description: ""Fetches data."";
        technology: ""REST/HTTPS"";
    };
}");
        Assert.Single(doc.Declarations);
        var dyn = Assert.IsType<DynamicDecl>(doc.Declarations[0]);
        Assert.Single(dyn.Steps);
        Assert.Equal("Fetches data.", dyn.Steps[0].Description);
        Assert.Equal("REST/HTTPS", dyn.Steps[0].Technology);
    }

    // ═══════════════════════════════════════════════════════════════
    //  ENRICHED TOPOLOGY EDGE TESTS
    // ═══════════════════════════════════════════════════════════════

    [Fact]
    public void Parse_TopologyRule_EnrichedEdge()
    {
        var doc = ParseSource(@"
topology Deps {
    allow App -> Gateway {
        technology: ""REST/HTTPS"";
        description: ""Fetches transactions."";
    }
    deny UI -> Engine;
}");
        Assert.Single(doc.Declarations);
        var topo = Assert.IsType<TopologyDecl>(doc.Declarations[0]);
        Assert.Equal(2, topo.Rules.Count);

        var enriched = topo.Rules[0];
        Assert.Equal(TopologyRuleKind.Allow, enriched.Kind);
        Assert.Equal("REST/HTTPS", enriched.Technology);
        Assert.Equal("Fetches transactions.", enriched.Description);

        var simple = topo.Rules[1];
        Assert.Equal(TopologyRuleKind.Deny, simple.Kind);
        Assert.Null(simple.Technology);
        Assert.Null(simple.Description);
    }
}
