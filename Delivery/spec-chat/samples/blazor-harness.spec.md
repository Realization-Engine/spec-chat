# F* Visual Test Harness -- System Specification

## Tracking

| Field | Value |
|---|---|
| Created | 2026-04-03 |
| State | Approved |
| Reviewed | 2026-04-03 |
| Approved | 2026-04-03 |
| Executed | |
| Verified | |
| Dependencies | None (base system specification) |

This specification describes the Blazor WebAssembly standalone app that provides
interactive visualizations for the 45 equations in *The Multiplier and the Mirror*.
The app references the `FStarEquations` class library directly; all computation
runs client-side in the browser.

This document is expressed in SpecChat syntax and serves as the Phase 2 validation
artifact: if the grammar can parse this file end-to-end, the systems specification
layer is proven against a real system.

## System Declaration

```spec
system FStarVisualHarness {
    target: "net10.0";
    responsibility: "Interactive visualizations for 45 equations
                     from The Multiplier and the Mirror.
                     All computation runs client-side in the browser.";

    // ===== Authored components: things we build =====

    authored component FStarEquations {
        kind: library;
        path: "src/FStarEquations";
        status: existing;
        responsibility: "Pure computation. 45 equation methods across
                         13 static classes. No UI, no IO,
                         no external dependencies.";

        contract {
            guarantees "BaseModel: ComputeOutput,
                        ValidateLayerOrdering,
                        ComputeOutputAcrossDomains,
                        SkillValueSensitivity";
            guarantees "VarianceAmplification: OutputVariance,
                        OutputVarianceLowerBound,
                        OutputVarianceCorrelated,
                        AbsoluteOutputGap, MarketValue";
            guarantees "CreationEvaluation: EvaluationThroughput,
                        ReallocatedForce";
            guarantees "NegativeForce: DirectedForce, Damage,
                        EpistemicGap";
            guarantees "ForceDynamics: DfDt, DfSurfaceDt,
                        DfMiddleDt, DfDeepDt, TippingPoint,
                        IsHysteresisPresent";
            guarantees "TacitKnowledge: KnowledgeStockNext,
                        Transmission, SharedWork,
                        IsPipelineBroken";
            guarantees "DivergenceTrajectories: DfHighDt, DfLowDt,
                        IsGapWidening, IsGapAccelerating,
                        InitialForce";
            guarantees "OrganizationalDynamics: MarginalReturn,
                        AssessmentSnr, MeasuredForce,
                        OrganizationalThroughput,
                        IndecisionCost, CompetitiveAdvantage";
            guarantees "Motivation: MotivationDecay";
            guarantees "Sovereignty: NationalCapability,
                        IsSovereignResilient";
            guarantees "ModelGrowth: MultiplierAtTime";
            guarantees "ForceToModelTransfer: TransferRate,
                        ValidateTransferEfficiencies,
                        AbsorptionCeiling, TotalTimeAllocation,
                        IsKnowledgePreserved,
                        HasTippingPointRisen,
                        ModelQualityNext";
            guarantees "Integration: ForwardEuler,
                        ForwardEulerSystem";
        }

        rationale "Pre-existing component. The equation library and its
                   116 equation library runtime cases predate this
                   specification. It must be added to the solution but
                   not created. The contract above declares the API surface
                   that the harness app and its tests depend on.";
    }

    authored component FStar.UI {
        kind: "razor-library";
        path: "src/FStar.UI";
        responsibility: "Reusable Blazor chart and control components.
                         Generic. No domain coupling.";

        rationale {
            context "Charts are reused across 16 pages with
                     different equation data.";
            decision "Separate Razor class library with no knowledge
                      of the domain.";
            consequence "The app wires domain data to components.
                         The library is reusable outside fstar.";
        }
    }

    authored component FStarEquations.App {
        kind: "blazor-wasm-host";
        path: "src/FStarEquations.App";
        responsibility: "Blazor WASM host. Bridges equations
                         to visualizations. 17 pages (Home plus
                         13 section pages plus 3 dashboards),
                         25 standalone charts,
                         12 composite visualizations.";
    }

    authored component FStarEquations.App.Tests {
        kind: "unit-test-harness";
        path: "src/FStarEquations.App.Tests";
        responsibility: "BUnit tests for all chart components,
                         controls, layout wrappers, and pages.";
        includes: TheoryCardTests (9 test methods in
                  Pages/TheoryCardTests.cs) counted toward BUnit
                  test totals. See feature-theory-tabs.spec.md.
    }

    // ===== Consumed components: things we use =====

    consumed component BlazorWasm {
        source: nuget("Microsoft.AspNetCore.Components.WebAssembly");
        version: "10.*";
        responsibility: "Blazor WebAssembly hosting runtime.
                         Provides client-side .NET execution in browser.";
        used_by: [FStarEquations.App];

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
        used_by: [FStar.UI];
    }

    consumed component JSInterop {
        source: nuget("Microsoft.JSInterop");
        version: "10.*";
        responsibility: "Bridge between .NET and JavaScript
                         for Canvas rendering.";
        used_by: [FStar.UI];
    }

    consumed component BUnit {
        source: nuget("bunit");
        version: "2.*";
        responsibility: "In-process Blazor component rendering for tests.
                         No browser required.";
        used_by: [FStarEquations.App.Tests];

        rationale {
            context "Blazor components cannot be tested with standard
                     xunit assertions alone. They require a rendering host.";
            decision "BUnit renders components in-process, asserts on
                      markup and interaction, no browser dependency.";
            consequence "Tests run fast, in CI, without Selenium
                         or Playwright.";
        }
    }

    consumed component XUnit {
        source: nuget("xunit");
        version: "2.*";
        responsibility: "Test framework.";
        used_by: [FStarEquations.App.Tests];
    }

    consumed component TestSdk {
        source: nuget("Microsoft.NET.Test.Sdk");
        version: "17.*";
        responsibility: "Test execution infrastructure.";
        used_by: [FStarEquations.App.Tests];
    }
}
```

## Platform Realization

```spec
dotnet solution FStarEquations {
    format: slnx;
    startup: FStarEquations.App;

    folder "src" {
        projects: [FStarEquations, FStar.UI, FStarEquations.App,
                   FStarEquations.App.Tests, FStarEquationsTests];
    }

    rationale {
        context "The .NET SDK defaults to .slnx in .NET 10.
                 The fstar project already uses FStarEquations.slnx.";
        decision "Use .slnx format. All projects reside under a single
                  src/ folder. Test projects are not separated into a
                  tests/ folder because the existing layout predates
                  the specification and reorganizing would disrupt
                  git history for no functional benefit.
                  The .slnx also contains a /Docs/ solution folder
                  with design documents and implementation plans.
                  This folder holds files, not projects, so it cannot
                  be expressed as a SolutionFolderDecl (which requires
                  a projects list). Its presence is noted here instead.";
        consequence "All phase gate commands target the solution root:
                     dotnet build FStarEquations.slnx
                     dotnet test FStarEquations.slnx";
    }
}
```

## Package Policy

```spec
package_policy HarnessPolicy {
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
                  "Microsoft.NET.Test.Sdk", "Moq", "NSubstitute",
                  "coverlet.collector"];

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
```

## Topology

```spec
topology ProjectDependencies {
    // Authored-to-authored edges
    allow FStarEquations.App -> FStar.UI;
    allow FStarEquations.App -> FStarEquations;
    allow FStarEquations.App.Tests -> FStar.UI;
    allow FStarEquations.App.Tests -> FStarEquations;
    allow FStarEquations.App.Tests -> FStarEquations.App;
    deny  FStar.UI -> FStarEquations;

    invariant "nullable everywhere":
        all authored components satisfy nullable == enabled;

    rationale "deny FStar.UI -> FStarEquations" {
        context "FStar.UI is a generic component library.
                 FStarEquations is domain logic.";
        decision "The library must not know about the domain.
                  Only the app bridges them.";
        consequence "Components are reusable outside fstar.
                     Domain changes never break the UI library.";
    }
}
```

