# Core SpecLang Absorption Design

## Tracking

| Field | Value |
|---|---|
| Document ID | GC-CSA-001 |
| itemType | DesignDocument |
| slug | core-speclang-absorption-design |
| Version | 0.1 |
| Created | 2026-04-17 |
| Last Reviewed | 2026-04-17 |
| State | Draft |
| publishStatus | Draft |
| retentionPolicy | indefinite |
| Freshness SLA | 90 days |
| Owner | PER-01 Lena Brandt, Chief Architect |
| Approver | PER-11 Anja Petersen, Chair EARB |
| Dependencies | BTABOK-Profile-v0.1-Design.md, BTABOK-MCP-Server-Integration-Design.md, CoDL-CaDL-Integration-Notes.md |

## 1. Purpose

This document records the decision to absorb a specific set of concepts, originally planned as BTABOK-profile extensions, directly into Core SpecLang. It specifies what moves, what stays in the BTABOK profile, what is deferred for later absorption, and what ripple-effect updates are needed across the existing design corpus and the codebase work.

The decision was reached after evaluating each BTABOK element against three criteria: universal applicability, worldview neutrality, and low incremental cost. The elements that pass all three benefit every SpecChat collection regardless of profile and do not impose a specific practice model on users who do not want it.

## 2. Decision

**Option X is adopted.** The Strong Absorption Candidates move into Core SpecLang as part of the same implementation work that would otherwise have delivered them inside the BTABOK profile. The Moderate and Borderline candidates remain on Option Y: they stay in the BTABOK profile for v0.1 and are observed for broader use before core absorption is reconsidered.

## 3. What Absorbs Into Core SpecLang

Seven elements move to Core SpecLang.

### 3.1 Standard Metadata Profile (core subset)

Every spec and every concept in Core SpecLang carries a standard metadata profile. This replaces the ad-hoc tracking-block fields currently in use with a named, typed set.

**Core SpecItem metadata fields:**

| Field | Type | Required | Purpose |
|---|---|---|---|
| `slug` | slug (URL-safe, lowercase, hyphenated) | yes | Stable human-readable identifier |
| `itemType` | shortText | yes | The spec or concept type name |
| `name` | shortText | yes | Primary human-readable label |
| `shortDescription` | text | no | One-line summary |
| `version` | integer | yes | Monotonic version number |
| `publishStatus` | enum(Draft, Reviewed, Approved, Executed, Verified) | yes | Current lifecycle state |
| `authors` | list of PersonRef | yes, min 1 | People who authored this artifact |
| `reviewers` | list of PersonRef | no | People who reviewed it |
| `committer` | PersonRef | yes | Single accountable owner |
| `tags` | list of shortText | no | Freeform classification |
| `createdAt` | datetime | yes | When the artifact was created |
| `updatedAt` | datetime | yes | When the artifact was last modified |
| `retentionPolicy` | enum(indefinite, archive-on-deprecated, delete-on-superseded) | yes | Lifecycle disposition |
| `freshnessSla` | duration | required when `retentionPolicy == indefinite` | Maximum review interval |
| `lastReviewed` | date | required when `retentionPolicy == indefinite` | Most recent review date |
| `dependencies` | list of ref | no | Upstream artifacts this depends on |

**PersonRef:**

```
PersonRef {
  userId: id
  name: shortText
  email: email optional
  addedAt: datetime optional
  addedBy: id optional
}
```

SpecChat's existing lifecycle states (Draft, Reviewed, Approved, Executed, Verified) are retained as canonical. The CoDL publishStatus vocabulary (Draft, Review, Approved, Published, Retired) is preserved as an interop mapping for BTABOK-profile collections, not as a core replacement. Vocabulary alignment is deferred (see Section 5).

### 3.2 Reference Types

Three reference types become first-class Core SpecLang constructs for cross-document references.

| Reference type | Semantics | Resolution policy |
|---|---|---|
| `ref<T>` | Strong reference to a concept or spec instance in the same collection | Must resolve at validation time. Unresolved references are errors. |
| `weakRef` | Reference where the target may not exist locally | Resolution attempted; unresolved references are warnings. Used for cross-collection links or intentional forward references. |
| `externalRef` | Reference to a non-SpecChat system (issue tracker, document store, design tool) | Format-validated only. Carries `{ system: shortText, refId: shortText, url: url optional }`. |

