# Human-readable unique ids

**Status:** Complete — phases 1–6 in LokrLab 0.12.9–0.12.24 / LokrCharacterLoader 1.1.9. Leftover numeric folders stay valid until the user Renames them.
**Raised:** 2026-08-14  
**Last updated:** 2026-08-15  
**Owner:** LokrLab (create / write) + LokrCharacterLoader (load expand)

Lab packages must stay hand-editable without keeping the old
`Mods/*/RLHeroes/` + `NewAbilities/` layout as the authoring surface.
Two community “Necromancer” packs must both load and be playable
together. New users must not research existing ids.

See also [roadmaps/README.md](../README.md).

---

## Constraints

- Display name may repeat (`Necromancer` + `Necromancer` in the roster).
- Engine keys (folder, `UniqueId`, block key, `MetaExo`, roster id,
  `UNIT_*` stem, `SpawnUnit` `#word`) must be unique without a global
  census.
- `#UnitName` must start with a letter (ability expression `word`
  grammar). See
  [sandbox-summon-missing-unit-view.md](../../issues/resolved/sandbox-summon-missing-unit-view.md).
- Hand-edit means the Lab package
  (`Mods/LokrLab/LokrCharacterLab/<id>/`, Ability Lab `ability.txt`)
  is readable and complete. The loader may still read leftover flat
  folders; that is not the promised workflow.
- A bare folder `necromancer` under the shared write root
  `Mods/LokrLab/LokrCharacterLab/` overwrites on disk. The same id
  first-wins in
  [CharacterLabContentLoader](../../../LokrCharacterLoader/CustomRigs/CharacterLabContentLoader.cs)
  and in `DefinitionsByUnique` / roster.

---

## Recommended identity

- **Display name** — player-facing, duplicates OK (`Necromancer`).
- **Slug** — the human stem of the id (`necromancer`). The user can
  edit it. Default / **Auto** fills it from the display name in
  lowercase (strip to a legal stem: start with a letter, `[a-z0-9_]`).
  Turning Auto off keeps the typed slug when the name changes.
- **Id** — `slug` + short random token, e.g. `necromancer_k7m2p9` or
  `necromancer_ad8174`. This is the folder name and every engine key
  listed above, including `SpawnUnit` `#necromancer_ad8174`. The token
  is Lab-minted; the user does not pick it.
- Create flow: user types the display name. Slug auto-fills from that
  name (lowercase) and stays editable. Alias Auto copies the current
  slug; turning it off lets the user pick a shorter `$alias` key
  (`necro` while the folder is `necromancer_ad8174`). Empty alias
  falls back to the slug. Confirm mints the token.
  Local `Directory.Exists` + retry, same shape as
  `CharacterLabPaths.GenerateNewCharacterId`.
- Optional author prefix is just a slug edit (`tooth_necromancer`),
  never a required field.
- Existing 18-digit folders stay valid; optional later rename onto
  `slug_token`. Changing the slug after create is that same rename
  (folder + engine keys), not a live in-place edit.
- **Token** — exactly 6 Crockford / base36 characters. That length is
  the settled size (readable, local `Directory.Exists` + retry is
  enough; do not shorten or lengthen).

---

## Per-folder aliases

Add-on, not a replacement for unique ids. Each Lab package folder may
have `aliases.json`, a local map from short name to that folder’s
unique id:

```json
{
  "necromancer": "necromancer_ad8174"
}
```

Loaders expand aliases **only in files in that same folder** before
the game sees the text. Alice’s `"zombie"` and Bob’s `"zombie"` never
meet.

Authored files write `$necromancer`. Lab should keep that form on save
so a round-trip does not bake tokens into every line.
`CharacterLabContentLoader` / `AbilityLabContentLoader` substitute
`$key` → value, then `LabExpressionIds.RewriteAbilityText` turns a
`SpawnUnit` `UnitName` unique id into a `#word` (`$necromancer` →
`necromancer_ad8174` → `#necromancer_ad8174`). Without that prefix the
ability parser treats the id as a function name. See
[alias-unitname-parsed-as-function.md](../../issues/resolved/alias-unitname-parsed-as-function.md).

**`$alias` only.** Do not replace any KV value that happens to equal a
key (`InheritsFrom "Hero"`, vanilla `Model`, display strings). Do not
use `#` for aliases — the ability grammar already uses `#` as a string
literal.

