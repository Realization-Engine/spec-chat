# SpecChat Design Decisions Record

## Tracking

| Field | Value |
|---|---|
| Document ID | GC-DDR-001 |
| itemType | DecisionsRecord |
| slug | specchat-design-decisions-record |
| Version | 0.1.0 |
| Created | 2026-04-17 |
| Last Reviewed | 2026-04-17 |
| State | Draft |
| publishStatus | Draft |
| retentionPolicy | indefinite |
| Freshness SLA | 180 days |
| Owner | PER-01 Lena Brandt, Chief Architect |
| Approver | PER-11 Anja Petersen, Chair EARB |
| Dependencies | SpecLang-Design.md, MCP-Server-Integration-Design.md, SpecChat-Versioning-Policy.md |

## 1. Purpose

This document is the authoritative record of every settled architectural decision for SpecChat as of 2026-04-17. It exists as a single reference so that implementers, reviewers, and future designers can see what was decided, what option was chosen, and why, without re-reading the design docs end to end.

Each entry lists the decision identifier, the chosen option, a one-sentence rationale, and a pointer to the design doc where the decision's full implications live.

This record covers 22 individual decisions plus 14 bundle dispositions. It is the output of a single decision-settling pass that worked through every open item in the WIP corpus in sequence.

## 2. Summary Matrix

### 2.1 Tier 1: Pre-Phase-1 Architectural Decisions

| ID | Decision | Chosen option | One-line rationale |
|---|---|---|---|
| D-01 | Co-locate BTABOK in SpecChat.Language or split project | A | Match The Standard precedent; co-locate for lowest friction at pre-1.0. |
| D-03 | Reserved keywords versus contextual identifiers | B | Reserve only top-level block starters; field-level tokens are contextual. |
| D-04 | Collection-level analysis entry point | B | New `WorkspaceAnalyzer` class for collection scope; `SemanticAnalyzer` stays per-file. |
| D-11 | Profile declaration placement | A | Manifest only; manifest is the authoritative entry point. |
| D-13 | Core metadata required fields strictness | A | All CoDL-required fields required immediately; migration mode handles transition. |
| D-14 | PersonRef source | A | Inline in every spec; honor CoDL's native PersonRef type. |
| D-15 | Retention policy default when unspecified | B | Default by spec type (ever-green for base, archive-on-deprecated for evolution) with `SPEC-RET-001` info diagnostic. |

### 2.2 Tier 2: Other Architectural Decisions

| ID | Decision | Chosen option | One-line rationale |
|---|---|---|---|
| D-02 | CaDL rendering location | A | Co-locate in `SpecChat.Language/Rendering/`; consistent with D-01, splittable later. |
| D-05 | Diagnostic code governance | B | Backfill existing validators opportunistically; build code registry during Phase 1. |
| D-06 | Tool set growth | A | One MCP tool per validator plus one `validate_collection` convenience wrapper. |
| D-07 | Cross-profile reference policy | B | Cross-collection uses `weakRef`; `ref<T>` is same-collection only. |
| D-08 | CaDL rendering output targets | A | Markdown plus Mermaid only for v0.1; JSON and HTML deferred to v0.2+. |
| D-09 | Treat-warnings-as-errors trigger surface | D | Manifest as floor with CLI-only escalation; no de-escalation allowed. |
| D-10 | CollectionIndex caching invalidation | B | Cache by file mtime; `clear_cache` MCP tool for explicit refresh. |
| D-12 | Versioning of BTABOK profile | B | Semver (`0.1.0`); settled by SpecChat Versioning Policy Rule 2. |
| D-16 | CollectionIndex initialization timing | A | Lazy build on first validator invocation; cache thereafter. |

### 2.3 Tier 3: Versioning Policy Sub-Decisions

| ID | Decision | Chosen option | One-line rationale |
|---|---|---|---|
| VD-1 | Core SpecLang version declaration | B | Explicit `specLangVersion` field in manifest; starting value `0.1.0`. |
| VD-2 | Profile version scheme | A | Semver for all SpecChat-owned profiles; external (CoDL, CaDL) retain upstream scheme. |
| VD-3 | Compatibility declaration | D | Tolerate with warnings; strict mode available via escalation. |
| VD-4 | Migration policy | B | Opt-in migration with deprecation schedule; soft sunset path. |
| VD-5 | Instance version evolution | D | Author discretion with soft monotonicity check (`SPEC-VER-005`). |

### 2.4 Tier 4: Naming and Convention

| ID | Decision | Chosen option | One-line rationale |
|---|---|---|---|
| SD-04 | Core validator naming convention | C | Keep `check_` prefix for all validators; consistent with existing codebase. |