Reference resolution becomes a core SpecLang capability. The `CollectionIndex` service (introduced in BTABOK-MCP-Server-Integration-Design.md) moves from the BTABOK profile to Core SpecLang.

### 3.3 Relationship Declarations with Cardinality

Relationship declarations move to core. Core specs can declare typed relationships with explicit cardinality:

```
relationships {
  uses<BaseSystemSpec>      as implements  cardinality(1..1)
  uses<DecisionSpec>        as decisions   cardinality(0..*)
  implements<BaseSystemSpec> as base       cardinality(1..1)
  supersedes<self>          as supersedes  cardinality(0..1)
  supersededBy<self>        as supersededBy cardinality(0..1)
}
```

The relationship kinds (`uses`, `implements`, `supersedes`, `supersededBy`) and the `cardinality(<range>)` syntax are core. Profile-specific relationship kinds can be added by extensions.

### 3.4 Retention Policy

The `retentionPolicy` enum and the associated `freshnessSla` and `lastReviewed` fields move to core as part of the standard metadata profile. Every spec carries a retention disposition:

- **indefinite**: The spec is ever-green and continuously maintained. Requires `freshnessSla` and `lastReviewed`.
- **archive-on-deprecated**: The spec is point-in-time. It remains accessible after it is superseded but is marked deprecated.
- **delete-on-superseded**: The spec is removed when superseded. Used for artifacts that should not accumulate historical versions.

Defaults vary by spec type: base system specs default to `indefinite`; feature, bug, and amendment specs default to `archive-on-deprecated`.

### 3.5 Diagnostic Record Extensions

Three optional fields are added to the core `Diagnostic` record.

| Field | Type | Purpose |
|---|---|---|
| `code` | shortText | Searchable diagnostic code (for example `SPEC-REF-001`, `SPEC-SLUG-002`) |
| `validator` | shortText | Name of the validator that produced the diagnostic |
| `suggestion` | text | Optional fix hint shown to the author |

Diagnostic code namespacing:
- `SPEC-` prefix for core SpecLang validators
- `STANDARD-` prefix for The Standard profile validators
- `BTABOK-` prefix for BTABOK profile validators

A diagnostic code registry (a single markdown file cataloging every defined code) is introduced alongside core SpecLang. New validators register their codes in the registry; duplicates are rejected at test time.

### 3.6 Slug Uniqueness and Format Rules

Slug format is normative in core SpecLang:
- Lowercase letters, digits, and hyphens only
- Must start with a letter
- No consecutive hyphens
- Maximum 64 characters

Slugs are unique within a collection. Duplicate slugs produce error `SPEC-SLUG-001`.

### 3.7 Core Validators (absorbed)

The following validators move from the BTABOK profile roster to the core validator roster. They run on every SpecChat collection regardless of profile.

| Core validator | Code range | Checks |
|---|---|---|
| `check_metadata_completeness` | SPEC-MET | Every spec has a complete core metadata profile |
| `check_slug_uniqueness` | SPEC-SLUG | Slugs are unique within the collection |
| `check_slug_format` | SPEC-SLUG | Slugs match the format rules |
| `check_reference_resolution` | SPEC-REF | All `ref<T>` targets exist in the collection |
| `check_weakref_resolution` | SPEC-REF | All `weakRef` targets are declared as intentionally external (warning only) |
| `check_externalref_validity` | SPEC-REF | `externalRef` entries have valid system/refId/url format |
| `check_freshness_sla` | SPEC-FRS | Every `retentionPolicy: indefinite` spec has `lastReviewed + freshnessSla` not in the past |
| `check_profile_composition` | SPEC-PRF | Exactly one profile is declared at the collection level |
| `check_relationship_cardinality` | SPEC-REL | Relationship declarations respect their cardinality bounds |
| `check_supersedes_cycles` | SPEC-REF | No cycles in `supersedes` chains |

Ten core validators absorbed.

## 4. What Remains in the BTABOK Profile

