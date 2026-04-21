# SpecLang Design

## Tracking

| Field | Value |
|---|---|
| Document ID | GC-SLD-001 |
| CoDL itemType | DesignDocument |
| CoDL slug | speclang-design |
| Version | 0.1.0 |
| Created | 2026-04-16 |
| Last Reviewed | 2026-04-17 |
| State | Draft |
| CoDL publishStatus | Draft |
| CoDL retentionPolicy | indefinite |
| Freshness SLA | 90 days |
| Owner | PER-01 Lena Brandt, Chief Architect |
| Committer | PER-01 Lena Brandt, Chief Architect |
| Approver | PER-11 Anja Petersen, Chair EARB |
| Dependencies | Spec-Type-System.md, Global-Corp-Exemplar.md |

This document is the single authoritative design reference for SpecLang. It covers the Core SpecLang surface and the BTABOK profile v0.1. It consolidates and supersedes the four WIP design documents that preceded it (the BTABOK Profile v0.1 Design, the Core SpecLang Absorption Design, the CoDL and CaDL Integration Notes, and the BTABOK Engagement Model Mapping). Reading this document should be sufficient; the four source documents are archived.

## 1. Purpose and Scope

### 1.1 What SpecLang is

SpecLang is the specification language used by SpecChat. It has two layers: a core language that every spec collection uses, and a profile mechanism that layers practice-specific vocabulary, validators, and projections on top of the core. Core SpecLang is worldview-neutral. Profiles encode specific practice models (for example, The Standard or BTABOK) that teams opt in to.

### 1.2 Core SpecLang and the BTABOK profile

Core SpecLang handles what every spec collection needs: the five registers (context, systems, deployment, dynamics, design), standard metadata, stable identifiers, typed cross-document references, relationship declarations, retention policy, freshness tracking, and a diagnostic framework with codes. The BTABOK profile adds architecture-practice concepts (governance, stakeholders, capabilities, roadmaps, principles, waivers, risks, scorecards, experiments, ASRs, decisions in richer form, viewpoints, canvases).

### 1.3 What this document does not cover

- Core register grammar details. The five registers and their constructs are specified in `SpecLang-Specification.md` and `SpecLang-Grammar.md`.
- The Standard profile. That profile is covered in `TheStandard-Extension-Overview.md` and related documents.
- BTABOK models other than the Engagement Model. Value Model, People Model, and Competency Model are out of scope.
- Profiles for BTABOK Value/People/Competency models. Deferred to future profile versions.
- CLI or MCP server implementation details beyond the validator plug-in points.
- Repository tooling outside SpecChat itself. Jira, Confluence, Miro, and similar systems are `externalRef` targets, not profile scope.

## 2. Governing Principles

Seven principles govern how concepts enter SpecLang and how core and profile responsibilities divide.

**1. Governed, not governing.** BTABOK warns against "police state" governance and states that architects should be governed, not governing. Governance should emerge bottom-up during design and delivery, not top-down through inspection. SpecLang's validation machinery (contracts, constraints, package policies, deny rules, topology prohibitions) is powerful enough to drift toward compliance bureaucracy if wielded without restraint. Specs are tools for thinking and communication first, compliance instruments second. Validation exists to catch structural errors, not to enforce organizational conformity.

**2. Spec types before grammar expansion.** New concepts enter as spec types with validation profiles before they are considered for DSL extensions. A spec type requires only manifest recognition, section schemas, and cross-document rules. A DSL extension requires new keywords, AST nodes, parser changes, and semantic analysis. A concept earns a DSL extension only when it is formal, reusable, and semantically checkable across multiple documents.

**3. Viewpoints as a classification axis.** Viewpoints are stakeholder-concern-driven frames, not artifact types. In SpecLang, viewpoints are a classification axis on spec types, not a standalone spec type. Any structural spec can declare which viewpoint(s) it serves. If a reusable viewpoint template definition is needed (defining audience, concerns, required models, validation criteria for a viewpoint), it is introduced as a BTABOK `ViewpointCard`, not a core artifact.

**4. Ever-green vs. point-in-time.** Artifacts are either ever-green (continuously maintained, always current) or point-in-time (relevant during a phase, then archived). Core SpecLang expresses this with `retentionPolicy` (`indefinite`, `archive-on-deprecated`, `delete-on-superseded`), `freshnessSla`, and `lastReviewed`. Base system specs default to indefinite; feature, bug, amendment, and decision specs default to archive-on-deprecated.

**5. Profile activation, not core grammar inflation.** Practice-specific constructs enter SpecLang through a profile mechanism. Core grammar does not change. A profile adds metadata fields, validation rules, concept types, and projection capabilities that are active only when the profile is declared. This keeps the core language stable for users who do not need profile alignment.

**6. CoDL and CaDL are canonical.** The BTABOK profile adopts CoDL (Concept Definition Language) as the canonical schema language for concept records and CaDL (Canvas Definition Language) as the canonical language for canvas projections. The profile does not invent parallel constructs. Where SpecLang-style short forms are offered, they exist only as optional ergonomic aliases that desugar to CoDL records (see Section 4.4).

**7. Core absorption precedes profile scope.** Elements that are universally applicable, worldview-neutral, and low in incremental cost move into Core SpecLang rather than remaining in a profile. Universal infrastructure (stable identifiers, standard metadata, cross-document references, retention policy, relationship declarations, diagnostic framework) belongs in core because every spec collection needs it. Practice-specific concerns (governance bodies, ASRs with significance classification, stakeholders with influence grids, capability maturity, roadmap transitions, principles, waivers) remain profile-scoped because they encode a specific worldview. This is the formal basis for the Option X absorption decision recorded in Section 3.

## 3. Settled Architectural Decisions

The following decisions are settled across the corpus and are not revisited in this document. They are recorded here as a single table for traceability.

| ID | Decision | Resolution | Source |
|---|---|---|---|
| SD-SCOPE | Scope of BTABOK integration | Engagement Model only | Engagement Model Mapping |
| SD-ONEPROF | Profile composition | One profile active at a time per collection | User decision |
| SD-SLUG | Stable identifier model | CoDL `slug` (human-readable, URL-safe); now core | Option X |
| SD-REF | Cross-document reference model | `ref<T>`, `weakRef`, `externalRef`; now core | Option X |
| SD-LIFECYCLE | Lifecycle state mapping | SpecChat lifecycle canonical in core; CoDL `publishStatus` is an interop mapping used on BTABOK export | Option X |
| SD-OWNERSHIP | Ownership and approval | CoDL `authors`, `reviewers`, `committer`; now core via Core SpecItem metadata | Option X |
| SD-POSTURE | Governance posture | Warnings by default; treat-warnings-as-errors is a per-initiative opt-in | User decision |
| SD-EVERGREEN | Ever-green vs. point-in-time | `retentionPolicy` enum plus `freshnessSla` and `lastReviewed`; now core | Option X |
| SD-LEGACY | Legacy spec migration | Legacy specs must be brought up to meet whatever profile they claim; no grandfathering | User decision |
| SD-EXEMPLAR | Worked example basis | Global Corp enterprise architecture exemplar (`Global-Corp-Exemplar.md`) | User decision |
| SD-CMD | Authoring command | `/spec-btabok` slash command for BTABOK-profile authoring | User decision |
| SD-VALIDRESP | Validation responsibility | SpecLang is robust enough to validate each spec | User decision |
| SD-OPTIONA | Canonical concept syntax (Option A) | CoDL 0.2 canonical; SpecLang-style short forms are optional aliases | CoDL/CaDL Integration |
| SD-OPTIONX | Core absorption (Option X) | Seven elements (Standard Metadata Profile core subset, reference types, relationship declarations with cardinality, retention policy, diagnostic code extensions, slug rules, ten core validators) move from BTABOK profile to Core SpecLang | Core SpecLang Absorption |
| SD-CODLVER | CoDL version | BTABOK profile v0.1 targets CoDL 0.2 | CoDL/CaDL Integration |
| SD-CADLVER | CaDL version | BTABOK profile v0.1 targets CaDL 0.1 | CoDL/CaDL Integration |
| SD-DIAGPFX | Diagnostic code prefixes | `SPEC-` for core; `STANDARD-` for The Standard; `BTABOK-` for BTABOK | Option X |
| SD-CANVAS-GOV | Canvas governing principle | A canvas is a view of a concept, not a separate stored object | CoDL/CaDL Integration |

