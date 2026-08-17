# Character Lab / Loader — pre-UI-redesign audit

**Status:** Phase 0 of [editor-redesign.md](editor-redesign.md) — that
redesign's Phase 9 is complete. Remaining P0/P1 items are filed under
[`../../issues/`](../../issues/). Do not treat this inventory as a gate
on work that already shipped.

**Purpose:** Inventory bugs, error-handling gaps, SRP violations, and ModAPI
extraction candidates in `LokrCharacterLab` and `LokrCharacterLoader` *before*
the planned UI redesign, so fixes can be scheduled into that work rather than
discovered mid-refactor.

**Scope:** All source `.cs` in both plugins (84 Lab + 23 Loader files), cross-read
against `LokrModAPI`.

**Last updated:** 2026-08-15

---

## How to use this doc

| Priority | Meaning |
|----------|---------|
| **P0** | Fix before or at the start of UI redesign — data loss, crashes, or silent wrong behavior today |
| **P1** | Fix during redesign — high coupling to UI/session structure; refactor alongside new panels |
| **P2** | Structural / ModAPI — longer-term maintainability; can trail redesign if P0/P1 are done |

Each item has a stable **ID** (`L-…` Loader, `C-…` Character Lab, `M-…` ModAPI,
`X-…` cross-cutting). Remaining confirmed P0/P1 bugs are filed under
[`../../issues/unresolved/`](../../issues/unresolved/).

---

## Executive summary

**Loader** is well-factored around `CharacterAPI` resolver chains, but several
Harmony patches are fragile against UI prefab changes, duplicate-key override
semantics are inconsistent, and reload invalidation is incomplete.

**Character Lab** has three structural problems that will fight any UI redesign:

1. **`RigEditorScene`** (~3,370 lines) — scene build, input, animation authoring,
   save/load, preview, and status in one static god class.
2. **Hand-built JSON/KV writers** with inconsistent escaping — can corrupt
   character data on save (documented 2026-08-11 incident).
3. **Static session state** — panels and workstations share globals that are not
   fully reset on lab close; re-open can touch destroyed GameObjects.

**ModAPI** already covers mod-folder scan, textures, audio, and config, but both
plugins still duplicate locale maps, localization KV parsing, texture save/load
shortcuts, and the on-disk character-folder contract is split across assemblies
(Lab writes, Loader reads).

---

## P0 — Fix first

### Loader

| ID | Issue | Location | Notes |
|----|-------|----------|-------|
| L-01 | **Cross-file unit overrides silently dropped** | `UnityDefinitionsParserPatches.LoadData_Prefix` | **Code fixed in 1.1.16** (last-wins across files + UniqueId index). In-game confirm still on [vanilla-character-edit.md](vanilla-character-edit.md) Phase 5. |
| L-02 | **Null deref on rig resolution** | `HeroExoSkeletonPatches.Prefix`, `UnitViewExoSkeletonPatches.Postfix` | Filed: [`exo-skeleton-null-unitdefinition.md`](../../issues/unresolved-tested/exo-skeleton-null-unitdefinition.md). |
| L-03 | **Buff store hero index OOB** | `PortraitPatches` ~line 162 | Filed: [`portrait-patches-buff-store-index.md`](../../issues/unresolved-tested/portrait-patches-buff-store-index.md). |
| L-04 | **Hard-coded UI hierarchy** | `PortraitPatches` ~119–120 | Filed: [`portrait-patches-hardcoded-hierarchy.md`](../../issues/unresolved/portrait-patches-hardcoded-hierarchy.md). |
| L-05 | **`FindPartIndex` without validation** | `ExoSkeletonDataPatches`, `PartyTokenComponentPatches` | Filed: [`find-part-index-unvalidated.md`](../../issues/unresolved-tested/find-part-index-unvalidated.md). |
| L-06 | **`ReplaceWithFlatImage` self-parent** | `PortraitPatches` line 187 | Filed: [`portrait-patches-self-parent.md`](../../issues/unresolved/portrait-patches-self-parent.md). |

### Character Lab

