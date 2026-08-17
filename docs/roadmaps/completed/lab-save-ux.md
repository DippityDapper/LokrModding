# Lab save UX

**Status:** Complete — confirmed in-game 2026-08-15  
**Raised:** 2026-08-14  
**Last updated:** 2026-08-15  
**Owner:** LokrLab (shell + Character / Ability sessions)

Surfaces that need a manual save (Animator first, then any dirty
project) should behave like a normal editor.

See also [roadmaps/README.md](../README.md).

---

## What exists (0.12.25–0.12.26)

[LabSaveUx.cs](../../../LokrLab/Shell/LabSaveUx.cs) owns dirty chrome:

- Animator `CaptureBeforeChange`, Ability form/card edits, and (since
  2026-08-17) Character Properties field edits all set `session.IsDirty`
  instead of writing through — see
  [`CharacterProfileService.MarkDirtyAndRefresh`](../../../LokrLab/Character/Editor/General/CharacterProfileService.cs).
  Aliases (`LabAliasesInspector`) still write through and do not use the
  flag.
- File → Save and Ctrl+S call the same write path as Animator Save /
  Ability Save, then clear dirty.
- The LoKR Lab title on the menu bar (right of File / Edit / View / Help)
  and the status bar show `*` while dirty.
- Close Lab, Close Project, and jump/return prompt save / discard /
  cancel. Cancel stays in the project.

**In-game confirm (2026-08-15):** dirty `*` after an unsaved Animator
edit, Ctrl+S / File → Save clears it, Close Lab / Close Project / jump
with dirty state offers save / discard / cancel.

---

## Phases

1. [x] **Wire `IsDirty`** — set true on Animator pose/clip/rig writes
   and Ability form edits. Clear on successful save and on load.
2. [x] **Ctrl+S** — save the current project. Same path File → Save
   uses.
3. [x] **Title indicator** — `*` on the LoKR Lab title (not only the
   status-bar name), driven by the same `IsDirty` flag.
4. [x] **Close prompt** — Close Lab, close project, or leave with
   unsaved changes: save / discard / cancel. Cancel stays in the
   project.
