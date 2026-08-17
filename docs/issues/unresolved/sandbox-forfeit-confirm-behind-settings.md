# Sandbox: Forfeit confirm draws behind the settings panel

Area: LokrLab Sandbox (`EmbeddedFightHost`, `EmbeddedSceneHudFitter`)
Status: unresolved

As of 2026-08-14: in an embedded sandbox fight, open the vanilla settings
menu and press Forfeit. The confirm dialog appears behind the settings
panel, so its buttons cannot be clicked.

The Lab does not own that dialog. Likely canvas sort order, hole
cropping, or EventSystem stacking in the additive fight embed
(`EmbeddedFightHost`, `EmbeddedSceneHudFitter`), not missing Lab UI.

Reproduce: Character Lab Sandbox, start fight, settings, Forfeit.

Do not mark resolved until a running sandbox fight can confirm Forfeit
from that dialog.

## Attempted fix (LokrLab 0.12.6) — did not work

Tried 2026-08-14. Confirmed still broken in a running sandbox fight.

Hypothesis: remapping Overlay HUD to Screen Space Camera put
`UISimpleModalDialog` (Forfeit Yes/No) on the same plane as `UIOptions`,
or left settings as Overlay on top of a Camera-space confirm.

What 0.12.6 shipped (still in the tree; do not revert without a better
plan):

- `EmbeddedSceneHost.IsLabCanvas` treats `UIOptions` and
  `UISimpleModalDialog` like `Icon` / `EndTurn` (not lab chrome), so the
  HUD fitter remaps them into the hole.
- `EmbeddedSceneHudFitter.PromoteVisibleModal` — while the confirm is
  visible, `SetAsLastSibling`, `overrideSorting` 500, `planeDistance`
  0.5 (closer than the default fit plane of 1).
- `EmbeddedScenePointer.IsFightHudControl` also treats those two types
  as fight HUD so hole raycast stripping does not drop their clicks.

That did not put Yes/No above settings. Next look should not repeat
sort/plane promotion alone. More likely: the confirm and settings share
one canvas and a full-screen settings `Image` / `CanvasGroup` still
wins; or the confirm is a different object than `UISimpleModalDialog`
(another popup prefab); or Lab Overlay (sort 20000) / hole input
stripping is still eating the dialog. Capture hierarchy, `renderMode`,
`sortingOrder`, `planeDistance`, and parent of both panels while the
confirm is up before trying another stack fix.

## Proposed solution (Pass 1, 2026-08-15)

**Home:** LokrLab
**Approach:** Lab HUD-fitter guard: hide the settings sheet while the confirm is up, instead of restacking canvases. `UIOptions` is an `IUIWindow` whose `CanvasGroup` keeps `blocksRaycasts` / `interactable` on a full-screen sheet; that still covers Yes/No after 0.12.6's `SetAsLastSibling` / `overrideSorting` 500 / `planeDistance` 0.5. `UIOptions.OnPressForfeit` is documented to drive `UISimpleModalDialog` (OnCancel already dismisses that type). Leave the 0.12.6 remap in place; add a visibility/raycast mute of settings, not another sort/plane promotion.
**Exact change:** In `EmbeddedSceneHudFitter.Apply` (already `LateUpdate`): when `FindObjectOfType<UISimpleModalDialog>(true)` has `IsVisible`, mute every `UIOptions` `CanvasGroup` (`alpha` 0, `blocksRaycasts` false, `interactable` false) without calling `CloseWindow`. Restore those three fields when the modal is not visible. If the visible modal's transform is under a `UIOptions` (confirm would vanish with the sheet), reparent the modal root to the hole camera canvas (or a small dedicated overlay child of the fight HUD canvas already remapped by `FitCanvas`) before muting. One-shot `LogInfo` on first visible confirm: modal type/name, parent path, canvas `renderMode` / `sortingOrder` / `planeDistance` / `overrideSorting`, sibling index, whether parented under `UIOptions`, and the `UIOptions` `CanvasGroup` state. Keep `EmbeddedScenePointer.IsFightHudControl` treating both types as fight HUD. No Harmony rewrite of `OnPressForfeit`.
**Do not:** Do not repeat `SetAsLastSibling`, `overrideSorting`, or `planeDistance` as the fix. Do not `CloseWindow` the settings sheet (that can drop the forfeit callback). Do not revert the 0.12.6 `IsLabCanvas` / fitter remap. Do not patch vanilla `UIOptions` / `UISimpleModalDialog` in `LokrPatch`.
**In-game verify:** 1. Character Lab Sandbox, start fight. 2. Open settings, press Forfeit. 3. Confirm Yes/No is visible and clickable in the hole; Yes forfeits, No returns to settings. 4. After No, settings still works (audio / close). If Yes/No is still covered or missing, capture from the one-shot log (or a hierarchy dump): modal type if not `UISimpleModalDialog`, parent path, both canvases' `renderMode` / `sortingOrder` / `planeDistance` / `overrideSorting`, sibling index, `CanvasGroup` alpha/blocksRaycasts on `UIOptions`, and whether hole raycast stripping listed the Yes/No graphics.
**Risk:** Sandbox embed only. Muting `UIOptions` must not hide a child confirm (hence the reparent check). Restoring the `CanvasGroup` after dismiss must not leave settings unclickable. No save data or vanilla fight HUD when Lab is closed.
