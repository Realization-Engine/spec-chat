# Spec Type Taxonomy v0.1

## Purpose

This document defines a first-pass taxonomy for SpecChat specification types. It is designed to preserve the strengths already visible in the current SpecChat corpus:

- the spec is the primary artifact
- the manifest governs the collection
- the system already supports base, feature, bug, decision, and amendment specs
- the system already tolerates profile-driven variation, as shown by The Standard
- the system already supports independently executable subsystem specs such as PayGate and SendGate

The goal is to expand SpecChat's specification ecology without bloating the core grammar. The taxonomy should separate:

- **what kind of artifact a document is**
- **what scope it applies to**
- **what kind of authority it carries**
- **what architectural profile shapes it**

This is especially important if SpecChat later incorporates a BTABOK profile. BTABOK should influence spec families, profiles, metadata, and required viewpoints more than it should immediately inflate the core DSL keyword set.

## 0. CoDL and CaDL Alignment Note

As of April 2026, IASA Global publishes CoDL (Concept Definition Language) and CaDL (Canvas Definition Language) as the authoritative BTABoK schema and canvas languages on the BTABoK 3.2 education portal. CoDL defines the stored data structure of any BTABoK concept. CaDL defines how a concept is rendered as a visual canvas. The governing principle is that a canvas is a view of a concept, not a separate stored object type.

Each spec type in this taxonomy corresponds to a CoDL concept type. Where a type aligns with a published BTABoK concept, the CoDL concept name is the BTABoK standard name (for example, DecisionRecord, ASRCard, StakeholderCard, ViewpointCard, WaiverRecord). Where a type is SpecChat-specific, the CoDL concept name is defined using CoDL syntax inside the SpecChat BTABOK profile (for example, SystemSpec, FeatureSpec, BugSpec, AmendmentSpec).

Projections from spec types (registries, matrices, coverage reports) are defined using CaDL canvas declarations over the relevant CoDL concepts. The spec type label and the CoDL concept name are two vantages on the same artifact: the spec type names it from the SpecChat authoring perspective; the CoDL concept name names it from the BTABoK stored-schema perspective.

For the full alignment, including the Option A decision (adopt CoDL syntax as canonical, allow SpecLang-style aliases), see CoDL-CaDL-Integration-Notes.md.

As of the Option X decision, specific CoDL infrastructure elements are Core SpecLang features, not BTABOK-profile features. The standard metadata profile (slug, itemType, name, version, publishStatus, authors, reviewers, committer, tags, createdAt, updatedAt, retentionPolicy, freshnessSla, lastReviewed, dependencies), reference types (ref<T>, weakRef, externalRef), relationship declarations with cardinality, the retention policy enum, and diagnostic record extensions (code, validator, suggestion) are all part of Core SpecLang. Every spec carries this infrastructure regardless of profile. BTABOK-specific concepts, metadata extensions, and validators still live in the BTABOK profile. For the full record of what moved and what remained, see Core-SpecLang-Absorption-Design.md.

## 1. Governing idea

A spec type should not be treated as a filename label only. It should be treated as a first-class classification with five dimensions.

### 1.1 Scope
What boundary does this spec describe?

### 1.2 Intent
Is it defining structure, evolving structure, resolving conflict, governing execution, or describing transition?

### 1.3 Binding strength
Is it descriptive, prescriptive, corrective, authorizing, or gate-controlling?

### 1.4 Profile
Is it Core SpecLang only, or Core plus an architectural profile such as The Standard today and BTABOK later?

### 1.5 Lifecycle role
Does it originate a system, refine it, authorize it, or verify it?

This approach lets the manifest carry the taxonomy while keeping the grammar lean.

## 2. Top-level taxonomy families

I recommend four top-level families.

### 2.1 Foundational specs
These establish the existence, identity, and governing rules of a scope.

- Manifest
- Base System Spec
- Architecture Profile Spec
- Scope Manifest, for larger collections later if needed

### 2.2 Structural specs
These describe a system or a bounded part of a system.

- Subsystem Spec
- Context / Stakeholder Spec
- ASR / Quality Spec
- Deployment / Runtime Spec
- Viewpoint Spec

### 2.3 Evolution specs
These change, extend, repair, or clarify structure.

