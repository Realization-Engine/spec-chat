# SpecLang Extension: The Standard Architecture

**Version:** 0.1
**Date:** 2026-04-04
**Companion to:** TheStandard-Extension-Grammar.md
**Extends:** SpecLang-Specification.md, SpecLang-Grammar.md

---

## Purpose

This document defines an opt-in extension to SpecLang that allows specifications to encode the architectural rules of The Standard (Hassan Habib). The Standard prescribes a specific internal code structure for systems: Brokers (dependencies), Services at multiple layers (Foundation, Processing, Orchestration, Coordination, Aggregation), and Exposers (exposure). It governs these through rules including the Florance Pattern, Flow Forward, single-entity integration, autonomous components, and a rigid naming discipline across layers.

SpecChat's base language can already express *what* a system's components are, their topology, their contracts, and their phased construction. It cannot express *how* the internal code must be layered according to a named architectural standard. This extension closes that gap for systems targeting The Standard, without altering the base language for systems that do not.

The extension is activated by a top-level `architecture` declaration. When present, semantic analysis enforces additional rules derived from The Standard. When absent, all constructs introduced here are unavailable and the base language operates unchanged.

---

## What the Base Language Already Carries

Before introducing new constructs, it is worth identifying where SpecChat's existing constructs already encode Standard-compatible rules. The extension builds on these rather than duplicating them.

### Consumed components and the Broker layer

The Standard defines Brokers as disposable wrappers around external resources: databases, APIs, queues, file systems. They implement local interfaces, contain no business logic, no flow control, and no exception handling. They own their configurations and speak the language of the technology they wrap.

In The Standard's architecture, a Broker has two parts: the external resource itself (a NuGet package, an npm module, a database engine) and the authored wrapper class that abstracts it behind a local interface (e.g., `StorageBroker` wrapping Entity Framework, `DateTimeBroker` wrapping the system clock).

SpecChat's base language already represents the external resource side through `consumed component` declarations:

| External Resource Aspect | SpecChat Construct |
|---|---|
| Package identity | `source: nuget("...")` or `npm("...")` or `crate("...")` |
| Version constraint | `version: "10.*"` |
| Boundary contract (what we expect from it) | `contract { guarantees "..."; }` |
| Which authored components use it | `used_by: [...]` |
| Rationale for the choice | `rationale { context ...; decision ...; }` |

What the base language does *not* represent is the authored Broker wrapper. This extension closes that gap with the `broker` declaration keyword, which is one of the Standard-vocabulary declaration forms introduced in section 2 below. When the extension is active, the authored wrapper is declared as:

```spec
broker StorageBroker {
    kind: library;
    path: "src/Brokers/Storage";
    responsibility: "Wraps Entity Framework. Partial classes per entity.
                     No flow control, no exception handling.";
}
```

The `broker` keyword, along with `foundation service`, `processing service`, `orchestration service`, `coordination service`, `aggregation service`, `exposer`, and `test`, are first-class declaration forms that bring The Standard's full vocabulary into SpecLang. All Standard-specific keywords are enabled by the extension; they are not available in the base language.

Consumed components remain implicitly at the `broker` layer. They do not receive an explicit `layer` property because their disposition (consumed, not authored) already determines their architectural role. Semantic analysis treats all consumed components as broker-layer when evaluating topology rules, Florance counts, and Flow Forward constraints. The distinction is: consumed components declare the external resource; `broker` declarations declare the authored wrapper around it.

### Topology encodes dependency direction

The Standard's "Up & Sideways" rule says Brokers cannot call Services, and Services cannot call Brokers in the reverse direction. The "Flow Forward" rule says services at the same level cannot call each other. Both are dependency direction constraints.

SpecChat's `topology` with `allow`/`deny` rules encodes these constraints directly. When an `architecture` declaration is present, semantic analysis interprets topology rules through the lens of The Standard's layer hierarchy and flags violations that would otherwise pass as valid topology.

### Package policies encode dependency governance

The Standard mandates that consumed dependencies be explicitly justified. SpecChat's `package_policy` with `default: require_rationale` already enforces this: any package not in an allow category requires a `consumed component` declaration with rationale.

### Phase gates encode progressive validation

The Standard requires test-driven development with validation at every stage. SpecChat's `phase` declarations with `gate` blocks and `expects` clauses encode progressive validation obligations. The extension does not alter phases but does add a semantic rule: when an architecture is declared, every phase must have at least one gate with a test expectation.

---

## New Constructs

### 1. Architecture Declaration

