# BTABOK Models and Material Out of Scope for SpecChat Integration

## Tracking

| Field | Value |
|---|---|
| Document ID | OOS-001 |
| itemType | ScopeReference |
| slug | btabok-out-of-scope-models |
| Version | 0.1.0 |
| Created | 2026-04-17 |
| Last Reviewed | 2026-04-17 |
| State | Draft |
| retentionPolicy | indefinite |
| Freshness SLA | 180 days |
| Owner | PER-01 Lena Brandt, Chief Architect |
| Approver | PER-11 Anja Petersen, Chair EARB |
| Dependencies | SpecLang-Design.md |

## 1. Purpose

This document enumerates the portions of the BTABOK that are **not addressed** by the SpecChat integration. It exists as a standalone reference so anyone reading the corpus can quickly see the scope boundary.

The SpecChat integration has been scoped to the BTABOK **Engagement Model only**. This scoping decision is recorded in [SpecLang-Design.md](SpecLang-Design.md) Section 7 and is a settled architectural decision. The BTABOK includes three additional models plus cross-cutting material that fall outside this integration. Those are catalogued here for completeness and for future reference if any subset is ever reconsidered for inclusion.

## 2. The BTABOK at a Glance

The BTABOK has four major models:

| Model | Scope for SpecChat |
|---|---|
| **Engagement Model** | In scope (covered by the BTABOK profile) |
| **Value Model** | Out of scope |
| **People Model** | Out of scope |
| **Competency Model** | Out of scope |

Plus cross-cutting material also out of scope:

| Material | Scope for SpecChat |
|---|---|
| Structured Canvas Library (75+ canvases) | Out of scope as a library; CaDL mechanism adopted |
| Topic Areas (AI, DevOps, Cloud, Security, Sustainability, etc.) | Out of scope |

The rest of this document describes each out-of-scope area.

## 3. Value Model (Out of Scope)

The Value Model covers the business-value and investment-planning substrate that connects architecture work to organizational outcomes. Its sub-areas:

- Objectives
- Technical Debt
- Investment Planning
- Scope and Context
- Structural Complexity
- Coverage
- Principles (as value-driven guardrails)
- Analysis
- Value Streams
- Benefits Realization
- Value Methods
- Risk Methods

### 3.1 Why excluded

These are business-strategy and portfolio-investment concerns. They inform **why** architecture work is funded, not **how** it is specified. SpecChat's BTABOK profile can carry references to business cases and metric baselines (`MetricDefinition`, `ScorecardDefinition` are profile concepts), but the full Value Model framework (benefit dependency models, value stream mapping, investment prioritization, technical-debt portfolios) is not expressible as specification constructs.

### 3.2 Partial overlap

Two areas have deliberate overlap with the in-scope Engagement Model integration:

- **Principles as value-driven guardrails.** BTABOK discusses principles both as Value Model guardrails and as Engagement Model governance artifacts. SpecChat's `PrincipleCard` concept (in the BTABOK profile) handles the Engagement Model face. Value Model principle treatment is not modeled.
- **Metrics and scorecards.** SpecChat defines `MetricDefinition` and `ScorecardDefinition` as concept types so the Engagement Model concept map (ASRs, decisions, roadmap transitions) can reference metric baselines and targets. Full Value Model benefits realization is not modeled.

## 4. People Model (Out of Scope)

The People Model covers the architecture profession as an organizational and human system. Its sub-areas:

- Extended Team
- Organization
- Career
- Roles
- Competency (at the organizational deployment level)
- Job Description
- Community

### 4.1 Why excluded

This defines who architects are, how they are organized, how they advance, and how they relate to each other. BTABOK is explicit that it is a **people framework**. That framing is important at the practice level, but it operates at the organizational level, not the specification level. A spec language cannot meaningfully model a career ladder, a community of practice, or an organizational hierarchy beyond naming the actors involved.

### 4.2 Partial overlap

SpecChat does handle **named ownership** universally through the Core SpecLang Standard SpecItem metadata profile (fields `authors`, `reviewers`, `committer` using `PersonRef`). This lets any spec identify the people responsible for it. That is a minimal identity concern shared with the People Model, but it is not the People Model. SpecChat's `PersonRef` stores identity references only; it does not model role definitions, career paths, organizational structure, or competency deployment.

## 5. Competency Model (Out of Scope)

