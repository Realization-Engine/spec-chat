# Standard Test -- Architecture Extension Test

Spec with `architecture TheStandard` declaration for validating
that The Standard extension rules activate correctly.

```spec
architecture TheStandard {
    version: "1.0";
    enforce: [layers, flow_forward, florance,
              entity_ownership, autonomy, vocabulary];

    vocabulary {
        broker:      [Insert, Select, Update, Delete];
        foundation:  [Add, Retrieve, Modify, Remove];
        processing:  [Ensure, Upsert, Verify, TryRemove];
        exposer:     [Post, Get, Put, Delete];
    }

    rationale {
        context "Test system following The Standard architecture.";
        decision "All authored components declare their layer.";
        consequence "Semantic analysis enforces layer-specific rules.";
    }
}
```

```spec
system StandardTestSystem {
    target: "net10.0";
    responsibility: "Test system for Standard extension validation.";

    authored component TestBroker {
        kind: library;
        path: "src/Brokers/TestBroker";
        responsibility: "Wraps data storage. No flow control.";
    }

    authored component TestFoundation {
        kind: library;
        path: "src/Services/TestFoundation";
        responsibility: "Foundation service for TestEntity.";
    }
}
```

```spec
entity TestEntity {
    id: int;
    name: string;

    invariant "name not empty": name != "";
}
```
