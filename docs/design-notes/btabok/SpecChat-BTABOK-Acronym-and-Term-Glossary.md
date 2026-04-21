# SpecChat and BTABOK Acronym and Term Glossary

## Tracking

| Field | Value |
|---|---|
| slug | speclang-btabok-acronym-and-term-glossary |
| itemType | GlossaryDocument |
| name | SpecChat and BTABOK Acronym and Term Glossary |
| version | 0.1.0 |
| publishStatus | Draft |
| authors | dennis@dennislandi.com |
| reviewers | (pending) |
| committer | dennis@dennislandi.com |
| tags | glossary, acronyms, speclang, btabok, codl, cadl, reference |
| createdAt | 2026-04-16 |
| updatedAt | 2026-04-16 |
| retentionPolicy | indefinite |
| freshnessSla | 90 days |
| lastReviewed | 2026-04-16 |
| dependencies | SpecLang-Design.md, Spec-Type-System.md, MCP-Server-Integration-Design.md, Global-Corp-Exemplar.md |

## 1. Purpose

This document is the single reference for acronyms, abbreviations, and terminology used across the SpecChat WIP corpus. Entries are organized by category. Every entry lists the expansion, a one-sentence definition, and the principal doc(s) where the term appears. Readers encountering an unfamiliar acronym in any active WIP doc can look it up here.

## 2. How to Use This Glossary

Entries are grouped by concern: SpecChat internals, BTABOK practice terminology, supply chain and logistics standards, regulatory and security frameworks, organizational roles and titles, Global Corp exemplar identifier prefixes, and general technical terms. A flat alphabetical index follows in Section 10 for quick lookup. Cross-references point to the WIP doc filename and, where practical, a section number. Where a term appears in multiple categories, the primary entry lives in one section and the others cross-reference it.

## 3. SpecChat Internals

**SpecChat**: SpecChat (product name)
The overarching specification-driven authoring and validation system. The specification is the primary engineering artifact. Appears in all four WIP docs.

**SpecLang**: SpecLang (language name)
The core language used to express SpecChat specifications. Covers data, context, systems, deployment, and view/dynamic/design registers. Defined in SpecLang-Design.md.

**CoDL**: Concept Definition Language
A schema and type language for the stored data structure of any BTABoK concept, published by IASA Global on the BTABoK 3.2 education portal in April 2026. Authored by Paul Preiss. Defines primitive types, sections, relationships, and the Standard BTABoK Metadata profile. See SpecLang-Design.md Section 4.1.

**CaDL**: Canvas Definition Language
A visual and rendering language that projects CoDL concepts onto canvases. Published by IASA Global alongside CoDL. A canvas is a view of a concept, not a separate stored object type. See SpecLang-Design.md Section 4.2.

**BTABoKItem**: BTABoK Metadata Profile Item
The CoDL metadata record attached to every BTABoK concept instance under the BTABOK profile. Includes `accessTier`, `bokStatus`, `certainty`, `baseVersion`, and the BTABoK identifier set. See SpecLang-Design.md Section 6.2.

**SpecItem**: Standard SpecItem Metadata
The core Core SpecLang metadata record carried by every spec regardless of profile. Fields include slug, itemType, name, version, publishStatus, authors, reviewers, committer, tags, createdAt, updatedAt, retentionPolicy, freshnessSla, lastReviewed, dependencies. See SpecLang-Design.md Section 5.1.

**MCP**: Model Context Protocol
The protocol used by the SpecChat MCP Server to expose tools (validators, parsers, rendering) to client agents. The server targets `ModelContextProtocol 1.2.0`. See MCP-Server-Integration-Design.md Section 2.

**AST**: Abstract Syntax Tree
The parsed tree representation of a spec document used by analyzers and code generation. See MCP-Server-Integration-Design.md Section 4.2.

**EBNF**: Extended Backus-Naur Form
A notation used to express SpecLang grammar productions. Referenced in SpecLang-Design.md discussions of grammar surface.

**DSL**: Domain-Specific Language
A grammar or dialect dedicated to a domain concern. SpecLang is a DSL; a DSL Extension adds AST nodes and semantic analysis for a specific concept. See Spec-Type-System.md Section 5.1.

**DI**: Dependency Injection
Standard .NET pattern used across the `SpecChat.Language` assembly. See MCP-Server-Integration-Design.md.

**CLI**: Command-Line Interface
The operator-facing tool surface alongside the MCP server. See SpecLang-Design.md and MCP-Server-Integration-Design.md Decision D-09.

**ADR**: Architecture Decision Record
A durable record of an accepted architectural decision. In CoDL terms, realized by the `DecisionRecord` concept. Also appears as a BTABOK practice term; see Section 4.

**profile**: Profile
An optional discipline layered over Core SpecLang that adds profile-specific metadata, concept types, constraints, and validators. Current profiles: Core (default), The Standard, BTABOK. See Spec-Type-System.md Section 3.2.

**spec type**: Spec Type
A first-class classification of what kind of artifact a document is (Manifest, Base System Spec, Decision Spec, etc.). Each spec type corresponds to a CoDL concept. See Spec-Type-System.md Section 4.

**Type Profile**: Type Profile Contract
A deterministic validation contract per spec type covering required sections, required metadata, allowed and required core SpecLang declarations, CoDL concept definition, semantic checks, and pass/fail criteria. See Spec-Type-System.md Section 5.3.

