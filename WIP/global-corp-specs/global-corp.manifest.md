# Global Corp Platform -- System Manifest

## Tracking

| Field | Value |
|---|---|
| slug | global-corp |
| itemType | CollectionManifest |
| name | Global Corp Platform |
| shortDescription | Root manifest for the Global Corp Supply Chain Visibility and Trust Platform spec collection. Local-simulation-first with Aspire orchestration and Docker-based external gate simulators |
| version | 2 |
| specLangVersion | 0.1.0 |
| publishStatus | Draft |
| retentionPolicy | indefinite |
| freshnessSla | P90D |
| lastReviewed | 2026-04-18 |
| authors | [PER-01 Lena Brandt] |
| reviewers | [PER-11 Anja Petersen] |
| committer | PER-01 Lena Brandt |
| tags | [enterprise-architecture, btabok-profile, supply-chain, local-simulation-first, aspire] |
| createdAt | 2026-04-17T00:00:00Z |
| updatedAt | 2026-04-18T00:00:00Z |
| Dependencies | None (root document) |
| State | Draft |
| Reviewed | |
| Approved | |
| Executed | |
| Verified | |

## Profile

| Field | Value |
|---|---|
| Profile | BTABOK |
| profileVersion | 0.1.0 |
| codlVersion | 0.2 |
| cadlVersion | 0.1 |

Per D-11 Option A (Profile declared in manifest only), this Profile block is the single authoritative declaration of the profile active across the collection. Individual specs do not re-declare the profile; they inherit it from this manifest.

## System

| Field | Value |
|---|---|
| System | Global Corp Platform |
| Base spec | global-corp.architecture.spec.md |
| Target | net10.0 |
| Aspire version | 13.2.x |
| Primary deployment profile | Local Simulation (Aspire AppHost, Docker Desktop) |
| Spec count (authored in this collection) | 21 |
| Spec count (reused from samples collection) | 2 (PayGate, SendGate) |
| Spec count (planned for Phase 2c completion) | 10 |

This manifest governs the specification collection for the Global Corp Platform, a fictional multinational supply chain visibility and trust platform. The collection comprises: the enterprise base system spec (`global-corp.architecture.spec.md`), 10 application-domain subsystem specs (APP-CX through APP-SO), 2 cross-cutting platform specs (`aspire-apphost.spec.md`, `service-defaults.spec.md`), and 8 external gate simulator specs (`carrier-gate`, `wms-gate`, `iot-gate`, `idp-gate`, `customs-gate`, `dpp-registry-gate`, `fsma-gate`, `audit-gate`). Two additional gate specs (`PayGate`, `SendGate`) are reused by reference from the SpecChat samples collection at `src/MCPServer/DotNet/Docs/Specs/`.

The enterprise-level BTABOK concept specs (stakeholders, ASRs, decisions, governance spec, viewpoint catalog, and so on) remain planned for Phase 2c of the SpecChat BTABOK implementation per [`SpecChat-BTABOK-Implementation-Plan.md`](../SpecChat-BTABOK-Implementation-Plan.md) Section 7.3.

The narrative design artifact describing the full enterprise architecture is [`Global-Corp-Exemplar.md`](../Global-Corp-Exemplar.md). That document is prose and not machine-validated; this spec collection is the authoritative, validator-targetable representation.

## Lifecycle States

Every spec document moves through a defined sequence of states. Each transition is recorded in the Tracking block with a date.

| State | Meaning | Tracked by |
|---|---|---|
| Draft | Written, not yet reviewed for correctness | Created date |
| Reviewed | Passed consistency check and validator pass | Reviewed date |
| Approved | Ready to execute; decisions resolved, dependencies satisfied | Approved date |
| Executed | Code implemented or amendments applied to base spec | Executed date |
| Verified | Post-execution confirmation passed | Verified date |

**Rules:**
- States are sequential: Draft, Reviewed, Approved, Executed, Verified.
- A spec cannot skip states.
- Decision specs enter Approved when the recommendation is accepted.
- Feature and bug specs enter Executed when their code implementation is complete.
- Amendment specs enter Executed when their corrections are applied to the base spec.
- Waiver specs enter Approved when the EARB records the approval; they enter Executed when the waiver becomes active; they enter Verified at expiration.
- The base spec enters Executed when it has been used to generate or validate source code.
- Verified requires an independent check (test pass, audit, or review) confirming the execution is correct.

