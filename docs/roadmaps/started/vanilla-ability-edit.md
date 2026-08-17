# Vanilla Ability Edit

**Status:** Started — Phases 1–2 confirmed in-game 2026-08-17  
**Raised:** 2026-08-17  
**Last updated:** 2026-08-17  
**Owner:** LokrLab Ability + LokrCharacterLoader

Old-system authors often change **shipped skills** (`gerald_swing`,
`sasquatch_smash`, traits) in place. Ability Lab today only authors
**new** `slug_token` abilities. This track researches opening a vanilla
ability in the card editor and choosing **override** (same id, global)
vs **fork** (new id, vanilla untouched).

Sibling tracks: [vanilla-character-edit.md](../completed/vanilla-character-edit.md),
[vanilla-encounter-edit.md](../not-started/vanilla-encounter-edit.md). Catalog already
exists from the [Ability Lab overhaul](../completed/ability-lab-overhaul.md)
Phase 1 extract.

---

## Why research first

**Runtime override already works** because `AbilitiesDefinitionsPatches`
last-writes. Lab UX, round-trip safety, and blast-radius warnings are
the missing product. The load path is still **ours to tweak** if
override needs scoping (only when a Lab folder is marked vanilla
source), uninstall/reload edge cases, or modifier-id merge rules. Do
not invent a second ability VM; do change `ExecuteLoad` if the current
global last-wins is too blunt.

Vanilla abilities are KV1 `TextAsset`s under `Balance/AbilitiesScript`.
Block key = `abilityId`. Parsed by `AbilityParser.ParseAbility` into
`AbilitiesDefinitions.instance.abilities`. Extract: 431 ids under
[`docs/character-reference/_extracted/base-game/AbilitiesScript/`](../../character-reference/_extracted/base-game/AbilitiesScript/).

Example: [`sasquatch_smash.txt`](../../character-reference/_extracted/base-game/AbilitiesScript/sasquatch_smash.txt)
— `OnCustomTargeting`, `ActOnHexas`, `AddAsAffected`, `AIConfigB`. Those
action types are **opaque cards** in the current editor.

---

## What works today

| Action | Result |
|---|---|
| New Ability | Always mints `slug_token`. Five Lab seed templates, not vanilla copies. |
| Library browser | Lab library folders only. No vanilla catalog. |
| Pickers | Reference vanilla FX / clip / sound **names**. Do not edit those assets. |
| Legacy pack import | Official Pack `NewAbilities/*.txt`. **Always mints** a new id. |
| Hand-written override | A Lab `ability.txt` whose block key is `sasquatch_smash` **does** replace vanilla at load. |

`AbilitiesDefinitionsPatches.ExecuteLoad`:

1. Vanilla `ResourcesWrapper.LoadAll<TextAsset>(abilitiesFolder)`
2. `CharacterAPI.BuildingAbilities` (`NewAbilities/` then
   `AbilityLabContentLoader`)
3. Parse in list order: `__instance.abilities[ability.abilityId] = ability`
   (last write wins). Same for `ability_modifiers[modifierId]`.
4. `CharacterAPI.RegisterAbility` applied last.

Loc: `localization_<suffix>.txt` in the ability folder merges last-wins
for `SKILL_<id>_*`. Icons: nested `icons/` wins over bundle
`LoadSkillIcon`.

Allowed Loader tweaks (only if product needs them):

- Apply last-wins only for folders marked as vanilla override, so a
  mis-keyed Lab id cannot silently replace Smash.
- Reload / uninstall: drop the override and restore the vanilla
  `Ability` object, not a stale Lab parse.
- `ability_modifiers` collision policy (Lab replace vs merge keys).

**On-delete refresh (do not skip).** Character's equivalent track hit
this: deleting an override folder from the Project Browser left the
last-merged state in memory until a full restart, because nothing
called a reload after the delete. Fixed there by adding
`ProjectTypeRegistration.OnDeleted` (`LokrLabApi/ProjectTypeRegistration.cs`)
— a per-project-type hook the browser invokes after a successful
delete — and wiring `AbilityLibraryProjectType` (or the per-ability
delete path inside the library, if deletion is node-level rather than
project-level for this type) to reload merged ability content the same
way `CharacterProjectType.OnCharacterDeleted` does. See
[vanilla-character-edit.md](../completed/vanilla-character-edit.md) Phase
5 and [character-close-lab-crash-after-deleting-open-project.md](../../issues/resolved/character-close-lab-crash-after-deleting-open-project.md)
for a follow-on bug to watch for: also clear any stale "currently
open" session state that still points at the deleted folder, or Lab
close can crash trying to persist into a folder that no longer exists.

---

## Product decision (gate)

**Override.** Folder is a minted `slug_token`. The KV **block key**
stays `sasquatch_smash`. Every unit that references that id (vanilla
Sasquatch, campaign, Lab heroes) gets the new template. One in-memory
`Ability` per id — no per-adventure scope. Same split as Character:
folder name is unique per author; engine id last-wins.

