# LokrLab suite merge

**Status:** Complete — confirmed in-game 2026-08-15.
**Raised:** 2026-08-14
**Last updated:** 2026-08-15
**Owner:** LokrLab (suite). LokrLabApi stays the tool plugin.

Agreed shape: **LokrLabApi** stays the tool plugin (project types, Host,
jump, embed/fight delegates). **LokrLab** becomes the first-party suite
(host + Character + Ability, later Encounter / Adventure). Do not merge
Lab + LabApi. Do not merge Character and Ability into one project type.

Encounter Creator is
[started/encounter-creator.md](../started/encounter-creator.md).
This track no longer blocks it; Lab save UX is confirmed in-game, so
Encounter is unblocked. See the rest of the [phasing.md](../phasing.md)
follow-up list.

## Target layout

Today (double nest, two mod packages):

```
Mods/LokrCharacterLab/LokrCharacterLab/<characterId>/
Mods/LokrAbilityLab/LokrAbilityLab/<libraryId>/<abilityId>/
```

Target (one package, two categories):

```
Mods/LokrLab/
├── LokrCharacterLab/
│   └── <characterId>/
├── LokrAbilityLab/
│   └── <libraryId>/
│       ├── project.json
│       └── <abilityId>/
└── EditorData/
```

Category names stay `LokrCharacterLab` and `LokrAbilityLab` so
`LokrCharacterLoader` keeps scanning the same folder names across every
mod package. Write roots move to `Mods/LokrLab/…`.

Boot migration (first-folder-wins, warn on id collision):

- `Mods/LokrCharacterLab/LokrCharacterLab/<id>` → `Mods/LokrLab/LokrCharacterLab/<id>`
- leftover `Mods/LokrCharacterLab/Characters/<id>` and `Mods/LokrLab/Characters/<id>`
- `Mods/LokrAbilityLab/LokrAbilityLab/<libraryId>` → `Mods/LokrLab/LokrAbilityLab/<libraryId>`
- leftover `Mods/LokrAbilityLab/Abilities/` (wrap-into-one-library, then move)
- `Mods/LokrCharacterLab/EditorData` → `Mods/LokrLab/EditorData`
- Ability placeholders: `Mods/LokrLab/LokrAbilityLab/placeholders/` (first install; Rename ports the folder and stamps `placeholdersLibrary` on `project.json`)

Do not rename character ids, library ids, or ability ids.

## Why Lab and LabApi stay separate

- **LokrLabApi** — tooling: register project types, session, menus, Host,
  jump/return, embed/fight delegates. No Harmony, no SimpleUI.
- **LokrLab** — the suite: scene, Project Browser, docks, Character,
  Ability, later Encounter.

A third-party editor depends on LabApi (+ SimpleUI if it builds UI), not
on the suite DLL. Do not move `EmbeddedSceneHost` Harmony into LabApi.

## Phases

### Phase 0 — File the track (this doc) — done

Index from [README.md](../README.md) and [phasing.md](../phasing.md).

### Phase 1 — On-disk roots + boot migration — done

Point write roots at `Mods/LokrLab`. Loader category constants stay
unchanged so leftover unmigrated packages still load for one session.

### Phase 2 — Assembly merge — done

Move Character Lab and Ability Lab source into `LokrLab`. One
`[BepInPlugin]` (`com.lokrmodding.lab`). Overlay Ability Lab scene
stays. `EmbeddedFightHost` still assigns `LokrLabApi.StartEmbeddedFight`.

### Phase 3 — Docs and close — done (confirmed in-game 2026-08-15)

Update `CLAUDE.md`, `ARCHITECTURE.md`, plugin docs. Confirmed in-game
2026-08-15: migrated character + Ability Library under `Mods/LokrLab/`,
jump Character → ability, Stage Stop/Start, Loader-only still loads
the same folders.

## What not to do

- Do not fold LabApi into LokrLab.
- Do not put `EmbeddedSceneHost` / input Harmony in LabApi.
- Do not merge Character and Ability into one project type or one
  category folder.
- Do not move `CharacterAPI` / `CustomRigLoader` into the suite.
- Do not rename ability or character ids as part of the folder move.

## Confirm in-game (gate)

Confirmed 2026-08-15: migrated character + an Ability Library, Project
Browser lists both under `Mods/LokrLab/`, jump Character → ability
works, Stage Stop/Start still confirms, Loader-only (labs off) still
loads the same folders.
