# SkillTooltip missing variables resolve to 999

Area: LokrLab (sandbox / in-fight tooltips) and any custom ability loc string
Status: unresolved-tested

`AbilityInstance.ResolveFloatVariable` and `ModifierInstance.ResolveFloatVariable` log an error and return `999f` when the named configuration/instance key is missing. Sandbox and encyclopedia tooltips that reference an `AbilitySpecial` key the author omitted (or misspelled) show 999 as a real number.

Suggested fix: Harmony-postfix both methods to return 0 (or throw into our tooltip layer) when the object is null. Confirm a Lab skill whose loc string uses `{missingVar}` no longer displays 999.

## Proposed solution (Pass 1, 2026-08-15)

**Home:** LokrPatch
**Approach:** Harmony prefixes on `AbilityInstance.ResolveFloatVariable` and `ModifierInstance.ResolveFloatVariable` skip-and-log when the named key is missing, returning `0f` instead of `999f`. Prefix (not postfix) so a real configured value of 999 is left alone.
**Exact change:** New `LokrPatch/Patches/ResolveFloatVariableMissingPatch.cs` with two nested patches: `AbilityInstance.ResolveFloatVariable(string variableName)` and `ModifierInstance.ResolveFloatVariable(string variableName)`, both `bool Prefix(..., string variableName, ref float __result)`. Read `instanceContext` / `sourceTargetContext` via Traverse; if `GetObject(variableName)` is null, `LokrPatchPlugin.Log.LogWarning` (same skill/modifier id vanilla already logs), `__result = 0f`, return `false`. If the object is an `IExpression` or a convertible number, return `true` and let vanilla run. `SkillActivityPointer.ResolveVariable` already forwards to `AbilityInstance.ResolveFloatVariable`, so encyclopedia / sandbox tooltips that go through `AbilitiesIFormatter` pick up the 0.
**Do not:** Postfix-replace every `999f` result. Do not add a Lab localization parser. Do not change `ResolveStringVariable`.
**In-game verify:** 1. Build, launch via Steam/Proton, Ability Lab. 2. Skill loc uses `{missingVar}` with no AbilitySpecial of that name; hover tooltip in sandbox (and encyclopedia if the skill is on a hero) shows 0, not 999. 3. A real AbilitySpecial `{myVar}` with value 5 still shows 5. 4. Vanilla skill tooltips that resolve shipped keys still match live numbers.
**Risk:** Authors who treated 999 as a "missing" sentinel in loc will now see 0. Display-only; no save data, no combat math.

## Unit tests (unresolved-tested)

Moved: 2026-08-15
Tests:
- LokrModding.Tests.Patch.PatchRulesTests.MissingVariable_ReturnsZeroNot999
Still needs in-game confirm. A passing unit test is not a running-game
confirm.
