# HeroProgressWindow.ShowHeroProgress NREs on an unknown uniqueId

Area: docs/api/base-game (victory HUD)
Status: unresolved-tested

As of 2026-08-15: `HeroProgressWindow.ShowHeroProgress` in
`ih-original/Ironhide.Legends/Ironhide/Legends/View/Hud/VictoryWindow/HeroProgress/HeroProgressWindow.cs`
looks up `UnityDefinitionsParser.DefinitionsByUnique` with
`HeroProgressInfo.id` (the hero `UniqueId` from
`HeroManager.GetAllHeroes()` union `GetOldHeroes()`, via
`VictoryWindow.CalcData`). On a miss it logs
`Could not find unit with uniqueId` and then immediately reads
`valueOrDefault.metaExo` and `cinematicTags`, which NREs and aborts
the victory screen.

This is not the save-load wipe in
[`save-sanitize-drops-unknown-ids.md`](save-sanitize-drops-unknown-ids.md)
or the party-trio reset in
[`save-party-reset-to-vanilla-trio.md`](save-party-reset-to-vanilla-trio.md).
Those run on load. This runs at victory in the current session.
Character Lab live-reload and community-pack uninstall can leave a
`UniqueId` on `HeroManager` after the definition is gone. Custom
heroes that stay registered do not hit this lookup; they can still
NRE later in `ShowHeroProgressAnim` when
`GetSkillUnlockedAtLevel` returns null (or a missing ability id)
and the code reads `valueOrDefault2.Icon` with no null check.

Suggested fix: skip or stub the bar when `DefinitionsByUnique`
misses, and skip `ShowUnlockSkillAnimation` when the unlock skill
or ability is null. Do not start that patch from the HTML-docs track.

See
[`HeroProgressWindow.html`](../../api/base-game/Ironhide/Legends/View/Hud/VictoryWindow/HeroProgress/HeroProgressWindow.html).

## Proposed solution (Pass 1, 2026-08-15)

**Home:** LokrPatch
**Approach:** Harmony prefix on `ShowHeroProgress` that drops `HeroProgressInfo` rows whose uniqueId is missing from `DefinitionsByUnique` (log and skip the bar) so `progressData` and `heroBars` stay the same length. Small transpiler on `ShowHeroProgressAnim` to skip `ShowUnlockSkillAnimation` when `GetSkillUnlockedAtLevel` / `abilities.GetValueOrDefault` is null, still setting `gainedLevel` so the XP loop continues. Victory HUD only; not the save-load wipe.
**Exact change:** `HeroProgressWindowPatches` in LokrPatch. `[HarmonyPatch(typeof(HeroProgressWindow), nameof(HeroProgressWindow.ShowHeroProgress))]` prefix (`void ShowHeroProgress(List<HeroProgressInfo> data)`): `RemoveAll` infos where `UnityDefinitionsParser.instance.DefinitionsByUnique` lacks `info.id`; `LogWarning` each skipped uniqueId; return true so vanilla builds bars for the remainder (mutating `data` keeps `this.progressData` aligned with `heroBars`). `[HarmonyPatch(typeof(HeroProgressWindow), nameof(HeroProgressWindow.ShowHeroProgressAnim))]` transpiler: after `abilities.GetValueOrDefault(skillUnlockedAtLevel, null)`, if the `Ability` is null, skip the `StartCoroutine(ShowUnlockSkillAnimation(...))` call and still execute `gainedLevel = true`. Do not copy the whole coroutine. Do not stub `metaExo` / cinematicTags.
**Do not:** Swallow the rest of the victory screen with a MoveNext try/catch. Rewrite `VictoryWindow.CalcData`. Patch `GetSkillUnlockedAtLevel` globally (hero-room also calls it). Fold into Sanitize / party-reset. Invent Encyclopedia or Lab UI.
**In-game verify:** 1. Win a fight with a custom hero still registered: XP bars and rank-up skill unlock anim play as vanilla. 2. Win after Lab-reload / pack uninstall left that uniqueId on `HeroManager` but not in `DefinitionsByUnique`: victory window opens; that hero’s bar is omitted; other party bars animate; log names the missing uniqueId. 3. Vanilla trio victory: three bars, unlock anim when a rank-up grants a known skill. 4. Custom hero whose unlock skill id is missing from `abilities`: rank-up still completes, unlock icon anim skipped, no NRE.
**Risk:** Skipping a bar hides XP for a hero whose definition is gone this session (progress remains on the roster save). A bad transpiler could skip `gainedLevel = true` and spin the `do/while`. Vanilla victory flow must stay intact when every uniqueId and unlock skill exists.

## Unit tests (unresolved-tested)

Moved: 2026-08-15
Tests:
- LokrModding.Tests.Patch.PatchRulesTests.UnknownUniqueId_IsDropped
Still needs in-game confirm. A passing unit test is not a running-game
confirm.
