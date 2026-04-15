# SpecLang: A Specification Language for Human-to-LLM Communication

## Context

The current practice of human-to-LLM communication is document-and-chat: architecture in one document, constraints in another, execution rules in a prompt, boundary conditions in naming conventions, and the real refinement path only in the author's head. The LLM can often navigate this successfully, but that does not mean the medium is sound. It means the author is compensating for the medium's weakness.

The "Enquiry Into Specification as Meaningful Struggle" identifies what is missing: a native specification medium that forces contact with boundaries, invariants, and contracts, rather than letting the medium itself be the struggle. The Enquiry's division of labor is precise: the human authors the commitments; the machine realizes within them.

SpecChat is that medium. It is a markdown-embedded specification language where the `.spec.md` document is the primary engineering artifact. The human authors specifications, entities, invariants, contracts, rationale, confidence signals, and refinement links. The LLM reads those specifications and realizes systems within them. Everything else (generated code, validation logic, test harnesses, documentation) is a projection of the specification, not its purpose.

## What Structure Alone Cannot Carry

Type schemas say what shape data takes: field names, field types, which fields are optional. That is *structure*. Structure answers the question "what does this data look like?" It does not answer four other questions that matter for correct, trustworthy, and maintainable specification.

### Contracts

A contract states what a boundary commits to. It has two sides: what the boundary *requires* from its caller (preconditions) and what it *ensures* in return (postconditions).

A type schema for an API gateway might declare that a `Route` has a `path: string` and an `authRequired: boolean`. That is structure. It says nothing about the commitment: that every request receives a unique tracking identifier, that no internal error details leak to external callers, that unauthenticated requests to protected routes receive a 401 rather than silently passing through.

Contracts make boundary behavior explicit. Without them, the boundary's promises exist only in the developer's head or scattered across documentation. The LLM has no way to know what the boundary guarantees, and no evaluation can check whether the guarantee holds.

This corresponds to the Enquiry's requirement that "every boundary must encode commitments and assumptions, not merely signatures" (sec. 5). A type signature is a structural description. A contract is a behavioral commitment.

### Invariants

An invariant is a rule that must hold across the fields of a single entity or across related entities. It is a cross-field constraint: a statement about what combinations of values are valid and which are not.

A type schema for a coffee order might declare `drink: "espresso" | "latte" | "cappuccino"` and `size: "small" | "medium" | "large"`. Both fields type-check independently. But espresso is always served in a small cup. An order for a large espresso is structurally valid and semantically wrong.

Invariants catch this class of error. They force the specifier to think about which combinations of values are meaningful and which are contradictory, then state those rules explicitly so they can be enforced.

The Enquiry identifies invariants as one of the things the specification medium "must carry" (sec. 5) and lists them as part of what makes specification genuinely hard: "What must always hold, what must never happen, what may vary, and what must be checked." Without invariants, a schema describes the space of all structurally valid data. With invariants, it describes the space of all *meaningful* data. The difference is where bugs hide.

### Rationale

Rationale is the recorded reasoning behind a specification choice. It answers "why is this field here?" and "what ambiguity does this type resolve?"

Consider a `specialInstructions: string?` field on a coffee order item. Why is it optional? Why is it a free-form string rather than an enum? What real-world inputs does it capture that the structured fields cannot? Without rationale, the next developer who reads the specification, or the next LLM that consumes it, must guess. And guessing is where drift begins.

Rationale is not documentation in the general sense. It is the specific record of *why a specification choice was made*, preserved alongside the choice itself. It serves two purposes. First, it prevents the knowledge behind the specification from decaying as people rotate off the project (Eq. 12 models this as tacit knowledge stock with exponential decay). Second, it resists what the Enquiry calls "cosmetic relocation": the risk that a specification looks rigorous but the reasoning behind it was never done or has been lost. A specification with rationale is one whose author was forced to articulate their thinking. A specification without rationale might be a copy of a template.

#### Two-Tier Rationale (ADR-Informed)

SpecChat's rationale borrows from the Architecture Decision Record (ADR) tradition, but at specification granularity rather than system granularity. ADRs capture context, decision, status, and consequences for architectural choices. SpecChat adapts this pattern into two tiers, scaled to field-level and entity-level specification choices.

**Tier 1: Simple rationale.** A prose string for straightforward fields where the reasoning is brief. Low ceremony. Most fields use this tier.

```spec
entity LineItem {
    drink: CoffeeDrink @confidence(high);
    rationale "The primary item being ordered.";
}
```

**Tier 2: Structured rationale.** A block carrying context, decision, consequence, and optionally supersedes, for fields or entities where the reasoning is non-obvious and the consequences matter. This is a micro-ADR embedded in the specification.

```spec
entity LineItem {
    specialInstructions: string? @confidence(low);

    rationale "specialInstructions" {
        context "Users frequently say things like 'extra hot' or 'light foam'
                 that do not map to any structured field.";
        decision "Free-form string, optional, low confidence.";
        consequence "Consuming applications must not act on this field
                     without user confirmation. The LLM will attempt
                     extraction but accuracy is unreliable.";
        supersedes "Previous design used an enum of common modifiers.
                    Abandoned because the modifier set is open-ended.";
    }
}
```

The `supersedes` clause is optional. When present, it records what previous design was replaced and why, preserving the decision history that ADRs are known for. Unlike system-level ADRs that live in separate numbered files, these micro-ADRs live inline with the artifact they explain. The reasoning cannot drift from the specification because it is part of the specification.

The specification quality analyzer uses the tier distinction as a signal. Entities or fields with high complexity (multiple invariants, low confidence, cross-field dependencies) but only Tier 1 rationale trigger a diagnostic: "consider structured rationale." Not an error; a nudge. The specifier decides whether the complexity warrants structured reasoning.

This two-tier design addresses the concern about knowledge transmission (Eq. 12). Simple rationale prevents the most common failure: no reasoning recorded at all. Structured rationale prevents the subtler failure: reasoning that was done but recorded too loosely to survive personnel turnover or revisitation six months later.

### Confidence Signals

A confidence signal declares how reliable the LLM's extraction of a particular field is expected to be. It is an honest statement about uncertainty.

When a user says "I'll have a large latte," the drink field is high confidence: the user named it explicitly. When a user says "the usual," every field is low confidence: the LLM is guessing. Structural validation treats both cases identically. If the data type-checks, it passes. The consuming application has no way to know which fields were extracted from clear evidence and which were inferred, defaulted, or fabricated.

Confidence signals address this by attaching an expected reliability level to each field at specification time (`@confidence(high)`, `@confidence(medium)`, `@confidence(low)`) and computing an actual reliability score at runtime based on extraction signals (explicitly stated, inferred from context, default applied, LLM hedged, ambiguous reference, not mentioned).

This maps directly to the epistemic gap (Eq. 10: Delta = Mp / (Ms * Fi)). The gap between how correct the output *looks* (presentation fidelity, Mp) and how correct it *is* (substance fidelity, Ms * Fi) is the central risk of LLM-mediated systems. Confidence signals make that gap visible rather than hiding it behind a boolean validation gate. The consuming application can then decide: accept high-confidence fields, prompt the user to confirm low-confidence ones, reject the extraction entirely if aggregate confidence is too low.

## Five Registers of Specification

SpecChat operates in five registers that cover the full architectural scope from stakeholder to server.

**Data specification** answers: "What does this data look like and what constraints hold on its fields?" Entities, enums, field-level invariants, confidence signals, and contracts at request/response boundaries. This is what type systems and schema languages approximate, extended with the semantic commitments that structure alone cannot carry.

**Context specification** answers: "Who uses this system, and what external systems does it interact with?" Persons (human actors), external systems (services outside our boundary), and relationships (labeled, directional edges with description and technology). This is C4 Level 1: the outermost zoom level that establishes what is being built, for whom, and what it depends on before decomposing internals. Context specification also introduces general-purpose tagging (`@tag`) that works across all registers, enabling view-based filtering of any element by classification.

**Systems specification** answers: "What are the parts of this system, how do they relate, in what order are they built, and what architectural rules hold across the whole?" Components, dependency topology (with enriched edges carrying technology and description), phased construction with validation gates, cross-cutting traceability, system-level constraints, and package policies. This is what architecture documents carry in prose, made explicit and machine-checkable.

**Deployment specification** answers: "Where does each part of the system run?" Deployment environments (Production, Staging, Development), infrastructure nodes (servers, cloud services, container orchestrators, Kubernetes pods), and component instances that link logical architecture to operational topology. This is C4's deployment view: the bridge between what you build and where it runs. Deployment nodes nest to model infrastructure hierarchy, and instance declarations reference authored components from the system tree, creating a traceable chain from specification to server.

**View and dynamic specification** answers: "What subset of the model should be visible in a given diagram, and how do elements collaborate at runtime?" View declarations implement the Structurizr principle of model-vs-views separation: the model is defined once; views select subsets for visualization at different zoom levels (system landscape, system context, container, component, deployment). Dynamic declarations capture runtime interaction sequences for specific use cases, filling the gap between static topology (who may depend on whom) and runtime flow (how a request moves through the system for a specific scenario).

**Design specification** answers: "What does each part of the system look like, behave like, and communicate to its users?" Pages, visualizations, parameter bindings, slider declarations, layout intent, and behavioral commitments. This is what design documents carry through interleaved prose and structured content: the specification of what a page shows, how a visualization maps computation outputs to chart parameters, what interaction patterns the user experiences, and why each design choice was made.

Design specification is where the Enquiry's $\alpha S$ term is most directly at work. Choosing which visualization to use for a given data source, deciding what parameters to expose as controls, defining what insight a chart should make visible: these are genuinely hard specification decisions that build durable FORCE. A specification medium that cannot carry them forces the specifier into prose documents and chat sessions, which is the fragile medium the Enquiry warns against.

In SpecChat, design specification is expressed through `page` and `visualization` constructs that live within the system tree (as children of authored components) and through **prose intent**: natural-language design rationale that is architecturally associated with the formal declarations it accompanies. The prose is not documentation about the specification; it is part of the specification. The LLM receives both the formal declarations and their prose intent during realization.

