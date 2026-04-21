# Global Corp Platform Implementation Brief

## Tracking

| Field | Value |
|---|---|
| Document ID | IMPL-002 |
| itemType | ImplementationBrief |
| slug | global-corp-platform-implementation-brief |
| name | Global Corp Platform Implementation Brief |
| shortDescription | Local-first implementation constraints and refactoring guidance for the Global Corp exemplar spec collection; intended for use in a fresh chat session |
| version | 1 |
| specLangVersion | 0.1.0 |
| publishStatus | Draft |
| retentionPolicy | indefinite |
| freshnessSla | P90D |
| lastReviewed | 2026-04-18 |
| authors | [PER-01 Lena Brandt] |
| reviewers | [PER-11 Anja Petersen] |
| committer | PER-01 Lena Brandt |
| createdAt | 2026-04-18T00:00:00Z |
| updatedAt | 2026-04-18T00:00:00Z |
| Dependencies | SpecLang-Design.md, Global-Corp-Exemplar.md, global-corp.manifest.md |

## 1. Purpose and Audience

This document briefs the next working session on the local-first implementation constraints for the Global Corp Platform and the refactoring required to align the existing spec collection with those constraints.

**Audience:** Claude (or any implementer) starting a fresh chat session to refactor the Global Corp spec collection. This brief should be read first, before any spec file is opened for editing.

**Scope of this brief:**
- Records the governing constraints (the 7 rules you established)
- Records all stack choices you made answering Q1 through Q6
- Documents the target versions of .NET, Aspire, and related tooling per research dated 2026-04-18
- Enumerates the refactoring work ahead in priority order
- Identifies open items that still need decisions before Phase 2c Strategy specs are authored

**Out of scope of this brief:**
- Actual spec file modifications (those happen in the fresh session)
- Code generation from the specs (a downstream activity)
- Cloud deployment details (deferred per your constraint "cloud deployment optional later")

## 2. Prerequisites to Read Before Starting Refactoring

Read these in order. The working session cannot produce coherent refactoring without them.

### 2.1 Active WIP design corpus (9 docs)