## Construction Phases

```spec
phase Scaffolding {
    produces: [FStar.UI, FStarEquations.App,
               FStarEquations.App.Tests];

    gate build {
        command: "dotnet build FStarEquations.slnx
                  -p:TreatWarningsAsErrors=true";
        expects: errors == 0, warnings == 0;
    }

    gate equation_tests {
        command: "dotnet test FStarEquations.slnx
                  --filter FullyQualifiedName~FStarEquationsTests";
        expects: pass >= 116, fail == 0;
    }

    gate app_launches {
        command: "dotnet run --project src/FStarEquations.App/";
        expects: "Loads in browser, nav links work,
                  all 17 pages show placeholder heading";
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
    produces: [LineChart, BarChart, ParameterSlider,
               SliderPanel, ChartCard, SplitPanel,
               ThresholdIndicator];

    gate tests {
        command: "dotnet test FStarEquations.slnx";
        expects: pass >= 134, fail == 0;
    }
}

phase HubAndStandaloneCharts {
    requires: CoreComponents;
    produces: [TippingPoint, NumberLine, TornadoDiagram,
               PhasePortrait, HeatMap, WaterfallChart];

    gate tests {
        command: "dotnet test FStarEquations.slnx";
        expects: pass >= 164, fail == 0;
    }
}

phase CompositeVisualizations {
    requires: HubAndStandaloneCharts;
    produces: [TimeSeriesAnimator];

    gate tests {
        command: "dotnet test FStarEquations.slnx";
        expects: pass >= 225, fail == 0;
    }
}

phase Dashboards {
    requires: CompositeVisualizations;
    produces: [CascadeDashboard, TerminalDynamics,
               TimelineDashboard];

    gate tests {
        command: "dotnet test FStarEquations.slnx";
        expects: pass >= 225, fail == 0;
    }
}

phase Polish {
    requires: Dashboards;

    gate build {
        command: "dotnet build FStarEquations.slnx
                  -p:TreatWarningsAsErrors=true";
        expects: errors == 0, warnings == 0;
    }

    rationale "Final quality pass: responsive layout, keyboard
               accessibility, dark/light theme, section prose.";
}
```

## Traceability

```spec
trace EquationsToPages {
    Eq1   -> [BaseModelPage, CascadeDashboard, TimelineDashboard];
    Eq1a  -> [BaseModelPage];
    Eq2   -> [BaseModelPage];
    Eq3   -> [BaseModelPage];
    Eq4   -> [VariancePage, ModelGrowthPage, CascadeDashboard];
    Eq4a  -> [VariancePage];
    Eq5   -> [VariancePage];
    Eq6   -> [VariancePage, CascadeDashboard];
    Eq7   -> [CreationEvaluationPage, CascadeDashboard];
    Eq7a  -> [CreationEvaluationPage, TransferPage];
    Eq8   -> [NegativeForcePage];
    Eq9   -> [NegativeForcePage];
    Eq10  -> [NegativeForcePage, CascadeDashboard];
    Eq11  -> [ForceDynamicsPage, CascadeDashboard,
              TerminalDynamics, TimelineDashboard];
    Eq11a -> [ForceDynamicsPage];
    Eq11b -> [ForceDynamicsPage];
    Eq11c -> [ForceDynamicsPage];
    Eq12  -> [TacitKnowledgePage, CascadeDashboard, TimelineDashboard];
    Eq12a -> [TacitKnowledgePage];
    Eq12b -> [TacitKnowledgePage];
    Eq13  -> [TacitKnowledgePage];
    Eq14  -> [TippingPoint, ForceDynamicsPage, ModelGrowthPage,
              CascadeDashboard, TimelineDashboard];
    Eq14a -> [TippingPoint];
    Eq15a -> [DivergencePage];
    Eq15b -> [DivergencePage];
    Eq16  -> [DivergencePage];
    Eq16a -> [DivergencePage];
    Eq17  -> [OrganizationalPage];
    Eq18  -> [OrganizationalPage, TimelineDashboard];
    Eq19  -> [OrganizationalPage];
    Eq20  -> [OrganizationalPage];
    Eq21  -> [OrganizationalPage, ModelGrowthPage];
    Eq22  -> [OrganizationalPage];
    Eq23  -> [MotivationPage, CascadeDashboard];
    Eq24  -> [SovereigntyPage];
    Eq24a -> [SovereigntyPage];
    Eq25  -> [ModelGrowthPage,
              TerminalDynamics, TimelineDashboard];
    Eq26  -> [TransferPage, CascadeDashboard];
    Eq26a -> [TransferPage];
    Eq27  -> [TransferPage];
    Eq28  -> [TransferPage, CreationEvaluationPage];
    Eq30  -> [TransferPage, TippingPoint];
    Eq31  -> [TransferPage, TerminalDynamics,
              TimelineDashboard, CascadeDashboard];
    Eq32  -> [DivergencePage, CascadeDashboard];

    invariant "every equation has at least one page":
        all sources have count(targets) >= 1;

    invariant "hub page covers core equations":
        TippingPoint.sources contains [Eq14, Eq14a];
}

trace ChartComponentsToPages {
    LineChart        -> [BaseModelPage, VariancePage, MotivationPage,
                         ModelGrowthPage, DivergencePage,
                         OrganizationalPage, ForceDynamicsPage,
                         TacitKnowledgePage, SovereigntyPage,
                         TimelineDashboard];
    BarChart         -> [BaseModelPage, CreationEvaluationPage,
                         OrganizationalPage, SovereigntyPage,
                         TransferPage];
    HeatMap          -> [BaseModelPage, NegativeForcePage];
    WaterfallChart   -> [NegativeForcePage];
    NumberLine       -> [TippingPoint, TransferPage];
    TornadoDiagram   -> [TippingPoint];
    PhasePortrait    -> [TippingPoint, TerminalDynamics];
    TimeSeriesAnimator -> [TimeSeriesPlayerPage];
}
```

## System-Level Constraints

```spec
constraint CssDiscipline {
    scope: all authored components;
    rule: "App CSS only in Site.css.
           Component CSS only in .razor.css files within FStar.UI.";

    rationale {
        context "CSS sprawl across arbitrary locations caused
                 maintenance problems in previous projects.";
        decision "Two locations only. App controls placement.
                  Library controls appearance.";
        consequence "Any CSS outside these two patterns is a violation.";
    }
}

constraint TestNaming {
    scope: [FStarEquations.App.Tests];
    rule: "MethodName_Scenario_ExpectedResult";

    rationale "Consistent naming makes test failures immediately
               interpretable without reading the test body.";
}

constraint NoJsFrameworks {
    scope: [FStar.UI, FStarEquations.App];
    rule: "No third-party JavaScript frameworks.
           All JS interop is custom, minimal, and lives in
           FStar.UI.lib.module.js only.";

    rationale {
        context "JavaScript framework dependencies create version
                 conflicts, bundle size growth, and update obligations
                 disproportionate to the value they provide for
                 SVG/Canvas chart rendering.";
        decision "Custom JS interop via a single module file.";
        consequence "Higher initial cost for Canvas rendering.
                     No framework upgrade burden. Full control
                     over rendering pipeline.";
    }
}

constraint FutureWorkExtraction {
    scope: [FStarEquations.App];
    rule: "If a second page requires a directed-graph flow
           visualization, the inline SVG pattern from
           CascadeDashboard shall be extracted into a reusable
           FlowDiagram authored component in
           src/FStar.UI/Components/Charts/ before the second
           page is merged.";

    rationale "Single-use inline SVG is acceptable. Duplicated
               inline SVG is not. This constraint defers the
               abstraction cost until a second consumer proves
               the need.";
}
```

## Data Specification: Chart Component Model

The chart components in FStar.UI use a shared data model for series, data points,
and bar items. This section specifies the data contracts that flow between the
equation library (computation) and the chart components (rendering).

