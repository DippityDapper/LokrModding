# LokrAbilityLab


*Decided 2026-08-11, before any code exists for it: ability authoring does
**not** become a workstation inside the Character Creator hub the way §4/§5
did. It becomes its own plugin — working name `LokrAbilityLab` — that
`LokrCharacterLab` neither depends on nor is depended on by. Goal
unchanged from the original plan: the same expressive power the mod's
existing ability system already has (`CharacterAPI.RegisterAbility` /
`AbilitiesDefinitions` KV-text + Lua — targeting, damage, conditions,
status effects, all well covered per the existing capabilities
assessment), reached through forms and pickers instead of hand-written
KV/Lua text. What changed is *where that lives*, not what it does.*

**Naming note**: the plugin this section repeatedly cites as precedent and
depends on for `CharacterAPI` was itself renamed 2026-08-11 —
`LokrCharacterCreator` → **`LokrCharacterLoader`**, GUID
`com.lokrmodding.charactercreator` → `com.lokrmodding.characterloader`.
Same plugin, same code, same responsibilities (it never had any
character-*authoring* logic of its own to move — `LokrCharacterLab`
already owned 100% of that); the old name was actively misleading once
`LokrCharacterLab` existed as the thing that actually creates characters,
for the exact same "the name should say what it does" reason `LokrAbilityLab`
is a plugin and not a workstation. If anything elsewhere (old commit
messages, a stale local build, memory) still says `LokrCharacterCreator`,
treat `LokrCharacterLoader` as current — every reference in this roadmap
below already uses the new name.

### Why a separate plugin, not a workstation

§3's hub model gates every workstation but General behind "a character is
currently loaded" — the workstation operates on *that* character's one
document. Abilities don't fit that shape: the full-port survey (§9) found
abilities routinely shared across several characters (`Empty Units`' nine
`empty_units_immune_*` traits, `Resources/NewAbilities/`'s cross-character
files), not owned by any one character's document at all. Bolting a
"shared library" concept onto a workstation model built around "one open
project" would be fighting the model, not using it.

This mirrors a split this codebase already made once: `LokrCharacterLoader`
(runtime content-loading) and `LokrCharacterLab` (the authoring tool) are
separate plugins specifically so the authoring tool can be iterated on
aggressively without dragging the stable runtime patches along — see
`LokrLabPlugin.cs`'s own doc comment. Ability authoring being
comparably complex to the whole rig-authoring system already in
`LokrCharacterLab` (stats, conditions, effects, targeting, and — per §8 —
eventually real scripting) is the same signal: folding it into
`LokrCharacterLab` risks turning it into the kind of monolith that split
was meant to avoid in the first place. A full custom-scripting story for
abilities (§8) also reads more naturally as an extension *of* a dedicated
Ability Lab plugin than as an extension of a Character Creator workstation
that happens to host abilities.

**Concretely, `LokrAbilityLab`:**
- Depends on `LokrModAPI` and `LokrCharacterLoader` (for `CharacterAPI`),
  same as `LokrCharacterLab` does — **not** on `LokrCharacterLab` itself,
  and `LokrCharacterLab` doesn't depend on it either. Both are decoupled
  siblings under `LokrCharacterLoader`, exactly like `LokrCharacterLab`
  and `LokrEncyclopedia` already are today. Ability content flows between
  them purely through `CharacterAPI`/file conventions — a character
  references an ability by id in its own `skillProgression`; neither
  plugin needs to know the other exists at compile time.
- Owns a **shared, mod-wide ability library** as its native storage model
  — not per-character. Concretely, its own convention (e.g.
  `Mods/LokrAbilityLab/Abilities/<id>/`) rather than nesting inside any
  character's own folder. This makes "an ability shared by several
  characters" the *default* case, not a workaround, closing §9.3.C's
  shared-ability gap by construction rather than by special-casing it.
  ~~**Migration note**: the 2026-08-11 `LegacyModImporter` fix currently
  copies abilities into `Mods/LokrLab/NewAbilities/` with an
  id-prefix, since this plugin didn't exist yet — that was always a
  stop-gap. Once `LokrAbilityLab` ships, `LegacyModImporter` (or whatever
  migration path exists by then) should target its convention instead,
  and the id-prefix hack goes away since sharing is no longer something
  to work around.~~ **Done 2026-08-12.** `LegacyModImporter.
  CopyAbilitiesIntoAbilityLabLibrary` now parses each legacy ability file
  with the real `KVLib` parser and writes one file per top-level ability
  block straight into `CharacterLabPaths.AbilityLabAbilitiesRoot`
  (`Mods/LokrAbilityLab/Abilities/`), named after that block's own real
  ability id — no character-id prefix, since id-based naming is the whole
  point of a shared library where sharing isn't a workaround. Onagro's own
  9 already-imported custom abilities (plus one unreferenced leftover,
  `onagro_smokescreen`) were manually migrated the same way as part of
  this change — moved, not copied, so `Mods/LokrLab/NewAbilities/`
  has nothing left in it. Ability icons now live under each ability folder's
  `Abilities/<id>/icons/<iconName>.png` (with flat `Mods/*/AbilityIcons/`
  kept as a fallback for hand-authored mods). `PortraitPatches.cs` checks
  nested icons first, then the flat folder. Onagro's icons were migrated
  into their per-ability `icons/` subfolders; the leftover flat
  `Mods/LokrAbilityLab/AbilityIcons/Goblidrones.png` is unused.
  **Still not handled**: legacy-import icon copy — see
  [`../../issues/resolved/legacy-import-skips-ability-icons.md`](../../issues/resolved/legacy-import-skips-ability-icons.md).
