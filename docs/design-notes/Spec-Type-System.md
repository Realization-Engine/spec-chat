# SpecLang Spec Type System

## Tracking

| Field | Value |
|---|---|
| slug | speclang-spec-type-system |
| itemType | DesignDocument |
| name | SpecLang Spec Type System |
| version | 0.1.0 |
| publishStatus | Draft |
| authors | dennis@dennislandi.com |
| reviewers | (pending) |
| committer | dennis@dennislandi.com |
| tags | speclang, spec-types, taxonomy, validation, codl, cadl, btabok |
| createdAt | 2026-04-17 |
| updatedAt | 2026-04-17 |
| retentionPolicy | Indefinite |
| freshnessSla | 90 days |
| lastReviewed | 2026-04-17 |
| dependencies | SpecLang-Design.md, Global-Corp-Exemplar.md |

## 1. Purpose

This is the single authoritative design document for the SpecChat spec type system. It covers three things as one coherent topic: the rationale for having a spec type taxonomy at all, the taxonomy itself (families, canonical types, CoDL concept alignment), and the validation architecture that makes each type deterministically checkable. It supersedes the three predecessor WIP notes (`Why-We-Created-the-Spec-Type-Taxonomy.md`, `Spec-Type-Taxonomy-v0.1.md`, and `Spec-Type-Validation-Analysis.md`, now in `WIP/Archive/`) and subsumes their content.

## 2. Rationale: Why a Spec Type Taxonomy Exists

### 2.1 The design problem

SpecChat already has a clear center of gravity: the specification is the primary engineering artifact, the core language is intentionally lean, complexity is opt-in, manifests govern collections of specs, multiple document types already exist in practice, and the system already supports domain-specific extension through variants such as The Standard.

The question was never, "Should BTABOK ideas be added?" The real question was: where should BTABOK concepts live so that SpecChat becomes more capable without becoming bloated?

### 2.2 Why not merge BTABOK wholesale into core SpecLang

BTABOK is broader than a syntax for architecture description. It covers architectural practice as much as architectural structure: stakeholder-centered views and viewpoints, architecturally significant requirements and decisions, repositories organized for architects first, lightweight architecture deliverables used for thinking and communication, stakeholder mapping, business cases, principles, risk registers, benefits dependency models, waivers, and roadmaps.

Those concerns are important, but many of them are not the same kind of thing as the current SpecLang core constructs. The core language is strongest when it describes things that are structural, reusable across many systems, semantically checkable, and suitable for machine interpretation and realization. That is why the existing five-register core (data, context, systems, deployment, view/dynamic/design) should remain lean.

Merging BTABOK wholesale into the top-level grammar would produce a language that mixes structural declarations, governance concepts, business abstractions, review artifacts, and lifecycle documents at the same level. That would make the language harder to reason about, harder to parse cleanly, and easier to bloat over time.

### 2.3 The chosen move: stratification

Instead of flattening everything into the core, a three-layer model was adopted. Core SpecLang remains the executable substrate. BTABOK becomes an optional profile or extension layer above the core, carrying practice-oriented concepts (`asr`, `decision`, `principle`, `stakeholder`, `concern`, `viewpoint`, `risk`, `waiver`, `capability`, `benefit`, `owner`, `approver`). The Spec Type Taxonomy is the third layer: rather than treating every concern as a new grammar keyword, the manifest and the document model determine what kinds of specs exist for which scopes.

Many BTABOK-aligned concerns belong more naturally in spec types and metadata blocks than in the universal grammar.

### 2.4 The core principle

The taxonomy rests on one rule:

- Put something in the **core DSL** if it is structural, reusable across most software systems, and semantically checkable.
- Put something in a **profile** (The Standard, BTABOK) if it expresses architectural practice, governance, value, or stakeholder concerns.
- Put something in a **spec type** if it is primarily an artifact of scope, audience, or lifecycle stage.

This rule prevents category confusion. It keeps the grammar from becoming a dumping ground for architecture concepts while allowing the overall system to grow richer.

### 2.5 What stratification protects

