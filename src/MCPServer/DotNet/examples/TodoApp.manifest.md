# TodoApp -- System Manifest

## Tracking

| Field | Value |
|---|---|
| Created | 2026-04-05 |
| State | Reviewed |
| Reviewed | 2026-04-06 |
| Approved | |
| Executed | |
| Verified | |
| Dependencies | None (root document) |

## System

| Field | Value |
|---|---|
| System | TodoApp |
| Base spec | TodoApp.spec.md |
| Target | net10.0 |
| Spec count | 1 |

This manifest governs the specification collection for the TodoApp system. The base spec (TodoApp.spec.md) defines the system skeleton: components, topology, phases, traces, constraints, data entities, contracts, package policy, and platform realization. All future specs are incremental additions that reference the base.

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
- Feature and bug specs enter Executed when their code implementation is complete.
- Amendment specs enter Executed when their corrections are applied to the base spec.
- The base spec enters Executed when it has been used to generate or validate source code.
- Verified requires an independent check (test pass, audit, or review) confirming the execution is correct.

## Tracking Block Convention

Every spec document must contain a Tracking block immediately after the title and header metadata, before the first content section.

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

**Rules:**
- Created uses the file creation date.
- State reflects the current lifecycle state.
- Date fields are set when the corresponding state transition occurs.
- Dependencies lists other spec filenames that must reach Executed state before this spec can be executed.
- "None" if the spec has no prerequisites beyond the base spec.

## Document Type Registry

| Type | Purpose | Template |
|---|---|---|
| Base system spec | System skeleton: components, topology, phases, traces, constraints, data, contracts | TodoApp.spec.md |
| Feature spec | New capability: component additions, entities, constraints, tests | (none yet) |
| Bug spec | Source-gap correction: root cause, fix, regression tests | (none yet) |
| Decision spec | Architecture choice: options, recommendation, rationale | (none yet) |
| Amendment spec | Corrections to existing spec: field changes, invariant updates | (none yet) |

## Conventions

### Writing Style
- No em-dashes. Use commas, semicolons, colons, or separate sentences.
- No emoticons.
- Precise file paths and source references.

### SpecLang Syntax
- Formal declarations (authored component, entity, trace, contract, constraint, rationale) follow the SpecLang grammar.
- `requires` and `ensures` take expressions; `guarantees` takes prose strings.
- `invariant` requires a name string, colon, and expression.
- `rationale` uses either simple form (`rationale STRING;`) or structured form (`rationale { context ...; decision ...; consequence ...; }`).

## Spec Inventory

| Filename | Type | State | Tier | Dependencies |
|---|---|---|---|---|
| TodoApp.spec.md | Base | Reviewed | -- | None |

## Execution Order

Specs are grouped into tiers by dependency. All specs in a tier can be executed in parallel. A tier cannot begin until all dependencies from prior tiers have reached Executed state.

Currently only the base spec exists. Tiers will be added as incremental specs are filed.

## Open Items

None at this time.