**The build-vs-consume boundary** is a first-class specification concern. Real systems are not written from scratch. They are assembled from authored code and consumed packages. The decision to build custom (e.g., hand-rolled SVG charting) versus consume a package (e.g., bunit for testing) is an architectural commitment with consequences for maintenance, security, upgrade obligations, and vendor coupling. A specification that cannot express this boundary forces the LLM to guess which problems to solve with code and which to solve with packages. Package policies (`package_policy`) make the boundary explicit: what is pre-approved, what is prohibited, what requires justification. Every modern platform has a package ecosystem (NuGet for .NET, npm for JavaScript, Cargo for Rust, pip for Python), and the specification must account for it.

**Platform realization** bridges the specification to the target platform's workspace semantics. The `system` declaration is platform-agnostic. But every platform has a top-level composition boundary that organizes projects, governs build entry, and shapes developer workflow. On .NET, that boundary is the Solution (`.slnx` in .NET 10+). On Rust, it is the Cargo workspace. On Node/TypeScript, it is `package.json` workspaces or monorepo tooling. On Java, it is Gradle multi-project or Maven multi-module. Platform realization makes this boundary explicit in the specification rather than leaving it as an implicit convention the LLM must guess. The realization construct carries solution name, format, project membership, folder organization, startup project, and rationale. Phase gates target the platform's workspace root, not individual projects.

The distinction maps to the Enquiry's "What the Medium Must Carry" (sec. 5). The Enquiry lists typed identity for "subsystem, actor, interface, state region" alongside "invariant, requirement, refinement node, verification case." Data specification covers the latter group. Systems specification covers the former. Both live in the same `.spec.md` document because real specifications mix them: a component's contract references the entities it processes, a phase gate validates invariants on the data the phase produces, a trace maps domain concepts to the components that implement them.

### Evidence from practice

The systems specification constructs in SpecChat were derived from real project experience. A Blazor WebAssembly visualization harness, built before SpecChat existed, carried all five categories in prose design documents:

- **Component declarations** with responsibilities and boundaries
- **Dependency prohibitions** that prevent architectural erosion (e.g., a generic UI library must not reference domain logic)
- **Phased construction** with cumulative validation gates (test count thresholds at each phase)
- **Traceability matrices** mapping domain concepts to charts to pages to test classes (many-to-many)
- **System-level constraints** that apply across all components (e.g., "no third-party charting libraries," "CSS discipline: two locations only")

All five categories were load-bearing: they prevented real architectural drift during implementation. None of them can be expressed in SpecChat's data-specification constructs. A specification language scoped only to entities and fields would force the specifier to carry the architectural rules in their head or in unstructured prose, which is the medium-management struggle the Enquiry warns against (sec. 5).

## DSL Syntax

Specification blocks live inside ` ```spec ` fenced code blocks within markdown. The surrounding markdown carries prose rationale and design discussion. A `.spec.md` file is simultaneously a readable document and a formal specification.

### Data specification constructs

```spec
entity CoffeeOrder {
    items: LineItem[];
    invariant "at least one item": count(items) > 0;
}

entity LineItem {
    drink: CoffeeDrink @confidence(high);
    size: CoffeeSize @default(medium) @confidence(medium);
    temperature: Temperature @default(hot);
    quantity: int @range(1..10) @default(1);
    specialInstructions: string? @confidence(low);

    invariant "espresso is always small":
        drink == CoffeeDrink.espresso implies size == CoffeeSize.small;

    rationale "drink is the primary item; size, temperature, and quantity
               are modifiers with sensible defaults.";

    rationale "specialInstructions" {
        context "Users frequently say things like 'extra hot' or 'light foam'
                 that do not map to any structured field.";
        decision "Free-form string, optional, low confidence.";
        consequence "Consuming applications must not act on this field
                     without user confirmation.";
        supersedes "Previous design used an enum of common modifiers.
                    Abandoned because the modifier set is open-ended.";
    }
}

enum CoffeeDrink {
    latte: "espresso with steamed milk",
    cappuccino: "espresso with foamed milk, traditionally hot",
    americano: "espresso with hot water",
    espresso: "straight shot, no milk, always small"
}

contract IngressBoundary {
    requires request.size <= 10485760;
    ensures response.headers contains "X-Request-Id";
    guarantees "No internal error details leak to external callers";
}

refines CoffeeOrder as DetailedCoffeeOrder {
    loyaltyId: string?;
    invariant "loyalty orders have at most 10 items": count(items) <= 10;
}
```

### Context specification constructs

Context specification establishes the outermost boundary: who uses the system, what external systems it interacts with, and how elements relate to each other. These constructs answer C4 Level 1 questions before any internal decomposition begins.

```spec
// --- Persons: human users and actors ---

person Analyst {
    description: "Business analyst reviewing revenue and
                  retention metrics across segments.";
    @tag("stakeholder", "primary-user");
}

person Executive {
    description: "C-suite executive using the Monday
                  morning dashboard for high-level metrics.";
    @tag("stakeholder", "read-only");
}

// --- External systems: services outside our boundary ---
//
// External systems are not consumed packages (those are
// build-time dependencies declared with `consumed component`).
// External systems are runtime peers: services the system
// calls, receives data from, or integrates with at runtime.

external system PaymentGateway {
    description: "Stripe payment processing API. Source of
                  transaction data for revenue calculations.";
    technology: "REST/HTTPS";
    @tag("external", "financial");

    rationale "Revenue calculations require granular transaction
               data. Stripe's reporting API provides segment-level
               detail that our internal systems do not store.";
}

external system DataWarehouse {
    description: "Snowflake data warehouse. Source of historical
                  retention and churn metrics.";
    technology: "Snowflake SQL/HTTPS";
    @tag("external", "data-source");
}

// --- Relationships: labeled edges between elements ---
//
// Relationships connect persons, external systems, and
// internal systems. Each carries a description (what is
// communicated) and optionally a technology (how it is
// communicated). Relationships are directional.

Analyst -> AnalyticsDashboard : "Reviews revenue and retention dashboards.";

Executive -> AnalyticsDashboard : "Views Monday morning executive summary.";

AnalyticsDashboard -> PaymentGateway {
    description: "Fetches transaction history for revenue calculations.";
    technology: "REST/HTTPS";
}

AnalyticsDashboard -> DataWarehouse {
    description: "Queries historical retention and churn data.";
    technology: "Snowflake SQL/HTTPS";
}
```

### Systems specification constructs

The specification is a hierarchical decomposition. `system` is the root node. It decomposes into child nodes until the system is adequately described. Every node in the tree has a disposition: `authored` (we write this) or `consumed` (we use this; someone else wrote it). This distinction is structural, not incidental. An authored node carries responsibility, internal structure, phases, and gates. A consumed node carries contracts (what we expect from it), version constraints, and rationale (why we chose it). The specifier does not describe the internals of consumed nodes; they describe the boundary.

This mirrors how real systems are built. Aspire's `AddPostgres("db")` does not mean "write a database." It means "this system includes a Postgres database; here is what we expect from it." SpecChat generalizes this: every node in the decomposition tree is either something you build or something you use, and the specification must say which.

```spec
// --- System: the root of the decomposition tree ---

system AnalyticsDashboard {
    target: "net10.0";
    responsibility: "Interactive web dashboard for business analytics
                     metrics. All computation runs client-side
                     in the browser.";

    // --- Authored components: things we build ---

    authored component Analytics.Engine {
        kind: library;
        path: "src/Analytics.Engine";
        status: existing;
        responsibility: "Pure computation. Metric calculations across
                         multiple analysis domains. No UI, no IO,
                         no external dependencies.";

        rationale "Pre-existing component. The engine library and its
                   80 tests (Analytics.Engine.Tests) predate this
                   specification. The contract above declares the API
                   surface that the dashboard app and its tests
                   depend on.";

        contract {
            guarantees "Revenue: MonthlyRevenue,
                        RevenueGrowthRate,
                        RevenueBySegment";
            guarantees "Retention: ChurnRate, CohortRetention,
                        LifetimeValue";
            guarantees "Forecasting: LinearForecast,
                        SeasonalAdjustment";
            // ... remaining domains omitted for brevity
        }
    }

    authored component Dashboard.UI {
        kind: library;
        path: "src/Dashboard.UI";
        responsibility: "Reusable Blazor chart and control components.
                         Generic. No domain coupling.";

        rationale {
            context "Charts are reused across multiple pages with
                     different metric data.";
            decision "Separate Razor class library with no knowledge
                      of the domain.";
            consequence "The app wires domain data to components.
                         The library is reusable in other projects.";
        }
    }

    authored component Analytics.App {
        kind: application;
        path: "src/Analytics.App";
        responsibility: "Blazor WASM host. Bridges engine
                         to visualizations.";
    }

    authored component Analytics.App.Tests {
        kind: tests;
        path: "tests/Analytics.App.Tests";
        responsibility: "BUnit tests for all chart components,
                         controls, and pages.";
    }

    // --- Consumed components: things we use ---
    //
    // Consumed nodes describe what we expect from external
    // packages or services, not their internals. The specifier
    // authors the boundary contract and the rationale for
    // choosing this dependency. The LLM must respect these
    // choices: it may not substitute alternatives without
    // human authorization.

    consumed component BlazorWasm {
        source: nuget("Microsoft.AspNetCore.Components.WebAssembly");
        version: "10.*";
        responsibility: "Blazor WebAssembly hosting runtime.
                         Provides client-side .NET execution in browser.";
        used_by: [Analytics.App];

        contract {
            guarantees "Client-side .NET execution via WebAssembly";
            guarantees "Component rendering lifecycle";
            guarantees "JS interop bridge";
        }

        rationale "Framework choice for client-side interactive web.
                   No server required. All computation runs in browser.";
    }

    consumed component AspNetComponents {
        source: nuget("Microsoft.AspNetCore.Components.Web");
        version: "10.*";
        responsibility: "Blazor component model, rendering, and DOM interop.";
        used_by: [Dashboard.UI];
    }

    consumed component JSInterop {
        source: nuget("Microsoft.JSInterop");
        version: "10.*";
        responsibility: "Bridge between .NET and JavaScript for Canvas rendering.";
        used_by: [Dashboard.UI];
    }

    consumed component BUnit {
        source: nuget("bunit");
        version: "2.*";
        responsibility: "In-process Blazor component rendering for tests.
                         No browser required.";
        used_by: [Analytics.App.Tests];

        rationale {
            context "Blazor components cannot be tested with standard
                     xunit assertions alone. They require a rendering host.";
            decision "BUnit renders components in-process, asserts on
                      markup and interaction, no browser dependency.";
            consequence "Tests run fast, in CI, without Selenium or Playwright.";
        }
    }

    consumed component XUnit {
        source: nuget("xunit");
        version: "2.*";
        responsibility: "Test framework.";
        used_by: [Analytics.App.Tests];
    }

    consumed component TestSdk {
        source: nuget("Microsoft.NET.Test.Sdk");
        version: "17.*";
        responsibility: "Test execution infrastructure.";
        used_by: [Analytics.App.Tests];
    }
}

