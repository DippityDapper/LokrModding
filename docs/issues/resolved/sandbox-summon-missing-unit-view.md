# Sandbox SpawnUnit of a Lab summon shows the purple missing-unit mesh

Area: LokrCharacterLoader (`GetDefinition`, Lab definition/ability load) and
LokrLab (generated character ids written into `SpawnUnit` `UnitName`)
Status: resolved

As of 2026-08-14: in the Onagro sandbox fight, `onagro_mine_games` should
drop the vanilla Bombardier mine. Instead the fight spawns
`UNIT-Snake-MissingUnitViewComplex` (magenta placeholder) and Unity logs
`Animation Spawn doesn't exist`. The same ability worked in the pre-Lab
mod (`Downloads/Onagro`: `SpawnUnit` `#OnagroMine`, definition block
`OnagroMine` with `Model "BombardierMine"`).

`GetDefinition` looks up the KV **block key**, then falls back to
`MissingUnitDefinition` (`Model` `MissingUnitViewComplex`) without
throwing. `CodeName` `Snake` is just the next name from the combat name
pool, not the unit type.

Legacy import rewrote the summon to folder/block key
`1529850519430193474` and the ability to `UnitName "#1529850519430193474"`.
The ability expression grammar's `word` rule must start with a letter
(`#OnagroMine` is valid; `#1529…` is not), so the spawn id never matches
the Lab definition. Combat then instantiates the missing-unit prefab,
which has no `Spawn` clip.

Related log, not this mesh: `ApplyModifier: skipped missing modifier
'modifier_onagro_mine_tracker'`. The original ability applies that
modifier and never defines it; LokrPatch skipping it is expected.

The KV **block key** must stay the folder id. Prefixing it with `c`
(0.12.4) made `EmbeddedFightHost` fail `Definitions.ContainsKey(folderId)`
and refuse to start the fight. Only `#UnitName` literals use the `c` form.

Resolved: 2026-08-14

Resolution: LokrCharacterLoader 1.1.7 / LokrLab 0.12.5. Ability load
rewrites `#1529…` to `#c1529…` so the expression parser accepts it;
`GetDefinition` resolves both spellings. Lab definition fragments are
appended as their own wrapped `units` assets. Block keys stay the folder
id (a brief `c` prefix on those keys was reverted; leftover files are
normalized; UniqueId is aliased after the UniqueId index is built).
Confirmed in game: Onagro sandbox starts, mine is BombardierMine, no
purple silhouette.
