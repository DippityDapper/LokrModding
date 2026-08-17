# Encounter Creator

**Status:** Started — Phase 17 (hero spawn points + Sandbox; schema v13) confirmed in-game 2026-08-17 (LokrLab 0.12.104). Phase 16a (exploration pockets, per-unit aggro radius, fight-end fence; schema v10) and Phase 16b (painted trigger regions with a catalog + pocket targeting + overlapping per-trigger hex sets; schema v12) both confirmed in-game 2026-08-16 (LokrLab 0.12.90 / 0.12.96). Phase 15 (Play camera bounds, schema v8) and Phase 14 (scenario props) both confirmed in-game 2026-08-16 (LokrLab 0.12.77 / 0.12.71). Combatants/Props visual catalogues in 0.12.75 (enemy kind-prefab exo; spritesheet-bundle prop thumbs). Phase 13 terrain catalog confirmed in-game 2026-08-16. Phase 12 floor-tile paint confirmed in-game 2026-08-16. Phase 11 grow-board confirmed in-game 2026-08-16.  
**Raised:** 2026-08-16  
**Last updated:** 2026-08-17  
**Owner:** LokrLab (suite). LokrLabApi stays contracts-only.

A **separate project type** (`encounter`) in the LokrLab suite. The
intended product is a **full visual map editor** (draw the hex grid,
drag enemies and props, paint floor tiles) plus Play in the in-lab fight
hole. v1 is the project-type scaffold on a stock template, not that
editor yet. It is **not** a Sandbox feature, **not** Custom Adventures,
and **not** a Unity prefab editor. Character Sandbox stays the live
iterate tool and may **load** an Encounter project to play it. This is
[phasing.md](../phasing.md) item 7 and editor-redesign Phase 10.

See also [sandbox-workstation.md](../completed/sandbox-workstation.md),
[editor-redesign.md](editor-redesign.md) §2.5 / §6.3 / Phase 10,
[lab-suite-merge.md](../completed/lab-suite-merge.md),
[extensions.md](../not-started/extensions.md) (Custom scripting + Custom Adventures stay
there).

---

## Why a separate roadmap

[editor-redesign.md](editor-redesign.md) Phase 10 is one
paragraph: "Encounter as the second many-projects type." That is the
*shell* reason this exists (cross-project refs, not a singleton). It is
not enough to build from.

[extensions.md](../not-started/extensions.md) still lists Encounter next to a later Lua
plugin and Custom Adventures. Those are different tracks. This file owns
Encounter only.

Ability Lab overhaul taught the same split: a research/design gate, then
implementation phases, with Custom Adventures and campaign injection
explicitly out.

---

## Why this is not a Unity prefab editor

Vanilla combat rooms are **prefabs** in the `templates` asset bundle, not
KV/JSON the Lab can write:

| Base-game type | What it actually is |
|---|---|
| `EncounterTemplate` | Prefab wrapper: board, walls, background, a list of `EncounterDefinition` variants |
| `EncounterDefinition` | `MonoBehaviour` on that prefab: instantiates props, enemies, cinematic units, hero spawn slots |
| `EncounterData` | Runtime lists: `spawnDataHeroes` / `spawnDataEnemies` / `spawnDataCinematicUnits` / `encounterObjs` |
| `SpawnUnitData` | `unitId`, `spawnPos`, `flipped`, `hasInitiative`, `isLegendSpawn`, `cinematicId` |

Sandbox already bypasses all of that. It loads `fighttesterempty` (an
empty 16x20 walkable board) and calls `Stage.AddUnit` for a hero plus
one enemy. Encounter Creator v1 **authors Lab JSON and feeds that same
spawn path**. Do not try to emit new `EncounterDefinition` prefabs.

Win/lose in the engine is `Stage.CheckFightEnd`: living `GoodSide == 0`
is a loss; else living `BadSide == 0` (and `OwnSide == 0`) is a win.
Mutual wipe is a loss. Custom objectives (survive N turns, kill a named
unit) are not v1.

Vanilla "waves" are sequential **rooms** on `MapQuestStatus.encounters`
(a list of template names). That is Custom Adventures, not this editor.

---

## Already decided

Recorded here so Phase 2 does not re-litigate them.

| Decision | Source |
|---|---|
| Lives in the LokrLab **suite** (`LokrLab/Encounter/`), not a new plugin, not on `LokrLabApi` | [lab-suite-merge.md](../completed/lab-suite-merge.md) |
| Many-projects type (`encounter`), not Ability Library's singleton | [editor-redesign.md](editor-redesign.md) §2.5.1 |
| Per-type folder root under `Mods/LokrLab/` | editor-redesign §2.3; suite merge nested categories |
| Combatants are **read-only `ProjectReference`s** (plus vanilla unit ids); jump **switches** session, no split-view | editor-redesign §2.5 |
| Play uses the existing additive embed (`StartEmbeddedFight` / `fighttesterempty` hole). Lab stays open. | [sandbox-workstation.md](../completed/sandbox-workstation.md) |
| Encounter Lab is its **own project type**. Do not build it into Character Sandbox (no Sandbox workspace, no Sandbox node-tree branch). | this doc (2026-08-16) |
| Target UX is a **full visual editor** (draw grid, drag units/props, paint tiles). v1 does not ship that editor. Phase 1b/1c: **high with Harmony** for grow-and-paint + visual editor + exploration; not infinite streaming. | this doc (2026-08-16) |
| Encounter Harmony patches live in **LokrLab** (extend the existing embed host). Not `LokrPatch`, not `LokrCharacterLoader`. Guard so campaign fights stay vanilla. | Phase 1c (2026-08-16) |
| v1 workspaces are **Setup** (default, summary) and **Play** (embed). Setup becomes the map editor after Phase 8. | Phase 2 (2026-08-16) |
| Authored placement is **OffsetCoord** (`col` / `row`). Convert with `HexBoard.HexToCenter` at spawn. Do not store vanilla world `spawnPos`. | Phase 1a (2026-08-16) |
| v1 default template is `fighttesterempty`. Optional second empty board: `combat_wip`. `combat_blank` is not empty. | Phase 1a (2026-08-16) |
| Character Sandbox stays the live iterate tool (debug panel, one hero). It may **load** an Encounter project to play-test. Authoring stays on Encounter. | this doc |
| File → Save / Ctrl+S / dirty `*` / close prompt already exist; Encounter must set `ProjectSession.IsDirty` | [lab-save-ux.md](../completed/lab-save-ux.md) |
| Hover strip already exists; Encounter binds a new sidecar when the UI exists | [lab-hover-coverage.md](../completed/lab-hover-coverage.md) |
| No Ability card registry (or Encounter-specific widgets) on `LokrLabApi` | prior track constraint |
| Custom Adventures waits on this | [extensions.md](../not-started/extensions.md) |

Recommended on-disk root (confirm in Phase 2, match suite categories):

```
Mods/LokrLab/LokrEncounterLab/<encounterId>/
  project.json
  encounter.json
  aliases.json          (optional; same slug/$alias rules as Character)
```

`ScanCategory` = `LokrEncounterLab`. There is **no** runtime loader in
`LokrCharacterLoader` for v1 — play-only users cannot run an authored
encounter in campaign until Custom Adventures (or a later loader) exists.
That matches editor-redesign §2.7: FolderRoot stays with the authoring
plugin until a runtime consumer exists.

---

## Target editor (the product)

The Encounter Lab workspace should become a **WYSIWYG map editor**, not a
form that happens to spawn a fight. Animator is the precedent: a live
viewport you draw into, with the inspector as details.

| Tool | What the author does |
|---|---|
| Grid | Draw / erase hexes; set exact shape and size; grow the board |
| Units | Drag Character refs and vanilla enemies onto hexes (facing, side, rank on drop or in the inspector) |
| Props | Drag obstacles / scenery onto hexes from a palette |
| Floor | Paint floor tiles (grass, stone, …) onto hexes |
| Triggers | (Phase 1b) paint proximity / trigger regions for exploration-combat |

v1 is still Node Tree + inspector + Play on `fighttesterempty`. That is
the scaffold so Save / Play / combatants exist. The visual editor is the
goal Phase 1b is researching, then later phases after 8. Do not ship a
second "good enough" inspector-only Encounter Lab and call the track
done.

---

## What v1 is

A Lab project you can create, save, reopen, and Play:

- One arena template (default `fighttesterempty`; other empty-enough
  templates if Phase 1 finds them).
- A list of combatants: Character project refs and/or vanilla unit ids,
  each with side (`GoodSide` / `BadSide`), optional hero rank, hex
  placement, facing.
- At least one `GoodSide` unit (otherwise `CheckFightEnd` auto-defeats
  on the first tick — same load-bearing rule as Sandbox).
- Play / Stop in the **Play** workspace hole, same embed as Stage /
  Sandbox. **Setup** is the default tab (summary in v1).
- Node Tree + inspector + File Tree; create sheet (Name / Slug / Alias).
- Hover copy for Encounter controls.

## What v1 is not

- Custom Adventures (map, quest chain, multiple rooms, rewards).
- Drawn / expandable boards, drag-in units/props, painted floor tiles,
  exploration-before-combat — **the target editor**; research in
  Phase 1b; not v1 even if the answer is yes.
- Mid-fight waves or `encounterHelper` Lua.
- Custom win/lose beyond vanilla wipe.
- Authoring obstacles / walls / backgrounds on a stock template (those
  are baked into the prefab). A later drawn-board phase may own
  walkability paint if Phase 1b allows it.
- Building Encounter into Character Sandbox or Ability Lab Stage.
- Injecting encounters into campaign / arena / tavern.
- A runtime `CharacterAPI` encounter event (no play-without-Lab path).
- A new BepInEx plugin.
- Expanding `EmbeddedFightRequest` into a general roster API unless Phase
  2 proves Stage's 1v1 cannot stay a separate call. Prefer a
  suite-internal spawn list next to `SandboxRoster`.

