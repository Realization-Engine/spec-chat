# CoDL and CaDL Integration Notes

## Tracking

| Field | Value |
|---|---|
| Document ID | GC-CODL-001 |
| Version | 0.1 |
| Created | 2026-04-17 |
| Last Reviewed | 2026-04-17 |
| State | Draft |
| Owner | PER-01 Lena Brandt, Chief Architect |
| Approver | PER-11 Anja Petersen, Chair EARB |
| Ever-green | Yes |
| Freshness SLA | 90 days |
| Dependencies | BTABOK-EngagementModel-Mapping.md, Spec-Type-Taxonomy-v0.1.md, Spec-Type-Validation-Analysis.md |
| Classification | Integration notes documenting alignment between SpecChat's BTABOK profile work and the authoritative CoDL/CaDL sources published by IASA |

## 1. Purpose

This document records the alignment between SpecChat's BTABOK profile design and the authoritative CoDL (Concept Definition Language) and CaDL (Canvas Definition Language) specifications published by IASA Global through the BTABoK 3.2 portal. [R1]

Before April 2026 the public BTABOK mirror did not publish CoDL or CaDL. The SpecChat WIP corpus developed an architectural direction that independently converged on the same structure: a typed semantic layer for concept records and a separate projection layer for human-facing canvases. When the authoritative source material became available, the WIP corpus required alignment rather than redirection. This document records the alignment decisions and the specific revisions that flow from them.

## 2. Source Summary

### 2.1 CoDL (Concept Definition Language)

CoDL is a schema/type language authored by Paul Preiss, founder of IASA. It defines the stored data structure of any BTABoK concept. Its scope is strictly the data layer. It does not define visual layout. [R1]

CoDL answers: what sections does a concept contain, what fields are stored, what types are allowed, what relationships exist to other concepts, how is the record persisted and transported.

The language surface includes:

- **Primitive scalar types:** `shortText`, `text`, `richText`, `id`, `slug`, `date`, `datetime`, `duration`, `boolean`, `integer`, `decimal`, `currency`, `percentage`, `score1to10`, `url`, `email`, `semver`
- **Composite types:** `enum(values...)`, `flags(values...)`, `range<T>`, `measurement`, `threshold`, `keyValue`, `contact`
- **Reference types:** `ref<ConceptType>`, `weakRef`, `externalRef`
- **Section modifiers:** `[list]`, `[set]`, `[map]`, `[matrix]`, `[timeline]`, `[optional]`
- **Constraints:** `required`, `optional`, `min <n>`, `max <n>`
- **Relationship declarations (0.2):** `uses<T>`, `implements<T>`, `supersedes<self>`, `supersededBy<self>` with cardinality
- **Storage directives (0.2):** `primaryKey`, `indexOn`, `partitionBy`, `immutableFields`, `auditFields`, `retentionPolicy`
- **Transport envelope (0.2):** `format`, `version`, `sourceSystem`, `targetSystem`, `correlationId`, `sentAt`, `schemaRef`
- **Display hints (0.2, inline, consumed by CaDL):** `@display(widget=..., colorMap=..., prominence=..., label=...)`

CoDL 0.2 also specifies a **Standard BTABoK Metadata profile** (`BTABoKItem`) that every concept carries. Its fields include `slug`, `itemType`, `name`, `shortDescription`, `version`, `baseVersion`, `bokStatus`, `publishStatus`, `accessTier`, `authors`, `reviewers`, `committer`, `tags`, `certainty`, `createdAt`, `updatedAt`.

### 2.2 CaDL (Canvas Definition Language)

CaDL is the visual/rendering language, also authored by Paul Preiss. It describes how a CoDL concept is displayed as a canvas. Its governing principle is decisive: **"A canvas is a view of a concept, not a separate stored object type."** [R1]

A canvas always references a CoDL concept. CaDL does not redefine the data model. It selects which sections are shown, how they are grouped, which fields are rendered, and how related concepts are surfaced.