```spec
entity ChartSeries {
    label: string;
    points: DataPoint[];
    color: string @default("#2563eb");
    strokeWidth: double @default(2.0);
    showDots: bool @default(false);

    invariant "at least one point": count(points) > 0;

    rationale "Each series represents one line or curve on a chart.
               Color and stroke are visual defaults that pages override
               when multiple series share a chart.";
}

entity DataPoint {
    x: double;
    y: double;
}

entity BarItem {
    label: string;
    value: double;
    color: string @default("#2563eb");
}

entity WaterfallItem {
    label: string;
    value: double;
    isTotal: bool @default(false);

    rationale "isTotal" {
        context "Waterfall charts distinguish running steps from
                 summary totals. Steps draw from the previous bar's
                 end position. Totals draw from the baseline.";
        decision "Boolean flag on each item.";
        consequence "The chart component uses isTotal to choose
                     baseline (absolute) vs. stacked rendering.";
    }
}

entity NumberLinePoint {
    value: double;
    color: string;
}

entity TornadoFactor {
    label: string;
    positiveContribution: double;
    negativeContribution: double;

    invariant "positive is non-negative":
        positiveContribution >= 0;
    invariant "negative is non-positive":
        negativeContribution <= 0;
}

enum BarOrientation {
    vertical: "Bars extend upward from the X axis",
    horizontal: "Bars extend rightward from the Y axis"
}
```

## Base Model

The base production function captures the core relationship between the multiplier M and force F. Output is a Cobb-Douglas product of M and the components of F, which means that every force component must be positive for output to be nonzero. The layer ordering constraint ensures that surface effectiveness always exceeds middle, and middle always exceeds deep. Across multiple domains, total output sums per-domain terms, each weighted by its own multiplier and force vector. Skill-level sensitivity shows how the marginal return on investing in a given skill depends on the average force already present.

```spec
page BaseModelPage {
    host: FStarEquations.App;
    route: "/base-model";
    concepts: [Eq1, Eq1a, Eq2, Eq3];
    role: "Foundation. Establishes the multiplicative structure
           that every subsequent equation builds on.";
    cross_links: [VariancePage, CreationEvaluationPage,
                  NegativeForcePage, ForceDynamicsPage];
}
```

### Cobb-Douglas Sensitivity Surface

A two-dimensional heat map showing output as a function of two selected force components while holding the others fixed. The color gradient runs from near-zero to high output. The critical pattern is the collapse zone along each axis: when any single force component approaches zero, the entire product collapses regardless of how strong the other components are.

```spec
visualization CobbDouglasSensitivitySurface {
    page: BaseModelPage;
    component: HeatMap;

    parameters {
        output: BaseModel.ComputeOutput(M, f_domain, f_judgment,
                remainingForces, weights);
    }

    sliders: [M, f_domain, f_judgment, remainingForces, weights];
}
```

### Layer Ordering

A bar chart with three bars representing surface, middle, and deep effectiveness, overlaid with a threshold indicator that flags when the ordering invariant is violated. Under normal calibration the hierarchy is always maintained, which makes the structural constraint feel inevitable rather than arbitrary.

```spec
visualization LayerOrdering {
    page: BaseModelPage;
    component: BarChart;

    parameters {
        isValid: BaseModel.ValidateLayerOrdering(mEffSurface,
                 mEffMiddle, mEffDeep);
    }

    sliders: [mEffSurface, mEffMiddle, mEffDeep];
}
```

### Domain Output Breakdown

A stacked bar chart decomposing total output across domains. Each bar segment represents one domain's contribution. Total output is dominated by domains where both M and F are high; domains where either factor is weak contribute almost nothing.

```spec
visualization DomainOutputBreakdown {
    page: BaseModelPage;
    component: BarChart;

    parameters {
        domainOutputs: BaseModel.ComputeOutputAcrossDomains(
                       domainMultipliers, domainForces);
    }

    sliders: [domainMultipliers, domainForces];
}
```

### Skill Value Sensitivity

A line chart plotting the marginal return on skill investment against M for several fixed force levels. The curves fan out: at low force, marginal returns are shallow; at high force, the same investment yields a much steeper return.

```spec
visualization SkillValueSensitivity {
    page: BaseModelPage;
    component: LineChart;

    parameters {
        sensitivity: BaseModel.SkillValueSensitivity(dMsDIp,
                     avgForce);
    }

    sliders: [dMsDIp, avgForce];
}
```

## Variance and Barbell

When force is not uniform across a population, the variance of force enters the output equation directly. Output variance scales with the square of M, so higher-multiplier environments amplify force dispersion into output dispersion. The market value function maps force levels to a piecewise curve that produces the characteristic hollowed-out middle: a low floor, a compressed midrange, and a steep premium tier at the top.

```spec
page VariancePage {
    host: FStarEquations.App;
    route: "/variance";
    concepts: [Eq4, Eq4a, Eq5, Eq6];
    role: "Connects individual force variation to population-level
           output dispersion and labor market structure.";
    cross_links: [BaseModelPage, ModelGrowthPage,
                  DivergencePage, CascadeDashboard];
}
```

### Variance Amplification Curve

A line chart plotting output variance against force variance for several fixed values of M. The parabolic shape is the central result: because output variance scales with M squared, a modest increase in the multiplier can dramatically widen the output distribution.

```spec
visualization VarianceAmplificationCurve {
    page: VariancePage;
    component: LineChart;

    parameters {
        variance: VarianceAmplification.OutputVariance(M,
                  varianceF);
    }

    sliders: [M, varianceF];
}
```

### Output Gap Divergence

A composite line chart with three series overlaid: the true output variance, the lower bound estimate, and the absolute output gap. Plotting all three together reveals that the lower bound consistently understates the true gap, and the understatement grows as M increases.

```spec
visualization OutputGapDivergence {
    page: VariancePage;
    component: LineChart;

    parameters {
        trueVariance: VarianceAmplification.OutputVariance(M,
                      varianceF);
        lowerBound: VarianceAmplification.OutputVarianceLowerBound(
                    M, varianceF);
        absoluteGap: VarianceAmplification.AbsoluteOutputGap(M,
                     forceHigh, forceLow);
    }

    sliders: [M, forceHigh, forceLow, varianceF];
}
```

### Market Value Piecewise

A line chart with three visually distinct segments corresponding to the three regimes. The low-force region maps to a flat floor wage. The mid-force region maps to a compressed band. The high-force region maps to a steep premium. The resulting shape is the hollowed middle.

```spec
visualization MarketValuePiecewise {
    page: VariancePage;
    component: LineChart;

    parameters {
        marketValue: VarianceAmplification.MarketValue(force,
                     thresholdLow, thresholdHigh, premiumHigh,
                     wageMid, floorLow);
    }

    sliders: [force, thresholdLow, thresholdHigh, premiumHigh,
              wageMid, floorLow];
}
```

### Barbell and Variance Composite

A composite line chart that overlays the market value curve with the output variance curve on a shared force axis. As M increases, the variance curve steepens while the market value premium tier stretches upward. The premium-tier advantage grows quadratically with M.

```spec
visualization BarbellVarianceComposite {
    page: VariancePage;
    component: LineChart;

    parameters {
        marketValue: VarianceAmplification.MarketValue(force,
                     thresholdLow, thresholdHigh, premiumHigh,
                     wageMid, floorLow);
        varianceOverlay: VarianceAmplification.OutputVariance(M,
                         varianceF);
    }

    sliders: [M, force, thresholdLow, thresholdHigh, premiumHigh,
              wageMid, floorLow, varianceF];
}
```

## Creation vs Evaluation

The evaluation bottleneck arises because each unit of created output must be evaluated before it can be used, and evaluation has its own cost structure. High-force individuals are pulled between creating, evaluating, and transferring knowledge.

```spec
page CreationEvaluationPage {
    host: FStarEquations.App;
    route: "/creation-evaluation";
    concepts: [Eq7, Eq7a];
    role: "Reveals the evaluation bottleneck and the three-way
           resource allocation tension on high-force individuals.";
    cross_links: [BaseModelPage, TransferPage,
                  NegativeForcePage, CascadeDashboard];
}
```

