# Vanilla Encounter Edit

**Status:** Not started — research first  
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
[vanilla-ability-edit.md](../started/vanilla-ability-edit.md). Builds on
[encounter-creator.md](../started/encounter-creator.md) Phases 1–17
(confirmed in-game 2026-08-17).

---

## Why research first

Vanilla rooms are **Unity prefabs** in the `templates` asset bundle
(`EncounterTemplate` → `EncounterDefinition` → `EncounterData` /
`SpawnUnitData`). They are not JSON.
`AssetBundleManager.LoadAsset<T>` is read-only. There is no API to write
a modified prefab back into the bundle.

610 container names:
[`templates/catalog/template-names.txt`](../../character-reference/_extracted/base-game/templates/catalog/template-names.txt).
`spawnPos` is **world space**; Lab stores OffsetCoord `col` / `row`.
Props on the prefab are **PPtrs**; Lab props are **scenario names**.

So “edit `combat_wip` and save the prefab back” is impossible. The
feasible products are **import → remix → Sandbox** and **import →
override at load** (campaign still names `combat_banditambush`; the
hook feeds Lab JSON + host instead of the shipped `EncounterDefinition`).

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

Fidelity ceiling (import):

| Vanilla | Lab | Reconstruct? |
|---|---|---|
| Enemy / cinematic `SpawnUnitData` | `combatants[]` unit | Partial (world → hex) |
| `isHeroSpawn` / `isLegendSpawn` | `source="spawn"` | Partial |
| `boardMetadata` | `overrides[]` | High |
| Tilemap | `tiles[]` / `terrains[]` | Partial (import path exists) |
| `encounterObjs` PPtrs | `props[]` names | Low — name resolve is research |
| `encounterLimits` | `camera` | Partial |
| `EncounterLogic` Lua / waves | — | No |
| Variant randomization | — | No |

---

## Research phases

### Phase 1 — Import spike (one room)

In-game, read-only: load `combat_banditambush` the way
`EncounterTerrainCatalog.LoadPrefab` already does. Read
`EncounterDefinition.encounterData`. Convert `spawnPos` → OffsetCoord
(`PointToHexItem` / `HexToCenter` round-trip). Map hero slots to
`source="spawn"`. Copy `boardMetadata` impassables (account for
LevelManager +8 / +4 pad). Write a draft `encounter.json` and play it
in Sandbox. Document the loss list (Lua, cinematics, variants).

Open: is `EncounterDefinition` on the root or a random variant child
after `InitializeEncounterTemplate`? Do `CheckCanSpawn` keys change
composition by quest?

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