All under `E:\Archive\GitHub\dlandi\spec-chat\WIP\`:

1. `SpecLang-Design.md` (2,600+ lines) Language design, BTABOK profile, CoDL/CaDL, Option X absorption.
2. `Spec-Type-System.md` Taxonomy and validation architecture.
3. `MCP-Server-Integration-Design.md` C# codebase integration plan.
4. `SpecChat-Versioning-Policy.md` 5 versioning rules (VD-1 through VD-5).
5. `SpecChat-Design-Decisions-Record.md` 22 settled decisions.
6. `SpecChat-BTABOK-Implementation-Plan.md` 6-phase implementation plan.
7. `SpecChat-BTABOK-Acronym-and-Term-Glossary.md` Glossary for acronyms.
8. `BTABOK-Out-of-Scope-Models.md` What is explicitly out of scope (Value Model, People Model, Competency Model).
9. `Global-Corp-Exemplar.md` Narrative description of the fictional Global Corp enterprise architecture (~1,640 lines).

### 2.2 Current spec collection (12 files)

All under `E:\Archive\GitHub\dlandi\spec-chat\WIP\global-corp-specs\`:

1. `global-corp.manifest.md` (313 lines)
2. `global-corp.architecture.spec.md` (963 lines) Enterprise base system spec.
3. `app-cx.customer-experience.spec.md` (1,034 lines)
4. `app-es.enterprise-services.spec.md` (1,203 lines)
5. `app-pc.partner-connectivity.spec.md` (1,080 lines)
6. `app-eb.event-backbone.spec.md` (1,219 lines)
7. `app-tc.traceability-core.spec.md` (1,354 lines)
8. `app-dp.data-platform.spec.md` (1,285 lines)
9. `app-oi.operational-intelligence.spec.md` (1,094 lines)
10. `app-cc.compliance-core.spec.md` (1,044 lines)
11. `app-sd.sustainability-dpp.spec.md` (958 lines)
12. `app-so.security-operations.spec.md` (1,207 lines)

### 2.3 Pattern reference: existing PayGate sample

`E:\Archive\GitHub\dlandi\spec-chat\src\MCPServer\DotNet\Docs\Specs\PayGate.spec.md` The canonical simulator-gate pattern. Every new gate in this refactor follows its shape: Stub / Record / Replay / FaultInject modes, REST surface that mimics a real external, in-memory log for inspection, Docker-hosted.

### 2.4 Pattern reference: blazor-harness sample

`E:\Archive\GitHub\dlandi\spec-chat\Delivery\spec-chat\samples\blazor-harness.spec.md` The canonical Blazor WebAssembly + Razor Library + authored-SVG-chart pattern. Every UI component in the Global Corp refactor follows this shape: a Razor Library named FStar.UI-equivalent containing authored chart primitives and its own CSS, a standalone Blazor WebAssembly app consuming it, a package policy that denies 3rd-party charting and CSS-framework NuGets.

### 2.5 Brief history of the BTABOK work

The collection was authored in a specific sequence. The next session should know that:

- **Dec 2025 through mid-Apr 2026:** BTABOK integration design work produced the 9 active WIP docs.
- **2026-04-17:** Global-Corp-Exemplar.md was written as a narrative fiction describing the enterprise architecture.
- **2026-04-17:** 10 subsystem specs plus architecture spec plus manifest were authored in one session, producing the 12-file collection.
- **2026-04-18 (this document):** Local-first constraints introduced. This brief records the resulting refactoring plan, **before** any spec file is changed.

## 3. Governing Constraints (The Seven Rules)

These are the non-negotiable architectural constraints you established. Every refactored spec must comply with every rule.

1. **Local simulation first, cloud optional later.** The system runs end-to-end on a developer's local machine (Docker Desktop or equivalent) before any cloud deployment. Cloud is a future option, not a requirement.
2. **Cloud-deployable code.** Despite local-first posture, the code is written using abstractions so that cloud deployment is a configuration change, not a rewrite. Aspire's resource model accommodates this directly.
3. **.NET Aspire for all .NET orchestration.** Every .NET project is composed under the Aspire AppHost. Every external container is declared in the AppHost. Connection strings, URLs, health checks, and telemetry flow through Aspire's dependency-injection and resource binding.
4. **Blazor WebAssembly + Razor Libraries for all UI.** All UI is Blazor WebAssembly (standalone mode, not interactive server render). Reusable UI components live in Razor Class Libraries that the WebAssembly apps consume.
5. **Zero 3rd-party JavaScript.** No Chart.js, no D3, no Leaflet, no MermaidJS in the shipped UI. Built-in browser APIs via JSInterop are fine (clipboard, localStorage, geolocation). 3rd-party library JavaScript is not.
6. **SVG + CSS for all diagrams and charts.** Every chart, map, diagram, and visualization is authored as SVG markup plus CSS styling. Pattern reference is the FStar.UI chart library in the blazor-harness sample. The Razor Library authoring these components includes its own CSS with no external 3rd-party dependencies.
7. **All external subsystems in Docker containers locally.** Every external system the platform integrates with has a local Docker container simulator, following the PayGate pattern. The Aspire AppHost pulls these containers for local development.

## 4. Approved Local Simulation Stack

These are the specific technology choices made in answer to Q1 through Q6. Specs in the refactored collection declare consumed components at these exact names and versions.

### 4.1 Infrastructure components (containerized)

| Concern | Technology | Container source | Notes |
|---|---|---|---|
| Event broker (streams) | **Redis Streams** | `redis:7-alpine` (Docker Hub) | Used by APP-EB as the primary event-distribution mechanism. Redis Streams, not Redis Pub/Sub. |
| Event store | **PostgreSQL 17** | `postgres:17-alpine` | Used by APP-EB and APP-TC for persistent state. |
| Relational database (multi-region simulation) | **PostgreSQL 17** | `postgres:17-alpine` x 3 instances (eu-pg, us-pg, apac-pg) | Simulates regional data planes per Q5. |
| Graph store | **PostgreSQL 17 + Apache AGE extension** | Custom image `apache/age` or `postgres:17 + AGE` layered Dockerfile | APP-TC's canonical graph. Apache AGE is the mature PostgreSQL graph extension (openCypher-compatible). |
| Object storage (lakehouse) | **MinIO** | `minio/minio:latest` | S3-compatible; APP-DP uses as the lakehouse file store. |
| Analytics engine | **DuckDB embedded** | NuGet: `DuckDB.NET.Data.Full` | In-process DuckDB via the .NET driver. No container needed. APP-DP's analytics engine. |
| Cache | **Redis** | `redis:7-alpine` (shared with streams) | Application-level cache; same container as event broker or a separate one. |
| MQTT broker | **Eclipse Mosquitto** | `eclipse-mosquitto:2-openssl` | IoT ingestion path. Used by `iot-gate` simulator and APP-PC. |

### 4.2 In-process components (no container)

| Concern | Technology | Notes |
|---|---|---|
| Identity provider | **OpenIddict in-process** | APP-ES.Identity hosts OpenIddict as an ASP.NET Core OIDC server. Authorization Code flow with PKCE for Blazor WebAssembly clients. See Section 9. |
| Observability | **Aspire Dashboard** | Built-in Aspire telemetry. Production-grade observability stack deferred. |
| Secrets (dev) | **Aspire parameter resources** | Aspire's native parameter resource mechanism. Production-grade secret management deferred. |

### 4.3 Container-based external simulators (gates, following PayGate pattern)

Each external system Global Corp integrates with gets its own gate simulator. Each gate:
- Runs as a Docker container
- Exposes a REST surface mimicking the real external
- Supports four behavior modes: Stub, Record, Replay, FaultInject
- Keeps an in-memory log for inspection
- Is declared and hosted by the Aspire AppHost

| Gate simulator | Mimics | Related subsystem |
|---|---|---|
| `PayGate` (exists; reuse) | Stripe | APP-ES.Billing |
| `SendGate` (exists; reuse) | SendGrid / email delivery | APP-CX.NotificationEngine |
| `CarrierGate` (new) | Ocean and road/air carrier APIs (DCSA, EDI X12, EDI EDIFACT, project44-style) | APP-PC |
| `WmsGate` (new) | Warehouse Management Systems, 3PL platforms, EPCIS event sources | APP-PC |
| `IotGate` (new) | IoT device telemetry (MQTT publishers, LoRaWAN gateways) | APP-PC |
| `IdpGate` (new) | External identity providers (corporate OIDC, SAML) for federated login tests | APP-ES |
| `CustomsGate` (new) | Customs and regulator submission systems | APP-CC |
| `DppRegistryGate` (new) | EU Digital Product Passport registry | APP-SD |
| `FsmaGate` (new) | FDA FSMA reporting portal | APP-CC |
| `AuditGate` (new) | Third-party audit and compliance review platforms | APP-CC |

Ten new gate specs to author, two existing to reuse.

## 5. Target .NET and Aspire Versions

Research dated 2026-04-18.

### 5.1 Runtime target

**.NET 10 LTS.** Current long-term-support release. Released alongside C# 14 and Visual Studio 2026. Stable and the right target for this work. Sources: [The Official Aspire Support Policy](https://dotnet.microsoft.com/en-us/platform/support/policy/aspire), [.NET 10 LTS Arrives with C# 14, Visual Studio 2026, Aspire 13, and Copilot](https://windowsforum.com/threads/net-10-lts-arrives-with-c-14-visual-studio-2026-aspire-13-and-copilot.389084/).

### 5.2 Aspire target

**.NET Aspire 13.2.** Released 2026-03-23. Latest available. Notable features relevant to this work:
- AI-agent-native CLI (dev-loop support for detached startup, resource-level control, health-status waiting, isolated environments)
- Preview TypeScript AppHost (not relevant to our pure-C# scenario)
- Dashboard trace/log/resource export and telemetry-bundle import
- New integrations including Certbot and an overhauled AI Foundry integration
- Bun support for JavaScript integrations (not relevant; we have no 3rd-party JS)

Sources: [What's new in Aspire 13](https://aspire.dev/whats-new/aspire-13/), [What's new in Aspire 13.1](https://aspire.dev/whats-new/aspire-13-1/), [.NET Aspire 13.2 Adds AI-Agent CLI, TypeScript AppHost Support](https://visualstudiomagazine.com/articles/2026/03/25/net-aspire-13-2-adds-ai-agent-cli-typescript-apphost-support.aspx), [Announcing Aspire 13.2](https://devblogs.microsoft.com/aspire/aspire-13-2-announcement/).

### 5.3 Versioning note for the specs

You correctly observed that Aspire updates frequently (minor releases throughout the year, roughly one major per year). The specs should surface this:

- Each subsystem spec that depends on Aspire declares a target Aspire version in its tracking metadata or consumed components section.
- The enterprise base spec declares the platform's current Aspire target in the Package Policy section.
- The refactor uses **`13.2.x`** (patch-tolerant within 13.2 minor).
- Per the SpecChat Versioning Policy (Rule 3, warnings-first compatibility), version drift between the declared Aspire target and the actual installed version surfaces warnings rather than blocks.

## 6. Package Policy

Every Global Corp subsystem inherits a common package policy enforced at the enterprise level. The policy follows the blazor-harness pattern.

```spec
package_policy GlobalCorpPolicy {
    source: nuget("https://api.nuget.org/v3/index.json");

    deny category("charting")
        includes ["Plotly.Blazor", "ChartJs.Blazor", "Radzen.Blazor",
                  "ApexCharts", "Syncfusion.*", "Telerik.*",
                  "DevExpress.*", "MudBlazor.Charts"];

    deny category("css-framework")
        includes ["Bootstrap", "Tailwind.*", "MudBlazor",
                  "Radzen.Blazor", "AntDesign"];

    deny category("js-wrapper-libraries")
        includes ["Blazored.Modal", "Blazored.Toast",
                  "Blazored.Typeahead", "Blazor.Leaflet.*"];
    rationale "deny category(js-wrapper-libraries)" {
        context "Even Blazor-prefixed NuGets often wrap 3rd-party JS
                 libraries. Per the zero-3rd-party-JS rule, these are
                 not permitted.";
        decision "Review each NuGet in this category on a case-by-case
                  basis; default posture is deny.";
    }

    allow category("platform")
        includes ["Microsoft.AspNetCore.*", "Microsoft.Extensions.*",
                  "Microsoft.JSInterop", "System.*", "Microsoft.Identity.*"];

    allow category("aspire")
        includes ["Aspire.*", "Aspire.Hosting.*", "Aspire.Microsoft.*",
                  "Aspire.Azure.*", "Aspire.StackExchange.*"];

    allow category("auth")
        includes ["OpenIddict.*", "Microsoft.IdentityModel.*"];

    allow category("storage-drivers")
        includes ["Npgsql*", "StackExchange.Redis",
                  "Minio", "DuckDB.NET.Data.Full",
                  "MQTTnet*"];

    allow category("testing")
        includes ["xunit*", "bunit*", "Microsoft.NET.Test.Sdk",
                  "Moq", "NSubstitute", "coverlet.collector",
                  "Testcontainers*"];

    allow category("observability")
        includes ["OpenTelemetry.*"];

    default: require_rationale;
}
```

Each subsystem spec references this policy by `weakRef<PackagePolicy>(GlobalCorpPolicy)` and may add subsystem-local allowances only with rationale.

## 7. Authentication Pattern (OpenIddict + Blazor WebAssembly Standalone)

Research dated 2026-04-18. Primary reference: [Damien Bowden's AspNetCoreOpeniddict repository](https://github.com/damienbod/AspNetCoreOpeniddict), updated 2026-02-24 for .NET 10 with passkey support.

### 7.1 Flow

**OAuth 2.1 Authorization Code flow with PKCE** is the correct flow for Blazor WebAssembly standalone applications per [Microsoft's Secure ASP.NET Core Blazor WebAssembly guidance](https://learn.microsoft.com/en-us/aspnet/core/blazor/security/webassembly/) and [Secure an ASP.NET Core Blazor WebAssembly standalone app with the Authentication library](https://learn.microsoft.com/en-us/aspnet/core/blazor/security/webassembly/standalone-with-authentication-library). The Blazor WebAssembly Authentication library (Authentication.js) supports this flow via MSAL patterns or direct OIDC integration.

### 7.2 Components

- **`AppEs.Identity`** (in APP-ES): ASP.NET Core host running OpenIddict as the OIDC server. Issues tokens to Blazor WebAssembly clients. Manages client registrations, user accounts, consent records. Runs in-process (no container).
- **Blazor WebAssembly client** (in APP-CX): Uses `Microsoft.AspNetCore.Components.WebAssembly.Authentication` with OIDC configuration pointing at `AppEs.Identity`. Receives access tokens, attaches them to API calls.
- **REST backend APIs** (in each subsystem): Validate bearer tokens against OpenIddict-issued JWTs. Use `AddJwtBearer` with configuration pointing at `AppEs.Identity`.

### 7.3 Token scope design

- **Scopes mirror subsystem API surfaces.** Examples: `gc.shipments.read`, `gc.shipments.write`, `gc.compliance.export`, `gc.tenant.admin`.
- **Tenant claim** is issued as a custom claim `gc_tenant` resolved at authentication time from the user's organization.
- **Role claim** `gc_role` encodes BTABOK-stakeholder role mapping (for example, `supply-chain-director`, `regulatory-affairs`, `partner-integrator`).

### 7.4 Passkey support

OpenIddict gained passkey support in its 2026 releases per the damienbod repository. The refactored `app-es.enterprise-services.spec.md` includes passkey authentication as a supported option for customer portals.

### 7.5 Federated login (optional, local simulation)

Customer organizations may federate their own IdP. For local testing, the `IdpGate` simulator (see Section 4.3) provides a mock external OIDC issuer. APP-ES is configured to accept tokens from `IdpGate` in dev profile.

## 8. UI Architecture Pattern

Follows the blazor-harness sample with supply-chain-specific elaborations.

### 8.1 Project layout (per UI-bearing subsystem)

For APP-CX specifically (the primary UI-bearing subsystem):

```
AppCx/
├── AppCx.Portal/              (Blazor WebAssembly standalone host)
│   ├── Program.cs
│   ├── App.razor
│   ├── wwwroot/
│   │   ├── index.html
│   │   └── css/
│   │       └── site.css       (authored, no 3rd-party imports)
├── AppCx.UI/                  (Razor Class Library; shared UI components)
│   ├── AppCx.UI.csproj
│   ├── Components/
│   │   ├── Layout/
│   │   └── Forms/
│   └── wwwroot/
│       └── css/
│           └── AppCx.UI.css   (authored, bundled with the library)
├── AppCx.UI.Charts/           (Razor Class Library; chart primitives)
│   ├── LineChart.razor
│   ├── TimelineChart.razor
│   ├── GeoMap.razor
│   └── ...
├── AppCx.UI.Dashboards/       (Razor Class Library; composed dashboards)
├── AppCx.Api/                 (ASP.NET Core REST backend)
└── AppCx.Tests/               (xUnit + bunit)
```

### 8.2 Razor Libraries carry their own CSS

Per Q6, specific named components reside in a Razor Library that contains its own CSS with no external 3rd-party dependencies. Each Razor Library bundles its own `wwwroot/css/<LibraryName>.css`. The consuming Blazor WebAssembly host references the library; the library's CSS is included automatically via Blazor component CSS isolation or explicit `<link>` entries in `index.html`.

No Bootstrap, Tailwind, MudBlazor, Radzen, or similar CSS frameworks.

### 8.3 Cross-subsystem UI sharing

`AppCx.UI` is the canonical shared UI library, but other subsystems that expose small UI surfaces (for example, APP-SO observability dashboards for operators) reference it. They do not duplicate chart primitives.

## 9. Chart Primitive Catalog

Per Q6, these are specific named components residing in `AppCx.UI.Charts` (and smaller libraries where useful). Each is authored as a Razor component with an SVG body and CSS styling. None use 3rd-party JavaScript.

### 9.1 Core chart primitives (~15 components)

| Component | Purpose | Typical consumer dashboard |
|---|---|---|
| `LineChart` | Time-series lines (exception counts, ETA drift, throughput) | Control Tower trends |
| `BarChart` | Category comparisons (volume by lane, events by partner) | Partner dashboard |
| `StackedBarChart` | Categorical composition over time (exception categories) | Exception trends |
| `HeatMap` | Matrix visualization (lane x time capacity, risk register) | Capacity map, risk view |
| `TimelineChart` | Horizontal timeline with event markers (shipment lifecycle) | Shipment detail |
| `GanttChart` | Scheduled bars with dependencies (roadmap, transitions) | Roadmap page (future) |
| `NetworkDiagram` | Nodes and edges (traceability graph) | Provenance view |
| `TreeMap` | Hierarchical area (DPP material composition) | DPP detail |
| `Sankey` | Flow visualization (carbon footprint, material provenance) | Sustainability view |
| `GeoMap` | SVG world or regional map | Shipment tracking |
| `RouteOverlay` | Route lines on a geo map | Shipment tracking |
| `QuadrantChart` | Two-axis grid with quadrants (power-interest, risk matrix) | Stakeholder view, risk view |
| `RadialGauge` | Single-value radial indicator (compliance coverage) | Compliance dashboard |
| `DonutChart` | Categorical proportions in a ring (status mix) | Status overview |
| `WaterfallChart` | Sequential positive/negative contributions | KPI decomposition |

### 9.2 Component contract shape

Each component follows a uniform contract pattern drawn from FStar.UI:

- **Props:** data series or data model, optional title, optional axis labels, optional theme selector.
- **Events:** `OnDataPointClick`, `OnHover`, optional `OnZoom` where interactive.
- **Accessibility:** semantic SVG with `role` and `aria-label` attributes; keyboard navigation via Tab + Enter; text alternatives for screen readers.
- **Theming:** color scheme driven by CSS variables; dark mode supported through CSS media queries; no runtime JavaScript theme switching beyond attribute toggling.
- **Size:** responsive via `viewBox` and `preserveAspectRatio`; consumer controls the container size via CSS.

### 9.3 Map specifics

`GeoMap` needs a world SVG basemap. Candidate sources (all licensed for inclusion):

- Natural Earth data converted to simplified SVG paths
- D3's topojson world dataset, pre-rendered to SVG paths at build time (not consumed as a 3rd-party JS dependency; the paths become static assets)

The preferred approach is to bake the map SVG into the library as an `.svg` asset file, then render overlays programmatically. No runtime 3rd-party mapping library.

## 10. External Gate Simulator Pattern

Every gate follows the PayGate template. Pattern summary (already detailed in `PayGate.spec.md`):

### 10.1 Gate surface

- **Server project** (`GateName.Server`): ASP.NET Core minimal API, REST endpoints mimicking the real external's surface.
- **Client library** (`GateName.Client`): .NET client that wraps the server's endpoints; consumer code uses this client instead of the real external's SDK.
- **Four modes:** configured at runtime, switchable without restart.
  - **Stub:** returns preconfigured canned responses.
  - **Record:** proxies to the real external and records request/response pairs.
  - **Replay:** returns previously recorded responses for matching requests.
  - **FaultInject:** returns configurable failures (4xx, 5xx, timeouts, malformed bodies).
- **Management API:** endpoints for configuring the mode, inspecting the log, loading recorded fixtures.
- **Docker image:** packaged for Aspire AppHost consumption.

### 10.2 Gate inventory

Ten gate specs to author:

1. **`carrier-gate.spec.md`** Mimics ocean, road, and air carrier APIs. Supports DCSA Track-and-Trace, EDI 214, EDI X12, project44-style visibility. Has endpoints for milestone queries, shipment status, route tracking.
2. **`wms-gate.spec.md`** Mimics WMS, TMS, and 3PL platforms. Supports EPCIS event capture/query, REST webhooks, AS2/SFTP file exchange.
3. **`iot-gate.spec.md`** Includes Mosquitto MQTT broker plus a telemetry simulator producing temperature, shock, humidity, GPS streams. Scriptable scenarios (normal, cold-chain excursion, tamper event).
4. **`idp-gate.spec.md`** Mimics external OIDC providers (Okta, Azure AD). Returns test user tokens for federated login testing.
5. **`customs-gate.spec.md`** Mimics customs declaration and clearance systems.
6. **`dpp-registry-gate.spec.md`** Mimics the EU Digital Product Passport registry. Accepts DPP submissions, returns registry URIs.
7. **`fsma-gate.spec.md`** Mimics the FDA FSMA reporting portal. Accepts CTE/KDE submissions.
8. **`audit-gate.spec.md`** Mimics third-party audit platforms. Accepts signed evidence bundles.
9. **`PayGate.spec.md`** (reuse existing from `src/MCPServer/DotNet/Docs/Specs/`). Mimics Stripe.
10. **`SendGate.spec.md`** (reuse existing). Mimics SendGrid / email delivery.

Each is a separate .NET solution with its own server and client projects, packaged as a Docker image, registered with Aspire.

## 11. Multi-Region Simulation

Per Q5, simulate three regional data planes using three PostgreSQL containers.

### 11.1 Containers

- `pg-eu` PostgreSQL container acting as the EU region's data plane.
- `pg-us` PostgreSQL container acting as the US region's data plane.
- `pg-apac` PostgreSQL container acting as the APAC region's data plane.

Each container is pre-provisioned with the same schema at startup via a shared migration project. Tenant data written to a region's container stays in that container (INV-06 PII regional control).

### 11.2 Routing logic

- `AppEs.TenantManagement` owns the tenant-to-region mapping (`gc_tenant -> region`).
- When a request arrives, APP-ES resolves the tenant's region.
- Downstream subsystems (APP-EB, APP-TC, APP-CC) route reads and writes to the matching regional PostgreSQL container.
- Each subsystem maintains a connection-factory abstraction that selects the right connection based on the request's tenant context.

### 11.3 Cross-region metadata replication

The only permitted cross-region data flow is metadata replication for the EU DPP index (WVR-02 in the exemplar narrative). Implementation:

- A background replication worker in APP-DP reads metadata rows from `pg-eu` and `pg-us` and writes scrubbed metadata (no PII) to a shared index database (a fourth small PostgreSQL container or a table in the EU container tagged as "cross-region safe").
- The waiver record (WVR-02) must be authored in Phase 2c as a `WaiverRecord` spec before this replication path passes validators.

### 11.4 Aspire AppHost composition for multi-region

```csharp
// Pseudocode sketch
var pgEu = builder.AddPostgres("pg-eu").WithVolume("pg-eu-data");
var pgUs = builder.AddPostgres("pg-us").WithVolume("pg-us-data");
var pgApac = builder.AddPostgres("pg-apac").WithVolume("pg-apac-data");

