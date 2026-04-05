# SpecChat MCP Server + Copilot Integration Plan

## Context

SpecChat currently integrates with Claude Code via slash commands (`.claude/commands/spec-chat.md`). To reach developers using GitHub Copilot in VS Code and Visual Studio 2026, we need an MCP server that exposes SpecLang parsing, validation, and realization support as tools, plus the `.agent.md`, `.instructions.md`, and `.prompt.md` files that wire those tools into Copilot's agent mode.

The MCP server is the only integration path worth pursuing. Prompt-only integration cannot carry the SpecLang grammar's disambiguation rules or perform real semantic analysis. The server makes SpecChat a first-class language in Copilot's toolchain.

## Dual-Implementation Strategy

The repository structure reserves space for two implementations. The C# (.NET) implementation is the current scope. The TypeScript (Node) implementation is deferred to a future effort; its design is documented here so the C# implementation can be built with portability in mind.

Both implementations will expose identical MCP tools with identical names, parameters, and return schemas. A consumer's `mcp.json` points to whichever runtime is available. The tool contract is the shared surface; the runtime is an installation choice.

| | C# (.NET) | TypeScript (Node) |
|---|---|---|
| **Location** | `src/MCPServer/DotNet/` | `src/MCPServer/TypeScript/` |
| **SDK** | `ModelContextProtocol` NuGet (v1.x) | `@modelcontextprotocol/sdk` npm |
| **Transport** | stdio (`WithStdioServerTransport()`) | stdio (`StdioServerTransport`) |
| **Distribution** | NuGet dotnet tool (`dnx`) | npm package (`npx`) |
| **Primary audience** | .NET developers, VS 2026 users | VS Code users, non-.NET ecosystems |
| **Scope** | Current (Phases 1-5) | Deferred (Phase 6, future) |

The C# implementation is primary because the SpecLang grammar was written for a C# recursive-descent parser, the sample system targets .NET, and the `ModelContextProtocol` NuGet SDK has first-class Visual Studio integration. The TypeScript implementation will follow the same architecture, ported from the C# design, to serve developers who do not have .NET installed.

## Deliverables

### 1a. DotNet MCP Server

A .NET 10 console application using the official `ModelContextProtocol` NuGet SDK with stdio transport, publishable to NuGet as a dotnet tool.

**Project location:** `src/MCPServer/DotNet/`

**Projects within:**
- `SpecChat.Language/` -- parser class library
- `SpecChat.Language.Tests/` -- parser unit tests
- `SpecChat.Mcp/` -- MCP server console app
- `SpecChat.Mcp.Tests/` -- MCP tool integration tests

**Created via:** `dotnet new mcpserver` (.NET 10 MCP Server App template from `Microsoft.McpServer.ProjectTemplates`)

**NuGet packages:**
- `ModelContextProtocol` (v1.x) -- hosting, DI, stdio transport, attribute-based tool discovery
- `Microsoft.Extensions.Hosting` -- generic host
- `Microsoft.Extensions.Logging.Console` -- stderr logging (required; stdout is reserved for MCP protocol)

**Program.cs structure:**
```csharp
var builder = Host.CreateApplicationBuilder(args);
builder.Logging.AddConsole(options =>
{
    options.LogToStandardErrorThreshold = LogLevel.Trace;
});
builder.Services
    .AddMcpServer()
    .WithStdioServerTransport()
    .WithToolsFromAssembly();
await builder.Build().RunAsync();
```

Tool classes use the `[McpServerToolType]` / `[McpServerTool]` attribute pattern for automatic discovery by `WithToolsFromAssembly()`:

```csharp
[McpServerToolType]
public static class SpecParsingTools
{
    [McpServerTool, Description("Parse a .spec.md file, return AST summary and diagnostics.")]
    public static string ParseSpec(
        [Description("Absolute path to the .spec.md file")] string filePath)
    {
        // ...
    }
}
```

**SpecChat.Mcp.csproj configuration (.NET 10 dotnet tool with Native AOT):**
```xml
<PropertyGroup>
    <OutputType>Exe</OutputType>
    <TargetFramework>net10.0</TargetFramework>
    <RuntimeIdentifiers>linux-x64;linux-arm64;win-x64;win-arm64;osx-x64;osx-arm64</RuntimeIdentifiers>
    <PublishSelfContained>true</PublishSelfContained>
    <PublishAot>true</PublishAot>
    <StripSymbols>true</StripSymbols>
    <PackAsTool>true</PackAsTool>
    <ToolCommandName>specchat-mcp</ToolCommandName>
    <PackageId>SpecChat.Mcp</PackageId>
</PropertyGroup>
```