CoDL `publishStatus` interop mapping: SpecChat's Draft, Reviewed, Approved, Executed, Verified map to CoDL's Draft, Review, Approved, Published, Published (with a Verified audit record). See [`SpecChat-Versioning-Policy.md`](../SpecChat-Versioning-Policy.md) Rule 3 and [`CoDL-CaDL-Integration-Notes`](../Archive/CoDL-CaDL-Integration-Notes.md) Section 5.1.

## Tracking Block Convention

Every spec document must contain a Tracking block immediately after the title, before the first content section. The block includes all fields required by Core SpecLang SpecItem metadata per D-13 Option A.

Required fields:

| Field | Type | Purpose |
|---|---|---|
| slug | slug | Stable URL-safe identifier, unique within the collection |
| itemType | shortText | The CoDL concept type (`SystemSpec`, `ASRCard`, `DecisionRecord`, etc.) |
| name | shortText | Primary human-readable label |
| version | integer | Monotonic instance version (author discretion per D-16) |
| specLangVersion | semver | Starts at 0.1.0; collection-wide value declared in this manifest |
| publishStatus | enum | Draft, Reviewed, Approved, Executed, Verified |
| retentionPolicy | enum | indefinite, archive-on-deprecated, delete-on-superseded (default by spec type per D-15) |
| freshnessSla | duration | Required when retentionPolicy is indefinite |
| lastReviewed | date | Required when retentionPolicy is indefinite |
| authors | list<PersonRef> | min 1 |
| reviewers | list<PersonRef> | optional |
| committer | PersonRef | single owning steward |
| tags | list<shortText> | optional |
| createdAt | datetime | creation timestamp |
| updatedAt | datetime | last modification timestamp |
| Dependencies | list<ref> | other specs that must reach Executed state first, or "None" |

**Rules:**
- `slug` matches the filename base (for `app-cx.customer-experience.spec.md` the slug is `app-cx-customer-experience`).
- Retention policy defaults per D-15 Option B: Base and Subsystem specs default to `indefinite`; evolution specs (Feature, Bug, Amendment, Decision, Waiver) default to `archive-on-deprecated`.
- Date fields are set when the corresponding state transition occurs.

## Document Type Registry

Per D-13 Option A (all CoDL-required fields required) and the Manifest Type Registry design, every spec file has a declared `itemType` matching one of the entries below.

| Type | CoDL Concept | Required in Collection | Description |
|---|---|---|---|
| Manifest | CollectionManifest | yes, exactly 1 | Root document binding a spec collection |
| Base System Spec | SystemSpec | yes, exactly 1 | Enterprise base spec describing the system at root level |
| Subsystem Spec | SystemSpec | 10 authored | Application-domain subsystem spec (APP-CX, APP-PC, etc.) |
| Context/Stakeholder Spec | StakeholderCard | planned | Stakeholder catalog with concerns and influence |
| ASR/Quality Spec | ASRCard | planned | Architecturally significant requirements catalog |
| Decision Registry | DecisionRecord | planned | Architecturally significant decisions |
| Governance Spec | GovernanceBody, GovernanceRule | planned | Governance bodies, rules, RACI |
| Waiver Register | WaiverRecord | planned | Approved exceptions to principles and rules |
| Roadmap/Transition Spec | TransitionArchitecture, RoadmapItem | planned | Multi-year roadmap with transition architectures |
| Viewpoint Catalog | ViewpointCard | planned | Reusable viewpoint templates |
| View Gallery | CanvasDefinition | planned | Instantiated views, one per viewpoint |
| Standards Catalog | StandardCard | planned | External and internal standards adopted |
| Legacy Modernization Spec | LegacyModernizationRecord | planned | Decommissioning plans for legacy systems |
| Experiment Spec | ExperimentCard | planned | Innovation experiment cards |
| Scorecard Spec | ScorecardDefinition | planned | Outcome scorecard with metric aggregation |

