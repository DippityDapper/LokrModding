# Ability Lab overhaul

**Status:** Complete (LokrLab **0.12.34**) — Phase 1–10 shipped (catalog, rules, nested-card editor, custom sprite VFX / clips, viewport host, Lua card, embedded Stage fight, context pickers, hover info). Lua card and hover strip confirmed in-game 2026-08-16.  
**Raised:** 2026-08-13  
**Last updated:** 2026-08-16  
**Owner:** LokrAbilityLab (docs + later editor). Character Lab does not write `ability.txt`.

v1 Ability Lab (envelope + raw KV) is **complete** — see
[ability-lab.md](ability-lab.md). Phase 4 replaced the raw
body box as the primary editor. This is a **new track**, not a rewrite
of that v1 doc.

Goal: turn Ability Lab from a glorified text editor into a complete,
user-friendly authoring tool (visual / block-style is the intended
direction) that can also grow custom scripting and custom VFX / cast
animations. The engine already has the ability *system*. What we do not
have is a complete picture of how vanilla skills are structured, which
combinations are legal, and how FX / animations / AI / loc attach. Design
the editor only after that picture exists.

See also [Character Creator roadmaps](../README.md),
[phasing.md](../phasing.md), [extensions.md](../not-started/extensions.md).

---

## Why research first

Definitions are KV1 `TextAsset`s parsed by `AbilityParser.ParseAbility()`
into `Ability` + nested `Modifier` graphs — not ScriptableObjects. Runtime
is an event/action DSL (~70 registered action types, arbitrary nesting),
plus optional `Lua` actions and `CallFunction` reflection into shipped C#
helpers.

[abilities.html](../../api/character-reference/abilities.html) documents
the **schema**. Appendices J–X only saw a **partial extract** (~24 ability
ids under `_extracted/base-game/resources/filter-Abilities/`). Hero kits
such as `gerald_swing` exist in `resources.assets` as individual TextAssets
and are referenced from `RLHeroes_new.txt`, but were never dumped.
Designing a block editor against the schema page plus four shared files
would guess at combinations the game never uses and miss real
incompatibilities.

Passives and actives are the **same file format**. `PASSIVE` (flag 256)
makes the parser skip combat fields; traits usually live in unit `skills`
slots `100` / `200` / `300` and auto-apply nested modifiers with
`"Passive" "1"`. Both stay in Ability Lab.

VFX and cast animations are part of a complete ability, not a later
leftover. Today a modded ability can only **reuse** a shipped `CastFXId` /
`EffectName` / projectile `Model` (`FXManager.LoadFXMega` throws on
unknown names), and `AnimationID` must be a clip on the caster's
exo-skeleton with `AbilityAction` / `AbilityEnd` events. Character Lab can
already author new clips on a custom rig; Ability Lab has no catalog of
vanilla FX names and no plan for injecting custom FXMega prefabs.

```
Phase 1 extract  -->  Phase 1 HTML catalog
                         |              |
                         v              v
                  Phase 2 rules   Phase 2 VFX / cast-anim pipelines
                         |              |
                         +------+-------+
                                v
                         Phase 3 editor design
                                v
                         Phase 4 visual editor
                                v
                         Phase 5 custom VFX / cast anims
                                v
                         Phase 6 viewport host
                                v
                    +-----------+-----------+
                    v                       v
         Phase 7 custom scripting   Phase 8 embedded Stage fight
         (Lua card; no viewer dep)  (does not wait on Phase 7)
                                |
                                v
                    Phase 9 context-aware pickers
                                |
                                v
                    Phase 10 hover info box
```

**Gate:** Phase 3 picked the visual model (nested action cards). Phase 4
implements it. No custom FX assets until Phase 5.

Phase 1 (2026-08-13): 409 AbilityBehavior TextAssets → 431 ability
pages at
[skills-catalog.html](../../api/character-reference/skills-catalog.html).
Phase 2 (2026-08-13):
[ability-rules.html](../../api/character-reference/ability-rules.html)
and
[ability-vfx-animation.html](../../api/character-reference/ability-vfx-animation.html).

---

## Phase 1 — Extract and document

Goal: a browsable HTML catalog of **every vanilla ability**, in the same
chrome as stats and tags.

### Extract

Use the existing AssetStudioModCLI path in
[docs/reference/README.md](../../reference/README.md):

```bash
DOTNET_ROLL_FORWARD=LatestMajor \
  ~/dev/lokr-modding/lokr-modding/AssetStudioModCLI/AssetStudioModCLI_net9_linux64/AssetStudioModCLI \
  "$GAME/legends_Data/resources.assets" \
  -m export -t textasset \
  -o bepinex/docs/character-reference/_extracted/base-game/AbilitiesScript -r
```

Keep files that contain `"AbilityBehavior"`. Cross-check ids from
`defaultSkill` / `skills` / `skillProgression` in already-extracted
`RLHeroes_new.txt` and enemy defs; flag missing definitions. Also index
every nested `Modifiers` block (ids are **global**).

Vanilla only — same rule as appendices. Official Pack `NewAbilities/` is
a later comparison corpus (see completeness inventory), not the source of
"what is allowed."

### HTML

- Hand-maintained narrative stays in
  [abilities.html](../../api/character-reference/abilities.html) (schema).
  Extend it; do not replace it.
- New generated **Skills catalog** under
  `docs/api/character-reference/` (index + per-ability pages), registered
  in [sync_sidebar.py](../../api/character-reference/sync_sidebar.py)
  `PAGES`.
- New generator next to
  [generate_appendices.py](../../character-reference/generate_appendices.py),
  using the same `page_shell()` / sidebar / `style.css`.
- Re-run appendices so §J–§X reflect the full dump, not 24 ids.

Each ability page should show: id, behavior flags, envelope fields,
event/action tree, nested modifiers, which units reference it
(`defaultSkill` / `skills` / `skillProgression`), **every FX/animation
string it names** (`CastFXId`, `AnimationID`, `EffectName`, projectile
`Model`, modifier `EffectName` / `ModifierFXName`), and the raw KV.

Also generate indexes from the same dump:

- **FX catalog** — every distinct FXMega / `EffectName` / projectile
  `Model` / `AttachEffect` string, which abilities use it
- **Animation catalog** — every `AnimationID` plus `PlayAnimation` /
  `OverrideAnimation` clip names
- **Icon catalog** — every `Icon` stem (vanilla `AbilityIcons/` names)
- **Sound catalog** — every `PlaySound` / `StopSound` name used in
  abilities
- **CallFunction catalog** — every `"Function"` type string the dump
  actually calls