// --- Platform realization: how the system maps to the target platform ---
//
// The system declaration is platform-agnostic. Platform
// realization declares how the system's components are
// organized according to the target platform's workspace
// semantics.
//
// On .NET, the primary workspace boundary is the Solution.
// It is not a formatting detail or an incidental projection
// output. It is the platform's top-level composition container:
// the boundary that organizes projects, solution folders,
// build entry, test entry, and developer workflow. In .NET 10+,
// the default format is .slnx (XML-based); .sln remains
// available for legacy compatibility.
//
// Other platforms have analogous workspace boundaries:
//   - Rust: Cargo workspace (Cargo.toml with [workspace])
//   - Node/TypeScript: package.json workspaces or monorepo tools
//   - Java: Gradle multi-project or Maven multi-module
//   - Go: go.work workspace
//
// The platform realization construct makes these explicit so
// the LLM generates the correct workspace structure and phase
// gates target the correct entry point.

dotnet solution AnalyticsDashboard {
    format: slnx;
    startup: Analytics.App;

    folder "src" {
        projects: [Analytics.Engine, Dashboard.UI, Analytics.App];
    }

    folder "tests" {
        projects: [Analytics.App.Tests];
    }

    rationale {
        context "The .NET SDK defaults to .slnx in .NET 10.";
        decision "Use .slnx format. Organize by src/tests folders.
                  Startup project is the Blazor WASM app.";
        consequence "All phase gate commands target the solution root:
                     dotnet build AnalyticsDashboard.slnx
                     dotnet test AnalyticsDashboard.slnx";
    }
}

// --- Package policy: what may and must not be consumed ---
//
// Package policies govern the boundary between authored and
// consumed. They answer: "When the LLM generates code or
// project files, which external packages is it permitted to
// introduce, which are prohibited, and which require explicit
// justification?"

package_policy DashboardPolicy {
    source: nuget("https://api.nuget.org/v3/index.json");

    deny category("charting")
        includes ["Plotly.Blazor", "ChartJs.Blazor", "Radzen.Blazor",
                  "ApexCharts", "Syncfusion.*"];

    deny category("css-framework")
        includes ["Bootstrap", "Tailwind.*", "MudBlazor"];

    allow category("platform")
        includes ["Microsoft.AspNetCore.*", "Microsoft.Extensions.*",
                  "Microsoft.JSInterop", "System.*"];

    allow category("testing")
        includes ["xunit", "xunit.*", "bunit", "bunit.*",
                  "Microsoft.NET.Test.Sdk", "Moq", "NSubstitute"];

    // Packages not in any allow category require explicit
    // justification via a consumed component declaration
    // with rationale.
    default: require_rationale;

    rationale {
        context "Third-party UI libraries impose their own abstractions,
                 limit control, and create upgrade/security maintenance
                 obligations. Testing libraries are low-risk and high-value.";
        decision "Platform and testing packages are pre-approved.
                  Charting and CSS framework packages are prohibited.
                  Everything else requires a consumed component declaration
                  with rationale.";
        consequence "LLM realization must solve charting and styling with
                     authored code. It may freely use platform APIs and
                     testing infrastructure.";
    }
}

// --- Topology: who may depend on whom ---
//
// Topology rules apply to authored components (internal
// dependency structure) and to authored-consumed edges
// (which authored components may use which consumed components).
// The consumed components' used_by fields must be consistent
// with topology allow rules.

topology ProjectDependencies {
    allow Analytics.App -> Dashboard.UI;
    allow Analytics.App -> Analytics.Engine;
    allow Analytics.App -> PaymentGateway {
        technology: "REST/HTTPS";
        description: "Fetches transaction history for revenue calculations.";
    };
    allow Analytics.App -> DataWarehouse {
        technology: "Snowflake SQL/HTTPS";
        description: "Queries historical retention and churn data.";
    };
    allow Analytics.App.Tests -> Dashboard.UI;
    allow Analytics.App.Tests -> Analytics.Engine;
    allow Analytics.App.Tests -> Analytics.App;
    deny  Dashboard.UI -> Analytics.Engine;

    invariant "nullable everywhere":
        all authored components satisfy nullable == enabled;

    rationale "deny Dashboard.UI -> Analytics.Engine" {
        context "Dashboard.UI is a generic component library.
                 Analytics.Engine is domain logic.";
        decision "The library must not know about the domain.
                 Only the app bridges them.";
        consequence "Components are reusable in other projects.
                     Domain changes never break the UI library.";
    }
}

// --- Phases: ordered construction with validation gates ---
//
// Phases apply to authored components. Consumed components
// are prerequisites that must be available (installed,
// restorable) before the phase that first references them.

phase Scaffolding {
    produces: [Dashboard.UI, Analytics.App,
               Analytics.App.Tests];

    gate build {
        command: "dotnet build AnalyticsDashboard.slnx
                  -p:TreatWarningsAsErrors=true";
        expects: errors == 0, warnings == 0;
    }

    gate engine_tests {
        command: "dotnet test AnalyticsDashboard.slnx
                  --filter FullyQualifiedName~Analytics.Engine.Tests";
        expects: pass >= 80, fail == 0;
    }

    gate app_launches {
        command: "dotnet run --project src/Analytics.App/";
        expects: "Loads in browser, nav links work,
                  all pages show placeholder heading";
    }

    rationale {
        context "Building charts before the component library exists
                 causes coupling mistakes discovered too late.";
        decision "Scaffold all projects, prove they compile and link,
                  before any component work begins.";
    }
}

phase CoreComponents {
    requires: Scaffolding;
    // Chart components are authored within Dashboard.UI.
    // Each would have a component declaration in the full spec.
    produces: [LineChart, BarChart, ParameterSlider,
               ChartCard, SplitPanel, ThresholdIndicator];

    gate tests {
        command: "dotnet test AnalyticsDashboard.slnx";
        expects: pass >= 120, fail == 0;
    }
}

phase AdvancedVisualizations {
    requires: CoreComponents;
    produces: [HeatMap, TornadoDiagram, WaterfallChart,
               FunnelChart];

    gate tests {
        command: "dotnet test AnalyticsDashboard.slnx";
        expects: pass >= 160, fail == 0;
    }
}

phase CompositeDashboards {
    requires: AdvancedVisualizations;

    gate tests {
        command: "dotnet test AnalyticsDashboard.slnx";
        expects: pass >= 210, fail == 0;
    }
}

// --- Traces: many-to-many cross-reference ---
//
// Pages referenced here (RevenuePage, RetentionPage, etc.)
// would each have a page declaration in the full spec.
// Only ExecutiveDashboard is shown in the design
// specification example below.

trace MetricsToPages {
    Revenue    -> [RevenuePage, ExecutiveDashboard, TrendDashboard];
    Retention  -> [RetentionPage, CohortPage, ExecutiveDashboard];
    Churn      -> [RetentionPage, ExecutiveDashboard,
                   TrendDashboard];
    Forecast   -> [ForecastPage, ExecutiveDashboard,
                   TrendDashboard];

    invariant "every metric has at least one page":
        all sources have count(targets) >= 1;

    invariant "executive dashboard covers core metrics":
        ExecutiveDashboard.sources contains [Revenue, Retention, Churn];
}

trace ComponentsToChartTypes {
    LineChart       -> [RevenuePage, RetentionPage, ForecastPage,
                        TrendDashboard];
    BarChart        -> [RevenuePage, CohortPage, ExecutiveDashboard];
    HeatMap         -> [CohortPage, RetentionPage];
    FunnelChart     -> [RevenuePage];
    WaterfallChart  -> [RevenuePage, ExecutiveDashboard];
}

// --- System-level constraints ---

constraint CssDiscipline {
    scope: all authored components;
    rule: "App CSS only in Site.css.
           Component CSS only in .razor.css files within Dashboard.UI.";

    rationale {
        context "CSS sprawl across arbitrary locations caused
                 maintenance problems in previous projects.";
        decision "Two locations only. App controls placement.
                  Library controls appearance.";
        consequence "Any CSS outside these two patterns is a violation.";
    }
}

constraint TestNaming {
    scope: all test methods;
    rule: "MethodName_Scenario_ExpectedResult";

    rationale "Consistent naming makes test failures immediately
               interpretable without reading the test body.";
}
```

### Deployment specification constructs

Deployment declarations map logical components onto infrastructure. Each deployment block represents a named environment. Nodes nest to model infrastructure hierarchy: a cloud region contains a cluster, a cluster contains nodes, nodes contain component instances. Instance declarations reference authored components from the system tree, creating a traceable link from logical architecture to operational topology.

```spec
// --- Deployment: where each part of the system runs ---

deployment Production {
    node "Azure Static Web Apps" {
        technology: "Azure SWA/West US 2";

        node "Blazor WASM Host" {
            technology: "WebAssembly";
            instance: Analytics.App;
        }
    }

    node "Azure Functions" {
        technology: "Consumption Plan/West US 2";
        instance: Analytics.Engine;
        @tag("serverless");
    }

    rationale {
        context "The dashboard is a client-side Blazor WASM app.
                 All computation runs in the browser. The static
                 host serves the compiled WASM bundle.";
        decision "Azure Static Web Apps for the WASM host.
                  Azure Functions for any server-side
                  pre-computation if needed.";
        consequence "No always-on server cost for the dashboard.
                     Scaling is handled by the CDN and the
                     Functions consumption plan.";
    }
}

