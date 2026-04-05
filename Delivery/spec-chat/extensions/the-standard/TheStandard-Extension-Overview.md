# The Standard Extension Overview

This is an opt-in extension to SpecChat that allows specifications to encode the architectural rules of Hassan Habib's "The Standard." It adds Standard-vocabulary declaration forms, layer contracts, realization directives, and semantic validation rules to the base SpecLang language. Systems that do not activate the extension are unaffected.

This document explains what the extension adds, how its files relate to the base SpecChat system, and how the pieces fit together.

## Relationship to the Base System

The base SpecChat system has four layers (described in SpecChat-Overview.md): the language definition, the system specification, incremental specifications, and the manifest. This extension operates at Layer 1. It extends the language definition with new constructs, keywords, and semantic rules. Layers 2 through 4 are unchanged in structure; they gain access to new constructs when the extension is active.

The extension does not replace any base construct. Every base-language spec remains valid. The extension adds alternatives (layer-prefixed declarations alongside `authored component`), new top-level declarations (`architecture`, `layer_contract`), new properties (`owns`, `@suppress`, `@validation`), and new blocks within the architecture declaration (`vocabulary`, `realize`).

## The Extension Files

Two files define the extension, mirroring the base language's two-file structure.

1. **TheStandard-Extension-Specification.md** defines what the new constructs mean: the architecture declaration and its activation semantics, layer-prefixed declaration forms and their desugaring, entity ownership, the autonomy constraint and `@suppress` mechanism, the Florance Pattern, vocabulary mapping, validation ordering with `@validation`, Flow Forward enrichment of topology, layer contracts, and realization directives. It also contains a complete PizzApp example showing all constructs in use and a mapping table showing how Standard code-level patterns are carried by layer contracts and realization directives.

2. **TheStandard-Extension-Grammar.md** defines how to parse the new constructs: 18 new keywords, the `ArchitectureDecl` and `LayerContractDecl` top-level productions, the `RealizeDecl` architecture member production, layer-prefixed system member declarations (`BrokerDecl`, `ServiceDecl`, `ExposerDecl`, `TestDecl`), the `SuppressAnnotation` and `ValidationAnnotation` productions, the `LayerKeywordOrIdent` shared production for layer-name positions, 12 ambiguity resolution notes, and complete token stream examples.

A human or LLM reads the base language files first (SpecLang-Specification.md and SpecLang-Grammar.md), then reads these two extension files. The combination gives the full vocabulary and syntax for Standard-compliant specifications.

## Activation

The extension is activated by a top-level `architecture` declaration in the base system spec:

```spec
architecture TheStandard {
    version: "1.0";
    enforce: [layers, flow_forward, florance,
              entity_ownership, autonomy, vocabulary];
    ...
}
```

When this declaration is present, semantic analysis enforces the activated rule sets. When absent, the extension's constructs are unavailable and the base language operates unchanged.

