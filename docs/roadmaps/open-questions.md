# Open questions and deferred work

Active items only. Resolved historical notes (migrations, bugs fixed during
verification) live in [lessons-learned.md](completed/lessons-learned.md). Full-port-specific
questions also in [full-port/open-questions-port.md](completed/full-port/open-questions-port.md).

**Last updated:** 2026-08-17

---

## Next on the main phasing track

See [phasing.md](phasing.md) — core v1 (items 1–5) is complete. **Item 6:
[Ability Lab overhaul](completed/ability-lab-overhaul.md)** is complete
(LokrLab 0.12.34; Lua card confirmed in-game 2026-08-16). **Item 7:
[Encounter Creator](started/encounter-creator.md)** Phases 1–17 are
confirmed in-game 2026-08-17. **Item 8:** vanilla asset edit.
[Character](completed/vanilla-character-edit.md) is complete (Phases
1–5 all confirmed in-game 2026-08-17).
[Ability](started/vanilla-ability-edit.md) is started (Phases 1–3 —
browser, copy-into-library, fidelity audit — confirmed in-game
2026-08-17; round-trip safe enough to proceed).
[Encounter](not-started/vanilla-encounter-edit.md) stays research.
Custom scripting (later plugin) and Custom Adventures stay on
[extensions.md](not-started/extensions.md).

---

## Character Creator / plugins

### Sandbox fight-end behavior

Superseded 2026-08-13 (0.9.40): Sandbox and Stage use an additive fight
embed. Fight-end unloads the hole; the lab stays open. `ReopenAfterFight`
is gone. Close Lab still returns to the pre-lab origin. See
[sandbox-workstation.md](completed/sandbox-workstation.md).

### Ability Lab — curated action/effect UI

v1 shipped envelope fields + raw KV body. Phase 4 of the Ability Lab
overhaul replaced that body box with nested action cards
([editor-design.md](../../LokrLab/docs/ability/editor-design.md)).
See [ability-lab-overhaul.md](completed/ability-lab-overhaul.md). Custom
sprite FX / clips shipped in Phase 5 (0.5.0); viewport host (browser,
card canvas, Stage) shipped in Phase 6 (0.6.0); simulated Stage viewer
shipped in Phase 8 (0.8.0); Lua card, context pickers, and hover info
shipped in 0.12.34 (Phases 7, 9, 10). Hover coverage (Ability leftovers +
Character / Animator / Sandbox) shipped in 0.12.35 and confirmed in-game
2026-08-16:
[lab-hover-coverage.md](completed/lab-hover-coverage.md). Encounter
controls wait on [encounter-creator.md](started/encounter-creator.md)
Phase 8.

### Ability Lab — scene transition

Resolved 2026-08-13 (LokrAbilityLab 0.4.1). The fallback `AbilityLabScene`
uses the same `FadeScreen` + `UnloadSceneAsync` + `TransitionSceneComponent`
pattern as LokrLab. Opening an Ability Library through the shell already
used LokrLab's fade; the leftover overlay path now does too.

### Animator — `rootMotions`

Authorable in LokrLab 0.12.32 (Frame inspector **Root X (px)**;
`rig.json` `rootMotions` + sidecar). See
[animator-feel.md](completed/animator-feel.md) Phase 4. Combat uses
`moveCurve`; the Lab viewport does not offset the grid live.

### Animator feel (rest pose, temp pivot, pose leak)

Phases 2–4 shipped in 0.12.32; Copy/Override Rest Pose in 0.12.33
([animator-feel.md](completed/animator-feel.md)). Pose leak with Mass
Edit off is still
[animator-pose-leaks-across-frames.md](../issues/unresolved/animator-pose-leaks-across-frames.md)
(code complete; confirm in-game). Rest Pose seeds frame 0 of a **new**
clip only. Temporary multi-select pivot is separate from rest-wide
`PivotOffset`.

### Human-readable unique ids