### 2.5 Tier 5: Option Y Deferrals (All Kept Deferred)

| ID | Item | Disposition |
|---|---|---|
| OY-1 | `publishStatus` vocabulary alignment to CoDL | Kept deferred |
| OY-2 | Canvas as first-class core concept | Kept deferred |
| OY-3 | Decision Spec enrichment in core | Kept deferred |
| OY-4 | Viewpoint templates in core | Kept deferred |
| OY-5 | ASR as generalized core spec type | Kept deferred |
| OY-6 | MetricDefinition in core | Kept deferred (Value Model territory) |
| OY-7 | ExperimentCard in core | Kept deferred (Value Model territory) |

### 2.6 Tier 6: v0.2 Deferrals

| ID | Item | Disposition |
|---|---|---|
| V2-1 | Profile composition (multiple profiles in one collection) | Kept deferred to v0.2+ |
| V2-2 | Value Model profile | Kept deferred (out of scope) |
| V2-3 | People Model profile | Kept deferred (out of scope) |
| V2-4 | Bidirectional CoDL export | Kept deferred |
| V2-5 | Transport envelope in CLI | Kept deferred (bundled with V2-4) |
| V2-6 | Diagnostic code reference document | **Promoted to Phase 1** per D-05 |
| V2-7 | Additional canvases beyond the v0.1 catalog | Kept deferred |

## 3. Tier 1 Detail: Pre-Phase-1 Architectural Decisions

These seven decisions are settled before any C# code is written for the BTABOK profile integration.

### D-01: Co-locate BTABOK in SpecChat.Language or split project

**Chosen**: Option A (co-locate).

**Rationale**: The Standard extension already demonstrates co-location works at this codebase scale. Splitting to a separate project adds solution complexity without a current payoff. Pre-1.0 posture favors minimal structural commitments.

**Scope affected**: [MCP-Server-Integration-Design.md](MCP-Server-Integration-Design.md) Section 4.

### D-03: Reserved keywords versus contextual identifiers

**Chosen**: Option B (top-level reserved, field-level contextual).

**Rationale**: Ten words become globally reserved (`profile`, `concept`, `canvas`, `section`, `meta`, `relationships`, `storage`, `envelope`, `item`, `area`). Section modifiers, relationship verbs, primitive types, and CaDL directives remain contextual to minimize collision with existing spec identifiers. A keyword collision audit confirms the reserved ten are safe during Phase 1 implementation.

**Scope affected**: [MCP-Server-Integration-Design.md](MCP-Server-Integration-Design.md) Section 4.1.

### D-04: Collection-level analysis entry point

**Chosen**: Option B (new `WorkspaceAnalyzer` class).

**Rationale**: `SemanticAnalyzer` stays focused on single-file analysis. `WorkspaceAnalyzer` becomes the orchestrator for collection-level work (referenced as a new class in Phase 2b). `CollectionIndex` is owned by `WorkspaceAnalyzer`. Delegation from `WorkspaceAnalyzer` to `SemanticAnalyzer` for per-file checks mirrors the pattern already used for The Standard extension.

**Scope affected**: [MCP-Server-Integration-Design.md](MCP-Server-Integration-Design.md) Sections 4.5, 4.6.

### D-11: Profile declaration placement

**Chosen**: Option A (manifest only).

**Rationale**: The manifest is already the authoritative entry point for the collection. Profile declaration fits there without requiring per-spec profile annotations. The Standard's existing `architecture TheStandard` spec-level declaration can be grandfathered on its existing form or migrated to manifest-level as convenient.

**Scope affected**: [SpecLang-Design.md](SpecLang-Design.md) Section 3.

### D-13: Core metadata required fields strictness

**Chosen**: Option A (all CoDL-required fields required immediately).

**Rationale**: Pre-1.0 has no legacy to grandfather. CoDL canonicality (Option A of the earlier CoDL architecture decision) implies honoring CoDL's required flags. The `migrationFrom` manifest field handles the transition for existing sample collections during Phase 2a without permanently relaxing strictness.

**Scope affected**: [SpecLang-Design.md](SpecLang-Design.md) Section 5.1; Phase 2a sample migration.

### D-14: PersonRef source

**Chosen**: Option A (inline in every spec).

**Rationale**: Honors CoDL's native inline `PersonRef` type. No SpecChat-specific `Person` concept invented. Directory conventions (like Global Corp's Personas Directory) remain a prose concern that validators do not formally resolve. Option C (hybrid with optional directory) can be added as a v0.2 additive feature without breaking v0.1 specs.

