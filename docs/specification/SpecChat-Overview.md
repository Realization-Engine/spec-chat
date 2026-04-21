# SpecChat Overview

SpecChat is a specification-driven system for authoring software through collaboration between humans and LLMs. It replaces traditional design documents with a formal specification language (SpecLang) and a structured workflow for evolving systems through incremental specs.

This document explains how the system is put together: the critical files, their layering, and the workflow they enable.

## Layer 1: The Language Definition

Two files define what SpecLang is and how it works.

1. **SpecLang-Specification.md** defines the language itself: what constructs exist across five registers (data, context, systems, deployment, view/dynamic, design), what they mean, and how they compose. This is the "what can you say" document.

2. **SpecLang-Grammar.md** defines how to parse it: lexer tokens, expression precedence, production rules, ambiguity resolution. This is the "how do you say it" document.

A human or LLM reads these two files and knows the full vocabulary and syntax of specification. The five registers cover the full architectural scope from stakeholder to server:
- **Data:** entities, enums, contracts, invariants, confidence signals, rationale
- **Context:** persons (human actors), external systems, relationships, tags
- **Systems:** authored/consumed components, topology, phases, traces, constraints, package policies, platform realization
- **Deployment:** environments, infrastructure nodes, component instances
- **View/Dynamic:** architectural diagram projections at multiple zoom levels, behavioral interaction sequences
- **Design:** pages, visualizations, parameter bindings, prose intent

## Layer 2: The System Specification

One file declares the system being built.

3. **samples/blazor-harness.spec.md** is the base system spec. It uses the language from Layer 1 to declare a specific system: its persons and external systems (who uses it, what it talks to), its components and their topology (who can call whom, who cannot), the build phases with gate conditions, the deployment topology (where each part runs), views at multiple zoom levels, dynamic interaction sequences, the data entities, the page declarations with visualization bindings, and the traceability mappings. This is the system skeleton. Everything about the system's structure lives here.

This file is what an LLM reads to understand the system well enough to generate or modify source code. It replaces the original design documents.

## Layer 3: Incremental Specifications

Incremental specs extend the base spec through four document types.

**Decision specs** resolve conflicts between what the spec says and what the source actually does. Each presents options, records a recommendation, and declares amendments to the base spec. Once executed, their amendments are applied to the base spec.

**Amendments** correct the base spec without adding capability: count corrections, dependency accounting, structural adjustments.

**Feature specs** declare new capabilities. Each follows a standard structure: purpose, component additions with contracts, data models, page integration, test obligations, and a concrete example.

**Bug specs** declare source gaps the spec correctly identifies. They specify current behavior, the specified behavior, root cause analysis, acceptance criteria, and implementation guidance.

Every incremental spec has a Tracking block with lifecycle state (Draft, Reviewed, Approved, Executed, Verified) and a Dependencies field listing which other specs must reach Executed state first.

## Layer 4: The Manifest

One file governs the entire collection.

4. **samples/blazor-harness.manifest.md** is the entry point. It binds everything together: which system these specs describe, what document types exist, the lifecycle states and their rules, the Tracking block convention, writing style and SpecLang syntax conventions, the full inventory of all specs with their current states, and the tiered execution order.

Someone encountering the spec collection for the first time reads this file first.

## The Workflow

### Specifying a new system

Write a base spec using the language from Layer 1. Create a manifest. The system now exists as a specification.

### Evolving the system

Write incremental specs (features, bugs, decisions, amendments). Each references the base spec, declares its dependencies, and carries a Tracking block. The manifest's inventory and execution order update accordingly.

### Implementing from spec

An LLM reads the manifest to understand what exists and what state it is in. It reads the base spec for the system skeleton. It reads the relevant feature or bug spec for the specific work. It has enough information to generate or modify source code without implementation instructions, because the spec carries the contracts, entities, constraints, and prose intent.

### Verifying

Compare generated source against the spec's contracts and constraints. Run the tests declared in the spec's test obligations. Move the spec from Executed to Verified.

## Guided Authoring

The `/spec-chat` slash command in Claude Code provides a guided authoring flow for producing spec documents. The user describes what they want in natural language; the command walks them through a staged question sequence that extracts the information each SpecLang construct requires. Answers accumulate across stages, and the LLM generates spec blocks progressively, showing previews after each stage for the user to review and revise.

The command supports all five document types: base system specs, feature specs, bug specs, decision specs, and amendments. It classifies the user's intent from their opening message and routes to the appropriate question path. The output is a complete `.spec.md` file written to the project's `Docs/Spec-Chat/` directory.

The command file lives at `.claude/commands/spec-chat.md`.

## Critical Insight

The spec is not documentation written after the fact. It is the source of truth that code is generated from and verified against. Design documents, delta audits, and working plans are transitional artifacts that serve their purpose during a session and are not referenced afterward. What remains is the language, the specs, and the manifest.
