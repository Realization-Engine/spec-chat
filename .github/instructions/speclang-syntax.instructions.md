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

## Systems Specification

### System (root node)
```spec
system Name {
    target: "platform";
    responsibility: "what the system does";
}
```

### Authored Component (we build this)
```spec
authored component Name {
    kind: library | application | tests;
    path: "src/path";
    status: new | existing;
    responsibility: "what it does";
    contract { ... }
}
```

### Consumed Component (external dependency)
```spec
consumed component Name {
    source: nuget("PackageName") | npm("package") | crate("name");
    version: "constraint";
    responsibility: "what we use it for";
    used_by: [ComponentA, ComponentB];
    contract { ... }
}
```

### Topology
```spec
topology {
    allow App -> Engine;
    deny UI -> Engine;    // no direct coupling
}
```

### Phases with Gates
```spec
phase "Phase Name" {
    depends_on: ["Previous Phase"];
    gate {
        command: "dotnet test";
        expected: "all pass";
    }
}
```

### Traces
```spec
trace DomainConcept -> [ComponentA, ComponentB, TestC];
```
Many-to-many mapping from domain concepts to implementation artifacts.

### Constraints (system-level)
```spec
constraint "No third-party charting libraries";
constraint "CSS in two locations only";
```

### Package Policy
```spec
package_policy {
    pre_approved: [nuget("Microsoft.*")];
    prohibited: [nuget("Newtonsoft.Json")];
    requires_justification: [nuget("*")];
}
```

### Platform Realization (.NET example)
```spec
dotnet solution {
    name: "MySolution";
    format: "slnx";
    projects: [Engine, App, Tests];
    startup: App;
}
```

## Design Specification

### Page
```spec
page Name {
    route: "/path";
    component: ComponentName;
    visualizations: [Viz1, Viz2];
}
```

### Visualization
```spec
visualization Name {
    type: line_chart | bar_chart | scatter | heatmap;
    data_source: ComponentName.Method;
    parameters: { x: field, y: field };
}
```

## Expression Syntax

Operators: `implies`, `and`, `or`, `not`, `==`, `!=`, `>`, `<`, `>=`, `<=`
Functions: `count(collection)`, `contains`, `excludes`, `in`
Quantifiers: `all x in collection: predicate`, `exists x in collection: predicate`
