# Sandbox camera zoom leaks into loading-screen UI

Area: LokrLab Sandbox (`EmbeddedFightCameraPatches`, `SandboxFightControls`,
`EmbeddedFightHost.Stop`)
Status: resolved

As of 2026-08-17: in a Sandbox fight that allows wheel zoom (Character /
Ability 1v1, Encounter without `lockZoom`), leaving the camera off the
default zoom makes later **loading-screen UI shrink or enlarge**. The
sandbox ortho is not restored when the embed stops, so the next
`transitionscene` (or any Screen Space Camera canvas on that camera)
inherits it.

Reproduce: Character Lab Sandbox, Start fight, wheel-zoom in or out,
Stop (or Close Lab / leave to a scene that shows the loading overlay).
The loading chrome is smaller after zoom-in, larger after zoom-out.
Default zoom does not show it.

Not Encounter Creator camera bounds. Not
[`campaign-fight-loading-stuck.md`](campaign-fight-loading-stuck.md)
(overlay never dismisses). This overlay does show; its scale is wrong.

Likely cause: two leftover writes. `EmbeddedFightCameraPatches.ApplyWheelZoom`
sets `camera.orthographicSize` and `UnlockCameraBounds` raises CameraBase
min/max to 0.4–50, while `EmbeddedSceneHudFitter.FitCanvas` remaps every
non-lab Overlay canvas — including DDOL **FadeScreen** — onto that hole
camera as Screen Space Camera. `EmbeddedFightHost.Stop` used to unload
without restoring ortho, MainCamera, or FadeScreen's Overlay mode. The
next `transitionscene` / `ShowFadeOut` then scales with the leftover zoom.

Suggested fix: skip FadeScreen in FitCanvas; on Stop restore captured
CameraBase / hole ortho, untag the hole camera (while the reference still
exists), retag the lab backdrop, and restore FadeScreen to Overlay.

Attempted in LokrLab **0.12.110** (`SandboxFightControls.CaptureCameraIfNeeded`
/ `RestoreCapturedCamera`, `EmbeddedSceneHudFitter.RestorePersistentCanvases`,
`EmbeddedSceneHost.RestoreLabMainCamera` before clearing the hole camera).

Resolved: 2026-08-17

Resolution: Confirmed in-game on LokrLab 0.12.110. After a zoomed Sandbox
fight, Stop / Close Lab shows the loading overlay at normal size.