Two community packs with the same display name must both load. Design
settled in [human-readable-ids.md](completed/human-readable-ids.md):
`slug_token` folder/engine id plus per-folder `aliases.json` / `$alias`.
**Complete** (phases 1–6 in LokrLab 0.12.9–0.12.24 / LokrCharacterLoader
1.1.9). Existing 18-digit folders stay valid until the user Renames them.

### Lab save UX

Confirmed in-game 2026-08-15. LokrLab 0.12.25+ (`LabSaveUx`): Animator
and Ability edits set `session.IsDirty`, Ctrl+S / File → Save flush,
`*` on the LoKR Lab title and status bar, save / discard / cancel on
Close Lab, Close Project, and jump. See
[lab-save-ux.md](completed/lab-save-ux.md).

### Sandbox Forfeit confirm

Forfeit confirm draws behind the vanilla settings panel in the
embedded sandbox fight. See
[sandbox-forfeit-confirm-behind-settings.md](../issues/unresolved/sandbox-forfeit-confirm-behind-settings.md).

### Ability VFX / wholly new animations

Tracked on the [Ability Lab overhaul](completed/ability-lab-overhaul.md):
Phase 1 catalogs every vanilla FX/animation name; Phase 2 documents the
`FXManager` / exo-skeleton pipelines (reuse vs custom, which plugin owns
which piece) — see
[ability-vfx-animation.html](../api/character-reference/ability-vfx-animation.html).
Phase 5 implements custom sprite FX / projectiles via Loader inject.
Unknown names still throw unless a matching `fx/<name>/` or
`projectiles/<name>/` folder exists. Full particle FXMega still needs
a Unity AssetBundle. Seeing those prefabs fly/play in the Stage dock (not a fight) shipped
as overhaul Phase 8 (0.8.0). Phase 9 (context-aware pickers) and
Phase 10 (hover info box) shipped in 0.12.34.

### Shared mod-wide resources

No first-class concept for content that is not any one character's (shared roster
banners, global fallbacks, etc.). Design decision, low urgency — see
[full-port/open-questions-port.md](completed/full-port/open-questions-port.md).

### `Model` field (`unitDefinition.kind`)

**Resolved 2026-08-14** — it is the vanilla `units`-bundle prefab combat
instantiates (`UnitViewManager.FindPrefab(unit.kind)`), then the custom
rig is swapped onto it. Controllers (clip names) come from that prefab,
so Onagro stays `ObeliskLvl4` even though the mesh is custom. Do not
delete the field; new characters already default `HumanArcher`. UI
relabel shipped with
[legacy-pack-port.md](completed/legacy-pack-port.md).

### `Background` roster field

Likely dead — no confirmed rendering path in decompiled source. Low priority.

---

## Live reload

Phase 1–2 are **implemented** ([live-reload.md](started/live-reload.md)): `Reload in Game`,
auto-reload on lab close, full content re-read. **Next on that track:** Phase 3
selective/scoped reload (dirty flags, minimal `ReloadScope` per edit type).

Open verification: save safety (test T9), hero room open during reload, in-place
UI refresh without re-opening screens (Phase 4).

---

## Pre-UI-redesign audit

Full bug/SRP/ModAPI inventory for Character Lab and Loader:
[character-lab-loader-pre-redesign-audit.md](started/character-lab-loader-pre-redesign-audit.md).
That was Phase 0 of [editor-redesign.md](started/editor-redesign.md)
(Phase 9 complete). Remaining P0/P1 items are filed under
`docs/issues/`; do not treat the audit as a gate on work that already
shipped.

---

## Extension API stability

`CharacterAPI` / `CharacterCreatorAPI` public surfaces should stabilize before
third-party plugins depend on them heavily. Shape is proven; formal stability
commitment is timing, not design.

---

## Combat animation names (monitoring)

**Resolved 2026-08-12** (`CombatSequenceNames.ForModel`, readiness checks) — keep
monitoring when adding new `Model` prefabs. Missing sequence names **throw** in
combat, not warn.
