# SpecChat BTABOK Implementation Plan

## Tracking

| Field | Value |
|---|---|
| Document ID | IMPL-001 |
| itemType | ImplementationPlan |
| slug | specchat-btabok-implementation-plan |
| Version | 0.1.0 |
| Created | 2026-04-17 |
| Last Reviewed | 2026-04-17 |
| State | Draft |
| retentionPolicy | indefinite |
| Freshness SLA | 90 days |
| Owner | PER-01 Lena Brandt, Chief Architect |
| Approver | PER-11 Anja Petersen, Chair EARB |
| Dependencies | SpecLang-Design.md, MCP-Server-Integration-Design.md, SpecChat-Design-Decisions-Record.md, SpecChat-Versioning-Policy.md |

## 1. Purpose and Scope

This document is the authoritative implementation plan for the BTABOK integration into SpecChat. It translates the architectural decisions settled in the design corpus into a sequenced, task-level plan against the existing C# codebase at `src/MCPServer/DotNet/`.

**In scope:**
- All six implementation phases with task-level detail
- File-level impact per task (new files, modified files)
- Inter-task dependencies that drive sequencing
- Test strategy per phase
- Sample collection migration
- Success criteria for each phase

**Out of scope:**
- Re-litigating settled architectural decisions (see `SpecChat-Design-Decisions-Record.md`)
- Value Model or People Model profile implementation (deferred per `BTABOK-Out-of-Scope-Models.md`)
- Profile composition (v0.2+ per V2-1 deferral)
- The `/spec-btabok` slash command file content (lives in `.claude/commands/`, not in the C# codebase)

## 2. Strategy Overview

The plan follows these principles:

1. **Phase-gated delivery.** Each phase produces a shippable increment with clear ship conditions. Phases may overlap but ship conditions are sequential.
2. **Test-first where feasible.** Every validator, every parser production, every AST node gets a test fixture before the implementation closes the PR.
3. **Failure isolation.** Phase 1 (foundation) deliberately ships with no validators so the parser and AST landing is independent of semantic-check work.
4. **Sample-driven ship conditions.** Phase 2a's ship condition is that the existing sample collections (blazor-harness, TodoApp, PizzaShop, todo-app-the-standard) pass the 10 core validators. Phase 2c's ship condition adds the Global Corp exemplar passing all 23 validators.
5. **Minimal codebase surface per phase.** Most phases touch fewer than 15 files. Phase 1 is the largest at roughly 30-40 files affected.

## 3. Pre-Implementation Setup

Three preparatory items that block or materially de-risk Phase 1.

### 3.1 Keyword collision audit

Mechanical scan of the existing sample specs for proposed reserved keywords.

**Reserved keywords to audit (per D-03 Option B):**
`profile`, `concept`, `canvas`, `section`, `meta`, `relationships`, `storage`, `envelope`, `item`, `area`.

**Scan targets:**
- `src/MCPServer/DotNet/samples/blazor-harness.spec.md` and `.manifest.md`
- `src/MCPServer/DotNet/samples/TodoApp.spec.md` and `.manifest.md`
- `src/MCPServer/DotNet/samples/PizzaShop.spec.md` and `.manifest.md`
- `src/MCPServer/DotNet/samples/todo-app-the-standard.spec.md` and `.manifest.md`
- `tests/fixtures/*.spec.md`

**Expected collisions:** `item`, `area`, possibly `for` (inside CaDL-like positions, but `for` is contextual, not reserved).

**Output:** Short audit report listing any matches. For each match, recommend either:
- Rename the identifier in the sample (preferred for low-value names).
- Demote the keyword to contextual (if the collision is inside a construct where the keyword makes sense).

**Effort:** 30 minutes. One session.

### 3.2 Diagnostic code pre-allocation

Draft the `DiagnosticCodes.cs` static class structure before Phase 1 begins. This establishes the namespace layout without committing to specific messages.

**Namespaces:**
- `Spec.Met` (metadata completeness)
- `Spec.Slug` (slug rules)
- `Spec.Ref` (reference resolution and supersedes cycles)
- `Spec.Frs` (freshness SLA)
- `Spec.Prf` (profile composition)
- `Spec.Rel` (relationship cardinality)
- `Spec.Ret` (retention defaults; info-level)
- `Spec.Ver` (version compatibility and monotonicity)
- `Standard.*` (existing The Standard codes, backfilled opportunistically)
- `Btabok.Asr`, `Btabok.Dec`, `Btabok.Prn`, `Btabok.Stk`, `Btabok.Vpt`, `Btabok.Wvr`, `Btabok.Gov`, `Btabok.Rmp`, `Btabok.Cnv`, `Btabok.Met` (profile validators)

**Effort:** 1 hour.

### 3.3 Test fixture layout decision

Establish the physical layout of test fixtures before Phase 1 writes any tests.

**Layout:**
```
tests/fixtures/
├── core/
│   ├── known-good/
│   │   ├── minimal-core/              (Core profile, 3 specs)
│   │   ├── minimal-standard/          (TheStandard profile, 3 specs)
│   │   └── minimal-btabok/            (BTABOK profile, 3 specs)
│   └── known-bad/
│       ├── spec-met-001-missing-slug/
│       ├── spec-slug-001-duplicate/
│       ├── ... (one folder per diagnostic code)
├── btabok-profile/
│   ├── known-good/
│   │   └── global-corp/               (the full exemplar once authored)
│   └── known-bad/
│       ├── btabok-asr-001-no-trace/
│       ├── btabok-dec-001-no-scope/
│       ├── ... (one folder per BTABOK validator)
└── migration/
    └── core-to-btabok/                (migrationFrom fixture)
```

**Effort:** 2 hours (create directory structure plus README per folder).

## 4. Phase 1: Foundation

**Goal:** parser and AST for core metadata, references, relationships, retention, diagnostics, plus BTABOK profile lexer and AST. No validation logic yet.

**Ship condition:** given a BTABOK-profile manifest and a concept-containing spec, the parser produces a clean AST with no errors and no logic checks applied.

**Estimated effort:** 3 to 5 weeks for a single engineer; 2 to 3 weeks with parallel work.

### 4.1 Task group: Lexer additions

| Task | Files affected | Effort |
|---|---|---|
| L-1.1 Add core reference-type tokens | `SpecChat.Language/Lexer.cs`, `TokenKind.cs` | 2 hr |
| L-1.2 Add core relationship-verb tokens | `SpecChat.Language/Lexer.cs`, `TokenKind.cs` | 2 hr |
| L-1.3 Add core retention policy enum values | `SpecChat.Language/Lexer.cs` | 1 hr |
| L-1.4 Add core `meta` block introducer token | `SpecChat.Language/Lexer.cs` | 1 hr |
| L-1.5 Add BTABOK top-level reserved keywords (profile, concept, canvas) | `SpecChat.Language/Lexer.cs` | 1 hr |
| L-1.6 Add BTABOK block-structure tokens (section, relationships, storage, envelope, item, area) | `SpecChat.Language/Lexer.cs` | 2 hr |
| L-1.7 Add BTABOK contextual identifiers (section modifiers, primitive types, CaDL directives) | `SpecChat.Language/Lexer.cs` with context detection | 4 hr |
| L-1.8 Lexer tests for each new token family | `SpecChat.Language.Tests/LexerTests.cs` (extend) | 4 hr |

**Task group total:** roughly 17 hours / 2 days.

### 4.2 Task group: AST additions

| Task | Files affected | Effort |
|---|---|---|
| A-1.1 Create CoreMetadataDeclarations.cs | `Ast/CoreMetadataDeclarations.cs` (new) | 4 hr |
| A-1.2 MetaBlock record | Above file | (included) |
| A-1.3 FieldDecl reference-type node extensions | Above file | 2 hr |
| A-1.4 RelationshipsBlock and RelationshipDecl | Above file | 2 hr |
| A-1.5 PersonRef record | Above file | 1 hr |
| A-1.6 RetentionPolicy enum | Above file | 30 min |
| A-1.7 Diagnostic record extension (Code, Validator, Suggestion) | `DiagnosticBag.cs`, `Diagnostic.cs` | 2 hr |
| A-1.8 Create BtabokDeclarations.cs | `Ast/BtabokDeclarations.cs` (new) | 4 hr |
| A-1.9 ProfileDecl, ConceptDecl, SectionDecl, StorageBlock | Above file | 3 hr |
| A-1.10 CanvasDecl, AreaDecl, ShowsClause | Above file | 3 hr |
| A-1.11 BTABoKItem metadata extensions, SectionModifier, AccessTier, BokStatus enums | Above file | 2 hr |
| A-1.12 SpecDocument extensions (add Profile, Concepts, Canvases, Metadata, Relationships) | `Ast/Declarations.cs` | 2 hr |
| A-1.13 AST tests | `SpecChat.Language.Tests/AstTests.cs` (new) | 4 hr |

**Task group total:** roughly 30 hours / 4 days.

### 4.3 Task group: Parser additions

| Task | Files affected | Effort |
|---|---|---|
| P-1.1 Core metadata block parsing | `Parser.cs` (extend) | 4 hr |
| P-1.2 Core reference type parsing (ref<T>, weakRef, externalRef) | `Parser.cs` | 3 hr |
| P-1.3 Core relationship block parsing with cardinality | `Parser.cs` | 4 hr |
| P-1.4 Core cardinality range parsing | `Parser.cs` | 2 hr |
| P-1.5 Create Parser.Btabok.cs partial class | `Parser.Btabok.cs` (new) | 6 hr |
| P-1.6 Profile declaration parsing | `Parser.Btabok.cs` | 2 hr |
| P-1.7 Concept body parsing (meta, sections, relationships, storage) | `Parser.Btabok.cs` | 8 hr |
| P-1.8 Composite profile type parsing (range<T>, flags(...), enum(...), measurement, threshold) | `Parser.Btabok.cs` | 4 hr |
| P-1.9 Display hint annotation parsing (@display) | `Parser.Btabok.cs` | 2 hr |
| P-1.10 CaDL canvas and area parsing | `Parser.Btabok.cs` | 4 hr |
| P-1.11 CaDL transform expression parsing | `Parser.Btabok.cs` | 3 hr |
| P-1.12 Parser test suite for each production | `ParserTests.cs`, `ParserBtabokTests.cs` (new) | 12 hr |

**Task group total:** roughly 54 hours / 7 days.

### 4.4 Task group: Manifest model

| Task | Files affected | Effort |
|---|---|---|
| M-1.1 Redefine ManifestDocument record with new fields | `ManifestParser.cs` | 4 hr |
| M-1.2 Define ProfileInfo, TypeRegistryEntry, GovernancePosture records | `ManifestParser.cs` | 2 hr |
| M-1.3 Redefine SpecEntry with CoDL-aligned fields | `ManifestParser.cs` | 2 hr |
| M-1.4 Rewrite ManifestParser.Parse to produce new shape | `ManifestParser.cs` | 8 hr |
| M-1.5 Add specLangVersion parsing | `ManifestParser.cs` | 2 hr |
| M-1.6 Add profile version, CoDL version, CaDL version parsing | `ManifestParser.cs` | 2 hr |
| M-1.7 Parse type registry table | `ManifestParser.cs` | 3 hr |
| M-1.8 Parse severity overrides table | `ManifestParser.cs` | 2 hr |
| M-1.9 Parse governance posture | `ManifestParser.cs` | 1 hr |
| M-1.10 ManifestParser tests | `ManifestParserTests.cs` | 6 hr |

**Task group total:** roughly 32 hours / 4 days.

### 4.5 Task group: Diagnostic code registry

| Task | Files affected | Effort |
|---|---|---|
| D-1.1 Create DiagnosticCodes.cs static class | `SpecChat.Language/DiagnosticCodes.cs` (new) | 3 hr |
| D-1.2 Populate SPEC- prefix codes (10 validator codes plus SPEC-VER- codes) | Above file | 3 hr |
| D-1.3 Populate BTABOK- prefix codes (13 validator codes) | Above file | 2 hr |
| D-1.4 Reserve STANDARD- prefix codes (backfill Phase 2 onward) | Above file | 1 hr |
| D-1.5 Write diagnostic code reference markdown doc | `docs/diagnostic-codes.md` (new) | 6 hr |
| D-1.6 Test: code registry has no duplicates; markdown matches class | `DiagnosticCodesTests.cs` (new) | 3 hr |

**Task group total:** roughly 18 hours / 2 days.

### 4.6 Task group: Phase 1 MCP tools

| Task | Files affected | Effort |
|---|---|---|
| T-1.1 get_supported_versions MCP tool | New `SpecChat.Mcp/Tools/VersionTools.cs` | 3 hr |
| T-1.2 get_deprecation_schedule MCP tool | Above file | 3 hr |
| T-1.3 Deprecation schedule data source (initial release notes) | `deprecation-schedule.json` resource | 2 hr |
| T-1.4 MCP tool tests | `SpecChat.Mcp.Tests/VersionToolsTests.cs` (new) | 3 hr |

**Task group total:** roughly 11 hours / 1.5 days.

### 4.7 Phase 1 ship condition

**Verification:**
1. A fresh BTABOK-profile manifest (minimal) parses without error.
2. A fresh Core-profile manifest parses without error.
3. A concept definition spec parses into a clean AST.
4. Every reserved keyword is recognized; every contextual identifier is recognized only in the expected context.
5. The keyword collision audit (Section 3.1) produced no unresolved issues.
6. `get_supported_versions` returns a structured response.
7. `DiagnosticCodes.cs` contains the expected namespaces with unique codes.

**Tests pass:** LexerTests, AstTests, ParserTests, ParserBtabokTests, ManifestParserTests, DiagnosticCodesTests, VersionToolsTests.

**No validation logic** exists yet. All validators are Phase 2a or later.

## 5. Phase 2a: Core Validators

**Goal:** the 10 core validators (SPEC- prefix) run correctly on every collection regardless of profile.

**Ship condition:** existing sample collections (blazor-harness, TodoApp, PizzaShop, todo-app-the-standard) pass all 10 core validators after sample migration.

**Estimated effort:** 2 to 3 weeks.

### 5.1 Task group: CoreMetadataAnalyzer

| Task | Files affected | Effort |
|---|---|---|
| V-2a.1 Create CoreMetadataAnalyzer class | `SpecChat.Language/CoreMetadataAnalyzer.cs` (new) | 2 hr |
| V-2a.2 check_metadata_completeness | Above file | 4 hr |
| V-2a.3 check_slug_uniqueness | Above file | 3 hr |
| V-2a.4 check_slug_format | Above file | 2 hr |
| V-2a.5 check_freshness_sla | Above file | 3 hr |
| V-2a.6 check_profile_composition | Above file | 3 hr |
| V-2a.7 check_relationship_cardinality | Above file | 4 hr |
| V-2a.8 check_supersedes_cycles | Above file | 5 hr |
| V-2a.9 check_version_compatibility (per VD-3) | Above file | 4 hr |
| V-2a.10 check_version_monotonicity (per VD-5) | Above file | 5 hr |
| V-2a.11 Per-validator fixture tests | `CoreMetadataAnalyzerTests.cs` (new) | 16 hr |

*Note: check_reference_resolution and check_weakref_resolution land in Phase 2b along with `CollectionIndex`, since they need collection-level visibility.*

**Task group total:** roughly 51 hours / 7 days.

### 5.2 Task group: Core validator MCP tools

| Task | Files affected | Effort |
|---|---|---|
| T-2a.1 Create CoreMetadataValidationTools.cs | `SpecChat.Mcp/Tools/CoreMetadataValidationTools.cs` (new) | 2 hr |
| T-2a.2 One MCP method per validator (10 tools) | Above file | 8 hr |
| T-2a.3 validate_collection convenience wrapper | Above file | 3 hr |
| T-2a.4 Tool-level tests | `CoreMetadataValidationToolsTests.cs` (new) | 6 hr |

**Task group total:** roughly 19 hours / 2.5 days.

### 5.3 Task group: Sample collection migration

Each of the four sample collections needs a manual migration pass to adopt the Core SpecLang post-Option-X metadata requirements.

| Task | Files affected | Effort |
|---|---|---|
| M-2a.1 Migrate blazor-harness manifest and specs | Samples folder | 3 hr |
| M-2a.2 Migrate TodoApp manifest and specs | Samples folder | 2 hr |
| M-2a.3 Migrate PizzaShop manifest and specs | Samples folder | 4 hr |
| M-2a.4 Migrate todo-app-the-standard manifest and specs (includes Profile: TheStandard, version 1.0.0 migration) | Samples folder | 3 hr |
| M-2a.5 Backfill diagnostic codes on existing Core and Standard validators | `SemanticAnalyzer.cs`, `StandardSemanticAnalyzer.cs` | 6 hr |

**Task group total:** roughly 18 hours / 2.5 days.

### 5.4 Phase 2a ship condition

**Verification:**
1. All four sample collections pass `validate_collection` with no errors.
2. Each core validator has at least one passing and one failing fixture case.
3. Samples declare the appropriate `specLangVersion`, `Profile`, `profileVersion` fields.
4. Diagnostic output from running all validators shows expected codes.

## 6. Phase 2b: Core Infrastructure

**Goal:** collection-level analysis capability. WorkspaceAnalyzer, CollectionIndex, ValidatorSeverityPolicy as core services.

**Ship condition:** a collection-level analysis run produces the full diagnostic set across all specs, with severity overrides applied correctly per manifest.

**Estimated effort:** 1.5 to 2 weeks.

### 6.1 Task group: WorkspaceAnalyzer and CollectionIndex

| Task | Files affected | Effort |
|---|---|---|
| W-2b.1 Create WorkspaceAnalyzer class | `SpecChat.Language/WorkspaceAnalyzer.cs` (new) | 4 hr |
| W-2b.2 AnalyzeCollection entry point | Above file | 4 hr |
| W-2b.3 Delegation to SemanticAnalyzer for per-file work | Above file | 3 hr |
| W-2b.4 Create CollectionIndex service | `SpecChat.Language/CollectionIndex.cs` (new) | 4 hr |
| W-2b.5 Mtime-based cache per D-10 | Above file | 4 hr |
| W-2b.6 Lazy initialization per D-16 | Above file | 2 hr |
| W-2b.7 Resolve(refType, slug) API | Above file | 2 hr |
| W-2b.8 check_reference_resolution (uses CollectionIndex) | `CoreMetadataAnalyzer.cs` (extend) | 4 hr |
| W-2b.9 check_weakref_resolution | Above file | 3 hr |
| W-2b.10 check_externalref_validity | Above file | 3 hr |
| W-2b.11 WorkspaceAnalyzer and CollectionIndex tests | `WorkspaceAnalyzerTests.cs`, `CollectionIndexTests.cs` (new) | 10 hr |

**Task group total:** roughly 43 hours / 6 days.

### 6.2 Task group: ValidatorSeverityPolicy

| Task | Files affected | Effort |
|---|---|---|
| S-2b.1 Create ValidatorSeverityPolicy service | `SpecChat.Language/ValidatorSeverityPolicy.cs` (new) | 4 hr |
| S-2b.2 Load severity overrides from manifest | Above file | 3 hr |
| S-2b.3 Enforce override-direction rules | Above file | 3 hr |
| S-2b.4 Apply governancePosture: strict promotion | Above file | 3 hr |
| S-2b.5 Hook into DiagnosticBag post-processing at tool boundary | `SpecChat.Mcp/Tools/` (multiple) | 4 hr |
| S-2b.6 Severity policy tests | `ValidatorSeverityPolicyTests.cs` (new) | 5 hr |

**Task group total:** roughly 22 hours / 3 days.

### 6.3 Task group: Cache-management tooling

| Task | Files affected | Effort |
|---|---|---|
| C-2b.1 clear_cache MCP tool | `SpecChat.Mcp/Tools/CacheTools.cs` (new) | 3 hr |
| C-2b.2 Tool tests | `CacheToolsTests.cs` (new) | 2 hr |

**Task group total:** 5 hours.

### 6.4 Phase 2b ship condition

**Verification:**
1. `WorkspaceAnalyzer.AnalyzeCollection` produces diagnostics across all inventory entries.
2. Mtime-based cache hits for repeated invocations.
3. Severity overrides from the manifest apply correctly; governancePosture: strict promotes warnings to errors.
4. `clear_cache` evicts the cached index for a given manifest.
5. All three reference-resolution validators run and produce expected diagnostics on the known-bad fixtures.

## 7. Phase 2c: BTABOK-Profile Validators

**Goal:** the 13 BTABOK-specific validators (BTABOK- prefix) run correctly when the BTABOK profile is active.

**Ship condition:** the Global Corp exemplar (once converted to spec files) passes all 23 validators (10 core plus 13 BTABOK).

**Estimated effort:** 2 to 3 weeks.

### 7.1 Task group: BtabokSemanticAnalyzer

| Task | Files affected | Effort |
|---|---|---|
| B-2c.1 Create BtabokSemanticAnalyzer class | `SpecChat.Language/BtabokSemanticAnalyzer.cs` (new) | 3 hr |
| B-2c.2 Activation hook in SemanticAnalyzer.Analyze | `SemanticAnalyzer.cs` | 2 hr |
| B-2c.3 check_asr_traceability | `BtabokSemanticAnalyzer.cs` | 4 hr |
| B-2c.4 check_asr_addressed_by_decision | Above file | 3 hr |
| B-2c.5 check_decision_scope_type | Above file | 3 hr |
| B-2c.6 check_decision_cascades | Above file | 5 hr |
| B-2c.7 check_principle_links | Above file | 3 hr |
| B-2c.8 check_stakeholder_coverage | Above file | 5 hr |
| B-2c.9 check_viewpoint_coverage | Above file | 4 hr |
| B-2c.10 check_waiver_expiration | Above file | 3 hr |
| B-2c.11 check_waiver_rule_reference | Above file | 3 hr |
| B-2c.12 check_governance_approval | Above file | 4 hr |
| B-2c.13 check_roadmap_capability_moves | Above file | 4 hr |
| B-2c.14 check_canvas_target_exists | Above file | 3 hr |
| B-2c.15 check_metric_baseline_target | Above file | 3 hr |
| B-2c.16 Per-validator fixture tests | `BtabokSemanticAnalyzerTests.cs` (new) | 20 hr |

**Task group total:** roughly 72 hours / 9 days.

### 7.2 Task group: BtabokValidationTools

| Task | Files affected | Effort |
|---|---|---|
| T-2c.1 Create BtabokValidationTools.cs | `SpecChat.Mcp/Tools/BtabokValidationTools.cs` (new) | 2 hr |
| T-2c.2 One MCP method per validator (13 tools) | Above file | 10 hr |
| T-2c.3 Tool-level tests | `BtabokValidationToolsTests.cs` (new) | 7 hr |

**Task group total:** roughly 19 hours / 2.5 days.

### 7.3 Task group: Global Corp exemplar spec-file generation

The Global Corp exemplar currently exists only as a descriptive design doc. Phase 2c requires it to exist as actual spec files so validators have real content to validate.

| Task | Files affected | Effort |
|---|---|---|
| E-2c.1 Generate global-corp.manifest.md | `samples/global-corp/` (new folder) | 3 hr |
| E-2c.2 Generate global-corp.architecture.spec.md | Above folder | 4 hr |
| E-2c.3 Generate global-corp.stakeholders.spec.md | Above folder | 3 hr |
| E-2c.4 Generate global-corp.asrs.spec.md | Above folder | 3 hr |
| E-2c.5 Generate global-corp.decisions.spec.md | Above folder | 4 hr |
| E-2c.6 Generate global-corp.governance.spec.md | Above folder | 3 hr |
| E-2c.7 Generate global-corp.waivers.spec.md | Above folder | 2 hr |
| E-2c.8 Generate global-corp.roadmap.spec.md | Above folder | 3 hr |
| E-2c.9 Generate global-corp.viewpoints.spec.md | Above folder | 3 hr |
| E-2c.10 Generate global-corp.views.spec.md | Above folder | 4 hr |
| E-2c.11 Generate global-corp.standards.spec.md | Above folder | 2 hr |
| E-2c.12 Generate outcome-scorecard.spec.md | Above folder | 3 hr |
| E-2c.13 Generate six subsystem specs (event-backbone, traceability-core, partner-connectivity, compliance-core, operational-intelligence, dpp-sustainability) | Above folder | 12 hr |
| E-2c.14 Generate legacy-globaltrack-apac.decommission.spec.md | Above folder | 2 hr |
| E-2c.15 Generate three experiment specs | Above folder | 3 hr |

**Task group total:** roughly 54 hours / 7 days.

### 7.4 Phase 2c ship condition

**Verification:**
1. Global Corp exemplar folder exists with 21 spec files plus manifest.
2. All 13 BTABOK validators run on the exemplar and emit expected diagnostics.
3. Every expected trace, cascade, reference resolves correctly.
4. Every waiver references a valid rule; every ASR has at least one addressing decision; every canvas targets an existing concept type.
5. The exemplar passes `validate_collection` with `governancePosture: warnings` (some info diagnostics expected from `check_freshness_sla` given artificial dates).

## 8. Phase 3: Polish and Additional Validators

**Goal:** harden validators based on exemplar usage; add any validators that emerge as needed.

**Ship condition:** no known exemplar-triggered false positives or false negatives.

**Estimated effort:** 1 week (triage-driven).

### 8.1 Anticipated tasks

- Fine-tune diagnostic messages for clarity
- Add validators discovered during exemplar authoring (for example, a `check_canvas_field_exists` validator that checks CaDL `shows:` field references resolve against the target concept's Appendix A definition)
- Add fixture cases uncovered by exemplar edge cases
- Performance profiling of `CollectionIndex` against the 21-file exemplar

**Effort:** 40 hours / 1 week.

## 9. Phase 4: Advanced Features

**Goal:** full governance posture surface, per-validator severity overrides, CLI strict flag.

**Ship condition:** BTABOK Profile v0.1 feature complete on the validator surface.

**Estimated effort:** 1 to 1.5 weeks.

### 9.1 Task group: Severity override manifest syntax

| Task | Files affected | Effort |
|---|---|---|
| P-4.1 Parse severity override table format | `ManifestParser.cs` | 4 hr |
| P-4.2 Enforce direction rules (error cannot be demoted) | `ValidatorSeverityPolicy.cs` | 3 hr |
| P-4.3 Per-validator override documentation | `docs/severity-overrides.md` (new) | 3 hr |
| P-4.4 Tests for override parsing and enforcement | `ValidatorSeverityPolicyTests.cs` (extend) | 5 hr |

### 9.2 Task group: CLI strict flag

| Task | Files affected | Effort |
|---|---|---|
| C-4.1 Add `--strict` CLI flag to MCP tool boundary | `SpecChat.Mcp/Tools/` (boundary update) | 4 hr |
| C-4.2 Escalation-only semantics (D-09 Option D) | `ValidatorSeverityPolicy.cs` | 3 hr |
| C-4.3 Tests | `ValidatorSeverityPolicyTests.cs` | 4 hr |

### 9.3 Task group: Migration-mode relaxation

| Task | Files affected | Effort |
|---|---|---|
| M-4.1 Parse `Profile.migrationFrom` field | `ManifestParser.cs` | 2 hr |
| M-4.2 Relax reference-resolution errors to warnings during migration | `CoreMetadataAnalyzer.cs` | 3 hr |
| M-4.3 Tests | `ManifestParserTests.cs` (extend) | 3 hr |

**Phase 4 total:** roughly 34 hours / 4.5 days.

## 10. Phase 5: CaDL Rendering

**Goal:** the 20 canvases render against the Global Corp exemplar.

**Ship condition:** `spec project --canvas <name>` produces correct output for every canvas in the catalog.

**Estimated effort:** 3 to 4 weeks.

### 10.1 Task group: Rendering infrastructure

| Task | Files affected | Effort |
|---|---|---|
| R-5.1 Create CanvasRenderer orchestrator | `SpecChat.Language/Rendering/CanvasRenderer.cs` (new) | 6 hr |
| R-5.2 Create CanvasRegistry for the 20 built-in canvases | `SpecChat.Language/Rendering/CanvasRegistry.cs` (new) | 4 hr |
| R-5.3 Create MarkdownTableRenderer | `SpecChat.Language/Rendering/MarkdownTableRenderer.cs` (new) | 8 hr |
| R-5.4 Create MermaidRenderer dispatcher | `SpecChat.Language/Rendering/MermaidRenderer.cs` (new) | 6 hr |
| R-5.5 Mermaid flowchart renderer | Above file | 6 hr |
| R-5.6 Mermaid quadrantChart renderer | Above file | 4 hr |
| R-5.7 Mermaid gantt renderer | Above file | 5 hr |
| R-5.8 Mermaid timeline renderer | Above file | 4 hr |

**Task group total:** roughly 43 hours / 5.5 days.

### 10.2 Task group: Per-canvas implementations (20 canvases, Phase 5a and 5b)

Phase 5a: 10 simpler canvases (table-based).
Phase 5b: 10 more complex canvases (Mermaid, joins, computed fields).

| Canvas | Phase | Effort |
|---|---|---|
| DecisionRegistry | 5a | 4 hr |
| DecisionRecordCard | 5a | 4 hr |
| ASRMatrix | 5a | 4 hr |
| ASRCard | 5a | 3 hr |
| StakeholderMap | 5a | 4 hr |
| ConcernMap | 5a | 5 hr |
| ViewpointCatalog | 5a | 3 hr |
| PrincipleCatalog | 5a | 3 hr |
| StandardsCatalog | 5a | 3 hr |
| WaiverRegister | 5a | 3 hr |
| WaiverCard | 5a | 3 hr |
| PowerInterestGrid | 5b | 6 hr |
| CapabilityHeatMap | 5b | 6 hr |
| RoadmapTimeline | 5b | 8 hr |
| FreshnessReport | 5b | 7 hr |
| OutcomeScorecard | 5b | 7 hr |
| GovernanceApprovalFlow | 5b | 6 hr |
| RiskRegister | 5b | 4 hr |
| ExperimentBoard | 5b | 4 hr |
| ViewpointCoverageReport | 5b | 5 hr |

**Per-canvas total:** roughly 92 hours / 12 days.

### 10.3 Task group: project_canvas MCP tool

| Task | Files affected | Effort |
|---|---|---|
| T-5.1 Create BtabokProjectionTools.cs | `SpecChat.Mcp/Tools/BtabokProjectionTools.cs` (new) | 3 hr |
| T-5.2 project_canvas tool | Above file | 4 hr |
| T-5.3 Tool tests | `BtabokProjectionToolsTests.cs` (new) | 5 hr |

### 10.4 Phase 5 ship condition

**Verification:**
1. Every canvas renders against the Global Corp exemplar without error.
2. Rendered outputs are diff-stable (deterministic rendering).
3. Freshness warnings appear correctly at the head of rendered output when concepts are stale.

## 11. Phase 6: Migration Tooling

**Goal:** automated migration of existing Core-profile collections to the BTABOK profile.

**Ship condition:** each of the four sample collections can be migrated via `migrate_to_btabok` without manual intervention.

**Estimated effort:** 1.5 weeks.

### 11.1 Task group: Migration tool

| Task | Files affected | Effort |
|---|---|---|
| M-6.1 Create BtabokMigrationTools.cs | `SpecChat.Mcp/Tools/BtabokMigrationTools.cs` (new) | 3 hr |
| M-6.2 migrate_to_btabok MCP tool | Above file | 6 hr |
| M-6.3 MigrationPlan concept and tracking | `SpecChat.Language/MigrationPlan.cs` (new) | 4 hr |
| M-6.4 Detect required metadata gaps | `BtabokMigrationTools.cs` | 5 hr |
| M-6.5 Produce migration plan as patch suggestions | Above file | 6 hr |
| M-6.6 Exercise against all four sample collections | Sample folders | 4 hr |
| M-6.7 Migration tool tests | `BtabokMigrationToolsTests.cs` (new) | 6 hr |

**Task group total:** roughly 34 hours / 4.5 days.

## 12. Cross-Phase Concerns

### 12.1 Continuous integration

Every phase ships a CI pipeline update:
- xUnit test runs
- Code coverage report
- Validator corpus check (every new validator has at least one passing and one failing fixture)
- Diagnostic code uniqueness check

### 12.2 Release cadence

Suggested: ship each phase as a preview NuGet package:
- 0.1.0-preview-phase1 after Phase 1
- 0.1.0-preview-phase2a after Phase 2a
- ... etc
- 0.1.0 as the v0.1 GA release after Phase 4 (validators complete)
- Phase 5 and Phase 6 are post-GA enhancements

### 12.3 Documentation

- Keep `docs/diagnostic-codes.md` up to date each phase
- `docs/migration-guide.md` updated when Phase 6 lands
- Release notes per NuGet release citing which validators and canvases are new

### 12.4 Deprecation schedule maintenance

The deprecation schedule (per VD-4) lives in `deprecation-schedule.json`. Each server release updates it if any versions pass through a deprecation stage.

### 12.5 Performance monitoring

Track Global Corp exemplar validation latency as the canary. Target: full `validate_collection` run under 5 seconds on a cold cache, under 500ms on a warm cache.

## 13. Risks and Mitigations

| Risk | Phase | Mitigation |
|---|---|---|
| Keyword collisions surface during Phase 1 | 1 | Pre-implementation audit (Section 3.1); demote to contextual if collision found |
| CollectionIndex performance degrades on collections over 50 specs | 2b | Benchmark against Global Corp (21 specs); mtime cache; clear_cache tool |
| Circular reference detection is expensive for deep supersedes chains | 2a | Cache computed cycles per validation run |
| CaDL rendering of Mermaid gantt and timeline is complex | 5b | Start with simple variants; defer complex interaction to v0.2 |
| Sample collection migration breaks existing behavior | 2a | Run existing tests before and after migration; explicit diff review |
| The Standard validator diagnostic-code backfill is boring and gets skipped | 2a | Make it a Phase 2a ship condition |
| Profile-version range notation (semver) is unfamiliar to .NET engineers | 1 | Use `Semver` library (NuGet); do not implement semver parsing from scratch |

## 14. Success Metrics per Phase

| Phase | Metric | Target |
|---|---|---|
| 1 | Parser throughput on Global Corp exemplar | < 2 sec cold |
| 1 | AST node count for BTABOK profile activation | All 19 concept types parseable |
| 2a | Sample collections passing validators | 4 of 4 |
| 2a | Validator unit test count | >= 30 (10 validators x 3 cases) |
| 2b | CollectionIndex cache hit rate on warm calls | > 95% |
| 2c | Global Corp exemplar passing all 23 validators | Yes |
| 3 | Known false-positive rate | 0 |
| 4 | Severity override enforcement tests | 100% pass |
| 5 | Canvas rendering success rate | 20 of 20 |
| 6 | Sample migration success rate | 4 of 4 |

## 15. Task Tracking and Dependencies

### 15.1 Cross-phase dependencies

- Phase 2a depends on Phase 1 AST and parser
- Phase 2b depends on Phase 2a validators (reference-resolution validators live in CoreMetadataAnalyzer but need CollectionIndex from 2b; split as noted)
- Phase 2c depends on Phase 2b (CollectionIndex) and Phase 1 (BTABOK AST nodes)
- Phase 3 depends on Phase 2c (exemplar authored)
- Phase 4 can start in parallel with Phase 2c
- Phase 5 depends on Phase 2c (canvases need concept instances)
- Phase 6 depends on Phase 2b (CoreMetadataAnalyzer) and Phase 2c (BtabokSemanticAnalyzer)

### 15.2 Parallelization opportunities

- Within Phase 1: lexer, AST, and parser tasks can parallelize once the AST types are defined
- Within Phase 2c: the 13 BTABOK validators can each be a separate PR
- Within Phase 5: the 20 canvases can parallelize across contributors
- Sample migration (Phase 2a) can start as soon as Phase 1 lands, in parallel with core validator development

### 15.3 Total estimated effort

Rough totals by phase:

| Phase | Effort | Duration (1 eng) | Duration (2-3 eng) |
|---|---|---|---|
| Pre-impl | 3 hr | 0.5 day | 0.5 day |
| 1 | 162 hr | 4 weeks | 2 weeks |
| 2a | 88 hr | 2 weeks | 1.5 weeks |
| 2b | 70 hr | 1.5 weeks | 1 week |
| 2c | 145 hr | 3.5 weeks | 2 weeks |
| 3 | 40 hr | 1 week | 1 week |
| 4 | 34 hr | 1 week | 0.5 week |
| 5 | ~150 hr | 3.5 weeks | 2 weeks |
| 6 | 34 hr | 1 week | 0.5 week |
| **Total** | **~726 hr** | **~17 weeks** | **~10 weeks** |

This is a large project. Realistic calendar (with review, iteration, holidays): 5-6 months for one engineer, 3-4 months for two to three.

## 16. Open Items for Later Implementation Decisions

Small items that can be settled by the implementer during implementation:

1. **Specific diagnostic message text.** The codes are reserved; the message strings can be authored per validator with user-friendly wording.
2. **CLI flag names beyond `--strict`.** Likely need `--no-cache`, `--verbose`, `--format`. Settled per tool as implemented.
3. **Test fixture naming conventions.** Suggested pattern: `<diagnostic-code>-<short-description>/` (e.g., `spec-slug-001-duplicate-slug/`).
4. **Error recovery in the parser.** The parser could fail fast or recover and continue. Recovery with multiple diagnostics per parse is the user-friendlier choice.
5. **Parallel validator execution.** Validators within a phase could run in parallel on a thread pool. Defer optimization until needed.

## 17. Source References

**[R1]** [SpecLang-Design.md](SpecLang-Design.md). Authoritative language design. Sections 4 (CoDL/CaDL), 5 (Core SpecLang Surface), 6 (BTABOK Profile), 13 (Test Corpus Plan).

**[R2]** [Spec-Type-System.md](Spec-Type-System.md). Spec taxonomy and validation architecture.

**[R3]** [MCP-Server-Integration-Design.md](MCP-Server-Integration-Design.md). Codebase-level integration design. Section 4 (Layer-by-Layer Change Map) and Section 5 (Phasing) are the direct source of this implementation plan.

**[R4]** [SpecChat-Versioning-Policy.md](SpecChat-Versioning-Policy.md). Versioning rules applied throughout.

**[R5]** [SpecChat-Design-Decisions-Record.md](SpecChat-Design-Decisions-Record.md). All 22 settled decisions. Cross-reference index in Section 9 of that doc maps decisions to affected codebase surfaces.

**[R6]** [Global-Corp-Exemplar.md](Global-Corp-Exemplar.md). The authoritative source for Phase 2c exemplar spec generation (task group E-2c).

**[R7]** [SpecChat-BTABOK-Acronym-and-Term-Glossary.md](SpecChat-BTABOK-Acronym-and-Term-Glossary.md). Terminology reference.

**[R8]** [BTABOK-Out-of-Scope-Models.md](BTABOK-Out-of-Scope-Models.md). Context for deferred items (Value Model, People Model, Competency Model).

**[R9]** Existing codebase at `src/MCPServer/DotNet/`. Current validator and tool patterns that new additions should match.