Elements that encode a specific architecture-practice worldview stay in the BTABOK profile. These remain opt-in and activate only when the profile is declared.

### 4.1 BTABOK concept types (profile-only)

- `GovernanceBody`, `GovernanceRule`, `WaiverRecord`
- `StakeholderCard` with power/interest grid
- `CapabilityCard` with baseline and target maturity
- `RoadmapItem`, `TransitionArchitecture`
- `StandardCard` with adoption status
- `RiskCard` with impact and probability
- `ScorecardDefinition`, `MetricDefinition`
- `PrincipleCard`
- `ExperimentCard`
- `ASRCard`
- `DecisionRecord` (in its full BTABOK form with scope, type, method, reversibility, cascades)
- `ViewpointCard`
- `CanvasDefinition`
- `QualityAttributeScenario`
- `LegacyModernizationRecord`

The BTABOK profile still carries 16 concept types. It is smaller than the original 19 because three universal infrastructure concerns (metadata, references, retention) are now core.

### 4.2 BTABOK-specific metadata extensions

CoDL's `BTABoKItem` metadata extends the core SpecItem with BTABOK-specific fields. These remain profile-only:

- `accessTier: enum(Free, Member, Paid, Restricted)`
- `bokStatus: enum(Add, Update, Remove, Archive)`
- `certainty: score1to10`
- `baseVersion: integer`
- `topicAreaId`, `publishedBokId`, `collectionId`, `documentId`, `baseDocumentId`
- `referenceCollectionId`, `referenceDocumentId`, `commentThreadId`
- `imageId`, `databaseId`, `tableId`

A BTABOK-profile spec carries a combined metadata profile: core SpecItem fields plus BTABoK-specific fields.

### 4.3 BTABOK-specific validators

Of the 10 core validators absorbed, 8 were drawn from the original 21 BTABOK validators and 2 are new core concepts (`check_relationship_cardinality`, `check_supersedes_cycles`) introduced during the absorption work. The original 21 BTABOK validators minus the 8 absorbed leaves 13 BTABOK-profile validators:

- `check_asr_traceability`, `check_asr_addressed_by_decision`
- `check_decision_scope_type`, `check_decision_cascades`
- `check_principle_links`
- `check_stakeholder_coverage`
- `check_viewpoint_coverage`
- `check_waiver_expiration`, `check_waiver_rule_reference`
- `check_governance_approval`
- `check_roadmap_capability_moves`
- `check_canvas_target_exists`
- `check_metric_baseline_target`

These retain the `BTABOK-` diagnostic code prefix.

### 4.4 CaDL canvases remain profile-only at v0.1

All 20 CaDL canvas renderings remain in the BTABOK profile. Canvas/projection as a general SpecLang concept is deferred to Option Y observation (see Section 5).

## 5. What Is Deferred (Option Y)

The following are observed for real-use evidence in BTABOK-profile collections before core absorption is reconsidered. Each is worth revisiting after Phase 4 of the BTABOK Profile v0.1 implementation completes.

- **`publishStatus` vocabulary alignment** with CoDL's Draft/Review/Approved/Published/Retired. Currently: SpecChat-native names retained in core, CoDL vocabulary is an interop mapping inside the BTABOK profile.
- **Canvas/projection as a first-class core concept**. Currently: CaDL canvases live in the BTABOK profile only.
- **Decision Spec enrichment** with scope, type, method, reversibility, cascades. Currently: BTABOK DecisionRecord has these; core Decision Spec does not.
- **Viewpoint as a reusable template distinct from view**. Currently: core has `view` declarations; BTABOK has `ViewpointCard` as a reusable template.
- **ASR as a generalized spec type** in core. Currently: core has contracts, invariants, and constraints; BTABOK has explicit ASRCard.
- **MetricDefinition** as a general measurement concept. Currently: BTABOK profile only.
- **ExperimentCard** as a general innovation-management concept. Currently: BTABOK profile only.

Each deferred item has a clear observation question: does BTABOK-profile usage show sustained demand for this concept in non-architecture-practice contexts?

## 6. Impact on Existing Design Documents

The following documents need updates to reflect the Option X decision. These updates are enumerated here; actual revisions are a follow-up task.