var appEs = builder.AddProject<Projects.AppEs_Api>("app-es")
    .WithReference(pgEu)  // tenant directory is in EU (primary)
    .WithReference(pgUs).WithReference(pgApac);  // enables routing

var appTc = builder.AddProject<Projects.AppTc_Api>("app-tc")
    .WithReference(pgEu).WithReference(pgUs).WithReference(pgApac)
    .WithReference(appEs);  // for tenant resolution
// ... similar for other region-aware subsystems
```

## 12. Deployment Profile Split in the Architecture Spec

The enterprise architecture spec's Deployment section needs to split into two profiles.

### 12.1 Local Simulation Profile (primary, dev)

Everything runs on one developer machine via Aspire:

- 3 PostgreSQL containers (regional simulation)
- 1 Redis container (streams + cache)
- 1 MinIO container (object storage)
- 1 Mosquitto container (MQTT)
- 10 gate simulator containers (carrier, wms, iot, idp, customs, dpp-registry, fsma, audit, paygate, sendgate)
- N .NET projects (one per subsystem, totaling roughly 30 to 50 projects when components are broken out)
- Aspire Dashboard for observability
- OpenIddict running in-process as part of `AppEs.Identity`

Developer experience: `dotnet run` against the AppHost project launches everything. The Aspire Dashboard opens in a browser showing all services, logs, traces, metrics.

### 12.2 Cloud Production Profile (deferred)

The original multi-region cloud architecture from the exemplar. Deferred. Documented in specs as the intended long-term target but not implemented.

Aspire AppHost targets cloud providers via the `Aspire.Azure.*`, `Aspire.AWS.*` integrations when/if cloud deployment becomes active. Code does not change; composition does.

## 13. New Specs to Author

Beyond refactoring existing specs, this list of new specs is needed.

### 13.1 Cross-cutting platform specs (2)

| Spec | Purpose |
|---|---|
| `aspire-apphost.spec.md` | Aspire AppHost composition: which projects, which containers, which connection strings, which health checks |
| `service-defaults.spec.md` | Shared telemetry, resilience policies, service discovery, health check conventions, OpenTelemetry setup |

### 13.2 Gate simulator specs (10; 2 reused)

Per Section 10.2.

### 13.3 Shared UI library spec (1, possibly more)

| Spec | Purpose |
|---|---|
| `app-cx-ui-charts.spec.md` | Razor Class Library declaring the 15 chart primitive components and their contracts. May be a sub-spec under APP-CX rather than its own top-level spec. |

### 13.4 Enterprise package policy spec (1)

| Spec | Purpose |
|---|---|
| `global-corp-package-policy.spec.md` | The enterprise-level package policy per Section 6. Referenced by every subsystem spec. Alternative: include as a section within `global-corp.architecture.spec.md` rather than its own spec. |

Total new specs: 13 if package policy becomes its own spec, 12 if embedded in the architecture spec. Preference: embed in architecture spec to reduce proliferation.

## 14. Existing Specs to Refactor

The 12 current specs need varying amounts of rework.

### 14.1 Manifest

`global-corp.manifest.md` needs updates:
- Add inventory entries for the new specs (2 platform + 8 gates + any new UI library specs)
- Revise execution order tiers to reflect gate dependencies
- Add Local Simulation Profile metadata

### 14.2 Architecture spec

`global-corp.architecture.spec.md` needs substantial expansion:
- New **Section: Aspire Composition** describing the AppHost-level orchestration
- New **Section: Package Policy** (per Section 6 of this brief)
- **Section 8 Deployment** split into Local Simulation Profile and Cloud Production Profile per Section 12 of this brief
- New **Section: Gate Inventory** listing all 10 gate simulators
- **Section 3 System Declaration** adds `AppHost` and `ServiceDefaults` as subsystems and `PayGate`, `SendGate`, and the 8 new gates as subsystems
- **Section 11 Dynamics** updates to reflect gate-mediated flows (for example, DYN-01 partner event flow now goes through `CarrierGate` in dev profile)

### 14.3 Subsystem specs

Per-spec refactoring scope:

| Subsystem | Scope | Primary changes |
|---|---|---|
| **APP-CX** | Large | Rework System Declaration: Blazor WASM host + Razor Libraries + SVG charts. Add `AppCx.UI`, `AppCx.UI.Charts`, `AppCx.UI.Dashboards` as authored components. Rework authentication section: OpenIddict integration via Microsoft.AspNetCore.Components.WebAssembly.Authentication. Rework notification engine to reference `SendGate` dependency. |
| **APP-ES** | Moderate | Identity becomes OpenIddict in-process (Section 9 pattern). Billing becomes `PayGate` reference. Federated login uses `IdpGate`. Tenant management remains core. Multi-region tenant-to-region mapping added. |
| **APP-PC** | Large | Add dependencies on `CarrierGate`, `WmsGate`, `IotGate`. Partner adapters become Gate clients for dev profile, real partner clients for prod profile (configuration-driven). |
| **APP-EB** | Moderate | Replace pub/sub abstraction with Redis Streams. PostgreSQL containers for event store. Multi-region event store with regional routing. |
| **APP-TC** | Moderate | Graph store becomes PostgreSQL + Apache AGE. Add AGE schema definitions. Keep component structure. |
| **APP-DP** | Large | Lakehouse becomes MinIO for object storage + DuckDB.NET for analytics. Remove cloud-warehouse assumptions. Revise data pipelines to use local containers. |
| **APP-OI** | Small | Rule engine stays in-process. ETA model uses ML.NET or heuristic. Alerting via SignalR (built-in, not 3rd-party). |
| **APP-CC** | Moderate | KMS becomes Aspire parameter resources (dev). Add `CustomsGate`, `FsmaGate`, `AuditGate` dependencies. Signing service uses local key material from Aspire. |
| **APP-SD** | Moderate | Add `DppRegistryGate` dependency. Publisher targets the gate in dev. |
| **APP-SO** | Moderate | Observability becomes Aspire Dashboard. Production SIEM deferred. Vault becomes Aspire parameter resources. |

## 15. Refactoring Sequence

Recommended order of work in the next session.

1. **Phase A: Update the architecture spec first.** Add Aspire composition, package policy, deployment profile split, gate inventory. This is the foundation everything else consumes.
2. **Phase B: Author the 2 cross-cutting platform specs** (`aspire-apphost.spec.md`, `service-defaults.spec.md`).
3. **Phase C: Author the 8 new gate specs.** Follow PayGate shape.
4. **Phase D: Refactor subsystem specs in dependency order.** Foundation tier first (APP-ES, APP-SO), then ingestion (APP-PC, APP-EB), then data (APP-TC, APP-DP), then consumption (APP-OI, APP-CX, APP-CC), then regulatory (APP-SD).
5. **Phase E: Update the manifest** to reflect the new inventory and tier structure.
6. **Phase F: Verify cross-references.** Every `weakRef` in the collection should resolve correctly; every topology rule should trace to valid components.

Only after Phase F is the collection ready for Phase 2c (BTABOK concept specs) per the sequencing lesson we settled earlier.

## 16. Open Items Still Needing Decisions

These are small but real. They do not block refactoring but the refactored specs will have to pick a direction.

### 16.1 Apache AGE vs alternatives for graph storage

PostgreSQL with Apache AGE is the chosen path per Q1. Open questions:
- Exact image source: `apache/age:latest`, a pinned version, or a custom Dockerfile extending `postgres:17-alpine` with AGE layered in?
- openCypher compatibility level expected?
- Migration strategy for when the underlying PostgreSQL version bumps (AGE tracks PG versions with some lag)?

### 16.2 Redis Streams topology

Redis Streams can run as a single node, a replicated cluster, or Redis Sentinel. For local simulation:
- Single Redis container is simplest.
- Multi-container Redis cluster is closer to production shape but heavier on dev machines.

Recommended: single Redis container in dev; cluster-shape deferred.

### 16.3 MinIO bucket policy

MinIO in dev: which buckets exist, who writes to them, how retention is simulated?

Recommended initial buckets:
- `gc-events-eu`, `gc-events-us`, `gc-events-apac` (raw event archive per region)
- `gc-compliance-bundles` (signed evidence storage)
- `gc-dpp-assets` (DPP binary attachments)
- `gc-analytics-marts` (columnar files for DuckDB reads)

### 16.4 OpenIddict client registration strategy

- **Option X:** OpenIddict server issues client registrations statically at startup (one per known Blazor WebAssembly app).
- **Option Y:** Dynamic client registration supported at runtime (partner-portal tenants can self-register).

Recommended: static for dev, dynamic deferred.

### 16.5 Passkey support in scope for v0.1?

OpenIddict has passkey support. Customer portals (APP-CX) could support passkey login as an option. Adds complexity. Recommended: stretch goal, not v0.1 required.

### 16.6 Aspire dashboard retention and export

Aspire 13.2 added trace/log/resource export. For local simulation, is the exported telemetry kept? Recommended: ephemeral by default; explicit export when a developer needs to share a trace bundle.

### 16.7 Testing container strategy

Tests can use Aspire's own test fixtures or `Testcontainers-dotnet`. Recommended: Testcontainers for unit/integration tests that need Postgres/Redis; Aspire's `Aspire.Hosting.Testing` for end-to-end AppHost tests.

## 17. Dependencies on Existing Samples

The existing SpecChat samples provide reusable code and patterns:

- **`PayGate.spec.md`** and its implementation at `src/MCPServer/DotNet/Docs/Specs/PayGate.spec.md`: reuse verbatim, adapt configuration for Global Corp integration.
- **`SendGate.spec.md`** and its implementation: reuse verbatim.
- **`blazor-harness.spec.md`** at `Delivery/spec-chat/samples/`: use as the structural template for `AppCx.UI.Charts`. The chart primitives, CSS discipline, and package policy are directly applicable.

## 18. What This Brief Does Not Settle

- **Phase 2c Strategy specs** (stakeholders, ASRs, decisions, principles, viewpoints, governance, waivers, roadmap, views, standards, scorecard) are still deferred to after the refactoring completes. The BTABOK-orthodox sequencing lesson (Strategy before Transformation) still applies; this refactoring effectively is still Transformation-tier work, but grounded in the constraints.
- **Actual spec file modifications.** This brief guides the modifications; it does not perform them.
- **Production cloud deployment.** Deferred per your constraint.
- **Specific CI/CD pipeline.** Deferred; Aspire supports multi-environment deploys, but the pipeline itself is a separate work stream.

## 19. Success Criteria for the Refactoring Session

The next session succeeds when:

1. The enterprise architecture spec reflects the seven governing constraints and the local simulation stack.
2. Two cross-cutting platform specs (AppHost, ServiceDefaults) are authored.
3. Eight new gate specs are authored (carrier, wms, iot, idp, customs, dpp-registry, fsma, audit).
4. Ten subsystem specs are refactored to reference the gates, use the approved stack, and align with the UI pattern.
5. The manifest inventory reflects the new spec files.
6. No cross-reference is broken within the collection (beyond the still-unauthored Phase 2c weakRefs, which are expected).
7. No 3rd-party charting or CSS-framework NuGet appears in any subsystem's consumed components list.
8. Every external integration point routes through a gate simulator in the dev profile and preserves a cloud-deploy path as configuration.

## 20. Source References

- **.NET Aspire 13.2 release** (2026-03-23). [Announcing Aspire 13.2 (Microsoft DevBlogs)](https://devblogs.microsoft.com/aspire/aspire-13-2-announcement/), [What's new in Aspire 13.1](https://aspire.dev/whats-new/aspire-13-1/), [What's new in Aspire 13](https://aspire.dev/whats-new/aspire-13/), [.NET Aspire 13.2 adds AI-agent CLI, TypeScript AppHost support (Visual Studio Magazine)](https://visualstudiomagazine.com/articles/2026/03/25/net-aspire-13-2-adds-ai-agent-cli-typescript-apphost-support.aspx).
- **.NET 10 LTS**. [.NET 10 LTS arrives with C# 14, Visual Studio 2026, Aspire 13, Copilot (Windows Forum)](https://windowsforum.com/threads/net-10-lts-arrives-with-c-14-visual-studio-2026-aspire-13-and-copilot.389084/).
- **Aspire support policy**. [The official Aspire support policy (Microsoft)](https://dotnet.microsoft.com/en-us/platform/support/policy/aspire).
- **Aspire releases**. [microsoft/aspire releases on GitHub](https://github.com/microsoft/aspire/releases).
- **OpenIddict with Blazor WASM + PKCE**. [damienbod AspNetCoreOpeniddict (repo, updated 2026-02-24 for .NET 10 + passkeys)](https://github.com/damienbod/AspNetCoreOpeniddict).
- **Microsoft Blazor WebAssembly security guidance**. [Secure ASP.NET Core Blazor WebAssembly (Microsoft Learn)](https://learn.microsoft.com/en-us/aspnet/core/blazor/security/webassembly/), [Secure an ASP.NET Core Blazor WebAssembly standalone app with the Authentication library (Microsoft Learn)](https://learn.microsoft.com/en-us/aspnet/core/blazor/security/webassembly/standalone-with-authentication-library?view=aspnetcore-9.0).
- **OpenIddict repository issue** discussing Blazor WASM auth patterns. [Best way to implement Auth for Standalone Wasm + Asp Web Api (openiddict/openiddict-core issue 1918)](https://github.com/openiddict/openiddict-core/issues/1918).
- **BlazorAppWasmAuth example** (standalone with OIDC). [wildermedeiros/BlazorAppWasmAuth (GitHub)](https://github.com/wildermedeiros/BlazorAppWasmAuth).
- **ITfoxtec.Identity.BlazorWebAssembly.OpenidConnect** (alternative library, for reference). [ITfoxtec/ITfoxtec.Identity.BlazorWebAssembly.OpenidConnect (GitHub)](https://github.com/ITfoxtec/ITfoxtec.Identity.BlazorWebAssembly.OpenidConnect).
- **Blazor harness sample** (reference for SVG charts and no-3rd-party-JS pattern). `E:\Archive\GitHub\dlandi\spec-chat\Delivery\spec-chat\samples\blazor-harness.spec.md`.
- **PayGate sample** (reference for simulator-gate pattern). `E:\Archive\GitHub\dlandi\spec-chat\src\MCPServer\DotNet\Docs\Specs\PayGate.spec.md`.
- **Current Global Corp collection**. `E:\Archive\GitHub\dlandi\spec-chat\WIP\global-corp-specs\` (12 files).
- **Global Corp narrative exemplar**. `E:\Archive\GitHub\dlandi\spec-chat\WIP\Global-Corp-Exemplar.md`.
- **SpecChat design corpus**. The 9 active docs in `E:\Archive\GitHub\dlandi\spec-chat\WIP\` per Section 2.1.

## Appendix A: Aspire AppHost Composition Pseudocode

Skeleton of the AppHost that will eventually be authored. Not C# verbatim; this is illustrative for spec content.

```csharp
var builder = DistributedApplication.CreateBuilder(args);