The `architecture` declaration is a new top-level construct that activates Standard-specific semantic rules for the entire spec collection. It names the architectural standard, optionally specifies a version, and declares which rule sets are enforced.

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
        context "The system follows Hassan Habib's Standard for
                 software architecture: Tri-Nature decomposition
                 into Brokers, Services, and Exposers with fractal
                 layering.";
        decision "All authored components declare their layer.
                  Semantic analysis enforces layer-specific rules.";
        consequence "LLM realization generates Standard-compliant
                     code: Foundation services with validation,
                     Processing services with higher-order logic,
                     Orchestration services combining entity flows.";
    }
}
```

**Semantics:**

- There may be at most one `architecture` declaration per spec collection.
- The `version` property is a STRING carrying the Standard's version. Optional; defaults to `"1.0"`.
- The `enforce` property is a list of rule-set identifiers. Each identifier activates a specific set of semantic validation rules. The recognized identifiers are:

| Identifier | Rule Set |
|---|---|
| `layers` | Every authored component must declare a `layer` property. Layer values must be from the recognized set. |
| `flow_forward` | Services at the same layer cannot depend on each other. Topology `allow` rules that violate this are flagged. |
| `florance` | Orchestration-layer components must have 2 or 3 service-layer dependencies (not counting utility brokers). |
| `entity_ownership` | Foundation-layer components must declare an `owns` property naming exactly one entity. |
| `autonomy` | No authored component may be referenced in `used_by` by more than one peer at the same layer, unless it is at a strictly lower layer. Shared libraries consumed by peer components at the same layer are flagged as horizontal entanglement. |
| `vocabulary` | Operation names in contracts and guarantees are checked against the vocabulary mapping for the component's layer. Mismatches produce diagnostics (warnings, not errors). |

If `enforce` is omitted, all rule sets are active. This is the All-In default, consistent with The Standard's "All-In/All-Out" principle (sec. 0.2.8). To selectively disable a rule set during incremental adoption, the specifier lists only the desired identifiers.

### 2. Layer-Prefixed Declarations (Standard Vocabulary)

When an `architecture` declaration is active, authored components can be declared using The Standard's own vocabulary instead of the generic `authored component` form. Each declaration form implies a specific layer and uses the noun that Standard practitioners expect.

| Declaration Form | Implied Layer | Standard Noun |
|---|---|---|
| `broker Name { ... }` | broker | broker |
| `foundation service Name { ... }` | foundation | service |
| `processing service Name { ... }` | processing | service |
| `orchestration service Name { ... }` | orchestration | service |
| `coordination service Name { ... }` | coordination | service |
| `aggregation service Name { ... }` | aggregation | service |
| `exposer Name { ... }` | exposer | exposer |
| `test Name { ... }` | test | (standalone) |

The body of each form accepts the same properties as `authored component`, minus the `layer` property (which is implied by the declaration keyword). The `kind`, `path`, `status`, `owns`, `responsibility`, `@suppress`, inline contracts, and rationale properties all work identically.

```spec
foundation service StudentService {
    kind: library;
    path: "src/Services/Foundations/Students";
    owns: Student;
    responsibility: "Validation and primitive CRUD operations for
                     Student entities. Single entity integration
                     with the StorageBroker.";
}
```

This is equivalent to:

```spec
authored component StudentService {
    kind: library;
    path: "src/Services/Foundations/Students";
    layer: foundation;
    owns: Student;
    responsibility: "Validation and primitive CRUD operations for
                     Student entities. Single entity integration
                     with the StorageBroker.";
}
```

**Both forms are valid.** The layer-prefixed form is preferred when an architecture declaration is active because it uses The Standard's vocabulary directly. The generic `authored component` with an explicit `layer` property remains available for cases where the specifier prefers uniformity or where the architecture declaration is absent.

**Semantic equivalence:** The parser desugars every layer-prefixed declaration into an `authored component` with the corresponding `layer` property set. All semantic rules (Flow Forward, Florance, entity ownership, autonomy) apply identically regardless of which declaration form was used.

### Layer Values and Hierarchy

| Layer | Standard Role | Allowed Dependencies |
|---|---|---|
| `broker` | Integration with external resources | External resources only (consumed components). No service dependencies. |
| `foundation` | Broker-neighboring service. Validation + primitive operations. | Exactly one entity broker, plus any number of support brokers (DateTime, Logging). |
| `processing` | Higher-order single-entity logic. | Exactly one foundation service, plus support brokers. |
| `orchestration` | Multi-entity combination logic. | 2-3 processing or foundation services (Florance Pattern), plus support brokers. |
| `coordination` | Orchestrator of orchestrators. | 2-3 orchestration services, plus support brokers. |
| `aggregation` | Exposure gateway. Ties multiple services to one exposure point. | Any number of services at the same variation level. No dependency count limit. |
| `exposer` | API controller, UI component, or other exposure technology. | Exactly one service (aggregation, orchestration, or foundation). For multi-page applications, the exposer depends on an aggregation service that bundles multiple orchestrations; alternatively, multiple exposer components each depend on their own service. |
| `test` | Test project. Exempt from layer hierarchy, Flow Forward, and Florance rules. | Any number of dependencies at any layer. Tests must reach across all layers to verify behavior. |

**Semantic rules when `layers` is enforced:**

1. Every authored component must have a layer, either through a layer-prefixed declaration form (`broker`, `foundation service`, etc.) or through an explicit `layer` property on a generic `authored component`.
2. The layer value must be one of the recognized values above.
3. Topology `allow` rules are checked against the layer hierarchy. A component at a given layer may only depend on components at the same or lower layer, never higher. "Lower" follows the ordering: broker < foundation < processing < orchestration < coordination < aggregation < exposer.
4. The `kind` property is independent of `layer`. A foundation service is still `kind: library` in platform terms. The `layer` property adds architectural semantics on top of the platform kind.
5. Components at the `test` layer are exempt from rules 3 (layer hierarchy), the Flow Forward rule set, and the Florance Pattern rule set. Tests must depend on components at every layer to verify their behavior; restricting test dependencies would make the rules unimplementable. The `autonomy` rule set also does not apply to test-layer components.

### 3. Entity Ownership

A new property `owns` is added to foundation-layer components. It declares which entity a Foundation service is responsible for, encoding The Standard's single-entity integration rule.

```spec
foundation service OrderService {
    kind: library;
    path: "src/Services/Foundations/Orders";
    owns: Order;
    responsibility: "Validation and primitive CRUD for Order entities.";
}
```

**Semantics:**

- `owns` takes a single `DOTTED_IDENT` naming an entity declared elsewhere in the spec.
- When `entity_ownership` is enforced, every `foundation`-layer component must have exactly one `owns` property.
- Two foundation-layer components cannot own the same entity. If `Order` needs multiple foundation services (e.g., one for storage, one for a queue), the spec must distinguish them by entity or by introducing a more specific entity type.
- Processing-layer components inherit entity affinity from their single foundation dependency. The `owns` property is optional on processing-layer components; if present, it must match the entity owned by their foundation dependency.
- Orchestration-layer and above do not use `owns`. They combine multiple entity flows by definition.

### 4. Autonomy Constraint

The Standard prohibits horizontal entanglement: no `Utils`, `Commons`, or shared libraries that create coupling between peer components. Every component must be self-sufficient, implementing its own validations and tooling.

When the `autonomy` rule set is enforced, a new built-in constraint activates:

**Rule:** An authored component at layer L may be referenced in `used_by` or topology `allow` rules by components at layer L or higher. However, if two or more components at the *same* layer both depend on a single authored component at that same layer, this is flagged as horizontal entanglement.

**What this catches:**

Consider the existing PizzApp spec. `PizzaOrders.UI` is an authored library. The topology allows both `PizzaOrders` (the host app) and `PizzaOrdersTests` (tests) to depend on it. If the spec were placed under an architecture declaration and both `PizzaOrders` and `PizzaOrders.UI` were assigned the same layer (e.g., both at `foundation`), the autonomy rule would flag it: two foundation-layer components depending on a third foundation-layer component is horizontal entanglement. If instead `PizzaOrders` is an `exposer` and `PizzaOrders.UI` is a `broker` (the Base Component pattern), the layers differ and the autonomy rule does not fire. The diagnostic depends on the layer assignment the specifier chooses. The Standard-compliant classification for a shared UI wrapper library is `broker` (analogous to The Standard's Base Components, sec. 3.2.1.2.0.0).

**What this does not catch:**

- Consumed components (external packages) shared across multiple authored components. This is permitted; The Standard's autonomy rule applies to authored code, not external dependencies.
- Components at different layers depending on the same lower-layer component. A foundation service used by multiple processing services is normal upward dependency, not entanglement.

**Explicit suppression:**

If the specifier intentionally violates a rule set (e.g., a shared base-component library justified by The Standard's own UI Bases pattern from sec. 3.2.1.2.0.0), they can suppress specific diagnostics with a `@suppress` annotation on the authored component:

```spec
authored component PizzaOrders.UI {
    kind: library;
    path: "src/PizzaOrders.UI";
    layer: broker;
    responsibility: "Base components for UI. Wrappers around native
                     Blazor elements, analogous to Brokers for the
                     UI rendering layer.";

    @suppress(autonomy);

    rationale {
        context "The Standard permits shared Base Components as thin
                 wrappers around native UI elements (sec. 3.2.1.2.0.0).
                 These are the UI equivalent of Brokers.";
        decision "Shared library, justified by Standard Base Component
                  pattern.";
        consequence "Components in this library must be pure wrappers.
                     No business logic, no flow control, no exception
                     handling.";
    }
}
```

The `@suppress` annotation takes a rule-set identifier from the `enforce` list (`autonomy`, `flow_forward`, `florance`, `entity_ownership`, `vocabulary`, `layers`). It suppresses diagnostics from that rule set for the annotated component only. Multiple `@suppress` annotations may appear on the same component. Semantic analysis records each suppression. A `rationale` should accompany every `@suppress` to document the justification, but this is a convention enforced by review, not by the parser.

### 5. Dependency Cardinality (Florance Pattern)

The Standard's Florance Pattern dictates that orchestration-layer services must have 2 or 3 service dependencies. Not 1 (that would be processing, not orchestration), not 4 or more (that would require normalization into sub-orchestrators or a coordination layer).

When the `florance` rule set is enforced, semantic analysis counts the service-layer dependencies of each orchestration-layer component (from topology `allow` rules) and flags violations:

| Dependency Count | Diagnostic |
|---|---|
| 0 | Error: orchestration component has no service dependencies. |
| 1 | Error: single dependency is processing, not orchestration. Consider reclassifying as processing or adding the missing dependency. |
| 2-3 | Valid. |
| 4+ | Warning: exceeds Florance Pattern. Consider normalizing into sub-orchestrators with a coordination layer. |

Support broker dependencies (DateTimeBroker, LoggingBroker, etc.) are excluded from this count. Only dependencies on authored components at the foundation or processing layer are counted.

The Florance Pattern also requires *balanced* dependencies: an orchestration service may not mix foundation and processing dependencies. All service dependencies must be at the same layer. If the specifier needs to combine a foundation service with a processing service, a pass-through processing service must be introduced for the foundation dependency to balance the architecture.

**Encoding in topology:**

No new topology syntax is needed. The existing `allow` rules combined with `layer` properties on components give semantic analysis enough information to compute dependency counts and layer balance.

### 6. Vocabulary

The Standard uses a rigid naming discipline across layers. Brokers say Insert/Select/Update/Delete. Foundation services say Add/Retrieve/Modify/Remove. Exposers say Post/Get/Put/Delete. Processing services use higher-order verbs (Ensure, Upsert, Verify, TryRemove).

The `vocabulary` block within the `architecture` declaration maps operation verb sets to layers:

```spec
vocabulary {
    broker:      [Insert, Select, Update, Delete];
    foundation:  [Add, Retrieve, Modify, Remove];
    processing:  [Ensure, Upsert, Verify, TryRemove];
    exposer:     [Post, Get, Put, Delete];
}
```

**Semantics:**

- Each line maps a layer name to a list of recognized operation verbs for that layer.
- When the `vocabulary` rule set is enforced, semantic analysis scans `guarantees` strings and contract method references in components at each layer, looking for verbs from the wrong layer's vocabulary.
- Mismatches produce warnings, not errors. The vocabulary is guidance for LLM realization, not a hard parse constraint.
- Orchestration, coordination, and aggregation layers do not have fixed vocabularies. Their method names are business-domain-specific (e.g., `RegisterStudent`, `NotifyAllAdmins`). If the specifier provides a vocabulary mapping for one of these layers, it is accepted and used for checking, but it is not required. The `test` layer also has no fixed vocabulary.
- The vocabulary block is optional within the architecture declaration. If omitted, no vocabulary checking occurs, but the LLM still receives the architecture declaration and can infer Standard naming conventions from the layer classification.

### 7. Validation Ordering in Contracts

The Standard requires a specific validation order within Foundation services: structural validation first (null checks, default values, required fields), then logical validation (cross-field rules, business invariants), then external validation (does the entity exist in storage, is the reference valid). This ordering minimizes wasted computation: cheap checks first, expensive checks last.

A new optional annotation `@validation` is added to `requires` clauses in contracts. This is a distinct grammar production (`ValidationAnnotation`) from the base language's `Annotation` production, which appears on field declarations. The two are structurally disjoint: `@validation` appears only after expressions in contract `requires` clauses, while base annotations appear after type expressions in entity field declarations. See the companion grammar document for details.

```spec
contract OrderSubmission {
    requires count(cart.items) > 0
        @validation(structural);
    requires customer.name != null and customer.phone != null
        @validation(structural);
    requires fulfillment == FulfillmentMethod.delivery implies
             customer.deliveryAddress != null
        @validation(logical);
    requires fulfillment == FulfillmentMethod.delivery implies
             order.total >= store.minimumDeliveryAmount
        @validation(external);
    ensures order.status == OrderStatus.received;
    guarantees "All cart item prices are frozen at submission time.";
}
```

**Recognized validation categories:**

| Category | Meaning | Standard Reference |
|---|---|---|
| `structural` | Null checks, type checks, required fields, default values | Sec. 2.1.3.1: cheapest, checked first |
| `logical` | Cross-field rules, business invariants, value range checks | Sec. 2.1.3.1: checked second |
| `external` | Existence checks against external resources, reference validation | Sec. 2.1.3.1: most expensive, checked last |

**Semantics:**

- The `@validation` annotation is optional on `requires` clauses. If omitted, no ordering check occurs for that clause.
- When present, semantic analysis verifies that structural clauses precede logical clauses, which precede external clauses, within the same contract body.
- Misordered clauses produce warnings. The contract is still valid; the ordering is a quality signal.
- `ensures` and `guarantees` clauses do not carry `@validation` annotations. They describe postconditions and commitments, not input validation.

---

## Flow Forward: Semantic Enrichment of Existing Topology

When the `flow_forward` rule set is enforced, the existing topology construct gains additional semantic weight. The rules are:

1. **No same-layer lateral calls.** If component A and component B are both at layer `foundation`, an `allow A -> B` rule in the topology is flagged as a Flow Forward violation. Foundation services cannot call other Foundation services.

2. **No downward calls from exposers.** An exposer-layer component may depend on exactly one service. If the topology allows an exposer to call a broker directly (skipping the service layer), this is flagged.

3. **No upward calls from brokers.** A broker-layer component cannot depend on any service-layer or exposer-layer component. Topology rules that allow this are flagged.

4. **Public API isolation.** Within a single service (a single authored component), public methods cannot call other public methods. This is a code-level rule that cannot be checked at spec level, but the architecture declaration signals to the LLM that this rule applies during realization.

These rules do not require new topology syntax. They are semantic interpretations of existing `allow`/`deny` rules, activated by the architecture declaration and the layer properties on components.

---

## Layer Contracts

A `layer_contract` is a named contract block that attaches behavioral commitments to an entire layer rather than to a single entity or boundary. Layer contracts declare what every component at a given layer must do, providing the LLM with formal obligations it must satisfy during code generation.

```spec
layer_contract FoundationContract {
    layer: foundation;

    guarantees "Every public method is wrapped in a TryCatch exception
                noise cancellation delegate that catches native broker
                exceptions and maps them to categorical local exception
                models (ValidationException, DependencyException,
                ServiceException).";

    guarantees "Validation executes in order: structural (null checks,
                required fields, default values), then logical (cross-field
                rules, value ranges, business invariants), then external
                (existence checks against external resources). Structural
                failures short-circuit before logical; logical failures
                short-circuit before external.";

    guarantees "Each public method calls exactly one broker method.
                No method combines multiple broker calls. Higher-order
                combinations belong to the processing layer.";

    guarantees "The service speaks business language, not storage language:
                Add (not Insert), Retrieve (not Select), Modify (not Update),
                Remove (not Delete).";
}