---

```
Phase 1a vanilla rooms     --+
Phase 1b authored board      +-->  Phase 2 editor design (gate)
Phase 1c Harmony surfaces  --+         |
                                       v
                             Phase 3 project type + save
                                       |
                                       v
                             Phase 4 combatants
                                       |
                                       v
                             Phase 5 placement
                                       |
                                       v
                             Phase 6 Play in embed
                                       |
                                       v
                             Phase 7 arena template picker
                                       |
                                       v
                             Phase 8 hover copy
                                       |
                                       v
                             Phase 9 Setup board + click-to-place
                                       |
                                       v
                             Phase 10 walkability paint
                                       |
                                       v
                             Phase 11 grow-board
                                       |
                                       v
                             Phase 12 floor-tile paint
                                       |
                                       v
                             Phase 13 terrain catalog
                                       |
                                       v
                             Phase 14 scenario props
                                       |
                                       v
                             Phase 15 Play camera bounds
                                       |
                                       v
                             Phase 16a exploration pockets
                                       |
                                       v
                             Phase 16b painted triggers
                                       |
                                       v
                             Phase 17 hero spawn points + Sandbox load
```

**Gate:** Phases 7–17 are all confirmed in-game.

Phase 1b/1c do **not** gate v1. Phase 2 records the answer (**high with
Harmony** for the three product goals; not infinite streaming) so later
phases can be added without pretending v1 already has a drawn map.
Phase 6 does not wait on Phase 7: Play can hardcode `fighttesterempty`
the way Sandbox does today. Phase 8 can overlap Phase 6/7. Sandbox
"load this Encounter" is a later consumer, after Encounter Play works.

---

## Phase 1a — Research vanilla rooms

**Status:** done 2026-08-16 (decompile + `templates` bundle dump). No
Encounter code. Catalog:
[templates/catalog/](../../character-reference/_extracted/base-game/templates/catalog/).

Goal was a short catalog of what a Lab encounter can *reuse*, not a
second Base Game HTML pass.

### Findings

Vanilla rooms are **prefabs**, loaded by name:
`AssetBundleManager.LoadAsset<GameObject>("templates", encounterName)`.
The bundle has **610** containers (611 `EncounterTemplate` assets; one
leftover). Almost every room has a Unity `Tilemap` (609 Tilemaps).

**Board size** from 610 `EncounterBkgDefinition.boardMetadata`:

| Size | Cells | Count | Notes |
|---|---|---|---|
| 16 x 20 | 320 | 413 | Dominant skirmish / quest room |
| 20 x 24 | 480 | 182 | Arena layouts |
| 15 x 30 | 450 | 4 | `combat_wastelandbf*` |
| 23 x 28 | 644 | 2 | `combat_stormcloudmbf`, `_tony` — vanilla max |
| 20 x 22 | 440 | 2 | `combat_krumthrone`, `combat_krumcoliseum_thony` |
| 13 x 20 | 260 | 2 | dungeon-spider rooms |
| 12 x 12 | 144 | 2 | tutorial dungeon intros |
| 13 x 12 / 10 x 20 | 156 / 200 | 1 each | tutorial / spider |
| 0 x 0 | 0 | 1 | `encounter3` — mesh leftover, not a board |

There is **no** vanilla huge or infinite map. `LevelManager` then pads a
single room: `width += 8`, `height += 4`, then `Stage.CreateBoard` +
stamps `boardState` + `HexBoardViewComponent.SetBoard`. Live
`fighttesterempty` is therefore **24 x 24**. Largest live board is about
**31 x 32**. Hex size constant is `(0.55, -0.33275)`.

**Empty boards** (EncounterDefinition byte size 104, all spawn lists
size 0):

| Template | Board | `boardState` | Use |
|---|---|---|---|
| `fighttesterempty` | 16 x 20 | 101 impassable cells (border / shape) | **v1 default** |
| `combat_wip` | 16 x 20 | 136 impassable | optional second empty |

`combat_blank` is 1128 bytes — not empty. `combat_blank 1` is a named
variant, also not empty. Do not treat small-but-populated test rooms
(`combat_joacotest`, `skilltestscene`) as empty hosts.

**`spawnPos` is world `Vector2`**, not hex OffsetCoord.
`EncounterDefinition.CreateUnit` does
`new Unit(data.spawnPos + gridPos, …)`. Hex snap is later:
`LevelManager.SetUnitsObstacleStatus` → `PointToHexItem(position)`.
`combat_banditambush` hero slots are world points such as
`(6.60, -5.82)` with `config` keys `isHeroSpawn` / `isLegendSpawn`;
enemies carry `unitId` (e.g. `HumanMaleVillagerA`) plus
`unitGroup=BadSide`, `isAI=True`. Lab must store **col / row** and
convert with `HexBoard.HexToCenter`. `GetHexItem` **clamps** off-board
coords to the edge; always `IsCoordInBounds` first (Sandbox already
does).

**`boardMetadata`** is `LevelBoardSave`: `hexWidth`, `hexHeight`, sparse
`boardState` of `{HexCoord q,r,s, walkable}`.
`TheLevel.IsDefaultState` = walkable; the list is mostly **impassable**
cells. Cells not listed default `isPassable = true`
(`GameHexGridItemData` ctor). Reuse this format for authored
walkability; do not invent a second board file.

**Win/lose** (unchanged): `Stage.CheckFightEnd` only if `isFighting`.
`GoodSide == 0` → loss; else `BadSide == 0` and `OwnSide == 0` → win.
Mutual wipe is a loss. `LevelManager.AddTurnMarker()` always adds a
`TURNMARKER` (`UnitGroup.TurnMaker`); that group is neither Good nor
Bad, so it does not affect the wipe check.

Deliverable: locked. Coordinate space = OffsetCoord. Default template =
`fighttesterempty`. Picker second entry = `combat_bridge` (same live
24×24, bridge corridor + props, no enemy spawns). `combat_wip` is the
other empty field; it looks like the default, so it is not in the picker.

---

## Phase 1b — Research authored boards and exploration combat

**Status:** done 2026-08-16 (decompile + dump). **Research only** — no
implementation scheduled from this section. v1 still plays on
`fighttesterempty`.

**Verdict (vanilla only): partial.** Grow-and-paint and a visual editor
are engine-possible with hard limits. Exploration-then-combat has no
stock mode. True infinite streaming is not this type.

**Verdict (with Harmony, Phase 1c): high** for all three product goals
on a large finite board. See Phase 1c. The dummy-unit workaround is a
fallback only.

### The idea

Make encounters more unique than a borrowed arena, authored in a **full
UI editor** (not a property list):

1. **Drawn hex grid.** Paint hexes on and off, set exact shape and size,
   grow the board. Goal: explorable maps for large strategic play, not
   a 16x20 skirmish rectangle.
2. **Drag-in contents.** Enemies, heroes, and props come from palettes
   onto hexes (same gesture as placing parts in spirit, not typing
   col/row).
3. **Painted floor tiles.** Different ground art per hex, not one
   template background for the whole room.
4. **Exploration, then combat.** Entering an encounter does not have to
   mean the fight has started. The player takes their characters' turns
   on the board until a **proximity** (near enemies) or an authored
   **trigger** starts combat — Baldur's Gate 3-style.

### Verdict table

| Goal | Vanilla | With Harmony (1c) | Limiting fact |
|---|---|---|---|
| Drawn / expandable board | **partial** | **high** | Fixed rectangle. Grow width/height + paint `isPassable`. Not infinite. Pathfinding `BinaryHeap` throws above 2^18 nodes (~512x512). Vanilla max authored is 23x28; Lab can go larger. |
| Visual editor | **partial** | **high** | Units: `AddUnit` + existing hex-tap patch. Floor: `Tilemap.SetTile` / clone `HexaTile`. Props: `LoadAsset("scenario", name)` or Prefix `InstantiateObject`. Drag is Lab UI. |
| Exploration-then-combat | **partial / hostile** | **high** | No stock mode. Keep `isFighting`, Prefix `CheckFightEnd` while exploring. Park enemies `NOT_IN_INITIATIVE_BAR` until aggro. Dummy `PREVENT_END_FIGHT` is fallback only. |
| `CreateBoard` after fight load | **feasible** | **high** | Call CreateBoard + SetBoard from Lab. Postfix pool shrink. Camera limits must be grown (`fighttesterempty` AABB is 0). |
| Unity level editor leftovers | **dead as UI** | n/a | Steal `LevelBoardSave` only. |

### Board and view

- **CreateBoard + SetBoard after load:** yes in principle. Vanilla
  `LevelManager.CreateLevelFromFile` does exactly that from
  `boardMetadata` plus padding. Camera max ortho comes from
  `encounterLimits` (background collider). Walls / floor art stay on
  the loaded prefab unless Lab also replaces the Tilemap / props.
- **Hard cap:** none in pathfinding or initiative. Practical cap is
  vanilla's 23x28 metadata / ~31x32 live. `EnsureGridItemsExist` grows
  the hex pool and never shrinks. Huge maps are unproven (pathfinding +
  pool + camera cost). Do not promise infinite or city-scale boards.
- **Arbitrary shape:** paint `isPassable = false` (and hide inactive
  cells). `HexGridItemComponent.SetStatus(false)` hides the **walk
  overlay**, not the floor. Floor shape is the Tilemap, not the hex
  sprites (`HexGridItemComponentAsset` is walk/target states).
- **Grow after save:** yes — bump width/height, keep existing
  `boardState`, CreateBoard + SetBoard. **Shrink:** model yes; view
  leaves leftover pooled hexes unless Lab destroys or hides them.

`ModifyHexPassable` is an ability action that flips one cell's
`isPassable` at runtime. Useful as a paint primitive, not a map editor.

