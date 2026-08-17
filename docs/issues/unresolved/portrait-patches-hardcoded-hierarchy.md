# PortraitPatches: MAP slot walks a hardcoded UI path

Area: LokrCharacterLoader (`Patches/PortraitPatches.cs`)
Status: unresolved

As of 2026-08-14: the MAP portrait patch does
`transform.Find("Content").Find("IconMask").Find("ExoSkeletonPortrait")`
(lines 119–120) with no null checks. A prefab rename NREs. Pre-redesign
audit L-04.

Suggested fix: resolve each child with a null guard and skip the flat
swap when the path is missing. Do not invent a new portrait renderer.

## Proposed solution (Pass 1, 2026-08-15)

**Home:** LokrCharacterLoader
**Approach:** Harmony postfix stays on `MapHeroBarPortraitComponent.SetHero`. Stop walking `Content/IconMask/ExoSkeletonPortrait`. Use the component's own `portraitData` field (what vanilla already updates) and skip-and-log when it is missing.
**Exact change:** In `LokrCharacterLoader/Patches/PortraitPatches.cs`, `MapHeroBarPortraitComponent_SetHero_Patch.Postfix` (lines 107–121): after the existing `hero == null` / `ResolvePortrait(..., "MAP")` early returns, replace lines 119–120 (`transform.Find("Content").Find("IconMask").Find("ExoSkeletonPortrait").gameObject`) with `__instance.portraitData`. Vanilla `SetHero` already does `this.portraitData.UpdateAsset` / `SetAnimFrame("Portrait", "StandStatic", 0)` (`ih-original/Ironhide.Legends/MapHeroBarPortraitComponent.cs:49–50`; field at line 373). If `portraitData` is null, `LokrCharacterLoaderPlugin.Log.LogWarning` and `return` — do not call `ReplaceWithFlatImage`. If non-null, `ReplaceWithFlatImage(portraitData.gameObject, sprite, new Vector2(0f, 0f), new Vector2(0.5f, 0.5f))` unchanged. Optional belt: if keeping `Find` as a fallback when `portraitData` is null, resolve `Find("Content")`, then `Find("IconMask")`, then `Find("ExoSkeletonPortrait")` as separate locals and skip on the first null; prefer the field so a prefab rename of those child names cannot NRE.
**Do not:** Invent a new `Image` on the bar root, search the whole hierarchy, or rewrite MAP portrait rendering. Do not change MINI/BIG/BANNER postfixes or the CHALLENGE prefixes that already use `targetPortraitExo` / `portraitData`.
**In-game verify:**
1. Launch through Steam. Use a hero with `<id>_MAP.png`.
2. Adventure-map hero bar: flat MAP portrait still appears in the mask/frame.
3. Open Team Manage / hero manage so `SetHero` runs again; portrait still there.
4. Confirm `LogOutput.log` has no NRE from `Find` / `.gameObject` on that screen.
**Risk:** None for saves or combat. If `portraitData` ever pointed at a different object than `ExoSkeletonPortrait`, the flat swap would follow vanilla's exo target (correct). Vanilla heroes without a MAP PNG never enter this postfix body past the sprite null check (line 115–118).
