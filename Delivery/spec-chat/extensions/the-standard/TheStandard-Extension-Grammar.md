# SpecLang Grammar Extension: The Standard Architecture

**Version:** 0.1
**Date:** 2026-04-04
**Companion to:** TheStandard-Extension-Specification.md
**Extends:** SpecLang-Grammar.md

---

## Purpose

This document defines the grammar additions required to support the architectural extension described in TheStandard-Extension-Specification.md. It follows the same conventions as the base grammar: EBNF notation, lexer token definitions first, then production rules, then ambiguity resolution.

All additions are additive. Existing specs that do not contain an `architecture` declaration parse identically to before. The `requires` alternative in `ContractClause` is extended with an optional `ValidationAnnotation`; this is a superset change, not a breaking one, since existing inputs (without the annotation) continue to parse unchanged.

---

## 1. New Lexer Tokens

### 1.1 New keywords

The following keywords are added to the lexer's keyword table. They are recognized only as keywords; they do not affect DOTTED_IDENT resolution (per base grammar rule 1.4.2).

#### Architecture keywords

```
KW_ARCHITECTURE = "architecture"
KW_ENFORCE      = "enforce"
KW_VOCABULARY   = "vocabulary"
```

#### Layer property keywords

```
KW_LAYER        = "layer"
KW_OWNS         = "owns"
```

#### Standard declaration keywords

These keywords enable layer-prefixed declaration forms that use The Standard's vocabulary. They double as layer identifiers in `LayerValue` positions.

```
KW_BROKER       = "broker"
KW_FOUNDATION   = "foundation"
KW_PROCESSING   = "processing"
KW_ORCHESTRATION = "orchestration"
KW_COORDINATION = "coordination"
KW_AGGREGATION  = "aggregation"
KW_EXPOSER      = "exposer"
KW_TEST         = "test"
KW_SERVICE      = "service"
```

#### Contract and directive keywords

```
KW_LAYER_CONTRACT = "layer_contract"
KW_REALIZE        = "realize"
```

#### Annotation keywords

```
KW_VALIDATION   = "validation"
KW_SUPPRESS     = "suppress"
```

### 1.2 Enforce rule-set identifiers

Enforce rule-set identifiers (`layers`, `flow_forward`, `florance`, `entity_ownership`, `autonomy`, `vocabulary`) are parsed as IDENT tokens and validated semantically. They are NOT keywords because they never appear in a position where the parser must distinguish them from identifiers at the lexer level. Note: `florance` is the canonical spelling from The Standard (sec. 2.0.2.1, "Two-Three (Florance Pattern)"), not a misspelling of "Florence."

### 1.3 Contextual keyword additions

The following tokens are added to the ContextualKeyword production (base grammar sec. 3.2) so they can appear as field names in entity bodies without parse conflicts:

The extended `ContextualKeyword` production is listed here in full, including all base alternatives and the new additions (marked with comments):

```ebnf
ContextualKeyword = "source" | "version" | "target" | "kind"
                  | "responsibility"
                  | "rule" | "format" | "command" | "scope"
                  | "context" | "decision" | "consequence"
                  | "supersedes" | "startup" | "folder"
                  | "projects" | "gate" | "phase" | "trace"
                  | "constraint" | "component"
                  | "path" | "status" | "existing" | "new"
                  | "page" | "host" | "route" | "concepts"
                  | "role" | "cross_links"
                  | "visualization" | "parameters" | "sliders"
                  | "architecture"                              (* NEW *)
                  | "enforce"                                   (* NEW *)
                  | "vocabulary"                                (* NEW *)
                  | "layer"                                     (* NEW *)
                  | "owns"                                      (* NEW *)
                  | "validation"                                (* NEW *)
                  | "suppress"                                  (* NEW *)
                  | "broker"                                    (* NEW *)
                  | "foundation"                                (* NEW *)
                  | "processing"                                (* NEW *)
                  | "orchestration"                             (* NEW *)
                  | "coordination"                              (* NEW *)
                  | "aggregation"                               (* NEW *)
                  | "exposer"                                   (* NEW *)
                  | "test"                                      (* NEW *)
                  | "service"                                   (* NEW *)
                  | "layer_contract"                            (* NEW *)
                  | "realize" ;                                 (* NEW *)
```

