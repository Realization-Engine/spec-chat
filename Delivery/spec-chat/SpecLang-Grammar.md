# SpecChat Grammar Specification

**Version:** 0.1
**Date:** 2026-04-02
**Companion to:** SpecLang-Specification.md

---

## Purpose

This document defines the formal grammar of the SpecChat specification language. It provides the lexer token definitions, the expression language grammar, and the full DSL grammar in EBNF notation, precise enough for a recursive descent parser implementation in C#.

The grammar covers five layers of specification:
- **Data specification:** entities, enums, contracts, refinements, invariants, rationale, confidence signals
- **Context specification:** persons, external systems, relationships, tags
- **Systems specification:** system trees, authored/consumed components, topology, phases, traces, constraints, package policies, platform realization
- **Deployment specification:** deployment environments, infrastructure nodes, component instances
- **View and dynamic specification:** architectural views with include/exclude filters, behavioral interaction sequences

---

## 1. Lexer Token Definitions

The lexer produces a stream of typed tokens from raw text inside ` ```spec ` fenced code blocks within markdown. Whitespace (spaces, tabs, newlines) is skipped between tokens except inside string literals. Comments begin with `//` and extend to end of line. Comment detection is disabled inside string literals: `"//"` within a quoted string is part of the string content, not a comment start.

### 1.1 Token categories

```
(* Literals *)
INTEGER         = digit { digit }
FLOAT           = digit { digit } "." digit { digit }
STRING          = '"' { any-char-except-unescaped-quote
                      | '\\"' | '\\\\' | '\\n' | '\\t'
                      | newline } '"'
                  (* Strings may span multiple lines. The lexer
                     preserves internal newlines. Leading whitespace
                     on continuation lines is preserved as-is;
                     the consumer may trim it. *)
BOOLEAN         = "true" | "false"

(* Identifiers *)
IDENT           = letter { letter | digit | "_" }
DOTTED_IDENT    = IDENT { "." IDENT }
                  (* The lexer emits DOTTED_IDENT as a single token.
                     The parser splits on "." when member access is
                     needed. See Ambiguity Resolution §5.1. *)
```

### 1.2 Keywords

#### Data specification keywords

```
KW_ENTITY       = "entity"
KW_ENUM         = "enum"
KW_INVARIANT    = "invariant"
KW_CONTRACT     = "contract"
KW_REQUIRES     = "requires"
KW_ENSURES      = "ensures"
KW_GUARANTEES   = "guarantees"
KW_REFINES      = "refines"
KW_AS           = "as"
KW_RATIONALE    = "rationale"
KW_CONTEXT      = "context"
KW_DECISION     = "decision"
KW_CONSEQUENCE  = "consequence"
KW_SUPERSEDES   = "supersedes"
```

#### Expression keywords

```
KW_IMPLIES      = "implies"
KW_AND          = "and"
KW_OR           = "or"
KW_NOT          = "not"
KW_CONTAINS     = "contains"
KW_EXCLUDES     = "excludes"
KW_IN           = "in"
KW_ALL          = "all"
KW_EXISTS       = "exists"
KW_HAVE         = "have"
KW_SATISFY      = "satisfy"
KW_COUNT        = "count"
```

#### Systems specification keywords

```
KW_SYSTEM       = "system"
KW_TARGET       = "target"
KW_RESPONSIBILITY = "responsibility"
KW_AUTHORED     = "authored"
KW_CONSUMED     = "consumed"
KW_COMPONENT    = "component"
KW_KIND         = "kind"
KW_SOURCE       = "source"
KW_VERSION      = "version"
KW_USED_BY      = "used_by"
KW_TOPOLOGY     = "topology"
KW_ALLOW        = "allow"
KW_DENY         = "deny"
KW_PHASE        = "phase"
KW_PRODUCES     = "produces"
KW_GATE         = "gate"
KW_COMMAND      = "command"
KW_EXPECTS      = "expects"
KW_TRACE        = "trace"
KW_CONSTRAINT   = "constraint"
KW_SCOPE        = "scope"
KW_RULE         = "rule"
KW_PACKAGE_POLICY = "package_policy"
KW_CATEGORY     = "category"
KW_INCLUDES     = "includes"
KW_DEFAULT      = "default"
KW_DOTNET       = "dotnet"
KW_SOLUTION     = "solution"
KW_FORMAT       = "format"
KW_STARTUP      = "startup"
KW_FOLDER       = "folder"
KW_PROJECTS     = "projects"
KW_PATH         = "path"
KW_STATUS       = "status"
KW_EXISTING     = "existing"
KW_NEW          = "new"
KW_PERSON       = "person"
KW_EXTERNAL     = "external"
KW_DESCRIPTION  = "description"
KW_TECHNOLOGY   = "technology"
KW_DEPLOYMENT   = "deployment"
KW_NODE         = "node"
KW_INSTANCE     = "instance"
KW_VIEW         = "view"
KW_INCLUDE      = "include"
KW_EXCLUDE      = "exclude"
KW_AUTOLAYOUT   = "autoLayout"
KW_DYNAMIC      = "dynamic"
KW_TAG          = "tag"
```

#### Design specification keywords

```
KW_PAGE         = "page"
KW_HOST         = "host"
KW_ROUTE        = "route"
KW_CONCEPTS     = "concepts"
KW_ROLE         = "role"
KW_CROSS_LINKS  = "cross_links"
KW_VISUALIZATION = "visualization"
KW_PARAMETERS   = "parameters"
KW_SLIDERS      = "sliders"
```

#### Primitive type keywords

```
KW_STRING       = "string"
KW_INT          = "int"
KW_DOUBLE       = "double"
KW_BOOL         = "bool"
KW_UNKNOWN      = "unknown"
```

### 1.3 Symbols

```
LBRACE          = "{"
RBRACE          = "}"
LBRACKET        = "["
RBRACKET        = "]"
LPAREN          = "("
RPAREN          = ")"
COLON           = ":"
SEMICOLON       = ";"
COMMA           = ","
DOT             = "."
QUESTION        = "?"
AT              = "@"
ARROW           = "->"
DOTDOT          = ".."
EQ              = "=="
NEQ             = "!="
LT              = "<"
GT              = ">"
LTE             = "<="
GTE             = ">="
EOF             = (* end of token stream *)
```

### 1.4 Lexer resolution rules

1. **Keyword vs. identifier vs. boolean.** After the lexer consumes a single-segment letter-based token, it classifies it in this order: (a) check the keyword table; if matched, emit the keyword token; (b) if the text is `true` or `false`, emit BOOLEAN; (c) otherwise emit IDENT. `true` and `false` are not in the keyword table; they are literal values handled at classification step (b).

2. **DOTTED_IDENT vs. keyword vs. DOT.** The lexer greedily consumes `letter { letter | digit | "_" } { "." letter { letter | digit | "_" } }` as a single candidate token. Then:
   - If the candidate has **no dots** (single segment), check the keyword table. If it matches a keyword, emit the keyword token. Otherwise emit IDENT.
   - If the candidate has **one or more dots** (multiple segments), emit DOTTED_IDENT. Keyword checking does NOT apply to multi-segment tokens. This means `source.Config` lexes as DOTTED_IDENT (not KW_SOURCE DOT IDENT), which is the correct behavior for declaration names.
   - A lone `.` not preceded/followed by an identifier character is emitted as DOT.

3. **`->` vs. `-` and `>`.** The lexer matches `->` as ARROW before attempting individual characters. Note: `-` alone is not a valid token in SpecChat (no arithmetic negation).

4. **`..` vs. `.`.** The lexer matches `..` as DOTDOT before attempting a single DOT.