## Conventions

### Writing Style
- No em-dashes. Use commas, semicolons, colons, or separate sentences.
- No emoticons.
- No purple prose or marketing-type phrases.
- Precise file paths and source references.

### SpecLang Syntax
- Formal declarations (`authored component`, `entity`, `trace`, `contract`, `constraint`, `rationale`) follow the grammar defined in SpecLang-Grammar.md.
- `requires` and `ensures` take expressions; `guarantees` takes prose strings.
- `invariant` requires a name string, colon, and expression.
- `rationale` uses either simple form (`rationale STRING;`) or structured form (`rationale { context ...; decision ...; consequence ...; }`).

### BTABOK Profile Syntax (CoDL and CaDL)
- Concept records use CoDL syntax per [`SpecLang-Design.md`](../SpecLang-Design.md) Section 5.
- Canvas definitions use CaDL syntax per [`SpecLang-Design.md`](../SpecLang-Design.md) Section 6.5.
- Cross-references use `ref<ConceptType>` for same-collection strong references, `weakRef` for cross-collection or pre-resolution references, `externalRef` for non-SpecChat targets (per D-07 Option B).

### Subsystem Specs
- The 10 application-domain specs (APP-CX through APP-SO) are each subsystem specs with their own authored components, entities, contracts, and deployment.
- Subsystems may be built and operated independently of each other; cross-subsystem contracts are declared explicitly.
- The base spec (once authored in Phase 2c) references the subsystems as system nodes, describes the enterprise-scope topology, and holds the collection's invariants that span multiple subsystems.

### Standard BTABoK Metadata (BTABoKItem extension)
- For concept records defined by the BTABOK profile (ASRCard, DecisionRecord, StakeholderCard, etc.), the CoDL Standard BTABoK Metadata extension adds fields beyond the Core SpecItem profile: `accessTier`, `bokStatus`, `certainty`, `baseVersion`, and BTABoK-specific identity fields (topicAreaId, publishedBokId, etc.).
- Authors of BTABOK concept specs populate these fields per the CoDL 0.2 specification.

## Governance Posture

| Field | Value |
|---|---|
| governancePosture | warnings |

Per D-09 Option D (manifest as floor, CLI can escalate), this collection's default validation posture is `warnings`. Individual validator runs may escalate to `strict` via CLI flag; the manifest does not permit de-escalation below `warnings` since `warnings` is already the softest permitted posture.

## Severity Overrides

No per-validator severity overrides declared at this version. Future overrides may appear here per the format:

| Validator | Severity override | Rationale |
|---|---|---|

## Spec Inventory

Per D-13 Option A, every spec in the inventory carries the Core SpecItem metadata. For brevity this table shows the identifying fields; the full metadata lives in each spec file's Tracking block.

### Authored spec files (21)

#### Enterprise base spec (1)

| Filename | itemType | slug | State | Tier | retentionPolicy | freshnessSla | committer | Dependencies |
|---|---|---|---|---|---|---|---|---|
| global-corp.architecture.spec.md | SystemSpec | global-corp-architecture | Draft | 0 | indefinite | P90D | PER-01 Lena Brandt | aspire-apphost, service-defaults |

#### Cross-cutting platform specs (2)

| Filename | itemType | slug | State | Tier | retentionPolicy | freshnessSla | committer | Dependencies |
|---|---|---|---|---|---|---|---|---|
| aspire-apphost.spec.md | SystemSpec | aspire-apphost | Draft | 0 | indefinite | P90D | PER-01 Lena Brandt | global-corp-architecture, service-defaults |
| service-defaults.spec.md | SystemSpec | service-defaults | Draft | 0 | indefinite | P90D | PER-01 Lena Brandt | global-corp-architecture |

#### Gate simulator specs authored in this collection (8)

