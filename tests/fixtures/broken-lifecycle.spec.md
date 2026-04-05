# Broken Lifecycle -- Negative Test Manifest

## Tracking

| Field | Value |
|---|---|
| Created | 2026-04-05 |
| State | Draft |
| Reviewed | |
| Approved | |
| Executed | |
| Verified | |
| Dependencies | None (root document) |

## System

| Field | Value |
|---|---|
| System | TestSystem |
| Base spec | test.spec.md |
| Target | net10.0 |
| Spec count | 3 |

## Lifecycle States

| State | Meaning | Tracked by |
|---|---|---|
| Draft | Written, not yet reviewed | Created date |
| Reviewed | Passed consistency check | Reviewed date |
| Approved | Ready to execute | Approved date |
| Executed | Code implemented | Executed date |
| Verified | Post-execution check passed | Verified date |

## Spec Inventory

| Filename | Type | State | Tier | Dependencies |
|---|---|---|---|---|
| test.spec.md | Base | Approved | -- | None |
| feature-one.spec.md | Feature | InvalidState | 1 | None |
| feature-two.spec.md | Feature | Reviewed | 1 | nonexistent-spec.spec.md |

## Execution Order

### Tier 0: Base

1. test.spec.md

### Tier 1: Features

2. feature-one.spec.md
3. feature-two.spec.md