These words are common enough in domain vocabulary (e.g., a `Building` entity might have a field named `layer`) that they must be accepted as field names inside entity and refinement bodies.

### 1.4 Collision analysis

The new keywords do not collide with any existing keyword in the base grammar:

| New Keyword | Nearest Existing | Conflict? |
|---|---|---|
| `architecture` | (none) | No |
| `enforce` | (none) | No |
| `vocabulary` | (none) | No |
| `layer` | (none) | No |
| `owns` | (none) | No |
| `validation` | (none) | No |
| `suppress` | (none) | No |
| `broker` | (none) | No |
| `foundation` | (none) | No |
| `processing` | (none) | No |
| `orchestration` | (none) | No |
| `coordination` | (none) | No |
| `aggregation` | (none) | No |
| `exposer` | (none) | No |
| `test` | (none) | No |
| `service` | (none) | No |
| `layer_contract` | (none) | No |
| `realize` | (none) | No |

No new symbols or operators are introduced. The existing `@` (AT) token is reused for the `@validation` and `@suppress` annotations, which fit the existing Annotation production pattern.

---

## 2. New Productions

### 2.1 Top-level and system-member extensions

The `TopLevelDecl` production in the base grammar (sec. 3.1) is extended with one new alternative:

```ebnf
TopLevelDecl    = EntityDecl
                | EnumDecl
                | ContractDecl
                | RefinementDecl
                | SystemDecl
                | TopologyDecl
                | PhaseDecl
                | TraceDecl
                | ConstraintDecl
                | PackagePolicyDecl
                | DotNetSolutionDecl
                | PageDecl
                | VisualizationDecl
                | ArchitectureDecl            (* NEW *)
                | LayerContractDecl ;         (* NEW *)
```

The `SystemMember` production in the base grammar (sec. 3.3) is extended with layer-prefixed declaration forms:

```ebnf
SystemMember    = SystemProperty
                | AuthoredComponentDecl
                | ConsumedComponentDecl
                | LayerPrefixedDecl ;         (* NEW *)
```

#### 2.1.1 Layer-prefixed declarations

Layer-prefixed declarations are syntactic sugar for `AuthoredComponentDecl` with an implied `layer` property. They use The Standard's vocabulary: `broker`, `service` (with a layer qualifier), `exposer`, and `test`. The parser desugars each form into an `AuthoredComponentDecl` node with the corresponding `layer` set.

```ebnf
LayerPrefixedDecl
                = BrokerDecl
                | ServiceDecl
                | ExposerDecl
                | TestDecl ;

BrokerDecl      = "broker" DOTTED_IDENT "{"
                    { LayerPrefixedProperty }
                  "}" ;

ServiceDecl     = ServiceLayerKeyword "service" DOTTED_IDENT "{"
                    { LayerPrefixedProperty }
                  "}" ;

ServiceLayerKeyword
                = "foundation"
                | "processing"
                | "orchestration"
                | "coordination"
                | "aggregation" ;

ExposerDecl     = "exposer" DOTTED_IDENT "{"
                    { LayerPrefixedProperty }
                  "}" ;

TestDecl        = "test" DOTTED_IDENT "{"
                    { LayerPrefixedProperty }
                  "}" ;

LayerPrefixedProperty
                = "kind" ":" KindValue ";"
                | "target" ":" TargetValue ";"
                | "responsibility" ":" STRING ";"
                | "path" ":" STRING ";"
                | "status" ":" StatusValue ";"
                | "owns" ":" DOTTED_IDENT ";"
                | SuppressAnnotation
                | InlineContractDecl
                | RationaleDecl ;
```

**Notes:**

