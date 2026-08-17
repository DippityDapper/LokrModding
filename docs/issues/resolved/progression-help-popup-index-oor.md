# UIProgressionHelpPopup.ShowPage throws on Next

Area: vanilla `UIProgressionHelpPopup` (campaign adventure start);
hurts atlas / first-adventure help, not Lab sandbox.
Status: resolved

As of 2026-08-15 (in-game Pass 2 confirm): starting an adventure from a
save (`startedAdventure` — `adventureId=forest|legend=Gerald`) then
clicking Next on the progression-help popup throws

```
ArgumentOutOfRangeException: Index was out of range.
UIProgressionHelpPopup.ShowPage(int index)
UIProgressionHelpPopup.Next()
```

The player can dismiss / continue to the map. Not the same as
[`fight-started-empty-initiative-nre.md`](fight-started-empty-initiative-nre.md)
(Lab empty initiative) or
[`campaign-fight-loading-stuck.md`](campaign-fight-loading-stuck.md)
(fight-node FadeScreen).

Decompiled `ShowPage` indexes `titles[index]` with no clamp. `Next`
increments `pageIndex` and only calls `Finished()` when
`pageIndex == pages.Count`. If `titles.Count` is smaller than
`pages.Count` (or `pages` is empty after Start already showed page 0),
`ShowPage` throws. Steam also logs `Unkown achievement: seen_progression_popup`
on Proton; that line is noise, not this index.

Suggested fix: LokrPatch prefix `ShowPage` / `Next` to clamp against
`Math.Min(pages.Count, titles.Count)` and call `Finished()` when Next
would walk off either list. Do not restack the popup or invent pages.

## Proposed solution (Pass 1, 2026-08-15)

**Home:** LokrPatch
**Approach:** Skip-and-log when the serialized `titles` / `pages` lists disagree. Vanilla `Start` already calls `ShowPage(0)`; that succeeded in the reported session, so both lists have at least one entry. `Next` then incremented into a hole.
**Exact change:** Prefix `UIProgressionHelpPopup.Next`: if `pageIndex + 1 >= pages.Count` or `pageIndex + 1 >= titles.Count`, call `Finished()` (or return false after invoking Finished via Traverse) instead of `ShowPage`. Prefix `ShowPage(int index)` to no-op when `index` is outside either list. Do not patch `Back` beyond the existing `pageIndex > 0` guard unless titles is shorter.
**Do not:** Fold this into FightStarted empty-initiative. Do not hide the Steam unknown-achievement line here (that is
[`../unresolved/achievements-nre-on-atlas-load.md`](../unresolved/achievements-nre-on-atlas-load.md)).
**In-game verify:** 1. Build LokrPatch. 2. Continue a save, start Forest (or any adventure) so the progression popup appears. 3. Click Next through every page and Cancel. 4. Confirm no `ArgumentOutOfRangeException` from `ShowPage`. 5. Confirm a vanilla first-run that already had matching lists still pages and closes.
**Risk:** Clamping only changes the out-of-sync serialized case. Matching lists still walk to `Finished` on the last page.

## Unit tests (unresolved-tested)

Moved: 2026-08-15
Tests:
- LokrModding.Tests.Patch.PatchRulesTests.ProgressionHelp_ClampsAndFinishesOffEitherList

Resolved: 2026-08-15

Resolution: Confirmed in-game. Continue a save, start an adventure, click
Next through the progression-help popup: no
`ArgumentOutOfRangeException` from `UIProgressionHelpPopup.ShowPage` /
`Next` (LokrPatch 1.0.6 `ProgressionHelpPopupPatch`).