The Competency Model is the professional development substrate for architects. Its structure:

- **9 competency pillars**: Business Technology Strategy, Human Dynamics, Design, IT Environment, Quality Attributes, Business Architecture, Information Architecture, Infrastructure Architecture, Software Architecture
- **80 competency areas** distributed across the pillars
- **5 proficiency levels**: Awareness, Basic, Delivery, Experienced, Shaping
- **4 primary specializations** (Business, Information, Infrastructure, Software) built on shared pillars
- **CITA certification levels**: Foundation, Associate, Professional, Distinguished
- **Related external frameworks**: SFIA+, TOGAF Skills Framework, European e-CF

### 5.1 Why excluded

This governs architect capability, not system architecture. It is the professional development ladder. A spec language has no business carrying competency assessments, learning objectives, or certification progression. These concerns belong to HR systems, training platforms, and professional bodies, not to specifications of systems.

### 5.2 No overlap

The SpecChat integration has no contact surface with the Competency Model. The WIP corpus does not reference competencies, pillars, proficiency levels, or certification. If practitioner certification ever becomes relevant to SpecChat (for example, to gate approval rights on a DecisionRecord), it would be added through an external identity system, not by absorbing the Competency Model into SpecLang.

## 6. Structured Canvas Library (Out of Scope as a Library)

The BTABoK ships with over 75 canvases, cards, worksheets, and documents organized into eight categories (Business, Chief, Core, Engagement Model, Information, Infrastructure, Software, Solution). Examples:

- Business Model Canvas
- Architecture Definition Canvas
- Bounded Context Canvas
- Service Blueprint Canvas
- Service Interface Design Canvas
- Architect Organization Capability Canvas
- Strategic Roadmap Canvas
- Capability Card
- Architecture Hypothesis Canvas

The full BTABoK canvas library is **not imported wholesale** into SpecChat.

### 6.1 The distinction: mechanism vs. library

A critical distinction applies here. SpecChat's BTABOK profile **does adopt CaDL as a mechanism**: the Canvas Definition Language is the formal way to declare canvases over CoDL concepts. SpecChat ships 20 canvases in its CaDL canvas catalog ([SpecLang-Design.md Section 6.5](SpecLang-Design.md)), each specifically relevant to the Engagement Model concepts covered by the BTABOK profile.

What is out of scope is the broader BTABoK canvas *library* as published by IASA. Those canvases are valuable artifacts in BTABOK practice, but most of them are:
- Value Model canvases (business model canvas, strategic roadmap canvas)
- People Model canvases (architect organization capability canvas)
- Topic-area canvases (information, infrastructure, software specifics)

A future SpecChat profile covering a different BTABOK model could import the relevant canvas subset from that library. This is not an architectural obstacle; it is simply not current scope.

### 6.2 Summary

| Concern | Status |
|---|---|
| CaDL as a formal canvas language mechanism | **In scope**, implemented in SpecChat's BTABOK profile |
| 20 canvases targeted at Engagement Model concepts | **In scope**, shipped with the BTABOK profile |
| The 75+ BTABoK canvas library as published by IASA | Out of scope as a library |

## 7. Topic Areas (Out of Scope)

BTABOK calls out cross-cutting knowledge domains as Topic Areas:

- Artificial Intelligence
- Systems
- DevOps
- Cloud
- Security
- Integration
- Sustainability

### 7.1 Why excluded

These are knowledge domains that influence how architecture work happens in specific technology contexts. They do not define new specification constructs. A SpecChat collection working in a regulated cloud environment will naturally address security and cloud concerns through its contracts, constraints, package policies, deployment topology, and platform realization declarations, but it does so using Core SpecLang and the Engagement Model profile, not a specialized topic-area profile.

### 7.2 No profile planned

There is no current or planned BTABOK-Topic-Area profile. If topic-specific specification conventions ever prove necessary (for example, a regulated-sustainability profile that enforces DPP-style fields on product specifications), it would be a new profile separate from both the Engagement Model profile and any future Value Model or People Model profiles.

## 8. Summary Matrix