Three things at once. It protects the core language: the core DSL remains coherent, disciplined, and technically meaningful. It protects extensibility: new architectural profiles, including a BTABOK-aligned one, can be added without destabilizing the base language. It protects scope clarity: different documents can exist because they serve different purposes, audiences, and lifecycle roles, not because the grammar became chaotic.

The governing position: SpecLang is not being turned into BTABOK. SpecLang is the executable substrate, and BTABOK acts as an architectural profile that shapes which specs exist and what metadata those specs must carry. Until the taxonomy is stable, grammar expansion is premature. Once the taxonomy is clear, the necessary grammar changes become much easier to identify and justify.

## 3. The Three Layers

### 3.1 Core SpecLang (the executable substrate)

Core SpecLang carries the parts of the system that are fundamentally structural and machine-checkable. It stays small, disciplined, and reusable across domains. Following the Option X decision, Core SpecLang also owns the infrastructure that every profile relies on: standard SpecItem metadata, reference types (`ref<T>`, `weakRef`, `externalRef`), relationship declarations with cardinality, retention policy, diagnostic record extensions, slug rules, and the ten core validators. See Section 7 and `SpecLang-Design.md` for details.

### 3.2 Profiles (The Standard, BTABOK)

A profile is an optional discipline layered over the core. It adds profile-specific metadata, concept types, constraints, and validators. The Standard specializes component vocabulary and realization rules. The BTABOK profile adds BTABoKItem metadata and practice-oriented concept types (DecisionRecord, ASRCard, StakeholderCard, ViewpointCard, WaiverRecord, and the rest of the BTABOK Engagement Model concepts). Under Option X, profiles no longer carry universal infrastructure; that belongs to the core. See `SpecLang-Design.md` for the BTABOK profile design and the Engagement Model mapping.

### 3.3 Spec Types (scope-specific artifacts)

A spec type is a first-class classification of what kind of artifact a document is. Each spec type corresponds to a CoDL concept type; these are two vantages on the same artifact. The spec type names it from the SpecChat authoring perspective; the CoDL concept names the stored schema from the BTABoK perspective. There is no second artifact.

### 3.4 Composition

Every spec is simultaneously: (a) a spec type, (b) expressed in core SpecLang (with its universal infrastructure), and (c) optionally shaped by one profile. Under Option X, standard metadata, references, relationships, retention, and core validators are present regardless of profile. BTABoKItem fields, CoDL concept extensions, and BTABOK validators activate only when the BTABOK profile is declared.

## 4. The Spec Type Taxonomy

### 4.1 Governing idea

A spec type is not a filename label. It is a first-class classification with five dimensions.

- **Scope.** What boundary does this spec describe?
- **Intent.** Is it defining, evolving, resolving conflict, governing, or transitioning?
- **Binding strength.** Descriptive, prescriptive, corrective, authorizing, or gate-controlling?
- **Profile.** Core only, or Core plus The Standard, or Core plus BTABOK?
- **Lifecycle role.** Does it originate a system, refine it, authorize it, or verify it?

The manifest carries the taxonomy while the grammar stays lean.

### 4.2 Taxonomy families

Four top-level families.

- **Foundational specs.** Establish the existence, identity, and governing rules of a scope: Manifest, Base System Spec, Architecture Profile Spec, and future Scope Manifest.
- **Structural specs.** Describe a system or a bounded part of one: Subsystem Spec, Context/Stakeholder Spec, ASR/Quality Spec. (Deployment/Runtime Spec and Viewpoint Spec are cataloged as candidates for v0.2 promotion and are not part of the v0.1 canonical taxonomy.)
- **Evolution specs.** Change, extend, repair, or clarify structure: Feature Spec, Bug Spec, Amendment Spec, Decision Spec.
- **Governance and transition specs.** Control approval, exceptions, sequencing, and verification: Governance Spec, Waiver Spec, Roadmap/Transition Spec, Verification Spec.

### 4.3 Canonical spec types

