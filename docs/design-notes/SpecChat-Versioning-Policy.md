# SpecChat Versioning Policy

## Tracking

| Field | Value |
|---|---|
| Document ID | GC-VPL-001 |
| itemType | PolicyDocument |
| slug | specchat-versioning-policy |
| Version | 0.1.0 |
| Created | 2026-04-17 |
| Last Reviewed | 2026-04-17 |
| State | Draft |
| publishStatus | Draft |
| retentionPolicy | indefinite |
| Freshness SLA | 180 days |
| Owner | PER-01 Lena Brandt, Chief Architect |
| Approver | PER-11 Anja Petersen, Chair EARB |
| Dependencies | SpecLang-Design.md |

## 1. Purpose

This document establishes the versioning policy for SpecChat. It applies to Core SpecLang, all SpecChat-owned profiles (The Standard, BTABOK, and future profiles), and the relationships those have with external language dependencies (CoDL, CaDL). It also specifies the compatibility, migration, and instance-version rules that govern how versions change over time and how validators respond to version declarations.

The policy is the outcome of five working-decision sessions (VD-1 through VD-5). It is the authoritative reference for any versioning question in the SpecChat ecosystem.

## 2. Scope

**In scope:**
- Version identifiers for Core SpecLang
- Version identifiers for SpecChat-owned profiles
- Compatibility declarations in manifests
- Migration policy between versions
- Instance version evolution on concept records

**Out of scope:**
- CoDL and CaDL versioning (externally owned by IASA)
- ModelContextProtocol SDK versioning (externally owned)
- Third-party profiles authored outside SpecChat (they follow this policy by convention, not by enforcement)

## 3. The Five Rules

### Rule 1: Core SpecLang Version Is Declared in the Manifest

**Source decision:** VD-1 Option B.

Core SpecLang carries an explicit version identifier. Collections declare the SpecLang version they target via the manifest field `specLangVersion`.

- Scheme: semver (`MAJOR.MINOR.PATCH`).
- Starting value: `0.1.0`.
- Pre-1.0 convention: breaking changes may occur in MINOR bumps; PATCH is backward-compatible only.
- Post-1.0 convention: standard semver (breaking in MAJOR, features in MINOR, fixes in PATCH).
- Validators publish a supported range per Rule 3.

### Rule 2: Profiles Use Semver

**Source decision:** VD-2 Option A.

Every SpecChat-owned profile uses semver for its version identifier.

- Applies to The Standard, BTABOK, and any future SpecChat-authored profile (Value Model, People Model, and so on).
- The Standard's existing informal `"1.0"` becomes `1.0.0`.
- BTABOK's proposed `0.1` becomes `0.1.0`.
- External profile authors are encouraged but not required to follow the same scheme.

**External dependencies (CoDL, CaDL) retain their upstream versions verbatim.** CoDL's decimal major.minor (`0.2`) and CaDL's decimal major.minor (`0.1`) are not rewritten as semver. The policy boundary is clear: SpecChat-owned versions use semver; externally-owned versions use whatever the upstream publishes.

### Rule 3: Compatibility Is Warnings-First

**Source decision:** VD-3 Option D.

When a manifest declares a version outside the validator's supported range, the default response is a warning, not an error. The validator proceeds with best-effort processing.

Applied to each version dimension:

| Dimension | Example supported range | Response to out-of-range |
|---|---|---|
| `specLangVersion` | `>= 0.1.0, < 0.2.0` | Warning `SPEC-VER-001` |
| `profileVersion` | `>= 0.1.0, < 0.2.0` (per profile) | Warning `SPEC-VER-002` |
| `codlVersion` | `0.2.x` | Warning `SPEC-VER-003` |
| `cadlVersion` | `0.1.x` | Warning `SPEC-VER-004` |

The publishing mechanism for these supported ranges is the `get_supported_versions` MCP tool described in Section 8.

Escalation to error is available via the `governancePosture: strict` manifest setting or the CLI `--strict` flag (per D-09 Option D). Manifest-as-floor: CLI can escalate but cannot relax below the manifest's declared posture.

The governance rationale is explicit: version compatibility follows the same "governed, not governing" principle as validation. Soft enforcement by default; strict enforcement on opt-in.

### Rule 4: Migration Is Opt-In With a Deprecation Schedule