CaDL surface:

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

### 2.3 Core Governing Principle

The single most important rule in the source material: **an ADR is a CoDL concept plus one or more CaDL canvas definitions.** The same concept can be rendered as a registry row, a decision card, or an ADR printout without being re-declared. Storage is in CoDL. Display is in CaDL. The two languages do not overlap.

## 3. Relationship to the SpecChat WIP Corpus

Before the CoDL/CaDL source was available, the SpecChat WIP corpus had established:

1. A three-layer stratification (Core SpecLang / BTABOK Profile / Spec Type Taxonomy) in [Why-We-Created-the-Spec-Type-Taxonomy.md](Why-We-Created-the-Spec-Type-Taxonomy.md)
2. A Spec Type Taxonomy with 14 types across 4 families in [Spec-Type-Taxonomy-v0.1.md](Spec-Type-Taxonomy-v0.1.md)
3. A distinction between Type Profile (validation contract) and DSL Extension (new syntax) in [Spec-Type-Validation-Analysis.md](Spec-Type-Validation-Analysis.md)
4. A scoped concept map limited to the BTABOK Engagement Model in [BTABOK-EngagementModel-Mapping.md](BTABOK-EngagementModel-Mapping.md)
5. A worked example exemplar in [Global-Corp-BTABOK-Enterprise-Architecture.md](Global-Corp-BTABOK-Enterprise-Architecture.md)

The mapping between those constructs and CoDL/CaDL is as follows.

Note: the Option X decision captured in [Core-SpecLang-Absorption-Design.md](Core-SpecLang-Absorption-Design.md) reshapes this mapping. A substantial portion of what was originally envisioned as a BTABOK profile addition has moved into Core SpecLang. The "Scope" column below records, for each row, whether the construct now lives in Core SpecLang (and applies to every collection regardless of profile) or remains within the BTABOK profile.

| WIP corpus construct | CoDL/CaDL equivalent | Scope |
|---|---|---|
| BTABOK Profile for SpecLang | A SpecLang dialect that authors spec files using CoDL concept syntax beyond core | BTABOK profile |
| Proposed DSL extensions (decision, asr, stakeholder, waiver, principle, risk, viewpoint, capability) | CoDL concept types specific to BTABOK, defined using CoDL's `concept` keyword | BTABOK profile |
| Type Profile (validation contract) for BTABOK concepts | The CoDL concept definition itself for BTABOK-specific types, including required/optional fields, cardinality, relationship declarations | BTABOK profile |
| Projection (Decision Registry, ASR Matrix, Stakeholder Concern Map, Viewpoint Coverage Report, Freshness Report) | CaDL canvas declarations over BTABOK concepts | BTABOK profile |
| Stable human-readable ID (ASR-01, ASD-01, WVR-01) | CoDL `slug` field in the Standard Metadata profile | Core SpecLang |
| Owner and approver fields | CoDL `authors`, `reviewers`, `committer` in the Standard Metadata profile | Core SpecLang |
| Ever-green flag and freshness SLA | `retentionPolicy` enum plus `freshnessSla` and `lastReviewed`; CoDL's enum happens to match | Core SpecLang |
| Cross-document reference convention | `ref<T>`, `weakRef`, `externalRef` reference types | Core SpecLang |
| Lifecycle states (Draft, Reviewed, Approved, Executed, Verified) | SpecChat lifecycle vocabulary is canonical in core; CoDL `publishStatus` (Draft, Review, Approved, Published, Retired) is an interop mapping used by the BTABOK profile on export | Core SpecLang (vocabulary mapping); BTABOK profile (CoDL-vocabulary interop export) |
| Warnings with treat-warnings-as-errors opt-in | Core posture that applies to all profiles; CoDL is silent on validation posture | Core SpecLang |
| View Gallery (one view per viewpoint) | CaDL canvas definitions over the relevant CoDL concepts | BTABOK profile |

