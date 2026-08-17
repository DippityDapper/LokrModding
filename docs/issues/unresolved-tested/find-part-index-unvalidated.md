# Party-token / ReplacePart: FindPartIndex result is unchecked

Area: LokrCharacterLoader (`ExoSkeletonDataPatches`, `PartyTokenComponentPatches`)
Status: unresolved-tested

As of 2026-08-14: both patches index `parts[FindPartIndex(...)]` with
no check that the part exists. A custom rig or a missing
`Asst_Party_Banner` / `unitOnMap` name throws on map party-token
refresh. Pre-redesign audit L-05.

Suggested fix: skip the swap when `FindPartIndex` returns a miss
(confirmed `-1` on both `ExoSkeletonData.FindPartIndex` and
`ExoSkeletonDataAsset.FindPartIndex`). Do not invent banner parts on
custom rigs.

Vanilla `ReloadData` also writes that `-1` into `renderOrder` when a
frame names an unknown part — load-path sibling, tracked as
[`reload-data-missing-sprite-nre.md`](reload-data-missing-sprite-nre.md).

## Proposed solution (Pass 1, 2026-08-15)

**Home:** LokrCharacterLoader
**Approach:** Skip-and-log in the two existing Harmony prefixes when `FindPartIndex` returns `-1` (confirmed at `ih-original/Ironhide.Legends/Ironhide/ExoSkeleton/ExoSkeletonData.cs:152–161` and `ExoSkeletonDataAsset.cs:36–45`). Add a small prefix on vanilla `PartyTokenComponent.SetFlagVisibility` so skipping the banner capture cannot desync `partVertices`. Do not invent banner / `unitOnMap` parts.
**Exact change:**
1. `LokrCharacterLoader/Patches/ExoSkeletonDataPatches.cs` Prefix on `ExoSkeletonData.ReplacePart(string, string)` (lines 21–84). After the MAPMINI sprite is resolved (lines 29–33) and the texture is applied (line 35): `int index = __instance.FindPartIndex(oldPart);` — if `index < 0`, `LogWarning` (`oldPart`, hero id) and do not write `parts[index]`. Same for `FindPartIndex("Asst_Shadow")` at lines 59–60. Still bump `partsVersion`/`renderVersion` and `return false` so vanilla `ReplacePart` (`ExoSkeletonData.cs:164–171`, also unguarded) does not run and NRE. Do not synthesize a quad for a missing name.
2. `LokrCharacterLoader/Patches/PartyTokenComponentPatches.cs` Prefix on `UpdateHeroes` (lines 25–64). Before `ReplacePart(unitOnMap, "Asst_Party_Base")` (lines 49–54), if `FindPartIndex(unitOnMap) < 0` or `FindPartIndex("Asst_Party_Base") < 0`, skip that `ReplacePart` and log (covers the MAPMINI-miss fall-through into vanilla). At lines 55–57, if `FindPartIndex("Asst_Party_Banner") < 0`, log and `partVertices.Add(Array.Empty<Vector2>())` so the parallel array stays aligned with `unitsOnMap`; do not index `parts[-1]`.
3. New Harmony Prefix on `PartyTokenComponent.SetFlagVisibility` in the same file (or a sibling in `Patches/`). Vanilla (`PartyTokenComponent.cs:135–166`) does `parts[FindPartIndex("Asst_Party_Banner")]` per token. If the index is `< 0`, skip that unit (log once). Required because `SetFlagVisibility(false)` is called from our `UpdateHeroes` at line 60.
**Do not:** Merge with [`reload-data-missing-sprite-nre.md`](reload-data-missing-sprite-nre.md) (different call site: JSON `renderOrder`). Do not Harmony-replace vanilla `ReplacePart(string,string)` for Golemize/debug. Do not add `Asst_Party_Banner` / `Asst_Party_Base` meshes to custom rigs.
**In-game verify:**
1. Launch through Steam. Adventure map with vanilla party: tokens, banner hide/show, and movement still work.
2. Give a custom hero a `_MAPMINI.png` and a `unitOnMap` name that is not on the party-token template; refresh tokens (enter map / add a unit). Confirm no throw; token may keep the template base part.
3. Custom rig used as a party token without `Asst_Party_Banner`: map loads, `SetFlagVisibility` does not NRE; `LogOutput.log` has the skip warnings.
**Risk:** None for save data. Combat untouched. Vanilla templates include `Asst_Party_Banner` / `Asst_Party_Base`, so zero-mod parties should not hit the new skips. A missing `unitOnMap` part leaves the template silhouette instead of crashing.

## Unit tests (unresolved-tested)

Moved: 2026-08-15
Tests:
- LokrModding.Tests.CharacterLoader.ContentRulesTests.ShouldWritePartAtIndex_SkipsMinusOne
Still needs in-game confirm. A passing unit test is not a running-game
confirm.
