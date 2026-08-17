# Mods Folder Structure

All mod content lives under `<GameDir>/Mods/`. Each immediate subfolder of
`Mods/` is one mod package (load order is enumeration order — first match wins
for `ModAPI.Files.TryFindFile`).

## Shared conventions (any mod folder)

| Category | Path pattern | Used for |
|---|---|---|
| `RLHeroes/` | `Mods/<Mod>/RLHeroes/*.txt` | Hero/companion unit-definition fragments |
| `EnemiesDefinitions/` | `Mods/<Mod>/EnemiesDefinitions/*.txt` | Enemy/summon definition fragments |
| `HeroRoster/` | `Mods/<Mod>/HeroRoster/*.txt` | Roster JSON splicing (`legends` / `companions`) |
| `NewAbilities/` | `Mods/<Mod>/NewAbilities/*.txt` | Ability KV-text files |
| `Portraits/` | `Mods/<Mod>/Portraits/<heroId>/<file>.png` | Legacy flat portrait layout |
| `AbilityIcons/` | `Mods/<Mod>/AbilityIcons/<name>.png` | Ability icon overrides |
| `FXMega/` | `Mods/<Mod>/FXMega/<name>/sprite.png` + `fx.json` | Custom sprite FXMega (flat, hand-authored) |
| `Projectiles/` | `Mods/<Mod>/Projectiles/<name>/sprite.png` + `projectile.json` | Custom projectile models (flat) |
| `Exoskeletons/` | `Mods/<Mod>/Exoskeletons/<textureName>.png` | Body-texture re-skin (existing rig only) |
| `Sounds/` | `Mods/<Mod>/Sounds/<unitId>/<unitId>_<event>.wav` | Legacy flat sound layout |
| `Localization/` | `Mods/<Mod>/Localization/*_<locale>.txt` | Per-language KV strings |
| `Lua/` | `Mods/<Mod>/Lua/<scriptName>.lua` | Script overrides |
| `Characters/<RigId>/` | `rig.json` + one PNG per part | Custom rigs (`CustomRigLoader`) |

Portrait and sound resolvers also check per-character nested paths under
`LokrCharacterLab/<id>/portraits/` and `LokrCharacterLab/<id>/sounds/` first
(Character Lab layout), then leftover `Characters/<id>/…`, before falling back
to the flat `Portraits/` / `Sounds/` tables above.

## Character Lab export layout

Character Lab writes new characters under
`Mods/LokrLab/LokrCharacterLab/<characterId>/`. A boot migration
moves leftover `Mods/LokrCharacterLab/LokrCharacterLab/` and `Characters/`
trees into that folder. Loader scans use the category name `LokrCharacterLab`
so they do not collide with a generic `Characters/` folder from another mod.
Leftover `Characters/<id>/` folders that still have `character.json` or
`project.json` also load.

```
Mods/LokrLab/
├── LokrCharacterLab/
│   └── <characterId>/
│       ├── project.json                (universal marker: projectType + schemaVersion)
│       ├── character.json              (profile metadata)
│       ├── aliases.json                (short name → unique id; loaders expand $alias)
│       ├── definition/
│       │   └── rlheroes.txt            (unit stats / MetaExo / skills)
│       ├── roster.json                 (hero-room entry; omitted for EnemySummon)
│       ├── localization_<locale>.txt
│       ├── rig/
│       │   ├── rig.json
│       │   └── *.png                   (one per part name in rig.json)
│       ├── portraits/                  (MINI, BIG, BANNER, …)
│       └── sounds/                     (combat / select / promote WAVs)
└── EditorData/                         (recent files, editor-only)
```

`<characterId>` is the folder name and doubles as `UniqueId` / `MetaExo` /
the KV block key. New Lab creates use `slug` plus a 6-character token
(`necromancer_ad8174`). Leftover 18-digit folders still load. Display name
stays on `character.json` / localization. Authored files may write
`$necromancer`; `aliases.json` in that same folder expands `$alias` before
the game sees the text. Abilities `SpawnUnit` the unique id
(`UnitName "$necromancer"` in the ability folder, expanded to
`#necromancer_ad8174`). Two folders with the same
`character.json` id (for example a copy named `onagro` next to the
id-named folder) are a content layout mistake, not a loader bug: the
first folder wins and later copies are skipped with a warning.
`CharacterLabContentLoader`
(in `LokrCharacterLoader`, not `LokrCharacterLab`) reads the sidecar files
above into `CharacterAPI` events; `CustomRigLoader` reads `rig/rig.json` +
PNGs when present and skips folders with no custom rig. Because both live
in `LokrCharacterLoader`, a character folder loads for a player with only
`LokrCharacterLoader` installed, under any mod folder name — not just
`Mods/LokrCharacterLab/` — since `ModAPI.Files`' category scanning is
folder-name-agnostic (see the "Ability Lab export layout" section below
for the equivalent fix that was still needed for abilities as of
2026-08-12).