- `LayerPrefixedProperty` is identical to `AuthoredProperty` minus the `layer` property (which is implied by the declaration keyword). The `layer` property is prohibited inside a layer-prefixed body; specifying it is a parse error because it is redundant.
- `ServiceDecl` is a two-keyword form: the layer qualifier (`foundation`, `processing`, `orchestration`, `coordination`, `aggregation`) followed by `"service"`. This mirrors The Standard's vocabulary where all non-broker, non-exposer components are called "services" with a layer qualifier.
- `BrokerDecl`, `ExposerDecl`, and `TestDecl` are single-keyword forms because The Standard uses standalone nouns for these roles.
- All four forms desugar to the same AST node (`AuthoredComponentDeclNode`) with the `layer` property set to the corresponding value. Downstream semantic analysis cannot distinguish between a layer-prefixed declaration and a generic `authored component` with an explicit `layer` property.
- When no `architecture` declaration is active, layer-prefixed declarations are still parseable but semantic analysis emits a warning: "layer-prefixed declaration 'broker' used without an architecture declaration."

### 2.2 Architecture declaration

```ebnf
ArchitectureDecl
                = "architecture" IDENT "{"
                    { ArchitectureMember }
                  "}" ;

ArchitectureMember
                = "version" ":" STRING ";"
                | "enforce" ":" "[" IdentList "]" ";"
                | VocabularyDecl
                | RealizeDecl                                  (* NEW *)
                | RationaleDecl ;
```

**Notes:**

- The architecture name is a plain IDENT, not DOTTED_IDENT. Architecture names are expected to be short identifiers like `TheStandard`.
- `version` is optional. If absent, semantic analysis defaults to `"1.0"`.
- `enforce` is optional. If absent, all rule sets are active.
- At most one `VocabularyDecl` may appear. Semantic analysis flags duplicates.
- Multiple `RealizeDecl` blocks may appear, each targeting a different layer.
- `RationaleDecl` reuses the existing two-tier rationale production from the base grammar.

### 2.3 Vocabulary declaration

```ebnf
VocabularyDecl  = "vocabulary" "{"
                    { VocabularyMapping }
                  "}" ;

VocabularyMapping
                = LayerKeywordOrIdent ":" "[" IdentList "]" ";" ;

LayerKeywordOrIdent
                = "broker" | "foundation" | "processing"
                | "orchestration" | "coordination" | "aggregation"
                | "exposer" | "test" | IDENT ;
```

**Notes:**

- The left-hand side of a VocabularyMapping accepts layer-name keywords or a plain IDENT. Since layer names are now keywords, they do not match IDENT; this production lists them explicitly. The IDENT fallback allows future layer names not yet keyworded.
- The `IdentList` on the right contains operation verb names (e.g., `Insert`, `Select`, `Add`, `Retrieve`). These are parsed as IDENT tokens.
- The reuse of `IdentList` from the base grammar (sec. 3.3) is intentional. `IdentList = DOTTED_IDENT { "," DOTTED_IDENT }` accepts single-segment identifiers because a plain IDENT is a valid DOTTED_IDENT.
- `RationaleDecl` does not appear inside VocabularyDecl. Vocabulary mappings are mechanical; rationale for vocabulary choices belongs at the `ArchitectureMember` level (as a sibling to VocabularyDecl inside the architecture block), consistent with how the `ArchitectureDecl` production is defined in sec. 2.2.

### 2.4 Layer contract declaration

`LayerContractDecl` is a top-level declaration that attaches behavioral commitments to an entire layer.

```ebnf
LayerContractDecl
                = "layer_contract" IDENT "{"
                    "layer" ":" LayerKeywordOrIdent ";"
                    { ContractClause }
                  "}" ;
```

**Notes:**

- The `IDENT` after `layer_contract` is the contract name (e.g., `FoundationContract`).
- The `layer` property is required and binds the contract to a recognized layer value.
- The body contains `ContractClause` alternatives: `requires`, `ensures`, and `guarantees`. In practice, layer contracts use `guarantees` clauses almost exclusively because the commitments are prose behavioral obligations, not machine-evaluable expressions. `requires` and `ensures` with `@validation` annotations are also permitted for layer contracts that encode validation ordering.
- `layer_contract` is a single underscore-joined token, following the same lexer pattern as `package_policy` in the base grammar.

### 2.5 Realize declaration

`RealizeDecl` appears inside an `ArchitectureDecl` body as an `ArchitectureMember` alternative. It provides advisory prose directives for code generation at a specific layer.