### Floor tiles and props

- Floor art is a **separate layer** from the hex overlay. Almost every
  room has `Tilemap` + `TileTestController` + `HexaTile` (neighbor-masked
  terrains). Painting = `Tilemap.SetTile` plus a `HexaTile` /
  `TerrainData` catalog (`terrainId`, autoconfig from sprite names).
  Neighbor tiles can differ. `encounter3` is a `MeshFilter` leftover,
  not the pattern to copy.
- Props: `EncounterData.encounterObjs` →
  `EncounterEntitiesGenerator.InstantiateObject` uses
  **`prefabReference` PPtr** and world `xPos` / `yPos`. `prefabName` /
  `propId` sit on the struct but the instantiate path ignores them.
  Cinematics load scenario prefabs by name
  (`CinematicHelper.InstantiateGenericPrefab` →
  `AssetBundleManager.LoadAsset<GameObject>("scenario", prefabName)`).
  Drag-in props need a **named palette** from `scenario` / `stuff`,
  then Lab `Instantiate` at a hex. They do not auto-block walkability;
  Lab must paint `isPassable` (or stamp `hasObstacle` for display).
  Prop-name inventory is **not** dumped yet — do that when the visual
  editor phase starts, not for v1.

### Exploration vs combat

Vanilla facts (unchanged). The Harmony path is Phase 1c.

- No vanilla mode with units on a HexBoard, turns happening, and
  `isFighting == false`. `StartFight` sets the flag and requires
  `initiative.ActiveUnit`.
- `isFighting` is only read in a handful of places (`Stage`, hex/target
  HUD, fight nav, initiative bar, lifebars). Those gates **want** the
  flag true so movement and skills keep working.
- Parked enemies as `NOT_IN_INITIATIVE_BAR` drop out of `CheckFightEnd`
  → instant player win unless that method is fenced.
- `EncounterLogic` is Lua cinematics, not exploration.
- Combat end raises `FightEnded`; `LevelManager.FightEndedHandler`
  starts victory / defeat / next-room. `EmbeddedFightHost.OnFightEnded`
  already `Stop()`s the hole — pocket wipe must not fire that event.

### Authoring (the visual editor)

- The Play hole can host brushes later: click-to-hex exists on the
  fight camera; drag-from-list-to-hex does not (Lab UI). Units drop
  via `AddUnit` at `HexToCenter`.
- Leftover Unity editor is dead. Reuse `LevelBoardSave` in
  `encounter.json` (or a sidecar) for width/height + sparse
  impassable cells. Store units as OffsetCoord. Store props as
  `prefabName` + col/row once a palette exists. Store tiles as
  `terrainId` per hex once the `HexaTile` catalog is dumped.
- v1 placement stays inspector fields. Brushes are post-8.

Later phases after 8, in this order: visual map editor (grid + drag
units/props + tile paint), then exploration-combat. Triggers need a
board you authored. Harmony patch list is Phase 1c.

Deliverable: written answer above. Not code.

---

## Phase 1c — Harmony patch plan

**Status:** done 2026-08-16 (decompile + existing LokrLab embed patches).
**Research only.** Do not add these patches until the post-8 visual
editor / exploration phases. v1 Play reuses the embed as-is.

This codebase already replaces tight fight methods for the Lab hole.
Encounter should **extend that host**, not invent a second embed or put
fight-flag patches in `LokrPatch` / `LokrCharacterLoader`.

### What already exists (reuse)

| Patch | What it does | Encounter use |
|---|---|---|
| `EmbeddedFightStagePatch` | Full-method `Stage.Update` while `EmbeddedFightHost.IsActive` (`SafeUpdateWithoutEncounter`). Calls `CheckFightEnd` from **our** loop. | Exploration fence belongs here (or a Prefix on `CheckFightEnd` gated the same way). Do not write a second Update replacement. |
| `EmbeddedFightStartFightSpawnPatch` | Prefix `StartFight` (priority 600); spawns roster before `FightStartedEvent`. | Generalize `SandboxRoster` / `TrySpawnRosterBeforeFight` for N combatants. |
| `EmbeddedFightHexInputPatch` | Prefix `UnitController.OnFingerTap`; hole-camera → hex. | Click-to-place and drag-drop land on this path. |
| `EmbeddedFightCameraPatches` | Replaces edge-scroll with hole drag + wheel zoom. | Keep. Do not re-apply vanilla `encounterLimits` clamps. |
| `EmbeddedSceneHexGridPatch` | Parents `HexGridRoot` under the fight scene. | Keep so a resized board unloads with Stop. |
| HUD NRE guards (`InitiativeBar`, `UnitViewComponent`, `PortraitSkill`, `PowerBarsVisibility`) | Embed-only. | Keep. |

Guard new prefixes with embed-active **and** an Encounter mode flag so
Sandbox 1v1 and campaign fights stay vanilla.

### Board and camera

| Target | Kind | Why |
|---|---|---|
| Lab calls `Stage.CreateBoard` + `HexBoardViewComponent.SetBoard` after the fight scene is ready | call, not a patch | Vanilla `LevelManager` already does this from `boardMetadata` + padding. Calling again replaces `Stage.board`. |
| `HexBoardViewComponent.EnsureGridItemsExist` / `SetBoard` | Postfix | Pool **grows only**. After shrink, hide or destroy leftover pooled hexes (`SetStatus(false)` disables the walk sprite). |
| `CameraBase.ClampTargetCameraPosition` / `SetCameraLimits` / `GrowCameraLimits` + `inGameMaxOrthoSize` | embed-only prefix/postfix | Sandbox and Setup stay unclamped. Encounter Play with an authored `camera` writes those limits and lets vanilla clamp run; missing `camera` keeps today's unlock. Campaign fights stay vanilla. |
| `HexGrid.OddRToMapDataIndex` | **do not patch globally** | Clamps off-board col/row to the edge. Campaign and pathfinding rely on that. Lab always `IsCoordInBounds` first (Sandbox already does). |

Do **not** Harmony-replace `HexBoard` / `GameHexGrid` with a sparse or
streaming grid. `PathFinder` + `BinaryHeap.Expand` **throws above
262144 nodes**. A 200×200 board (40k cells) is inside that; a 512×512
is the hard stop. "Infinitely expandable" means the **editor grows the
rect as the author paints**, then `CreateBoard` + `SetBoard`. Measure
frame cost in the hole before promising city-scale maps.

### Visual editor

| Target | Kind | Why |
|---|---|---|
| Units | none | `Stage.AddUnit` at `HexToCenter`. Hex tap already patched. Drag-from-palette is SimpleUI. |
| Walkability | none (or tiny) | Write `GameHexGridItemData.isPassable` the same way `LevelManager` stamps `boardState`. `ModifyHexPassable` is an ability action, not an editor API. |
| Floor tiles | Lab first; optional Prefix | `Tilemap.SetTile` + `ScriptableObject.CreateInstance<HexaTile>()` cloning `terrainData` from a tile already on `fighttesterempty`. `HexaTile.RefreshTile` updates neighbors. `TileTestController.ConstrainMap` **deletes** tiles outside its `width`/`height` — bump those fields before refresh, or Prefix `ConstrainMap` while Lab is painting. Do not enable SR `TilemapHack` (bakes a mesh and disables the Tilemap). |
| Props | Lab `LoadAsset` first; optional Prefix | `EncounterEntitiesGenerator.InstantiateObject` uses `prefabReference` and ignores `prefabName`. Cinematics already do `AssetBundleManager.LoadAsset<GameObject>("scenario", name)`. Prefer Lab `Instantiate` at a hex. Prefix `InstantiateObject` only if we later fill vanilla `TemplateObjectData` with a name and null PPtr. Props do not auto-block walk; Lab paints `isPassable`. Dump the `scenario` name palette when this phase starts. |

### Exploration-then-combat (preferred Harmony path)

Keep `isFighting == true` so the existing turn loop, hex input, skills
bar, and HUD keep working. Fence the wipe check. Do not invent a
Lab-owned movement loop.

| Target | Kind | Why |
|---|---|---|
| `Stage.CheckFightEnd` | Prefix, return false while Encounter exploration is on | One method. Stops instant win when enemies are `NOT_IN_INITIATIVE_BAR`. Stops `FightEnded` from firing on a pocket that is not the encounter end. Cleaner than a hidden `PREVENT_END_FIGHT` dummy. |
| `EmbeddedFightStagePatch.SafeUpdateWithoutEncounter` | extend | Already the embed `Stage.Update`. After a move / end-turn, run a Lab aggro watcher (hex `Distance` or painted trigger). On trigger: `states.Disable("NOT_IN_INITIATIVE_BAR")` on that pocket and clear the fence. |
| `LevelManager.FightEndedHandler` | Prefix skip when embed + Encounter | Belt-and-suspenders if `FightEnded` still raises (true encounter end vs pocket). Vanilla starts victory / defeat / next-room coroutines. |
| `EmbeddedFightHost.OnFightEnded` | Lab change | Today it `Stop()`s the hole. True encounter end unloads; pocket wipe must not. |
| Initiative / HUD | none extra | `NewInitiativeHandler` already excludes `NOT_IN_INITIATIVE_BAR`. Lifebars hide those units. `TakeOverAICheat` still keys off `isFighting`. |

Do not invent trigger Lua until this loop exists. `EncounterLogic`
(`OnStart` / `OnEndTurn`) is cinematic, not aggro.

Returning to exploration on the **same** board (multiple pockets) is
Lab state: re-enable the `CheckFightEnd` fence after a pocket wipe,
leave units where they stand, do not unload. Full encounter end is a
Lab decision (leave the hole, or a painted exit).

### Where patches live

```
LokrLab/Encounter/Patches/     (new Encounter-only prefixes)
LokrLab/Character/Patches/     (extend embed host / roster / OnFightEnded)
```

