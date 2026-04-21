# BTABOK Profile v0.1 Design

## Tracking

| Field | Value |
|---|---|
| Document ID | GC-BPD-001 |
| CoDL itemType | ProfileDefinition |
| CoDL slug | btabok-profile-v0-1-design |
| Version | 0.1 |
| Created | 2026-04-17 |
| Last Reviewed | 2026-04-17 |
| State | Draft |
| CoDL publishStatus | Draft |
| CoDL retentionPolicy | indefinite |
| Freshness SLA | 90 days |
| Owner | PER-01 Lena Brandt, Chief Architect |
| Approver | PER-11 Anja Petersen, Chair EARB |
| Dependencies | CoDL-CaDL-Integration-Notes.md, BTABOK-EngagementModel-Mapping.md, Spec-Type-Taxonomy-v0.1.md, Spec-Type-Validation-Analysis.md |

## 1. Purpose and Scope

This document specifies version 0.1 of the BTABOK Profile for SpecLang. It is the implementation design that follows from the scoping, taxonomy, and alignment work recorded in the prior WIP documents.

**What this document produces:**
- The formal activation mechanism for the BTABOK profile
- The catalog of CoDL concept types the profile defines
- The catalog of CaDL canvases the profile ships with
- The Manifest Type Registry schema
- The validator enumeration and plug-in model
- The diagnostic model (warnings by default, treat-warnings-as-errors opt-in)
- Cross-document reference resolution rules
- Conflict and precedence rules
- The `/spec-btabok` authoring workflow
- Legacy spec migration path and backward-compatibility guarantees
- Test corpus plan

**What this document does not cover:**
- The BTABOK Profile v0.2 or later (future increments)
- Profiles for BTABoK models other than the Engagement Model (Value Model, People Model, Competency Model are out of scope)
- SpecLang core grammar changes (the profile does not modify the core language)
- CLI or MCP server implementation details beyond the validator plug-in points
- Repository tooling outside SpecChat itself (Jira, Confluence, Miro integrations are `externalRef` targets, not profile scope)

## 2. Architectural Recap

The following decisions are already settled and are not revisited in this document.

| Decision | Resolution | Source |
|---|---|---|
| Scope of BTABOK integration | Engagement Model only | BTABOK-EngagementModel-Mapping.md |
| Profile composition | One profile active at a time | User decision |
| Stable identifier model | CoDL `slug` (human-readable, URL-safe) | CoDL-CaDL-Integration-Notes.md |
| Cross-document reference model | CoDL `ref<T>`, `weakRef`, `externalRef` | CoDL-CaDL-Integration-Notes.md |
| Lifecycle state mapping | SpecChat lifecycle mapped onto CoDL `publishStatus` with Executed to Published and Verified as SpecChat-local refinement | CoDL-CaDL-Integration-Notes.md |
| Ownership and approval | CoDL `authors`, `reviewers`, `committer` from the Standard BTABoK Metadata profile | CoDL-CaDL-Integration-Notes.md |
| Governance posture | Warnings by default; treat-warnings-as-errors is a per-initiative opt-in | User decision |
| Ever-green vs. point-in-time | CoDL `retentionPolicy` enum; `freshnessSla` as SpecChat extension for indefinite records | CoDL-CaDL-Integration-Notes.md |
| Legacy spec migration | Legacy specs must be brought up to meet whatever profile they claim | User decision |
| Worked example basis | Global Corp enterprise architecture exemplar | User decision |
| Authoring command | `/spec-btabok` slash command | User decision |
| Validation responsibility | SpecLang must be robust enough to validate each spec | User decision |
| Canonical concept syntax | CoDL 0.2 with optional SpecLang-style aliases (Option A) | CoDL-CaDL-Integration-Notes.md |
| Core SpecLang absorption | Option X: Standard Metadata Profile fields, reference types, relationship declarations with cardinality, retention policy, diagnostic code extensions, slug rules, and ten core validators move from the BTABOK profile to Core SpecLang | Core-SpecLang-Absorption-Design.md |

## 3. Profile Activation and Composition

### 3.1 Profile declaration

A BTABOK-profile spec collection activates the profile via a declaration in the manifest's metadata table and, optionally, at the top of each base spec in the collection. The manifest declaration applies to the whole collection. The spec-level declaration is only permitted if it matches the manifest declaration (it cannot override to a different profile).

Manifest declaration (required):

```markdown
| Field | Value |
|---|---|
| System | Global Corp Supply Chain Platform |
| Base spec | global-corp.architecture.spec.md |
| Profile | BTABOK |
| Profile version | 0.1 |
| CoDL version | 0.2 |
| CaDL version | 0.1 |
```

Base spec declaration (optional, must match manifest):

```spec
profile BTABOK {
  version: "0.1";
  codl: "0.2";
  cadl: "0.1";
}
```

### 3.2 One profile at a time

A spec collection declares exactly one profile. The profile options at v0.1 are:

| Profile | Purpose |
|---|---|
| `Core` | Core SpecLang only; no profile extensions active |
| `TheStandard` | Hassan Habib's The Standard profile |
| `BTABOK` | The BTABOK Engagement Model profile defined by this document |

A collection cannot activate both `TheStandard` and `BTABOK` simultaneously. Composition of multiple profiles is deferred to a future SpecChat version and is out of scope for v0.1.

This decision does not prevent a BTABOK-profile collection from **referencing** systems built under a different profile. A Global Corp `global-corp.manifest.md` declaring `BTABOK` can hold `externalRef` pointers to a subsystem spec collection declaring `TheStandard`. The profile applies to the collection, not to the reference graph.

### 3.3 Profile version tracking

The profile declaration carries three version fields:

- `version`: The SpecChat BTABOK Profile version (this document is v0.1)
- `codl`: The CoDL specification version the profile targets (v0.2 at this writing)
- `cadl`: The CaDL specification version the profile targets (v0.1)

SpecLang validators use these to select the correct validation rules. A collection declaring an unsupported version is rejected with a clear diagnostic.

### 3.4 What activation changes

Activating the BTABOK profile makes the following available:

- CoDL concept definitions (canonical syntax: `concept <Name> { ... }`)
- CaDL canvas definitions (canonical syntax: `canvas <Name> for <Concept> { ... }`)
- The Standard BTABoK Metadata profile on every concept
- CoDL reference types (`ref<T>`, `weakRef`, `externalRef`)
- CoDL storage directives and transport envelopes
- The BTABOK-profile validator set (see Section 10)
- The `/spec-btabok` authoring command and its question paths
- The CaDL projection commands in the CLI

Activating the profile does NOT change:

- Core SpecLang grammar (the five registers and their constructs remain unchanged)
- Existing manifest lifecycle state names (Draft, Reviewed, Approved, Executed, Verified)
- The spec file naming convention
- Any spec authored under `Core` remains valid under `BTABOK` after migration (see Section 15)

## 4. CoDL Concept Catalog

The BTABOK profile defines 14 CoDL concept types at v0.1. The concepts are grouped by the BTABOK concern they serve.

> Note: The Standard Metadata Profile fields used on every concept (slug, itemType, name, shortDescription, version, publishStatus, authors, reviewers, committer, tags, createdAt, updatedAt, retentionPolicy, freshnessSla, lastReviewed, dependencies) are no longer BTABOK-specific. They live in Core SpecLang per the Core SpecLang Absorption design (see Appendix D). `BTABoKItem` is a profile-specific extension that adds BTABOK-specific fields (accessTier, bokStatus, certainty, baseVersion, topicAreaId, and the other items listed in that design) on top of the core `SpecItem` metadata.

Each BTABOK-profile concept therefore carries a combined metadata profile: the Core SpecItem metadata (implicit, supplied by core SpecLang) plus the BTABoKItem extension (added by the profile). The concept definitions below and in Appendix A do not repeat the core fields; they are implicit. Profile-specific fields from BTABoKItem are what the BTABOK profile adds.

Full CoDL definitions for each concept are in Appendix A. This section describes their purpose, scope, and principal relationships.

### 4.1 Stakeholder and Concern Concepts

**StakeholderCard** (BTABoK standard concept)

Captures a stakeholder, their concerns, influence, and required viewpoints. Serves the Stakeholder viewpoint (VP-03). Sections include identity, concerns (list), influence (enum), engagement strategy, and required viewpoints (list of `ref<ViewpointCard>`).

**PrincipleCard** (BTABoK standard concept)

Captures an enterprise architecture principle, its statement, rationale, and links to the ASRs and decisions that implement or depend on it. Relationships include `implements<ASRCard>` and `uses<DecisionRecord>`.

### 4.2 Requirement and Decision Concepts

**ASRCard** (BTABoK standard concept)

Captures an architecturally significant requirement with its statement, significance classification, rationale, verification approach, and characteristics (complete, traceable, current, verifiable, valuable). Relationships include `implements<PrincipleCard>` and `usedBy<DecisionRecord>`.

**DecisionRecord** (BTABoK standard concept)

Captures an architecturally significant decision with scope, type, method, options considered, recommendation, reversibility, linked ASRs, linked principles, cascades, status, and decision date. This is the single most load-bearing concept in the profile. Relationships include `uses<ASRCard>`, `implements<PrincipleCard>`, `supersedes<self>`, `supersededBy<self>`, and `cascades<DecisionRecord>`.

### 4.3 Governance Concepts

**GovernanceBody**

Captures a governance body (ETSC, EARB, Domain Design Council, Regional Architecture Forum, Repository Stewardship Group). Sections include scope, authority, chair, membership, cadence. Relationships include `composedOf<PersonRef>` and `oversees<DecisionRecord>`.

**GovernanceRule**

Captures a governance rule (every material change requires a decision record, every exception requires a waiver, etc.). Sections include rule statement, scope, applicability, enforcement posture (warning vs. error by default), and linked ASRs. Relationships include `enforcedBy<GovernanceBody>`.

**WaiverRecord** (BTABoK standard concept)

Captures an approved exception to a rule, principle, or constraint. Required sections include rule reference, description, justification, risk, compensating controls, expiration, and approver. Relationships include `waives<PrincipleCard>` or `waives<GovernanceRule>` and `approvedBy<GovernanceBody>`. Carries `retentionPolicy: archive-on-deprecated` by convention (waivers are point-in-time).

### 4.4 View and Viewpoint Concepts

**ViewpointCard** (BTABoK standard concept)

Captures a reusable viewpoint template with audience, concerns answered, required models, and owner. Relationships include `addresses<ConcernRef>` and `renderedBy<CanvasDefinition>`.

**CanvasDefinition**

Captures a CaDL canvas declaration as a concept, so canvases themselves are first-class stored artifacts. This is a SpecChat-specific concept used to catalog the profile's canvas library. Relationships include `renders<ConceptType>`.

### 4.5 Quality and Standards Concepts

