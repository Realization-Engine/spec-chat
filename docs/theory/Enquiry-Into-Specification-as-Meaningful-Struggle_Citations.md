---
title: "Citations for *Enquiry Into Specification as Meaningful Struggle*"
subtitle: "Supporting citations for the memorandum, updated alongside its versions."
author: "Dennis A. Landi"
version: "0.09"
date: "2026-04-18"
category: "Citations"
folio: "№ III · a"
project: "Spec Chat"
source: "https://github.com/Realization-Engine/spec-chat"
---

## Editorial Note

This document supports the memorandum that instantiates the **Multiplier-Mirror framework** (Folio Nº II) at the specification medium.

This revision follows the structure of the current memorandum (v0.09).

Earlier versions cast a wide net across many adjacent traditions. This version keeps only the citation categories that directly support the paper’s argument:

1. specification-driven realization already exists, now formalized through SpecChat,
2. the specification medium must support explicit commitments, refinement, and machine execution,
3. the discipline must still preserve meaningful human struggle,
4. the developmental question (whether specification struggle builds FORCE without prior implementation experience) remains the most consequential open problem,
5. architecture practice contributes a body of knowledge that identifies which AI elements practice requires and which judgments practice categorically refuses, with ASAP as a concrete proposal grounded in the BTABoK.

The author’s internal Storyvizor/DomainEngine materials helped sharpen the paper, but they are **illustrative only** and are not intended to carry the final paper’s argument by themselves.

---

## 1. Formal Specification and Formal Methods

**[1.1]** Hoare, C. A. R. “An Axiomatic Basis for Computer Programming.” *Communications of the ACM*, 12(10), 1969, pp. 576–580, 583. DOI: 10.1145/363235.363259.  
**Digest:** Foundational statement that programming can be treated as precise reasoning about behavior rather than merely manual construction.

**[1.2]** Lamport, L. *Specifying Systems: The TLA+ Language and Tools for Hardware and Software Engineers.* Addison-Wesley, 2002.  
**Digest:** Key support for treating specification as a primary engineering artifact and for tying high-level intent to machine-checkable behavior.

**[1.3]** Abrial, J.-R. *Modeling in Event-B: System and Software Engineering.* Cambridge University Press, 2010.  
**Digest:** Important for refinement as an explicit relation with proof obligations, which directly supports the memorandum’s emphasis on preserving meaning across levels.

**[1.4]** Jackson, D. *Software Abstractions: Logic, Language, and Analysis.* MIT Press, revised edition, 2012.  
**Digest:** Strong precedent for constraint-based modeling plus automatic counterexample generation. Supports the memorandum’s insistence that a true specification medium must support falsification.

---

## 2. MBSE, Semantic Kernels, and Multi-View Systems Modeling

**[2.1]** NASA. *NASA Systems Modeling Handbook for Systems Engineering (NASA-HDBK-1009A).* 2025.  
**Digest:** Strong institutional evidence that systems work is moving toward model-centered, multi-view engineering rather than isolated diagrams.

**[2.2]** Object Management Group. *About SysML v2* and associated SysML v2 / KerML specification materials, 2025–2026.  
**Digest:** Supports the memorandum’s claim that the real center of gravity is an underlying semantic model with multiple textual and graphical projections.

**[2.3]** KerML specification materials and OMG explanatory pages.  
**Digest:** Particularly relevant to the idea of a semantic kernel beneath the specification medium. SpecChat's architecture follows this pattern: a specification model with multiple projections.

**[2.4]** INCOSE / MBSE practice literature on model-centric engineering and traceable system views.  
**Digest:** Supports the claim that the right target is not one perfect diagram, but a coordinated model with multiple views and explicit traceability.

---

## 3. Architecture, Interfaces, Contracts, and Refinement Boundaries

**[3.1]** ISO/IEC/IEEE 42010:2022. *Software, Systems and Enterprise — Architecture Description.*  
**Digest:** Supports the memorandum’s requirement that viewpoints, boundaries, and architecture descriptions be treated systematically.

**[3.2]** de Alfaro, L., & Henzinger, T. A. “Interface Automata.” Proceedings of the 8th European Software Engineering Conference / Foundations of Software Engineering, 2001.  
**Digest:** Strong support for the memorandum’s emphasis on interfaces as behavioral contracts rather than mere connection points.

**[3.3]** Vogler, W., & Lüttgen, G. “A Linear-Time Branching-Time Perspective on Interface Automata.” *Acta Informatica*, 57, 2020, pp. 513–550.  
**Digest:** Shows the maturity of contract and compatibility thinking around interface models.

**[3.4]** Feiler, P. H., Gluch, D. P., & Hudak, J. J. *The Architecture Analysis & Design Language (AADL): An Introduction.* CMU/SEI-2006-TN-011, 2006.  
**Digest:** Important example of an architecture language that can support analysis rather than merely documentation.

