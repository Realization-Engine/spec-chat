---
description: 'SpecLang manifest conventions for .manifest.md files'
applyTo: '**/*.manifest.md'
---

# Manifest Conventions

## Purpose

The manifest is the entry point for a spec collection. It binds all specifications for a system into a governed collection with lifecycle tracking and execution ordering.

## Tracking Block

Every incremental spec (feature, bug, decision, amendment) carries a Tracking block in its frontmatter or header section:

```
### Tracking

| Field        | Value                        |
|--------------|------------------------------|
| Created      | YYYY-MM-DD                   |
| State        | Draft                        |
| Reviewed     | -                            |
| Approved     | -                            |
| Executed     | -                            |
| Verified     | -                            |
| Dependencies | SPEC-001, SPEC-003           |
```

Each date field is filled when the spec reaches that lifecycle state. Dependencies list spec identifiers that must reach Executed state before this spec can be executed.

## Lifecycle States

States are sequential. No skipping is allowed.

1. **Draft** -- initial authoring; spec is being written or revised.
2. **Reviewed** -- spec has been read and assessed for correctness and completeness.
3. **Approved** -- spec is accepted for implementation. No further content changes without a new amendment.
4. **Executed** -- code has been generated or modified to conform to the spec.
5. **Verified** -- generated code has been verified against the spec's contracts, invariants, and test obligations.

A spec cannot move to Executed until all its Dependencies have reached Executed. A spec cannot move to Verified until its own Executed state is confirmed and verification passes.

## Document Types

| Type           | Purpose                                                    |
|----------------|------------------------------------------------------------|
| Base spec      | Declares the system: components, topology, phases, entities, pages. One per system. |
| Feature spec   | Declares a new capability. Adds components, entities, contracts, test obligations. |
| Bug spec       | Declares a source gap the spec correctly identifies. Current vs. specified behavior. |
| Decision spec  | Resolves conflicts between spec and source. Options, recommendation, amendments. |
| Amendment      | Corrects the base spec without adding capability. Structural adjustments only. |
| Manifest       | This document type. Governs the collection. One per system. |

## Spec Inventory Table

The manifest contains a table listing every spec in the collection:

```
## Spec Inventory

| ID       | Title                     | Type     | State    | Dependencies |
|----------|---------------------------|----------|----------|--------------|
| SPEC-001 | Base System Specification | base     | Verified | -            |
| SPEC-002 | User Authentication       | feature  | Approved | SPEC-001     |
| SPEC-003 | Fix Revenue Calculation   | bug      | Draft    | SPEC-001     |
```

## Execution Order Tiers

Specs are grouped into tiers based on dependency analysis. All specs in a tier can be executed in parallel; all specs in a prior tier must reach Executed first.

```
## Execution Order

### Tier 1 (no dependencies)
- SPEC-001: Base System Specification

### Tier 2 (depends on Tier 1)
- SPEC-002: User Authentication
- SPEC-003: Fix Revenue Calculation

### Tier 3 (depends on Tier 2)
- SPEC-004: Role-Based Access Control
```

Within a tier, specs are independent and can be executed in any order.