layer_contract ProcessingContract {
    layer: processing;

    guarantees "Processing services combine or transform the outcome of
                primitive foundation operations. They may call multiple
                methods on their single foundation dependency.";

    guarantees "Validation is scoped to what the processing method uses.
                Full entity validation is delegated to the foundation layer.";

    guarantees "Processing services use higher-order verbs: Ensure, Upsert,
                Verify, TryRemove.";
}

layer_contract OrchestrationContract {
    layer: orchestration;

    guarantees "Orchestration services combine 2-3 processing or foundation
                services to execute multi-entity business flows.";

    guarantees "Dependencies are balanced: all service dependencies must be
                at the same layer (all processing or all foundation, not mixed).
                If a foundation service is needed alongside a processing service,
                introduce a pass-through processing service to balance.";

    guarantees "Orchestration methods speak business language that is close
                to the domain: RegisterStudent, SubmitOrder, NotifyAllAdmins.";
}

layer_contract BrokerContract {
    layer: broker;

    guarantees "No flow control: no if-statements, no while-loops, no
                switch-cases. The broker delegates to the external resource
                and returns the result.";

    guarantees "No exception handling. Native exceptions propagate to the
                foundation layer, which maps them to local models.";

    guarantees "The broker speaks the language of the technology it wraps:
                Insert, Select, Update, Delete for storage; Post, Get for
                APIs; Enqueue, Dequeue for queues.";

    guarantees "Partial classes organize entity-specific operations into
                separate files: StorageBroker.Students.cs,
                StorageBroker.Orders.cs.";
}

