# Empty AI brain Considerations divide by zero on Eval

Area: LokrLab (Ability Lab `AIConfigB` / `AIBrain*` raw inner KV) and any custom ability brain
Status: unresolved-tested

`AbilityParser.ParseBrain` builds `AIDecisionScoreEvaluator.considerations` from `Selection.Considerations` children, skipping keys that start with `#`. An empty block (or only comments) parses as an empty list. `AIDecisionScoreEvaluator.Eval` then does `1f - 1f / (float)this.considerations.Count` with no empty check and throws `DivideByZeroException` on the first AI think.

Ability Lab "Add AIConfigB" writes a named block with empty `InnerKv`. A fully empty `AIConfigB { }` fails earlier (`ParseBrain` needs `Selection` / `Weight` / `Considerations`) and `ParseAbility` returns null. The divide-by-zero is the shape Lab authors actually type while learning the format:

```
AIConfigB
{
    Selection
    {
        Weight 1
        Considerations
        {
        }
    }
}
```

That loads, then crashes when `AIExperiment` or an OnThink action (`GetInRangeAI`, `KeepDistanceAI2`, `RetreatIfWeekAI`, …) calls `brain.dse.Eval`.

Suggested fix: Harmony-prefix `Eval` to return 0 (or the weight) when `considerations` is empty; Ability Lab should refuse or warn on an empty Considerations list. Confirm a Lab ability with the block above no longer throws on the unit's first think.

## Proposed solution (Pass 1, 2026-08-15)

**Home:** LokrPatch + LokrLab
**Approach:** Harmony prefix on `AIDecisionScoreEvaluator.Eval` returns 0 when `considerations` is null or empty (skip-and-log); Ability Lab warns on AIConfigB / AIBrain* blocks whose InnerKv has an empty or comment-only Considerations list, including the empty InnerKv that "Add AIConfigB" writes.
**Exact change:** New `LokrPatch/Patches/AIDecisionScoreEvaluatorEmptyPatch.cs`: `[HarmonyPatch(typeof(AIDecisionScoreEvaluator), nameof(AIDecisionScoreEvaluator.Eval))]` prefix `bool Prefix(AIDecisionScoreEvaluator __instance, ref float __result)`. If `__instance.considerations == null || Count == 0`, log a warning, set `__result = 0f`, return `false`. Otherwise return `true`. In `AbilityValidation.CollectWarnings`, for each `AiBlock`, warn when `InnerKv` is empty or a `Considerations { }` block has no non-`#` children (string scan of InnerKv is enough; do not parse a full brain). Do not seed a fake consideration on Add AIConfigB — that would change AI scores.
**Do not:** Return `weight` for an empty list (product of no considerations is 1, so that would make empty brains always score full weight). Do not change `AbilityParser.ParseBrain`. Do not refuse save — empty Considerations is parse-legal.
**In-game verify:** 1. Build, launch via Steam/Proton, Ability Lab. 2. Add AIConfigB with the empty-Considerations KV from this issue, give the skill an OnThink action (`GetInRangeAI` or similar), sandbox vs an AI unit. 3. First think: no `DivideByZeroException`, log warning, unit does not pick that brain as a sure thing. 4. Confirm status-line warning on save/load of that ability. 5. Confirm a vanilla AI unit still evaluates brains with real considerations.
**Risk:** Empty brains score 0 instead of crashing; they will not dominate action pick. Vanilla brains always have considerations. No save data.

## Unit tests (unresolved-tested)

Moved: 2026-08-15
Tests:
- LokrModding.Tests.Patch.PatchRulesTests.EmptyConsiderations_ReturnsZero
Still needs in-game confirm. A passing unit test is not a running-game
confirm.