- **Spawn / chain catalog** — `SpawnUnit` unit ids and `TriggerSkill`
  ability ids
- Pair each ability page with localization keys when those TextAssets
  are extracted: `SKILL_<id>_NAME`, `_DESCRIPTION`, `_DESCRIPTION_DATA`,
  `_DESCRIPTION_EPIC`, `_DESCRIPTION_EXTRA`, plus
  `COMBAT_MODIFIER_<modifierId>_*`

### Done when

- [x] Every `RLHeroes` / enemy skill id has a definition page or is
      listed as missing (only `fixed_ranged_attack` is referenced with no
      TextAsset — BanditArcherNoDamageRooted `defaultSkill`)
- [x] Appendices counts jump from ~24 to the real catalog size (appendix
      X: 433 ids; catalog parser: 431)
- [x] The indexes above exist even if prefab/audio internals are still
      unknown — [skills-catalog.html](../../api/character-reference/skills-catalog.html)
      plus FX / animation / icon / sound / CallFunction / spawn pages

---

## Phase 2 — Rules analysis

Still docs, no UI. Read at least `AbilityParser.cs`, `Ability.cs`,
`Modifier.cs`, `AbilityMeleeActivity.cs`, `AbilityEvents.cs`,
`ModifierEvents.cs` under
`lokr-modding/ih-original/Ironhide.Legends/.../Abilities/`.

Write a hand-maintained **Ability rules** page (same character-reference
chrome) covering:

- Active vs `defaultSkill` vs `PASSIVE` traits (same KV, different unit
  wiring and parser path)
- Behavior-flag combinations the dump actually uses vs combinations the
  parser rejects or ignores (e.g. combat fields on `PASSIVE`; AOE fields
  without `AOE`)
- Modifier rules: `IncompatibleStates`, `AutoRemoveModifierIds`,
  `AutoRemoveTags`, stacking, `"Passive" "1"` auto-apply
- Action nesting: which actions are legal under which events;
  `CallFunction` / `Lua` as escape hatches
- Empty expressions drop the whole ability from the registry
- Hero-room / combat UI contract (see completeness inventory)

### VFX and cast-animation pipelines (same phase, own HTML page)

Do not treat these as a known ceiling and stop. Document how they
actually work so a complete Ability Lab can plan authoring, reuse, and
(if viable) custom assets. Read at least:

- `FXManager.cs` — `Preload()` fills `fxMegaPrefabs` from
  `DataHelper.LoadAllFXMegaList()`; `LoadFXMega(name)` **throws** if the
  name is missing. Prefab path constant:
  `Assets/ResourcesBundle/Prefabs/Scenario/FXMega`
- `AbilityMeleeActivity.cs` — `CastFXId` →
  `AddFXMegaToUnit(..., "CAST")`
- `HitAction.cs` — `EffectName` on the target
- `ModifierInstance.cs` — modifier FX on apply
- Projectile / tracking actions — `Model` and related fields
- Character Lab + `CustomRigLoader` — custom `rig.json` clips already
  ship; a new `AnimationID` on a *custom* character may already play if
  the clip has the right frame events. That is a different problem from
  "vanilla MetaExo reuse"

Research questions the page must answer (findings, not guesses):

- Where do FXMega prefabs actually live on disk (which bundle /
  Resources path), and can AssetStudio list every prefab name?
- What is inside an FXMega (sprites, particles, attach points, sounds)?
  Is a simpler mod-authorable format possible, or only Unity prefabs?
- Can a `CharacterAPI` FX resolver (same pattern as portraits/sounds)
  inject into `fxMegaPrefabs` without shipping AssetBundles?
- Which ability fields are name-lookups into that dictionary vs
  something else (projectiles, sounds, icons)?
- Cast animation: `AnimationID` is a clip name on the **caster's**
  exo-skeleton, not a global animation library. How should Ability Lab
  pick clips when the ability is shared across characters with different
  rigs?
- What is the exact frame-event contract (`AbilityAction`, `AbilityEnd`,
  custom event strings)?
- What can Ability Lab own vs what must stay Character Lab (clip
  authoring, attach points) vs what needs a Loader hook?

### Done when

- [x] We can answer "what may be on at the same time" from the dump +
      parser, not from memory —
      [ability-rules.html](../../api/character-reference/ability-rules.html)
      (35 vanilla `AbilityBehavior` combos; parser skips/overrides)
- [x] Written VFX/animation pipeline (reuse vs custom, which plugin owns
      which piece) —
      [ability-vfx-animation.html](../../api/character-reference/ability-vfx-animation.html)
      (`scenario` bundle, 460 FXMega names, public `fxMegaPrefabs` inject
      point, clip frame-event contract)
- [x] Completeness inventory below is covered on the rules page or a
      sibling page (not left as "later")

That pair of pages is the input to Phase 3. **No editor design in this
phase.** Phase 2 finished 2026-08-13.

---

## Completeness inventory

The KV body, flags, VFX, and cast clips are not the whole ability. Phase
1 indexes the names; Phase 2 explains the rules. A complete Ability Lab
has to plan for all of these, even if some stay "picker over vanilla
names" forever.

