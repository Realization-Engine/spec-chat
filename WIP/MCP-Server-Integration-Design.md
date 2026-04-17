# MCP Server Integration Design

## Tracking

| Field | Value |
|---|---|
| Document ID | GC-MCP-001 |
| itemType | DesignDocument |
| slug | mcp-server-integration-design |
| Version | 0.1.0 |
| Created | 2026-04-17 |
| Last Reviewed | 2026-04-17 |
| State | Draft |
| publishStatus | Draft |
| retentionPolicy | indefinite |
| Freshness SLA | 90 days |
| Owner | PER-01 Lena Brandt, Chief Architect |
| Approver | PER-11 Anja Petersen, Chair EARB |
| Dependencies | SpecLang-Design.md, SpecChat-Design-Decisions-Record.md, SpecChat-Versioning-Policy.md |

## 1. Purpose

This document captures the working design for integrating the BTABOK profile into the SpecChat MCP Server C# codebase at `src/MCPServer/DotNet/`. It covers what needs to change across parser, AST, analyzers, manifest model, MCP tool registration, diagnostics, and rendering.

SpecChat is pre-1.0. There is no legacy compatibility surface to preserve. The manifest model, diagnostic record, and analyzer signatures are free to evolve as needed to support the BTABOK profile.

A subsequent decision, recorded in [SpecLang-Design.md](SpecLang-Design.md) as Option X, absorbs a specific set of infrastructure concerns (standard metadata profile, reference types, relationship declarations with cardinality, retention policy, diagnostic record extensions, slug rules, and ten validators) from the BTABOK profile into Core SpecLang. That decision reshapes the layer-by-layer change map and the phasing in this document: several items originally scoped as BTABOK-profile additions are now core additions. The revisions below reflect the post-Option-X scope split.

This document has been updated to reflect settled architectural decisions. The decisions that were previously listed as open items in Section 7 are now recorded in [SpecChat-Design-Decisions-Record.md](SpecChat-Design-Decisions-Record.md), and the versioning sub-decisions are formalized in [SpecChat-Versioning-Policy.md](SpecChat-Versioning-Policy.md). Section 7 of this doc now serves as a reference-style summary of those settled outcomes. Implementation can proceed against the guidance captured here.

## 2. Current Architecture Summary

