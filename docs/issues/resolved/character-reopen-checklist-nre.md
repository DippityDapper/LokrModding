# Character project: second Open of Onagro NREs on readiness checklist

Area: LokrLab Character (`ReadinessChecklistPanel`, `HomeWorkstationScene`)
Status: resolved

As of 2026-08-14: open Onagro, Close Lab, reopen the lab, open Onagro
again. `CharacterProjectType.Load` calls
`HomeWorkstationScene.OnLoadCharacterSelected`, which refreshes
`ReadinessChecklistPanel`. That panel still holds `emptyLabel` / `list`
from the previous lab scene (Close Lab does not `Unbind`). The C# refs
are non-null; the GameObjects are destroyed. `Visible()` then
NullReferenceExceptions (`UiElement.cs:92`). Project Browser open
aborts.

HomeNavPanel already no-ops when `GameObject == null`. The checklist
only checks C# null.

Suggested fix: treat destroyed widgets as unbound in `Refresh`; call
`Unbind` from `LabClosing`; Load should still succeed if Home chrome is
not built.

Resolved: 2026-08-14

Resolution: LokrLab 0.12.3. `ReadinessChecklistPanel.Refresh` no-ops
when the widgets are Unity fake-null (`IsLive`). `Unbind` runs from
`CharacterLabHooks.OnLabClosing`. `HomeWorkstationScene.ResetSession`
clears the same statics. Confirmed in game: Close Lab, reopen, open
Onagro again without the checklist NRE.