```ebnf
RealizeDecl     = "realize" LayerKeywordOrIdent "{"
                    { STRING ";" }
                  "}" ;
```

**Notes:**

- The layer target uses `LayerKeywordOrIdent`, the same production used by `VocabularyMapping` and `LayerValue`.
- The body contains one or more prose STRING directives, each terminated by `;`.
- `RealizeDecl` does not allow `RationaleDecl` inside it. The directives themselves serve as the rationale for code organization choices.
- Multiple `RealizeDecl` blocks may target the same layer; their directives accumulate.

### 2.6 Authored component property extensions

The `AuthoredProperty` production in the base grammar (sec. 3.3) is extended with two new alternatives:

```ebnf
AuthoredProperty
                = "kind" ":" KindValue ";"
                | "target" ":" TargetValue ";"
                | "responsibility" ":" STRING ";"
                | "path" ":" STRING ";"
                | "status" ":" StatusValue ";"
                | "layer" ":" LayerValue ";"       (* NEW *)
                | "owns" ":" DOTTED_IDENT ";"      (* NEW *)
                | SuppressAnnotation               (* NEW *)
                | InlineContractDecl
                | RationaleDecl ;

LayerValue      = LayerKeywordOrIdent ;

SuppressAnnotation
                = "@" "suppress" "(" IDENT ")" ";" ;
```

**Notes:**

- `LayerValue` accepts layer-name keywords (`broker`, `foundation`, `processing`, `orchestration`, `coordination`, `aggregation`, `exposer`, `test`) or a plain IDENT for forward compatibility. Writing `layer: "foundation"` (quoted) is a parse error because STRING does not match `LayerKeywordOrIdent`. The `layer` property on `authored component` is only needed when not using a layer-prefixed declaration form; it is redundant and prohibited inside layer-prefixed bodies.
- `owns` takes a DOTTED_IDENT referencing an entity declared elsewhere. Semantic analysis resolves the reference and validates that the entity exists.
- Both `layer` and `owns` are optional in the grammar. When the `layers` or `entity_ownership` enforce rules are active, semantic analysis makes them required for the appropriate component layers.
- `SuppressAnnotation` takes a rule-set identifier (`autonomy`, `flow_forward`, `florance`, `entity_ownership`, `vocabulary`, `layers`) and suppresses diagnostics from that rule set for the enclosing component. Multiple `@suppress` annotations may appear on the same component. The annotation is terminated by `;` (unlike `ValidationAnnotation`, which is inline within a `requires` clause).

### 2.7 Validation annotation on contract clauses

The `ContractClause` production in the base grammar (sec. 3.2) is extended to allow an optional annotation after `requires` clauses:

```ebnf
ContractClause  = "requires" Expr [ ValidationAnnotation ] ";"
                | "ensures" Expr ";"
                | "guarantees" STRING ";" ;

ValidationAnnotation
                = "@" "validation" "(" IDENT ")" ;
```

**Notes:**

- `ValidationAnnotation` follows the same pattern as the existing `Annotation` production (`@` + name + `(` + value + `)`) but is restricted to the `"validation"` annotation name and an IDENT value.
- Recognized IDENT values: `structural`, `logical`, `external`. Semantic analysis validates the value.
- The annotation is optional. `requires` clauses without it are parsed normally and produce no ordering diagnostics.
- The annotation appears AFTER the expression and BEFORE the semicolon. This placement is unambiguous because expressions cannot end with `@`.

**Disambiguation:**

The `@` token after an expression in a `requires` clause could theoretically begin a new statement if the next token happened to be a valid start-of-declaration keyword. However, since `@` is not a valid expression operator and the parser is already inside a contract body (delimited by `{` `}`), the `@` is unambiguously the start of a `ValidationAnnotation`. The parser consumes it greedily.

---

## 3. Integration with Existing Grammar

### 3.1 No changes to expression grammar

The expression language (base grammar sec. 2) is unchanged. No new operators, quantifiers, or primary expressions are introduced.

### 3.2 No changes to lexer resolution rules

