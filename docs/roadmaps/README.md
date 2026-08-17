# Roadmaps

Planning docs for the Character Creator hub, companion plugins, live reload, and
full-port work. Tracks are grouped by status under `completed/`, `started/`, and
`not-started/`.

**Last updated:** 2026-08-17

---

## Folder layout

| Folder | Meaning |
|--------|---------|
| [completed/](completed/) | Shipped v1 scope (workstations, Ability Lab, full-port tooling gaps) |
| [started/](started/) | In progress — partial implementation, more phases planned |
| [not-started/](not-started/) | Next major tracks not yet begun |

Hub docs at this level: [phasing.md](phasing.md), [vision-and-extensibility.md](vision-and-extensibility.md), [open-questions.md](open-questions.md).

---

## Current status (short)

| Track | Status | Doc |
|-------|--------|-----|
| Animator near-term | **Complete** (v1) | [animator-workstation.md](completed/animator-workstation.md) — feel / `rootMotions` in [animator-feel.md](completed/animator-feel.md) |
| General / Properties / Load | **Complete (v1)** | [general-workstation.md](completed/general-workstation.md) |
| Stats / entity model | **Complete** | [full-port/gaps.md](completed/full-port/gaps.md) |
| LokrAbilityLab v1 | **Complete, verified in-game** | [ability-lab.md](completed/ability-lab.md) |
| Sandbox Encounter v1 | **Complete** (in-lab fight embed; lab stays open) | [sandbox-workstation.md](completed/sandbox-workstation.md) |
| Live reload | **Phase 1–2 done**; Phase 3+ next | [live-reload.md](started/live-reload.md) |
| **Ability Lab overhaul** | **Complete** (0.12.34) — Phases 1–10; Lua card confirmed in-game 2026-08-16 | [ability-lab-overhaul.md](completed/ability-lab-overhaul.md) |
| **Animator feel** | **Complete** (Phases 1–4 in 0.12.32; Copy/Override Rest Pose in 0.12.33; pose leak awaiting in-game confirm) | [animator-feel.md](completed/animator-feel.md) |
| **Lab save UX** | **Complete** — confirmed in-game 2026-08-15 (`IsDirty`, Ctrl+S, title `*`, close prompt) | [lab-save-ux.md](completed/lab-save-ux.md) |
| **Human-readable ids** | **Complete** — phases 1–6 in 0.12.9–0.12.24 / 1.1.9; leftover folders stay valid until Rename | [human-readable-ids.md](completed/human-readable-ids.md) |
| **Legacy pack port** | **Complete** — confirmed in-game 2026-08-15 | [legacy-pack-port.md](completed/legacy-pack-port.md) |
| **Lab hover coverage** | **Complete** (0.12.35) — Ability leftovers + Character / Animator / Sandbox bound; confirmed in-game 2026-08-16 | [lab-hover-coverage.md](completed/lab-hover-coverage.md) |
| **Base Game HTML docs** | **Complete** — all 1631 pages `verified` | [base-game-html-docs.md](completed/base-game-html-docs.md) |
| **Editor redesign** | **Phase 9 complete** — Ability Library project type; Phase 10 is [Encounter Creator](started/encounter-creator.md) | [editor-redesign.md](started/editor-redesign.md) |
| **Encounter Creator** | **Started** — Phases 1–17 confirmed in-game 2026-08-17 (LokrLab 0.12.104) | [encounter-creator.md](started/encounter-creator.md) |
| **Vanilla Character Edit** | **Complete** — Phases 1–5 all confirmed in-game 2026-08-17 | [vanilla-character-edit.md](completed/vanilla-character-edit.md) |
| **Vanilla Ability Edit** | **Not started** — research | [vanilla-ability-edit.md](not-started/vanilla-ability-edit.md) |
| **Vanilla Encounter Edit** | **Not started** — research | [vanilla-encounter-edit.md](not-started/vanilla-encounter-edit.md) |
| **LokrLab suite merge** | **Complete** — confirmed in-game 2026-08-15 | [lab-suite-merge.md](completed/lab-suite-merge.md) |
| **Issue resolution tests** | **Started** — Pass 1–2 code done; in-game checklist partial (party stow / inventory confirmed; sanitize 4-hero and vanilla-trio steps not run) | [issue-resolution-in-game-tests.md](started/issue-resolution-in-game-tests.md) |
| **Automated test suite** | **Complete** (Layer 1) — 97 xUnit tests; 29 issues in `unresolved-tested/` | [test-suite.md](completed/test-suite.md) |

---

## Completed

