# SpecChat

A markdown-embedded specification language where the human authors formal commitments and the LLM operates as a realization engine.

**[Read the published documentation](https://realization-engine.github.io/spec-chat/docs/)**

---

## The Problem

LLMs are absorbing implementation work. As that happens, the human's value migrates to specification, evaluation, and architecture under ambiguity. But the working medium for specification today is prose documents and chat sessions. Architecture lives in one file, constraints in another, execution rules in a prompt, test obligations in yet another, and the real refinement path only in the author's head. The LLM can often navigate this, but only because the author compensates for the medium's weakness.

Meaning is present but distributed. Refinement happens but implicitly. Cross-view consistency is possible but brittle. The effort goes into keeping documents consistent and re-explaining context, not into reasoning about systems. A formal specification medium is needed: one where the specifier's struggle is system reasoning, not medium management.

The full argument for why specification is the site where durable human capability must now be built, and why the medium matters for that, is in the companion whitepaper: [Enquiry Into Specification as Meaningful Struggle](Docs/Enquiry-Into-Specification-as-Meaningful-Struggle.md).

## What SpecChat Is

SpecChat is a specification language embedded in Markdown. Spec blocks are typed engineering objects with formal semantics: entities with contracts and invariants, components with topology rules, build phases with gate conditions, traces, constraints, package policies, and verification obligations. The `.spec.md` document is the primary engineering artifact. Everything generated from it (code, tests, documentation, dependency graphs) is a projection of the specification model.

The division of labor is explicit. The human authors commitments: abstractions, boundaries, constraints, invariants, contracts, and judgments about what must be preserved. The LLM operates as the realization engine: it consumes the specification and produces systems that conform to it. The boundary between the two is the specification model.

SpecChat organizes around four layers:

1. **Language definition.** Two files define what SpecLang constructs exist and how to parse them: the [SpecLang Specification](Delivery/spec-chat/SpecLang-Specification.md) and the [SpecLang Grammar](Delivery/spec-chat/SpecLang-Grammar.md).

2. **System specification.** A base `.spec.md` file declares a specific system: its components, their topology, build phases with gate conditions, data entities, page declarations, and traceability mappings.

3. **Incremental specifications.** Four document types extend the base spec over time: *decision specs* (resolve conflicts between spec and source), *amendments* (correct the base spec without adding capability), *feature specs* (declare new capabilities), and *bug specs* (declare source gaps the spec correctly identifies). Each carries a Tracking block with lifecycle state and dependency declarations.

4. **Manifest.** A single entry-point file binds the collection together: which system, what document types exist, lifecycle rules, the full inventory of specs with their states, and execution order.

## The Standard Extension

An opt-in extension allows specifications to encode the architectural rules of Hassan Habib's "The Standard." It adds layer-prefixed declaration forms (broker, foundation service, processing service, orchestration service, exposer, test), layer contracts that attach behavioral obligations to every component at a given layer, realization directives for advisory code-generation conventions, and semantic validation rules (topology checking against the layer hierarchy, Florance Pattern enforcement, entity ownership, autonomy constraints, vocabulary checking, validation ordering).

The extension is activated by an `architecture TheStandard` declaration in the base system spec. When absent, the base language operates unchanged.

See the [Extension Overview](Delivery/spec-chat/extensions/the-standard/TheStandard-Extension-Overview.md) for full details.

## Repository Layout

| Path | Description |
|---|---|
| `Docs/` | Whitepaper and citations |
| `Delivery/spec-chat/SpecChat-Overview.md` | Motivation, design rationale, and overview of the SpecChat system |
| `Delivery/spec-chat/SpecLang-Specification.md` | Language definition: constructs, semantics, three registers of specification |
| `Delivery/spec-chat/SpecLang-Grammar.md` | Formal EBNF grammar: lexer tokens, productions, ambiguity resolution |
| `Delivery/spec-chat/samples/` | Sample manifest and system spec (Blazor analytics harness) |
| `Delivery/spec-chat/extensions/the-standard/` | The Standard extension: overview, specification, grammar |

## Where to Start

1. This README for orientation.
2. The [Enquiry](Docs/Enquiry-Into-Specification-as-Meaningful-Struggle.md) for the theoretical foundation: why specification, why a formal medium, what the realization engine concept means.
3. The [SpecChat Overview](Delivery/spec-chat/SpecChat-Overview.md) for how the system is put together.
4. The [SpecLang Specification](Delivery/spec-chat/SpecLang-Specification.md) and [Grammar](Delivery/spec-chat/SpecLang-Grammar.md) for the full language definition.
5. The [sample system spec](Delivery/spec-chat/samples/blazor-harness.spec.md) and [manifest](Delivery/spec-chat/samples/blazor-harness.manifest.md) for a concrete example.

## Installation

SpecChat has been tested with [Claude Code](https://docs.anthropic.com/en/docs/claude-code). Support for other LLM environments is planned.

### Claude Code

Copy the `Delivery/spec-chat/` folder from this repository into your Claude Code configuration directory:

| Platform | Destination |
|---|---|
| macOS / Linux | `~/.claude/spec-chat/` |
| Windows | `%USERPROFILE%\.claude\spec-chat\` |

After copying, your `.claude/` directory should contain:

```
.claude/
  spec-chat/
    SpecChat-Overview.md
    SpecLang-Specification.md
    SpecLang-Grammar.md
    extensions/
      the-standard/
        TheStandard-Extension-Overview.md
        TheStandard-Extension-Specification.md
        TheStandard-Extension-Grammar.md
    samples/
      blazor-harness.manifest.md
      blazor-harness.spec.md
```

Claude Code will read these files as context when working with `.spec.md` documents.

## License

[MIT](LICENSE)
