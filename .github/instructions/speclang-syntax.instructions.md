---
description: 'SpecLang syntax reference for .spec.md files'
applyTo: '**/*.spec.md'
---

# SpecLang Construct Reference

## Data Specification

### Entity
```spec
entity Name {
    field: Type;
    optionalField: Type?;
    listField: Type[];
}
```

### Enum
```spec
enum Name {
    value1: "description",
    value2: "description"
}
```

### Field Annotations
- `@confidence(high | medium | low)` -- expected extraction reliability
- `@default(value)` -- default value when not specified
- `@range(min..max)` -- numeric range constraint
- `@pattern("regex")` -- regex validation for strings

### Invariant
```spec
invariant "descriptive name": expression;
```
Declared inside an entity or at system level. Cross-field or cross-entity constraint.

### Contract
```spec
contract Name {
    requires expression;
    ensures expression;
    guarantees "prose commitment";
}
```
Contracts attach to components or stand alone. `requires` = preconditions, `ensures` = postconditions, `guarantees` = prose obligations.

### Rationale (simple)
```spec
rationale "Brief explanation of the design choice.";
```

### Rationale (structured, micro-ADR)
```spec
rationale "fieldOrEntityName" {
    context "What situation prompted this choice.";
    decision "What was decided.";
    consequence "What follows from this decision.";
    supersedes "What previous design this replaces.";  // optional
}
```

### Refinement
```spec
refines BaseEntity as RefinedEntity {
    additionalField: Type;
    invariant "refinement rule": expression;
}
```

## Context Specification

### Person (human user or actor)
```spec
person Name {
    description: "Who this person is and what they do.";
    @tag("stakeholder", "primary-user");
}
```

### External System (runtime peer outside our boundary)
```spec
external system Name {
    description: "What this system does.";
    technology: "REST/HTTPS";
    @tag("external", "financial");
    rationale "Why we integrate with this system.";
}
```
External systems are runtime peers, not build-time packages. Use `consumed component` for build dependencies.

### Relationship (labeled directional edge)
```spec
// Short form:
Source -> Target : "What is communicated.";

// Block form:
Source -> Target {
    description: "What is communicated.";
    technology: "REST/HTTPS";
}
```
Connects persons, systems, and external systems. Every relationship carries a description and optionally a technology.

### Tag Annotation (general-purpose classification)
```spec
@tag("category1", "category2");
```
Attachable to any declaration. Used by view filters to include/exclude elements by classification.

## Systems Specification

### System (root node)
```spec
system Name {
    target: "net10.0";
    responsibility: "What the system does.";
}
```

### Authored Component (we build this)
```spec
authored component Name {
    kind: library | application | tests;
    path: "src/path";
    status: new | existing;
    responsibility: "What it does.";
    contract { ... }
}
```

### Consumed Component (external dependency)
```spec
consumed component Name {
    source: nuget("PackageName") | npm("package") | container("image");
    version: "constraint";
    responsibility: "What we use it for.";
    used_by: [ComponentA, ComponentB];
    contract { ... }
}
```

### Topology
```spec
topology Name {
    allow App -> Engine;
    deny  UI -> Engine;

    // Enriched edges with technology and description:
    allow App -> PaymentGateway {
        technology: "REST/HTTPS";
        description: "Fetches transaction history.";
    };

    invariant "rule name": expression;

    rationale "deny UI -> Engine" {
        context "...";
        decision "...";
        consequence "...";
    }
}
```

### Phases with Gates
```spec
phase PhaseName {
    requires: PreviousPhase;
    produces: [ComponentA, ComponentB];

    gate gateName {
        command: "dotnet test Solution.slnx";
        expects: pass >= 80, fail == 0;
    }
}
```

### Traces
```spec
trace Name {
    Revenue -> [RevenuePage, ExecutiveDashboard];
    Retention -> [RetentionPage, CohortPage];

    invariant "every metric has a page":
        all sources have count(targets) >= 1;
}
```
Many-to-many mapping from domain concepts to implementation artifacts.

### Constraints (system-level)
```spec
constraint Name {
    scope: all authored components;
    rule: "The rule that must hold.";
    rationale "Why this constraint exists.";
}
```

### Package Policy
```spec
package_policy Name {
    source: nuget("https://api.nuget.org/v3/index.json");

    deny category("charting")
        includes ["Plotly.Blazor", "ChartJs.Blazor"];

    allow category("testing")
        includes ["xunit", "bunit"];

    default: require_rationale;
}
```

### Platform Realization (.NET example)
```spec
dotnet solution Name {
    format: slnx;
    startup: App;

    folder "src" {
        projects: [Engine, UI, App];
    }

    folder "tests" {
        projects: [App.Tests];
    }
}
```

## Deployment Specification

### Deployment Environment
```spec
deployment Production {
    node "Azure App Service" {
        technology: "Linux/P1v3";
        instance: App;

        node "Child Node" {
            technology: "WebAssembly";
            instance: ClientApp;
        }
    }

    node "Azure SQL" {
        technology: "SQL Database S3";
        instance: Database;
        @tag("infrastructure");
    }
}
```
Nodes nest to model infrastructure hierarchy. `instance` references authored components from the system tree.

## View Specification

### View (architectural diagram projection)
```spec
view systemContext of SystemName ViewName {
    include: all;
    autoLayout: top-down;
    description: "The system and its connections.";
}

view container of SystemName ContainerViewName {
    include: all;
    exclude: tagged "internal-only";
    autoLayout: left-right;
}

view deployment of Production DeploymentViewName {
    include: all;
    autoLayout: top-down;
}
```

View kinds: `systemLandscape`, `systemContext`, `container`, `component`, `deployment`.

Include/exclude filters: `all`, `tagged "tagname"`, `[Element1, Element2]`.

Layout directions: `top-down`, `left-right`, `bottom-up`, `right-left`.

## Dynamic Specification

### Dynamic (behavioral interaction sequence)
```spec
dynamic ScenarioName {
    1: User -> App : "Opens dashboard.";
    2: App -> Gateway {
        description: "Fetches data.";
        technology: "REST/HTTPS";
    };
    3: Gateway -> App : "Returns results.";
    4: App -> User : "Renders visualization.";
}
```
Numbered steps show how elements collaborate for a specific use case. References persons, systems, external systems, and components.

## Design Specification

### Page
```spec
page Name {
    host: AuthoredComponent;
    route: "/path";
    concepts: [DomainConcept1, DomainConcept2];
    role: "What this page communicates.";
    cross_links: [OtherPage1, OtherPage2];
}
```

### Visualization
```spec
visualization Name {
    page: PageName;
    component: ChartComponent;

    parameters {
        paramName: Domain.Method(arg1, arg2);
    }

    sliders: [arg1, arg2];
}
```

## Expression Syntax

Operators (lowest to highest precedence): `implies`, `or`, `and`, `not`, `==`, `!=`, `>`, `<`, `>=`, `<=`, `contains`, `excludes`, `in`

Member access: `object.field`, `object.method(args)`

Quantifiers: `all <scope> have <predicate>`, `all <scope> satisfy <predicate>`, `exists <scope> satisfy <predicate>`

Functions: `count(collection)`

List literals: `[item1, item2, item3]`