**Fork.** Mint a new id; rewire only Lab characters. This is today’s
default create path. Vanilla Smash unchanged.

v1 should make **both** explicit. Default create stays fork. “Edit
Vanilla Ability…” copies extract KV into a library and asks override vs
fork before opening the editor.

Non-goals: a second ability VM; editing `Mods/*/Lua/` script files
(that is `CharacterAPI.ResolvingScript`, a parallel path).

---

## Research phases

### Phase 1 — Reference browser (read-only)

**Status:** Complete — confirmed in-game 2026-08-17.

Correction to the original plan: [`abilities.html`](../../api/character-reference/abilities.html)
is a hand-maintained schema page, not a real catalog — the actual
per-ability catalog is `docs/api/character-reference/skills/<id>.html`
(431 pages, generated by `docs/character-reference/generate_skills_catalog.py`),
and neither is deployed with a shipped build anyway (`docs/character-reference/`
and `docs/api/` are dev-repo-only — see
[`git-and-releases.md`](../../git-and-releases.md)). Built against **live
game data instead**: `VanillaAbilityCatalog`
([`LokrLab/Ability/Editor/VanillaAbilityCatalog.cs`](../../../LokrLab/Ability/Editor/VanillaAbilityCatalog.cs))
reads the same `Resources.LoadAll<TextAsset>(abilitiesFolder)` bundle
`AbilitiesDefinitionsPatches.ExecuteLoad` does, so it always reflects
vanilla only (Lab/mod overrides never land in that Resources folder) and
works in a shipped build with no extract dependency. A handful of bundle
TextAssets hold more than one top-level ability block (e.g.
`_basicAbilities.txt`), so `AbilityKvIO` gained
`LoadFromKeyValue`/`LoadAllFromText` to split those — `TryLoad`'s
single-root file path is unchanged, Lab-authored folders still use it.

File → **Browse Vanilla Abilities...**
([`VanillaAbilityBrowserModal.cs`](../../../LokrLab/Ability/Projects/VanillaAbilityBrowserModal.cs),
registered with no `isVisible` guard like Edit Vanilla Hero, so it works
before any library is open) — searchable list, then a read-only detail
view: envelope fields, icon/FX names, the event list (cards per event,
opaque ones marked), a distinct-opaque-action-types summary with their
raw KV shown verbatim, plus modifiers/AI/other-unrecognized blocks for
completeness. Opens nothing, copies nothing, implies no override/fork
yet. Hover copy on the modal and an explicit override-vs-fork explainer
row (`ability.vanilla.Browse`, `ability.vanilla.OverrideVsFork` in
[`LokrLab/Sidecars/ability-hover.md`](../../../LokrLab/Sidecars/ability-hover.md),
with compiled fallbacks in `AbilityHoverCopy.LoadDefaults`).

**In-game verify:** confirmed 2026-08-17.

### Phase 2 — Copy-into-library pipeline