Not `LokrPatch` (vanilla bugfixes). Not `LokrCharacterLoader` (content
resolvers). Not `LokrLabApi` (contracts only).

### Do not patch

- `HexBoard` / `GameHexGrid` constructors (no sparse/infinite engine).
- `HexGrid.OddRToMapDataIndex` globally (clamp is load-bearing).
- `_LevelEditorStuff` / `LevelEditorRuntimeHelpers` (returns null).
- Emitting `EncounterDefinition` prefabs.
- Campaign `LevelManager.CreateLevelFromFile` for all fights.

Deliverable: this section. Not code.

---

## Phase 2 — Editor design (gate)

**Status:** locked 2026-08-16. Implement from
[`LokrLab/docs/encounter/editor-design.md`](../../LokrLab/docs/encounter/editor-design.md).
Phase 3 may start. Do not put a drawn-board editor in v1.

Goal was the Ability-overhaul equivalent of "nested cards" — pick the
shape before code.

**Phase 1b/1c verdict (one line):** **high with Harmony** — grow-and-paint
boards and a visual editor are Lab calls + small view/camera/tile
patches; exploration is Prefix `CheckFightEnd` plus the existing embed
`Stage.Update` (keep `isFighting`, park enemies until aggro). Not
infinite streaming. Not v1.

### Locked decisions

| Piece | Lock |
|---|---|
| Type id | `encounter` (`LokrLabApi.EncounterTypeId`) |
| Shape | Many-projects (`IsSingleton = false`), not Ability's old singleton |
| Folder | `Mods/LokrLab/LokrEncounterLab/<slug_token>/` |
| `ScanCategory` | `LokrEncounterLab` |
| Code | `LokrLab/Encounter/`, namespace `LokrLab.Encounter` |
| `ReferenceableProjectTypes` | `character` only |
| Workspaces | **Setup** (priority 0, default) and **Play** (priority 10). Setup is the map viewport (Phase 9) plus walkability paint (Phase 10). Not a single Play tab. |
| Bottom panels | none |
| Create sheet | Name / Slug / Alias via `LabSlugCreateFields` (`hoverPrefix` `encounter.create`) |
| Node Tree | `Encounter` root → `Combatants` folder → one `Combatant` each; `Aliases` like Character / Ability |
| Add Combatant | one node factory on `Combatants` (modal: Character ref or vanilla unit id) + File → Add Combatant |
| Jump | `OnNodeActivated` on a Character combatant → `JumpToProject`. Missing folder: inspector warning, no jump |
| Spawn | Do **not** grow `EmbeddedFightRequest`. Suite-internal `EncounterRoster` (generalize `SandboxRoster`). Pass first `GoodSide` as `CasterUnitId`; ignore default BanditRaider when the roster override is set |
| Save | `EncounterSession.IsDirty`; File → Save / Ctrl+S writes `encounter.json`. Wire `LabSaveUx.TrySaveCurrent` (today only Character / Ability) |
| schema v9 | template + combatants + sparse walkability `overrides` + tiles + terrains + props (`snap` / `x` / `y`) + optional `camera`. Triggers later |

### Data

`project.json` holds identity (same marker as Ability):

```json
{
  "projectType": "encounter",
  "schemaVersion": 1,
  "displayName": "Bandit Ambush"
}
```

`encounter.json` is the fight payload. `displayName` lives only on
`project.json` so rename updates one file.

```json
{
  "schemaVersion": 6,
  "template": "fighttesterempty",
  "walkableDefault": false,
  "tilesDefault": false,
  "overrides": [
    { "col": 8, "row": 10, "walkable": false }
  ],
  "tiles": [
    { "col": 8, "row": 10, "terrainId": 1, "template": "combat_bridge" }
  ],
  "terrains": [
    { "terrainId": 1, "name": "Ice", "source": "import", "template": "combat_bridge" }
  ],
  "combatants": [
    {
      "id": "gerald_1",
      "side": "GoodSide",
      "source": "character",
      "projectId": "necromancer_ad8174",
      "level": 1,
      "col": 6,
      "row": 10,
      "flipped": false
    },
    {
      "id": "banditraider_1",
      "side": "BadSide",
      "source": "unit",
      "unitId": "BanditRaider",
      "flipped": true
    }
  ]
}
```

| Field | Rule |
|---|---|
| `schemaVersion` | Required. v6 writes `tilesDefault`. v1–v5 files still load. |
| `template` | Prefab name. Default `fighttesterempty`. Picker also offers `combat_bridge`. |
| `walkableDefault` | New files write false (blank blocked canvas). Missing key = true (template cells stand). |
| `tilesDefault` | New files write false (strip the template floor). Missing key = true (v1–v5 keep the host Tilemap). |
| `width` / `height` | Legacy 0.12.53 only. Expanded into walkable overrides; not rewritten. Size is derived from walkable hexes. |
| `overrides[]` | Sparse walkability deltas from the loaded template. `col`, `row`, `walkable`. Empty array is legal. |
| `tiles[]` | Sparse floor-tile stamps. `col`, `row`, `terrainId`, optional `template` when the art is not from the host. Empty plus `tilesDefault` true means the template Tilemap stands. |
| `terrains[]` | Node Tree catalog. `terrainId`, `name`, `source` (`template` / `import` / `custom`), `template` prefab. Empty means scan the host room. |
| `combatants[].id` | Required, unique, legal slug. Mint from character slug / `unitId` + index (`gerald_1`). |
| `side` | `GoodSide` or `BadSide` only. Reject anything else. |
| `source` | `character` or `unit`. |
| `projectId` | Required when `source` is `character` (Character folder id). Resolve lazily; missing folder stays listed. |
| `unitId` | Required when `source` is `unit`. |
| `level` | Optional, default `1`. Hero ranks / Character only. |
| `col` / `row` | Optional OffsetCoord. Omitted = Play uses Sandbox center-offset. Phase 5 authors them. |
| `flipped` | Optional, default `false`. |

Unknown keys: rewrite may drop them (typed model, same as Ability).
Empty `combatants` is legal to save, illegal to Play. Play also refuses
zero `GoodSide`. Empty `overrides` means the template cells stand.

Not in schema v6: props, triggers, `OwnSide`, `cinematicId`,
`hasInitiative` (Play units always enter initiative).

### Hover keys (copy in Phase 8)

`encounter.create.Name` / `.Slug` / `.SlugAuto` / `.Alias` / `.AliasAuto`
/ `.IdPreview`; `encounter.template`; `encounter.combatants.Add`;
`encounter.combatant.Source` / `.Project` / `.UnitId` / `.Side` /
`.Level` / `.Col` / `.Row` / `.Flipped` / `.Clear` / `.Remove`;
`encounter.play.Start` / `.Stop`; `encounter.setup.Show` / `.Hide` /
`.Place` / `.Block` / `.Unblock` / `.Tile` / `.Erase` / `.Terrain`.

### Setup vs Play

| Workspace | v1 | Later |
|---|---|---|
| **Setup** | Summary: name, template, Good/Bad counts, read-only combatant list. Inspector still edits the selected node. | Map editor (draw grid, drag units/props, paint tiles). |
| **Play** | Embed hole + Start / Stop. Same `StartEmbeddedFight` / `fighttesterempty` host as Sandbox / Stage. | Unchanged host; exploration fence is Phase 1c / post-8. |

Opening an Encounter project lands on Setup. Switching to Play does not
auto-start the fight.

---

## Phase 3 — Project type + save

**Status:** Done. Confirmed in-game 2026-08-16 (LokrLab 0.12.36).

Implement [editor-design.md](../../LokrLab/docs/encounter/editor-design.md).

- `LokrLabApi.EncounterTypeId = "encounter"`.
- `EncounterProjectType.Register()` from suite `Awake` next to Character
  and Ability.
- Paths, `project.json` marker, create / load / delete, display name.
- `EncounterSession` with `IsDirty` on edits; File → Save writes
  `encounter.json` (empty combatants list is legal to save, illegal to
  Play). `LabSaveUx.TrySaveCurrent` calls `EncounterSession.TrySave`.
- Setup workspace summary (no Play, no combatant factory yet).
- Project Browser shows the type; New Project wizard includes it.
- xUnit: empty and combatant JSON round-trip; `ValidateCombatant` rejects
  unknown side / bad id; invalid rows are skipped on parse.

No Play yet. Create / reopen / dirty star / close prompt confirmed
in-game 2026-08-16.

---

## Phase 4 — Combatants

**Status:** Done. Confirmed in-game 2026-08-16 (LokrLab 0.12.37).

- Node Tree + Add Combatant (Character ref and vanilla unit id).
- Inspector: side, level (Character / hero ranks only), remove.
- Jump to referenced Character (switch session + back breadcrumb).
- Missing Character folder: inspector warning, still listed.
- xUnit: JSON round-trip, reject unknown `side`, slug id, mint unused
  `stem_n`.

Placement can default to Sandbox's center-offset heuristic until Phase 5.
Add Combatant (Character + unit), Jump + Back, missing-folder warning,
and dirty star confirmed in-game 2026-08-16.

---

## Phase 5 — Placement

**Status:** Done. Confirmed in-game 2026-08-16 (LokrLab 0.12.38).

- Author `col` / `row` / `flipped` (Phase 1a locked OffsetCoord).
- Clamp to the **live** board (authored + LevelManager pad 8×4).
  `fighttesterempty` / `combat_wip` are 24×24. Empty hex = Play
  center-offset.
- Warn on duplicate hex and partial col/row. Play still rejects
  duplicates.
- v1 is inspector fields, not a click-to-place hex overlay. Drag-onto-hex
  is the target editor (Phase 1b / post-8), not a v1 gate.

Clamp, Clear placement, flipped, and duplicate warning confirmed
in-game 2026-08-16.

---

## Phase 6 — Play in embed