deployment Staging {
    node "Azure Static Web Apps" {
        technology: "Azure SWA/West US 2 (staging slot)";

        node "Blazor WASM Host" {
            technology: "WebAssembly";
            instance: Analytics.App;
        }
    }
}
```

### View specification constructs

View declarations define which subset of the model to render as a diagram. This implements the model-vs-views separation principle: the model (persons, systems, components, external systems, deployment nodes) is defined once; views select subsets for visualization at different zoom levels.

Views are specification-level declarations, not rendering instructions. The projection layer decides how to render them (Mermaid, DOT, Structurizr DSL, PlantUML, etc.).

```spec
// --- Views: what to show at each zoom level ---

view systemContext of AnalyticsDashboard SystemContextView {
    include: all;
    autoLayout: top-down;
    description: "The Analytics Dashboard and its external
                  dependencies, as seen by its users.";
}

view container of AnalyticsDashboard ContainerView {
    include: all;
    exclude: tagged "internal-only";
    autoLayout: left-right;
    description: "Internal structure of the Analytics Dashboard
                  showing authored and consumed components.";
}

view deployment of Production ProductionDeploymentView {
    include: all;
    autoLayout: top-down;
    description: "Production infrastructure showing where
                  each component runs.";
}
```

### Dynamic specification constructs

Dynamic declarations capture runtime behavior by showing how elements collaborate for a specific use case or scenario. Each step is a numbered interaction. Dynamic declarations fill the gap between static topology (who may depend on whom) and runtime flow (how a specific request moves through the system).

```spec
// --- Dynamic: behavioral interaction sequences ---

dynamic ExecutiveDashboardLoad {
    1: Executive -> Analytics.App
        : "Opens /executive route in browser.";
    2: Analytics.App -> PaymentGateway {
        description: "Fetches revenue transactions for default period.";
        technology: "REST/HTTPS";
    };
    3: Analytics.App -> DataWarehouse {
        description: "Queries retention and churn metrics.";
        technology: "Snowflake SQL/HTTPS";
    };
    4: Analytics.App -> Analytics.Engine
        : "Computes revenue waterfall, cohort retention, churn rate.";
    5: Analytics.Engine -> Analytics.App
        : "Returns computed metric results.";
    6: Analytics.App -> Executive
        : "Renders executive dashboard with all visualizations.";
}
```

### Design specification constructs

Page and visualization declarations are semantically children of authored components (they belong to the host application). Syntactically, they appear in their own spec blocks in the `.spec.md` file, referencing their parent via `host:` (pages) or `page:` (visualizations). This preserves the natural interleaving of formal structure and prose intent.

A markdown heading followed by a spec block whose first keyword is `page` or `visualization` opens a **prose context**. All subsequent markdown prose and child spec blocks, until the next heading at the same or higher level, are collected as children of that context. The prose between spec blocks is the **design intent**: the human-authored description of what the visualization should communicate, what interaction patterns the user should experience, and why design choices were made. The LLM receives both the formal declarations and their prose intent during realization.

The following example shows the ExecutiveDashboard page with two of its visualizations, interleaved with prose intent. In a `.spec.md` file, this would span multiple heading sections:

```spec
page ExecutiveDashboard {
    host: Analytics.App;
    route: "/executive";
    concepts: [Revenue, Retention, Churn, Forecast];
    role: "High-level overview. The single page a CEO
           opens on Monday morning.";
    cross_links: [RevenuePage, RetentionPage,
                  ForecastPage, CohortPage];
}
```

Prose intent (in surrounding markdown, not inside the spec block):

> The primary visualization is a revenue waterfall showing starting
> revenue, each growth and contraction factor, and ending revenue
> for the selected period. Green bars for growth segments, red bars
> for contraction. As the user adjusts the time range slider, the
> waterfall recomputes and the relative weight of each factor shifts.
> Key insight: the user sees not just the total change but which
> factors drove it.

```spec
visualization RevenueWaterfall {
    page: ExecutiveDashboard;
    component: WaterfallChart;

    parameters {
        segments: Revenue.RevenueBySegment(startDate, endDate,
                  segmentType);
        baseline: Revenue.MonthlyRevenue(startDate);
    }

    sliders: [startDate, endDate, segmentType];
}
```

Prose intent for the next visualization:

> Shows retention curves for each customer cohort over time.
> Each line is a cohort month. Steeper drop-off indicates
> a retention problem in that cohort. Hover reveals the
> specific retention percentage at each time point.

```spec
visualization CohortRetentionCurves {
    page: ExecutiveDashboard;
    component: LineChart;

    parameters {
        series: Retention.CohortRetention(cohortStart,
                cohortEnd, interval);
    }

    sliders: [cohortStart, cohortEnd, interval];
}
```

### Keywords mapped to theoretical concepts

#### Data specification keywords

| Keyword | Purpose | Theoretical Connection |
|---------|---------|-----------------|
| `entity` | Typed structure with fields | Typed identity (Enquiry sec. 5) |
| `enum` | Constrained values with descriptions | Prevents cosmetic relocation |
| `invariant` | Cross-field constraint | Forces contact with what is hard |
| `rationale` | Why this exists (two tiers: simple string or structured ADR block with context/decision/consequence/supersedes) | Addresses medium-management struggle; knowledge transmission (Eq. 12) |
| `contract` | Pre/postconditions for boundaries | Boundary commitments (Enquiry sec. 6) |
| `refines` | Links sub-spec to parent | Refinement traceability |
| `@confidence` | LLM extraction reliability | Epistemic gap (Eq. 10) |
| `@default` | Fallback when extraction fails | Explicit unknown handling |
| `unknown` | Type for unresolvable fields | Escape hatch |

#### Context specification keywords

| Keyword | Purpose | Theoretical Connection |
|---------|---------|-----------------|
| `person` | Human user or actor who interacts with the system | Typed identity for actors (C4 Level 1; Enquiry sec. 5 "actor") |
| `external system` | Software system outside our boundary that we interact with at runtime | Typed identity for external peers; distinguishes runtime dependencies from build-time consumed packages |
| `description` | Prose description of a person, external system, or relationship | Forces articulation of purpose at the context level |
| `technology` | Communication protocol or technology stack | Makes communication mechanisms explicit and auditable |
| `->` (relationship) | Labeled directional edge between persons, systems, and external systems | Meaningful relations at the outermost scope (Enquiry sec. 5 "depends on, constrains") |
| `@tag` | General-purpose classification for any element across all registers | Enables view-based filtering; tag-driven include/exclude for diagram projections |

#### Systems specification keywords

| Keyword | Purpose | Theoretical Connection |
|---------|---------|-----------------|
| `system` | Root of the decomposition tree | The whole; typed identity at the highest level (Enquiry sec. 5) |
| `target` | Platform and runtime target for the system | Platform realization entry point |
| `responsibility` | Prose description of what a system or component does | Forces articulation of purpose at every level of decomposition |
| `authored component` | System building block that we write | Typed identity for subsystems we build (Enquiry sec. 5) |
| `consumed component` | External package or service that we use | Build-vs-consume boundary; the system includes things we did not write |
| `source` | Package registry or service origin for consumed components | Package provenance (NuGet, npm, crates.io, pip, etc.) |
| `version` | Version constraint on a consumed component | Dependency governance |
| `used_by` | Which authored components consume this dependency | Authored-to-consumed edge in the topology |
| `path` | Relative directory where a component lives or will be created | Concrete disk location for project creation and solution membership |
| `status` | Whether a component already exists (`existing`) or must be created (`new`) | Distinguishes pre-existing projects from to-be-created projects |
| `topology` | Dependency and communication rules between components | Meaningful relations: "depends on, constrains" (Enquiry sec. 5) |
| `allow` | Permitted dependency between components | Explicit authorization (Enquiry sec. 5) |
| `deny` | Prohibited dependency between components | Architectural prohibition; prevents entropy |
| `phase` | Ordered construction stage with validation gates | Refinement with temporal ordering and proof obligations |
| `requires` (phase) | Phase ordering constraint | Sequenced refinement (Enquiry sec. 5, research item 3) |
| `gate` | Executable validation criterion within a phase | Verification obligation (Enquiry sec. 5) |
| `produces` | Artifacts a phase delivers | Refinement links (Enquiry sec. 5) |
| `trace` | Many-to-many cross-reference between concerns | Meaningful relations across the system graph |
| `constraint` | System-level rule applying across components | Invariants at architectural scope |
| `scope` | Which components a constraint applies to | Constraint targeting |
| `package_policy` | System-wide rules for package consumption | Architectural prohibition at the dependency level |
| `category` | Named group of packages for allow/deny rules | Package classification for policy enforcement |
| `default: require_rationale` | Unlisted packages need a consumed component declaration with rationale | Prevents silent dependency accumulation |
| `dotnet solution` | .NET platform realization: solution as workspace boundary | Platform semantic for project composition (not just a projection) |
| `format` | Solution file format (`slnx` or `sln`) | .NET 10+ defaults to slnx |
| `startup` | Startup project within the solution | Developer workflow entry point |
| `folder` | Solution folder organizing projects | Workspace structure |
| `projects` | Project membership within a solution folder | Composition boundary |

#### Design specification keywords

| Keyword | Purpose | Theoretical Connection |
|---------|---------|-----------------|
| `page` | Routable view within a host application | Typed identity for UI views; decomposition below the component level |
| `host` | Which authored component a page belongs to | Parent reference in the system tree |
| `route` | URL path for a page | Platform realization (Blazor routing) |
| `concepts` | Which domain concepts a page visualizes | Traceability from domain concepts to views |
| `role` | Human-readable purpose of a page | Forces articulation of design intent |
| `cross_links` | Navigation links to related pages | Meaningful relations between views |
| `visualization` | Single chart or diagram within a page | Typed identity for visual elements |
| `parameters` | Bindings from computation methods to chart inputs | The bridge between computation and rendering |
| `sliders` | Interactive controls exposed to the user | Specifies what the user can manipulate |
| Prose intent (markdown) | Natural-language design rationale between spec blocks | The human voice in the specification; carried alongside formal structure |

#### Deployment specification keywords

| Keyword | Purpose | Theoretical Connection |
|---------|---------|-----------------|
| `deployment` | Named environment (Production, Staging, etc.) | Infrastructure as specification, not as an afterthought |
| `node` | Infrastructure element (server, cloud service, pod, cluster) | Typed identity for infrastructure; nests to model hierarchy |
| `instance` | Links a logical component to the infrastructure that hosts it | Traceability from specification to server; bridges logical and operational topology |

#### View and dynamic specification keywords

| Keyword | Purpose | Theoretical Connection |
|---------|---------|-----------------|
| `view` | Declares a diagram as a subset of the model at a specific zoom level | Model-vs-views separation; one model, many projections (Structurizr principle) |
| `systemLandscape` | View kind: all persons and systems across the enterprise | C4 Level 0: the widest zoom |
| `systemContext` | View kind: one system with its direct connections | C4 Level 1: who uses it, what does it talk to |
| `container` | View kind: internal containers within a system | C4 Level 2: deployable units |
| `component` | View kind: components within a container | C4 Level 3: logical building blocks |
| `include` | Elements to show in a view (all, tagged, or explicit list) | View scoping: what is visible |
| `exclude` | Elements to hide from a view (tagged or explicit list) | View scoping: what is filtered out |
| `autoLayout` | Layout direction hint for rendering | Rendering guidance, not constraint |
| `dynamic` | Named behavioral interaction sequence for a specific scenario | Runtime flow: how elements collaborate for a use case (C4 dynamic diagram) |

## How Theoretical Concepts Inform Each Feature

### Epistemic Gap (Eq. 10: Delta = Mp / (Ms * Fi))

Structural validation cannot distinguish between high-confidence and low-confidence extractions. A coffee order where the LLM infers "latte" from "I'll have a coffee" passes validation identically to one where the user explicitly says "latte."

SpecChat's `@confidence` annotations and runtime confidence scoring address this directly. Each field carries a declared confidence level, and the protocol computes an extracted confidence based on signals (explicitly stated, inferred from context, default applied, LLM hedged, ambiguous reference). The gap between declared and extracted confidence is the epistemic gap made visible.

### SNR Collapse (Eq. 18: SNR = Var(Ftrue) / (Var(Ftrue) + Mp^2 * Var(e) + Var(n)))

As LLMs improve, their output becomes more consistently well-formed. Structural validation pass rates rise, but the signal about semantic correctness drops. Passing type-checking tells you less and less.

Invariant evaluation catches errors invisible to structural checking. An order for an "iced cappuccino" type-checks perfectly but violates the domain invariant. This is precisely the SNR collapse: presentation passes, substance fails. Invariants are the independent signal source that resists collapse.

### Medium Question (Enquiry sec. 5)

The Enquiry warns that if the specification medium is too fragile, human effort goes into medium management rather than system reasoning. The current document-and-chat practice is that fragile medium.

SpecChat forces explicit declaration of what matters: boundaries (contracts), what must always hold (invariants), what the specifier is uncertain about (confidence), and why choices were made (rationale). The struggle shifts from wrangling a broken medium to reasoning about the system.

### Cosmetic Relocation (Enquiry sec. 7)

The Enquiry warns that specification can succeed as a realization method while failing as a developmental one. If developers copy specification templates without confronting the hard questions, the specifications are cargo-cult artifacts.

SpecChat's quality analyzer detects this: naked string fields with no constraints, entities with no invariants, enum values with no semantic descriptions, missing rationale. The quality score is not pass/fail; it is a feedback signal that preserves the learning opportunity.

### FORCE Preservation (Eq. 11: dF/dt = alpha*S + gamma*E*F - beta*R - sigma*M_absorbed)

Silent repair loops remove the learning signal. The developer never sees what failed or why. SpecChat replaces this with transparent conversation: full history of what was attempted, what violations were found, what was fixed, what remains uncertain. The developer sees the diagnostic chain and can learn from it, raising the gamma*E*F (engagement) term rather than the beta*R (passive reliance) term.

### Division of Labor (Enquiry sec. 6)

The human authors the commitments (the specification). The machine realizes within them (conversation protocol, projections, generated artifacts). This distinction is structural in SpecChat: the `.spec.md` file is the authored commitment; everything generated from it is machine elaboration. The boundary is the specification model.

## Architecture

SpecChat is organized around the specification as the primary artifact. Everything else is either parsing the specification, conversing against it, or projecting it into secondary forms.

### Specification Model

The specification model is the core. It is not an intermediate representation on the way to code generation; it is the product.

```
.spec.md document
  --> [SpecChat.Language] parse into specification model
  --> [SpecChat.Core] the model: queryable, diffable, versionable
        |
        +--> [SpecChat.Protocol] human-LLM conversation constrained by the spec
        +--> [SpecChat.Projections] secondary artifacts: C#, TypeScript, Python, JSON Schema, docs
        +--> [SpecChat.Quality] specification quality analysis