4a. **FLOAT vs. DOTDOT.** When the lexer has consumed an INTEGER and sees `.`, it must look ahead one more character. If the next character is also `.` (forming `..`), the first `.` is NOT the start of a FLOAT fractional part; the INTEGER is complete and `..` is emitted as DOTDOT. If the next character is a digit, the lexer continues consuming a FLOAT. If the next character is a letter (start of an IDENT segment), the lexer emits the INTEGER and then starts a new DOTTED_IDENT or DOT token. Example: `1..10` lexes as INTEGER(1) DOTDOT INTEGER(10), not as a malformed FLOAT.

5. **`==` vs. `=`.** The lexer matches `==` as EQ. Single `=` is not a valid token. Assignment does not exist in SpecChat.

6. **Two-word keywords.** `authored component`, `consumed component`, `dotnet solution`, and `package_policy` are handled at the parser level, not the lexer level. The lexer emits them as separate tokens (KW_AUTHORED + KW_COMPONENT, KW_DOTNET + KW_SOLUTION). `package_policy` is a single token because the underscore makes it one IDENT that matches the keyword.

---

## 2. Expression Language Grammar

The expression language is used inside invariant conditions, contract requires/ensures clauses, gate expects clauses, topology invariants, and trace invariants. It is not Turing-complete: no loops, no variable declarations, no function definitions.

### 2.1 Operator precedence (lowest to highest)

| Precedence | Operators | Associativity | Description |
|------------|-----------|---------------|-------------|
| 1 (lowest) | `implies` | right | Logical implication |
| 2 | `or` | left | Logical disjunction |
| 3 | `and` | left | Logical conjunction |
| 4 | `not` | prefix (unary) | Logical negation |
| 5 | `==` `!=` `<` `>` `<=` `>=` `contains` `excludes` `in` | none (non-associative) | Comparison and membership |
| 6 (highest) | `.` (member access), `()` (function call) | left | Access and invocation |

### 2.2 Expression grammar (EBNF)

```ebnf
(* ===== Entry point ===== *)

Expr            = ImpliesExpr ;

(* ===== Precedence 1: implication (right-associative) ===== *)

ImpliesExpr     = OrExpr [ "implies" ImpliesExpr ] ;

(* ===== Precedence 2: disjunction ===== *)

OrExpr          = AndExpr { "or" AndExpr } ;

(* ===== Precedence 3: conjunction ===== *)

AndExpr         = NotExpr { "and" NotExpr } ;

(* ===== Precedence 4: negation ===== *)

NotExpr         = "not" NotExpr
                | CompareExpr ;

(* ===== Precedence 5: comparison and membership ===== *)

CompareExpr     = AccessExpr [ CompareOp AccessExpr ] ;

CompareOp       = "==" | "!=" | "<" | ">" | "<=" | ">="
                | "contains" | "excludes" | "in" ;

(* ===== Precedence 6: member access and function calls ===== *)

AccessExpr      = PrimaryExpr { "." IDENT [ "(" [ ArgList ] ")" ] } ;

(* Note on DOTTED_IDENT in expression context:
   The lexer emits "CoffeeDrink.espresso" as a single
   DOTTED_IDENT token. In expression context, the parser
   must split this into an AccessExpr chain. Two valid
   implementation strategies:

   Strategy A (re-tokenize): When the parser is in expression
   context and encounters a DOTTED_IDENT token, it splits it
   on "." boundaries and processes the segments as if they
   were separate IDENT DOT IDENT tokens.

   Strategy B (context-sensitive lexer): The lexer tracks
   whether it is inside an expression context (after ":" in
   invariant, after "requires"/"ensures" in contract, after
   "expects:" in gate) and emits separate IDENT and DOT
   tokens instead of DOTTED_IDENT.

   Either strategy produces the same AST. Strategy A is
   simpler to implement. The grammar below is written as
   if separate tokens are available (IDENT and DOT). *)

(* ===== Primary expressions ===== *)

PrimaryExpr     = IDENT [ "(" [ ArgList ] ")" ]
                | DOTTED_IDENT                           (* re-tokenized into AccessExpr chain *)
                | INTEGER
                | FLOAT
                | STRING
                | BOOLEAN
                | "[" [ ExprList ] "]"
                | "(" Expr ")"
                | QuantifierExpr ;

(* When PrimaryExpr matches DOTTED_IDENT, the parser
   splits it and constructs the equivalent AccessExpr:
   DOTTED_IDENT "A.B.C" becomes
   AccessExpr(AccessExpr(PrimaryExpr("A"), "B"), "C").

   Important: the individual segments are treated as plain
   names, NOT checked against the keyword table. This means
   "request.source" produces AccessExpr with member name
   "source", even though "source" is a keyword (KW_SOURCE)
   in declaration context. This is correct because member
   access names are semantic, not syntactic. *)

ArgList         = Expr { "," Expr } ;
ExprList        = Expr { "," Expr } ;

(* ===== Quantifier expressions ===== *)

QuantifierExpr  = "all" ScopeRef "have" Expr
                | "all" ScopeRef "satisfy" Expr
                | "exists" ScopeRef "have" Expr
                | "count" "(" Expr ")" ;

(* Scope reference: one or two words naming a collection.
   Two-word forms like "authored components" or "test methods"
   are parsed as qualifier + noun. Single-word forms like
   "sources" or "components" are parsed as noun only.

   The qualifier may be a keyword token (KW_AUTHORED,
   KW_CONSUMED) because these words serve double duty:
   they are keywords in declaration context and qualifiers
   in scope context.

   Disambiguation: the parser peeks ahead after consuming
   the first word. If the next token is "have" or "satisfy",
   the scope is single-word. Otherwise, consume a second
   word, then expect "have" or "satisfy". *)

ScopeWord       = IDENT | "authored" | "consumed" ;
ScopeRef        = ScopeWord [ ScopeWord ] ;
```

### 2.3 Expression language notes

1. **Quantifiers are syntactic forms, not higher-order functions.** `all sources have count(targets) >= 1` parses as a QuantifierExpr where `sources` is the collection and `count(targets) >= 1` is the predicate. No lambda syntax is needed.

2. **Member access vs. dotted identifier.** In expression context, `CoffeeDrink.espresso` is parsed as AccessExpr: PrimaryExpr(`CoffeeDrink`) DOT IDENT(`espresso`). In declaration name context (after `entity`, `component`, etc.), `Dashboard.UI` is parsed as a single DOTTED_IDENT. The parser distinguishes by production rule. See Ambiguity Resolution §5.1.

3. **`contains` and `excludes` are binary comparison operators.** `response.headers contains "X-Request-Id"` parses as CompareExpr with operator `contains`. `list excludes item` is the negation.

4. **List literals** use brackets: `[Revenue, Retention]`. Elements are expressions (usually identifiers).

5. **No assignment.** `==` is comparison only. There is no `=` operator.

6. **Range literals** (`1..10`) are NOT part of the expression grammar. They appear only in `@range` annotations and are parsed by the annotation production rule.

7. **No arithmetic.** The expression language does not support `+`, `-`, `*`, `/`. It is a constraint language, not a computation language. If arithmetic is needed in the future, it would be added at precedence levels between comparison and member access.

8. **`count` is a keyword, not a function.** `count(items)` is parsed as a QuantifierExpr, not as a function call. Since `count` is reserved as KW_COUNT, it does not match the IDENT alternative in PrimaryExpr. The method-call form `items.count()` is NOT valid because AccessExpr's dot-chain expects IDENT after the dot, and `count` is a keyword token. The only valid form is `count(expr)`.