**DSL Extension**: DSL Extension
A grammar and AST extension that introduces new language-surface semantics for a reusable concept. The Standard and candidate BTABOK concepts are examples. See Spec-Type-System.md Section 5.4.

**manifest**: Manifest
The root control document for a spec collection, recording profile, lifecycle states, document type registry, spec inventory, execution order, and conventions. See Spec-Type-System.md Section 4.3.1.

**The Standard**: The Standard (profile name)
An existing SpecChat profile specializing component vocabulary and realization rules. Used as the precedent for profile-driven grammar extensions. See Spec-Type-System.md Section 3.2 and MCP-Server-Integration-Design.md Section 2.

**validator**: Validator
A deterministic check that runs against a spec or a collection and emits diagnostics. Core validators carry the `SPEC-` prefix, Standard validators carry `STANDARD-`, BTABOK validators carry `BTABOK-`. See SpecLang-Design.md Section 5.7.

**diagnostic**: Diagnostic
A structured record produced by a validator: severity, message, location, optional code, validator name, and suggestion. See SpecLang-Design.md Section 5.5 and MCP-Server-Integration-Design.md Section 4.7.

**DiagnosticBag**: DiagnosticBag
The shared collector that every validator in the .NET codebase writes into; serialized at the MCP tool boundary. See MCP-Server-Integration-Design.md Section 2.

**CollectionIndex**: CollectionIndex
A core service that indexes every concept and spec instance in a collection by `(itemType, slug)` to support reference resolution, relationship validation, and cycle detection. See SpecLang-Design.md Section 5.8.

**ValidatorSeverityPolicy**: ValidatorSeverityPolicy
The core service that applies manifest-declared severity overrides and enforces the treat-warnings-as-errors posture. See MCP-Server-Integration-Design.md Section 4.7.

**WorkspaceAnalyzer**: WorkspaceAnalyzer
A new C# class introduced by D-04 Option B to host collection-level analysis. Owns the CollectionIndex and orchestrates core collection validators plus profile-specific collection validators. Distinct from the per-file `SemanticAnalyzer`. See MCP-Server-Integration-Design.md Section 4.6.

**governance posture**: Governance Posture
Manifest-level declaration of `warnings` (default) or `strict`, controlling whether validator warnings are treated as errors. Per D-09 Option D, the manifest sets a floor that the CLI `--strict` flag can escalate but not relax.

**migration mode**: Migration Mode
A transitional manifest state declared via `Profile.migrationFrom: <prior profile>` that temporarily relaxes certain reference-resolution errors to warnings while a collection is being brought into compliance with a new profile. Used during Phase 2a sample migration.

**specLangVersion**: SpecLang Version
Required manifest field (per VD-1 Option B) declaring the Core SpecLang version a collection targets. Semver, starting `0.1.0`.

**profileVersion**: Profile Version
Manifest field declaring the active profile's version. Semver per VD-2 Option A for SpecChat-owned profiles (The Standard, BTABOK). Required when a non-Core profile is declared.

**Phase 1**: Foundation Phase
The first implementation phase of the MCP server integration. Produces the lexer, AST, parser, manifest model, diagnostic record extensions, code registry, and versioning-support MCP tools. See MCP-Server-Integration-Design.md Section 5.

**Phase 2a**: Core Validators Phase
The phase implementing the 10 core validators (`SPEC-` prefix), `CoreMetadataAnalyzer`, and sample collection migration.

**Phase 2b**: Core Infrastructure Phase
The phase implementing `CollectionIndex` and `ValidatorSeverityPolicy` as core services, plus the core diagnostic registry.

**Phase 2c**: BTABOK-Profile Validators Phase
The phase implementing the 13 BTABOK-specific validators (`BTABOK-` prefix) in `BtabokSemanticAnalyzer`.

**SPEC-**: Core Diagnostic Code Prefix
Prefix reserved for Core SpecLang validators. Examples: `SPEC-MET-`, `SPEC-SLUG-`, `SPEC-REF-`, `SPEC-FRS-`, `SPEC-PRF-`, `SPEC-REL-`, `SPEC-VER-`, `SPEC-RET-`.

**STANDARD-**: The Standard Diagnostic Code Prefix
Prefix reserved for The Standard profile validators (broker/foundation/processing/orchestration layer rules, Florance Pattern, Flow Forward, autonomy, entity ownership, vocabulary).

**BTABOK-**: BTABOK Profile Diagnostic Code Prefix
Prefix reserved for BTABOK-profile validators. Examples: `BTABOK-ASR-`, `BTABOK-DEC-`, `BTABOK-PRN-`, `BTABOK-STK-`, `BTABOK-VPT-`, `BTABOK-WVR-`, `BTABOK-GOV-`, `BTABOK-RMP-`, `BTABOK-CNV-`, `BTABOK-MET-`.

**D-**: Architectural Decision Prefix
Prefix for settled architectural decisions. D-01 through D-16 are captured in `SpecChat-Design-Decisions-Record.md`.

**VD-**: Versioning Decision Prefix
Prefix for versioning-policy sub-decisions (VD-1 through VD-5). Captured in `SpecChat-Versioning-Policy.md`.

**SD-**: Sub-Decision Prefix
Prefix for Option-X sub-decisions. SD-01 through SD-03 and SD-05 were folded into D-13, D-14, D-15, D-16. SD-04 (core validator naming convention) survives as the only independent SD-decision.