layer_contract ExposerContract {
    layer: exposer;

    guarantees "Exposers are pure mapping layers. No business logic, no
                sequencing, no iteration, no selection beyond mapping
                responses to protocol status codes.";

    guarantees "Each exposer integrates with exactly one service dependency.
                Multiple pages or endpoints share the same service entry
                point (aggregation or orchestration).";

    guarantees "Exception-to-status-code mapping: ValidationException maps
                to 400 (BadRequest) or 409 (Conflict); DependencyException
                maps to 500 (InternalServerError); ServiceException maps
                to 500.";
}

layer_contract TestContract {
    layer: test;

    guarantees "Tests follow Given/When/Then structure with randomized
                inputs: CreateRandomStudent(), CreateRandomOrder().";

    guarantees "Expected values are deep-cloned from input values to
                detect unintended mutations: expectedStudent =
                inputStudent.DeepClone().";

    guarantees "Mock verification uses Times.Once for each expected call,
                followed by VerifyNoOtherCalls() on every mock to ensure
                no unexpected broker or service interactions.";

    guarantees "Test naming follows MethodName_Scenario_ExpectedResult
                convention.";
}
```

**Semantics:**

- A `layer_contract` is a top-level declaration, appearing alongside system declarations, topology, and other spec constructs.
- The `layer` property binds the contract to a recognized layer value. Every component at that layer inherits these commitments.
- The `guarantees` clauses are prose strings, consistent with the existing contract construct. They are behavioral commitments the LLM must satisfy during realization. They are not machine-evaluable expressions; they are human-readable obligations.
- Multiple `layer_contract` blocks can target the same layer; their guarantees accumulate.
- Layer contracts are distinct from entity-level contracts (which bind to a specific entity or boundary). They bind to every component at a layer.
- The LLM receives layer contracts alongside the component declarations and topology during realization. The contracts answer the question: "I know this component is a foundation service; what patterns must its code follow?"

---

## Realization Directives

A `realize` block within the architecture declaration provides prose instructions that the LLM receives during code generation. Unlike layer contracts (which are formal commitments verifiable against the generated code), realization directives are conventions and preferences that guide code style without rising to the level of contractual obligations.

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

    realize broker {
        "Use partial classes to separate entity-specific operations
         into distinct files (e.g., StorageBroker.Orders.cs,
         StorageBroker.MenuItems.cs). The base StorageBroker.cs holds
         configuration and generic CRUD methods.";

        "Support brokers (DateTimeBroker, LoggingBroker) are single-file,
         single-purpose classes. No partial class splitting needed.";
    }

    realize foundation {
        "Each foundation service class uses a TryCatch partial class
         (e.g., OrderService.Exceptions.cs) that contains the exception
         noise cancellation delegate mapping native exceptions to local
         categorical models.";

        "Validation logic lives in a separate partial class
         (e.g., OrderService.Validations.cs) with methods named
         ValidateOrderOnAdd, ValidateOrderOnModify, etc.";
    }

    realize processing {
        "Processing services are simpler than foundations. No TryCatch
         partial class unless the processing method introduces new
         failure modes beyond what the foundation already handles.";
    }

    realize test {
        "Every service gets its own test class. Foundation service tests
         verify each validation rule individually: one test per structural
         validation, one per logical validation, one per external check.";

        "Use Moq for mock setup. Use FluentAssertions for assertion
         readability. Use Force.DeepCloner for expected value isolation.";

        "The Happy Path test for each CRUD operation is the first test
         written. Validation tests follow.";
    }

    realize exposer {
        "API controllers use RESTFulSense for typed action results.
         Each controller method maps exactly one service call to one
         HTTP response.";

        "For Blazor UI exposers, a ViewService sits between the core
         service layer and UI components. The ViewService is a
         foundation-like service for the UI: it calls the downstream
         orchestration or aggregation service, maps the result to a
         view model, and exposes it to the Blazor component. The
         component never calls core services directly.";
    }

    rationale {
        context "PizzApp follows The Standard for clean, testable,
                 and maintainable architecture.";
        decision "Full enforcement of all rule sets. Layer contracts
                  define formal obligations. Realization directives
                  guide code style.";
        consequence "Components are declared with Standard vocabulary:
                     broker, foundation service, processing service,
                     orchestration service, exposer. LLM realization
                     generates code matching each layer's role.";
    }
}
```