- Feature Spec
- Bug Spec
- Amendment Spec
- Decision Spec

### 2.4 Governance and transition specs
These control approval, exceptions, sequencing, and verification.

- Governance Spec
- Waiver Spec
- Roadmap / Transition Spec
- Verification Spec

## 3. Canonical spec types

### 3.1 Manifest
**Role:** Root control document for a collection.

**CoDL concept:** CollectionManifest (SpecChat-specific; not a BTABoK standard concept, but modeled as a CoDL concept for uniformity). The concept definition combines Core SpecLang metadata (slug, itemType, name, version, authors, committer, retentionPolicy, and the rest of the standard metadata profile) with any profile-specific extensions, such as BTABoKItem fields when the BTABOK profile is active.

**Scope:** One spec collection.

**Intent:** Govern.

**Binding strength:** Gate-controlling.

**What it must contain**
- system identity
- base spec reference
- lifecycle states
- tracking block convention
- document type registry
- spec inventory
- execution order
- conventions

The current manifests already behave this way.

### 3.2 Base System Spec
**Role:** Canonical system skeleton.

**CoDL concept:** SystemSpec (SpecChat-specific)

**Scope:** One bounded system.

**Intent:** Define.

**Binding strength:** Prescriptive.

**What it must contain**
- context
- system declaration
- topology
- constraints
- contracts
- core data model

**What it may contain**
- phases
- traces
- deployment
- views and dynamics
- design and pages
- package policy
- platform realization

### 3.3 Architecture Profile Spec
**Role:** Define an architectural dialect or discipline that constrains how base and evolution specs are written.

**CoDL concept:** ProfileDefinition (SpecChat-specific)

**Scope:** Reusable across one or more collections.

**Intent:** Specialize.

**Binding strength:** Prescriptive.

This is the natural home for:
- The Standard
- a future BTABOK Profile
- future domain profiles

The Standard already demonstrates that profile-driven specialization can shape component vocabulary, realization directives, and conventions without replacing the collection model.

### 3.4 Subsystem Spec
**Role:** Describe a bounded supporting system that can be built and operated independently.

**CoDL concept:** SubsystemSpec (SpecChat-specific)

**Scope:** Child or peer system.

**Intent:** Define.

**Binding strength:** Prescriptive.

A subsystem spec:
- has its own context
- has its own authored and consumed components
- has its own data and contracts
- may have its own deployment
- can execute independently of the parent base spec

PayGate and SendGate are already strong proofs of this category.

### 3.5 Feature Spec
**Role:** Add capability.

**CoDL concept:** FeatureSpec (SpecChat-specific)

**Scope:** Targeted evolution.

**Intent:** Extend.

**Binding strength:** Prescriptive.

**Standard shape**
- purpose
- component additions with contracts
- data models
- page integration
- test obligations
- concrete example

### 3.6 Bug Spec
**Role:** Correct source behavior when the spec is already right.

**CoDL concept:** BugSpec (SpecChat-specific)

**Scope:** Targeted defect.

**Intent:** Repair.

**Binding strength:** Corrective.

**Standard shape**
- current behavior
- specified behavior
- root cause analysis
- acceptance criteria
- implementation guidance

### 3.7 Amendment Spec
**Role:** Correct the spec itself without adding new capability.

**CoDL concept:** AmendmentSpec (SpecChat-specific)

**Scope:** Targeted correction.

**Intent:** Normalize.

**Binding strength:** Corrective.

**Standard shape**
- count corrections
- dependency accounting
- structural adjustments

### 3.8 Decision Spec
**Role:** Resolve ambiguity, tension, or conflict.

**CoDL concept:** DecisionRecord (BTABoK standard)

**Scope:** Targeted architectural question.

**Intent:** Decide.

**Binding strength:** Authorizing.

**Standard shape**
- options
- trade-offs
- recommendation
- amendments to base spec
- effectivity once accepted and executed

This is the most natural home for BTABOK-style architecture decisioning.

### 3.9 Context / Stakeholder Spec
**Role:** Describe stakeholders, concerns, and system-boundary expectations at a scope above or beside the technical base spec.

**CoDL concept:** StakeholderCard (BTABoK standard)

**Scope:** Business, program, enterprise, or large-solution context.

**Intent:** Frame.

**Binding strength:** Prescriptive where referenced.