**Status:** Done. Confirmed in-game 2026-08-16 (LokrLab 0.12.39).

- Play workspace hole; Start / Stop.
- Spawn the authored roster **before** `Stage.StartFight` (same
  `TrySpawnRosterBeforeFight` rule as
  [fight-started-empty-initiative-nre.md](../../issues/resolved/fight-started-empty-initiative-nre.md)).
- Refuse Play with zero `GoodSide` units.
- Fight-end unloads the hole; lab stays on Encounter Play.
- Do not call `ReopenAfterFight`. Do not enable the vanilla debug spawn
  panel unless we explicitly want it (default: off — this is an authored
  fight, not Sandbox iterate).
- Ability Stage and Character Sandbox 1v1 must still work.
- Do not add "load Encounter" to Sandbox in this phase. That consumer
  comes after Encounter Play is confirmed.

Code: `EncounterPlay` (arm flag), `EncounterRoster` (spawn),
`EncounterPlayRules` (Unity-free validate), `EncounterPlayViewport`
(Start / Stop hole). `EmbeddedFightHost` uses the session `template`
and the Encounter roster when armed. Play uses the in-memory payload
(no auto-save).

Start / Stop, authored roster spawn, GoodSide validation, and Stage /
Sandbox 1v1 confirmed in-game 2026-08-16.

---

## Phase 7 — Arena template picker

**Status:** Done. Confirmed in-game 2026-08-16 (LokrLab 0.12.41).

- Dropdown of Phase 1's empty-enough template names; default
  `fighttesterempty` (open field). Second entry: `combat_bridge`
  (bridge corridor, no enemies). Both live 24×24. `combat_wip` looks
  like the default, so it is not offered.
- Changing template dirties the project and clamps placement; Play uses
  the in-memory name.
- Two usable templates exist, so the picker is shown. An unknown saved
  name stays on the list until the author picks an empty host.
- If only one usable template exists, skip the picker and document that.

Code: `EncounterTemplateRules` (Unity-free names / options). Encounter
inspector dropdown replaces the free-text field.

Dropdown, `combat_bridge` vs open field, dirty star, and Play using the
in-memory name confirmed in-game 2026-08-16.

---

## Phase 8 — Hover copy

**Status:** Done. Confirmed in-game 2026-08-16 (LokrLab 0.12.42).

- `LokrLab/Sidecars/encounter-hover.md` plus optional
  `Mods/LokrLab/encounter-hover.md` overlay.
- Extend `AbilityHoverCopy.Reload` / `LabHoverInfo` the same way
  character-hover was added (third sidecar, later files win per key).
- Bind create sheet, combatant fields, Play / Stop, template.
- Copy rules: engine meaning + gotcha + legal values
  ([lab-hover-coverage.md](../completed/lab-hover-coverage.md)).

`LabHoverInfo` loads the third sidecar after character-hover. Later
files win per key. Compiled fallbacks cover `encounter.template`,
`.play.Start`, and `.combatant.Side`.

Hover strip, create sheet, template, combatants, and Play / Stop keys
confirmed in-game 2026-08-16.

---

## Phase 9 — Setup board hole + click-to-place

**Status:** Done and confirmed in-game 2026-08-16 (LokrLab 0.12.47).

First visual-editor slice. Taps write existing `col` / `row`.

- Setup toolbar: Show board / Hide board, hint, template line, hole.
- `EncounterEdit` arms the embed; `EncounterRoster` previews the
  authored roster. Empty roster is legal. No GoodSide uses
  `BanditRaider` as caster id only — do not spawn the default 1v1.
- Prefix `Stage.CheckFightEnd` while edit is armed so the preview
  does not auto-defeat. The embed `Stage.Update` skips the turn loop;
  fight HUD stays hidden; walkable hexes are painted (not a move range).
- Tap a preview unit to select that combatant. Tap an empty hex with
  a combatant selected to write col/row, dirty Save, and move the
  preview. Reject OOB, duplicate, and impassable hex. Do not
  call `SelectPoint`.
- Play Start still works. Stage / Sandbox stay 1v1.
- Wipe / win unloads the hole. Does not Single-load victory or defeat.

Confirmed in-game 2026-08-16:

- Setup → Show board: template appears; empty roster does not auto-defeat.
- Select a combatant, tap a hex: inspector col/row update, dirty `*`, preview moves.
- Tap a preview unit: that combatant selects in the tree.
- Duplicate / OOB / impassable tap: status bar, no write.
- Hide board / leave Setup: embed stops. Play Start still works. Stage / Sandbox stay 1v1.
- No AI turns; no fight HUD; overlay is walkable hexes only.

---

## Phase 10 — Walkability paint

**Status:** Done and confirmed in-game 2026-08-16 (LokrLab 0.12.52).

Sparse authored walkability on the stock template.

- Schema v2: `overrides: [{ col, row, walkable }]`. v1 files still
  load (empty overrides). Always write schema 2 (until Phase 11 writes 3).
- Setup tools: Place / Block / Unblock. Default Place.
- Block / Unblock tap an empty hex, dirty Save, write live
  `isPassable`, refresh the walkable-only overlay. Occupied hex:
  select the unit and refuse paint.
- Apply overrides before roster spawn on Show board and Play.
- Play AI-to-player HUD:
  [`play-ai-first-missing-walk-and-skills.md`](../../issues/resolved/play-ai-first-missing-walk-and-skills.md).

Confirmed in-game 2026-08-16: Place / Block / Unblock, Play uses
overrides, fight-end stays in Lab, AI-first player turn shows walk
hexes and the skills bar.

---

## Phase 11 — Grow-board

**Status:** Done and confirmed in-game 2026-08-16 (LokrLab 0.12.59).

Grow or shrink the live HexBoard from walkable hexes. No stored
width/height.

- Schema v3. Live size is derived from walkable overrides and
  placements (template live minimum 24×24, cap 64). Leftover 0.12.53
  `width` / `height` expand into walkable overrides and are not rewritten.
- Setup Unblock on the one-hex impassable ring past the walkable edge
  grows the board. Block those outer walkable cells shrinks back to the
  template minimum. Play has no halo.
- Drag-paint Block / Unblock. Off-board clicks use unclamped `PointToHex`
  and grow to the cursor hex. Hover ghost marks the off-grid target.
- Embed camera is unclamped (pan + wheel zoom). Place snaps without
  `Unit.Move` occupancy so no leftover invalid hex.

Confirmed in-game 2026-08-16: grow-to-cursor, drag-paint, hover ghost,
no stray pool hexes, cannot Block under a unit, Place leaves no trailing
invalid overlay, camera pan/zoom in the hole.

---

## Phase 12 — Floor-tile paint

**Status:** Done and confirmed in-game 2026-08-16 (LokrLab 0.12.63).

Sparse `HexaTile` stamps on the template Tilemap. No props. Size still
comes from walkable hexes.

- Schema v4: `tiles: [{ col, row, terrainId }]`. v1–v3 still load
  (empty tiles). Always wrote schema 4 (v5 now).
- Setup Tile / Erase. Only terrains with neighbor-masked hex sprites
  paint. Names like `MountainDarkDirt` that only have an `errorSprite`
  are omitted. Left-drag paints; Erase or right-drag restores the
  template. Off-board: Unblock first.
- Map odd-r hexes onto the vanilla rectangular Grid. Each combat hex
  is two cells wide (left + right A/B sprites) — stamp both or only
  the left half fills. Odd rows shift +1. Reuse a `HexaTile` already
  on the map. While Encounter owns the embed, a `GetTileData` postfix
  swaps `errorSprite` for an interior hex sprite when neighbor masks
  miss.
- Prefix `TileTestController.ConstrainMap` while Encounter owns the
  embed so vanilla crop does not delete painted cells past the template
  rect. Campaign fights stay vanilla.
- Apply on Show board and Play.

Confirmed in-game 2026-08-16: two-cell hex stamps sit on the hexes;
Erase restores the template; Tile does not grow the board.

---

## Phase 13 — Terrain catalog

**Status:** Done and confirmed in-game 2026-08-16 (LokrLab 0.12.70).

Move terrains out of the Setup toolbar and into the Node Tree so
imported stage art and later custom terrains have a home.

- Schema v6: `tilesDefault` false on new files (strip the template
  floor). `terrains: [{ terrainId, name, source, template }]`.
  Optional `tiles[].template` when the stamp is not from the host.
  v1–v5 still load (missing `tilesDefault` keeps the host floor).
  Always write schema 6.
- Terrains folder beside Combatants. Host hex-art terrains scan from
  the already-loaded `templates` bundle (do not call `LoadAssetBundle`
  again — that nulls the cache and Show board NREs). Import from
  another prefab. Custom mints `custom_N` (no art yet).
- Select a terrain node (or Use for Tile) to switch Setup to Tile.
  Toolbar dropdown is gone.
- Paint resolves `HexaTile` from the source prefab, not only the live
  host Tilemap. Custom without art refuses paint.
- Lab embeds hide vanilla Paralax foregrounds (leaves, vines). The
  unclamped embed camera makes those layers sit wrong. Campaign
  fights keep them.

Confirmed in-game 2026-08-16: Terrains folder lists host art; import
from another stage paints that sheet; custom is a stub; Paralax
foregrounds stay off in the embed.

---

## Phase 14 — Scenario props

**Status:** Done and confirmed in-game 2026-08-16 (LokrLab 0.12.71).

Place `scenario` deco prefabs on hexes. Vanilla rooms store props as
PPtrs; Lab loads by name the same way cinematics do
(`LoadAsset("scenario", name)`, lowercased). Palette dump:
[scenario/catalog/](../../character-reference/_extracted/base-game/scenario/catalog/).

- Schema v7: `props: [{ id, prefabName, col, row, flipped }]`.
  v1–v6 still load (missing key = empty). Schema v9 adds `snap`
  (default true) and free-move `x` / `y`. Always write the current
  schema.
