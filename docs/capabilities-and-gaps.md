# Capabilities & Gaps Assessment

**Goal being assessed against:** add *complete* characters — stats, sprites,
abilities, and animations — with proper integration into the game.

**Bottom line up front:** the current system (`LokrModAPI` +
`LokrCharacterLoader` + editor plugins) is a working content platform for
re-skinning, re-stating, and **authoring custom rigs/animations** via
Character Lab + `CustomRigLoader`. The installed community pack still mostly
reuses base-game skeleton names (§2.1), so it reads like "re-skin only" in
practice — but the engine path for genuinely new rigs now ships. Separately,
unlocking the Encyclopedia button (`LokrEncyclopedia`) turns on a button with
**no confirmed backing feature** in the base game at all (§3).

## 1. What works today

Confirmed working (built, deployed, and smoke-tested against the
installed 18-mod community pack — see the BepInEx migration docs):

| Content type | Mechanism | Notes |
|---|---|---|
| Unit stats (heroes, companions, enemies) | `CharacterAPI.BuildingUnitDefinitions` → `UnityDefinitionsParser` | Each fragment is appended as its own wrapped `units` asset; duplicate IDs override rather than crash |
| Hero roster entry (selectable in Hero Room / Guild) | `CharacterAPI.BuildingHeroRoster` → `HeroRosterManager` | Splices into the roster JSON's `legends`/`companions` arrays |
| Abilities | `CharacterAPI.BuildingAbilities` + `RegisterAbility` → `AbilitiesDefinitions` | KV-text files or direct `Ability` object registration; duplicate ability/modifier IDs override |
| Portraits — 6 UI slots (`MINI`/`BIG`/`BANNER`/`MAP`/`MAPMINI`/`CHALLENGE`) | `CharacterAPI.RegisterPortraitResolver` | Static PNG per slot; MAP/MAPMINI/CHALLENGE work by **tearing down the rigged skeleton and substituting a flat `Image`** — see §2.2, this is not the same as a real animated portrait |
| Ability icons | `CharacterAPI.RegisterAbilityIconResolver` | Static PNG |
| Body-texture re-skin (world + UI exoskeleton rendering) | `ModAPI.Files`/`Assets` directly (`ExoSkeletonRendererPatches`/`ExoSkeletonUIGraphicPatches`) | Replaces the **existing** rig's spritesheet texture; does not touch skeleton geometry or animation data (§2.1) |
| Sounds (combat events, promote, select-hero) | `CharacterAPI.RegisterSoundResolver` | WAV, PCM only, randomized among matches |
| Localization | `CharacterAPI.ContributingLocalization` | Per-language KV files; JA-hardcoding bug fixed in the last pass |
| Lua scripts (quests/encounters/loot tables) | `CharacterAPI.ResolvingScript` | Full script override by name |
| Assassin-style state-tied visual effect | `CharacterAPI.RegisterStateVisualEffect` | Generic hook now, but only one example subscriber exists |
| Global mod config | `ModAPI.Config` (BepInEx `ConfigFile`) | `DebugMode`/`SkipSplashScreen`/`TakeOverAI` |

This covers everything the current 18-mod community pack actually uses —
confirmed by grepping every mod folder's content against the resolver/event
list above; nothing in the installed pack references a content type this
system doesn't already handle.

## 2. Where it falls short of "complete characters... sprites... animations"

### 2.1 Sprites & animations: community pack re-skins; custom rigs now ship

**The path that now ships:** `LokrCharacterLoader.CustomRigLoader` builds rigs from
`Mods/*/Characters/<id>/rig/rig.json` + part PNGs at runtime (via
`ExoSkeletonDataAsset.ReloadData`). Character Lab exports this format; heroes reference
custom rig ids in `MetaExo`. See [`../LokrCharacterLoader/docs/custom-rig-loader.md`](../LokrCharacterLoader/docs/custom-rig-loader.md)
and [`roadmaps/README.md`](roadmaps/README.md).

**Legacy community-pack pattern:** every hero in the official pack still points
`MetaExo` at an **existing base-game skeleton asset name** (re-skin only):