| Filename | itemType | slug | State | Tier | retentionPolicy | freshnessSla | committer | Dependencies |
|---|---|---|---|---|---|---|---|---|
| carrier-gate.spec.md | SystemSpec | carrier-gate | Draft | 0 | indefinite | P90D | PER-01 Lena Brandt | global-corp-architecture |
| wms-gate.spec.md | SystemSpec | wms-gate | Draft | 0 | indefinite | P90D | PER-01 Lena Brandt | global-corp-architecture |
| iot-gate.spec.md | SystemSpec | iot-gate | Draft | 0 | indefinite | P90D | PER-01 Lena Brandt | global-corp-architecture |
| idp-gate.spec.md | SystemSpec | idp-gate | Draft | 0 | indefinite | P90D | PER-01 Lena Brandt | global-corp-architecture |
| customs-gate.spec.md | SystemSpec | customs-gate | Draft | 0 | indefinite | P90D | PER-01 Lena Brandt | global-corp-architecture |
| dpp-registry-gate.spec.md | SystemSpec | dpp-registry-gate | Draft | 0 | indefinite | P90D | PER-01 Lena Brandt | global-corp-architecture |
| fsma-gate.spec.md | SystemSpec | fsma-gate | Draft | 0 | indefinite | P90D | PER-01 Lena Brandt | global-corp-architecture |
| audit-gate.spec.md | SystemSpec | audit-gate | Draft | 0 | indefinite | P90D | PER-01 Lena Brandt | global-corp-architecture |

#### Subsystem specs (10)

| Filename | itemType | slug | State | Tier | retentionPolicy | freshnessSla | committer | Dependencies |
|---|---|---|---|---|---|---|---|---|
| app-cx.customer-experience.spec.md | SystemSpec | app-cx-customer-experience | Draft | 3 | indefinite | P180D | PER-19 Emma Richardson | app-oi, app-tc, app-es, aspire-apphost, service-defaults, SendGate (reused) |
| app-es.enterprise-services.spec.md | SystemSpec | app-es-enterprise-services | Draft | 0 | indefinite | P180D | PER-01 Lena Brandt | aspire-apphost, service-defaults, PayGate (reused), idp-gate |
| app-pc.partner-connectivity.spec.md | SystemSpec | app-pc-partner-connectivity | Draft | 1 | indefinite | P180D | PER-03 Maria Oliveira | app-es, app-eb, aspire-apphost, service-defaults, carrier-gate, wms-gate, iot-gate |
| app-eb.event-backbone.spec.md | SystemSpec | app-eb-event-backbone | Draft | 1 | indefinite | P180D | PER-02 Arjun Desai | app-es, aspire-apphost, service-defaults |
| app-tc.traceability-core.spec.md | SystemSpec | app-tc-traceability-core | Draft | 2 | indefinite | P180D | PER-02 Arjun Desai | app-eb, app-es, aspire-apphost, service-defaults |
| app-dp.data-platform.spec.md | SystemSpec | app-dp-data-platform | Draft | 2 | indefinite | P180D | PER-02 Arjun Desai | app-eb, app-tc, app-cc, aspire-apphost, service-defaults |
| app-oi.operational-intelligence.spec.md | SystemSpec | app-oi-operational-intelligence | Draft | 3 | indefinite | P180D | PER-18 Kenji Sato | app-eb, app-tc, app-dp, app-es, aspire-apphost, service-defaults |
| app-so.security-operations.spec.md | SystemSpec | app-so-security-operations | Draft | 0 | indefinite | P180D | PER-04 Daniel Park | app-es, aspire-apphost, service-defaults |
| app-cc.compliance-core.spec.md | SystemSpec | app-cc-compliance-core | Draft | 3 | indefinite | P180D | PER-17 Isabelle Laurent | app-tc, app-eb, app-so, app-es, aspire-apphost, service-defaults, customs-gate, fsma-gate, audit-gate |
| app-sd.sustainability-dpp.spec.md | SystemSpec | app-sd-sustainability-dpp | Draft | 4 | indefinite | P180D | PER-15 Marcus Weber | app-tc, app-cc, app-eb, aspire-apphost, service-defaults, dpp-registry-gate |

#### Reused gate specs (from SpecChat samples collection; referenced, not authored here)

| Filename | Location | itemType | slug | Consumed by |
|---|---|---|---|---|
| PayGate.spec.md | src/MCPServer/DotNet/Docs/Specs/ | SystemSpec | paygate | app-es |
| SendGate.spec.md | src/MCPServer/DotNet/Docs/Specs/ | SystemSpec | sendgate | app-cx |

