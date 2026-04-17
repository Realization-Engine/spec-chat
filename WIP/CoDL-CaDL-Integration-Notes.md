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

| WIP corpus construct | CoDL/CaDL equivalent |
|---|---|
| BTABOK Profile for SpecLang | A SpecLang dialect that authors CoDL concept records |
| Proposed DSL extensions (decision, asr, stakeholder, waiver, principle, risk, viewpoint, capability) | CoDL concept types defined using CoDL's `concept` keyword |
| Type Profile (validation contract) | The CoDL concept definition itself, including required/optional fields, cardinality, relationship declarations |
| Projection (Decision Registry, ASR Matrix, Stakeholder Concern Map, Viewpoint Coverage Report, Freshness Report) | CaDL canvas declarations over CoDL concepts |
| Stable human-readable ID (ASR-01, ASD-01, WVR-01) | CoDL `slug` field in the Standard Metadata profile |
| Owner and approver fields | CoDL `authors`, `reviewers`, `committer` in the Standard Metadata profile |
| Ever-green flag and freshness SLA | CoDL `retentionPolicy` enum combined with `auditFields` |
| Cross-document reference convention | CoDL `ref<ConceptType>`, `weakRef`, `externalRef` |
| Lifecycle states (Draft, Reviewed, Approved, Executed, Verified) | CoDL `publishStatus` enum (Draft, Review, Approved, Published, Retired) with Executed/Verified as SpecChat-specific extensions |
| Warnings with treat-warnings-as-errors opt-in | Orthogonal to CoDL; CoDL is silent on validation posture |
| View Gallery (one view per viewpoint) | CaDL canvas definitions over the relevant CoDL concepts |

The convergence is substantial. The WIP corpus was not wrong; it was reinventing what Preiss had already published. The alignment path is straightforward: adopt CoDL syntax for concept definitions, adopt CaDL syntax for canvas definitions, and use the Standard BTABoK Metadata profile everywhere.

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

## 5. Standard BTABoK Metadata in SpecChat

Every concept defined by the SpecChat BTABOK profile carries the Standard BTABoK Metadata profile. This replaces several parallel fields the WIP corpus had proposed.

| Standard CoDL field | Purpose | WIP corpus equivalent being replaced |
|---|---|---|
| `slug` | URL-safe stable identifier | Stable human-readable IDs (ASR-01, ASD-01, WVR-01) |
| `itemType` | The concept type name (ASRCard, DecisionRecord, etc.) | Spec type label |
| `name` | Primary human-readable label | Title of the spec or tracking-block name |
| `shortDescription` | Short summary | First line of purpose section |
| `version` | Concept version number | Version field in tracking block |
| `bokStatus` | Add / Update / Remove / Archive | Implicit in manifest operations |
| `publishStatus` | Draft / Review / Approved / Published / Retired | SpecChat lifecycle states (Draft, Reviewed, Approved, Executed, Verified) |
| `accessTier` | Free / Member / Paid / Restricted | Not previously modeled |
| `authors` | List of PersonRef | Owner field |
| `reviewers` | List of PersonRef | Reviewer field (previously informal) |
| `committer` | Single PersonRef | Approver field |
| `tags` | List of shortText | Tags field |
| `certainty` | Score 1 to 10 | Not previously modeled |
| `createdAt`, `updatedAt` | Datetime | Created and Last Reviewed fields in tracking block |

### 5.1 Lifecycle state mapping

SpecChat uses five lifecycle states: Draft, Reviewed, Approved, Executed, Verified. CoDL uses a five-value `publishStatus` enum: Draft, Review, Approved, Published, Retired.

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

The cross-document reference model (one of the four blocking gaps identified before the CoDL source was available) is resolved by adopting CoDL's three reference types directly.

| Use case | CoDL construct |
|---|---|
| Stable reference to another concept in the same collection | `ref<ConceptType>` resolved to `{ refType, refId, label, resolved?: {...} }` |
| Reference that may not resolve locally (cross-collection or pre-creation) | `weakRef` |
| Reference to a non-BTABOK system (Jira, Confluence, ADO, Miro) | `externalRef` with `{ system, refId, url }` |

All cross-document references in SpecChat's BTABOK profile use these three forms. Plain text-based references are not valid; a reference must resolve through one of the three typed forms.

## 7. Retention, Freshness, and Ever-Green

The WIP corpus introduced an `ever-green` boolean and a `freshness-sla` duration field. CoDL provides a richer `retentionPolicy` enum that subsumes both.

| SpecChat concept | CoDL equivalent | Notes |
|---|---|---|
| Ever-green = true | `retentionPolicy: indefinite` | The concept is continuously maintained |
| Ever-green = false, point-in-time | `retentionPolicy: archive-on-deprecated` | The concept is archived when superseded |
| Explicit deletion on supersession | `retentionPolicy: delete-on-superseded` | For concepts that should not accumulate historical versions |
| Freshness SLA | Derived field, not in CoDL base | SpecChat extension: `freshnessSla: duration` as an additional Standard Metadata field for `indefinite` records |

The SpecChat profile extends CoDL's metadata with `freshnessSla` for concepts marked `indefinite`. This is an additive extension that does not conflict with CoDL.

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

**The SpecChat BTABOK profile defines concept types for:**
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

## 10. Revisions to the WIP Corpus

The following revisions flow from this alignment. Each revision is applied in a separate pass to the corresponding WIP document.

| Document | Revision |
|---|---|
| [BTABOK-EngagementModel-Mapping.md](BTABOK-EngagementModel-Mapping.md) | Add a CoDL/CaDL section. Reframe DSL Extension candidates as CoDL concept definitions. Update the profile mechanism description to adopt Option A. |
| [Spec-Type-Taxonomy-v0.1.md](Spec-Type-Taxonomy-v0.1.md) | Annotate each BTABOK-aligned spec type with its corresponding CoDL concept name (ASRCard, DecisionRecord, etc.). |
| [Spec-Type-Validation-Analysis.md](Spec-Type-Validation-Analysis.md) | Clarify that BTABOK DSL extensions should mirror CoDL syntax rather than invent parallel syntax. Add CaDL as the formal mechanism for projections. |
| [Global-Corp-BTABOK-Enterprise-Architecture.md](Global-Corp-BTABOK-Enterprise-Architecture.md) | Rename owner/approver columns to align with authors/reviewers/committer where semantics match. Annotate the View Gallery entries as CaDL canvases. Add `slug` field references where stable IDs currently stand alone. |

## 11. Open Questions

These items remain unresolved after this alignment and will be addressed in the BTABOK Profile v0.1 design document.

1. **CoDL version tracking.** SpecChat must declare which CoDL version (0.2 at time of writing) the BTABOK profile targets. Version upgrades require migration planning.
2. **CoDL concept type catalog.** The specific CoDL concept types SpecChat will define are listed above, but the detailed field schemas for each type remain to be drafted.
3. **CaDL canvas catalog.** The specific canvases the BTABOK profile ships with (Decision Registry, ASR Matrix, etc.) need formal CaDL definitions.
4. **Bidirectional export.** Whether SpecChat can emit standalone CoDL files for external tools, and whether it can ingest external CoDL files into its own collection.
5. **Alias disambiguation.** If Option A with aliases is adopted, the alias set needs to be finalized and documented. Collisions with SpecLang keywords must be resolved.
6. **Transport envelope responsibility.** CoDL 0.2 requires a transport envelope for inter-system exchange. SpecChat's CLI and MCP server interactions will need to emit conformant envelopes for BTABOK-profile specs.

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