// Regional PostgreSQL data planes
var pgEu   = builder.AddPostgres("pg-eu").WithVolume("pg-eu-data").WithImage("postgres:17-alpine");
var pgUs   = builder.AddPostgres("pg-us").WithVolume("pg-us-data").WithImage("postgres:17-alpine");
var pgApac = builder.AddPostgres("pg-apac").WithVolume("pg-apac-data").WithImage("postgres:17-alpine");

// Graph-extension-enabled PostgreSQL for APP-TC
var pgGraph = builder.AddPostgres("pg-graph")
    .WithImage("apache/age")  // PostgreSQL with AGE extension
    .WithVolume("pg-graph-data");

// Shared infrastructure
var redis  = builder.AddRedis("redis");
var minio  = builder.AddContainer("minio", "minio/minio:latest");
var mqtt   = builder.AddContainer("mosquitto", "eclipse-mosquitto:2-openssl");

// External simulator gates
var paygate          = builder.AddContainer("paygate", "globalcorp/paygate:latest");
var sendgate         = builder.AddContainer("sendgate", "globalcorp/sendgate:latest");
var carrierGate      = builder.AddContainer("carrier-gate", "globalcorp/carrier-gate:latest");
var wmsGate          = builder.AddContainer("wms-gate", "globalcorp/wms-gate:latest");
var iotGate          = builder.AddContainer("iot-gate", "globalcorp/iot-gate:latest").WithReference(mqtt);
var idpGate          = builder.AddContainer("idp-gate", "globalcorp/idp-gate:latest");
var customsGate      = builder.AddContainer("customs-gate", "globalcorp/customs-gate:latest");
var dppRegistryGate  = builder.AddContainer("dpp-registry-gate", "globalcorp/dpp-registry-gate:latest");
var fsmaGate         = builder.AddContainer("fsma-gate", "globalcorp/fsma-gate:latest");
var auditGate        = builder.AddContainer("audit-gate", "globalcorp/audit-gate:latest");