```
$ grep -rih "metaexo" Mods/*/RLHeroes/*.txt
"MetaExo" "ExoSkeletonHumanRanger_MetaDataAsset"
"MetaExo" "ExoSkeletonHumanArcaneMage_MetaDataAsset"
"MetaExo" "ExoSkeletonHumanSylvanElf_MetaDataAsset"
"MetaExo" "ExoSkeletonHumanCleric_MetaDataAsset"
"MetaExo" "ExoSkeletonOrcCleaver_MetaDataAsset"
"MetaExo" "ExoSkeletonBraveBark_MetaDataAsset"
"MetaExo" "ExoSkeletonHumanGeraldLightSeeker_MetaDataAsset"
"MetaExo" "ExoSkeletonHumanDwarfFemale_MetaDataAsset"
"MetaExo" "ExoSkeletonHumanAsra_MetaDataAsset"
"MetaExo" "ExoSkeletonRenegadeOrcShaman_MetaDataAsset"
"MetaExo" "ExoSkeletonHumanFemaleKnight_MetaDataAsset"
"MetaExo" "ExoSkeletonHumanBarbarian_MetaDataAsset"
```

These are all names that already ship in the base game's `"units"` asset
bundle. **No mod in the pack defines a new skeleton** — a content-author
choice today, not an engine block (`CustomRigLoader` supports new rigs when
`MetaExo` matches a `Characters/<id>/` folder). Legacy pack art is mostly
"re-skin an existing rig's single spritesheet texture"
(`Exoskeletons/<textureName>.png`, only 4 of 18 mods even use this) plus
"replace one of 6 UI portrait slots with a flat, non-animated PNG"
(destroying the rigged renderer to do it — see
`PortraitPatches.ReplaceWithFlatImage`).

**What remains hard / partial:**
- Community pack heroes still mostly use base-game `MetaExo` names (content choice, not engine block)
- Portrait MAP/MAPMINI/CHALLENGE still use flat-image workaround (§2.2)
- Full particle FXMega still asset-bundle bound; sprite FX / custom clips shipped in Phase 5 (§2.3)
- Ability Lab + full ability reload path newer than roster/rig reload — verify in-game after edits

**Why asset-bundle `MetaExo` alone wasn't enough (pre-`CustomRigLoader`):**

```csharp
// Ironhide/Legends/Model/Metagame/Heroes/Hero.cs
this._exoSkeletonDataAsset = AssetBundleManager.LoadAsset<ExoSkeletonDataAsset>("units", this.unitDefinition.metaExo);
```

`AssetBundleManager.LoadAsset<T>` only reads from pre-baked bundles — no
external override. Mods cannot add new named assets to `"units"`.

**Shipped workaround:** `CustomRigLoader` + `HeroExoSkeletonPatches` +
`CharacterAPI.RegisterExoSkeletonResolver` build rigs from
`Mods/*/Characters/<RigId>/rig/rig.json` + part PNGs via
`ExoSkeletonDataAsset.ReloadData`. See
[`custom-rig-loader.md`](../LokrCharacterLoader/docs/custom-rig-loader.md).

### 2.2 Portraits are a workaround, not real art support

Worth naming explicitly: `MAP`/`MAPMINI`/`CHALLENGE` portraits work by
**destroying** `ExoSkeletonUIGraphic`/`ExoSkeletonData` and substituting a
flat `Image`. This means a modded hero's portrait in those three contexts
is a static image — no idle animation, no breathing/blinking, nothing —
while every base-game hero's portrait in the same UI slot is subtly
animated via the rig. It's a reasonable, low-effort way to get *a*
recognizable custom portrait shipped, but it's a visible quality gap
against vanilla content, and it's a symptom of the same root cause as
§2.1 (no way to give a portrait its own small custom rig either).

### 2.3 Abilities: functionally solid, but bounded by the same asset ceiling

Ability *logic* (targeting, damage, conditions, status effects) is well
covered — `AbilitiesDefinitions`/KV-text/Lua together are a real, working
scripting surface, and nothing observed suggests the AI system needs
special-casing per ability (`AIBrain`/`AIConsideration`/"RunThisSkillAI"
score abilities generically off their config, not by hardcoded ID — not
exhaustively verified, but no contrary evidence found). What's *not*
covered:
- Custom ability **VFX** — Phase 5 (2026-08-13) closed the
  "reuse vanilla names or throw" gap for **sprite** FX and projectiles:
  `CharacterAPI.RegisterFxMegaResolver` / `RegisterProjectileResolver`
  plus `fx/<name>/` and `projectiles/<name>/` folders. Full particle
  FXMega still needs a Unity prefab AssetBundle; Ability Lab is not a
  particle editor.
