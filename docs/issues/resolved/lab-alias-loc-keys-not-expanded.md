# Lab UNIT_$alias loc keys miss the roster name lookup

Area: LokrCharacterLoader (`LabAliases.Expand`) and LokrLab (`RLHeroesGenerator.SyncLocalizationFile`)
Status: resolved

As of 2026-08-15: a fresh save's hero roster shows Lab-imported Assassin and
Musketeer as `UNIT_ASSASSIN_Z7V9V1_NAME` / `UNIT_MUSKETEER_C3AWGR_NAME`
(and empty lore). Abilities are correct. Onagro displays "Onagro" because
its `localization_en_US.txt` still has `UNIT_onagro_0nzj37_NAME_0001`.

Lab writes `UNIT_$assassin_NAME_0001`. `LabAliases.Expand` uses a greedy
`$([A-Za-z][A-Za-z0-9_]*)` capture, so `$assassin_NAME_0001` is looked up
as alias `assassin_NAME_0001` (miss) and the key never becomes
`UNIT_assassin_z7v9v1_NAME_0001`. Roster `UnitNameKey` is
`UNIT_` + expanded uniqueId + `_NAME_` + `0001`.

The extra Musketeer card is the leftover Official Pack folder, not vanilla
— see
[`../unresolved/legacy-pack-and-lab-import-both-roster.md`](../unresolved/legacy-pack-and-lab-import-both-roster.md).

Resolved: 2026-08-15

Resolution: LokrCharacterLoader 1.1.13 longest-prefix `$alias` expand;
LokrLab 0.12.29 writes `UNIT_<uniqueId>_*` stems. Confirmed in-game: Lab
Assassin and Musketeer show proper name and lore.