## 4. CoDL and CaDL: The Authoritative Source

In April 2026 IASA Global published, through the BTABoK 3.2 education portal, two authoritative specifications by Paul Preiss, founder of IASA:

- **CoDL (Concept Definition Language)**: a schema and type language for the stored data structure of any BTABoK concept
- **CaDL (Canvas Definition Language)**: a visual and rendering language that projects CoDL concepts onto canvases

Before April 2026 the public BTABOK mirror did not publish CoDL or CaDL. The SpecChat WIP corpus had developed an architectural direction that independently converged on the same structure: a typed semantic layer for concept records and a separate projection layer for human-facing canvases. When the authoritative sources became available, the WIP corpus required alignment rather than redirection.

### 4.1 CoDL summary

CoDL defines the stored data structure of any BTABoK concept. Its scope is strictly the data layer; it does not define visual layout. CoDL answers: what sections does a concept contain, what fields are stored, what types are allowed, what relationships exist, how is the record persisted and transported.

CoDL language surface:

- **Primitive scalar types:** `shortText`, `text`, `richText`, `id`, `slug`, `date`, `datetime`, `duration`, `boolean`, `integer`, `decimal`, `currency`, `percentage`, `score1to10`, `url`, `email`, `semver`
- **Composite types:** `enum(values...)`, `flags(values...)`, `range<T>`, `measurement`, `threshold`, `keyValue`, `contact`
- **Reference types:** `ref<ConceptType>`, `weakRef`, `externalRef`
- **Section modifiers:** `[list]`, `[set]`, `[map]`, `[matrix]`, `[timeline]`, `[optional]`
- **Constraints:** `required`, `optional`, `min <n>`, `max <n>`
- **Relationship declarations (0.2):** `uses<T>`, `implements<T>`, `supersedes<self>`, `supersededBy<self>` with cardinality
- **Storage directives (0.2):** `primaryKey`, `indexOn`, `partitionBy`, `immutableFields`, `auditFields`, `retentionPolicy`
- **Transport envelope (0.2):** `format`, `version`, `sourceSystem`, `targetSystem`, `correlationId`, `sentAt`, `schemaRef`
- **Display hints (0.2, inline, consumed by CaDL):** `@display(widget=..., colorMap=..., prominence=..., label=...)`

CoDL 0.2 specifies a Standard BTABoK Metadata profile (`BTABoKItem`) whose fields include `slug`, `itemType`, `name`, `shortDescription`, `version`, `baseVersion`, `bokStatus`, `publishStatus`, `accessTier`, `authors`, `reviewers`, `committer`, `tags`, `certainty`, `createdAt`, `updatedAt`. Under the Option X absorption (Section 5.1), the universal subset of that profile is part of Core SpecLang; the BTABOK-specific remainder stays in the profile.

### 4.2 CaDL summary

CaDL describes how a CoDL concept is displayed as a canvas. CaDL surface:

```
canvas <CanvasName> for <ConceptName> {
  area <AreaName> {
    shows: <sectionName>.<fieldName>
  }
  area <AreaName> {
    repeats: <sectionName>
    shows: <fieldName>
  }
  area <AreaName> {
    repeats: <sectionName>
    shows: <relationshipField> -> <displayField>
  }
}
```

A canvas always references a CoDL concept. CaDL does not redefine the data model. It selects which sections are shown, how they are grouped, which fields are rendered, and how related concepts are surfaced.

#### 4.2.1 CaDL directive reference

Appendix B uses several directives beyond the minimal surface shown above. They are introduced here for reference; full semantics live in the upstream CaDL specification.

- `output: enum(markdown-table, mermaid, both) default(markdown-table)`. The rendering target for the canvas.
- `for <Concept> collection`. Targets a collection of instances rather than a single instance.
- `joins: <OtherConcept> via <relationshipField>`. Joins a second concept into the canvas via the named relationship field.
- `format: enum(table-row, badge, card-header, timeline-entry, node)`. Per-area rendering hint consumed by the rendering pipeline.
- `freshnessCheck: include`. Emits a warning at the head of the rendered output if any referenced concept is past its freshness SLA.

### 4.3 The Core Governing Principle

**A canvas is a view of a concept, not a separate stored object type.** The same concept can be rendered as a registry row, a decision card, or an ADR printout without being re-declared. Storage is in CoDL. Display is in CaDL. The two languages do not overlap.

### 4.4 Option A: CoDL syntax canonical with optional aliases

When CoDL became available, two implementation options emerged for how the SpecChat BTABOK profile should relate to CoDL syntactically.

- **Option A.** Spec files with the BTABOK profile active contain CoDL `concept` blocks directly.
- **Option B.** SpecLang-native declarations (`decision`, `asr`, `stakeholder`, etc.) compile to CoDL concept records behind the scenes.

**Adopted: Option A with optional SpecLang-style aliases.** CoDL syntax is the canonical form. Optional short forms desugar to CoDL records, following The Standard precedent. Reasoning:

1. **Interoperability first.** CoDL is the BTABOK standard. Round-tripping with other BTABOK-aligned tools is higher value than local ergonomics.
2. **One source of truth.** If the canonical form is CoDL, there is no drift between SpecChat specs and BTABOK concept records.
3. **The Standard precedent supports it.** The Standard extension already demonstrates that ergonomic aliases can desugar to canonical forms inside SpecLang.
4. **Scope stays honest.** CoDL is neutral on concept types. SpecChat's BTABOK profile defines which CoDL concepts are in scope for Engagement Model alignment.

## 5. Core SpecLang Surface

Under the Option X absorption, seven elements move into Core SpecLang. Every SpecChat collection, regardless of profile, benefits from these features. A profile layers practice-specific concept types, metadata extensions, validators, and canvases on top.

### 5.1 Standard SpecItem Metadata Profile

Every spec and every concept in Core SpecLang carries a standard metadata profile. This replaces ad-hoc tracking-block fields with a named, typed set.

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

SpecChat's existing lifecycle states (Draft, Reviewed, Approved, Executed, Verified) are canonical in core. CoDL's `publishStatus` vocabulary (Draft, Review, Approved, Published, Retired) is preserved as an interop mapping for BTABOK-profile export, not as a core replacement. The mapping is:

| SpecChat state | CoDL publishStatus | Notes |
|---|---|---|
| Draft | Draft | Identical |
| Reviewed | Review | Preserve SpecChat spelling in specs; emit CoDL form in exports |
| Approved | Approved | Identical |
| Executed | Published | SpecChat's Executed state corresponds to CoDL's Published (the concept is live and operative) |
| Verified | Published (with verification audit record) | CoDL has no separate Verified state; SpecChat preserves Verified as a local extension backed by a verification record |
| (archived/superseded) | Retired | Aligns with SpecChat's archival via `supersededBy` or retention policy |