This type is new, but it aligns cleanly with existing SpecLang context constructs:
- persons
- external systems
- relationships
- tags

Initially, this can be mostly structured markdown plus allowed SpecLang context blocks. Later, a BTABOK profile can add first-class constructs such as `stakeholder`, `concern`, and `viewpoint` if warranted.

### 3.10 ASR / Quality Spec
**Role:** Isolate architecturally significant requirements and measurable quality constraints.

**CoDL concept:** ASRCard (BTABoK standard)

**Scope:** Cross-cutting.

**Intent:** Constrain.

**Binding strength:** Prescriptive.

This type is also new, but it maps well to current constructs:
- contracts
- constraints
- topology prohibitions
- phase gates
- traces

### 3.11 Governance Spec
**Role:** Define review authority, approval rules, compliance gates, and execution policy.

**CoDL concept:** GovernanceBody or GovernanceRule (BTABoK standard; the type may be specialized by instance)

**Scope:** Collection or portfolio.

**Intent:** Govern.

**Binding strength:** Gate-controlling.

This can initially live mostly in the manifest layer and does not require immediate grammar expansion.

### 3.12 Waiver Spec
**Role:** Record an approved exception to a rule, principle, or policy.

**CoDL concept:** WaiverRecord (BTABoK standard)

**Scope:** Narrow and time-bounded.

**Intent:** Exempt.

**Binding strength:** Authorizing, but temporary.

A waiver does not mean the rule changes. It means a bounded exception has been granted.

### 3.13 Roadmap / Transition Spec
**Role:** Describe baseline, target, transitions, dependencies, and sequencing.

**CoDL concept:** RoadmapItem or TransitionArchitecture (BTABoK standard)

**Scope:** Time-bound transformation.

**Intent:** Transition.

**Binding strength:** Planning and gating.

This generalizes the tiering and execution-order logic already present in the manifests into a reusable transition artifact.

### 3.14 Verification Spec
**Role:** Define how correctness is independently confirmed after execution.

**CoDL concept:** VerificationRecord (SpecChat-specific)

**Scope:** Any other spec type.

**Intent:** Verify.

**Binding strength:** Gate-closing.

This formalizes what is currently implied by the Verified lifecycle state.

## 4. Recommended taxonomy matrix

### 4.1 Collection-governing
- Manifest (CoDL: CollectionManifest)
- Governance Spec (CoDL: GovernanceBody or GovernanceRule)

### 4.2 Scope-defining
- Base System Spec (CoDL: SystemSpec)
- Subsystem Spec (CoDL: SubsystemSpec)
- Context / Stakeholder Spec (CoDL: StakeholderCard)
- ASR / Quality Spec (CoDL: ASRCard)
- Viewpoint Spec (CoDL: ViewpointCard)
- Deployment / Runtime Spec (CoDL: DeploymentSpec)

### 4.3 Change-driving
- Feature Spec (CoDL: FeatureSpec)
- Bug Spec (CoDL: BugSpec)
- Amendment Spec (CoDL: AmendmentSpec)
- Decision Spec (CoDL: DecisionRecord)
- Waiver Spec (CoDL: WaiverRecord)

### 4.4 Time-driving
- Roadmap / Transition Spec (CoDL: RoadmapItem or TransitionArchitecture)
- Verification Spec (CoDL: VerificationRecord)

### 4.5 Profile-defining
- Architecture Profile Spec (CoDL: ProfileDefinition)

## 5. Critical distinction: spec type vs profile

These must remain separate.

A **spec type** answers:
> What kind of artifact is this?

A **profile** answers:
> What architectural discipline or vocabulary shapes this artifact?

Examples:
- Base System Spec + Core
- Base System Spec + TheStandard
- Decision Spec + BTABOK
- ASR Spec + BTABOK
- Subsystem Spec + Core

This separation is already foreshadowed by The Standard example, where the same collection model and lifecycle exist but the language surface is specialized.

### 5.1 Spec type vs CoDL concept type

A further distinction is needed now that CoDL is in scope.

A **spec type** in this taxonomy answers:
> What kind of artifact is this in SpecChat?

A **CoDL concept type** answers:
> What is the stored schema of this concept in BTABoK?