| Area | What to research | Notes |
|------|------------------|-------|
| **AI** | `AIConfigB`, `AIBrain*`, `OnThink`, consideration types (`PerAffectedAI`, `DistanceToTarget`, `UnitStat`, …) | Custom abilities get generic scoring unless this is authored. [capabilities-and-gaps.md](../../capabilities-and-gaps.md) already flags "correct but not well." |
| **Expressions and AbilitySpecial** | Full `expressionFunctions` registry, context tokens (`%CASTER`, `%TARGET`, hit/projectile/encounter contexts), which expressions are legal on which fields | Schema page exists; the dump will show real usage. |
| **Targeting** | `AbilityTeamFilter`, `AbilityTargetFilterFlags`, `AbilityCustomTargetFilter`, `AbilityShowDetailFilter`, AOE filters, `OnCustomTargeting` | Sixteen shipped C# helpers under `Content/Abilities/` (`KrumSelectTargets`, teleport/summon targeters, diversifiers) are `CallFunction` / custom-target only — inventory them so the lab knows what cannot be expressed in KV alone. |
| **Hit and damage pipeline** | `Hit` / `AddDamage` / `Damage`, `DamageType`, hit chance (`HAS_CHANCE_TO_HIT`, `HitChanceModifier`), armor, legendary hits, `CancelHit` | How most actives actually resolve. |
| **Sounds** | `PlaySound` / `StopSound` in the graph; unit `soundConfig` `useSkill`; audio that lives inside FXMega | Three different sources; do not fold them into VFX. |
| **Icons and localization** | `Icon` stems; `SKILL_*` and `COMBAT_MODIFIER_*` keys | `RegisterAbilityIconResolver` already exists. Hero-room traits need a non-null `Icon` or they do not show. |
| **Projectiles** | `TrackingProjectile`, `KeepProjectileGoing`, `OnProjectileHitUnit` / `Missed` / `DestinationReached` | Confirm whether `Model` is an FXMega name or a different asset class. |
| **Extra animation actions** | `PlayAnimation`, `PlayActivityAnimation`, `OverrideAnimation`, `RemoveOverrideAnimation` | Besides envelope `AnimationID`. |
| **Three FX attach paths** | `CastFXId` (cast), `Hit.EffectName` / `AttachEffect` (impact), modifier FX (duration) | Catalog and rules must not treat them as one field. |
| **Summons** | `SpawnUnit` unit ids | Ability Lab picks; it does not create creatures (already decided in v1). Character Lab owns the entity. |
| **Chaining** | `TriggerSkill`, `QueueAttackUnit`, `ResetCooldown`, `OffsetCooldown` | |
| **Cinematics** | `CINEMATIC` flag, `QueueCinematic`, cinematic-id expression functions | |
| **Battlefield control** | `AreaControl`, `ModifyHexPassable`, `Knockback`, `MoveUnit`, `CameraControl` | |
| **Metagame hooks** | Map/upgrade modifiers (`mapMapping`), darkness modifiers, `AchievementIncrement` | How often vanilla uses them, and whether the lab must expose them. |
| **In-ability Lua** | `Lua` action globals (`abilitiesHelper`, `encounterHelper`, `cinematicHelper`) | Different from encounter `Mods/*/Lua/*.lua`. |
| **Hero-room / combat UI contract** | `skillProgression` always indexes ranks 1–3 (2/3/3 slots); `defaultSkill` must not also appear in progression; passives in `skills` need `PASSIVE` + `Icon` | Already learned from Greg; write it into the rules page. |
| **Official Pack** | After vanilla is cataloged, scan `NewAbilities/` | Stretch corpus (what mods already do that vanilla never does). Not the source of "allowed." |

Phase 3 should treat this list as the editor's coverage checklist
(pickers / blocks / "advanced" / "not in v1 of the visual editor"), not
discover it while building UI.

---

## Phase 3 — Editor design discussion

Decided 2026-08-13. Full write-up:
[LokrLab/docs/ability/editor-design.md](../../../LokrLab/docs/ability/editor-design.md).

**Pick:** nested action cards — events as section hats, actions as
stacked SimpleUI cards with child stacks. That is the in-game
translation of “blocks.” Not a web Blockly host. Not a node graph
(`UiTree` is a hierarchy; KV is a list; wires would invent layout the
file does not have).

Evaluated against the catalog and SimpleUI (`UiStack`, `UiList`,
`UiComboBox`, `UiTabGroup`, `UiContextMenu`, `UiModal` — no canvas):

- Scratch / Blockly — right mental model, wrong host
- Node graph — better for wide `Conditional` / `ActOnTargets` graphs,
  unbuildable in SimpleUI without a new toolkit; 278/431 abilities are
  an `OnAbilityAction` list
- Forms + pickers + nested cards — 1:1 with `KeyValue` children;
  unknown types stay opaque cards so the tail is not one whole-file
  textarea

Envelope form stays. On-disk format stays `ability.txt`. Completeness
inventory is mapped in the design doc (v1 card set vs Advanced vs
opaque). Extensibility: `AbilityLabAPI.RegisterActionCard` on this
plugin, not `LokrLabApi`.

### Done when

- [x] Visual model picked against the catalog, not from memory
- [x] Completeness inventory assigned (picker / card / advanced /
      not-in-visual-editor-v1)
- [x] Round-trip and “no LokrLab / Character Lab DLL ref” constraints
      written into the design
- [x] Phase 4 slice listed so implementation does not re-litigate the
      model

---

## Phase 4 — Visual editor

Implemented 2026-08-13 (LokrAbilityLab **0.4.0**; expression pickers
**0.4.2**; function composer **0.4.3**).
[editor-design.md](../../../LokrLab/docs/ability/editor-design.md).

Replaced the raw-body textarea as the primary editor with nested action
cards. KV stays the on-disk format. New action types register through
`AbilityLabAPI.RegisterActionCard`. Envelope form stays. Vanilla
FX/animation **pickers** (reuse shipped names) shipped here; expression
fields are comboboxes over parser functions, context tokens, and dump
snippets. Custom-asset authoring is still Phase 5.

### Done when

- [x] Shared form builder used by overlay + Ability Library inspector
- [x] Body tree in `AbilityFileModel` / `AbilityKvIO` with opaque fallback
- [x] Tabs: Envelope | Events | Modifiers | Special | AI | Advanced
- [x] v1 card set + event hats
- [x] Shipped picker catalogs + warn-on-unknown
- [x] Expression / CallFunction / stat / unit-ref / damage-type comboboxes (0.4.2)
- [x] One-level expression function composer (0.4.3)
- [x] `AbilityLabAPI.RegisterActionCard`
- [x] Create-sheet templates (Melee / Ranged / Ally buff / Passive / Point AOE)
- [x] Raw whole-file box gone as the primary editor; Advanced remainder for the tail

---

## Phase 5 — Custom VFX and cast animations

Implemented 2026-08-13 (LokrCharacterLoader **1.1.1**, LokrAbilityLab
**0.5.2**). Phase 2's thinner workaround: a folder + PNG + JSON, the
Loader builds a minimal `FXMegaComponent` / projectile prefab, Ability
Lab only stores the name. Combat rebuilds a missing prefab from the
folder on `LoadFXMega` so a scene change cannot throw
`Could not load FXMega`. Attach points are sockets (`Chest`), not
expression tokens (`#Chest`).

- **Reuse** — Phase 4 pickers over the 460 FXMega names, projectile
  models, and vanilla clip names
- **Custom cast clips** — Ability Lab lists clip names scraped from
  Character Lab `rig/rig.json` (strings only, no DLL reference). The
  envelope Animation Id field documents the `AbilityAction` /
  `AbilityEnd` contract; Character Lab Save already backfills those on
  Attack / SpecialAttack / SpellCast*
- **Custom FX** — `CharacterAPI.RegisterFxMegaResolver` + file
  convention `fx/<name>/sprite.png` + `fx.json`. `CustomFxLoader`
  injects into `FXManager.fxMegaPrefabs` after Preload. Hit / modifier
  / Cast FX all use the same name
