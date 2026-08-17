# Atlas load logs AchievementListener / UIAchievements / CheckAchievements errors

Area: vanilla metagame achievements on `krlegendsatlasscreen` (Proton / Linux
Steam). Hurts log noise; does not block map or fights.
Status: unresolved

As of 2026-08-15, loading a slot into the atlas prints this set every time.
Long-standing; does not affect gameplay; annoying in `LogOutput.log`.

```
cant find a gameobject of instance Ironhide.Legends.Model.Metagame.Achievements.AchievementListener!
NullReferenceException: UIAchievements.Start
  MonoSingleton<AchievementListener>.Instance.IncrementProgressEvent += ...
Unkown achievement (it doesn't exist in Steam): migration_easy_mode
NullReferenceException: FullMetagameSessionData.CheckAchievements
  GetAchievementInstance(...).IsCompleted()
```

`UIAchievements.Start` subscribes to `AchievementListener.Instance` with no
null check. The listener MonoSingleton is not in the atlas scene (same
class of miss as `NewMapManagerComponent` on that load).

`CheckAchievements` builds `wasteland_completed_with_` + each legend
roster id and calls `IsCompleted()` on `GetAchievementInstance`, which
returns null when that achievement id is not in `AchievementsConfig`.
A Lab legend (`onagro_0nzj37`) has no `wasteland_completed_with_onagro_0nzj37`
row, so the Count lambda NREs. Vanilla legend ids still have those rows.

`migration_easy_mode` is a vanilla migration achievement
(`AchievementHelper.MIGRATION_EASY_MODE_ACHIEVEMENT`). Steam on Proton
logs it as unknown; the increment still runs in-process.

Suggested fix: LokrPatch skip-and-log — prefix `UIAchievements.Start` when
`AchievementListener` is missing; prefix `CheckAchievements` (or the
wasteland Count) to skip null instances; leave the Steam unknown line
unless a platform stub exists. Do not invent Steam achievements.

Attempted in LokrPatch **1.0.11** (`AchievementsNrePatch`). Not confirmed
in-game.

See also [`../../capabilities-and-gaps.md`](../../capabilities-and-gaps.md) §2
achievements bullet.
