# Suggested phasing

**Last updated:** 2026-08-17

See [full-port/README.md](completed/full-port/README.md) for how a full port folds into this
order — [priorities.md](completed/full-port/priorities.md) gives its own priority list.

---

## Completed (items 1–6)

1. **Animator near-term scope** — **complete** ([animator-workstation.md](completed/animator-workstation.md)):
   pivots, atlas import, attach points/events, undo/redo, extensibility
   registries, plus multi-select, viewport grid, Mass Edit, easing round-trip fix.

2. **General workstation v1** — **complete** ([general-workstation.md](completed/general-workstation.md)):
   Load / Home / Properties hub, character scaffolding, identity fields, readiness
   checklist, Legacy Mod Import. End-to-end verified 2026-08-11 (Onagro).
   `CharacterCreatorAPI.RegisterWorkstation` shipped 2026-08-11.

3. **General stats / entity-model extension** — **complete**
   ([full-port/gaps.md](completed/full-port/gaps.md)): custom stats, level chains, states,
   sound config, achievement unlock, multi-locale localization, Hero vs
   `EnemySummon`. Verified 2026-08-11/2026-08-12.

4. **`LokrAbilityLab` v1** — **complete, verified in-game 2026-08-12**
   ([ability-lab.md](completed/ability-lab.md)): envelope form + raw KV body, per-ability
   folders under `Mods/LokrAbilityLab/LokrAbilityLab/<libraryId>/<abilityId>/`.

5. **Sandbox Encounter v1** — **complete**. As of 0.9.40 Sandbox uses the
   same in-lab fight embed as Ability Lab Stage (`StartEmbeddedFight`);
   fight-end unloads the hole and does not call `ReopenAfterFight`.
   ([sandbox-workstation.md](completed/sandbox-workstation.md)).

6. **[Ability Lab overhaul](completed/ability-lab-overhaul.md)** —
   **complete** (LokrLab 0.12.34). Phases 1–10: catalog, rules,
   nested-card editor, custom sprite VFX / clips, viewport host, Lua
   card, embedded Stage fight, context pickers, hover info. Lua card
   confirmed in-game 2026-08-16.

---

## Next (item 7)

7. **[Encounter Creator](started/encounter-creator.md)** — Phases 1–17
   confirmed in-game 2026-08-17 (LokrLab 0.12.104). v1 map editor +
   Sandbox is done.

8. **Vanilla asset edit** — before Custom Adventures.
   [vanilla-character-edit.md](completed/vanilla-character-edit.md) is
   **complete** (Phases 1–5 all confirmed in-game 2026-08-17).
   [vanilla-ability-edit.md](started/vanilla-ability-edit.md) is
   **started** (Phases 1–2 (browser, copy-into-library) confirmed in-game 2026-08-17).
   [vanilla-encounter-edit.md](not-started/vanilla-encounter-edit.md)
   stays research. Custom scripting and Custom Adventures stay on
   [extensions.md](not-started/extensions.md) (quest / map authoring,
   not one-room override).

---

## Parallel track: legacy pack port

[legacy-pack-port.md](completed/legacy-pack-port.md) — **complete**,
confirmed in-game 2026-08-15. Independent of the Ability Lab overhaul
(complete in 0.12.34).

---

## Parallel track: LokrLab suite merge

[lab-suite-merge.md](completed/lab-suite-merge.md) — **complete**,
confirmed in-game 2026-08-15. Character + Ability authoring live in
LokrLab; on-disk content under `Mods/LokrLab/`. Encounter Creator no
longer waits on this track — see
[encounter-creator.md](started/encounter-creator.md).

---

## Parallel track: live reload

Phase 1–2 done ([live-reload.md](started/live-reload.md)). Phase 3 (selective/scoped reload)
is next on that track, independent of items 6–7.

---

## Parallel track: base-game HTML docs

[base-game-html-docs.md](completed/base-game-html-docs.md) — **complete**.
All 1631 Base Game Reference pages are `DOC-STATUS: verified` (mod surface, bugs,
three passes). Documentation only; does not block item 6 or 7. The older
[base-game-documentation-checklist.md](../base-game-documentation-checklist.md)
stays as the curated spine.

---

## Parallel track: Lab hover coverage

[lab-hover-coverage.md](completed/lab-hover-coverage.md) — **complete**,
confirmed in-game 2026-08-16 (LokrLab 0.12.35). Strip shipped in Ability
Lab overhaul Phase 10; Phases 1–4 bound Ability leftovers, Character
Properties, Animator, and Sandbox. Encounter binds its own sidecar in
[encounter-creator.md](started/encounter-creator.md) Phase 8.

---

## Parallel tracks: Lab follow-ups (filed 2026-08-14)

Docs only until a code pass. Suggested order when implementing:

1. Sandbox Forfeit confirm behind settings
   ([sandbox-forfeit-confirm-behind-settings.md](../issues/unresolved/sandbox-forfeit-confirm-behind-settings.md)).
2. Animator pose leak
   ([animator-pose-leaks-across-frames.md](../issues/unresolved/animator-pose-leaks-across-frames.md))
   — Phase 1 of [animator-feel.md](completed/animator-feel.md)
   (code in 0.12.32; confirm in-game before `resolved/`).
3. [Lab save UX](completed/lab-save-ux.md) (Ctrl+S, dirty `*`, close
   prompt) — **complete**, confirmed in-game 2026-08-15. Unblocks
   Encounter Creator.
4. [Human-readable ids](completed/human-readable-ids.md)
   (`slug_token` + per-folder `aliases.json` / `$alias`). **Complete**
   (phases 1–6 in LokrLab 0.12.9–0.12.24 / LokrCharacterLoader 1.1.9).
   Leftover numeric folders stay valid until the user Renames them.
5. Ability overhaul Phases 7 / 9 / 10 (Lua card, filtered pickers,
   hover info) shipped in 0.12.34
   ([ability-lab-overhaul.md](completed/ability-lab-overhaul.md)).
   Animator feel Phases 2–4 (rest-as-clip-seed, temp pivot,
   `rootMotions`) shipped in 0.12.32; Copy/Override Rest Pose in 0.12.33
   ([animator-feel.md](completed/animator-feel.md)).

Do not move [animator-workstation.md](completed/animator-workstation.md)
out of completed — that doc is v1 scope. Feel / `rootMotions` live in
[animator-feel.md](completed/animator-feel.md).

---

## Parallel track: automated tests

[test-suite.md](completed/test-suite.md) — **complete** (Layer 1, 2026-08-15; 86 tests). xUnit for
helpers; `unresolved-tested/` for issues whose tests pass but are not
yet confirmed in-game. Does not replace in-game confirm for `resolved/`.

---

## Design note

Each workstation's (or companion plugin's) extension points
([vision-and-extensibility.md](vision-and-extensibility.md)) should land
**alongside** v1, not after — retrofitting extension surfaces onto hardcoded
internals is substantially more expensive than designing against an extension
point from the start.
