# Analysis: Deterministic Validation by Spec Type

## Purpose

This document analyzes the proposition that **each spec type in the taxonomy should have its own DSL extension so that spec documents can be validated deterministically**.

## Conclusion

The proposition is directionally correct, but it needs one refinement:

**Each spec type should have its own deterministic validation contract, but not necessarily its own full grammar extension.**

That is the cleanest architectural interpretation of the idea.

SpecChat already separates three concerns that should remain distinct:

1. **Core language**
2. **Document type system**
3. **Workflow and manifest governance**

The current corpus shows that SpecChat defines a formal language across multiple registers, uses manifests to define document types and lifecycle, and validates through parsing, semantic analysis, and quality checks. That layered design should be preserved.

## Why a Full DSL Extension for Every Type Is Too Much

The current system treats **document types** as governed artifacts within a collection. The manifest defines:

- the document type registry
- lifecycle states
- tracking conventions
- inventory
- execution order

The overview also describes feature, bug, decision, and amendment as different document forms in the workflow, not as separate language families.

If every spec type becomes its own grammar, the taxonomy risks turning into a proliferation of dialects. That would weaken one of SpecChat's strongest properties: one primary executable substrate that source generation and validation can rely on.

The better move is to keep one shared core language and add type-specific validation on top of it.

## The Right Rule

The clean rule is:

**Every spec type should have a type-specific validation profile.**  
**Some spec types should also have a DSL extension.**

Those are not the same thing.

Deterministic validation can come from a combination of:

- document schema validation
- embedded core SpecLang blocks
- AST validation
- semantic checks
- cross-document rules

A separate top-level grammar is not required in every case.

## What Deterministic Validation Should Mean

For each spec type, deterministic validation should answer five questions.

### 1. Document Identity

Is the document declared as the correct spec type in the manifest, with the required tracking metadata and lifecycle state?

### 2. Section Schema

Does the document contain the required headings and sections, in the required order?

### 3. Formal Block Contract

Which `spec` blocks are:

- required
- allowed
- optional
- forbidden

for this document type?

### 4. Semantic Rules

Do the formal declarations resolve correctly, and do the type-specific rules hold?

### 5. Cross-Document Rules

Does the document correctly reference:

- base specs
- dependencies
- amendments
- waivers
- manifest inventory entries

That is the real basis of deterministic validation.

## Recommended Design: Type Profiles

The right abstraction is a **Type Profile**.

A Type Profile should define:

- required metadata
- required sections
- allowed core declarations
- required core declarations
- optional extension declarations
- semantic rules
- cross-document rules
- deterministic pass/fail criteria

That gives every spec type a deterministic validator without forcing grammar growth where it does not belong.

## When a DSL Extension Is Justified

A spec type should get a real DSL extension only when it introduces **reusable formal semantics** that deserve dedicated AST nodes and semantic analysis.

The existing precedent is **The Standard**. It is not just a document template. It changes the language surface with specialized declarations such as broker-oriented components and realization rules. The manifest explicitly treats it as an extension layered onto the core grammar.

That gives a strong design precedent:

- when the semantics are formal and reusable, use an extension
- when the semantics are mostly structural, procedural, or documentary, use a validation profile

Under the CoDL alignment, a "DSL extension" for a BTABoK-aligned concept is best expressed as a CoDL concept definition, not as a bespoke SpecLang grammar addition. The question becomes "is this concept load-bearing enough to warrant a formal CoDL definition in the BTABOK profile?" rather than "does this deserve its own grammar?" The criterion itself, formal and reusable and semantically checkable across multiple documents, remains unchanged.

## Spec Types That Are Strong Candidates for Real DSL Extensions

These are the types most likely to justify first-class syntax.

### Decision Spec (CoDL concept: `DecisionRecord`)

This type builds on Core SpecLang infrastructure. The metadata, reference types, and relationship declarations used in a Decision Spec come from Core SpecLang. Only the concept-specific fields listed below are BTABOK-profile extensions.

CoDL concept fields:

- option
- recommendation
- consequence
- supersedes
- amendment link

These are stable, repeatable semantics and map well to deterministic validation.

