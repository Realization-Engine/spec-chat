# BTABOK Engagement Model Mapping to SpecChat

## Scope Declaration

### What Is In

This document maps the **BTABOK Engagement Model's operating-model concepts** to SpecChat constructs. The Engagement Model is the subset of BTABOK that describes how architecture work is organized, executed, governed, and delivered. Its operating-model layer contains these concept areas:

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

### What Is Out

The following BTABOK models are **excluded from this mapping**. They are organizational, professional, or strategic concerns that do not belong in a specification language.

**Value Model** (Objectives, Technical Debt, Investment Planning, Scope and Context, Structural Complexity, Coverage, Principles, Analysis, Value Streams, Benefits Realization, Value Methods, Risk Methods). These are business-strategy and portfolio-investment concerns. They inform why architecture work is funded, but they are not expressible as specification constructs.

**People Model** (Extended Team, Organization, Career, Roles, Competency, Job Description, Community). These define the architecture profession itself: who architects are, how they are organized, how they advance. BTABOK is explicit that it is a people framework. That framing is important, but it operates at the organizational level, not the specification level.

**Competency Model** (9 pillars, 80 competency areas, BIISS specializations, certification levels). This is the professional development substrate. It governs architect capability, not system architecture.

**Structured Canvases** (75+ workshop tools across 8 categories). These are stakeholder engagement instruments. CaDL (Canvas Definition Language) is the formal language for canvas definitions within SpecChat's BTABOK profile, so SpecChat accepts CaDL as a mechanism for projecting CoDL concepts into canvas views. The specific content of BTABoK's 75+ workshop canvases, however, remains outside the Engagement Model scope. The distinction is between accepting CaDL as a mechanism (in scope) and importing the full BTABoK canvas library as specification artifacts (out of scope). Where canvas-shaped outputs are useful, SpecChat can produce them as CaDL projections from spec data, but the full workshop canvas library is not a spec-type obligation.

**Topic Areas** (AI, Systems, DevOps, Cloud, Security, Integration, Sustainability). These are knowledge domains that cut across the engagement model. They may influence how specs are written in specific domains but do not define new specification constructs.

### Why This Boundary

BTABOK's own structure justifies the boundary. Its four models serve different purposes:

| BTABOK Model | Purpose | Relationship to SpecChat |
|---|---|---|
| Engagement Model | How architecture work is done | Maps to spec constructs, types, and validation |
| Value Model | Why architecture work is funded | Informs priorities; not expressible as specs |
| People Model | Who does architecture work | Organizational; outside spec language scope |
| Competency Model | How architects develop professionally | Professional development; outside spec language scope |

Only the Engagement Model produces concepts that are structural, reusable across systems, and suitable for machine interpretation. That is the same criterion SpecChat uses to decide what belongs in the core language.

---

## Governing Principles

Six principles govern how Engagement Model concepts enter SpecChat.

### 1. Governed, Not Governing

BTABOK warns repeatedly against "police state" governance. It states that architects should be governed, not governing. Governance should emerge bottom-up during design and delivery, not top-down through inspection boards.

SpecChat's validation machinery (contracts, constraints, package policies, deny rules, topology prohibitions) is powerful. That power can drift toward compliance bureaucracy if wielded without restraint. The governing rule is: **specs are tools for thinking and communication first, compliance instruments second.** Validation exists to catch structural errors, not to enforce organizational conformity.

When adding Engagement Model concepts, prefer enabling over policing. An ASR classification helps architects think about requirement significance. A decision-scope field helps architects communicate impact radius. Neither should become a gate that blocks work without architectural justification.

### 2. Spec Types Before Grammar Expansion

New Engagement Model concepts should enter as **spec types with validation profiles** before they are considered for DSL extensions. A spec type requires only manifest recognition, section schemas, and cross-document rules. A DSL extension requires new keywords, AST nodes, parser changes, and semantic analysis.

The rule from the Spec Type Validation Analysis applies: a concept earns a DSL extension only when it is formal, reusable, and semantically checkable across multiple documents. Until then, it lives in the type profile layer.

### 3. Viewpoints as a Classification Axis

BTABOK defines seven primary viewpoint types: Strategic, Landscape, Context, Process, Application, Physical, and Logical/Information. These are stakeholder-concern-driven frames, not artifact types.

In SpecChat, viewpoints are treated as a **classification axis on spec types**, not as a standalone spec type. Any structural spec can declare which viewpoint(s) it serves. A Deployment/Runtime Spec declares `viewpoint: Physical`. A Context/Stakeholder Spec declares `viewpoint: Context`. This connects specs to the viewpoint taxonomy without creating a separate artifact that duplicates the structural specs.

If a reusable viewpoint template definition is needed later (defining audience, concerns, required models, validation criteria for a viewpoint), that can be introduced as a Viewpoint Template artifact. But it is not part of the minimum viable implementation.