**OY-**: Option Y Deferral Prefix
Prefix for the seven Option Y absorption candidates (OY-1 through OY-7), all deferred for observation.

**V2-**: v0.2 Deferral Prefix
Prefix for seven items deferred to v0.2 or later (V2-1 through V2-7). V2-6 (diagnostic code reference doc) was promoted to Phase 1 per D-05 settlement.

**slug**: Slug
A URL-safe, lowercase, hyphenated stable identifier for any spec or concept. Uniqueness is enforced within a collection. See SpecLang-Design.md Section 5.6.

**retentionPolicy**: Retention Policy
A core enum covering `indefinite`, `archive-on-deprecated`, and `delete-on-superseded`. Applies to every spec regardless of profile. See SpecLang-Design.md Section 5.4.

**Freshness SLA**: Freshness Service Level Agreement
A declared maximum allowable age between reviews for a spec. Enforced by the `check_freshness_sla` core validator. See SpecLang-Design.md Section 5.4.

**ref**: Strong Reference
A resolved, in-collection reference expressed as `ref<T>`. Must resolve; unresolved refs produce an error diagnostic. See SpecLang-Design.md Section 5.2.

**weakRef**: Weak Reference
A best-effort reference that may resolve outside the collection; unresolved weakRefs produce a warning, not an error.

**externalRef**: External Reference
A URI-based reference to a non-SpecChat artifact (for example Jira, Confluence, Miro).

**Option A**: Option A (settled decision)
The decision that CoDL syntax is canonical for BTABOK-profile concepts, with optional SpecLang-style aliases that desugar to CoDL. See SpecLang-Design.md Section 4.4.

**Option X**: Option X (settled decision)
The decision that absorbs seven infrastructure elements (Standard Metadata Profile core subset, reference types, relationship declarations with cardinality, retention policy, diagnostic code extensions, slug rules, ten core validators) from the BTABOK profile into Core SpecLang. See SpecLang-Design.md Section 5.

**Option Y**: Option Y (deferred absorption candidates)
The set of seven absorption candidates deferred during the Option X work. Each candidate was evaluated for core absorption but held for observation before promotion. The seven items: publishStatus vocabulary alignment with CoDL, canvas as first-class core concept, Decision Spec enrichment (scope, type, method, reversibility), viewpoint templates as core, ASR as generalized core spec type, MetricDefinition in core, ExperimentCard in core. All seven remain deferred as of v0.1 per the Decisions Record's Tier 4 bundle disposition. See SpecLang-Design.md Section 14 and SpecChat-Design-Decisions-Record.md Section 2.5.

**Engagement Model**: BTABOK Engagement Model
The BTABOK practice area covering stakeholders, decisions, governance, deliverables, and lifecycle. SpecChat's BTABOK profile targets this model; Value Model and People Model are explicitly out of scope. See SpecLang-Design.md Section 7 and Global-Corp-Exemplar.md Appendix C.

**Value Model**: BTABOK Value Model
BTABOK practice area covering business case, benefits, and strategic value. Declared out of scope for SpecChat v0.1 BTABOK profile. See Global-Corp-Exemplar.md Appendix C.

**People Model**: BTABOK People Model
BTABOK practice area covering career paths, competency, and organizational structure. Out of scope for SpecChat v0.1.

## 4. BTABOK Practice Terminology

**BTABOK** (also **BTABoK**): Business Technology Architecture Body of Knowledge
The IASA Global body of knowledge for business technology architecture practice. The capitalization `BTABoK` denotes the formal IASA naming; `BTABOK` is the SpecChat internal spelling. See Global-Corp-Exemplar.md Reference R1.

**IASA**: International Association of Software Architects (trading as IASA Global)
The professional body that publishes BTABOK, CoDL, and CaDL. See https://iasa-global.github.io/btabok/.

**ASR**: Architecturally Significant Requirement
A requirement whose realization materially shapes the architecture. Realized in CoDL as `ASRCard`. See Global-Corp-Exemplar.md Section 21.

**ASD**: Architecturally Significant Decision
A decision that materially shapes the architecture. Realized in CoDL as `DecisionRecord`. See Global-Corp-Exemplar.md Section 22.

**ADLC**: Architecture Development Life Cycle
The BTABOK six-stage lifecycle: innovation, strategy, planning, transformation, utilize-and-measure, decommission. See Global-Corp-Exemplar.md Section 12.

**ARB**: Architecture Review Board
The generic BTABOK construct for an architecture governance body. In Global Corp the specific instance is the EARB.

**EARB**: Enterprise Architecture Review Board
The top-level architecture governance board; reviews reference architectures, major decisions, and waivers. See Global-Corp-Exemplar.md Section 25.1 (GOV-EARB).

**ETSC**: Executive Technology Steering Committee
The executive-level committee that approves funding, strategic direction, and major platform bets. See Global-Corp-Exemplar.md Section 25.1 (GOV-ETSC).

**DDC**: Domain Design Council
A BTABOK governance body that defines standards for a specific domain (Data, Integration, Security, Operations, Compliance). See Global-Corp-Exemplar.md Section 25.1 (GOV-DDC-*).

**RAF**: Regional Architecture Forum
A BTABOK governance body that addresses regional legal, operational, and exception-handling concerns. See Global-Corp-Exemplar.md Section 25.1 (GOV-RAF-*).

