# SpecChat

A markdown-embedded specification language where the human authors formal commitments and the LLM operates as a realization engine.

**[Read the published documentation](https://realization-engine.github.io/spec-chat/docs/)**

---

## The Problem

LLMs are absorbing implementation work. As that happens, the human's value migrates to specification, evaluation, and architecture under ambiguity. But the working medium for specification today is prose documents and chat sessions. Architecture lives in one file, constraints in another, execution rules in a prompt, test obligations in yet another, and the real refinement path only in the author's head. The LLM can often navigate this, but only because the author compensates for the medium's weakness.

Meaning is present but distributed. Refinement happens but implicitly. Cross-view consistency is possible but brittle. The effort goes into keeping documents consistent and re-explaining context, not into reasoning about systems. A formal specification medium is needed: one where the specifier's struggle is system reasoning, not medium management.

The full argument for why specification is the site where durable human capability must now be built, and why the medium matters for that, is in the companion whitepaper: [Enquiry Into Specification as Meaningful Struggle](docs/theory/Enquiry-Into-Specification-as-Meaningful-Struggle.md).

## What SpecChat Is

SpecChat is a specification-driven system for authoring software through collaboration between humans and LLMs. Its formal language, SpecLang, is embedded in Markdown: spec blocks are typed engineering objects with formal semantics, covering entities, contracts, invariants, persons, external systems, components with topology rules, build phases with gate conditions, traces, constraints, package policies, deployment environments, architectural views, dynamic interaction sequences, and verification obligations. The `.spec.md` document is the primary engineering artifact. Everything generated from it (code, tests, documentation, dependency graphs, deployment diagrams) is a projection of the specification model.

The division of labor is explicit. The human authors commitments: abstractions, boundaries, constraints, invariants, contracts, and judgments about what must be preserved. The LLM operates as the realization engine: it consumes the specification and produces systems that conform to it. The boundary between the two is the specification model.

SpecChat organizes around four layers:

1. **Language definition.** Two files define what SpecLang constructs exist and how to parse them: the [SpecLang Specification](docs/specification/SpecLang-Specification.md) and the [SpecLang Grammar](docs/specification/SpecLang-Grammar.md).

2. **System specification.** A base `.spec.md` file declares a specific system: its components, their topology, build phases with gate conditions, data entities, page declarations, and traceability mappings.

3. **Incremental specifications.** Four document types extend the base spec over time: *decision specs* (resolve conflicts between spec and source), *amendments* (correct the base spec without adding capability), *feature specs* (declare new capabilities), and *bug specs* (declare source gaps the spec correctly identifies). Each carries a Tracking block with lifecycle state and dependency declarations.

4. **Manifest.** A single entry-point file binds the collection together: which system, what document types exist, lifecycle rules, the full inventory of specs with their states, and execution order.

## The SpecLang DSL

SpecLang is the formal language inside SpecChat. Specification blocks live in ` ```spec ` fenced code blocks within ordinary Markdown; the surrounding prose carries design rationale and discussion. A `.spec.md` file is simultaneously a readable document and a machine-checkable specification.

The language covers five registers of specification:

**Data specification** defines the domain model: entities with typed fields, enums with semantic descriptions, cross-field invariants, contracts at request/response boundaries, confidence signals that declare expected extraction reliability, and rationale (inline or structured micro-ADRs) that records why each choice was made.

```spec
entity LineItem {
    drink: CoffeeDrink @confidence(high);
    size: CoffeeSize @default(medium) @confidence(medium);
    quantity: int @range(1..10) @default(1);

    invariant "espresso is always small":
        drink == CoffeeDrink.espresso implies size == CoffeeSize.small;

    rationale "drink is the primary item; size, temperature,
               and quantity are modifiers with sensible defaults.";
}
```

**Context specification** identifies who uses the system and what external systems it interacts with, before any internal decomposition. Persons (human actors), external systems (runtime peers), and relationships (labeled directional edges with description and technology) establish the outermost scope. General-purpose `@tag` annotations enable view-based filtering across all registers.

```spec
person Analyst {
    description: "Business analyst reviewing revenue metrics.";
    @tag("stakeholder", "primary-user");
}

external system PaymentGateway {
    description: "Stripe payment processing API.";
    technology: "REST/HTTPS";
}

Analyst -> AnalyticsDashboard : "Reviews revenue dashboards.";
```

**Systems specification** defines architecture: a hierarchical decomposition rooted at a `system` node, where every child is either `authored` (we build it) or `consumed` (we depend on it). Authored components carry responsibilities, internal structure, build phases with gate conditions, and contracts. Consumed components carry version constraints and boundary expectations. Topology rules (with enriched edges carrying technology and description), traceability mappings, cross-cutting constraints, and package policies are all first-class constructs.

**Deployment specification** maps logical components onto infrastructure: deployment environments (Production, Staging), infrastructure nodes (servers, cloud services, pods), and component instances that link the system tree to operational topology.

**View and dynamic specification** defines architectural diagrams and runtime behavior. View declarations implement model-vs-views separation: the model is defined once, views select subsets at different zoom levels (system landscape, system context, container, component, deployment). Dynamic declarations capture numbered interaction sequences for specific scenarios.

**Design specification** defines what users see and interact with: pages, visualizations, parameter bindings, layout intent, and behavioral commitments. Prose intent is architecturally associated with formal declarations, so the LLM receives both during realization.

The full language definition is in the [SpecLang Specification](docs/specification/SpecLang-Specification.md). The formal EBNF grammar is in the [SpecLang Grammar](docs/specification/SpecLang-Grammar.md).

## The Standard Extension

An opt-in extension allows specifications to encode the architectural rules of Hassan Habib's [The Standard](https://github.com/hassanhabib/The-Standard). It adds layer-prefixed declaration forms (broker, foundation service, processing service, orchestration service, exposer, test), layer contracts that attach behavioral obligations to every component at a given layer, realization directives for advisory code-generation conventions, and semantic validation rules (topology checking against the layer hierarchy, Florance Pattern enforcement, entity ownership, autonomy constraints, vocabulary checking, validation ordering).

The extension is activated by an `architecture TheStandard` declaration in the base system spec. When absent, the base language operates unchanged.

See the [Extension Overview](docs/specification/extensions/the-standard/TheStandard-Extension-Overview.md) for full details.

## Repository Layout

The repository organizes content into a single `docs/` tree for reading material, a `src/` tree for code, and supporting directories for tests, templates, and archived material.

| Path | Description |
|---|---|
| `docs/theory/` | Whitepaper (*Enquiry Into Specification as Meaningful Struggle*) and citations |
| `docs/specification/SpecChat-Overview.md` | Motivation, design rationale, and overview of the SpecChat system |
| `docs/specification/SpecLang-Specification.md` | Language definition: constructs, semantics, five registers of specification |
| `docs/specification/SpecLang-Grammar.md` | Formal EBNF grammar: lexer tokens, productions, ambiguity resolution |
| `docs/specification/extensions/the-standard/` | The Standard extension: overview, specification, grammar |
| `docs/examples/blazor-harness/` | Canonical sample (small, complete): manifest and system spec for a Blazor analytics harness |
| `docs/examples/global-corp/` | Scale exemplar: a fictional BTABOK-complete enterprise (Global Corp Inc.) shown both as a [narrative architecture document](docs/examples/global-corp/Global-Corp-Exemplar.md) and as the corresponding spec collection (~22 files) |
| `docs/design-notes/` | Internal design records, type system, versioning policy, decision log. Repo-only, not published to the website |
| `docs/design-notes/btabok/` | BTABOK integration material (out-of-scope models, glossary, implementation plan, exemplar essay). Repo-only |
| `docs/assets/` | CSS and KaTeX assets used by the published documentation site |
| `src/MCPServer/DotNet/` | SpecChat MCP server and Language SDK (.NET 10) |
| `src/MCPServer/DotNet/examples/` | Example specs the .NET tooling exercises (PayGate, PizzaShop, SendGate, TodoApp) |
| `tests/fixtures/` | Developer-facing test specs, including failure-mode fixtures |
| `templates/copilot/` | Workspace template for VS Code + GitHub Copilot users |
| `archive/` | Retired design material, kept for provenance |

## Where to Start

1. This README for orientation.
2. The [Enquiry](docs/theory/Enquiry-Into-Specification-as-Meaningful-Struggle.md) for the theoretical foundation: why specification, why a formal medium, what the realization engine concept means.
3. The [SpecChat Overview](docs/specification/SpecChat-Overview.md) for how the system is put together.
4. The [SpecLang Specification](docs/specification/SpecLang-Specification.md) and [Grammar](docs/specification/SpecLang-Grammar.md) for the full language definition.
5. The [Blazor Harness sample](docs/examples/blazor-harness/blazor-harness.spec.md) (and its [manifest](docs/examples/blazor-harness/blazor-harness.manifest.md)) for a small concrete example.
6. The [Global Corp exemplar](docs/examples/global-corp/Global-Corp-Exemplar.md) for SpecLang at scale (narrative document, paired with the formal spec collection in the same directory).

## Installation

There are two ways to use SpecChat: as context files for Claude Code, or as an MCP server for any MCP-compatible IDE.

### Claude Code

The Claude Code install bundle is assembled from two locations in this repository: the language definition under `docs/specification/` and the canonical sample under `docs/examples/blazor-harness/`. Copy them into your Claude Code configuration directory under a single `spec-chat/` folder:

| Platform | Destination |
|---|---|
| macOS / Linux | `~/.claude/spec-chat/` |
| Windows | `%USERPROFILE%\.claude\spec-chat\` |

The destination should look like this once both copies are complete (note that `blazor-harness/` from `docs/examples/` is renamed to `samples/` to match the layout Claude Code expects):

```
.claude/
  spec-chat/
    SpecChat-Overview.md           ← from docs/specification/
    SpecLang-Specification.md      ← from docs/specification/
    SpecLang-Grammar.md            ← from docs/specification/
    extensions/
      the-standard/                ← from docs/specification/extensions/
        TheStandard-Extension-Overview.md
        TheStandard-Extension-Specification.md
        TheStandard-Extension-Grammar.md
    samples/                       ← from docs/examples/blazor-harness/ (renamed)
      blazor-harness.manifest.md
      blazor-harness.spec.md
```

Claude Code will read these files as context when working with `.spec.md` documents.

### MCP Server

The SpecChat MCP server is a .NET 10 application that exposes SpecLang parsing, validation, and code generation as [Model Context Protocol](https://modelcontextprotocol.io/) tools. It works with any MCP-compatible IDE, including VS Code (Copilot agent mode) and Visual Studio 2026.

The server provides five tool groups:

| Tool group | What it does |
|---|---|
| **Parsing** | Parse `.spec.md` files and individual spec blocks into an AST; list declared constructs |
| **Validation** | Semantic validation, topology checking, trace coverage, phase gate consistency, package policy compliance |
| **Standard extension** | Layer hierarchy, Flow Forward, Florance Pattern, entity ownership, autonomy, and vocabulary validation |
| **Manifest** | Parse manifests, check lifecycle transitions, identify next executable specs, advance spec state |
| **Realization** | Extract contracts and entities, generate project scaffolds, produce interface/type definitions |

#### Run from source

Requires the [.NET 10 SDK](https://dotnet.microsoft.com/download).

Add an MCP server entry pointing at the project. The configuration file location depends on your IDE:

- **VS Code**: `.vscode/mcp.json` in your workspace
- **Visual Studio**: `.mcp.json` in your solution directory
- **Claude Code**: `.mcp.json` in your project root

```json
{
  "servers": {
    "specchat-mcp": {
      "type": "stdio",
      "command": "dotnet",
      "args": [
        "run",
        "--project",
        "<path-to-repo>/src/MCPServer/DotNet/SpecChat.Mcp"
      ]
    }
  }
}
```

#### Install from NuGet (once published)

```json
{
  "servers": {
    "specchat-mcp": {
      "type": "stdio",
      "command": "dnx",
      "args": ["SpecChat.Mcp", "--version", "0.1.0-beta", "--yes"]
    }
  }
}
```

The source is in [`src/MCPServer/DotNet/`](src/MCPServer/DotNet/).

## License

[MIT](LICENSE)