### 4. Ever-Green vs. Point-in-Time

BTABOK's repository model distinguishes "ever-green" artifacts (continuously maintained, always current) from point-in-time artifacts (relevant during a phase, then archived).

In SpecChat, this distinction should be expressible in the manifest or type profile:

- **Ever-green**: Base System Spec, Subsystem Spec, Governance Spec. These remain authoritative until explicitly superseded. They carry a freshness SLA: if not reviewed within the SLA period, validation produces a warning.
- **Point-in-time**: Feature Spec, Bug Spec, Amendment Spec, Decision Spec, Waiver Spec. These matter during their execution window. Once Verified, they become historical records. No freshness obligation.

### 5. Profile Activation, Not Core Grammar Inflation

BTABOK-specific constructs enter SpecChat through a **profile mechanism**, following the precedent set by The Standard extension. The core SpecLang grammar does not change. A BTABOK profile adds metadata fields, validation rules, and projection capabilities that are active only when the profile is declared.

This keeps the core language stable for users who do not need BTABOK alignment.

### 6. CoDL and CaDL Are Canonical

The BTABOK profile adopts CoDL (Concept Definition Language) as the canonical schema language for concept records and CaDL (Canvas Definition Language) as the canonical language for canvas projections. The profile does not invent parallel constructs for concepts or canvases. Where SpecLang-style short forms are offered, they exist only as optional ergonomic aliases that desugar to CoDL records. See Section 11 for the full alignment and [CoDL-CaDL-Integration-Notes.md](CoDL-CaDL-Integration-Notes.md) for source material and detailed mapping.

---

## Engagement Model Concept Map

The table below maps every Engagement Model operating-model concept to SpecChat. Each concept is classified by integration layer and current status.

**Integration layers:**
- **Core**: Already present in core SpecLang
- **Manifest**: Expressible through manifest metadata with minimal additions
- **Type Profile**: Requires a spec type from the taxonomy with a validation profile
- **DSL Extension**: Requires new formal syntax in a BTABOK profile extension
- **Out of Scope**: Not suitable for specification-language expression, even within the Engagement Model

**Status values:**
- **Exists**: Fully present in the current SpecChat corpus
- **Latent**: Raw material exists but is not formalized as this concept
- **Planned**: Defined in the taxonomy or validation analysis but not yet implemented
- **Gap**: No current representation