// Foundation subsystems (Tier 0)
var appEs = builder.AddProject<Projects.AppEs_Api>("app-es")
    .WithReference(pgEu).WithReference(pgUs).WithReference(pgApac)
    .WithReference(paygate).WithReference(idpGate)
    .WithServiceDefaults();

var appSo = builder.AddProject<Projects.AppSo_Api>("app-so")
    .WithServiceDefaults();

// Ingestion (Tier 1)
var appPc = builder.AddProject<Projects.AppPc_Api>("app-pc")
    .WithReference(appEs)
    .WithReference(carrierGate).WithReference(wmsGate).WithReference(iotGate)
    .WithReference(redis);

var appEb = builder.AddProject<Projects.AppEb_Api>("app-eb")
    .WithReference(redis)
    .WithReference(pgEu).WithReference(pgUs).WithReference(pgApac)
    .WithReference(appEs);

// Data (Tier 2)
var appTc = builder.AddProject<Projects.AppTc_Api>("app-tc")
    .WithReference(pgGraph).WithReference(appEb).WithReference(appEs);

var appDp = builder.AddProject<Projects.AppDp_Api>("app-dp")
    .WithReference(minio).WithReference(appEb).WithReference(appTc);
// DuckDB is embedded in-process via NuGet; no reference needed.