Whether the core vocabulary should eventually realign with CoDL is tracked as an Option Y item in Section 14.

### 5.2 Reference Types

Three reference types are first-class Core SpecLang constructs.

| Type | Semantics | Resolution policy |
|---|---|---|
| `ref<ConceptType>` | Strong reference to a concept instance in the same collection | Must resolve at validation time. Unresolved references are errors (SPEC-REF-001). |
| `weakRef` | Reference where the target may not exist locally | Resolution attempted; unresolved references are warnings (SPEC-REF-002). Used for cross-collection references or intentional forward references. |
| `externalRef` | Reference to a non-SpecChat system (Jira, Confluence, ADO, Miro) | Not resolved by the validator; format-validated only. Carries `{ system, refId, url }`. |

The `CollectionIndex` service that supports reference resolution is a core service.

### 5.3 Relationship Declarations with Cardinality

Core specs can declare typed relationships with explicit cardinality:

```
relationships {
  uses<BaseSystemSpec>      as implements   cardinality(1..1)
  uses<DecisionSpec>        as decisions    cardinality(0..*)
  implements<BaseSystemSpec> as base        cardinality(1..1)
  supersedes<self>          as supersedes   cardinality(0..1)
  supersededBy<self>        as supersededBy cardinality(0..1)
}
```

The relationship kinds (`uses`, `implements`, `supersedes`, `supersededBy`) and the `cardinality(<range>)` syntax are core. Profile-specific relationship kinds can be added by extensions.

Profiles extend this vocabulary with domain-specific kinds. Appendix A uses an extended relationship vocabulary introduced by the BTABOK profile. The kinds are grouped by family below; the list is indicative rather than exhaustive.

- Addressing and rendering: `addresses`, `addressesASR`, `renderedBy`, `renders`.
- Enforcement and oversight: `enforcedBy`, `enforces`, `oversees`.
- Sponsorship and feedback: `sponsoredBy`, `usedBy`, `presentedTo`, `feedsInto`.
- Mitigation and replacement: `mitigatedBy`, `replacedBy`, `approvedBy`, `usesWaiver`.
- Aggregation and measurement: `aggregates`, `measures`, `targets`, `contains`.
- Supply and targeting: `suppliedBy`, `targetedBy`, `movesCapability`, `dependsOn`.

Profile extensions declare their relationship kinds in the profile definition. Validators defined by the profile enforce any cardinality or directionality rules on top of the core cardinality machinery.

### 5.4 Retention Policy and Freshness

The `retentionPolicy` enum and the associated `freshnessSla` and `lastReviewed` fields are core metadata. Every spec carries a retention disposition:

- **indefinite.** Continuously maintained; requires `freshnessSla` and `lastReviewed`.
- **archive-on-deprecated.** Point-in-time; remains accessible after it is superseded but is marked deprecated.
- **delete-on-superseded.** Removed when superseded.

Defaults vary by spec type: base system specs default to `indefinite`; feature, bug, amendment, and decision specs default to `archive-on-deprecated`.

### 5.5 Diagnostic Record Extensions

Three optional fields are added to the core `Diagnostic` record:

| Field | Type | Purpose |
|---|---|---|
| `code` | shortText | Searchable diagnostic code (for example `SPEC-REF-001`, `SPEC-SLUG-002`) |
| `validator` | shortText | Name of the validator that produced the diagnostic |
| `suggestion` | text | Optional fix hint shown to the author |

Diagnostic code namespacing:

- `SPEC-` prefix for core SpecLang validators
- `STANDARD-` prefix for The Standard profile validators
- `BTABOK-` prefix for BTABOK profile validators

Codes follow the pattern `<PREFIX>-<3-letter category>-<3-digit number>`. Example: `SPEC-REF-001`, `STANDARD-FLO-002`, `BTABOK-ASR-001`.

Core `SPEC-` categories at v0.1:

- `MET` metadata
- `SLUG` slug format and uniqueness
- `REF` reference resolution
- `FRS` freshness
- `PRF` profile composition
- `REL` relationships and cardinality

A diagnostic code registry lives alongside core SpecLang. New validators register their codes in the registry; duplicates are rejected at test time.

### 5.6 Slug Rules

Slug format is normative in core SpecLang:

- Lowercase letters, digits, and hyphens only
- Must start with a letter
- No consecutive hyphens
- Maximum 64 characters

Slugs are unique within a collection. Duplicate slugs produce error `SPEC-SLUG-001`. Format violations produce `SPEC-SLUG-002`.

### 5.7 Core Validators

Ten validators run on every collection regardless of profile.

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

Each validator is an MCP tool that accepts a manifest reference and returns a diagnostic list. Validators do not mutate specs.

Diagnostic shape:

```
Diagnostic {
  severity: enum(error, warning, info) required
  code: shortText required
  validator: shortText required
  target: ref<ConceptInstance> required
  message: text required
  suggestion: text optional
  sourceLine: integer optional
}
```

### 5.8 Core Collection Index

`CollectionIndex` is a core service. On validator invocation it indexes every concept and spec instance in the collection by `(itemType, slug)`. Reference resolution, relationship validation, and supersedes-cycle detection all run against the index. The index is invalidated on manifest change; rebuild timing is settled per D-16 Option A (lazy build on first validator invocation; cache until manifest changes), recorded in Section 14.2.

## 6. The BTABOK Profile

The BTABOK profile layers architecture-practice concepts on top of Core SpecLang. At v0.1 it contributes 19 concept types, a metadata extension, 13 profile-specific validators, and 20 CaDL canvases.

### 6.1 Profile Activation

A BTABOK-profile spec collection activates the profile via a declaration in the manifest's metadata table and, optionally, at the top of each base spec. The manifest declaration applies to the whole collection. A spec-level declaration is only permitted if it matches the manifest declaration.

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

**One profile at a time.** A collection declares exactly one profile. The profile options at v0.1 are:

| Profile | Purpose |
|---|---|
| `Core` | Core SpecLang only; no profile extensions active |
| `TheStandard` | Hassan Habib's The Standard profile |
| `BTABOK` | The BTABOK Engagement Model profile |

A BTABOK-profile collection can still reference systems built under a different profile via `externalRef`. The profile applies to the collection, not to the reference graph.

**Profile version tracking.** The declaration carries three versions: `version` (SpecChat BTABOK profile version), `codl` (CoDL specification version), and `cadl` (CaDL specification version). Validators use these to select the correct validation rules.

**What activation changes.** Activation makes available: CoDL concept definitions (canonical `concept <Name> { ... }`), CaDL canvas definitions (`canvas <Name> for <Concept> { ... }`), the BTABoKItem metadata extension, the BTABOK validator set, the `/spec-btabok` authoring command, and CaDL projection commands. Activation does not change core SpecLang grammar, lifecycle state names, spec file naming, or the validity of any `Core`-profile spec that migrates to `BTABOK`.

### 6.2 BTABoKItem Metadata Extensions

When the BTABOK profile is active, the `BTABoKItem` extension adds BTABOK-specific metadata on top of the Core SpecItem fields. These remain profile-scoped:

| BTABoKItem field | Purpose |
|---|---|
| `accessTier` | Free / Member / Paid / Restricted |
| `bokStatus` | Add / Update / Remove / Archive |
| `certainty` | score1to10 |
| `baseVersion` | integer |
| `topicAreaId` | BTABOK topic area identity |
| `publishedBokId` | Published BoK identity |
| `collectionId` | BTABOK collection identity |
| `documentId` | BTABOK document identity |
| `baseDocumentId` | Base document identity for versioned BoK entries |
| `referenceCollectionId` | Cross-reference to another BTABOK collection |
| `referenceDocumentId` | Cross-reference to another BTABOK document |
| `commentThreadId` | Comment thread identity |
| `imageId`, `databaseId`, `tableId` | Asset and data identities specific to BTABOK content |

A BTABOK-profile concept therefore carries a composite metadata surface: the Core SpecItem fields (implicit because they are core) plus the BTABoKItem fields layered above.

### 6.3 BTABOK Concept Catalog

The BTABOK profile defines 19 CoDL concept types at v0.1, grouped by concern. Full CoDL definitions are in Appendix A.

**Stakeholder and Concern Concepts**

- **StakeholderCard** (BTABoK standard). Stakeholder, concerns, influence, engagement strategy, required viewpoints. Relationships: `uses<ViewpointCard>`.
- **PrincipleCard** (BTABoK standard). Enterprise architecture principle with statement, rationale, and links to ASRs it implements and decisions that depend on it.

**Requirement and Decision Concepts**

- **ASRCard** (BTABoK standard). Architecturally significant requirement with statement, significance classification, rationale, verification, and characteristics (Complete, Traceable, Current, Verifiable, Valuable).
- **DecisionRecord** (BTABoK standard). Architecturally significant decision with scope, type, method, options, recommendation, reversibility, linked ASRs, linked principles, cascades, status, and decision date.

**Governance Concepts**

- **GovernanceBody**. Governance body (ETSC, EARB, Domain Design Council, Regional Architecture Forum, Repository Stewardship Group) with scope, authority, chair, membership, cadence.
- **GovernanceRule**. Governance rule (every material change requires a decision record, every exception requires a waiver, etc.) with applicability, enforcement posture, linked ASRs.
- **WaiverRecord** (BTABoK standard). Approved exception to a rule, principle, or constraint, with rule reference, description, justification, risk, compensating controls, expiration, approver.

**View and Viewpoint Concepts**

- **ViewpointCard** (BTABoK standard). Reusable viewpoint template with audience, concerns answered, required models, owner.
- **CanvasDefinition**. CaDL canvas declaration as a concept, so canvases themselves are first-class stored artifacts.

**Quality and Standards Concepts**

- **QualityAttributeScenario**. Quality attribute scenario with stimulus, environment, response, response measure, threshold.
- **StandardCard**. External or internal standard adopted by the organization (EPCIS, DCSA, ISO 28000, NIST CSF, internal canonical schemas).

**Capability and Roadmap Concepts**

- **CapabilityCard** (BTABoK standard). Business or technology capability with baseline maturity, target maturity, strategic importance, gap analysis.
- **RoadmapItem**. Individual item on the strategic roadmap with milestone, sequence, dependencies, capability movements.
- **TransitionArchitecture**. Baseline-to-target transition plan with scope, baseline state, target state, sequencing, dependencies.

**Operational Concepts**

- **RiskCard**. Architectural risk with impact, probability, owner, mitigation.
- **ExperimentCard** (BTABoK standard). Innovation experiment with hypothesis, cost cap, duration, kill criteria, success measures.
- **LegacyModernizationRecord**. Legacy-system decommissioning or modernization plan with current state, target state, migration window, evidence preservation plan, customer migration plan.

**Scorecard and Metric Concepts**

- **MetricDefinition**. Metric with baseline, target, measurement method, measurement period, owner.
- **ScorecardDefinition**. Scorecard aggregating metrics for a specific audience (executive, architecture, operational).

Of the 19 concepts, 8 are BTABoK-standard (StakeholderCard, PrincipleCard, ASRCard, DecisionRecord, WaiverRecord, ViewpointCard, ExperimentCard, CapabilityCard) and 11 are SpecChat-specific additions that fit the BTABOK profile's needs but are not part of IASA's published BTABoK concept catalog (GovernanceBody, GovernanceRule, CanvasDefinition, QualityAttributeScenario, StandardCard, RoadmapItem, TransitionArchitecture, RiskCard, LegacyModernizationRecord, MetricDefinition, ScorecardDefinition).

### 6.4 BTABOK Validators

Thirteen profile-specific validators activate only when `Profile.profileName = BTABOK`. They run after the ten core validators.

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
| `check_governance_approval` | BTABOK-GOV | Every `DecisionRecord` at publishStatus Approved or later has a committer that is a registered member of a GovernanceBody with approval authority for the decision's scope | Error |
| `check_roadmap_capability_moves` | BTABOK-RMP | Every `TransitionArchitecture` references valid `CapabilityCard` instances | Error |
| `check_canvas_target_exists` | BTABOK-CNV | Every `CanvasDefinition` targets an existing concept type | Error |
| `check_metric_baseline_target` | BTABOK-MET | Every `MetricDefinition` declares baseline and target | Warning |

BTABOK category codes at v0.1: `ASR` requirements, `DEC` decisions, `PRN` principles, `STK` stakeholders, `VPT` viewpoints, `WVR` waivers, `GOV` governance, `RMP` roadmap, `CNV` canvases, `MET` metrics.

**Severity posture.** Warnings are surfaced; errors block. Each validator declares a default severity. Collections can override severity per validator in the manifest Conventions section, within these rules:

- `error` defaults cannot be demoted to warning or info
- `warning` defaults can be promoted to error or demoted to info
- `info` defaults can be promoted but not further demoted

The per-initiative `governancePosture: strict` flag (or `--strict` on the CLI) promotes all warnings to errors.

Per D-09 Option D, manifest `governancePosture` sets a floor. The CLI `--strict` flag can escalate a warnings-posture collection to strict for specific runs but cannot relax a strict-posture collection to warnings. De-escalation is not supported.

Override syntax:

```markdown
| Validator | Severity override |
|---|---|
| check_stakeholder_coverage | error |
| check_externalref_validity | info |
```

### 6.5 CaDL Canvas Catalog

The BTABOK profile ships with a library of 20 CaDL canvases that project its CoDL concepts into stakeholder-oriented views. Canvases are themselves CoDL concepts (type `CanvasDefinition`) so they can be cataloged, owned, and versioned like any other concept. Full CaDL definitions are in Appendix B.

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

**Canvas rendering.** Canvases produce Mermaid diagrams, markdown tables, or a combination, depending on layout. The CLI command `spec project --canvas <CanvasName>` resolves the canvas definition, queries concept instances, and renders the output. A canvas referencing concepts past their freshness SLA emits a warning at the head of the rendered artifact by default. With `--strict`, rendering fails with a clear message.

### 6.6 Manifest Schema

The BTABOK-profile manifest schema combines Core SpecLang metadata fields with BTABOK-specific fields. Because SpecChat is pre-1.0, this registry is not versioned.