9. **Contextual keywords in field-name and member-name positions.** Many keywords (`source`, `version`, `target`, `kind`, `rule`, `format`, `command`, etc.) are ordinary English words that specifiers might use as field names or enum members. The FieldName and EnumMember productions accept these as ContextualKeyword tokens. This prevents parse failures when domain vocabulary overlaps with DSL keywords. See the ContextualKeyword production in §3.2.

---

## 3. DSL Grammar

### 3.1 Top-level structure

```ebnf
(* A .spec.md file contains markdown prose interleaved
   with ```spec fenced code blocks. The markdown extractor
   (SpecChat.Language, pre-parser phase) separates the two
   and recognizes prose contexts:

   SpecFile        = { ProseSection | SpecBlock
                     | PageContext | VisualizationContext } ;
   ProseSection    = (* markdown text outside spec fences,
                       not inside a page or visualization context *) ;
   SpecBlock       = "```spec" TokenStream "```" ;

   PageContext     = HeadingNode SpecBlock(page)
                     { ProseIntent | SpecBlock(visualization)
                       | VisualizationContext } ;
   VisualizationContext = HeadingNode SpecBlock(visualization)
                          { ProseIntent } ;
   ProseIntent     = (* markdown text within a page or
                       visualization context *) ;

   Context boundaries are determined by heading level.
   A heading followed by a spec block whose first keyword
   is "page" opens a PageContext. All content until the
   next heading at the same or higher level belongs to that
   context. Within a PageContext, a heading followed by a
   spec block whose first keyword is "visualization" opens
   a VisualizationContext.

   The extractor peeks at the first keyword of each spec
   block to determine whether it opens a context. See §5.15.

   The parser operates on the concatenated token streams
   of all SpecBlocks. ProseIntent content is preserved
   in the AST as ProseIntent nodes associated with their
   parent PageDecl or VisualizationDecl, for the LLM to
   read alongside formal declarations during realization.

   The grammar below defines the spec-block content: *)

SpecDocument    = { TopLevelDecl } EOF ;

TopLevelDecl    = EntityDecl
                | EnumDecl
                | ContractDecl
                | RefinementDecl
                | PersonDecl
                | ExternalSystemDecl
                | RelationshipDecl
                | SystemDecl
                | TopologyDecl
                | PhaseDecl
                | TraceDecl
                | ConstraintDecl
                | PackagePolicyDecl
                | DotNetSolutionDecl
                | DeploymentDecl
                | ViewDecl
                | DynamicDecl
                | PageDecl
                | VisualizationDecl ;
```

### 3.2 Data specification productions

```ebnf
(* ===== Entity ===== *)

EntityDecl      = "entity" DOTTED_IDENT "{" { EntityMember } "}" ;

EntityMember    = FieldDecl
                | InvariantDecl
                | RationaleDecl ;

FieldDecl       = FieldName ":" TypeExpr { Annotation } ";" ;

FieldName       = IDENT | ContextualKeyword ;

(* Field names may collide with keywords that are only
   meaningful in specific declaration contexts. Words like
   "source", "version", "target", "kind", "rule", "format",
   "command", "scope", "context", "decision" are common
   domain terms. The parser accepts them as field names
   because the FieldDecl production is only reached inside
   an entity or refinement body, where these keywords have
   no structural meaning. *)

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
                  | "person" | "external" | "description"
                  | "technology" | "deployment" | "node"
                  | "instance" | "view" | "include" | "exclude"
                  | "autoLayout" | "dynamic" | "tag" ;

(* ===== Type expressions ===== *)

TypeExpr        = BaseType [ "?" ] [ "[" "]" ] ;

BaseType        = PrimitiveType
                | DOTTED_IDENT ;

PrimitiveType   = "string" | "int" | "double" | "bool" | "unknown" ;

(* Optional marker "?" and array marker "[" "]" may combine:
   string?     -- optional string
   LineItem[]  -- array of LineItem
   string?[]   -- array of optional strings (rare but valid)
   The parser consumes "?" then "[" "]" in that order if present.
   Note: "[" and "]" are separate tokens (LBRACKET, RBRACKET).
   The parser expects them adjacent with no content between. *)

(* ===== Annotations ===== *)

Annotation      = "@" AnnotationName "(" AnnotationValue ")" ;

AnnotationName  = IDENT | "default" | "source" | "version" | "target" ;
                  (* Annotation names may collide with keywords.
                     The parser accepts any of these tokens after "@".
                     The most common case is @default(...), where
                     "default" is reserved as KW_DEFAULT but must be
                     accepted as an annotation name. *)

AnnotationValue = IDENT
                | INTEGER
                | FLOAT
                | STRING
                | BOOLEAN
                | RangeValue ;

RangeValue      = INTEGER ".." INTEGER ;

(* Known annotation names and their expected value types:
   @confidence(high | medium | low)   -- IDENT
   @default(value)                    -- IDENT, INTEGER, or FLOAT
   @range(min..max)                   -- RangeValue
   @pattern("regex")                  -- STRING
   @reason("text")                    -- STRING

   The parser accepts any @AnnotationName(...) form. Semantic
   analysis validates that the annotation name is known and
   the value type is correct. *)

(* ===== Enum ===== *)

EnumDecl        = "enum" DOTTED_IDENT "{"
                    EnumMember { "," EnumMember } [ "," ]
                  "}" ;

EnumMember      = ( IDENT | ContextualKeyword ) ":" STRING ;

(* ===== Contract ===== *)

ContractDecl    = "contract" DOTTED_IDENT "{"
                    { ContractClause }
                  "}" ;

ContractClause  = "requires" Expr ";"
                | "ensures" Expr ";"
                | "guarantees" STRING ";" ;

(* ===== Refinement ===== *)

RefinementDecl  = "refines" DOTTED_IDENT "as" DOTTED_IDENT "{"
                    { EntityMember }
                  "}" ;

(* ===== Invariant ===== *)

InvariantDecl   = "invariant" STRING ":" Expr ";" ;

(* The STRING is the invariant's human-readable description.
   The Expr is the machine-evaluable condition.
   Example: invariant "espresso is always small":
              drink == CoffeeDrink.espresso implies
              size == CoffeeSize.small; *)

(* ===== Rationale (two-tier) ===== *)

RationaleDecl   = "rationale" STRING ";"
                | "rationale" [ STRING ] "{"
                    "context" STRING ";"
                    "decision" STRING ";"
                    "consequence" STRING ";"
                    [ "supersedes" STRING ";" ]
                  "}" ;

(* Tier 1 (simple): rationale "The primary item being ordered.";
   Tier 2 (structured, unnamed): rationale { context "..."; ... }
   Tier 2 (structured, named target): rationale "fieldName" { ... }

   Disambiguation: see §5.5. *)
```

### 3.3 Context specification productions

```ebnf
(* ===== Context specification: people and external systems =====
   
   C4 Model Level 1 (System Context) identifies who uses
   the system and what external systems it interacts with
   before decomposing internals. Person and external system
   declarations provide the outermost zoom level. They
   answer: "Who uses this system, and what does it talk to?"
   
   Relationship declarations connect persons, external
   systems, and internal systems with labeled, directional
   edges that carry description and technology. *)

(* ===== Person: a human user or actor ===== *)

PersonDecl      = "person" DOTTED_IDENT "{"
                    { PersonProperty }
                  "}" ;

PersonProperty  = "description" ":" STRING ";"
                | TagAnnotation
                | RationaleDecl ;

(* ===== External system: a system outside our boundary ===== *)

ExternalSystemDecl
                = "external" "system" DOTTED_IDENT "{"
                    { ExternalSystemProperty }
                  "}" ;

ExternalSystemProperty
                = "description" ":" STRING ";"
                | "technology" ":" STRING ";"
                | TagAnnotation
                | RationaleDecl ;

(* ===== Relationship: labeled directional edge ===== *)

