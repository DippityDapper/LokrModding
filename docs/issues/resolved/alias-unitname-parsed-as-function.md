# SpawnUnit UnitName $alias expands to a bare id and fails to parse

Area: LokrCharacterLoader (`LabAliases` expand + `LabExpressionIds.RewriteAbilityText`) and LokrLab (Ability save / leftover-id rewrite write `$alias` for UnitName)
Status: resolved

As of 2026-08-14: opening Lab and starting an Onagro Stage fight logs

```
ERROR PARSING: onagro_mine_games, OnProjectileDestinationReached, SpawnUnit, UnitName
System.Exception: Function onagro_mine_6htjnq is not defined
ERROR PARSING: Could not load ability onagro_mine_games
Unit($onagro_Lvl3): Could not load skill onagro_mine_games
```

`onagro_mine_games` summons the leftover-renamed mine folder
`onagro_mine_6htjnq`. Lab wrote `UnitName "$alias"` (or the bare unique
id). `AbilityLabContentLoader` expands `$alias` to `onagro_mine_6htjnq`
and `RewriteAbilityText` only rewrites all-digit `#1529…` literals to
`#c1529…`. A `slug_token` id is a legal `#word` but a bare word is a
function name, so AbilityParser throws and the skill never loads.

Related leftover warnings in the same log (`NewMapManagerComponent`,
`NotoSansArabic-Bold SDF`, `ScriptLoading: Duplicate script dummy`) are
vanilla / Proton noise, not this parse failure.

Suggested fix: after `$alias` expand, prefix `#` on `UnitName` values
that are a single identifier so the expression parser sees a `#word`.
Keep authored files as `$alias` (no `#` on the alias token). Confirm in
the Onagro Stage fight that `onagro_mine_games` loads and the mine
spawns.

## Proposed solution (Pass 1, 2026-08-15)

**Home:** LokrCharacterLoader
**Approach:** Load-time rewrite only. `AbilityLabContentLoader` already expands `$alias` then calls `LabExpressionIds.RewriteAbilityText`, which prefixes `#` on a bare `UnitName` identifier (`BareUnitName`) after the all-digit `#1529…` → `#c1529…` pass. Authored files stay `$alias`. Pass 2 is in-game confirm of the Onagro Stage fight; add C# only if a UnitName form still throws `Function … is not defined`.
**Exact change:** No new rewrite unless verify fails. If it fails, inspect the expanded `UnitName` line and tighten `BareUnitName` only (still `("UnitName"\s+")(?!#)([A-Za-z][A-Za-z0-9_]*)(")` plus whatever form the log shows — e.g. leftover quotes or a `$` that did not expand). `GetDefinition_Prefix` already strips a leading `#`, so `#onagro_mine_6htjnq` resolves to the folder id. `NewAbilities` files already go through `RewriteAbilityText` without folder expand (no per-file alias map).
**Do not:** Write `#` onto `$alias` in `ability.txt`. Do not teach the expression parser that every bare word is a `#word`. Do not change SpawnUnit parse config. Do not re-implement `BareUnitName` if the Onagro fight already loads.
**In-game verify:** 1. Confirm `Mods/LokrLab/LokrAbilityLab/.../onagro_mine_games/ability.txt` still has `UnitName "$alias"` (or the alias token), not a baked `#`. 2. Launch through Steam / Proton. 3. Open Lab, start the Onagro Stage fight. 4. Confirm LogOutput has no `Function onagro_mine_* is not defined` and no `Could not load ability onagro_mine_games`. 5. Confirm the mine unit spawns (not MissingUnitView). 6. If it still throws, copy the exact `UnitName` line from the named parse preview and only then change the regex.
**Risk:** Prefixing `#` on a UnitName that was intentionally a function call would break that skill; vanilla UnitName values are `#word` or quoted ids, not function names. Save / authored aliases unchanged. Combat only changes for summons that previously failed to parse.

Resolved: 2026-08-15

Resolution: Confirmed in-game: Lab Onagro sandbox, mine ability loads,
enemy walking onto the mine triggers it. Load-time `#` prefix on bare
`UnitName` after `$alias` expand is enough; no further C#. Remaining
`ERROR PARSING` for `assassin_quickstep` is a corrupt KV file, not this
rewrite — see
[`../resolved/ability-kv-parse-empty-filename.md`](../resolved/ability-kv-parse-empty-filename.md).
`ApplyModifier: skipped missing modifier 'modifier_onagro_mine_tracker'`
is the existing LokrPatch skip, not a UnitName parse miss.