The convergence is substantial. The WIP corpus was not wrong; it was reinventing what Preiss had already published. After Option X, the alignment path has two parts: Core SpecLang adopts the Standard Metadata subset, reference types, relationship declarations, retention policy, and diagnostic extensions directly; the BTABOK profile adopts CoDL concept syntax for its specific concept types and adopts CaDL syntax for its canvases.

## 4. Architectural Decision: Option A vs. Option B

When the CoDL/CaDL source became available, two implementation options emerged for how the SpecChat BTABOK profile should relate to CoDL syntactically.

### 4.1 Option A: SpecLang's BTABOK profile accepts CoDL syntax verbatim

Spec files with the BTABOK profile active can contain CoDL `concept` blocks directly. For example:

```spec
profile BTABOK {
  version: "0.2";
}

concept DecisionRecord {
  meta { ... Standard BTABoK metadata ... }
  section Context { description: text required }
  section Options [list] {
    item {
      name: shortText required
      tradeoffs: text required
    }
  }
  section Recommendation { rationale: text required }
  relationships {
    uses<ASRCard> as requirements cardinality(1..*)
    implements<Principle> as principles cardinality(0..*)
    supersedes<self> as supersedes cardinality(0..1)
  }
}
```

The SpecLang parser recognizes `concept`, `section`, and the CoDL type system when the BTABOK profile is active. Files round-trip cleanly with any other CoDL-compliant tool.

### 4.2 Option B: SpecLang's BTABOK profile maps CoDL concepts to ergonomic aliases

SpecLang-native declarations (`decision`, `asr`, `stakeholder`, etc.) compile to CoDL concept records behind the scenes. For example:

```spec
profile BTABOK {
  version: "0.2";
}

decision "ASD-01 Adopt standards-first canonical model" {
  scope: enterprise;
  type: structural;
  status: accepted;
  addresses: [ASR-01, ASR-04, ASR-07];
  owner: PER-02;
  approver: PER-11;
}
```

The ergonomic form desugars to an equivalent CoDL record. This is the pattern The Standard extension already uses (its `broker`, `foundation service`, `exposer` forms desugar to `authored component` with the corresponding layer property).

### 4.3 Recommended Decision: Option A with Optional Aliases

Adopt CoDL syntax as the canonical form. Allow SpecLang-style short forms as optional aliases for author convenience. Reasoning:

1. **Interoperability first.** CoDL is the BTABOK standard. Round-tripping with other BTABOK-aligned tools is higher value than local ergonomics.
2. **One source of truth.** If the canonical form is CoDL, there is no drift between SpecChat specs and BTABOK concept records.
3. **The Standard precedent supports it.** The Standard extension already demonstrates that ergonomic aliases can desugar to canonical forms inside SpecLang. The same mechanism applies to CoDL.
4. **Scope stays honest.** CoDL is neutral on concept types. SpecChat's BTABOK profile will define which CoDL concepts are in scope for Engagement Model alignment. Value Model and People Model concept types are not defined by the SpecChat profile even though CoDL could carry them.

The decision lands on **Option A with optional SpecLang-style aliases** following The Standard precedent.

Closing note on Option X interaction: several CoDL Standard Metadata fields are now Core SpecLang features, meaning core-profile specs also benefit from CoDL-style metadata discipline even when the BTABOK profile is not active. The Option A decision applies specifically to BTABOK-profile CoDL concept authoring. Core metadata adoption is a separate absorption path recorded as Option X in [Core-SpecLang-Absorption-Design.md](Core-SpecLang-Absorption-Design.md).

## 5. Standard BTABoK Metadata in SpecChat

After the Option X decision, the Standard BTABoK Metadata surface splits into two layers. The core subset is absorbed into Core SpecLang as the Core SpecItem metadata profile; the remainder stays in the BTABOK profile as the `BTABoKItem` extension. A BTABOK-profile concept carries the Core SpecItem metadata implicitly, and the BTABoKItem extension adds BTABOK-specific fields on top.

### Core SpecItem metadata (absorbed into Core SpecLang)