**Source decision:** VD-4 Option B.

Collections migrate on their own timeline. The SpecChat server publishes a deprecation schedule that declares when older versions lose support.

**Deprecation lifecycle for an older version:**

1. **Current.** Version is in the supported range. Validators process it without diagnostics.
2. **Soft-deprecated.** Version is outside the current preferred range but still supported. Diagnostic severity: `info`. Message includes the sunset release.
3. **Hard-deprecated.** Version is scheduled for removal within one minor server release. Diagnostic severity: `warning`. Composition with Rule 3 means users already see a warning for out-of-range versions; the hard-deprecated message refines it with a sunset date.
4. **Removed.** Version is no longer supported. Diagnostic severity: `error`. Validators reject collections at this version unless `governancePosture: permissive` is set (a safety valve for recovering ancient collections, not for day-to-day use).

Deprecation transitions are declared in the server release notes and exposed via an MCP tool (`get_deprecation_schedule`).

**Migration tools are optional.** Each release may ship a migration tool for the transition it introduces, but tooling is not a release blocker. Manual migration remains the fallback.

### Rule 5: Instance Versions Are Author Discretion

**Source decision:** VD-5 Option D.

The `version: integer` field on CoDL concept records increments at the author's discretion. SpecChat does not mandate a specific increment rule.

**Monotonicity is enforced.** The core validator `check_version_monotonicity` (diagnostic `SPEC-VER-005`, severity `warning`) warns when a concept's version integer decreases relative to any previously observed version for the same `slug`. Increase is always valid; decrease is always suspicious.

**Recommended convention (documented, not enforced):** bump `version` on publishStatus transitions to `Approved` or later. This gives the version count an auditable meaning ("this concept has been formally accepted N times") without forcing the practice on authors who prefer a different convention.

## 4. Manifest Declaration Pattern

A BTABOK-profile manifest after this policy declares the following version fields:

```markdown
| specLangVersion | 0.1.0 |
| Profile         | BTABOK |
| profileVersion  | 0.1.0 |
| codlVersion     | 0.2 |
| cadlVersion     | 0.1 |
```

A Core-profile collection declares only the SpecLang version and profile name:

```markdown
| specLangVersion | 0.1.0 |
| Profile         | Core |
```

A TheStandard-profile collection:

```markdown
| specLangVersion | 0.1.0 |
| Profile         | TheStandard |
| profileVersion  | 1.0.0 |
```

Notice the mixed-scheme visibility: SpecChat-owned versions are semver (`0.1.0`, `1.0.0`), externally-owned versions use their upstream scheme (`0.2`, `0.1`). This is intentional and documented per Rule 2.

## 5. Current Version Baseline (2026-04-17)

These are the versions declared by SpecChat at the time of this policy's establishment.

| Artifact | Current version | Supported range (server expectation) |
|---|---|---|
| Core SpecLang | `0.1.0` | `>= 0.1.0, < 0.2.0` |
| BTABOK profile | `0.1.0` | `>= 0.1.0, < 0.2.0` |
| The Standard profile | `1.0.0` | `>= 1.0.0, < 2.0.0` |
| SpecChat.Mcp NuGet package | `0.1.0-beta` | N/A (external artifact) |
| ModelContextProtocol SDK | `1.2.0` | N/A (external dependency) |
| CoDL | `0.2` | `0.2.x` |
| CaDL | `0.1` | `0.1.x` |

The baseline is updated on each SpecChat release. Historical baselines are preserved in a changelog (not part of this document).

## 6. Diagnostic Code Reservation

The `SPEC-VER-` diagnostic code namespace is reserved for version-related diagnostics produced by this policy's validators.

| Code | Meaning | Default severity |
|---|---|---|
| `SPEC-VER-001` | `specLangVersion` out of supported range | warning |
| `SPEC-VER-002` | `profileVersion` out of supported range | warning |
| `SPEC-VER-003` | `codlVersion` out of supported range | warning |
| `SPEC-VER-004` | `cadlVersion` out of supported range | warning |
| `SPEC-VER-005` | Instance `version` decrease detected | warning |
| `SPEC-VER-006` | Version declared in deprecated range (sunset approaching) | info or warning per deprecation stage |
| `SPEC-VER-007` | Version declared in removed range | error (unless `governancePosture: permissive` is set as a safety valve for recovering ancient collections) |