### 6.1 BTABOK-Profile-v0.1-Design.md

- Section 4 (CoDL Concept Catalog): remove the three absorbed concepts (none) but note that Standard Metadata fields, reference types, and relationship declarations are now core, not profile-specific. Concept definitions still use these, but they are no longer BTABOK inventions.
- Section 5 (CaDL Canvas Catalog): unchanged.
- Section 6 (Manifest Type Registry): the core metadata fields move to a core section; BTABOK adds only the profile-specific metadata extensions.
- Section 8.2 (Validator list): reduce from 21 to 11 BTABOK-specific validators. Add a note that 10 core validators run automatically on BTABOK collections as they do on every collection.
- Section 9 (Diagnostic model): update the code namespace section to include SPEC-, STANDARD-, and BTABOK- prefixes.
- Section 10 (Cross-Document Reference Resolution): note that this is now core behavior.
- Appendix A (CoDL concept definitions): update meta blocks to reference core SpecItem metadata plus BTABoKItem extensions.

### 6.2 BTABOK-MCP-Server-Integration-Design.md

- Section 3 (requirements mapping): the rows for slug, metadata, reference types, relationship declarations, retention policy, and diagnostic codes move from "BTABOK profile only" to "core SpecLang."
- Section 4 (layer change map): core SpecLang changes expand; BTABOK profile changes contract. Specifically:
  - Lexer keywords for reference types and relationship verbs become core
  - AST additions for RelationshipsBlock, RelationshipDecl, reference type nodes, and core metadata become core
  - Parser additions (not Parser.Btabok.cs) handle the core absorbed syntax
  - Manifest model changes become core, not BTABOK-specific
  - New validators in the absorbed set get implemented as core validators
  - `CollectionIndex` and `ValidatorSeverityPolicy` become core services
- Section 5 (Phasing): Phase 2 splits into Phase 2a (core validators) and Phase 2b (BTABOK-specific validators). Phase 1 absorbs additional core-lexer and core-AST work that was previously scoped to BTABOK.
- Section 7 (Open Decisions): D-05 (diagnostic code governance) becomes a core concern rather than a BTABOK concern.

### 6.3 CoDL-CaDL-Integration-Notes.md

- Section 3 (mapping table): the rows for slug, owner/approver, ever-green, cross-document reference, and retention policy move from "WIP corpus construct maps to CoDL" to "absorbed into core SpecLang, interop with CoDL."
- Section 5 (Standard BTABoK Metadata): add a section distinguishing the core SpecItem metadata subset from the BTABoKItem extension set.
- Section 9 (Scope Discipline): adjust the in-scope list for the BTABOK profile to exclude the absorbed concerns.

### 6.4 BTABOK-EngagementModel-Mapping.md

- Section 11 (CoDL and CaDL Alignment): add a subsection noting that the Option X decision moves specific infrastructure concerns into core.
- Integration Layers section: reclassify several items from "Requires New Spec Type" or "Requires DSL Extension" down to "Core SpecLang."
- Section "What Is Out" update: clarify that absorbed elements are now universal core features, not BTABOK features.

### 6.5 Spec-Type-Taxonomy-v0.1.md

- Section 0 (alignment note): update to reference this document.
- Section 5.1 (Spec type vs CoDL concept type): add a third distinction: some CoDL metadata fields are core SpecLang, not BTABOK-profile specific.

### 6.6 Spec-Type-Validation-Analysis.md

- DSL Extension candidate list: the candidates that handled reference resolution, metadata validation, and slug governance are now core concerns and move out of the BTABOK-specific candidates list.
- Recommended Validation Architecture: Layer 3 now includes core validators absorbed from the BTABOK list; Layer 4 retains only profile-specific validation.

### 6.7 Global-Corp-BTABOK-Enterprise-Architecture.md

- The "slug is a core concept" and "authors/committer is a core field" annotations throughout the document are still correct; their meaning shifts from "CoDL convention" to "core SpecLang convention."
- Section 28 (View Gallery) remains as BTABOK-profile content since canvases are deferred (Option Y).
- Appendix B (SpecChat Manifest Scaffolding): the core SpecLang metadata columns apply to every spec regardless of profile annotation.

