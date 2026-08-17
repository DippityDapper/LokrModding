# Vanilla Encounter Edit

**Status:** Started — Phase 1 (import spike) code complete 2026-08-17, in-game confirm pending. Both open questions resolved; several of this doc's original technical assumptions corrected against decompiled source  
**Raised:** 2026-08-17  
**Last updated:** 2026-08-17  
**Owner:** LokrLab Encounter

Old-system authors often tweak **shipped combat rooms**. Encounter Lab
v1 authors **new** `encounter.json` on empty hosts. This track covers
**reconstructing** a vanilla `templates` prefab into a Lab project,
playing it in Sandbox, and — if we lock it — **substituting that JSON
when the campaign asks for the same room name**.

**Load paths are ours to change.** We cannot write a prefab back into
the `templates` bundle. We **can** hook
`LevelManager.CreateLevelFromFile`, `EncounterDefinition.InitializeEncounter`,
and `EncounterManager.LoadEncounterLogic` so a Lab override for
`combat_banditambush` wins. Encounter Creator Phase 1c forbade a
**global** campaign rewrite while the Lab was unfinished; a **guarded**
hook (only when a Lab project claims that template name) is in scope
here. [Custom Adventures](extensions.md) is still quest chains / maps,
not “one room override.”

Sibling tracks: [vanilla-character-edit.md](../completed/vanilla-character-edit.md),
[vanilla-ability-edit.md](../completed/vanilla-ability-edit.md). Builds on
[encounter-creator.md](../started/encounter-creator.md) Phases 1–17
(confirmed in-game 2026-08-17).

---

## Why research first

Vanilla rooms are **Unity prefabs** in the `templates` asset bundle.
They are not JSON. `AssetBundleManager.LoadAsset<T>` is read-only.
There is no API to write a modified prefab back into the bundle.

610 container names:
[`templates/catalog/template-names.txt`](../../character-reference/_extracted/base-game/templates/catalog/template-names.txt).
`spawnPos` is **world space**; Lab stores OffsetCoord `col` / `row`.
Props on the prefab are **PPtrs** (with a serialized `prefabName`
string alongside — see Phase 1 corrections below); Lab props are
**scenario names**.

So “edit `combat_wip` and save the prefab back” is impossible. The
feasible products are **import → remix → Sandbox** and **import →
override at load** (campaign still names `combat_banditambush`; the
hook feeds Lab JSON + host instead of the shipped `EncounterDefinition`).

**Corrected prefab structure** (the original chain above was wrong —
verified against decompiled source 2026-08-17, see Phase 1):
`EncounterTemplate` (root) holds a **list** of `EncounterDefinition`
variants (`encounterDefinitions`) *and, separately*, a list of
`EncounterBkgDefinition` variants (`encounterBkgDefinitions`) — the
board (`boardMetadata`) lives on the **bkg** definition, not the
encounter definition. Both lists are randomly indexed at runtime by
`InitializeEncounterTemplate`, which never runs during a cold,
read-only prefab inspect — so there is no `selectedEncounterDef` to
read on a freshly loaded asset; import must pick an index itself
(index 0) and log when a list has more than one entry.

---

## What works today

| Action | Result |
|---|---|
| New Encounter | Lab JSON on `fighttesterempty` or `combat_bridge`. |
| Template field | Host art / Tilemap. Combatants come from JSON, not `EncounterDefinition`. |
| Place vanilla units | `source="unit"` ids. Read-only refs. |
| Import Terrains | Scan another `templates` prefab for hex art (precedent for read-only prefab inspect). |
| Place scenario props | `scenario` deco catalog (~1030 names). |
| Campaign fight | Untouched. Encounter Harmony gates on `EmbeddedFightHost.IsActive`. |

No encounter loader yet. Adding one (Lab JSON keyed by template name,
applied only when an override project exists) is the intended way to
make campaign rooms use the remix. Play-without-Lab for *new* rooms
stays [encounter-creator.md](../started/encounter-creator.md) “Later.”

---

## Product decision (gate)