---

## 4. Visual Notation, Diagram Cognition, and the Limits of “Pretty Pictures”

**[4.1]** Larkin, J. H., & Simon, H. A. “Why a Diagram Is (Sometimes) Worth Ten Thousand Words.” *Cognitive Science*, 11(1), 1987, pp. 65–100.  
**Digest:** Supports the claim that pictures can change how reasoning is performed, not just how results are displayed.

**[4.2]** Moody, D. L. “The ‘Physics’ of Notations: Toward a Scientific Basis for Constructing Visual Notations in Software Engineering.” *IEEE Transactions on Software Engineering*, 35(6), 2009, pp. 756–779.  
**Digest:** Critical support for treating notation design as an engineering problem with cognitive constraints.

**[4.3]** Green, T. R. G., & Petre, M. “Usability Analysis of Visual Programming Environments: A ‘Cognitive Dimensions’ Framework.” *Journal of Visual Languages & Computing*, 7(2), 1996, pp. 131–174.  
**Digest:** Supports the memorandum’s concern that a formal medium can still fail if humans cannot actually work in it.

**[4.4]** Petre, M. “Why Looking Isn’t Always Seeing: Readership Skills and Graphical Programming.” *Communications of the ACM*, 38(6), 1995, pp. 33–44.  
**Digest:** Important warning that diagrams require learned reading practices and can easily become misleadingly fluent.

---

## 5. Program Synthesis, Model-Driven Execution, and the Shift from Specification to Realization

**[5.1]** Mernik, M., Heering, J., & Sloane, A. M. “When and How to Develop Domain-Specific Languages.” *ACM Computing Surveys*, 37(4), 2005, pp. 316–344.  
**Digest:** Supports the memorandum’s treatment of the target medium as a language-design problem rather than a tool-selection problem.

**[5.2]** Gulwani, S., Polozov, O., & Singh, R. *Program Synthesis.* *Foundations and Trends in Programming Languages*, 4(1–2), 2017, pp. 1–119.  
**Digest:** Supports the broader claim that high-quality realization depends on high-quality specification.

**[5.3]** David, C., & Kroening, D. “Program Synthesis: Challenges and Opportunities.” *Philosophical Transactions of the Royal Society A*, 375(2104), 2017.  
**Digest:** Useful for the specification-to-program problem in general and for the limits of underspecified intent.

**[5.4]** Brambilla, M., Cabot, J., & Wimmer, M. *Model-Driven Software Engineering in Practice.* Morgan & Claypool, 2012.  
**Digest:** Supports the claim that execution from models is already real. The human working medium, which this tradition left unresolved, is what SpecChat addresses.

---

## 6. Productive Struggle, Capability Formation, and the FORCE Question

**[6.1]** Bjork, R. A. “Memory and Metamemory Considerations in the Training of Human Beings.” In *Metacognition: Knowing About Knowing*, 1994.  
**Digest:** Supports the memorandum’s concern that the new medium must preserve productive difficulty rather than eliminating thought.

**[6.2]** Bjork, R. A., & Bjork, E. L. “Making Things Hard on Yourself, But in a Good Way: Creating Desirable Difficulties to Enhance Learning.” In *Psychology and the Real World*, 2011.  
**Digest:** Relevant to the question of whether specification work can still build durable capability.

**[6.3]** Kapur, M. “Productive Failure.” *Cognition and Instruction*, 26(3), 2008, pp. 379–424.  
**Digest:** Supports the argument that struggle can remain developmentally valuable if it is directed at the right kinds of problems.

---

## 7. Automation, Deskilling, and Out-of-the-Loop Risk

**[7.1]** Bainbridge, L. “Ironies of Automation.” *Automatica*, 19(6), 1983, pp. 775–779.  
**Digest:** Still the clearest warning that automation can remove the practice that originally built the human’s skill.

**[7.2]** Endsley, M. R., & Kiris, E. O. “The Out-of-the-Loop Performance Problem and Level of Control in Automation.” *Human Factors*, 37(2), 1995.  
**Digest:** Supports the memorandum’s warning that humans can lose deep engagement when moved into supervisory roles.

**[7.3]** Skitka, L. J., Mosier, K. L., & Burdick, M. “Does Automation Bias Decision-Making?” *International Journal of Human-Computer Studies*, 51(5), 1999, pp. 991–1006.  
**Digest:** Important support for the risk that fluent machine outputs will be trusted too easily.

---

## 8. Collaborative Specification and Workspace Design

**[8.1]** Figma Help Center. “Guide to Components in Figma.”
**Digest:** Reference for the component/instance/library model. SpecChat adopts an analogous pattern: authored components compose into systems, consumed components carry boundary contracts. The collaboration medium is version-controlled text rather than a visual canvas.

