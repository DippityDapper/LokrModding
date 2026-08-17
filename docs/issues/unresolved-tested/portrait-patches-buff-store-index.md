# PortraitPatches: buff-store hero index is unchecked

Area: LokrCharacterLoader (`Patches/PortraitPatches.cs`)
Status: unresolved-tested

As of 2026-08-14: `UIBuffStoreItem.SetItem` postfix indexes
`GetAllHeroes()[itemBehavior.heroPosition]` (line 162) with no bounds
or null check on the list. A bad `heroPosition` throws. Pre-redesign
audit L-03.

Suggested fix: read the list once, skip when `heroPosition` is out of
range or the hero is null. Leave the CHALLENGE flat-image path as-is
when a hero is present.

## Proposed solution (Pass 1, 2026-08-15)

**Home:** LokrCharacterLoader
**Approach:** Keep the existing Harmony postfix on `UIBuffStoreItem.SetItem`. Guard the `GetAllHeroes()` index before any CHALLENGE resolve. Skip-and-log; do not change the flat-image path when a hero is present.
**Exact change:** In `LokrCharacterLoader/Patches/PortraitPatches.cs`, `UIBuffStoreItem_SetItem_Patch.Postfix` (lines 156–172). `itemBehavior` is a struct (`BuffStoreModelAdapter.BuffStoreItemBehavior` at `ih-original/Ironhide.Legends/BuffStoreModelAdapter.cs:85–107`; field at `UIBuffStoreItem.cs:249`), so it is never null. After the Traverse read (lines 159–160): if `MetagameManager.instance` or `HeroManager` is null, return. `List<Hero> heroes = MetagameManager.instance.HeroManager.GetAllHeroes();` once (`HeroManager.cs:99–102` returns `this.heroes`). If `heroes` is null, or `itemBehavior.heroPosition < 0`, or `heroPosition >= heroes.Count`, `LogWarning` with the index and count and `return`. `Hero hero = heroes[itemBehavior.heroPosition];` then keep the existing `hero == null` return (lines 163–166). Also skip if `hero.unitDefinition` is null before `uniqueId`. When a hero is present, leave lines 167–171 as they are (`ResolvePortrait(..., "CHALLENGE")` + `ReplaceWithFlatImage(__instance.portraitData.gameObject, ...)`). Vanilla `SetItem` already indexes the same list at `UIBuffStoreItem.cs:129–130`; this postfix is defense for a stale `heroPosition` or a list that shrank before the postfix runs.
**Do not:** Reimplement `UIBuffStoreItem.SetItem`. Do not patch `BuffStoreModelAdapter.GetHeroBuffStoreItemValues` (also indexes `allHeroes[heroPosition]` at decompile lines 56–58). Do not invent a default hero or clamp the index to `0`.
**In-game verify:**
1. Launch through Steam. Start a run with the usual three heroes. Open the map buff store.
2. Confirm each item's CHALLENGE portrait still appears for heroes that have `<id>_CHALLENGE.png`, and vanilla exo portraits still appear for those that do not.
3. Buy a buff (vanilla re-calls `SetItem` at line 146) and confirm no throw.
4. Confirm `LogOutput.log` has no `IndexOutOfRangeException` from this postfix.
**Risk:** None for saves or combat balance. Skipping the postfix leaves vanilla's already-applied `portraitData.UpdateAsset` in place (`UIBuffStoreItem.cs:133–134`).

## Unit tests (unresolved-tested)

Moved: 2026-08-15
Tests:
- LokrModding.Tests.CharacterLoader.ContentRulesTests.BuffStoreHeroPosition_OutOfRange
Still needs in-game confirm. A passing unit test is not a running-game
confirm.