### ASR / Quality Spec (CoDL concept: `ASRCard`)

This type builds on Core SpecLang infrastructure. Metadata, references, and relationships come from the core; only the concept-specific fields below are BTABOK-profile extensions.

CoDL concept fields:

- quality attribute
- measurable scenario
- stimulus
- response
- threshold
- verification obligation

These are architecturally significant and naturally machine-checkable.

### Context / Stakeholder Spec (CoDL concept: `StakeholderCard`)

This type builds on Core SpecLang infrastructure. Metadata, references, and relationships come from the core; only the concept-specific fields below are BTABOK-profile extensions.

CoDL concept fields:

- stakeholder
- concern
- viewpoint
- owner
- approver

These are central to BTABOK-style architectural practice and likely deserve first-class handling if used broadly.

### Governance / Waiver Spec (CoDL concepts: `GovernanceBody`, `WaiverRecord`)

This type builds on Core SpecLang infrastructure. Metadata, references, and relationships come from the core; only the concept-specific fields below are BTABOK-profile extensions.

CoDL concept fields:

- rule
- waiver
- scope
- approver
- expiration

These are formal governance elements, not just prose headings.

### Roadmap / Transition Spec (CoDL concepts: `RoadmapItem`, `TransitionArchitecture`)

This type builds on Core SpecLang infrastructure. Metadata, references, and relationships come from the core; only the concept-specific fields below are BTABOK-profile extensions.

CoDL concept fields:

- baseline
- target
- transition
- milestone
- dependency

These are structurally rich and suitable for deterministic validation if the taxonomy uses them repeatedly.

### Example: ASRCard in CoDL syntax

A minimal ASRCard concept in CoDL form looks like this:

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

This is the target form for any BTABOK-aligned concept under the profile.

## Spec Types That Should Probably Stay Profile-Driven First

These types can be validated deterministically through template rules, section schemas, and embedded core SpecLang blocks without requiring large grammar changes.

### Better Treated as Validation Profiles First

- Manifest
- Base System Spec
- Subsystem Spec
- Feature Spec
- Bug Spec
- Amendment Spec
- Verification Spec

Why these should stay lighter at first:

- their determinism mostly comes from required sections
- they depend heavily on tracking blocks and lifecycle rules
- they are validated by allowed and required formal blocks
- much of their meaning lives in cross-document workflow relationships

That is already how the current system behaves.

## Recommended Validation Architecture

The cleanest implementation uses four validation layers.

### Layer 1: Document-Schema Validation

Validate:

- headings
- tracking block
- tables
- required sections
- manifest registration

### Layer 2: Core SpecLang Validation

Parse all embedded formal blocks with the shared core grammar and AST.

This layer also runs the ten core validators absorbed under the Option X decision:

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

Layer 2 now does much more than parse formal blocks. It enforces core metadata completeness, slug format and uniqueness, reference resolution across `ref<T>`, `weakRef`, and `externalRef`, relationship cardinality, freshness SLAs, profile composition, and supersedes-cycle detection. These checks run on every collection regardless of profile.

This preserves a single executable substrate.

### Layer 3: Type-Profile Validation

Apply spec-type rules such as:

- required sections
- required block kinds
- forbidden declarations
- dependency references
- required examples
- required acceptance criteria
- required verification clauses

Under the BTABOK profile, Layer 3 handles the 13 BTABOK-specific validators that enforce concept-specific rules, including ASR traceability, decision scope and type, stakeholder coverage, viewpoint coverage, waiver expiration and rule reference, governance approval, roadmap capability moves, canvas target existence, and metric baseline/target presence. Layer 3 runs only when the relevant profile is active. Type-profile validation rules are expressed as CoDL concept constraints: required and optional fields, cardinality declarations, and reference resolution through `ref<>`, `weakRef`, and `externalRef` (the latter three provided by Core SpecLang).

### Layer 4: Extension Validation

Layer 4 focuses on CaDL canvas completeness and other projection-specific checks. Canvas validation is profile-specific: under the BTABOK profile this layer checks that each CaDL canvas renders its underlying CoDL concept completely and that every canvas target exists. Other profiles can introduce their own projection checks here.