These fields apply to every spec or concept in every collection, regardless of profile.

| Core SpecItem field | Purpose | WIP corpus equivalent being replaced |
|---|---|---|
| `slug` | URL-safe stable identifier | Stable human-readable IDs (ASR-01, ASD-01, WVR-01) |
| `itemType` | The spec or concept type name | Spec type label |
| `name` | Primary human-readable label | Title of the spec or tracking-block name |
| `shortDescription` | Short summary | First line of purpose section |
| `version` | Version number | Version field in tracking block |
| `publishStatus` | SpecChat lifecycle state (Draft, Reviewed, Approved, Executed, Verified) | Lifecycle states in tracking block |
| `authors` | List of PersonRef | Owner field |
| `reviewers` | List of PersonRef | Reviewer field (previously informal) |
| `committer` | Single PersonRef | Approver field |
| `tags` | List of shortText | Tags field |
| `createdAt`, `updatedAt` | Datetime | Created and Last Reviewed fields in tracking block |
| `retentionPolicy` | Enum: `indefinite`, `archive-on-deprecated`, `delete-on-superseded` | Ever-green flag, retention convention |
| `freshnessSla` | Duration; required when `retentionPolicy == indefinite` | Freshness SLA field |
| `lastReviewed` | Date; required when `retentionPolicy == indefinite` | Last Reviewed field |
| `dependencies` | List of ref | Dependencies field |

### BTABoKItem extension (BTABOK profile only)

When the BTABOK profile is active, the BTABoKItem extension adds BTABOK-specific metadata on top of the Core SpecItem fields. These remain profile-scoped.

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

### 5.1 Lifecycle state mapping

Lifecycle state mapping is now a core concern. SpecChat's five lifecycle states (Draft, Reviewed, Approved, Executed, Verified) are canonical Core SpecLang states. CoDL's `publishStatus` enum (Draft, Review, Approved, Published, Retired) is an interop mapping used when exporting to external BTABOK-compliant systems.

The mapping is:

| SpecChat state | CoDL publishStatus | Notes |
|---|---|---|
| Draft | Draft | Identical |
| Reviewed | Review | Preserve SpecChat spelling in specs; emit CoDL form in exports |
| Approved | Approved | Identical |
| Executed | Published | SpecChat's Executed state corresponds to CoDL's Published (the concept is live and operative) |
| Verified | Published (with verification audit record) | CoDL has no separate Verified state; SpecChat preserves it as a local extension backed by a verification record |
| (archived/superseded) | Retired | CoDL Retired aligns with SpecChat's archival via `supersededBy` or retention policy |

The Executed-to-Published mapping is the less obvious one. A SpecChat spec in Executed state has produced implementation and is operative. That matches CoDL's Published semantics. Verified is a SpecChat-only refinement.

## 6. Reference Model Alignment

The cross-document reference model (one of the four blocking gaps identified before the CoDL source was available) is resolved by adopting the three reference types as Core SpecLang constructs. After the Option X decision, `ref<T>`, `weakRef`, and `externalRef` are core features, not BTABOK-profile features. They apply to every collection regardless of profile. CoDL's alignment is preserved because the three forms match CoDL's reference model directly.

| Use case | Core SpecLang construct |
|---|---|
| Stable reference to another concept in the same collection | `ref<ConceptType>` resolved to `{ refType, refId, label, resolved?: {...} }` |
| Reference that may not resolve locally (cross-collection or pre-creation) | `weakRef` |
| Reference to an external system (Jira, Confluence, ADO, Miro) | `externalRef` with `{ system, refId, url }` |

All cross-document references in SpecChat, under any profile, use these three forms. Plain text-based references are not valid; a reference must resolve through one of the three typed forms.

## 7. Retention, Freshness, and Ever-Green