RelationshipDecl
                = DOTTED_IDENT "->" DOTTED_IDENT "{"
                    { RelationshipProperty }
                  "}" 
                | DOTTED_IDENT "->" DOTTED_IDENT ":"
                    STRING ";" ;

(* Short form: Source -> Target : "description";
   Block form: Source -> Target { description: "..."; technology: "..."; }

   The short form is syntactic sugar for the common case
   where only a description is needed. The parser
   distinguishes from topology allow/deny rules by parent
   context: RelationshipDecl appears at the top level,
   while allow/deny rules appear inside a topology block.
   See §5.17 for disambiguation. *)

RelationshipProperty
                = "description" ":" STRING ";"
                | "technology" ":" STRING ";"
                | TagAnnotation
                | RationaleDecl ;

(* ===== Tag annotation: general-purpose element tagging ===== *)

TagAnnotation   = "@" "tag" "(" StringList ")" ";" ;

(* Tags are a general-purpose classification mechanism
   that works across all registers. Any declaration that
   includes TagAnnotation in its property list can be tagged.
   Tags are used by view declarations to include/exclude
   elements by classification.
   
   Example: @tag("frontend", "blazor");
   
   The tag annotation uses the existing "@" Annotation syntax
   but is listed as a separate production because it accepts
   a StringList (multiple tags) rather than a single
   AnnotationValue. Semantic analysis validates that tag
   values are non-empty strings. *)
```

### 3.4 Systems specification productions

```ebnf
(* ===== System: root of the decomposition tree ===== *)

SystemDecl      = "system" DOTTED_IDENT "{"
                    { SystemMember }
                  "}" ;

SystemMember    = SystemProperty
                | AuthoredComponentDecl
                | ConsumedComponentDecl ;

SystemProperty  = "target" ":" TargetValue ";"
                | "responsibility" ":" STRING ";" ;

TargetValue     = DOTTED_IDENT | STRING ;
                  (* Target framework monikers like "net10.0" have
                     a digit-leading segment after the dot, which is
                     not valid as a DOTTED_IDENT (IDENT requires a
                     leading letter). The specifier must quote these:
                     target: "net10.0";
                     DOTTED_IDENT covers cases where the target is a
                     valid dotted name (e.g., net10 without a minor
                     version, though this is uncommon in practice).
                     A single IDENT is a valid DOTTED_IDENT, so no
                     separate IDENT alternative is needed. *)

(* ===== Authored component: things we build ===== *)

AuthoredComponentDecl
                = "authored" "component" DOTTED_IDENT "{"
                    { AuthoredProperty }
                  "}" ;

AuthoredProperty
                = "kind" ":" KindValue ";"
                | "target" ":" TargetValue ";"
                | "responsibility" ":" STRING ";"
                | "path" ":" STRING ";"
                | "status" ":" StatusValue ";"
                | InlineContractDecl
                | RationaleDecl ;

(* Inline contracts on authored components describe the API
   surface that the component exposes to its dependents. This
   is especially important for existing components: the spec
   cannot describe their internals (they are already built),
   but it must declare what they provide at the boundary so
   that dependent components and the LLM know what is
   available. For new components, the contract declares the
   intended API surface before the code is written.

   This reuses the same InlineContractDecl production used
   by consumed components. The semantics differ slightly:
   on a consumed component, the contract states what we
   expect from an external dependency; on an authored
   component, it states what we commit to providing. Both
   are boundary commitments. *)

StatusValue     = "existing" | "new" ;
                  (* "existing" means the component's project and source
                     files already exist on disk at the declared path.
                     It must be added to the solution but not created.
                     "new" (the default if omitted) means the component
                     does not yet exist and must be created via the
                     platform's project creation tooling (e.g., dotnet new).
                     Semantic analysis defaults to "new" when status
                     is absent. *)

                  (* "path" is the relative directory where the component
                     lives or will be created, relative to the solution
                     root. Example: path: "src/Analytics.Engine";
                     For existing components, the path tells the LLM
                     where to find the .csproj for dotnet sln add and
                     dotnet add reference. For new components, the path
                     tells the LLM where to create the project. *)

KindValue       = IDENT | STRING ;
                  (* Component kind values like "library", "application",
                     "tests" are plain IDENTs. The taxonomy also defines
                     hyphenated kinds like "blazor-server-host" and
                     "api-host". Since IDENT does not allow hyphens,
                     hyphenated kinds must be quoted as STRING:
                     kind: "blazor-server-host";
                     Plain kinds remain unquoted for brevity:
                     kind: library; *)

(* ===== Consumed component: things we use ===== *)

ConsumedComponentDecl
                = "consumed" "component" DOTTED_IDENT "{"
                    { ConsumedProperty }
                  "}" ;

ConsumedProperty
                = "source" ":" SourceExpr ";"
                | "version" ":" STRING ";"
                | "responsibility" ":" STRING ";"
                | "used_by" ":" "[" IdentList "]" ";"
                | InlineContractDecl
                | RationaleDecl ;

InlineContractDecl
                = "contract" "{" { ContractClause } "}" ;

(* Inline contracts have no name. The contract belongs to
   the enclosing consumed component. See §5.8. *)

SourceExpr      = IDENT "(" STRING ")" ;

(* Source registries:
   nuget("package-name")
   nuget("https://registry-url")
   npm("package-name")
   container("image-name")
   The IDENT names the registry type. Semantic analysis
   validates that the registry type is known. *)

IdentList       = DOTTED_IDENT { "," DOTTED_IDENT } ;

(* ===== Topology: dependency rules ===== *)

TopologyDecl    = "topology" DOTTED_IDENT "{"
                    { TopologyMember }
                  "}" ;

TopologyMember  = "allow" DOTTED_IDENT "->" DOTTED_IDENT ";"
                | "allow" DOTTED_IDENT "->" DOTTED_IDENT "{"
                    { TopologyEdgeProperty } "}"
                | "deny"  DOTTED_IDENT "->" DOTTED_IDENT ";"
                | "deny"  DOTTED_IDENT "->" DOTTED_IDENT "{"
                    { TopologyEdgeProperty } "}"
                | InvariantDecl
                | RationaleDecl ;

TopologyEdgeProperty
                = "description" ":" STRING ";"
                | "technology" ":" STRING ";"
                | RationaleDecl ;

(* Topology edges may optionally carry a block with description,
   technology, and rationale. The block form enriches the edge
   with communication protocol and intent. The simple semicolon
   form remains valid for brevity.
   
   Example (simple):   allow Dashboard.App -> Analytics.Engine;
   Example (enriched): allow Dashboard.App -> PaymentGateway {
                          technology: "REST/HTTPS";
                          description: "Fetches transaction history.";
                        }
   
   Disambiguation: after "allow"/"deny" DOTTED_IDENT "->" DOTTED_IDENT,
   the parser peeks at the next token. If ";" -> simple form.
   If "{" -> block form. See §5.18. *)

(* ===== Phase: ordered construction with gates ===== *)

PhaseDecl       = "phase" IDENT "{"
                    { PhaseMember }
                  "}" ;

PhaseMember     = "requires" ":" PhaseRefList ";"
                | "produces" ":" "[" IdentList "]" ";"
                | GateDecl
                | RationaleDecl ;

PhaseRefList    = IDENT { "," IDENT } ;

GateDecl        = "gate" IDENT "{"
                    { GateProperty }
                  "}" ;

GateProperty    = "command" ":" STRING ";"
                | "expects" ":" GateExpectsList ";" ;

(* Both command and expects are required in a well-formed
   gate, but the grammar accepts them in any order. Semantic
   analysis validates that both are present. *)

GateExpectsList = GateExpect { "," GateExpect } ;