| EM Concept | BTABOK Definition | SpecChat Construct | Layer | Status |
|---|---|---|---|---|
| **Architecture Lifecycle** | Six-stage ADLC: Innovation, Strategy, Planning, Transformation, Utilize/Measure, Decommissioning | Manifest lifecycle states (Draft, Reviewed, Approved, Executed, Verified) map to Planning through Utilize/Measure. Innovation and Strategy stages are pre-spec. Decommissioning is post-spec. | Manifest | Latent |
| **Decisions** | Architecturally significant decisions with scope, type, method, reversibility, cascades, and stakeholder impact | Decision Spec document type with structured rationale, options, recommendation, amendments. Traces link decisions to requirements. | Type Profile + DSL Extension | Latent |
| **Decision scope** | Six levels: ecosystem, enterprise, value stream, solution, product/service, module | Not currently classified. Decision specs exist but do not declare scope. | DSL Extension | Gap |
| **Decision type** | Structural, product/framework, constraint, cascade, principle | Not currently classified. | DSL Extension | Gap |
| **Decision method** | Budgeted evaluation, bake-off, cost-benefit analysis, scoring, opinion | Not currently modeled. Belongs in prose sections of decision specs, not in formal syntax. | Out of Scope | Gap |
| **Decision cascades** | One decision triggering dependent decisions requiring independent tradeoff analysis | Manifest dependency fields link specs. Cross-document cascade chains are not formalized. | Manifest | Latent |
| **Decision records (ADR)** | Traceable decision with context, options, consequences, approver, status | Decision Spec already carries this substance. Needs standardized fields for approver and status. | Type Profile | Latent |
| **Design** | Technical design for software implementation; component composition, patterns, interactions | Core SpecLang systems register: authored/consumed components, topology, contracts, constraints, platform realization. | Core | Exists |
| **Patterns** | Reusable design solutions; reference models, architectural styles | Topology rules, layer contracts (via The Standard), constraints. No pattern catalog artifact. | Core | Latent |
| **Stakeholders** | Identify stakeholders, their concerns, influence, and engagement strategy | Context register: `person`, `external system`, `relationship`, `@tag`. Concerns and influence metadata are absent. | Core + Type Profile | Latent |
| **Stakeholder concerns** | Specific interests driving viewpoint selection and architecture communication | Not formalized. Persons exist but do not carry concern declarations. | DSL Extension | Gap |
| **Stakeholder influence** | Power/interest mapping, engagement strategy | Not representable as spec constructs. Organizational concern. | Out of Scope | Gap |
| **Requirements** | Three types: functional, quality attribute, constraint. ASR classification by significance. | Contracts (functional), constraints (constraint-type), invariants. No explicit ASR classification or significance scoring. | Core + Type Profile | Latent |
| **ASR classification** | Structural impact, quality attribute impact, technical capability impact, political/stakeholder impact, incremental innovation | Not formalized. Requirements exist but are not classified by architectural significance. | DSL Extension | Gap |
| **ASR characteristics** | Complete, traceable, current, verifiable, valuable | Traces provide traceability. Contracts provide verifiability. Completeness, currency, and value are not checked. | Type Profile | Latent |
| **Views** | Representation of architecture aspects addressing stakeholder concerns | `view` declarations with Mermaid renderings, model-vs-views separation. | Core | Exists |
| **Viewpoints** | Reusable templates defining audience, concerns, models, and principles for constructing views | Not formalized as reusable templates. Views exist at instance level. Viewpoint is treated as a classification axis (see Governing Principles). | Type Profile | Gap |
| **Viewpoint types** | Strategic, Landscape, Context, Process, Application, Physical, Logical/Information | Context register maps to Context viewpoint. Deployment register maps to Physical viewpoint. Systems register maps to Application viewpoint. Dynamic declarations map to Process viewpoint. No Strategic or Landscape viewpoint. | Core | Latent |
| **Quality Attributes** | Cross-cutting qualities needing explicit planning, scenarios, and measurement | Contracts, constraints, topology prohibitions, phase gates. No dedicated quality-attribute scenario artifact. | Core + Type Profile | Latent |
| **Quality attribute scenarios** | Stimulus, response, threshold, verification obligation | Not formalized as structured scenarios. Constraints carry some of this substance implicitly. | DSL Extension | Gap |
| **Deliverables** | Seven types: documents, collaboration, models, advisory, tools, decisions, influence | Specs and manifests are the document deliverables. Projections are model deliverables. Other types (collaboration, advisory, influence) are not spec artifacts. | Core + Manifest | Latent |
| **Deliverable ownership** | Named owner, usage tracking, freshness SLA | No owner field in specs or manifests. No freshness tracking. | Manifest | Gap |
| **Deliverable usage** | Unused deliverables should be archived and deleted | No usage telemetry. No archival mechanism. | Out of Scope | Gap |
| **Repository** | Evergreen store of critical artifacts for architect decision-making and communication | Spec collection + manifest + projections. Repository operating rules (freshness, ownership, query/report) are absent. | Manifest | Latent |
| **Repository ever-green** | Artifacts the team commits to keeping current; everything else archived | Not formalized. See Governing Principle 4. | Manifest | Gap |
| **Architecture Tools** | Tools supporting lifecycle, traceability, collaboration, APIs, and reporting | CLI, projections, semantic analysis, guided authoring. Enterprise tool ecosystem integration is not specified. | Core | Latent |
| **Tool integration** | Connectors to issue trackers, CI, source control, architecture dashboards | Not specified. | Out of Scope | Gap |
| **Quality Assurance** | Verification and validation processes ensuring architecture meets requirements | Spec verification flow: contracts checked against source, tests declared in spec, lifecycle state moves to Verified. | Core + Manifest | Exists |
| **Governance** | Framework of processes and decision-making structure; review authority, compliance, waivers | Manifest lifecycle states imply review/approval. No formal governance body, approval authority, or waiver mechanism. | Type Profile | Latent |
| **Governance boards (ARB)** | Named review body with authority to approve, reject, or waive | Not modeled. | Out of Scope | Gap |
| **Governance waivers** | Controlled deviation from standards with tracked risk and expiration | Not modeled. Waiver Spec is proposed in taxonomy. | Type Profile | Planned |
| **Governance bottom-up** | Governance during design and delivery, not post-hoc inspection | SpecChat's design-time validation (contracts, constraints, topology) is inherently bottom-up. | Core | Exists |
| **Product and Project** | Architecture integrated with product/project delivery | Phases, execution tiers, dependency ordering, verification gates. | Core + Manifest | Exists |
| **Roadmap** | Baseline, target, transitions, dependencies, sequencing, milestones | Phases and manifest execution order cover delivery sequencing. No business roadmap or transition-architecture artifact. | Core + Type Profile | Latent |
| **Roadmap transitions** | Explicit baseline-to-target transition plans | Not formalized. Phases describe build order, not architectural transitions. | Type Profile | Gap |
| **Services** | Architecture services delivered to the organization | Not a spec-language concern. Organizational operating model. | Out of Scope | Gap |
| **Assignment** | How architects are assigned to work | Organizational. Not a spec-language concern. | Out of Scope | Gap |
| **Legacy Modernization** | Assessment and migration of legacy systems | Not a spec-language concern at the construct level. A legacy system can be modeled as an `external system` in context. | Core | Latent |
| **Experiments** | Controlled experiments testing architectural hypotheses | Not formalized. Could be modeled as a spec type but no demand signal yet. | Out of Scope | Gap |