// Consumption (Tier 3)
var appOi = builder.AddProject<Projects.AppOi_Api>("app-oi")
    .WithReference(appEb).WithReference(appTc).WithReference(appDp).WithReference(appEs);

var appCxApi = builder.AddProject<Projects.AppCx_Api>("app-cx-api")
    .WithReference(appOi).WithReference(appTc).WithReference(appEs)
    .WithReference(sendgate);

var appCxPortal = builder.AddProject<Projects.AppCx_Portal>("app-cx-portal")
    .WithReference(appCxApi).WithReference(appEs);

var appCc = builder.AddProject<Projects.AppCc_Api>("app-cc")
    .WithReference(appTc).WithReference(appEb).WithReference(appSo).WithReference(appEs)
    .WithReference(customsGate).WithReference(fsmaGate).WithReference(auditGate);

// Regulatory (Tier 4)
var appSd = builder.AddProject<Projects.AppSd_Api>("app-sd")
    .WithReference(appTc).WithReference(appCc).WithReference(appEb)
    .WithReference(dppRegistryGate);

builder.Build().Run();
```

## Appendix B: OpenIddict + Blazor WebAssembly Standalone Setup Pattern

Sketch of the auth flow to be realized in the refactored APP-ES and APP-CX specs.

### B.1 APP-ES Identity host (server)

```csharp
// In AppEs.Api / AppEs.Identity startup
services.AddDbContext<AppEsDbContext>(options =>
    options.UseNpgsql(config.GetConnectionString("pg-eu"))
           .UseOpenIddict());