The `enforce` list controls which rule sets are active. Omitting it activates all rules (the All-In default, consistent with The Standard's "All-In/All-Out" principle). Listing specific identifiers allows incremental adoption.

## What the Extension Adds

### Standard-vocabulary declarations

The base language declares authored components generically: `authored component Name { layer: foundation; ... }`. The extension adds declaration forms that use The Standard's own nouns:

- `broker Name { ... }` for Broker-layer components
- `foundation service Name { ... }` for Foundation services
- `processing service Name { ... }` for Processing services
- `orchestration service Name { ... }` for Orchestration services
- `coordination service Name { ... }` for Coordination services
- `aggregation service Name { ... }` for Aggregation services
- `exposer Name { ... }` for Exposer-layer components
- `test Name { ... }` for test projects

Each desugars to `authored component` with the corresponding `layer` property. Both forms produce identical ASTs.

### Layer contracts

A `layer_contract` is a top-level declaration that attaches behavioral commitments to every component at a given layer. It answers: "what must this layer's code always do?"

Six default layer contracts encode The Standard's core patterns:

- **FoundationContract**: TryCatch exception noise cancellation, tri-level validation ordering (structural then logical then external), one broker call per public method, business-language naming.
- **ProcessingContract**: higher-order combinations of foundation operations, scoped validation, processing-verb naming.
- **OrchestrationContract**: 2-3 balanced service dependencies, domain-language naming.
- **BrokerContract**: no flow control, no exception handling, technology-language naming, partial-class organization.
- **ExposerContract**: pure mapping, single service dependency, exception-to-status-code mapping.
- **TestContract**: Given/When/Then with randomized inputs, deep-cloned expected values, mock verification with Times.Once and VerifyNoOtherCalls, MethodName_Scenario_ExpectedResult naming.

### Realization directives

A `realize` block inside the architecture declaration provides advisory prose that the LLM receives during code generation. Unlike layer contracts (formal "you must" obligations), realization directives are conventions ("you should"): file naming, partial class organization, library choices, code style.

### Semantic rules

When the architecture is active, existing SpecChat constructs gain additional semantic weight:

- **Topology** rules are checked against the layer hierarchy. Same-layer lateral calls (Flow Forward), upward calls from brokers, and downward calls from exposers are flagged.
- **Florance Pattern** checks that orchestration-layer components have 2-3 service dependencies, all at the same layer.
- **Entity ownership** requires each foundation service to declare `owns` with exactly one entity.
- **Autonomy** flags horizontal entanglement (shared authored components consumed by peers at the same layer). The `@suppress` annotation provides a formal override mechanism.
- **Vocabulary** checks operation verb usage against the declared mapping per layer.
- **Validation ordering** checks that `@validation(structural)` precedes `@validation(logical)` precedes `@validation(external)` within contract `requires` clauses.

### The test layer

The `test` layer is exempt from the layer hierarchy, Flow Forward, Florance, and autonomy rules. Tests must depend on components at every layer to verify behavior; restricting test dependencies would make the architecture rules unimplementable.

## How Code-Level Patterns Are Carried

The Standard prescribes patterns at the method-body level: TryCatch wrapping, validation ordering, mock verification in tests, no flow control in brokers. These are not structural (they do not affect the system's component graph), but they are critical for Standard-compliant code.

The extension carries them through two mechanisms:

- **Layer contracts** encode formal obligations verifiable against generated code. A reviewer can check: does every foundation method actually wrap in TryCatch? Do tests use randomized inputs?
- **Realization directives** encode advisory conventions. A reviewer can assess: are partial classes named consistently? Is the Happy Path test written first?

The distinction matters for the LLM. Layer contract guarantees are commitments the LLM must satisfy. Realization directives are preferences the LLM should follow but can deviate from with justification.

## What Remains Excluded

Two aspects of The Standard are not encoded:

- **Configuration models** (Startup, DI registration): covered by the base language's platform realization constructs.
- **Runtime observability** (logging calls): too diffuse to encode formally. The LoggingBroker's existence signals the pattern.

## The Workflow Under The Standard

### Specifying a new system

The specifier activates the extension with an `architecture TheStandard` declaration. Components are declared using Standard vocabulary (broker, foundation service, etc.). Layer contracts and realization directives are included in the base spec. The topology is derived from the layer hierarchy. Phases follow bottom-up construction (scaffold, foundations, processing, orchestration, exposer).

### Implementing from spec

The LLM reads the architecture declaration, layer contracts, realization directives, component declarations, topology, and entity definitions. It has enough information to generate Standard-compliant code: Foundation services with TryCatch and tri-level validation, Processing services with higher-order logic, Orchestration services combining entity flows, Brokers with no flow control, Exposers as pure mapping layers, and tests with Given/When/Then structure and mock verification.

### Evolving the system

Feature specs add new components using layer-prefixed declarations. Each new foundation service must declare `owns`. New layer contracts or realization directives can be added through amendments to the architecture declaration.

### Verifying

In addition to the base workflow's contract and constraint verification, Standard-compliant specs allow layer-contract verification: does the generated code satisfy every `guarantees` clause in the applicable layer contract?

## Guided Authoring

The `/spec-the-standard` slash command in Claude Code provides a guided authoring flow for Standard-compliant specs. It extends `/spec-chat` with:

- Automatic generation of the architecture declaration with built-in defaults (vocabulary, realize blocks, rationale).
- Component decomposition by layer (brokers first, then foundations, processing, orchestration, exposer, tests) instead of a flat list.
- DateTimeBroker and LoggingBroker included as default support brokers.
- Florance Pattern validation during orchestration service definition.
- Topology generated from the layer hierarchy, with follow-up questions for additional rules.
- Bottom-up phase suggestion (scaffold, foundations, processing, orchestration, exposer).
- `@validation` tagging on contract `requires` clauses.
- Default layer contracts generated and presented for user review.
- Assembly with Architecture and Layer Contracts sections in the output spec.

The command supports all five document types. Feature specs generate layer-prefixed declarations with entity ownership. Bug, decision, and amendment specs follow the base `/spec-chat` flow with Standard-aware modifications.

The command file lives at `.claude/commands/spec-the-standard.md`.

## File Inventory

| File | Role |
|---|---|
| `TheStandard-Extension-Overview.md` | This document. Entry point for the extension. |
| `TheStandard-Extension-Specification.md` | Language semantics: what the new constructs mean. |
| `TheStandard-Extension-Grammar.md` | Formal grammar: how to parse the new constructs. |
| `.claude/commands/spec-the-standard.md` | Guided authoring command for Standard-compliant specs. |