#### 4.3.1 Manifest
- **Role.** Root control document for a collection.
- **CoDL concept.** `CollectionManifest` (SpecChat-specific; modeled in CoDL for uniformity; combines Core SpecLang metadata with any active profile extensions such as BTABoKItem fields).
- **Scope.** One spec collection.
- **Intent.** Govern.
- **Binding strength.** Gate-controlling.
- **Must contain.** System identity, base spec reference, lifecycle states, tracking block convention, document type registry, spec inventory, execution order, conventions.

#### 4.3.2 Base System Spec
- **Role.** Canonical system skeleton.
- **CoDL concept.** `SystemSpec` (SpecChat-specific).
- **Scope.** One bounded system.
- **Intent.** Define.
- **Binding strength.** Prescriptive.
- **Must contain.** Context, system declaration, topology, constraints, contracts, core data model.
- **May contain.** Phases, traces, deployment, views and dynamics, design and pages, package policy, platform realization.

#### 4.3.3 Architecture Profile Spec
- **Role.** Define an architectural dialect or discipline that constrains how base and evolution specs are written. Natural home for The Standard, a future BTABOK Profile, and future domain profiles.
- **CoDL concept.** `ProfileDefinition` (SpecChat-specific).
- **Scope.** Reusable across one or more collections.
- **Intent.** Specialize.
- **Binding strength.** Prescriptive.

#### 4.3.4 Subsystem Spec
- **Role.** Describe a bounded supporting system with its own context, authored and consumed components, data, contracts, and optional deployment; can execute independently of the parent base spec. PayGate and SendGate exemplify this category.
- **CoDL concept.** `SubsystemSpec` (SpecChat-specific).
- **Scope.** Child or peer system.
- **Intent.** Define.
- **Binding strength.** Prescriptive.

#### 4.3.5 Feature Spec
- **Role.** Add capability.
- **CoDL concept.** `FeatureSpec` (SpecChat-specific).
- **Scope.** Targeted evolution.
- **Intent.** Extend.
- **Binding strength.** Prescriptive.
- **Standard shape.** Purpose, component additions with contracts, data models, page integration, test obligations, concrete example.

#### 4.3.6 Bug Spec
- **Role.** Correct source behavior when the spec is already right.
- **CoDL concept.** `BugSpec` (SpecChat-specific).
- **Scope.** Targeted defect.
- **Intent.** Repair.
- **Binding strength.** Corrective.
- **Standard shape.** Current behavior, specified behavior, root cause analysis, acceptance criteria, implementation guidance.

#### 4.3.7 Amendment Spec
- **Role.** Correct the spec itself without adding new capability.
- **CoDL concept.** `AmendmentSpec` (SpecChat-specific).
- **Scope.** Targeted correction.
- **Intent.** Normalize.
- **Binding strength.** Corrective.
- **Standard shape.** Count corrections, dependency accounting, structural adjustments.

#### 4.3.8 Decision Spec
- **Role.** Resolve ambiguity, tension, or conflict. Natural home for BTABOK-style architecture decisioning.
- **CoDL concept.** `DecisionRecord` (BTABoK standard).
- **Scope.** Targeted architectural question.
- **Intent.** Decide.
- **Binding strength.** Authorizing.
- **Standard shape.** Options, trade-offs, recommendation, amendments to base spec, effectivity once accepted and executed.

#### 4.3.9 Context / Stakeholder Spec
- **Role.** Describe stakeholders, concerns, and system-boundary expectations at a scope above or beside the technical base spec. Aligns with existing SpecLang context constructs (persons, external systems, relationships, tags).
- **CoDL concept.** `StakeholderCard` (BTABoK standard).
- **Scope.** Business, program, enterprise, or large-solution context.
- **Intent.** Frame.
- **Binding strength.** Prescriptive where referenced.

#### 4.3.10 ASR / Quality Spec
- **Role.** Isolate architecturally significant requirements and measurable quality constraints. Maps to existing constructs: contracts, constraints, topology prohibitions, phase gates, traces.
- **CoDL concept.** `ASRCard` (BTABoK standard).
- **Scope.** Cross-cutting.
- **Intent.** Constrain.
- **Binding strength.** Prescriptive.