GateExpect      = Expr
                | STRING ;

(* Gate expects entries are either machine-evaluable
   expressions (errors == 0) or prose expectations
   ("Loads in browser..."). See §5.6 for disambiguation. *)

(* ===== Trace: many-to-many cross-reference ===== *)

TraceDecl       = "trace" DOTTED_IDENT "{"
                    { TraceMember }
                  "}" ;

TraceMember     = DOTTED_IDENT "->" "[" IdentList "]" ";"
                | InvariantDecl ;

(* ===== Constraint: system-level rule ===== *)

ConstraintDecl  = "constraint" DOTTED_IDENT "{"
                    { ConstraintMember }
                  "}" ;

ConstraintMember
                = "scope" ":" ScopeExpr ";"
                | "rule" ":" STRING ";"
                | RationaleDecl ;

ScopeExpr       = "all" ScopeWord [ ScopeWord ]
                | "[" IdentList "]" ;

(* ScopeWord is defined in §2.2 (expression grammar):
   ScopeWord = IDENT | "authored" | "consumed" ;

   "all authored components" -> all + qualifier(KW_AUTHORED) + noun(IDENT)
   "all components"          -> all + noun(IDENT), no qualifier
   "[Dashboard.UI, Analytics.App]" -> explicit list

   Disambiguation: see §5.7. *)

(* ===== Package policy: dependency governance ===== *)

PackagePolicyDecl
                = "package_policy" DOTTED_IDENT "{"
                    { PolicyMember }
                  "}" ;

PolicyMember    = "source" ":" SourceExpr ";"
                | PolicyCategoryRule
                | "default" ":" PolicyDefault ";"
                | RationaleDecl ;

PolicyDefault   = IDENT | "allow" | "deny" ;
                  (* Valid default policy values include
                     require_rationale (IDENT), allow (KW_ALLOW),
                     and deny (KW_DENY). The parser accepts
                     keywords here because policy defaults are
                     semantic values, not structural keywords
                     in this position. *)

PolicyCategoryRule
                = ( "deny" | "allow" ) "category" "(" STRING ")"
                    "includes" "[" StringList "]" ";" ;

StringList      = STRING { "," STRING } ;

(* ===== Platform realization: .NET solution ===== *)

DotNetSolutionDecl
                = "dotnet" "solution" DOTTED_IDENT "{"
                    { SolutionMember }
                  "}" ;

SolutionMember  = "format" ":" IDENT ";"
                | "startup" ":" DOTTED_IDENT ";"
                | SolutionFolderDecl
                | RationaleDecl ;

SolutionFolderDecl
                = "folder" STRING "{"
                    "projects" ":" "[" IdentList "]" ";"
                    { RationaleDecl }
                  "}" ;

```

### 3.5 Deployment specification productions

```ebnf
(* ===== Deployment: infrastructure mapping =====

   Deployment declarations map the logical system (containers
   and components) onto physical or virtual infrastructure.
   Each deployment block represents a named environment
   (Production, Staging, Development). Deployment nodes
   represent infrastructure elements: servers, cloud services,
   container orchestrators, Kubernetes pods, etc. Nodes
   nest to model infrastructure hierarchy (cloud region
   contains a cluster, cluster contains a pod, pod contains
   a container instance).

   Instance declarations within nodes reference authored
   components from the system tree, creating a traceable
   link from logical architecture to operational topology.

   This corresponds to C4 Model deployment diagrams:
   the bridge between logical architecture and operational
   reality. *)

DeploymentDecl  = "deployment" DOTTED_IDENT "{"
                    { DeploymentMember }
                  "}" ;

DeploymentMember
                = DeploymentNodeDecl
                | RationaleDecl ;

DeploymentNodeDecl
                = "node" STRING "{"
                    { DeploymentNodeProperty }
                  "}" ;

(* Nodes use STRING names (not IDENT) because infrastructure
   names often contain spaces, dots, or special characters:
   "Azure App Service", "us-east-1", "k8s-prod-cluster".
   Nodes may nest to represent infrastructure hierarchy. *)

DeploymentNodeProperty
                = "technology" ":" STRING ";"
                | "instance" ":" DOTTED_IDENT ";"
                | DeploymentNodeDecl
                | TagAnnotation
                | RationaleDecl ;

(* "instance" references an authored component declared in
   the system tree. It is the link between the logical
   component and the infrastructure node that hosts it.
   A node may contain multiple instances and/or child nodes.

   Example:
   deployment Production {
       node "Azure App Service" {
           technology: "Linux/P1v3";
           instance: Analytics.App;
       }
       node "Azure SQL" {
           technology: "SQL Database S3";
           instance: Dashboard.Database;
       }
   }
*)
```

### 3.6 View specification productions

```ebnf
(* ===== Views: model-to-diagram projections =====

   View declarations define which subset of the model to
   render as a diagram. This implements the Structurizr
   principle of model-vs-views separation: the model
   (systems, components, persons, external systems,
   deployment nodes) is defined once; views select subsets
   for visualization.

   Views are not rendering instructions. They are
   specification-level declarations of what to show. The
   projection layer decides how to render them (Mermaid,
   DOT, Structurizr, etc.).

   View scoping:
   - "of DOTTED_IDENT" scopes to a declared system,
     component, or deployment environment
   - ViewKind determines the zoom level and diagram type *)

ViewDecl        = "view" ViewKind [ "of" DOTTED_IDENT ]
                    DOTTED_IDENT "{"
                    { ViewProperty }
                  "}" ;

ViewKind        = "systemLandscape"
                | "systemContext"
                | "container"
                | "component"
                | "deployment" ;

(* ViewKind determines what abstraction level the view
   renders:
   - systemLandscape: all persons, systems, external systems
   - systemContext: one system with its connections
   - container: containers within a system
   - component: components within a container
   - deployment: infrastructure nodes for an environment

   The "of" clause scopes the view:
   - systemContext requires "of <system>"
   - container requires "of <system>"
   - component requires "of <container>"
   - deployment requires "of <deployment-environment>"
   - systemLandscape does not use "of" *)

ViewProperty    = "include" ":" ViewFilterExpr ";"
                | "exclude" ":" ViewFilterExpr ";"
                | "autoLayout" ":" LayoutDirection ";"
                | "description" ":" STRING ";"
                | TagAnnotation
                | RationaleDecl ;

ViewFilterExpr  = "all"
                | "tagged" STRING
                | "[" IdentList "]" ;

(* View filters select elements for inclusion or exclusion:
   - "all": include/exclude everything in scope
   - "tagged" STRING: elements carrying a matching @tag
   - "[" IdentList "]": explicit element list by name

   Example:
   view systemContext of AnalyticsDashboard SystemContextView {
       include: all;
       exclude: tagged "internal-only";
       autoLayout: top-down;
   }
*)

LayoutDirection = "top-down" | "left-right"
                | "bottom-up" | "right-left" ;

(* Layout is a hint to the rendering engine, not a precise
   constraint. Renderers may adjust layout for readability.
   Quoted because hyphenated values are not valid IDENTs. *)
```

### 3.7 Dynamic specification productions

```ebnf
(* ===== Dynamic: behavioral interaction sequences =====

   Dynamic declarations capture runtime behavior by showing
   how elements collaborate to fulfill a specific use case
   or scenario. Each step is a numbered interaction between
   two elements with a description and optional technology.

   This corresponds to C4 dynamic diagrams and fills the
   gap between static topology (who may depend on whom) and
   runtime flow (how a specific request moves through the
   system). Topology defines the rules; dynamic declarations
   show how a scenario exercises those rules. *)

DynamicDecl     = "dynamic" DOTTED_IDENT "{"
                    { DynamicStep }
                  "}" ;