### 1b. TypeScript MCP Server (deferred)

A Node.js application using the official `@modelcontextprotocol/sdk` npm package with stdio transport, publishable to npm. This deliverable is deferred to a future effort; the design is recorded here for continuity.

**Project location:** `src/MCPServer/TypeScript/`

**Structure within:**
- `src/language/` -- parser library (lexer, parser, AST, spec block extractor, semantic analyzer, manifest parser, diagnostic bag)
- `src/tools/` -- MCP tool implementations
- `src/index.ts` -- server entry point
- `tests/` -- test suite

**npm packages:**
- `@modelcontextprotocol/sdk` -- MCP server SDK
- `zod` (v3.25+) -- schema validation (peer dependency of SDK)

**Entry point structure:**
```typescript
import { McpServer } from "@modelcontextprotocol/sdk/server/mcp.js";
import { StdioServerTransport } from "@modelcontextprotocol/sdk/server/stdio.js";

const server = new McpServer({ name: "specchat-mcp", version: "1.0.0" });
// register tools...
const transport = new StdioServerTransport();
await server.connect(transport);
```

### 2. Parser Libraries (one per runtime)

Each runtime contains its own parser implementation following the same architecture.

**Components (identical in both runtimes):**
- `Lexer` -- tokenizer per Grammar sec. 1 (keywords, symbols, DOTTED_IDENT, STRING, etc.)
- `Parser` -- recursive-descent parser per Grammar sec. 2-3 (expressions, DSL productions)
- `Ast/` -- AST node types for all constructs (EntityDecl, SystemDecl, TopologyDecl, PhaseDecl, TraceDecl, ConstraintDecl, PageDecl, etc.)
- `SpecBlockExtractor` -- markdown pre-parser that extracts spec fenced blocks and prose intent contexts
- `SemanticAnalyzer` -- post-parse validation (topology consistency, trace coverage, phase ordering, package policy compliance)
- `ManifestParser` -- markdown table extraction for manifest files (separate from the SpecLang recursive-descent parser; manifests are structured markdown, not spec blocks)
- `DiagnosticBag` -- error/warning collection with source locations

### 3. MCP Tool Contract (shared across both runtimes)

Both implementations expose the same tools with the same names, parameters, and return shapes. The tool contract is defined here once.

#### Parsing Tools (`SpecParsingTools`)

| Tool | Description | Parameters | Returns |
|---|---|---|---|
| `parse_spec` | Parse a `.spec.md` file, return AST summary and diagnostics | `filePath: string` | Parsed construct list + errors/warnings |
| `parse_spec_block` | Parse a single spec block string | `specBlock: string` | AST node + diagnostics |
| `list_constructs` | List all declared constructs in a spec file | `filePath: string` | Typed inventory: entities, components, topologies, phases, etc. |

#### Validation Tools (`SpecValidationTools`)

| Tool | Description | Parameters | Returns |
|---|---|---|---|
| `validate_spec` | Full semantic validation of a spec file | `filePath: string` | Diagnostic list (errors, warnings, info) |
| `check_topology` | Validate topology rules against declared components | `filePath: string` | Topology violations or "valid" |
| `check_traces` | Verify trace coverage invariants | `filePath: string` | Uncovered sources, violated invariants |
| `check_phase_gates` | Validate phase ordering and gate consistency | `filePath: string` | Phase dependency issues |
| `check_package_policy` | Validate consumed components against package policy | `filePath: string` | Policy violations |

#### Manifest Tools (`ManifestTools`)

Note: Manifests are markdown tables, not spec fenced blocks. These tools use the `ManifestParser` (markdown table extraction), not the SpecLang recursive-descent parser.

| Tool | Description | Parameters | Returns |
|---|---|---|---|
| `read_manifest` | Parse manifest, return full inventory with states | `filePath: string` | Spec inventory table with lifecycle states |
| `next_executable` | Identify specs ready for execution based on dependency tiers | `filePath: string` | List of specs whose dependencies are all Executed |
| `check_lifecycle` | Validate lifecycle state transitions | `filePath: string` | State transition violations |
| `update_spec_state` | Advance a spec's lifecycle state (returns proposed edit) | `manifestPath: string, specFilename: string, newState: string` | Proposed manifest edit with date |

#### Realization Tools (`RealizationTools`)

These tools generate output for the *target platform* declared in the spec's `target:` field (e.g., `"net10.0"`), not for the MCP server's own runtime. Both the C# and TypeScript servers produce identical output for a given spec.

