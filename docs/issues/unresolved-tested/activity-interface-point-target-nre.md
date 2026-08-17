# POINT_TARGET leaves BaseActivityInterface.targetFilter null

Area: vanilla combat targeting (`BaseActivityInterface`) plus Ability Lab
(`POINT_TARGET` / `MELEE` envelope flags, `GetCloseToUnitAI`).
Status: unresolved-tested

As of 2026-08-15: `AbilityBehavior.POINT_TARGET` skips `CreateTargetFilter`,
so `targetFilter` stays null. `IsPossibleTarget(Unit)` returns false.
`GetValidTargets`, `GetValidTargetsIgnoringRange`,
`IsPossibleTargetIgnoringPosition`, and `SetCenter` NullRef.

`MoveAndSkillController.InternalReset` avoids `GetValidTargets` when
`!TargetsUnits` (pure POINT_TARGET uses hexes). These paths still crash:

- `LoadMeleeCandidates` / `OnSelectedUnitMelee` call
  `IsPossibleTargetIgnoringPosition` — a Lab envelope with both `MELEE`
  and `POINT_TARGET` NullRefs on skill select
- `GetCloseToUnitAI.Execute` calls `GetValidTargetsIgnoringRange`
  with no null check — a POINT_TARGET ability whose AI uses that action
  NullRefs

Suggested fix: null-check `targetFilter` in the four methods (return
false / empty / no-op), or create a dummy filter for POINT_TARGET.
Until then, Ability Lab should warn on `MELEE` + `POINT_TARGET` and on
`GetCloseToUnitAI` under `POINT_TARGET`.

## Proposed solution (Pass 1, 2026-08-15)

**Home:** LokrPatch + LokrLab
**Approach:** Harmony prefixes null-check `BaseActivityInterface.targetFilter` on the four methods that NRE; Ability Lab warns on `MELEE` + `POINT_TARGET` and on a `GetCloseToUnitAI` card when Behavior includes `POINT_TARGET`.
**Exact change:** New `LokrPatch/Patches/BaseActivityInterfaceNullFilterPatch.cs`. `targetFilter` is private — Traverse it. Prefixes: `IsPossibleTargetIgnoringPosition(Unit)` → if filter null, `__result = false`, return `false`; `GetValidTargets(List<Unit>)` and `GetValidTargetsIgnoringRange(List<Unit>)` → if null, `__result = new List<Unit>()`, return `false`; `SetCenter(Vector2)` → if null, return `false` (no-op). Do not prefix `IsPossibleTarget(Unit)` (already `targetFilter != null &&`). That covers `LoadMeleeCandidates` / `OnSelectedUnitMelee` and `GetCloseToUnitAI.Execute` (`GetValidTargetsIgnoringRange`). In `AbilityValidation.CollectWarnings`: if flags contain both `MELEE` and `POINT_TARGET`, warn that melee select NullRefs without the patch; walk cards (including opaque `TypeId`) and warn if `GetCloseToUnitAI` appears under `POINT_TARGET`.
**Do not:** Create a dummy `CreateTargetFilter()` for POINT_TARGET (would make hex skills start targeting units). Do not change `AbilityParser` POINT_TARGET team-filter overwrite. Do not prefix `GetAffectedTargets` (uses `affectedFilter`, created when AOE).
**In-game verify:** 1. Build, launch via Steam/Proton, Ability Lab. 2. Envelope `MELEE | POINT_TARGET`, select the skill in sandbox — no NRE, no unit candidates (or empty). 3. POINT_TARGET skill whose OnThink has GetCloseToUnitAI — think does not NRE. 4. Status warnings on save for both shapes. 5. Vanilla melee UNIT_TARGET and pure POINT_TARGET AOE (point_aoe template) still target as before.
**Risk:** POINT_TARGET+MELEE becomes a no-op for unit targeting instead of a crash. Vanilla does not combine those flags. No save data.

## Unit tests (unresolved-tested)

Moved: 2026-08-15
Tests:
- LokrModding.Tests.Patch.PatchRulesTests.NullTargetFilter_IsSkipped
Still needs in-game confirm. A passing unit test is not a running-game
confirm.