#### 4.3.11 Governance Spec
- **Role.** Define review authority, approval rules, compliance gates, execution policy. Initially lives mostly in the manifest layer.
- **CoDL concept.** `GovernanceBody` or `GovernanceRule` (BTABoK standard; specialized by instance).
- **Scope.** Collection or portfolio.
- **Intent.** Govern.
- **Binding strength.** Gate-controlling.

#### 4.3.12 Waiver Spec
- **Role.** Record an approved exception to a rule, principle, or policy. A waiver does not mean the rule changes; it means a bounded exception has been granted.
- **CoDL concept.** `WaiverRecord` (BTABoK standard).
- **Scope.** Narrow and time-bounded.
- **Intent.** Exempt.
- **Binding strength.** Authorizing, temporary.

#### 4.3.13 Roadmap / Transition Spec
- **Role.** Describe baseline, target, transitions, dependencies, and sequencing. Generalizes the tiering and execution-order logic already in manifests into a reusable transition artifact.
- **CoDL concept.** `RoadmapItem` or `TransitionArchitecture` (BTABoK standard).
- **Scope.** Time-bound transformation.
- **Intent.** Transition.
- **Binding strength.** Planning and gating.

#### 4.3.14 Verification Spec
- **Role.** Define how correctness is independently confirmed after execution. Formalizes what is currently implied by the Verified lifecycle state.
- **CoDL concept.** `VerificationRecord` (SpecChat-specific).
- **Scope.** Any other spec type.
- **Intent.** Verify.
- **Binding strength.** Gate-closing.

### 4.4 Taxonomy matrix

| Dimension | Types |
|---|---|
| Collection-governing | Manifest (`CollectionManifest`); Governance Spec (`GovernanceBody` / `GovernanceRule`) |
| Scope-defining | Base System Spec (`SystemSpec`); Subsystem Spec (`SubsystemSpec`); Context/Stakeholder Spec (`StakeholderCard`); ASR/Quality Spec (`ASRCard`) |
| Change-driving | Feature Spec (`FeatureSpec`); Bug Spec (`BugSpec`); Amendment Spec (`AmendmentSpec`); Decision Spec (`DecisionRecord`); Waiver Spec (`WaiverRecord`) |
| Time-driving | Roadmap/Transition Spec (`RoadmapItem` / `TransitionArchitecture`); Verification Spec (`VerificationRecord`) |
| Profile-defining | Architecture Profile Spec (`ProfileDefinition`) |

### 4.5 Spec type vs CoDL concept type

A **spec type** answers: what kind of artifact is this in SpecChat? A **CoDL concept type** answers: what is the stored schema of this concept in BTABoK?

A SpecChat spec type IS a CoDL concept. A Decision Spec in SpecChat is a `DecisionRecord` in CoDL. A Base System Spec is a `SystemSpec`. The two terms describe the same artifact from two vantages. Under the Option A decision (see `SpecLang-Design.md`), CoDL syntax is canonical; SpecLang-style aliases are permitted and desugar to CoDL.

Some CoDL metadata fields belong to Core SpecLang as universal infrastructure; others are profile extensions. Core SpecLang provides the base SpecItem metadata that every spec carries regardless of profile. The BTABOK profile adds BTABoKItem fields such as `accessTier`, `bokStatus`, `certainty`, and the BTABoK identifier set. Every concept definition in every profile combines Core SpecLang metadata with whatever profile-specific metadata the active profile declares.

### 4.6 Spec type vs profile

A **spec type** answers: what kind of artifact is this? A **profile** answers: what architectural discipline or vocabulary shapes this artifact?

These must remain separate. Examples of valid combinations: Base System Spec + Core; Base System Spec + The Standard; Decision Spec + BTABOK; ASR Spec + BTABOK; Subsystem Spec + Core. A profile shapes which spec types and which CoDL extensions are in scope for a given collection.

## 5. Type Validation Architecture

### 5.1 The decisive rule: Type Profile vs DSL Extension

The proposition that each spec type should be validated deterministically is directionally correct, with one refinement: each spec type should have its own deterministic validation contract, but not necessarily its own full grammar extension.

The rule:

> Every spec type should have a type-specific validation profile. Some spec types should also have a DSL extension. Those are not the same thing.

