# KV PerAffectedAI is not an AbilityAction and drops the ability

Area: LokrLab (Ability Lab OnThink / event actions) and LokrCharacterLoader (AbilityParser action registry)
Status: unresolved-tested

Vanilla `AbilityParser` registers `PerAffectedAI` in `genericClassConfigs` next to real `AbilityAction` types, but the class is `PerAffectedAIEvaluator : AIEvaluator`. `ParseAction` does `(AbilityAction)ParseGenericClassWithDictionaryParams(...)`. That cast throws `InvalidCastException`. `ParseAbility` catches it, logs `ERROR PARSING`, and returns null — the whole ability is dropped.

`Evaluate` is never called from `ih-original` (no `AIEvaluator.Evaluate` invoke, no reflection). `ParseAIConfig` would accept the type without a cast, but nothing calls `ParseAIConfig`; `AIConfigB` / `AIBrain*` go through `ParseBrain` (considerations only).

No vanilla `AbilitiesScript` file uses `PerAffectedAI`. Ability Lab stores event actions as cards / raw KV. An author who picks or types `PerAffectedAI` (the overhaul notes list it as an AI type) loses the ability at load.

Suggested fix: unregister `PerAffectedAI` from the action map, or wrap it as a real `AbilityAction` that scores `%POSSIBLEACTIONS` from `affectedUnits`. Ability Lab should warn or hide the name until then. Confirm a Lab ability whose OnThink contains `PerAffectedAI { }` is rejected with a clear message instead of a silent null parse.

## Proposed solution (Pass 1, 2026-08-15)

**Home:** LokrPatch
**Approach:** Do not wrap `PerAffectedAIEvaluator` as a fake `AbilityAction`. Harmony prefix `AbilityParser.ParseActionList` to `RemoveChild` any `PerAffectedAI` blocks, log a warning, then run the original list parse so the rest of the ability loads. Lab already omits it from `AbilityCardDescriptors` (Add-card menu); treat a `PerAffectedAI` card (including opaque) as a blocking `AbilityValidation` error so Save cannot write a parse-killer, with the same warning text.
**Exact change:** New `LokrPatch` prefix on `ParseActionList(KeyValue kv, ParseConfig dummyParseConfig, List<object> extraParams)`: if `kv != null && kv.HasChildren`, copy `kv.Children` and `RemoveChild` each whose `Key == "PerAffectedAI"`, `LokrPatchPlugin.Log.LogWarning` once per removal (`PerAffectedAI` is an `AIEvaluator`, not an action; skipped). Return true. Optional ctor postfix `genericClassConfigs.Remove("PerAffectedAI")` so a missed child cannot InvalidCast. In LokrLab `AbilityValidation.TryValidate` / `WarnCard`: if `card.TypeId == "PerAffectedAI"`, set `error` (not only a warning) — "PerAffectedAI is not an AbilityAction; the loader skips it. Use a real OnThink action (GetInRangeAI, KeepDistanceAI2, RetreatIfWeekAI, …)." Do not `RegisterActionCard("PerAffectedAI", …)`.
**Do not:** Implement `Evaluate` wiring or a new `AbilityAction` wrapper. Do not Harmony-prefix `ParseAction` to return null (the list loop would NRE on `ActionContextId`). Do not unregister without skipping (unknown type still throws and `ParseAbility` returns null). Do not add it to the Add-card menu "so authors can try it."
**In-game verify:** 1. Build LokrPatch + LokrLab. 2. Launch through Steam / Proton. 3. Ability Lab: New ability with a real Hit (or Delay) plus an opaque/OnThink `PerAffectedAI { }`. 4. Confirm Save is blocked with the validation message. 5. Hand-drop `PerAffectedAI { }` into that `ability.txt` on disk, sandbox-reload. 6. Confirm LogOutput warns that `PerAffectedAI` was skipped, there is no `InvalidCastException` / `Could not load ability`, and the Hit still registers. 7. Confirm vanilla abilities (none use this type) still load.
**Risk:** No vanilla file uses `PerAffectedAI`. Skipping it cannot change shipped combat. Save data untouched. A Lab file that only contained this action would load as an ability with an empty event list — validation should stop authors from saving that.

## Unit tests (unresolved-tested)

Moved: 2026-08-15
Tests:
- LokrModding.Tests.Patch.PatchRulesTests.PerAffectedAi_IsSkippedActionKey
Still needs in-game confirm. A passing unit test is not a running-game
confirm.