```
concept CollectionManifest {
  meta {
    // Core SpecItem fields (supplied by Core SpecLang):
    slug: slug required
    itemType: shortText required                 // "CollectionManifest"
    name: shortText required
    shortDescription: text optional
    version: integer required
    publishStatus: enum(Draft, Reviewed, Approved, Executed, Verified) required
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
    specLangVersion: semver required
    profileVersion: semver required
    codlVersion: semver optional                 // required when profileName = BTABOK
    cadlVersion: semver optional                 // required when profileName = BTABOK
  }

  section SystemMetadata {
    systemName: shortText required
    baseSpec: shortText required
    targetFramework: shortText optional
    specCount: integer required
  }

  section TypeRegistry [list] {
    item {
      typeName: shortText required
      codlConcept: shortText required
      filePattern: shortText optional
      required: boolean required
    }
  }

  section Inventory [list] {
    item {
      filename: shortText required
      codlItemType: shortText required
      slug: slug required
      name: shortText required
      publishStatus: enum(Draft, Reviewed, Approved, Executed, Verified) required
      tier: integer required
      everGreen: boolean required
      retentionPolicy: enum(indefinite, archive-on-deprecated, delete-on-superseded) required
      freshnessSla: duration optional
      lastReviewed: date optional
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
    uses<StakeholderCard> as stakeholders     cardinality(0..*)
    uses<ASRCard>         as requirements     cardinality(0..*)
    uses<DecisionRecord>  as decisions        cardinality(0..*)
    uses<WaiverRecord>    as waivers          cardinality(0..*)
    uses<ViewpointCard>   as viewpoints       cardinality(0..*)
    uses<GovernanceBody>  as governanceBodies cardinality(0..*)
  }
}
```

**What the registry enforces.** Violations produce diagnostics per Section 6.4 severity rules.

- Every spec file in `Inventory` must declare its `codlItemType`, and that item type must appear in `TypeRegistry`.
- Every spec marked `everGreen: true` must declare a `freshnessSla`.
- Every spec's `lastReviewed + freshnessSla` must not be in the past (warning).
- Every reference in `dependencies` must resolve to another inventory entry (error).
- `ExecutionOrder` must be a complete partition of the inventory.
- Execution order respects the dependency graph.
- `Profile.profileName` must be `BTABOK` for this profile's rules to engage.

**Profile compatibility.** SpecChat is pre-1.0; no prior manifest version to remain compatible with. Manifests declaring `Core` that lack registry fields pass through core validation only. Manifests declaring `BTABOK` but lacking registry fields required here fail with migration diagnostics per Section 11.

## 7. Engagement Model Scope

### 7.1 In scope

The BTABOK Engagement Model's operating-model concepts map to SpecChat constructs. The Engagement Model is the subset of BTABOK that describes how architecture work is organized, executed, governed, and delivered. Its operating-model layer contains these concept areas:

- Services
- Assignment
- Architecture Lifecycle
- Decisions
- Design
- Patterns
- Stakeholders
- Requirements
- Views
- Quality Attributes
- Deliverables
- Legacy Modernization
- Repository
- Architecture Tools
- Quality Assurance
- Governance
- Product and Project
- Roadmap
- Experiments

These are the concepts that can be expressed as specification-language constructs, manifest metadata, spec types, or validation rules.

### 7.2 Out of scope

The following BTABOK models are excluded. They are organizational, professional, or strategic concerns that do not belong in a specification language.

**Value Model** (Objectives, Technical Debt, Investment Planning, Scope and Context, Structural Complexity, Coverage, Principles, Analysis, Value Streams, Benefits Realization, Value Methods, Risk Methods). Business-strategy and portfolio-investment concerns. They inform why architecture work is funded; they are not expressible as specification constructs.

**People Model** (Extended Team, Organization, Career, Roles, Competency, Job Description, Community). These define the architecture profession: who architects are, how they are organized, how they advance. Organizational level, not specification level.

**Competency Model** (9 pillars, 80 competency areas, BIISS specializations, certification levels). Professional development substrate. Governs architect capability, not system architecture.

**Structured Canvases** (75+ workshop tools across 8 categories). Stakeholder engagement instruments. CaDL is accepted as the mechanism for projecting CoDL concepts into canvas views (in scope). The specific content of BTABoK's 75+ workshop canvases is not imported as specification artifacts (out of scope).

**Topic Areas** (AI, Systems, DevOps, Cloud, Security, Integration, Sustainability). Knowledge domains cutting across the engagement model. They may influence how specs are written in specific domains but do not define new specification constructs.

### 7.3 Why this boundary

BTABOK's four models serve different purposes:

| BTABOK Model | Purpose | Relationship to SpecChat |
|---|---|---|
| Engagement Model | How architecture work is done | Maps to spec constructs, types, and validation |
| Value Model | Why architecture work is funded | Informs priorities; not expressible as specs |
| People Model | Who does architecture work | Organizational; outside spec language scope |
| Competency Model | How architects develop professionally | Professional development; outside spec language scope |

Only the Engagement Model produces concepts that are structural, reusable across systems, and suitable for machine interpretation. That is the same criterion SpecChat uses to decide what belongs in the core language.

### 7.4 Concept map table

Every Engagement Model operating-model concept is classified by integration layer and current status.

**Integration layers:**
- **Core**: Already present in core SpecLang (including the absorbed infrastructure)
- **Manifest**: Expressible through manifest metadata with minimal additions beyond the core metadata profile
- **Profile**: Requires a BTABOK-profile concept type or profile-specific metadata extension
- **Out of Scope**: Not suitable for specification-language expression

**Status values:** Exists, Latent, Planned, Gap.