| ID | Issue | Location | Notes |
|----|-------|----------|-------|
| C-01 | **Done, 2026-08-12.** ~~Unescaped hand-built JSON/KV~~ | `RLHeroesGenerator`, `RigEditorScene` save paths, `CharacterImporter` | Fixed via a new shared `LokrModAPI.Serialization.TextEscaping` (`JsonEscape`/`KvEscape`, the latter verified against `PenguinParser`'s real decompiled source, not guessed) applied at every hand-built write site in all three files, plus `CharacterProfileSidecar`'s own pre-existing (but incomplete — no control-character handling) local escaper consolidated onto the same shared helper. `LokrAbilityLab`'s `AbilityKvIO` was checked too and found already safe via a different, equally valid strategy (rejects a save with an actionable error on a literal quote, rather than escaping) — no change needed there. |
| C-02 | **Done, 2026-08-12.** ~~Non-atomic save~~ | `RigEditorScene.OnSaveClicked` | Fixed via a new `RigEditorScene.WriteAllTextAtomic` helper (temp file + rename) applied to `rig.json` and both sidecars — eliminates the interrupted-write-truncates-the-file failure mode. Does not make the three-file write a single cross-file transaction (see the method's own doc comment for why that's out of scope here). |
| C-03 | **Done, 2026-08-12.** ~~`RigEditorScene` not reset on lab close~~ | `CharacterLabScene.CloseTo` | Fixed via a new `RigEditorScene.ResetSession()`, called from `Build()`'s own start, `CharacterLabScene.CloseTo`, and `CharacterLabScene.ForceClose` — destroys any still-alive `DraggablePart`/`ReferenceCharacter` GameObjects and clears every session field. `OnLoadClicked` keeps its own narrower clear block (same fields except reference overlays, which a same-session folder switch should not discard) rather than calling `ResetSession()` itself. |
| C-04 | **Done, 2026-08-12.** ~~Animator opens with no character~~ | `CharacterLabScene` workstation `onShow` | Fixed: `SwitchToWorkstation` now enforces `WorkstationEntry.RequiresCharacterLoaded` server-side (redirects to Home with a logged warning) instead of relying solely on `HomeNavPanel` hiding the nav button — closes the gap for any caller reaching a gated workstation some other way (`CharacterCreatorAPI` is a public extension point). |

---

## P1 — Fix during UI redesign

### Loader — runtime & reload

| ID | Issue | Location |
|----|-------|----------|
| L-07 | Resolver/event subscribers not isolated — one throwing handler breaks chain | `CharacterAPI.ResolverChain`, all `Raise*` events |
| L-08 | `CustomRigLoader.Build` uncaught I/O/parse errors | `CustomRigLoader.cs` |
| L-09 | Sound clip cache never invalidated on reload | `SoundPatches`, `ContentReloader` |
| L-10 | `RegisterAbility` list grows; ability reload semantics unclear | `CharacterAPI`, `AbilitiesDefinitionsPatches` |
| L-11 | Invisibility exit fires for all non-invisible units every turn | Filed with L-12: [`invisibility-exit-fires-every-turn.md`](../../issues/unresolved-tested/invisibility-exit-fires-every-turn.md) |
| L-12 | Invisibility uses global `FindObjectsOfType<ExoSkeletonRenderer>()` | Same issue as L-11 |
| L-13 | String-splice JSON/KV injection (not JSON-aware) | `HeroRosterManagerPatches.ApplyRosterContributions`, `UnityDefinitionsParserPatches.InsertFragments` |
| L-14 | Duplicated locale suffix map (3 copies) | `LocalizationManagerPatches`, `CharacterLabContentLoader`, `LocaleCodes` |
| L-15 | `CharacterLabContentLoader` rescans disk every event; localization contributors run per language path | `CharacterLabContentLoader`, `LocalizationManagerPatches` |
| L-16 | Ability icon lookup O(mods × abilities) nested scan | `PortraitPatches` |
| L-17 | No unregister/clear APIs — hot reload double-registers | All `Register*` / `+=` in Loader |

### Character Lab — session & I/O

| ID | Issue | Location |
|----|-------|----------|
| C-05 | `LegacyModImporter` mutates session mid-import; no rollback | Creates character via `OnCreateCharacterConfirmed()` before validation completes |
| C-06 | `RLHeroesParser.ParseInto` throws on bad KV | Used from import, sidecar load, enemy loops |
| C-07 | `CharacterIdentityRekey` can fail mid-migration | File rewrites before `Directory.Move` |
| C-08 | `PersistAndSync` / disk writes unguarded in Properties/Home | `HomeWorkstationScene`, panels |
| C-09 | Legacy import swallows KV errors silently | `LegacyModImporter` empty catch blocks |
| C-10 | Only first `RLHeroes/*.txt` used when multiple exist | `LegacyModImporter` |
| C-11 | `SandboxFightHooks` enables debug panel globally | May persist after sandbox fight |

### UI redesign — structural (Lab)

| ID | Issue | Impact on redesign |
|----|-------|-------------------|
| C-UI-01 | **Three UI paradigms** — SimpleUI, `EditorUiHelpers`, `CharacterLabScene.CreateLabel` | Done, 2026-08-13. Investigation found the "three paradigms" premise itself was stale: `EditorUiHelpers.cs` (209 lines) had **zero real callers anywhere** — every panel `conventions.md`/`supporting-classes.md` described as using it (`InspectorPanel`, `MenuBarPanel`, `FileBrowserPanel`, `MetaExoPickerPanel`, `ReplacePartPickerPanel`, `EditHistoryPanel`, `AnimationsPanel`, `AnimationTimelinePanel`) had already migrated to real `SimpleUI` widgets. `CharacterLabScene.CreateInputField` was likewise dead. Only 5 real call sites remained for `CreateLabel`/`CreateButton`, all canvas-level chrome needing an absolute anchor point (`CharacterLabScene`'s own title/close, `HomeWorkstationScene`'s status label, `RigEditorScene`'s viewport labels, `PropertiesWorkstationScene`'s Home button). Migrated all 5 to `UiLabel.Create`/`UiButton.Create` + direct `RectTransform.anchorMin/anchorMax/sizeDelta` (same math as before, matched theme colors/fonts exactly — `UiTheme.Default.ButtonColor`/`.LabelColor`/`.Font` were already byte-identical to the old hardcoded values). Deleted `EditorUiHelpers.cs` and `CharacterLabScene`'s `CreateLabel`/`CreateButton`/`CreateInputField`. Verified via `dotnet build LokrModding.sln` (0 errors). Docs updated: `architecture.md`, `conventions.md`, `supporting-classes.md`, `animation-data-model.md` (LokrCharacterLab), `SimpleUI/docs/cross-references.md`, `docs/api/classes.json` (removed stale `EditorUiHelpers` manifest entry + orphaned HTML page). |
| C-UI-02 | **Static panel state not reset on close** | `InspectorPanel` now resets. Remainder (`IslandAtlasPickerPanel`, `MenuBarPanel`, `EditHistoryPanel`) resolved: [`lab-static-panels-not-reset-on-close.md`](../../issues/resolved/lab-static-panels-not-reset-on-close.md) |
| C-UI-03 | **`InspectorPanel.Refresh` on every playback tick** | In-place update semantics required; naive rebuild breaks buttons |
| C-UI-04 | **`PersistAndSync` on every field change** | Properties may need dirty-flag batching |
| C-UI-05 | **Inconsistent navigation chrome** | Animator MenuBar vs Properties Home button vs Load/Home nav panels |
| C-UI-06 | **Layout duplication** | `RigEditorScene.Build` hardcodes dock rect; `EditorLayout` has overlapping constants |
| C-UI-07 | **Workstation lazy-build once per session** | Redesign must respect `OnShow`/`OnHide`/`ResetSession` contract |

---

## P2 — Architecture & ModAPI

### SRP / god classes (Character Lab)

| Class | Lines | Mixed concerns |
|-------|-------|----------------|
| `RigEditorScene` | ~3,370 | Scene/camera, UI wiring, selection, animation, easing, save/load, preview, mass edit, attach points, status |
| `InspectorPanel` | ~907 | Build + 4 inspector modes + in-place refresh during playback |
| `HomeWorkstationScene` | ~893 | Session state, ~40 `SetX` mutators, persistence, recents, readiness, reload |
| `IslandAtlasPickerPanel` | ~637 | Modal UI + island detection + PNG export |
| `LegacyModImporter` | ~315 | Regex KV + scaffold + abilities + enemies + sounds |

**Target split (redesign):**

- **Done, 2026-08-12:** `CharacterSession` — folder, profile, editing-level (dirty flags not yet
  tracked — none existed to extract). `HomeWorkstationScene`'s own `CurrentCharacterFolder`/
  `CurrentProfile`/`CurrentEditingLevel` are now thin forwarding properties onto it, so every
  existing call site kept compiling unchanged; new consumers (`CharacterLabScene`'s C-04 guard
  and its Animator `onShow` hookup) read `CharacterSession` directly.
- **Done, 2026-08-12:** `CharacterProfileService` — persistence + sync (replaces `PersistAndSync`
  sprawl). All ~40 `SetX`/`AddX`/`RemoveX` field mutators plus `PersistAndSync` itself and its
  private helpers (`FindLevel`/`FindStat`/`RenumberLevels`/skill-progression validation) moved out
  of `HomeWorkstationScene`, which keeps a same-named thin forward for every one still called from
  a Properties panel (~15 panel files, ~60 call sites, none of which needed to change). Every
  method reads/writes `CharacterSession.Profile`/`.Folder` directly rather than through
  `HomeWorkstationScene`'s own private-setter properties.
- **Done, 2026-08-12:** `RigPreviewService` / `RigSaveService` / `RigLoadService` — extracted
  from `RigEditorScene`, in that order (most self-contained first, riskiest last, per
  [`editor-redesign.md`](editor-redesign.md)'s own risk note that this piece can only be verified
  by compiling, not by playtesting in-game). `RigEditorScene.cs` went from ~3,370 to ~2,820 lines.
  Real scope note, discovered mid-extraction rather than assumed up front: the three services own
  exactly what was already cleanly separable from RigEditorScene's own live editing state (Preview's
  own rig instance; the atomic-write/sidecar-serialization mechanics; rig.json+sidecar parsing) —
  `OnLoadClicked`/`OnSaveClicked` themselves **stayed** in `RigEditorScene`, since both spawn/mutate
  live session state (GameObjects under `partsRoot`, active-clip/frame/selection state) genuinely
  coupled to the rest of the file, not to file I/O. Splitting those two further would mean inventing
  new method boundaries inside intricate, hard-to-verify logic (matrix math, duplicate-part
  handling, baking) — judged not safe to do blind, so left for a session with in-game verification.
  A handful of previously-`private` utilities (`GetOrCreateRestPose`/`BaseName`/`F`/`DuplicateName`/
  `DuplicateMarker`/`PixelsToUnits`) became `internal` so the extracted services can call back into
  them — RigEditorScene still owns them, since they're used by its own remaining live-editing code
  too, not just the parts that moved.
