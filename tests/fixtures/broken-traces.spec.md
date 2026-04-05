# Broken Traces -- Negative Test

Spec with duplicate trace sources and empty target lists.

```spec
system TraceTestSystem {
    target: "net10.0";
    responsibility: "Test system for negative trace validation.";

    authored component AppComponent {
        kind: application;
        path: "src/App";
        responsibility: "Main app.";
    }
}
```

```spec
trace BrokenTrace {
    Concept1 -> [PageA, PageB];
    Concept2 -> [];
    Concept1 -> [PageC];

    invariant "every concept has at least one page":
        all sources have count(targets) >= 1;
}
```