### Evaluation Bottleneck

A bar chart showing evaluation throughput as a function of budget and per-unit cost. The binding constraint is evaluation cost: no matter how much creation capacity is available, output is gated by the ability to evaluate it.

```spec
visualization EvaluationBottleneck {
    page: CreationEvaluationPage;
    component: BarChart;

    parameters {
        throughput: CreationEvaluation.EvaluationThroughput(
                    budgetEval, costEval);
    }

    sliders: [budgetEval, costEval];
}
```

### Resource Allocation

A stacked bar chart showing how a high-force individual's time is divided among creation, evaluation, and knowledge transfer. The zero-sum nature is visible: increasing demand on any one function starves the other two.

```spec
visualization ResourceAllocation {
    page: CreationEvaluationPage;
    component: BarChart;

    parameters {
        reallocated: CreationEvaluation.ReallocatedForce(forceHigh,
                     fractionToEval);
        timeAllocation: ForceToModelTransfer.TotalTimeAllocation(
                        tauCreate, tauEval, tauExtract);
    }

    sliders: [forceHigh, fractionToEval, tauCreate, tauEval,
              tauExtract];
}
```

## Negative Force

Not all force is constructive. Directed force is a weighted sum of components that can be individually positive or negative. Damage scales linearly in both M and the magnitude of negative force. The epistemic gap measures the divergence between presented and substantive capability.

```spec
page NegativeForcePage {
    host: FStarEquations.App;
    route: "/negative-force";
    concepts: [Eq8, Eq9, Eq10];
    role: "Makes destructive contributions and epistemic
           distortion visible and quantifiable.";
    cross_links: [BaseModelPage, ForceDynamicsPage,
                  OrganizationalPage, CascadeDashboard];
}
```

### Directed Force Composition

A waterfall chart decomposing total directed force into its weighted components. Each bar segment is colored green when positive and red when negative. Negative components visibly drag the running total down.

```spec
visualization DirectedForceComposition {
    page: NegativeForcePage;
    component: WaterfallChart;

    parameters {
        directedForce: NegativeForce.DirectedForce(weights,
                       forces);
    }

    sliders: [weights, forces];
}
```

### Damage Scaling

A heat map showing damage as a function of M and the magnitude of negative force. Damage scales linearly in both dimensions: doubling M doubles damage, and doubling negative force doubles damage independently.

```spec
visualization DamageScaling {
    page: NegativeForcePage;
    component: HeatMap;

    parameters {
        damage: NegativeForce.Damage(M, forceNeg, tau);
    }

    sliders: [M, forceNeg, tau];
}
```

### Epistemic Gap Surface

A heat map showing the epistemic gap as a function of the presentation multiplier and the substance multiplier, with individual force held fixed via slider. The gap is largest in the low-force, low-substance corner, where presentation can most easily outrun reality.

```spec
visualization EpistemicGapSurface {
    page: NegativeForcePage;
    component: HeatMap;

    parameters {
        gap: NegativeForce.EpistemicGap(mPresentation,
             mSubstance, forceIndividual);
    }

    sliders: [mPresentation, mSubstance, forceIndividual];
}
```

## Force Dynamics

The force dynamics page is the flagship simulation of the harness. It integrates the full ODE system that governs how an engineer's force evolves across three structural layers: surface, middle, and deep. The composite ODE simulator runs all four equations simultaneously, revealing the layered structure of force change in real time.

```spec
page ForceDynamicsPage {
    host: FStarEquations.App;
    route: "/force-dynamics";
    concepts: [Eq11, Eq11a, Eq11b, Eq11c];
    role: "ODE simulator. The most technically rich
           interactive visualization in the harness.";
    cross_links: [TippingPoint, TacitKnowledgePage,
                  DivergencePage, CascadeDashboard,
                  TerminalDynamics];
}
```

### ODE Simulator

The primary composite visualization. It uses the forward Euler system integrator to advance all four force-layer ODEs simultaneously, then plots the resulting trajectories as four lines sharing a common time axis. F_total(t) is the aggregate. f_surface(t) shows rapid adaptation and equally rapid decay. f_middle(t) is the contested zone where judgment and integration compete with organizational friction. f_deep(t) barely moves on any human timescale. Adjusting the balance between S, E, and R is where the insight lives: small changes in struggle availability can flip the total trajectory from growth to decline.

```spec
visualization ODESimulator {
    page: ForceDynamicsPage;
    component: LineChart;

    parameters {
        system: Integration.ForwardEulerSystem(
                    [ForceDynamics.DfSurfaceDt,
                     ForceDynamics.DfMiddleDt,
                     ForceDynamics.DfDeepDt,
                     ForceDynamics.DfDt],
                    dt, t_max);
    }

    sliders: [alpha, beta_s, beta_m, beta_d, gamma_m,
              S, E, R, sigma, M_abs, dt];
}
```

### Layer Decay Comparison

Three small-multiple line charts, one per layer, drawn from the same ODE integration. Each chart shares a common time axis but uses an independent Y axis scaled to its own range. The rate differences are impossible to miss: surface moves in weeks, middle in months to years, deep in decades.

```spec
visualization LayerDecayComparison {
    page: ForceDynamicsPage;
    component: LineChart;

    parameters {
        surface: Integration.ForwardEuler(
                     ForceDynamics.DfSurfaceDt, dt, t_max);
        middle: Integration.ForwardEuler(
                    ForceDynamics.DfMiddleDt, dt, t_max);
        deep: Integration.ForwardEuler(
                  ForceDynamics.DfDeepDt, dt, t_max);
    }

    sliders: [alpha, beta_s, beta_m, beta_d, gamma_m,
              S, E, R, sigma, M_abs, dt];
}
```

## Tacit Knowledge

The tacit knowledge page visualizes the pipeline through which organizational knowledge survives or dies. Knowledge accrues through transmission from senior practitioners and decays at rate delta. Shared work depends on management overhead M. The pipeline break condition marks where decay overwhelms transmission and the stock begins irreversible decline.

```spec
page TacitKnowledgePage {
    host: FStarEquations.App;
    route: "/tacit-knowledge";
    concepts: [Eq12, Eq12a, Eq12b, Eq13];
    role: "Knowledge pipeline simulator. Shows where
           organizational knowledge dies.";
    cross_links: [ForceDynamicsPage, TippingPoint,
                  DivergencePage, CascadeDashboard];
}
```

### Knowledge Pipeline Simulator

The composite simulator integrates KnowledgeStockNext over successive time steps, producing three plotted quantities: K_tacit(t) as the current knowledge stock, Transmission(t) as the inflow rate, and the decay threshold delta*K(t) as a reference line. A threshold indicator turns red the moment IsPipelineBroken returns true. The critical experiment is dragging M upward: shared work decays, transmission drops below the decay threshold, and the indicator flips.

```spec
visualization KnowledgePipelineSimulator {
    page: TacitKnowledgePage;
    component: LineChart;

    parameters {
        stock: Integration.ForwardEuler(
                   TacitKnowledge.KnowledgeStockNext, dt, t_max);
        transmission: TacitKnowledge.Transmission(phi, W, forceSenior);
        breakIndicator: TacitKnowledge.IsPipelineBroken(
                            delta, phi, W, forceSenior);
    }

    sliders: [delta, phi, W0, psi, M, forceSenior];
}
```

### Shared Work Decay

A single line chart sweeping TacitKnowledge.SharedWork over management headcount M. The curve's steepness at low M values is the point: adding even a few managers from a lean baseline eliminates most of the shared work that transmission depends on.

```spec
visualization SharedWorkDecay {
    page: TacitKnowledgePage;
    component: LineChart;

    parameters {
        curve: sweep(TacitKnowledge.SharedWork, M, 0.0, M_max,
                     W0, psi);
    }

    sliders: [W0, psi];
}
```

## The Tipping Point

