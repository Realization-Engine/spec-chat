# Broken Topology -- Negative Test

Spec with topology referencing undeclared components and contradictory rules.

```spec
system BrokenSystem {
    target: "net10.0";
    responsibility: "Test system for negative topology validation.";

    authored component Alpha {
        kind: library;
        path: "src/Alpha";
        responsibility: "First component.";
    }

    authored component Beta {
        kind: library;
        path: "src/Beta";
        responsibility: "Second component.";
    }
}
```

```spec
topology BrokenTopology {
    allow Alpha -> Beta;
    allow Alpha -> Gamma;
    deny Alpha -> Beta;

    rationale "Gamma does not exist and Alpha->Beta is both allowed and denied.";
}
```