All `SPEC-VER-` diagnostics are subject to the severity-override rules in D-09 (manifest can escalate, CLI can escalate further, manifest cannot be escalated below by CLI).

## 7. Interactions with Other Settled Decisions

This policy composes with the architectural decisions settled alongside it.

| Interaction | Effect |
|---|---|
| D-09 (treat-warnings-as-errors) | Version warnings follow the same escalation path as validator warnings. Manifest `governancePosture: strict` or CLI `--strict` promotes all `SPEC-VER-` warnings to errors. |
| D-07 (cross-profile references) | `weakRef` targets in other collections may declare different versions; tolerance applies per Rule 3 regardless of source. |
| D-11 (profile declaration placement) | All version declarations live in the manifest. Specs do not declare versions individually (except the instance `version` field in their metadata). |
| D-04 (WorkspaceAnalyzer) | Version-range checks run at collection-level analysis, producing diagnostics through the shared DiagnosticBag. |
| D-05 (diagnostic code governance) | `SPEC-VER-` codes are part of the core diagnostic registry, built during Phase 1. |

## 8. Implementation Notes

**Phase 1 (foundation) additions required by this policy:**

- Manifest parser recognizes `specLangVersion`, `profileVersion`, `codlVersion`, `cadlVersion` fields.
- `ManifestDocument` record gains corresponding properties.
- Validator constants declare supported ranges per version dimension.
- Diagnostic codes `SPEC-VER-001` through `SPEC-VER-007` registered in the code registry.
- `get_supported_versions` MCP tool returns the server's current supported ranges for all version dimensions.
- `get_deprecation_schedule` MCP tool returns the server's declared deprecation lifecycle.

**Phase 2a (core validators) additions:**

- `check_version_compatibility` validator runs during manifest parsing and during any collection-level operation. Checks each declared version against its supported range.
- `check_version_monotonicity` validator runs during collection-level analysis. Uses `CollectionIndex` historical tracking (or, in absence of in-process history, git log if available) to detect version decreases.

**Backfill:**

- The Standard's existing version declaration `"1.0"` is normalized to `1.0.0` during Phase 2a sample migration.
- Existing sample manifests (blazor-harness, TodoApp, PizzaShop, todo-app-the-standard) gain `specLangVersion: 0.1.0` declarations during Phase 2a.

## 9. Open Items for Future Policy Revisions

These concerns are acknowledged but deferred beyond v0.1.

1. **Semantic-diff tooling.** When a version bump claims to be a PATCH, tooling that confirms no API surface changed would be valuable. Not in v0.1 scope.
2. **Automatic migration tool framework.** Rule 4 allows migration tools but does not specify a framework for authoring them. A future policy revision may define a standard shape for migration tools.
3. **Third-party profile certification.** External profile authors currently follow this policy by convention. A future policy revision may define a formal certification process.
4. **CoDL version negotiation.** If CoDL ever publishes v1.0 with a different versioning scheme, this policy may need to evolve its "external scheme" rule.
5. **Deprecation-schedule persistence.** The per-release deprecation schedule lives in release notes today. A machine-readable form may be useful later.

## 10. Revision History

| Policy version | Date | Summary |
|---|---|---|
| 0.1.0 | 2026-04-17 | Initial policy establishing VD-1 through VD-5 as settled rules. |

## 11. Source References

**[R1]** SpecLang Design. Workspace: [SpecLang-Design.md](SpecLang-Design.md).

**[R2]** Spec Type System. Workspace: [Spec-Type-System.md](Spec-Type-System.md).

**[R3]** MCP Server Integration Design. Workspace: [MCP-Server-Integration-Design.md](MCP-Server-Integration-Design.md). Contains the architectural decision series (D-01 through D-16) that this policy's VD series complements.

**[R4]** Global Corp Exemplar. Workspace: [Global-Corp-Exemplar.md](Global-Corp-Exemplar.md). Applies this policy's version declarations in its manifest scaffolding.

**[R5]** Semver specification. `https://semver.org/`. The upstream semver 2.0.0 specification.

**[R6]** Paul Preiss. Structured Concept Definition Language. BTABoK 3.2, IASA Global Education Portal (2026). Defines CoDL and CaDL versioning conventions that SpecChat accepts as external upstream.