- Props folder beside Terrains. Add Prop picks a deco name (asset
  path contains `deco`). File → Add Prop.
- Select a prop, Place, tap a live hex. Does **not** auto-block walk
  (use Block). Does **not** grow the board (Unblock first).
- Show board and Play instantiate under `HexGridRoot`. Campaign
  fights stay vanilla.

Confirmed in-game 2026-08-16:

- Add Prop lists deco names. Pick one; a Node Tree row appears.
- Place on a live hex: the prefab appears. Save / reload / Show /
  Play keep it. Flipped mirrors X.
- The hex stays walkable until Block. Off-board taps do not grow.
- Stage / Sandbox / campaign rooms are unchanged.

---

## Phase 15 — Play camera bounds

**Status:** Done and confirmed in-game 2026-08-16 (LokrLab 0.12.77).

Author a world-space AABB in Setup and apply it only during Encounter
Play. Setup stays unclamped so the rect can be drawn. Sandbox and
campaign fights stay vanilla. Do not grow `EmbeddedFightRequest`.

- Schema v8 optional `camera`: `{ minX, minY, maxX, maxY, lockZoom,
  orthoSize? }`. Missing object = today's unclamped embed. v1–v7 still
  load.
- Setup Camera tool: drag to create, handles to resize, interior to
  move. Cyan overlay. Encounter inspector: numbers, lock zoom, Use
  current view (copy the Setup hole frustum), Clear.
- Play writes `CameraBase.cameraLimits`, enables vanilla
  `ClampTargetCameraPosition`, and skips wheel zoom when `lockZoom` is
  on. Authored `orthoSize` is clamped so the view still fits the rect.
  `extendEncounterLimitsWidth` stays false on authored bounds.

Confirmed in-game 2026-08-16:

- Show board, Camera tool (or Use current view), Save, Play: pan stays
  inside the cyan rect and zoom stays locked when the toggle is on.
- Clear camera (or a v7 file) keeps Play unclamped.
- Setup still pans and zooms freely while drawing the rect.
- Sandbox and campaign fights are unchanged.

---

## Phase 16 — Exploration-then-combat

**Status:** Done and confirmed in-game 2026-08-16. Sub-phase 16a
(pockets, per-unit aggro radius, fight-end fence, LokrLab 0.12.90) and
sub-phase 16b (painted trigger regions with a catalog + pocket
targeting, LokrLab 0.12.96) both confirmed.

Entering an Encounter doesn't have to mean the fight has already
started. The player moves GoodSide units on the board — `isFighting`
stays true so the existing turn loop, hex input, skills bar, and HUD
keep working — while BadSide combatants sit parked
(`NOT_IN_INITIATIVE_BAR`, no turn, hidden from the bar) until a **pocket**
of them aggroes. True multi-pocket: independent enemy groups wake
independently, a partial kill never falsely ends the encounter, and a
real party wipe or a true full clear (every pocket, parked or not) still
fires normally with zero changes to `LevelManager.FightEndedHandler` or
`EmbeddedFightHost.OnFightEnded` — see the fence design below.

### Key fence insight (why no pocket bookkeeping is needed in `CheckFightEnd`)

Vanilla `Stage.CheckFightEnd` excludes `NOT_IN_INITIATIVE_BAR` units from
its living-side counts. GoodSide is never parked, so a real party wipe is
always detected correctly. The only bug case: some BadSide units are
parked-but-alive while the currently-active pocket is fully dead, so
vanilla's *filtered* BadSide count hits 0 and declares victory early.

**Correction found in first in-game test:** the fence must not scan
`stage.units` for *any* `BadSide && NOT_IN_INITIATIVE_BAR` unit — some
unit definitions (dummy / hazard / trap kinds, e.g. `IcicleRainDummyUnit`,
`DummyUnitThornTrap`) ship `NOT_IN_INITIATIVE_BAR` **and**
`NON_TARGETABLE` baked into their own `states` block, permanently, by
design, unrelated to exploration. A blind scan treats such a unit as an
eternally-unaggroed pocket and blocks victory forever even after every
authored enemy is dead. The fence instead asks
`EncounterExploration.HasLivingParkedMembers()` — true only for units
this system actually parked and is still tracking. Units that already
carry `NOT_IN_INITIATIVE_BAR` right after spawn (before Encounter's own
`Enable` call) are skipped from pocket tracking entirely in
`EncounterRoster.BeginExploration`, left exactly as authored. With that
scoping: **is any exploration-tracked BadSide unit alive and still
parked?** If not, delegate straight to the original method — covers
mid-fight, a true full clear, and a real defeat identically.

### Sub-phase 16a — pockets, per-unit aggro radius, fight-end fence

**Status:** Done and confirmed in-game 2026-08-16 (LokrLab 0.12.90).