**Status:** Complete — confirmed in-game 2026-08-17 (Gerald's sword ability, both Override and Fork).

Same source correction as Phase 1: copies from `VanillaAbilityCatalog`'s
live-read cache (`VanillaAbilityCatalog.FindSourceText`), not the
dev-only `_extracted/base-game/AbilitiesScript/<id>.txt`.
[`VanillaAbilityImporter`](../../../LokrLab/Ability/Editor/VanillaAbilityImporter.cs)
writes the vanilla KV block text back out close to verbatim (not a
`TryLoad`→`AbilityFileModel`→`TryBuildText` round-trip, which is lossy —
see Phase 3, and the class's own remarks) into a newly minted
`slug_token` folder in both modes:

- **Fork:** rewrites the block key and, if present, an explicit
  `LocalizationId` field to the minted id, so `SKILL_<mintedId>_*`
  resolves independently of vanilla.
- **Override:** keeps the vanilla block key untouched inside
  `ability.txt`; only the *folder* is a minted `slug_token`, so two
  overrides (or an override sitting next to an unrelated ability) never
  collide on disk. Confirmed no "warn this is global" UI yet — that's
  Phase 4's confirm modal, not built here.

Localization: pulls `SKILL_<id or LocalizationId>_*` and every
referenced `COMBAT_MODIFIER_<id>_*` (modifier ids from
`Body.Modifiers` and `ApplyModifier`/`RemoveModifier`'s `ModifierName`
field) from the **live merged `LocalizationManager.instance.DatabaseClone`**
table (not the dev-only extract either) into `localization_en_US.txt`.
Vanilla `Icon` string is left untouched — confirmed it already resolves
from the bundle unless the author later adds `icons/<Icon>.png`
(`PortraitPatches.LoadSkillIcon_Patch`).

Drive-by fix bundled with this: `AbilityEventNames.AllModifierEvents`
was missing `OnSpawn` (present in `DefaultModifierEvents`, so the
add-event menu already offered it) — silently rejected any `TrySave` of
the ~13 vanilla files whose modifiers use it. Found while scoping this
pipeline; unrelated to the raw-text-copy approach above but affects the
same files, so fixed alongside it.

Exposed via **Copy into Library (Override / Fork)** buttons on the
Phase 1 browser's detail view — not a File → "Edit Vanilla Ability..."
menu entry, since Phase 3 gates that until round-trip fidelity is
measured. Targets the currently open library only — no longer falls
back to "the first library that happens to exist" or silently mints one
(changed 2026-08-17 after the first pass did exactly that: both a
Fork and an Override copy landed in whatever library was open, which
worked but wasn't the intent). `ResolveTargetLibrary` now returns null
and the copy buttons show a status message telling the author to open
the target library first.

**Suspected gap, disproven in-game:** worried the Ability Library
browser's node tree (built from folder names,
`AbilityLabPaths.EnumerateAbilitiesIn`, not each folder's parsed block
key) would not reliably navigate to an Override copy, since its folder
name (minted) and `ability.txt` block key (vanilla) intentionally
diverge. Tested directly: both the Override and Fork copies showed up
in the Assassin library browser, and Open correctly loaded the Override
copy's content — so this is not a live bug for at least this case. Left
as a documented risk rather than deleted outright, since only one
ability was tested; revisit if a future copy doesn't open correctly.

**Delete bug found and fixed 2026-08-17:** deleting an Override copy
from its own editor ("Delete" button, `AbilityEditorPanel.OnDeleteClicked`)
silently no-opped. It deleted via
`AbilityListPanel.DeleteAbility(current.Id)`, which re-derives the
folder from the id through `AbilityLabPaths.FindLibraryFolderForAbility`
— an id→folder-name lookup that assumes they're equal, true for every
Lab-authored ability but not for an Override copy (folder name is
minted, block key stays vanilla, by design). The lookup found nothing
and returned early. Fixed by deleting from
`AbilityFileModel.SourceFilePath`'s own directory instead (added
`AbilityListPanel.DeleteAbilityFolder`) — exact, no re-derivation, works
for both modes. `Save` was already safe (`TrySave` prefers
`SourceFilePath` over an id-based path); this was specifically a Delete
bug.

### Phase 3 — Round-trip fidelity audit

No “Edit Vanilla” button until this is measured.

Stratified sample (~30–50 ids): simple melee, projectile, passive,
point AOE, `OnCustomTargeting`, Lua, CallFunction. Pipeline: extract →
`AbilityKvIO.TryLoad` → `TryBuildText` → diff.

Classify: benign reorder vs semantic (`#RANGED` → `#PROJECTILE` rewrite,
omitted empty keys). `sasquatch_smash` is the hard case (opaque
`ActOnHexas` / `AddAsAffected`, `ActOnTargets` Target block in ExtraKv).

`test-suite.md` already names AbilityKvIO corpus tests; they are not
written.

### Phase 4 — Blast radius (“Used by”)

`AbilityUsage` only sees loaded `UnitDefinition` + Lab `character.json`.
Extend the scan to extracted `RLHeroes_new.txt` and EnemiesDefinitions.
Warn on global modifier id collisions (`ability_modifiers` is flat).
Confirm modal for override save. Consider a blocklist (tutorial /
progression ids).

### Phase 5 — In-game confirm protocol

- Override Smash damage only → vanilla Sasquatch in Sandbox / a campaign
  spot-check
- Fork wired to one Lab hero → vanilla Sasquatch unchanged
- Save → `ReloadLabContent(Abilities)` → no parse errors
- Tooltip tokens (`{MinDamage}`) still resolve after loc edit

---

## Open questions

1. Product default: keep fork-first, or first-class override editing?
2. Acceptable round-trip: semantic equivalence vs byte-stable KV?
3. Block save when opaque-card ratio is high in override mode?
4. Who references an id beyond units (encounters, Lua `TriggerSkill`)?
5. Import all locales or English first?
6. Share UI with Legacy Pack import or keep vanilla import separate?

---

## Related docs

- [ability-load-path.md](../../ability-load-path.md)
- [AbilitiesDefinitionsPatches](../../../LokrCharacterLoader/Patches/AbilitiesDefinitionsPatches.cs)
- [AbilityLabContentLoader](../../../LokrCharacterLoader/CustomRigs/AbilityLabContentLoader.cs)
- [AbilityKvIO](../../../LokrLab/Ability/Editor/AbilityKvIO.cs)
- [AbilityUsage](../../../LokrLab/Ability/Editor/AbilityUsage.cs)
- [ability-lab-overhaul.md](../completed/ability-lab-overhaul.md)
- [LokrLab ability architecture](../../../LokrLab/docs/ability/architecture.md)
