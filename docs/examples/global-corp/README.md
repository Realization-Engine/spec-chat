# Global Corp — spec collection

This directory contains the SpecChat spec collection for the fictional **Global Corp.** enterprise used as a scale exemplar across the Realization Engine projects.

The files here are the machine-processable artifacts: architecture spec, application specs, gate specs, and the collection manifest. They exercise SpecChat's validators end-to-end and are authoritative for all cross-references, counts, and IDs in the exemplar.

## The narrative companion lives in ASAP

The long-form narrative document that describes what Global Corp. *is* as an enterprise (strategic thesis, stakeholder catalog, ASR and ASD registries, governance bodies, view gallery, roadmap, and the rest) is maintained in the ASAP repository, because *The Architect and the Agent* (folio Nº IV) treats Global Corp. as the worked enterprise against which the ASAP practice architecture is derived.

- Source: [`github.com/Realization-Engine/asap/blob/main/docs/examples/global-corp/Global-Corp-Exemplar.md`](https://github.com/Realization-Engine/asap/blob/main/docs/examples/global-corp/Global-Corp-Exemplar.md)
- Published: [`realizationengine.net/asap/docs/examples/global-corp/Global-Corp-Exemplar.html`](https://realizationengine.net/asap/docs/examples/global-corp/Global-Corp-Exemplar.html)

Readers looking to understand Global Corp. as an architecture should start with the narrative and then follow the cross-references back to the spec files in this directory.

## What's in this directory

- `global-corp.manifest.md` — collection manifest
- `global-corp.architecture.spec.md` — enterprise architecture spec
- `app-*.spec.md` — application specs for each Global Corp. application
- `*-gate.spec.md` — gate specs for integrations and boundaries
- `service-defaults.spec.md`, `aspire-apphost.spec.md` — platform specs

Run SpecChat validators against this directory (not against the narrative document) to exercise the collection.
