# 9.2 What a full port already gets for free today (learned from Onagro)


No new code needed for any of this — it's the mechanical part of a port,
already proven end-to-end:

- **Identity, roster placement, localization name/lore** — hand-edit
  `character.json`/`definition/rlheroes.txt`/`localization_en_US.txt`
  directly inside the character's own folder; `CharacterLabContentLoader`
  picks all three up automatically.
- **Abilities and ability icons** — one folder per ability under
  `Mods/LokrAbilityLab/Abilities/<abilityId>/` (`ability.txt`, optional
  `icons/`, optional `localization_*.txt`). Ability ids are stable
  library keys referenced from the hero's `skillProgression` — not
  generated per character.
- **Sounds** — copy WAVs into `Characters/<id>/sounds/` (nested layout).
- **Enemy/summon definitions** — create a separate `EnemySummon`
  character folder under `Characters/<generatedId>/` with its own
  `definition/rlheroes.txt`; rewrite any ability `SpawnUnit` `UnitName`
  to `#<generatedId>`. Or use Legacy Mod Import, which does this
  automatically (see §4).
- **Portraits** — copy into `Characters/<id>/portraits/`,
  matching the character's own `UniqueId`.
- **Rig** — build it for real in the Animator. This is *the* answer to
  "solve the reskin problem": Onagro's rig is a genuine from-scratch
  custom skeleton, not a texture swap of an existing one, and the Official
  Pack survey found only 4 of 16 characters (Arcane Archer, Assassin,
  General, Musketeer) even bothered with the old system's texture-reskin
  option (`Exoskeletons/*.png`) — the other 12 just borrowed an existing
  hero's rig wholesale via `MetaExo`, visually identical to that hero and
  with zero visual customization at all. Even among the 4 that did use it,
  it was still a texture swap on somebody else's skeleton, not a new one —
  evidence the old "solution" was, at best, a partial workaround most
  authors didn't find worth reaching for.

**Three gotchas found doing this by hand for Onagro, worth calling out
explicitly since they're easy to get wrong and silently break the
character instead of erroring:**

1. **`MetaExo` must be repointed at the character's own folder id**, not
   left as whatever the old mod referenced — `CustomRigLoader.Resolve`
   looks up a rig by exact `MetaExo` string match against the folder
   name. Leave it as the old mod's base-game reskin reference and the
   custom rig you built in the Animator never gets used at all.
2. **`Name` (the localization key stem) must equal the character's own
   `Id`**, not whatever casing/value the old mod used internally — General's
   own `RLHeroesGenerator.SyncLocalizationNameAndLore` always upserts
   `UNIT_<Id>_NAME_0001`/`UNIT_<Id>_LORE`, so any mismatch here means
   future edits through the Properties panel silently stop taking effect
   (they'd write to keys the game never reads). This is the exact same
   class of bug the 2026-08-11 `LegacyModImporter` fix closed for the
   *automated* import path — it applies just as much to a manual one.
3. **Set `importedFromLegacyMod: true`** in `character.json` even when
   not going through the Legacy Mod Import button — it's the only thing
   gating `RLHeroesGenerator.Sync` from overwriting hand-merged content
   with the generic placeholder template the next time anything triggers
   a sync (e.g. editing the description in Properties).