**RSG**: Repository Stewardship Group
The BTABOK governance body responsible for artifact freshness, taxonomy, and viewpoint catalog curation. See Global-Corp-Exemplar.md Section 25.1 (GOV-RSG).

**NABC**: Need, Approach, Benefits, Competition
The BTABOK business-case framing used to structure strategic propositions. See Global-Corp-Exemplar.md Section 6.

**RACI**: Responsible, Accountable, Consulted, Informed
A decision-rights matrix used to describe role participation in decisions. See Global-Corp-Exemplar.md Section 25.4.

**Viewpoint**: Viewpoint
A reusable template describing audience, concerns answered, and required models. Realized in CoDL as `ViewpointCard`. See Global-Corp-Exemplar.md Section 27.

**View**: View
A concrete instantiation of a viewpoint. Realized in CaDL as a `CanvasDefinition`. See Global-Corp-Exemplar.md Section 28.

**Waiver**: Waiver
An approved, time-bounded exception to a rule, principle, or policy. Realized in CoDL as `WaiverRecord`. See Global-Corp-Exemplar.md Section 26.

**Principle**: Architecture Principle
A durable enterprise rule that guides decisions. Realized in CoDL as `PrincipleCard`. See Global-Corp-Exemplar.md Section 11.

**Transition Architecture**: Transition Architecture
A BTABOK term for a baseline-to-target roadmap increment with defined capability movements. Realized as `TransitionArchitecture` and `RoadmapItem`. See Global-Corp-Exemplar.md Section 30.

**Capability**: Capability
A business or technology capability with baseline and target maturity. Realized as `CapabilityCard`. See Global-Corp-Exemplar.md Section 8.2.

**KPI**: Key Performance Indicator
A measurable outcome metric tracked on the Outcome Scorecard. See Global-Corp-Exemplar.md Section 32.

**TOGAF**: The Open Group Architecture Framework
An alternative enterprise-architecture framework. Referenced for contrast with BTABOK's lighter deliverable set. See Global-Corp-Exemplar.md Section 22.8.

## 5. Supply Chain and Logistics Standards

**GS1**: GS1 (global standards organization)
The international standards organization that publishes identification and event standards for supply chains. See https://www.gs1.org.

**EPCIS**: Electronic Product Code Information Services
A GS1 standard (version 2.0) for capturing and sharing supply-chain visibility events (what, when, where, why, how) across organizations. Version 2.0 adds sensor data support and developer-friendly APIs. See Global-Corp-Exemplar.md Section 3.2.

**CBV**: Core Business Vocabulary
A GS1 companion standard to EPCIS defining business-step codes and disposition codes used within events. See Global-Corp-Exemplar.md Section 33 (STD-01).

**DCSA**: Digital Container Shipping Association
A standards body for container-shipping industry APIs and event models. Publishes Track and Trace standards with common milestone structures. See Global-Corp-Exemplar.md Section 3.3.

**DPP**: Digital Product Passport
A digital identity for products, components, and materials introduced by the EU Ecodesign for Sustainable Products Regulation. See Global-Corp-Exemplar.md Section 3.4.

**LPI**: Logistics Performance Index
A World Bank index measuring reliability, service quality, infrastructure, and border processes across countries. See Global-Corp-Exemplar.md Section 3.1.

**3PL**: Third-Party Logistics
A logistics provider that warehouses or transports goods on behalf of manufacturers, retailers, or shippers. See Global-Corp-Exemplar.md Section 4.1.

**EDI**: Electronic Data Interchange
A legacy structured-message format used by logistics partners. Carriers may still transmit via EDI 214 and similar messages. See Global-Corp-Exemplar.md Section 17.1 and LGY-02.

**AS2**: Applicability Statement 2
A secure transport protocol used for EDI and business-document exchange. See Global-Corp-Exemplar.md Section 17.1.

**SFTP**: Secure File Transfer Protocol
A legacy transport used for batch file exchange with logistics partners. See Global-Corp-Exemplar.md Section 17.1.

**REST**: Representational State Transfer
The dominant API architectural style for partner integrations. See Global-Corp-Exemplar.md Section 17.1.

**JSON**: JavaScript Object Notation
The default payload format for REST APIs in Global Corp integrations and for MCP tool boundaries. Appears in MCP-Server-Integration-Design.md and Global-Corp-Exemplar.md Section 17.1.

**API**: Application Programming Interface
A defined interface exposed by a system for programmatic use. See Global-Corp-Exemplar.md and MCP-Server-Integration-Design.md.

**IoT**: Internet of Things
Sensor-based telemetry (temperature, shock, location) ingested into the platform. See Global-Corp-Exemplar.md Section 17.1.

**ETA**: Estimated Time of Arrival
A predicted arrival time, sourced from carrier signals and platform models. Covered by contract CTR-04. See Global-Corp-Exemplar.md Section 16.3.

**CTE**: Critical Tracking Event
An FSMA 204 term for an event in a food supply chain that must be recorded for traceability. See Global-Corp-Exemplar.md Section 3.4.

**KDE**: Key Data Element
An FSMA 204 term for a required data field attached to a Critical Tracking Event. See Global-Corp-Exemplar.md Section 3.4.

## 6. Regulatory and Security Frameworks

**FSMA**: Food Safety Modernization Act
The U.S. statute whose Section 204 final rule (2023) mandates Critical Tracking Events and Key Data Elements for covered foods, with a compliance date of January 20, 2026. See Global-Corp-Exemplar.md Section 3.4 (STD-06).