---

## Integration Layers

### Already Present in Core SpecLang

These Engagement Model concepts are already carried by core SpecLang constructs with no changes needed.

**Design.** The systems register (authored/consumed components, topology, contracts, constraints, platform realization) directly implements BTABOK's design concept. Component composition, dependency rules, and structural patterns are first-class.

**Views.** The `view` declaration with mandatory Mermaid renderings, model-vs-views separation, and multi-zoom-level support corresponds to BTABOK's view concept and aligns with ISO 42010.

**Dynamic interactions.** The `dynamic` declaration for behavioral interaction sequences maps to BTABOK's Process viewpoint.

**Quality Assurance.** The spec verification flow (contracts checked against source, tests declared in spec, lifecycle state progression to Verified) implements bottom-up quality assurance.

**Bottom-up governance.** SpecChat's design-time validation (contracts, constraints, topology rules, package policies) is governance during design, not post-hoc inspection. This aligns with BTABOK's preferred governance posture.

**Product and Project delivery.** Phases with validation gates, execution tiers with dependency ordering, and verification flow implement the delivery-lifecycle integration BTABOK expects.

### Expressible Through Manifest/Type System

These concepts can be addressed through manifest metadata additions and type-system formalization, without grammar changes.

**Architecture Lifecycle.** The current five-state lifecycle (Draft, Reviewed, Approved, Executed, Verified) maps to the middle stages of BTABOK's six-stage ADLC (Planning through Utilize/Measure). Two additions are needed:

- An `ever-green` flag per spec in the manifest inventory, indicating whether the artifact requires continuous maintenance or is point-in-time.
- A `freshness-sla` field for ever-green specs, specifying the maximum interval between reviews.

Innovation and Strategy are pre-spec activities. Decommissioning is post-spec. These stages do not need spec-level representation.

**Decision cascades.** The manifest's dependency field already links specs. Formalizing cascade chains requires only that the dependency field distinguish "execution dependency" (must complete first) from "decision cascade" (triggered by this decision). A `dependency-type` qualifier handles this.

**Deliverable ownership.** Adding `owner` and `approver` fields to the manifest's spec inventory table gives every spec a named responsible party. This is the single most impactful metadata addition for BTABOK alignment.

**Repository ever-green.** The `ever-green` flag described under Architecture Lifecycle gives the repository model its freshness concept. Validation can warn when an ever-green artifact's last-reviewed date exceeds its freshness SLA.

**Deliverables.** Specs and manifests are already document deliverables. Projections are model deliverables. Non-document deliverables (collaboration, advisory, influence) are organizational activities, not spec artifacts.

### Requires New Spec Type

These concepts need spec types from the taxonomy, each with a validation profile defining required sections, allowed/forbidden blocks, and cross-document rules. Each new spec type corresponds to a CoDL concept type: the validation profile is expressed as a CoDL `concept` definition when the BTABOK profile is active.

**Decision Records (ADR) (CoDL: DecisionRecord).** The Decision Spec type already exists but needs a standardized validation profile: required fields for scope, type, options, recommendation, approver, status, and amendment links. The profile formalizes what is currently implicit.

**Requirements / ASR Spec (CoDL: ASRCard).** A new spec type isolating architecturally significant requirements with classification (functional, quality attribute, constraint), significance indicators, and links to the contracts and constraints that implement them.

**Governance Spec (CoDL: GovernanceRule, GovernanceBody).** A new spec type defining review authority, approval rules, compliance gates, and execution policy for a collection. Initially this can live mostly in the manifest layer as structured metadata.

**Waiver Spec (CoDL: WaiverRecord).** A new spec type recording an approved exception to a rule, principle, or policy. Required fields: scope, expiration, approver, linked rule, risk assessment.

**Roadmap / Transition Spec (CoDL: RoadmapItem, TransitionArchitecture).** A new spec type describing baseline, target, transitions, dependencies, and sequencing. Generalizes the phase and execution-order logic already present in manifests into a reusable transition artifact.

**Context / Stakeholder Spec (CoDL: StakeholderCard).** A new spec type describing stakeholders, their concerns, and system-boundary expectations. Uses existing context-register constructs (`person`, `external system`, `relationship`) plus new metadata for concerns and viewpoint alignment.

**Quality Attribute Spec (CoDL: QualityAttributeScenario).** A new spec type isolating quality-attribute scenarios: stimulus, response, threshold, verification obligation. Uses existing constraints and contracts as building blocks.

**Viewpoint Spec (CoDL: ViewpointCard).** A reusable viewpoint template (audience, concerns, required models, validation criteria), introduced only if demand emerges per Governing Principle 3.

### Requires DSL Extension