**[8.2]** Wallace, E. “How Figma’s Multiplayer Technology Works.” *Figma Blog*, 16 October 2019.
**Digest:** Supports the collaboration requirement: one evolving shared artifact. SpecChat achieves this through `.spec.md` files under version control, where incremental specifications (features, bugs, decisions, amendments) extend a base spec through a governed lifecycle.

**[8.3]** Choudhury, A., Malavolta, I., Ciccozzi, F., Aslam, K., et al. “The Technological Landscape of Collaborative Model-Driven Software Engineering.” *Software and Systems Modeling*, 24, 2025, pp. 1595-1619.
**Digest:** Strong evidence that collaborative MDSE exists but remains fragmented. SpecChat’s approach (text-native specification with projections to code, tests, and documentation) sidesteps the visual toolchain fragmentation problem.

**[8.4]** Jongeling, R., Cicchetti, A., & Ciccozzi, F. “How Are Informal Diagrams Used in Software Engineering? An Exploratory Study of Open-Source and Industrial Practices.” *Software and Systems Modeling*, 24, 2025, pp. 601-613.
**Digest:** Important warning that informal diagrams are widespread and useful, but usually too weak to serve as true specifications. SpecChat chose formal text over diagrams for this reason: the specification must carry semantic commitments, not just visual organization.

**[8.5]** Renger, M., Kolfschoten, G. L., & de Vreede, G.-J. “Evaluation of Collaborative Modeling Processes for Knowledge Articulation and Alignment.” *Information Systems and e-Business Management*, 15, 2017, pp. 717-749.
**Digest:** Supports the claim that collaborative modeling is also a process of collective articulation and alignment, not just artifact production. SpecChat’s guided authoring flow (the `/spec-chat` command) makes this articulation process explicit through staged questions that force the specifier to confront boundaries, invariants, and design rationale.

---

## 9. Markup, Serialization, and the Carrier-vs-Semantics Distinction

**[9.1]** W3C. “Extensible Markup Language (XML) 1.0 Fifth Edition Is a W3C Recommendation.” 2008.  
**Digest:** Supports the paper’s clarification that human-readable and machine-processable interchange is not the novel problem here.

**[9.2]** W3C. *XML Schema Definition Language (XSD) 1.1 Part 1: Structures.* W3C Recommendation, 2012.  
**Digest:** Supports the distinction between structural validation and semantic commitment.

**[9.3]** W3C. *XHTML 1.0: The Extensible HyperText Markup Language (Second Edition).* W3C Recommendation, revised 2002.  
**Digest:** A clean example of a carrier format that transports structure while drawing meaning from a distinct semantic specification.

---

## 10. Architecture Practice and Bodies of Knowledge

**[10.1]** IASA Global. *Business Technology Architecture Body of Knowledge (BTABoK).* https://iasa-global.github.io/btabok/  
**Digest:** Source for the AI-elements derivation in the Practice Question and Shape of a Practice Architecture sections. BTABoK organizes architecture practice into models (Engagement, Value, People, Competency) with specific workflows, roles, cadences, and judgment boundaries. The memorandum draws on BTABoK to identify the concrete elements practice requires from AI and the categorical refusals practice asserts.

---

## Cross-Reference: Memorandum Sections to Citation Categories

| Memorandum Section | Primary Citation Categories |
|---|---|
| The FORCE Problem | 6, 7 |
| The Four Futures and the Specification Hypothesis | 1, 5, 7 |
| Specification-Driven Realization in Practice | 5 |
| The Developmental Question | 1, 6, 7 |
| The Medium Question | 2, 3, 4, 8, 9 |
| What the Medium Must Carry | 1, 2, 3, 8, 9 |
| The Division of Labor | 1, 5, 7 |
| The Danger of Cosmetic Relocation | 6, 7 |
| The Practice Question | 6, 7, 10 |
| The Shape of a Practice Architecture | 6, 7, 10 |
| Existing Traditions | 1, 2, 4, 5, 7 |
| Research Agenda | 1, 2, 3, 4, 6, 8, 10 |
| Conclusion | 1, 2, 5, 6, 7, 10 |

---

## Closing Note

The v0.09 memorandum makes three claims more directly than earlier versions. First: the ability to specify substantial systems for machine realization already exists, now formalized through SpecChat's specification language and conversation protocol. Second: the discipline around the medium can be given an architecture that classifies machine contact by authority, suitability, and interaction pattern, declares categorical stop-lines, and refuses to widen the medium's formal scope, with ASAP as a concrete proposal grounded in the BTABoK. Third, and most consequential: whether specification struggle can build the durable capability that implementation struggle built remains an open empirical question. The medium has a concrete proposal. The practice architecture has a concrete proposal. The developmental question does not.
