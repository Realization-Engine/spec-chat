# PizzaShop -- System Manifest

## Tracking

| Field | Value |
|---|---|
| Created | 2026-04-16 |
| State | Draft |
| Reviewed | |
| Approved | |
| Executed | |
| Verified | |
| Dependencies | None (root document) |

## System

| Field | Value |
|---|---|
| System | PizzaShop |
| Base spec | PizzaShop.spec.md |
| Target | net10.0 |
| Spec count | 3 |

This manifest governs the specification collection for the PizzaShop system. The base spec (PizzaShop.spec.md) defines the system skeleton: components, topology, phases, traces, contracts, constraints, data entities, pages, visualizations, package policy, and platform realization. PayGate.spec.md and SendGate.spec.md define companion test harness subsystems that stand in for Stripe and SendGrid in dev and test environments.

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

**Rules:**
- Created uses the file creation date.
- State reflects the current lifecycle state.
- Date fields are set when the corresponding state transition occurs.
- Dependencies lists other spec filenames that must reach Executed state before this spec can be executed.
- "None" if the spec has no prerequisites beyond the base spec.

## Document Type Registry

| Type | Purpose | Template |
|---|---|---|
| Base system spec | System skeleton: components, topology, phases, traces, constraints, data, contracts, pages | PizzaShop.spec.md |
| Subsystem spec | Companion subsystem with its own components, topology, phases, and deployment | PayGate.spec.md |
| Feature spec | New capability: component additions, entities, constraints, tests | (none yet) |
| Bug spec | Source gap the spec identifies: current vs. specified behavior, acceptance criteria | (none yet) |
| Decision spec | Conflict between spec and source: options, trade-offs, recommendation | (none yet) |
| Amendment | Base spec correction without new capability | (none yet) |
| Manifest | Root document binding a spec collection | PizzaShop.manifest.md |

## Conventions

### Writing Style
- No em-dashes. Use commas, semicolons, colons, or separate sentences.
- No emoticons.
- No purple prose or marketing-type phrases.
- Precise file paths and source references.

### SpecLang Syntax
- Formal declarations (authored component, entity, trace, contract, constraint, rationale) follow the grammar defined in SpecLang-Grammar.md.
- `requires` and `ensures` take expressions; `guarantees` takes prose strings.
- `invariant` requires a name string, colon, and expression.
- `rationale` uses either simple form (`rationale STRING;`) or structured form (`rationale { context ...; decision ...; consequence ...; }`).

### Subsystem Specs
- PayGate and SendGate are independent subsystems with their own authored components, data entities, contracts, and deployment.
- Each gate mimics the REST API surface of its target external system (Stripe, SendGrid).
- The base spec references the gates as `external system` nodes in its Context section and includes them in the Development deployment.
- Gate specs do not depend on the base spec reaching any particular state; they can be executed independently.
- The base spec's Integration phase gate should require running gate containers once the gate specs reach Executed state.

## Spec Inventory

| Filename | Type | State | Tier | Dependencies |
|---|---|---|---|---|
| PizzaShop.spec.md | Base | Draft | -- | None |
| PayGate.spec.md | Subsystem | Draft | 0 | None |
| SendGate.spec.md | Subsystem | Draft | 0 | None |

## Execution Order

Specs are grouped into tiers by dependency. All specs in a tier can be executed in parallel. A tier cannot begin until all dependencies from prior tiers have reached Executed state.

### Tier 0: Subsystem Specs (Draft)

PayGate and SendGate have no dependencies on the base spec and can be executed independently. They produce standalone services that the base system consumes via configuration.

1. PayGate.spec.md
2. SendGate.spec.md

### Base Spec (Draft)

The base spec defines the complete PizzaShop system. It can be executed independently of the subsystem specs; however, the Integration phase gate for external service testing requires the gate containers to be available.

3. PizzaShop.spec.md

## Open Items

- Once feature, bug, decision, or amendment specs are filed against PizzaShop, add them to the inventory and assign tiers.
- The Integration phase in PizzaShop.spec.md should add a gate requiring PayGate and SendGate containers once both subsystem specs reach Executed state.