## 7. Impact on Codebase Work

The Option X decision materially reshapes the MCP Server integration phasing.

### 7.1 Phase 1 (Foundation) expands

Lexer additions for reference-type tokens (`ref`, `weakRef`, `externalRef`) and relationship verbs (`uses`, `implements`, `supersedes`, `supersededBy`, `cardinality`) become core lexer changes, not profile-specific. AST additions for metadata blocks, reference expressions, and relationship declarations are core.

### 7.2 Phase 2 splits

- **Phase 2a (Core validators).** The 10 absorbed validators get implemented as core SpecLang validators. They run on every collection regardless of profile. Delivered as `CoreSemanticAnalyzer` extensions or as a new `CoreMetadataAnalyzer` class.
- **Phase 2b (BTABOK-specific core infrastructure).** CollectionIndex and ValidatorSeverityPolicy are implemented, now as core services rather than profile services.
- **Phase 2c (BTABOK-profile validators).** The 11 profile-specific validators go into `BtabokSemanticAnalyzer`.

### 7.3 Open Decision D-04 partially settled

The question of where collection-level analysis lives now has a forced answer: `CollectionIndex` is a core service because core validators need it. The question that remains is organizational: `SemanticAnalyzer.AnalyzeCollection()` versus a new `WorkspaceAnalyzer` class.

### 7.4 Testing strategy expands

The core test suite gains known-good and known-bad fixtures for the 10 absorbed validators. These fixtures must cover `Core`, `TheStandard`, and `BTABOK` profiles because the core validators run on all three.

## 8. Migration of Existing Samples

The existing sample collections (`blazor-harness`, `TodoApp`, `PizzaShop`, `todo-app-the-standard`) must be updated to meet the new core metadata requirements, regardless of whether they adopt a profile.

### 8.1 Required updates per sample

For each spec in the existing samples:
- Add `slug` to the tracking block
- Add `retentionPolicy` (most will be `indefinite` for base specs, `archive-on-deprecated` for feature/bug/amendment)
- Add `freshnessSla` and `lastReviewed` for indefinite specs
- Add `authors` and `committer` (use existing owner information from tracking blocks where available; otherwise add a placeholder author)
- Convert any informal cross-references to `ref<T>` syntax

### 8.2 Sample manifest updates

Each sample manifest gains:
- A `Profile` declaration (most will declare `Core`; `todo-app-the-standard` declares `TheStandard`)
- The `TypeRegistry` section
- Updated inventory columns (slug, retentionPolicy, freshnessSla)

### 8.3 Migration timing

Sample migration is part of Phase 2a (core validators). The validators cannot ship without samples that exercise them. This tightens the coupling between validator implementation and sample update.

## 9. Open Sub-Decisions

The Option X decision itself is settled. These sub-decisions remain open and should be worked alongside the decisions already called out in BTABOK-MCP-Server-Integration-Design.md Section 7.

### Sub-Decision SD-01: Core metadata required fields strictness

**Options:**
- A. All core metadata fields are required on every spec immediately.
- B. Required fields are required on new specs; existing specs are grandfathered with a warning until next revision.
- C. Only `slug`, `itemType`, `name`, `committer`, `retentionPolicy` are required; others are optional.

**Tradeoffs:** Option A is strictest and cleanest. Option B eases migration of existing samples. Option C is minimal-viable.

**Open question:** How much rework are we willing to do on existing samples during Phase 2a?

### Sub-Decision SD-02: PersonRef source

**Options:**
- A. PersonRef is inline in every spec (name, email, userId per spec).
- B. PersonRef resolves to a separate "persons" directory spec, referenced by `ref<PersonRef>`.
- C. Hybrid: inline name is required; userId resolves to an optional directory.

**Tradeoffs:** Option A is simplest. Option B gives a single source of truth for people. Option C balances them.

**Open question:** Is there enough identity management to justify a separate persons directory in core SpecLang, or should it be a BTABOK-profile concept?

### Sub-Decision SD-03: Retention policy default when unspecified