### Planned spec files (10 Phase 2c BTABOK concept specs, not yet authored)

These files do not exist yet. Validator reference-resolution runs against this manifest will produce `weakRef` warnings for references that target these planned specs; such warnings are expected pending Phase 2c completion.

| Filename (planned) | itemType | slug | Expected Tier | Notes |
|---|---|---|---|---|
| global-corp.stakeholders.spec.md | StakeholderCard (collection) | global-corp-stakeholders | 0 | STK-01 through STK-12 |
| global-corp.asrs.spec.md | ASRCard (collection) | global-corp-asrs | 0 | ASR-01 through ASR-10 |
| global-corp.decisions.spec.md | DecisionRecord (collection) | global-corp-decisions | 1 | ASD-01 through ASD-08 |
| global-corp.governance.spec.md | GovernanceBody, GovernanceRule | global-corp-governance | 1 | ETSC, EARB, Domain Design Councils, Regional Architecture Forums, Repository Stewardship Group |
| global-corp.waivers.spec.md | WaiverRecord (collection) | global-corp-waivers | 2 | WVR-01 through WVR-03 |
| global-corp.roadmap.spec.md | TransitionArchitecture, RoadmapItem | global-corp-roadmap | 1 | T1 Foundation, T2 Operational Excellence, T3 Compliance and Sustainability, Target |
| global-corp.viewpoints.spec.md | ViewpointCard (collection) | global-corp-viewpoints | 0 | VP-01 through VP-10 |
| global-corp.views.spec.md | CanvasDefinition (collection) | global-corp-views | 1 | V-01 through V-10 |
| global-corp.standards.spec.md | StandardCard (collection) | global-corp-standards | 0 | STD-01 through STD-10 |
| global-corp.outcome-scorecard.spec.md | ScorecardDefinition | global-corp-outcome-scorecard | 2 | MET-BZ-01..05, MET-AR-01..05, MET-OP-01..06 |

### Other planned spec files (not tied to the collection root but part of Phase 2c)

| Filename (planned) | itemType | slug | Tier | Notes |
|---|---|---|---|---|
| legacy-globaltrack-apac.decommission.spec.md | LegacyModernizationRecord | legacy-globaltrack-apac | 4 | LGY-01 decommissioning plan |
| experiment-cold-chain-fusion.spec.md | ExperimentCard | exp-cold-chain-fusion | 4 | EXP-01 |
| experiment-dpp-electronics.spec.md | ExperimentCard | exp-dpp-electronics | 4 | EXP-02 (proceed to productization) |
| experiment-multimodal-eta.spec.md | ExperimentCard | exp-multimodal-eta | 4 | EXP-03 |

## Execution Order

Specs are grouped into tiers by dependency. All specs in a tier can be executed in parallel. A tier cannot begin until all dependencies from prior tiers have reached Executed state. Local Simulation Profile execution launches via `dotnet run --project GlobalCorp.AppHost` after the Aspire AppHost spec, ServiceDefaults spec, and gate specs are Executed.

### Tier 0a: Platform substrate (no in-collection dependencies)

These platform specs must reach Executed state before any subsystem spec can be executed against the Local Simulation Profile.

1. `global-corp.architecture.spec.md` (enterprise base system spec; declares Package Policy, Aspire Composition, Gate Inventory, and Deployment profiles)
2. `service-defaults.spec.md` (shared cross-cutting library; every subsystem project consumes it)
3. `aspire-apphost.spec.md` (single composition root; launches every project and container)

### Tier 0b: Gate simulators (depend on Tier 0a architecture spec for Package Policy and composition slot)

These gates must reach Executed state (Docker image available locally) before the consuming subsystem spec can launch in the Local Simulation Profile.

