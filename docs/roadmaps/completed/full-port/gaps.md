# 9.3 What's missing for a true full port


Grouped by which workstation should own closing it, per §3's own
resolver-chain-not-a-closed-list principle — none of this should require
forking core code, matching how everything else in this roadmap is
supposed to extend.

**A. General workstation (§4) — extend the identity, stats, and entity model**

Everything below is Character Creator's own concern, not `LokrAbilityLab`'s
(§6) — abilities only *reference* a character's stats/states by id, they
don't own or define them:

- ~~**Achievement-gated unlock.**~~ **Resolved 2026-08-12.**
  `CharacterProfile.UnlockAchievement` (string, alongside `Locked`) is
  live end to end: `CharacterProfileSidecar` round-trips it through
  `character.json`, `RLHeroesGenerator.WriteHeroRoster` emits it into
  `roster.json` as `"unlockAchievement":"<id>"` (omitted when empty,
  matching the old system's own convention), and it's editable via
  `CharacterHeroRosterPanel`'s Unlock Achievement dropdown — not a
  free-text field, but a live list read straight from
  `MetagameManager.instance.Player.AchievementManager.
  AchievementDefinitionConfig.achievements`, so it can't drift from the
  real achievement set. This also settles §9.6's "generic hook or
  hardcoded set" open question: the old system's own
  `HeroRosterManager.cs` confirms `achievementManager.IsCompleted(id)`
  takes any achievement id generically, matching what this dropdown
  offers.
- ~~**Multi-locale localization.**~~ **Implemented and verified in-game
  2026-08-12.** Went with the full structured editor, not the lighter
  paste-in-raw-text alternative also considered: `CharacterProfile.
  Localizations` (`Dictionary<string, CharacterLocalizedText>`, keyed by
  locale-suffix — see new `LocaleCodes.AllNonEnglish`, the 15
  non-English `LanguageCode` values as their file-suffix strings, e.g.
  "es", "zh-Hans") holds a Name/Description pair per locale, alongside
  the existing English `Name`/`Description` fields (English itself isn't
  a `Localizations` entry). Round-trips through `character.json` the
  same array-of-objects convention every other collection field here
  already uses (`States`/`SoundClips`). `RLHeroesGenerator.Sync`'s old
  `SyncLocalizationNameAndLore` became `SyncLocalizationFile`, now called
  once for English plus once per `Localizations` entry, writing
  `localization_<suffix>.txt` per locale with the same
  `UNIT_<Id>_NAME_0001`/`UNIT_<Id>_LORE` upsert logic as before. A new
  "Localization" Properties category (`CharacterLocalizationPanel`,
  mirroring `CharacterSoundPanel`'s `UiList` add/remove-row pattern) is
  the UI — an "Add locale" dropdown offers only locales not yet added,
  each row is Name + Description text fields plus a remove button. On
  the read side, `CharacterLabContentLoader.OnContributingLocalization`
  no longer hardcodes `LanguageCode.EN` — a new
  `LocaleFileSuffixes` map (mirroring `LocaleCodes.AllNonEnglish`,
  duplicated across the `LokrCharacterLab`/`LokrCharacterLoader` plugin
  boundary rather than shared, since the two plugins don't reference
  each other) lets it merge whichever `localization_<suffix>.txt` file
  exists for any of the 16 `LanguageCode` values, not just English.
  **Verified in-game against Onagro** (2026-08-12), not just built — the
  add-locale/edit/remove round trip was clicked through for real: a
  locale added via the dropdown, its `localization_es.txt` file
  confirmed created with the right `UNIT_*_NAME_0001`/`UNIT_*_LORE`
  lines, and removing the row deleted the file again. Same "clean build
  is necessary, not sufficient" lesson §11 already documents from the
  hub-level extensibility work, applied here proactively instead of
  reactively.
- **Roster card banner / map token resolver.** Resolved 2026-08-11 — this
  turned out to be two already-solved problems and one likely non-problem,
  not one big gap. Investigation (prompted by the Official Pack survey's
  16/16-characters-hit-this claim) found: `Icon` and `UnitOnMap` are
  vestigial KV fields the base game never actually reads — the real
  lookups are `DataHelper.LoadHeroBanner` (keyed off the hero's own id,
  bundle-asset-name lookup, `Asst_Banner_<heroId>`) and
  `PartyTokenComponent`'s `ExoSkeletonData.ReplacePart` (an ExoSkeleton
  part swap, not a sprite lookup) — both of which
  `CharacterAPI.RegisterPortraitResolver`'s existing `"BANNER"` and
  `"MAPMINI"` slots (`PortraitPatches.cs`, `ExoSkeletonDataPatches.cs`,
  `PartyTokenComponentPatches.cs`) already cover, no new code needed.
  `GeneralReadinessChecks` now checks for the real files
  (`Characters/<id>/portraits/<id>_BANNER.png` /
  `<id>_MAPMINI.png`, with fallback to flat `Mods/*/Portraits/<id>/`) instead of unconditionally warning. `Background`
  is the one field investigation couldn't find *any* rendering path for
  anywhere in decompiled source — every hero-room/roster-list screen
  checked either doesn't reference it or reads a different field
  (`portraitBackgroundColor`, a hex tint, not this). Left as a low-priority
  open question rather than assumed dead outright, since decompilation
  can't rule out a Lua- or serialized-scene-driven path — see §11-style
  caution around `LokrEncyclopedia`'s unverified button. Not worth a
  resolver until/unless a live consumer is actually found.
- ~~**Arbitrary custom stat fields.**~~ **Implemented 2026-08-11.** The
  placeholder template's ~8 fixed stats were nowhere near the real
  ceiling — the Official Pack alone adds `evasion`, `aoeRadius`,
  `maxTargets`, `maxBolt`,
  `infernalSacrificeDamage`/`infernalCombustionDamage`, `holyPoints`,
  `fireBarrier`, `epicHeal`, `currentBolts`, `armorOfThornsDamage`,
  `burnDuration`, and more — each invented per-character and referenced
  from that character's own abilities via `stat(%CASTER, #fieldName)`.
  `CharacterProfile.Stats` (a `List<StatEntry>`, name+value) is now real
  Lab-owned data, persisted in `character.json` and driving
  `rlheroes.txt`'s `stats` block directly — an "add a custom field"
  affordance, not a fixed dropdown list, the same "resolver-chain, not a
  closed list" commitment §3 already makes elsewhere in this roadmap. A
  new `CharacterStatsPanel` (Properties workstation, right of the
  identity form) lists every stat as an editable name/value row with
  Add/rename/remove, seeded from the old placeholder's 8 defaults on
  character creation so a brand-new character still fights out of the
  box. Disabled (with an explanatory notice) for imported characters,
  whose real stats live in their own untouched `rlheroes.txt` instead —
  same boundary `RLHeroesGenerator.Sync`'s `ImportedFromLegacyMod` check
  already draws. A new readiness-checklist row (`GeneralReadinessChecks.
  CheckStats`) flags a non-imported character with zero stats as an
  Error. Skills/skillProgression/soundConfig are still the static
  MinionRanger-cloned footer template, untouched by this change — they
  remain their own separate sub-items below.
- ~~**Placeholder regeneration silently discards hand-edited content for
  non-imported characters.**~~ **Fully resolved 2026-08-11.** Found
  2026-08-11 as a stats-only risk, then closed entirely the same day as
  part of the full-absorption work below: `RLHeroesGenerator.Sync` now
  always regenerates the *entire* `rlheroes.txt` (every field, every rank)
  from `CharacterProfile`'s own data — never a hardcoded template, never a
  preserved-raw-file passthrough — so there is nothing left outside the
  Lab's own model for a resync to silently overwrite, for any character.
  The old `ImportedFromLegacyMod`-gated "skip regeneration entirely" path
  is gone; see this bullet's own history (still in git) and the
  full-absorption bullet directly below for how the risk that made this
  matter for imported characters specifically was closed.
- **Full absorption: imported characters are Lab-owned data end to end.**
  Raised and resolved 2026-08-11. Until this point, a `LegacyModImporter`
  character (Onagro included) kept its old mod's `rlheroes.txt`
  byte-for-byte untouched forever (`ImportedFromLegacyMod` gated
  `RLHeroesGenerator.Sync` from ever rewriting it) — meaning Stats and
  every other Properties field were disabled/uneditable for exactly the
  one real character this project uses as its own validation case,
  directly contradicting the "everything editable in the Lab, no raw
  files" goal. Closed by building `RLHeroesParser`, a real parser (KVLib,
  the same library the base game's own `UnityDefinitionsParser`/
  `AbilitiesDefinitions` use, not a regex) that reads an *entire* existing
  `rlheroes.txt` into `CharacterProfile`'s fields at import time; from
  then on `RLHeroesGenerator.Sync` always regenerates the file fresh from
  that profile, for every character alike. `ImportedFromLegacyMod` is now
  historical provenance only, no longer a behavior gate. **This changed
  scope mid-flight**: the first pass only modeled a single KV block, but
  Onagro's real file turned out to be a 3-block rank-up chain
  (`InheritsFrom`/`nextLevelArchetype`) plus a `states` block neither the
  parser nor the data model had any concept of — parsing it with the
  single-block assumption would have silently discarded Onagro's level-2/3
  stat growth and its LEGEND flag. Caught by a round-trip verification
  pass (parse → regenerate → re-parse Onagro's actual file, diffed
  field-by-field) *before* ever running this against the real file — see
  the Level-chain progression and States bullets below for what that
  turned into. Verified: **every field round-trips exactly** through
  Onagro's real file.
