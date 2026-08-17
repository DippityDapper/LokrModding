# Vanilla-override description needs a full game restart

Area: LokrLab Character Properties + live reload (`LabContentReloader`,
`CharacterLabScene.CloseTo`)
Status: resolved

As of 2026-08-17: editing Gerald's Description (hero-room lore) in a
vanilla override did not show in the campaign until the process was
restarted. Disk was correct (`localization_en_US.txt` had the new
`UNIT_GERALD_LIGHTSEEKER_LORE`). `LogOutput.log` after Close Lab showed
`LokrLab CloseTo` then `transitionscene` with **no**
`Auto-reload on Lab close` / `ContentReloader` line — localization was
never rebuilt.

0.12.108 flushed the multiline Description field on close but still
reloaded from `LabClosing`. If that handler threw, CloseTo never reached
reload. 0.12.109 moves reload into `CloseTo` itself (try/catch, always
logs `Auto-reload on Lab close: starting`).

Hero-room lore is applied in `UIHeroRoomHeroData.SetHero` from
`LocalizationManager` at click time. Close the hero room and select
Gerald again after Close Lab. An already-open detail panel will not
refresh in place.

Related console NREs (LokrPatch 1.0.11, not confirmed):
`UIHeroRoom.LateUpdate` / `TooltipManager.LateUpdate` when EventSystem
has no input module; `CheckAchievements` /
`UIAchievements.Start` on atlas load (see
[`achievements-nre-on-atlas-load.md`](../unresolved/achievements-nre-on-atlas-load.md)).

Resolved: 2026-08-17

Resolution: Confirmed in-game on LokrLab 0.12.109. Close Lab logs
`Auto-reload on Lab close: starting` / `completed`. Re-selecting Gerald
in the hero room shows the Lab Description without a process restart.