These concepts are formal, reusable, and semantically checkable across multiple documents. Rather than inventing new SpecLang grammar, the BTABOK profile expresses each as a **CoDL concept definition**. When the profile is active, CoDL syntax is available inside SpecLang documents (Option A, per [CoDL-CaDL-Integration-Notes.md](CoDL-CaDL-Integration-Notes.md)). Optional SpecLang-style short forms may be provided as ergonomic aliases that desugar to the same CoDL records, following The Standard extension's precedent.

**Decision scope and type.** Fields on the `DecisionRecord` CoDL concept using `enum(...)` types. These enable cross-document validation: does the decision's scope match the spec collection's scope? Do cascade references point to decisions at the correct scope level?

**ASR classification.** Enumerated significance categories (structural impact, quality attribute impact, technical capability impact) as fields on the `ASRCard` CoDL concept. These enable validation: does every ASR with structural impact have a corresponding trace to a component?

**Stakeholder concerns.** A `concerns` section on the `StakeholderCard` CoDL concept using `ref<QualityAttributeScenario>` or `ref<ConstraintCard>` with cardinality. This enables validation: does every declared concern have at least one view that addresses it?

**Quality attribute scenarios.** Structured fields on the `QualityAttributeScenario` CoDL concept (stimulus, environment, response, measure, threshold). These are architecturally significant and naturally machine-checkable through CoDL's constraint machinery.

**Viewpoint alignment.** A `viewpoint` field on structural spec types and view declarations, referencing a `ViewpointCard` CoDL concept. This enables validation: does every declared viewpoint have at least one conforming view?

Example: a minimal `DecisionRecord` expressed in CoDL syntax inside a BTABOK-profile spec.

```codl
concept DecisionRecord {
  meta { /* Standard BTABoK metadata profile */ }
  section Classification {
    scope: enum(ecosystem, enterprise, valueStream, solution, productService, module) required
    decisionType: enum(structural, productFramework, constraint, cascade, principle) required
  }
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
    implements<PrincipleCard> as principles cardinality(0..*)
    supersedes<self> as supersedes cardinality(0..1)
  }
}
```

These extensions follow The Standard's precedent: they activate only when the BTABOK profile is declared, they add to the language without replacing anything, and they produce CoDL records that round-trip with any BTABOK-aligned tool.

### Out of SpecChat's Scope

These Engagement Model concepts are not suitable for specification-language expression, even though they are legitimate architecture-practice concerns.

**Decision method.** The specific technique used to reach a decision (budgeted evaluation, bake-off, scoring, opinion) is a process concern, not a structural one. It can be described in the prose section of a Decision Spec but does not warrant formal syntax.

**Stakeholder influence mapping.** Power/interest grids and engagement strategies are organizational instruments. A person's influence posture changes over time and context in ways that do not belong in a system specification.

**Deliverable usage telemetry.** Tracking which artifacts are actually consumed is a repository-operations concern, not a spec-language concern.

**Tool integration specifics.** Connectors to Jira, CI systems, and architecture dashboards are implementation details of the SpecChat CLI and tooling ecosystem, not language constructs.

**Governance boards (ARB).** Named review bodies with membership, authority, and meeting cadence are organizational structures. The manifest can record that a spec was approved by an ARB (via the `approver` field), but the board's constitution is not a spec artifact.

**Services and Assignment.** How architecture services are defined and how architects are assigned to work are operating-model concerns at the organizational level.

**Experiments.** Controlled architectural experiments could potentially become a spec type, but there is no demand signal from the current corpus. If needed later, they follow the same adoption path: type profile first, DSL extension only if justified.

---

## The BTABOK Profile Mechanism

### Activation Model

The BTABOK profile follows the same activation pattern as The Standard extension, with the addition that activating the profile makes CoDL concept syntax and CaDL canvas syntax available inside SpecLang documents.

A BTABOK profile is activated by a declaration in the base system spec:

```spec
profile BTABOK {
    version: "0.2";
    enforce: [decisions, asrs, stakeholders, quality_scenarios, viewpoints];
}
```

The `version` field tracks the CoDL specification version the profile targets (0.2 at time of writing). When this declaration is present, CoDL `concept` definitions, the Standard BTABoK Metadata profile, CoDL reference types (`ref<>`, `weakRef`, `externalRef`), storage directives, and CaDL `canvas` definitions become available. When absent, the system operates on core SpecLang only.

The `enforce` list controls which rule sets are active. Omitting it activates all BTABOK rules. Listing specific identifiers allows incremental adoption. This matches The Standard's activation semantics.

A manifest can also declare the profile at the collection level:

```markdown
| Field | Value |
|---|---|
| System | OrderSystem |
| Base spec | OrderSystem.spec.md |
| Profile | BTABOK |
| Profile version | 0.2 (CoDL 0.2) |
```

The manifest-level declaration applies the profile to all specs in the collection. The spec-level declaration can override or extend with specific enforcement choices.