| EM Concept | BTABOK Definition | SpecChat Construct | Layer | Status |
|---|---|---|---|---|
| **Architecture Lifecycle** | Six-stage ADLC: Innovation, Strategy, Planning, Transformation, Utilize/Measure, Decommissioning | Manifest lifecycle states (Draft, Reviewed, Approved, Executed, Verified) map to Planning through Utilize/Measure. Innovation and Strategy are pre-spec. Decommissioning is post-spec. | Manifest | Latent |
| **Decisions** | Architecturally significant decisions with scope, type, method, reversibility, cascades, stakeholder impact | Decision Spec document type with structured rationale, options, recommendation, amendments. Traces link decisions to requirements. | Profile | Latent |
| **Decision scope** | Six levels: ecosystem, enterprise, value stream, solution, product/service, module | Not currently classified. Decision specs exist but do not declare scope. | Profile | Gap |
| **Decision type** | Structural, product/framework, constraint, cascade, principle | Not currently classified. | Profile | Gap |
| **Decision method** | Budgeted evaluation, bake-off, cost-benefit analysis, scoring, opinion | Not modeled. Belongs in prose, not formal syntax. | Out of Scope | Gap |
| **Decision cascades** | One decision triggering dependent decisions requiring independent tradeoff analysis | Core relationship declarations with cardinality plus `ref<DecisionRecord>` and manifest `dependencies`. | Core | Latent |
| **Decision records (ADR)** | Traceable decision with context, options, consequences, approver, status | Core SpecItem metadata provides `authors`, `reviewers`, `committer`, `publishStatus`. BTABOK DecisionRecord adds scope/type/method fields on top. | Core + Profile | Latent |
| **Design** | Technical design for software implementation; component composition, patterns, interactions | Core SpecLang systems register: authored/consumed components, topology, contracts, constraints, platform realization. | Core | Exists |
| **Patterns** | Reusable design solutions; reference models, architectural styles | Topology rules, layer contracts (via The Standard), constraints. No pattern catalog artifact. | Core | Latent |
| **Stakeholders** | Identify stakeholders, their concerns, influence, engagement strategy | Context register: `person`, `external system`, `relationship`, `@tag`. Concerns and influence metadata are absent in core; BTABOK `StakeholderCard` adds them. | Core + Profile | Latent |
| **Stakeholder concerns** | Specific interests driving viewpoint selection | Not formalized. BTABOK `StakeholderCard` carries concerns. | Profile | Gap |
| **Stakeholder influence** | Power/interest mapping, engagement strategy | Not representable as core spec constructs. BTABOK-profile concern. | Out of Scope | Gap |
| **Requirements** | Three types: functional, quality attribute, constraint. ASR classification by significance. | Contracts (functional), constraints (constraint-type), invariants. BTABOK `ASRCard` adds explicit significance classification. | Core + Profile | Latent |
| **ASR classification** | Structural impact, quality attribute impact, technical capability impact, political/stakeholder impact, incremental innovation | Not formalized in core. `ASRCard.significance` carries it. | Profile | Gap |
| **ASR characteristics** | Complete, Traceable, Current, Verifiable, Valuable | Core reference resolution and relationship cardinality checks handle traceability; core freshness SLA checks handle currency; contracts provide verifiability. Complete and Valuable remain profile-level judgments. | Core | Latent |
| **Views** | Representation of architecture aspects addressing stakeholder concerns | `view` declarations with Mermaid renderings, model-vs-views separation. | Core | Exists |
| **Viewpoints** | Reusable templates defining audience, concerns, models, principles | Not formalized as reusable templates in core. BTABOK `ViewpointCard`. | Profile | Gap |
| **Viewpoint types** | Strategic, Landscape, Context, Process, Application, Physical, Logical/Information | Context register maps to Context. Deployment maps to Physical. Systems maps to Application. Dynamic maps to Process. No Strategic or Landscape. | Core | Latent |
| **Quality Attributes** | Cross-cutting qualities needing explicit planning, scenarios, measurement | Contracts, constraints, topology prohibitions, phase gates. BTABOK `QualityAttributeScenario` adds structured scenarios. | Core + Profile | Latent |
| **Quality attribute scenarios** | Stimulus, response, threshold, verification obligation | Not formalized in core. `QualityAttributeScenario`. | Profile | Gap |
| **Deliverables** | Seven types: documents, collaboration, models, advisory, tools, decisions, influence | Specs and manifests (documents). Projections (models). Other types not spec artifacts. | Core | Latent |
| **Deliverable ownership** | Named owner, usage tracking, freshness SLA | Core SpecItem metadata carries `authors`, `reviewers`, `committer`. Freshness tracking is core via `retentionPolicy`, `freshnessSla`, `lastReviewed`. | Core | Planned |
| **Deliverable usage** | Unused deliverables should be archived and deleted | No usage telemetry. No archival mechanism. | Out of Scope | Gap |
| **Repository** | Evergreen store of critical artifacts | Spec collection + manifest + projections. Core reference resolution plus core retention/freshness. | Core | Latent |
| **Repository ever-green** | Artifacts the team commits to keeping current | Core `retentionPolicy: indefinite` plus `freshnessSla` and `lastReviewed`, enforced by `check_freshness_sla`. | Core | Planned |
| **Architecture Tools** | Tools supporting lifecycle, traceability, collaboration, APIs, reporting | CLI, projections, semantic analysis, guided authoring. | Core | Latent |
| **Tool integration** | Connectors to issue trackers, CI, source control, dashboards | Not specified. | Out of Scope | Gap |
| **Quality Assurance** | Verification and validation processes | Spec verification flow: contracts against source, declared tests, lifecycle state moves to Verified. | Core + Manifest | Exists |
| **Governance** | Review authority, compliance, waivers | Manifest lifecycle states imply review/approval. Formal bodies, authority, and waivers are profile concepts. | Profile | Latent |
| **Governance boards (ARB)** | Named review body with authority to approve, reject, or waive | BTABOK `GovernanceBody`. | Profile | Planned |
| **Governance waivers** | Controlled deviation from standards with tracked risk and expiration | BTABOK `WaiverRecord`. Uses core infrastructure underneath. | Profile | Planned |
| **Governance bottom-up** | Governance during design and delivery, not post-hoc inspection | SpecChat's design-time validation is inherently bottom-up. | Core | Exists |
| **Product and Project** | Architecture integrated with product/project delivery | Phases, execution tiers, dependency ordering, verification gates. | Core + Manifest | Exists |
| **Roadmap** | Baseline, target, transitions, dependencies, sequencing, milestones | Phases and manifest execution order cover delivery sequencing. BTABOK `RoadmapItem` and `TransitionArchitecture` add business roadmap. | Core + Profile | Latent |
| **Roadmap transitions** | Explicit baseline-to-target transition plans | BTABOK `TransitionArchitecture`. | Profile | Gap |
| **Services** | Architecture services delivered to the organization | Not a spec-language concern. | Out of Scope | Gap |
| **Assignment** | How architects are assigned to work | Organizational. Not a spec-language concern. | Out of Scope | Gap |
| **Legacy Modernization** | Assessment and migration of legacy systems | Core: `external system` in context. Profile: BTABOK `LegacyModernizationRecord`. | Core + Profile | Latent |
| **Experiments** | Controlled experiments testing architectural hypotheses | BTABOK `ExperimentCard`. No core demand signal. | Profile | Gap |

## 8. Cross-Document Reference Resolution

Cross-document reference resolution is a Core SpecLang capability (Section 5.2). It applies to every collection regardless of profile. This section describes how the core rules apply inside a BTABOK-profile collection.

### 8.1 Resolution algorithm

1. All concept instances in the collection are indexed by `(itemType, slug)`.
2. For each `ref<T>` field, the validator looks up `(T, refId)` in the index. Miss produces an error (SPEC-REF-001).
3. For each `weakRef` field, the validator attempts the same lookup. Miss produces a warning (SPEC-REF-002).
4. For each `externalRef` field, the validator checks field format only (system, refId, url).

### 8.2 Reference display

CaDL canvases dereference `ref<T>` using the pattern `shows: fieldName -> relatedField`. For example, `shows: requirements -> name` renders the `name` of each referenced ASRCard instance. The validator ensures the referenced field exists on the target concept.

### 8.3 Cycle detection

For relationships with `supersedes` and `supersededBy`, the validator walks the graph and reports cycles as errors (SPEC-REF-003). A concept cannot supersede itself transitively.

## 9. Conflict and Precedence Rules

When a spec expresses rules that conflict at runtime, the following precedence applies.

### 9.1 Constraint vs. waiver

A `WaiverRecord` that waives a `PrincipleCard` or `GovernanceRule` takes precedence over the principle or rule for the scope and duration of the waiver. The validator:

1. Resolves the waiver's `waives<>` target
2. Suppresses the corresponding check for the waiver's scope
3. Emits an informational diagnostic citing the waiver ID
4. Enforces expiration; after expiration the waiver no longer suppresses the check

### 9.2 ASR vs. topology constraint

If an ASR declares a requirement that a topology constraint appears to violate:

1. The topology constraint is authoritative at the structural level (it is a CoDL `ref<>`-resolvable invariant).
2. The ASR is authoritative at the intent level.
3. A mismatch is an error unless a `DecisionRecord` explicitly reconciles them.

This forces apparent conflicts to become explicit decisions, which is the BTABOK preferred posture.

### 9.3 Profile rule vs. manifest override

Validator severity overrides in the manifest (Section 6.4) are permitted only within the allowed directions. An attempt to demote an error to a warning is rejected with `SPEC-PRF-001`. Profile composition is a core concern; the validator `check_profile_composition` and its diagnostic use the core `SPEC-` prefix rather than a profile-specific prefix.

### 9.4 CoDL concept field vs. SpecLang alias

When a spec uses a SpecLang-style alias (e.g., `decision "..." { ... }`) that desugars to a CoDL concept, the canonical form wins on any apparent mismatch. Aliases are syntactic sugar; the validator normalizes to the canonical CoDL form before running checks.

## 10. Authoring Workflow

### 10.1 /spec-btabok command

