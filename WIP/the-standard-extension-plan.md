# Plan: Integrate The Standard Extension into SpecChat MCP Server

## Context

The Standard extension (3 files in `Delivery/spec-chat/extensions/the-standard/`) defines an opt-in architectural extension to SpecChat for Hassan Habib's "The Standard." It adds 18 keywords, ~10 new AST nodes, layer-prefixed declaration forms, and 6 semantic validation rule sets. None of this is currently implemented in the MCP Server at `src/MCPServer/DotNet/`. The goal is to integrate it cleanly so that:
- Existing specs without `architecture` declarations parse and validate identically (no breaking behavior)
- Specs with `architecture TheStandard { ... }` get full parsing and enforcement
- The extension code is clearly separated from base language code in the file structure

## Recommended Approach: Separate Files Within the Same Project

Add new files for extension-specific logic, modify existing files only at well-defined integration points. No plugin infrastructure needed. Uses C# partial classes for the parser.

**Why not a separate assembly?** The `TokenKind` enum, the `FrozenDictionary` keyword table, and the parser's switch dispatch all require compile-time membership in the same project. A plugin system would require abstracting all three, which is over-engineering for one extension.

**Why not inline everything?** The extension adds ~18 keywords, ~8 parse methods, 6 semantic rules, and 6 MCP tool endpoints. Scattering this across existing files makes it hard to see what belongs to base vs. extension.

---

## Implementation Steps

### Step 1: Lexer Keywords

**Modify `SpecChat.Language/TokenKind.cs`**
- Add 18 enum members in a new `// The Standard extension keywords` section

**Modify `SpecChat.Language/Lexer.cs`**
- Add 18 entries to the `Keywords` frozen dictionary in a new commented block

Keywords: `architecture`, `enforce`, `vocabulary`, `layer`, `owns`, `broker`, `foundation`, `processing`, `orchestration`, `coordination`, `aggregation`, `exposer`, `test`, `service`, `layer_contract`, `realize`, `validation`, `suppress`

### Step 2: AST Nodes

**Create `SpecChat.Language/Ast/StandardDeclarations.cs`** (new file)
- `ArchitectureDecl : TopLevelDecl` with Name, Version, EnforceRules, Vocabulary, Realizations, Rationales
- `VocabularyDecl : AstNode` with list of VocabularyMapping
- `VocabularyMapping : AstNode` with LayerName and Verbs list
- `RealizeDecl : AstNode` with LayerName and Directives list
- `LayerContractDecl : TopLevelDecl` with Name, LayerName, and contract Clauses

**Modify `SpecChat.Language/Ast/SystemDeclarations.cs`**
- Extend `ComponentDecl` record (line 46) with 3 new properties: `Layer` (string?), `Owns` (string?), `Suppressions` (List<string>)
- Exactly 2 construction sites in `Parser.cs` (lines 761, 850) need updating to pass default values (`null, null, []`)
- No `ComponentDecl` constructors exist in test files or MCP tool code (verified by grep)

**Modify `SpecChat.Language/Ast/Declarations.cs`**
- Extend `ContractClause` record (line 108) with `ValidationCategory` (string?) for the `@validation` annotation
- Exactly 3 construction sites in `Parser.cs` (lines 476, 482, 488) need updating to pass `null`

### Step 3: Parser

**Modify `SpecChat.Language/Parser.cs`**
- Make class `partial` (line 9: `public sealed class Parser` becomes `public sealed partial class Parser`)
- Add `KwArchitecture` and `KwLayerContract` arms to `ParseTopLevelDecl()` switch (line 236)
- Add them to `IsTopLevelKeyword()` (line 105)
- Add all 18 new keywords to `IsContextualKeyword()` (line 121) so they work as field names in entity bodies
- In `ParseSystemDecl()` body loop (line 645): add layer-prefixed dispatch **after** the existing `authored`/`consumed` checks (lines 661-668) and **before** the `else` error fallthrough (line 669). Order matters for correct disambiguation.
- In `ParseAuthoredComponentDecl()` body loop (line 706): add `layer:`, `owns:`, `@suppress()` parsing branches
- In `ParseContractClauses()` (line 466): for `requires` clauses, check for `@validation(...)` **between** `ParseExpr()` and `Expect(Semicolon)`. Verified safe: `@` (TokenKind.At) is not a binary operator, comparison operator, or dot accessor, so the expression parsing chain (ParseImpliesExpr -> ParseOrExpr -> ParseAndExpr -> ParseCompareExpr -> ParseAccessExpr -> ParsePrimaryExpr) returns cleanly without consuming it.

