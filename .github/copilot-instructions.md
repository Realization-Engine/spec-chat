# SpecChat Project Instructions

## Core Principle

The `.spec.md` file is the source of truth. Code is a projection of the specification, not the other way around. When the spec and the code disagree, the spec wins unless a decision spec or amendment has been filed.

## Specification-First Workflow

1. Before modifying code, read the relevant `.spec.md` file. Check contracts for boundary commitments, invariants for field constraints, and rationale for design reasoning.
2. Before adding a feature, read the `.manifest.md` file. It declares the full spec inventory, lifecycle states, dependency ordering, and execution tiers.
3. When generating new code, extract contracts and entities from the spec. The generated code must conform to declared contracts and satisfy all invariants.
4. After implementation, verify against the spec's contracts and constraints. Run any phase gate commands declared in the spec.

## Spec Block Syntax

Specification blocks use fenced code blocks with the `spec` language identifier:

````
```spec
entity MyEntity {
    field: Type;
    invariant "rule name": expression;
}
```
````

These blocks contain formal SpecLang constructs: entities, enums, contracts, invariants, components, topology, phases, traces, constraints, package policies, and platform realization.

## The Manifest

The manifest (`.manifest.md`) is the entry point for any spec collection. It contains:
- The spec inventory with lifecycle states (Draft, Reviewed, Approved, Executed, Verified)
- Execution order tiers based on dependency analysis
- Tracking block conventions for incremental specs
- Document type definitions (base spec, feature, bug, decision, amendment)

Always read the manifest first when encountering a spec collection.

## MCP Server Tools

The `specchat-mcp` server provides validation and query tools. Use these to:
- Parse and validate spec blocks against the SpecLang grammar
- Check topology rules, trace completeness, and phase gate conditions
- Extract contracts, entities, and constraints from spec files
- Read manifests and determine next executable specs
- Update spec lifecycle state after implementation

## Key Conventions

- Spec files use the `.spec.md` extension
- Manifest files use the `.manifest.md` extension
- Each incremental spec carries a Tracking block with lifecycle state and dependencies
- Lifecycle states are sequential: Draft, Reviewed, Approved, Executed, Verified (no skipping)
- Components are either `authored` (we build them) or `consumed` (we use them; someone else built them)
- Package policies govern which external packages are pre-approved, prohibited, or require justification