- Panels — view-only; bind to session/services — **not started**

### SRP / coupling (Loader)

| Area | Issue |
|------|-------|
| `CharacterAPI.cs` | Facade + resolver infra + reload entry + registries in one static class |
| `PortraitPatches.cs` | Resolution + UI surgery + default file scanning |
| Full-method Harmony copies | `UnityDefinitionsParserPatches` (~400+ lines), `DialogViewManagerMapPatches`, `SoundPatches`, `HeroRosterManagerPatches`, `LocalizationManagerPatches`, `PartyTokenComponentPatches`, `IronhideScriptLoaderPatches` — fragile on game/UI updates |
| Game namespaces | `UnityDefinitionsParserPatches`, `AbilitiesDefinitionsPatches` in `Ironhide.*` namespaces for type access |

**2026-08-12 update:** the abilities half of the Loader↔Lab boundary
below is now addressed — `AbilitiesContribution` (the
`CharacterAPI.BuildingAbilities`/`ContributingLocalization` subscriber for
`Abilities/<id>/` folders) moved from `LokrAbilityLab` into
`LokrCharacterLoader` as `CustomRigs/AbilityLabContentLoader.cs`, mirroring
`CharacterLabContentLoader`'s existing shape, plus a new shared
`CustomRigs/LocaleFileSuffixes.cs` deduplicating what were two identical
private locale-suffix tables about to sit in the same assembly. See
[`editor-redesign.md`](editor-redesign.md) §2.7
for the full reasoning (player-facing: ability content now loads without
`LokrAbilityLab` installed, matching what was already true for
characters). `CharacterLabPaths`/profile/RLHeroes-round-trip/`CharacterIdentityRekey`
below remain Lab-owned, not yet moved.