Per the Option A decision recorded in [CoDL-CaDL-Integration-Notes.md](CoDL-CaDL-Integration-Notes.md), CoDL syntax is the canonical authoring form. Optional SpecLang-style short forms (`decision`, `asr`, `stakeholder`, etc.) may be offered as ergonomic aliases that desugar to equivalent CoDL records.

### Metadata Additions

When the BTABOK profile is active, every concept record adopts the **CoDL Standard BTABoK Metadata profile (`BTABoKItem`)** directly. Prior parallel metadata fields proposed by the WIP corpus (owner, approver, tracking-block version, informal reviewer field) are superseded by the CoDL fields. The Standard Metadata profile contributes:

`slug`, `itemType`, `name`, `shortDescription`, `version`, `baseVersion`, `bokStatus` (Add/Update/Remove/Archive), `publishStatus` (Draft/Review/Approved/Published/Retired), `accessTier` (Free/Member/Paid/Restricted), `authors` (list<PersonRef>), `reviewers` (list<PersonRef>), `committer` (PersonRef), `tags`, `certainty` (score1to10), `createdAt`, `updatedAt`.

SpecChat adds one additive extension on top of CoDL's metadata to preserve the ever-green/freshness SLA concept from Governing Principle 4:

| SpecChat extension | Purpose | Type | CoDL interaction |
|---|---|---|---|
| `freshnessSla` | Maximum interval between reviews for `retentionPolicy: indefinite` records | `duration` | Additive field on records with `retentionPolicy: indefinite` |

Ever-green semantics are otherwise carried by CoDL's `retentionPolicy` enum (`indefinite` | `archive-on-deprecated` | `delete-on-superseded`). The full mapping of WIP metadata fields to CoDL Standard Metadata is recorded in [CoDL-CaDL-Integration-Notes.md](CoDL-CaDL-Integration-Notes.md) Section 5.

Concept-specific fields (decision scope and type, ASR significance, stakeholder concerns, viewpoint alignment) are expressed as typed sections on the corresponding CoDL concepts (`DecisionRecord`, `ASRCard`, `StakeholderCard`, `ViewpointCard`), not as parallel metadata. See the Requires DSL Extension subsection for the concept-level CoDL example.

### Validation Rules

The BTABOK profile adds these validation rules when active:

1. Every Decision Spec must declare `scope` and `decision type`.
2. Every cascade reference must point to an existing Decision Spec in the collection.
3. Every ASR with structural significance must have at least one trace to a component.
4. Every declared stakeholder concern must be addressed by at least one view or constraint.
5. Every ever-green artifact must have a last-reviewed date within its freshness SLA.
6. Every spec declaring a viewpoint must contain the sections and blocks appropriate to that viewpoint type.

### Projection Outputs

Each projection is a **CaDL canvas definition** over one or more CoDL concepts. CaDL is the canonical language for canvas projections in the BTABOK profile; the profile does not invent a parallel projection mechanism.

- **Decision Registry**: A CaDL canvas over `DecisionRecord`, ordered by scope and `publishStatus`.
- **ASR Matrix**: A CaDL canvas over `ASRCard` with related-component display.
- **Stakeholder Concern Map**: A CaDL canvas over `StakeholderCard` surfacing concerns and the views or constraints that address each concern.
- **Viewpoint Coverage Report**: A CaDL canvas over `ViewpointCard` with coverage and gap indicators.
- **Freshness Report**: A CaDL canvas over any CoDL concept with `retentionPolicy: indefinite`, surfacing `updatedAt` against `freshnessSla`.

Example: the Decision Registry projection expressed in CaDL.

```cadl
canvas DecisionRegistry for DecisionRecord {
  area "Header" {
    shows: slug
    shows: name
    shows: publishStatus
  }
  area "Classification" {
    shows: scope
    shows: decisionType
  }
  area "Traceability" {
    repeats: requirements
    shows: slug -> name
  }
  area "Approvers" {
    shows: committer -> name
  }
}
```

---

## Minimum Viable Implementation

### Pass 1: Formalize What Already Exists

No grammar changes. No new spec types. Only manifest and type-profile formalization.

| Action | What changes | Effort |
|---|---|---|
| Add `owner` and `approver` columns to manifest spec inventory | Manifest template | Low |
| Add `ever-green` and `freshness-sla` fields to manifest spec inventory | Manifest template | Low |
| Define validation profile for Decision Spec: required sections, required fields | Type Profile document | Low |
| Define validation profile for Base System Spec: required/allowed registers | Type Profile document | Low |
| Classify existing specs against BTABOK viewpoint types | Metadata annotation | Low |

This pass closes the biggest gap (ownership and approval authority) and formalizes what the current system already implies.

### Pass 2: Add Type Profiles and Manifest Metadata

Introduce new spec types with validation profiles. Still no grammar changes.

