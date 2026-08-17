# LokrLab: Close Lab from a project, then reopen fails

Area: LokrLab (`CharacterLabScene`, shell statics)
Status: resolved

As of 2026-08-14: close the lab while a Character or Ability project is
open, then reopen from the mod menu.

First report: `FadeScreen` stayed on LOADING with the legacy "LoKR Lab"
title and `Close (\`)` button on top. Close Lab left `CurrentSession`
set; the next `Open` called `SwitchToShell` against destroyed shell
widgets and skipped `ShowFadeIn`.

After 0.12.1: clicking **LokrLab** in the mod menu closes the menu and
leaves the player on `krlegendsmainmenu`. No lab scene, no loading
graphic. `CharacterLabAccess.Open` always closes the menu first, then
`CharacterLabScene.Open`. That no-ops when `isTransitioning` is still
true (close fade / `UnloadSceneAsync` never reached `FinishOpen`'s
finally) and the active scene is not `LokrLab`.

Suggested fix: never ignore `Open()` from a real game scene; do not wait
on origin unload to show the lab; log open/close so a later miss is
visible in `LogOutput.log`.

Resolved: 2026-08-14

Resolution: LokrLab 0.12.2. Close Lab clears `CurrentSession` and shell
widget refs. `Open()` from a real scene always starts a load
(`wasTransitioning` is only logged). `FinishOpen` always `ShowFadeIn`.
Origin unload is fire-and-forget. Confirmed in game: close from a
Character project, reopen from the mod menu, Project Browser appears.