**QualityAttributeScenario**

Captures a quality attribute scenario with stimulus, environment, response, response measure, and threshold. Sections use CoDL's `threshold` composite type for the response measure. Relationships include `addressesASR<ASRCard>`.

**StandardCard**

Captures an external or internal standard adopted by the organization (EPCIS, DCSA, ISO 28000, NIST CSF, internal canonical schemas). Sections include standard reference, version, scope, adoption status, and review cadence. Relationships include `adoptedBy<SystemSpec>`.

### 4.6 Capability and Roadmap Concepts

**CapabilityCard** (BTABoK standard concept)

Captures a business or technology capability with baseline maturity, target maturity, strategic importance, and gap analysis. Relationships include `suppliedBy<SystemSpec>` and `targetedBy<TransitionArchitecture>`.

**RoadmapItem**

Captures an individual item on the strategic roadmap with milestone, sequence, dependencies, and target capability movements. Relationships include `dependsOn<RoadmapItem>` and `targets<CapabilityCard>`.

**TransitionArchitecture**

Captures a baseline-to-target transition plan with scope, baseline state, target state, sequencing, and dependencies. Relationships include `contains<RoadmapItem>` and `movesCapability<CapabilityCard>`.

### 4.7 Operational Concepts

**RiskCard**

Captures an architectural risk with impact, probability, owner, and mitigation. Relationships include `mitigatedBy<DecisionRecord>` or `mitigatedBy<PrincipleCard>`.

**ExperimentCard** (BTABoK standard concept)

Captures an innovation experiment with hypothesis, cost cap, duration, kill criteria, and success measures. Relationships include `sponsoredBy<PersonRef>` and `feedsInto<DecisionRecord>`.

**LegacyModernizationRecord**

Captures a legacy-system decommissioning or modernization plan with system identity, current state, target state, migration window, evidence preservation plan, and customer migration plan. Relationships include `replacedBy<SystemSpec>` and `usesWaiver<WaiverRecord>`.

### 4.8 Scorecard and Metric Concepts

**MetricDefinition**

Captures a metric with baseline, target, measurement method, measurement period, and owner. Relationships include `measures<CapabilityCard>` or `measures<ASRCard>`.

**ScorecardDefinition**

Captures a scorecard that aggregates metrics for a specific audience (executive, architecture, operational). Sections include audience, metric list, review cadence. Relationships include `aggregates<MetricDefinition>` and `presentedTo<StakeholderCard>`.

### 4.9 Concept count

19 concept types total at v0.1. The BTABOK-EngagementModel-Mapping referred to 14 concept types in its prior revision; this design adds `CanvasDefinition`, `MetricDefinition`, and `ScorecardDefinition` which emerged from the Global Corp exemplar work but were not formally enumerated in the mapping document. These three are SpecChat-local additions that use CoDL syntax but are not BTABoK-standard concepts. The v0.1 catalog therefore is: `StakeholderCard`, `PrincipleCard`, `ASRCard`, `DecisionRecord`, `GovernanceBody`, `GovernanceRule`, `WaiverRecord`, `ViewpointCard`, `CanvasDefinition`, `QualityAttributeScenario`, `StandardCard`, `CapabilityCard`, `RoadmapItem`, `TransitionArchitecture`, `RiskCard`, `ExperimentCard`, `LegacyModernizationRecord`, `MetricDefinition`, and `ScorecardDefinition`.

While 19 concept types are defined for the BTABOK profile, the infrastructure concerns underpinning them (metadata, references, retention, relationship declarations, diagnostic codes) are no longer BTABOK-invented. They are Core SpecLang features that the BTABOK concept definitions use. The profile contributes the concept shapes and the BTABoKItem metadata extension; the core plumbing that carries them is shared with every profile.

Full CoDL definitions in **Appendix A**.

## 5. CaDL Canvas Catalog

The BTABOK profile ships with a library of CaDL canvases that project its CoDL concepts into stakeholder-oriented views. Canvases are themselves CoDL concepts (of type `CanvasDefinition`) so they can be cataloged, owned, and versioned like any other concept.

### 5.1 Canvas inventory at v0.1

| Canvas | Targets concept(s) | Primary audience |
|---|---|---|
| `DecisionRegistry` | `DecisionRecord` (collection) | EARB, architects |
| `DecisionRecordCard` | `DecisionRecord` (single) | EARB, approvers |
| `ASRMatrix` | `ASRCard` (collection) | Architects, engineers |
| `ASRCard` | `ASRCard` (single) | Architects |
| `StakeholderMap` | `StakeholderCard` (collection) | Sponsors, change leaders |
| `PowerInterestGrid` | `StakeholderCard` (collection) | Sponsors |
| `ConcernMap` | `StakeholderCard` joined with `ViewpointCard` | Architects |
| `ViewpointCatalog` | `ViewpointCard` (collection) | Everyone |
| `ViewpointCoverageReport` | `ViewpointCard` joined with `CanvasDefinition` | Architects |
| `PrincipleCatalog` | `PrincipleCard` (collection) | Everyone |
| `CapabilityHeatMap` | `CapabilityCard` (collection) | Product, executives |
| `RoadmapTimeline` | `TransitionArchitecture` containing `RoadmapItem` | Product, executives |
| `WaiverRegister` | `WaiverRecord` (collection) | EARB, auditors |
| `WaiverCard` | `WaiverRecord` (single) | Approvers |
| `StandardsCatalog` | `StandardCard` (collection) | Engineering, partners |
| `FreshnessReport` | All concepts with `retentionPolicy: indefinite` | Repository Stewardship Group |
| `OutcomeScorecard` | `ScorecardDefinition` with `MetricDefinition` instances | Executives, CFO |
| `GovernanceApprovalFlow` | `DecisionRecord` lifecycle projection | EARB, architects |
| `RiskRegister` | `RiskCard` (collection) | EARB, CISO |
| `ExperimentBoard` | `ExperimentCard` (collection) | Innovation sponsors |

20 canvases at v0.1.

### 5.2 Canvas rendering

Canvases produce Mermaid diagrams, markdown tables, or a combination, depending on the canvas layout. The CLI command `spec project --canvas <CanvasName>` resolves the canvas definition, queries the concept instances it targets, and renders the output.

Canvases respect the governance posture. A canvas that references concepts behind their freshness SLA emits a warning in the rendered output (at the top of the produced artifact) by default. With `--strict` (the CaDL equivalent of treat-warnings-as-errors), the rendering fails with a clear message.

Full CaDL definitions in **Appendix B**.

## 6. Manifest Type Registry

The Manifest Type Registry is the canonical manifest schema for BTABOK-profile collections. SpecChat is pre-1.0 and there is no prior manifest version to be backward compatible with, so the registry is not versioned as "v2." It replaces the informal document type table previously used in the exemplar work with a structured definition the validator can enforce.

Fields used for metadata, retention, and cross-document references draw from Core SpecLang rather than from profile-specific conventions. The registry shape below therefore mixes core fields (which every profile sees) with BTABOK-specific fields (which apply only when `Profile.profileName = BTABOK`).

### 6.1 Registry structure

```
concept CollectionManifest {
  // The meta block carries the Core SpecItem metadata profile (slug, itemType,
  // name, shortDescription, version, publishStatus, authors, reviewers,
  // committer, tags, createdAt, updatedAt, retentionPolicy, freshnessSla,
  // lastReviewed, dependencies). Those fields are supplied by Core SpecLang
  // and apply to every profile. The only field below that is BTABOK-specific
  // is `accessTier`, contributed by the BTABoKItem metadata extension.
  meta {
    // Core SpecItem fields (supplied by Core SpecLang):
    slug: slug required
    itemType: shortText required                 // "CollectionManifest"
    name: shortText required
    shortDescription: text optional
    version: integer required
    publishStatus: enum(Draft, Review, Approved, Published, Retired) required
    authors: list<PersonRef> required min 1
    reviewers: list<PersonRef> optional
    committer: PersonRef required
    createdAt: datetime required
    updatedAt: datetime required

    // BTABoKItem extension fields (BTABOK profile only):
    accessTier: enum(Free, Member, Paid, Restricted) required
  }

  section Profile {
    profileName: enum(Core, TheStandard, BTABOK) required
    profileVersion: semver required
    codlVersion: semver optional                 // required when profileName = BTABOK
    cadlVersion: semver optional                 // required when profileName = BTABOK
  }

  section SystemMetadata {
    systemName: shortText required
    baseSpec: shortText required                 // filename of base spec
    targetFramework: shortText optional
    specCount: integer required
  }

  section TypeRegistry [list] {
    item {
      typeName: shortText required               // "ASRCard", "DecisionRecord", etc.
      codlConcept: shortText required            // CoDL concept name (usually == typeName)
      filePattern: shortText optional            // glob for specs of this type
      required: boolean required                 // whether every collection must have one
    }
  }

  section Inventory [list] {
    item {
      filename: shortText required
      codlItemType: shortText required
      slug: slug required
      name: shortText required
      publishStatus: enum(Draft, Review, Approved, Published, Retired) required
      tier: integer required                     // execution tier
      everGreen: boolean required
      retentionPolicy: enum(indefinite, archive-on-deprecated, delete-on-superseded) required
      freshnessSla: duration optional            // required when everGreen == true
      lastReviewed: date optional                // required when everGreen == true
      authors: list<PersonRef> required min 1
      committer: PersonRef required
      dependencies: list<ref<self>> optional
    }
  }

  section ExecutionOrder [list] {
    item {
      tier: integer required
      parallelizable: boolean required
      entries: list<ref<self.Inventory>> required min 1
    }
  }

  section Conventions {
    writingStyle: text optional
    specLangSyntax: text optional
    governancePosture: enum(warnings, strict) required default(warnings)
  }

  relationships {
    uses<StakeholderCard> as stakeholders cardinality(0..*)
    uses<ASRCard>         as requirements cardinality(0..*)
    uses<DecisionRecord>  as decisions    cardinality(0..*)
    uses<WaiverRecord>    as waivers      cardinality(0..*)
    uses<ViewpointCard>   as viewpoints   cardinality(0..*)
    uses<GovernanceBody>  as governanceBodies cardinality(0..*)
  }
}
```

### 6.2 What the registry enforces

The registry is machine-validated. Violations produce diagnostics per Section 11.

- Every spec file in `Inventory` must declare its `codlItemType` and that item type must appear in `TypeRegistry`.
- Every spec marked `everGreen: true` must declare a `freshnessSla`.
- Every spec's `lastReviewed` date plus `freshnessSla` must not be in the past (else: warning).
- Every reference in `dependencies` must resolve to another inventory entry (else: error).
- `ExecutionOrder` must be a complete partition of the inventory (every inventory entry appears in exactly one tier).
- Execution order respects the dependency graph (no tier N entry depends on a tier N+k entry).
- `Profile.profileName` must be `BTABOK` for this profile to apply; otherwise this document's rules do not engage.