- **Properties workstation restructured: category nav + shared inspector,
  not a fixed panel row.** Resolved 2026-08-11, prompted directly by the
  full-absorption work above running out of room — Properties had grown
  to 3 fixed side-by-side panels (Portraits/Identity/Stats), then a whole
  separate Advanced workstation for 3 more (Appearance/Skills/Sound), and
  adding Levels + States on top of that would have meant a 4th and 5th
  hardcoded panel with nowhere left to put them. Replaced with the same
  shape as the Animator's own Scene Tree → Inspector pattern: a left-hand
  category list (`PropertiesNavPanel`) and one shared right-hand inspector
  whose content swaps per category, built once and toggled visible/hidden
  rather than rebuilt per click (`InspectorPanel`'s own "build everything,
  toggle Visible" approach). `PropertiesCategoryRegistry.RegisterCategory`
  is the extension point — adding a new field group now means one more
  registration call, not a new panel-and-region pair, addressing the
  concrete problem that motivated this. The standalone Advanced
  workstation is gone; General/Hero Roster/Portraits/Level Properties/
  States/Appearance/Skills/Sound are now all categories of one Properties
  workstation.
- ~~**Level-chain progression.**~~ **Resolved 2026-08-11.** The
  `InheritsFrom`/`nextLevelArchetype` multi-block pattern (a level-1 block
  plus override-only level-2/3+ blocks, each just the stats that changed)
  is now a first-class part of the character model —
  `CharacterProfile.Levels` (a `List<CharacterLevel>`), parsed from and
  regenerated back into the real multi-block file by
  `RLHeroesParser`/`RLHeroesGenerator`. The Level Properties category
  (Properties workstation) is the UI: a tab per rank, Add/Remove Level,
  and that rank's own stat-override rows. This was the discovery that
  turned "make stats editable for imported characters" into a full
  redesign — Onagro's real file turned out to be exactly this pattern (3
  ranks), not the single block the earlier stats-only pass assumed; see
  the full-absorption bullet below.