- **Custom projectiles** — `CharacterAPI.RegisterProjectileResolver` +
  `projectiles/<name>/`. Prefix on `DataHelper.LoadProjectile` (not the
  FXMega dictionary)
- Full Unity particle FXMega still means an external AssetBundle; this
  phase does not become a particle editor

### Done when

- [x] Loader injects sprite FXMega into `fxMegaPrefabs` so unknown
      names no longer throw if a folder exists
- [x] Projectile Model has the same folder inject
- [x] Ability Lab can create `fx/<name>/` and `projectiles/<name>/`
      and pick those names
- [x] Custom clip names from Character Lab rigs appear in Animation Id
- [x] No `LokrCharacterLab.dll` / `LokrLab.dll` reference
- [x] Docs: folder layout, CharacterAPI, VFX page, this roadmap

---

## Phase 6 — Viewport

Implemented 2026-08-13 (LokrAbilityLab **0.6.0**).

The Library workspace `BuildViewport` is a help label. The inspector
hosts the whole editor (envelope + nested cards) in a dock that is too
narrow for stacks, while the center is unused. Character Lab already
treats the center as workspace-owned working surface (Properties /
Animator / Sandbox). Ability Lab should do the same.

An **ability preview belongs here** — play the authored sequence on a
cheap stage (caster dummy, target dummy, Cast FX, projectile path, hit
FX, `PlaySound`) so sprite / attach / timing mistakes show up without
entering a fight. Preview is not the only job. The viewport should also
do the things a form is bad at: **browse, lay out the cards, space,
time, and “what actually happens.”**

`WorkspaceRegistration.BuildViewport` is the hook
([editor-redesign.md](../started/editor-redesign.md) §5.8). Do not
embed `KRLegendsFightGameplay02` in the dock — Sandbox’s fight stays a
scene jump (§6.2). Do not reopen a node graph or Blockly as the editor
(Phase 3 still holds). Do not fill the center with a static PNG of the
ability icon.

Recommended order (each slice can ship on its own):

1. **Library browser** — when the library root is selected, the
   viewport is a filterable grid (icon, behavior flags, range, has
   projectile). Search and template chips. “Used by” via the existing
   Character reference hook. Selecting a card jumps the Node Tree and
   inspector. Uses the center when you are not mid-edit.
2. **Card canvas** — move Events / Modifiers / Special stacks into the
   viewport. Inspector keeps envelope + the *selected* card. Same
   nested-card model, larger pane. This is the layout fix, not a new
   visual language.
3. **Stage workspace** (new tab, or selection-dependent content) —
   - **Preview** — play/scrub clip → Cast FX → projectile → hit; hear
     sounds; see attach points and pixels/unit. Custom prefabs from
     Phase 5 already exist; this is a stage, not the fight scene.
   - **Targeting** — click a hex: highlight Cast Range / AOE / team
     filter. Optional: pick SourcePos from a socket instead of typing
     `unitPosition(%SOURCE, #Head)`.
   - **Expressions** — show `CanExecute` / hit-chance / range evaluated
     against a mock `%CASTER` / `%TARGET`.
   - **Read-only flow** — hats as a map (OnAbilityAction → projectile
     → OnProjectileHitUnit → damage). A view of the file, not a second
     editor.
   - **Try in Sandbox** — scene jump once a test unit can hold the
     skill. Phase 8 later also embeds the same arena in the Stage hole.

### Done when

- [x] Library root uses the viewport as a browser, not a help label
- [x] Ability edit uses the viewport for the card stacks (or an equally
      large working surface); inspector is no longer the only editor
- [x] Stage can preview the authored sequence (clip / FX / projectile /
      sound) without opening a fight — **dry-run beat list only**
      (highlight + custom PNG). Full prefab / clip / projectile
      simulation is Phase 8.
- [x] Stage answers at least one non-preview question (range/AOE hexes,
      expression eval, or read-only event flow)
- [x] Fight scene is still a jump, not a dock *(Phase 6 host; Phase 8 embed is a later slice)*
- [x] Phase 3 card model unchanged (no graph / Blockly host)

Phase 6 shipped the **host** (browser, card canvas, Stage tab, uGUI hex
board). Play sequence does not instantiate a projectile prefab, play a
clip, or tick `ProjectileForceMovementComponent`. That gap is Phase 8.

---

## Phase 7 — Custom scripting

Implemented 2026-08-15 (LokrLab **0.12.34**). Lua is a real Advanced
card (`AbilityCardDescriptors` / `AbilityCardFactory`), not an opaque
KV subtree. Field `Action` is a multiline editor; save flattens
newlines to one quoted KV string (vanilla shape). Double quotes inside
the body warn — PenguinParser cannot round-trip them. Default stub:
`return function(ctx) end`.

Plugin-registered action surface is still
`AbilityLabAPI.RegisterActionCard` (same path the Lua card uses). A
later [extensions.md](../not-started/extensions.md) scripting plugin
can register more cards; it is not this phase.

This phase does not block Phase 8. A Lua card is authoring; the viewer
is playback. They share `AbilityFileModel` / `AbilityKvIO` only.

### Done when

- [x] `Lua` is a typed Advanced card with an `Action` field
- [x] New cards seed `return function(ctx) end`
- [x] Existing vanilla `"Action" "return function(ctx) …"` files parse as typed fields
- [x] Empty Action / double-quote Action warn on validate
- [x] `AbilityLabAPI.RegisterActionCard` remains the plugin surface

---

## Phase 8 — Simulated ability viewer

**Current (2026-08-14):** Stage Play is embed-only (`StartEmbeddedFight`,
0.8.20+). The simulated / mannequin viewer notes below are historical.

Implemented 2026-08-13 (LokrAbilityLab **0.8.0**; **0.8.4** Stage uses
lab mannequins only — no Character Lab `CustomRigLoader` standees).
Research recorded
the same day; slices 8.1–8.4 shipped together. 8.5 extras
(AOE extra dummies, knockback slide, modifier-FX-only apply) stay
follow-ups.

Goal: press Play in the Stage workspace and see the **real** authored
ability in the center dock — caster clip, Cast FX on a socket, projectile
prefab steered by `ProjectileForceMovementComponent`, hit FX, and
`PlaySound` — with **stage controls** for attach points, target
distance, flip, hit/miss, caster/target dummy, and time. The mannequin
viewer shipped first. A later slice (below) hosts a real fight in the
same hole when Character Lab can start one.

Harmony patches on Ironhide types are in scope. Put them on
`LokrAbilityLab` (preview isolation) or `LokrCharacterLoader` (content
resolve). **Do not** put them on `LokrPatch` — that plugin is vanilla
resilience only and must not depend on lab/preview state.