If every spec type became its own grammar, the taxonomy would proliferate dialects and weaken the strongest property of SpecChat: one primary executable substrate that source generation and validation rely on. Keep one shared core language; add type-specific validation on top.

### 5.2 Deterministic validation (what it means)

For each spec type, deterministic validation answers five questions.

1. **Document identity.** Is the document declared as the correct spec type in the manifest, with required tracking metadata and lifecycle state?
2. **Section schema.** Does the document contain the required headings and sections, in the required order?
3. **Formal block contract.** Which `spec` blocks are required, allowed, optional, or forbidden?
4. **Semantic rules.** Do the formal declarations resolve correctly and do the type-specific rules hold?
5. **Cross-document rules.** Does the document correctly reference base specs, dependencies, amendments, waivers, and manifest inventory entries?

### 5.3 Type Profile Contracts

A Type Profile Contract has these fields:

- **Required sections.**
- **Required metadata.** Combines Core SpecLang SpecItem metadata (slug, itemType, name, version, publishStatus, authors, reviewers, committer, tags, createdAt, updatedAt, retentionPolicy, freshnessSla, lastReviewed, dependencies) with any profile-specific extensions, such as BTABoKItem fields under the BTABOK profile.
- **Allowed core SpecLang declarations** (data/context/systems/deployment/view registers).
- **Required core SpecLang declarations** (same registers).
- **CoDL concept definition** (if the type maps to a BTABoK-aligned concept).
- **Type-specific semantic checks.**
- **Cross-document checks.** Use Core SpecLang reference resolution through `ref<T>`, `weakRef`, and `externalRef`; resolution machinery is provided by the core.
- **Deterministic pass/fail criteria.**

Most of this infrastructure (required metadata, reference validation) is handled by Core SpecLang. Only concept-specific validation (field constraints, cardinality on profile-specific relationships, concept-specific semantic rules) needs specification per type.

### 5.4 When a DSL Extension is justified (post Option X)

A spec type earns a DSL extension only when it introduces reusable formal semantics that deserve dedicated AST nodes and semantic analysis. The precedent is The Standard: broker-oriented components and realization rules are real language-surface changes, not templates.

Under CoDL alignment, a "DSL extension" for a BTABoK-aligned concept is best expressed as a CoDL concept definition, not a bespoke SpecLang grammar addition. The question becomes "is this concept load-bearing enough to warrant a formal CoDL definition in the BTABOK profile?" The criterion (formal, reusable, semantically checkable across multiple documents) is unchanged.

Under Option X, several validation concerns have moved from the DSL Extension candidates list into Core SpecLang and are now universal (slug uniqueness and format, reference resolution for `ref<T>`/`weakRef`/`externalRef`, metadata completeness against the Core SpecItem profile, relationship cardinality, freshness SLA). The remaining DSL Extension candidates are BTABOK-profile specific: decision scope/type/method, ASR classification, stakeholder concerns and coverage, quality attribute scenarios, viewpoint alignment, waiver rules and expiration, governance approval, roadmap capability moves, canvas target validation, and metric baseline and target.

### 5.5 Strong candidates for DSL extensions

Each of the five candidates below builds on Core SpecLang infrastructure. Metadata, references, and relationships come from the core; only the concept-specific fields are BTABOK-profile extensions.

#### 5.5.1 Decision Spec (`DecisionRecord`)
Concept fields: option, recommendation, consequence, supersedes, amendment link. Stable, repeatable semantics; maps well to deterministic validation.

#### 5.5.2 ASR / Quality Spec (`ASRCard`)
Concept fields: quality attribute, measurable scenario, stimulus, response, threshold, verification obligation. Architecturally significant and naturally machine-checkable.

Example in CoDL syntax (target form for any BTABOK-aligned concept under the profile):

```
concept ASRCard {
  meta { slug: slug required; name: shortText required }
  section QualityAttribute { attribute: shortText required }
  section Scenario {
    stimulus: text required
    response: text required
    threshold: measurement required
  }
  relationships {
    implements<Principle> as principles cardinality(0..*)
  }
}
```

