# Vanilla Ability Edit

**Status:** Not started — research first  
**Raised:** 2026-08-17  
**Last updated:** 2026-08-17  
**Owner:** LokrLab Ability + LokrCharacterLoader

Old-system authors often change **shipped skills** (`gerald_swing`,
`sasquatch_smash`, traits) in place. Ability Lab today only authors
**new** `slug_token` abilities. This track researches opening a vanilla
ability in the card editor and choosing **override** (same id, global)
vs **fork** (new id, vanilla untouched).

Sibling tracks: [vanilla-character-edit.md](../completed/vanilla-character-edit.md),
[vanilla-encounter-edit.md](vanilla-encounter-edit.md). Catalog already
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

Index the extract + overhaul HTML catalog
([`abilities.html`](../../api/character-reference/abilities.html)).
Show envelope, event list, opaque action types, icon/FX names. Do not
imply override yet. Document override vs fork in hover / help.

### Phase 2 — Copy-into-library pipeline

Copy `_extracted/base-game/AbilitiesScript/<id>.txt` into
`AbilityLabPaths.AbilityDefinitionPath`. Branch:

- Fork: mint `slug_token`, rekey with `AbilityIdentityRekey` patterns.
- Override: mint `slug_token` folder; keep vanilla block key; warn that
  this is global.

Pull `SKILL_<id>_*` and referenced `COMBAT_MODIFIER_*` into
`localization_en_US.txt`. Vanilla `Icon` string still resolves from the
bundle unless the author adds `icons/<Icon>.png`.

Reuse `AbilityKvIO.TryLoad`, `LegacyModImporter` loc copy. If override
needs a load-path flag (`project.json` / folder convention), add it
on `AbilityLabContentLoader` rather than a second importer.

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