Equation (14) is the fulcrum of the entire framework. F* is the threshold force
level above which an engineer's trajectory rises and below which it declines.
F* is not static: every major dynamic in the paper either raises F* or erodes
the force that F* measures. This page is the conceptual hub of the harness.
It gets its own primary navigation entry, separate from the Force Dynamics
ODE simulator.

```spec
page TippingPoint {
    host: FStarEquations.App;
    route: "/tipping-point";
    concepts: [Eq14, Eq14a];
    role: "Conceptual hub. The visualization that makes
           a room go quiet.";
    cross_links: [ForceDynamicsPage, TransferPage,
                  ModelGrowthPage, DivergencePage,
                  MotivationPage];
}
```

### Moving Threshold

The primary visualization is a population of engineers plotted as dots along a
force axis, with F* as a vertical dividing line. Dots above F* are green
(growth trajectory). Dots below are red (decay trajectory).

As the user adjusts sliders (M_absorbed, R, sigma, gamma, E), F* slides along
the axis. Engineers whose dots are swept past the threshold flip from green to
red in real time.

Population model: configurable distribution (normal or bimodal) with mean and
spread sliders.

Key insight: the threshold moves *through* the population. Engineers do not get
weaker; the bar gets higher.

```spec
visualization MovingThreshold {
    page: TippingPoint;
    component: NumberLine;

    parameters {
        threshold: ForceDynamics.TippingPoint(beta, R, sigma,
                   M_abs, gamma, E);
        points: population(distribution, mean, spread);
    }

    sliders: [beta, R, sigma, M_abs, gamma, E,
              distribution, mean, spread];
}
```

### Parameter Decomposition

Shows which parameter has the most influence on F* and in which direction.
Numerator terms (beta*R, sigma*M_abs) push F* right (higher threshold).
Denominator terms (gamma*E) push it left (lower threshold). Users see which
lever matters most and can experiment with the balance.

```spec
visualization ParameterDecomposition {
    page: TippingPoint;
    component: TornadoDiagram;

    parameters {
        factors: decompose(ForceDynamics.TippingPoint,
                           [beta, R, sigma, M_abs, gamma, E]);
    }

    sliders: [beta, R, sigma, M_abs, gamma, E];
}
```

### Phase Portrait

The curve of dF/dt vs F, with F* marked as the zero-crossing. The green region
(dF/dt > 0) shows where force grows; the red region (dF/dt < 0) shows where
it decays. Moving sliders shifts the crossing point, making the two basins of
attraction visible. This is the mathematical companion to the Moving Threshold:
that visualization shows the *population consequence*; this one shows the
*dynamical structure*.

```spec
visualization ForcePhasePortrait {
    page: TippingPoint;
    component: PhasePortrait;

    parameters {
        curve: sweep(ForceDynamics.DfDt, F, 0.0, 1.0,
                     alpha, S, gamma, E, beta, R, sigma, M_abs);
        zeroCrossing: ForceDynamics.TippingPoint(beta, R, sigma,
                      M_abs, gamma, E);
    }

    sliders: [alpha, S, gamma, E, beta, R, sigma, M_abs];
}
```

### Hysteresis

Asymmetric funnel on a number line showing |dF/dt| for decay vs. recovery at
equal distances from F*. The steep descent arrow on the left and shallow ascent
arrow on the right make the asymmetry visceral: it is easier to fall than to
climb back. F* is a cliff, not a hill.

```spec
visualization Hysteresis {
    page: TippingPoint;
    component: NumberLine;

    parameters {
        decayRate: ForceDynamics.DfDt(alpha, S, gamma, E,
                   beta, R, sigma, M_abs, F_below);
        recoveryRate: ForceDynamics.DfDt(alpha, S, gamma, E,
                      beta, R, sigma, M_abs, F_above);
        isAsymmetric: ForceDynamics.IsHysteresisPresent(
                      decayRate, recoveryRate);
    }

    sliders: [alpha, S, gamma, E, beta, R, sigma, M_abs,
              distance];

    rationale "Uses the same NumberLine component as MovingThreshold
               but in a different mode: showing rate magnitudes as
               arrows rather than population dots. The asymmetry
               between decay and recovery is the key insight.";
}
```

## The Accelerating Gap

Engineers who start above F* compound upward; those who start below it decay toward a floor. The gap between them does not just widen; it accelerates. Successive cohorts enter with lower force ceilings because the formative environment offers less resistance to grow against.

```spec
page DivergencePage {
    host: FStarEquations.App;
    route: "/divergence";
    concepts: [Eq15a, Eq15b, Eq16, Eq16a, Eq32];
    role: "Divergence trajectories. Shows that the gap
           between high-force and low-force engineers
           accelerates over time.";
    cross_links: [TippingPoint, ForceDynamicsPage,
                  TacitKnowledgePage, CascadeDashboard];
}
```

### Divergence Trajectories

Two lines integrated from the same starting time: F_H(t) rising under compounding growth, and F_L(t) decaying toward a structural floor. The area between the curves is shaded, making the gap a visible geometric quantity. The shaded region's rate of expansion itself increases over time, producing a funnel that widens faster and faster.

```spec
visualization DivergenceTrajectories {
    page: DivergencePage;
    component: LineChart;

    parameters {
        system: Integration.ForwardEulerSystem(
                    [DivergenceTrajectories.DfHighDt,
                     DivergenceTrajectories.DfLowDt],
                    dt, t_max);
        lines: [F_H, F_L];
    }

    sliders: [alpha, S0, gamma, beta, M, kappa,
              F_H_initial, F_L_initial];
}
```

### Cohort Force Ceiling

A single line chart sweeping DivergenceTrajectories.InitialForce over cohort entry year. The line slopes downward as struggle availability shrinks across cohorts. The ceiling drops not because people are less capable, but because the formative environment offers less resistance to grow against.

```spec
visualization CohortForceCeiling {
    page: DivergencePage;
    component: LineChart;

    parameters {
        curve: sweep(DivergenceTrajectories.InitialForce,
                     cohort, cohort_start, cohort_end,
                     forceMax, struggleAvailable, strugglePre, rho);
    }

    sliders: [forceMax, struggleAvailable, strugglePre, rho];
}
```

### Generational Step-Down

Multiple trajectory lines on the same time axis, each representing a different cohort. Every line starts at the InitialForce ceiling computed for its cohort, then evolves under the same dynamics. Earlier cohorts begin higher and reach higher peaks. Later cohorts begin lower and converge to lower steady states. Even with the same dynamics, later cohorts cannot reach the heights of earlier ones.

```spec
visualization GenerationalStepDown {
    page: DivergencePage;
    component: LineChart;

    parameters {
        cohorts: for_each(cohort, cohort_start, cohort_end, step,
                     Integration.ForwardEulerSystem(
                         [DivergenceTrajectories.DfHighDt,
                          DivergenceTrajectories.DfLowDt],
                         dt, t_max,
                         initial: DivergenceTrajectories.InitialForce(
                             cohort, forceMax, struggleAvailable,
                             strugglePre, rho)));
    }

    sliders: [alpha, S0, gamma, beta, M, kappa,
              forceMax, struggleAvailable, strugglePre, rho];
}
```

## Organizational Consequences

The organizational consequences page collects the equations that describe how firms respond to and are shaped by the force distribution. Return on investment diverges by force level: high-force engineers yield superlinear returns while low-force engineers yield diminishing returns, making investment allocation a high-stakes decision. Assessment signal-to-noise collapses as the multiplier grows, because the variance of output overwhelms the signal from individual force differences. Goodhart dynamics distort measured force once organizations optimize against observable proxies. Decision throughput and indecision cost capture the bottleneck that forms when organizations cannot evaluate options fast enough. Competitive advantage aggregates these effects into a single measure of organizational capability relative to rivals.

```spec
page OrganizationalPage {
    host: FStarEquations.App;
    route: "/organizational";
    concepts: [Eq17, Eq18, Eq19, Eq20, Eq21, Eq22];
    role: "Organizational response surface. Shows how firms
           allocate, assess, and compete under rising M.";
    cross_links: [VariancePage, NegativeForcePage,
                  ModelGrowthPage, CascadeDashboard];
}
```

