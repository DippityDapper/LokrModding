# PortraitPatches: ReplaceWithFlatImage parents a transform to itself

Area: LokrCharacterLoader (`Patches/PortraitPatches.cs`)
Status: unresolved

As of 2026-08-14: `ReplaceWithFlatImage` ends with
`gameObject.transform.SetParent(gameObject.transform)` (line 187). That
is a no-op self-parent, used at every MAP / MAPMINI / CHALLENGE flat
portrait call site. Pre-redesign audit L-06.

Suggested fix: delete the `SetParent` line, or parent to the previous
parent if a reparent was actually intended. Confirm MAP / MAPMINI /
CHALLENGE portraits still appear after the change.

## Proposed solution (Pass 1, 2026-08-15)

**Home:** LokrCharacterLoader
**Approach:** One-line deletion in the existing `ReplaceWithFlatImage` helper. No new Harmony patch. The self-parent is a no-op copied from the pre-BepInEx mod (`ih-modded/Ironhide.Legends/MapHeroBarPortraitComponent.cs:418`, `UIBuffStoreItem.cs:159`, `RewardViewComponent.cs:313`, `DialogViewManagerMap.cs:214` and `:262`); Unity rejects parenting a transform to itself.
**Exact change:** In `LokrCharacterLoader/Patches/PortraitPatches.cs`, `ReplaceWithFlatImage` (lines 177–188), delete line 187 `gameObject.transform.SetParent(gameObject.transform);`. Leave the `DestroyImmediate` of `ExoSkeletonUIGraphic`/`ExoSkeletonData`, the `RectTransform` anchor/pivot writes, and `Image` add as they are. All MAP / CHALLENGE call sites share this helper (`MapHeroBarPortraitComponent_SetHero_Patch` line 120, `RewardViewComponent_SetTargetPortrait_Patch` line 142, `UIBuffStoreItem_SetItem_Patch` line 170, plus `DialogViewManagerMapPatches` lines 160 and 188). MAPMINI does not use this helper (`ExoSkeletonDataPatches` uses `ReplacePart`).
**Do not:** Reparent to `transform.parent` or the canvas; the original mod never stored a previous parent, so there is nothing to restore. Do not rewrite flat-image teardown. Do not touch MAPMINI `ReplacePart`.
**In-game verify:**
1. Launch the game through Steam (Proton). Equip a hero that has `Portraits/<id>/<id>_MAP.png`, `_CHALLENGE.png` (and optionally `_MAPMINI.png`).
2. On the adventure map, confirm the hero-bar MAP portrait is the flat PNG, still inside the frame, not detached or missing.
3. Open a challenge / reward portrait and the buff store; confirm CHALLENGE flats still show.
4. Confirm `BepInEx/LogOutput.log` has no Unity "parented to itself" error after those screens.
**Risk:** None for save data or combat. Vanilla heroes without overlay PNGs never enter `ReplaceWithFlatImage`. Worst case a flat portrait sits at the same local pose it already had (the `SetParent` was a no-op).