**FDA**: U.S. Food and Drug Administration
The U.S. regulator that publishes and enforces FSMA 204. See https://www.fda.gov.

**NIST**: National Institute of Standards and Technology
The U.S. agency that publishes the Cybersecurity Framework (CSF) and SP 1305 quick-start guide for cybersecurity supply chain risk management. See Global-Corp-Exemplar.md Section 33 (STD-04).

**CSF**: Cybersecurity Framework
The NIST framework (version 2.0) covering cybersecurity risk management including supply chain risk. See Global-Corp-Exemplar.md Section 3.5.

**ISO**: International Organization for Standardization
The international body that publishes ISO 28000 and related standards. See https://www.iso.org.

**ISO 28000**: ISO 28000:2022
The ISO standard for security management systems relevant to supply chain security and resilience. See Global-Corp-Exemplar.md Section 3.5 (STD-03).

**EU Ecodesign Sustainable Products Regulation**: EU Ecodesign Sustainable Products Regulation
The European Union regulation that introduces the Digital Product Passport. See Global-Corp-Exemplar.md Section 3.4 (STD-05).

**PII**: Personally Identifiable Information
Personal data subject to regional minimization and residency controls. See Global-Corp-Exemplar.md Section 16.4 (INV-06).

**OAuth**: OAuth 2.1
The delegated authorization standard used for customer identity flows. See Global-Corp-Exemplar.md Section 33 (STD-07).

**OIDC**: OpenID Connect
Identity layer on top of OAuth used for customer and partner identity. See Global-Corp-Exemplar.md Section 33 (STD-07).

**mTLS**: Mutual Transport Layer Security
Certificate-based two-way TLS used for service-to-service and partner calls. See Global-Corp-Exemplar.md Section 33 (STD-08).

**KMS**: Key Management Service
Regional cryptographic key management used for compliance signing and tenant-scoped encryption. See Global-Corp-Exemplar.md Section 28.7.

**SIEM**: Security Information and Event Management
Centralized security monitoring and alerting. Consumed by APP-SO. See Global-Corp-Exemplar.md Section 15.1.

**SRE**: Site Reliability Engineering
The operational discipline covering platform reliability, observability, and incident response. PER-20 is the Platform SRE Lead. See Global-Corp-Exemplar.md Section 18.4.

**OpenTelemetry**: OpenTelemetry
The CNCF observability standard adopted by Global Corp for telemetry. See Global-Corp-Exemplar.md Section 33 (STD-09).

## 7. Organizational Roles and Titles

**CEO**: Chief Executive Officer
Global Corp role PER-05 Sven Lindqvist. See Global-Corp-Exemplar.md Section 9.

**CFO**: Chief Financial Officer
Global Corp role PER-06 Priya Raman.

**COO**: Chief Operating Officer
Global Corp role PER-07 Hiroshi Tanaka.

**CPO**: Chief Product Officer
Global Corp role PER-09 Elena Vasquez.

**CISO**: Chief Information Security Officer
Global Corp role PER-08 Chioma Okafor.

**Chief Architect**: Chief Architect
Global Corp role PER-01 Lena Brandt. Discipline-specific variants include Chief Architect of Data (PER-02), of Integration (PER-03), of Security (PER-04), of Compliance (PER-17), and of Operations (PER-18).

**HR**: Human Resources
Organizational function referenced under enterprise management capability CAP-ENT-01. See Global-Corp-Exemplar.md Section 8.2.

**PMO**: Project Management Office
Organizational function listed under CAP-ENT-01.

**Regional Operations Lead**: Regional Operations Lead
Role responsible for regional execution, customs timing, and SLA practicality. Instances at APAC (PER-12), Americas (PER-13), MEA (PER-14).

**Repository Steward Lead**: Repository Steward Lead
Role responsible for architecture-repository discipline and freshness. PER-16 Thomas Muller. See Global-Corp-Exemplar.md Section 9.

**Head of Regulatory Affairs**: Head of Regulatory Affairs
PER-10 Yuki Nakamura. See Global-Corp-Exemplar.md Section 9.

**Head of Sustainability**: Head of Sustainability
PER-15 Marcus Weber. See Global-Corp-Exemplar.md Section 9.

**Platform SRE Lead**: Platform Site Reliability Engineering Lead
PER-20 Aleksandr Volkov. See Global-Corp-Exemplar.md Section 9.

## 8. Global Corp Exemplar Identifier Prefixes

Each prefix below identifies a category of artifact in the Global Corp exemplar. The ID following the prefix serves as the CoDL `slug` field.

**ASR-**: Architecturally Significant Requirement
Identifies entries in the ASR catalog (ASR-01 through ASR-10). See Global-Corp-Exemplar.md Section 21.

**ASD-**: Architecturally Significant Decision
Identifies entries in the decision registry (ASD-01 through ASD-08). See Global-Corp-Exemplar.md Section 22.

**WVR-**: Waiver
Identifies approved waivers (WVR-01 through WVR-03). See Global-Corp-Exemplar.md Section 26.

**STK-**: Stakeholder
Identifies stakeholder catalog entries (STK-01 through STK-12). See Global-Corp-Exemplar.md Section 10.

**VP-**: Viewpoint
Identifies reusable viewpoints (VP-01 through VP-10). See Global-Corp-Exemplar.md Section 27.