Ability folders are not inside the character folder
(`LokrAbilityLab/<library>/<ability>/`). An ability that summons a
zombie must list `"zombie": "zombie_def456"` in **that ability
folder’s** `aliases.json`. Lab copies the mapping when a summon is
linked. No cross-folder lookup. A later library-level `aliases.json`
can overlay; v1 is per-folder only.

Hand-edit: open `LokrCharacterLab/necromancer_ad8174/`, read
`aliases.json` as the index, edit `definition/rlheroes.txt` with
`$necromancer`, edit the ability’s own `aliases.json` and `$zombie` in
`ability.txt`.

---

## Lab UI

These features are authored in the Lab, not only by editing files.

### Create / identity

[CharacterCreateSheet](../../../LokrLab/Character/Projects/CharacterCreateSheet.cs)
gains a **Slug** field next to the display name, plus an **Auto**
control that fills the slug from the name in lowercase (legal stem).
Auto on: name edits refresh the slug. Auto off: the typed slug stays.
An **Alias** field with its own Auto (default on) copies the current
slug; turn it off to pick the `$alias` key independently. The
6-character token is minted on confirm and is not a form field.
Show a read-only preview of the full id (`slug` + `_` + token, or
`slug_??????` until confirm).

The Character node inspector already shows `Id` and folder
([CharacterInspectorDrawers.DrawCharacter](../../../LokrLab/Character/Projects/CharacterInspectorDrawers.cs)).
Keep the full id read-only there. Leftover numeric / named folders
show Slug + Alias + Auto + **Rename** (folder + engine keys). Do not
make slug a silent live edit.

Ability create uses the same name / slug / alias / Auto / token
pattern when that folder needs a unique id. Library display name
stays a display name; the library folder itself is also a
`slug_token` on new creates, with Rename on leftover numeric
library folders (including the shipped `placeholders` library;
`project.json` `placeholdersLibrary` keeps install pointed at the
moved folder). Leftover ability folders (`new_ability`,
18-digit ids) get Rename on the Library card and inspector.

### Aliases node

`aliases.json` is a first-class Node Tree entry, not a hidden file.

- **Character** — one **Aliases** node, sibling of Abilities
  (`CharacterNodeKinds` + a contributor next to
  `ContributeAbilities`). One list for
  `LokrCharacterLab/<id>/aliases.json`.
- **Ability** — one **Aliases** child on each Ability node (that
  ability folder’s `aliases.json`). Not on the library root in v1
  (library-level overlay stays later).

Selecting the node draws the map in the **inspector**: a list of
rows, each short name → unique id, with add / remove / edit. Seed
the package’s own self-alias on create (the Alias field, defaulting
to the slug: `necromancer` → `necromancer_ad8174`). Linking a summon
appends a row on the ability list (phase 3) and still shows up here.

The inspector is the Lab surface; `aliases.json` remains the
hand-edit file. Saving the list writes that file. Do not invent a
second store.

---

## Rejected options

| Option | Why not |
|---|---|
| Slug-only id (`necromancer`) | Two Necromancer packs collide (folder overwrite, first-win skip). |
| Digits-only id | Unreadable; illegal as `#word`. |
| Forced `LokrLab_` prefix | Two Lab Necromancers still collide. |
| Rewrite colliding ids at load | `SpawnUnit #necromancer` would hit the wrong unit. |
| Bare-key alias expand | Accidental rewrite of vanilla keys and display strings. |
| Keep old `RLHeroes/` + `NewAbilities/` as the hand-edit surface | Hand-edit means the Lab package, not the pre-Lab layout. |

---

## Phases

1. [x] Mint `slug_token` on create (editable slug, Auto-from-name
   lowercase on the create sheet); keep existing numeric ids loadable.
2. [x] Write and expand per-folder `aliases.json` / `$alias` (character +
   ability folders). Lab save preserves `$alias`.
3. [x] **Aliases node** on Character (sibling of Abilities) and on each
   Ability; inspector list edits the map. Seed the self-alias on
   create.
4. [x] When linking a summon, copy the target’s alias into the ability
   folder’s `aliases.json` (row appears in that Ability’s Aliases
   inspector).
5. [x] Optional: rename existing 18-digit / leftover folders onto
   `slug_token` (slug + alias + Auto + Rename on the Character and
   Ability inspectors). Does not auto-rekey on load.
6. [x] Legacy import mints `slug_token` for abilities and characters
   (same as create) and seeds per-folder `$alias` from leftover pack
   keys. Existing imported folders keep leftover ability ids until
   re-import.