**Semantics:**

- `realize` blocks appear inside the `architecture` declaration as `ArchitectureMember` alternatives.
- Each `realize` block names a layer and contains one or more prose STRING directives.
- Realization directives are informational, not contractual. They do not trigger semantic validation errors. They are passed to the LLM as context during code generation.
- The distinction between `layer_contract` and `realize` is: layer contracts say "you must"; realization directives say "you should." A layer contract's `guarantees` can be verified against generated code (does the foundation service actually wrap every method in TryCatch?). A realization directive's prose is advisory (should the partial class be named `OrderService.Exceptions.cs` or `OrderService.ExceptionHandling.cs`? Either is acceptable).
- If a `realize` block targets a layer that has no `layer_contract`, the directives still apply. The two constructs are independent.

---

## Example: PizzApp Fragment Under The Standard

The following fragment shows how the PizzApp spec would look with The Standard extension active. This is not a complete spec; it illustrates the new constructs on a subset of the system. The architecture declaration, layer contracts, and realization directives shown in the sections above apply here. For brevity, they are not repeated; the LLM receives them alongside this system declaration during realization.

```spec
system PizzApp {
    target: "net10.0";
    responsibility: "Online pizza ordering and restaurant fulfillment.";

    // --- Brokers (consumed components, unchanged from base spec) ---

    consumed component BlazorWasm {
        source: nuget("Microsoft.AspNetCore.Components.WebAssembly");
        version: "10.*";
        responsibility: "Blazor WebAssembly hosting runtime.";
        used_by: [PizzApp.Web];

        contract {
            guarantees "Client-side .NET execution via WebAssembly";
        }
    }

    consumed component StorageBroker {
        source: nuget("Microsoft.EntityFrameworkCore");
        version: "10.*";
        responsibility: "ORM for local storage persistence.";
        used_by: [PizzApp.Brokers.Storage];
    }

    // --- Authored Brokers ---

    broker PizzApp.Brokers.Storage {
        kind: library;
        path: "src/PizzApp.Brokers.Storage";
        responsibility: "Storage broker. Wraps Entity Framework for
                         persistence. Partial classes per entity.
                         No flow control, no exception handling.";
    }

    broker PizzApp.Brokers.DateTime {
        kind: library;
        path: "src/PizzApp.Brokers.DateTime";
        responsibility: "Support broker. Abstracts system clock for
                         testable time-dependent logic.";
    }

    broker PizzApp.Brokers.Logging {
        kind: library;
        path: "src/PizzApp.Brokers.Logging";
        responsibility: "Support broker. Wraps ILogger for structured
                         logging.";
    }

    // --- Foundation services ---

    foundation service PizzApp.Services.Foundations.Orders {
        kind: library;
        path: "src/PizzApp.Services/Foundations/Orders";
        owns: Order;
        responsibility: "Validation and primitive CRUD for Order
                         entities. Structural, logical, and external
                         validation in that order.";

        contract {
            guarantees "Add, Retrieve, Modify, Remove operations
                        for Order entities";
        }
    }

    foundation service PizzApp.Services.Foundations.MenuItems {
        kind: library;
        path: "src/PizzApp.Services/Foundations/MenuItems";
        owns: MenuItem;
        responsibility: "Validation and primitive CRUD for MenuItem
                         entities.";
    }

    foundation service PizzApp.Services.Foundations.Customers {
        kind: library;
        path: "src/PizzApp.Services/Foundations/Customers";
        owns: Customer;
        responsibility: "Validation and primitive CRUD for Customer
                         entities.";
    }

    foundation service PizzApp.Services.Foundations.Promotions {
        kind: library;
        path: "src/PizzApp.Services/Foundations/Promotions";
        owns: Promotion;
        responsibility: "Validation and primitive CRUD for Promotion
                         entities.";
    }

    // --- Processing services ---

    processing service PizzApp.Services.Processings.Orders {
        kind: library;
        path: "src/PizzApp.Services/Processings/Orders";
        owns: Order;
        responsibility: "Higher-order order logic: EnsureOrderExists,
                         UpsertOrder, VerifyOrderStatus.";
    }

    processing service PizzApp.Services.Processings.MenuItems {
        kind: library;
        path: "src/PizzApp.Services/Processings/MenuItems";
        owns: MenuItem;
        responsibility: "Higher-order menu logic:
                         VerifyMenuItemAvailable, EnsureMenuItemExists.";
    }

    // --- Orchestration services ---

    orchestration service PizzApp.Services.Orchestrations.OrderSubmission {
        kind: library;
        path: "src/PizzApp.Services/Orchestrations/OrderSubmission";
        responsibility: "Combines order processing and menu item
                         processing to submit a complete order.
                         Validates cross-entity rules.";
    }

    // --- Exposer ---

    exposer PizzApp.Web {
        kind: application;
        path: "src/PizzApp.Web";
        responsibility: "Blazor WASM host. API controllers and pages.
                         Single point of contact with the orchestration
                         or aggregation layer.";
    }

    // --- Tests ---

    test PizzApp.Tests {
        kind: tests;
        path: "tests/PizzApp.Tests";
        responsibility: "Unit, integration, and acceptance tests.
                         Every foundation, processing, and orchestration
                         service is fully unit-tested.";
    }
}
```