**V-**: View
Identifies concrete views in the View Gallery (V-01 through V-10). See Global-Corp-Exemplar.md Section 28.

**CAP-**: Capability
Identifies business capabilities (CAP-COM-01, CAP-PAR-01, CAP-TRC-01, etc.). See Global-Corp-Exemplar.md Section 8.2.

**P-**: Principle
Identifies architecture principles (P-01 through P-10). See Global-Corp-Exemplar.md Section 11.

**PER-**: Person
Identifies entries in the Personas Directory (PER-01 through PER-20). See Global-Corp-Exemplar.md Section 9.

**BSVC-**: Business Service
Identifies exposed business services (BSVC-01 through BSVC-07). See Global-Corp-Exemplar.md Section 14.2.

**APP-**: Application Domain
Identifies core application domains (APP-CX, APP-PC, APP-EB, APP-TC, APP-OI, APP-CC, APP-SD, APP-ES, APP-DP, APP-SO). See Global-Corp-Exemplar.md Section 15.1.

**ENT-**: Entity
Identifies canonical information objects (ENT-01 through ENT-19). See Global-Corp-Exemplar.md Section 16.1.

**CTR-**: Contract
Identifies defined system contracts (CTR-01 through CTR-05). See Global-Corp-Exemplar.md Section 16.3.

**INV-**: Invariant
Identifies information-architecture invariants (INV-01 through INV-06). See Global-Corp-Exemplar.md Section 16.4.

**DYN-**: Dynamic Interaction Sequence
Identifies runtime interaction diagrams (DYN-01 through DYN-04). See Global-Corp-Exemplar.md Section 20.

**EXP-**: Experiment
Identifies innovation experiment cards (EXP-01 through EXP-03). See Global-Corp-Exemplar.md Section 13.

**LGY-**: Legacy System
Identifies legacy-modernization records (LGY-01, LGY-02). See Global-Corp-Exemplar.md Section 29.

**RSK-**: Risk
Identifies entries in the risk register (RSK-01 through RSK-10). See Global-Corp-Exemplar.md Section 31.

**MET-**: Metric
Identifies metrics on the Outcome Scorecard. Sub-prefixed by category: `MET-BZ-` business, `MET-AR-` architecture, `MET-OP-` operational. See Global-Corp-Exemplar.md Section 32.

**STD-**: Standard
Identifies entries in the Standards Catalog (STD-01 through STD-10). See Global-Corp-Exemplar.md Section 33.

**DEL-**: Deliverable
Identifies entries in the Minimum Durable Deliverable Set (DEL-01 through DEL-12). See Global-Corp-Exemplar.md Section 24.2.

**REP-**: Repository Area
Identifies repository sections (REP-ST, REP-PS, REP-DR, REP-VL, REP-DM, REP-RA, REP-DA, REP-OM). See Global-Corp-Exemplar.md Section 24.1.

**GOV-**: Governance Body
Identifies governance bodies (GOV-ETSC, GOV-EARB, GOV-DDC-*, GOV-RAF-*, GOV-RSG). See Global-Corp-Exemplar.md Section 25.1.

**VS-**: Value Stream
Identifies core value streams (VS-01 through VS-06). See Global-Corp-Exemplar.md Section 8.1.

**GC-**: Global Corp Document Identifier
Document-level identifier prefix used on Global Corp artifacts (GC-EA-001, GC-MCP-001). See Global-Corp-Exemplar.md tracking block and MCP-Server-Integration-Design.md tracking block.

## 9. Technical and Software Engineering Terms

**net10.0**: .NET 10.0
The .NET target framework for the SpecChat MCP Server codebase. See MCP-Server-Integration-Design.md Section 2.

**xUnit**: xUnit.net
The .NET test framework used by `SpecChat.Language.Tests` and `SpecChat.Mcp.Tests`. See MCP-Server-Integration-Design.md Section 2.

**slnx**: .slnx Solution File
The XML-based replacement for legacy `.sln` files; `SpecChat.slnx` is the solution manifest. See MCP-Server-Integration-Design.md Section 2.

**csproj**: C# Project File
The MSBuild project file format for individual C# projects within the solution.

**stdio**: Standard Input/Output
The default MCP transport for the SpecChat server process.

**JSON-RPC**: JSON Remote Procedure Call
The wire protocol carried over stdio for MCP tool invocations.

**UUID**: Universally Unique Identifier
A 128-bit identifier; UUIDv7 is used for time-ordered event IDs. See Global-Corp-Exemplar.md Section 16.2.

**mtime**: Modification Time
The filesystem last-modified timestamp used as a cache-invalidation signal in `CollectionIndex`. See MCP-Server-Integration-Design.md Section 4.6.

**p95**: 95th Percentile
Latency-distribution measurement point. Common SLO target in operational metrics. See Global-Corp-Exemplar.md Section 32.3.

**regex**: Regular Expression
A pattern-matching syntax used for slug format validation and similar checks. See SpecLang-Design.md Section 5.6.

**SHA-256**: SHA-256 Hash
The cryptographic hash used for payload-hash retention on canonical events (INV-01). See Global-Corp-Exemplar.md Section 16.4.

**Semver**: Semantic Versioning
Version-numbering scheme (major.minor.patch). Available as a CoDL primitive type and discussed as a profile-versioning option. See SpecLang-Design.md Section 4.1 and MCP-Server-Integration-Design.md Decision D-12.