### Why the official combat path cannot be the host

`TrackingProjectileAction.Execute` only constructs a `Projectile` and
calls `Stage.instance.AddEntity`. The view appears because the **fight
scene** has `ProjectileViewManager` listening for `EntityAddedEvent`
(`ih-original/.../ProjectileViewManager.cs`). The lab scene does not.

Worse, `Stage.instance` is a lazy **process-wide** singleton
(`Stage.cs`). `AddEntity` appends to `entities` and is only cleared when
the fight/map sets `Stage.instance = null`
(`StageControllerComponent`, `NewMapManagerComponent`). A lab
`AddEntity` would either spawn nothing (no view manager) or pollute the
singleton a later Sandbox fight reuses.

`AbilityMeleeActivity.Execute` is the real cast path: `OnAbilityStart`,
`FXManager.AddFXMegaToUnit(CastFXId)`,
`unitView.masterAnimationController.PlayAnimation(AnimationID)`, then
clip events `AbilityAction` / `AbilityEnd` → `ability.OnEvent`.
`HitAction.Execute` builds a `HitExecution` and calls `Stage.DoHit` →
`Unit.DoHit` (damage, `OnPreHit` broadcasts, HUD popups, then
`logicAlways` which is the `EffectName` FXMega).

So: **reuse the view types and the parsed `Ability` graph; do not reuse
the fight scene, `Stage.AddEntity`, or `Unit.DoHit` as-is.**

### What “simulated” means (two layers)

Ship as slices. Layer A is already useful; Layer B is the “proper”
viewer.

| Layer | What runs | What the author sees |
|-------|-----------|----------------------|
| **A — Visual playback** | Lab-owned player instantiates the same prefabs combat uses and ticks movement / clip / FXMega / MasterAudio | Flight, particles, rotation, attach sockets, sounds. Graph side effects do not run. |
| **B — Isolated graph** | `AbilityParser.ParseAbility` on the current file → `AbilityInstance` + stub `Unit`s → lab activity shim that mirrors `AbilityMeleeActivity` event timing → `ability.OnEvent("OnAbilityAction")` | Same as A, plus `TrackingProjectile` / `Hit` / `PlaySound` / `AttachEffect` / `PlayAnimation` / `KeepProjectileGoing` / `Conditional` from the **live** graph. Damage, summons, cinematics, camera, and metagame actions are fenced. |

Layer A can play from the card tree (today’s `BuildSteps`) if parse
fails. Layer B is the default once parse succeeds. A Stage toggle
“Visual only / Live graph” stays for debugging fences.

### Research findings (do not re-litigate)

**Prefabs load in the lab.** `DataHelper.LoadProjectile` /
`LoadFXMega` are `AssetBundleManager.LoadAsset("scenario", name)`.
Character Lab and `CustomFxLoader.TryCloneVanillaProjectile` already
load those bundles outside combat. Custom names hit
`CharacterAPI.ResolveProjectile` / `ResolveFxMega` (`FxPatches`).
`FXManager.AddFXMegaToUnit` uses a **private** `LoadFXMega` that throws
if `fxMegaPrefabs` was never filled. `FXManager.Preload` is idempotent
and is normally called from `StageControllerComponent.Awake` (fight
only). In the lab, `CustomFxLoader.InjectIntoFxManager` no-ops when
`FXManager` was never created; the `LoadFXMega` / `ResolveFxMega`
prefix still works **per name**. Vanilla `CastFXId` / `EffectName`
therefore need an explicit `FXManager.Instance.Preload()` (or
`Instantiate(DataHelper.LoadFXMega)` + `FXMegaController.StartFX`)
plus `CharacterAPI.RefreshCustomVisuals()`. Prefer
`AddFXMegaIndependentToPosition` / `StartFX` on a lab attach container
over `AddFXMegaToUnit` until a real `unitView` exists.

**Flight is view-side.** `ProjectileForceMovementComponent.Fire` /
`Tick` + `ProjectileViewComponent.Update` / `UpdateTransforms`.
`Projectile.Update` only ticks `turnActionController` (fight camera).
`SetProjectile` also sets `InterfaceDataRepository.instance.projectileToFollow`
(lazy singleton; safe) and calls `DestinationReached` at the end.
`DestinationReached` NREs if both `ability` and `modifier` are null
(`else this.modifier.OnEvent`). `KeepProjectileGoing` has **no KV
fields** (default `%projectile`); it only sets `finishTravel = false`
and lives inside `Hit.Actions`. Custom motion knobs already exist on
`projectiles/<name>/projectile.json` (`maxSpeed`, `maxForce`,
`slowingDistance`, `forceMultiplier`, `keepTrackingTarget`,
`ignoresRotation`) — Stage should expose those as readouts, not
re-author them.

**Clips: prefer an exo standee, not `FindPrefab`.**
`CustomRigLoader.BuildFromFolder` (public on `LokrCharacterLoader`)
builds an `ExoSkeletonDataAsset` from a Character Lab `rig/` folder —
same path Character Lab’s preview uses, no Character Lab DLL.
`UnitViewManager.FindPrefab(kind)` loads a full combat prefab and
`UnitViewComponent.InitUnitView` calls
`StageControllerComponent.Instance.AddUnitView` — **NRE in the lab**.
Do not call `InitUnitView`. If a full `UnitViewComponent` is needed
later, Harmony-prefix `AddUnitView` while previewing, or only call
`SetUnit` (wires `attachPointProvider` + `unitView`) and skip Init.

`PlayAnimation(AnimationID)` on a wired view is enough to see the
clip. Frame events: `HandleAnimationEvent` → `unit.HandleViewEvent` →
`selectedActivity.HandleViewEvent`. If `selectedActivity` is null,
**AbilityAction never fires**. Character Lab’s `RigPreviewService`
plays clips and does **not** forward those events into an ability
graph — Layer A must fire `AbilityAction` / `AbilityEnd` from
`rig.json` frame times, or Layer B must set a lab activity.
`ExoSkeletonUnitAnimationController.HandleAnimationEvent` comparisons
are no-ops. Default-attack abilities also use `OnAttackStart` /
`OnAttackAction` (same timing idea).

**`unitPosition(%SOURCE, #Chest)` needs a `Unit`.**
`FunctionUnitPositionExpression.GetObject` casts to `Unit` and calls
`GetAttachPoint`, which uses `attachPointProvider`. Vanilla sockets
combat looks up: `Head`, `Chest`, `Base`. Ability Lab’s FX picker
already lists `Chest`, `Base`, `Head`, `CastPoint`, `RayPoint`
(`AbilityEditorSprites.FxAttachPoints`) — Stage overrides should use
that list, not Character Lab’s `CombatPlaybackRequirements`. KV
expressions use `#Chest` / `#CastPoint`; `fx.json` uses the bare
socket (`Chest`). Missed socket → `(0,0)` plus a log.