The codebase lives at `E:\Archive\GitHub\dlandi\spec-chat\src\MCPServer\DotNet\` and targets net10.0.

```
src/MCPServer/DotNet/
├── SpecChat.slnx                             (solution, xUnit, ModelContextProtocol 1.2.0)
├── SpecChat.Language/                        (parser, analyzers, AST, manifest)
│   ├── Lexer.cs, Parser.cs, Parser.Standard.cs
│   ├── SemanticAnalyzer.cs                   (core orchestrator, 743 lines)
│   ├── StandardSemanticAnalyzer.cs           (Standard extension, 355 lines)
│   ├── ManifestParser.cs
│   ├── DiagnosticBag.cs, SourceLocation.cs, Token.cs, TokenKind.cs
│   ├── SpecBlockExtractor.cs
│   └── Ast/
│       ├── Declarations.cs, StandardDeclarations.cs
│       ├── SystemDeclarations.cs, ContextDeclarations.cs
│       ├── DeploymentDeclarations.cs, ViewDeclarations.cs
│       ├── DesignDeclarations.cs, Expressions.cs
├── SpecChat.Mcp/                             (thin MCP tool layer)
│   ├── Program.cs                            (WithToolsFromAssembly)
│   └── Tools/
│       ├── StandardValidationTools.cs        (6 tools)
│       ├── SpecValidationTools.cs            (5 tools)
│       ├── SpecParsingTools.cs               (3 tools)
│       ├── ManifestTools.cs                  (4 tools)
│       └── RealizationTools.cs               (5 tools)
├── SpecChat.Language.Tests/                  (xUnit)
└── SpecChat.Mcp.Tests/                       (xUnit)
```

Four patterns from this codebase are directly relevant to the BTABOK work:

1. **Attribute-based MCP registration.** `[McpServerToolType]` on a static class, `[McpServerTool]` on static methods, `[Description(...)]` for schema. `WithToolsFromAssembly()` in `Program.cs` auto-discovers everything in the Tools folder.
2. **Conditional analyzer activation.** `SemanticAnalyzer.Analyze()` checks the parsed document for an `ArchitectureDecl` and delegates to `StandardSemanticAnalyzer` if present. This is the extension hook pattern.
3. **Partial-class parser extensions.** `Parser.cs` plus `Parser.Standard.cs` partition grammar so extension productions live next to core productions.
4. **Shared `DiagnosticBag`.** Every validator accumulates into the same bag. The bag is JSON-serialized at the tool boundary.

The Standard extension is a near-perfect precedent for how a BTABOK profile would plug in.

## 3. Requirements Mapped to Codebase Layers

After Option X, the surface area is split between Core SpecLang and the BTABOK profile. The following table assigns each requirement to one of two scopes.

| Requirement surface | Scope | Codebase layer affected |
|---|---|---|
| Standard metadata profile (slug, itemType, name, version, publishStatus, authors, reviewers, committer, tags, createdAt, updatedAt, retentionPolicy, freshnessSla, lastReviewed, dependencies) | Core | Lexer, AST (`CoreMetadataDeclarations.cs`), Parser core productions, `ManifestDocument` |
| Reference types (`ref<T>`, `weakRef`, `externalRef`) | Core | Lexer keywords, AST type nodes, Parser core productions |
| Relationship declarations (`uses`, `implements`, `supersedes`, `supersededBy`, `cardinality`) | Core | Lexer keywords, AST `RelationshipsBlock` and `RelationshipDecl`, Parser core productions |
| Retention policy enum (`indefinite`, `archive-on-deprecated`, `delete-on-superseded`) | Core | Lexer values, AST `RetentionPolicy` enum |
| Slug uniqueness and format rules | Core | Core validator logic |
| Diagnostic record extensions (`Code`, `Validator`, `Suggestion`) | Core | `Diagnostic` record; diagnostic code registry |
| Ten core validators (`check_metadata_completeness`, `check_slug_uniqueness`, `check_slug_format`, `check_reference_resolution`, `check_weakref_resolution`, `check_externalref_validity`, `check_freshness_sla`, `check_profile_composition`, `check_relationship_cardinality`, `check_supersedes_cycles`) | Core | `CoreMetadataAnalyzer` or extensions to `SemanticAnalyzer` |
| `CollectionIndex` cross-document resolution service | Core | New `SpecChat.Language/CollectionIndex.cs` |
| `ValidatorSeverityPolicy` and governance posture | Core | New `SpecChat.Language/ValidatorSeverityPolicy.cs` |
| Core manifest structure (profile declaration, typed inventory with slug and core metadata, dependency resolution) | Core | `ManifestDocument`, `ManifestParser` |
| `profile BTABOK { ... }` declaration | Profile | BTABOK-profile Lexer additions, AST, `Parser.Btabok.cs` |
| CoDL `concept { meta { ... } section { ... } relationships { ... } storage { ... } }` syntax | Profile | `Parser.Btabok.cs`, `BtabokDeclarations.cs` |
| 19 BTABOK concept types (GovernanceBody, StakeholderCard, CapabilityCard, RoadmapItem, RiskCard, ASRCard, etc.; 11 of the 19 are SpecChat-specific, 8 are BTABoK-standard) | Profile | `BtabokDeclarations.cs`, `BtabokSemanticAnalyzer` |
| `BTABoKItem` metadata extensions (accessTier, bokStatus, certainty, baseVersion, topicAreaId, etc.) | Profile | BTABOK manifest inventory columns, `BtabokDeclarations.cs` |
| CaDL `canvas { area { shows ... } }` syntax and 20 canvas renderings | Profile | Profile lexer, AST, `Parser.Btabok.cs`, canvas renderer |
| 13 BTABOK-specific validators | Profile | `BtabokSemanticAnalyzer` |
| BTABOK-specific diagnostic codes (BTABOK-ASR-001, etc.) | Profile | Registered in the core diagnostic registry under the `BTABOK-` prefix |
| BTABOK-profile manifest additions (type registry entries, profile-specific inventory columns, severity overrides scoped to profile validators) | Profile | Extensions to `ManifestDocument` parsing |
| `/spec-btabok` slash command | Outside codebase scope | Lives in `.claude/commands/` |

Diagnostic code prefixes: `SPEC-` for core validators, `STANDARD-` for The Standard profile validators, `BTABOK-` for BTABOK profile validators.

## 4. Layer-by-Layer Change Map

This section is organized by codebase layer. Each layer subsection splits into Core SpecLang additions and BTABOK-profile additions.

### 4.1 Lexer (`SpecChat.Language/Lexer.cs`)

#### 4.1.a Core SpecLang lexer additions

- **Reference-type tokens:** `ref`, `weakRef`, `externalRef`
- **Relationship verbs:** `uses`, `implements`, `supersedes`, `supersededBy`, `cardinality`
- **Retention policy enum values:** `indefinite`, `archive-on-deprecated`, `delete-on-superseded`
- **Metadata block introducer:** `meta` (as a core block starter for the standard metadata profile)

#### 4.1.b BTABOK-profile lexer additions

- **Top-level reserved:** `profile`, `concept`, `canvas`
- **Block-structure reserved:** `section`, `relationships`, `storage`, `envelope`, `item`, `area`
- **Contextual identifiers (recognized inside concept/canvas blocks only):**
  - Section modifiers: `list`, `set`, `map`, `matrix`, `timeline`, `optional`
  - Profile-specific primitive types: `currency`, `percentage`, `measurement`, `threshold`, `keyValue`, `contact`, `flags`, `range`, `semver`
  - CaDL directives: `for`, `shows`, `repeats`, `joins`, `format`, `output`, `freshnessCheck`, `collection`

Collision audit against the existing sample corpus is required before implementation. Some of these names (`for`, `item`, `list`) may already appear as identifiers in existing specs. The core additions have the same collision concern and should be audited together.

### 4.2 AST (`SpecChat.Language/Ast/`)

#### 4.2.a Core AST additions

New file `SpecChat.Language/Ast/CoreMetadataDeclarations.cs`. Proposed records:

- `MetaBlock(List<FieldDecl> Fields)` for the standard metadata profile
- `FieldDecl` extensions to carry reference-type nodes (`RefType`, `WeakRefType`, `ExternalRefType`)
- `RelationshipsBlock(List<RelationshipDecl> Relationships)`
- `RelationshipDecl(RelationshipKind Kind, string TargetConcept, string Role, Cardinality Cardinality)`
- `PersonRef(string UserId, string Name, string? Email, DateTime? AddedAt, string? AddedBy)`
- Enumerations: `RelationshipKind`, `Cardinality`, `RetentionPolicy`
- `Diagnostic` record gains optional `Code`, `Validator`, `Suggestion` fields (covered in 4.7)

Extension of existing `SpecDocument`: add `MetaBlock Metadata`, `RelationshipsBlock Relationships`, `ProfileDecl? Profile` (the profile declaration itself is core-visible but the body is profile-specific).

#### 4.2.b BTABOK AST additions

New file `SpecChat.Language/Ast/BtabokDeclarations.cs` mirroring `StandardDeclarations.cs`. Proposed records:

- `ProfileDecl(string Name, string Version, string CodlVersion, string CadlVersion)`
- `ConceptDecl(string Name, MetaBlock Meta, List<SectionDecl> Sections, RelationshipsBlock Relationships, StorageBlock Storage)`
- `SectionDecl(string Name, SectionModifier Modifier, List<FieldDecl> Fields, ItemDecl Item, List<Annotation> Annotations)`
- `StorageBlock(string PrimaryKey, List<string> IndexOn, string PartitionBy, List<string> ImmutableFields, List<string> AuditFields, RetentionPolicy RetentionPolicy)`
- `CanvasDecl(string Name, string TargetConcept, bool IsCollection, CanvasOptions Options, List<AreaDecl> Areas)`
- `AreaDecl(string Name, List<ShowsClause> Shows, RepeatsClause Repeats, string Format)`
- `ShowsClause(List<string> FieldPath, ShowsTransform Transform)`
- `BTABoKItem` metadata extension records
- Enumerations: `SectionModifier`, `AccessTier`, `BokStatus`

Extension of existing `SpecDocument` when BTABOK profile is active: add `List<ConceptDecl> Concepts`, `List<CanvasDecl> Canvases`.

### 4.3 Parser

#### 4.3.a Core Parser additions

Hosted in existing `Parser.cs` or in a new partial file `Parser.CoreMetadata.cs`. Productions:

- Metadata block parsing (`meta { slug: ..., itemType: ..., authors: [...] }`)
- Reference type parsing (`ref<T>`, `weakRef`, `externalRef` in field-type position)
- Relationship declaration parsing (`relationships { uses<T> as role cardinality(1..1) ... }`)
- Cardinality range parsing (`0..*`, `1..*`, `0..1`, `1..1`)

These productions run for every spec regardless of profile.

#### 4.3.b BTABOK Parser additions

New partial class `Parser.Btabok.cs` mirroring `Parser.Standard.cs`. Entry point: when the lexer produces a `profile`, `concept`, or `canvas` token at top level, the main parser delegates to the Btabok-specific productions.

Notable profile productions:
- `profile BTABOK { ... }` declaration block
- `concept { ... }` full body (meta, sections, relationships, storage)
- Composite profile types (`range<T>`, `flags(...)`, `enum(...)`, `measurement`, `threshold`)
- Display hint annotations (`@display(widget=..., colorMap={...})`)
- CaDL `canvas { area { shows ... } }` productions
- CaDL transform expressions (`shows: field -> relatedField`)

Parser coverage for these productions should be thorough. Each composite type warrants its own test suite.

### 4.4 Manifest Model (`SpecChat.Language/ManifestParser.cs`)

The manifest model splits by scope. The core structure (profile declaration field, typed inventory with slug and core metadata columns, dependency resolution, governance posture) is Core SpecLang. The BTABOK profile adds the type registry entries and profile-specific inventory columns (access tier, bok status, certainty, etc.).

The existing `ManifestDocument` record is extended to carry profile declaration, CoDL metadata, typed inventory, severity overrides, and governance posture. No backward-compatibility branch is needed; the manifest model is redefined outright.

Proposed record shape:

```csharp
public sealed record ManifestDocument(
    string System,
    string BaseSpec,
    string Target,
    int SpecCount,
    string SpecLangVersion,               // NEW per VD-1, semver starting 0.1.0
    ProfileInfo Profile,
    List<LifecycleState> LifecycleStates,
    TypeRegistryEntry[] TypeRegistry,
    List<SpecEntry> Inventory,
    List<ExecutionTier> ExecutionOrder,
    Dictionary<string, DiagnosticSeverity> SeverityOverrides,
    GovernancePosture GovernancePosture,
    string FilePath);