#### 5.5.3 Context / Stakeholder Spec (`StakeholderCard`)
Concept fields: stakeholder, concern, viewpoint, owner, approver. Central to BTABOK-style architectural practice.

#### 5.5.4 Governance / Waiver Spec (`GovernanceBody`, `WaiverRecord`)
Concept fields: rule, waiver, scope, approver, expiration. Formal governance elements, not just prose headings.

#### 5.5.5 Roadmap / Transition Spec (`RoadmapItem`, `TransitionArchitecture`)
Concept fields: baseline, target, transition, milestone, dependency. Structurally rich; suitable for deterministic validation if used repeatedly.

### 5.6 Candidates that stay profile-driven

These can be validated deterministically through template rules, section schemas, and embedded core SpecLang blocks without requiring large grammar changes:

- Manifest
- Base System Spec
- Subsystem Spec
- Feature Spec
- Bug Spec
- Amendment Spec
- Verification Spec

Determinism for these types comes from required sections, tracking blocks and lifecycle rules, allowed and required formal blocks, and cross-document workflow relationships. That is already how the current system behaves.

## 6. The Four Validation Layers

### 6.1 Layer 1: Document-Schema Validation

Validate headings, tracking block, tables, required sections, and manifest registration.

### 6.2 Layer 2: Core SpecLang Validation

Parse all embedded formal blocks with the shared core grammar and AST. Under Option X, this layer also runs the ten core validators absorbed from the BTABOK profile:

- `check_metadata_completeness`
- `check_slug_uniqueness`
- `check_slug_format`
- `check_reference_resolution`
- `check_weakref_resolution`
- `check_externalref_validity`
- `check_freshness_sla`
- `check_profile_composition`
- `check_relationship_cardinality`
- `check_supersedes_cycles`

Layer 2 does much more than parse. It enforces core metadata completeness, slug format and uniqueness, reference resolution across `ref<T>`, `weakRef`, and `externalRef`, relationship cardinality, freshness SLAs, profile composition, and supersedes-cycle detection. These checks run on every collection regardless of profile. This preserves a single executable substrate.

### 6.3 Layer 3: Type-Profile Validation

Apply spec-type rules: required sections, required block kinds, forbidden declarations, dependency references, required examples, required acceptance criteria, required verification clauses.

Under the BTABOK profile, Layer 3 handles 13 BTABOK-specific validators grouped by concept family:

- **ASR validators (2)**: `check_asr_traceability`, `check_asr_addressed_by_decision`.
- **Decision validators (2)**: `check_decision_scope_type`, `check_decision_cascades`.
- **Principle validator (1)**: `check_principle_links`.
- **Stakeholder validator (1)**: `check_stakeholder_coverage`.
- **Viewpoint validator (1)**: `check_viewpoint_coverage`.
- **Waiver validators (2)**: `check_waiver_expiration`, `check_waiver_rule_reference`.
- **Governance validator (1)**: `check_governance_approval`.
- **Roadmap validator (1)**: `check_roadmap_capability_moves`.
- **Canvas validator (1)**: `check_canvas_target_exists`.
- **Metric validator (1)**: `check_metric_baseline_target`.

Layer 3 runs only when the relevant profile is active. Rules are expressed as CoDL concept constraints: required and optional fields, cardinality declarations, and reference resolution through core-provided `ref<>`, `weakRef`, and `externalRef`.

### 6.4 Layer 4: Extension Validation

Layer 4 focuses on CaDL canvas completeness and other projection-specific checks. Canvas validation is profile-specific: under the BTABOK profile this layer checks that each CaDL canvas renders its underlying CoDL concept completely and that every canvas target exists. Other profiles can introduce their own projection checks here. The same general pattern is already demonstrated by The Standard.

## 7. Core SpecLang Infrastructure Used by Every Type

Every spec, regardless of type and regardless of profile, carries the following Core SpecLang infrastructure. Full details live in `SpecLang-Design.md`; this is a pointer summary.

