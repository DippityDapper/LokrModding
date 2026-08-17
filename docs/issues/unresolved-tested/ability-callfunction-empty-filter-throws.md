# Ability Lab CallFunction helpers throw when the unit filter is empty

Area: LokrLab (Ability Lab CallFunction picker) plus vanilla
`Ironhide.Legends.Content.Abilities` helpers
Status: unresolved-tested

As of 2026-08-15: Ability Lab's CallFunction card lists all 16 shipped
type names (`AbilityPickerCatalog.CallFunctions`). Several `Execute`
paths assume `UnitFilter` / `HeroFilter` / `TargetMarkerFilter` matched
at least one unit and throw when the list is empty:

- `ClosestTargetPreferNoFlip` — `list[0]` is `IndexOutOfRangeException`
- `KrumSelectTargets` — `Utils.Random` returns null, then
  `unit3.HexGridItem` NullRefs
- `SBFAspectPhysicalTeleportTarget` — empty hero stamps leave max
  `<= 0.1`, then fallback `ToList()[0]` throws
  `IndexOutOfRangeException`
- `SBFAspectSummonerTeleportTarget` — same empty-fallback
  `ToList()[0]`
- `WBFIrizaTeleportTarget` — same empty-fallback `ToList()[0]`
- `WBFOverseerSelectTentacleSpawn` — empty markers throw
  `Need to define massive tentacle spawn markers`; empty `HeroFilter`
  makes the closest-spawn `heroes.Min(...)` throw

`SBFAspectMagicTeleportTarget` does not throw on empty filters: both
facing maps stay at 0, `Filter` still matches every cell at that max,
and `Random` picks a hex. Pass C (K1) dropped it from this list.

A Lab sandbox card that picks one of these types and uses a filter that
matches nobody (or a fight with no heroes / no tentacle markers) crashes
the skill. Vanilla named fights usually have units on the board; Lab
does not.

Suggested fix: Harmony-prefix those `Execute` methods (or a shared
empty-filter guard) so an empty match skips `Actions` instead of
throwing. Until then, Ability Lab should warn on these six Function
names when the filter can be empty.

Not the same as `ability-aoe-range-cone-empty.md` (AOE kind never fills)
or `alias-unitname-parsed-as-function.md` (UnitName `$alias` parse; resolved).

## Proposed solution (Pass 1, 2026-08-15)

**Home:** LokrPatch + LokrLab
**Approach:** Harmony prefixes on the six `Execute` methods skip-and-log when the named filter matches nobody, so `Actions` never run; Ability Lab warns when a CallFunction card picks one of those six type names.
**Exact change:** New `LokrPatch/Patches/CallFunctionEmptyFilterPatches.cs` with nested `[HarmonyPatch]` types, each a `Prefix` on `void Execute(AbilityContext context)` for `ClosestTargetPreferNoFlip`, `KrumSelectTargets`, `SBFAspectPhysicalTeleportTarget`, `SBFAspectSummonerTeleportTarget`, `WBFIrizaTeleportTarget`, and `WBFOverseerSelectTentacleSpawn` (`Ironhide.Legends.Content.Abilities`). Shared helper reads the private `attributes` dictionary via Traverse, runs the same `UnitTargetHelper.Execute` + `Filter(Stage.instance.units)` as vanilla, and if the list is empty logs `LokrPatchPlugin.Log.LogWarning` and returns `false` (skip original). Overseer also skips when `TargetMarkerFilter` is empty (vanilla throws `Need to define massive tentacle spawn markers`) or `HeroFilter` is empty (`heroes.Min` NRE). Teleport helpers skip on empty `HeroFilter` only (that is the `ToList()[0]` fallback). In `AbilityValidation.WarnCard`, when `Function` is one of those six catalog strings, add a non-blocking note that an empty filter skips the helper (after the patch) / crashed before it.
**Do not:** Prefix `SBFAspectMagicTeleportTarget` (empty filters do not throw). Do not reimplement ray / influence / tentacle selection. Do not hide the six names from the CallFunction picker — vanilla fights still need them.
**In-game verify:** 1. Build, launch via Steam/Proton, open Ability Lab. 2. New sandbox skill, CallFunction = `ClosestTargetPreferNoFlip` with a UnitFilter that matches nobody; cast — skill continues, log has the skip warning, no `IndexOutOfRangeException`. 3. Repeat for `KrumSelectTargets`, one teleport helper (`WBFIrizaTeleportTarget`), and `WBFOverseerSelectTentacleSpawn` with empty markers. 4. Confirm a vanilla named fight that uses one of these helpers (Krum / Iriza / Overseer) still selects targets. 5. Save the Lab skill and confirm the CallFunction warning appears on the status line.
**Risk:** Vanilla named fights that always have units on the board keep running the original `Execute`. Skip-and-log only changes Lab / empty-board cases. No save data. Combat balance unchanged when the filter matches at least one unit.

## Unit tests (unresolved-tested)

Moved: 2026-08-15
Tests:
- LokrModding.Tests.Patch.PatchRulesTests.EmptyCallFunctionFilter_IsSkipped
Still needs in-game confirm. A passing unit test is not a running-game
confirm.
