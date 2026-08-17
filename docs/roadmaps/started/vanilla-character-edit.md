# Vanilla Character Edit

**Status:** Started — Phase 2 confirmed in-game 2026-08-17; extract mints slug_token and imports the Model prefab combat exo in LokrLab 0.12.107; Close Lab always reloads loc in 0.12.109 (in-game confirm pending)  
**Raised:** 2026-08-17  
**Last updated:** 2026-08-17  
**Owner:** LokrLab Character + LokrCharacterLoader

Old-system authors change **Gerald, Asra, and other shipped heroes** in
place. Character Lab today authors **new** `slug_token` projects. This
track opens a vanilla hero in the Lab and keeps the campaign treating
them as that hero — or forks them safely.

**Load paths are ours to change.** Override-in-place changes
`UnityDefinitionsParserPatches` and `HeroRosterManagerPatches` so Lab
last-wins, the same way abilities already last-write.

Sibling tracks: [vanilla-ability-edit.md](../not-started/vanilla-ability-edit.md),
[vanilla-encounter-edit.md](../not-started/vanilla-encounter-edit.md).
See also [legacy-pack-port.md](../completed/legacy-pack-port.md),
[character-importer.md](../../../LokrLab/docs/character/character-importer.md),
[character-lab-loader-pre-redesign-audit.md](character-lab-loader-pre-redesign-audit.md)
(L-01).

---

## Already decided

| Decision | Source |
|---|---|
| Override **and** fork. “Edit Vanilla Hero…” is override. New Project stays a fork. | this doc Phase 1 (2026-08-17) |
| Loader merge is last-wins by block key / UniqueId / roster id. No required `project.json` flag for load. | Phase 1 / Phase 2 |
| `project.json` `vanillaSourceUniqueId` is for extract / UX (Phases 3–4), not a load gate. | Phase 1 |
| Override keeps vanilla block keys (`RLHumanGeraldLightSeekerLvl*`) so saves still resolve. | Phase 1 |
| Extract writes the **full** level chain (Lvl1–3), not Lvl1 only. | Phase 1 |
| Roster row is replaced whole. Extract copies vanilla `locked` / `unlockAchievement` so Gerald does not unlock by accident. | Phase 1 |
| Loader ships before save-sanitize is fully resolved. Save continue is a confirm item, not a code gate. | Phase 1 |
| Do not edit tutorial Lua, achievement definitions, or ship vanilla KV as a redistributable game dump. | Phase 1 |

---

## Why a vanilla hero is several ids

| Layer | Identifier | Example |
|---|---|---|
| UniqueId / roster id | `Gerald` | Hero room, saves, portraits, sounds |
| Unit block keys | `RLHumanGeraldLightSeekerLvl1`…`Lvl3` | Saves store archetype |
| Name / loc stem | `GERALD_LIGHTSEEKER` | `UNIT_GERALD_LIGHTSEEKER_NAME_0001` |
| MetaExo | `ExoSkeletonHumanGeraldLightSeeker_MetaDataAsset` | `units` bundle rig |
| Skills | `gerald_swing`, … | Ability ids (own track) |

Lab New Project mints a folder id and sets `MetaExo` to that folder
(`RLHeroesGenerator`). Correct for a fork; wrong for “still Gerald.”

---

## Phase 1 — Product matrix

**Status:** done 2026-08-17 (locked from the overwrite product call).

| User story | Layers | Mode |
|---|---|---|
| Reskin Gerald | Rig, portraits, sounds | Override. Mostly works today (resolvers / `CustomRigLoader`). |
| Rebalance Gerald | Unit def stats, skill list, level chain | Override. Needs Phase 2 last-wins. |
| Replace Gerald, keep saves | Same UniqueId + block keys | Override. |
| Duplicate Gerald as an alt legend | New `slug_token` | Fork. Today’s New Project. |

Non-goals: tutorial Lua, achievement defs, license-unclear vanilla KV
redistribution, a second unit-definition VM.

---

## Phase 2 — Change the Loader

**Status:** confirmed in-game 2026-08-17 (hand-written `gerald_lab_override`,
99 HP / “Gerald (Lab)”).

Merge rule (same as abilities): vanilla loads first; later Lab/mod
fragments **replace** the same key.

| Layer | Before | After (1.1.16) |
|---|---|---|
| Unit def block key | First file wins; later logs ERROR and drops | Last-wins; log Info on replace |
| UniqueId index (Lvl1) | First Lvl1 wins | Last Lvl1 wins; `definitions[UniqueId]` points at that row |
| Hero roster | Skip fragment if `id` already present | Same `id` replaces the JSON object; new ids still append |
| `UnitDefinitionLoaded` / `KnownUnitDefinitions` | Fired only on first add | Fired on add and replace |
| Live reload | Re-runs `LoadData` / `HeroRosterManager.Init` | Picks up replace automatically |