The base grammar's lexer resolution rules (sec. 1.4) are unchanged. The new keywords follow the existing keyword-vs-identifier classification: single-segment tokens are checked against the keyword table first. Multi-segment tokens (DOTTED_IDENT) bypass keyword checking, so `layer.name` lexes as DOTTED_IDENT, not KW_LAYER DOT IDENT. This is the correct behavior.

### 3.3 Two-word keyword handling

The base grammar handles two-word keywords (`authored component`, `consumed component`, `dotnet solution`, `package_policy`) at the parser level, not the lexer level. No new two-word keywords are introduced in this extension. `architecture` is a single keyword.

### 3.4 IdentList reuse

The `IdentList` production (`DOTTED_IDENT { "," DOTTED_IDENT }`) from the base grammar (sec. 3.3) is reused in:

- `enforce: [ IdentList ];` within `ArchitectureDecl`
- The right-hand side of `VocabularyMapping`

Since rule-set identifiers and verb names are plain IDENTs (no dots), and a plain IDENT is a valid DOTTED_IDENT, this reuse is correct without modification.

---

## 4. Grammar-to-AST Mapping

The following AST node types correspond to the new productions:

| Production | AST Node |
|---|---|
| `ArchitectureDecl` | `ArchitectureDeclNode` |
| `ArchitectureMember` (version) | `VersionPropertyNode` (reused from base) |
| `ArchitectureMember` (enforce) | `EnforcePropertyNode` |
| `VocabularyDecl` | `VocabularyDeclNode` |
| `VocabularyMapping` | `VocabularyMappingNode` |
| `AuthoredProperty` (layer) | `LayerPropertyNode` |
| `AuthoredProperty` (owns) | `OwnsPropertyNode` |
| `ValidationAnnotation` | `ValidationAnnotationNode` |
| `SuppressAnnotation` | `SuppressAnnotationNode` |
| `LayerPrefixedDecl` | `AuthoredComponentDeclNode` (desugared; layer property set from keyword) |
| `BrokerDecl` | `AuthoredComponentDeclNode` (layer = broker) |
| `ServiceDecl` | `AuthoredComponentDeclNode` (layer = foundation/processing/orchestration/coordination/aggregation) |
| `ExposerDecl` | `AuthoredComponentDeclNode` (layer = exposer) |
| `TestDecl` | `AuthoredComponentDeclNode` (layer = test) |
| `LayerKeywordOrIdent` | (no separate node; value stored as string in parent) |
| `LayerContractDecl` | `LayerContractDeclNode` |
| `RealizeDecl` | `RealizeDeclNode` |

The `ArchitectureDeclNode` is a child of the `SpecDocumentNode` root, at the same level as `SystemDeclNode`, `TopologyDeclNode`, and other top-level declarations. `LayerContractDeclNode` is also a top-level child. `RealizeDeclNode` is a child of `ArchitectureDeclNode`. Layer-prefixed declarations desugar into the same `AuthoredComponentDeclNode` used by `authored component`, making them indistinguishable in the AST.

---

## 5. Ambiguity Resolution

### 5.1 `architecture` as keyword vs. field name

The keyword `architecture` is added to the keyword table. In declaration context (after a `{` inside an entity body), it would be recognized as `KW_ARCHITECTURE` rather than `IDENT`. To allow `architecture` as a field name in entity bodies, it is added to the `ContextualKeyword` production. The `FieldName` production already accepts `ContextualKeyword`, so `entity Building { architecture: string; }` parses correctly: inside the entity body, `architecture` matches `ContextualKeyword` in the `FieldName` position.

At top level, `architecture` starts an `ArchitectureDecl`. There is no ambiguity because no other top-level production begins with the keyword `architecture`.

### 5.2 `layer` as keyword vs. field name

Same resolution as 5.1. `layer` is a keyword at the lexer level, added to `ContextualKeyword` for field-name positions. Inside an `authored component` body, `layer` starts the `LayerPropertyNode` production. Inside an entity body, `layer` is a valid field name.