That is the same general pattern already demonstrated by The Standard. For the BTABOK profile specifically, extension validation covers CaDL canvas completeness over the underlying CoDL concepts.

## The Decisive Point

So the correct position is:

**Yes, each spec type should have its own deterministic validation layer.**  
**No, that should not automatically mean a separate full DSL extension for every type.**

Instead:

- every spec type gets a **Type Profile**
- some spec types also get a **DSL Extension**
- all of them continue to sit on top of one shared **Core SpecLang**

That preserves coherence, keeps validation deterministic, and avoids grammar bloat.

## Strongest Formulation

The cleanest rule to adopt is:

**A spec type must define its validation profile. A spec type earns a DSL extension only when its concepts are formal, reusable, and semantically checkable across multiple documents.**

That is the boundary line that keeps the taxonomy coherent.

## CoDL and CaDL Alignment

In April 2026 IASA Global published two authoritative specifications by Paul Preiss through the BTABoK 3.2 education portal: **CoDL (Concept Definition Language)** and **CaDL (Canvas Definition Language)**. CoDL defines the stored data structure of any BTABoK concept. It is concept-type-neutral and covers primitive types, composite types, reference types, section modifiers, constraints, relationship declarations, storage directives, a transport envelope, display hints, and a Standard BTABoK Metadata profile. CaDL defines how a CoDL concept is rendered as a canvas. Its governing principle is explicit: a canvas is a view of a concept, not a separate stored object type.

These two languages are the authoritative form for the constructs this document describes. The concepts developed here map onto them cleanly.

| This document's construct | CoDL/CaDL equivalent |
|---|---|
| Type Profile (validation contract) | CoDL concept definition; the concept itself is the contract |
| DSL Extension (new formal syntax) | CoDL concept authored in CoDL syntax when the BTABOK profile is active |
| Projection / Extension Validation | CaDL canvas definition over the underlying CoDL concept |

### Decision: Option A with optional aliases

The SpecChat BTABOK profile accepts CoDL syntax verbatim as the canonical form. When the BTABOK profile is active, SpecLang parses `concept`, `section`, `relationships`, and the CoDL type system directly. Files round-trip cleanly with any other CoDL-compliant tool.

Optional SpecLang-style short forms may desugar to CoDL concepts, following the precedent set by The Standard extension (where `broker`, `foundation service`, and `exposer` desugar to `authored component` with a layer property). The ergonomic layer is author convenience. The canonical form is CoDL.

This decision has three consequences for the rest of this analysis:

1. A "DSL extension" in the BTABOK context is a CoDL concept definition, not a bespoke grammar addition to SpecLang.
2. A projection such as a Decision Registry or ASR Matrix is a CaDL canvas, not a separate validation artifact.
3. Every BTABOK-aligned concept carries the Standard BTABoK Metadata profile (`slug`, `itemType`, `name`, `version`, `bokStatus`, `publishStatus`, `accessTier`, `authors`, `reviewers`, `committer`, `tags`, `certainty`, `createdAt`, `updatedAt`), which replaces several parallel metadata fields the WIP corpus had proposed.

Full alignment details, including the Standard Metadata mapping, lifecycle state mapping, reference model alignment, retention and freshness handling, and the scope discipline that keeps the BTABOK profile bounded to the Engagement Model, are recorded in [CoDL-CaDL-Integration-Notes.md](CoDL-CaDL-Integration-Notes.md).

### Scope shift after the Option X decision

As of the Option X decision, several validation concerns have moved from the DSL Extension candidates list into Core SpecLang. The following are now core, not BTABOK-profile specific:

- Slug uniqueness and format
- Reference resolution for `ref<T>`, `weakRef`, and `externalRef`
- Metadata completeness against the Core SpecItem profile
- Relationship cardinality
- Freshness SLA

Every spec collection gets these checks regardless of which profile is active.

The remaining DSL Extension candidates are BTABOK-profile specific. They encode an architecture-practice worldview and activate only when the BTABOK profile is declared:

- Decision scope, type, and method classification
- ASR classification
- Stakeholder concerns and coverage
- Quality attribute scenarios
- Viewpoint alignment
- Waiver rules and expiration
- Governance approval
- Roadmap capability moves
- Canvas target validation
- Metric baseline and target