- Reads a character's own stat-field names (from the character document
  General/the Animator already own) when building targeting/damage
  formulas, but does **not** own or author a character's stats,
  level-chain progression, states, or sound config — those stay Character
  Creator's own concern (General/a future Stats sub-feature); see §9.3.C's
  updated split. `LokrAbilityLab` only owns ability *logic* and the
  library it lives in.
- Does **not** author enemy/summon creatures either. Per §9.3 (updated),
  a "this ability spawns a creature" effect just references an entity —
  hero or enemy — built with the Character Creator's own tooling
  (General + Animator), the same way it'd reference any other ability.
  `LokrAbilityLab` picks from entities that already exist; it doesn't
  create them.

### Scope

This is a **visual front end over an existing, already-solid backend** —
the ability *logic* isn't the gap, the authoring experience is.
Concretely:

- A structured editor (fields, dropdowns, condition builders) that reads
  and writes the same `AbilitiesDefinitions` shape `RegisterAbility`
  already consumes, so an ability built here is indistinguishable at
  runtime from one written by hand today.
- Its own hub, in the same spirit as `LokrCharacterLab`'s Load/Home
  screens but shaped around a library instead of "one open project": a
  browsable, searchable list of every ability in the shared library, plus
  a form-based editor for whichever one is currently open. No per-ability
  "load a character first" gate, since there's no character to load.
- ~~Targeting, damage, conditions, and status-effect authoring all get
  dedicated UI — this is the part of the existing system already confirmed
  to be well covered, so the job is presenting it clearly, not extending
  its logical power.~~ **Revised 2026-08-12, before building v1**: research
  against real code and ~190 real shipped ability files found the
  condition/effect/status-effect *body* (`OnAbilityAction`/
  `AbilitySpecial`/`Modifiers`/`AIConfigB`) is an open ~90-type action
  catalogue with arbitrary nesting — well covered as *engine capability*,
  yes, but not a fixed schema a dropdown-based form can realistically
  cover in a first pass, contrary to what this bullet originally implied.
  v1's actual split (see §10 item 4): **targeting flags, range, cooldown,
  AP cost, AOE shape, and icon/animation/localization refs** — the real
  flat, closed portion of `Ability.cs`'s own schema — get a dedicated
  form; damage/condition/status-effect *logic* stays raw KV text for now.
  Dedicated condition/effect/status-effect UI is real, later scope (see
  the Extensibility note just below, now with nothing to hook into yet),
  not v1's own job. **Shipped 2026-08-13 as overhaul Phase 4**
  ([ability-lab-overhaul.md](ability-lab-overhaul.md),
  LokrAbilityLab 0.4.0): nested action cards replaced that raw-body box
  as the primary editor. This v1 doc stays the placement/rationale
  record.
- **Explicitly deferred to a later extension** (see §8): custom ability
  **VFX** (particle effects, hit impacts, projectile sprites) and abilities
  that need a **new animation** to play are both blocked on asset-pipeline
  gaps larger than this plugin alone — VFX assets are still an
  asset-bundle-only ceiling today, and a new cast/attack animation is a
  cross-plugin dependency on the Animator's attach-point support (§5, in
  `LokrCharacterLab`). Track both here as blockers to revisit once the
  Animator work in §5 lands, not as day-one scope.

### Extensibility

Same resolver-chain philosophy as every other extension point in this
roadmap (§3), just scoped to this plugin instead of a hub workstation: the
form-based editor's condition/effect/targeting pickers shouldn't be a
closed, hardcoded list — a plugin introducing a new gameplay mechanic (via
its own ability logic registered through the existing
`CharacterAPI.RegisterAbility`/KV-text surface, or eventually through the
Custom Scripting extension in §8) should be able to register first-class
UI for authoring that mechanic here too, instead of it only being
reachable by hand-writing KV/Lua text. This mirrors how
`PortraitPatches`/`SoundPatches` register their default resolvers into
`CharacterAPI` at ordinary priority: a plugin's contributed condition/
effect/targeting type is just another entry in the same list the built-in
ones live in, not a special case.

