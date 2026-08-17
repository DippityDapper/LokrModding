# Ability equal() NullRefs when the left argument is null

Area: LokrLab (Ability Lab expressions / sandbox eval) and any custom ability KV using `equal`
Status: unresolved-tested

Vanilla `FunctionEqualsObjectExpression.GetFloat` does `a.Equals(b)` on the two `GetObject` results. A null left-hand side NullRefs. Ability Lab's picker catalog includes many vanilla-copied forms (`equal(%DEAD, %TARGET)`, `equal(activeUnit(), %HITTARGET)`, `equal(%customEvent, #AbilityAction02)`). A missing or unset context key on the left crashes sandbox eval. `isNull` is the null-safe check; `safeEquals` is float `Approximately` only and does not replace `equal`.

Suggested fix: Harmony-prefix `FunctionEqualsObjectExpression.GetFloat` to use `object.Equals(a, b)` (null-safe). Confirm a Lab skill with `equal(%MISSING, %TARGET)` evaluates to 0 instead of throwing.

## Proposed solution (Pass 1, 2026-08-15)

**Home:** LokrPatch
**Approach:** Harmony prefix replaces `FunctionEqualsObjectExpression.GetFloat` with a null-safe `object.Equals` so a missing left-hand context key evaluates to 0 (false) instead of NRE. No Lab picker change — `equal` is a valid function; `isNull` stays the explicit null check.
**Exact change:** New `LokrPatch/Patches/FunctionEqualsObjectExpressionPatch.cs`: `[HarmonyPatch(typeof(FunctionEqualsObjectExpression), nameof(FunctionEqualsObjectExpression.GetFloat))]` prefix `bool Prefix(FunctionEqualsObjectExpression __instance, IAbilityContext context, ref float __result)`. Traverse private `IExpression[] expressions`, `object a = expressions[0].GetObject(context)`, `object b = expressions[1].GetObject(context)`, `__result = Util.BoolToFloat(object.Equals(a, b))`, return `false`. `GetInt` / `GetObject` already call `GetFloat`, so one prefix covers them. Both-null is true (1); null vs non-null is false (0).
**Do not:** Patch `safeEquals` or `isNull`. Do not postfix. Do not warn on every Lab `equal(...)` snippet — those vanilla-copied forms are correct once null-safe.
**In-game verify:** 1. Build, launch via Steam/Proton, Ability Lab sandbox. 2. Conditional or expression using `equal(%MISSING, %TARGET)` — evaluates to 0, no NRE. 3. `equal(%TARGET, %TARGET)` still 1 when TARGET is set. 4. `isNull(%MISSING)` still works. 5. Cast a vanilla skill that uses `equal(%DEAD, %TARGET)` or `equal(activeUnit(), %HITTARGET)` and confirm behavior unchanged when both sides are non-null.
**Risk:** `equal(null, null)` becomes true; vanilla left-hand sides are set in those fights. No save data. No combat-balance change for shipped content.

## Unit tests (unresolved-tested)

Moved: 2026-08-15
Tests:
- LokrModding.Tests.Patch.PatchRulesTests.NullSafeEquals_NullLhs
Still needs in-game confirm. A passing unit test is not a running-game
confirm.
