# 9.1 The old system's full content surface, and where the new system stands today


| Old-system category | New-system status | Owning workstation |
|---|---|---|
| Character ID / roster key | Works — `CharacterProfile.Id`, fixed at creation | General (§4) |
| Name / description | Works — Properties panel, English only | General (§4) |
| Locked / unlocked | Works — `Locked` toggle | General (§4) |
| **Achievement-gated unlock** (`unlockAchievement`) | **Resolved 2026-08-12** — `CharacterProfile.UnlockAchievement`, round-tripped through `character.json`/`roster.json` | General (§4) — §9.3.A |
| Legend vs. Companion tier | **Resolved 2026-08-11** — a Legend toggle on the Hero Roster category | General (§4) |
| Rig / animations | **Works fully** — the Animator is a real custom-rig editor, not a reskin tool | Animator (§5) |
| Stats block (fixed fields) | **Resolved 2026-08-11** — the Level Properties category's stat rows, per rank | General (§4) |
| **Stats block (arbitrary custom fields)** | **Resolved 2026-08-11** — Level Properties (Properties) edits real add/rename/remove-able stat data per rank, written straight into `rlheroes.txt` | General (§4) — §9.3.A |
| **Level-chain progression** (`InheritsFrom`/`nextLevelArchetype`, multi-block level-ups) | **Resolved 2026-08-11** — `CharacterProfile.Levels`/`RLHeroesParser`/`RLHeroesGenerator` model and round-trip the real multi-block archetype chain (verified against Onagro's own 3-rank file before trusting it); Level Properties' level tabs are the UI | General (§4) — §9.3.A |
| Skills / skillProgression / defaultSkill | **Resolved 2026-08-11**, as a plain-text editor (Skills category) rather than a polished picker yet — still real Lab-owned data, no raw-file editing required | General (§4) owns the reference list; `LokrAbilityLab` (§6) owns what each id actually does |
| Abilities (`NewAbilities/*.txt`) | **Migrated 2026-08-12** to `LokrAbilityLab`'s `Mods/LokrAbilityLab/Abilities/<id>/` folders (`ability.txt` + `icons/` + localization) — see §6 | `LokrAbilityLab` (§6, v1 built and verified in-game) |
| **Shared/library abilities** (one ability file used by several characters) | **Resolved by design** (2026-08-11) — `LokrAbilityLab`'s native storage is a shared, mod-wide library, not per-character; nothing to work around once it exists | `LokrAbilityLab` (§6) |
| Ability icons | **Nested 2026-08-12** under `Abilities/<id>/icons/`, with flat `Mods/*/AbilityIcons/` kept as a fallback for hand-authored mods | `LokrAbilityLab` (§6) + `PortraitPatches` |
| **Enemy/summon definitions** (`EnemiesDefinitions/*.txt`) | **Lab-authored as `EnemySummon` characters under `Characters/<generatedId>/` as of 2026-08-12** — same opaque-id rule as heroes; `SpawnUnit` `UnitName` is rewritten to the new id; hand-authored `Mods/*/EnemiesDefinitions/` still loads | General/Animator (§4/§5) |
| States (LEGEND/immunity flags/behavior flags) | **Resolved 2026-08-11** — an open-ended add/remove/toggle list (States category), the same "not a closed list" principle §3 already commits to | General (§4) — §9.3.A |
| Sound config (combat-event sounds) | **Resolved 2026-08-11**, as a plain-text editor (Sound category) rather than a polished picker yet — still real Lab-owned data, no raw-file editing required | General (§4) — §9.3.A |
| **Custom animation-triggered sounds** (footsteps, roars, idle barks) | Likely belongs on the Animator's own `events` system (already shipped, §5), **not cross-linked from anywhere** | Animator (§5) — §9.3.C |
| Portraits (6 slots) | **Resolved 2026-08-11** — `CharacterPortraitsPanel` (Properties) Browse/Clear per slot, writing straight into `Characters/<id>/portraits/`, no manual file placement. MAP/CHALLENGE additionally get an explicit "Use custom image" toggle over a flat image vs. the character's own animated exoskeleton Portrait/StandStatic pose — both already fell back to the animated pose whenever no custom file was set (the old system's own texture-reskin-only ceiling meant that fallback was never usable before), the toggle just makes the choice explicit instead of implicit-via-file-presence | General (§4) |
| **Roster card banner / map token** (`Icon`/`UnitOnMap`) | **Resolved 2026-08-11** — turned out already covered by `CharacterAPI.RegisterPortraitResolver`'s existing "BANNER" and "MAPMINI" slots (`Icon`/`UnitOnMap` themselves are unread by the base game; the real lookups key off the hero's own id, see §9.3.A) | General (§4) readiness checklist now checks for the actual files, §9.3.A |
| **Roster card background** (`Background`) | **Investigated 2026-08-11 — likely a dead field.** Exhaustive search of decompiled source found no code path that turns this value into a rendered sprite anywhere in the current build; every 16/16-Official-Pack "workaround" may be copying a value that does nothing | Open question, low priority — §9.3.A |
| **Multi-locale localization** (es/ru/pt/zh-Hans/zh-Hant/fr/ar, etc.) | **Implemented and verified in-game 2026-08-12** — see §9.3.A | General (§4) — §9.3.A |
| **`Model` field** (`unitDefinition.kind`) | **Resolved 2026-08-14** — vanilla `units`-bundle prefab combat instantiates; custom rig is swapped onto it. Appearance combo stays; new characters default `HumanArcher`. Do not remove. | Appearance — [legacy-pack-port.md](../legacy-pack-port.md) |
| **Shared, mod-wide resources** (default fallback art, shared roster banners, a global `properties.txt`) | **No concept of "content that isn't any one character's"** at all | Unowned — §9.3.D |
| Map/quest Lua scripting (tavern scenes introducing a new hero, custom encounters) | Out of scope for character creation — see §9.3.E | Encounter Creator / Custom Adventures (§8) |

