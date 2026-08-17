# Official Pack folder and Lab import both appear on the roster

Area: LokrCharacterLoader (`HeroRosterManagerPatches.RegisterDefaults` +
`CharacterLabContentLoader.OnBuildingHeroRoster`)
Status: unresolved-tested

As of 2026-08-15: a fresh-save roster showed two Musketeers. Selecting
one logs unique id `Musketeer`; the other logs `musketeer_c3awgr`.
Musketeer is not a vanilla Ironhide hero.

Both copies are real and both loaders are doing what they were written
to do:

1. `Mods/Musketeer/HeroRoster/companion_musketeer.txt` — leftover Official
   Pack / old-mod layout. `HeroRosterManagerPatches` still scans
   `Mods/*/HeroRoster` (`companion_` → companion). UniqueId `Musketeer`.
2. `Mods/LokrLab/LokrCharacterLab/musketeer_c3awgr/roster.json` — Lab
   import of that pack (`importedFromLegacyMod: true`). UniqueId
   `musketeer_c3awgr` after `$musketeer` expand.

Duplicate-id skip in `ApplyRosterContributions` is exact string match, so
`Musketeer` and `musketeer_c3awgr` both splice in. Assassin only appears
once because `Mods/Assassin/` is already gone; only the Lab folder
remains. Onagro is Lab-only (`Mods/` has no Onagro pack).

Confirmed in-game: name/lore on the Lab cards is fixed
([lab-alias-loc-keys-not-expanded.md](../resolved/lab-alias-loc-keys-not-expanded.md)).
This file is the remaining double card, not a loc miss.

Suggested fix: do not disable the old `Mods/*/HeroRoster` scan (hand-authored
packs still use it). After a Lab import, warn that the source pack folder
will keep injecting a second roster row until it is removed or renamed.
Optional later: skip a `HeroRoster` fragment whose `id` matches a Lab
character's alias (case-insensitive) when that Lab character already
contributed `roster.json`. Until then, delete or move `Mods/Musketeer/`
if only the Lab copy should show.

## Unit tests (unresolved-tested)

Moved: 2026-08-15
Tests:
- LokrModding.Tests.CharacterLoader.ContentRulesTests.MergeUniqueIds_KeepsBothSources

This test covers the merge-by-id rule only. Two roster cards in the running game still need in-game confirm.
Still needs in-game confirm. A passing unit test is not a running-game
confirm.
