# Stats.ApplyModifier throws on a missing stat key

Area: LokrLab (sandbox / Ability ApplyModifier) and base-game `Stats.ApplyModifier`
Status: unresolved-tested

As of 2026-08-15 (Pass B, Units HTML): `Stats.ApplyModifier` throws
`Exception` `could not find stat with key ...` when any additive or
multiplicative pair names a stat the live `Stats` dictionary does not
have. Ability Lab `ApplyModifier` cards and custom Lab modifiers that
typo a stat id, or that name a custom stat not copied onto the unit,
abort sandbox combat. LokrPatch's `ApplyModifierMissingPatch` only skips
unknown *modifier* ids, not missing *stat* keys.

Suggested fix: skip (and log) missing stat keys in a Harmony prefix on
`Stats.ApplyModifier`, or validate modifier stat names in Ability Lab
before play.

## Proposed solution (Pass 1, 2026-08-15)

**Home:** LokrPatch + LokrLab
**Approach:** New Harmony prefix on `Stats.ApplyModifier(StatModifier)` skip-and-logs missing *stat keys* (separate from existing `ApplyModifierMissingPatch`, which only skips unknown *modifier ids* on `ApplyModifierAction.Execute`). Ability Lab warns on PropertiesAdd / PropertiesMult keys and SetStat `Stat` fields that are not in `AbilityPickerCatalog.StatRefs`.
**Exact change:** New `LokrPatch/Patches/StatsApplyModifierMissingStatPatch.cs`: `[HarmonyPatch(typeof(Stats), nameof(Stats.ApplyModifier))]` prefix `bool Prefix(Stats __instance, StatModifier statModifier)`. If `statModifier` is null, return `true`. Reimplement the two vanilla loops: for each pair in `additiveModifiers` / `multiplicativeModifiers`, `TryGetValue` on `__instance.stats`; if missing, `LogWarning` `"Stats.ApplyModifier: skipped missing stat '…'"` and continue; else call `AddAdditiveModifier` / `AddMultiplicativeModifier` as vanilla. Return `false`. Apply remaining keys on the same modifier — do not skip the whole modifier because one key is typo'd. Lab: `AbilityValidation` warn when `ModifierDef.PropertiesAddKv` (and PropertiesMult if present in ExtraKv) names a key not in `StatRefs` (strip `#`); same for `SetStat` cards' `Stat` field via existing catalog kind.
**Do not:** Extend `ApplyModifierMissingPatch` (wrong type: modifier id vs stat key). Do not `AddStat` for unknown keys (that would invent combat stats). Do not change `SetStatValue` (it already adds missing stats).
**In-game verify:** 1. Build, launch via Steam/Proton, Ability Lab. 2. Modifier PropertiesAdd with a typo stat id; apply in sandbox — no `could not find stat with key` throw, log skip, other valid keys on that modifier still apply. 3. Status warning on save. 4. Vanilla modifier that adds `#health` / `#armor` still changes those stats. 5. ApplyModifier with a missing *modifier id* still hits the existing patch, not this one.
**Risk:** A typo'd stat no longer aborts the rest of the modifier or the ability event (input no longer stuck). Partial apply is the skip-and-log intent. Vanilla units have the shipped stat keys. No save data.

## Unit tests (unresolved-tested)

Moved: 2026-08-15
Tests:
- LokrModding.Tests.Patch.PatchRulesTests.MissingStat_IsSkipped
Still needs in-game confirm. A passing unit test is not a running-game
confirm.