public sealed record ProfileInfo(
    string Name,                           // "Core", "TheStandard", or "BTABOK"
    string Version,                        // semver per VD-2; e.g. "0.1.0" or "1.0.0"
    string? CodlVersion,                   // decimal per upstream; required when Name is BTABOK
    string? CadlVersion);                  // decimal per upstream; required when Name is BTABOK

public sealed record TypeRegistryEntry(
    string TypeName,
    string CodlConcept,
    string? FilePattern,
    bool Required);

public enum GovernancePosture { Warnings, Strict }

public sealed record SpecEntry(
    string Filename,
    string CodlItemType,
    string Slug,
    string Name,
    string State,
    int Tier,
    bool EverGreen,
    RetentionPolicy RetentionPolicy,
    TimeSpan? FreshnessSla,
    DateOnly? LastReviewed,
    List<PersonRef> Authors,
    PersonRef Committer,
    List<string> Dependencies);

public sealed record PersonRef(
    string UserId,
    string Name,
    string? Email,
    DateTime? AddedAt,
    string? AddedBy);
```

`ManifestParser` is rewritten to produce this shape. Parsing responsibilities:
- Detect profile declaration (required; default implicit is Core if absent)
- Parse `specLangVersion` (required per VD-1)
- Validate semver format for `SpecLangVersion` and `Profile.Version` fields
- Parse the type registry table
- Parse the typed inventory table with all CoDL metadata columns
- Parse severity overrides table if present
- Parse governance posture from the Conventions section

### 4.5 Semantic Analyzers

#### 4.5.a Core validators

The 10 absorbed validators go into a new class `CoreMetadataAnalyzer` (or as extensions on `SemanticAnalyzer` directly). They run on every collection regardless of profile. Validators:

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

Diagnostic codes use the `SPEC-` prefix.

#### 4.5.b BTABOK-profile validators

New file `SpecChat.Language/BtabokSemanticAnalyzer.cs` following `StandardSemanticAnalyzer` layout. Method per validator. The 13 BTABOK-specific validators (enumerated in SpecLang-Design.md Section 6.4) cover ASR traceability, decision scope and cascades, principle links, stakeholder coverage, viewpoint coverage, waiver expiration and rule references, governance approval, roadmap capability moves, canvas target existence, and metric baseline/target.

Diagnostic codes use the `BTABOK-` prefix.

Activation hook in `WorkspaceAnalyzer.AnalyzeCollection()`. `WorkspaceAnalyzer` orchestrates collection-scoped work and delegates per-file analysis to `SemanticAnalyzer`:

```csharp
// Inside WorkspaceAnalyzer.AnalyzeCollection(manifestPath, ct):
var coreAnalyzer = new CoreMetadataAnalyzer(_diagnostics, _collectionIndex);
coreAnalyzer.AnalyzeCollection(manifest, _collectionIndex);