```

### SpecChat.Core (the specification model)

The semantic model is a hierarchy of C# record types. It must be rich enough to support querying, diffing, versioning, and multi-view projection.

#### Data specification nodes

- `SpecDocument` -- root, contains `SpecBlock[]` and `ProseSection[]`
- `EntityDecl` -- named entity with optional base type and members
- `EnumDecl` -- named enum with `EnumMemberDecl[]` (each with optional description)
- `ContractDecl` -- named contract with requires/ensures/guarantees clauses
- `RefinementDecl` -- links parent name to child entity
- `FieldDecl` -- name, TypeExpr, Annotation[]
- `RationaleDecl` -- two variants: `SimpleRationale` (prose string) or `StructuredRationale` (named target, context, decision, consequence, optional supersedes)
- `InvariantDecl` -- description string and Expr condition
- `TypeExpr` -- PrimitiveTypeExpr, NamedTypeExpr, ArrayTypeExpr, OptionalTypeExpr, UnknownTypeExpr
- `Expr` -- BinaryExpr, UnaryExpr, MemberAccessExpr, LiteralExpr, IdentifierExpr, InExpr, CallExpr, QuantifierExpr, ListExpr
- `TextSpan` -- source location for error reporting back to the original .spec.md

#### Context specification nodes

- `PersonDecl` -- named human user or actor with `Description` (string), optional `TagAnnotation[]`, optional `RationaleDecl`
- `ExternalSystemDecl` -- named external system with `Description` (string), `Technology` (string), optional `TagAnnotation[]`, optional `RationaleDecl`
- `RelationshipDecl` -- directional edge between two named elements with `Description` (string), optional `Technology` (string), optional `TagAnnotation[]`, optional `RationaleDecl`. Short form (description only) and block form (multiple properties) map to the same AST node.
- `TagAnnotation` -- list of tag strings attached to a declaration. Not a standalone declaration; embedded within declarations that support tagging.

#### Systems specification nodes

- `SystemDecl` -- root of the decomposition tree with `Target`, `Responsibility`, child `ComponentDecl[]` (authored and consumed)
- `AuthoredComponentDecl` -- named building block that we write, with `Kind` (library, application, tests), `Path` (relative directory), `Status` (existing or new; defaults to new), `Responsibility` (prose), optional `ContractDecl` (API surface), optional `RationaleDecl`
- `ConsumedComponentDecl` -- named external dependency that we use, with `Source` (registry + package name), `Version` (constraint), `Responsibility` (prose), `UsedBy` (authored component references), optional `ContractDecl`, optional `RationaleDecl`
- `TopologyDecl` -- named dependency/communication ruleset containing `DependencyRule[]` and optional `InvariantDecl[]`, `RationaleDecl[]`. Rules apply to authored-to-authored edges and authored-to-consumed edges.
- `DependencyRule` -- `Allow` or `Deny` between two component references, with optional `Description` (string), `Technology` (string), `RationaleDecl`. The block form enriches edges with communication protocol and intent.
- `PhaseDecl` -- named construction stage with `Requires` (predecessor phase names), `Produces` (authored component/artifact references), `GateDecl[]`, optional `RationaleDecl`. Phases apply to authored components; consumed components are prerequisites that must be available.
- `GateDecl` -- named validation criterion with `Command` (executable string), `Expects` (assertion expressions), optional prose description
- `TraceDecl` -- named many-to-many mapping with `TraceLink[]` and optional `InvariantDecl[]`
- `TraceLink` -- source reference to target reference list (e.g., Revenue -> [RevenuePage, ExecutiveDashboard])
- `ConstraintDecl` -- named system-level rule with `Scope` (component list, `all authored components`, or `all`), `Rule` (prose), optional `RationaleDecl`
- `PackagePolicyDecl` -- named system-wide policy with `Source` (registry URL), `CategoryRule[]` (allow/deny with package name lists), `DefaultPolicy` (require_rationale or allow), optional `RationaleDecl`
- `CategoryRule` -- `Allow` or `Deny` applied to a named category with an `includes` list of package name patterns
- `PlatformRealizationDecl` -- abstract base for platform-specific workspace semantics
- `DotNetSolutionDecl` -- .NET platform realization with `Format` (slnx or sln), `Startup` (project reference), `SolutionFolderDecl[]` (named folders with project membership), optional `RationaleDecl`
- `SolutionFolderDecl` -- named folder within a solution containing `Projects` (component references)

#### Deployment specification nodes

- `DeploymentDecl` -- named deployment environment with child `DeploymentNodeDecl[]` and optional `RationaleDecl`
- `DeploymentNodeDecl` -- named infrastructure element with `Technology` (string), optional `Instance` (authored component reference), child `DeploymentNodeDecl[]` (recursive nesting for infrastructure hierarchy), optional `TagAnnotation[]`, optional `RationaleDecl`

#### View specification nodes

- `ViewDecl` -- named view with `ViewKind` (systemLandscape, systemContext, container, component, deployment), optional `Scope` (DOTTED_IDENT reference to the scoped element), `Include` filter, `Exclude` filter, `AutoLayout` direction, `Description` (string), optional `TagAnnotation[]`, optional `RationaleDecl`
- `ViewFilter` -- abstract base with three variants: `AllFilter`, `TaggedFilter` (single tag string), `ExplicitFilter` (ident list)

#### Dynamic specification nodes

- `DynamicDecl` -- named behavioral sequence with child `DynamicStep[]`
- `DynamicStep` -- sequence number (integer), source reference, target reference, `Description` (string), optional `Technology` (string)

#### Design specification nodes

- `PageDecl` -- named routable view with `Host` (authored component reference), `Route` (string), `Concepts` (ident list), `Role` (string), `CrossLinks` (ident list), child `VisualizationDecl[]`, child `ProseIntent[]`, optional `RationaleDecl`
- `VisualizationDecl` -- named chart/diagram with `Page` (page reference), `Component` (chart component reference), `ParameterBindingDecl[]`, `Sliders` (ident list), child `ProseIntent[]`, optional `RationaleDecl`
- `ParameterBindingDecl` -- target name and source expression (method call or function reference binding computation output to chart input)
- `ProseIntent` -- markdown text architecturally associated with a page or visualization context, with `TextSpan` for source location. Not parsed by the spec grammar; preserved as-is for the LLM to read alongside formal declarations during realization

### SpecChat.Language (the parser)

Parses `.spec.md` files into the specification model.

1. **Markdown extraction** -- extract ` ```spec ` fenced blocks, preserve line/col positions
2. **Lexer** -- tokenize spec block content into Token stream
3. **Parser** -- recursive descent, produces an AST (Abstract Syntax Tree: the structured, in-memory representation of the parsed specification that all downstream tools, protocol, quality analyzer, projections, operate on)
4. **Semantic analyzer** -- resolves references, checks consistency