1. `carrier-gate.spec.md` (consumed by app-pc)
2. `wms-gate.spec.md` (consumed by app-pc)
3. `iot-gate.spec.md` (consumed by app-pc; depends on shared mosquitto container)
4. `idp-gate.spec.md` (consumed by app-es federation path)
5. `customs-gate.spec.md` (consumed by app-cc)
6. `dpp-registry-gate.spec.md` (consumed by app-sd)
7. `fsma-gate.spec.md` (consumed by app-cc)
8. `audit-gate.spec.md` (consumed by app-cc)
9. `PayGate.spec.md` (reused from samples; consumed by app-es billing)
10. `SendGate.spec.md` (reused from samples; consumed by app-cx notification engine)

### Tier 0c: Foundation Subsystems (depend on Tier 0a; consumed by every other subsystem)

1. `app-es.enterprise-services.spec.md` (identity, tenant, billing; every other subsystem depends on it)
2. `app-so.security-operations.spec.md` (security and observability; consumed by every subsystem for telemetry)
3. `global-corp.stakeholders.spec.md` (planned)
4. `global-corp.asrs.spec.md` (planned)
5. `global-corp.viewpoints.spec.md` (planned)
6. `global-corp.standards.spec.md` (planned)

### Tier 1: Ingestion and Planning (depend on Tier 0c)

1. `app-pc.partner-connectivity.spec.md` (depends on app-es, app-eb, carrier-gate, wms-gate, iot-gate)
2. `app-eb.event-backbone.spec.md` (depends on app-es; feeds all downstream data)
3. `global-corp.decisions.spec.md` (planned; references ASRs from Tier 0c)
4. `global-corp.governance.spec.md` (planned; references stakeholders and decisions)
5. `global-corp.roadmap.spec.md` (planned; references capabilities and decisions)
6. `global-corp.views.spec.md` (planned; requires viewpoints from Tier 0c)

### Tier 2: Data and Aggregation (depend on Tier 1)

1. `app-tc.traceability-core.spec.md` (depends on app-eb, app-es; graph store on Apache AGE)
2. `app-dp.data-platform.spec.md` (depends on app-eb, app-tc, and app-cc for compliance snapshots; MinIO + DuckDB)
3. `global-corp.waivers.spec.md` (planned; references principles and governance rules)
4. `global-corp.outcome-scorecard.spec.md` (planned; aggregates MET-XX metrics)

### Tier 3: Customer-Facing and Compliance (depend on Tier 2)

1. `app-oi.operational-intelligence.spec.md` (depends on app-eb, app-tc, app-dp, app-es)
2. `app-cx.customer-experience.spec.md` (depends on app-oi, app-tc, app-es, SendGate reused)
3. `app-cc.compliance-core.spec.md` (depends on app-tc, app-eb, app-so, app-es, customs-gate, fsma-gate, audit-gate)

### Tier 4: Regulatory Layer and Long-Horizon Items (depend on Tier 3)

1. `app-sd.sustainability-dpp.spec.md` (depends on app-tc, app-cc, app-eb, dpp-registry-gate)
2. `legacy-globaltrack-apac.decommission.spec.md` (planned)
3. `experiment-cold-chain-fusion.spec.md` (planned)
4. `experiment-dpp-electronics.spec.md` (planned)
5. `experiment-multimodal-eta.spec.md` (planned)

## Relationships to BTABOK Concepts

This manifest itself is a CoDL concept instance of type `CollectionManifest`. Its relationships:

```codl
relationships {
  uses<StakeholderCard>      as stakeholders      cardinality(0..*) via weakRef
  uses<ASRCard>              as requirements      cardinality(0..*) via weakRef
  uses<DecisionRecord>       as decisions         cardinality(0..*) via weakRef
  uses<WaiverRecord>         as waivers           cardinality(0..*) via weakRef
  uses<ViewpointCard>        as viewpoints        cardinality(0..*) via weakRef
  uses<GovernanceBody>       as governanceBodies  cardinality(0..*) via weakRef
  uses<StandardCard>         as standards         cardinality(0..*) via weakRef
  contains<SystemSpec>       as subsystems        cardinality(1..*)
}
```

The `via weakRef` annotations reflect that the targeted concept records live in planned specs (Phase 2c). Once those specs are authored, the relationships can be promoted to `ref<T>` (strong, same-collection references).

## Validator Diagnostic Expectations (pre-Phase-2c)