foreach (var document in parsedDocuments)
{
    _semanticAnalyzer.Analyze(document);      // per-file work stays on SemanticAnalyzer
}

if (manifest.Profile.Name == "BTABOK")
{
    var btabokAnalyzer = new BtabokSemanticAnalyzer(_diagnostics, _collectionIndex);
    btabokAnalyzer.AnalyzeCollection(manifest, parsedDocuments, _collectionIndex);
}
```

The one-profile-at-a-time rule is enforced by the core `check_profile_composition` validator, which rejects manifests declaring both TheStandard and BTABOK in the Profile field.

### 4.6 Collection-Level Analysis

All current validators operate on a single `SpecDocument`. Core reference-resolution validators (`check_reference_resolution`, `check_weakref_resolution`, `check_externalref_validity`, `check_supersedes_cycles`) need visibility across the whole collection. Because these validators are core, `CollectionIndex` becomes a Core SpecLang service. This is an explicit change from the pre-Option-X scope: the index is no longer a BTABOK-profile concern.

New service: `SpecChat.Language/CollectionIndex.cs`

Responsibilities:
- Load and parse the manifest
- For each inventory entry, parse the spec file to AST
- Build an index: `(codlItemType, slug) -> ConceptInstance`
- Expose `Resolve(refType, slug) -> ConceptInstance?`

New entry point on `WorkspaceAnalyzer`: `AnalyzeCollection(manifestPath, cancellationToken)`. `WorkspaceAnalyzer` is a new class introduced by D-04 Option B. `SemanticAnalyzer` remains the single-file analyzer and is delegated to by `WorkspaceAnalyzer` for per-file work. The existing single-spec `Analyze()` signature on `SemanticAnalyzer` is preserved for tools that operate per-file.

Performance: cache parsed specs by mtime inside the CollectionIndex. A cold Global Corp collection (21 specs) should parse in under 2 seconds on a typical developer machine; repeated validator calls should hit the cache.

### 4.7 Diagnostic Model

The `Diagnostic` record extensions (`Code`, `Validator`, `Suggestion`) are core additions. They apply uniformly to core validators, The Standard validators, and BTABOK-profile validators. The diagnostic code prefix space is split: `SPEC-` for core, `STANDARD-` for The Standard, `BTABOK-` for BTABOK profile.

Extend `Diagnostic` with three optional fields. All current emission sites continue to compile unchanged.

```csharp
public sealed record Diagnostic(
    DiagnosticSeverity Severity,
    string Message,
    SourceLocation Location,
    string? Code = null,
    string? Validator = null,
    string? Suggestion = null);
