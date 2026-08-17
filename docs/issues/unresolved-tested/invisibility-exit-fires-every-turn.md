# InvisibilityPatches: exit event fires for every non-invisible unit

Area: LokrCharacterLoader (`Patches/InvisibilityPatches.cs`)
Status: unresolved-tested

As of 2026-08-14: the `Unit.TurnEnded` postfix raises
`RaiseStateVisualEffect("INVISIBLE", unit, false)` whenever the unit
is not invisible. That is every non-Assassin, every turn. The Assassin
subscriber then `FindObjectsOfType<ExoSkeletonRenderer>()` and skips
non-Assassin ids. Pre-redesign audit L-11 / L-12.

Suggested fix: raise the exit event only when the unit actually left
INVISIBLE this turn (compare previous state, or only notify
subscribers that care). Keep the Assassin color effect as a
`RegisterStateVisualEffect` subscriber; prefer the unit's own renderer
over a global find.

## Proposed solution (Pass 1, 2026-08-15)

**Home:** LokrCharacterLoader
**Approach:** Keep `RegisterStateVisualEffect("INVISIBLE", …)` as the Assassin subscriber. Change the `Unit.TurnEnded` postfix so it raises exit only on a true on→off edge (`__state`). Scope the color write to that unit's view renderer instead of `Object.FindObjectsOfType<ExoSkeletonRenderer>()`. Same-file: gate `AddModifier` enter on a false→true edge so a second modifier while stealthed does not re-raise.
**Exact change:** In `LokrCharacterLoader/Patches/InvisibilityPatches.cs`:
1. `Unit_TurnEnded_Patch` (lines 34–45): add `[HarmonyPrefix] private static void Prefix(Unit __instance, out bool __state)` that sets `__state = __instance.states != null && __instance.states.IsOn("INVISIBLE")`. Change the postfix to `Postfix(Unit __instance, bool __state)`: raise `CharacterAPI.RaiseStateVisualEffect("INVISIBLE", __instance, false)` only when `__state && __instance.states != null && !__instance.states.IsOn("INVISIBLE")`. Vanilla `TurnEnded` (`ih-original/.../Units/Unit.cs:855–868`) does not clear INVISIBLE itself; `OnEvent("OnTurnFinished")` at line 857 can expire the modifier before the postfix, which is why `__state` must be captured in a prefix. This is the same TurnEnded hook the pre-BepInEx mod used (`ih-modded/.../Unit.cs:958–968`), minus the every-non-invisible fire.
2. `Unit_AddModifier_Patch` (lines 21–31): same `__state` pattern; raise enter only when `!__state &&` now `IsOn("INVISIBLE")`.
3. `RegisterDefaults` (lines 49–66): keep the `unit.isHero && uniqueId == "Assassin"` filter. Replace `Object.FindObjectsOfType<ExoSkeletonRenderer>()` (line 57) with the unit's own view: if `unit.unitView == null`, return; `ExoSkeletonRenderer[]` from `unit.unitView.GetComponentsInChildren<ExoSkeletonRenderer>(true)`; still only tint renderers named `"Graphic"` (`ExoSkeletonRenderer.color` at decompile line 129) to `alpha 0.5` on enter and `1` on exit. `UnitViewComponent.GraphicUnit` (`UnitViewComponent.cs:348–357`) is the exo transform; children search stays consistent with the `"Graphic"` name the original mod used (`ih-modded/.../Unit.cs:740–747`).
**Do not:** Hardcode Assassin in the Unit patches (keep dogfooding `RegisterStateVisualEffect`). Do not postfix `RemoveModifier` as a second exit path in this pass (TurnEnded `__state` matches the original hook). Do not tint every `"Graphic"` in the scene. Do not change stealth duration or INVISIBLE modifier KV.
**In-game verify:**
1. Launch through Steam. Fight with Assassin plus at least one other hero and one enemy.
2. Apply Assassin invisibility: only Assassin's combat mesh goes translucent, not the rest of the board.
3. End turns on the non-Assassin units while Assassin is still invisible: Assassin stays translucent; `LogOutput.log` is not spammed with the old `Debug.Log(IsOn("INVISIBLE"))` (already absent) and no extra work from a global find every turn.
4. When Assassin's INVISIBLE expires at turn end (or after the stealth-breaking action that clears it before postfix), opacity returns to 1 on Assassin only.
5. Repeat a fight with no Assassin: no unit is tinted.
**Risk:** None for save data. Combat balance unchanged (visual only). If INVISIBLE is cleared outside `TurnEnded` (mid-turn `RemoveModifier`) and we do not postfix `RemoveModifier`, the translucent tint can linger until the next TurnEnded edge — same as the original mod's TurnEnded-only exit. If that shows up in-game, a follow-up is a `RemoveModifier(ModifierInstance)` postfix with the same `__state` edge, not a global find.

## Unit tests (unresolved-tested)

Moved: 2026-08-15
Tests:
- LokrModding.Tests.CharacterLoader.ContentRulesTests.InvisibilityExit_OnlyOnTrueToFalseEdge
Still needs in-game confirm. A passing unit test is not a running-game
confirm.
