# KV pointMagnitude constructs FunctionPointMult

Area: LokrLab (Ability Lab expression picker) and any hand-authored ability KV
Status: unresolved-tested

`BaseLogicParser.expressionFunctions["pointMagnitude"]` is `typeof(FunctionPointMult)`, not `FunctionPointMagnitude`. `FunctionPointMagnitude.GetFloat` returns `Vector3.magnitude` and is unreachable from ability KV.

Ability Lab lists `pointMagnitude` in `AbilityPickerCatalog.ExpressionFunctions` (and `generate_ability_picker_catalog.py` `PARSER_FUNCTIONS`). `AbilityCatalogLookups.ExpressionOptions()` concatenates that list, so authors can type or pick the name. `FunctionsFor(Position)` offers `pointMult` but not `pointMagnitude`; the unfiltered catalog still includes it.

A one-arg call (matching `FunctionPointMagnitude`) throws at parse because `FunctionPointMult` wants `(point, float)` and `ParseAbility` drops the ability. A two-arg call scalar-multiplies instead of returning a length.

Suggested fix: Harmony-replace the `expressionFunctions` entry so `pointMagnitude` maps to `FunctionPointMagnitude`. Confirm a Lab skill that uses `pointMagnitude(pointSub(A, B))` loads and returns a length.

## Proposed solution (Pass 1, 2026-08-15)

**Home:** LokrPatch
**Approach:** Vanilla `BaseLogicParser.expressionFunctions["pointMagnitude"]` is `typeof(FunctionPointMult)` (copy-paste; `FunctionPointMagnitude` exists and is unreachable). Harmony postfix the `AbilityParser` constructor and overwrite that one dictionary entry. Lab already lists `pointMagnitude` in `AbilityPickerCatalog.ExpressionFunctions`; add it to `AbilityCatalogLookups.FunctionsFor(Position)` so the filtered Position picker matches the parser after the remap.
**Exact change:** New `LokrPatch` file, `[HarmonyPatch(typeof(AbilityParser), MethodType.Constructor)]` postfix: `__instance.expressionFunctions["pointMagnitude"] = typeof(FunctionPointMagnitude)`. Run after the ctor `MergeWith` so ability-only functions stay intact. In LokrLab, append `"pointMagnitude"` to the Position `FunctionsFor` array next to `pointMult`. Leave `PARSER_FUNCTIONS` as-is.
**Do not:** Remap `pointMult` or `pointMultElements`. Do not postfix `BaseLogicParser` or `StatsParser` (separate dictionary). Do not reimplement `FunctionPointMagnitude`. Do not hide `pointMagnitude` from the unfiltered catalog.
**In-game verify:** 1. Build LokrPatch + LokrLab. 2. Launch through Steam / Proton. 3. Ability Lab: New ability, add a number field `pointMagnitude(pointSub(unitPosition(%TARGET), unitPosition(%CASTER)))` (or a Hit/condition that uses it). 4. Save and sandbox-reload. 5. Confirm LogOutput has no `FunctionPointMult Function needs 2 parameters` and the ability id is in `AbilitiesDefinition: Loaded`. 6. In fight, confirm the value is a length (distance), not a scaled point. 7. Confirm a two-arg `pointMult(P, 2)` skill still scalar-multiplies.
**Risk:** No vanilla `AbilitiesScript` file calls `pointMagnitude`, so shipped content is unchanged. Combat balance unchanged. Save data untouched. Authors who already wrote two-arg `pointMagnitude(...)` expecting multiply will start throwing (one-arg ctor); that call was already the wrong function.

## Unit tests (unresolved-tested)

Moved: 2026-08-15
Tests:
- LokrModding.Tests.Patch.PatchRulesTests.PointMagnitude_MapsToFunctionPointMagnitude
Still needs in-game confirm. A passing unit test is not a running-game
confirm.