A SpecChat spec type IS a CoDL concept. The two terms describe the same artifact from two vantages: the spec type names it from the SpecChat authoring perspective; the CoDL concept type names it from the BTABoK stored-schema perspective. A Decision Spec in SpecChat is a DecisionRecord in CoDL. A Base System Spec in SpecChat is a SystemSpec in CoDL. There is no second artifact.

A profile, by contrast, shapes which spec types and concept types are in scope for a given collection and which CoDL extensions are active. Core, The Standard, and a future BTABOK profile each enable a different working set of concept types.

A third distinction now matters. Some CoDL metadata fields belong to Core SpecLang as universal infrastructure, while others are profile extensions. Core SpecLang provides the base SpecItem metadata (slug, itemType, name, version, publishStatus, authors, reviewers, committer, tags, createdAt, updatedAt, retentionPolicy, freshnessSla, lastReviewed, dependencies) that every spec carries regardless of profile. Profiles add their own metadata fields on top. The BTABOK profile, for example, adds BTABoKItem fields such as accessTier, bokStatus, certainty, and the BTABoK identifier set. Every concept definition in every profile therefore combines Core SpecLang metadata with whatever profile-specific metadata the active profile declares.

## 6. Recommended adoption order

### Pass 1: Taxonomy only, no grammar change
Update the manifest model to recognize these canonical types:

- Manifest
- Base
- Subsystem
- Feature
- Bug
- Amendment
- Decision
- ArchitectureProfile
- Context
- ASR
- Governance
- Waiver
- Roadmap
- Verification

This can be done immediately because manifests already carry a document type registry and an inventory.

### Pass 2: Formal profile mechanism
Formalize `Profile` as manifest metadata:

- Core
- TheStandard
- BTABOK
- future custom profiles

This keeps BTABOK out of the core grammar until repeated use shows exactly which constructs deserve first-class syntax.

Adopting the BTABOK profile also means adopting CoDL 0.2 as the canonical concept syntax and CaDL as the canvas syntax for BTABOK-specific concept types. This is the Option A decision documented in CoDL-CaDL-Integration-Notes.md: CoDL concept records are the canonical form; SpecLang-style aliases are permitted for author convenience but desugar to CoDL. The CoDL 0.2 adoption sits on top of the Core SpecLang infrastructure (standard metadata, reference types, relationship declarations, retention policy, diagnostic extensions) that is always present regardless of profile.

### Pass 3: Selective grammar expansion
Only after real use, add BTABOK-native constructs that prove load-bearing, likely in this order:

- `viewpoint`
- `concern`
- `asr`
- `principle`
- `risk`
- `waiver`

These items are BTABOK-specific concept types. The core language already handles slug, metadata, references, and relationships after Option X, so the grammar work in this pass is limited to profile-specific concept vocabulary rather than infrastructure. These are ergonomic SpecLang aliases that desugar to CoDL concept records. They are not separate from CoDL; they are short forms that compile to the corresponding CoDL concept type (ViewpointCard, StakeholderCard, ASRCard, PrincipleCard, RiskCard, WaiverRecord).

Do not begin by adding the entire BTABOK vocabulary to the core grammar.

## 7. Recommended rule set

1. Every collection has exactly one Manifest.
2. Every bounded implementation scope has exactly one Base System Spec.
3. Every non-core architectural discipline is represented as a Profile, not as a new base taxonomy family.
4. Subsystem Specs are peer structural specs, not glorified feature specs.
5. Decision, Amendment, Bug, and Feature remain the core evolution quartet.
6. Context, ASR, Governance, Waiver, Roadmap, and Verification are the first BTABOK-aligned expansion set.
7. New grammar keywords are added only after repeated use shows they are more than metadata or document-structure concerns.

## 8. Recommended v0.1 taxonomy, final form

If a short canonical list is needed now, adopt this set:

- Manifest (CoDL: CollectionManifest)
- Base System Spec (CoDL: SystemSpec)
- Subsystem Spec (CoDL: SubsystemSpec)
- Architecture Profile Spec (CoDL: ProfileDefinition)
- Context / Stakeholder Spec (CoDL: StakeholderCard)
- ASR / Quality Spec (CoDL: ASRCard)
- Feature Spec (CoDL: FeatureSpec)
- Bug Spec (CoDL: BugSpec)
- Amendment Spec (CoDL: AmendmentSpec)
- Decision Spec (CoDL: DecisionRecord)
- Governance Spec (CoDL: GovernanceBody or GovernanceRule)
- Waiver Spec (CoDL: WaiverRecord)
- Roadmap / Transition Spec (CoDL: RoadmapItem or TransitionArchitecture)
- Verification Spec (CoDL: VerificationRecord)