- Schema v10: file-level `exploration` (bool, default false — off is
  today's instant-fight behavior byte-for-byte) and `defaultAggroRadius`
  (int, default 4). Per-BadSide-combatant `pocket` (string, optional —
  shared non-empty value wakes together; empty = solo pocket keyed by
  the combatant's own id) and `aggroRadius` (int, optional — overrides
  the file default for that unit only). v1–v9 files still load.
- `EncounterRoster.Spawn` → `BeginExploration`: when `exploration` is
  true, every BadSide unit that does **not already** carry
  `NOT_IN_INITIATIVE_BAR` right after spawn parks
  (`states.Enable("NOT_IN_INITIATIVE_BAR")`) and is grouped into pockets
  by `pocket`/id, each member carrying its resolved radius. A unit that
  is *already* parked at spawn (dummy / hazard / trap `UnitDefinition`s
  with `NOT_IN_INITIATIVE_BAR` baked into their own `states` block) is
  skipped — never tracked, never woken, never touched.
- `EncounterExploration.Tick`, wired into
  `EmbeddedFightStagePatch.SafeUpdateWithoutEncounter` right after the
  `EncounterEdit.IsArmed` branch (no early return — normal turn logic
  keeps running for GoodSide's own turns): every embed frame, checks
  still-parked pockets against living GoodSide hexes via
  `EncounterExplorationRules.PocketsToAggro` (Unity-free hex-distance
  math). A pocket wakes the instant **any** living member's own radius
  reaches a GoodSide unit — mixed short/long-range members in the same
  pocket are allowed. Once aggroed, a pocket is not rescanned. Waking a
  member also calls `InitiativeBar.initiativeInst.AddPortrait(unitView)`
  — the bar's `initiativePositions` is a one-time snapshot built at
  `FightStartedEventHandler` from whichever units aren't parked *then*;
  nothing else re-adds a portrait for a unit that becomes visible later,
  so this call is required or the woken unit fights with no bar entry.
  Guarded against units with no `ExoSkeletonData` (vanilla's own
  `AddPortrait` NREs on that; dummy/hazard kinds have none, but they're
  already excluded from tracking above, so this is a defensive backstop).
- `EncounterExplorationFightEndPatch` prefixes `Stage.CheckFightEnd`
  per the fence insight above — asks
  `EncounterExploration.HasLivingParkedMembers()`, not a raw
  `stage.units` scan.
- Inspector: root Encounter node gets an `Exploration` toggle and
  `Default Aggro Radius` field; each BadSide combatant gets `Pocket`
  and `Aggro Radius` fields (shown only while Exploration is on).
- xUnit: schema round-trip for `exploration` / `defaultAggroRadius` /
  per-combatant `pocket` / `aggroRadius`; `EncounterExplorationRules`
  pocket-aggro math (in/out of radius, multiple pockets, per-member
  radius, no GoodSide hexes, empty pockets).

Confirmed in-game 2026-08-16:

- `exploration:false` (default/legacy file): Play behaves identically
  to today — every combatant joins initiative immediately.
- Two pockets (a tagged pair sharing `pocket`, plus a solo untagged
  enemy) with different `aggroRadius` per member: enemies stand on the
  board but are absent from the initiative bar, skills bar, and AP bar;
  the player can move freely turn after turn without any enemy acting.
- Approaching only the solo enemy wakes just it and adds its portrait
  to the initiative bar; killing it does not end the encounter (no
  premature victory) while the tagged pair stays parked and inert.
- Approaching the tagged pair wakes both together, both get portraits;
  clearing everything ends the fight normally (victory fires, hole
  tears down as today).
- A dummy/hazard/trap combatant (already `NOT_IN_INITIATIVE_BAR` from
  its own definition) in the same encounter never joins initiative,
  never gets a portrait, and never blocks victory once every real
  enemy is dead.
- Save / reload keeps `exploration`, `defaultAggroRadius`, and every
  `pocket` / `aggroRadius`.
- Setup, Sandbox's own 1v1, and campaign fights are unaffected.
  Sandbox Load Encounter (Phase 17) must run this same exploration
  path — see 0.12.100.

### Sub-phase 16b — painted trigger regions

**Status:** Done and confirmed in-game 2026-08-16 (LokrLab 0.12.96).
Redesigned once already (below) from first-pass feedback before ever
confirming — schema v11 (same-day) does not migrate to v12.

Adds a drawable alternative to plain radius: a **trigger catalog**
(mirrors Terrains: a real Node Tree row per trigger, not inferred from
painted cells) where a trigger can name a **target pocket** — entering
the painted region wakes that whole pocket directly, which is the
primary authoring path (tag your pocket, paint one region, done — no
per-unit bookkeeping). A combatant can *also* opt into a trigger
individually via its own `Trigger` field, independent of which pocket
that trigger targets, for one-off "this specific unit also wakes on
this region" cases. Either path can fire; a pocket wakes on whichever
condition (its own trigger, a member's radius, or a member's individual
trigger) is satisfied first.

**First-pass feedback that drove this redesign:** Add Trigger didn't
show in the tree until a cell was painted (ids were inferred from
painted/referenced data, not a real catalog row); the paint overlay
only updated on hover, not on tool/selection change; there was no
rename; and forcing every pocket member to set its own `Trigger` field
was redundant busywork instead of one assignment on the trigger itself.

- Schema v12: **`triggers`** is now the catalog —
  `[{ id, pocketKey? }]` — one row per trigger, `pocketKey` optional
  (which pocket this trigger wakes as a whole). **`triggerCells`** is
  the sparse paint — `[{ col, row, triggerId }]`, one triggerId per hex,
  last paint wins (same model as `tiles`). Per-BadSide-combatant
  `triggerId` (string, optional) is unchanged: an individual opt-in,
  independent of the trigger's own `pocketKey`. v1–v10 files still
  load; v11 (same-day, never confirmed) does not migrate — that shape
  reused the `triggers` key for painted cells directly, which v12's
  catalog now owns.
- `EncounterTriggerRules.cs` (Unity-free), catalog half: `Find(file,
  id)`, `Add`, `Rename` (cascades to every painted cell and every
  combatant's `triggerId`), `RemoveDefinition` (drops the catalog row,
  every painted cell, and every combatant reference), `CatalogIds`,
  `CombatantsUsing` (individual opt-ins), `PocketMembers` (BadSide
  combatants sharing the trigger's `pocketKey`, matching the same
  "blank Pocket = solo keyed by id" rule as 16a), `MintTriggerId`. Cell
  half unchanged in shape, renamed to operate on `TriggerCells`:
  `Find(file, col, row)`, `Set`, `Clear`, `RemoveAllCells`, `HexesFor`.
- `EncounterExplorationRules.PocketsToAggro` gained an optional
  `pocketRegions` parameter (`pocket key → List<HashSet<HexPos>>`):
  checked per pocket *in addition to* member conditions — a pocket
  wakes if either its own named-trigger region is entered, or any
  member's own radius/region condition fires. Looked up only for
  pockets already present in `parkedPockets`; a trigger naming a
  pocket that isn't tracked (already fully aggroed, or a typo) is a
  safe no-op. `EncounterRoster.BeginExploration` builds this map from
  every catalog trigger with a non-empty `pocketKey`, resolved via
  `OffsetCoord.RoffsetToCube` same as per-member regions.
- Setup: `Trigger` tool unchanged (left-drag paints the Node-Tree-
  selected id, right-drag erases, `EncounterPaintRules.ForEachOnLine`,
  off-board needs Draw Hex first, `AREA_DAMAGE` hex overlay). **Fixed:**
  the overlay now repaints (`EncounterEdit.ShowFullGrid()`) on every
  tool switch and every trigger selection change (toolbar button, Node
  Tree Paint button, Add Trigger), not only on hover. **Fixed:**
  `EmbeddedFightCameraPatches`'s right-drag-owns-paint check
  (`paintOwnsRight`) only listed `DrawHex`/`PaintTerrain`, so right-drag
  erase on the Trigger tool also panned the embed camera underneath the
  stroke. Added `EncounterEditTool.Trigger` to that list. **Fixed:**
  `PaintTriggerLine`/`EraseTriggerLine` dirtied Save and refreshed the
  Setup toolbar per stroke but never called
  `LokrLabApi.LokrLabApi.RequestRefresh()`, so the open trigger's
  inspector (painted-hex count, connected-units lists) went stale while
  painting — Tile/Terrain painting never needed this since their
  inspectors don't show a live paint-derived count. Both now call it.
  **Fixed:** cells were keyed by `(col, row)` alone (one trigger per
  hex, matching Tile's one-terrain-per-hex model), so painting trigger B
  over a hex already owned by trigger A silently stole it — and
  right-drag erase while A was selected could erase B's cell at that
  hex. Cells are now keyed by `(col, row, triggerId)`: `Find(file, col,
  row)` is gone; `HasCell`/`Set`/`Clear` all take the trigger id, so
  each trigger owns its own independent hex set and regions can overlap
  on the same hex without disturbing each other.
- **Fixed, shared embed input (not Encounter-specific):**
  `LokrCharacterLab.Patches.EmbeddedFightHexInputPatch` (the
  `UnitController.OnFingerTap` Prefix every embedded fight uses —
  Sandbox, Ability Stage, Encounter Setup/Play) never checked which
  mouse button caused the tap. LeanTouch's mouse-simulated finger fires
  `OnFingerTap` for right- and middle-clicks too, so panning the embed
  camera with the middle button (or right-drag on a non-paint tool)
  also selected, placed, or moved a unit underneath the drag. Added
  `IsNonLeftMouseTap()`: swallows the tap when a mouse button is
  involved and it isn't specifically a left-button release; pure touch
  input (no mouse buttons down or just released) is unaffected.
- Node Tree: `Triggers` folder lists `file.Triggers` (the catalog)
  directly, so **Add Trigger appears immediately** — mints into the
  catalog, arms the tool, selects the new node. Each row: **rename**
  (id text field, cascades), **Pocket** field (`pocketKey`, blank =
  none), cell count, Paint, a live list of connected units split into
  "Pocket members" (via `pocketKey`) and "Individually opted in" (via
  each combatant's own `Trigger` field), and Remove (full cascade).
- Per-BadSide-combatant inspector: `Trigger` dropdown sourced from the
  catalog (`CatalogIds`), unchanged position/behavior otherwise.
- Hover copy: added `encounter.trigger.Rename`, `encounter.trigger.Pocket`;
  updated `encounter.triggers.Add`, `encounter.trigger.Paint`,
  `encounter.trigger.Remove`, `encounter.combatant.TriggerId` for the
  catalog + pocket-targeting model.
- xUnit: `EncounterTriggerRulesTests` rewritten for the catalog API
  (Add/Rename-cascades/RemoveDefinition-cascades/CombatantsUsing/
  PocketMembers/MintTriggerId) plus cell storage; `EncounterExplorationRulesTests`
  extended for `pocketRegions` (wakes regardless of member conditions,
  no false trigger outside the region, safe no-op for an untracked
  pocket key).

Confirmed in-game 2026-08-16:

- Add Trigger: the row appears in the Triggers folder immediately, with
  zero painted cells.
- Select a trigger (toolbar button or Node Tree Paint): the painted
  region shows the `AREA_DAMAGE` overlay immediately, without hovering.
- Rename a trigger: painted cells and any combatant's Trigger field
  still resolve correctly under the new id.
- Set a trigger's Pocket to a tagged pocket (no per-unit Trigger
  fields set): entering the painted region wakes every member of that
  pocket at once.
- A combatant outside that pocket can still individually select the
  same trigger from its own Trigger field and wake on the same region.
- The trigger's inspector lists the right pocket members and individual
  opt-ins as combatants are added/changed.
- Remove Trigger clears the catalog row, every painted cell, and every
  combatant reference (both pocket-targeting and individual opt-in
  paths stop firing).
- Two triggers painted over the same hex: each shows its own correct
  cell count; right-drag erasing one while it's selected leaves the
  other's cell on that hex untouched.
- Painting off-board does not grow the board; Draw Hex first does.
- Save / reload / Show board / Play all keep the catalog, cells, and
  every combatant's Trigger field.
- Setup, Sandbox's own 1v1, and campaign fights are unaffected.
  Sandbox Load Encounter (Phase 17) must run this same exploration
  path — see 0.12.100.

---

## Phase 17 — Hero spawn points + Sandbox "Load Encounter"

**Status:** Confirmed in-game 2026-08-17 (LokrLab 0.12.104).

0.12.102 removes Encounter Play and Ability Stage. Character, Ability,
and Encounter all use a Sandbox workspace (Start sandbox / Stop
sandbox) and shared `LabSandboxChrome`. The live-fight arm is
`EncounterSandbox`.

0.12.101 folds Sandbox Load Encounter into `EncounterPlay` (optional
spawn fill + debug panel). One live-fight arm: tiles, exploration,
camera bounds, enemy AI, and win/lose. The exploration fight-end fence
uses vanilla's living-GoodSide filter so a dead hero ends the fight
even while pockets are still parked. See
[`docs/issues/resolved/sandbox-load-encounter-hero-death-does-not-end.md`](../../issues/resolved/sandbox-load-encounter-hero-death-does-not-end.md).

0.12.99's Sandbox load put every enemy into initiative on an
`exploration` encounter: parking, aggro/trigger tick, and the
fight-end fence were Play-only. 0.12.100 ran that path on a third arm.
Confirmed in-game 2026-08-17. See
[`docs/issues/resolved/sandbox-load-encounter-skips-exploration.md`](../../issues/resolved/sandbox-load-encounter-skips-exploration.md).

0.12.98's Sandbox load left the host `fighttesterempty` grass instead of
the authored floor: the tile crop/sprite patches only treated Setup and
Play as "Encounter owns the embed." 0.12.99 includes Sandbox-loaded
Encounter in that gate and re-applies tiles after fight start. Confirmed
in-game 2026-08-17. See
[`docs/issues/resolved/sandbox-load-encounter-drops-authored-tiles.md`](../../issues/resolved/sandbox-load-encounter-drops-authored-tiles.md).

0.12.97's first in-game pass found three bugs, fixed in 0.12.98: adding
a spawn point restarted the whole Setup preview (full board reload),
leaving the "Loading board…" overlay stuck visible over the already-
loaded board; clicking a hex to place a spawn point spawned a broken
placeholder unit there (the click-to-place path called
`EncounterRoster.TrySpawnOne`, which had no spawn-row skip — only
`EncounterRoster.Spawn`, the full-roster path, did); and spawn points
were a third source button mixed into the Combatants catalogue instead
of their own thing. See below for what changed.

Every GoodSide row before this phase was a **fixed** hero (a specific
Character project or vanilla unit id) — fine for an encounter authored
to always fight the same named hero, but unusable for Sandbox loading
an Encounter (the hero should be whichever character is currently open
in Character Lab) or, later, Custom Adventures (heroes are the
player's dynamically assembled party, never known at authoring time).
Vanilla already has this exact concept for campaign rooms —
`LevelPieceConfig.IS_HERO_SPAWN` / `isLegendSpawn` mark a **position**,
not an identity. This phase brings that into the Lab schema as an
authored **Hero Spawn Point**: a GoodSide combatant row with a hex
position and no fixed Character/unit reference, filled at load time by
whoever spawns into the encounter.

**Locked:** Encounter Lab's own standalone Play does **not** fill spawn
points — they're reserved but empty, and "needs at least one GoodSide
combatant" only counts real (Character/unit) rows. An encounter
authored entirely with spawn points can't be Play-tested standalone;
Sandbox-load is how you test it with a real character. The schema
supports **multiple** spawn points (any number of `source="spawn"`
rows); Sandbox's loader only ever fills the **first**, since it only
ever has one hero — a future Adventures feature extends the same fill
mechanism to loop over a party, no further schema change needed then.

- Schema v13: combatant `source` gains `"spawn"` (GoodSide only, no
  `projectId`/`unitId` written). v1–v12 files still load (no spawn rows
  existed before, nothing to migrate).
- `EncounterPlayRules.CanPlay`: the "needs one GoodSide" check now
  counts only non-spawn rows; a spawn row skips the "no unit id" check
  but must have both Col and Row set, else Play refuses with "Hero spawn
  point '&lt;id&gt;' needs a hex — place it before Play." `FirstGoodSide`
  skips spawn rows (so a real authored hero, if any, still gets tagged
  `isHero` correctly); new `FirstSpawnPoint` finds the first spawn row.
- `EncounterRoster.Spawn` gained optional `fillUnitId`/`fillLevel`
  parameters (default null — every existing caller is unaffected). A
  spawn row with no fill available is skipped silently (an intentionally
  empty slot, not an error); the first spawn row is filled when a fill
  is supplied, resolved the same way a unit-sourced combatant already
  is (`SandboxRoster.ResolveDefinitionAtLevel`). If resolving or placing
  the first spawn row fails, the fill rolls over to try the next one.
- Setup preview: spawn points show a persistent cyan marker (distinct
  from the yellow hover-ghost and the red trigger overlay) instead of a
  spawned unit — no persistent multi-instance board marker existed
  before this (`EncounterEdit`'s `hoverGhost` was a single shared
  instance for the mouse cursor only). Refreshed on board show and on
  any placement change.
- Node Tree: Spawn Points is its own top-level folder, a sibling of
  Combatants/Terrains/Props/Triggers, not a third Combatants-catalogue
  source. "Add Spawn Point" mints a row and selects it directly (no
  catalogue, no card — mirrors Triggers' "Add Trigger"). The row
  inspector shows an explanatory line and placement fields only (no
  Character/Unit fields, no Side dropdown — always GoodSide).
- `EncounterRoster.TrySpawnOne` (the click-to-place path used when
  placing a combatant by tapping a hex, as opposed to `Spawn`'s
  full-roster pass) now also skips `source="spawn"` rows, and
  `EncounterEdit.SpawnPreviewCombatant` short-circuits to a marker
  refresh before calling it at all — placing a spawn point by clicking
  a hex no longer spawns anything there.
- Adding or placing a spawn point never restarts the live Setup
  preview (`AfterSpawnPointsChanged` always passes `restartPreview:
  false` and calls `EncounterEdit.RefreshSpawnMarkers()` directly) — a
  spawn point needs a marker repositioned, not a full roster respawn.
- Sandbox Load Encounter arms `EncounterPlay` with a fill (current
  character + level) and `ShowDebugPanel`. Same live-fight path as
  Encounter Lab Play: roster, tiles, exploration, camera bounds, enemy
  AI, win/lose. Spawn points stay empty on standalone Play. Sandbox's
  own 1v1 (Start sandbox, no project) is unchanged.
- Sandbox gets a "Load Encounter" toolbar button reusing the existing
  `ProjectReferencePickerModal` (no new picker UI needed — it already
  lists projects of any type). Refuses a pick with no Hero Spawn Point.
  On load: current character + selected level fill the first spawn
  point; every other authored combatant, terrain, prop, trigger, and
  exploration rule (pockets, aggro radius, painted regions) plays as
  authored.
- Hover copy: `sandbox.LoadEncounter`, `encounter.spawnpoints.Add`,
  `encounter.spawnpoint.Remove`.
- xUnit: `EncounterFileModelTests` (spawn round-trip, no projectId/unitId
  written, rejects spawn+BadSide); `EncounterPlayRulesTests` (spawn rows
  excluded from the GoodSide-count / unit-id checks, blocked without
  placement, `FirstGoodSide`/`FirstSpawnPoint` skip/find correctly). The
  roster fill mechanism and Sandbox UI are Unity-coupled, consistent
  with the rest of those files not being unit-tested.

In-game confirm (do not mark this phase done until these):

- Spawn Points is its own Node Tree folder next to Combatants. Add
  Spawn Point does not restart the Setup preview or leave "Loading
  board…" stuck on screen.
- Place a spawn point by clicking a hex (Place tool): only the cyan
  marker appears, never a placeholder/broken unit.
- Add a Hero Spawn Point, place it: Setup preview shows the cyan
  marker, not a spawned unit.
- Standalone Play with only a spawn point (no other GoodSide row):
  refused with a clear message. Add a real hero alongside it: Play
  works, the spawn point stays empty, the real hero is still the
  skills-bar / facing hero.
- Sandbox → Load Encounter → pick a project with a spawn point: the
  currently open character spawns at the authored position and facing,
  at Sandbox's selected level. The authored floor (not
  `fighttesterempty` grass), combatants, props, and triggers play as
  authored. An `exploration` encounter parks BadSide pockets until
  aggro radius or a painted trigger; killing a woken pocket does not
  end the fight while another pocket is still parked. Hero death is a
  defeat even while pockets are still parked. Authored Play camera
  bounds apply. Stop / Start Sandbox's own 1v1 afterward still works
  unaffected.
- Two spawn points authored: Sandbox-load fills only the first; the
  second stays empty.
- Loading an Encounter with no spawn point: Sandbox refuses with a
  clear status message, does not start the embed.
- Save / reload keeps `source="spawn"` rows and their placement.

---

## Later (not v1)

After Phase 17:

- `stuff` bundle deco names, if any are missing from `scenario`.
- **Vanilla room import / remix / load override** is a new research
  track: [vanilla-encounter-edit.md](../started/vanilla-encounter-edit.md).
  Reconstruct a `templates` prefab into `encounter.json`, play it in
  Sandbox, then (if locked) a **guarded** campaign load hook so that
  room name uses Lab JSON. Do not write prefabs back. Do not globally
  rewrite every `CreateLevelFromFile` call.

Still not this track:

- Custom Adventures (map, quest chain, multiple rooms, rewards).
- Waves / `encounterHelper` Lua.
- Custom win/lose beyond vanilla wipe.
- Authoring Encounter inside Sandbox.
- Third-party spawnable types (Sandbox's deferred extension point).

---

## Risks

| Risk | Mitigation |
|---|---|
| Over-fitting `ProjectTypeRegistration` to Character/Ability | Encounter is the many-projects + cross-ref validation; keep Ability's library shape unchanged |
| Bloated `EmbeddedFightRequest` breaks Stage | Suite-internal roster spawn; Stage stays 1v1 |
| Empty `GoodSide` auto-defeat | Validate before Play; spawn before `StartFight` |
| Template prefabs with hidden units | Phase 1 empty-board filter; default `fighttesterempty` |
| Coordinate space guess | Locked: OffsetCoord; convert with `HexToCenter` |
| Scope creep into Adventures | Rooms, map, rewards stay out of v1 |
| Drawn-board / BG3 exploration swallowed into v1 | Phase 1b/1c is **high with Harmony**; visual editor + exploration patches are post-8 |
| Global hex-clamp / HexBoard rewrite | Do not patch `OddRToMapDataIndex`; do not replace `HexBoard` with streaming |
| Fight-flag patches in LokrPatch | Encounter prefixes stay in LokrLab, gated on embed + Encounter mode |
| Calling inspector-only Encounter "done" | Target editor section; do not close the track without the map UI if 1b said yes |
| Encounter UI growing inside Sandbox | Own project type; Sandbox only loads |

---

## Related docs

- [sandbox-workstation.md](../completed/sandbox-workstation.md) — embed + spawn; later load-Encounter consumer
- [HexBoard.html](../../api/base-game/Ironhide/Legends/Model/Game/HexBoard.html)
- [TemplateObjectData.html](../../api/base-game/TemplateObjectData.html)
- [EncounterBkgDefinition.html](../../api/base-game/EncounterBkgDefinition.html)
- [HexBoardViewComponent.html](../../api/base-game/Ironhide/Battlechest/Client/View/HexBoardViewComponent.html)
- [GameHexGridItemData.html](../../api/base-game/Ironhide/Battlechest/Common/Game/Gameboard/GameHexGridItemData.html)
- [editor-redesign.md](editor-redesign.md) §2.5, §6.3, Phase 10
- [lab-suite-merge.md](../completed/lab-suite-merge.md) — suite ownership
- [lab-save-ux.md](../completed/lab-save-ux.md)
- [lab-hover-coverage.md](../completed/lab-hover-coverage.md)
- [human-readable-ids.md](../completed/human-readable-ids.md)
- [extensions.md](../not-started/extensions.md) — Custom scripting, Custom Adventures
- [mods-folder-structure.md](../../mods-folder-structure.md)
- [templates/catalog/](../../character-reference/_extracted/base-game/templates/catalog/) — 610 names + board sizes
