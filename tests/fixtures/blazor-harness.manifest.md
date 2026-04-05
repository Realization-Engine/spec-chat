# FStarVisualHarness -- System Manifest

## Tracking

| Field | Value |
|---|---|
| Created | 2026-04-03 |
| State | Draft |
| Reviewed | |
| Approved | |
| Executed | |
| Verified | |
| Dependencies | None (root document) |

## System

| Field | Value |
|---|---|
| System | FStarVisualHarness |
| Base spec | blazor-harness.spec.md |
| Target | net10.0 |
| Spec count | 13 |

This manifest governs the specification collection for the FStarVisualHarness system. The base spec (blazor-harness.spec.md) defines the system skeleton: components, topology, phases, traces, constraints, data entities, and page declarations. All other specs are incremental additions that reference the base.

## Lifecycle States

Every spec document moves through a defined sequence of states. Each transition is recorded in the Tracking block with a date.

| State | Meaning | Tracked by |
|---|---|---|
| Draft | Written, not yet reviewed for correctness | Created date |
| Reviewed | Passed consistency check and grammar audit | Reviewed date |
| Approved | Ready to execute; decisions resolved, dependencies satisfied | Approved date |
| Executed | Amendments applied to base spec, or code implemented | Executed date |
| Verified | Post-execution confirmation passed | Verified date |

**Rules:**
- States are sequential: Draft, Reviewed, Approved, Executed, Verified.
- A spec cannot skip states.
- Decision specs enter Approved when the recommendation is accepted.
- Feature and bug specs enter Executed when their code implementation is complete.
- Amendment specs enter Executed when their corrections are applied to the base spec.
- The base spec enters Executed when it has been used to generate or validate source code.
- Verified requires an independent check (test pass, audit, or review) confirming the execution is correct.

## Tracking Block Convention

Every spec document must contain a Tracking block immediately after the title and header metadata, before the first content section.

Format:

| Field | Value |
|---|---|
| Created | YYYY-MM-DD (file creation date) |
| State | Current lifecycle state |
| Reviewed | YYYY-MM-DD or blank |
| Approved | YYYY-MM-DD or blank |
| Executed | YYYY-MM-DD or blank |
| Verified | YYYY-MM-DD or blank |
| Dependencies | Spec filenames with parenthetical notes, or "None" |

**Rules:**
- Created uses the file's creation date.
- State reflects the current lifecycle state.
- Date fields are set when the corresponding state transition occurs.
- Dependencies lists other spec filenames that must reach Executed state before this spec can be executed.
- "None" if the spec has no prerequisites beyond the base spec.

## Document Type Registry

| Type | Purpose | Template |
|---|---|---|
| Base system spec | System skeleton: components, topology, phases, traces, constraints, data, pages | blazor-harness.spec.md |
| Feature spec | New capability: component additions, entities, constraints, tests, example | feature-theory-tabs.spec.md |
| Bug spec | Source gap the spec identifies: current vs. specified behavior, acceptance criteria | bug-timeline-crosshair-sync.spec.md |
| Decision spec | Conflict between spec and source: options, trade-offs, recommendation, resolution | decision-folder-structure.spec.md |
| Amendment | Base spec correction without new capability: count fixes, dependency notes | amendment-test-counts.spec.md |
| Manifest | Root document binding a spec collection: inventory, lifecycle, conventions, execution order | blazor-harness.manifest.md |

## Conventions

### Writing Style
- No em-dashes. Use commas, semicolons, colons, or separate sentences.
- No emoticons.
- No purple prose or marketing-type phrases.
- Precise file paths and source references.

### SpecLang Syntax
- Formal declarations (authored component, entity, trace, contract, constraint, rationale) follow the grammar defined in SpecLang-Grammar.md.
- `requires` and `ensures` take expressions; `guarantees` takes prose strings.
- `invariant` requires a name string, colon, and expression.
- `rationale` uses either simple form (`rationale STRING;`) or structured form (`rationale { context ...; decision ...; consequence ...; }`).

### Spec Document Structure
- Title line with document type suffix (e.g., "-- Feature Specification").
- Header metadata (System, Feature/Bug/Decision, Related specs).
- Tracking block (mandatory).
- Content sections specific to the document type.

## Spec Inventory

| Filename | Type | State | Tier | Dependencies |
|---|---|---|---|---|
| blazor-harness.spec.md | Base | Approved | -- | None |
| decision-timeseries-animator.spec.md | Decision | Executed | 0 | None |
| decision-flow-diagram.spec.md | Decision | Executed | 0 | None |
| decision-folder-structure.spec.md | Decision | Executed | 0 | None |
| amendment-test-counts.spec.md | Amendment | Executed | 0 | None |
| feature-radar-chart.spec.md | Feature | Reviewed | 1 | None |
| feature-crosshair-markers.spec.md | Feature | Reviewed | 1 | None |
| feature-theory-tabs.spec.md | Feature | Reviewed | 1 | None |
| feature-floating-slider.spec.md | Feature | Reviewed | 2 | None |
| feature-heatmap-expansion.spec.md | Feature | Reviewed | 2 | feature-crosshair-markers.spec.md |
| feature-scenario-presets.spec.md | Feature | Reviewed | 3 | feature-floating-slider.spec.md |
| feature-timeseries-player.spec.md | Feature | Reviewed | 3 | decision-timeseries-animator.spec.md, feature-floating-slider.spec.md, feature-theory-tabs.spec.md |
| bug-timeline-crosshair-sync.spec.md | Bug | Reviewed | 4 | feature-crosshair-markers.spec.md |

## Execution Order

Specs are grouped into tiers by dependency. All specs in a tier can be executed in parallel. A tier cannot begin until all dependencies from prior tiers have reached Executed state.

### Tier 0: Decisions and Amendment (Executed)

These resolve conflicts and correct the base spec. No code changes; spec-only amendments.

1. decision-folder-structure.spec.md
2. decision-flow-diagram.spec.md
3. decision-timeseries-animator.spec.md
4. amendment-test-counts.spec.md

### Tier 1: Standalone Components (Reviewed, ready for Approved)

New components and entities that depend only on the base spec.

5. feature-radar-chart.spec.md (RadarChart component, RadarAxis entity)
6. feature-crosshair-markers.spec.md (ChartMarker entity, LineChart markers/tooltips, HeatMap crosshair)
7. feature-theory-tabs.spec.md (KatexBlock, KaTeX library, ChartCard TheoryContent, KatexTest page)

### Tier 2: Dependent Features (Reviewed, blocked on Tier 1)

Require Tier 1 components to exist.

8. feature-floating-slider.spec.md (FloatingSliderPanel, provides PresetContent host)
9. feature-heatmap-expansion.spec.md (HeatMap trace expansion, BarSegment/BarStack; depends on crosshair-markers)

### Tier 3: Cross-Cutting Features (Reviewed, blocked on Tier 2)

Require Tier 2 infrastructure.

10. feature-scenario-presets.spec.md (preset buttons in FloatingSliderPanel.PresetContent)
11. feature-timeseries-player.spec.md (TimeSeriesPlayerPage; depends on decision, floating-slider, theory-tabs)

### Tier 4: Bug Fix (Reviewed, blocked on Tier 1)

Extends LineChart with synchronized crosshair; requires crosshair vocabulary from Tier 1.

12. bug-timeline-crosshair-sync.spec.md (CrosshairTime/OnTimeHover on LineChart for TimelineDashboard)

## Open Items

None at this time.