- **Standard SpecItem metadata.** slug, itemType, name, version, publishStatus, authors, reviewers, committer, tags, createdAt, updatedAt, retentionPolicy, freshnessSla, lastReviewed, dependencies.
- **Reference types.** `ref<T>` (resolved strong reference), `weakRef` (best-effort), `externalRef` (URI-based cross-repo).
- **Relationship declarations with cardinality.** Declared on concept definitions; cardinality enforced by Layer 2.
- **Retention policy.** Enum covering Indefinite, Timed, Superseded-on-replacement, and related modes.
- **Diagnostic record extensions.** `code`, `validator`, and `suggestion` fields on every diagnostic.
- **Ten core validators.** Listed in Section 6.2.

## 8. Adoption Passes

### 8.1 Pass 1: Taxonomy only

Update the manifest model to recognize the canonical spec types without any grammar change. Manifests already carry a document type registry and an inventory, so this is immediate. Canonical types to register: Manifest, Base, Subsystem, Feature, Bug, Amendment, Decision, ArchitectureProfile, Context, ASR, Governance, Waiver, Roadmap, Verification.

### 8.2 Pass 2: Formal profile mechanism

Formalize `Profile` as manifest metadata with values Core, TheStandard, BTABOK, and future custom profiles. This keeps BTABOK out of the core grammar until repeated use shows which constructs deserve first-class syntax. Adopting the BTABOK profile means adopting CoDL 0.2 as canonical concept syntax and CaDL as canvas syntax for BTABOK-specific concept types. CoDL adoption sits on top of the Core SpecLang infrastructure (standard metadata, reference types, relationship declarations, retention policy, diagnostic extensions) that is always present regardless of profile.

### 8.3 Pass 3: Selective grammar expansion (post Option X)

Only after real use, add BTABOK-native constructs that prove load-bearing. Likely order: `viewpoint`, `asr`, `principle`, `risk`, `waiver`. These are BTABOK-specific concept types. The core language already handles slug, metadata, references, and relationships after Option X, so the grammar work in this pass is limited to profile-specific concept vocabulary rather than infrastructure. These are ergonomic SpecLang aliases that desugar to CoDL concept records; they are not separate from CoDL but short forms that compile to the corresponding CoDL concept type (`ViewpointCard`, `ASRCard`, `PrincipleCard`, `RiskCard`, `WaiverRecord`). Field-level constructs like stakeholder `concern` remain part of the encompassing concept (`StakeholderCard`) rather than first-class spec-type keywords.

Do not begin by adding the entire BTABOK vocabulary to the core grammar.

## 9. Rule Set

1. Every collection has exactly one Manifest.
2. Every bounded implementation scope has exactly one Base System Spec.
3. Every non-core architectural discipline is represented as a Profile, not as a new base taxonomy family.
4. Subsystem Specs are peer structural specs, not glorified feature specs.
5. Decision, Amendment, Bug, and Feature remain the core evolution quartet.
6. Context, ASR, Governance, Waiver, Roadmap, and Verification are the first BTABOK-aligned expansion set.
7. New grammar keywords are added only after repeated use shows they are more than metadata or document-structure concerns.

## 10. Recommended v0.1 Taxonomy

Canonical list for v0.1, each annotated with its CoDL concept:

- Manifest (`CollectionManifest`)
- Base System Spec (`SystemSpec`)
- Subsystem Spec (`SubsystemSpec`)
- Architecture Profile Spec (`ProfileDefinition`)
- Context / Stakeholder Spec (`StakeholderCard`)
- ASR / Quality Spec (`ASRCard`)
- Feature Spec (`FeatureSpec`)
- Bug Spec (`BugSpec`)
- Amendment Spec (`AmendmentSpec`)
- Decision Spec (`DecisionRecord`)
- Governance Spec (`GovernanceBody` or `GovernanceRule`)
- Waiver Spec (`WaiverRecord`)
- Roadmap / Transition Spec (`RoadmapItem` or `TransitionArchitecture`)
- Verification Spec (`VerificationRecord`)

A strong backbone for expansion without overcommitting the grammar.

## 11. Open Items and Next Steps