For the authoritative record of what moved and what remained, see [Core-SpecLang-Absorption-Design.md](Core-SpecLang-Absorption-Design.md).

## Recommended Next Step

For each taxonomy entry, define a **Type Profile Contract** with these fields:

- Required sections
- Required metadata. This combines Core SpecLang SpecItem metadata (slug, itemType, name, version, publishStatus, authors, reviewers, committer, tags, createdAt, updatedAt, retentionPolicy, freshnessSla, lastReviewed, dependencies) with any profile-specific metadata extensions, such as BTABoKItem fields under the BTABOK profile.
- Allowed core SpecLang declarations (data/context/systems/deployment/view registers)
- Required core SpecLang declarations (data/context/systems/deployment/view registers)
- CoDL concept definition (if the type maps to a BTABoK-aligned concept)
- Type-specific semantic checks
- Cross-document checks. These use Core SpecLang reference resolution through `ref<T>`, `weakRef`, and `externalRef`; the resolution machinery is provided by the core, not by each type profile.
- Deterministic pass/fail criteria

Most of the Type Profile Contract infrastructure, specifically required metadata and reference validation, is handled by Core SpecLang. Only the concept-specific validation (field constraints, cardinality on profile-specific relationships, and concept-specific semantic rules) needs to be specified per type.

Once that exists, grammar work can proceed selectively and safely.

---

## Source References

### Uploaded SpecChat corpus

1. **SpecChat-Overview.md**  
   Used for the description of the four-layer system, document types, manifests, and workflow-driven incremental specs.

2. **SpecLang-Specification.md**  
   Used for the description of SpecLang as the primary engineering artifact, the five registers, and the overall principle that the language is a shared executable substrate.

3. **SpecLang-Grammar.md**  
   Used for the distinction between grammar, AST, parsing, and semantic analysis, and for the idea that core formal parsing is already layered and deterministic.

4. **TodoApp.manifest.md**  
   Used as evidence that manifests govern lifecycle states, tracking blocks, document type registries, and collection rules.

5. **PizzaShop.manifest.md**  
   Used as further evidence that manifests govern spec collections, lifecycle states, execution ordering, and document type policy.

6. **PayGate.spec.md**  
   Used as evidence that subsystem specs are full structural specs with their own context, components, contracts, and data model.

7. **SendGate.spec.md**  
   Used alongside PayGate as evidence for independently modeled subsystem specs.

8. **todo-app-the-standard.spec.md**  
   Used as the key precedent showing that a real profile or extension can legitimately alter the language surface and semantics beyond plain document templating.

9. **todo-app-the-standard.manifest.md**  
   Used as supporting evidence that The Standard is treated as an extension-bearing architectural discipline within the same overall document governance model.

### External authoritative sources

10. **Preiss, Paul. Structured Concept Definition Language. BTABoK 3.2, IASA Global Education Portal (2026).**  
    Authoritative source for CoDL (Concept Definition Language) and CaDL (Canvas Definition Language). Used to align this document's DSL Extension and Projection concepts with the published BTABoK languages.

11. **WIP workspace: CoDL-CaDL-Integration-Notes.md**  
    Companion integration notes recording the full alignment between this WIP corpus and the authoritative CoDL/CaDL sources, including the Option A decision and the Standard BTABoK Metadata mapping.

12. **WIP workspace: Core-SpecLang-Absorption-Design.md**  
    Authoritative record of the Option X decision. Specifies the seven infrastructure elements absorbed from the BTABOK profile into Core SpecLang (standard metadata profile, reference types, relationship declarations, retention policy, diagnostic extensions, slug rules, and ten core validators), and the BTABOK-profile scope that remains after the absorption. Used to reclassify the validation concerns in this analysis as core or profile-specific.

### Interpretive synthesis from the analysis

This analysis derives one architectural distinction from those sources:

- **Type Profile** = deterministic validation contract for a spec type
- **DSL Extension** = additional formal syntax and semantics only where the concepts are stable, reusable, and machine-checkable

That distinction is an analytical recommendation built from the source corpus, not a direct quotation from any single file.
