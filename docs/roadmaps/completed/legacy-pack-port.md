# Legacy pack port (DNSpy / Official Pack → Lab)

**Status:** Complete — confirmed in-game 2026-08-15  
**Raised:** 2026-08-14  
**Last updated:** 2026-08-15  
**Owner:** LokrLab (`LegacyPackScan`, `LegacyModImporter`, `CharacterImporter`, import UI)

Turn an old-system mod folder — the DNSpy `Ironhide.Legends.dll` +
`Mods/<name>/` layout — into Lab-owned Character and Ability projects
without hand-copying files. Grounded in
[`../../../lokr-modding/Official Pack`](../../../lokr-modding/Official%20Pack)
(16 heroes + `Resources` / `Empty Units` / `new_heroes_lib`) and Onagro
(`Downloads/Onagro`).

This is **not** a redo of [full-port/](full-port/README.md).
That track closed authoring gaps (stats, levels, Ability Lab, entity
types). This track is the **importer**: scan, ask, then write Lab
folders — including the base-game exoskeleton plus the pack's reskin.

See also [roadmaps/README.md](../README.md) and
[`../../mods-folder-structure.md`](../../mods-folder-structure.md).

---

## Why this track exists

`LegacyModImporter` used to be a one-shot, no-picker convert:

- Took the **first** `RLHeroes/*.txt` only (audit C-10).
- Created one hero immediately via `OnCreateCharacterConfirmed`.
- Split every `NewAbilities` / `EnemiesDefinitions` top-level KV block
  into its own folder — **all of them, no ask**.
- Copied sounds. Skipped portraits, ability icons, and the rig.
- Post-import modal: "rebuild the rig in the Animator; set portraits by
  hand."

The Official Pack and Onagro are mostly **reskins of a shipped
exoskeleton**, not from-scratch custom rigs. 0.12.13 combines
`CharacterImporter` (vanilla exo → parts) with the pack
`Exoskeletons/<textureName>.png` so an Official Pack hero can open in
the Animator after Import.

A second Official Pack shape is **several entities in one txt**.
Onagro's `EnemiesDefinitions/onagro_props.txt` is the example: `OnagroMine`
and `SulfurBomb` share one file. The selection sheet lists each block.

---

## Implementation (0.12.13)

- `LegacyPackScan` — scan-only; pack root vs single folder; rank chains
  vs sibling blocks; parse errors as rows.
- `LegacyModImportPanel` — Select all / none / per-row; ability library
  combo (existing library or a typed name that creates one);
  unchecked-summon warning; Confirm writes only checked rows.
- `CharacterImporter.ImportInto` — reconstruct into the new character
  folder; optional reskin PNG as the atlas.
- Portraits, ability icons, and localization copy on import.
- Appearance **Combat prefab (Model)** label. Field is not removed.
- 0.12.15: **File → Import Legacy Pack** is always on the Project Browser
  File menu and toolbar. The folder picker starts in `Mods/`.
- 0.12.24: abilities mint `slug_token` folders (not leftover pack ids).
  Character `defaultSkill` / skills / `skillProgression` write
  `$assassin_lethal_strike`; each imported folder's `aliases.json` maps
  that leftover key to the minted id. Sandbox reload uses
  `ReloadScope.All` so those abilities exist without a game restart.

Confirmed in-game 2026-08-15. This doc lives in `completed/`.

---

## Old-system layout (Official Pack)

Each pack entry is a folder under `Official Pack/Mods/<Name>/`. The old
DLL only scanned these conventional subfolders (see
[`../../../lokr-modding/docs/content-systems.md`](../../../lokr-modding/docs/content-systems.md)):

| Folder | What it is | Lab destination |
|---|---|---|
| `RLHeroes/*.txt` | Hero/companion KV. One file may be a 3-block rank chain (`InheritsFrom` / `nextLevelArchetype`). `Empty Units` has **three files** in one folder. | One Character project per **hero**, not per block. Rank chain → `CharacterProfile.Levels`. Extra files → picker. |
| `HeroRoster/` | `legend_*` / `companion_*` JSON fragments (`locked`, `unlockAchievement`). | `roster.json` + profile Tier / Locked / UnlockAchievement. |
| `NewAbilities/*.txt` | Ability KV. One file may hold several top-level ability blocks (and nested modifiers). | Ability Lab library: one folder per **selected** ability id. |
| `EnemiesDefinitions/*.txt` | Summons / props. Onagro: `OnagroMine` + `SulfurBomb` in one file. | One `EnemySummon` Character per **selected** block. Rewrite `SpawnUnit` `UnitName`. |
| `Exoskeletons/<name>.png` | Atlas named after the Model / combat prefab (Musketeer: `BanditArcher.png`), not necessarily `MetaExo`. Pack: Arcane Archer, Assassin, General, Musketeer (4 of 16). | Reconstruct that Model's exo, then crop this PNG. Set `MetaExo` to the new character id. |
| `Portraits/<heroId>/*_SLOT.png` | Six flat slots. | `LokrCharacterLab/<id>/portraits/`. |
| `AbilityIcons/<name>.png` | Flat icons. | `<abilityId>/icons/`. |
| `Sounds/<unitId>/` | WAVs. | `sounds/`. |
| `Localization/*_<locale>.txt` | `UNIT_*` / `SKILL_*` lines. | Character + ability localization files. |
| `Resources/`, `new_heroes_lib/Lua` | Shared banners / map scripts. | Out of scope here (shared-resources + Encounter / Adventures). |

---

## Model: combat prefab, not the mesh

`UnitViewManager.FindPrefab(unit.kind)` instantiates a vanilla
`units`-bundle prefab. That prefab's controllers decide combat clip
names (`CombatSequenceNames.ForModel`). The mesh is `MetaExo` / the Lab
rig, swapped onto the prefab. Do not remove the field. New characters
default `HumanArcher`. Import copies the old Model.

---

## Out of scope

- Replacing or shipping `Ironhide.Legends.dll`.
- `Resources/` shared banners / `DEFAULT_*.png`.
- `new_heroes_lib/Lua` tavern / quest scripts.
- Perfect reconstruction of a from-scratch spritesheet that is not a
  vanilla atlas (Onagro). Reconstruct + warn is enough.
- Treating a rank-up chain as multiple characters.

---

## Acceptance (in-game)

Confirmed 2026-08-15 against Official Pack (Arcane Archer / Musketeer
exo+reskin, pack-root subset, Empty Units, Assassin rank chain, icons
and portraits) and Onagro (two summon rows from one file, Model
`ObeliskLvl4`).