```

New service `SpecChat.Language/ValidatorSeverityPolicy.cs`:
- Loads severity overrides from the manifest
- Enforces override-direction rules (error cannot be demoted; warning and info can be promoted)
- Post-processes a `DiagnosticBag` when `GovernancePosture.Strict` is active (promotes all warnings to errors)

Post-processing happens at the tool boundary so validator methods stay uniform.

### 4.8 MCP Tools (`SpecChat.Mcp/Tools/`)

#### 4.8.a Core tool additions

Extensions or additions to existing tool classes. A new class `CoreMetadataValidationTools.cs` exposes the 10 absorbed core validators as individual MCP tools. `SpecValidationTools` is extended where core coverage overlaps with existing single-spec validation surfaces.

#### 4.8.b BTABOK-profile tool classes

New files:

- `BtabokValidationTools.cs`: 13 MCP tool methods, one per profile validator
- `BtabokProjectionTools.cs`: canvas rendering, initial tool `project_canvas(manifestPath, canvasName) -> renderedMarkdown`
- `BtabokMigrationTools.cs`: migration tooling, initial tool `migrate_to_btabok(manifestPath) -> migrationPlan`

Tool count grows from 23 to approximately 50 (10 core validators, 13 BTABOK validators, `validate_collection`, `project_canvas`, `migrate_to_btabok`, `get_supported_versions`, `get_deprecation_schedule`, `clear_cache`, plus rendering and meta-tools). Tool-namespace growth deserves attention but is not blocking for v0.1.

### 4.9 CaDL Rendering (profile concern)

Per Option Y deferral in SpecLang-Design.md Section 14, CaDL canvas rendering remains a BTABOK-profile concern for v0.1. The components below live with the profile code.

CaDL rendering is conceptually distinct from validation. Validation answers "is this spec correct?" Rendering answers "produce an artifact from this spec."

Proposed components (location undecided, see Section 7; all remain profile-scoped):

- `CanvasRenderer` orchestrator
- `MarkdownTableRenderer`
- `MermaidRenderer` (dispatches by diagram type: flowchart, quadrantChart, gantt, timeline)
- `CanvasRegistry` (the 20 built-in canvases from SpecLang-Design.md Section 6.5)

### 4.10 Test Fixtures and Test Projects

Fixtures split by scope.

Core validator fixtures go under `tests/fixtures/core/`. These cover all three profiles (Core, TheStandard, BTABOK) because the core validators run on every collection. Contents:
- Known-good minimal collections for each profile
- Per-validator known-bad cases for the 10 core validators

BTABOK-specific fixtures go under `tests/fixtures/btabok-profile/`:
- One known-good minimal BTABOK collection (manifest plus 3 to 5 specs)
- Per-validator known-bad cases for the 13 profile validators
- A migration-state fixture (Core manifest being migrated to BTABOK)

New test files:
- `SpecChat.Language.Tests/CoreMetadataAnalyzerTests.cs`
- `SpecChat.Language.Tests/WorkspaceAnalyzerTests.cs` (core)
- `SpecChat.Language.Tests/CollectionIndexTests.cs` (core)
- `SpecChat.Language.Tests/ValidatorSeverityPolicyTests.cs` (core)
- `SpecChat.Language.Tests/VersionCompatibilityTests.cs` (core)
- `SpecChat.Language.Tests/VersionMonotonicityTests.cs` (core)
- `SpecChat.Language.Tests/BtabokLexerTests.cs`
- `SpecChat.Language.Tests/BtabokParserTests.cs`
- `SpecChat.Language.Tests/BtabokSemanticAnalyzerTests.cs`
- `SpecChat.Mcp.Tests/CoreMetadataValidationToolsTests.cs`
- `SpecChat.Mcp.Tests/BtabokValidationToolsTests.cs`

xUnit framework and existing `FixturePath` helper carry over unchanged.

## 5. Phasing

Six phases, each producing a shippable increment. All architectural decisions that shape this phasing are settled (see Section 7 and the Decisions Record).

### Phase 1: Foundation (expanded)

Phase 1 is larger than originally planned. Lexer and AST work originally scoped to the BTABOK profile now includes core additions for reference types, relationship declarations, and the standard metadata profile. Contents:

- Core lexer additions: reference-type tokens (`ref`, `weakRef`, `externalRef`), relationship verbs (`uses`, `implements`, `supersedes`, `supersededBy`, `cardinality`), retention policy enum values, `meta` block starter
- Core AST additions: `CoreMetadataDeclarations.cs` (MetaBlock, reference-type FieldDecl extensions, RelationshipsBlock, RelationshipDecl, PersonRef, RetentionPolicy enum)
- BTABOK lexer additions: `profile`, `concept`, `canvas`, block-structure reserved tokens, profile-scoped contextual identifiers
- BTABOK AST additions: `BtabokDeclarations.cs` (ProfileDecl, ConceptDecl, SectionDecl, StorageBlock, CanvasDecl, AreaDecl)
- Core Parser additions (metadata blocks, reference types, relationship declarations)
- BTABOK Parser partial class `Parser.Btabok.cs`
- Diagnostic record extension (`Code`, `Validator`, `Suggestion` fields) as a core change
- `specLangVersion` field added to `ManifestDocument` record per VD-1
- Validator constants for supported SpecLang and profile version ranges
- Diagnostic code registry covering SPEC-, STANDARD-, BTABOK-, and SPEC-VER- prefixes, shipped as both a `SpecChat.Language/DiagnosticCodes.cs` static class and a markdown reference document (per V2-6 promoted from D-05)
- `get_supported_versions` MCP tool per VD-3
- `get_deprecation_schedule` MCP tool per VD-4
- Core manifest structure redefinition; BTABOK-specific manifest extensions
- Test coverage for lexer, parser, manifest at both scopes

Ship condition: parses a core-profile manifest and a BTABOK-profile manifest plus a concept-containing spec into a clean AST without error. No validation logic yet.

### Phase 2a: Core validators

- `CoreMetadataAnalyzer` class (or extensions to `SemanticAnalyzer`)
- The 10 absorbed core validators: `check_metadata_completeness`, `check_slug_uniqueness`, `check_slug_format`, `check_reference_resolution`, `check_weakref_resolution`, `check_externalref_validity`, `check_freshness_sla`, `check_profile_composition`, `check_relationship_cardinality`, `check_supersedes_cycles`
- Versioning validators `check_version_compatibility` (per VD-3) and `check_version_monotonicity` (per VD-5), both using the `SPEC-VER-` diagnostic prefix
- `CoreMetadataValidationTools` MCP tool class exposing the core validators
- `validate_collection` convenience wrapper tool per D-06
- Fixtures under `tests/fixtures/core/` covering all three profiles (Core, TheStandard, BTABOK)
- Sample collection migration: `blazor-harness`, `TodoApp`, `PizzaShop`, `todo-app-the-standard` updated to satisfy core metadata requirements (slug, retentionPolicy, freshnessSla, authors, committer, ref<T> conversions). Core validators run against these samples as part of this phase.

Ship condition: existing sample collections pass the 10 core validators.

### Phase 2b: Core infrastructure

- `CollectionIndex` service implemented as a core service
- `ValidatorSeverityPolicy` service implemented as a core service, covering governance posture and severity overrides
- Diagnostic code registry seeded with SPEC- prefix entries for the 10 core validators

Ship condition: CollectionIndex operational, severity overrides applied correctly at the tool boundary.

### Phase 2c: BTABOK-profile validators

- `BtabokSemanticAnalyzer` class
- The 13 BTABOK-specific validators: `check_asr_traceability`, `check_asr_addressed_by_decision`, `check_decision_scope_type`, `check_decision_cascades`, `check_principle_links`, `check_stakeholder_coverage`, `check_viewpoint_coverage`, `check_waiver_expiration`, `check_waiver_rule_reference`, `check_governance_approval`, `check_roadmap_capability_moves`, `check_canvas_target_exists`, `check_metric_baseline_target`
- `BtabokValidationTools` MCP tool class with 13 tools
- Fixtures under `tests/fixtures/btabok-profile/` for the 13 profile validators

Ship condition: Global Corp exemplar passes all 23 validators (10 core + 13 BTABOK-profile).

### Phase 3: Additional BTABOK-profile validators

- Any BTABOK-profile validators beyond the first 13 that are added in later increments (for example, additional cascade or coverage rules that emerge from Global Corp usage)
- Corresponding known-good and known-bad fixtures

Ship condition: Global Corp exemplar passes the expanded validator set.

### Phase 4: Advanced features

- Severity override mechanism exposed via manifest (full surface)
- Treat-warnings-as-errors flag
- Additional cycle and cascade detection for BTABOK-specific chains

Ship condition: BTABOK Profile v0.1 feature complete on the validator surface.

### Phase 5: CaDL rendering

- CanvasRenderer and CanvasRegistry
- The 20 canvas renderings (possibly split into 5a and 5b by canvas complexity)
- project_canvas MCP tool
- Per-canvas fixture tests

Ship condition: all 20 canvases render against the Global Corp exemplar.

### Phase 6: Migration tooling

- migrate_to_btabok MCP tool
- MigrationPlan concept and tracking
- Exercised against the blazor-harness, TodoApp, PizzaShop sample collections

Ship condition: existing sample collections can be migrated to the BTABOK profile via tooling.

## 6. Risks

| Risk | Impact | Mitigation |
|---|---|---|
| Keyword collisions with existing SpecLang identifiers | Existing specs fail to parse | Pre-implementation audit against the sample corpus; keep most CoDL names contextual, not globally reserved |
| CollectionIndex performance for large collections | Validators become slow | Cache parsed specs by mtime; measure Global Corp collection timing as benchmark |
| Cross-profile `ref<T>` resolution (BTABOK spec referencing a Core spec) | Previously weakly defined | Settled per D-07 Option B: cross-collection references use `weakRef`; `ref<T>` is same-collection only |
| Diagnostic code sprawl without a registry | Inconsistent codes, duplicates | Publish a diagnostic code reference file; enforce registration in tests |
| CaDL rendering complexity grows beyond the 20 canvases | Rendering surface bloats | Keep the canvas registry explicit; new canvases are additions, not core |
| User-visible tool namespace grows to 50+ | Cognitive load for integrators | Document tool groupings in MCP server.json; revisit a catalog tool later |
| The Standard and BTABOK both evolving | Maintenance divergence | Shared utilities (DiagnosticBag, SourceLocation, CollectionIndex) stay in SpecChat.Language core; extensions consume them |
| Parser productions for rich CoDL types (range, measurement, threshold, flags) | Subtle parse bugs | Per-type parser test suite; fuzz against handwritten concept definitions |

## 7. Settled Decisions

Every architectural decision that was previously open in this section is now settled. The authoritative record is [SpecChat-Design-Decisions-Record.md](SpecChat-Design-Decisions-Record.md); the versioning sub-decisions are formalized in [SpecChat-Versioning-Policy.md](SpecChat-Versioning-Policy.md). This section provides a reference-style summary so readers of this design doc can see the outcomes in one place.

### 7.1 Tier 1: Pre-Phase-1 architectural decisions

| ID | Decision | Chosen | Impact on this doc |
|---|---|---|---|
| D-01 | Co-locate BTABOK in SpecChat.Language | A | Section 4 structure; no new projects introduced |
| D-03 | Reserved keywords versus contextual identifiers | B | Section 4.1; ten top-level words reserved, field-level tokens contextual |
| D-04 | Collection-level analysis entry point | B | Sections 4.5, 4.6; new `WorkspaceAnalyzer` class |
| D-11 | Profile declaration placement | A | Section 4.5; profile is manifest-only |
| D-13 | Core metadata required fields strictness | A | Section 4.5, Phase 2a sample migration |
| D-14 | PersonRef source | A | Section 4.2; `PersonRef` is inline, no directory concept |
| D-15 | Retention policy default when unspecified | B | Core validator logic; `SPEC-RET-001` info diagnostic |

### 7.2 Tier 2: Other architectural decisions

| ID | Decision | Chosen | Impact on this doc |
|---|---|---|---|
| D-02 | CaDL rendering location | A | Section 4.9; `SpecChat.Language/Rendering/` |
| D-05 | Diagnostic code governance | B | Section 4.7, Phase 1; registry in Phase 1 as class and markdown |
| D-06 | Tool set growth | A | Section 4.8; one tool per validator plus `validate_collection` wrapper |
| D-07 | Cross-profile reference policy | B | Section 6 risk row; `weakRef` for cross-collection, `ref<T>` same-collection only |
| D-08 | CaDL rendering output targets | A | Section 4.9; Markdown plus Mermaid only in v0.1 |
| D-09 | Treat-warnings-as-errors trigger surface | D | Manifest floor with CLI-only escalation; no de-escalation |
| D-10 | CollectionIndex caching invalidation | B | Section 4.6; mtime-based cache plus `clear_cache` MCP tool |
| D-12 | Versioning of BTABOK profile | B | BTABOK profile uses semver (`0.1.0`) |
| D-16 | CollectionIndex initialization timing | A | Section 4.6; lazy build on first validator invocation |

### 7.3 Versioning policy (VD-1 through VD-5)

| ID | Decision | Chosen | Impact on this doc |
|---|---|---|---|
| VD-1 | Core SpecLang version declaration | B | `specLangVersion` in manifest, semver, starting `0.1.0`; added to `ManifestDocument` record |
| VD-2 | Profile version scheme | A | All SpecChat-owned profiles use semver; CoDL and CaDL retain upstream decimal schemes |
| VD-3 | Compatibility declaration | D | Warnings-first; strict via CLI escalation; `check_version_compatibility` in Phase 2a; `get_supported_versions` tool in Phase 1 |
| VD-4 | Migration policy | B | Opt-in migration with deprecation schedule; `get_deprecation_schedule` tool in Phase 1 |
| VD-5 | Instance version evolution | D | Author discretion with `check_version_monotonicity` soft check (`SPEC-VER-005`) in Phase 2a |

### 7.4 Naming and convention

| ID | Decision | Chosen | Impact on this doc |
|---|---|---|---|
| SD-04 | Core validator naming convention | C | All validators use `check_` prefix |

### 7.5 Sub-decisions consolidated into D-series

The sub-decisions originally tagged SD-01, SD-02, SD-03, and SD-05 are now captured as D-13, D-14, D-15, and D-16 respectively in the Decisions Record:

- D-13 (formerly SD-01): All CoDL-required core metadata fields are required on every spec immediately. Migration mode (`migrationFrom` manifest field) handles the Phase 2a transition for existing samples.
- D-14 (formerly SD-02): `PersonRef` is inline in every spec. No separate Person concept.
- D-15 (formerly SD-03): Retention policy default is by spec type: base System Spec, Manifest, Subsystem Spec, Governance Spec, Viewpoint Catalog, Standards Catalog, Principles, and Risks default to `indefinite`; Feature Spec, Bug Spec, Amendment Spec, Decision Record, Waiver Record, Roadmap Item, Experiment Card, and Legacy Modernization Record default to `archive-on-deprecated`. The validator emits `SPEC-RET-001` at info level when a default is applied.
- D-16 (formerly SD-05): `CollectionIndex` is built lazily on first validator invocation and cached thereafter.

### 7.6 Bundle dispositions

All seven Option Y items (publishStatus vocabulary alignment, canvas-as-core, Decision Spec enrichment in core, viewpoint templates in core, ASR-as-core, MetricDefinition-as-core, ExperimentCard-as-core) remain deferred. Six v0.2 items (V2-1 profile composition, V2-2 Value Model profile, V2-3 People Model profile, V2-4 bidirectional CoDL export, V2-5 transport envelope, V2-7 additional canvases) remain deferred. V2-6 (diagnostic code reference document) was promoted to Phase 1 as a consequence of D-05 Option B.

For full rationale and cross-references on any of the above, see [SpecChat-Design-Decisions-Record.md](SpecChat-Design-Decisions-Record.md).

## 8. Next Steps

1. Architectural decisions are now fully settled (see Section 7 and the Decisions Record). Implementation can proceed.
2. Recommended starting point: Phase 1 (Foundation) as described in Section 5.
3. Mechanical preparatory item: the keyword collision audit remains pending. Verify that the ten reserved words (`profile`, `concept`, `canvas`, `section`, `meta`, `relationships`, `storage`, `envelope`, `item`, `area`) do not appear as identifiers in any existing sample spec (`blazor-harness`, `TodoApp`, `PizzaShop`, `todo-app-the-standard`). Any collisions trigger a small sample edit to rename the identifier.

## 9. Appendix: Source References

**[R1]** SpecLang Design. Workspace: [SpecLang-Design.md](SpecLang-Design.md). Consolidated design covering Core SpecLang, the BTABOK profile, CoDL and CaDL alignment, and the Engagement Model scope. Supersedes and replaces the prior WIP sources BTABOK-Profile-v0.1-Design.md, Core-SpecLang-Absorption-Design.md, CoDL-CaDL-Integration-Notes.md, and BTABOK-EngagementModel-Mapping.md (all now in `WIP/Archive/`).

**[R2]** Spec Type System. Workspace: [Spec-Type-System.md](Spec-Type-System.md). Consolidated design covering the spec type taxonomy, rationale, and validation architecture. Supersedes Spec-Type-Taxonomy-v0.1.md, Spec-Type-Validation-Analysis.md, and Why-We-Created-the-Spec-Type-Taxonomy.md (all now in `WIP/Archive/`).

**[R3]** Global Corp Exemplar. Workspace: [Global-Corp-Exemplar.md](Global-Corp-Exemplar.md). The canonical BTABOK-complete worked example.

**[R4]** SpecChat Overview. Workspace: `Delivery/spec-chat/SpecChat-Overview.md`.

**[R5]** SpecLang Specification. Workspace: `Delivery/spec-chat/SpecLang-Specification.md`.

**[R6]** SpecLang Grammar. Workspace: `Delivery/spec-chat/SpecLang-Grammar.md`.

**[R7]** The Standard Extension Overview. Workspace: `Delivery/spec-chat/extensions/the-standard/TheStandard-Extension-Overview.md`.

**[R8]** Codebase analysis performed 2026-04-17 via Explore agent against `src/MCPServer/DotNet/`.

**[R9]** Prior historical analysis. Workspace: `WIP/Archive/BTA-BOK-integration.md`. The original broad-scope gap analysis that the scoped Engagement Model mapping (now folded into SpecLang-Design.md) replaced.
