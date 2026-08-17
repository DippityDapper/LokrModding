# General workstation


*Implemented (v1) as of 2026-08-11 — see "Implementation status" just
below. The hub's mandatory entry point and the only one available before
a character exists — this is where a character is created or loaded,
where its own identity fields live (as opposed to its rig, abilities,
etc., each owned by a later workstation), and where the tool tells the
user exactly what's left before the character actually works in-game.*

### Implementation status

Built as **three screens instead of one**, all under
`LokrLab/Editor/` and orchestrated by `CharacterLabScene.cs`
(which now boots into a real hub — Load/Home/Properties/Animator — instead
of straight into the Animator):

- **Load** (`LoadWorkstationScene.cs`) — the mandatory entry screen; this
  is what implements §3's "only General available with no character
  loaded" rule in practice — nothing else is reachable from here except by
  creating, loading, or importing a character first. Hosts
  `CharacterListPanel` (Create New / Load Existing... / recent-characters
  list, reusing `FileBrowserPanel`/`RecentFilesStore` exactly as planned)
  and `LegacyModImportPanel` (new, not in the original plan — see below).
- **Home** (`HomeWorkstationScene.cs`) — the hub every other workstation
  returns to; owns `CurrentCharacterFolder`/`CurrentProfile`, the one
  shared-document state every screen reads and writes. Hosts
  `HomeNavPanel` (ID/name summary + buttons to Properties/Animator/Switch
  Character) and `ReadinessChecklistPanel`.
- **Properties** (`PropertiesWorkstationScene.cs`) — the identity-fields
  editor, via `CharacterIdentityPanel`.

This is a heavier split than the roadmap originally sketched (one
"General" tab), but implements the same underlying rules: one shared
`CharacterProfile` document (`Editor/General/CharacterProfile.cs`),
scaffolded upfront on creation, edited in place from whichever screen
touches it rather than a per-screen copy.

**Gap closed 2026-08-11**: the hub itself (`CharacterLabScene`) was a
hardcoded 4-screen switcher when this section was first written — §3's
planned `CharacterCreatorAPI.RegisterWorkstation` extension point now
exists (see §11, resolved), and Properties/Animator both register through
it as ordinary participants rather than being special-cased. "General v1
matches the extensibility model this roadmap set out" for the hub level
too now, not just per-workstation. Verified in-game, not just built — see
§11 for the two regressions that verification pass caught.

### Character creation — scaffold everything upfront

Creating a new character should create every file it's going to need
immediately, not lazily the first time some later workstation happens to
touch it — so no workstation ever has to special-case "this character's
own file doesn't exist yet" as distinct from "this character's file
exists but is empty/default." Concretely, on creation:

- The character's folder (inside `Characters` — renamed from `CharacterRigs`
  2026-08-12 once Sounds/Portraits moved in alongside rig/sprites/definition,
  see §11's own entry on this — keyed by its ID), reusing `CharacterLabPaths`'
  existing mod-folder conventions.
- A new `character.json` sidecar holding the identity fields this
  workstation owns (id, name, description, roster icon/background
  reference) — a new file, since none of this has anywhere to live in
  `rig.json`'s own schema (same "no schema changes, sidecar for
  editor-only data" convention as `rig.pivots.json`/`rig.animsource.json`,
  see [`../../../LokrLab/docs/character/rig-editor-scene.md`](../../../LokrLab/docs/character/rig-editor-scene.md)).
  Unlike those two sidecars, this one is **never optional/lazily
  written** — they're allowed to be absent because a well-defined
  fallback exists (identity pivot, flat-parse load); `character.json`
  holds the character's own identity, which has no such fallback, so it's
  always created, even with placeholder/empty values.
- An empty `rig.json` (`parts: []`, `animations: []`) so the Animator
  workstation can immediately open the new character's folder through
  its existing Load path with nothing special-cased for "brand new."