**Import / reconstruct.** Pick `combat_banditambush` → mint
`encounter.json` → edit in Setup → play in Sandbox. Always required.

**Runtime override (this track, after import works).** Lab folder is a
minted `slug_token`. The project claims the vanilla template name
(`combat_banditambush`) in `encounter.json` / `project.json`. That name
replaces the shipped room when the campaign (or any `CreateLevelFromFile`)
loads it. Guard: only fire when a Lab project claims that template; never
rewrite every fight. Host art can stay the vanilla prefab; combatants /
tiles / props come from JSON (same as embed). Same split as Character:
folder name is unique per author; template name last-wins.

**Host-only (already shipped).** Set `template` to a vanilla name to
borrow art. Not a full room import.

**Not this track:** new quests, maps, or `MapQuestStatus` graphs —
[Custom Adventures](extensions.md).

Fidelity ceiling (import) — corrected 2026-08-17 against decompiled
source (see Phase 1's "Corrections to this doc's original assumptions"):

| Vanilla | Lab | Reconstruct? |
|---|---|---|
| `spawnDataHeroes` (list membership, not a flag) | `source="spawn"`, GoodSide only | High (world → hex, exact) |
| `spawnDataEnemies` (`unitGroup` config key decides side) | `source="unit"` | High |
| `spawnDataCinematicUnits` | — | No — no Lab equivalent, drop |
| `boardMetadata` (on `EncounterBkgDefinition`, a separately-randomized sibling) | `overrides[]` | High — sparse impassable-list semantics match Lab's `walkableDefault:true` exactly |
| `TemplateObjectData` (`encounterObjs`) — has both a PPtr **and** a serialized `prefabName` string | `props[]` names | Medium — try `prefabName` first, PPtr is a fallback, not the only path |
| Tilemap | `tiles[]` / `terrains[]` | `terrains[]` free via existing `EncounterTerrainCatalog` scan; per-cell `tiles[]` likely skippable when `template` is kept (host already paints the floor) |
| `encounterLimits` | `camera` | Low as a direct read — it's all zeros on a cold prefab (assigned at runtime from a runtime-created `BoxCollider2D`); must be recomputed from `EncounterBkgDefinition`'s mesh/`TileTestController`, or skipped in favor of deriving `camera` from the imported board extent |
| `CheckCanSpawn` (quest / `variant-chance` / darkness / quest-context gates) | — | No direct Lab equivalent; raw prefab read is a strict *superset* of what any one playthrough sees — decide per-spawn whether to import gated-but-inactive entries |
| `EncounterLogic` Lua / waves | — | No |
| `EncounterDefinition` variant randomization | — | No — but empirically near-moot: 609/610 shipped rooms have exactly 1 `EncounterDefinition`. The real per-room variance is `CheckCanSpawn`, and separately, `EncounterBkgDefinition` variant count is *not yet confirmed* (see Phase 1) |

---

## Research phases

### Phase 1 — Import spike (one room)

**Status:** Code complete 2026-08-17. In-game confirm pending — see
verify list below. Both open questions resolved against decompiled
source (below).

**Implementation.** [`VanillaEncounterImporter`](../../../LokrLab/Encounter/VanillaEncounterImporter.cs)
reads `EncounterTemplate.encounterDefinitions[0]` /
`.encounterBkgDefinitions[0]` off a `templates`-bundle prefab (reusing
`EncounterTerrainCatalog.LoadPrefab`, promoted to `internal` — never
instantiates the prefab, never starts a fight embed), converts
`spawnPos`/`boardMetadata` room-local cube coords to live board
`OffsetCoord` via a freshly-constructed `Layout` (the same hex-size
constant `LevelManager.CreateLevelFromFile` hardcodes — no live
`Stage`/`HexBoard` needed, since none exists during a cold read-only
inspect) plus the confirmed `col+=4, row+=2` pad shortcut, and writes a
brand-new `EncounterFileModel` project: `spawnDataHeroes` →
`source="spawn"`/GoodSide, `spawnDataEnemies` → `source="unit"` with
side from the `unitGroup` config key, `boardMetadata`'s impassable
cells → `overrides[]`, terrains merged for free via the existing
`EncounterTerrainCatalog.EnsureHostTerrains`. Cinematic spawns dropped
(no Lab equivalent); `CheckCanSpawn`-gated entries (`notInQuest`,
`variant-chance`, `variant-quest-context`) are imported anyway and
counted, not filtered (filtering is impossible at import time — see
Phase 1's research notes below). Out-of-bounds conversions are dropped
and counted, never silently clamped.

Also fixed a real pre-existing bug found during research:
`EncounterPlacementRules.AuthoredSize` was hardcoded to 16×20 for every
template except one special case — wrong for the ~182 shipped rooms
(30%) actually sized 20×24. Added `RegisterAuthoredSize`, called by the
importer with the real `hexWidth`/`hexHeight` it just read, so
`AuthoredSize`'s existing callers (board sizing, placement clamping,
warnings) get the correct live size for any template that's been
imported at least once, without needing to change.

**Triggered via File → Import Vanilla Encounter...**
([`VanillaEncounterImportModal`](../../../LokrLab/Encounter/VanillaEncounterImportModal.cs))
— a name picker (reusing `EncounterTerrainCatalog.ListStages()`), no
separate detail/preview step before committing (unlike the Ability
track's browser) since importing always creates a brand-new project
rather than touching anything existing, so there's no override
blast-radius concern to show first. Registered with no `isVisible`
guard, reachable from the Project Browser. Full loss-list counts
(cinematic dropped, gated, out of bounds, variant-count warnings)
logged; a summary shown in the modal's status label.

**Not attempted in this pass:** `props[]`, `tiles[]`, and `camera` are
all explicitly deferred to Phase 2/3 per this doc's own phase
boundaries — the imported project keeps `template` set to the vanilla
name, so host art (including the floor) renders without needing a tile
import, and `camera` is skippable in v1 since vanilla's own
`encounterLimits` is unusable anyway (see the corrections below).

**In-game verify needed** (nothing here has been run against the live
game yet): pick `combat_banditambush` from the picker; confirm hero and
enemy spawns land on plausible hexes (the hex-math constants are
transcribed from decompiled source, not yet checked against a real
board); confirm impassable overrides match the vanilla room's actual
walkable/blocked layout; confirm the imported project opens in Setup
and plays correctly in Sandbox; check the log for variant-count
warnings on rooms with more than one `EncounterBkgDefinition`; try a
20×24 room (e.g. from the "What fraction are 20×24" open question) to
confirm the `AuthoredSize` fix actually produces a correctly-sized live
board instead of clipping at the old hardcoded 16×20.

In-game, read-only: load `combat_banditambush` the way
`EncounterTerrainCatalog.LoadPrefab` already does. Read the encounter
and board data (see corrections below — not simply
`EncounterDefinition.encounterData` off the root). Convert `spawnPos` →
live OffsetCoord. Map hero slots to `source="spawn"`. Copy
`boardMetadata` impassables (account for the `+8`/`+4` pad). Write a
draft `encounter.json` and play it in Sandbox. Document the loss list.

**Open question 1 resolved: `EncounterDefinition` is on neither the
root nor a "random variant child" in the sense feared.** The root
component is `EncounterTemplate`, holding `List<EncounterDefinition>
encounterDefinitions` and, separately, `List<EncounterBkgDefinition>
encounterBkgDefinitions` — the board lives on the **bkg** list, not the
encounter list. `InitializeEncounterTemplate` randomly indexes both
lists at runtime (seeded by the room's generator seed) and assigns the
result to `selectedEncounterDef`/`selectedEncounterBkg` — but that
method requires a live `LevelManager.CurrentRoom` and never runs during
a cold, read-only prefab inspect, so `GetComponent<EncounterDefinition>()`
on the prefab root returns **null** and `selectedEncounterDef` is null.
Read `encounterDefinitions[0]` (and `encounterBkgDefinitions[0]`)
directly, logging the count when it's more than 1.

Empirically this barely matters for `EncounterDefinition`: **609 of 610
shipped rooms have exactly one.** (Source: a prior AssetStudio catalog
pass, `docs/character-reference/_extracted/base-game/templates/catalog/board-sizes.csv`
— not independently re-verified this pass.) The `EncounterBkgDefinition`
count (where the board actually lives) is **not confirmed** by that
catalog and is the real remaining risk — flag it in Phase 1's own
loss-list report per room rather than assuming 1:1.

**Open question 2 resolved: yes, `CheckCanSpawn` filters spawn
composition, on four independent axes** — quest-vs-editor
(`notInQuest` config key), a probabilistic `variant-chance` (re-rolled
every load, not seeded), a map-darkness band, and a
`variant-quest-context` key/value match against live quest state. All
four read live `MetagameManager` state that isn't meaningful during a
cold inspect, so a raw prefab read is a strict **superset** of what any
one playthrough ever sees — including editor-only test units
(`notInQuest=true`) and mutually-exclusive quest-context variants
stacked on the same hexes with no marker distinguishing them in the
geometry. Decision needed before Phase 1 ships a draft: import
everything and flag gated entries in the loss-list report (recommended
— filtering at import time is impossible anyway, since `variant-chance`
is nondeterministic and quest-context requires live state), vs. some
other filter.

**Other corrections to this doc's original assumptions** (found while
resolving the two questions above):

- `EncounterData` has **three** spawn lists, not one `spawns` field:
  `spawnDataHeroes` / `spawnDataEnemies` / `spawnDataCinematicUnits`.
  Cinematic entries have no Lab equivalent — drop them, don't import as
  BadSide combatants.
- `isHeroSpawn` is not a runtime field — hero slots are just
  `spawnDataHeroes` list membership. `isLegendSpawn` is read from the
  per-spawn `LevelPieceConfig` bag (`data.config.GetConfig("isLegendSpawn", ...)`),
  not a `SpawnUnitData` field.
- `encounterLimits` (the doc's source for `camera`) is **all zeros on
  a cold prefab** — it's assigned at runtime from a `BoxCollider2D`
  that `EncounterBkgDefinition.Initialize` creates on the fly. Reading
  it directly gets nothing; either recompute what `Initialize` would
  produce (from the bkg's `TileTestController` or mesh bounds) or skip
  `camera` in the v1 draft and derive it later from the imported board
  extent.
- The `+8`/`+4` live-board pad ( `LevelManager.CreateLevelFromFile`)
  shifts each room's origin by `OffsetCoord(4, 2)`. Because the row
  shift (2) is even, odd-r parity is preserved and the conversion
  collapses to a plain `col += 4; row += 2` on top of the room-local
  cube coords — no full cube-math re-derivation needed. This exact
  shortcut does not generalize to an odd row shift.
- `LokrLab/Encounter/EncounterPlacementRules.AuthoredSize` — an
  **existing** helper this track was going to reuse — hardcodes
  16×20 for every template except one special case. That's correct for
  `combat_banditambush` and roughly 413/610 rooms, but wrong for the
  182 rooms (~30%) actually sized 20×24, plus a handful of other sizes.
  Phase 1 needs to read `hexWidth`/`hexHeight` off the imported
  `boardMetadata` directly rather than relying on this stub as-is —
  worth fixing the stub itself, not just working around it, since other
  callers already depend on it.
- `TemplateObjectData` (`encounterObjs`) carries a serialized
  `prefabName` string field **in addition to** the PPtr — the doc's
  "props are PPtrs, name resolve is research" was pessimistic. Try the
  string first in Phase 2.
- Coordinate conversion: reuse `LokrLab/Encounter/EncounterEdit.cs`'s
  existing `WorldToOffset` (world → OffsetCoord, currently private —
  promote to `internal`) rather than reimplementing `Layout`/`OffsetCoord`
  math from scratch. It does **not** clamp out-of-bounds results the
  way the vanilla `PointToHexItem` convenience method does — pair it
  with an explicit bounds check, which is what Phase 1 wants anyway
  (a spawn that lands off-board should be a loss-list entry, not a
  silently clamped edge cell).
- `EncounterTerrainCatalog`'s existing `EnsureTemplatesBundle` (the
  `templates`-bundle loader Phase 1 is told to copy) deliberately does
  **not** call the obvious `AssetBundleManager.LoadAsset<T>` API —
  that call hits disk and overwrites the bundle cache with `null` on a
  second load even when Unity refuses the duplicate, breaking
  `LevelRoom`'s own later load. Reuse `EnsureTemplatesBundle` itself
  (promote to `internal`), don't re-implement bundle loading.

### Phase 2 — Props and tiles

Resolve `prefabReference` → name vs `scenario/catalog/deco-names.txt`.
Enumerate Tilemap → `tiles[]` (Phase 12 two-cell mapping). Decide
whether full tile import is worth it when the host template already
paints the floor.

### Phase 3 — Host strategy

Imported rooms: keep `template: "combat_banditambush"` (full art, risk
of hidden vanilla spawns) vs normalize to `fighttesterempty` + imported
placements. Verify `EmbeddedFightStartFightSpawnPatch` prevents
double-spawn on a **populated** host (today’s hosts are empty-enough
on purpose).

### Phase 4 — Scope lock

Import + Sandbox is the first ship. Runtime override is the second
ship on **this** track (guarded load hook). Custom Adventures stays
quest / map authoring. Do not grow `encounter.json` into a room array
here.

### Phase 5 — Guarded campaign load hook

Design then implement. When a Lab override claims `combat_banditambush`:

- Intercept `CreateLevelFromFile` / `InitializeEncounter` /
  `LoadEncounterLogic` only for that name.
- Spawn combatants from `EncounterRoster` the way embed already does.
- Keep vanilla host art unless the project switched template.
- Uninstall the Lab folder → shipped prefab returns. **Wire this
  through `ProjectTypeRegistration.OnDeleted`** (`LokrLabApi/ProjectTypeRegistration.cs`),
  the per-project-type post-delete hook added for the Character track —
  do not rely on the Project Browser's `Refresh()` alone, it only
  rebuilds the browser's own row list and does not touch runtime
  content state. See [vanilla-character-edit.md](../completed/vanilla-character-edit.md)
  Phase 5 (`CharacterProjectType.OnCharacterDeleted`) for the reference
  implementation, and [character-close-lab-crash-after-deleting-open-project.md](../../issues/resolved/character-close-lab-crash-after-deleting-open-project.md)
  for a follow-on bug to watch for: also clear any stale "currently
  open" session state pointing at the deleted folder, or Lab close can
  crash trying to persist into it.
- Confirm: tutorial / scripted rooms, `CheckCanSpawn` variants,
  mid-run live reload, save continue into an overridden room.

Questions: one Lab project per room vs later room array? New template
names that have no bundle asset (that is closer to Adventures)?

---

## Open questions

1. Acceptable import fidelity for “looks like bandit ambush”?
2. What fraction of vanilla props are not in the scenario deco catalog?
3. Double-spawn risk on populated hosts?
4. Offline AssetStudio batch dump vs on-demand runtime import?
5. Keep calling this “edit vanilla” in the UI, or “Import room / remix”?

---

## Related docs

- [encounter-creator.md](../started/encounter-creator.md) Phase 1a–1c, 17, Later
- [extensions.md](extensions.md) — Custom Adventures
- [EncounterTemplateRules](../../../LokrLab/Encounter/EncounterTemplateRules.cs)
- [EncounterRoster](../../../LokrLab/Encounter/EncounterRoster.cs)
- [EncounterTerrainCatalog](../../../LokrLab/Encounter/EncounterTerrainCatalog.cs)
- [EmbeddedFightHost](../../../LokrLab/Character/EmbeddedFightHost.cs)
- Base-game: [EncounterDefinition](../../api/base-game/EncounterDefinition.html),
  [SpawnUnitData](../../api/base-game/SpawnUnitData.html),
  [MapQuestStatus](../../api/base-game/Ironhide/Legends/Model/Metagame/Map/Quests/MapQuestStatus.html),
  [AssetBundleManager](../../api/base-game/Ironhide/AssetBundles/AssetBundleManager.html)