**Do not use `new Unit(position, flip, kind, …)`.** That ctor loads
`defaultSkill` from `AbilitiesDefinitions`, localizes, and walks the
definition’s skill list. Use `new Unit()` (empty ctor) and set public
fields: `position`, `isFlipped`, `kind`, `unitGroup`, `stats`,
`states`, then `SetUnit` on an instantiated view. `stats` / `states`
are already constructed on the instance.

**Parse the current editor model, not only disk.**
`AbilityParser.ParseAbility(KeyValue)` is the same parser combat uses
(`AbilitiesDefinitions.Load`). `AbilityKvIO.TrySave` already emits
legal KV; add `ToText(model)` (or parse a temp string) so **unsaved**
card edits preview. `new AbilityInstance(ability, caster)` needs that
parsed `Ability` plus the stub caster (`CASTER` / `SOURCE` /
`ABILITY` are set on `instanceContext`). `AbilityContext` is cheap
(`new AbilityContext()` / `SetObject("TARGET", unit)`).
`AbilityInstance.ChanceToHit` reads `Stage.instance.units` — do not
call it in preview. `Unit.Move` writes `Stage.instance.board`
passability — fence `MoveUnit` / knockback until 8.5. Keep
`Stage.instance.isFighting` false if anything touches the singleton.

**Hex math already exists.** `new HexBoard(size, offset, width, height)`
fills `hexGridItem.data.position` via `HexToCenter` (pointy layout).
Fight testbed offset in `FXTestbedController.Board` is
`(0.55, -0.33275)` — use a documented constant so C→T distance matches
combat pixels. Today’s Stage board is **uGUI Images**; a world projectile
will not cross those cells. Rebuild hexes in world space under the
preview camera (or keep the uGUI board as a schematic *and* add a
separate 3D pane — worse UX). Click-to-place T uses camera
`ScreenToWorld`, same idea as Character Lab’s `ViewportCameraBinder`.

**Sounds.** `PlaySoundAction` calls `unit.PlaySound` or
`MasterAudio.PlaySound(name, …)`. Mixer groups may be unloaded in the
lab; treat missing voices as a status line, not a throw. Mute is a
stage control. FXMega create-events can also fire MasterAudio
(`FXMegaComponentAction.soundId`) — those play if the prefab is
instantiated for real.

**Character Lab already hosts a world camera in the dock.**
`ViewportCameraBinder` + `RigEditorScene` preview camera + see-through
viewport panel (`LokrLab.LabShell.MakeSeeThrough`). `LokrLabApi.LabHost`
already exposes `LabScene`, `BackdropCamera`, and `Canvas` — spawn the
preview root there. Ability Lab **must not** reference
`LokrCharacterLab.dll`. Options: (1) lift a generic `LabViewportCamera`
into `LokrLab` and expose a bind call on `LokrLabApi`; (2) duplicate
the binder in Ability Lab (~one MonoBehaviour). Use a dedicated
culling layer (Character Lab uses layer 31) so the Stage camera does
not pick up map/fight leftovers. Do not use `Time.timeScale` for
slow-mo (global). Tick movement with a lab delta.

**Game-owned FX testbed is not reusable.** `FXTestbedController` is an
editor test scene (`Assets/Scenes/TestScenes/FXTestbed`), not a runtime
API.

### Isolation patches (Layer B)

Gate every prefix on a lab flag (`AbilityLabPreview.IsPlaying`) so
Sandbox fights are untouched. Tear the flag down in `finally` / Stop /
workspace deactivate.

| Target | Action when preview | Why |
|--------|---------------------|-----|
| `TrackingProjectileAction.Execute` | Instantiate `DataHelper.LoadProjectile(Model)`, `SetProjectile` on a **lab** `Projectile` (ability = preview instance), parent under the preview root. **Do not** `Stage.AddEntity`. | Official path has no view manager in lab and dirties `Stage`. |
| `Stage.AddEntity` / `AddUnit` | No-op + log | Belt and braces if some action still calls it. |
| `StageControllerComponent.AddUnitView` | No-op while previewing | `InitUnitView` NREs in the lab; only needed if a full unit prefab is spawned. |
| `Projectile.DestinationReached` | Fire `ability.OnEvent` on the preview instance only; if ability/modifier null, set `finishTravel` / `remove` and return | Avoids NRE; keeps `OnProjectileHitUnit` / `DestinationReached` / `Missed`. |
| `Projectile.Update` | Already prefixed in Loader when `view` is incomplete. In preview, skip `turnActionController` | Fight camera follow. |
| `HitAction.Execute` or `Stage.DoHit` / `Unit.DoHit` | Run `logicAlways` (EffectName FXMega) + optional `logicAfterConnect` **without** `ProcessHitDamages`, `BroadcastEvent`, `PopupManager`, `CombatLog` | Hit FX and nested visual actions; no HP / HUD / global OnPreHit. |
| `SpawnUnit`, `KillUnit`, `CameraControl`, `QueueCinematic`, `AchievementIncrement`, `ModifyHexPassable`, `AreaControl`, `InterruptUnit`, `TriggerSkill`, `QueueAttackUnit`, `DebugGolemize`, `StartLogicTick`, `DestroyGO` | No-op; status line names the skipped type | Combat / metagame / other units. |
| `Knockback`, `MoveUnit` | No-op in 8.1–8.4; optional visual slide in 8.5 | Needs hex occupancy. |
| `ApplyModifier` / `RemoveModifier` | Play `ModifierFXName` / skip duration logic | Buff FX without stacking on a real unit. |
| `CallFunction` / `Lua` | Allow if the helper is pure; fence `encounterHelper` / cinematic globals | Phase 7 cards must not crash the viewer. |
| `CombatLog.instance.*` | No-op | `AbilityMeleeActivity` reports ability used. |
| `Events.Raise(UnitStartedActivity / UnitEndedActivity)` | Swallow or raise only if no fight listeners | Avoid stray HUD. |

`LokrPatch` stays out of this table. Loader patches that already exist
(`DataHelper.LoadProjectile`, `FXManager.LoadFXMega`, incomplete
`Projectile.Update`) stay in the Loader.

### Configurable stage (viewer-only — not written to `ability.txt`)

Persist under the library or a per-user lab prefs file if we want
remembered dummies; never mutate the ability KV from these controls.

**Board**