DynamicStep     = INTEGER ":" DOTTED_IDENT "->" DOTTED_IDENT
                    ":" STRING [ ";" ]
                | INTEGER ":" DOTTED_IDENT "->" DOTTED_IDENT
                    "{" { DynamicStepProperty } "}" ;

DynamicStepProperty
                = "description" ":" STRING ";"
                | "technology" ":" STRING ";" ;

(* Each step carries a sequence number, source, target,
   and description. The sequence number is part of the
   specification (not inferred by position) so that steps
   can be reordered in the source without changing the
   declared sequence.

   Simple form: 1: Analyst -> Dashboard.App : "Submits filter parameters.";
   Block form:  2: Dashboard.App -> Analytics.Engine {
                    description: "Requests revenue computation.";
                    technology: "method call";
                }

   Dynamic declarations reference persons, external systems,
   and components from the model. Semantic analysis validates
   that all participants are declared. *)
```

### 3.8 Design specification productions

```ebnf
(* ===== Design specification: pages and visualizations ===== *)

(* Pages and visualizations are semantically children of
   authored components but syntactically appear as top-level
   declarations in their own spec blocks. This preserves the
   natural interleaving of formal structure and prose intent
   in the .spec.md document.

   A markdown heading followed by a spec block whose first
   keyword is "page" or "visualization" opens a prose context.
   All subsequent prose and child spec blocks, until the next
   heading at the same or higher level, are collected as
   children of that context. See §5.15. *)

PageDecl        = "page" DOTTED_IDENT "{"
                    { PageProperty }
                  "}" ;

PageProperty    = "host" ":" DOTTED_IDENT ";"
                | "route" ":" STRING ";"
                | "concepts" ":" "[" IdentList "]" ";"
                | "role" ":" STRING ";"
                | "cross_links" ":" "[" IdentList "]" ";"
                | RationaleDecl ;

VisualizationDecl
                = "visualization" DOTTED_IDENT "{"
                    { VisualizationProperty }
                  "}" ;

VisualizationProperty
                = "page" ":" DOTTED_IDENT ";"
                | "component" ":" DOTTED_IDENT ";"
                | ParametersBlock
                | "sliders" ":" "[" IdentList "]" ";"
                | RationaleDecl ;

ParametersBlock = "parameters" "{"
                    { ParameterBinding }
                  "}" ;

ParameterBinding = IDENT ":" Expr ";" ;