Semantic analysis (40 checks):

Data specification checks (7):

1. Type resolution: every NamedTypeExpr resolves to a declared entity or enum
2. Circular reference detection
3. Invariant field resolution: every field reference in an invariant exists
4. Refinement consistency: refines blocks do not violate parent invariants
5. Enum completeness: invariants referencing enums cover meaningful cases
6. Contract scope validation: requires/ensures reference existing fields
7. Annotation consistency: @range only on numeric types, @pattern only on strings

Context specification checks (4):

8. Person resolution: every person name is unique across all person declarations
9. External system resolution: every external system name is unique and does not collide with declared system names
10. Relationship endpoint resolution: every source and target in a relationship declaration references a declared person, system, or external system
11. Tag consistency: tag values are non-empty strings; tags referenced in view filters exist on at least one element

Systems specification checks (14):

12. System structure: exactly one `system` root; no duplicate component names across authored and consumed
13. Topology reference resolution: every allow/deny references declared components (authored, consumed, or external systems)
14. Topology cycle detection: deny rules do not contradict allow rules; allow graph has no unintended cycles
15. Phase ordering: requires references resolve to declared phases; no circular phase dependencies
16. Phase coverage: every authored component with `status: new` (or no explicit status) appears in at least one phase's produces list. Components with `status: existing` and consumed components are excluded; they are prerequisites, not products.
16a. Consumed component consistency: every consumed component's `used_by` references resolve to declared authored components; `used_by` edges are consistent with topology allow rules
17. Trace endpoint resolution: every source and target in a trace references a declared entity, component, or named artifact
18. Constraint scope resolution: scope references resolve to declared components
19. Package policy consistency: no package appears in both an allow and deny category
20. Consumed component compliance: every consumed component's `source` package satisfies the active `package_policy` (not in a denied category; if in no allowed category, the consumed component must have rationale)
21. Package policy coverage: every system with consumed components is subject to at least one `package_policy`
22. Platform realization consistency: every authored component appears in exactly one solution folder; no project listed in a solution folder is undeclared as a component
23. Startup project resolution: the `startup` project references a declared authored component with an executable kind (application, host, etc.)
24. Solution-system alignment: the `dotnet solution` name resolves to a declared `system`; format matches target (slnx for net10.0+)

Deployment specification checks (4):

25. Deployment instance resolution: every `instance:` references a declared authored component in the system tree
26. Deployment coverage: every authored component with a host or application kind appears in at least one deployment environment (quality warning, not error)
27. Deployment node nesting: no circular nesting; node names are unique within a deployment environment
28. Deployment environment naming: deployment names are unique across all deployment declarations

View and dynamic specification checks (5):

29. View scope resolution: the `of` clause references a declared system (for systemContext, container), container (for component), or deployment environment (for deployment)
30. View filter resolution: explicit element lists reference declared persons, systems, external systems, or components; tagged filters reference tags that exist on at least one element
31. View kind consistency: systemLandscape views do not use `of`; systemContext, container, component, and deployment views require `of`
32. Dynamic step endpoint resolution: every source and target in a dynamic step references a declared person, system, external system, or component
33. Dynamic step ordering: sequence numbers are unique and contiguous within a dynamic declaration (warning on gaps)

Design specification checks (5):

34. Page host resolution: `host:` references a declared authored component with a host kind (application, blazor-wasm-host, etc.)
35. Page concept resolution: `concepts:` entries reference identifiers that appear as trace sources in a declared `trace` block
36. Visualization page resolution: `page:` references a declared page
37. Visualization component resolution: `component:` references a chart component that appears in a phase `produces` list or a `trace` source
38. Slider-parameter consistency: every identifier in `sliders:` appears as a parameter name in the visualization's `parameters` block or in the parameter binding expressions

### SpecChat.Protocol (the conversation)

This is where SpecChat differs fundamentally from type-based approaches. The protocol governs the conversation between human and LLM, constrained by the specification.

**Prompt construction.** The specification model is rendered into a form the LLM can consume. This is not "emit TypeScript types." It is a projection of the full specification, both data and systems layers, into the LLM's context, in whatever notation best communicates the constraints. The protocol decides what the LLM needs to see.

For data specification, the prompt includes entities, invariants, contracts, and confidence signals. For systems specification, the prompt includes component responsibilities, topology rules (especially deny rules), phase ordering, and system-level constraints. The LLM must know what it is forbidden to do (create a dependency that violates a deny rule, skip a phase gate, introduce a third-party library that violates a constraint) as well as what it is expected to produce.

For design specification, the prompt includes the page declaration (route, concepts, role, cross-links), each visualization declaration (component, parameter bindings, sliders), AND the prose intent associated with each. When the LLM realizes a page, it receives the formal structure that constrains what to build alongside the natural-language intent that describes what the visualization should communicate, what the key insight is, and what interaction pattern the user should experience. The formal structure is machine-checkable; the prose intent is machine-readable. Both govern the realization.

**Response evaluation.** When the LLM responds, the protocol evaluates the response against the specification. Data-level and systems-level evaluation are distinct pipelines that converge in observability.

Data-level evaluation (7 stages):

1. **Response extraction** -- extract structured data from the LLM's response
2. **Structural validation** -- required fields present, types correct, enum values valid
3. **Constraint validation** -- @range, @pattern, @default application
4. **Invariant evaluation** -- evaluate cross-field expressions against the data
5. **Contract evaluation** -- pre/postconditions at boundary level
6. **Confidence scoring** -- per-field extraction confidence based on signals
7. **Observability** -- assemble full diagnostic result

Systems-level evaluation (6 stages, when the LLM generates architectural artifacts):

1. **Topology compliance** -- generated project references match allow rules; no deny violations
2. **Package policy compliance** -- generated `.csproj` / `package.json` / `Cargo.toml` references only introduce packages declared as consumed components in the system tree; all consumed packages satisfy the active `package_policy`; no denied packages introduced; packages outside allowed categories require a consumed component declaration with rationale
3. **Phase ordering** -- generated artifacts respect phase sequencing; no phase produces an artifact before its prerequisites are met
4. **Constraint compliance** -- generated artifacts satisfy all system-level constraints (e.g., naming conventions followed, CSS discipline maintained)
5. **Trace coverage** -- generated artifacts cover the targets specified in trace declarations; flag orphaned or missing implementations
6. **Observability** -- assemble systems diagnostic result alongside data diagnostic result

**Conversational repair.** When evaluation finds violations, the protocol constructs a repair turn: a message back to the LLM naming the specific violations and requesting correction. Configurable attempts (default 3), each with a diagnostic history of what was tried and what remains.

```
Your previous response had these issues:
1. INVARIANT VIOLATION: "espresso is always small" -- drink is espresso but size is large.
2. RANGE VIOLATION: quantity was 0, must be between 1 and 10.
3. MISSING FIELD: "drink" is required but absent for item 2.
Fix these specific issues and return corrected output.
```

Each turn records what was fixed and what remains, building a conversation history visible to the developer.

**Confidence scoring.** Per-field signals:

- ExplicitlyStated (user said "large latte" -> drink=latte is high confidence)
- InferredFromContext (user said "the usual" -> guessed)
- DefaultApplied (field not mentioned, default used)
- LlmHedged (response contained "I think" or "probably")
- ConflictingSignals (multiple interpretations possible)
- AmbiguousReference ("coffee" could be multiple drinks)
- NotMentioned (field not referenced in input)

Aggregate entity confidence = minimum field confidence weighted by declared confidence level.

**Conversation result.**

```csharp
public record ConversationResult<T>(
    bool IsValid,
    T? Value,
    ConfidenceReport Confidence,
    IReadOnlyList<Violation> Violations,
    IReadOnlyList<Warning> Warnings,
    IReadOnlyList<ConversationTurn> History,
    SpecQualityReport? SpecQuality,
    TimeSpan TotalDuration);
```

### SpecChat.Projections (secondary artifacts)

Projections are views of the specification, not its purpose. Each projection reads the specification model and generates an artifact appropriate to a particular context.

#### Data projections

- **C# projection** -- records, enums, invariant validation logic
- **TypeScript projection** -- interfaces, union types, validation functions
- **Python projection** -- dataclasses, Pydantic models, validation logic
- **JSON Schema projection** -- for interoperability with existing tooling