The WIP corpus introduced an `ever-green` boolean and a `freshness-sla` duration field. Under Option X, the retention model is absorbed into Core SpecLang directly. The `retentionPolicy` enum, `freshnessSla`, and `lastReviewed` are core metadata fields on every spec, not profile extensions. CoDL alignment holds because CoDL's enum happens to match the absorbed Core SpecLang design.

| SpecChat concept (Core SpecLang) | CoDL equivalent | Notes |
|---|---|---|
| `retentionPolicy: indefinite` (ever-green) | `retentionPolicy: indefinite` | The spec or concept is continuously maintained; requires `freshnessSla` and `lastReviewed` |
| `retentionPolicy: archive-on-deprecated` (point-in-time) | `retentionPolicy: archive-on-deprecated` | The spec or concept is archived when superseded |
| `retentionPolicy: delete-on-superseded` | `retentionPolicy: delete-on-superseded` | For artifacts that should not accumulate historical versions |
| `freshnessSla` | Not in CoDL base; absorbed as a Core SpecLang metadata field | Maximum review interval for `indefinite` records |
| `lastReviewed` | Not in CoDL base; absorbed as a Core SpecLang metadata field | Most recent review date for `indefinite` records |

Core SpecLang's retention surface is compatible with CoDL's `retentionPolicy` enum and adds the `freshnessSla` and `lastReviewed` fields that the WIP corpus required. This is additive relative to CoDL and does not conflict with it.

## 8. Canvas and Projection Alignment

The WIP corpus described five projection types (Decision Registry, ASR Matrix, Stakeholder Concern Map, Viewpoint Coverage Report, Freshness Report). Each of these is a CaDL canvas over a CoDL concept or a collection of concepts.

Example: the Decision Registry projection becomes:

```
canvas DecisionRegistry for DecisionRecord {
  area "Header" {
    shows: slug
    shows: name
    shows: publishStatus
  }
  area "Scope" {
    shows: scope
    shows: type
  }
  area "Traceability" {
    repeats: requirements
    shows: slug -> name
  }
  area "Approvers" {
    repeats: committer
    shows: name
  }
}
```

The View Gallery in the Global Corp exemplar (10 views for 10 viewpoints) is a CaDL canvas set. Each view becomes a canvas declaration over the relevant CoDL concept.

## 9. Scope Discipline Under CoDL

CoDL is concept-type-neutral. Any concept can be defined in CoDL. The Engagement-Model-only scope that governs SpecChat's BTABOK integration must therefore be enforced at the profile level, not at the language level.

**The SpecChat BTABOK profile defines concept types for** (each concept type uses Core SpecLang infrastructure for metadata, references, relationships, retention, and diagnostics; the concept types themselves are BTABOK-specific):
- Architecturally Significant Requirements (ASRCard)
- Architecturally Significant Decisions (DecisionRecord)
- Principles (PrincipleCard)
- Stakeholders and concerns (StakeholderCard)
- Viewpoints and views (ViewpointCard, CanvasDefinition)
- Waivers (WaiverRecord)
- Risks (RiskCard)
- Capabilities (CapabilityCard)
- Standards (StandardCard)
- Roadmap and transitions (RoadmapItem, TransitionArchitecture)
- Governance bodies and rules (GovernanceBody, GovernanceRule)
- Legacy modernization cases (LegacyModernizationRecord)
- Innovation experiments (ExperimentCard)

**The SpecChat BTABOK profile does NOT define concept types for:**
- Business cases beyond a minimal reference form (Value Model)
- Benefit dependency models (Value Model)
- Objective trees and measures (Value Model)
- Competency assessments (Competency Model)
- Career paths and roles (People Model)
- Organizational structure (People Model)

If a Value Model or People Model profile is needed later, it is a separate SpecChat profile. Per the one-profile-at-a-time rule, it cannot be activated in parallel with the BTABOK Engagement Model profile.

### 9.1 Core SpecLang Provides the Infrastructure

Every profile (the default Core profile, The Standard profile, and the BTABOK profile) builds on the same Core SpecLang infrastructure. Under Option X, the infrastructure explicitly includes:

