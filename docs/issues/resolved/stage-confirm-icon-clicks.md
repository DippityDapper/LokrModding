# Embedded Stage: confirm / cast icon not clickable after first Stop

Area: LokrCharacterLab (Sandbox / Ability Lab Stage embed)
Status: resolved

As of 2026-08-14: the first Stage Start accepts hex clicks, debug-panel
buttons, and (via hotkeys 1-4) ability select. After Stop then Start,
those still work, but the walk / confirm / cast boot above the chosen
hex does nothing. Confirmed in Ability Lab Stage; hotkeys select the
correct ability, so `SkillsBar.InstSkill` is the live bar.

Vanilla `TargetInteractionView.Awake` Instantiates four
`ConfirmButtonMove` canvases and sets `worldCamera = Camera.main`. After
Stop the lab is the active scene and `MainCamera`. The next Awake binds
the lab camera (or a fight camera `FitScene` later disables). The hole
camera still draws the WorldSpace boot; GraphicRaycaster aims through
the stale camera, so EventSystem never delivers `ConfirmButton.OnTap`.
`EmbeddedSceneHudFitter.FitCanvas` skips `WorldSpace` on purpose (the
boot must stay on the hex). Hex clicks use
`EmbeddedFightHexInputPatch` + the hole camera and do not need that
canvas. Debug buttons are scene-authored Unity `Button`s, not this path.

Vanilla fight leave is `StageControllerComponent.MapSceneReset` (statics
+ `Events.DestroyInstance`); Instantiated HUD dies with the fight scene.
It never rebinds confirm `worldCamera` because `Camera.main` is always
the fight camera in a normal load.

Suggested fix: in Character Lab only (via `LokrLabApi.GetEmbeddedSceneCamera`,
not `LokrLab.dll`):

1. After the hole camera exists, rebind every `ConfirmButton` parent
   `Canvas.worldCamera` to that camera. Leave `renderMode` as
   `WorldSpace`. Do not run these canvases through `FitCanvas`.
2. Do that from `SandboxFightControls.EnsureFightInput` (already runs
   on FightStarted / FightStartTurn and already writes
   `UnitController.mainCamera`). Optional Harmony postfix on
   `TargetInteractionView.Awake` for the same bind when the API camera
   is already set.
3. If a confirm canvas root is still in the lab scene, move it into the
   fight scene (`AdoptHexGridRoot` pattern).
4. Keep the hex-prefix rule: RaycastAll hit on `Icon` / `EndTurn` skips
   `OnFingerTap` so EventSystem can deliver `OnTap`.

Do not call `MapSceneReset` as part of this change. Do not add an Ability
Lab project reference. Do not invent a hex-click confirm path. Skill-hex
mouse clicks are out of scope unless they stay dead after the boot works.

Test: first Stage still confirms by clicking the boot; Stop then Start
(and a third time) confirm after hotkey select; hexes and debug buttons
unchanged; boot stays on the chosen hex.

Resolved: 2026-08-14

Resolution: Character Lab 0.9.45. `SandboxFightControls.BindConfirmCanvases`
sets each `ConfirmButton` canvas `worldCamera` to the hole camera from
`EnsureFightInput` (and a `TargetInteractionView.Awake` postfix when the
API camera is already set). WorldSpace is left alone. Unparented confirm
roots in the lab scene are moved into the fight scene. Confirmed in
game: Stop then Start, hotkey-select an ability, click the boot on the
hex.