### 6.3 Profile compatibility

SpecChat is pre-1.0; there is no prior manifest version to remain compatible with. Manifests that do not use this registry structure but declare `Profile.profileName: Core` are treated as `Core`-profile collections and pass through core validation only. Manifests that declare `Profile.profileName: BTABOK` but lack registry fields required here fail validation with migration diagnostics per Section 15.

## 7. Type Profile Contracts

Each CoDL concept type is simultaneously a Type Profile Contract in the sense of [Spec-Type-Validation-Analysis.md](Spec-Type-Validation-Analysis.md). The CoDL definition IS the contract. Required/optional fields, cardinality, reference resolution, and relationship declarations together constitute the validation rules for that type.

The contracts for the 17 concept types are the CoDL definitions in Appendix A. No separate contract document is maintained; the CoDL concept file is the single source of truth.

## 8. Validator Plug-in Model

### 8.1 Existing validator architecture

SpecChat's MCP server already exposes typed validation tools:
- `check_topology`
- `check_standard_layers`, `check_standard_vocabulary`
- `check_autonomy`, `check_florance`, `check_flow_forward`
- `check_entity_ownership`
- `check_lifecycle`, `check_traces`
- `check_package_policy`, `check_phase_gates`

The BTABOK profile extends this set without modifying existing validators.

### 8.2 Validators at v0.1

The validator roster splits into two tiers: core validators that run on every collection regardless of profile, and BTABOK-profile validators that run only when `Profile.profileName = BTABOK`.

**Core SpecLang validators (run on every collection regardless of profile):**

| Validator | Code | Checks | Severity default |
|---|---|---|---|
| `check_metadata_completeness` | SPEC-MET | Every spec has a complete core metadata profile | Error |
| `check_slug_uniqueness` | SPEC-SLUG | Slugs are unique within the collection | Error |
| `check_slug_format` | SPEC-SLUG | Slugs are URL-safe lowercase with hyphens | Error |
| `check_reference_resolution` | SPEC-REF | All `ref<T>` targets exist in the collection | Error |
| `check_weakref_resolution` | SPEC-REF | All `weakRef` targets are declared as intentionally external | Warning |
| `check_externalref_validity` | SPEC-REF | `externalRef` entries have valid `system`, `refId`, and `url` | Warning |
| `check_freshness_sla` | SPEC-FRS | Every indefinite-retention concept has valid `lastReviewed + freshnessSla` | Warning |
| `check_profile_composition` | SPEC-PRF | Exactly one profile is declared | Error |
| `check_relationship_cardinality` | SPEC-REL | Relationships respect declared cardinality bounds | Error |
| `check_supersedes_cycles` | SPEC-REF | No cycles in `supersedes` chains | Error |

`check_metadata_completeness` was previously called `check_codl_metadata` in earlier drafts of this profile. The rename reflects its move to Core SpecLang; it now validates the core metadata profile that every spec carries, not a BTABoK-specific metadata set.

**BTABOK-profile validators (active only under the BTABOK profile):**

| Validator | Code | Checks | Severity default |
|---|---|---|---|
| `check_asr_traceability` | BTABOK-ASR | Every ASR with structural significance has at least one trace to a component | Warning |
| `check_asr_addressed_by_decision` | BTABOK-ASR | Every ASR has at least one addressing `DecisionRecord` | Warning |
| `check_decision_scope_type` | BTABOK-DEC | Every `DecisionRecord` declares `scope` and `type` | Error |
| `check_decision_cascades` | BTABOK-DEC | Cascade references resolve to existing `DecisionRecord` instances | Error |
| `check_principle_links` | BTABOK-PRN | Every `PrincipleCard` links to at least one ASR it implements | Warning |
| `check_stakeholder_coverage` | BTABOK-STK | Every `StakeholderCard` concern has at least one addressing view or constraint | Warning |
| `check_viewpoint_coverage` | BTABOK-VPT | Every declared viewpoint has at least one conforming view | Warning |
| `check_waiver_expiration` | BTABOK-WVR | Every active `WaiverRecord` has expiration not in the past | Warning |
| `check_waiver_rule_reference` | BTABOK-WVR | Every `WaiverRecord` references an existing `PrincipleCard` or `GovernanceRule` | Error |
| `check_governance_approval` | BTABOK-GOV | Every approved `DecisionRecord` has a non-empty `committer` | Error |
| `check_roadmap_capability_moves` | BTABOK-RMP | Every `TransitionArchitecture` references valid `CapabilityCard` instances | Error |
| `check_canvas_target_exists` | BTABOK-CNV | Every `CanvasDefinition` targets an existing concept type | Error |
| `check_metric_baseline_target` | BTABOK-MET | Every `MetricDefinition` declares baseline and target | Warning |

10 core validators and 13 BTABOK-profile validators (23 total) at v0.1.

### 8.3 Plug-in contract

Each validator is an MCP tool that accepts a manifest reference and returns a diagnostic list. Diagnostic shape:

```
Diagnostic {
  severity: enum(error, warning, info) required
  code: shortText required              // e.g. "BTABOK-ASR-001"
  validator: shortText required          // e.g. "check_asr_traceability"
  target: ref<ConceptInstance> required
  message: text required
  suggestion: text optional
  sourceLine: integer optional           // for inline-embedded CoDL in markdown
}
```

Validators do not mutate specs. They only report. Fixing a diagnostic is an authoring action, possibly assisted by `/spec-btabok` or a targeted CLI command.

## 9. Diagnostic Model

### 9.1 Default posture

Warnings are surfaced; errors block. The default posture is:
- `error`: Spec collection fails validation. The manifest's `publishStatus` cannot advance past Draft.
- `warning`: Surfaced in the diagnostic report. Does not block. The manifest's `publishStatus` can still advance.
- `info`: Surfaced in the diagnostic report but not counted toward gating.

### 9.2 Treat warnings as errors

A per-initiative opt-in flag, `governancePosture: strict` in the manifest's Conventions section (or `--strict` on the CLI), promotes all warnings to errors. This is the mechanism for initiatives that require full governance compliance before advancing.

Each validator declares a default severity (Section 8.2). Collections can override severity per validator in the manifest, within these rules:
- `error` defaults cannot be demoted to warning or info. Errors stay errors.
- `warning` defaults can be promoted to error or demoted to info.
- `info` defaults can be promoted but not further demoted.

Override syntax in the manifest:

```markdown
| Validator | Severity override |
|---|---|
| check_stakeholder_coverage | error |
| check_externalref_validity | info |
```

### 9.3 Diagnostic code namespace

Diagnostic codes use one of three prefixes, followed by a three-letter category and a three-digit number. Example: `SPEC-REF-001`, `STANDARD-FLO-002`, `BTABOK-ASR-001`.

Prefixes:
- `SPEC-` for Core SpecLang validators (run on every collection)
- `STANDARD-` for The Standard profile validators
- `BTABOK-` for BTABOK profile validators

Core SpecLang (`SPEC-`) categories at v0.1:
- `MET` (metadata)
- `SLUG` (slug format and uniqueness)
- `REF` (reference resolution)
- `FRS` (freshness)
- `PRF` (profile composition)
- `REL` (relationships and cardinality)

BTABOK profile (`BTABOK-`) categories at v0.1:
- `ASR` (requirements)
- `DEC` (decisions)
- `PRN` (principles)
- `STK` (stakeholders)
- `VPT` (viewpoints)
- `WVR` (waivers)
- `GOV` (governance)
- `RMP` (roadmap)
- `CNV` (canvases)

A complete codes appendix is out of scope for v0.1; it will be published as a separate reference document maintained alongside Core SpecLang.

## 10. Cross-Document Reference Resolution

> Note: Cross-document reference resolution is a Core SpecLang capability, not a BTABOK-profile feature. The rules in this section (`ref<T>` must resolve, `weakRef` resolution failures are warnings, `externalRef` is format-validated only) apply to every collection regardless of profile. The BTABOK profile inherits this behavior rather than defining it. The corresponding diagnostic codes use the `SPEC-REF` category (see Section 9.3). This section describes how the core rules apply inside a BTABOK-profile collection.

### 10.1 The three reference types

| Type | Semantics | Resolution policy |
|---|---|---|
| `ref<ConceptType>` | Strong reference to a concept instance in the same collection | Must resolve at validation time. Unresolved references are errors (SPEC-REF-001). |
| `weakRef` | Reference where the target may not exist locally | Resolution attempted; unresolved references are warnings (SPEC-REF-002), not errors. Used for cross-collection references or intentional forward references. |
| `externalRef` | Reference to a non-BTABoK system (Jira, Confluence, ADO, Miro) | Not resolved by the validator; only syntactic validation applies (system, refId, url format). |

### 10.2 Resolution algorithm

1. All concept instances in the collection are indexed by `(itemType, slug)`.
2. For each `ref<T>` field, the validator looks up `(T, refId)` in the index. Miss produces an error.
3. For each `weakRef` field, the validator attempts the same lookup. Miss produces a warning.
4. For each `externalRef` field, the validator checks field format only.

### 10.3 Reference display

CaDL canvases dereference `ref<T>` using the pattern `shows: fieldName -> relatedField`. For example, `shows: requirements -> name` renders the `name` of each referenced ASRCard instance. The validator ensures the referenced field exists on the target concept.

### 10.4 Cycle detection

For relationships with `supersedes` and `supersededBy`, the validator walks the graph and reports cycles as errors (SPEC-REF-003). A Decision Record cannot supersede itself transitively.

## 11. Conflict and Precedence Rules

When a spec expresses rules that conflict at runtime, the following precedence applies.

### 11.1 Constraint vs. waiver

A `WaiverRecord` that waives a `PrincipleCard` or `GovernanceRule` takes precedence over the principle or rule for the scope and duration of the waiver. The validator:
1. Resolves the waiver's `waives<>` target
2. Suppresses the corresponding check for the waiver's scope
3. Emits an informational diagnostic citing the waiver ID
4. Enforces expiration: after expiration the waiver no longer suppresses the check

### 11.2 ASR vs. topology constraint

If an ASR declares a requirement that a topology constraint appears to violate, the reconciliation rule is:
1. The topology constraint is authoritative at the structural level (it is a CoDL `ref<>`-resolvable invariant).
2. The ASR is authoritative at the intent level.
3. A mismatch is an error unless a `DecisionRecord` explicitly reconciles them (the decision declares which of the two has priority and why).

This forces apparent conflicts to become explicit decisions, which is the BTABOK preferred posture.

### 11.3 Profile rule vs. manifest override

Validator severity overrides in the manifest (Section 9.2) are permitted only within the allowed directions. An attempt to demote an error to a warning is rejected with an error (BTABOK-PRF-001).