- Empty placeholder files for whatever later workstations will need, for
  the same "never special-case missing vs. empty" reason. (Abilities
  themselves don't need one of these — per §6, they're not part of a
  character's own folder at all, only referenced by id from it.)

Loading an existing character means picking one of these folders,
re-using the Animator's already-built `RecentFilesStore`/`FileBrowserPanel`
infrastructure rather than a parallel picker.

**Implemented as planned, with one addition**: `HomeWorkstationScene.
OnCreateCharacterConfirmed` creates `rig/`, `sprites/`, `definition/`, writes
`character.json` via `CharacterProfileSidecar`, an empty `rig.json`, then
calls `RLHeroesGenerator.Sync` to write `definition/rlheroes.txt`,
`roster.json`, and `localization_en_US.txt` in the same pass — nothing
lazy, matching the rule above. The one divergence: a brand-new
character's `rlheroes.txt` isn't blank — `RLHeroesGenerator.
PlaceholderTemplate` seeds it from a known-good stats/skills/ability
scaffold cloned from the real shipped "MinionRanger" companion, so a
freshly created character is combat-functional with a stock skillset
immediately, not just visually present. The "empty placeholder files for
later workstations" idea hasn't been needed yet — General doesn't
currently have a later workstation waiting on one (abilities turned out
not to need one at all, since they don't live in a character's own folder
— see §6). Loading reuses `RecentFilesStore`/`FileBrowserPanel` exactly
as planned, via `CharacterListPanel`.

### Identity fields editable here

- **Character ID** — the folder/key name every other piece of data
  (rig, `UnitDefinition`, roster entry, ability defs) is keyed by.
  **Resolved** (was an open question in §11): fixed at creation, never
  editable — `CharacterLabPaths.GenerateNewCharacterId` generates a
  random, opaque integer never typed or seen by the user, and
  `CharacterIdentityPanel` shows it read-only. Rename support was not
  built.
- **Character name** — read by the base game for on-screen display
  (roster card, hero bar, etc.). Implemented.
- **Description** — cosmetic, shown on the roster card/hero info panel.
  Implemented.
- **Roster card icon / background** — see the readiness checklist below;
  there's still no `CharacterAPI` resolver for a *custom* one at all, so
  this remains real implementation work this workstation still needs, not
  just a UI gap. Unchanged from the original plan.
- **Locked** *(not in the original plan)* — a toggle mirroring the real
  `HeroRoster` schema's own `locked` field; a new character defaults to
  locked (no unlock condition wired up yet).
- **Tier** (Companion/Legend) *(not in the original plan)* — which roster
  list (`HeroRoster.AddLegend` vs. `AddCompanion`) the character's entry
  goes into. Set at creation (defaults Companion) or inferred by
  `LegacyModImporter` from an imported mod's own roster file; there's no
  UI to change it after creation.
- Further fields (e.g. a stat block) belong here only until/unless a more
  specific workstation exists to own them — still not resolved, see §11.

### Readiness checklist — the core new idea

A live, always-current, step-by-step list of what's left before the
loaded character actually works in-game, split into two severities:

- **Error** — the character is missing something the base game's own code
  requires to not crash or to be selectable at all. Errors block the
  character from working, full stop.
- **Warning** — incomplete, but the character still functions; usually
  because a sensible default/placeholder covers the gap.

This list is **not hand-maintained by General itself** — each item's
check is owned by whichever workstation the underlying data belongs to
(the Animator's own `AnimatorValidatorRegistry` results, e.g., feed
straight into this list rather than General re-implementing "does this
rig have a Stand animation") — see Extensibility below.

Known items today, compiled from the current Animator/`CharacterAPI`
capabilities. Two rows below were originally marked "unverified" pending
a real end-to-end test (nobody had taken a wholly new, non-reskinned hero
through this whole pipeline yet) — both are now settled as a deliberate
design decision rather than an engine-crash finding: a character missing
either isn't worth shipping regardless of whether the base game literally
throws, so both stay Error. One row (non-required animations) is still
genuinely open, pending investigation with the game's own asset-extraction
tooling — see §11:

| Item | Severity | Why | Owning workstation |
|---|---|---|---|
| Character ID | Error | Every other file/registration (rig folder, `UnitDefinition` key, roster entry) is keyed by this | General |
| Character name | Error | Required for the `UnitDefinition`/roster entry; base game reads it for on-screen display | General |
| At least one part/sprite loaded | Error | Nothing to render — an empty rig can't even preview | Animator |
| `Stand` animation | Error | Every base-game read of `Hero.exoSkeletonDataAsset` (map hero bar, party visual, buff store, reward screen, dialog views) throws without it | Animator (`RequiredAnimationNamesValidator`) |
| `Portrait` or `StandStatic` animation | Error | Same crash class as `Stand` | Animator (`RequiredAnimationNamesValidator`) |
| `UnitDefinition` entry registered | Error | The hero doesn't exist as a data entity to the base game without one | General, via `CharacterAPI.BuildingUnitDefinitions` |
| Roster entry registered | Error | This is literally what "selectable" means — without it the character is valid data the player is never offered | General, via `CharacterAPI.BuildingHeroRoster` |
| Localization entry for the display name | Error | Kept as Error by design (2026-08-11) even though whether the base game literally crashes without one is still unconfirmed — a character with no display name isn't shippable regardless | General, via `CharacterAPI.ContributingLocalization` |
| Description | Warning | Cosmetic only; blank is an acceptable roster-card state | General |
| Roster card icon / background | Warning | Falls back to a base-game default placeholder today | General (needs the new resolver noted above) |
| Sound resolver entries | Warning | Cosmetic; falls back to silence/default | Existing `CharacterAPI.RegisterSoundResolver` |
| Non-required animations (walk/attack/hit/death, etc.) | Warning *(exact required set still unresolved)* | Needed for combat to look right, not for menu selectability; which names combat actually needs is data-driven, not a fixed list — see §11's existing open question | Animator |
| At least one ability | Error *(upgraded from Warning, 2026-08-11)* | Same design-decision reasoning as localization above — a character nobody put an ability on isn't shippable, whether or not it technically crashes | [Ability Lab](ability-lab.md) (built 2026-08-12); hand-authored KV under `Abilities/<id>/` works regardless |

**Implementation status (2026-08-11)**: `CharacterReadinessRegistry`
(`Editor/General/`) is built exactly as this section planned — same
register-by-name, run-all-and-flatten shape as `AnimatorValidatorRegistry`,
plus the Error/Warning split. `RegisterDefaults()` wires in two check
groups covering 10 of the 13 rows above:
- `GeneralReadinessChecks` — Character ID, Character name, Description,
  Unit definition entry (`definition/rlheroes.txt` exists), Roster entry
  (`roster.json` exists), Localization entry, Roster icon.
- `AnimatorReadinessChecks` — At least one part, Stand/Portrait
  animations, Non-required animations (still just the acknowledgment this
  table always said it would be, not a real check — the required set is
  still unresolved, see §11).

Two rows are **not yet implemented** as live checks, both deliberately,
not overlooked: **Sound resolver entries** (no check registered), and
**At least one ability** (no check registered — every new character
currently ships with the MinionRanger placeholder's stock skillset, so
"zero abilities" can't actually happen through General's own Create path
today regardless; a real check belongs with the Ability Lab plugin, §6,
once it exists, not General re-implementing it).
Unlike General/Animator's own registries, `CharacterAPI.
BuildingUnitDefinitions`/`BuildingHeroRoster`/`ContributingLocalization`
aren't called directly by General — a new bridge,
`LokrCharacterLoader/CustomRigs/CharacterLabContentLoader.cs`, subscribes
to those existing events and reads the sidecar files General writes
(`definition/rlheroes.txt`, `roster.json`, `localization_en_US.txt`),
so a Lab-authored character is indistinguishable at load time from a
hand-authored mod using the same events — zero changes needed to
`CharacterAPI.cs` itself, per the "no special-cased fast path for
built-in content" principle in §3.

Explicitly **not** part of this checklist: anything that's purely an
editing-session convenience rather than part of the character document's
own completeness (undo/redo history depth, grid visibility, camera
zoom/pan position, which tool is currently active, etc.).

### Legacy Mod Import *(new, not in the original plan)*

`LegacyModImporter`/`LegacyModImportPanel`, reachable from Load's
"Import Legacy Mod..." button. As of 0.12.13 this is a scan + selection
sheet (see [legacy-pack-port.md](legacy-pack-port.md)), not a
one-shot convert. Converts an old, pre-BepInEx mod folder
(`RLHeroes`/`HeroRoster`/`Localization`/`NewAbilities`/`Sounds`/
`EnemiesDefinitions` subfolders — see `../../lokr-modding` for
what this is importing from) into a General-workstation character: reuses
the exact same `OnCreateCharacterConfirmed` scaffolding path a manual
"Create New Character" uses, then overwrites the parsed identity/roster
fields and copies the old mod's real, hand-authored `rlheroes.txt`
verbatim — `CharacterProfile.ImportedFromLegacyMod` gates
`RLHeroesGenerator.Sync` so it never overwrites that with the generic
placeholder template. Deliberately does **not** convert the rig/
spritesheet (must be rebuilt via the Animator's atlas-import workflow) or
portrait images (no resolver exists yet, same gap as roster card icon/
background above) — both are surfaced as warnings in the post-import
result modal rather than silently dropped.

**Fixed 2026-08-11, two importer bugs found while auditing this against
the old system's own content conventions (`../../lokr-modding/
docs/content-systems.md`)**:
- The copied `rlheroes.txt` kept the old mod's own `UniqueId`/`MetaExo`/
  `Name` values, while `roster.json`/localization were written keyed by
  the *new* random `profile.Id` — since `HeroRosterManager` resolves a
  roster entry via `DefinitionsByUnique[roster id]`, this id mismatch
  meant an imported character's roster entry pointed at a `UniqueId` that
  didn't exist in the unit-definitions table at all: not just a missing
  display name, an unresolvable roster entry. `LegacyModImporter.
  RewriteRLHeroesIdentity` now repoints all three fields at the new
  `profile.Id` after copying, matching how a brand-new character's own
  placeholder template already keys itself.
- Abilities/sounds/enemy definitions were copied into `legacy_abilities/`,
  `legacy_sounds/`, `legacy_enemies/` inside the character's own folder —
  folders nothing reads. **Fixed 2026-08-12** — import now targets the
  current conventions:
  - **Abilities** → `LegacyModImporter.CopyAbilitiesIntoAbilityLabLibrary`
    writes one `Mods/LokrAbilityLab/Abilities/<abilityId>/ability.txt`
    per top-level KV block (folder name = ability id, no character-id
    prefix). Icons and per-ability localization land under each folder's
    `icons/` and `localization_*.txt` when present.
  - **Sounds** → `Characters/<heroId>/sounds/` (nested per character).
  - **Enemy/summon definitions** → separate `EnemySummon` characters under
    `Characters/<generatedId>/` (same opaque-id rule as heroes);
    `LegacyModImporter.RewriteAbilityUnitNames` repoints any Lab ability
    `SpawnUnit` `UnitName` at the new block key. Hand-authored mods can
    still use flat `Mods/*/EnemiesDefinitions/` — the loader accepts both.
  The result modal's warnings were updated to match — portraits/rig are
  still real gaps; abilities/sounds/enemies are not.

A practical on-ramp for migrating this project's own historical mods into the new
workstation-based format; worth folding into §10's phasing note as a
General v1 deliverable even though it wasn't originally scoped.

### Extensibility

Per §3's per-workstation model:

- **Readiness-check registry.** A plugin that adds a new required-or-
  optional field to the character document (its own new sidecar file, an
  extra identity field, a new required workstation altogether) should be
  able to register its own error/warning check into this same checklist —
  mirroring `AnimatorValidatorRegistry`'s existing pattern (see
  [`../../../LokrLab/docs/character/animation-data-model.md`](../../../LokrLab/docs/character/animation-data-model.md))
  but scoped to whole-character readiness instead of just the Animator's
  own rig validity. General's own checklist is this registry's first
  consumer, not a special case of it — each other workstation's existing
  validator registry (where one exists) should register into it too,
  rather than General reimplementing checks that workstation already owns.
  **Implemented** — `CharacterReadinessRegistry` is exactly this, and
  `AnimatorReadinessChecks` is already its second registrant alongside
  `GeneralReadinessChecks`, per the "first consumer, not a special case"
  framing above.