```spec
topology PizzAppTopology {
    // Broker -> external (implicit, via consumed component used_by)

    // Foundation -> Broker
    allow PizzApp.Services.Foundations.Orders -> PizzApp.Brokers.Storage;
    allow PizzApp.Services.Foundations.Orders -> PizzApp.Brokers.DateTime;
    allow PizzApp.Services.Foundations.Orders -> PizzApp.Brokers.Logging;
    allow PizzApp.Services.Foundations.MenuItems -> PizzApp.Brokers.Storage;
    allow PizzApp.Services.Foundations.MenuItems -> PizzApp.Brokers.Logging;
    allow PizzApp.Services.Foundations.Customers -> PizzApp.Brokers.Storage;
    allow PizzApp.Services.Foundations.Customers -> PizzApp.Brokers.Logging;
    allow PizzApp.Services.Foundations.Promotions -> PizzApp.Brokers.Storage;
    allow PizzApp.Services.Foundations.Promotions -> PizzApp.Brokers.Logging;

    // Processing -> Foundation (one-to-one)
    allow PizzApp.Services.Processings.Orders -> PizzApp.Services.Foundations.Orders;
    allow PizzApp.Services.Processings.MenuItems -> PizzApp.Services.Foundations.MenuItems;

    // Orchestration -> Processing (2-3, Florance)
    allow PizzApp.Services.Orchestrations.OrderSubmission -> PizzApp.Services.Processings.Orders;
    allow PizzApp.Services.Orchestrations.OrderSubmission -> PizzApp.Services.Processings.MenuItems;

    // Exposer -> Orchestration (single point of contact)
    allow PizzApp.Web -> PizzApp.Services.Orchestrations.OrderSubmission;

    // Tests -> everything
    allow PizzApp.Tests -> PizzApp.Services.Foundations.Orders;
    allow PizzApp.Tests -> PizzApp.Services.Foundations.MenuItems;
    allow PizzApp.Tests -> PizzApp.Services.Foundations.Customers;
    allow PizzApp.Tests -> PizzApp.Services.Foundations.Promotions;
    allow PizzApp.Tests -> PizzApp.Services.Processings.Orders;
    allow PizzApp.Tests -> PizzApp.Services.Processings.MenuItems;
    allow PizzApp.Tests -> PizzApp.Services.Orchestrations.OrderSubmission;
    allow PizzApp.Tests -> PizzApp.Brokers.Storage;

    // Flow Forward violations (these would be flagged):
    // deny PizzApp.Services.Foundations.Orders -> PizzApp.Services.Foundations.Customers;
    // deny PizzApp.Brokers.Storage -> PizzApp.Services.Foundations.Orders;

    invariant "nullable everywhere":
        all authored components satisfy nullable == enabled;
}
```

