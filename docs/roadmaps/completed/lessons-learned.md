# Lessons learned (historical)

Resolved bugs, migrations, and verification notes from Character Creator
development. **Active** open questions and deferred work are in
[open-questions.md](../open-questions.md) — read that first.

**Last updated:** 2026-08-13

---

# Open questions / risks (archived raw log)

*Full-port-specific open questions live in [full-port/open-questions-port.md](full-port/open-questions-port.md).*

- ~~**Whether General's Error-severity "unverified" items really are
  errors.**~~ **Resolved 2026-08-11, two ways at once.** Onagro (see §9)
  went through creation → roster → in-game with both localization and a
  full ability set present, and confirmed working — the first real
  end-to-end test this question was waiting on. Separately, both items
  are now kept as Errors **by design** regardless of what that test
  showed (a character missing either isn't worth shipping, whether or not
  the engine itself would hard-crash without them — see §4's readiness
  checklist table). The one thing this doesn't confirm is
  the pure-curiosity question of what happens if localization/abilities
  are *removed* from an otherwise-working character — genuinely untested,
  but now low-priority, since the policy answer doesn't depend on it.
- ~~**Character ID mutability.**~~ **Resolved** in General v1: the ID is
  fixed at creation and never editable — `CharacterLabPaths.
  GenerateNewCharacterId` generates a random, opaque integer, never typed
  or shown as editable to the user (`CharacterIdentityPanel` displays it
  read-only). Rename support was not built; the simpler of the original
  two options was chosen.
