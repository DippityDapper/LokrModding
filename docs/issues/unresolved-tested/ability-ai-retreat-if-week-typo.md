# AI type RetreatIfWeekAI is the only parseable spelling

Area: LokrLab (Ability Lab AI blocks) and LokrCharacterLoader (AbilityParser registry)
Status: unresolved-tested

Vanilla `AbilityParser` registers the AI class `RetreatIfWeakAI` under the KV type name `RetreatIfWeekAI` (Week, not Weak). Vanilla content uses the typo (`retreat_if_weak_troll_ai.txt`). A correctly spelled `RetreatIfWeakAI` key throws `Could not parse` and drops the parent ability.

Ability Lab stores `AIConfigB` / `AIBrain*` as raw inner KV. An author who types the class name (or the English word) will fail to load. Copying a vanilla block works.

Suggested fix: register both spellings in a Harmony postfix on the `AbilityParser` constructor (or document the typo in the Ability Lab AI picker when that exists). Confirm a Lab ability whose AI `Type` is `RetreatIfWeakAI` loads and scores.

## Proposed solution (Pass 1, 2026-08-15)

**Home:** LokrPatch
**Approach:** Vanilla registers `RetreatIfWeakAI` (class) only under the KV key `RetreatIfWeekAI`. Postfix the `AbilityParser` constructor and add the English spelling to `genericClassConfigs` with the same `RetreatIfWeakAI.GetParseConfig()`. Leave the typo key in place so `retreat_if_weak_troll_ai.txt` still loads. Lab AI is a raw `InnerKv` text field (no type picker yet); add a one-line hint on the AI tab that both spellings parse.
**Exact change:** New `LokrPatch` `[HarmonyPatch(typeof(AbilityParser), MethodType.Constructor)]` postfix: `__instance.genericClassConfigs["RetreatIfWeakAI"] = RetreatIfWeakAI.GetParseConfig()` (same `ParseConfig` as the existing `RetreatIfWeekAI` entry). Do not remove or rename the typo key. In LokrLab `AbilityEditorForm` AI host help string, note `Type` may be `RetreatIfWeekAI` (vanilla files) or `RetreatIfWeakAI`.
**Do not:** Rewrite vanilla `AbilitiesScript`. Do not Harmony-rename the class. Do not add a full AI type picker in this pass. Do not treat `RetreatIfWeakAI` as a Lab action card (it is an OnThink / AIConfig action, not a Hit-style card).
**In-game verify:** 1. Build LokrPatch + LokrLab. 2. Launch through Steam / Proton. 3. Confirm a vanilla troll with `retreat_if_weak_troll_ai.txt` still loads (typo key). 4. Ability Lab: New ability, OnThink (or AI block) with `Type` / action `RetreatIfWeakAI` and the same fields as the vanilla file (`Unit`, `MaxDistance`, `BrainId`, …). 5. Save, sandbox-reload. 6. Confirm LogOutput has no `Could not parse action: RetreatIfWeakAI` and the ability registers. 7. In fight, confirm the unit still produces retreat candidates (name `RetreatIfWeakAI` / comment `RETREAT`) when walkSpeedUsed is 0.
**Risk:** Vanilla content keeps using the typo key; adding an alias cannot change those scores. No save-data change. A Lab author who types the English spelling will load instead of dropping the parent ability.

## Unit tests (unresolved-tested)

Moved: 2026-08-15
Tests:
- LokrModding.Tests.Patch.PatchRulesTests.RetreatIfWeak_CorrectKeyDiffersFromTypo
Still needs in-game confirm. A passing unit test is not a running-game
confirm.