#### Systems projections

- **Solution structure projection** -- generated from the `dotnet solution` declaration (or equivalent platform realization construct): `.slnx`/`.sln` file, directory layout, `.csproj` files with project references matching the topology's allow/deny rules, package references matching declared consumed components and `package_policy`. The platform realization is the governing semantic; the projection materializes it.
- **Dependency graph projection** -- Mermaid or DOT diagram of component dependencies (both internal references and external packages), with denied edges shown as dashed red lines
- **Phase plan projection** -- ordered checklist of phases with gate commands, suitable for CI/CD pipeline generation or human execution
- **Traceability matrix projection** -- table mapping sources to targets across all trace declarations
- **Package audit projection** -- table of all consumed packages across components, their categories, policy status (allowed/denied/requires-rationale), and declared reasons

#### Context and view projections

- **System context diagram projection** -- generated from person, external system, and relationship declarations: Mermaid, DOT, PlantUML, or Structurizr DSL rendering of C4 Level 1 showing the system, its users, and its external dependencies
- **View diagram projection** -- each `view` declaration generates a diagram at the specified zoom level (systemLandscape, systemContext, container, component, deployment), filtered by include/exclude rules, with autoLayout hint applied
- **Dynamic sequence projection** -- each `dynamic` declaration generates a numbered interaction diagram (Mermaid sequence diagram, PlantUML sequence, or DOT) showing runtime flow for a specific scenario
- **Deployment diagram projection** -- generated from deployment declarations: infrastructure nodes with nested instances, rendered as Mermaid, DOT, or Structurizr DSL deployment views

#### Cross-cutting projections

- **Documentation projection** -- human-readable specification summary combining both data and systems views

#### Inline diagram convention

Mermaid diagrams may appear inline in `.spec.md` files as rendered companions to view, topology, deployment, and dynamic declarations. Each diagram is a ` ```mermaid ` fenced code block introduced by a one-line prose sentence (e.g., "Rendered system context:"). The diagram renders the data already declared in the adjacent ` ```spec ` block, following this mapping:

| Declaration type | Mermaid diagram type | Notes |
|---|---|---|
| `view systemContext`, `view systemLandscape` | `C4Context` | Preserves Person/System semantic shapes |
| `view container`, `view component` | `flowchart LR` with `classDef` | Flowchart provides left-right layout; C4 native only supports top-down |
| `view deployment` | `C4Deployment` | Native nested `Deployment_Node` support |
| `topology` | `flowchart LR` with `classDef` | Allow edges solid, deny edges dashed red via `linkStyle` |
| `dynamic` | `sequenceDiagram` with `autonumber` | Preserves temporal ordering with numbered steps |

Diagrams are not part of the formal specification. They are rendering artifacts that visualize the model for human readers. The spec blocks remain the source of truth; diagrams are regenerated from them. A `.spec.md` file is complete with or without inline diagrams.

Projections are optional. A specification is complete without them.

### SpecChat.Quality (specification analysis)

Evaluates the specification itself, independent of any LLM interaction. Detects under-specification and cosmetic relocation.

#### Data specification quality checks

| Check | What It Detects |
|-------|----------------|
| Naked string fields | Fields typed `string` with no enum, pattern, or rationale |
| Missing invariants | Entities with 3+ fields but zero invariants |
| Ungrounded enums | Enum members with no semantic description |
| Missing rationale | Complex entities with no rationale block |
| Shallow rationale | High-complexity fields (multiple invariants, low confidence) with only Tier 1 simple rationale |
| Confidence gaps | Fields with no @confidence annotation |
| No escape hatch | Entity with all required fields, no optionals, no unknown |
| Dead invariants | Invariants that are tautologically true |

#### Context specification quality checks

| Check | What It Detects |
|-------|----------------|
| No persons declared | System has pages or visualizations but no person declarations (who is the audience?) |
| Orphaned person | Person declared but not referenced in any relationship or dynamic step |
| External system without rationale | External system declared with no rationale (integration chosen without recorded reasoning) |
| Orphaned external system | External system declared but not referenced in any relationship, topology, or dynamic step |
| Unconnected system | System declared with persons and/or external systems but no relationship declarations connecting them (context diagram is empty) |

#### Systems specification quality checks

| Check | What It Detects |
|-------|----------------|
| Orphaned component | Authored component declared but not referenced in any topology or consumed component's `used_by` |
| All-allow topology | Topology with only `allow` rules and no `deny` rules (suspiciously permissive; architectural entropy unguarded) |
| Rationale-free denial | `deny` rule with no rationale (prohibitions without reasoning are fragile; the next developer will remove them) |
| Unphased new component | Authored component with `status: new` (or no status, which defaults to new) not assigned to any phase's `produces` list. Components with `status: existing` are exempt; they are prerequisites, not products. |
| Rationale-free consumed component | Consumed component with no rationale (dependency chosen without recorded reasoning; knowledge transmission failure, Eq. 12) |
| Contract-free consumed component | Consumed component with no contract (boundary expectations are implicit; the system trusts without stating what it trusts) |
| Gate-less phase | Phase with no validation gate (construction without proof) |
| Broken trace endpoint | Trace source or target referencing an undeclared entity or component |
| Orphaned trace source | Trace source with zero targets (a concern with no implementation) |
| Uncovered trace target | Component or entity that appears in no trace (a component disconnected from the concern graph) |
| Rationale-free constraint | System-level constraint with no rationale (rules without reasoning invite cargo-cult compliance) |
| Undeclared dependency | Authored component references a package not declared as a consumed component in the system tree (shadow dependency) |
| No package policy | System has consumed components but no `package_policy` (dependency governance is absent) |
| Empty deny list | `package_policy` with no `deny` categories (suspiciously permissive; no dependency boundaries) |
| Missing platform realization | System targets .NET but has no `dotnet solution` declaration (workspace structure is implicit) |
| Orphaned from solution | Authored component declared in the system tree but not in any solution folder |
| Legacy format on modern target | `dotnet solution` uses `format: sln` but system target is net10.0+ (should default to slnx) |

#### Deployment specification quality checks

| Check | What It Detects |
|-------|----------------|
| No deployment declared | System has authored components but no deployment declarations (where things run is unspecified) |
| Undeployed component | Authored component with application or host kind not assigned to any deployment node's `instance` |
| Empty deployment node | Deployment node with no instances and no child nodes (infrastructure declared but nothing runs on it) |
| Deployment without rationale | Deployment environment with no rationale (infrastructure choices without recorded reasoning) |

#### View and dynamic specification quality checks

| Check | What It Detects |
|-------|----------------|
| No views declared | System has persons, external systems, and components but no view declarations (the model has no diagrams) |
| View with no includes | View declared with no include filter (nothing visible; empty diagram) |
| Dynamic with no steps | Dynamic declaration with no steps (scenario is empty) |
| Dynamic step gap | Dynamic steps have non-contiguous sequence numbers (possible missing interaction) |
| Unreferenced dynamic participant | Person or external system declared and appears in relationships but never in a dynamic step (static connections exist but no scenario exercises them) |

#### Design specification quality checks