### ROI by Force Level

A bar chart showing marginal return on investment for several force levels. The bars diverge: at high force, marginal returns are steep; at low force, returns flatten. The pattern makes the rational but destructive investment logic visible: organizations that optimize for ROI will concentrate resources on already-strong engineers.

```spec
visualization ROIByForceLevel {
    page: OrganizationalPage;
    component: BarChart;

    parameters {
        roi: OrganizationalDynamics.MarginalReturn(M, force,
             investmentLevel);
    }

    sliders: [M, force, investmentLevel];
}
```

### Assessment SNR Collapse

A line chart plotting assessment signal-to-noise ratio against M. As M increases, output variance grows faster than the signal from individual force differences, and the SNR curve drops toward the noise floor. The practical consequence is that performance reviews become unreliable precisely when accurate assessment matters most.

```spec
visualization AssessmentSNRCollapse {
    page: OrganizationalPage;
    component: LineChart;

    parameters {
        snr: OrganizationalDynamics.AssessmentSnr(M, varianceF,
             noiseMeasurement);
    }

    sliders: [M, varianceF, noiseMeasurement];
}
```

### Goodhart Gaming

A bar chart comparing true force and measured force across several individuals. As the gaming parameter increases, measured force diverges from true force, with the largest distortions appearing among individuals who have the most to gain from gaming the metric.

```spec
visualization GoodhartGaming {
    page: OrganizationalPage;
    component: BarChart;

    parameters {
        measuredForce: OrganizationalDynamics.MeasuredForce(
                       trueForce, gamingEffort, metricSensitivity);
    }

    sliders: [trueForce, gamingEffort, metricSensitivity];
}
```

### Decision Bottleneck

A line chart with two overlaid series: organizational throughput declining as decision load increases, and indecision cost rising as throughput falls. The crossover point marks where the cost of not deciding exceeds the cost of deciding badly.

```spec
visualization DecisionBottleneck {
    page: OrganizationalPage;
    component: LineChart;

    parameters {
        throughput: OrganizationalDynamics.OrganizationalThroughput(
                    decisionLoad, decisionCapacity);
        indecisionCost: OrganizationalDynamics.IndecisionCost(
                        throughput, costRate);
    }

    sliders: [decisionLoad, decisionCapacity, costRate];
}
```

### Competitive Advantage

A bar chart comparing competitive advantage scores across several organizations with different force distributions and multiplier levels. The bars make visible that advantage concentrates in organizations that combine high M with high mean force, while organizations with high M but low force are worse off than low-M organizations.

```spec
visualization CompetitiveAdvantage {
    page: OrganizationalPage;
    component: BarChart;

    parameters {
        advantage: OrganizationalDynamics.CompetitiveAdvantage(
                   M, meanForce, forceVariance, assessmentQuality);
    }

    sliders: [M, meanForce, forceVariance, assessmentQuality];
}
```

## The Meaning Problem

When M rises and the formative struggle that built force disappears, motivation decays. The motivation equation captures a feedback loop: lower motivation reduces engagement, which reduces the struggle exposure that sustains force, which further reduces motivation. The decay is self-reinforcing until it reaches a floor set by intrinsic interest.

```spec
page MotivationPage {
    host: FStarEquations.App;
    route: "/motivation";
    concepts: [Eq23];
    role: "Motivation feedback loop. Shows how meaning
           erosion becomes self-reinforcing.";
    cross_links: [ForceDynamicsPage, TippingPoint,
                  OrganizationalPage, CascadeDashboard];
}
```

### Motivation Decay Curve

A line chart plotting motivation over time as struggle availability declines. The curve shows the characteristic shape: a slow initial decline followed by an accelerating drop as the feedback loop engages, then a leveling off at the intrinsic floor.

```spec
visualization MotivationDecayCurve {
    page: MotivationPage;
    component: LineChart;

    parameters {
        motivation: Motivation.MotivationDecay(
                    struggleAvailable, intrinsicFloor,
                    feedbackStrength, t);
    }

    sliders: [struggleAvailable, intrinsicFloor,
              feedbackStrength];
}
```

### Motivation to Force Feedback

A dual-panel line chart. The top panel shows motivation over time; the bottom panel shows the resulting output trajectory. The connection between panels makes the feedback visible: as motivation drops, force growth slows, which reduces output, which further erodes meaning.

```spec
visualization MotivationToForceFeedback {
    page: MotivationPage;
    component: LineChart;

    parameters {
        motivation: Motivation.MotivationDecay(
                    struggleAvailable, intrinsicFloor,
                    feedbackStrength, t);
        output: BaseModel.ComputeOutput(M, force, weights);
    }

    sliders: [M, struggleAvailable, intrinsicFloor,
              feedbackStrength, force, weights];
}
```

## Sovereignty

National capability depends on the domestic stock of high-force engineers. When critical capability is accessed through foreign platforms rather than built domestically, sovereignty becomes contingent on continued access. The resilience test checks whether a nation can sustain capability above a critical threshold if access is revoked.

```spec
page SovereigntyPage {
    host: FStarEquations.App;
    route: "/sovereignty";
    concepts: [Eq24, Eq24a];
    role: "Sovereign capability assessment. Shows where
           national capability becomes access-dependent.";
    cross_links: [DivergencePage, TacitKnowledgePage,
                  ModelGrowthPage];
}
```

### National Capability Under Access Risk

A bar chart showing national capability decomposed into domestic and access-dependent components. Adjusting the access-risk slider reduces the access-dependent component, revealing the residual domestic capability. The gap between total capability and domestic-only capability is the sovereignty risk.

```spec
visualization NationalCapabilityUnderAccessRisk {
    page: SovereigntyPage;
    component: BarChart;

    parameters {
        capability: Sovereignty.NationalCapability(
                    domesticForce, accessDependentForce,
                    accessRisk);
    }

    sliders: [domesticForce, accessDependentForce, accessRisk];
}
```

### Sovereign Resilience Test

A bar chart with a threshold indicator showing whether the nation passes or fails the resilience test under current parameters. The indicator turns red when domestic capability falls below the critical threshold after access revocation.

```spec
visualization SovereignResilienceTest {
    page: SovereigntyPage;
    component: BarChart;

    parameters {
        resilient: Sovereignty.IsSovereignResilient(
                   domesticForce, criticalThreshold);
        indicator: ThresholdIndicator;
    }

    sliders: [domesticForce, accessDependentForce,
              criticalThreshold, accessRisk];
}
```

## Model Growth

The multiplier M is not static. It grows exponentially as models absorb more capability. This page shows how M(t) evolves over time and how that growth cascades through the other equations. The composite visualization connects M(t) to output variance, the tipping point, and indecision cost, revealing that exponential multiplier growth drives all four quantities simultaneously.

```spec
page ModelGrowthPage {
    host: FStarEquations.App;
    route: "/model-growth";
    concepts: [Eq25, Eq4, Eq14, Eq21];
    role: "Exponential multiplier dynamics. Shows how M(t)
           drives cascading consequences across the system.";
    cross_links: [VariancePage, TippingPoint,
                  OrganizationalPage, TransferPage,
                  CascadeDashboard, TerminalDynamics];
}
```

### Exponential Multiplier Growth

A line chart plotting M(t) over time. The exponential curve makes the acceleration visible: early growth appears modest, but the rate of increase itself increases, producing the characteristic hockey-stick shape.

```spec
visualization ExponentialMultiplierGrowth {
    page: ModelGrowthPage;
    component: LineChart;

    parameters {
        multiplier: ModelGrowth.MultiplierAtTime(M0, growthRate, t);
    }

    sliders: [M0, growthRate];
}
```

### M(t) Impact Cascade

A four-panel composite line chart. Each panel shares the same time axis. Panel one shows M(t). Panel two shows output variance driven by M(t). Panel three shows F*(t) rising as M(t) feeds into the tipping point equation. Panel four shows indecision cost growing as decision load scales with M(t). The visual alignment across panels makes the causal chain unmistakable.