- `slug`: URL-safe stable identifier with uniqueness and format rules
- Reference types: `ref<T>`, `weakRef`, `externalRef`
- Relationship declarations with cardinality: `uses<T>`, `implements<T>`, `supersedes<self>`, `supersededBy<self>`, `cardinality(<range>)`
- Retention policy: `retentionPolicy` enum plus `freshnessSla` and `lastReviewed`
- Diagnostic codes: `code`, `validator`, `suggestion` fields on the core `Diagnostic` record, with the `SPEC-` prefix reserved for core validators
- Freshness SLA: enforced for any spec with `retentionPolicy: indefinite`
- Standard metadata fields: `slug`, `itemType`, `name`, `shortDescription`, `version`, `publishStatus`, `authors`, `reviewers`, `committer`, `tags`, `createdAt`, `updatedAt`, `dependencies`

Profiles layer on top. The BTABOK profile adds BTABoKItem metadata, 16 concept types, 13 profile-specific validators (with the `BTABOK-` diagnostic prefix), and 20 CaDL canvases. It does not redefine the infrastructure.

## 10. Revisions to the WIP Corpus

The following revisions flow from this alignment. Each revision is applied in a separate pass to the corresponding WIP document. The Option X decision captured in [Core-SpecLang-Absorption-Design.md](Core-SpecLang-Absorption-Design.md) is a further revision pass that reshapes all of these documents by reclassifying several constructs from BTABOK-profile scope to Core SpecLang scope.

| Document | Revision |
|---|---|
| [Core-SpecLang-Absorption-Design.md](Core-SpecLang-Absorption-Design.md) | The design record for the Option X absorption. Drives reclassification across this document and the others listed below. |
| [BTABOK-EngagementModel-Mapping.md](BTABOK-EngagementModel-Mapping.md) | Add a CoDL/CaDL section. Reframe DSL Extension candidates as CoDL concept definitions. Update the profile mechanism description to adopt Option A. Reclassify absorbed items from BTABOK to Core per Option X. |
| [Spec-Type-Taxonomy-v0.1.md](Spec-Type-Taxonomy-v0.1.md) | Annotate each BTABOK-aligned spec type with its corresponding CoDL concept name (ASRCard, DecisionRecord, etc.). Note which metadata fields are core versus profile-scoped. |
| [Spec-Type-Validation-Analysis.md](Spec-Type-Validation-Analysis.md) | Clarify that BTABOK DSL extensions should mirror CoDL syntax rather than invent parallel syntax. Add CaDL as the formal mechanism for projections. Move absorbed validators into the core layer. |
| [Global-Corp-BTABOK-Enterprise-Architecture.md](Global-Corp-BTABOK-Enterprise-Architecture.md) | Rename owner/approver columns to align with authors/reviewers/committer where semantics match. Annotate the View Gallery entries as CaDL canvases. Add `slug` field references where stable IDs currently stand alone. Reframe core metadata annotations as Core SpecLang features rather than CoDL conventions. |

## 11. Open Questions

Items previously listed here have been partly settled by the Option X decision. The list below separates items that are now resolved from items that remain open (the Option Y deferrals and profile-level questions).

### 11.1 Settled by Option X

1. **Reference model placement.** Resolved: `ref<T>`, `weakRef`, and `externalRef` are Core SpecLang constructs.
2. **Standard metadata placement.** Resolved: the Core SpecItem metadata profile is core; the BTABoKItem extension is profile-scoped.
3. **Retention policy placement.** Resolved: `retentionPolicy`, `freshnessSla`, and `lastReviewed` are core metadata fields.
4. **Diagnostic code namespacing.** Resolved: `SPEC-` prefix for core validators, `STANDARD-` for The Standard, `BTABOK-` for the BTABOK profile.
5. **Slug rules placement.** Resolved: slug format and uniqueness are core concerns; the `SPEC-SLUG-` codes are core diagnostics.

### 11.2 Still open (Option Y deferrals and profile-level questions)