- **Manifest Type Registry v2.** Define the registry plus the required metadata fields for each spec type. This makes the taxonomy operational before any grammar change.
- **Per-type Type Profile Contracts.** For each of the 14 canonical types, author the contract as specified in Section 5.3. Concept-specific semantic rules are the only new work per type; core infrastructure is already covered.
- **CoDL concept definitions for the five strong DSL candidates.** Author `DecisionRecord`, `ASRCard`, `StakeholderCard`, `GovernanceBody`/`WaiverRecord`, and `RoadmapItem`/`TransitionArchitecture` as CoDL concepts under the BTABOK profile. See `SpecLang-Design.md` for the profile-level design.
- **Viewpoint Spec and Deployment/Runtime Spec.** Both are listed in the structural family (Section 4.2) but omitted from the v0.1 canonical list in Section 10. Decide whether to promote them in v0.1 or defer until Pass 3.
- **Scope Manifest.** Placeholder in the foundational family for larger collections. Unclear whether needed; leave as v0.2 candidate.
- **Cross-document workflow validation.** The cross-document rules in Section 5.2 lean on manifest inventory and lifecycle state. Decide whether these checks belong in Layer 1 (document-schema) or Layer 3 (type-profile) when the reference crosses into workflow rather than formal `ref<T>`.
- **CaDL canvas catalog for v0.1.** For each type with a matching BTABoK concept, enumerate the expected canvases and the Layer 4 completeness checks.

## Appendix A. Source References

### A.1 SpecChat corpus

1. **SpecChat-Overview.md.** Layered model of SpecChat; the manifest's governing role; the core incremental types (feature, bug, decision, amendment).
2. **SpecLang-Specification.md.** The specification is the primary artifact; complexity is opt-in; the five registers are peers.
3. **SpecLang-Grammar.md.** Current grammar surface; caution against premature keyword expansion; distinction between grammar, AST, parsing, and semantic analysis.
4. **TodoApp.manifest.md.** Sample manifest: lifecycle states, tracking convention, document type registry.
5. **PizzaShop.manifest.md.** Sample manifest with subsystem specs in the registry and execution ordering by tier.
6. **todo-app-the-standard.manifest.md.** Precedent for profile-driven variation.
7. **todo-app-the-standard.spec.md.** Precedent that a real profile or extension can alter the language surface and semantics beyond plain templating.
8. **PayGate.spec.md.** Independently describable and executable subsystem spec.
9. **SendGate.spec.md.** Companion evidence of the same subsystem pattern.
10. **PizzaShop.spec.md.** Subsystem specs referenced as external systems by a parent base spec.

### A.2 External BTABOK and IASA sources

1. **IASA BTABOK: Views and Viewpoints.** https://iasa-global.github.io/btabok/views.html
2. **IASA BTABOK: Views & Viewpoints.** https://iasa-global.github.io/btabok/views_viewpoints.html
3. **IASA BTABOK: Requirements.** https://iasa-global.github.io/btabok/requirements.html
4. **IASA BTABOK: Decisions.** https://iasa-global.github.io/btabok/decisions.html
5. **IASA Education Portal: BTABoK Architecture Overview.** https://education.iasaglobal.org/browse/btabok/3.2/core-site/core/canvas/architecture-overview
6. **Preiss, Paul. Structured Concept Definition Language. BTABoK 3.2, IASA Global Education Portal (2026).** Authoritative source for CoDL and CaDL, concept type catalog, the Standard BTABoK Metadata profile, and the canvas-as-view-of-concept principle.

### A.3 Sibling WIP and consolidation documents

1. **SpecLang-Design.md.** Sibling consolidation of Core SpecLang, the BTABOK profile, Core-SpecLang absorption (Option X), CoDL/CaDL integration (Option A), and the BTABOK Engagement Model mapping.
2. **Global-Corp-Exemplar.md.** Sibling consolidation covering the Global Corp Enterprise Architecture worked example.

### A.4 Superseded source notes

This document supersedes and consolidates the following, now preserved in `WIP/Archive/`:

- `Why-We-Created-the-Spec-Type-Taxonomy.md` (content now in Section 2).
- `Spec-Type-Taxonomy-v0.1.md` (content now in Sections 4, 8, 9, 10).
- `Spec-Type-Validation-Analysis.md` (content now in Sections 5, 6).