| Action | What changes | Effort |
|---|---|---|
| Define ASR/Quality Spec type profile | New type with required sections, allowed blocks, cross-document rules | Medium |
| Define Context/Stakeholder Spec type profile | New type using existing context-register constructs plus concern metadata | Medium |
| Define Governance Spec type profile | New type for review authority, approval rules, compliance gates | Medium |
| Define Waiver Spec type profile | New type for approved exceptions with scope, expiration, risk | Medium |
| Define Roadmap/Transition Spec type profile | New type for baseline, target, transitions, sequencing | Medium |
| Update manifest document type registry to recognize new types | Manifest template | Low |
| Define the BTABOK profile activation mechanism in the manifest | Manifest template + profile document | Medium |

This pass creates the artifact ecology. Each new type is a validation contract (required sections, required metadata, allowed/forbidden blocks, cross-document rules), not a grammar extension.

### Pass 3: Selective DSL Extensions After Proven Use

Only after real specs of each type have been written and validated through type profiles, promote concepts to formal syntax.

| Candidate | Trigger for promotion | Expected syntax |
|---|---|---|
| Decision scope and type | Multiple Decision Specs across different scope levels in the same collection | `scope: solution` and `type: structural` as typed fields in `decision` blocks |
| ASR classification | Multiple ASR Specs with significance categories used for trace validation | `significance: structural` as a typed field in `asr` blocks |
| Stakeholder concerns | Multiple Context/Stakeholder Specs where concern-to-view traceability is validated | `concern "Performance" { quality: latency; stakeholder: "API Consumer"; }` |
| Quality attribute scenarios | Multiple Quality Specs with structured stimulus/response/threshold patterns | `scenario "Peak Load" { stimulus: ...; response: ...; measure: ...; threshold: ...; }` |
| Viewpoint annotation | Consistent use of viewpoint classification across structural specs | `viewpoint: Physical` as a typed annotation on view or spec declarations |

The promotion criterion remains: **formal, reusable, and semantically checkable across multiple documents.** A concept stays in the type-profile layer until it meets that bar through actual use.

---

## 11. CoDL and CaDL Alignment

### 11.1 Authoritative Source

In April 2026 IASA Global published two authoritative specifications by Paul Preiss, founder of IASA, through the BTABoK 3.2 education portal:

- **CoDL (Concept Definition Language)**: the schema and type language for the stored data structure of any BTABoK concept. CoDL is concept-type-neutral: it defines the type system, storage directives, transport envelope, and Standard BTABoK Metadata profile, but it does not prescribe a fixed concept vocabulary.
- **CaDL (Canvas Definition Language)**: the visual and rendering language that projects CoDL concepts onto canvases. CaDL's governing principle is that a canvas is a view of a concept, not a separate stored object type.

Together, CoDL and CaDL are the authoritative BTABoK schema and canvas languages. The BTABOK Profile described earlier in this document is now formalized as a **CoDL implementation** with **CaDL canvas definitions** for the projection layer. Full source notes, the complete CoDL surface, and the detailed alignment analysis are recorded in [CoDL-CaDL-Integration-Notes.md](CoDL-CaDL-Integration-Notes.md).

### 11.2 Architectural Decision

The SpecChat BTABOK profile adopts **Option A with optional SpecLang-style aliases**: CoDL syntax is accepted verbatim as the canonical authoring form for BTABOK concept records. Optional SpecLang-style short forms (`decision`, `asr`, `stakeholder`, `waiver`, etc.) desugar to equivalent CoDL concept records, following the precedent set by The Standard extension. Canonical storage is CoDL; canonical display is CaDL.

Rationale, the Option A vs. Option B comparison, and worked examples appear in [CoDL-CaDL-Integration-Notes.md](CoDL-CaDL-Integration-Notes.md) Section 4.

### 11.3 WIP Corpus to CoDL/CaDL Mapping

| WIP corpus construct | CoDL/CaDL equivalent |
|---|---|
| Spec type (taxonomy entry) | CoDL `concept` definition with typed sections, relationships, and storage directives |
| Projection (Decision Registry, ASR Matrix, Stakeholder Concern Map, Viewpoint Coverage Report, Freshness Report) | CaDL `canvas` definition over the relevant CoDL concepts |
| Stable human-readable ID (ASR-01, ASD-01, WVR-01) | CoDL `slug` field in the Standard BTABoK Metadata profile |
| Owner and approver fields | CoDL `authors` (list<PersonRef>), `reviewers` (list<PersonRef>), `committer` (PersonRef) in the Standard Metadata profile |
| Ever-green flag | CoDL `retentionPolicy: indefinite` |
| Point-in-time artifact | CoDL `retentionPolicy: archive-on-deprecated` or `delete-on-superseded` |
| Freshness SLA | SpecChat additive extension: `freshnessSla: duration` on `indefinite` records |
| Cross-document reference | CoDL `ref<ConceptType>`, `weakRef`, `externalRef` |
| Lifecycle state (Draft/Reviewed/Approved/Executed/Verified) | CoDL `publishStatus` enum (Draft/Review/Approved/Published/Retired); Executed/Verified preserved as SpecChat-local refinements backed by verification records |
| Profile activation (BTABOK) | `profile BTABOK { version: "0.2"; ... }` where the version tracks the CoDL specification version |

