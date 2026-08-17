# ExoSkeleton patches: no guard on unitDefinition / metaExo

Area: LokrCharacterLoader (`HeroExoSkeletonPatches`, `UnitViewExoSkeletonPatches`)
Status: unresolved-tested

As of 2026-08-14: `HeroExoSkeletonPatches.Prefix` calls
`ResolveExoSkeleton(__instance.unitDefinition.metaExo)` when the
backing field is null, with no check that `unitDefinition` is non-null.
`UnitViewExoSkeletonPatches.Postfix` does the same on
`unit.unitDefinition.metaExo`. Pre-redesign audit L-02.

Suggested fix: return / fall through when `unitDefinition` or `metaExo`
is null. Do not change the seed-then-let-original-run cache behavior
when a definition is present.

## Proposed solution (Pass 1, 2026-08-15)

**Home:** LokrCharacterLoader
**Approach:** Null-guard the two existing Harmony patches. Do not change seed-then-let-original-run when `unitDefinition` and `metaExo` are present. Skip the original `Hero.exoSkeletonDataAsset` getter only when `unitDefinition` is null, because vanilla would NRE on `this.unitDefinition.metaExo` (`ih-original/.../Heroes/Hero.cs:28–36`).
**Exact change:**
1. `LokrCharacterLoader/Patches/HeroExoSkeletonPatches.cs` Prefix on `Hero.exoSkeletonDataAsset` getter (lines 26–31). Change to `bool Prefix(Hero __instance, ref ExoSkeletonDataAsset ____exoSkeletonDataAsset, ref ExoSkeletonDataAsset __result)`. If `__instance.unitDefinition` is null: leave the backing field alone, `__result = null`, `return false` (skip original). If `unitDefinition.metaExo` is null: do not call `ResolveExoSkeleton`; `return true` so vanilla `AssetBundleManager.LoadAsset("units", metaExo)` runs. If `____exoSkeletonDataAsset == null` and `metaExo` is non-null: keep `____exoSkeletonDataAsset = CharacterAPI.ResolveExoSkeleton(__instance.unitDefinition.metaExo); return true;` (current cache behavior). `CustomRigLoader.Resolve` already returns null when `metaExoName == null` (`CustomRigLoader.cs:89–91`).
2. `LokrCharacterLoader/Patches/UnitViewExoSkeletonPatches.cs` Postfix on `UnitViewManager.InstantiateUnitView` (lines 33–52). Before line 35: if `unit` is null, or `unit.unitDefinition` is null, or `__result` is null, `return`. Then `ResolveExoSkeleton(unit.unitDefinition.metaExo)` as now (`metaExo` null already yields null and returns at lines 36–38). Do not skip `UpdateAsset` / `PreloadAnimationIds` when a custom asset is present.
**Do not:** Wrap `ResolveExoSkeleton` in try/catch. Do not skip the original getter when a definition exists. Do not change `CustomRigLoader` indexing. Do not patch `AssetBundleManager.LoadAsset`.
**In-game verify:**
1. Launch through Steam. Vanilla hero on the map and in a fight: custom-rig path must not run; vanilla exo still shows (seed-then-original unchanged).
2. Custom-rig hero (`metaExo` matching a `rig/rig.json` folder): map hero bar and combat view still swap to the custom asset.
3. Confirm `LogOutput.log` has no NRE from `HeroExoSkeletonPatches` / `UnitViewExoSkeletonPatches` during map load and one fight start.
**Risk:** None for saves. Returning null from the getter when `unitDefinition` is missing can make a later vanilla `SetAnimFrame` throw on a missing asset; that is the same class of failure as today, without the Prefix NRE. Combat balance unchanged.

## Unit tests (unresolved-tested)

Moved: 2026-08-15
Tests:
- LokrModding.Tests.CharacterLoader.ContentRulesTests.ShouldSkipExoResolve_WhenDefinitionNull
Still needs in-game confirm. A passing unit test is not a running-game
confirm.
