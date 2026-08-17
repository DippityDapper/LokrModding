# Map HUD NREs when HeroModifiersAsset has no entry for a modifier

Area: LokrLab / custom abilities on the campaign map, and base-game map HUD
Status: unresolved-tested

As of 2026-08-15, `HeroModifiersAsset.GetConfigByKey` is a
`FirstOrDefault` and returns null on a miss. Three map HUD callers then
read `configByKey.overheadIcon` with no null check:

- `MapHeroBarPortraitModifiers.RefreshModifiers` keys on `modifier.id`
- `PortraitInitiativeMapModifiers.UpdateModifiers` keys on `mapMapping`
- `UnitDetailMapModifiers.UpdateModifiers` keys on `mapMapping`

A custom modifier (Ability Lab or a content pack) that has no
`HeroModifiersAsset` row crashes the map hero bar / unit-detail HUD
when that hero is shown. This is not a PortraitPatches hole — do not
fold into `portrait-patches-*`.

Suggested fix: skip (and log) a null config, or register map icons for
Lab modifiers before they can appear on the campaign HUD.

See
[`MapHeroBarPortraitModifiers.html`](../../api/base-game/Ironhide/Legends/View/Map/MapHeroBarPortraitModifiers.html),
[`PortraitInitiativeMapModifiers.html`](../../api/base-game/Ironhide/Legends/View/Map/PortraitInitiativeMapModifiers.html),
and
[`UnitDetailMapModifiers.html`](../../api/base-game/Ironhide/Legends/View/Map/UnitDetailMapModifiers.html).

## Proposed solution (Pass 1, 2026-08-15)

**Home:** LokrPatch
**Approach:** Harmony prefixes on the three map HUD callers (plus the map-rewards fourth site that does the same dereference) so a null `HeroModifiersAsset.GetConfigByKey` is logged and skipped instead of reading `overheadIcon`. Vanilla `GetConfigByKey` already returns null on a miss; do not stub a fake config (the `else` branches would enable a blank icon). Not a PortraitPatches hole and not Lab icon registration — Lab can add icons later; the HUD must not crash without them.
**Exact change:** `MapHudModifierConfigPatches` in LokrPatch. Prefix each of: `MapHeroBarPortraitModifiers.RefreshModifiers()`, `PortraitInitiativeMapModifiers.UpdateModifiers(Unit)`, `UnitDetailMapModifiers.SetMapModifiers(Unit)` (the issue title said `UpdateModifiers`; decompile’s method is `SetMapModifiers`). Each prefix returns false and copies the vanilla loop with `if (configByKey == null) { LokrPatchPlugin.Log.LogWarning(... id or mapMapping ...); continue; }` before `overheadIcon`. Same prefix on `RewardViewComponent`’s modifier-reward branch (`GetConfigByKey(modifier.id).modifierIcon` at the map rewards screen) so that NRE is covered in this file. Do not patch `HeroModifiersAsset.GetConfigByKey` itself.
**Do not:** Return a dummy `HeroModifiersConfig` from `GetConfigByKey` (enables a null sprite on the right overhead). Register Lab modifier icons here (LokrLab / content pack, follow-up). Fold into `portrait-patches-*`. Patch combat `ApplyModifier` (already `ApplyModifierMissingPatch`).
**In-game verify:** 1. Campaign map with a custom map modifier that has no `HeroModifiersAsset` row: hero bar, initiative portrait, and unit-detail panel open without NRE; log names the missing key; other modifiers still show icons. 2. Vanilla modifier with a config row: left/right overhead icons and tooltips unchanged. 3. Map reward that applies a custom modifier with no config: rewards UI does not crash. 4. Vanilla map HUD with stock modifiers only: no extra skip warnings.
**Risk:** Missing overhead art for unconfigured custom modifiers (expected). Wrong `continue` could skip a later valid modifier in the same category loop — keep the skip per miss only. Combat modifiers still apply; this is view-only.

## Unit tests (unresolved-tested)

Moved: 2026-08-15
Tests:
- LokrModding.Tests.Patch.PatchRulesTests.NullModifierConfig_IsSkipped
Still needs in-game confirm. A passing unit test is not a running-game
confirm.