### 11.4 Scope Discipline Under CoDL

CoDL is concept-type-neutral: any concept can in principle be defined in CoDL. The Engagement-Model-only scope that governs SpecChat's BTABOK integration must therefore be enforced at the **profile** level, not at the language level. The BTABOK profile defines concept types for Engagement Model concepts (ASRCard, DecisionRecord, StakeholderCard, ViewpointCard, CanvasDefinition, QualityAttributeScenario, WaiverRecord, RoadmapItem, TransitionArchitecture, GovernanceBody, GovernanceRule, RiskCard, PrincipleCard, CapabilityCard, StandardCard, LegacyModernizationRecord, ExperimentCard). Value Model and People Model concept types are out of scope for the SpecChat BTABOK profile; they may be introduced in separate profiles if demand emerges.

---

## Appendix A: Source References

### A.1 BTABOK Official Sources (Engagement Model Scope)

1. **BTABoK Main Page** -- https://iasa-global.github.io/btabok/
   Used for the Engagement Model structure: operating-model concept list, four-model organization.

2. **Architecture Lifecycle** -- https://iasa-global.github.io/btabok/architecture_lifecycle.html
   Used for the six-stage ADLC, lifecycle planning, innovation assessment, staffing.

3. **Decisions** -- https://iasa-global.github.io/btabok/decisions.html
   Used for decision scope classification, decision types, decision methods, cascades, ADR structure.

4. **Requirements** -- https://iasa-global.github.io/btabok/requirements.html
   Used for ASR definition, three requirement types, significance categories, ASR characteristics.

5. **Views** -- https://iasa-global.github.io/btabok/views.html
   Used for view/viewpoint definitions, seven viewpoint types, stakeholder-centric design.

6. **Views and Viewpoints** -- https://iasa-global.github.io/btabok/views_viewpoints.html
   Used for Kruchten 4+1, Views and Beyond, Rozanski-Woods, TOGAF domains.

7. **Deliverables** -- https://iasa-global.github.io/btabok/deliverables.html
   Used for seven deliverable output types, ownership management, basic and advanced engagement model deliverables.

8. **Repository** -- https://iasa-global.github.io/btabok/repository.html
   Used for repository principles, ever-green concept, meta-model elements, loading approaches.

9. **Governance** -- https://iasa-global.github.io/btabok/governance.html
   Used for governance definition, four focus areas, sub-capabilities, certification learning objectives.

10. **Governance (Engagement Model)** -- https://iasa-global.github.io/btabok/governance_em.html
    Used for "governed not governing" principle, governance anti-patterns, bottom-up governance, compliance checking, reference architecture application.

11. **Architecture Tools** -- https://iasa-global.github.io/btabok/architecture_tools.html
    Used for essential tool capabilities, tool selection process, meta-model support requirements.

12. **What is Architecture** -- https://iasa-global.github.io/btabok/what_is_architecture.html
    Used for the core definition of business technology architecture and the three complementary domains.

### A.2 SpecChat Corpus Sources

1. **SpecChat-Overview.md** -- Four-layer architecture, workflow, guided authoring, critical insight.
2. **SpecLang-Specification.md** -- Five registers, constructs, semantic model, CLI, implementation phases.
3. **SpecLang-Grammar.md** -- Lexer, tokens, EBNF grammar, parser assumptions.
4. **TheStandard-Extension-Overview.md** -- Profile activation precedent, layer contracts, realization directives.
5. **TheStandard-Extension-Specification.md** -- Extension semantics, validation rules, worked example.
6. **TheStandard-Extension-Grammar.md** -- Extension parser productions, keywords, ambiguity resolution.
7. **PizzaShop.spec.md**, **PayGate.spec.md**, **SendGate.spec.md** -- Sample specs demonstrating subsystem patterns.
8. **TodoApp.manifest.md**, **PizzaShop.manifest.md** -- Sample manifests demonstrating lifecycle and type registry.

### A.3 WIP Analysis Sources

1. **BTA-BOK-integration.md** -- Full BTABOK-to-SpecChat concept mapping and gap analysis.
2. **Why-We-Created-the-Spec-Type-Taxonomy.md** -- Stratification rationale.
3. **Spec-Type-Taxonomy-v0.1.md** -- 14 spec types across 4 families.
4. **Spec-Type-Validation-Analysis.md** -- Type Profile vs. DSL Extension distinction.
5. **CoDL-CaDL-Integration-Notes.md** -- Alignment between SpecChat's BTABOK profile and the authoritative CoDL/CaDL specifications, including the Option A decision, Standard BTABoK Metadata mapping, retention policy alignment, and canvas projection examples.