- Target hex (click) — already exists; drive **world** hexes
- Hex radius / show Cast Range / AOE (literal numbers; expressions use
  the same 1-hex stand-in as Phase 6 unless Layer B evaluates them)
- Optional extra dummies on AOE hexes (for `ActOnTargets` visuals)

**Units**

- Caster: “used by” Character Lab unit (folder via `AbilityUsage`,
  `Model` / `metaExo` strings only — no Character Lab DLL), or a vanilla
  `units` prefab kind, or a dummy with `AttachPointContainerSimple`
- Target: same, or a point-only marker for `POINT_TARGET`
- Flip caster / target (`Unit.isFlipped` mirrors attach X)
- `SELF_TARGET`: hide the target dummy

**Sockets and aim**

- Source attach override (`Head` / `Chest` / `Base` / `CastPoint` /
  `RayPoint` / typed) — replaces `unitPosition(%SOURCE, #…)` for this
  play
- Target attach override — same for `%TARGET` / card `TargetAttach`
- Show socket gizmos
- `DistanceToTravel` override (world units; apply
  `ApplyGamePerspective` the same way `TrackingProjectileAction` does)
- `InitialVelocity` override
- Force hit / force miss (`WillHit`)

**Playback**

- Play / Stop / Pause (lab delta, not `Time.timeScale`)
- Speed (0.25×–2×)
- Mute MasterAudio
- Start at `OnAbilityStart` vs skip to `AbilityAction` (for
  `NOANIMATION` / missing clip)
- Visual only / Live graph
- Status line: current event, skipped fenced actions, parse errors

**Open character…** stays a Sandbox **scene jump** for the full debug
panel / take-over-AI. Stage Play now prefers an embedded fight (see
Phase 8 embed slice below).

### Host and plugin ownership

| Piece | Owner | Notes |
|-------|--------|--------|
| Stage UI, lab player, isolation Harmony, stage prefs | `LokrAbilityLab` | Replaces `AbilityStagePlayer`’s 0.85s beat loop |
| `ToText` on `AbilityKvIO` | `LokrAbilityLab` | Preview unsaved cards |
| `FXManager.Preload` + custom resolve | `LokrCharacterLoader` (already) | Viewer calls Preload / `RefreshCustomVisuals` |
| Public `TryLoadUnitPreview(kind or unitId)` if exo swap is awkward | `LokrCharacterLoader` `CharacterAPI` | `ResolveExoSkeleton` is currently `internal`. Prefer `CustomRigLoader.BuildFromFolder` for a Character Lab caster. |
| Camera-rect binder in the center dock | `LokrLab` (+ optional `LokrLabApi` bind delegate) or a copy in Ability Lab | No `LokrCharacterLab.dll` |
| Vanilla bugfixes | `LokrPatch` | Not preview isolation |

Constraints unchanged: SimpleUI, no node graph, no
Character Lab / LokrLab implementation DLL reference from Ability Lab
(`LokrLabApi` only — fight start/stop is a Host contract). Overlay
fallback has no dock camera — Play there stays a status line or is hidden.

### Recommended slices

Each slice can ship on its own. Do not start Layer B until A plays a
vanilla `SimpleArrowProjectile` / `FireballProjectile` across world
hexes without touching `Stage.instance.entities`.

1. **8.1 Host** — Preview camera bound to the Stage pane; world hex
   board (`HexBoard` + click); stage config strip (distance, flip,
   attach combos). uGUI hex Images go away or become a debug overlay.
2. **8.2 Visual playback** — Caster standee via
   `CustomRigLoader.BuildFromFolder` (or a dummy); play `AnimationID`;
   fire `AbilityAction` / `AbilityEnd` from `rig.json` frame times
   (preview service does not); `LoadFXMega(CastFXId)` on caster socket;
   instantiate `Model`, tick force movement C→T; `LoadFXMega(EffectName)`
   on target; `MasterAudio.PlaySound` for Play Sound cards. Drive from
   the card tree + envelope, **not** `OnEvent`. `NOANIMATION` fires
   immediately. Do not call `InitUnitView`.
3. **8.3 Stub units + overrides** — Empty `Unit()` + `SetUnit` only
   (no `InitUnitView`); attach-point gizmos; Source/Target attach and
   distance overrides feed the visual player. “Used by” character:
   `AbilityUsage` folder → `CustomRigLoader.BuildFromFolder`. Full
   `units` prefab only if a later slice needs combat attach wiring.
4. **8.4 Isolated graph** — `AbilityKvIO.ToText` → `ParseAbility` →
   `AbilityInstance`; lab activity shim (copy of
   `AbilityMeleeActivity.Execute` / `HandleViewEvent` minus AP,
   CombatLog, `Stage.turnActionController`); isolation Harmony table
   above. Default Play path.
5. **8.5 Extras** — AOE extra dummies; miss path; `KeepProjectileGoing`;
   modifier FX; `PlayAnimation` / `AttachEffect` cards; optional
   Knockback slide; expression eval of literal-or-simple Cast Range
   against stub stats.

### Done when

- [x] Stage Play shows the named projectile prefab flying C→T with the
      real movement component (vanilla and custom `projectiles/<name>/`)
      — `DataHelper.LoadProjectile` + `ProjectileViewComponent.SetProjectile`
- [x] Cast FX and Hit `EffectName` instantiate the named FXMega (or
      custom `fx/<name>/`) on the chosen sockets
- [x] Caster `AnimationID` plays on a standee when that clip exists
      (`CustomRigLoader.BuildFromFolder`); `AbilityAction` / `AbilityEnd`
      time the live graph; `NOANIMATION` fires immediately
- [x] Attach points, target hex, flip, force hit/miss, mute, and
      playback speed are Stage controls and do not write `ability.txt`
- [x] Live graph path parses the **current** (including unsaved) model
      via `AbilityKvIO.TryBuildText` and runs `OnAbilityAction` /
      projectile / hit events; fenced actions are listed, not thrown
- [x] `Stage.AddEntity` is swallowed while previewing; projectiles never
      enter `Stage.instance.entities`
- [x] Fight scene is still a jump, not a dock *(superseded by the embed slice below; mannequin Play removed in 0.8.20)*
- [x] No `LokrCharacterLab.dll` / `LokrLab.dll` reference; no
      `LokrPatch` preview patches (`Patches/AbilityLabPreviewPatches.cs`)
- [x] Overlay fallback does not host Stage (shell workspace only)
- [x] Phase 3 card model unchanged

### Phase 8 embed — real fight in the Stage hole

