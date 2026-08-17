# Close Lab crashes after deleting the currently-open Character project

Area: LokrLab Character (`CharacterLabScene`, `HomeWorkstationScene` /
`CharacterSession`, `LabContentReloader`)
Status: resolved

As of 2026-08-17 (LokrLab, after the vanilla-override delete-reload fix
in [vanilla-character-edit.md](../../roadmaps/started/vanilla-character-edit.md)
Phase 5): delete the **currently open** Character project from the
Project Browser (e.g. a vanilla-override folder like `gerald_s9abm9`),
then close the Lab. Auto-reload on Lab close throws and the console
shows:

```
[Error :  LoKR Lab] LokrLab CloseTo reload failed: System.IO.DirectoryNotFoundException:
Could not find a part of the path ".../Mods/LokrLab/LokrCharacterLab/gerald_s9abm9/character.json".
  at ... CharacterProfileSidecar.Save (...) CharacterProfileSidecar.cs:253
  at ... CharacterProfileService.PersistToDisk () CharacterProfileService.cs:722
  at LokrLab.LabContentReloader.TryAutoReloadOnLabClose () LabContentReloader.cs:79
  at LokrLab.CharacterLabScene.CloseTo (System.String sceneName) CharacterLabScene.cs:297
```

The delete itself succeeds and the vanilla-override revert (the fix
this issue follows) works correctly — this is a separate, pre-existing
bug that the same repro path surfaces.

## Root cause

`CharacterLabScene.CloseProject()` (`LokrLab/CharacterLabScene.cs:525`)
clears `LokrLabApi.LokrLabApi.CurrentSession` and calls
`LabShell.UnloadProject()`, but neither of those touches
`CharacterSession` (`LokrLab/Character/Editor/General/CharacterSession.cs`),
the Character-specific "which character is active" state. `LabShell`
is a generic, project-type-agnostic shell; it has no knowledge of
`CharacterSession`.

`ProjectBrowser.OnDeleteConfirmed` (`LokrLab/Shell/ProjectBrowser.cs:474`)
calls `CharacterLabScene.CloseProject()` when the deleted row is the
open project, then deletes the folder from disk. `CharacterSession.Folder`
/ `.Profile` (surfaced as `HomeWorkstationScene.CurrentCharacterFolder`
/ `CurrentProfile`) are never cleared, so they keep pointing at the
now-deleted folder.

Later, closing the Lab entirely runs `LabContentReloader.TryAutoReloadOnLabClose`
(`LokrLab/Character/LabContentReloader.cs:70-97`), which checks
`HomeWorkstationScene.CurrentProfile != null && !string.IsNullOrEmpty(CurrentCharacterFolder)`
(line 76-77) — still true, stale — and calls
`CharacterProfileService.PersistToDisk()` → `CharacterProfileSidecar.Save`
(`CharacterProfileSidecar.cs:253`), which does `File.WriteAllText` into
the deleted folder and throws `DirectoryNotFoundException` because the
directory no longer exists.

## Likely fix

`CharacterSession` has no "clear" method today (`SetFolder`/`SetProfile`
only set a value, `CharacterSession.cs:34-43`). Either:

- Add `CharacterSession.Clear()` and call it from
  `CharacterProjectType`'s `OnDeleted` hook (`LokrLab/Character/Projects/CharacterProjectType.cs`,
  the same hook added for the vanilla-override revert fix) when the
  deleted folder matches `CharacterSession.Folder`, or
- Have `CharacterLabScene.CloseProject()` clear it unconditionally when
  the closed session was a Character project (it already knows the
  project type via `LokrLabApi.LokrLabApi.CurrentSession` before
  clearing it).

Either way, `TryAutoReloadOnLabClose` should not attempt to persist a
profile whose folder no longer exists — a `Directory.Exists` guard
before `CharacterProfileService.PersistToDisk()` would also be a
reasonable defense-in-depth check regardless of which state gets fixed.

## Reproduce

1. Open a Character project that overrides a vanilla hero (e.g. an
   `Edit Vanilla Hero…` extract like `gerald_s9abm9`).
2. From the Project Browser, delete that same open project (confirm the
   modal).
3. Close the Lab (return to the atlas screen).
4. Console shows the `DirectoryNotFoundException` above.

Do not mark resolved until a running Lab session can delete the open
project and close the Lab without this exception.

## Fix implemented (LokrLab 0.12.110) — in-game confirm pending

- Added `CharacterSession.Clear()` (`LokrLab/Character/Editor/General/CharacterSession.cs`)
  — resets `Folder` / `Profile` / `EditingLevel` back to "no character
  loaded".
- `CharacterLabScene.CloseProject()` (`LokrLab/CharacterLabScene.cs:525`)
  now captures the closing session before clearing
  `LokrLabApi.CurrentSession`, and calls `CharacterSession.Clear()`
  when the closing project's `ProjectTypeId` was Character. This is the
  general fix: it also covers plain "Close Project" (no delete
  involved), not just the delete-then-close-Lab repro.
- Defense-in-depth: `LabContentReloader.TryAutoReloadOnLabClose`
  (`LokrLab/Character/LabContentReloader.cs:76-81`) now also checks
  `Directory.Exists(HomeWorkstationScene.CurrentCharacterFolder)`
  before calling `CharacterProfileService.PersistToDisk()`, so any
  other path that leaves a stale folder reference skips the persist
  instead of throwing.

**In-game verify:**
1. Open a vanilla-override Character project (e.g. `Edit Vanilla
   Hero…` extract).
2. Delete that same open project from the Project Browser.
3. Close the Lab. Confirm no `DirectoryNotFoundException` in the log,
   and the hero reverts to vanilla (or another remaining override) as
   in the sibling fix.
4. Also confirm plain Close Project (no delete) still persists edits
   normally — the `CharacterSession.Clear()` call must not skip a
   legitimate save.

## Resolved (2026-08-17)

Confirmed in the running game: deleting the open override and closing
the Lab no longer throws; the vanilla-override revert still works
correctly alongside it.