Disambiguation: the parser knows whether it is inside an `AuthoredComponentDecl` body or an `EntityDecl` body based on the enclosing production. In `AuthoredComponentDecl`, `layer` followed by `:` matches the `AuthoredProperty` alternative. In `EntityDecl`, `layer` followed by `:` matches the `FieldDecl` alternative via `FieldName -> ContextualKeyword`.

### 5.3 `owns` as keyword vs. field name

Same pattern as 5.1 and 5.2. `owns` is added to `ContextualKeyword`.

### 5.4 `vocabulary` as keyword vs. field name

Same pattern. Inside an `ArchitectureDecl` body, `vocabulary` starts a `VocabularyDecl`. In entity bodies, it is a valid field name via `ContextualKeyword`.

### 5.5 `@validation` on requires clauses

The existing `Annotation` production (`@` + `AnnotationName` + `(` + `AnnotationValue` + `)`) appears on field declarations. The new `ValidationAnnotation` appears on `requires` clauses. There is no ambiguity because:

1. `Annotation` is only parsed after a `TypeExpr` in a `FieldDecl`.
2. `ValidationAnnotation` is only parsed after an `Expr` in a `ContractClause` starting with `requires`.
3. The two contexts are structurally disjoint (entity body vs. contract body).

The parser does not need lookahead to distinguish them; the enclosing production determines which annotation form to expect.

**Important implementation note:** The disjointness between `@validation` and the base `Annotation` relies on `validation` being a keyword (`KW_VALIDATION`). In the base `Annotation` production, `AnnotationName` accepts `IDENT | "default" | "source" | "version" | "target"`. Since `KW_VALIDATION` is not `IDENT` and is not in the base `AnnotationName` alternatives, `@validation` on a field declaration would fail to parse as a base `Annotation`. If `KW_VALIDATION` were ever removed from the keyword table, `validation` would revert to `IDENT` and could accidentally match `AnnotationName`. The keyword status of `validation` is load-bearing for disambiguation.

### 5.6 `@suppress` on authored components

The `SuppressAnnotation` production appears only inside `AuthoredComponentDecl` bodies (via the `AuthoredProperty` alternative). The `@` token in this context is unambiguous: `AuthoredProperty` explicitly lists `SuppressAnnotation` as an alternative. The base `Annotation` production does not appear in `AuthoredProperty`; it appears only in `FieldDecl` (entity bodies). Same keyword-dependency note applies: `suppress` being `KW_SUPPRESS` prevents it from matching `AnnotationName` in the base `Annotation` production.

### 5.7 `enforce` keyword vs. other uses

`enforce` is not a property keyword in any existing production. It only appears as a member of `ArchitectureDecl`. Inside an entity body, it would be parsed as a field name via `ContextualKeyword`. No ambiguity.

### 5.8 Layer-prefixed declarations vs. other SystemMember alternatives

Inside a `system` body, the parser encounters tokens that could start multiple alternatives. The disambiguation is:

- `"authored"` followed by `"component"`: existing `AuthoredComponentDecl`.
- `"consumed"` followed by `"component"`: existing `ConsumedComponentDecl`.
- `"broker"` followed by `DOTTED_IDENT`: `BrokerDecl`. No other SystemMember starts with `"broker"`.
- `"foundation"` followed by `"service"`: `ServiceDecl`. The two-keyword sequence is unambiguous.
- `"processing"` followed by `"service"`: `ServiceDecl`. Same pattern.
- `"orchestration"` followed by `"service"`: `ServiceDecl`. Same pattern.
- `"coordination"` followed by `"service"`: `ServiceDecl`. Same pattern.
- `"aggregation"` followed by `"service"`: `ServiceDecl`. Same pattern.
- `"exposer"` followed by `DOTTED_IDENT`: `ExposerDecl`.
- `"test"` followed by `DOTTED_IDENT`: `TestDecl`.
- `"target"` or `"responsibility"`: existing `SystemProperty`.

No two alternatives share the same leading token(s). The parser can select the correct alternative with at most one token of lookahead (two for the `ServiceDecl` forms, which require seeing the layer keyword followed by `"service"`).

### 5.9 Layer-name keywords in entity field names