(* ParameterBinding connects a chart parameter name to a
   computation method call or computed expression. Example:
   segments: Revenue.RevenueBySegment(startDate, endDate,
             segmentType);
   The left side is the chart parameter name (IDENT).
   The right side is an Expr, typically a function call
   referencing the computation library's API surface. *)
```

---

## 4. Grammar-to-AST Mapping

Every grammar production corresponds to an AST node type defined in the SpecChat.Core section of SpecLang-Specification.md. This table confirms completeness.

### Data specification nodes

| Grammar Production | AST Node (SpecChat.Core) |
|---|---|
| SpecDocument | `SpecDocument` |
| EntityDecl | `EntityDecl` |
| FieldDecl | `FieldDecl` |
| TypeExpr / BaseType | `TypeExpr` (PrimitiveTypeExpr, NamedTypeExpr, ArrayTypeExpr, OptionalTypeExpr, UnknownTypeExpr) |
| Annotation | `Annotation` |
| EnumDecl | `EnumDecl` |
| EnumMember | `EnumMemberDecl` |
| ContractDecl | `ContractDecl` |
| ContractClause | requires/ensures/guarantees clauses within ContractDecl |
| RefinementDecl | `RefinementDecl` |
| InvariantDecl | `InvariantDecl` |
| RationaleDecl | `RationaleDecl` (SimpleRationale or StructuredRationale) |
| Expr and sub-productions | `Expr` (BinaryExpr, UnaryExpr, MemberAccessExpr, IdentifierExpr, LiteralExpr, InExpr, CallExpr, QuantifierExpr, ListExpr) |

### Context specification nodes

| Grammar Production | AST Node (SpecChat.Core) |
|---|---|
| PersonDecl | `PersonDecl` |
| ExternalSystemDecl | `ExternalSystemDecl` |
| RelationshipDecl | `RelationshipDecl` |
| TagAnnotation | `TagAnnotation` (list of tag strings, attachable to any tagged declaration) |

### Systems specification nodes

| Grammar Production | AST Node (SpecChat.Core) |
|---|---|
| SystemDecl | `SystemDecl` |
| AuthoredComponentDecl | `AuthoredComponentDecl` |
| ConsumedComponentDecl | `ConsumedComponentDecl` |
| InlineContractDecl | `ContractDecl` (unnamed, parent reference to component) |
| SourceExpr | within `ConsumedComponentDecl` |
| TopologyDecl | `TopologyDecl` |
| allow/deny rules | `DependencyRule` (with optional `Description`, `Technology`, `RationaleDecl`) |
| TopologyEdgeProperty | within `DependencyRule` |
| PhaseDecl | `PhaseDecl` |
| GateDecl | `GateDecl` |
| TraceDecl | `TraceDecl` |
| TraceMember (arrow) | `TraceLink` |
| ConstraintDecl | `ConstraintDecl` |
| PackagePolicyDecl | `PackagePolicyDecl` |
| PolicyCategoryRule | `CategoryRule` |
| DotNetSolutionDecl | `DotNetSolutionDecl` |
| SolutionFolderDecl | `SolutionFolderDecl` |

### Deployment specification nodes

| Grammar Production | AST Node (SpecChat.Core) |
|---|---|
| DeploymentDecl | `DeploymentDecl` |
| DeploymentNodeDecl | `DeploymentNodeDecl` (recursive: nodes contain child nodes) |

### View specification nodes

| Grammar Production | AST Node (SpecChat.Core) |
|---|---|
| ViewDecl | `ViewDecl` |
| ViewFilterExpr | `ViewFilter` (AllFilter, TaggedFilter, ExplicitFilter) |

### Dynamic specification nodes

| Grammar Production | AST Node (SpecChat.Core) |
|---|---|
| DynamicDecl | `DynamicDecl` |
| DynamicStep | `DynamicStep` |

### Design specification nodes

| Grammar Production | AST Node (SpecChat.Core) |
|---|---|
| PageDecl | `PageDecl` |
| VisualizationDecl | `VisualizationDecl` |
| ParameterBinding | `ParameterBindingDecl` |
| (prose context) | `ProseIntent` (not a grammar production; assembled by the markdown extractor) |

All AST node types in the specification have corresponding grammar productions. Helper productions that do not map to dedicated AST types: FieldName, ContextualKeyword, AnnotationName, AnnotationValue, RangeValue, TargetValue, KindValue, StatusValue, ScopeWord, ScopeRef, ScopeExpr, PhaseRefList, GateProperty, GateExpectsList, GateExpect, PolicyDefault, StringList, IdentList, ParametersBlock, PersonProperty, ExternalSystemProperty, RelationshipProperty, TopologyEdgeProperty, DeploymentMember, DeploymentNodeProperty, ViewKind, ViewProperty, ViewFilterExpr, LayoutDirection, DynamicStepProperty. These are structural sub-parts of larger nodes.

---

## 5. Ambiguity Resolution

### 5.1 DOTTED_IDENT vs. member access in expressions

The lexer emits `Dashboard.UI` as a single DOTTED_IDENT token. In expression context, the parser must split it into a member access chain.

- In **declaration name positions** (after `entity`, `component`, `system`, topology component references, trace endpoints, etc.), DOTTED_IDENT is consumed whole as a name.
- In **expression positions** (inside invariants, contracts, requires/ensures), DOTTED_IDENT is split by the parser into a chain of AccessExpr nodes: `CoffeeDrink.espresso` becomes AccessExpr(PrimaryExpr("CoffeeDrink"), "espresso").

The parser knows which context it is in because declaration names follow specific keywords, while expressions follow `:` in invariants or appear as clause bodies in contracts.

### 5.2 `requires` keyword overloading

`requires` appears in two contexts:
- In a `contract`: `requires Expr ;` (a contract precondition)
- In a `phase`: `requires : PhaseRefList ;` (a phase dependency)

Resolution: the parser distinguishes by parent context. Inside a `contract { }` block, `requires` is followed by an expression. Inside a `phase { }` block, `requires` is followed by `:` then identifiers. The `:` disambiguates.

### 5.3 Semicolons vs. braces

Semicolons terminate single-line declarations (fields, properties, simple rationale). Braces delimit blocks (entities, components, structured rationale). There is no semicolon after a closing brace. Rule: if it ends with `}`, no trailing `;`.

### 5.4 Optional trailing commas

Enum member lists allow an optional trailing comma before `}`. The parser should also tolerate trailing commas in IdentList, StringList, and ExprList for usability, though the grammar does not require them.

### 5.5 Rationale disambiguation

After `rationale`, the parser peeks:
- STRING followed by `;` -> Tier 1 simple rationale
- STRING followed by `{` -> Tier 2 structured rationale with target name
- `{` immediately -> Tier 2 structured rationale without target name

### 5.6 Gate expects: expression vs. prose

Gate expects entries can be expressions (`errors == 0`) or prose strings (`"Loads in browser..."`). Disambiguation strategy: the parser attempts to parse an Expr. If successful and the next token is `,` or `;` (the list delimiter or terminator), the entry is an expression. If expression parsing fails, the parser rewinds and expects a STRING. In practice, prose expectations are always quoted strings, while expression expectations start with an IDENT (like `errors`, `pass`, `fail`), so the first token distinguishes them in all existing examples: IDENT starts an expression, STRING starts prose. But the try-then-fallback approach handles edge cases.

### 5.7 Scope word count disambiguation

ScopeRef (in quantifiers) and ScopeExpr (in constraints) both need to determine whether the scope is one word or two. The strategy is the same in both cases: after consuming `all` and the first ScopeWord, peek at the next token.

In **quantifier context** (ScopeRef): if the next token is `have` or `satisfy`, the scope is one word. Otherwise, consume a second ScopeWord, then expect `have` or `satisfy`.

In **constraint context** (ScopeExpr): if the next token is `;` (end of the scope property), the scope is one word. Otherwise, consume a second ScopeWord, then expect `;`.

The general rule: consume `all`, consume one ScopeWord, peek at the next token. If it is a **terminator** for the current context (`have`/`satisfy` for quantifiers, `;` for constraints), stop at one word. If it is another ScopeWord, consume it as the second word.

### 5.8 Inline contract inside consumed component

A consumed component can contain `contract { ... }` without a name. Top-level contracts require a name: `contract IngressBoundary { ... }`. Resolution: when `contract` appears inside a ConsumedComponentDecl block, the parser uses the InlineContractDecl production (no name). At the top level, it uses ContractDecl (name required).

### 5.9 `allow`/`deny` in topology vs. package policy

- In `topology`: `allow DOTTED_IDENT "->" DOTTED_IDENT ";"`
- In `package_policy`: `allow "category" "(" STRING ")" "includes" "[" StringList "]" ";"`

Resolution: by parent context. Inside a `topology { }` block, `allow`/`deny` is followed by a DOTTED_IDENT (component name). Inside a `package_policy { }` block, `allow`/`deny` is followed by `category`. The next token after `allow`/`deny` disambiguates.

### 5.10 Annotation name vs. keyword (`@default`)

`@default(medium)` uses `default` as an annotation name, but `default` is reserved as KW_DEFAULT (for `default: require_rationale;` in package policy). Resolution: the Annotation production uses AnnotationName, which explicitly accepts both IDENT and specific keyword tokens (`"default"`, `"source"`, `"version"`, `"target"`). After `@`, the parser accepts any of these tokens as an annotation name regardless of keyword status. This is safe because `@` never appears before a keyword in any other production.

### 5.11 FLOAT vs. DOTDOT in lexer (`1..10`)

In `@range(1..10)`, the lexer must not interpret `1.` as the beginning of a FLOAT literal. Resolution: after consuming INTEGER and seeing `.`, the lexer peeks one character ahead. If the next character is `.` (forming `..`), the INTEGER is complete and `..` is emitted as DOTDOT. See lexer resolution rule 4a.

### 5.12 Contextual keywords as field names

Inside an entity or refinement body, keyword tokens like `source`, `version`, `target`, `kind`, etc. must be accepted as field names. Resolution: the FieldDecl production uses FieldName, which accepts both IDENT and ContextualKeyword tokens. The parser knows it is in a FieldDecl context because it is inside an EntityDecl or RefinementDecl body and the next token is followed by `:` then a TypeExpr. Contextual keywords are only accepted in these name positions; in their native declaration contexts (e.g., `source:` inside a ConsumedComponentDecl), they retain their keyword role.

### 5.13 Comment tokens inside string literals

`//` inside a quoted string is string content, not a comment start. The lexer disables comment detection when it is inside a string literal (between an opening `"` and its matching closing `"`). This is standard lexer behavior but is stated explicitly because the grammar aims to be implementation-precise.

### 5.14 Keywords as scope qualifiers (`all authored components`)

In quantifier expressions and constraint scope declarations, words like `authored` and `consumed` serve as collection qualifiers. These words are reserved keywords (KW_AUTHORED, KW_CONSUMED) in declaration context. Resolution: the ScopeRef and ScopeExpr productions use ScopeWord, which explicitly accepts both IDENT and the keyword tokens `"authored"` and `"consumed"`. The parser knows it is in scope context because it has already consumed `all` (in a quantifier or scope declaration).

### 5.15 Prose context association (page and visualization)

The markdown extractor must determine whether a spec block opens a prose context. Resolution: after extracting a spec block, the extractor peeks at the first keyword token. If it is KW_PAGE, the block opens a PageContext. If it is KW_VISUALIZATION, the block opens a VisualizationContext. Otherwise, the block is a standalone SpecBlock.

Context boundaries are determined by markdown heading level. The extractor tracks the heading level of the heading immediately preceding the context-opening spec block. All subsequent content (prose and spec blocks) at a deeper heading level belongs to the context. When a heading at the same or higher level is encountered, the context closes.

Example: if `## Executive Dashboard` precedes a `page ExecutiveDashboard { ... }` spec block, the PageContext is at level 2. All `###`-level headings and their content belong to the ExecutiveDashboard context. The next `##` heading closes it.

Prose within a context becomes `ProseIntent` nodes associated with the parent `PageDecl` or `VisualizationDecl` in the AST. Prose outside any context remains `ProseSection` nodes (documentary, not architecturally associated).

### 5.16 `page` keyword in VisualizationProperty vs. top-level PageDecl

`page` appears in two contexts: as a top-level declaration keyword (`page ExecutiveDashboard { ... }`) and as a property within a visualization (`page: ExecutiveDashboard;`). Resolution: by parent context. At the top level (or in a PageContext), `page` followed by DOTTED_IDENT `{` starts a PageDecl. Inside a `visualization { }` block, `page` followed by `:` is a VisualizationProperty. The second token (`:` vs. DOTTED_IDENT) disambiguates.

### 5.17 RelationshipDecl vs. TopologyMember vs. TraceMember

`DOTTED_IDENT "->" DOTTED_IDENT` appears in three contexts: top-level relationship declarations, topology allow/deny rules, and trace member mappings. Resolution: by parent context. At the top level, `DOTTED_IDENT "->"` starts a RelationshipDecl. Inside a `topology { }` block, `allow` or `deny` precedes the arrow, so it is a TopologyMember. Inside a `trace { }` block, the arrow is followed by `"[" IdentList "]"` (a target list), making it a TraceMember. The top-level RelationshipDecl is the only context where a bare `DOTTED_IDENT` (without a leading keyword) starts a declaration; the parser recognizes this by process of elimination after checking all keyword-led alternatives.

### 5.18 Topology edge: simple vs. block form

After `allow`/`deny` DOTTED_IDENT `->` DOTTED_IDENT, the parser peeks at the next token. If `;`, the edge is the simple form (no properties). If `{`, the edge has a property block containing description, technology, and/or rationale. This is the same peek-ahead pattern used for rationale disambiguation (§5.5).

### 5.19 `external system` two-word keyword

`external system` is a two-word keyword sequence, handled at the parser level like `authored component` and `consumed component`. The lexer emits KW_EXTERNAL + KW_SYSTEM as separate tokens. The parser, upon seeing KW_EXTERNAL, expects KW_SYSTEM to follow and then consumes the ExternalSystemDecl production.

### 5.20 `deployment` keyword in ViewKind vs. DeploymentDecl

`deployment` appears as both a top-level declaration (`deployment Production { ... }`) and as a ViewKind value (`view deployment of Production ...`). Resolution: at the top level, `deployment` followed by DOTTED_IDENT `{` starts a DeploymentDecl. Inside a `view` declaration, `deployment` appears as the ViewKind before the optional `of` clause. The parser distinguishes by context: after `view`, the next token is a ViewKind; at the top level, it starts a DeploymentDecl.

### 5.21 `@tag` annotation vs. standard annotations

`@tag("frontend", "blazor")` uses a StringList (multiple values) rather than a single AnnotationValue. The parser distinguishes by annotation name: after `@`, if the next token is `tag`, the parser uses the TagAnnotation production (expecting `"(" StringList ")"`). For all other annotation names, it uses the standard Annotation production (expecting `"(" AnnotationValue ")"`). This is safe because `tag` is reserved as KW_TAG and will not appear as an annotation name in any other context.

### 5.22 DynamicStep sequence number vs. other INTEGER contexts

Inside a `dynamic { }` block, each step begins with `INTEGER ":"`. This is unambiguous because no other production inside a dynamic block starts with INTEGER, and the `:` after INTEGER distinguishes it from an expression context where INTEGER might appear as a literal.

---

## 6. Implementation Notes

### 6.1 Parser architecture

The grammar is designed for recursive descent. Each non-terminal becomes a method in the parser class. The expression grammar uses the standard precedence climbing pattern: each precedence level is a method that calls the next-higher-precedence method.

```
ParseExpr()          -> ParseImpliesExpr()
ParseImpliesExpr()   -> ParseOrExpr() [ "implies" ParseImpliesExpr() ]
ParseOrExpr()        -> ParseAndExpr() { "or" ParseAndExpr() }
ParseAndExpr()       -> ParseNotExpr() { "and" ParseNotExpr() }
ParseNotExpr()       -> "not" ParseNotExpr() | ParseCompareExpr()
ParseCompareExpr()   -> ParseAccessExpr() [ op ParseAccessExpr() ]
ParseAccessExpr()    -> ParsePrimaryExpr() { "." IDENT [ "(" args ")" ] }
ParsePrimaryExpr()   -> IDENT ["(" args ")"]  (* IdentifierExpr or CallExpr *)
                      | DOTTED_IDENT          (* re-tokenize into AccessExpr chain *)
                      | literal               (* LiteralExpr *)
                      | "[" list "]"          (* ListExpr *)
                      | "(" expr ")"          (* parenthesized *)
                      | quantifier            (* QuantifierExpr *)
```

### 6.2 Error recovery

The parser should support two modes:
- **Strict mode** (default): first error halts parsing; returns diagnostics.
- **Lenient mode** (for IDE/quality feedback): on error, skip tokens until a synchronization point (next `}` at the expected nesting depth, or next keyword that starts a top-level declaration), record a diagnostic, and continue parsing.

Synchronization points are top-level keywords: `entity`, `enum`, `contract`, `refines`, `person`, `external`, `system`, `topology`, `phase`, `trace`, `constraint`, `package_policy`, `dotnet`, `deployment`, `view`, `dynamic`, `page`, `visualization`.

### 6.3 Contextual keyword handling

The parser maintains a set of ContextualKeyword tokens that are accepted as identifiers in field-name and enum-member positions. Implementation: when the parser is inside an EntityDecl or RefinementDecl body and expects a field name, it calls a `ParseFieldName()` method that accepts either IDENT or any token in the ContextualKeyword set. Outside these positions, keyword tokens retain their reserved meaning. This avoids the need for a context-sensitive lexer; the lexer always emits keyword tokens, and the parser selectively accepts them in name positions.

### 6.4 Known limitation: keyword breadth

SpecChat reserves 89 keywords. The ContextualKeyword mechanism (§5.12) handles the most common collisions: words like `source`, `version`, `target`, `kind` can be used as field names because they only have keyword meaning in specific declaration contexts and the parser can distinguish by position.

Two categories of keywords CANNOT be used as field names and are permanently reserved:

- **Expression keywords** (`implies`, `and`, `or`, `not`, `contains`, `excludes`, `in`, `all`, `exists`, `have`, `satisfy`, `count`): these are consumed by the expression parser, which runs inside invariant and contract bodies that also appear inside entities.
- **Structural keywords** (`entity`, `enum`, `contract`, `refines`, `rationale`, `invariant`): these begin EntityMember or TopLevelDecl alternatives. Adding them to ContextualKeyword would create true parse ambiguity (is `rationale "text";` a simple rationale declaration or a field named `rationale` of type `"text"`?).

As the language evolves, further collisions may require moving more keywords from reserved to contextual. A future revision may adopt a smaller permanently-reserved set. For Phase 1, the current partition is workable.

### 6.5 EntityMember parse order

Inside an entity or refinement body, the parser must distinguish between FieldDecl, InvariantDecl, and RationaleDecl. The disambiguation is based on the first token:

- KW_INVARIANT -> InvariantDecl
- KW_RATIONALE -> RationaleDecl
- IDENT or ContextualKeyword -> FieldDecl (peek confirms `:` follows)

This order is unambiguous because the three alternatives start with distinct token classes.

### 6.6 TextSpan tracking

Every AST node carries a `TextSpan` recording its source location (start line, start column, end line, end column) in the original `.spec.md` file. The markdown extractor records the offset of each ` ```spec ` block so that line numbers in the token stream map back to the original document.

### 6.7 Implementation phase scope

Phase 1 (Data Model): lexer (all tokens), expression grammar (complete), and data specification productions (EntityDecl, EnumDecl, ContractDecl, RefinementDecl, FieldDecl, TypeExpr, InvariantDecl, RationaleDecl, Annotation).

Phase 2 (Systems Model): extends the parser with SystemDecl, AuthoredComponentDecl, ConsumedComponentDecl, TopologyDecl, PhaseDecl, GateDecl, TraceDecl, ConstraintDecl, PackagePolicyDecl, DotNetSolutionDecl.

Phase 3 (Design Model): extends the parser with PageDecl, VisualizationDecl, ParametersBlock, ParameterBinding. Extends the markdown extractor with prose context association (PageContext, VisualizationContext, ProseIntent).

The expression grammar does not change between phases. It is complete in Phase 1 and used by all three registers: invariants in entities (data), invariants in topology/traces (systems), and parameter bindings in visualizations (design).
