# TodoApp -- System Manifest

## Tracking

| Field | Value |
|---|---|
| Created | 2026-04-06 |
| State | Draft |
| Reviewed | |
| Approved | |
| Executed | |
| Verified | |
| Dependencies | None (root document) |

## System

| Field | Value |
|---|---|
| System | TodoApp |
| Base spec | todo-app-the-standard.spec.md |
| Target | net10.0 |
| Spec count | 1 |

This manifest governs the specification collection for the TodoApp system. The base spec defines the system skeleton: architecture declaration, layer-prefixed components, topology, phases, layer contracts, data entities, boundary contracts, constraints, and traceability.

## Lifecycle States

Every spec document moves through a defined sequence of states. Each transition is recorded in the Tracking block with a date.

| State | Meaning | Tracked by |
|---|---|---|
| Draft | Written, not yet reviewed for correctness | Created date |
| Reviewed | Passed consistency check and grammar audit | Reviewed date |
| Approved | Ready to execute; decisions resolved, dependencies satisfied | Approved date |
| Executed | Code implemented or amendments applied to base spec | Executed date |
| Verified | Post-execution confirmation passed | Verified date |

**Rules:**
- States are sequential: Draft, Reviewed, Approved, Executed, Verified.
- A spec cannot skip states.
- Decision specs enter Approved when the recommendation is accepted.
- Feature and bug specs enter Executed when their code implementation is complete.
- Amendment specs enter Executed when their corrections are applied to the base spec.
- The base spec enters Executed when it has been used to generate or validate source code.
- Verified requires an independent check (test pass, audit, or review) confirming the execution is correct.

## Tracking Block Convention

Every spec document must contain a Tracking block immediately after the title, before the first content section.

Format:

| Field | Value |
|---|---|
| Created | YYYY-MM-DD (file creation date) |
| State | Current lifecycle state |
| Reviewed | YYYY-MM-DD or blank |
| Approved | YYYY-MM-DD or blank |
| Executed | YYYY-MM-DD or blank |
| Verified | YYYY-MM-DD or blank |
| Dependencies | Spec filenames with parenthetical notes, or "None" |

## Document Type Registry

| Type | Purpose | Template |
|---|---|---|
| Base system spec | System skeleton: architecture, components, topology, phases, layer contracts, data, contracts | todo-app-the-standard.spec.md |
| Feature spec | New capability: component additions, entities, constraints, tests | (none yet) |
| Bug spec | Source gap the spec identifies: current vs. specified behavior, acceptance criteria | (none yet) |
| Decision spec | Conflict between spec and source: options, trade-offs, recommendation | (none yet) |
| Amendment | Base spec correction without new capability | (none yet) |
| Manifest | Root document binding a spec collection | todo-app-the-standard.manifest.md |

## Conventions

### Writing Style
- No em-dashes. Use commas, semicolons, colons, or separate sentences.
- No emoticons.
- No purple prose or marketing-type phrases.
- Precise file paths and source references.

### Architecture
- This system follows The Standard (Hassan Habib, version 1.0).
- All authored components use layer-prefixed declaration forms.
- Layer contracts and realization directives are defined in the base spec.

### SpecLang Syntax
- Formal declarations follow the grammar defined in SpecLang-Grammar.md with The Standard extension.
- Layer-prefixed forms (`broker`, `foundation service`, `exposer`, `test`) are preferred over generic `authored component`.
- `requires` and `ensures` take expressions; `guarantees` takes prose strings.
- `@validation` annotations classify preconditions as structural, logical, or external.
- `invariant` requires a name string, colon, and expression.

## Spec Inventory

| Filename | Type | State | Tier | Dependencies |
|---|---|---|---|---|
| todo-app-the-standard.spec.md | Base | Draft | -- | None |

## Execution Order

### Tier 0: Base System (Draft)

The base spec defines the complete system skeleton. Once reviewed and approved, it can be executed to generate the initial codebase.

1. todo-app-the-standard.spec.md

## Open Items

None at this time.