```spec
visualization MtImpactCascade {
    page: ModelGrowthPage;
    component: LineChart;

    parameters {
        multiplier: ModelGrowth.MultiplierAtTime(M0, growthRate, t);
        variance: VarianceAmplification.OutputVariance(
                  multiplier, varianceF);
        threshold: ForceDynamics.TippingPoint(beta, R, sigma,
                   multiplier, gamma, E);
        indecisionCost: OrganizationalDynamics.IndecisionCost(
                        throughput, costRate);
    }

    sliders: [M0, growthRate, varianceF, beta, R, sigma,
              gamma, E, throughput, costRate];
}
```

## F-to-M Transfer

The transfer page covers the mechanism by which human force is extracted and absorbed into the model. Transfer rate depends on the layer being extracted from, with surface knowledge transferring easily and deep knowledge resisting extraction. Absorption has a ceiling set by model architecture and data quality. Time allocation captures the zero-sum competition between creating, evaluating, and extracting. Data quality and model quality form a feedback loop: better data improves the model, which raises M, which changes what counts as good data. The tipping point shift tracks whether F* has risen as a consequence of transfer.

```spec
page TransferPage {
    host: FStarEquations.App;
    route: "/transfer";
    concepts: [Eq26, Eq26a, Eq27, Eq28, Eq30, Eq31];
    role: "Transfer mechanics. Shows how force moves from
           humans to models and what breaks in the process.";
    cross_links: [CreationEvaluationPage, TippingPoint,
                  ModelGrowthPage, ForceDynamicsPage,
                  CascadeDashboard, TerminalDynamics];
}
```

### Transfer Rate by Layer

A bar chart showing transfer rate for each structural layer: surface, middle, and deep. Surface transfers are fast and cheap. Middle transfers are slower and require more structured extraction. Deep transfers are near-zero under normal conditions. The relative heights make the layer resistance hierarchy visible.

```spec
visualization TransferRateByLayer {
    page: TransferPage;
    component: BarChart;

    parameters {
        rate: ForceToModelTransfer.TransferRate(layer,
              extractionEffort, layerResistance);
    }

    sliders: [extractionEffort, layerResistance];
}
```

### Absorption Ceiling

A bar chart showing the absorption ceiling as a function of model architecture quality and data quality. The ceiling is the maximum rate at which the model can incorporate transferred knowledge. Improving architecture raises the ceiling; poor data quality lowers it regardless of architecture.

```spec
visualization AbsorptionCeiling {
    page: TransferPage;
    component: BarChart;

    parameters {
        ceiling: ForceToModelTransfer.AbsorptionCeiling(
                 architectureQuality, dataQuality);
    }

    sliders: [architectureQuality, dataQuality];
}
```

### Time Allocation Tradeoff

A bar chart showing the three-way split of a high-force individual's time among creation, evaluation, and extraction. The zero-sum constraint is visible: increasing extraction time directly reduces creation and evaluation capacity.

```spec
visualization TimeAllocationTradeoff {
    page: TransferPage;
    component: BarChart;

    parameters {
        allocation: ForceToModelTransfer.TotalTimeAllocation(
                    tauCreate, tauEval, tauExtract);
    }

    sliders: [tauCreate, tauEval, tauExtract];
}
```

### Data Quality Spiral

A line chart with two overlaid series: model quality over successive iterations and M(t) driven by that quality. The feedback loop is visible as the curves co-evolve: better model quality raises M, which changes the data landscape, which can either improve or degrade the next iteration's quality depending on whether the data pipeline adapts.

```spec
visualization DataQualitySpiral {
    page: TransferPage;
    component: LineChart;

    parameters {
        modelQuality: ForceToModelTransfer.ModelQualityNext(
                      currentQuality, dataQuality,
                      absorptionRate);
        multiplier: ModelGrowth.MultiplierAtTime(M0, growthRate, t);
    }

    sliders: [currentQuality, dataQuality, absorptionRate,
              M0, growthRate];
}
```

### Tipping Point Shift

A number line showing the current F* and the projected F* after transfer. A threshold indicator marks whether the tipping point has risen. The visualization makes the paradox concrete: the very act of transferring force into the model raises the bar that remaining humans must clear.

```spec
visualization TippingPointShift {
    page: TransferPage;
    component: NumberLine;

    parameters {
        hasRisen: ForceToModelTransfer.HasTippingPointRisen(
                  fStarBefore, fStarAfter);
        threshold: ForceDynamics.TippingPoint(beta, R, sigma,
                   M_abs, gamma, E);
    }

    sliders: [beta, R, sigma, M_abs, gamma, E,
              fStarBefore, fStarAfter];
}
```

## Home

The home page is the navigation hub of the harness. It presents a card for every section page and dashboard, organized by the conceptual flow of the paper. The Tipping Point card is visually emphasized as the central concept. No equations are computed on this page; it exists solely to orient the user and provide entry points into the interactive sections.

```spec
page HomePage {
    host: FStarEquations.App;
    route: "/";
    concepts: [];
    role: "Navigation hub. Cards for every section page
           and dashboard.";
    cross_links: [BaseModelPage, VariancePage,
                  CreationEvaluationPage, NegativeForcePage,
                  ForceDynamicsPage, TacitKnowledgePage,
                  TippingPoint, DivergencePage,
                  OrganizationalPage, MotivationPage,
                  SovereigntyPage, ModelGrowthPage,
                  TransferPage, CascadeDashboard,
                  TerminalDynamics, TimelineDashboard];
}
```

## The Cascade

The cascade dashboard is an interactive flow diagram showing how equations feed into each other across the entire system. Each node represents a key quantity (M, F, F*, K_tacit, output variance, motivation, etc.) and is rendered as a live indicator showing its current value. Edges are labeled with equation numbers. Clicking any node navigates to the corresponding section page. The layout follows the causal flow of the paper: M(t) at the top, force dynamics and tacit knowledge in the middle, organizational consequences and divergence at the bottom.

```spec
page CascadeDashboard {
    host: FStarEquations.App;
    route: "/cascade";
    concepts: [Eq1, Eq4, Eq6, Eq7, Eq10, Eq11, Eq12, Eq14,
               Eq23, Eq26, Eq31, Eq32];
    role: "System-level flow diagram. Shows how equations
           connect across the entire framework.";
    cross_links: [BaseModelPage, VariancePage,
                  CreationEvaluationPage, NegativeForcePage,
                  ForceDynamicsPage, TacitKnowledgePage,
                  TippingPoint, DivergencePage,
                  OrganizationalPage, MotivationPage,
                  TransferPage, ModelGrowthPage];
}
```

### Cascade Flow Diagram

The primary visualization is a directed graph rendered as hand-coded inline SVG within CascadeDashboard.razor. Nodes are live indicators: each displays its current computed value with per-node color coding assigned at construction time. Edges show equation numbers and direction of influence. The layout is an elliptical loop of 12 nodes at fixed positions (SVG viewBox 0 0 1000 720) with a single feedback edge from Eq 32 back to Eq 11. Clicking a node navigates to the section page where that equation is explored in detail.

