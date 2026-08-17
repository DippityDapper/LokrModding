# Lab hover coverage

**Status:** Complete (LokrLab **0.12.35**) — Phases 1–4 bound; confirmed in-game 2026-08-16.  
**Raised:** 2026-08-15  
**Last updated:** 2026-08-16  
**Owner:** LokrLab (shared chrome in `LabHoverInfo`; copy in sidecars)

The hover-info strip shipped with Ability Lab overhaul Phase 10
([ability-lab-overhaul.md](ability-lab-overhaul.md)). This track filled
coverage: Ability leftovers, Character Properties, Animator, and Sandbox.

This track is **copy + bind**, not a new widget. Hovered field, card,
tool, or token shows a short description in the strip above the status
bar. Copy stays in editable markdown sidecars, not compiled strings.

See also [phasing.md](../phasing.md), [open-questions.md](../open-questions.md).

---

## What shipped (0.12.35)

- `AbilityHoverCopy.Reload` loads `ability-hover.md` and
  `character-hover.md` (plugin Sidecars plus `Mods/LokrLab/` overlays).
  Later files win per key. Overlay mtime is still rechecked on hover.
- Ability envelope behavior flags, AOE, Prewarm, Hit Chance, Cast FX
  sprite rows, modifiers, every built-in card TypeId/field, event hats
  (fired + dead), Special / AI / Advanced, Stage, create sheets.
- Character Properties categories, create / aliases, readiness, Reload
  in Game, Import Legacy Pack.
- Animator inspector / toolbar / timeline / Add Animation / atlas.
- Sandbox Level / Start / Stop.

Compiled fallbacks remain only for keys tests assert. Overlay still wins.

---

## Copy rules (what the body outlines)

Each sidecar entry is two lines of intent, not a restatement of the
label:

1. **Engine meaning** — what combat / parse / roster actually does with
   this value.
2. **Gotcha** — skip, overwrite, throw, or “looks related but is a
   different field” (roster Legend vs state `LEGEND`; Appearance Icon vs
   BANNER portrait; `#RANGED` vs `#PROJECTILE`).
3. **Legal values** — only when the list is closed or easy to get wrong
   (`TEAM_ALL`, `Head`/`Chest`/`Base`, `AbilityAction`/`AbilityEnd`).

Token keys (`token.%CASTER`, `token.#MELEE`) are appended when the
hovered field’s current value starts with `%` or `#`.

Skipped purely navigational chrome: Back, Close Lab, Close Project,
Cancel, category nav buttons, Scene Tree part-name rows, recent-file
rows, `x` on recents. **Add** is bound only when the add itself is a
domain concept (Add Level, Add Animation, Add variable).

---

## Sidecar layout

```
LokrLab/Sidecars/ability-hover.md
LokrLab/Sidecars/character-hover.md
Mods/LokrLab/ability-hover.md          (optional overlay)
Mods/LokrLab/character-hover.md        (optional overlay)
```

Key namespaces:

```
envelope.* / envelope.behavior.* / field.* / card.* / event.* / token.* / modifier.* / special.* / ai.*
ability.create.* / ability.library.create.* / ability.stage.*
character.general.* / roster.* / levels.* / states.* / appearance.* / skills.* / sound.*
character.localization.* / portraits.* / create.* / aliases.* / import.* / home.* / readiness.*
animator.part.* / clip.* / frame.* / toolbar.* / timeline.* / animations.* / reference.* / file.*
sandbox.*
```

---

## Out of scope (unchanged)

- Encounter Creator controls — shipped in [encounter-creator.md](../started/encounter-creator.md) Phase 8 (`encounter-hover.md`)
- Binding every list `x` / Add append button
- Scene Tree part rows, recent-file rows, category nav, Back / Close Lab
- Rewriting hover into tooltips on the widget itself (the strip is the
  product)
- Compiling copy into C# except test fallbacks

---

## Related docs

- [ability-lab-overhaul.md](ability-lab-overhaul.md) Phase 10 — chrome
- [LokrLab/Sidecars/ability-hover.md](../../../LokrLab/Sidecars/ability-hover.md)
- [LokrLab/Sidecars/character-hover.md](../../../LokrLab/Sidecars/character-hover.md)
- [ability-rules.html](../../api/character-reference/ability-rules.html)
- [general-workstation.md](general-workstation.md)
- [animator-feel.md](animator-feel.md)
- [sandbox-workstation.md](sandbox-workstation.md)
- [human-readable-ids.md](human-readable-ids.md)