- ~~**States (flags).**~~ **Resolved 2026-08-11.** LEGEND, combat
  immunities (`CANT_BE_POISONED`/`STUNNED`/`ROOTED`/etc.), and behavior
  flags (`NOT_IN_INITIATIVE_BAR`/`NON_TARGETABLE`/`FIXED_POSITION`/etc.)
  are all just KV pairs, so this went the open-ended route rather than a
  fixed checklist — `CharacterProfile.States` (`Dictionary<string, bool>`)
  plus a States category (add/remove/toggle rows), the same "resolver
  chain, not a closed list" principle §3 already commits to for stats.
- ~~**Sound config (combat-event sounds).**~~ **Resolved 2026-08-11**, as
  a plain-text editor (the Sound category's two fields: asset id, and one
  `event=clip` line per sound) rather than a polished per-event picker —
  still real, round-tripping `CharacterProfile` data (`SoundAssetId`/
  `SoundClips`), not raw-file editing. `soundConfig`'s `assetId` still
  references a shared, existing base-game sound-group name with no
  resolver for a fully custom one (same asset-bundle-reference ceiling §6
  already flags for VFX) — that ceiling is unchanged, only the *editing*
  gap closed.
- **Enemy/summon entities.** Ownership resolved 2026-08-11 (an enemy/summon
  creature is just another entity, built with General/the Animator the same
  way a hero is — not something `LokrAbilityLab` authors). The actual
  entity-type distinction this implied — **implemented and verified in-game
  2026-08-12**. `CharacterProfile.EntityType`
  (`CharacterEntityType.Hero`/`EnemySummon`, defaults `Hero`) round-trips
  through `character.json` the same way `Tier` does. `RLHeroesGenerator.
  WriteRLHeroes` branches on it: an `EnemySummon` block omits `UniqueId`
  and the roster/portrait-facing fields (`Icon`/`Background`/`UnitOnMap`/
  `PortraitBackgroundColor`), and writes `InheritsFrom "Base"` instead of
  `"Hero"` — matching real shipped `EnemiesDefinitions/*.txt` files
  (`Demons.txt`'s `SummonedDemonSpawn`) field-for-field. `MetaExo` is kept
  either way, deliberately diverging from those real files (which never
  carry it) — real `EnemiesDefinitions` entries only ever reskin an
  existing hero's rig, but this tool's whole point is a real
  Animator-authored custom rig, which needs `MetaExo` to resolve
  regardless of entity type. `RLHeroesGenerator.Sync` skips writing
  `roster.json` entirely for an `EnemySummon` (and deletes a stale one if
  the type was just switched away from `Hero`) rather than writing an
  always-locked placeholder — confirmed via decompiled
  `UnityDefinitionsParser.GetDefinition` that a unit definition with no
  `UniqueId` and no roster entry is already a normal, fully-supported
  base-game lookup case (`GetDefinition` looks up by KV block key only,
  never touches the roster-facing `definitionsByUnique` index), not
  something needing new engine accommodation. On the loader side,
  `CharacterLabContentLoader.OnBuildingUnitDefinitions` now reads
  `character.json`'s own `entityType` and calls `CharacterAPI.
  UnitDefinitionsBuilder.AddEnemyDefinition` instead of
  `AddHeroDefinition` for an `EnemySummon`, matching how real
  `EnemiesDefinitions/*.txt` content is categorized (mechanically this
  likely doesn't change lookup behavior either way, since both fragment
  lists get merged into the same unified `definitions` dictionary before
  parsing, but matching the base game's own category is the lower-risk
  choice over asserting that doesn't matter). `GeneralReadinessChecks`'
  Roster entry/Roster banner/Map token checks (hero-roster-specific) now
  skip entirely for a non-`Hero` entity rather than flagging phantom
  gaps. UI: a new toggle on the General category
  ("Enemy/Summon (off = playable Hero...)") — deliberately editable at
  any time, not fixed at creation like Character ID, since switching type
  is just a data-model concern `RLHeroesGenerator.Sync` already handles
  declaratively. **Known simplification, not fixed**: the Hero Roster
  category (Locked/Tier/Unlock Achievement) stays visible and editable
  for an `EnemySummon` even though none of it is written anywhere while
  in that state — harmless (the values are preserved and reused if
  switched back to `Hero`) but not hidden from the nav, since
  `PropertiesCategoryRegistry` has no per-entity-type visibility concept
  yet. Also out of scope here: the Animator's own required-animation
  checks (`Stand`/`Portrait`) still apply uniformly regardless of entity
  type — whether a pure combat-spawned `EnemySummon` genuinely needs the
  same map-hero-bar-facing animations a roster `Hero` does is unexamined,
  left as-is rather than guessed at.

**B. Animator (§5) — cross-link, not new capability**

The Animator doesn't need new capability for a full port — it already
*is* the fix for the reskin problem, per §9.2. The one real gap is
documentation/discoverability: the old system's animation-triggered sound
events (`walk`/`step`/`roar`/`howl`/`cheer`/`boo`/`victory`, found across
several Official Pack characters, distinct from the combat-hook sounds in
`soundConfig`) read as exactly what the Animator's own `events` system
(frame-triggered gameplay hooks, already shipped per §5's status table)
was built for. Nothing currently cross-links "old system had a sound
here" to "author it as an Animator frame event" — worth a short
`docs/animation-data-model.md` note once someone actually ports a
character that used one, to confirm the mechanism lines up before
promising it does.

**C. `LokrAbilityLab` (§6) — a much smaller list now that it's a separate plugin**

Most of what used to live here turned out to be General's own concern
(§9.3.A) once abilities got their own plugin — that's the actual value of
the 2026-08-11 architecture decision, not just "abilities live somewhere
else now." What's left is genuinely `LokrAbilityLab`'s:

- **Shared/library abilities** — already resolved by construction, not a
  gap: see §9.1's table and §6's own "why a separate plugin" section.
  Nothing to design here beyond what §6 already commits to.
- **Custom condition/effect/targeting types** stay extensible per §6's own
  Extensibility subsection — already planned, not a new full-port finding,
  just confirming the Official Pack survey didn't turn up anything that
  breaks that model.
- Everything else — VFX, new-animation abilities — stays deferred to §8
  per §6's own stated scope, unchanged by this survey.

**D. One remaining unowned gap**

- **Shared, mod-wide resources.** The Official Pack ships a `Resources/`
  folder that isn't a character at all — `DEFAULT_*.png` fallbacks,
  roster-banner backgrounds reused across many heroes by archetype
  (`Crafty_BANNER.png`, `Athletic_BANNER.png`), and a global
  `properties.txt`. (The fourth thing that used to be here — cross-character
  ability files — is resolved along with the rest of §9.3.C, since
  `LokrAbilityLab`'s shared library *is* the place for those now.) Nothing
  in General's design has a place for "content that belongs to the mod as
  a whole, not any one character" — this mirrors §11's existing "document
  format stability" open question one level up: worth deciding
  deliberately, the same way that question already asks to, rather than
  discovering it ad hoc the next time two characters need to share
  something.

**E. Explicitly out of scope for the Character Creator tool**

`new_heroes_lib/Lua` in the Official Pack (`WastelandTavern.lua`,
`SecondWind.lua`, `combat_tutBreach.lua`, and others) is map/tavern/quest
scripting used to narratively introduce a new hero into the game world.
A full grep of every `RLHeroes`/`NewAbilities` file across the entire
Official Pack turned up zero references to Lua from a character's own
definition or abilities — this is purely map-level content, squarely the
later, separate Encounter Creator / Custom Adventures territory (§8), not
something a *character* port needs to solve. Flagging its existence here
only so a future full-port effort doesn't mistake it for a Character
Creator gap.