- ~~**Hub-level extensibility (`CharacterCreatorAPI.RegisterWorkstation`)
  wasn't built alongside General v1**~~ **Built 2026-08-11.** `CharacterCreatorAPI`
  (`LokrLab/CharacterCreatorAPI.cs`) now exposes `RegisterWorkstation(name,
  displayLabel, build, onShow, onHide, requiresCharacterLoaded)`, mirroring
  `CharacterAPI`'s own shape one level up. `CharacterLabScene` no longer hardcodes
  Properties/Animator as special screens — both register through this exact same
  surface at startup (`RegisterBuiltInWorkstations`), same as `PortraitPatches`/
  `SoundPatches` register into `CharacterAPI` as ordinary participants, no special
  fast path. Home's nav panel builds its workstation buttons from
  `CharacterCreatorAPI.Workstations` dynamically instead of two hardcoded buttons,
  so a third-party plugin registering a new workstation gets a Home button for it
  for free. Load and Home stay hardcoded — they're the hub's own mandatory shell
  (entry point and return point), not workstations in this sense, per §6's own
  distinction. Landed before Sandbox (§10 item 5), as intended — the "second
  first-party workstation" case this section used to warn about now goes through
  the registry from day one instead of adding a 5th hardcoded screen.
  **Verified in-game 2026-08-11**, not just built — and that verification pass
  caught two real regressions the build alone didn't, both fixed the same day:

  1. **A workstation that used to build eagerly can't assume its own static
     state is already initialized by the time Home's eager refresh runs.**
     Properties went from building eagerly (in the old hardcoded
     `CharacterLabScene`) to lazily (via the registry, matching Animator's
     already-lazy pattern) — but `HomeWorkstationScene`'s own eager refresh
     still unconditionally called into `CharacterIdentityPanel.Refresh`
     (Properties' own panel) on every Home build, before Properties had ever
     been shown. Crashed with a `NullReferenceException` on the very first
     screen, since nothing had built the panel yet. Fixed by making
     `CharacterIdentityPanel.Refresh` a no-op until `Build()` has actually
     run — safe, since `Build()` always calls `Refresh()` itself right after
     constructing the panel, so the first real display is still correct.
  2. **`UiList<T>` defaults to `scrollable: true`, and a scrollable list has
     no self-sizing behavior of its own** — it just fills whatever height its
     parent hands it (per `UiStack`'s own doc comment), which collapses to
     zero inside a `VerticalLayoutGroup` unless given an explicit
     `FixedHeight`/`Grow()`. Home's new dynamic workstation-button list had
     neither, so its buttons existed in the hierarchy (correctly built, correct
     click handlers) but were invisible inside a zero-height, clipped
     viewport — only the unrelated, separately-fixed-height "Switch Character"
     button was ever visible. Fixed by passing `scrollable: false` instead
     (gets `UiStack`'s own `ContentSizeFitter`, sizing to exactly the
     buttons' combined height) — the right call regardless of the bug, since
     this list is small and curated by `RegisterWorkstation`, not
     open-ended like `CharacterListPanel`'s recent-characters list.

  Neither gap showed up in `dotnet build` or in the doc-comment/API-surface
  checks — both were only findable by actually launching the game and
  clicking through Home, the same lesson §9.2's Onagro gotchas already
  taught about hand-porting a character: a clean build is necessary, not
  sufficient, for "this workstation actually works."
- ~~**Combat-required animation names.**~~ **Resolved 2026-08-12**,
  corrected 2026-08-12 after Onagro combat threw `Can't play animation
  SpecialAttack`. Combat instantiates `CharacterProfile.Model` from the
  `units` bundle, then `UnitViewExoSkeletonPatches` swaps the custom rig
  onto that prefab. Controllers look up **`sequenceName`** (not the
  ability's `AnimationID`, which is the *controller* name) via
  `FindAnimationIndex`; a miss **throws** from
  `ExoSkeletonUnitAnimationController.StartAnimation`. Those names are
  **per-Model**, not a global HumanArcher list. Dumped from the `units`
  bundle: HumanArcher uses angled `Attack0`/`SpecialAttack0`/…;
  ObeliskLvl4 (Onagro) uses un-angled **`SpecialAttack`** for both Attack
  and SpecialAttack controllers; HumanGeraldLightSeeker uses `Attack` plus
  `SpecialAttackA/B/C`. `CombatSequenceNames.ForModel` is the source of
  truth; Save backfills that list; the Add Animation modal offers the
  union as presets; `AnimatorReadinessChecks` warns for missing
  `ForModel` names. Extra clips the Model never looks up are harmless.
- ~~**Lab characters used a placeholder rig in combat.**~~ **Resolved
  2026-08-12.** `HeroExoSkeletonPatches` (map/roster/portrait) and combat
  turned out to be two entirely disconnected rig-resolution paths.
  Combat instantiates a unit's view from a real, baked vanilla prefab
  (`UnitViewManager.InstantiateUnitView` → `FindPrefab(unit.kind)` →
  `AssetBundleManager.LoadAsset<GameObject>("units", kind)`, resolved from
  `UnitDefinition.kind`, which the parser sets from the `"Model"` KV key —
  `RLHeroesGenerator.WriteRLHeroes` writes `CharacterProfile.Model`,
  defaulting to `"HumanArcher"`, a real vanilla unit). None of that reads
  `MetaExo`/`CharacterAPI.ResolveExoSkeleton` at all, so a Lab character's
  own custom rig never had anywhere to plug in for combat specifically —
  the vanilla `HumanArcher` model (or whatever `Model` names) rendered and
  animated instead, correctly, just not as the character's own art.
  Fixed with one new patch, `LokrCharacterLoader/Patches/
  UnitViewExoSkeletonPatches.cs`: a `Postfix` on
  `UnitViewManager.InstantiateUnitView` that resolves the same custom rig
  `HeroExoSkeletonPatches` already resolves for the map
  (`CharacterAPI.ResolveExoSkeleton(unit.unitDefinition.metaExo)`) and, if
  found, swaps it onto the freshly-instantiated view's
  `ExoSkeletonAnimator.data` via `ExoSkeletonData.UpdateAsset` — a real,
  purpose-built base-game method for exactly this, not a workaround — then
  re-runs `PreloadAnimationIds()` on every
  `ExoSkeletonUnitAnimationController` on the view (there are several per
  unit, one per named sequence; each caches its own animation index against
  whichever asset was active at the time, so each needs refreshing after
  the swap). `Model` keeps serving as "which vanilla prefab acts as the
  visual/animation-rig template" — unchanged, no schema change needed —
  while `MetaExo` (already correctly wired for every Lab character) is what
  actually decides whether a custom rig replaces that template's art.
  **Verified in-game 2026-08-12** — Onagro's own rig loads for its combat
  view (`CustomRigLoader` log confirms it building from the character's
  own folder right before the fight scene loads) — but that same test run
  surfaced a follow-up crash, see the next entry.
- **Missing combat animation names could hang the whole game, not just
  skip the animation.** Found 2026-08-12 testing the fix above through a
  real story quest (not just Sandbox's minimal 1v1): a scripted intro
  cinematic threw `Exception: ... Can't play animation Run` from
  `ExoSkeletonUnitAnimationController.StartAnimation`, then the game hung
  on a black screen. Root cause, traced into decompiled source
  (`CinematicUtils.MoveUnitOnPath`, the code that walks a unit onto screen
  for a cinematic): it calls `StartAnimation` unconditionally once it
  finds a same-named `AnimationController` at all, with **no `CanPlay`
  check first** — unlike every other real call site, a missing clip here
  isn't safely inert, it's a hard, uncaught exception, severe enough to
  freeze the cinematic's own coroutine (and the game along with it) rather
  than just skip that one animation. The GameObject name in those
  exceptions (`UNIT-Nightshade-ObeliskLvl4`) is the combat-view prefab
  (`CharacterProfile.Model`), not a different unit — Onagro's Model is
  `ObeliskLvl4`, so that *is* Onagro. The first Save backfill used
  HumanArcher's angled `Attack0`/`SpecialAttack0` names, which ObeliskLvl4
  never looks up; the clip combat actually needs is un-angled
  `SpecialAttack`. **Fixed** by making `OnSaveClicked` backfill
  `CombatSequenceNames.ForModel(CharacterProfile.Model)` instead of the
  HumanArcher-only preset list, so `FindAnimationIndex` never returns -1
  for a name that prefab's controllers actually look up. Extra clips the
  Model never uses are left in place (harmless). A defensive
  `LokrPatch`-style patch making `StartAnimation`/`Play` itself tolerate a
  missing clip everywhere, not just for Lab-authored rigs, was considered
  but not built, since it touches core combat animation playback broadly;
  flagged here as a possible follow-up, not done speculatively.
- ~~**Combat attach points and AbilityAction events.**~~ **Resolved
  2026-08-12.** After the clip-name fix, Onagro's attack played but never
  spawned the missile. Two gaps on the swapped custom rig, not on the
  ability file: (1) every frame had empty `attachPoints`, so
  `unitPosition(%SOURCE, #Head)` resolved to the origin and the cinematic
  logged `Attach point name: Head not found in GameObject
  UNIT-Nightshade-ObeliskLvl4` — that GameObject name is the Model prefab
  (`ObeliskLvl4`), while the mesh is already Onagro's `MetaExo` rig;
  sockets after the swap come from the custom asset, not Obelisk's
  original art. (2) `SpecialAttack` had empty `events`, so
  `AbilityMeleeActivity` never saw `AbilityAction` (projectile) or
  `AbilityEnd` (activity finish). `CombatPlaybackRequirements` is the
  source of truth; Save backfills `Head`/`Chest`/`Base` and those two
  events; Onagro's rig on disk was patched the same way.
- **Ability VFX / new-animation ceiling.** Both are asset-bundle-only
  today (no `CharacterAPI` resolver exists yet, same root cause as the
  Animator's original "no way to load a new skeleton" gap that this whole
  project's Import/Save pipeline was built to solve). **Leaning as of
  2026-08-11: treat as its own separate investigation**, not an assumed
  extension of the rig fix — safer than assuming the pattern generalizes
  without checking.
- ~~**Document format stability across workstations.**~~ **Resolved
  2026-08-11**: stays several separate files (`character.json` + rig JSON
  + sidecars, referencing each other by character id), not one unified
  document — deliberately, for the user's sake: separate, focused files
  are easier to reason about one at a time than one large document, the
  same reasoning that already justified this project's own sidecar-file
  convention. (Scope note: this question no longer includes ability
  definitions at all — per §6, those live in `LokrAbilityLab`'s own
  separate plugin storage, referenced by id, not part of "the character
  document" the way this question originally assumed.)
- **Extension API stability.** The same concern as document format
  stability, one level up: once a third-party plugin registers a
  workstation or hooks into one via §3's extension points, changing those
  points' shape becomes a breaking change for everyone who's used them.
  **Answered 2026-08-11, with a nuance**: no formal stability freeze is
  needed *during active development* — extension-point shapes can keep
  changing while the project is still this early. But the *design itself*
  (how an extension point should look and feel) should get nailed down as
  early as practical anyway, even before a formal "this is now stable"
  guarantee is declared — treat "get the shape right soon" and "promise
  not to break it" as two different, separately-timed commitments rather
  than bundling them into one.
- **Live reload — reflecting Character Lab edits in an already-running
  game session.** Raised 2026-08-11. Lab `Sync()` writes disk immediately,
  but the game loads roster/unit/ability/rig/localization data once at
  boot — restart required today to see edits in gameplay. **Full plan:**
  [`live-reload.md`](../started/live-reload.md) (phases, cache
  inventory, investigation tasks, test matrix). **Stated goal:** edits
  visible without restart; realistic v1 is metagame UI after closing the
  Lab, not mid-combat. No implementation yet — Phase 0 investigation is
  next.
- ~~**Full-absorption migration bug — corrupted Onagro's real data with
  Ranger placeholder defaults.**~~ **Found and fixed 2026-08-11**, the day
  after full absorption (§9.3.A) shipped. `CharacterProfileSidecar.Load`
  only ever populated `CharacterProfile`'s new fields (`Levels`/`States`/
  `Model`/`Skills`/`SkillProgression`/`SoundClips`/etc.) from
  `character.json`'s own matching keys — but Onagro's `character.json`
  predated those keys entirely (it was written by the old, pre-2026-08-11
  sidecar format), so `Load` silently left every one of those fields at
  `CharacterProfile`'s own hardcoded field-initializer defaults, which
  happen to be the Ranger placeholder scaffold new characters start from.
  The very next `PersistAndSync` (triggered just by using the new
  Properties UI, e.g. touching any field) then wrote that Ranger-defaulted
  profile straight back over Onagro's real `rlheroes.txt` via
  `RLHeroesGenerator.Sync` — **and it did, in the live game folder**,
  discovered when the user reported Onagro now "looks like it's taking
  from the base game's Ranger hero." Root cause and fix: `Load` now falls
  back to `RLHeroesParser.ParseInto` against the character's own
  `definition/rlheroes.txt` whenever `character.json` has no `"levels"`
  key — the same one-time migration path a fresh `LegacyModImporter`
  import already goes through, just triggered lazily on first load instead
  of only at import time. **Recovery**: the user still had the original,
  never-imported `Onagro.zip` in Downloads; its `RLHeroes/Onagro.txt`
  was run through the real (already round-trip-verified)
  `RLHeroesParser`/`RLHeroesGenerator`/`CharacterProfileSidecar` code via
  a throwaway console harness, and the result — confirmed by eye to match
  what this project's own earlier round-trip verification pass had
  already captured from Onagro's pre-corruption file — was written back
  over the live `rlheroes.txt`/`character.json`. `roster.json` and
  `localization_en_US.txt` were untouched by the bug (their inputs come
  from fields `Load` populated correctly even without the migration
  fallback) and were left alone. **Lesson for next time a persisted-data
  schema gains new fields**: a "the field is just missing, keep the
  in-memory default" fallback is only safe when that default is
  genuinely neutral (empty, zero) — here it was a *specific, wrong,
  silently-plausible-looking* character (Ranger's own real data), which
  is exactly the shape of bug that doesn't announce itself until someone
  notices the wrong hero.
- ~~**Properties category registry leaked duplicates on every Lab
  re-entry, orphaning stale UI sections.**~~ **Found and fixed
  2026-08-11.** `PropertiesCategoryRegistry.RegisterCategory` just
  appended to its list, unlike `CharacterCreatorAPI.RegisterWorkstation`
  (which `RemoveAll`s by name first) — but
  `PropertiesWorkstationScene.RegisterBuiltInCategories()` is called from
  `CharacterLabScene.Open()` every single time the Lab is entered, the
  same "safe to call every Open()" contract `RegisterBuiltInWorkstations`
  itself already documents. Since the category list is plain static
  state that outlives the additive Lab scene's own unload/reload, every
  re-entry appended 8 more duplicate registrations. `PropertiesWorkstationScene.
  Build()` loops every registered entry once, so N duplicate
  registrations per category meant N built UI sections, with only the
  *last* one ever tracked for show/hide — the earlier N-1 stayed alive
  as orphaned, never-hidden `GameObject`s sitting behind whatever
  category was actually selected. Symptom reported by the user: a stray
  "new stat name" field visible while on the States category, and
  clicking Level Properties' level tabs appearing to do nothing (the
  visible tab row and the one `HomeWorkstationScene.SelectLevel`/
  `CharacterLevelsPanel.Refresh` were actually updating were two
  different orphaned instances). Fixed by giving `RegisterCategory` the
  same `RemoveAll`-by-name guard `RegisterWorkstation` already had — the
  very next `Open()` self-heals the list (no restart needed), since
  `RemoveAll` clears out every accumulated duplicate for a name before
  re-adding one. No data was at risk here (UI-only), unlike the
  migration bug above.
- ~~**Level Properties stat rows went stale across a level switch --
  `UiList<T>` key collision.**~~ **Found and fixed 2026-08-11**, the same
  day, after the registry-duplication fix above didn't actually resolve
  the user's report (switching Level Properties' level tabs still
  appeared to do nothing). Root cause was unrelated to the duplication
  bug: `UiList<T>.SetItems` only rebuilds a row when its key is genuinely
  new, reusing the existing row's `GameObject` -- and whatever text/
  event-handler closures were baked into it at build time -- whenever the
  key repeats. `CharacterLevelsPanel`'s stat rows were keyed by
  `stat.Name` alone; since stat names repeat across ranks (`health_max`,
  `armor_max`, etc. all appear on every one of Onagro's 3 levels),
  switching the active level never rebuilt anything -- the displayed
  values, and the `RenameStat`/`SetStatValue`/`RemoveStat` closures bound
  to them, stayed silently pointed at whichever level was on screen
  first. The level-tab row list had the same shape of bug one layer up:
  keyed by level number alone, so the active tab's highlighted styling
  never updated either, reinforcing the "nothing is happening" look.
  Fixed by folding the state that must force a rebuild into the key
  itself (`editingLevel + "|" + stat.Name` for stat rows,
  `level.Level + "|" + isActive` for tabs) -- the same technique
  `ReadinessChecklistPanel`/`InspectorPanel` already used correctly
  elsewhere in this plugin, just not applied here. `CharacterStatesPanel`
  had the identical latent shape (keyed by flag name alone, so a toggle's
  *displayed* value could go stale switching between two characters that
  happen to share a flag name) and got the same fix pre-emptively, even
  though it wasn't reported broken -- lower severity there since
  `SetState` reads the live current profile rather than a captured one,
  so no edit could land on the wrong character, only the checkbox's
  on-screen value could lag.
- ~~**Level Properties' (and States') own stat/state list collapsed to
  zero height -- a nested self-fit-vs-Grow() conflict, not a missing
  sizing hint.**~~ **Found and fixed 2026-08-11**, the same day, in two
  passes -- the first attempt (adding `section.Grow()` to every
  category's own top-level section) was based on a wrong premise
  (`UiStack.Vertical` defaults to `scrollable: false`/self-fitting, not
  `true` as first assumed) and turned out to be inert: nothing ever calls
  `inspectorContent.Add(section)` for a category's own section (it
  parents itself directly at `UiStack.Create()` time), so the "relax my
  own ContentSizeFitter because I'm hosting a Grow()n child" logic
  `UiStack.Add()` implements never had a trigger to fire from. The real
  chain: `statList`/`stateList` are `UiList&lt;T&gt;`, which *does*
  default `scrollable: true` -- and marking a scrollable (self-sizing-
  less) list `.Grow()` inside a category section that itself sits inside
  the shared inspector's own self-fitting scrollable content leaves no
  well-defined "leftover space" for that `Grow()` to claim, collapsing to
  zero -- functionally the same bug class as the original HomeNavPanel
  workstation-list bug (§11), just one nesting level deeper and easy to
  misdiagnose as a missing-Grow() problem instead of a
  Grow()-in-the-wrong-place one. Fixed by making `statList`/`stateList`
  non-scrollable (`scrollable: false`, matching HomeNavPanel's own fix)
  and dropping their `Grow()` calls entirely, letting the whole chain
  (list -&gt; section -&gt; shared inspector) self-fit correctly, with the
  inspector's own outer `ScrollRect` providing page-level scrolling if a
  category's real content overflows. The inert `section.Grow()` calls
  from the first pass were reverted from all eight category `Build()`
  methods rather than left in place, since they implied a design that
  wasn't actually true.
- ~~**Shell Inspector forms collapsed to a blank panel (nested
  ScrollRect).**~~ **Found and fixed 2026-08-13**, three times, same
  mechanism as the HomeNavPanel / Levels `UiList` collapse above. A
  `scrollable: true` `UiStack` is a stretch-to-fill `ScrollRect` with
  **no preferred height**. Putting one inside `InspectorDock`'s
  `ContentSizeFitter` host (or inside another scroll view) clips the
  built fields to a zero-height viewport — the inspector looks empty.
  Ability Library (`AbilityEditorPanel.BuildInto`, LokrLab 0.9.0),
  Animator (`InspectorPanel.BuildInto`, 0.9.2), and Properties
  (`PropertiesCategoryHost` + `propertiesHost` scroll, 0.9.9) each
  needed the same shape: **one** `ScrollRect` on the dock host
  (`Grow()`), inner forms `scrollable: false`. Rule is now in
  `SimpleUI/docs/conventions.md` ("One scroll viewport") and
  `LokrLab/docs/conventions.md`. An 2026-08-13 audit of every
  `scrollable: true` / default-`UiList` callsite left these as the
  safe patterns: InspectorDock's three hosts; category lists already
  `scrollable: false`; timeline chip/clip rows with `FixedHeight`;
  modal pickers that `Add(list.Grow())` inside a fixed-size
  `UiModal`; Node/File Tree `UiTree.Grow()` in a dock. The atlas
  picker's split row was the one leftover — it set
  `flexibleHeight` on a raw `GameObject` without going through
  `UiStack.Add()`, so the parent fitter never relaxed.
- **Character data was split across three top-level folders — consolidated
  2026-08-12, verified in-game (roster + rig render correctly).** A Lab character's Sounds
  (`Mods/LokrLab/Sounds/<id>/`) and Portraits
  (`Mods/LokrLab/Portraits/<id>/`) lived in separate flat,
  mod-wide folders from everything else about that character
  (`CharacterRigs/<id>/`'s own rig/sprites/definition/character.json/
  roster.json/localization). Renamed `CharacterRigs` → `Characters` (the
  old name undersold what it holds) and moved Sounds/Portraits inside it
  as `Characters/<id>/sounds/` and `Characters/<id>/portraits/` — one
  folder per character holds everything now. Two research passes (against
  real code, not assumption) found the rename touches `LokrCharacterLoader`
  too: `CustomRigLoader.cs` and `CharacterLabContentLoader.cs` each carry
  their own independent `private const string Category = "CharacterRigs"`
  — both had to move to `"Characters"` in the same change, or every
  existing Lab character (Onagro included) would have silently stopped
  loading. The flat `Mods/*/Portraits/<id>/` and `Mods/*/Sounds/<id>/`
  convention turned out to be **not** `LokrCharacterLab`'s own internal
  detail — it's `LokrCharacterLoader`'s documented default-resolver
  behavior (`PortraitPatches.cs`/`SoundPatches.cs`), explicitly built so
  hand-authored, non-Lab mods (the Official Pack included) keep working
  unmodified (`docs/modapi-plan.md` §5.1) — so it was **not** removed,
  only added to: both resolvers now check the new nested
  `Characters/<id>/sounds|portraits/` location first, falling back to the
  unchanged flat convention. `GeneralReadinessChecks`' roster-banner/
  map-token checks (which bypassed `CharacterLabPaths` entirely, calling
  `ModAPI.Files.TryFindFile("Portraits", ...)` directly) were switched to
  check the Lab's own nested location directly instead — arguably more
  correct than before, since the old cross-mod scan could technically
  match a coincidentally-same-named file from an unrelated mod.
  `HomeWorkstationScene.OnCreateCharacterConfirmed`/`CharacterImporter`
  now scaffold `sounds`/`portraits` subfolders upfront too, matching this
  project's own "never lazy" character-creation principle. Onagro's real
  data was migrated the same way (moved, not copied) as part of this
  change. **Verified in-game 2026-08-12**: Onagro renders and is
  selectable in the roster, confirming both `CustomRigLoader`'s and
  `CharacterLabContentLoader`'s renamed category strings resolved
  correctly — the actual regression risk this change carried. Banner/
  map-token art and combat sounds specifically weren't called out in that
  check; worth a closer look if anything looks off there later, but the
  roster+rig result already confirms the rename itself (the part that
  could have broken silently and completely) landed correctly.
- **Character Lab reworked from an additive overlay to a real scene
  transition — 2026-08-12, found and fixed a real cross-plugin bug during
  in-game verification.** `CharacterLabScene.Open()`/`Close()` now
  genuinely unload/reload the real Unity scene the player was in (via
  `FadeScreen` + `SceneManager.UnloadSceneAsync` + the base game's own
  `TransitionSceneComponent`) instead of hiding it underneath an overlay
  — see `LokrLab/docs/architecture.md` for the full design and
  why `TransitionSceneComponent` can only be reused for the *exit* leg
  (it resolves scenes by name against Unity's Build Settings list, which
  the lab's own `SceneManager.CreateScene`-built scene was never part
  of). Deleted the entire old "block every foreign EventSystem/camera/
  canvas/LeanTouch" isolation hack (`CharacterLabLeanTouchIsolation.cs`
  and friends) — nothing foreign is left to isolate once the origin scene
  is actually gone rather than just hidden.

  **Cross-plugin bug found via in-game testing, not research**:
  `LoadSceneMode.Single` — the mode every real scene transition in this
  game uses, and the one `TransitionSceneComponent` uses internally —
  unloads **every currently loaded Unity scene**, not just whichever one
  is "active." This wasn't obvious from source reading alone and directly
  contradicts the assumption baked into `ModMenuOverlay.cs`
  (`LokrModMenu`) and `AbilityLabScene.cs` (`LokrAbilityLab`): both build
  their own persistent scene exactly once (`SceneManager.CreateScene` +
  an `isBuilt` flag that's never reset) and assumed it would silently
  survive for the rest of the session. It doesn't — any `Single`-mode
  transition anywhere, including Character Lab's own new exit transition,
  destroys those scenes too, leaving `isBuilt` stuck `true` while pointing
  at dead GameObjects. Reproduced exactly as: open Character Lab, close
  it, then open the mod menu — `ModMenuOverlay.RefreshButtons` threw a
  `NullReferenceException` on a destroyed `Transform`. Fixed by replacing
  the `isBuilt`-only guard in both classes' own `EnsureBuilt()` with
  `isBuilt && scene.IsValid()` (`Scene.IsValid()` correctly reflects a
  scene that's been unloaded out from under a stale flag), plus a
  `RecoverIfSceneWasDestroyed()` check at the top of every public
  entry point (`Toggle`/`Open`/`Close`/`ForceClose`) so stale `isOpen`
  state gets dropped too, not just the build flag. `CharacterLabScene`
  itself got the same `labScene.IsValid()` backstop even though its own
  `CloseTo()` already resets `isBuilt` explicitly, as defense against any
  other path that might destroy its scene without going through that
  method first. **Lesson for any future `SceneManager.CreateScene`-based
  "persistent" scene in this codebase**: it is never actually safe to
  assume such a scene survives a `Single`-mode load happening *anywhere*
  in the game, even one this code didn't trigger itself — always guard
  `EnsureBuilt()`-style logic with the scene's own `IsValid()`, not a
  hand-maintained boolean alone.
- **`LokrAbilityLab` scene transition (2026-08-13, 0.4.1).** The fallback
  `AbilityLabScene` now uses the same `FadeScreen` + `UnloadSceneAsync` +
  `TransitionSceneComponent` pattern as LokrLab. The 2026-08-12
  `IsValid()` / `RecoverIfSceneWasDestroyed()` guard stays as a backstop
  when some other `Single`-mode load destroys the scene. Opening an
  Ability Library through the LokrLab shell already used that host's fade.