services.AddOpenIddict()
    .AddCore(o => o.UseEntityFrameworkCore().UseDbContext<AppEsDbContext>())
    .AddServer(o =>
    {
        o.SetAuthorizationEndpointUris("connect/authorize");
        o.SetTokenEndpointUris("connect/token");
        o.SetUserInfoEndpointUris("connect/userinfo");
        o.AllowAuthorizationCodeFlow().RequireProofKeyForCodeExchange();
        o.RegisterScopes(
            "openid", "profile", "email",
            "gc.shipments.read", "gc.shipments.write",
            "gc.compliance.export", "gc.tenant.admin",
            "gc.partner.onboard");
        // Passkey support (optional, per latest OpenIddict)
        o.EnablePasskeys();
        o.UseAspNetCore().EnableAuthorizationEndpointPassthrough();
    })
    .AddValidation(o =>
    {
        o.UseLocalServer();
        o.UseAspNetCore();
    });

// Static client registration at startup (per Open Item 16.4)
using (var scope = app.Services.CreateScope())
{
    var manager = scope.ServiceProvider.GetRequiredService<IOpenIddictApplicationManager>();
    await manager.CreateAsync(new OpenIddictApplicationDescriptor
    {
        ClientId = "app-cx-portal",
        ConsentType = ConsentTypes.Implicit,
        ClientType = ClientTypes.Public,
        RedirectUris = { new Uri("https://localhost:5001/authentication/login-callback") },
        PostLogoutRedirectUris = { new Uri("https://localhost:5001/authentication/logout-callback") },
        Permissions =
        {
            Permissions.Endpoints.Authorization,
            Permissions.Endpoints.Token,
            Permissions.GrantTypes.AuthorizationCode,
            Permissions.ResponseTypes.Code,
            Permissions.Scopes.Profile, Permissions.Scopes.Email,
            Permissions.Prefixes.Scope + "gc.shipments.read",
            // ... additional scopes per app-cx needs
        },
        Requirements =
        {
            Requirements.Features.ProofKeyForCodeExchange
        }
    });
}
```

### B.2 APP-CX Portal (Blazor WebAssembly standalone client)

```csharp
// In AppCx.Portal / Program.cs
builder.Services.AddOidcAuthentication(options =>
{
    options.ProviderOptions.Authority = "https://localhost:5002";  // AppEs.Identity
    options.ProviderOptions.ClientId = "app-cx-portal";
    options.ProviderOptions.ResponseType = "code";
    options.ProviderOptions.DefaultScopes.Add("openid");
    options.ProviderOptions.DefaultScopes.Add("profile");
    options.ProviderOptions.DefaultScopes.Add("gc.shipments.read");
    options.ProviderOptions.DefaultScopes.Add("gc.shipments.write");
});

// Attach access tokens to REST calls
builder.Services.AddScoped<AuthorizationMessageHandler>();
builder.Services.AddHttpClient("AppCxApi", client =>
    client.BaseAddress = new Uri(config["AppCxApi:BaseUrl"]))
    .AddHttpMessageHandler<AuthorizationMessageHandler>();
```

### B.3 Subsystem API (bearer token validation)

```csharp
// In each subsystem's Program.cs
services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.Authority = config["AppEs:Authority"];
        options.Audience = "gc.api";
        options.TokenValidationParameters.ValidateIssuer = true;
    });

services.AddAuthorization(options =>
{
    options.AddPolicy("ShipmentsRead", p => p.RequireClaim("scope", "gc.shipments.read"));
    options.AddPolicy("ComplianceExport", p => p.RequireClaim("scope", "gc.compliance.export"));
    // ... other policies
});
```

This pattern is documented in the refactored APP-ES and APP-CX specs, and every consuming subsystem references it.

---

End of brief. The next working session should begin with re-reading Section 3 (Governing Constraints), Section 4 (Approved Stack), and Section 15 (Refactoring Sequence), then proceed to Phase A of the refactoring sequence.
