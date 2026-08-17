# Character Lab: IslandAtlasPicker and MenuBar statics survive Close

Area: LokrCharacterLab (`IslandAtlasPickerPanel`, `MenuBarPanel`, `EditHistoryPanel`)
Status: resolved

As of 2026-08-14: `CharacterLabHooks` LabClosing resets RigEditor,
Properties, Inspector, category host, character list, and file
browser. `IslandAtlasPickerPanel` and `MenuBarPanel` still hold static
`UiModal` / texture / dropdown refs and have no `ResetSession`.
`IslandAtlasPickerPanel.Close` only hides the modal. Re-opening the
lab can touch destroyed widgets. Pre-redesign audit C-UI-02 remainder
(`InspectorPanel` is already reset).

Suggested fix: add `ResetSession` on both panels (null the static UI
and destroy leftover textures), and call them from `LabClosing` next
to the other resets. Do not reset RigEditor twice.

`ResetSession` was added on 2026-08-14 with the reopen-loading fix
([lab-reopen-loading-screen-stuck.md](lab-reopen-loading-screen-stuck.md)),
which is confirmed. Island Atlas / File menu ResetSession is wired.

As of 2026-08-15 in-game: Close Lab after Animator → Slice Atlas → Island
editor → Cancel, then reopen Lab and click the character in the Project
Browser, does **not** load the character. NRE:

```
SimpleUI.UiElement`1.Visible (UiElement.cs:92)  // GameObject.SetActive
LokrLab.Editor.EditHistoryPanel.Fill
EditHistoryPanel.Refresh → BuildInto
LabShell.RebuildBottomPanels → RebuildWorkspaceTabs → Refresh
CharacterLabScene.SwitchToShell
ProjectBrowser.BeginSession / OpenRow
```

`Fill` null-checked `list` but not Unity fake-null `emptyLabel`.
`EditHistoryPanel` had `UnbindDock` but no `ResetSession`, and
`OnLabClosing` did not clear modal/dock refs. After Close Lab,
`dockEmpty` still pointed at a destroyed label.

LokrLab 0.12.30 adds `EditHistoryPanel.ResetSession` (null modal + dock),
calls it from `OnLabClosing`, and `Fill` skips widgets whose
`GameObject` is Unity fake-null. Confirmed 2026-08-15: Close Lab →
Project Browser → open Onagro loads.

Same session: Animator → Slice Atlas after that reopen NREs
`UiModal.Show` (`backdrop.SetActive`) from `MenuBarPanel.OnAtlasMenuClicked`.
`EnsurePopups` only rebuilt when `atlasModal == null`. `UiModal` is a C#
wrapper, so a destroyed popup still looks non-null and Slice Atlas is a
no-op plus NRE. 0.12.31 rebuilds when `GameObject` is missing (same
`IsLive` as EditHistory / `UiFileBrowser.EnsureModal`).

## Proposed solution (Pass 1, 2026-08-15)

**Home:** LokrLab (`MenuBarPanel.EnsurePopups`) after in-game fail
**Approach:** 0.12.30 fixed EditHistory. Slice Atlas after reopen still Show()s a destroyed `atlasModal` because `EnsurePopups` used a C# null check. Match `UiFileBrowser.EnsureModal`: rebuild when `GameObject` is Unity fake-null. `OnAtlasMenuClicked` / `OpenSingleFieldPopup` call `EnsurePopups(Lab.Canvas)` before Show. `IslandAtlasPickerPanel.Build` skips when live; `Open` rebuilds if not.
**Exact change:** Shipped in 0.12.31. Remaining work is the in-game verify below.
**Do not:** Do not reset `RigEditorScene` twice. Do not treat `IslandAtlasPickerPanel.Close` (hide-only) as a bug by itself. Do not move this file to `resolved/` until Close Lab → open character → Animator → Slice Atlas popup appears (and Pick Islands / Cancel still work).
**In-game verify:** 1. Steam / Proton: `dotnet build` so LokrLab 0.12.31 is deployed. 2. Open Character Lab, load a character, open Animator. 3. File → Slice Atlas: popup appears; pick an image; Island editor; Cancel. 4. Close Lab. 5. Reopen Character Lab. Click the same character — it must load. 6. Open Animator. File → Slice Atlas: popup appears (no `UiModal.Show` NRE). Cancel. Pick Islands from that popup still opens. 7. Confirm `LogOutput.log` has no NRE from `MenuBarPanel` / `IslandAtlasPickerPanel` / `EditHistoryPanel` on that reopen.
**Risk:** None. Verify-only; no save, combat, or vanilla change.

Resolved: 2026-08-15

Resolution: Confirmed in-game. Close Lab → reopen → open character loads
(0.12.30 EditHistory `ResetSession` + fake-null `Fill`). Animator →
Slice Atlas a second time after that reopen shows the popup with no
`UiModal.Show` NRE (0.12.31 `EnsurePopups` rebuilds when `GameObject`
is missing).
