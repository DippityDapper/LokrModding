# AbilityParser NullRefs when AOE keys are omitted

Area: LokrCharacterLoader (AbilityParser via AbilitiesDefinitionsPatches) and LokrLab (Ability Lab / hand-authored KV)
Status: unresolved-tested

Vanilla `AbilityParser.ParseAbility` requires `AbilityAOECenterOnCaster` and `AbilityAOEAffectsCaster` whenever `AbilityBehavior` includes `AOE`. Both calls are `kv[key].GetFloat()` with no null check. A missing key NullRefs, `ParseAbility` catches it, returns null, and Load skips the ability.

Ability Lab's `AbilityKvIO` always writes both keys when the AOE flag is set, so a Lab save is safe. Hand-authored `BuildingAbilities` fragments, leftover copies, and pasted vanilla-style KV that set AOE without those two keys drop the whole skill.

Suggested fix: Harmony-prefix or postfix `ParseAbility` (or a small helper the patch already owns) so missing AOE center/affects keys default to 0, matching `AbilityAOEMinRange`. Confirm a Lab/sandbox load of an AOE skill that omits both keys still registers.

## Proposed solution (Pass 1, 2026-08-15)

**Home:** LokrPatch
**Approach:** Vanilla `ParseAbility` NullRefs on `kv["AbilityAOECenterOnCaster"].GetFloat()` / `AbilityAOEAffectsCaster` whenever `AbilityBehavior` includes `AOE` and the key is missing; the method catch returns null and Load skips the skill. Harmony prefix `AbilityParser.ParseAbility(KeyValue kv)` to inject `"0"` children for those two keys when absent, then let the original run. Lab `AbilityKvIO` already writes both keys; this is for hand-authored / leftover KV.
**Exact change:** New `LokrPatch` prefix on `AbilityParser.ParseAbility`. If `kv` is null, return true. Read `kv["AbilityBehavior"]`; if the pipe-split tokens include `AOE` (trim, exact token, not a substring of another flag), then for each of `AbilityAOECenterOnCaster` and `AbilityAOEAffectsCaster`, if `kv[key] == null`, `kv.AddChild(new KeyValue(key).Set(0f))`. Return true. Default `0` matches `AbilityAOEMinRange`'s missing-key `ConstantFloatExpression(0f)` and C# `bool` false (`GetFloat() != 0`).
**Do not:** Prefix/reimplement the whole `ParseAbility` body. Do not postfix (the NRE is already swallowed). Do not default `AbilityAOERange` / `AbilityAOEKind` (those are required and should still fail). Do not change Lab save (already writes the keys). Do not treat missing `AbilityAOEWidth` on non-circle kinds.
**In-game verify:** 1. Build LokrPatch. 2. Add a Lab or `NewAbilities` skill with `AbilityBehavior` containing `AOE`, plus Kind/Range/TeamFilter, and omit both center/affects keys. 3. Launch through Steam / Proton, open Lab, sandbox-reload. 4. Confirm LogOutput has no NullRef / `Could not load ability` for that id and `AbilitiesDefinition: Loaded` includes it. 5. Cast in sandbox: circle is centered on the selected target (not caster), caster is affected if inside the radius (AffectsCaster false means the filter excludes the caster — confirm the unit is not in `affectedUnits` when standing in the AoE). 6. Re-save a normal Lab point-AoE template and confirm authored `0`/`1` keys still win.
**Risk:** Vanilla AoE files already include both keys; injecting `0` never runs for them. False defaults match unset bools. No save-data change. Combat only changes for previously unloadable mod skills.

## Unit tests (unresolved-tested)

Moved: 2026-08-15
Tests:
- LokrModding.Tests.Patch.PatchRulesTests.HasAoeToken_StandaloneFlag
Still needs in-game confirm. A passing unit test is not a running-game
confirm.