### What semantic analysis flags on the original PizzApp spec

If the original PizzApp spec (from `BlazorWebAsm-Pizza/Docs/Spec-Chat/DotNet/pizzapp.spec.md`) were placed under an `architecture TheStandard` declaration without modification, semantic analysis would produce the following diagnostics:

1. **Missing layer property.** Every authored component (`PizzaOrders`, `PizzaOrders.UI`, `PizzaOrdersCore`, `PizzaOrdersTests`) lacks a `layer` property. Diagnostic: "authored component 'PizzaOrders' requires a layer property when architecture 'TheStandard' is declared."

2. **No entity ownership.** No authored component declares `owns`. Without entity ownership, the single-entity integration rule cannot be validated.

3. **Potential horizontal entanglement.** `PizzaOrders.UI` is a shared authored library. Whether the autonomy rule flags it depends on layer assignment. If `PizzaOrders` and `PizzaOrders.UI` are assigned the same layer, it is flagged. The Standard-compliant alternative is to classify `PizzaOrders.UI` at the `broker` layer (as a Base Component library, sec. 3.2.1.2.0.0 of The Standard), in which case components at higher layers depending on it is normal downward dependency.

4. **Flat topology.** The original topology has four authored components with direct allow rules between them. Under The Standard, these four components would decompose into separate broker, foundation, processing, orchestration, and exposer components, each at a declared layer. The flat structure is insufficient for Standard compliance.