All layer-name keywords (`broker`, `foundation`, `processing`, `orchestration`, `coordination`, `aggregation`, `exposer`, `test`, `service`) are added to `ContextualKeyword`. Inside entity and refinement bodies, they are accepted as field names. For example, `entity Room { service: string; }` parses correctly because `service` matches `ContextualKeyword` in the `FieldName` position.

### 5.10 Layer-name keywords in VocabularyMapping and RealizeDecl

In a `VocabularyMapping`, the left-hand side is `LayerKeywordOrIdent`, which explicitly accepts layer-name keywords. Without this, `broker: [Insert, Select]` inside a vocabulary block would fail because `broker` is `KW_BROKER`, not `IDENT`. The same `LayerKeywordOrIdent` production is used by `RealizeDecl` to identify the target layer.

### 5.11 `layer_contract` at top level

`layer_contract` is a single underscore-joined keyword (same lexer pattern as `package_policy`). At the top level, it unambiguously starts a `LayerContractDecl`. No other top-level production begins with `layer_contract`. Inside entity bodies, `layer_contract` is a valid field name via `ContextualKeyword`.

### 5.12 `realize` inside architecture body

`realize` starts a `RealizeDecl` inside an `ArchitectureDecl` body. No other `ArchitectureMember` alternative begins with `realize`. Inside entity bodies, `realize` is a valid field name via `ContextualKeyword`.

---

## 6. Complete Example Parse

The following spec fragment is parsed by the extended grammar. Token classifications and production matches are shown for the architecture declaration:

```spec
architecture TheStandard {
    version: "1.0";
    enforce: [layers, flow_forward, florance];
    vocabulary {
        broker: [Insert, Select, Update, Delete];
        foundation: [Add, Retrieve, Modify, Remove];
    }
}
```

**Token stream:**

```
KW_ARCHITECTURE  IDENT("TheStandard")  LBRACE
  KW_VERSION  COLON  STRING("1.0")  SEMICOLON
  KW_ENFORCE  COLON  LBRACKET
    IDENT("layers")  COMMA  IDENT("flow_forward")  COMMA  IDENT("florance")
  RBRACKET  SEMICOLON
  KW_VOCABULARY  LBRACE
    KW_BROKER  COLON  LBRACKET
      IDENT("Insert")  COMMA  IDENT("Select")  COMMA
      IDENT("Update")  COMMA  IDENT("Delete")
    RBRACKET  SEMICOLON
    KW_FOUNDATION  COLON  LBRACKET
      IDENT("Add")  COMMA  IDENT("Retrieve")  COMMA
      IDENT("Modify")  COMMA  IDENT("Remove")
    RBRACKET  SEMICOLON
  RBRACE
RBRACE
```

**Production trace:**

```
TopLevelDecl
  -> ArchitectureDecl
       "architecture" IDENT("TheStandard") "{"
         ArchitectureMember -> "version" ":" STRING ";"
         ArchitectureMember -> "enforce" ":" "[" IdentList "]" ";"
         ArchitectureMember -> VocabularyDecl
           "vocabulary" "{"
             VocabularyMapping -> KW_BROKER ":" "[" IdentList "]" ";"
             VocabularyMapping -> KW_FOUNDATION ":" "[" IdentList "]" ";"
           "}"
       "}"
```

For a layer-prefixed declaration (preferred form when architecture is active):

```spec
foundation service PizzApp.Services.Foundations.Orders {
    kind: library;
    path: "src/PizzApp.Services/Foundations/Orders";
    owns: Order;
    responsibility: "Validation and primitive CRUD for Order entities.";
}
```

**Token stream:**

```
KW_FOUNDATION  KW_SERVICE  DOTTED_IDENT("PizzApp.Services.Foundations.Orders")  LBRACE
  KW_KIND  COLON  IDENT("library")  SEMICOLON
  KW_PATH  COLON  STRING("src/PizzApp.Services/Foundations/Orders")  SEMICOLON
  KW_OWNS  COLON  IDENT("Order")  SEMICOLON
  KW_RESPONSIBILITY  COLON  STRING("...")  SEMICOLON
RBRACE
```

**Production trace (within SystemDecl body):**