### ModAPI extraction candidates

| ID | Candidate | From | Rationale |
|----|-----------|------|-----------|
| M-01 | `ResolverChain<T>` | `CharacterAPI` | Generic priority chain for any plugin |
| M-02 | `LocalizationLocaleMapping` | 3 duplicated tables (**2 of 3 consolidated 2026-08-12** — `LokrCharacterLoader/CustomRigs/LocaleFileSuffixes.cs` now shared by `CharacterLabContentLoader`/`AbilityLabContentLoader`; `LokrCharacterLab`'s own `LocaleCodes.cs` — a differently-shaped write-side list, not the same dictionary shape — still separate) | Single source for `LanguageCode` → file suffix |
| M-03 | `ParseLocalizationKv` / format helpers | Loader + Lab + `LegacyModImporter` | Stop regex triplication |
| M-04 | `ModsRoot` / `GetModFolder(name)` | `ModFileSystem` (private today) | Lab/AbilityLab hardcode `"LokrCharacterLab"` paths |
| M-05 | `SaveTexture` / readable texture helpers | Lab atlas importers, `CharacterImporter` | Bypass raw `File.WriteAllBytes` today |
| M-06 | `ReplaceWithFlatImage` + ExoSkeleton teardown | `PortraitPatches` | Any flat-portrait override needs this |
| M-07 | `ExoSkeletonModData.ApplyTextureToRenderer` | Loader patches | Shared rig skin binding |
| M-08 | JSON sidecar helpers (read/write + log) | `RecentFilesStore`, `CharacterProfileSidecar`, `RigEditorScene` | Consistent error handling |
| M-09 | `KvEscape` / `JsonEscape` | **Missing** — needed everywhere | Blocks C-01 fix |
| M-10 | Sound resolver cache + `ClearCache()` | `SoundPatches` | Belongs with `ModAPI.Audio` |
| M-11 | Safe read helpers (`TryReadAllText`, etc.) | Scattered try/catch | Reduce duplicated warning strings |

### Shared domain (Loader ↔ Lab boundary)

These belong in **Loader** or a small shared module both reference — not ModAPI
(character domain), but blocks clean refactor until unified:

| Item | Writer today | Reader today |
|------|--------------|--------------|
| `CharacterProfile` + sidecar | Lab | — |
| `RLHeroesParser` / `RLHeroesGenerator` | Lab | Loader (`CharacterLabContentLoader`) |
| `CharacterLabPaths` + folder layout | Lab | Loader (`CustomRigLoader`, content loader) |
| `CharacterIdentityRekey` | Lab | — |
| `AffineMatrixMath` + rig matrix codec | Lab | Loader (`CustomRigLoader`) |
| `CombatSequenceNames` | Lab | Loader (validators) |
| Atlas importers | Lab | Loader (rig load path) |

**Recommendation:** Introduce `LokrCharacterLoader/CharacterData/` (or similar)
for profile, paths, RLHeroes round-trip, and locale constants; Lab references
Loader assembly (already does for `CharacterAPI`).

---

## UI redesign checklist (derived)

Use this as a gate before merging redesign PRs:

- [ ] **P0 Loader** L-01–L-06 addressed or explicitly deferred with issue links
- [x] **P0 Lab** C-01–C-04 addressed (2026-08-12)
- [x] Central **`TextEscaping`** utility used by all writers (M-09) — `LokrModAPI.Serialization.TextEscaping`
- [x] **`RigEditorScene.ResetSession()`** called from `CloseTo` and start of `Build`
- [x] **`CharacterSession`** (or equivalent) replaces static globals for profile/folder (2026-08-12)
- [x] **`CharacterProfileService`** replaces `PersistAndSync` sprawl (2026-08-12)
- [x] **`RigPreviewService`/`RigSaveService`/`RigLoadService`** extracted from `RigEditorScene` (2026-08-12) — see the P2 section above for the real, narrower-than-first-envisioned scope (`OnLoadClicked`/`OnSaveClicked` themselves stayed, deliberately)
- [ ] All workstations reset panel statics on close (C-UI-02)
- [x] Single UI toolkit chosen; legacy helpers deprecated (C-UI-01) — done, 2026-08-13; `EditorUiHelpers.cs` and `CharacterLabScene.CreateLabel`/`CreateButton`/`CreateInputField` deleted, all real call sites on `SimpleUI`
- [ ] Unified menu bar / back navigation (C-UI-05)
- [ ] Portrait / party-token / dialog patches audited against new prefab names (L-04, L-05)
- [x] Atomic save for rig (C-02, 2026-08-12) — `RigEditorScene.WriteAllTextAtomic`; profile sync (`RLHeroesGenerator`, `CharacterProfileSidecar`) is still plain `File.WriteAllText`, not in this pass's scope
- [ ] Locale map consolidated (L-14 / M-02) — partially done (Loader's own two copies merged, see M-02 above); `LokrCharacterLab`'s `LocaleCodes.cs` still separate

---

## File index (high-touch)

### LokrCharacterLoader

| File | Audit focus |
|------|-------------|
| `CharacterAPI.cs` | Events, resolvers, reload, god-class split |
| `ContentReloader.cs` | Partial reload, cache invalidation |
| `CustomRigs/CustomRigLoader.cs` | Error handling, cache |
| `CustomRigs/CharacterLabContentLoader.cs` | Rescan perf, locale dup |
| `Patches/PortraitPatches.cs` | **UI redesign critical** |
| `Patches/UnityDefinitionsParserPatches.cs` | L-01, full-method copy |
| `Patches/DialogViewManagerMapPatches.cs` | Full-method, UI |
| `Patches/PartyTokenComponentPatches.cs` | Map UI, part indices |
| `Patches/InvisibilityPatches.cs` | Global side effects |
| `Patches/SoundPatches.cs` | Cache, substring matching |

### LokrCharacterLab

| File | Audit focus |
|------|-------------|
| `Editor/RigEditorScene.cs` | **Primary god class** |
| `Editor/HomeWorkstationScene.cs` | Session, PersistAndSync |
| `Editor/InspectorPanel.cs` | Playback refresh, static state |
| `Editor/General/RLHeroesGenerator.cs` | Escaping, atomic write |
| `Editor/General/LegacyModImporter.cs` | Transactions, error surfacing |
| `Editor/General/CharacterIdentityRekey.cs` | Migration safety |
| `Editor/General/CharacterProfileSidecar.cs` | JSON (good escaping — extend pattern) |
| `CharacterLabScene.cs` | Lifecycle, workstation registry, reset |
| `Editor/EditorLayout.cs` | Layout single source of truth |

---

## Related docs

- [open-questions.md](../open-questions.md) — active deferred items
- [completed/live-reload.md](../started/live-reload.md) — reload track (overlaps L-09, L-15)
- [capabilities-and-gaps.md](../../capabilities-and-gaps.md) — shipped vs missing features
- [LokrLab/docs/architecture.md](../../../LokrLab/docs/architecture.md) — current Lab structure
- [LokrCharacterLoader/docs/character-api.md](../../../LokrCharacterLoader/docs/character-api.md) — extension API

---

## Revision log

| Date | Change |
|------|--------|
| 2026-08-12 | Initial audit from full plugin scan pre-UI redesign |
| 2026-08-12 | Ability-loading half of the Loader↔Lab boundary addressed (`AbilityLabContentLoader` moved into `LokrCharacterLoader`); M-02 partially consolidated |
| 2026-08-12 | **All four P0 Lab items (C-01–C-04) fixed** — see each row above for what changed; also closed the corresponding checklist items and partially closed C-UI-02 (`RigEditorScene`'s own static state now resets on close, other panels' don't yet) |
| 2026-08-12 | `CharacterSession` extracted from `HomeWorkstationScene` (P2 target split, first of its four items) — folder/profile/editing-level state now has its own home; `HomeWorkstationScene`'s own properties are thin forwards, so no other call site changed. `RigLoadService`/`RigSaveService`/`RigPreviewService`/`CharacterProfileService` deliberately not attempted this pass — scoped narrower on purpose since compile-only verification isn't enough confidence for `RigEditorScene`'s own extraction |
| 2026-08-12 | **P2 target split completed, all four items** — `CharacterProfileService` (HomeWorkstationScene's ~40 mutators + PersistAndSync), then `RigPreviewService`/`RigSaveService`/`RigLoadService` (RigEditorScene, most-self-contained-first order). `RigEditorScene.cs` ~3,370 → ~2,820 lines. `OnLoadClicked`/`OnSaveClicked` deliberately stayed put — see the P2 section's own updated note for why. Full solution build verified clean after every individual extraction, not just at the end |
| 2026-08-13 | **C-UI-01 (three UI paradigms) fixed** — turned out to be two dead-code deletions plus 5 real call-site migrations, not a real three-way split (see the row above for the full finding). `EditorUiHelpers.cs` and `CharacterLabScene.CreateLabel`/`CreateButton`/`CreateInputField` deleted; the 5 real call sites now use `UiLabel.Create`/`UiButton.Create` with `RectTransform` set directly for their absolute anchor point. This was the last item in Phase 0 of `editor-redesign.md` — Phase 0 is now complete |
