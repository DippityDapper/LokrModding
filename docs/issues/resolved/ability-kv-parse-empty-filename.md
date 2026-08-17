# Ability KV parse error logs an empty filename

Area: LokrCharacterLoader
Status: resolved

On Lab open and again on sandbox reload (`ReloadScope.All`), Unity logs
twice:

```
ERROR PARSING: Could not parse kv in file  - EXCEPTION
  at KVLib.KeyValues.PenguinParser.ParseAllKeyValues ...
  at AbilitiesDefinitionsPatches.ExecuteLoad (...:73)
```

The empty filename is `new TextAsset(kvText)` with no `.name`. The catch
logged `ex.StackTrace` only, so the real parse reason (and which
fragment) is hidden. Assassin abilities still register afterward, so
this is not the sandbox spawn / FXMega-sound failure.

1.1.11 names each contributed TextAsset (source path when known) and
logs `ex.ToString()` plus a short preview. Use the next in-game log to
identify the two failing fragments and fix or skip them. Do not treat
this as resolved until that log names the files and the error is gone
or explained.

As of 2026-08-15 the named fragment is
`Mods/LokrLab/LokrAbilityLab/assassin_abilities_c6x8qe/assassin_quickstep_qy4z6j/ability.txt`.
PenguinParser: `Hit unnamed key while parsing without unnamed keys enabled`.
The file has a corrupt line (Official Pack `assassin_quickstep.txt` is
the same malformed `"AbilityAOETeamFilter   "TEAM_ALL"` source; Lab
import wrote trailing spaces in the key and an extra quote on values):

```
"AbilityAOETeamFilter   "	"TEAM_ALL""
```

Trailing spaces inside the key and an extra quote on the value. Assassin
skills still register afterward.

2026-08-15: the on-disk Lab copy
`Mods/LokrLab/LokrAbilityLab/assassin_abilities_c6x8qe/assassin_quickstep_qy4z6j/ability.txt`
was rewritten to valid `"AbilityAOETeamFilter"	"TEAM_ALL"` and
`"Target"	"%SOURCE"`. Re-importing Official Pack Assassin would copy
the corrupt pack line again.

## Proposed solution (Pass 1, 2026-08-15)

**Home:** verify-only
**Approach:** No new C# until the next Lab-open / sandbox-reload log. `AbilitiesDefinitionsPatches.ExecuteLoad` already sets `TextAsset.name` from `AbilitiesBuilder.KvTextSources` (path or `mod-ability-N`) and logs `ex.ToString()` plus a 160-character preview. That is enough to name the two PenguinParser failures. After the log exists, fix or skip those fragments in a follow-up; do not treat this issue as resolved on logging alone.
**Exact change:** None in Pass 2 unless the new log still shows `(unnamed)` or an empty name. If it does, name `fromResources` assets that have an empty Unity `name` (vanilla `ResourcesWrapper.LoadAll` entries) and require every `AddAbilityText` caller to pass a source. Once names appear: if the path is a Lab `ability.txt` or `NewAbilities/*.txt`, fix that KV or skip the fragment with a warning; if it is a third-party `BuildingAbilities` contribution, skip-and-log that fragment only. The catch stays around `KVParser.KV1.ParseAll` (syntax), distinct from `ParseAbility` returning null (semantic).
**Do not:** Re-work the 1.1.11 naming/preview. Do not wrap `ParseAll` in a per-ability splitter that hides which file failed. Do not move this to `resolved/` because Assassin skills still register.
**In-game verify:** 1. Confirm the Lab `assassin_quickstep_qy4z6j/ability.txt` has a clean `AbilityAOETeamFilter` line. 2. Launch through Steam / Proton. 3. Open Lab from the title Mods button, load Assassin, start sandbox. 4. Copy `BepInEx/LogOutput.log` `ERROR PARSING: Could not parse kv in file` lines — expect none for that path. 5. Other Assassin skills still register. 6. Leave unresolved until those fragments are gone (re-import of Official Pack Assassin can restore the corrupt pack line).
**Risk:** None while verify-only. Later skip-and-log of a named fragment drops only that ability, not the rest of the load. Vanilla Resources abilities are unchanged if their Unity names are already set.

Resolved: 2026-08-15

Resolution: Confirmed in-game: Assassin Lab open / sandbox no longer logs
`ERROR PARSING` for `assassin_quickstep_qy4z6j`. 1.1.11 named the
fragment; the Lab `ability.txt` was rewritten to valid KV. Official Pack
`assassin_quickstep.txt` is still malformed — a re-import of that pack
can restore the bad line.