This is a strong backbone for expansion without overcommitting the grammar.

## 9. Immediate next step

The next best move is to define a **Manifest Type Registry v2** plus the **required metadata fields for each spec type**. That will let the taxonomy become operational before any grammar changes are made.

## Appendix A. Reference sources

### A.1 Uploaded SpecChat and sample documents

1. **SpecChat-Overview.md**
   - Used for the layered model of SpecChat, the manifest's governing role, and the core incremental types: feature, bug, decision, and amendment.

2. **SpecLang-Specification.md**
   - Used for the design principles that the specification is the primary artifact, that complexity is opt-in, and that the five registers are peers.

3. **SpecLang-Grammar.md**
   - Used for confirmation of the current grammar surface and for caution against premature keyword expansion.

4. **TodoApp.manifest.md**
   - Used as a sample manifest showing lifecycle states, tracking convention, and document type registry.

5. **PizzaShop.manifest.md**
   - Used as a sample manifest showing subsystem specs in the registry and execution ordering by tier.

6. **todo-app-the-standard.manifest.md**
   - Used as evidence that profile-driven variation already exists through The Standard.

7. **PayGate.spec.md**
   - Used as evidence of an independently describable and executable subsystem spec.

8. **SendGate.spec.md**
   - Used as companion evidence of the same subsystem pattern.

9. **PizzaShop.spec.md**
   - Used to confirm that subsystem specs can be referenced as external systems by a parent base spec.

### A.2 External BTABOK and IASA sources

1. **IASA BTABOK: Views and Viewpoints**  
   https://iasa-global.github.io/btabok/views.html

   Used for the BTABOK emphasis on views, viewpoints, stakeholder concerns, and architecture communication.

2. **IASA BTABOK: Views & Viewpoints**  
   https://iasa-global.github.io/btabok/views_viewpoints.html

   Used for the formal distinction between views and viewpoints and the linkage to stakeholders and concerns.

3. **IASA BTABOK: Requirements**  
   https://iasa-global.github.io/btabok/requirements.html

   Used for the treatment of architecturally significant requirements and cross-cutting quality concerns.

4. **IASA BTABOK: Decisions**  
   https://iasa-global.github.io/btabok/decisions.html

   Used for the emphasis on architecturally significant decisions and their traceability.

5. **IASA Education Portal: BTABoK Architecture Overview**  
   https://education.iasaglobal.org/browse/btabok/3.2/core-site/core/canvas/architecture-overview

   Included as the primary BTABOK entry point supplied for this line of analysis.

6. **Preiss, Paul. Structured Concept Definition Language. BTABoK 3.2, IASA Global Education Portal (2026).**

   Used as the authoritative source for CoDL (Concept Definition Language) and CaDL (Canvas Definition Language), including the concept type catalog, the Standard BTABoK Metadata profile, and the canvas-as-view-of-concept principle.

### A.3 WIP workspace

1. **WIP workspace: CoDL-CaDL-Integration-Notes.md**

   Used as the companion document recording the full alignment between this taxonomy and the authoritative CoDL/CaDL sources, including the Option A decision to adopt CoDL syntax as the canonical form under the BTABOK profile.

2. **WIP workspace: Core-SpecLang-Absorption-Design.md**

   Used as the authoritative record of the Option X decision: the seven infrastructure elements (standard metadata profile, reference types, relationship declarations, retention policy, diagnostic extensions, slug rules, and ten core validators) absorbed from the BTABOK profile into Core SpecLang, and the BTABOK-profile scope that remains after the absorption.

## Appendix B. Notes on interpretation

This taxonomy is intentionally conservative.

It recommends expanding the **spec ecology** before expanding the **core DSL grammar**. In other words:

- add new document classes first
- add profile metadata second
- add new grammar constructs only after repeated use proves they are structurally load-bearing

That ordering preserves SpecChat's existing strengths while creating room for BTABOK-style architectural breadth.