**Scope affected**: [SpecLang-Design.md](SpecLang-Design.md) Section 5.1.

### D-15: Retention policy default when unspecified

**Chosen**: Option B (default by spec type).

**Rationale**: Base System Spec, Manifest, Subsystem Spec, Governance Spec, Viewpoint Catalog, Standards Catalog, Principles, Risks default to `indefinite`. Feature Spec, Bug Spec, Amendment Spec, Decision Record, Waiver Record, Roadmap Item, Experiment Card, Legacy Modernization Record default to `archive-on-deprecated`. The validator emits an info-level `SPEC-RET-001` diagnostic noting the default used. Authors can override explicitly.

**Scope affected**: [SpecLang-Design.md](SpecLang-Design.md) Section 5.4; core validator logic.

## 4. Tier 2 Detail: Other Architectural Decisions

### D-02: CaDL rendering location

**Chosen**: Option A (co-locate in `SpecChat.Language/Rendering/`).

**Rationale**: Consistent with D-01 co-location. CaDL rendering at v0.1 (20 canvases, Markdown plus Mermaid only per D-08) fits comfortably in the language project. Extraction to `SpecChat.Rendering` is a mechanical refactor available later if rendering grows beyond comfortable.

**Note**: SpecLang views and CaDL canvases are distinct concerns. SpecLang views are authored Mermaid; CaDL canvases are rendering templates over CoDL concept data. Both produce Mermaid output, but only CaDL canvases need a rendering pipeline.

### D-05: Diagnostic code governance

**Chosen**: Option B (require codes going forward, backfill opportunistically).

