# Loot anyOf chance always fires (int Random.Range)

Area: vanilla map loot (`LootItemGeneratorAnyOf.AddItems`). Hurts any
mod or Lab loot table that uses Lua `type = "anyOf"` with `chance` other
than 0 vs 1.
Status: unresolved-tested

As of 2026-08-15: `LootItemGeneratorAnyOf.AddItems` in
`ih-original/Ironhide.Legends/Ironhide/Legends/Model/Metagame/Map/Loot/LootItemGeneratorAnyOf.cs`
filters children with `(float)Random.Range(0, 1) < data.chance`.
`UnityEngine.Random.Range(int, int)` excludes max, so `Range(0, 1)` is
always 0. Every child with `chance > 0` always `Process`. They meant
`Random.Range(0f, 1f)`.

`LootTableLuaLoader.AnyOfParse` still reads `items[].chance` (default
1f). A community-pack or quest-Lua table that expects 30% anyOf rows
gives every row. `chance == 0` is the only skip.

Suggested fix: Harmony-prefix `AddItems` to use the float overload.
Until then, treat anyOf chance as a boolean. Do not start that patch
from the HTML-docs track.

See
[`LootItemGeneratorAnyOf.html`](../../api/base-game/Ironhide/Legends/Model/Metagame/Map/Loot/LootItemGeneratorAnyOf.html).

## Proposed solution (Pass 1, 2026-08-15)

**Home:** LokrPatch
**Approach:** Harmony prefix that fully replaces `LootItemGeneratorAnyOf.AddItems` so the roll uses Unity's float `Random.Range` instead of the int overload. Vanilla crash/brittle-assumption home: this is a stock loot-generator bug (int `Range(0, 1)` is always 0), so it belongs in LokrPatch with no CharacterLoader dependency.
**Exact change:** New `LokrPatch/Patches/LootItemGeneratorAnyOfPatch.cs`. `[HarmonyPatch(typeof(LootItemGeneratorAnyOf), "AddItems")]` — protected `void AddItems(LootTable.LootTableResult loot)`. Prefix reimplements the method and returns `false`: if `generators` is null, log a warning and return; otherwise `foreach` child `Data` and call `generator.Process(loot)` when `UnityEngine.Random.Range(0f, 1f) < data.chance` (keep the vanilla `<` comparison, including default `chance` 1f from `LootTableLuaLoader.AnyOfParse`). Do not patch `Process` or the Lua parser.
**Do not:** Transpile `Random.Range` globally; patch `LootTableLuaLoader.AnyOfParse`; change `oneOf`/`allOf`; treat chance as a boolean; rewrite the loot table system.
**In-game verify:** 1. Build and launch via Steam / Proton. 2. Confirm a vanilla chest/quest reward that uses `anyOf` with omitted or `chance = 1` still drops those rows. 3. Install or author a tiny loot Lua whose `type = "anyOf"` row has `chance = 0` (must never drop) and another with `chance = 0.3` (must drop on some rolls and skip on others across ~10 rewards, not every time). 4. Check `BepInEx/LogOutput.log` for no loot exceptions.
**Risk:** Vanilla `anyOf` rows that already used a fractional `chance` currently always fire; after this they roll as authored, which is the intended bugfix but does change drop rates for those tables. Save data is untouched. `chance == 1` stays effectively always (float `Range` is inclusive of 1f, so a exact-1.0 roll can skip a 100% row once in a blue moon — same as a correct Unity float roll, not a new design).

## Unit tests (unresolved-tested)

Moved: 2026-08-15
Tests:
- LokrModding.Tests.Patch.PatchRulesTests.LootChildFires_UsesFloatComparison
Still needs in-game confirm. A passing unit test is not a running-game
confirm.