1. **publishStatus vocabulary alignment.** Whether Core SpecLang should adopt CoDL's Draft/Review/Approved/Published/Retired vocabulary, or keep SpecChat-native Draft/Reviewed/Approved/Executed/Verified as canonical with CoDL as an interop mapping only. Currently the SpecChat vocabulary is canonical in core.
2. **Canvas and projection as a core concept.** Whether CaDL-style canvases should be a Core SpecLang capability or remain profile-scoped. Currently profile-scoped.
3. **Decision Spec enrichment.** Whether the core Decision Spec should gain scope, type, method, reversibility, and cascades fields that the BTABOK DecisionRecord carries. Currently profile-scoped.
4. **Viewpoint templates.** Whether Core SpecLang should gain a reusable viewpoint template distinct from a `view` declaration. Currently only the BTABOK profile has ViewpointCard.
5. **ASR generalization.** Whether a generalized ASR spec type belongs in core alongside contracts, invariants, and constraints, or remains the BTABOK-specific ASRCard.
6. **MetricDefinition generalization.** Whether MetricDefinition should become a general measurement concept in core, or stay in the BTABOK profile.
7. **ExperimentCard generalization.** Whether ExperimentCard should be a general innovation-management concept in core, or stay in the BTABOK profile.
8. **CoDL version tracking.** SpecChat must declare which CoDL version (0.2 at time of writing) the BTABOK profile targets. Version upgrades require migration planning.
9. **CoDL concept type catalog.** The 16 BTABOK concept types are listed, but the detailed field schemas for each type remain to be drafted.
10. **CaDL canvas catalog.** The 20 canvases the BTABOK profile ships with (Decision Registry, ASR Matrix, etc.) need formal CaDL definitions.
11. **Bidirectional export.** Whether SpecChat can emit standalone CoDL files for external tools, and whether it can ingest external CoDL files into its own collection.
12. **Alias disambiguation.** The alias set for Option A needs to be finalized and documented. Collisions with SpecLang keywords must be resolved.
13. **Transport envelope responsibility.** CoDL 0.2 requires a transport envelope for inter-system exchange. SpecChat's CLI and MCP server interactions will need to emit conformant envelopes for BTABOK-profile specs.

## 12. Source References

**[R1]** Preiss, Paul. "Structured Concept Definition Language." BTABoK 3.2, IASA Global Education Portal.
`https://education.iasaglobal.org/browse/btabok/3.2/core-site/core/article/structured-concept-definition-language`
Retrieved 2026-04-17. Content accessed via authenticated portal; verbatim content is preserved in the conversation transcript that produced this alignment.

**[R2]** Architecture & Governance Magazine. "Spec-Driven Development is Better With Core Architecture."
`https://www.architectureandgovernance.com/applications-technology/spec-driven-development-is-better-with-core-architecture/`
Used for public confirmation that CoDL is a named BTABOK artifact distinct from Views/Viewpoints and ASRs.

**[R3]** IASA Global. Business Technology Architecture Body of Knowledge (BTABoK).
`https://iasa-global.github.io/btabok/`
Used for the BTABOK Engagement Model scope boundary and the Structured Canvas Approach.

**[R4]** Prior workspace analysis: `BTA-BOK-integration.md`, `BTABOK-EngagementModel-Mapping.md`, `Spec-Type-Taxonomy-v0.1.md`, `Spec-Type-Validation-Analysis.md`, `Why-We-Created-the-Spec-Type-Taxonomy.md`, `Global-Corp-BTABOK-Enterprise-Architecture.md`.

**[R5]** Core SpecLang Absorption Design. Workspace: [Core-SpecLang-Absorption-Design.md](Core-SpecLang-Absorption-Design.md). Records the Option X decision that moves the Standard Metadata profile (core subset), reference types, relationship declarations with cardinality, retention policy, diagnostic record extensions, slug rules, and ten core validators from the BTABOK profile into Core SpecLang.