Hand-written path (no Lab picker yet): a `LokrCharacterLab/<folder>/`
with `definition/rlheroes.txt` block keys `RLHumanGeraldLightSeekerLvl*`
and `roster.json` `"id":"Gerald"` replaces shipped Gerald at boot and
on `ReloadLabContent`.

Two Lab folders with the same **folder name** (LokrCharacterLab vs leftover
Characters): `CharacterLabContentLoader` keeps the first scan. Engine
UniqueId collisions (two Gerald overrides in `gerald_ab12cd` and
`gerald_cd34ef`) both load; last-wins at the parser picks the later
fragment. Do not name override folders `gerald`.

`ContentRules.AssignLastWins`, `AssignLevel1UniqueIdLastWins`,
`MergeRosterArray` are the Unity-free rules. Tests in
`ContentRulesTests`.

---

## Phase 3 — Extract vanilla → Lab folder

**Status:** code complete in LokrLab **0.12.107**. In-game confirm pending.

`VanillaCharacterExtract` reads the live `KnownUnitDefinitions` chain
(UniqueId + `nextLevelArchetype`), roster lock fields from
`HeroRosterConfig`, and `UNIT_<NameStem>_*` loc. Writes a Character
project with `vanillaSourceUniqueId`, vanilla block keys, vanilla loc
stem, and a minted `slug_token` folder (`gerald_ab12cd`, never
`gerald`). Reconstructs the **Model prefab** exo (combat clips: Walk,
Attack, Death, …) into `rig/` + `sprites/`, not the MetaDataAsset
(that file is Stand / Portrait / Victory only — see the Ranger dump).
File → Save writes MetaExo as that folder id once `rig.json` exists.

`RLHeroesGenerator` keeps UniqueId / roster id / block keys vanilla.
`VanillaOverrideRules` is the Unity-free rule set. Sandbox spawn looks
up `SpawnUnitId` (Gerald), not the folder id.

File → Import Character remains the optional pack-reskin crop path.

---

## Phase 4 — Lab UX

**Status:** code complete in LokrLab **0.12.107**. In-game confirm pending.

File → **Edit Vanilla Hero…** lists live Hero UniqueIds. Pick opens an
existing override folder (same UniqueId / roster id /
`vanillaSourceUniqueId`) or extracts a new `LokrCharacterLab/<slug_token>/`
project. Leftover named folders under CharactersRoot (`gerald`,
`gerald_lab_override`) are renamed onto `slug_token` on open. New
Project stays a fork.

---

## Phase 5 — In-game confirm

**Status:** Phase 2 core items confirmed 2026-08-17. Extract / Save /
remaining campaign items still open.

- [x] Hand-written Lab folder with Gerald Lvl1–3 block keys: hero room
      and Sandbox use Lab stats, not vanilla
- [x] Log shows `LoadData: replaced unit definition
      'RLHumanGeraldLightSeekerLvl1'` (not ERROR duplicate)
- [x] Roster: Lab `roster.json` `"id":"Gerald"` replaces the shipped
      row (lock fields as written)
- [ ] File → Edit Vanilla Hero… extracts Asra (or another untouched
      hero) into a `slug_token` folder with a populated Animator rig
- [ ] File → Save on an override keeps vanilla block keys; MetaExo is
      the Lab folder id
- [ ] Edit Vanilla Hero on Gerald opens the existing override (renames
      leftover `gerald` / `gerald_lab_override` onto `slug_token`; no
      second engine UniqueId folder)
- [ ] Sandbox Start plays the override (lookup is UniqueId `Gerald`,
      not the folder id)
- [ ] Save with Gerald Lvl2 → edit Lvl2 stats → continue
- [ ] Change skill assignment mid-adventure
- [ ] Tutorial still requires Gerald
- [ ] Remove the Lab folder → vanilla Gerald returns
- [x] `CharacterAPI.ReloadLabContent` picks up a Description edit without
      restart ([`override-description-needs-restart.md`](../../issues/resolved/override-description-needs-restart.md))
- [ ] Fork New Project still does **not** collide with Gerald

---

## What still works without this track

| Action | Result |
|---|---|
| New Project | New companion / legend / enemy. Vanilla Gerald untouched. |
| File → Import Character… | Rig only into `LokrCharacterLab/<metaExoName>/`. |
| File → Import Legacy Pack | Official Pack mods. Always new `slug_token`. |
| Portraits / sounds / ability KV | Already override by UniqueId / last-write. |

---

## Related docs

- [Character importer](../../../LokrLab/docs/character/character-importer.md)
- [RLHeroes HTML](../../api/character-reference/rlheroes.html)
- [mods-folder-structure.md](../../mods-folder-structure.md)
- [UnityDefinitionsParserPatches](../../../LokrCharacterLoader/Patches/UnityDefinitionsParserPatches.cs)
- [HeroRosterManagerPatches](../../../LokrCharacterLoader/Patches/HeroRosterManagerPatches.cs)
- [ContentRules](../../../LokrCharacterLoader/ContentRules.cs)
- [CharacterLabContentLoader](../../../LokrCharacterLoader/CustomRigs/CharacterLabContentLoader.cs)