**CRM**: Customer Relationship Management
System of record for customer renewal data, source for MET-BZ-02. See Global-Corp-Exemplar.md Section 32.1.

**Mermaid**: Mermaid (diagram syntax)
A text-based diagram syntax used for the view-gallery renderings (flowchart, quadrantChart, gantt, timeline). See Global-Corp-Exemplar.md Section 28.

**Markdown**: Markdown
The authoring format for all SpecChat specs and for CaDL rendered output. Appears across all four WIP docs.

## 10. Acronym Index (Alphabetical)

| Acronym | Full form | Section |
|---|---|---|
| 3PL | Third-Party Logistics | Section 5 |
| ADLC | Architecture Development Life Cycle | Section 4 |
| ADR | Architecture Decision Record | Section 3 |
| API | Application Programming Interface | Section 5 |
| APP- | Application Domain (ID prefix) | Section 8 |
| ARB | Architecture Review Board | Section 4 |
| AS2 | Applicability Statement 2 | Section 5 |
| ASD | Architecturally Significant Decision | Section 4 |
| ASD- | Architecturally Significant Decision (ID prefix) | Section 8 |
| ASR | Architecturally Significant Requirement | Section 4 |
| ASR- | Architecturally Significant Requirement (ID prefix) | Section 8 |
| AST | Abstract Syntax Tree | Section 3 |
| BSVC- | Business Service (ID prefix) | Section 8 |
| BTABOK / BTABoK | Business Technology Architecture Body of Knowledge | Section 4 |
| BTABoKItem | BTABoK Metadata Profile Item | Section 3 |
| CaDL | Canvas Definition Language | Section 3 |
| CAP- | Capability (ID prefix) | Section 8 |
| CBV | Core Business Vocabulary | Section 5 |
| CEO | Chief Executive Officer | Section 7 |
| CFO | Chief Financial Officer | Section 7 |
| CISO | Chief Information Security Officer | Section 7 |
| CLI | Command-Line Interface | Section 3 |
| CoDL | Concept Definition Language | Section 3 |
| COO | Chief Operating Officer | Section 7 |
| CPO | Chief Product Officer | Section 7 |
| CRM | Customer Relationship Management | Section 9 |
| CSF | Cybersecurity Framework | Section 6 |
| csproj | C# Project File | Section 9 |
| CTE | Critical Tracking Event | Section 5 |
| CTR- | Contract (ID prefix) | Section 8 |
| DCSA | Digital Container Shipping Association | Section 5 |
| DDC | Domain Design Council | Section 4 |
| DEL- | Deliverable (ID prefix) | Section 8 |
| DI | Dependency Injection | Section 3 |
| DPP | Digital Product Passport | Section 5 |
| DSL | Domain-Specific Language | Section 3 |
| DYN- | Dynamic Interaction Sequence (ID prefix) | Section 8 |
| EARB | Enterprise Architecture Review Board | Section 4 |
| EBNF | Extended Backus-Naur Form | Section 3 |
| EDI | Electronic Data Interchange | Section 5 |
| ENT- | Entity (ID prefix) | Section 8 |
| EPCIS | Electronic Product Code Information Services | Section 5 |
| ETA | Estimated Time of Arrival | Section 5 |
| ETSC | Executive Technology Steering Committee | Section 4 |
| EXP- | Experiment (ID prefix) | Section 8 |
| externalRef | External Reference | Section 3 |
| FDA | U.S. Food and Drug Administration | Section 6 |
| FSMA | Food Safety Modernization Act | Section 6 |
| GC- | Global Corp Document Identifier (ID prefix) | Section 8 |
| GOV- | Governance Body (ID prefix) | Section 8 |
| GS1 | GS1 global standards organization | Section 5 |
| HR | Human Resources | Section 7 |
| IASA | International Association of Software Architects | Section 4 |
| IoT | Internet of Things | Section 5 |
| INV- | Invariant (ID prefix) | Section 8 |
| ISO | International Organization for Standardization | Section 6 |
| ISO 28000 | ISO 28000:2022 Security Management Systems | Section 6 |
| JSON | JavaScript Object Notation | Section 5 |
| JSON-RPC | JSON Remote Procedure Call | Section 9 |
| KDE | Key Data Element | Section 5 |
| KMS | Key Management Service | Section 6 |
| KPI | Key Performance Indicator | Section 4 |
| LGY- | Legacy System (ID prefix) | Section 8 |
| LPI | Logistics Performance Index | Section 5 |
| MCP | Model Context Protocol | Section 3 |
| MET- | Metric (ID prefix) | Section 8 |
| mTLS | Mutual Transport Layer Security | Section 6 |
| mtime | Modification Time | Section 9 |
| NABC | Need, Approach, Benefits, Competition | Section 4 |
| net10.0 | .NET 10.0 target framework | Section 9 |
| NIST | National Institute of Standards and Technology | Section 6 |
| OAuth | OAuth 2.1 | Section 6 |
| OIDC | OpenID Connect | Section 6 |
| p95 | 95th Percentile | Section 9 |
| P- | Principle (ID prefix) | Section 8 |
| PER- | Person (ID prefix) | Section 8 |
| PII | Personally Identifiable Information | Section 6 |
| PMO | Project Management Office | Section 7 |
| RACI | Responsible, Accountable, Consulted, Informed | Section 4 |
| RAF | Regional Architecture Forum | Section 4 |
| ref | Strong Reference | Section 3 |
| regex | Regular Expression | Section 9 |
| REP- | Repository Area (ID prefix) | Section 8 |
| REST | Representational State Transfer | Section 5 |
| RSG | Repository Stewardship Group | Section 4 |
| RSK- | Risk (ID prefix) | Section 8 |
| Semver | Semantic Versioning | Section 9 |
| SFTP | Secure File Transfer Protocol | Section 5 |
| SHA-256 | SHA-256 Hash | Section 9 |
| SIEM | Security Information and Event Management | Section 6 |
| slnx | .slnx Solution File | Section 9 |
| SRE | Site Reliability Engineering | Section 6 |
| STD- | Standard (ID prefix) | Section 8 |
| stdio | Standard Input/Output | Section 9 |
| STK- | Stakeholder (ID prefix) | Section 8 |
| TOGAF | The Open Group Architecture Framework | Section 4 |
| UUID | Universally Unique Identifier | Section 9 |
| V- | View (ID prefix) | Section 8 |
| VP- | Viewpoint (ID prefix) | Section 8 |
| VS- | Value Stream (ID prefix) | Section 8 |
| weakRef | Weak Reference | Section 3 |
| WVR- | Waiver (ID prefix) | Section 8 |
| xUnit | xUnit.net test framework | Section 9 |