Running the core validators (SPEC- prefix) against this collection as-is should produce:

- **Expected errors**: none for the 10 authored subsystem specs when run in isolation with `governancePosture: warnings`.
- **Expected warnings**:
  - `SPEC-REF-002` warnings for every `weakRef` that points to a planned-but-unauthored enterprise-level spec.
  - `SPEC-FRS-001` warnings possible for specs whose `lastReviewed + freshnessSla` will appear stale once real usage passes the SLA window.
- **Expected info**:
  - `SPEC-RET-001` info diagnostics on any spec that did not explicitly declare retentionPolicy (relies on the per-type default from D-15).

Running the BTABOK-profile validators (BTABOK- prefix) against this collection as-is will produce many `SPEC-REF-002` warnings because ASRCard, DecisionRecord, StakeholderCard, and other BTABOK concept instances live in planned specs that do not exist yet. These are expected and resolve when Phase 2c completes the collection.

## Open Items

- **Phase 2c completion**: 10 enterprise-level BTABOK concept specs (stakeholders, ASRs, decisions, governance, waivers, roadmap, viewpoints, views, standards, outcome scorecard) plus 4 additional planned specs (legacy modernization, 3 experiments) need authoring to round out the collection. Per [`SpecChat-BTABOK-Implementation-Plan.md`](../SpecChat-BTABOK-Implementation-Plan.md) task group E-2c.
- **Samples relocation**: Once the collection is validator-clean, move the entire `WIP/global-corp-specs/` folder to `src/MCPServer/DotNet/samples/global-corp/` to match the existing samples convention (blazor-harness, TodoApp, PizzaShop, todo-app-the-standard).
- **Validator run infrastructure**: Validators do not exist until Phase 2a ships. Until then, this manifest serves as a design artifact rather than an operational input.
- **Sample migration coordination**: The existing samples (blazor-harness, TodoApp, PizzaShop, todo-app-the-standard) will also be migrated to the Core SpecLang post-Option-X metadata requirements during Phase 2a. Their manifests will receive similar treatment to this one, though they use Core or TheStandard profile rather than BTABOK.
- **Gate image build pipeline**: Each of the 8 new gate specs defines its own Docker image (`globalcorp/<name>-gate:latest`). A shared build script or per-gate CI pipeline needs to produce and tag these images into the local Docker cache before the Aspire AppHost can launch. Candidate: `tools/build-gate-images.ps1` that iterates every gate solution.
- **Reused gate cross-collection path**: `PayGate.spec.md` and `SendGate.spec.md` live under `src/MCPServer/DotNet/Docs/Specs/` (their historical SpecChat samples location). Subsystem specs that reference them use relative paths. Decide whether to hard-copy them into this collection or keep the cross-collection reference pattern as-is. Default: keep cross-collection reference; review after samples relocation.
- **Aspire version drift**: The collection targets Aspire `13.2.x`. As Aspire ships new minor versions, some subsystem NuGets may require a bump. Per SpecChat Versioning Policy Rule 3 (warnings-first compatibility), version drift surfaces warnings rather than blocks.

## Source References

- Parent exemplar narrative: [`Global-Corp-Exemplar.md`](../Global-Corp-Exemplar.md)
- Language design: [`SpecLang-Design.md`](../SpecLang-Design.md)
- Versioning policy: [`SpecChat-Versioning-Policy.md`](../SpecChat-Versioning-Policy.md)
- Settled decisions: [`SpecChat-Design-Decisions-Record.md`](../SpecChat-Design-Decisions-Record.md)
- Implementation plan: [`SpecChat-BTABOK-Implementation-Plan.md`](../SpecChat-BTABOK-Implementation-Plan.md)
- MCP server integration: [`MCP-Server-Integration-Design.md`](../MCP-Server-Integration-Design.md)
- Glossary: [`SpecChat-BTABOK-Acronym-and-Term-Glossary.md`](../SpecChat-BTABOK-Acronym-and-Term-Glossary.md)
- Out-of-scope boundary: [`BTABOK-Out-of-Scope-Models.md`](../BTABOK-Out-of-Scope-Models.md)