- Custom **cast clips** — a new `AnimationID` already plays on a
  Character Lab rig if the clip exists with `AbilityAction` /
  `AbilityEnd`. Ability Lab Phase 5 lists those clip names (strings
  only) and documents the frame-event contract.

### 2.4 "Properly integrated" — confirmed gaps and unverified areas

**Encyclopedia click is vanilla Coming Soon:** unlocking the button
shows the shipped **Coming Soon!** popup. There is no Encyclopedia
window in C#. Confirmed 2026-08-15 — see
[`issues/resolved/encyclopedia-button-unverified-click.md`](issues/resolved/encyclopedia-button-unverified-click.md).

**Partially resolved, 2026-08-11 — roster card banner/map-token art is
overridable, `Background` looks like a dead field:** `Icon`/`UnitOnMap`
turned out to be unread by the base game — the real lookups
(`DataHelper.LoadHeroBanner`, `PartyTokenComponent`'s
`ExoSkeletonData.ReplacePart`) key off the hero's own id and are already
covered by `CharacterAPI.RegisterPortraitResolver`'s `"BANNER"`/
`"MAPMINI"` slots, no new code needed. `Background`
(`"hero_roster_legend_bg_forest"`) has no confirmed rendering path
anywhere in decompiled source — every mod in the pack pointing it at a
reused vanilla value may just be copying a value the game never reads.
See `docs/roadmaps/completed/full-port/gaps.md`.

**Confirmed gap — save load is hostile to custom ids:**
`SaveGameMetadata.Sanitize` discards the in-progress run when a hero
archetype, starting-hero uniqueId, quest id, inventory item, or
adventure id is unknown, and `HeroRosterManager.Load` resets a party
that is not exactly 3 known uniqueIds to Gerald/Ranger/ArcaneMage.
Tracked in
[`issues/unresolved-tested/save-sanitize-drops-unknown-ids.md`](issues/unresolved-tested/save-sanitize-drops-unknown-ids.md)
and
[`issues/unresolved-tested/save-party-reset-to-vanilla-trio.md`](issues/unresolved-tested/save-party-reset-to-vanilla-trio.md).
In-game (2026-08-15): hiding Onagro no longer discards the run, and the
id returns after restore. Slot compact into the gold legend frame is
confirmed fixed:
[`issues/resolved/party-stow-shifts-remaining-into-wrong-slots.md`](issues/resolved/party-stow-shifts-remaining-into-wrong-slots.md).

**Unverified, worth a follow-up pass rather than assumed either way:**
- **Campaign fight after Lab** — progression-help Next is confirmed
  clamped ([`issues/resolved/progression-help-popup-index-oor.md`](issues/resolved/progression-help-popup-index-oor.md)).
  Fight-node loading overlay and Lab empty-initiative are confirmed:
  [`issues/resolved/campaign-fight-loading-stuck.md`](issues/resolved/campaign-fight-loading-stuck.md),
  [`issues/resolved/fight-started-empty-initiative-nre.md`](issues/resolved/fight-started-empty-initiative-nre.md).
- **Achievements** — atlas load NREs and Steam `migration_easy_mode`
  unknown-id noise: see
  [`issues/unresolved/achievements-nre-on-atlas-load.md`](issues/unresolved/achievements-nre-on-atlas-load.md).
  Separate open question: whether the achievement system has hardcoded
  hero-ID lists that would exclude modded heroes from achievement content.
- **AI tuning quality** — confirmed AI *can* use custom abilities
  generically, not confirmed it uses them *well* (consideration curves
  are presumably tuned per-ability by the base game's own designers;
  a modded ability with no custom AI tuning gets whatever generic
  defaults apply, which may play "correctly but not well").

## 3. Suggested priority order

1. **Custom rigs already ship** (`CustomRigLoader` + Character Lab export;
   see §2.1). Remaining work is pack adoption and clip coverage, not a
   go/no-go on the engine.
2. **Full particle FXMega** is still an AssetBundle ceiling. Sprite
   FXMega / projectiles shipped in Phase 5. Roster-card `Icon` /
   `Background` are unread or unused fields, not the next loader gap.
3. Save-game hostility is confirmed (Sanitize / 3-slot party reset —
   see the issues in §2.4). Achievement integration is still unchecked.