| Check | What It Detects |
|-------|----------------|
| Page with no visualizations | Page declared but contains no visualization children (under-specified; the page has a route but no visual content) |
| Visualization with no parameter bindings | Visualization declared with a component but no parameter bindings (unconnected to computation methods; the chart has no data source) |
| Visualization with no prose intent | Formal visualization declaration with no associated prose context (structural declaration without design rationale; the Enquiry's cosmetic relocation risk) |
| Page with no prose intent | Page declared but no prose intent associated (the specifier defined the structure but did not articulate what the page should communicate) |
| Orphaned page | Page not referenced by any trace target list (a page exists but no domain concept maps to it) |
| Disconnected visualization | Visualization whose component does not appear in any ComponentsToChartTypes trace (the component is used but not tracked) |

Quality score (0-100) with explanatory diagnostics. Not pass/fail; a feedback signal. Data, context, systems, deployment, view/dynamic, and design checks contribute independently; a specification with perfect entity coverage but no topology is flagged for architectural under-specification, not penalized on entity quality. A specification with systems and deployment but no context is flagged for missing stakeholder identification.

## Project Structure

```
SpecChat/
  SpecChat.slnx
  src/
    SpecChat.Core/             -- specification model: AST, semantic types, diagnostics (no deps)
    SpecChat.Language/         -- parser: markdown extraction, lexer, parser, semantic analysis
    SpecChat.Protocol/         -- conversation: prompt construction, response evaluation,
                                  repair, confidence scoring, observability
    SpecChat.Projections/      -- secondary artifacts: C#, TypeScript, Python, JSON Schema, docs
    SpecChat.Quality/          -- specification quality analysis
    SpecChat.Cli/              -- CLI commands
  tests/
    SpecChat.Core.Tests/
    SpecChat.Language.Tests/
    SpecChat.Protocol.Tests/
    SpecChat.Projections.Tests/
    SpecChat.Quality.Tests/
    SpecChat.Integration.Tests/
  samples/
    coffee-order.spec.md         -- data specification: entities, invariants, confidence
    calendar-intent.spec.md      -- data specification: extraction protocol
    api-gateway.spec.md          -- mixed: data entities + system contracts
    analytics-dashboard.spec.md  -- mixed: context specification (persons, external systems, relationships) + systems specification (components, topology, phases, traces, constraints, package policy, platform realization) + deployment specification (environments, nodes, instances) + views and dynamic (diagram projections, interaction sequences) + data specification (metric model entities)
```

All projects target net10.0 with nullable enabled.

## CLI

```
spec check coffee-order.spec.md              -- parse, analyze, report errors and quality (data + systems)
spec converse coffee-order.spec.md           -- open LLM conversation constrained by the spec
spec project --target csharp order.spec.md   -- generate data projection
spec project --target typescript order.spec.md
spec project --target python order.spec.md
spec project --target solution analytics-dashboard.spec.md  -- generate solution structure from systems spec
spec project --target depgraph analytics-dashboard.spec.md  -- generate dependency graph (Mermaid/DOT)
spec project --target phases analytics-dashboard.spec.md    -- generate phase execution plan
spec project --target matrix analytics-dashboard.spec.md    -- generate traceability matrix
spec quality coffee-order.spec.md            -- specification quality analysis only
spec diff v1/order.spec.md v2/order.spec.md  -- compare two specification versions
spec refine order.spec.md                    -- trace refinement links
spec topology analytics-dashboard.spec.md                -- visualize and validate component dependency rules
spec phases analytics-dashboard.spec.md                  -- show phase ordering with gate status
spec trace analytics-dashboard.spec.md                   -- show cross-reference coverage
spec context analytics-dashboard.spec.md                 -- show system context diagram (persons, external systems, relationships)
spec deployment analytics-dashboard.spec.md              -- show deployment environments with infrastructure nodes
spec views analytics-dashboard.spec.md                   -- list declared views with scope and filters
spec dynamic analytics-dashboard.spec.md                 -- show dynamic interaction sequences
```

## Implementation Phases

### Phase 1: Data Model and Language (MVP)
- SpecChat.Core: data specification AST nodes (EntityDecl, EnumDecl, ContractDecl, RefinementDecl, FieldDecl, RationaleDecl, InvariantDecl, TypeExpr, Expr, TextSpan), Diagnostic types
- SpecChat.Language: MarkdownExtractor, Lexer, recursive descent Parser, SemanticAnalyzer (checks 1-7)
- SpecChat.Cli: `spec check` command (data specification)
- Sample: coffee-order.spec.md parsed and analyzed end-to-end
- Tests: parser tests, semantic analysis tests for all data constructs

### Phase 2: Context and Systems Model
- SpecChat.Core: context specification AST nodes (PersonDecl, ExternalSystemDecl, RelationshipDecl, TagAnnotation)
- SpecChat.Core: systems specification AST nodes (SystemDecl, AuthoredComponentDecl, ConsumedComponentDecl, TopologyDecl, DependencyRule with enriched edges, PhaseDecl, GateDecl, TraceDecl, TraceLink, ConstraintDecl, PackagePolicyDecl, CategoryRule, PlatformRealizationDecl, DotNetSolutionDecl, SolutionFolderDecl)
- SpecChat.Language: extended Lexer and Parser for context and systems keywords; SemanticAnalyzer (checks 8-24)
- SpecChat.Cli: `spec check` extended for context and systems specification; `spec topology`, `spec phases`, `spec trace` commands
- Sample: analytics-dashboard.spec.md parsed and analyzed end-to-end (a Blazor WebAssembly dashboard, expressed in SpecChat syntax, with persons, external systems, and relationships)
- Tests: parser tests for all context and systems constructs, semantic analysis tests for person resolution, relationship endpoints, topology validation, phase ordering, trace resolution

### Phase 3: Deployment, Views, Dynamic, and Design Model
- SpecChat.Core: deployment specification AST nodes (DeploymentDecl, DeploymentNodeDecl)
- SpecChat.Core: view specification AST nodes (ViewDecl, ViewFilter)
- SpecChat.Core: dynamic specification AST nodes (DynamicDecl, DynamicStep)
- SpecChat.Core: design specification AST nodes (PageDecl, VisualizationDecl, ParameterBindingDecl, ProseIntent)
- SpecChat.Language: extended markdown extractor for prose context association (heading + keyword context opening); extended Lexer and Parser for `deployment`, `view`, `dynamic`, `page`, `visualization`, `parameters` keywords; SemanticAnalyzer (checks 25-38)
- SpecChat.Cli: `spec check` extended for deployment, views, dynamic, and design specification
- Sample: analytics-dashboard.spec.md extended with deployment environments, views, a dynamic scenario, ExecutiveDashboard page and its visualizations, interleaved with prose intent
- Tests: parser tests for deployment/view/dynamic/page/visualization constructs, prose-intent association tests, semantic analysis tests for deployment instance resolution, view scope resolution, dynamic step endpoints, host resolution, concept resolution, component resolution, slider-parameter consistency

### Phase 4: Conversation Protocol
- SpecChat.Protocol: prompt construction from specification model (all three registers: data, systems, design)
- Data-level response evaluation (stages 1-7): extraction, structural, constraint, invariant, contract, confidence, observability
- Systems-level response evaluation (stages 1-6): topology compliance, package policy, phase ordering, constraint compliance, trace coverage, observability
- Conversational repair with diagnostic-informed turns
- Confidence scoring with per-field signals
- Observability: ConversationResult with full history
- SpecChat.Cli: `spec converse` command

### Phase 5: Quality Analysis
- SpecChat.Quality: all data quality checks (naked strings, missing invariants, ungrounded enums, missing rationale, shallow rationale, confidence gaps, no escape hatch, dead invariants)
- SpecChat.Quality: all systems quality checks (orphaned component, all-allow topology, rationale-free denial, unphased component, rationale-free consumed component, contract-free consumed component, gate-less phase, broken trace endpoint, orphaned trace source, uncovered trace target, rationale-free constraint, undeclared dependency, no package policy, empty deny list, missing platform realization, orphaned from solution, legacy format on modern target)
- SpecChat.Quality: all design quality checks (page with no visualizations, visualization with no parameter bindings, visualization with no prose intent, page with no prose intent, orphaned page, disconnected visualization)
- Combined quality score with independent data, systems, and design subscores
- SpecChat.Cli: `spec quality` command

### Phase 6: Projections
- Data projections: C#, TypeScript, Python, JSON Schema
- Systems projections: solution structure, dependency graph (Mermaid/DOT), phase plan, traceability matrix
- Context projections: system context diagram, view diagrams (Mermaid/DOT/Structurizr DSL), dynamic sequence diagrams, deployment diagrams
- Documentation projection (cross-cutting)
- SpecChat.Cli: `spec project` command with all targets

### Phase 7: Specification Lifecycle
- Contract evaluation (system-level specifications)
- Refinement checking and traceability
- Specification diffing (both data and systems changes)
- SpecChat.Cli: `spec diff`, `spec refine` commands
- api-gateway.spec.md sample end-to-end (mixed data + systems specification)

## Key Design Decisions

1. **The specification is the primary artifact.** Generated code, validation logic, solution structures, and prompts are projections of the specification. They are secondary and optional.
2. **The conversation is the interface.** SpecChat is not a compiler; it is a communication protocol. The human authors specifications; the LLM realizes within them; the protocol governs the exchange.
3. **All five registers are peers, not layers.** Context, data, systems, deployment, and view/dynamic specifications all live in the same `.spec.md` document. None is subordinate to another. A specification can contain any combination of registers. The quality analyzer evaluates each independently.
4. **Expression language is not Turing-complete**: comparisons, boolean logic, membership tests, count/exists/all. No loops, no variables, no function definitions.
5. **Complexity is opt-in**: a specification can start with entity + enum only, or with component + topology only. All other constructs (invariants, contracts, rationale, confidence, phases, traces, constraints) are optional. The language grows with the specifier's ambition.
6. **Prohibitions are first-class.** The `deny` keyword in topology and the `constraint` construct exist because architectural prohibitions are load-bearing. They prevent entropy. A specification language that can only express permissions is half a language.
7. **Phases carry proof obligations.** A phase without a gate is construction without proof. The quality analyzer flags this. The distinction between "what to build" and "how to prove it was built correctly" is structural, not advisory.
8. **Traceability is a graph, not a tree.** The `trace` construct supports many-to-many mappings because real systems have many-to-many relationships between concerns. Refinement (`refines`) is a tree. Traceability is a web. Both are needed.
9. **Transparent conversation over silent repair**: full conversation history is always available. The developer sees what the LLM attempted, what failed, what was corrected, what remains uncertain. Consistent with FORCE preservation (Eq. 11).
10. **No dependencies on existing type-chat or schema libraries.** SpecChat is standalone. Useful ideas from prior work are adopted as patterns, not as source code or package dependencies.
11. **The decomposition tree distinguishes authored from consumed.** Every node in the system tree has a disposition: `authored` (we write this) or `consumed` (we use this). Authored components carry internal structure, phases, and gates. Consumed components carry boundary contracts, version constraints, and rationale for why they were chosen. The specifier does not describe the internals of consumed nodes. This mirrors Aspire's insight: `AddPostgres("db")` does not mean "write a database." It means "this system includes a Postgres database." Package policies (`package_policy`) govern what may be consumed, with allow/deny categories and a default that requires justification. This is not dependency management (that is what NuGet, npm, and Cargo do). This is dependency governance: the specification-level decision about what enters the system and why.
12. **Systems specification scope is evidence-driven.** The systems construct families (component, topology, phase, trace, constraint, package policy, platform realization) are drawn from what real project specification practice proved to be load-bearing. Context specification (persons, external systems, relationships) was added because C4 practice demonstrates that identifying stakeholders and external dependencies before decomposing internals prevents architectural blind spots. Deployment specification was added because the gap between "what you build" and "where it runs" is where production incidents hide. Views and dynamics were added because a single model needs multiple projections at different zoom levels, and static topology alone cannot capture runtime interaction sequences.
13. **Model-vs-views separation.** The model (persons, systems, components, external systems, deployment nodes) is defined once. Views select subsets for visualization. This prevents the fragmentation that occurs when each diagram is an independent artifact with its own definitions. Changes to the model propagate to all views automatically.
14. **Context before decomposition.** Person and external system declarations force the specifier to answer "who uses this?" and "what does it talk to?" before diving into internal structure. This is the C4 Level 1 discipline: the outermost zoom level is where the most consequential architectural decisions are visible.

## Reference Files

- `Docs/Spec-Chat/SpecLang-Grammar.md` -- formal grammar (EBNF): lexer tokens, expression language, DSL productions (data, context, systems, deployment, view/dynamic, design), ambiguity resolution, parser architecture
- `Docs/Enquiry-Into-Specification-as-Meaningful-Struggle.md` -- theoretical foundation (the Enquiry)
- C4 Model (https://c4model.com) -- the abstraction-level framework that informed context, deployment, and view constructs
- Structurizr DSL (https://structurizr.com) -- the model-vs-views separation principle that informed view declarations
