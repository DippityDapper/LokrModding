# Aliases editor still writes through immediately, unlike Properties now

Area: LokrLab Shared (`LabAliasesInspector`)
Status: unresolved

As of 2026-08-17: Character Properties field edits were changed to mark
the session dirty (`LabSaveUx.MarkDirty`) instead of writing
`character.json` to disk on every change — see
[`CharacterProfileService.MarkDirtyAndRefresh`](../../../LokrLab/Character/Editor/General/CharacterProfileService.cs)
and [`lab-save-ux.md`](../../roadmaps/completed/lab-save-ux.md).
`LabAliasesInspector` (`LokrLab/Shared/LabAliasesInspector.cs`) has the
same pre-save-system write-through pattern for `aliases.json`, and was
deliberately left alone in that pass — not part of what was asked, and
structurally different enough to need its own design pass (see below).

Every field-change callback writes immediately via `LabAliases.Save(folder, map)`:
- `LabAliasesInspector.cs:43` — "Add alias" button
- `LabAliasesInspector.cs:67` — id field `OnEndEdit`
- `LabAliasesInspector.cs:73` — row "x" delete button
- `LabAliasesInspector.cs:100` — key rename (`Rename` helper, from
  `nameField.OnEndEdit` at `:64`)

Reached from the Character Properties node tree via
`CharacterInspectorDrawers.DrawAliases` →
`LabAliasesInspector.Draw` (`LokrLab/Character/Projects/CharacterInspectorDrawers.cs:239`),
and identically from `AbilityLibraryNodes.cs:196` and
`EncounterNodes.cs:1194` — this is shared across all three project types,
not Character-specific.

## Why this one is structurally different from the Properties fix

`CharacterProfileService`'s mutators all read/write
`CharacterSession.Profile`, an in-memory model that already exists for
the whole editing session — deferring its persist to Save just meant
routing through `LabSaveUx.MarkDirty()` instead of writing immediately.

`LabAliasesInspector` has no equivalent in-memory model: `Draw` loads the
alias dictionary fresh from disk every time it's shown
(`LabAliasesInspector.cs:30`) and writes it back immediately on every
edit. Deferring this to Save would need a small session-scoped holder for
the pending map (keyed by folder) that `Draw` reads from if present, that
`LabSaveUx.TrySaveCurrent` flushes to disk, and that gets cleared/reloaded
on Discard — a smaller version of the same three-part fix Properties just
got (mark dirty on edit, add a real save-path write, make Discard actually
discard).

## Reproduce

Not a bug in the sense of broken behavior — same as Properties before its
fix, aliases just always persist immediately regardless of Ctrl+S/Save/
Discard, so the manual save system's dirty tracking doesn't cover it.

Do not mark resolved until aliases participate in the same dirty-flag /
Ctrl+S / Discard flow Properties now does, confirmed in-game.