| Doc | Contents |
|-----|----------|
| [general-workstation.md](completed/general-workstation.md) | Load / Home / Properties, readiness checklist, legacy import |
| [animator-workstation.md](completed/animator-workstation.md) | Rig editor v1; feel / `rootMotions` in [animator-feel.md](completed/animator-feel.md) |
| [animator-feel.md](completed/animator-feel.md) | Animator feel after v1 — rest seeds new clips, temp group pivot, `rootMotions`, Copy/Override Rest Pose; pose leak code complete |
| [human-readable-ids.md](completed/human-readable-ids.md) | Unique `slug_token` ids plus per-folder `aliases.json` / `$alias` |
| [ability-lab.md](completed/ability-lab.md) | Separate plugin rationale, v1 scope, per-ability folders |
| [sandbox-workstation.md](completed/sandbox-workstation.md) | Live combat test workstation |
| [lessons-learned.md](completed/lessons-learned.md) | Resolved historical notes |
| [full-port/](completed/full-port/README.md) | Old mod → Lab format port plan and gap audit |
| [lab-suite-merge.md](completed/lab-suite-merge.md) | Fold Character Lab and Ability Lab into LokrLab; `Mods/LokrLab/` write root |
| [legacy-pack-port.md](completed/legacy-pack-port.md) | Official Pack / DNSpy → Lab: selection sheet, vanilla exo + reskin |
| [lab-save-ux.md](completed/lab-save-ux.md) | Manual save: Ctrl+S, title `*`, close prompt, wire `IsDirty` (confirmed in-game 2026-08-15) |
| [ability-lab-overhaul.md](completed/ability-lab-overhaul.md) | Research-first Ability Lab overhaul — Phases 1–10 in LokrLab 0.12.34 (Lua card, context pickers, hover info) |
| [lab-hover-coverage.md](completed/lab-hover-coverage.md) | Hover-info strip coverage: Ability leftovers + Character Properties / Animator / Sandbox (0.12.35) |
| [test-suite.md](completed/test-suite.md) | xUnit suite, plugin-by-plugin coverage, `unresolved-tested/` issue folder |
| [base-game-html-docs.md](completed/base-game-html-docs.md) | Fill all 1631 Base Game Reference HTML pages (Pass A/B/C complete) |
| [vanilla-character-edit.md](completed/vanilla-character-edit.md) | Open / override shipped heroes; Phases 1–5 (Loader last-wins, extract, Lab UX, all in-game confirms) all done 2026-08-17 |
| [archive/](completed/archive/) | Historical implementation plans (shipped) |

---

## Started

| Doc | Contents |
|-----|----------|
| [live-reload.md](started/live-reload.md) | Hot-reload Lab edits without restarting the game |
| [character-lab-loader-pre-redesign-audit.md](started/character-lab-loader-pre-redesign-audit.md) | Pre-UI-redesign bug/architecture audit (Lab + Loader) |
| [editor-redesign.md](started/editor-redesign.md) | `LokrLab`: dockable Godot-style editor shell generalized into a project-type framework (Character, Ability Library, Encounter, …) — node tree, inspector/workspace/bottom-panel/menu registries, SimpleUI docking primitives. Phase 0–9 complete; Phase 10 is [encounter-creator.md](started/encounter-creator.md) |
| [encounter-creator.md](started/encounter-creator.md) | Own project type; target is a visual map editor. Phase 13 terrain catalog in 0.12.64; Phase 12 confirmed in-game |
| [issue-resolution-in-game-tests.md](started/issue-resolution-in-game-tests.md) | In-game test checklist for the 2026-08-15 unresolved-issue campaign |

---

## Not started

| Doc | Contents |
|-----|----------|
| [extensions.md](not-started/extensions.md) | Custom scripting (Lua card shipped; later plugin unblocked), Custom Adventures (after Encounter Creator + vanilla-edit research) |
| [vanilla-ability-edit.md](not-started/vanilla-ability-edit.md) | Open / override shipped abilities (last-wins already; refine load if needed) |
| [vanilla-encounter-edit.md](not-started/vanilla-encounter-edit.md) | Reconstruct shipped rooms; optional guarded campaign load override |

---

## Hub docs (this level)

| Doc | Contents |
|-----|----------|
| [vision-and-extensibility.md](vision-and-extensibility.md) | Vision, guiding principles, extensibility model |
| [phasing.md](phasing.md) | Suggested build order and what's complete |
| [open-questions.md](open-questions.md) | Active risks and deferred work |

---

## Related docs (not roadmaps)

- [capabilities-and-gaps.md](../capabilities-and-gaps.md) — what works today vs gaps
- [mods-folder-structure.md](../mods-folder-structure.md) — on-disk layout
- [ARCHITECTURE.md](../ARCHITECTURE.md) — plugin architecture
- [base-game-documentation-checklist.md](../base-game-documentation-checklist.md) — curated spine for Base Game Reference HTML