```spec
visualization CascadeFlowDiagram {
    page: CascadeDashboard;
    component: InlineSvg;

    parameters {
        nodes: [
            node(M, ModelGrowth.MultiplierAtTime),
            node(F, ForceDynamics.DfDt),
            node(F_star, ForceDynamics.TippingPoint),
            node(K_tacit, TacitKnowledge.KnowledgeStockNext),
            node(Var_O, VarianceAmplification.OutputVariance),
            node(Motivation, Motivation.MotivationDecay),
            node(Transfer, ForceToModelTransfer.TransferRate),
            node(EpistemicGap, NegativeForce.EpistemicGap),
            node(Output, BaseModel.ComputeOutput),
            node(MarketValue, VarianceAmplification.MarketValue),
            node(Throughput, CreationEvaluation.EvaluationThroughput),
            node(Gap, DivergenceTrajectories.InitialForce)
        ];
        edges: [
            edge(Output, Var_O, "Eq(1)->Eq(4)"),
            edge(Var_O, MarketValue, "Eq(4)->Eq(6)"),
            edge(MarketValue, Throughput, "Eq(6)->Eq(7)"),
            edge(Throughput, EpistemicGap, "Eq(7)->Eq(10)"),
            edge(EpistemicGap, F, "Eq(10)->Eq(11)"),
            edge(F, K_tacit, "Eq(11)->Eq(12)"),
            edge(K_tacit, F_star, "Eq(12)->Eq(14)"),
            edge(F_star, Motivation, "Eq(14)->Eq(23)"),
            edge(Motivation, Transfer, "Eq(23)->Eq(26)"),
            edge(Transfer, M, "Eq(26)->Eq(31)"),
            edge(M, Gap, "Eq(31)->Eq(32)"),
            edge(Gap, F, "Eq(32)->Eq(11)")
        ];
    }

    sliders: [M, F, E, R, S, K];

    rationale {
        context "The cascade flow diagram is the only directed-graph
                 visualization in the system. Its layout is an
                 elliptical loop of 12 nodes with a single feedback
                 edge, specific to the cascade narrative.";
        decision "Hand-coded inline SVG in CascadeDashboard.razor
                  rather than a reusable FlowDiagram component.
                  The layout, edge routing, and node content are
                  tightly coupled to the cascade domain.";
        consequence "If a second flow-diagram page is added, the
                     FutureWorkExtraction constraint requires
                     extracting a reusable component at that time.";
    }
}
```

## Terminal Dynamics

The terminal dynamics dashboard is an animated phase-plane visualization showing the co-evolution of F and M over time. The phase portrait plots F on one axis and M on the other, with trajectories traced as animated curves. Three preset scenarios illustrate the qualitatively different outcomes: virtuous cycle (high initial F, moderate M growth, trajectory spirals outward), managed decline (moderate initial F, high M growth, trajectory curves downward but stabilizes), and collapse (low initial F, high M growth, trajectory drops to the floor). The user can also set custom initial conditions and growth parameters.

```spec
page TerminalDynamics {
    host: FStarEquations.App;
    route: "/terminal-dynamics";
    concepts: [Eq11, Eq25, Eq31];
    role: "Animated phase-plane. Shows the three qualitative
           endgames: virtuous cycle, managed decline, collapse.";
    cross_links: [ForceDynamicsPage, ModelGrowthPage,
                  TransferPage, CascadeDashboard,
                  TimelineDashboard];
}
```

### Phase Plane Animator

The primary visualization is an animated phase portrait. F is plotted on the vertical axis and M on the horizontal axis. The current state is a dot that moves along its trajectory as time advances. The F* threshold is drawn as a curve across the phase plane, dividing it into growth and decay regions. Three preset trajectories are available: Virtuous (F stays above F* as M grows), Managed Decline (F drops below F* but stabilizes at a floor), and Collapse (F drops rapidly to near-zero as M accelerates). Custom initial conditions allow exploration beyond the presets.

```spec
visualization PhasePlaneAnimator {
    page: TerminalDynamics;
    component: PhasePortrait;

    parameters {
        system: Integration.ForwardEulerSystem(
                    [ForceDynamics.DfDt,
                     ModelGrowth.MultiplierAtTime,
                     ForceToModelTransfer.TransferRate],
                    dt, t_max);
        threshold: ForceDynamics.TippingPoint(beta, R, sigma,
                   M_abs, gamma, E);
        presets: [Virtuous, ManagedDecline, Collapse];
    }

    sliders: [alpha, S, gamma, E, beta, R, sigma,
              M0, growthRate, F_initial, dt];
}
```

## Time Series Player

A standalone demo page that animates output trajectories under different force levels using TimeSeriesAnimator with playback controls. The demonstration formula O = M * F * (1 - exp(-t/Tau)) is not a specific framework equation; it exists to show how TimeSeriesAnimator renders animated pre-computed trajectories with play/pause and current-time scrubber controls.

```spec
page TimeSeriesPlayerPage {
    host: FStarEquations.App;
    route: "/time-series-player";
    role: "Standalone demo page that animates output trajectories
          under different force levels using TimeSeriesAnimator
          with playback controls. Uses a demonstration formula
          O = M * F * (1 - exp(-t/Tau)), not a specific framework
          equation.";
    cross_links: [ForceDynamicsPage, DivergencePage, VariancePage];
}
```

### Output Trajectory Player

Three pre-computed force-level trajectories animated on a shared time axis. Each series uses the demonstration formula O = M * F * (1 - exp(-t/Tau)) with a different F value. The player provides play/pause and a current-time scrubber. Adjusting M or Tau re-computes all three trajectories.

```spec
visualization OutputTrajectoryPlayer {
    page: TimeSeriesPlayerPage;
    component: TimeSeriesAnimator;

    parameters {
        series: [
            AnimatedSeries("F = 2", trajectory(2.0, M, Tau), "#ef4444"),
            AnimatedSeries("F = 5", trajectory(5.0, M, Tau), "#f59e0b"),
            AnimatedSeries("F = 8", trajectory(8.0, M, Tau), "#22c55e")
        ];
    }

    sliders: [M, Tau];
}
```

## The Full Timeline

The full timeline dashboard stacks seven line chart panels on a shared 20-year time axis, providing a comprehensive view of how every major quantity in the system evolves simultaneously. Each panel shows one key quantity, and all panels scroll and zoom together. The vertical alignment makes correlations and causal delays visible: when M(t) accelerates in the top panel, the downstream effects propagate through each successive panel with characteristic delays.

```spec
page TimelineDashboard {
    host: FStarEquations.App;
    route: "/timeline";
    concepts: [Eq11, Eq11a, Eq11b, Eq11c, Eq12, Eq14,
               Eq18, Eq25, Eq31];
    role: "Seven-panel synchronized timeline. The complete
           20-year trajectory of the system.";
    cross_links: [ForceDynamicsPage, TacitKnowledgePage,
                  TippingPoint, ModelGrowthPage,
                  OrganizationalPage, TransferPage,
                  CascadeDashboard, TerminalDynamics];
}
```

### Synchronized Timeline

Seven stacked line chart panels sharing a common time axis spanning 20 years. Panel one: M(t), the multiplier trajectory. Panel two: F_total(t), aggregate force. Panel three: f_surface(t), f_middle(t), f_deep(t) as three lines on a shared axis. Panel four: K_tacit(t), tacit knowledge stock. Panel five: Var(O)(t), output variance. Panel six: F*(t), the tipping point threshold. Panel seven: SNR(t), assessment signal-to-noise ratio. All panels scroll and zoom in lockstep, so temporal correlations are immediately visible.

```spec
visualization SynchronizedTimeline {
    page: TimelineDashboard;
    component: LineChart;

    parameters {
        multiplier: ModelGrowth.MultiplierAtTime(M0, growthRate, t);
        forceTotal: Integration.ForwardEuler(
                        ForceDynamics.DfDt, dt, t_max);
        forceLayers: Integration.ForwardEulerSystem(
                         [ForceDynamics.DfSurfaceDt,
                          ForceDynamics.DfMiddleDt,
                          ForceDynamics.DfDeepDt],
                         dt, t_max);
        knowledge: Integration.ForwardEuler(
                       TacitKnowledge.KnowledgeStockNext, dt, t_max);
        variance: VarianceAmplification.OutputVariance(
                  multiplier, varianceF);
        threshold: ForceDynamics.TippingPoint(beta, R, sigma,
                   multiplier, gamma, E);
        snr: OrganizationalDynamics.AssessmentSnr(multiplier,
             varianceF, noiseMeasurement);
    }

    sliders: [M0, growthRate, alpha, beta_s, beta_m, beta_d,
              gamma_m, S, E, R, sigma, delta, phi, W0, psi,
              varianceF, noiseMeasurement, dt];
}
```