## Ability Lab export layout

```
Mods/LokrLab/
└── LokrAbilityLab/
    └── <libraryId>/
        ├── project.json                (projectType + displayName)
        └── <abilityId>/
            ├── aliases.json            (short name → unique id for this ability only)
            ├── ability.txt             (KV ability definition)
            ├── icons/                  (PNGs keyed by the ability's Icon field)
            ├── fx/<name>/              (sprite.png + fx.json — custom FXMega)
            ├── projectiles/<name>/     (sprite.png + projectile.json)
            └── localization_<locale>.txt
```

`<libraryId>` is a generated folder id; the Project Browser shows `displayName`.
`<abilityId>` is the KV block key characters reference in `skillProgression` /
`defaultSkill`. New abilities use `slug_token`; leftover `new_ability` folders
still load. That ability folder's `aliases.json` expands `$alias` in
`ability.txt` (a summon lists `"zombie": "zombie_def456"` here, not on the
character). A boot migration wraps the old singleton `Abilities/<id>/` tree
into one library. `AbilityLabContentLoader` reads every library (and leftover
flat `Abilities/<id>/` folders) via `CharacterAPI.BuildingAbilities`. Nested
icons are resolved before the flat `Mods/*/AbilityIcons/` fallback.
`CustomFxLoader` (in `LokrCharacterLoader`) builds sprite FXMega /
projectile prefabs from `fx/<name>/` and `projectiles/<name>/` on each
ability folder, on the library folder itself (`<libraryId>/fx/`), leftover
`Abilities/<id>/` trees, and the flat `Mods/*/FXMega/<name>/` /
`Mods/*/Projectiles/<name>/` conventions. Ability Lab authors the
per-ability folders; combat inject lives in the Loader so a player
without Ability Lab still sees the FX.

Ability Lab copies its plugin `Placeholders/` folder into
`Mods/LokrLab/LokrAbilityLab/placeholders/` on boot (folder id is
fixed, not random; existing `ability.txt` files are left alone). New
heroes reference:

| Ability id | Used as |
|---|---|
| `placeholder_attack` | `defaultSkill` (basic attack) |
| `placeholder_skill` | `skillProgression` ranks 1, 2, and 3 (2 / 3 / 3 slots) |
| `placeholder_passive` / `_2` / `_3` | `skills` (PASSIVE traits) |

Those files are created only when missing, so edits in Ability Lab stick.
A new character also gets a one-part stub rig (`Stand`, `Portrait`,
`StandStatic`, plus HumanArcher combat clips) and solid-color portraits
so the hero room / map views have something to display.

## Encounter Lab export layout

`Mods/LokrLab/LokrEncounterLab/<slug_token>/` — `project.json` +
`encounter.json` (+ optional `aliases.json`). See
[roadmaps/started/encounter-creator.md](roadmaps/started/encounter-creator.md)
(Phase 3: type + save). No runtime loader yet; Play is Lab-only (Phase 6).

## Legacy global config

`Mods/Resources/properties.txt` — optional one-time import source for
`debug_mode`, `skip_splash_screen`, and `take_over_ai` when migrating from the
pre-BepInEx mod. BepInEx `.cfg` files are authoritative after first run.

## Debugging tips

- If content is missing, confirm the category folder name matches the table above
  (case-sensitive on Linux).
- Duplicate ids across mod folders: first mod folder in enumeration wins unless
  the patch explicitly merges/overrides (abilities, unit defs).
- After editing Lab content in-game, use **Reload in Game** or close the lab with
  `AutoReloadOnLabClose` enabled — see [`roadmaps/started/live-reload.md`](roadmaps/started/live-reload.md).