`/spec-btabok` is the Claude Code slash command that guides authoring of BTABOK-profile specs. The command file lives at `.claude/commands/spec-btabok.md`. It accepts a natural-language description of the artifact and routes to the appropriate question flow based on the detected CoDL concept type.

**Routing by intent signal:**

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

**Question flow structure (each flow):**

1. **Identity.** Collect slug, name, shortDescription, itemType (implicit from flow).
2. **Governance.** Collect authors, reviewers, committer, accessTier.
3. **Content.** Collect the concept-specific sections.
4. **Relationship.** Collect references to other concepts (existing or planned).
5. **Confirmation.** Preview the CoDL block; author can revise.
6. **Emit.** Write the spec file and update the manifest inventory.

**Output.** The output is a `.spec.md` file in the collection's configured directory, with a tracking block matching the manifest convention, a CoDL concept definition block, generated slugs, cross-references, and an author-editable markdown prose section.

### 10.2 Relationship to /spec-chat and /spec-the-standard

`/spec-chat` serves Core-profile authoring. When invoked in a BTABOK-profile collection, it suggests delegation to `/spec-btabok` but does not refuse; the author may intentionally want core-register authoring inside a BTABOK collection (for example, to edit a base system spec's topology).

`/spec-the-standard` serves TheStandard-profile authoring and is mutually exclusive with `/spec-btabok` at the collection level per the one-profile-at-a-time rule.

## 11. Legacy Spec Migration

### 11.1 Migration rule

A legacy spec that claims the BTABOK profile must meet the profile's requirements before the claim is accepted. There is no grandfathering. A manifest declaring `Profile.profileName: BTABOK` but containing legacy specs without Core SpecItem metadata and (where required) BTABoKItem extensions fails validation.

### 11.2 Per-spec migration checklist

1. Add the Core SpecItem metadata (slug, itemType, name, version, publishStatus, authors, committer, createdAt, updatedAt, retentionPolicy) to the tracking block.
2. For indefinite-retention specs, add `freshnessSla` and `lastReviewed`.
3. Add `codlItemType` declaration.
4. Convert informal cross-references to `ref<T>`, `weakRef`, or `externalRef`.
5. For BTABOK profile adoption, add BTABoKItem fields (accessTier, certainty, and the relevant identity fields).
6. Add validator severity overrides if the default posture is inappropriate for this spec.

### 11.3 Migration assistance

The CLI provides `spec migrate --profile BTABOK` which walks each spec, identifies missing fields, and produces a migration plan. The command does not auto-migrate; it produces a patch per spec for author review.

### 11.4 Validation during migration

A manifest in migration state can declare `Profile.migrationFrom: Core` (or `Profile.migrationFrom: TheStandard`). This temporarily relaxes reference-resolution errors to warnings during the transition. Once migration is complete (the author removes `migrationFrom`), full validation engages. The migration state itself is recorded as a spec of type `MigrationPlan` that tracks completed and pending items. A `MigrationPlan` carries `retentionPolicy: archive-on-deprecated` and is archived once complete.

### 11.5 Existing sample collections

The sample collections (`blazor-harness`, `TodoApp`, `PizzaShop`, `todo-app-the-standard`) must be updated to meet the new core metadata requirements, regardless of whether they adopt a profile. Sample migration is part of Phase 2a (core validators). The validators cannot ship without samples that exercise them. For each spec: add `slug`, `retentionPolicy` (most will be `indefinite` for base specs, `archive-on-deprecated` for feature/bug/amendment), `freshnessSla` and `lastReviewed` for indefinite specs, `authors` and `committer`, and convert informal cross-references to `ref<T>`. Each sample manifest gains a `Profile` declaration (most will declare `Core`; `todo-app-the-standard` declares `TheStandard`), the `TypeRegistry` section, and updated inventory columns.

## 12. Backward Compatibility

SpecChat is pre-1.0. The BTABOK profile is v0.1. Breaking changes may occur before v1.0.

**Guarantees.**
- A collection using `Profile: Core` is not affected by BTABOK-profile rules.
- A collection using `Profile: TheStandard` is not affected by BTABOK-profile rules.
- Validators introduced by the BTABOK profile do not run against non-BTABOK collections.
- Existing SpecLang grammar is not modified; the BTABOK profile adds recognition for CoDL syntax only when active.

**Breaking changes.** Any changes that would break v0.x specs must be noted in a migration guide and must support a migration window per Section 11.

**Forward compatibility.** A v0.1 collection is valid against v0.2 of the profile if v0.2 only adds concepts, canvases, or validators. A v0.2 collection is not guaranteed valid against v0.1.

## 13. Test Corpus Plan

### 13.1 Known-good specs

The Global Corp enterprise architecture exemplar at `Global-Corp-Exemplar.md` provides the known-good corpus for the BTABOK profile. Each spec file in that exemplar is intended as a passing test case for the profile's validators.

### 13.2 Known-bad specs

A parallel corpus of intentionally broken specs exercises each validator's negative path. At v0.1 the set includes at least one known-bad spec per validator (23 cases minimum: 10 core plus 13 BTABOK-profile).

Known-bad cases are tagged with their owning validator surface so that core cases can be reused across all three profiles (Core, TheStandard, BTABOK) and BTABOK-only cases remain scoped to BTABOK-profile runs.

**Core (SPEC-) known-bad cases:**
- A cross-reference to a non-existent slug (`check_reference_resolution`, SPEC-REF)
- A manifest declaring both `Core` and `BTABOK` profiles (`check_profile_composition`, SPEC-PRF)
- An ever-green spec without a `freshnessSla` (`check_freshness_sla`, SPEC-FRS)
- A cycle in a `supersedes` chain (`check_supersedes_cycles`, SPEC-REF-003)
- A severity override that demotes an error to a warning (SPEC-PRF, rejected at manifest load)

**BTABOK-profile (BTABOK-) known-bad cases:**
- An ASR without traceability to components (`check_asr_traceability`, BTABOK-ASR)
- A DecisionRecord without scope and type (`check_decision_scope_type`, BTABOK-DEC)
- A WaiverRecord with expiration in the past (`check_waiver_expiration`, BTABOK-WVR)
- A canvas targeting a non-existent concept type (`check_canvas_target_exists`, BTABOK-CNV)

### 13.3 Test corpus location

Core SpecLang fixtures live under `samples/core-speclang/`. BTABOK-profile fixtures live under `samples/btabok-profile/`. Each test case is a small collection with a manifest plus the minimal specs needed to exercise the validator.

### 13.4 Continuous validation

Every validator (core and BTABOK) must have at least one passing and one failing test case in the corpus before v0.1 is considered implementation-complete.

## 14. Open Items for Future Versions

### 14.1 Option Y deferrals

The following items were evaluated during Option X and deferred for observation. They remain in the BTABOK profile (or, where noted, absent) until broader demand is shown.

1. **`publishStatus` vocabulary alignment** with CoDL's Draft/Review/Approved/Published/Retired. Currently SpecChat-native names are canonical in core; CoDL vocabulary is an interop mapping used on BTABOK export.
2. **Canvas/projection as a first-class core concept.** Currently CaDL canvases live in the BTABOK profile only.
3. **Decision Spec enrichment** with scope, type, method, reversibility, cascades. Currently BTABOK `DecisionRecord` has these; core Decision Spec does not.
4. **Viewpoint as a reusable template distinct from view.** Currently core has `view` declarations; BTABOK has `ViewpointCard` as a reusable template.
5. **ASR as a generalized spec type** in core. Currently core has contracts, invariants, and constraints; BTABOK has explicit `ASRCard`.
6. **MetricDefinition** as a general measurement concept. Currently BTABOK-profile only.
7. **ExperimentCard** as a general innovation-management concept. Currently BTABOK-profile only.

Each deferred item has a clear observation question: does BTABOK-profile usage show sustained demand for this concept in non-architecture-practice contexts?

### 14.2 Settlement of Option X sub-decisions

The five sub-decisions that emerged during the Option X absorption work are all settled. They are recorded here with the chosen option and a one-line rationale. Full detail lives in `SpecChat-Design-Decisions-Record.md`.

**SD-01: Core metadata required fields strictness.** Settled as D-13 Option A: all core metadata fields are required on every spec immediately. Pre-1.0 has no legacy to grandfather; the `migrationFrom` manifest field handles transition for existing samples without permanently relaxing strictness.

**SD-02: PersonRef source.** Settled as D-14 Option A: PersonRef is inline in every spec. Honors CoDL's native inline `PersonRef` type; no SpecChat-specific `Person` concept is invented at v0.1.

**SD-03: Retention policy default when unspecified.** Settled as D-15 Option B: default by spec type (indefinite for base specs, archive-on-deprecated for evolution specs). The validator emits an info-level `SPEC-RET-001` diagnostic noting the default used; authors can override explicitly.

**SD-04: Core validator naming convention.** Settled as Option C: all validators use the `check_` prefix (core, Standard, BTABOK). Consistent with the existing codebase.

**SD-05: CollectionIndex initialization timing.** Settled as D-16 Option A: build on first validator invocation; cache until manifest changes. Pays for what callers use and amortizes the build cost across typical MCP usage patterns.

### 14.3 Items deferred to BTABOK profile v0.2 or later

1. **Profile composition.** Activating multiple profiles in one collection (e.g., BTABOK plus TheStandard).
2. **Value Model profile.** Benefits, objectives, measures. Currently out of scope for the Engagement Model profile.
3. **People Model profile.** Organization, roles, competencies.
4. **Bidirectional CoDL export.** Emitting standalone CoDL files for external tools and ingesting external CoDL files into SpecChat collections.
5. **Transport envelope in CLI.** Automatic envelope emission during cross-system export per CoDL 0.2 transport envelope specification.
6. **Diagnostic code reference document.** A complete catalog of every defined diagnostic code with examples.
7. **Additional canvases.** Future versions will add canvases as the exemplar corpus identifies gaps (Innovation Funnel, Architecture Fitness Function dashboard, Deprecation Schedule).
8. **Custom concept types.** A mechanism for collections to define their own CoDL concepts within a BTABOK-profile collection, without forking the profile.
9. **CaDL interactivity.** CaDL v0.1 specifies static rendering. Future versions may add interactive canvas features (filtering, zoom).
10. **Alias disambiguation.** The alias set for Option A must be finalized and documented. Collisions with SpecLang keywords must be resolved.

---

## Appendix A. CoDL Concept Definitions

This appendix contains the full CoDL definition for each of the 19 concept types in the BTABOK profile v0.1.

The meta blocks below represent the combined Core SpecItem metadata (supplied by Core SpecLang per Section 5.1) plus the BTABoKItem extension (supplied by the BTABOK profile per Section 6.2). To keep definitions readable, each concept's `meta` block below lists only concept-specific or non-default fields; the full Standard Metadata profile (slug, itemType, name, shortDescription, version, baseVersion, bokStatus, publishStatus, accessTier, authors, reviewers, committer, tags, certainty, createdAt, updatedAt) is implicit for every concept. The SpecChat extensions `freshnessSla` and `lastReviewed` are noted explicitly where the retention policy defaults to `indefinite`.

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

---

## Appendix B. CaDL Canvas Definitions

This appendix contains the full CaDL definition for each of the 20 canvases in the v0.1 library (Section 6.5). Each subsection gives a purpose statement, target concept(s), primary audience, default output format, and the CaDL source. All canvases emit a freshness warning at the head of the rendered output if any referenced concept is past its freshness SLA.

### B.0 Field Reference Rules

Canvas `shows` and `repeats` clauses reference concept fields. Field paths may be unqualified (just the field name) when unambiguous within the target concept, or qualified with the section name (for example, `Classification.scope`) when a concept has multiple fields of the same name across sections. Qualified form is always accepted. The unqualified form in the canvas definitions below assumes the field name is unique within the target concept's sections.

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

Purpose: A catalog of reusable viewpoints with audience, concerns answered, owner, and rendering canvases.

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
    shows: concernsAnswered
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

---

## Appendix C. Worked Example Pointer

The canonical worked example for SpecLang is the Global Corp enterprise architecture exemplar at `Global-Corp-Exemplar.md`. That document demonstrates the BTABOK profile at scale: an enterprise architecture with stakeholders, principles, ASRs, decisions, governance bodies, waivers, viewpoints, canvases, capabilities, a multi-year roadmap, and a transition architecture. Appendix B of the exemplar maps the narrative into a 21-file SpecChat spec collection.

Readers evaluating SpecLang should use the exemplar as the reference for what a complete BTABOK-profile collection looks like, including manifest structure, spec file layout, CoDL concept authoring, CaDL canvas authoring, and cross-document references.

---

## Appendix D. Source References

**[D1]** Preiss, Paul. "Structured Concept Definition Language." BTABoK 3.2, IASA Global Education Portal (2026).
`https://education.iasaglobal.org/browse/btabok/3.2/core-site/core/article/structured-concept-definition-language`

**[D2]** Architecture & Governance Magazine. "Spec-Driven Development is Better With Core Architecture."
`https://www.architectureandgovernance.com/applications-technology/spec-driven-development-is-better-with-core-architecture/`

**[D3]** IASA Global. Business Technology Architecture Body of Knowledge (BTABoK).
`https://iasa-global.github.io/btabok/`

**[D4]** BTABoK Main Page, Architecture Lifecycle, Decisions, Requirements, Views, Views and Viewpoints, Deliverables, Repository, Governance, Governance (Engagement Model), Architecture Tools, What is Architecture. IASA Global pages under `https://iasa-global.github.io/btabok/`.

**[D5]** Global Corp Enterprise Architecture Exemplar. Workspace: `Global-Corp-Exemplar.md`.

**[D6]** Spec Type System (consolidated from Spec-Type-Taxonomy-v0.1, Spec-Type-Validation-Analysis, and Why-We-Created-the-Spec-Type-Taxonomy). Workspace: `Spec-Type-System.md`.

**[D7]** SpecChat Overview. Workspace: `Delivery/spec-chat/SpecChat-Overview.md`.

**[D8]** SpecLang Specification. Workspace: `Delivery/spec-chat/SpecLang-Specification.md`.

**[D9]** SpecLang Grammar. Workspace: `Delivery/spec-chat/SpecLang-Grammar.md`.

**[D10]** The Standard Extension Overview. Workspace: `Delivery/spec-chat/extensions/the-standard/TheStandard-Extension-Overview.md`.

**[D11]** BTABOK and SpecChat Alignment Report. Workspace: `WIP/Archive/BTA-BOK-integration.md`. The original broad-scope gap analysis, preserved for provenance.

**[D12]** Prior WIP source documents consolidated into this document and preserved in `WIP/Archive/`: `BTABOK-Profile-v0.1-Design.md`, `Core-SpecLang-Absorption-Design.md`, `CoDL-CaDL-Integration-Notes.md`, `BTABOK-EngagementModel-Mapping.md`.