### 11.4 CoDL concept field vs. SpecLang alias

When a spec uses a SpecLang-style alias (e.g., `decision "..." { ... }`) that desugars to a CoDL concept, the canonical form wins on any apparent mismatch. Aliases are syntactic sugar; the validator normalizes to the canonical CoDL form before running checks.

## 12. Authoring Workflow: /spec-btabok

### 12.1 Purpose

`/spec-btabok` is the Claude Code slash command that guides authoring of BTABOK-profile specs. It parallels `/spec-chat` (Core profile) and `/spec-the-standard` (TheStandard profile).

### 12.2 Command entry

The command file lives at `.claude/commands/spec-btabok.md`. It accepts a natural-language description of the artifact to author and routes to the appropriate question flow based on the detected CoDL concept type.

### 12.3 Routing

The command detects intent from the opening message. Intents include:

| User intent signal | Routes to |
|---|---|
| "architecture decision", "ADR", "decide between" | Decision Record flow |
| "requirement", "ASR", "must" | ASR Card flow |
| "stakeholder", "who cares about" | Stakeholder Card flow |
| "waiver", "exception", "deviation" | Waiver Record flow |
| "viewpoint", "view for" | Viewpoint Card flow |
| "principle", "rule of thumb" | Principle Card flow |
| "risk", "worried about" | Risk Card flow |
| "experiment", "hypothesis" | Experiment Card flow |
| "roadmap", "transition" | Transition Architecture flow |
| "capability", "what we can do" | Capability Card flow |
| "standard", "adopting" | Standard Card flow |
| "legacy", "retire", "decommission" | Legacy Modernization Record flow |
| "new collection", "start a project" | Manifest plus base-spec flow |

### 12.4 Question flow structure

Each flow follows a staged pattern:

1. **Identity stage.** Collect slug, name, shortDescription, itemType (implicit from flow).
2. **Governance stage.** Collect authors, reviewers, committer, accessTier.
3. **Content stage.** Collect the concept-specific sections.
4. **Relationship stage.** Collect references to other concepts (existing or planned).
5. **Confirmation stage.** Preview the CoDL block; author can revise.
6. **Emit stage.** Write the spec file and update the manifest inventory.

### 12.5 Output

The output is a `.spec.md` file in the collection's configured directory, with:
- A tracking block matching the manifest convention
- A CoDL concept definition block for the captured artifact
- Any generated slugs, cross-references, and metadata
- A markdown section for prose explanation (author-editable)

### 12.6 Relationship to /spec-chat