**Create `SpecChat.Language/Parser.Standard.cs`** (new file, partial class)
- `ParseArchitectureDecl()` -- top-level architecture block
- `ParseLayerContractDecl()` -- top-level layer_contract block
- `ParseLayerPrefixedDecl(string layer)` -- broker/exposer/test desugaring to ComponentDecl
- `ParseServiceDecl(string layer)` -- foundation/processing/orchestration/coordination/aggregation service desugaring
- `ParseVocabularyDecl()` -- vocabulary block within architecture
- `ParseRealizeDecl()` -- realize block within architecture
- `ParseLayerKeywordOrIdent()` -- helper that accepts any layer keyword token OR plain Ident, returning the text
- `ParseEnforceList()` -- parses `[ident_or_keyword, ...]`; cannot reuse `ParseIdentList()` (see Risk #4)

All methods share the same instance state (_tokens, _pos, Peek(), Advance(), Expect(), etc.) via partial class.

**Note on `ExpectName()` (line 175):** Currently only accepts `Ident` and `DottedIdent`. Layer-prefixed parse methods in `Parser.Standard.cs` must use `ParseLayerKeywordOrIdent()` or consume keyword tokens directly. This is consistent with existing behavior -- the base language already reserves words like `system`, `topology`, `component` that cannot be used as entity names via `ExpectName()`.

**Note on `ParseIdentList()` (line 880):** Also only accepts `Ident`/`DottedIdent`. Cannot be reused for any list that may contain keyword-matching values (e.g., the `enforce` list, vocabulary layer names). New extension-specific list parsers are needed.

### Step 4: Semantic Analyzer

**Create `SpecChat.Language/StandardSemanticAnalyzer.cs`** (new file)
- Entry point: `Analyze(SpecDocument, ArchitectureDecl)` gates on enforce list
- 6 public check methods, each independently callable:
  - `CheckLayers()` -- every authored component has valid layer property
  - `CheckFlowForward()` -- no lateral/upward dependency calls (test exempt)
  - `CheckFlorance()` -- orchestration has 2-3 same-layer service deps
  - `CheckEntityOwnership()` -- foundation services own exactly one entity
  - `CheckAutonomy()` -- no horizontal entanglement (respects @suppress)
  - `CheckVocabulary()` -- contract verbs match layer vocabulary
- Static helper: `WarnLayerPrefixedWithoutArchitecture()` for specs using broker/service keywords without an architecture declaration

**Modify `SpecChat.Language/SemanticAnalyzer.cs`**
- At end of `Analyze()` (after line 27, `CheckCrossReferences`), find `ArchitectureDecl` in document; if present, delegate to `StandardSemanticAnalyzer`
- In `CollectAllKnownIdentifiers()` (line 372): add cases for `ArchitectureDecl` and `LayerContractDecl` to the switch

### Step 5: MCP Tools

**Create `SpecChat.Mcp/Tools/StandardValidationTools.cs`** (new file)
- 6 `[McpServerTool]` endpoints: CheckLayers, CheckFlowForward, CheckFlorance, CheckEntityOwnership, CheckAutonomy, CheckVocabulary
- Auto-discovered by existing `WithToolsFromAssembly()` in Program.cs (line 12)

**Modify `SpecChat.Mcp/Tools/SpecParsingTools.cs`** -- two separate code paths need updates:
- `ListConstructs()` (line 80): add `architectures` and `layerContracts` lists to the switch (line 107) and the result dictionary (line 151)
- `SummarizeDeclaration()` (line 181): add cases for `ArchitectureDecl` and `LayerContractDecl` to the switch (line 189)

**Modify `SpecChat.Mcp/Tools/RealizationTools.cs`** -- three impacts:
- `SerializeContract()` (line 395): include `ValidationCategory` in clause serialization when non-null
- `GenerateScaffold()` (line 236): emit `Layer` property for authored components when present (line 277)
- `SpecToInterface()` (line 351): optionally add a case for `LayerContractDecl` name lookup

### Step 6: Tests

**New test files:**
- `SpecChat.Language.Tests/StandardLexerTests.cs` -- keyword tokenization, verify keywords inside strings are unaffected
- `SpecChat.Language.Tests/StandardParserTests.cs` -- all new productions, desugaring equivalence, enforce list with keyword values
- `SpecChat.Language.Tests/StandardSemanticAnalyzerTests.cs` -- each rule set, enforce gating, suppress
- `SpecChat.Mcp.Tests/StandardValidationToolsTests.cs` -- MCP endpoint integration tests

**Existing fixture:** `tests/fixtures/standard-test.spec.md` already exists with architecture, vocabulary, and system declarations using Standard extension syntax. No existing test references it (verified by grep). New tests should use this fixture.

**New fixture files** in `tests/fixtures/`:
- `standard-broken-layers.spec.md` -- layer violations
- `standard-broken-flow.spec.md` -- flow forward violations

---

## Baseline Verification (pre-implementation)

Verified clean baseline before any changes:
- `dotnet test`: 55 tests (39 Language + 16 Mcp), all passing, 0 warnings, 0 errors
- `validate_spec` on TodoApp.spec.md: 0 diagnostics, hasErrors: false
- `parse_spec` on TodoApp.spec.md: 18 constructs parsed, 0 diagnostics
- Keyword clash check on TodoApp.spec.md: words like `test`, `service`, `layer`, `validation` appear only inside string literals or markdown prose, never as bare identifiers in structural positions. No backward compatibility issue.
- Keyword clash check on blazor-harness.spec.md: same result -- `test`, `service`, `layer` only inside strings/prose.
- `standard-test.spec.md` fixture exists but is not referenced by any existing test.

---

## Files Summary

| Action | File | Scope |
|--------|------|-------|
| Modify | `SpecChat.Language/TokenKind.cs` | +18 enum members |
| Modify | `SpecChat.Language/Lexer.cs` | +18 dictionary entries |
| Create | `SpecChat.Language/Ast/StandardDeclarations.cs` | 5 new AST node types |
| Modify | `SpecChat.Language/Ast/SystemDeclarations.cs` | +3 fields on ComponentDecl (2 call sites) |
| Modify | `SpecChat.Language/Ast/Declarations.cs` | +1 field on ContractClause (3 call sites) |
| Modify | `SpecChat.Language/Parser.cs` | partial, dispatch arms, contextual keywords, property/annotation parsing |
| Create | `SpecChat.Language/Parser.Standard.cs` | 8 new parse methods (incl. ParseEnforceList) |
| Create | `SpecChat.Language/StandardSemanticAnalyzer.cs` | 6 rule-set check methods |
| Modify | `SpecChat.Language/SemanticAnalyzer.cs` | +delegation call, +2 switch cases |
| Create | `SpecChat.Mcp/Tools/StandardValidationTools.cs` | 6 MCP tool endpoints |
| Modify | `SpecChat.Mcp/Tools/SpecParsingTools.cs` | +2 cases in ListConstructs, +2 cases in SummarizeDeclaration |
| Modify | `SpecChat.Mcp/Tools/RealizationTools.cs` | +ValidationCategory serialization, +Layer in scaffold, +LayerContractDecl in SpecToInterface |
| Create | `SpecChat.Language.Tests/StandardLexerTests.cs` | keyword tests |
| Create | `SpecChat.Language.Tests/StandardParserTests.cs` | production + desugaring tests |
| Create | `SpecChat.Language.Tests/StandardSemanticAnalyzerTests.cs` | rule-set tests |
| Create | `SpecChat.Mcp.Tests/StandardValidationToolsTests.cs` | MCP integration tests |

## Risks

1. **ComponentDecl record breakage** -- Adding 3 positional parameters breaks exactly 2 call sites in Parser.cs (lines 761, 850). No test files or MCP tools construct ComponentDecl directly (verified). Mitigate by updating both sites in the same commit.
2. **ContractClause record breakage** -- Adding 1 positional parameter breaks exactly 3 call sites in Parser.cs (lines 476, 482, 488). Same mitigation.
3. **Contextual keyword collisions** -- Words like `broker`, `test`, `service` becoming keywords means `ExpectName()` (line 175, only accepts Ident/DottedIdent) won't accept them. Layer-prefixed parse methods must use `ParseLayerKeywordOrIdent()` or consume keyword tokens directly. This is consistent with existing behavior: the base language already reserves `system`, `topology`, `component`, etc. Similarly, `ParseIdentList()` (line 880) only accepts Ident/DottedIdent, so any list containing keyword-matching values needs its own parser.
4. **`enforce` list contains `vocabulary` which is a keyword** -- In `enforce: [layers, flow_forward, ..., vocabulary]`, the value `vocabulary` will lex as `KwVocabulary` (not `Ident`). `ParseIdentList()` would reject it. Solution: `ParseArchitectureDecl()` must use a dedicated `ParseEnforceList()` that accepts both Ident and keyword tokens. Same applies to vocabulary mapping layer names (`broker:`, `foundation:`, etc.) which must use `ParseLayerKeywordOrIdent()`.
5. **@validation parsing safety** -- Verified safe: `ParseExpr()` returns cleanly when it encounters `@` because `TokenKind.At` is not a binary operator, comparison operator, or dot accessor. The expression chain (ParseImpliesExpr -> ... -> ParsePrimaryExpr) never reaches `ParsePrimaryExpr` for the `@` token since it stops at the binary/comparison level. `@` would only be consumed by `ParsePrimaryExpr` if it appeared at the start of an expression position (malformed input).
6. **@suppress disambiguation** -- Parsed in a dedicated code path within `ParseAuthoredComponentDecl()`, not through the generic `ParseAnnotation()` method. The grammar spec confirms this is correct.

## Post-Implementation Verification

1. `dotnet build` -- all projects compile
2. `dotnet test` -- all 55 existing tests pass (backward compatibility), all new tests pass
3. `validate_spec` on TodoApp.spec.md -- 0 diagnostics (regression check)
4. `parse_spec` on TodoApp.spec.md -- 18 constructs, 0 diagnostics (regression check)
5. Parse `standard-test.spec.md` fixture -- architecture, vocabulary, system with authored components produce correct AST
6. Parse a new Standard-enabled fixture with layer-prefixed declarations -- broker, foundation service, etc. desugar to ComponentDecl with correct Layer values
7. Validate a Standard fixture -- all 6 rule sets fire correctly based on enforce list
8. MCP tools -- new StandardValidation tools appear in tool list and return correct JSON
9. ListConstructs and SummarizeDeclaration report architecture and layer_contract declarations
10. ExtractContracts includes ValidationCategory when present
11. GenerateScaffold includes Layer property for authored components