**Rationale**: New SPEC-, STANDARD-, and BTABOK-prefixed validators always emit codes. Existing validators get codes backfilled as they are touched for other reasons. The code registry (both as a C# static class and as a markdown reference doc) is built during Phase 1. This decision promotes V2-6 (previously deferred) into a Phase 1 deliverable.

### D-06: Tool set growth

**Chosen**: Option A (one tool per validator) plus `validate_collection` wrapper.

**Rationale**: Matches existing codebase convention (every current MCP tool is per-validator or per-concern). Diagnostic codes are one-to-one with validators, which pairs cleanly with one-to-one tool mapping. The `validate_collection` wrapper provides the "run everything" path without imposing a dispatcher pattern. Tool count revisited in future versions if it approaches 80.

### D-07: Cross-profile reference policy

**Chosen**: Option B (weakRef for cross-collection; `ref<T>` is same-collection only).

**Rationale**: `CollectionIndex` boundary is the current collection. `ref<T>` requires local resolution. Any reference outside the current collection uses `weakRef` regardless of whether the target is in the same profile or a different profile. Preserves `ref<T>` semantic clarity.

### D-08: CaDL rendering output targets

**Chosen**: Option A (Markdown plus Mermaid only for v0.1).

**Rationale**: Every v0.1 canvas targets one of these two formats natively. MCP clients render both. No current consumer is waiting on JSON or HTML. Adding a format later is a one-renderer-class addition with a registry entry.

### D-09: Treat-warnings-as-errors trigger surface

**Chosen**: Option D (manifest as floor, CLI can escalate, no de-escalation).

**Rationale**: Collection authors declare minimum governance stance in the manifest (`governancePosture: strict` or `warnings`). CLI `--strict` flag escalates further for specific runs (CI tightening). Relaxation is not supported; the CLI cannot downgrade a strict collection to permissive. Matches the "governed, not governing" principle while allowing operator escalation for specific scenarios.

### D-10: CollectionIndex caching invalidation

**Chosen**: Option B (mtime-based caching) plus `clear_cache` MCP tool.

**Rationale**: Standard pattern for tools of this kind. Near-zero overhead on warm calls. Mtime anomalies (rare but possible) are addressable via the explicit `clear_cache` escape hatch. Content hashing (Option C) is overkill; explicit invalidation (Option D) is operator-unfriendly.

### D-12: Versioning of BTABOK profile

**Chosen**: Option B (semver, starting `0.1.0`).

**Rationale**: Settled as a consequence of the SpecChat Versioning Policy (VD-2 Rule 2: all SpecChat-owned profiles use semver). The Standard migrates from `"1.0"` to `1.0.0` during Phase 2a.

### D-16: CollectionIndex initialization timing

**Chosen**: Option A (lazy build on first validator invocation; cache thereafter).

**Rationale**: Pay for what you use. Tools that do not need the collection index (single-file parse, tracking metadata lookup) do not pay the build cost. Tools that need it pay once per session per collection. Amortization across typical MCP usage patterns keeps the first-call cost acceptable.

### SD-04: Core validator naming convention

**Chosen**: Option C (keep `check_` prefix for all validators).

**Rationale**: The existing codebase uses `check_` for every validator (`check_topology`, `check_traces`, `check_lifecycle`, `check_standard_layers`, and so on). Continuing the pattern avoids a refactor of existing validator names and keeps the codebase internally consistent. Diagnostic-code prefixes (`SPEC-`, `STANDARD-`, `BTABOK-`) already distinguish validator provenance; the tool name does not need to duplicate that distinction.

**Scope affected**: `SpecChat.Language/` and `SpecChat.Mcp/Tools/` throughout. No refactor needed.

## 5. Versioning Policy Summary

The five VD decisions are captured in detail in [SpecChat-Versioning-Policy.md](SpecChat-Versioning-Policy.md). The net effect is five policy rules:

1. **Core SpecLang version declared in manifest** (`specLangVersion`, semver, starting `0.1.0`).
2. **All SpecChat-owned profiles use semver**; externally-owned (CoDL, CaDL) retain their upstream scheme.
3. **Version compatibility is warnings-first**; strict enforcement is available via escalation.
4. **Migration is opt-in with a deprecation schedule**; soft sunset path.
5. **Instance version evolution is author discretion** with soft monotonicity check.

All version-related diagnostics use the `SPEC-VER-` prefix.

## 6. What Changed as Consequences

Some settled decisions had direct consequences on other items in the WIP corpus or the implementation plan. Those consequences are captured here to prevent them from being lost.

### Items promoted from deferral

| Item | Reason | New status |
|---|---|---|
| V2-6 Diagnostic code reference doc | D-05 Option B obligates Phase 1 delivery | Phase 1 deliverable |

### Items with new specific deliverables

| Source decision | New deliverable |
|---|---|
| D-04 (WorkspaceAnalyzer) | New `SpecChat.Language/WorkspaceAnalyzer.cs` class at Phase 2b |
| D-05 (diagnostic codes) | New `SpecChat.Language/DiagnosticCodes.cs` static class at Phase 1; markdown registry doc during Phase 1 |
| D-06 (validate_collection) | New `validate_collection` MCP tool at Phase 2a |
| D-10 (clear_cache) | New `clear_cache` MCP tool at Phase 2b |
| D-13 (metadata strictness) + D-15 (retention defaults) | Core validator `check_metadata_completeness` at Phase 2a |
| VD-1 (specLangVersion) | `specLangVersion` field added to `ManifestDocument` record at Phase 1 |
| VD-3 (compatibility warnings) | Core validator `check_version_compatibility` at Phase 2a |
| VD-4 (deprecation schedule) | New `get_deprecation_schedule` MCP tool at Phase 1; `get_supported_versions` MCP tool at Phase 1 |
| VD-5 (instance monotonicity) | Core validator `check_version_monotonicity` at Phase 2a |

### Sample collection migration obligations (Phase 2a)

- `blazor-harness` manifest and specs gain `specLangVersion: 0.1.0`, `Profile: Core`, and the full Core SpecItem metadata profile.
- `TodoApp` manifest and specs gain the same.
- `PizzaShop` manifest and specs gain the same.
- `todo-app-the-standard` manifest and specs gain `Profile: TheStandard`, `profileVersion: 1.0.0`, and the full Core SpecItem metadata profile. The existing `architecture TheStandard` declaration stays (grandfathered) or is migrated to manifest-level at implementer's convenience.

### New documents produced in this decision pass

Two documents were authored as part of settling these decisions:

- [SpecChat-Versioning-Policy.md](SpecChat-Versioning-Policy.md): Captures VD-1 through VD-5 as formal policy.
- [SpecChat-Design-Decisions-Record.md](SpecChat-Design-Decisions-Record.md): This document.

## 7. Implementation Sequence After This Pass

The Phase 1 through Phase 6 sequence from [MCP-Server-Integration-Design.md](MCP-Server-Integration-Design.md) Section 5 remains authoritative. These settled decisions remove the "pre-Phase-1 decisions" blockers noted in that doc. Implementation can now proceed to Phase 1 without further architectural settlement.

Phase 1 scope (foundation) now includes:
- Lexer additions per D-03 (ten reserved words, field-level tokens contextual)
- AST additions per D-01 (co-located) and D-04 (`WorkspaceAnalyzer` stub)
- Parser.Btabok.cs partial class
- Diagnostic record extension with `Code`, `Validator`, `Suggestion` fields per D-05
- Manifest model with `specLangVersion`, `profileVersion`, `codlVersion`, `cadlVersion` per VD-1 and VD-2
- `DiagnosticCodes.cs` static class with SPEC-, STANDARD-, BTABOK-, and SPEC-VER- namespaces per D-05
- Diagnostic code reference markdown doc per V2-6 promotion
- `get_supported_versions` and `get_deprecation_schedule` MCP tools per VD-3 and VD-4

Phase 2a scope (core validators) now includes:
- `check_metadata_completeness` per D-13
- `check_slug_uniqueness`, `check_slug_format` per Option X
- `check_reference_resolution`, `check_weakref_resolution`, `check_externalref_validity` per Option X, consistent with D-07
- `check_freshness_sla` per Option X
- `check_profile_composition` per Option X and D-11
- `check_relationship_cardinality`, `check_supersedes_cycles` per Option X
- `check_version_compatibility` per VD-3
- `check_version_monotonicity` per VD-5
- `validate_collection` convenience wrapper per D-06
- Sample collection migration per D-13 and Section 6 of this record

Phases 2b through 6 continue as specified in the integration design.

## 8. Open Items After This Pass

Nothing architectural remains open. Every decision in the WIP corpus has been addressed. Future open items fall into two categories:

1. **v0.2+ deferrals** as enumerated in Section 2.6. These are not blockers for v0.1.
2. **Implementation-discovered decisions** that may arise during Phase 1 through Phase 6. Examples: specific diagnostic message text, CLI flag names beyond `--strict`, test fixture structure. These are implementation decisions, not architectural ones, and can be made by implementers.

If architectural questions surface during implementation (not foreseen here), they are added to this record as they arise and settle via the same option-analysis pattern.

## 9. Cross-Reference Index

### Decisions affecting Core SpecLang

D-03, D-04, D-05, D-07, D-10, D-14, D-15, D-16, VD-1, VD-3, VD-4, VD-5, SD-04.

### Decisions affecting the BTABOK profile specifically

D-01, D-02, D-06, D-08, D-11, D-12, D-13, VD-2.

### Decisions affecting the MCP tool layer

D-01, D-02, D-04, D-05, D-06, D-09, D-10, D-16.

### Decisions affecting manifest structure

D-11, D-12, D-13, D-14, D-15, VD-1, VD-2, VD-3, VD-4.

### Decisions affecting validator behavior

D-05, D-07, D-09, D-13, D-15, SD-04, VD-3, VD-5.

### Decisions affecting the Global Corp exemplar

D-11, D-13, D-14, D-15, VD-1, VD-2, VD-3. The exemplar's manifest scaffolding (Appendix B of [Global-Corp-Exemplar.md](Global-Corp-Exemplar.md)) adopts the full declaration pattern once Phase 1 ships.

## 10. Source References

**[R1]** SpecLang Design. Workspace: [SpecLang-Design.md](SpecLang-Design.md). The authoritative language design referenced by many decisions.

**[R2]** Spec Type System. Workspace: [Spec-Type-System.md](Spec-Type-System.md). Taxonomy and validation architecture.

**[R3]** MCP Server Integration Design. Workspace: [MCP-Server-Integration-Design.md](MCP-Server-Integration-Design.md). Section 7 of that doc contained the D-01 through D-16 decisions in their original working-design form; this record captures their settled outcome.

**[R4]** SpecChat Versioning Policy. Workspace: [SpecChat-Versioning-Policy.md](SpecChat-Versioning-Policy.md). Produced alongside this record; captures the five versioning sub-decisions (VD-1 through VD-5) as formal policy.

**[R5]** Global Corp Exemplar. Workspace: [Global-Corp-Exemplar.md](Global-Corp-Exemplar.md). The worked example that many decisions reference.

**[R6]** SpecChat-BTABOK Acronym and Term Glossary. Workspace: [SpecChat-BTABOK-Acronym-and-Term-Glossary.md](SpecChat-BTABOK-Acronym-and-Term-Glossary.md). For acronym lookups.

**[R7]** BTABOK Out-of-Scope Models. Workspace: [BTABOK-Out-of-Scope-Models.md](BTABOK-Out-of-Scope-Models.md). Context for V2-2 and V2-3 deferrals.

**[R8]** Core-SpecLang-Absorption-Design.md (archived at `WIP/Archive/`). The source of Option X and the sub-decisions SD-01 through SD-05 that became D-13 through D-16 and SD-04.

**[R9]** BTABOK-Profile-v0.1-Design.md (archived at `WIP/Archive/`). The source of Option Y deferrals now in Tier 4.