```
SystemMember
  -> LayerPrefixedDecl
       -> ServiceDecl
            ServiceLayerKeyword("foundation") "service"
            DOTTED_IDENT("PizzApp.Services.Foundations.Orders") "{"
              LayerPrefixedProperty -> "kind" ":" KindValue(IDENT("library")) ";"
              LayerPrefixedProperty -> "path" ":" STRING ";"
              LayerPrefixedProperty -> "owns" ":" DOTTED_IDENT("Order") ";"
              LayerPrefixedProperty -> "responsibility" ":" STRING ";"
            "}"
(* Desugars to AuthoredComponentDeclNode with layer = foundation *)
```

For the equivalent generic form (both produce identical AST):

```spec
authored component PizzApp.Services.Foundations.Orders {
    kind: library;
    path: "src/PizzApp.Services/Foundations/Orders";
    layer: foundation;
    owns: Order;
    responsibility: "Validation and primitive CRUD for Order entities.";
}
```

**Production trace (within SystemDecl body):**

```
SystemMember
  -> AuthoredComponentDecl
       "authored" "component" DOTTED_IDENT("PizzApp.Services.Foundations.Orders") "{"
         AuthoredProperty -> "kind" ":" KindValue(IDENT("library")) ";"
         AuthoredProperty -> "path" ":" STRING ";"
         AuthoredProperty -> "layer" ":" LayerValue(KW_FOUNDATION) ";"
         AuthoredProperty -> "owns" ":" DOTTED_IDENT("Order") ";"
         AuthoredProperty -> "responsibility" ":" STRING ";"
       "}"
```

For a broker declaration:

```spec
broker PizzApp.Brokers.Storage {
    kind: library;
    path: "src/PizzApp.Brokers.Storage";
    responsibility: "Storage broker. Wraps Entity Framework.";
}
```

**Production trace:**

```
SystemMember
  -> LayerPrefixedDecl
       -> BrokerDecl
            "broker" DOTTED_IDENT("PizzApp.Brokers.Storage") "{"
              LayerPrefixedProperty -> "kind" ":" KindValue(IDENT("library")) ";"
              LayerPrefixedProperty -> "path" ":" STRING ";"
              LayerPrefixedProperty -> "responsibility" ":" STRING ";"
            "}"
(* Desugars to AuthoredComponentDeclNode with layer = broker *)
```

For a contract clause with validation annotation:

```spec
contract OrderSubmission {
    requires count(cart.items) > 0 @validation(structural);
    requires order.total >= store.minimumDeliveryAmount
        @validation(external);
    ensures order.status == OrderStatus.received;
}
```

**Production trace (within ContractDecl body):**

```
ContractClause -> "requires" Expr ValidationAnnotation ";"
  Expr: count(cart.items) > 0
  ValidationAnnotation: "@" "validation" "(" IDENT("structural") ")"

ContractClause -> "requires" Expr ValidationAnnotation ";"
  Expr: order.total >= store.minimumDeliveryAmount
  ValidationAnnotation: "@" "validation" "(" IDENT("external") ")"

ContractClause -> "ensures" Expr ";"
  Expr: order.status == OrderStatus.received
```

**Token stream for the contract example:**

```
KW_CONTRACT  IDENT("OrderSubmission")  LBRACE
  KW_REQUIRES  KW_COUNT  LPAREN  DOTTED_IDENT("cart.items")  RPAREN
    GT  INTEGER(0)  AT  KW_VALIDATION  LPAREN  IDENT("structural")  RPAREN  SEMICOLON
  KW_REQUIRES  DOTTED_IDENT("order.total")  GTE  DOTTED_IDENT("store.minimumDeliveryAmount")
    AT  KW_VALIDATION  LPAREN  IDENT("external")  RPAREN  SEMICOLON
  KW_ENSURES  DOTTED_IDENT("order.status")  EQ  DOTTED_IDENT("OrderStatus.received")  SEMICOLON
RBRACE
```

Note: `AT` (`@`) is not an expression operator, so the lexer emitting `AT` after the expression's last token is unambiguous. The parser recognizes it as the start of a `ValidationAnnotation`, not a continuation of the expression.