**Options:**
- A. No default; retentionPolicy must be explicit on every spec.
- B. Default is `indefinite` for base system specs, `archive-on-deprecated` for evolution specs (feature/bug/amendment/decision).
- C. Default is `archive-on-deprecated` universally; base specs must explicitly declare `indefinite`.

**Tradeoffs:** Option A is strictest. Option B minimizes author burden by matching expected conventions. Option C makes the ever-green status an intentional declaration.

**Open question:** Which is the better default for the "invisible" case where a spec author does not think about retention?

### Sub-Decision SD-04: Core validator naming convention

**Options:**
- A. Core validators use `check_<concern>` naming (e.g., `check_slug_format`).
- B. Core validators use `validate_<concern>` naming (e.g., `validate_slug_format`).
- C. Keep `check_` for all validators (core, Standard, BTABOK) for uniformity with existing code.

**Tradeoffs:** Option C preserves existing precedent. Options A and B introduce no substantive change over C.

**Recommendation mild:** Option C (keep existing `check_` prefix).

### Sub-Decision SD-05: CollectionIndex initialization timing

**Options:**
- A. Build CollectionIndex on first validator invocation; cache until manifest changes.
- B. Build CollectionIndex as part of manifest parsing; always current.
- C. Build on demand, no caching.

**Tradeoffs:** Option A balances performance and correctness. Option B couples parsing to indexing. Option C is simplest but slowest.

**Open question:** Does the MCP server have a natural lifecycle hook for index invalidation (for example, on file change notifications)?

## 10. Summary of the Shift

Core SpecLang before this decision handled: data entities, context, systems, topology, phases, traces, deployment, views, dynamics, design. Metadata was informal (tracking blocks). References were informal (filename strings). Diagnostics had severity only.

Core SpecLang after this decision additionally handles: stable human-readable identifiers, standard metadata profile, typed cross-document references, relationship declarations with cardinality, retention policy, freshness tracking, diagnostic codes, and ten validators that enforce these.

The BTABOK profile still adds specific architecture-practice concepts (governance, stakeholders, capabilities, roadmaps, principles, waivers, risks, scorecards, experiments, ASRs, decisions in their richer form, viewpoints, canvases). It is materially smaller than the pre-decision scope because the universal infrastructure is no longer profile-specific.

The SpecLang ecosystem after Option X has a cleaner layering: core handles what every spec collection needs; profiles add what specific practices want.

## 11. Immediate Next Steps

1. Settle sub-decisions SD-01 through SD-05 (scope similar to the Phase 1 settle items in BTABOK-MCP-Server-Integration-Design.md).
2. Produce the ripple-effect updates to the five impacted design documents (Section 6).
3. Reframe the MCP Server integration phasing per Section 7.
4. Start a diagnostic code registry at `docs/diagnostic-codes.md` (or similar).

## 12. Source References

**[R1]** BTABOK Profile v0.1 Design. Workspace: [BTABOK-Profile-v0.1-Design.md](BTABOK-Profile-v0.1-Design.md).

**[R2]** BTABOK MCP Server Integration Design. Workspace: [BTABOK-MCP-Server-Integration-Design.md](BTABOK-MCP-Server-Integration-Design.md).

**[R3]** CoDL and CaDL Integration Notes. Workspace: [CoDL-CaDL-Integration-Notes.md](CoDL-CaDL-Integration-Notes.md).

**[R4]** BTABOK Engagement Model Mapping. Workspace: [BTABOK-EngagementModel-Mapping.md](BTABOK-EngagementModel-Mapping.md).

**[R5]** Spec Type Taxonomy v0.1. Workspace: [Spec-Type-Taxonomy-v0.1.md](Spec-Type-Taxonomy-v0.1.md).

**[R6]** Spec Type Validation Analysis. Workspace: [Spec-Type-Validation-Analysis.md](Spec-Type-Validation-Analysis.md).

**[R7]** Global Corp BTABOK Enterprise Architecture. Workspace: [Global-Corp-BTABOK-Enterprise-Architecture.md](Global-Corp-BTABOK-Enterprise-Architecture.md).

**[R8]** Preiss, Paul. "Structured Concept Definition Language." BTABoK 3.2, IASA Global Education Portal (2026).