5. **No validation ordering.** The contracts in the original spec have `requires` clauses but no `@validation` annotations. This produces no errors (annotations are optional) but no ordering validation occurs.

---

## Interaction with Incremental Specs

The `architecture` declaration is a property of the spec collection, not of individual incremental specs. It appears in the base system spec or in its own dedicated spec file. All incremental specs (features, bugs, decisions, amendments) inherit the architecture declaration and must comply with its activated rules.

A feature spec that adds a new entity must also add a corresponding Foundation service with `owns` pointing to that entity. A decision spec that restructures the service hierarchy must update layer properties on affected components. An amendment that changes topology rules is validated against the architecture's Flow Forward and Florance rules.

The manifest's Tracking block and lifecycle states are unchanged. The architecture declaration has its own lifecycle: it enters Draft when first written, Approved when the team agrees on the architectural standard, and Executed when the first code is generated following its rules.

---

## How Code-Level Patterns Are Carried

Earlier versions of this extension excluded several code-level patterns from The Standard on the grounds that they were "realization details." In practice, excluding them meant the LLM had no signal to produce Standard-compliant code at the method level. This extension now carries them through two mechanisms:

**Layer contracts** (`layer_contract`) encode formal obligations that every component at a given layer must satisfy. These are verifiable: a reviewer can check whether the generated code actually wraps every foundation method in TryCatch, whether tests use randomized inputs with mock verification, whether brokers contain no flow control. Layer contracts answer: "what must this layer's code always do?"

**Realization directives** (`realize` blocks inside the architecture declaration) encode advisory conventions: file naming, partial class organization, library choices, code style. These are preferences, not hard requirements. They answer: "how should this layer's code be organized?"

The following table shows where each Standard pattern is now carried:

| Standard Pattern | Carried By | Construct |
|---|---|---|
| Exception Noise Cancellation (TryCatch) | Layer contract | `FoundationContract` guarantees wrapping in TryCatch delegate |
| Tri-level validation ordering | Layer contract + `@validation` annotation | `FoundationContract` guarantees structural-then-logical-then-external; `@validation` tags on contract clauses |
| Single broker call per foundation method | Layer contract | `FoundationContract` guarantees one broker call per public method |
| No flow control in brokers | Layer contract | `BrokerContract` guarantees no if/while/switch |
| No exception handling in brokers | Layer contract | `BrokerContract` guarantees exceptions propagate |
| Partial classes for brokers | Realization directive | `realize broker` advises partial class per entity |
| TryCatch partial class file | Realization directive | `realize foundation` advises Exceptions.cs partial |
| Validation partial class file | Realization directive | `realize foundation` advises Validations.cs partial |
| Given/When/Then test structure | Layer contract | `TestContract` guarantees Given/When/Then with randomized inputs |
| Mock verification (Times.Once, VerifyNoOtherCalls) | Layer contract | `TestContract` guarantees mock verification pattern |
| Deep-cloned expected values | Layer contract | `TestContract` guarantees DeepClone isolation |
| Test naming (MethodName_Scenario_ExpectedResult) | Layer contract | `TestContract` guarantees naming convention |
| Happy Path test first | Realization directive | `realize test` advises Happy Path ordering |
| Library choices (Moq, FluentAssertions, DeepCloner) | Realization directive | `realize test` advises specific libraries |
| Exposer exception-to-status mapping | Layer contract | `ExposerContract` guarantees categorical mapping |
| Balanced orchestration dependencies | Layer contract | `OrchestrationContract` guarantees same-layer deps |
| Processing higher-order verbs | Layer contract | `ProcessingContract` guarantees verb vocabulary |
| View services for UI | Realization directive | `realize exposer` advises ViewService intermediary |

### What remains excluded

Two aspects of The Standard are still not encoded in the spec:

**Configuration models (Startup, DI registration).** These are platform-specific and are already covered by SpecChat's platform realization constructs (`dotnet solution`, `startup` project). The DI registration pattern is conventional enough that the LLM infers it from the component declarations and their dependencies.

**Runtime observability (logging calls, monitoring).** The Standard uses `LoggingBroker` calls throughout service methods for observability. This is a cross-cutting concern that pervades every method body. Encoding it as a layer contract would produce a single guarantees clause ("every service method logs its entry, exit, and exceptions") that is too coarse to guide specific logging placement. The `LoggingBroker` is declared as a consumed or authored broker; the LLM infers logging calls from the broker's existence and the Standard's general patterns.