| BTABOK Component | SpecChat Integration Status | Rationale |
|---|---|---|
| Engagement Model | **In scope** (the BTABOK profile) | Its operating-model concepts map directly onto specification constructs |
| Value Model | Out of scope | Business-strategy and portfolio-investment concerns; not expressible as spec constructs |
| People Model | Out of scope | Organizational and human-system concerns; operates above the specification level |
| Competency Model | Out of scope | Professional development ladder; belongs to HR, training, and certification systems |
| Structured Canvas Library (75+ canvases) | Out of scope as a library; **CaDL mechanism in scope** | SpecChat ships 20 Engagement-Model-targeted canvases using CaDL; the full BTABoK canvas library is not imported |
| Topic Areas | Out of scope | Knowledge domains that influence practice but do not define specification constructs |

## 9. Implication for Future Work

If any of the excluded models or material is ever wanted in SpecChat, the implementation approach is:

**Separate profiles.** A Value Model profile or a People Model profile would each be an independent SpecChat profile, activated separately. Per the **one-profile-at-a-time** decision in v0.1 (see [SpecLang-Design.md](SpecLang-Design.md) Section 3 settled decisions), such a profile could not coexist with the BTABOK (Engagement Model) profile in the same collection. Cross-profile collections are a v0.2+ capability, explicitly listed as deferred in the open items section of the design.

**Option Y observation points.** Some absorption candidates in the WIP corpus sit near the Value Model boundary. The Option Y deferrals for `MetricDefinition`, `ExperimentCard`, and Decision Spec enrichment (scope, type, method, reversibility fields) touch value-management dimensions. They are deliberately held in the BTABOK profile for observation before any core absorption so that the boundary stays honest. If these concepts prove to need features that only make sense in a Value Model context, that is a signal to create a Value Model profile rather than leak value-model concerns into the Engagement Model profile.

**The single-purpose criterion.** Any future profile should pass the same three criteria that shaped the current scope:
1. **Universal applicability** within its target audience. A profile concept should apply across many collections of its kind, not just a single exemplar.
2. **Worldview neutrality** relative to SpecChat. A profile should not require SpecChat users who do not care about that practice to reason about its concepts.
3. **Low incremental cost**. A profile should not impose complexity on the rest of the language.

The Value Model, People Model, and Competency Model can each meet these criteria in principle if a clean profile is designed. They are excluded from v0.1 by scope choice, not by architectural impossibility.

## 10. Where This Scope Is Documented Across the Corpus

The scope boundary is referenced in several places for consistency:

| Doc | Reference |
|---|---|
| [SpecLang-Design.md](SpecLang-Design.md) Section 2 | Governing principle on scope discipline |
| [SpecLang-Design.md](SpecLang-Design.md) Section 7 | Engagement Model Scope section with in/out enumeration |
| [SpecLang-Design.md](SpecLang-Design.md) Section 14 | Open items noting future profile work |
| [Spec-Type-System.md](Spec-Type-System.md) Section 3 | The three-layer stratification that supports profile scoping |
| [Global-Corp-Exemplar.md](Global-Corp-Exemplar.md) Appendix C | Scope mapping from the exemplar to Engagement Model specs |
| `WIP/Archive/BTABOK-EngagementModel-Mapping.md` | Original scoping decision (preserved for provenance) |

This document is the dedicated reference for the out-of-scope side of that boundary.

## 11. Source References

**[R1]** SpecLang Design. Workspace: [SpecLang-Design.md](SpecLang-Design.md). The authoritative in-scope design including the Engagement Model concept map.

**[R2]** Spec Type System. Workspace: [Spec-Type-System.md](Spec-Type-System.md). The three-layer stratification that supports scoped profiles.

**[R3]** Global Corp Exemplar. Workspace: [Global-Corp-Exemplar.md](Global-Corp-Exemplar.md). Appendix C contains a section-by-section scope mapping.

**[R4]** IASA Global. Business Technology Architecture Body of Knowledge (BTABoK). `https://iasa-global.github.io/btabok/`. The authoritative source for the four-model structure, topic areas, and structured canvas library.

**[R5]** IASA Global. Competency Model. `https://iasa-global.github.io/btabok/competency_model_m.html`. Details of the nine pillars and 80 competency areas.

**[R6]** IASA Global. Structured Canvases. `https://iasa-global.github.io/btabok/structured_canvases_m.html`. The 75+ canvas library categorization.

**[R7]** Prior historical analysis. Workspace: `WIP/Archive/BTA-BOK-integration.md`. The original broad-scope gap analysis that the scoped Engagement Model mapping replaced.