## Appendix A. Term Coverage Report

This appendix lists acronyms first introduced by each source doc, to support audit completeness.

- **SpecLang-Design.md:** SpecLang, CoDL, CaDL, BTABoKItem, SpecItem, Option A, Option X, Option Y, Engagement Model, Value Model, People Model, profile, slug, ref, weakRef, externalRef, retentionPolicy, Freshness SLA, diagnostic, validator, CollectionIndex, ValidatorSeverityPolicy, SRE, KMS, SIEM, Semver.
- **Spec-Type-System.md:** spec type, Type Profile, DSL Extension, manifest, The Standard, DSL, AST, EBNF, CoDL concept, ADR.
- **MCP-Server-Integration-Design.md:** MCP, DI, net10.0, xUnit, slnx, csproj, stdio, JSON-RPC, DiagnosticBag, mtime, CLI, GC- document ID prefix.
- **Global-Corp-Exemplar.md:** ASR, ASD, WVR, STK, VP, V, CAP, P, PER, BSVC, APP, ENT, CTR, INV, DYN, EXP, LGY, RSK, MET (with MET-BZ, MET-AR, MET-OP subcategories), STD, DEL, REP, GOV, VS, GS1, EPCIS, CBV, DCSA, DPP, LPI, 3PL, EDI, AS2, SFTP, REST, JSON, API, IoT, ETA, CTE, KDE, FSMA, FDA, NIST, CSF, ISO 28000, OAuth, OIDC, mTLS, EU Ecodesign Sustainable Products Regulation, PII, CEO, CFO, COO, CPO, CISO, Chief Architect, HR, PMO, Regional Operations Lead, Repository Steward Lead, Head of Regulatory Affairs, Head of Sustainability, Platform SRE Lead, ADLC, NABC, RACI, ARB, EARB, ETSC, DDC, RAF, RSG, Viewpoint, View, Waiver, Principle, Transition Architecture, Capability, KPI, TOGAF, IASA, BTABOK, UUID, SHA-256, OpenTelemetry, CRM, Mermaid, Markdown, p95.

## Appendix B. Source References

### B.1 Active WIP design documents

1. `WIP/SpecLang-Design.md`. Consolidated design covering Core SpecLang, the BTABOK profile, CoDL and CaDL alignment, and the Engagement Model scope.
2. `WIP/Spec-Type-System.md`. Consolidated design covering the spec type taxonomy, rationale, and validation architecture.
3. `WIP/MCP-Server-Integration-Design.md`. Working design for integrating the BTABOK profile into the SpecChat MCP Server C# codebase.
4. `WIP/Global-Corp-Exemplar.md`. The canonical BTABOK-complete worked example.

### B.2 External standards bodies and authoritative sources

1. **IASA Global BTABoK.** https://iasa-global.github.io/btabok/
2. **IASA Education Portal, BTABoK 3.2.** https://education.iasaglobal.org/browse/btabok/3.2/
3. **GS1 EPCIS and CBV.** https://www.gs1.org/standards/epcis
4. **DCSA Track and Trace.** https://dcsa.org/standards/track-and-trace
5. **ISO 28000:2022.** https://www.iso.org/standard/79612.html
6. **NIST Cybersecurity Framework 2.0.** https://www.nist.gov/cyberframework
7. **FDA FSMA Section 204 Final Rule.** https://www.fda.gov/food/food-safety-modernization-act-fsma/fsma-final-rule-requirements-additional-traceability-records-certain-foods
8. **EU Ecodesign for Sustainable Products Regulation.** https://commission.europa.eu/energy-climate-change-environment/standards-tools-and-labels/products-labelling-rules-and-requirements/ecodesign-sustainable-products-regulation_en
9. **World Bank Logistics Performance Index 2023.** https://www.worldbank.org/en/news/press-release/2023/04/21/world-bank-releases-logistics-performance-index-2023
10. **Preiss, Paul. Structured Concept Definition Language. BTABoK 3.2, IASA Global Education Portal (2026).** Authoritative source for CoDL and CaDL.