| Tool | Description | Parameters | Returns |
|---|---|---|---|
| `extract_contracts` | Extract all contracts from a spec for code generation | `filePath: string` | Contract list with requires/ensures/guarantees |
| `extract_entities` | Extract entity definitions with invariants and annotations | `filePath: string` | Entity models with invariants, annotations, and confidence signals |
| `extract_phase_gates` | Extract phase gate commands for execution | `filePath: string, phaseName: string` | Gate commands, expected outcomes |
| `generate_scaffold` | Generate project/solution scaffold from system and platform realization declarations | `filePath: string` | File tree + project files + workspace structure (format per spec's target platform) |
| `spec_to_interface` | Generate interface/type definitions from entity/contract declarations | `filePath: string, constructName: string` | Source code in the spec's target language |

### 4. Copilot Integration Files

These files live in the consuming project's repo (not in the MCP server repo). We ship them as templates and document them.

#### `.github/copilot-instructions.md` (project-wide)

Establishes spec-first posture: `.spec.md` is source of truth, code is a projection, manifest governs execution order. ~50 lines.

#### `.instructions.md` files (in `.github/instructions/`)

- `speclang-syntax.instructions.md` -- `applyTo: "**/*.spec.md"` -- condensed SpecLang construct reference, keyword list, expression syntax, common patterns.
- `speclang-manifest.instructions.md` -- `applyTo: "**/*.manifest.md"` -- manifest conventions, lifecycle states, tracking block format.

#### `.prompt.md` files (in `.github/prompts/`)

- `spec-chat.prompt.md` -- guided authoring for all five document types. `agent: agent` mode. References MCP tools for validation.
- `spec-the-standard.prompt.md` -- The Standard extension variant.
- `spec-validate.prompt.md` -- validate an existing spec file against grammar and semantic rules.

#### `.agent.md` files (in `.github/agents/`)

- `spec-author.agent.md` -- SpecChat authoring agent. Tools: `edit`, `search/codebase`, `specchat-mcp/*`. Instructions carry the four-layer system, division of labor, and guided authoring flow.
- `spec-implementer.agent.md` -- Realization engine agent. Tools: `edit`, `search/codebase`, `run_in_terminal`, `specchat-mcp/*`. Reads manifest, identifies next spec, generates conforming code.
- Handoff: `spec-author` hands off to `spec-implementer` after spec is authored.

#### `mcp.json` configuration templates

**DotNet -- development (run from source):**
```json
{
  "servers": {
    "specchat-mcp": {
      "type": "stdio",
      "command": "dotnet",
      "args": ["run", "--project", "src/MCPServer/DotNet/SpecChat.Mcp/SpecChat.Mcp.csproj"]
    }
  }
}
```

**DotNet -- installed via NuGet:**
```json
{
  "servers": {
    "specchat-mcp": {
      "type": "stdio",
      "command": "dnx",
      "args": ["SpecChat.Mcp@1.0.0", "--yes"]
    }
  }
}
```

**TypeScript -- development (run from source):**
```json
{
  "servers": {
    "specchat-mcp": {
      "type": "stdio",
      "command": "node",
      "args": ["src/MCPServer/TypeScript/dist/index.js"]
    }
  }
}
```

**TypeScript -- installed via npm:**
```json
{
  "servers": {
    "specchat-mcp": {
      "type": "stdio",
      "command": "npx",
      "args": ["-y", "specchat-mcp@1.0.0"]
    }
  }
}
```

### 5. The Standard Extension Support

When the parser encounters an `architecture TheStandard` declaration, it activates extended validation (layer hierarchy, Flow Forward, Florance Pattern, entity ownership, autonomy, vocabulary checking). The same MCP tools return Standard-specific diagnostics. No separate server needed.

## Repository Structure

```
src/
  MCPServer/
    DotNet/                          # C# implementation (primary)
      SpecChat.Language/             # Parser library (class library)
        Lexer.cs
        Parser.cs
        SemanticAnalyzer.cs
        SpecBlockExtractor.cs
        ManifestParser.cs
        DiagnosticBag.cs
        Ast/
          ... (node types)
      SpecChat.Language.Tests/       # Parser unit tests (xunit)
      SpecChat.Mcp/                  # MCP server (console app, dotnet tool)
        Program.cs
        Tools/
          SpecParsingTools.cs
          SpecValidationTools.cs
          ManifestTools.cs
          RealizationTools.cs
        server.json                  # NuGet MCP registry metadata
      SpecChat.Mcp.Tests/            # MCP tool integration tests
      SpecChat.slnx                  # Solution file (.NET 10 default format)

    TypeScript/                      # TypeScript implementation (secondary)
      src/
        language/
          lexer.ts
          parser.ts
          semantic-analyzer.ts
          spec-block-extractor.ts
          manifest-parser.ts
          diagnostic-bag.ts
          ast/
            ... (node types)
        tools/
          spec-parsing-tools.ts
          spec-validation-tools.ts
          manifest-tools.ts
          realization-tools.ts
        index.ts                     # Server entry point
      tests/
        language/
        tools/
      package.json
      tsconfig.json

tests/
  fixtures/                          # Shared test fixtures (used by both runtimes)
    blazor-harness.spec.md           # Copy of Delivery sample for parser tests
    blazor-harness.manifest.md       # Copy of Delivery sample for manifest tests
    standard-test.spec.md            # Test spec with architecture TheStandard
    broken-topology.spec.md          # Negative test: invalid topology rules
    broken-traces.spec.md            # Negative test: uncovered trace sources
    broken-lifecycle.spec.md         # Negative test: invalid state transitions
    expected/                        # Golden JSON outputs for cross-runtime parity
      parse-spec.json
      validate-spec.json
      read-manifest.json
      list-constructs.json

templates/
  copilot/                           # Mirrors .github/ layout in consuming projects
    .github/
      copilot-instructions.md        # -> .github/copilot-instructions.md
      instructions/
        speclang-syntax.instructions.md
        speclang-manifest.instructions.md
      prompts/
        spec-chat.prompt.md
        spec-the-standard.prompt.md
        spec-validate.prompt.md
      agents/
        spec-author.agent.md
        spec-implementer.agent.md
    .vscode/
      mcp.json                       # Template (pick DotNet or TypeScript variant)
```

## Implementation Sequence

### Phase 1: C# parser foundation (`src/MCPServer/DotNet/`)
1. Create solution and `SpecChat.Language` class library project
2. Implement `SpecBlockExtractor` (markdown to spec block extraction)
3. Implement `Lexer` (full token set per Grammar sec. 1)
4. Implement `Parser` -- data specification subset first (entity, enum, contract, invariant, rationale, refinement)
5. Implement `Parser` -- systems specification (system, topology, phase, trace, constraint, package_policy, dotnet solution)
6. Implement `Parser` -- design specification (page, visualization)
7. Unit tests against `tests/fixtures/blazor-harness.spec.md`

### Phase 2: C# semantic analysis and manifest parsing
1. Implement `SemanticAnalyzer` -- topology validation
2. Implement trace coverage checking
3. Implement phase dependency ordering
4. Implement package policy compliance
5. Implement `ManifestParser` -- markdown table extraction (separate from SpecLang parser; manifests are structured markdown, not spec blocks)
6. Implement lifecycle state validation
7. Implement The Standard extension rules (conditional on architecture declaration)

### Phase 3: C# MCP server (`src/MCPServer/DotNet/SpecChat.Mcp/`)
1. Create `SpecChat.Mcp` project via `dotnet new mcpserver` (stdio transport, Native AOT enabled)
2. Implement `SpecParsingTools` (wire parser to MCP tool attributes)
3. Implement `SpecValidationTools`
4. Implement `ManifestTools`
5. Implement `RealizationTools` (contract extraction, entity extraction, scaffold generation)
6. Integration tests: invoke tools via MCP protocol, validate responses

### Phase 4: Copilot integration files (`templates/copilot/`)
1. Write `.instructions.md` files (condensed SpecLang reference)
2. Write `.prompt.md` files (guided authoring flows)
3. Write `.agent.md` files (spec-author, spec-implementer with handoffs)
4. Write `mcp.json` templates (DotNet and TypeScript variants)
5. Write project-wide `copilot-instructions.md`
6. Test end-to-end in VS Code Copilot agent mode with C# server

### Phase 5: C# distribution
1. Verify `PackAsTool`, `PublishAot`, and `RuntimeIdentifiers` in `.csproj` (set by template, confirm correct)
2. Pack platform-specific NuGet packages: `dotnet pack -o ./artifacts/packages`
3. Add `server.json` for NuGet MCP registry
4. Write installation documentation
5. Publish to NuGet

### Phase 6: TypeScript port (deferred, `src/MCPServer/TypeScript/`)

This phase is not part of the current implementation scope. It is documented here so the C# implementation decisions account for future portability. When this phase begins, the steps are:

1. Initialize Node project with `@modelcontextprotocol/sdk`, `zod`, TypeScript toolchain
2. Port `SpecBlockExtractor` and `Lexer` from C# to TypeScript
3. Port `Parser` (same recursive-descent structure, adapted to TS idioms)
4. Port `SemanticAnalyzer` and `ManifestParser`
5. Implement MCP tool wrappers (same tool names and schemas as C#)
6. Port test suite; verify identical behavior against all `tests/fixtures/` files and `tests/fixtures/expected/` golden outputs
7. Configure as npm package, publish

## Verification

All test specs referenced below live in `tests/fixtures/`. Both runtimes' test suites consume fixtures from this shared directory.

1. **Parser correctness:** `tests/fixtures/blazor-harness.spec.md` parses end-to-end with zero errors. All constructs produce correct AST nodes.
2. **Semantic validation:** Negative-test fixtures (`tests/fixtures/broken-topology.spec.md`, `broken-traces.spec.md`, `broken-lifecycle.spec.md`) produce the expected diagnostics.
3. **Manifest parsing:** `tests/fixtures/blazor-harness.manifest.md` parses correctly; inventory, lifecycle states, dependency tiers, and execution order are extracted accurately.
4. **MCP protocol:** Tools respond to JSON-RPC calls over stdio with correct schemas.
5. **Copilot integration:** In VS Code agent mode with the MCP server running, selecting the `spec-author` agent can author a new spec; selecting `spec-implementer` can read it and generate conforming code.
6. **The Standard extension:** `tests/fixtures/standard-test.spec.md` (contains `architecture TheStandard`) triggers extended validation (layer hierarchy, Flow Forward, Florance, entity ownership, autonomy, vocabulary). The same tools against `tests/fixtures/blazor-harness.spec.md` (no architecture declaration) produce no Standard-specific diagnostics.
7. **Cross-runtime parity (deferred, Phase 6):** Both C# and TypeScript servers produce identical tool output for every fixture file. Golden outputs in `tests/fixtures/expected/` are compared against actual output from each runtime. The C# implementation generates the golden outputs during Phases 1-3; the TypeScript port validates against them.

## Key Technical Decisions

- **Dual runtime, single tool contract.** Both servers expose identical MCP tools. Consumers pick whichever runtime they have installed. The tool contract (names, parameters, return schemas) is the shared API surface.
- **C# first, TypeScript deferred.** The grammar spec was written for C# recursive-descent parsing. Building C# first produces the reference implementation. The TypeScript port is a future effort that will verify behavioral parity against golden outputs generated by the C# server.
- **Stdio transport, not HTTP.** Local-only, no auth complexity, matches Copilot's primary MCP consumption model. HTTP can be added later for remote/cloud scenarios.
- **Proper recursive-descent parser, not regex/heuristics.** The grammar has real ambiguity resolution rules (DOTTED_IDENT vs keyword, expression context vs declaration context). Both runtimes implement the same parser architecture.
- **Single server, not per-extension.** The Standard extension activates conditionally within the same parser and tool set. No need for a separate MCP server.
- **Templates, not a VS Code extension.** The integration files (`.agent.md`, `.instructions.md`, `.prompt.md`) are plain markdown. They do not need an extension host. A scaffolding command or `dotnet new` / `npm init` template can create them.

## References

### MCP SDKs
- [MCP C# SDK](https://github.com/modelcontextprotocol/csharp-sdk) -- official C# SDK, v1.0+
- [MCP C# SDK v1.0 release](https://devblogs.microsoft.com/dotnet/release-v10-of-the-official-mcp-csharp-sdk/) -- latest SDK capabilities
- [Build an MCP server in C#](https://learn.microsoft.com/en-us/dotnet/ai/quickstarts/build-mcp-server) -- Microsoft quickstart
- [MCP TypeScript SDK](https://github.com/modelcontextprotocol/typescript-sdk) -- official TypeScript SDK
- [@modelcontextprotocol/sdk on npm](https://www.npmjs.com/package/@modelcontextprotocol/sdk) -- npm package

### Copilot Integration
- [Custom agents in VS Code](https://code.visualstudio.com/docs/copilot/customization/custom-agents) -- .agent.md format
- [Prompt files in VS Code](https://code.visualstudio.com/docs/copilot/customization/prompt-files) -- .prompt.md format
- [Custom instructions in VS Code](https://code.visualstudio.com/docs/copilot/customization/custom-instructions) -- .instructions.md format
- [MCP servers in VS Code](https://code.visualstudio.com/docs/copilot/customization/mcp-servers) -- mcp.json configuration
- [VS 2026 March Update: Custom Agents](https://devblogs.microsoft.com/visualstudio/visual-studio-march-update-build-your-own-custom-agents/) -- .agent.md in Visual Studio