`/spec-chat` continues to serve Core-profile authoring. When invoked in a BTABOK-profile collection, it suggests delegation to `/spec-btabok` but does not refuse (the author may intentionally want core-register authoring even inside a BTABOK collection, for example to edit a base system spec's topology).

## 13. Legacy Spec Migration

### 13.1 The migration rule

A legacy spec that claims the BTABOK profile must meet the profile's requirements before the claim is accepted. There is no grandfathering. A manifest declaring `Profile.profileName: BTABOK` but containing legacy specs without Standard BTABoK Metadata will fail validation.

### 13.2 Migration checklist per spec

A legacy spec migrates to BTABOK profile by completing these steps:

1. Add the Standard BTABoK Metadata profile to the tracking block (slug, itemType, publishStatus, retentionPolicy, authors, committer, certainty, createdAt, updatedAt).
2. Add the `codlItemType` declaration.
3. Convert any informal cross-references to `ref<T>`, `weakRef`, or `externalRef`.
4. Declare `everGreen` (true or false) and, if true, `freshnessSla`.
5. Add `lastReviewed` if `everGreen: true`.
6. Add validator severity overrides if the default posture is inappropriate for this spec.

### 13.3 Migration assistance

The CLI provides `spec migrate --profile BTABOK` which walks each spec in the collection, identifies missing fields, and produces a migration plan. The command does not auto-migrate; it produces a patch per spec for author review.

### 13.4 Validation during migration

A manifest in migration state can declare `Profile.migrationFrom: Core` (or `Profile.migrationFrom: TheStandard`). This temporarily relaxes reference-resolution errors to warnings during the transition. Once migration is complete (the author removes `migrationFrom`), full validation engages. The migration state itself is recorded as a spec of type `MigrationPlan` that tracks completed and pending items.

A `MigrationPlan` carries `retentionPolicy: archive-on-deprecated` and is archived once complete.

## 14. Backward Compatibility

### 14.1 Guarantees

- A collection using `Profile: Core` is not affected by this document.
- A collection using `Profile: TheStandard` is not affected by this document.
- Validators introduced by the BTABOK profile do not run against non-BTABOK collections.
- Existing SpecLang grammar is not modified; the BTABOK profile adds recognition for CoDL syntax only when active.

### 14.2 Breaking changes in future versions

This profile is v0.1. Breaking changes may occur before v1.0. Changes that would break v0.x specs must be noted in a migration guide and must support a migration window per the legacy migration process.

### 14.3 Forward compatibility

A v0.1 collection is valid against v0.2 of the profile if v0.2 only adds concepts, canvases, or validators. A v0.2 collection is not guaranteed valid against v0.1 (the newer features may be unrecognized).

## 15. Test Corpus Plan

### 15.1 Known-good specs

The Global Corp enterprise architecture exemplar provides the known-good corpus. Each spec file in Appendix B of the Global Corp document is intended as a passing test case for the profile's validators.

### 15.2 Known-bad specs

A parallel corpus of intentionally broken specs exercises each validator's negative path. At v0.1 the set includes at least one known-bad spec per validator (23 cases minimum: 10 core plus 13 BTABOK-profile).

The known-bad cases are tagged with their owning validator surface so that core cases can be reused across all three profiles (Core, TheStandard, BTABOK) and BTABOK-only cases remain scoped to BTABOK-profile runs.

**Core SpecLang (SPEC-) known-bad cases:**
- A cross-reference to a non-existent slug (`check_reference_resolution` error, SPEC-REF)
- A manifest declaring both `Core` and `BTABOK` profiles (`check_profile_composition` error, SPEC-PRF)
- An ever-green spec without a `freshnessSla` (`check_freshness_sla` error, SPEC-FRS)
- A cycle in `supersedes` chain (`check_supersedes_cycles` error, SPEC-REF-003)
- A severity override that demotes an error to a warning (SPEC-PRF, rejected at manifest load)

**BTABOK-profile (BTABOK-) known-bad cases:**
- An ASR without traceability to components (`check_asr_traceability` warning, BTABOK-ASR)
- A DecisionRecord without scope and type (`check_decision_scope_type` error, BTABOK-DEC)
- A WaiverRecord with expiration in the past (`check_waiver_expiration` warning, BTABOK-WVR)
- A canvas targeting a non-existent concept type (`check_canvas_target_exists` error, BTABOK-CNV)

### 15.3 Test corpus location

Known-good and known-bad test corpus lives under `samples/btabok-profile/` (path to be created during v0.1 implementation). Each test case is itself a small BTABOK-profile collection with a manifest plus the minimal set of specs needed to exercise the validator.

### 15.4 Continuous validation

Every validator listed in Section 8.2 must have at least one passing and one failing test case in the corpus before v0.1 is considered implementation-complete.

## 16. Open Items for Future Versions

These items are deferred to v0.2 or later.

1. **Profile composition.** Activating multiple profiles in one collection (e.g., BTABOK plus TheStandard for system-level specs).
2. **Value Model profile.** Extending to benefits, objectives, measures (Value Model), which is currently out of scope for the Engagement Model profile.
3. **People Model profile.** Extending to organization, roles, competencies.
4. **Bidirectional CoDL export.** Emitting standalone CoDL files for external tools and ingesting external CoDL files into SpecChat collections.
5. **Transport envelope in CLI.** Automatic envelope emission during cross-system export per CoDL 0.2 transport envelope specification.
6. **Diagnostic code reference document.** A complete catalog of every BTABOK-prefixed diagnostic code with examples.
7. **Additional canvases.** The v0.1 canvas library is 20 canvases. Future versions will add canvases as the exemplar corpus identifies gaps (for example, Innovation Funnel, Architecture Fitness Function dashboard, Deprecation Schedule).
8. **Custom concept types.** A mechanism for collections to define their own CoDL concepts within a BTABOK-profile collection, without forking the profile.
9. **CaDL interactivity.** CaDL v0.1 specifies static rendering. Future versions may add interactive canvas features (filtering, zoom).

---

## Appendix A. CoDL Concept Definitions

> Note: Following the Core SpecLang Absorption design (see Appendix D), the meta blocks in this appendix represent the combined Core SpecItem metadata plus the BTABoKItem extension. The Core SpecItem fields (slug, itemType, name, shortDescription, version, publishStatus, authors, reviewers, committer, tags, createdAt, updatedAt, retentionPolicy, freshnessSla, lastReviewed, dependencies) are supplied by Core SpecLang and apply to every profile. The BTABoKItem fields (accessTier, bokStatus, certainty, baseVersion, and the other BTABoK-specific items) are contributed by the BTABOK profile. Individual concept meta blocks below are left as they stand; the implicit "Standard BTABoK Metadata" profile referenced in each should be read as the combined core plus BTABoKItem set.

Each concept below carries the Standard BTABoK Metadata profile defined in Section 4. To keep the definitions readable, the meta block lists only concept-specific or non-default fields; the full Standard Metadata profile (slug, itemType, name, shortDescription, version, baseVersion, bokStatus, publishStatus, accessTier, authors, reviewers, committer, tags, certainty, createdAt, updatedAt) is implicit for every concept. The SpecChat extensions `freshnessSla` and `lastReviewed` are noted explicitly where the retention policy defaults to `indefinite`.

### A.1 StakeholderCard

**CoDL itemType:** StakeholderCard
**Source:** BTABoK standard
**Retention policy default:** indefinite
**Canvases that target this concept:** StakeholderMap, PowerInterestGrid, ConcernMap

Purpose: Captures a stakeholder, the concerns they hold, their influence, the engagement strategy, and the viewpoints required to address them.

```codl
concept StakeholderCard {
  meta {
    itemType: shortText required                  // "StakeholderCard"
    freshnessSla: duration optional               // required when retentionPolicy=indefinite
    lastReviewed: date optional                   // required when retentionPolicy=indefinite
  }

  section Identity {
    stakeholderId: id required
    displayName: shortText required
    role: shortText required
    organization: shortText optional
    contact: contact optional
  }

  section Concerns [list] {
    item {
      concernId: id required
      statement: text required
      priority: enum(Low, Medium, High, Critical) required @display(widget=pill, colorMap={Low:"#9ca3af", Medium:"#3b82f6", High:"#f59e0b", Critical:"#ef4444"})
    }
  }

  section Engagement {
    influence: enum(Low, Medium, High, Decisive) required
    interest: enum(Low, Medium, High) required
    strategy: text required                        // engage, consult, inform, monitor
    cadence: duration optional
  }

  relationships {
    uses<ViewpointCard> as requiredViewpoints cardinality(0..*)
  }

  storage {
    primaryKey: slug
    indexOn: [stakeholderId, Engagement.influence]
    retentionPolicy: indefinite
  }
}
```

Notes: `concerns` are inline so that a concern's priority lives next to its statement; cross-card concern reuse is handled via `weakRef` from `ViewpointCard.addresses`.

### A.2 PrincipleCard

**CoDL itemType:** PrincipleCard
**Source:** BTABoK standard
**Retention policy default:** indefinite
**Canvases that target this concept:** PrincipleCatalog

Purpose: Captures an enterprise architecture principle with statement, rationale, and effective date, and links to the ASRs it implements and the decisions that depend on it.

```codl
concept PrincipleCard {
  meta {
    itemType: shortText required                  // "PrincipleCard"
    freshnessSla: duration optional
    lastReviewed: date optional
  }

  section Statement {
    principleId: id required
    headline: shortText required
    statement: richText required
    rationale: richText required
    implications: text optional
  }

  section Lifecycle {
    effectiveDate: date required
    reviewCadence: duration required
    status: enum(Proposed, Active, Superseded, Retired) required @display(widget=pill)
  }

  relationships {
    implements<ASRCard> as addressedRequirements cardinality(0..*)
    uses<DecisionRecord> as dependentDecisions cardinality(0..*)
    supersedes<self> cardinality(0..1)
    supersededBy<self> cardinality(0..1)
  }

  storage {
    primaryKey: slug
    indexOn: [Lifecycle.status, Lifecycle.effectiveDate]
    retentionPolicy: indefinite
  }
}
```

Notes: Supersession is used when a principle is restated; the historical record is preserved by keeping the superseded card at `publishStatus: Retired`.

### A.3 ASRCard

**CoDL itemType:** ASRCard
**Source:** BTABoK standard
**Retention policy default:** indefinite
**Canvases that target this concept:** ASRMatrix, ASRCard

Purpose: Captures an architecturally significant requirement with its significance classification, rationale, verification approach, and characteristic flags.

```codl
concept ASRCard {
  meta {
    itemType: shortText required                  // "ASRCard"
    freshnessSla: duration optional
    lastReviewed: date optional
  }

  section Requirement {
    asrId: id required
    statement: richText required
    rationale: richText required
    significance: enum(Structural, QualityAttribute, TechnicalCapability) required @display(widget=pill)
  }

  section Verification {
    approach: text required
    acceptanceCriteria: list<text> required min 1
    verificationOwner: PersonRef required
  }

  section Characteristics {
    traits: flags(Complete, Traceable, Current, Verifiable, Valuable) required
  }

  relationships {
    implements<PrincipleCard> as supportedPrinciples cardinality(0..*)
    usedBy<DecisionRecord> as addressingDecisions cardinality(0..*)
  }

  storage {
    primaryKey: slug
    indexOn: [Requirement.significance, asrId]
    retentionPolicy: indefinite
  }
}
```

Notes: `Characteristics.traits` uses `flags` so that missing qualities are explicit; validators can count trait coverage per ASR.

### A.4 DecisionRecord

**CoDL itemType:** DecisionRecord
**Source:** BTABoK standard
**Retention policy default:** indefinite
**Canvases that target this concept:** DecisionRegistry, DecisionRecordCard, GovernanceApprovalFlow

Purpose: Captures an architecturally significant decision, the options considered, the recommendation, the reversibility classification, and its cascades.

```codl
concept DecisionRecord {
  meta {
    itemType: shortText required                  // "DecisionRecord"
    freshnessSla: duration optional
    lastReviewed: date optional
  }

  section Classification {
    decisionId: id required
    scope: enum(ecosystem, enterprise, value_stream, solution, product, module) required
    type: enum(structural, product, constraint, cascade, principle) required
    method: enum(budgeted_evaluation, bake_off, cost_benefit, scoring, opinion) required
    reversibility: enum(near_impossible, low, medium, high) required @display(widget=pill, colorMap={near_impossible:"#ef4444", low:"#f59e0b", medium:"#eab308", high:"#22c55e"})
    decisionDate: date required
  }

  section Problem {
    context: richText required
    drivers: list<text> required min 1
  }

  section Options [list] min 1 {
    item {
      optionId: id required
      name: shortText required
      summary: text required
      pros: list<text> optional
      cons: list<text> optional
      tradeoffs: text optional
      estimatedCost: currency optional
    }
  }

  section Outcome {
    recommendedOption: id required
    recommendation: richText required
    consequences: text required
  }

  section Cascades [list optional] {
    item {
      cascadedDecision: ref<DecisionRecord> required
      note: text optional
    }
  }

  relationships {
    uses<ASRCard> as addressedRequirements cardinality(0..*)
    implements<PrincipleCard> as supportedPrinciples cardinality(0..*)
    supersedes<self> cardinality(0..1)
    supersededBy<self> cardinality(0..1)
  }

  storage {
    primaryKey: slug
    indexOn: [Classification.scope, Classification.type, Classification.decisionDate]
    immutableFields: [Classification.decisionId, Classification.decisionDate]
    auditFields: [publishStatus, Outcome.recommendedOption]
    retentionPolicy: indefinite
  }
}
```

Notes: `Outcome.recommendedOption` references an `Options.item.optionId` by id; the validator checks the value appears in the Options list.

### A.5 GovernanceBody

**CoDL itemType:** GovernanceBody
**Source:** SpecChat-specific
**Retention policy default:** indefinite
**Canvases that target this concept:** GovernanceApprovalFlow (joined)

Purpose: Captures a governance body such as the EARB, ETSC, or a Domain Design Council with its scope, authority, chair, membership, and cadence.

```codl
concept GovernanceBody {
  meta {
    itemType: shortText required                  // "GovernanceBody"
    freshnessSla: duration optional
    lastReviewed: date optional
  }

  section Identity {
    bodyId: id required
    displayName: shortText required
    scope: enum(ecosystem, enterprise, value_stream, domain, product) required
    authority: text required
  }

  section Leadership {
    chair: PersonRef required
    secretary: PersonRef optional
  }

  section Membership [list] min 1 {
    item {
      member: PersonRef required
      role: shortText required
      votingRights: boolean required
    }
  }

  section Operations {
    cadence: duration required
    quorum: integer optional
    publicMinutes: boolean required
  }

  relationships {
    oversees<DecisionRecord> as overseenDecisions cardinality(0..*)
    enforces<GovernanceRule> as enforcedRules cardinality(0..*)
  }

  storage {
    primaryKey: slug
    indexOn: [Identity.scope]
    retentionPolicy: indefinite
  }
}
```

Notes: Membership list uses inline items so that role and voting rights stay attached to each member.

### A.6 GovernanceRule

**CoDL itemType:** GovernanceRule
**Source:** SpecChat-specific
**Retention policy default:** indefinite
**Canvases that target this concept:** (none at v0.1; referenced by WaiverRegister)

Purpose: Captures a governance rule such as "every material change requires a decision record" with its scope, applicability, enforcement posture, and linked ASRs.

```codl
concept GovernanceRule {
  meta {
    itemType: shortText required                  // "GovernanceRule"
    freshnessSla: duration optional
    lastReviewed: date optional
  }

  section Rule {
    ruleId: id required
    headline: shortText required
    statement: richText required
    applicability: text required
    scope: enum(ecosystem, enterprise, value_stream, domain, product, module) required
  }

  section Enforcement {
    posture: enum(warning, error) required @display(widget=pill, colorMap={warning:"#f59e0b", error:"#ef4444"})
    validatorCode: shortText optional             // BTABOK-GOV-xxx when backed by a validator
  }

  relationships {
    enforcedBy<GovernanceBody> as enforcers cardinality(1..*)
    implements<PrincipleCard> as supportedPrinciples cardinality(0..*)
    uses<ASRCard> as linkedRequirements cardinality(0..*)
  }

  storage {
    primaryKey: slug
    indexOn: [Rule.scope, Enforcement.posture]
    retentionPolicy: indefinite
  }
}
```

Notes: `Enforcement.validatorCode` ties a rule to a concrete BTABOK validator when one exists; otherwise the rule is enforced by review.

### A.7 WaiverRecord

**CoDL itemType:** WaiverRecord
**Source:** BTABoK standard
**Retention policy default:** archive-on-deprecated
**Canvases that target this concept:** WaiverRegister, WaiverCard

Purpose: Captures an approved exception to a principle or governance rule, including the justification, residual risk, compensating controls, expiration, and approver.

```codl
concept WaiverRecord {
  meta {
    itemType: shortText required                  // "WaiverRecord"
  }

  section Target {
    waiverId: id required
    waivesTarget: ref<PrincipleCard> optional
    waivesRule: ref<GovernanceRule> optional
    // validator requires exactly one of waivesTarget or waivesRule to be set
  }

  section Case {
    description: richText required
    justification: richText required
    residualRisk: enum(Low, Medium, High) required @display(widget=pill)
    compensatingControls: list<text> required min 1
  }

  section Duration {
    grantedDate: date required
    expiration: date required
    renewalAllowed: boolean required
  }

  section Approval {
    approver: PersonRef required
  }

  relationships {
    approvedBy<GovernanceBody> as approvingBody cardinality(1..1)
  }

  storage {
    primaryKey: slug
    indexOn: [Duration.expiration, Case.residualRisk]
    retentionPolicy: archive-on-deprecated
  }
}
```

Notes: The one-of constraint on `waivesTarget` vs. `waivesRule` is enforced by `check_waiver_rule_reference`. Waivers are archived, not deleted, so the audit trail survives.

### A.8 ViewpointCard

**CoDL itemType:** ViewpointCard
**Source:** BTABoK standard
**Retention policy default:** indefinite
**Canvases that target this concept:** ViewpointCatalog, ViewpointCoverageReport, ConcernMap

Purpose: Captures a reusable viewpoint template with audience, the concerns it answers, the required models, and the owner.

```codl
concept ViewpointCard {
  meta {
    itemType: shortText required                  // "ViewpointCard"
    freshnessSla: duration optional
    lastReviewed: date optional
  }

  section Identity {
    viewpointId: id required
    displayName: shortText required
    audience: list<shortText> required min 1
    owner: PersonRef required
  }

  section Content {
    purpose: text required
    concernsAnswered: list<text> required min 1
    requiredModels: list<shortText> required min 1
    notation: shortText optional
  }

  relationships {
    addresses<StakeholderCard> as servesStakeholders cardinality(0..*)
    renderedBy<CanvasDefinition> as canvases cardinality(0..*)
  }

  storage {
    primaryKey: slug
    indexOn: [Identity.viewpointId]
    retentionPolicy: indefinite
  }
}
```

Notes: `concernsAnswered` is a text list so that a viewpoint can answer concerns that are not yet cataloged as formal `StakeholderCard` entries.

### A.9 CanvasDefinition

**CoDL itemType:** CanvasDefinition
**Source:** SpecChat-specific
**Retention policy default:** indefinite
**Canvases that target this concept:** ViewpointCoverageReport (joined)

Purpose: Captures a CaDL canvas declaration as a first-class concept so canvases can be cataloged, owned, and versioned like any other artifact.

```codl
concept CanvasDefinition {
  meta {
    itemType: shortText required                  // "CanvasDefinition"
    freshnessSla: duration optional
    lastReviewed: date optional
  }

  section Identity {
    canvasId: id required
    displayName: shortText required
    targetConceptType: shortText required          // one of the 19 concept itemTypes
    collectionMode: enum(single, collection, joined) required
  }

  section Layout {
    layoutStyle: enum(grid, table, mermaid, mixed) required
    summary: text required
  }

  section Areas [list] min 1 {
    item {
      areaId: id required
      label: shortText required
      sourceSection: shortText required            // CoDL section name on the target concept
      sourceField: shortText optional              // field path when only one field is surfaced
      display: shortText optional                  // widget hint forwarded to CaDL
    }
  }

  relationships {
    renders<ViewpointCard> as fulfilsViewpoints cardinality(0..*)
  }

  storage {
    primaryKey: slug
    indexOn: [Identity.targetConceptType]
    retentionPolicy: indefinite
  }
}
```

Notes: `targetConceptType` is a `shortText` rather than a `ref<>` because the value names a CoDL concept type (metadata), not an instance.

### A.10 QualityAttributeScenario

**CoDL itemType:** QualityAttributeScenario
**Source:** SpecChat-specific
**Retention policy default:** indefinite
**Canvases that target this concept:** (none at v0.1; cross-referenced from ASRMatrix)

Purpose: Captures a quality attribute scenario in the SEI sense with stimulus, environment, response, response measure, and priority.

```codl
concept QualityAttributeScenario {
  meta {
    itemType: shortText required                  // "QualityAttributeScenario"
    freshnessSla: duration optional
    lastReviewed: date optional
  }

  section Scenario {
    scenarioId: id required
    qualityAttribute: enum(Performance, Availability, Security, Scalability, Modifiability, Usability, Testability, Interoperability, Deployability, Observability) required
    stimulus: text required
    stimulusSource: text required
    environment: text required
    artifact: text required
    response: text required
  }

  section Measure {
    responseMeasure: threshold required
    priority: enum(Low, Medium, High, Critical) required @display(widget=pill)
  }

  relationships {
    addressesASR<ASRCard> as supportedRequirements cardinality(1..*)
  }

  storage {
    primaryKey: slug
    indexOn: [Scenario.qualityAttribute, Measure.priority]
    retentionPolicy: indefinite
  }
}
```

Notes: `responseMeasure` uses the CoDL `threshold` composite so that a scenario can carry both the target and the tolerance band.

### A.11 StandardCard

**CoDL itemType:** StandardCard
**Source:** SpecChat-specific
**Retention policy default:** indefinite
**Canvases that target this concept:** StandardsCatalog

Purpose: Captures an external or internal standard the organization adopts, evaluates, or rejects, along with version, scope, adoption status, and review cadence.

```codl
concept StandardCard {
  meta {
    itemType: shortText required                  // "StandardCard"
    freshnessSla: duration optional
    lastReviewed: date optional
  }

  section Identity {
    standardId: id required
    name: shortText required
    issuingBody: shortText required
    versionLabel: shortText required
    referenceUrl: url optional
  }

  section Adoption {
    scope: text required
    adoptionStatus: enum(Adopted, Evaluated, Rejected, Retired) required @display(widget=pill)
    adoptedDate: date optional
    reviewCadence: duration required
  }

  relationships {
    adoptedBy<SystemSpec> as adoptingSystems cardinality(0..*) weakRef
  }

  storage {
    primaryKey: slug
    indexOn: [Adoption.adoptionStatus]
    retentionPolicy: indefinite
  }
}
```

Notes: `SystemSpec` lives in core SpecLang rather than this profile, so the reference is declared as `weakRef` and is validated by `check_weakref_resolution`.

### A.12 CapabilityCard

**CoDL itemType:** CapabilityCard
**Source:** BTABoK standard
**Retention policy default:** indefinite
**Canvases that target this concept:** CapabilityHeatMap

Purpose: Captures a business or technology capability with L1 and L2 hierarchy, baseline and target maturity, strategic importance, and gap notes.

```codl
concept CapabilityCard {
  meta {
    itemType: shortText required                  // "CapabilityCard"
    freshnessSla: duration optional
    lastReviewed: date optional
  }

  section Identity {
    capabilityId: id required
    displayName: shortText required
    l1Category: shortText required
    l2Category: shortText optional
    owner: PersonRef required
  }

  section Maturity {
    baseline: enum(Low, LowMedium, Medium, MediumHigh, High) required @display(widget=heatmap, colorMap={Low:"#ef4444", LowMedium:"#f59e0b", Medium:"#eab308", MediumHigh:"#84cc16", High:"#22c55e"})
    target: enum(Low, LowMedium, Medium, MediumHigh, High) required
    strategicImportance: enum(Low, Medium, High, Critical) required @display(widget=pill)
  }

  section Gap {
    notes: richText required
    confidence: score1to10 optional
  }

  relationships {
    suppliedBy<SystemSpec> as supplyingSystems cardinality(0..*) weakRef
    targetedBy<TransitionArchitecture> as transitions cardinality(0..*)
  }

  storage {
    primaryKey: slug
    indexOn: [Identity.l1Category, Maturity.strategicImportance]
    retentionPolicy: indefinite
  }
}
```

Notes: Maturity levels use the five-step scale commonly used by BTABoK assessments; the enum values use camelCase because CoDL enums are identifiers, not display labels.

### A.13 RoadmapItem

**CoDL itemType:** RoadmapItem
**Source:** SpecChat-specific
**Retention policy default:** archive-on-deprecated
**Canvases that target this concept:** RoadmapTimeline

Purpose: Captures a single roadmap entry with milestone, sequence, dependencies, and the capability movements it targets.

```codl
concept RoadmapItem {
  meta {
    itemType: shortText required                  // "RoadmapItem"
  }

  section Identity {
    itemId: id required
    displayName: shortText required
    sequence: integer required
    milestoneDate: date required
  }

  section Scope {
    summary: text required
    owner: PersonRef required
    estimatedEffort: duration optional
  }

  section CapabilityMovements [list] min 1 {
    item {
      capability: ref<CapabilityCard> required
      fromLevel: enum(Low, LowMedium, Medium, MediumHigh, High) required
      toLevel: enum(Low, LowMedium, Medium, MediumHigh, High) required
      rationale: text optional
    }
  }

  relationships {
    dependsOn<self> as predecessors cardinality(0..*)
    targets<CapabilityCard> as affectedCapabilities cardinality(1..*)
  }

  storage {
    primaryKey: slug
    indexOn: [Identity.milestoneDate, Identity.sequence]
    retentionPolicy: archive-on-deprecated
  }
}
```

Notes: `CapabilityMovements` carries the from/to pair inline so that a single roadmap item can move multiple capabilities.

### A.14 TransitionArchitecture

**CoDL itemType:** TransitionArchitecture
**Source:** SpecChat-specific
**Retention policy default:** archive-on-deprecated
**Canvases that target this concept:** RoadmapTimeline (as container)

Purpose: Captures a baseline-to-target transition plan with scope, baseline state, target state, and the sequenced roadmap items that realize the transition.

```codl
concept TransitionArchitecture {
  meta {
    itemType: shortText required                  // "TransitionArchitecture"
  }

  section Identity {
    transitionId: id required
    displayName: shortText required
    scope: enum(ecosystem, enterprise, value_stream, domain, product) required
    windowStart: date required
    windowEnd: date required
  }

  section States {
    baselineState: richText required
    targetState: richText required
    boundaryAssumptions: text optional
  }

  section Sequencing [list] min 1 {
    item {
      position: integer required
      roadmapItem: ref<RoadmapItem> required
      gating: text optional
    }
  }

  relationships {
    contains<RoadmapItem> as items cardinality(1..*)
    movesCapability<CapabilityCard> as capabilityMovements cardinality(1..*)
  }

  storage {
    primaryKey: slug
    indexOn: [Identity.scope, Identity.windowEnd]
    retentionPolicy: archive-on-deprecated
  }
}
```

Notes: `Sequencing` carries explicit position numbers so that reordering is an intentional act rather than a list shuffle.

### A.15 RiskCard

**CoDL itemType:** RiskCard
**Source:** SpecChat-specific
**Retention policy default:** indefinite
**Canvases that target this concept:** RiskRegister

Purpose: Captures an architectural risk with impact, probability, owner, and mitigation, plus links to the decisions or principles that mitigate it.

```codl
concept RiskCard {
  meta {
    itemType: shortText required                  // "RiskCard"
    freshnessSla: duration optional
    lastReviewed: date optional
  }

  section Identity {
    riskId: id required
    displayName: shortText required
    owner: PersonRef required
  }

  section Assessment {
    description: richText required
    impact: enum(Low, Medium, High, Critical) required @display(widget=pill, colorMap={Low:"#9ca3af", Medium:"#3b82f6", High:"#f59e0b", Critical:"#ef4444"})
    probability: enum(Low, Medium, High) required @display(widget=pill)
    status: enum(Open, Mitigating, Accepted, Closed) required
  }

  section Mitigation {
    plan: richText required
    reviewCadence: duration required
  }

  relationships {
    mitigatedBy<DecisionRecord> as mitigatingDecisions cardinality(0..*)
    mitigatedBy<PrincipleCard> as mitigatingPrinciples cardinality(0..*)
  }

  storage {
    primaryKey: slug
    indexOn: [Assessment.impact, Assessment.probability, Assessment.status]
    retentionPolicy: indefinite
  }
}
```

Notes: Two separate `mitigatedBy` relationships are declared with distinct role names so the validator can enforce type-correct resolution on each.

### A.16 ExperimentCard

**CoDL itemType:** ExperimentCard
**Source:** BTABoK standard
**Retention policy default:** archive-on-deprecated
**Canvases that target this concept:** ExperimentBoard

Purpose: Captures an innovation experiment with hypothesis, cost cap, duration, kill criteria, success measures, status, and sponsor.

```codl
concept ExperimentCard {
  meta {
    itemType: shortText required                  // "ExperimentCard"
  }

  section Hypothesis {
    experimentId: id required
    headline: shortText required
    hypothesis: richText required
    rationale: text required
  }

  section Bounds {
    costCap: currency required
    duration: duration required
    startDate: date required
  }

  section Criteria {
    killCriteria: list<text> required min 1
    successMeasures: list<text> required min 1
  }

  section Status {
    state: enum(Draft, Active, Concluded, Productized, Killed) required @display(widget=pill)
    sponsor: PersonRef required
    conclusion: richText optional
  }

  relationships {
    sponsoredBy<StakeholderCard> as sponsoringStakeholders cardinality(0..*)
    feedsInto<DecisionRecord> as informedDecisions cardinality(0..*)
  }

  storage {
    primaryKey: slug
    indexOn: [Status.state, Bounds.startDate]
    retentionPolicy: archive-on-deprecated
  }
}
```

Notes: `sponsor` in the status section is the PersonRef of record; `sponsoredBy` relates the experiment to any `StakeholderCard` entries representing sponsoring parties.

### A.17 LegacyModernizationRecord

**CoDL itemType:** LegacyModernizationRecord
**Source:** SpecChat-specific
**Retention policy default:** archive-on-deprecated
**Canvases that target this concept:** (none at v0.1; referenced from RoadmapTimeline)

Purpose: Captures a legacy-system decommissioning or modernization plan with identity, current and target state, migration window, evidence preservation plan, and customer migration plan.

```codl
concept LegacyModernizationRecord {
  meta {
    itemType: shortText required                  // "LegacyModernizationRecord"
  }

  section Identity {
    recordId: id required
    systemName: shortText required
    systemOwner: PersonRef required
    currentScope: text required
  }

  section Transition {
    currentState: enum(Production, FeatureFrozen, Decommissioning, Retired) required @display(widget=pill)
    targetState: enum(Production, FeatureFrozen, Decommissioning, Retired) required
    migrationWindow: range<date> required
  }

  section Plans {
    evidencePreservationPlan: richText required
    customerMigrationPlan: richText required
    mitigations: list<text> required min 1
  }

  section Risks [list optional] {
    item {
      risk: ref<RiskCard> required
      note: text optional
    }
  }

  relationships {
    replacedBy<SystemSpec> as successorSystems cardinality(0..*) weakRef
    usesWaiver<WaiverRecord> as activeWaivers cardinality(0..*)
  }

  storage {
    primaryKey: slug
    indexOn: [Transition.currentState, Transition.migrationWindow]
    retentionPolicy: archive-on-deprecated
  }
}
```

Notes: `replacedBy<SystemSpec>` is a `weakRef` because the successor system may be authored under a different profile (for example, `TheStandard`).

### A.18 MetricDefinition

**CoDL itemType:** MetricDefinition
**Source:** SpecChat-specific
**Retention policy default:** indefinite
**Canvases that target this concept:** OutcomeScorecard (as contained metric)

Purpose: Captures a metric with baseline, target, measurement method, measurement period, and owner, tied to the capability or ASR it measures.

```codl
concept MetricDefinition {
  meta {
    itemType: shortText required                  // "MetricDefinition"
    freshnessSla: duration optional
    lastReviewed: date optional
  }

  section Identity {
    metricId: id required
    displayName: shortText required
    owner: PersonRef required
    unit: shortText required
  }

  section Values {
    baseline: measurement required
    target: measurement required
    currentValue: measurement optional
    direction: enum(HigherIsBetter, LowerIsBetter, WithinBand) required
  }

  section Method {
    description: richText required
    dataSource: text required
    measurementPeriod: duration required
  }

  relationships {
    measures<CapabilityCard> as measuredCapabilities cardinality(0..*)
    measures<ASRCard> as measuredRequirements cardinality(0..*)
  }

  storage {
    primaryKey: slug
    indexOn: [Identity.metricId]
    retentionPolicy: indefinite
  }
}
```

Notes: A metric must measure at least one capability or requirement; the validator `check_metric_baseline_target` additionally enforces that baseline and target are both populated.

### A.19 ScorecardDefinition

**CoDL itemType:** ScorecardDefinition
**Source:** SpecChat-specific
**Retention policy default:** indefinite
**Canvases that target this concept:** OutcomeScorecard

Purpose: Captures a scorecard that aggregates metrics for a specific audience with a declared review cadence.

```codl
concept ScorecardDefinition {
  meta {
    itemType: shortText required                  // "ScorecardDefinition"
    freshnessSla: duration optional
    lastReviewed: date optional
  }

  section Identity {
    scorecardId: id required
    displayName: shortText required
    audience: enum(Executive, Architecture, Operational) required @display(widget=pill)
    owner: PersonRef required
  }

  section Contents [list] min 1 {
    item {
      metric: ref<MetricDefinition> required
      weight: percentage optional
      commentary: text optional
    }
  }

  section Cadence {
    reviewCadence: duration required
    nextReview: date optional
  }

  relationships {
    aggregates<MetricDefinition> as metrics cardinality(1..*)
    presentedTo<StakeholderCard> as audienceStakeholders cardinality(0..*)
  }

  storage {
    primaryKey: slug
    indexOn: [Identity.audience]
    retentionPolicy: indefinite
  }
}
```

Notes: `Contents` carries optional per-metric weight so that a scorecard can express composite scoring without requiring it.

## Appendix B. CaDL Canvas Definitions

This appendix contains the full CaDL definition for each of the 20 canvases in the v0.1 library (Section 5.1). Each subsection gives a purpose statement, target concept(s), primary audience, default output format, and the CaDL source. All canvases emit a freshness warning at the head of the rendered output if any referenced concept is past its freshness SLA.

### B.1 DecisionRegistry

Purpose: A flat registry of every `DecisionRecord` in the collection, intended as the EARB's working catalog.

- Target: `DecisionRecord` collection
- Audience: EARB, architects
- Output: markdown-table

```cadl
canvas DecisionRegistry for DecisionRecord collection {
  output: markdown-table
  freshnessCheck: include
  joins: PrincipleCard via principles

  area "Row" {
    repeats: meta
    shows: meta.slug
    shows: meta.name
    shows: scope
    shows: type
    shows: meta.publishStatus
    shows: meta.committer -> name
    shows: principles -> count
    format: table-row
  }
}
```

### B.2 DecisionRecordCard

Purpose: A single-decision detail view suitable for EARB review packs and for linking from other canvases.

- Target: `DecisionRecord` (single)
- Audience: EARB, approvers
- Output: both (markdown plus inline mermaid for cascade links)

```cadl
canvas DecisionRecordCard for DecisionRecord {
  output: both
  freshnessCheck: include

  area "Header" {
    shows: meta.slug
    shows: meta.name
    shows: meta.publishStatus
    format: card-header
  }

  area "Scope" {
    shows: scope
    shows: type
    shows: method
  }

  area "Options" {
    repeats: options
    shows: label
    shows: summary
  }

  area "Recommendation" {
    shows: recommendation.rationale
  }

  area "Traceability" {
    repeats: requirements
    shows: slug -> name
  }

  area "Approvers" {
    shows: meta.committer -> name
  }
}
```

### B.3 ASRMatrix

Purpose: A cross-reference matrix of architecturally significant requirements and the decisions that address them.

- Target: `ASRCard` collection
- Audience: Architects, engineers
- Output: markdown-table

```cadl
canvas ASRMatrix for ASRCard collection {
  output: markdown-table
  freshnessCheck: include
  joins: DecisionRecord via usedBy

  area "Row" {
    repeats: meta
    shows: meta.slug
    shows: statement
    shows: significance
    shows: characteristics
    shows: usedBy -> count
    format: table-row
  }
}
```

### B.4 ASRCard

Purpose: A single-requirement detail view showing statement, significance, characteristics, and traceability.

- Target: `ASRCard` (single)
- Audience: Architects
- Output: markdown-table

```cadl
canvas ASRCard for ASRCard {
  output: markdown-table
  freshnessCheck: include

  area "Header" {
    shows: meta.slug
    shows: meta.name
    shows: meta.publishStatus
    format: card-header
  }

  area "Statement" {
    shows: statement
  }

  area "Significance" {
    shows: significance
    shows: rationale
  }

  area "Characteristics" {
    shows: characteristics
    format: badge
  }

  area "Verification" {
    shows: verification.approach
    shows: verification.threshold
  }

  area "Traceability" {
    repeats: implements
    shows: slug -> name
  }

  area "Addressed by" {
    repeats: usedBy
    shows: slug -> name
  }
}
```

### B.5 StakeholderMap

Purpose: A flat catalog of stakeholders with role, influence, and viewpoint coverage count.

- Target: `StakeholderCard` collection
- Audience: Sponsors, change leaders
- Output: markdown-table

```cadl
canvas StakeholderMap for StakeholderCard collection {
  output: markdown-table
  freshnessCheck: include

  area "Row" {
    repeats: meta
    shows: meta.slug
    shows: meta.name
    shows: role
    shows: influence
    shows: requiredViewpoints -> count
    format: table-row
  }
}
```

### B.6 PowerInterestGrid

Purpose: A classic power-interest quadrant chart positioning each stakeholder by influence and interest.

- Target: `StakeholderCard` collection
- Audience: Sponsors
- Output: mermaid (quadrantChart)

```cadl
canvas PowerInterestGrid for StakeholderCard collection {
  output: mermaid
  freshnessCheck: include

  area "Plot" {
    repeats: meta
    shows: meta.name
    shows: interest
    shows: influence
    format: node
  }
}
```

Rendering note: the CLI emits a Mermaid `quadrantChart` with axes labeled Interest and Power, placing each stakeholder at `(interest, influence)`.

### B.7 ConcernMap

Purpose: A stakeholder-by-viewpoint cross-reference identifying which viewpoints each stakeholder requires.

- Target: `StakeholderCard` joined with `ViewpointCard`
- Audience: Architects
- Output: markdown-table

```cadl
canvas ConcernMap for StakeholderCard collection {
  output: markdown-table
  freshnessCheck: include
  joins: ViewpointCard via requiredViewpoints

  area "Row" {
    repeats: meta
    shows: meta.slug
    shows: meta.name
    shows: requiredViewpoints -> slug
    shows: requiredViewpoints -> name
    format: table-row
  }
}
```

### B.8 ViewpointCatalog

Purpose: A catalog of reusable viewpoints with audience, concern answered, owner, and rendering canvases.

- Target: `ViewpointCard` collection
- Audience: Everyone
- Output: markdown-table

```cadl
canvas ViewpointCatalog for ViewpointCard collection {
  output: markdown-table
  freshnessCheck: include
  joins: CanvasDefinition via renderedBy

  area "Row" {
    repeats: meta
    shows: meta.slug
    shows: meta.name
    shows: audience
    shows: concernAnswered
    shows: meta.committer -> name
    shows: renderedBy -> count
    format: table-row
  }
}
```

### B.9 ViewpointCoverageReport

Purpose: A gap analysis of viewpoints that lack a corresponding canvas, used by the Repository Stewardship Group to prioritize canvas authoring.

- Target: `ViewpointCard` joined with `CanvasDefinition`
- Audience: Architects
- Output: markdown-table

```cadl
canvas ViewpointCoverageReport for ViewpointCard collection {
  output: markdown-table
  freshnessCheck: include
  joins: CanvasDefinition via renderedBy

  area "Row" {
    repeats: meta
    shows: meta.slug
    shows: meta.name
    shows: audience
    shows: renderedBy -> count
    shows: renderedBy -> slug
    format: table-row
  }
}
```

Rendering note: rows where `renderedBy -> count` is zero are flagged with a warning banner listing uncovered viewpoints.

### B.10 PrincipleCatalog

Purpose: A catalog of enterprise architecture principles with owner and ASR implementation count.

- Target: `PrincipleCard` collection
- Audience: Everyone
- Output: markdown-table

```cadl
canvas PrincipleCatalog for PrincipleCard collection {
  output: markdown-table
  freshnessCheck: include
  joins: ASRCard via implements

  area "Row" {
    repeats: meta
    shows: meta.slug
    shows: statement
    shows: meta.committer -> name
    shows: implements -> count
    format: table-row
  }
}
```

### B.11 CapabilityHeatMap

Purpose: A flowchart-style heat map of capabilities colored by strategic importance, with baseline and target maturity on each node.

- Target: `CapabilityCard` collection
- Audience: Product, executives
- Output: mermaid (flowchart)

```cadl
canvas CapabilityHeatMap for CapabilityCard collection {
  output: mermaid
  freshnessCheck: include

  area "Node" {
    repeats: meta
    shows: meta.slug
    shows: meta.name
    shows: importance
    shows: baselineMaturity
    shows: targetMaturity
    format: node
  }
}
```

Rendering note: node fill color is derived from `importance`: critical renders red, high orange, medium amber, low grey. Node label shows `name`, `baseline -> target`.

### B.12 RoadmapTimeline

Purpose: A timeline view of roadmap items grouped by transition architecture.

- Target: `TransitionArchitecture` containing `RoadmapItem`
- Audience: Product, executives
- Output: mermaid (gantt)

```cadl
canvas RoadmapTimeline for TransitionArchitecture collection {
  output: mermaid
  freshnessCheck: include
  joins: RoadmapItem via contains

  area "Group" {
    repeats: meta
    shows: meta.slug
    shows: meta.name
  }

  area "Entry" {
    repeats: contains
    shows: slug -> name
    shows: slug -> milestone
    shows: slug -> sequence
    format: timeline-entry
  }
}
```

### B.13 WaiverRegister

Purpose: A registry of waivers with expiration and status, used by EARB and auditors.

- Target: `WaiverRecord` collection
- Audience: EARB, auditors
- Output: markdown-table

```cadl
canvas WaiverRegister for WaiverRecord collection {
  output: markdown-table
  freshnessCheck: include

  area "Row" {
    repeats: meta
    shows: meta.slug
    shows: waives -> slug
    shows: expiration
    shows: status
    shows: approvedBy -> name
    format: table-row
  }
}
```

### B.14 WaiverCard

Purpose: A single-waiver detail view for approver packs.

- Target: `WaiverRecord` (single)
- Audience: Approvers
- Output: markdown-table

```cadl
canvas WaiverCard for WaiverRecord {
  output: markdown-table
  freshnessCheck: include

  area "Header" {
    shows: meta.slug
    shows: meta.name
    shows: meta.publishStatus
    format: card-header
  }

  area "Rule" {
    shows: waives -> slug
    shows: waives -> name
  }

  area "Description" {
    shows: description
    shows: justification
  }

  area "Risk" {
    shows: risk
    shows: compensatingControls
  }

  area "Lifecycle" {
    shows: expiration
    shows: status
    shows: approvedBy -> name
  }
}
```

### B.15 StandardsCatalog

Purpose: A catalog of standards adopted by the organization, including version, scope, and adoption status.

- Target: `StandardCard` collection
- Audience: Engineering, partners
- Output: markdown-table

```cadl
canvas StandardsCatalog for StandardCard collection {
  output: markdown-table
  freshnessCheck: include

  area "Row" {
    repeats: meta
    shows: meta.slug
    shows: meta.name
    shows: version
    shows: scope
    shows: adoptionStatus
    shows: reviewCadence
    format: table-row
  }
}
```

### B.16 FreshnessReport

Purpose: A cross-concept freshness audit listing every ever-green concept with time remaining until its freshness SLA lapses.

- Target: every concept instance with `retentionPolicy: indefinite`
- Audience: Repository Stewardship Group
- Output: markdown-table

```cadl
canvas FreshnessReport for <any concept with retentionPolicy:indefinite> {
  output: markdown-table
  freshnessCheck: include

  area "Row" {
    repeats: meta
    shows: meta.itemType
    shows: meta.slug
    shows: meta.name
    shows: meta.lastReviewed
    shows: meta.freshnessSla
    shows: meta.daysUntilOverdue
    shows: meta.committer -> name
    format: table-row
  }
}
```

Rendering note: rows with `daysUntilOverdue` less than or equal to zero render with an overdue marker in the first column.

### B.17 OutcomeScorecard

Purpose: A scorecard of metrics grouped by audience (executive, architecture, operational) with baseline and target values.

- Target: `ScorecardDefinition` with `MetricDefinition` instances
- Audience: Executives, CFO
- Output: markdown-table (grouped by audience)

```cadl
canvas OutcomeScorecard for ScorecardDefinition collection {
  output: markdown-table
  freshnessCheck: include
  joins: MetricDefinition via aggregates

  area "Group" {
    repeats: meta
    shows: audience
  }

  area "Metric" {
    repeats: aggregates
    shows: slug -> meta.slug
    shows: slug -> meta.name
    shows: slug -> baseline
    shows: slug -> target
    shows: slug -> current
    shows: slug -> meta.committer
    format: table-row
  }
}
```

### B.18 GovernanceApprovalFlow

Purpose: A lifecycle projection of every `DecisionRecord` across the Draft, Review, Approved, Published, Retired states with counts at each state.

- Target: `DecisionRecord` lifecycle projection
- Audience: EARB, architects
- Output: mermaid (flowchart)

```cadl
canvas GovernanceApprovalFlow for DecisionRecord collection {
  output: mermaid
  freshnessCheck: include

  area "State" {
    repeats: meta.publishStatus
    shows: meta.publishStatus
    shows: meta.publishStatus -> count
    format: node
  }
}
```

Rendering note: the CLI emits a Mermaid `flowchart LR` with fixed nodes Draft, Review, Approved, Published, Retired and an incoming count derived from the current `publishStatus` distribution.

### B.19 RiskRegister

Purpose: A register of architectural risks with impact, probability, owner, and mitigation summary.

- Target: `RiskCard` collection
- Audience: EARB, CISO
- Output: markdown-table

```cadl
canvas RiskRegister for RiskCard collection {
  output: markdown-table
  freshnessCheck: include

  area "Row" {
    repeats: meta
    shows: meta.slug
    shows: meta.name
    shows: impact
    shows: probability
    shows: meta.committer -> name
    shows: mitigation.summary
    format: table-row
  }
}
```

### B.20 ExperimentBoard

Purpose: A board of innovation experiments grouped by status, with hypothesis, cost cap, duration, and sponsor.

- Target: `ExperimentCard` collection
- Audience: Innovation sponsors
- Output: markdown-table (grouped by status)

```cadl
canvas ExperimentBoard for ExperimentCard collection {
  output: markdown-table
  freshnessCheck: include

  area "Group" {
    repeats: meta.publishStatus
    shows: meta.publishStatus
  }

  area "Row" {
    repeats: meta
    shows: meta.slug
    shows: hypothesis
    shows: costCap
    shows: duration
    shows: meta.publishStatus
    shows: sponsoredBy -> name
    format: table-row
  }
}
```

## Appendix C. Worked Example

The Global Corp enterprise architecture exemplar at [Global-Corp-BTABOK-Enterprise-Architecture.md](Global-Corp-BTABOK-Enterprise-Architecture.md) is the canonical worked example for this profile. Appendix B of that document maps the exemplar into a 21-file SpecChat spec collection.

## Appendix D. Source References

**[D1]** Preiss, Paul. "Structured Concept Definition Language." BTABoK 3.2, IASA Global Education Portal (2026).
`https://education.iasaglobal.org/browse/btabok/3.2/core-site/core/article/structured-concept-definition-language`

**[D2]** BTABOK and SpecChat Alignment Report. Workspace: [BTA-BOK-integration.md](BTA-BOK-integration.md).

**[D2.5]** Core SpecLang Absorption Design. Workspace: [Core-SpecLang-Absorption-Design.md](Core-SpecLang-Absorption-Design.md). Records the Option X decision to absorb Standard Metadata Profile fields, reference types, relationship declarations with cardinality, retention policy, diagnostic code extensions, slug rules, and ten validators from the BTABOK profile into Core SpecLang.

**[D3]** CoDL and CaDL Integration Notes. Workspace: [CoDL-CaDL-Integration-Notes.md](CoDL-CaDL-Integration-Notes.md).

**[D4]** BTABOK Engagement Model Mapping to SpecChat. Workspace: [BTABOK-EngagementModel-Mapping.md](BTABOK-EngagementModel-Mapping.md).

**[D5]** Spec Type Taxonomy v0.1. Workspace: [Spec-Type-Taxonomy-v0.1.md](Spec-Type-Taxonomy-v0.1.md).

**[D6]** Spec Type Validation Analysis. Workspace: [Spec-Type-Validation-Analysis.md](Spec-Type-Validation-Analysis.md).

**[D7]** Global Corp BTABOK Enterprise Architecture. Workspace: [Global-Corp-BTABOK-Enterprise-Architecture.md](Global-Corp-BTABOK-Enterprise-Architecture.md).

**[D8]** SpecChat Overview. Workspace: `Delivery/spec-chat/SpecChat-Overview.md`.

**[D9]** SpecLang Specification. Workspace: `Delivery/spec-chat/SpecLang-Specification.md`.

**[D10]** SpecLang Grammar. Workspace: `Delivery/spec-chat/SpecLang-Grammar.md`.

**[D11]** The Standard Extension Overview. Workspace: `Delivery/spec-chat/extensions/the-standard/TheStandard-Extension-Overview.md`.

**[D12]** Why We Created the Spec Type Taxonomy. Workspace: [Why-We-Created-the-Spec-Type-Taxonomy.md](Why-We-Created-the-Spec-Type-Taxonomy.md).