Implemented 2026-08-13 (first slice LokrLabApi **1.4.4**, LokrCharacterLab
**0.9.23**, LokrAbilityLab **0.8.9**). Generic scene embed moved to
LokrLab in **1.5.0 / 0.11.0** (Character Lab **0.9.32**, Ability Lab
**0.8.18**). This is a new slice, not 8.5 cosmetics.

Session 1 (research): additive `AssetBundleManager.LoadScene("scenes",
fight, true)` of `fighttesterempty` / `KRLegendsFightGameplay02`, then
bind the fight gameplay camera’s `Camera.rect` to a sized hole.
Fight Overlay HUD canvases are remapped each LateUpdate
(`EmbeddedSceneHudFitter` in LokrLab) to Screen Space Camera on that
camera and scaled with `ConstantPixelSize` to the last hole rect.
The fitter only writes canvas properties when they changed (assigning
`renderMode` every frame reset `Camera.rect` to the full screen). The
hole binder writes `Camera.rect` last. Extra EventSystems / cameras /
AudioListeners in the fight scene are disabled. Render-to-texture was
not needed. `TransitionSceneComponent` is never used (that path is
Single and would unload the lab). Stage hides mannequin sidebar chrome
while the fight is live.

Session 2 (contract): `LabHost.StartEmbeddedScene` (LokrLab) plus
`StartEmbeddedFight` / `StopEmbeddedFight` / `IsEmbeddedFightActive`
(Character Lab). Character Lab implements fight via the same
ephemeral-quest + `SandboxRoster` spawn as Sandbox (`BanditRaider`
default enemy), then calls `StartEmbeddedScene`. Ability Lab Play
passes the `StageHole` `RectTransform` and does **not** set
`AbilityLabPreview.IsPlaying` while the fight is live (isolation fences
would swallow `SpawnUnit` / hits). Stop unloads the additive scene and
sets `Stage.instance = null` so a later Sandbox jump is not poisoned.
Fight-end does **not** call `ReopenAfterFight`. Embedded fights pan by
right/middle-drag inside the hole only (no screen-edge scroll).

**Will:** real `Unit` / `UnitView`, real `SpawnUnit` (mines, summons),
real hits and occupancy, combat sounds that already exist in the fight
scene.

**Will not yet:** auto-cast the selected ability on Play; keep the
mannequin hex overlay as the combat board; author particle FXMega.
Opening the lab from the main menu now starts the fight anyway: unit
definitions are loaded on Play, used-by is scanned from Character
folders, and `MetagameManager.instance` loads the last/default save
slot if none is selected (same `Load()` the fight scene would do).
Take-over-AI is on for both Sandbox and the embedded Stage fight
(`SandboxFightControls`). Embedded fights pan by right/middle-drag
inside the hole. Sandbox uses the same embed (no scene jump). The fight
HUD in the dock may still hide some debug-panel chrome.

**Fallback:** if start fails (no save, no used-by Character, no Host
implementation, load error), Play reports the error and does not spawn
standees. The 0.8.8 mannequin path was removed in 0.8.20.
`Open character…` still jumps to Sandbox.

### Out of scope for Phase 8

- Authoring particle FXMega (still an external AssetBundle)
- Character Lab writing `ability.txt`
- A second node-graph editor
- Making Play sequence’s beat list the long-term viewer (delete or
  demote it once 8.2 ships)

---

## Phase 9 — Context-aware pickers

**Status:** Done (LokrLab **0.12.34**).

[AbilityPickerRules.cs](../../../LokrLab/Ability/Editor/AbilityPickerRules.cs)
is the hand-edited allow-list. `AbilityCatalogLookups` still dumps the
full vanilla lists from `AbilityPickerCatalog`; UnitRef / unit-arg /
unit-snippet pickers filter to core tokens (`%TARGET`, `%CASTER`, …)
plus a small per-field extra set (Knockback `Center` →
`%knockbackCenter`). A loaded value that is not on the list stays
visible so a boss token is not stripped.

The HTML
[ability-rules.html](../../api/character-reference/ability-rules.html)
stays the human read; this table is the filter the picker actually uses.

### Done when

- [x] Hit `Target` no longer offers `%drainSource` / `%eye` / hex dumps
- [x] Current value is kept even when it is off the allow-list
- [x] Unity-free rules linked into `LokrModding.Tests`

---

## Phase 10 — Hover info box

**Status:** Done (LokrLab **0.12.34**).

Shared Lab chrome (`LabHoverInfo` above the status bar). Hovered field,
card, or event hat shows a short description. For a value like
`%CASTER`, the strip shows both the field (`Target` on
TrackingProjectile) and the token.

Copy lives in `LokrLab/Sidecars/ability-hover.md` (deployed next to the
DLL) plus an optional overlay at `Mods/LokrLab/ability-hover.md`.
Compiled fallbacks in `AbilityHoverCopy` keep tests and a missing file
working. Overlay write-time is rechecked on hover so the overlay can
be edited without a rebuild.

### Done when

- [x] Hover strip in the shell (Ability fields / cards / hats bound)
- [x] Token copy appended for `%` / `#` current values
- [x] Editable markdown sidecar, not compiled-only strings

---

## Out of scope for this track

- Encounter Creator —
  [encounter-creator.md](../started/encounter-creator.md). Do not
  start from this track.
- Changing the runtime loader or
  `Mods/LokrAbilityLab/LokrAbilityLab/<libraryId>/` layout until research
  says we must
- Character Lab writing `ability.txt`
- Implementing custom FX/animation **assets** before Phase 2 finishes
  (research and cataloging them is in scope)

---

## Related docs

- [ability-lab.md](ability-lab.md) — v1 (complete)
- [editor-design.md](../../../LokrLab/docs/ability/editor-design.md) — Phase 3 pick
- [abilities.html](../../api/character-reference/abilities.html) — schema
- [ability-rules.html](../../api/character-reference/ability-rules.html) — Phase 2 rules
- [ability-vfx-animation.html](../../api/character-reference/ability-vfx-animation.html) — Phase 2 VFX / clips
- [appendices.html](../../api/character-reference/appendices.html) — enumerations
- [capabilities-and-gaps.md](../../capabilities-and-gaps.md) §2.3 — VFX / new-animation ceiling
- [docs/reference/README.md](../../reference/README.md) — AssetStudioModCLI
- [extensions.md](../not-started/extensions.md) — later scripting plugin; overhaul Phase 7 (Lua card) is done
- Phase 8 embedded Stage fight (this file) — Stage playback; does not wait on Phase 7
- Phase 9 context-aware pickers (this file) — filtered lists from AbilityPickerRules
- Phase 10 hover info box (this file) — shared Lab chrome; sidecar copy
