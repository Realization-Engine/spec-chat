# Why We Created the Spec Type Taxonomy

## Purpose

This explainer captures the architectural reasoning behind the creation of the **Spec Type Taxonomy** in SpecChat. It answers a specific design question: how should SpecChat intersect with the BTABOK without losing the clarity and coherence of the existing language?

The conclusion was not to merge BTABOK wholesale into the core SpecLang grammar. Instead, the right move was to **stratify** the system and define a taxonomy of specification types before expanding the grammar.

## The Design Problem

SpecChat already has a clear center of gravity:

- the specification is the primary engineering artifact
- the core language is intentionally lean
- complexity is opt-in
- manifests govern collections of specs
- multiple document types already exist in practice
- the system already has precedent for domain-specific extension through variants such as **The Standard**

That means the question was never simply, "Should BTABOK ideas be added?" The real question was:

**Where should BTABOK concepts live so that SpecChat becomes more capable without becoming bloated?**

## Why We Did Not Merge BTABOK into the Core DSL

BTABOK is broader than a syntax for architecture description. It covers architectural practice as much as architectural structure. It emphasizes:

- stakeholder-centered views and viewpoints
- architecturally significant requirements and decisions
- repositories organized for architects first
- lightweight architecture deliverables used for thinking and communication
- stakeholder mapping
- business cases
- principles
- risk registers
- benefits dependency models
- waivers
- roadmaps

Those are important concerns, but many of them are **not** the same kind of thing as the current SpecLang core constructs.

The current core language is strongest when it describes things that are:

- structural
- reusable across many systems
- semantically checkable
- suitable for machine interpretation and realization

That is why the existing five-register core should remain lean:

- data
- context
- systems
- deployment
- view/dynamic/design

These registers are already proven load-bearing in the current corpus and in the existing example specs.

If BTABOK were merged wholesale into the top-level grammar, the result would likely be a language that mixes structural declarations, governance concepts, business abstractions, review artifacts, and lifecycle documents all at the same level. That would make the language harder to reason about, harder to parse cleanly, and easier to bloat over time.

## The Chosen Architectural Move: Stratification

Instead of flattening everything into the core language, we chose a three-layer model.

### 1. Core SpecLang

The core language remains the executable substrate.

Its job is to carry the parts of the system that are fundamentally structural and machine-checkable. It stays small, disciplined, and reusable across domains.

### 2. BTABOK Profile

BTABOK becomes an optional **profile** or **extension layer** above the core.

That profile can introduce practice-oriented concepts such as:

- `asr`
- `decision`
- `principle`
- `stakeholder`
- `concern`
- `viewpoint`
- `risk`
- `waiver`
- `capability`
- `benefit`
- `owner`
- `approver`

These concepts matter because they connect architecture to stakeholder concerns, value, governance, and traceable decisions. They are architecturally important, but they should not automatically become part of the universal core grammar.

### 3. Scope-Specific Spec Types

The third layer is the **Spec Type Taxonomy** itself.

Rather than treating every concern as a new grammar keyword, we allow the manifest and the document model to determine what kinds of specs exist for different scopes.

This is the key design move. Many BTABOK-aligned concerns belong more naturally in **spec types** and **metadata blocks** than in the universal grammar.

## Why a Taxonomy Was Necessary

Once we accepted stratification, a new problem appeared:

If BTABOK concepts are not all going directly into the grammar, how do we organize them?

The answer is: through a **Spec Type Taxonomy**.

A taxonomy gives the system an explicit model of artifact kinds. Instead of asking only, "What can the DSL express?" we also ask:

- What kind of spec is this?
- What scope does it serve?
- Who is its audience?
- What lifecycle role does it play?
- What metadata must it carry?
- What profile, if any, shapes it?

This allows SpecChat to support different kinds of architecture artifacts without forcing them all into a single syntactic plane.

## The Core Principle Behind the Taxonomy

The taxonomy rests on one simple rule:

- Put something in the **core DSL** if it is structural, reusable across most software systems, and semantically checkable.
- Put something in the **BTABOK profile** if it expresses architectural practice, governance, value, or stakeholder concerns.
- Put something in a **spec type** if it is primarily an artifact of scope, audience, or lifecycle stage.

This rule prevents category confusion.

It keeps the grammar from becoming a dumping ground for every architecture concept, while still allowing the overall system to become much richer.

## What the Taxonomy Makes Possible

Once spec types are made explicit, different scopes can be served cleanly.

Examples include:

### Enterprise / Strategy Spec
Focuses on capabilities, business outcomes, principles, roadmap transitions, and benefits.

### Stakeholder Spec
Focuses on stakeholder classes, concerns, influence posture, engagement needs, and required viewpoints.

### Architecture / Solution Spec
Focuses on the current core system shape: context, components, topology, deployment, views, and dynamics.

### Subsystem Spec
Focuses on bounded supporting systems such as PayGate or SendGate, each with its own structure and deployment.

### Decision Spec
Focuses on architecture decisions and their links to ASRs, principles, risks, waivers, and consequences.

### Quality / ASR Spec
Focuses on quality attributes, measurable scenarios, constraints, and architecturally significant requirements.

### Governance Spec
Focuses on compliance posture, exceptions, waivers, review authority, and approval chains.

### Roadmap / Transition Spec
Focuses on baselines, targets, sequencing, dependencies, transitions, and milestones.

### Feature / Bug / Amendment Spec
Focuses on the delivery-scale evolution model that SpecChat already uses successfully.

This gives SpecChat a way to support multiple levels of architectural work without making every spec look the same.

## The Architectural Payoff

The taxonomy protects three things at once.

### It protects the core language
The core DSL remains coherent, disciplined, and technically meaningful.

### It protects extensibility
New architectural profiles, including a BTABOK-aligned one, can be added without destabilizing the base language.

### It protects scope clarity
Different documents can now exist because they serve different purposes, audiences, and lifecycle roles, not because the grammar became more chaotic.

## The Governing Position

So the governing position is this:

We are **not** turning SpecLang into BTABOK.

We are making **SpecLang the executable substrate**, while allowing **BTABOK to act as an architectural profile** that shapes which specs exist and what metadata those specs must carry.

That is why the **Spec Type Taxonomy** had to come first.

Until the taxonomy is stable, grammar expansion is premature. Once the taxonomy is clear, the necessary grammar changes become much easier to identify and justify.

## Conclusion

The Spec Type Taxonomy was created to solve a structural design problem:

How do we broaden SpecChat to handle architectural practice, governance, stakeholder concerns, and strategy-level artifacts without corrupting the clarity of the core language?

The answer was not grammar inflation.

The answer was stratification:

- a lean **Core SpecLang**
- an optional **BTABOK Profile**
- a formal **Spec Type Taxonomy** that governs which artifacts exist at which scopes

This lets SpecChat grow upward in scope and architectural depth while preserving the coherence of its executable foundation.

## Appendix A. Reference Source

1. **Views and Viewpoints | IASA - BTABoK**  
   https://iasa-global.github.io/btabok/views.html?utm_source=chatgpt.com
