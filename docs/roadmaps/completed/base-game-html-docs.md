# Base Game HTML documentation (full coverage)

**Status:** Complete — Pass A, B, and C done. All 1631 pages are `DOC-STATUS: verified`.  
**Raised:** 2026-08-15  
**Last updated:** 2026-08-15  
**Owner:** docs (`docs/api/base-game/`). No plugin code. Does not block Lab save UX, Encounter Creator, or Ability Lab overhaul.

Fill every Base Game Reference HTML page so a later agent (or a human) can
understand a class, judge how mod-friendly it is, and find bugs / hostile
error handling without re-reading the decompile from scratch. The HTML already
exists. This track is authoring, not generation.

The older curated spine is
[base-game-documentation-checklist.md](../../base-game-documentation-checklist.md)
— keep using it for the ~20 load-path classes; this roadmap covers the rest of
the tree as well.

See also [roadmaps/README.md](../README.md),
[GENERATE_CLASS_DOCS.md](../../api/GENERATE_CLASS_DOCS.md)
(how pages were built and how to edit them),
[base-game-namespaces.md](../../base-game-namespaces.md).

---

## What already exists

| Piece | What it is |
|-------|------------|
| ~1631 HTML pages under [`docs/api/base-game/`](../../api/base-game/index.html) | Boilerplate from the deleted `generate_base_game_docs.py` (2026-08-11). Real signatures, `TODO` descriptions. |
| [checklist](../../base-game-documentation-checklist.md) | **Curated** character-pipeline spine (units, roster, abilities loader, exo). A handful of pages are `done`; most of the 1631 are untouched. |
| Hub markdown | [unit-load-path.md](../../unit-load-path.md), [roster-load-path.md](../../roster-load-path.md), [ability-load-path.md](../../ability-load-path.md), [exoskeleton-pipeline.md](../../exoskeleton-pipeline.md) |
| Decompiled source | `~/dev/lokr-modding/lokr-modding/ih-original/` (read-only). Each HTML page's "Source File" line points here. |
| Mod hooks | [LokrCharacterLoader/docs/patches.md](../../../LokrCharacterLoader/docs/patches.md) |

The checklist **deprioritized** `SRDebugger`, `SRF`, `DG.Tweening`, most map UI,
and the 422 `(global)` root files. This roadmap **includes them**, at a thinner
depth (see Depth). Do not treat "deprioritized on the checklist" as "skip here."

**Never** run `generate_docs.py --sync-descriptions` (or anything named that)
against `docs/api/base-game/`. There are no `/// <summary>` comments in
`ih-original/`; sync would wipe hand-written prose back to `TODO`. Edit the
HTML in place. Prefer `member_extraction.sync_auto_doc_blocks` from a throwaway
script over hand-typed HTML entities — see GENERATE_CLASS_DOCS.md § "The Base
Game Reference Section Has No Generator".

---

## Goal

For every page:

1. Replace every `TODO` in Description, member blocks, Usage Examples, and Remarks.
2. State **mod-friendliness**: can a mod expand this (new ids, extra entries,
   Harmony prefix/postfix), or does the code assume a closed vanilla set?
3. State **error handling**: throw / silent skip / log-and-continue, and whether
   that hurts mods (e.g. `FXManager.LoadFXMega` throwing on unknown names).
4. Spot **bugs, footguns, and dead fields**. Mark them; do not silently omit.
5. Leave **grep-able markers** so cleanup and verify passes can find unfinished
   or disputed claims without reading 1631 files.

A page is not `done` for this track until Pass C (verify) has checked it
against `ih-original/`.

---

## Three passes

Work is **per batch**, not one giant sweep of the tree. A later batch may start
Pass A while an earlier batch is in Pass B or C. **Do not** start Pass B on a
batch until Pass A for that batch is complete. **Do not** start Pass C until
Pass B for that batch is complete.

| Pass | Name | Who | Job |
|------|------|-----|-----|
| A | Initial | One agent per batch | Read `ih-original/` for every file in the batch. Fill all TODOs. Add Mod surface + markers. Do not "fix" other batches. |
| B | Cleanup | A **different** agent than Pass A for that batch | Read the HTML (and source when a marker or contradiction appears). Resolve or re-mark discrepancies, cross-link neighbors, promote confirmed bugs to `docs/issues/unresolved/` when they affect our mods. |
| C | Verify | A **different** agent than A and B | Open the matching `.cs` in `ih-original/` and confirm every non-trivial claim. Demote invented APIs. Flip `DOC-STATUS` to `verified` only when the page matches the code. |

If the same agent must do two passes on one batch (no other agent available),
it still has to re-read the source in Pass C rather than trusting its own
Pass A prose.

---

## Depth

Not every class deserves the same word count. Every class still gets its TODOs
replaced.

| Depth | Use for | Required |
|-------|---------|----------|
| **full** | Model, Controller, Content, ExoSkeleton, KVLib, parsers, loaders, ability actions / functions | Every member. Mod surface. Error handling. At least one Usage Example on the class. Markers wherever needed. |
| **standard** | View, Metagame screens, Services, Utils, `(global)` game glue | Every member, shorter. Mod surface still required. Usage Example may be "constructed by the scene; mods do not new this." |
| **thin** | `SRDebugger`, `SRF`, `DG.Tweening`, `Scenes/TestScenes`, `UnityStandardAssets`, `SharpNeatLib`, generated expression listeners | What it is, whether mods should touch it, one-line members. Do not reverse-engineer DOTween/SRDebugger internals. Still no `TODO` left. |

Batch table below assigns depth. An agent must not upgrade a thin batch to
full on a whim; if a thin class is actually a mod choke point, mark
`DOC-MARK:relook` and say why.

**Already `done` on the checklist** (`UnitDefinition`, `UnityDefinitionsParser`,
`StatHelper`, `HeroRosterManager`, `AbilitiesDefinitions`,
`MapHeroBarPortraitComponent`, `ExoSkeletonData`, …): do not rewrite the
spine. Pass A only fills remaining member TODOs (if any), adds Mod surface if
missing, and adds markers. Pass C still verifies.

---

## Marking system (Pass A must use this)

### Page-level comments

Put these **immediately after** the existing auto-generated boilerplate comment
in the `<body>` (before `<header>`), so grep works even if Remarks is huge:

```html
<!-- DOC-STATUS: initial -->
<!-- DOC-DEPTH: full -->
```

| Comment | Values | When to set |
|---------|--------|-------------|
| `DOC-STATUS` | `initial` / `cleanup` / `verified` | Pass A writes `initial`. Pass B writes `cleanup`. Pass C writes `verified`. |
| `DOC-DEPTH` | `full` / `standard` / `thin` | Copy from the batch table. |

### Required Remarks block: Mod surface

Every page's Remarks (after the existing `<!-- AUTO-DOC:class-remarks -->`
prose, still inside that block or immediately after it if the AUTO-DOC wrapper
is too tight — prefer **inside** Remarks, **outside** AUTO-DOC if you need
stable hand-written HTML that a future tool will not touch) must include:

```html
<h3>Mod surface</h3>
<p><strong>Expand:</strong> open / closed / hostile — one sentence (new ids, extra list entries, subclassing, Harmony).</p>
<p><strong>Errors:</strong> throw / skip / log — one sentence on whether that hurts a modded id or file.</p>
```

`open` = adding content in the usual KV/JSON/folder way works.  
`closed` = vanilla set is assumed; a patch or resolver is required.  
`hostile` = unknown ids throw, dictionaries overwrite silently, or a hardcoded
count/index will break.

### Inline markers (grep + visible)

Every marker is **both** an HTML comment (for grep) **and** a visible
`note` / `warning` / `caution` (for readers). Never one without the other.

| Marker | Comment | Visible wrapper | Meaning |
|--------|---------|-----------------|--------|
| uncertain | `<!-- DOC-MARK:uncertain -->` | `<div class="note"><strong>Uncertain:</strong> …</div>` | Claim not fully backed by the decompile. Pass B/C must resolve or keep. |
| bug | `<!-- DOC-MARK:bug -->` | `<div class="warning"><strong>Base-game bug:</strong> …</div>` | Wrong, crashy, or silently corrupt in vanilla. Include the method name. |
| relook | `<!-- DOC-MARK:relook -->` | `<div class="note"><strong>Relook:</strong> …</div>` | Too large, too coupled, or Pass A ran out of context. Pass B starts here. |
| important | `<!-- DOC-MARK:important -->` | `<div class="note"><strong>Important:</strong> …</div>` | Load-order, singleton, thread, or "mods will hit this." |
| mod-hostile | `<!-- DOC-MARK:mod-hostile -->` | `<div class="caution"><strong>Mod-hostile:</strong> …</div>` | Throws / clobbers / hardcoded set. Overlaps Expand: hostile; use both when a **method** is the problem. |
| patch-point | `<!-- DOC-MARK:patch-point -->` | `<div class="note"><strong>Patch point:</strong> …</div>` | We already Harmony-patch this, or it is the natural hook. Link `patches.md` or the patch class HTML. |

Grep from `docs/api/base-game/`:

```bash
rg -l "TODO" --glob "*.html" | wc -l
rg -l "DOC-STATUS: initial" --glob "*.html" | wc -l
rg -l "DOC-MARK:uncertain" --glob "*.html"
rg -l "DOC-MARK:bug" --glob "*.html"
```

Pass B/C should drive off those lists, not off memory.

### Bugs vs issues

- `DOC-MARK:bug` stays on the class page forever (it is documentation of vanilla).
- If the bug **affects our mods or Lab**, Pass B also files
  [`docs/issues/unresolved/`](../../issues/README.md) and links it from the
  marker. Do not file an issue for "DOTween exists."

---

## Agent rules (every pass)

1. **One batch per agent.** The batch id from the table below is the whole
   scope. Do not wander into a neighbor folder "while you're here."
2. **Source of truth is `ih-original/`**, not the HTML, not ChatGPT memory of
   Unity, not plugin docs. Plugin docs are cross-checks for patch-points only.
3. **Do not invent members.** If `member_extraction` missed a method, add it by
   hand and say so in a `DOC-MARK:relook` (scanner gaps are listed in
   GENERATE_CLASS_DOCS.md Troubleshooting).
4. **Do not delete AUTO-DOC markers** unless you are replacing a description
   that must not be overwritten later. For base-game there is no sync, but
   keep the markers so a future non-destructive tool can still find slots.
5. **Link, don't paste.** Neighbor classes: sibling `Foo.html`. Hubs:
   `docs/unit-load-path.md` etc. (path depth varies — follow an existing
   `done` page in the same folder). Plugin patches:
   `LokrCharacterLoader/docs/patches.md`.
6. **No emojis.** Status words only (`open`, `closed`, `hostile`, `todo`).
7. **No `--sync-descriptions`.** No regenerating `base-game/index.html` unless
   a new source file appeared (it will not, mid-track).
8. **Update the batch table** in this file when a pass finishes: `[x]` the
   pass cell, date, and a one-line note (e.g. "12 uncertain, 3 bugs").
9. **Do not commit** unless the user asked. Same as the rest of this repo.

### Pass A extra

- Fill **all** TODOs in the batch, including trivial properties ("The KV key
  `Foo`, copied onto the runtime object").
- If a file is huge (`Unit.html` is 185 member TODOs), still finish the batch;
  use `DOC-MARK:relook` on clusters of similar members rather than leaving
  TODOs.
- Read [patches.md](../../../LokrCharacterLoader/docs/patches.md) once per
  batch and mark `patch-point` when a method is already hooked.

### Pass B extra

- Grep the batch for `DOC-MARK:uncertain`, `relook`, and leftover `TODO`.
- Cross-link: if A said "see AbilityParser" and that page now exists, add the
  href.
- Contradictions between two pages in the **same** batch: fix both. Across
  batches: mark `uncertain` and name the other page; do not rewrite the other
  batch.
- Confirmed mod-facing bugs: file an issue.

### Pass C extra

- For each page, open the `.cs` listed under Source File.
- Spot-check every `DOC-MARK:bug` / `mod-hostile` / `important` against the
  method body.
- If a claim is wrong, fix the HTML and leave a one-line note in the batch
  table. Do not flip `verified` on a page you did not open the source for.
- After the batch is verified: `rg TODO` on that folder must be 0 (except the
  boilerplate `fill in the TODOs below` comment, which you may delete once
  the page is verified).

---

## How to run many agents

Launch **Pass A agents in parallel** across batches that do not share files.
Good first wave (high mod value, mostly independent):

- A1, U1, E1, M1, C1, L1 (abilities core, units core, exo, metagame root,
  combat controller, localization)

Then a second wave of the remaining Model / Controller / View batches.

Pass B for a batch can start as soon as that batch's Pass A is checked off,
even if other batches are still in A.

**Suggested agent prompt** (fill in `BATCH`, `PASS`, `DEPTH`, `PATHS`):

```
You are documenting Legends of Kingdom Rush base-game HTML pages.

Read docs/roadmaps/completed/base-game-html-docs.md and follow it exactly
(marking system, depth, agent rules, no --sync-descriptions).

Batch: BATCH
Pass: PASS   (A initial / B cleanup / C verify)
Depth: DEPTH
HTML folder(s): PATHS
Decompile root: ~/dev/lokr-modding/lokr-modding/ih-original/

When finished: update the batch row in that roadmap (checkbox + date +
one-line note). Do not start another batch.
```

---

## Batches

Counts are HTML files (one per `ih-original` source file). Paths are under
`docs/api/base-game/`.

Checkboxes: Pass A / B / C. Leave `[ ]` until that pass is done for the
**whole** batch.

### Model and content (full)

| Id | A | B | C | ~N | Depth | Path |
|----|---|---|---|----:|-------|------|
| U1 | [x] 2026-08-15 | [x] 2026-08-15 | [x] 2026-08-15 | 32 | full | `Ironhide/Legends/Model/Game/Units/` (not subfolders). Pass A: 0 TODO; 17 bug (dead combat events, `Path.Step.ToString` FormatException, `Unit.Heal` null source) / 5 mod-hostile / 6 relook. Pass B: 0 uncertain / 0 relook; filed `stats-apply-modifier-missing-stat-throws.md`; Heal demoted as issue. Pass C: 32 verified; `Path.ToString` also throws; `BaseActivityInterface` does subclass `ActivityInterface`; no new issues. |
| U2 | [x] 2026-08-15 | [x] 2026-08-15 | [x] 2026-08-15 | 19 | full | `Units/Activities/`, `ActivityInterfaces/`, `Projectiles/`, `QueuedAbilities/`. Pass A: 0 TODO; 12 bug (`DummyActivity` potion `UseItem` throw, `POINT_TARGET` NRE, empty-path `IndexOutOfRangeException`; `Projectile.Update` NRE already patched) / 7 mod-hostile / 5 relook / 2 uncertain. Pass B: 1 uncertain (`BaseActivityInterface` vs U1 subclass claim) / 0 relook; filed `activity-interface-point-target-nre.md`; DummyActivity and empty-path demoted as issues. Pass C: 19 verified; `BaseActivityInterface` does subclass `ActivityInterface` (uncertain dropped); 5 claims corrected; no new issues. |
| A1 | [x] 2026-08-15 | [x] 2026-08-15 | [x] 2026-08-15 | 19 | full | `Units/Abilities/` core / registries. Pass A: 0 TODO; 6 bug (inverted `IsOnCooldown`, AOE null-key NRE, `RetreatIfWeekAI` typo, `999f` fallbacks) / 10 mod-hostile / 10 relook. Pass B: 0 uncertain / 0 relook; filed `ability-aoe-missing-center-keys-nre.md`, `ability-ai-retreat-if-week-typo.md`, `ability-events-never-dispatched.md`, `ability-tooltip-missing-var-returns-999.md`; `IsOnCooldown` and Lua `CreateModifier` demoted as issues. Pass C: 19 verified; corrected `GetAbility` / `CheckLoop` / `AbilityAPCost`; no new issues. |
| A2 | [x] 2026-08-15 | [x] 2026-08-15 | [x] 2026-08-15 | 35 | full | `Units/Abilities/` action types whose names start with A–E (not `Ability*`, not `Function*`). Pass A: 0 TODO; 6 bug (`RANGE_CONE` empty, inverted `ActionsIfEmpty`, unused `Index`/`Unit`, `ConstantObjectExpression` NRE) / 7 mod-hostile / 3 relook / 2 uncertain. Pass B: 0 uncertain / 0 relook; linked `ability-aoe-range-cone-empty.md`; filed `ability-each-in-list-actions-if-empty-inverted.md`. Pass C: 35 verified; 6 claims corrected (`#word` not quotes; MAGICAL has no resist path; cone has no `ActOnHexas` case); no new issues. |
| A3 | [x] 2026-08-15 | [x] 2026-08-15 | [x] 2026-08-15 | 48 | full | `Units/Abilities/Function*` first half (A–M of the Function suffix). Pass A: 0 TODO; 78 bug (`hex*` DebugAsFloat FormatException, `equal` null NRE, `lerp` divide-by-zero) / 103 mod-hostile / 0 relook. Pass B: 0 uncertain / 0 relook; filed `ability-equal-null-lhs-nre.md`; hex DebugAsFloat and lerp demoted as issues. Pass C: 48 verified; 9 claims corrected (`abilityCooldown` is defined length; `TEAM_ALLY` invented); no new issues. |
| A4 | [x] 2026-08-15 | [x] 2026-08-15 | [x] 2026-08-15 | 35 | full | remaining `Function*`. Pass A: 0 TODO; 31 bug (KV `pointMagnitude` constructs `FunctionPointMult`, so `FunctionPointMagnitude` is unreachable) / 95 mod-hostile / 2 relook. Pass B: 0 uncertain / 0 relook; filed `ability-kv-pointmagnitude-constructs-pointmult.md`. Pass C: 35 verified; 4 claims corrected (`pointsInCircunferenceP` is XY plus Y-scale); no new issues. |
| A5 | [x] 2026-08-15 | [x] 2026-08-15 | [x] 2026-08-15 | 56 | full | `Units/Abilities/` G–Z actions and anything A2–A4 skipped. Pass A: 0 TODO; 14 bug (`RANGE_CONE` matches nobody, closed Hit tag whitelist, `ObjectValueData` boxed numbers as 0) / 4 mod-hostile / 5 relook / 2 uncertain. Pass B: 0 uncertain / 0 relook; filed `ability-hit-closed-tag-whitelist.md`; linked RANGE_CONE and SpawnUnit `$alias` issues. Pass C: 56 verified; 3 claims corrected (`UseSkill` indexes `unit.skills`; `ReadOnly` does not throw; invented `EvaluateFunction` dropped); no new issues. |
| G1 | [x] 2026-08-15 | [x] 2026-08-15 | [x] 2026-08-15 | 16 | full | `Ironhide/Legends/Model/Game/` (not Units). Pass A: 0 TODO; 8 bug (`IsCloserToUnitThan` throw, empty-board NRE, `GetHexItem` clamps off-map) / 3 mod-hostile / 5 relook. Pass B: 0 uncertain / 0 relook; filed `fight-started-empty-initiative-nre.md`; `IsCloserToUnitThan` and `GetHexItem` demoted as issues. Pass C: 16 verified; 6 claims corrected (`Stage.Update` NRE is EncounterManager/unitController; Lab replaces `Update`); no new issues. |
| M1 | [x] 2026-08-15 | [x] 2026-08-15 | [x] 2026-08-15 | 53 | full | `Metagame/` root only. Pass A: 0 TODO; 13 bug / 11 mod-hostile (`Sanitize` drops unknown hero ids; party not size 3 resets to Gerald/Ranger/ArcaneMage) / 3 relook. Pass B: 0 uncertain / 0 relook; filed `save-sanitize-drops-unknown-ids.md` and `save-party-reset-to-vanilla-trio.md`. Pass C: 53 verified; `LevelToRank` uses bracket max; CinematicUtils empty-path throws; no new issues. |
| M2 | [x] 2026-08-15 | [x] 2026-08-15 | [x] 2026-08-15 | 37 | full | `Metagame/Achievements/`, `Adventures/`, `Arena/`, `Encounters/`. Pass A: 0 TODO; 16 bug (`LoadRun` NRE, `MarkComplete` reports unchanged, empty-party `StartingHeroes[0]`) / 19 mod-hostile / 15 relook. Pass B: 0 uncertain / 0 relook; no issues filed (`LoadRun` / `MarkComplete` / `StartingHeroes[0]` not current Lab paths); `StartedCombat` after `ClearArena` promoted to bug. Pass C: 37 verified; corrected swapped `LeaderboardConfig` members, `GetCurrent` sum, `IsDailyChallenge` also via `SetupArena(true)`; no new issues. |
| M3 | [x] 2026-08-15 | [x] 2026-08-15 | [x] 2026-08-15 | 42 | full | `Metagame/Heroes/`, `Inventory/`, `Morgue/`, `Results/`, `Scripts/`, `Cheats/`, `Common/`, `Game/`. Pass A: 0 TODO; 24 bug (`UpdateSkills` unknown-id NRE, `AddItem` never sets id, `gainXP` grants guild XP) / 11 mod-hostile / 5 relook / 1 uncertain. Pass B: 0 uncertain / 0 relook; filed `hero-update-skills-unknown-id-nre.md` and `inventory-additem-never-sets-id.md`; `gainXP` demoted as issue. Pass C: 42 verified; 6 claims corrected (`UpdateSkills` fill uses `IndexOf`; Clone throws on null context); no new issues. |
| M4 | [x] 2026-08-15 | [x] 2026-08-15 | [x] 2026-08-15 | 45 | full | `Metagame/Dialogs/`, `Map/` (including Loot, Quests, Assigners, Logic). Pass A: 0 TODO; 28 bug (`LootItemGeneratorAnyOf` always fires, `GetQuestStatus` Not implemented, dialog `First()` no fallback) / 13 mod-hostile / 20 relook / 1 uncertain. Pass B: 1 uncertain (`RoadAssignerHelper` View/Map shapes) / 0 relook; filed `loot-anyof-chance-always-fires.md` and `dialog-first-no-fallback.md`. Pass C: 45 verified; `RoadAssignerHelper` shapes confirmed (uncertain dropped); 4 claims corrected; no new issues. |
| C1 | [x] 2026-08-15 | [x] 2026-08-15 | [x] 2026-08-15 | 16 | full | `Controller/Game/` + `Activities/` + `Units/` (not AI). Pass A: 0 TODO; 8 bug (`RANGE_CONE` empty, `UseSkill("Move")` throws) / 7 mod-hostile / 3 relook. Pass B: 0 uncertain / 1 relook; filed `ability-aoe-range-cone-empty.md`; `UseSkill("Move")` demoted as issue. Pass C: 16 verified; `OnFingerTap` relook resolved; 6 claims corrected; no new issues. |
| C2 | [x] 2026-08-15 | [x] 2026-08-15 | [x] 2026-08-15 | 33 | full | `Controller/Game/AI/`. Pass A: 0 TODO; 12 bug (`RetreatIfWeekAI` typo, logistic `2718f`, empty-brain divide-by-zero, `PerAffectedAI` not an AbilityAction) / 19 mod-hostile / 10 relook. Pass B: 0 uncertain / 0 relook; filed `ability-ai-per-affected-not-action.md` and `ability-ai-empty-brain-divide-by-zero.md`; linked `ability-ai-retreat-if-week-typo.md`; logistic `2718f` demoted as issue. Pass C: 33 verified; 5 claims corrected (`DistanceToCamera` is world XY; `KeepDistanceAI2` still adds zeroed candidates); no new issues. |
| E1 | [x] 2026-08-15 | [x] 2026-08-15 | [x] 2026-08-15 | 30 | full | `Ironhide/ExoSkeleton/` + `ExoSkeleton/Code/ExoSkeletonUIGraphic.html`. Pass A: 0 TODO; 10 bug / 5 mod-hostile / 8 relook / 2 uncertain. Pass B: 0 uncertain / 0 relook; filed `reload-data-missing-sprite-nre.md`; linked `find-part-index-unvalidated.md`. Pass C: 30 verified; invisibility writes `renderer.color`; `loopsByDefault` unread; no new issues. |
| K1 | [x] 2026-08-15 | [x] 2026-08-15 | [x] 2026-08-15 | 18 | full | `Ironhide/Legends/Content/Abilities/` + `Content/Visuals/`. Pass A: 0 TODO; 31 bug (empty-filter throws, Bruxa attach off-by-one) / 29 mod-hostile / 5 relook / 1 uncertain. Pass B: 0 uncertain / 0 relook; filed `ability-callfunction-empty-filter-throws.md`; Bruxa attach demoted as issue. Pass C: 18 verified; Magic teleport does not throw; Physical / Summoner teleport / Iriza added to existing empty-filter issue; no new issues. |
| P1 | [x] 2026-08-15 | [x] 2026-08-15 | [x] 2026-08-15 | 6 | full | `KVLib/`. Pass A: 0 TODO; 9 bug (`PenguinParser` off-by-one/EOF throws, `KeyValue` indexer/`ToString`/`AddChildren` parent bugs, dead `AderumParser`) / 3 mod-hostile / 2 relook. Pass B: 0 uncertain / 0 relook; no new issues; linked `ability-kv-parse-empty-filename.md`. Pass C: 6 verified; `GetInt` is not thousands-aware; empty `Parse` NullRefs before the null return; no new issues. |

### View, services, utils (standard)

| Id | A | B | C | ~N | Depth | Path |
|----|---|---|---|----:|-------|------|
| V1 | [x] 2026-08-15 | [x] 2026-08-15 | [x] 2026-08-15 | 41 | standard | `View/Game/` + `Units/` + `Projectiles/` + `Order/` + `Scene/` + `Level/`. Pass A: 0 TODO; 20 bug (animation-event no-ops, ray divide-by-zero, lightning Destroy-then-DestinationReached) / 11 mod-hostile / 6 relook / 2 uncertain. Pass B: 0 uncertain / 0 relook; no new issues; linked `exo-skeleton-null-unitdefinition.md`. Pass C: 41 verified; 12 claims corrected (health-bar animate is DOScale; `lastEventChecked` skips on replay); no new issues. |
| V2 | [x] 2026-08-15 | [x] 2026-08-15 | [x] 2026-08-15 | 41 | standard | `View/Game/Dialogs/`, `FX/`, `Tooltips/`, `Adventure/`, `CinematicIds/`, `Utils/`. Pass A: 0 TODO; 18 bug / 8 mod-hostile (`FXManager.LoadFXMega` throw, unknown cinematicId NRE) / 8 relook. Pass B: 0 uncertain / 0 relook; no issues filed (`LoadFXMega` already patched; cinematicId NREs not Lab-facing). Pass C: 41 verified; 4 claims corrected (exo attach points are live pose; `capturedId` is CinematicHelper scratch); no new issues. |
| V3 | [x] 2026-08-15 | [x] 2026-08-15 | [x] 2026-08-15 | 46 | standard | `View/Map/` root + `DebugStuff/` + `Visuals/`. Pass A: 0 TODO; 40 bug (inverted skip-fight scenes, auto-reveal ignores distance, discarded assigner type) / 15 mod-hostile / 9 relook. Pass B: 0 uncertain / 0 relook; filed `map-start-unknown-starting-hero-nre.md` and `map-hud-unknown-modifier-config-nre.md`. Pass C: 46 verified; 6 claims corrected (LEAVE has its own cinematic path; both-null connection NRE); no new issues. |
| V4 | [x] 2026-08-15 | [x] 2026-08-15 | [x] 2026-08-15 | 29 | standard | `View/Map/Screens/` (Store, Inventory, Rewards, Darkness, TeamManage). Pass A: 0 TODO; 34 bug (`map_item_store` duplicate id, `DarknessBar.OpenPanel` hides, `SetTargetPortrait(Sprite)` unused) / 10 mod-hostile / 5 relook. Pass B: 0 uncertain / 0 relook; no new issues; linked `portrait-patches-buff-store-index.md` and `save-party-reset-to-vanilla-trio.md`. Pass C: 29 verified; 6 claims corrected (`columns==0` is Infinity not NRE; `HideUITeamManage` double `OnHideWindow` demoted as Lab issue); no new issues. |
| V5 | [x] 2026-08-15 | [x] 2026-08-15 | [x] 2026-08-15 | 37 | standard | `View/Metagame/Screens/`. Pass A: 0 TODO; 13 bug (`ShowHelp` loads progression help, QPA Next repeats page 1) / 13 mod-hostile (vanilla 3-slot party, `skillProgression[1,2,3]`) / 4 relook. Pass B: 0 uncertain / 0 relook; no new issues; linked `save-party-reset-to-vanilla-trio.md` and `save-sanitize-drops-unknown-ids.md`. Pass C: 37 verified; 5 claims corrected (empty-pages Next throws; invented `HeroRosterManager.instance` demoted); no new issues. |
| V6 | [x] 2026-08-15 | [x] 2026-08-15 | [x] 2026-08-15 | 55 | standard | `View/Hud/` + remaining `View/` (Achievements, Paralax, TV, AdventureMetagame). Pass A: 0 TODO; 37 bug (`ScoreHudComponent` always Destroy, float RNG `(min-max)`, `HeroProgressWindow` unknown uniqueId NRE) / 13 mod-hostile / 10 relook / 3 uncertain. Pass B: 0 uncertain / 0 relook; filed `hero-progress-window-unknown-uniqueid-nre.md`; ScoreHud and float RNG demoted as issues. Pass C: 55 verified; 6 claims corrected (invented `GetSpriteByName` overload dropped); no new issues. |
| S1 | [x] 2026-08-15 | [x] 2026-08-15 | [x] 2026-08-15 | 44 | standard | `Services/Platform/`. Pass A: 0 TODO; 14 bug (etag/tag mixup, null leaderboard list, extra GameObject) / 5 mod-hostile (cloud hardcoded to 3 slots) / 3 relook. Pass B: 0 uncertain / 0 relook; no new issues; cloud `MAX_SLOTS=3` linked to `save-party-reset-to-vanilla-trio.md`. Pass C: 44 verified; Steam-down does not hang 30s (`SERVICE_UNAVAILABLE` next frame); `GetMaxSlots` returns literal 3; no new issues. |
| S2 | [x] 2026-08-15 | [x] 2026-08-15 | [x] 2026-08-15 | 35 | standard | other `Services/` + `Ironhide/Legends/Utils/`. Pass A: 0 TODO; 25 bug (`Storage` cache/disk desync, `Util.IsWithinDistance` infinite recursion, `UserReportGenerator` compress hang) / 10 mod-hostile / 7 relook / 2 uncertain. Pass B: 0 uncertain / 0 relook; no new issues; Storage remarks link M1 save issues. Pass C: 35 verified; `GameObjectExtensions` descriptions realigned to the right signatures; invented Analytics NRE dropped; no new issues. |
| L1 | [x] 2026-08-15 | [x] 2026-08-15 | [x] 2026-08-15 | 24 | standard | `Ironhide/Localization/` + `AssetBundles/` + `IronJSON/` + `Ironhide/Utils/`. Pass A: 0 TODO; 15 bug / 7 mod-hostile / 7 relook. `LocalizationManager.Load` marked as patch-point. Pass B: 1 uncertain (`JSONNode.DeepChildren`) / 0 relook; no issues filed; `forcedLanguageCode` → EN claim corrected. Pass C: 24 verified; `DeepChildren` is an empty walk as decompiled (uncertain dropped); `AbilitiesIFormatter` unknown vars format as 999 (already filed); no new issues. |
| B1 | [x] 2026-08-15 | [x] 2026-08-15 | [x] 2026-08-15 | 29 | standard | `Ironhide/Battlechest/`. Pass A: 0 TODO; 28 bug (`OddRToMapDataIndex` clamp, `PathFinder` never returns null, `ActionList.Update` never advances) / 6 mod-hostile / 9 relook. Pass B: 0 uncertain / 0 relook; no new issues; clamp / PathFinder / ActionList demoted as Lab issues. Pass C: 29 verified; 6 claims corrected (`DMinHeap.ChangeKey` pull/push was swapped; `LONG_WALK_*` tubes never selected); no new issues. |
| R1 | [x] 2026-08-15 | [x] 2026-08-15 | [x] 2026-08-15 | 46 | standard | leftover `Ironhide/Legends/` (Builds, Tutorial, Touch, Performance, Spritesheets, InputOverride, IngameDebug, HexaGrid, Scenes, SRDebuggerOptions, GenericDebug, root `Ironhide/Legends/` files). Pass A: 0 TODO; 28 bug (`SceneDB.DEFEAT` maps to victory scene, `BuildFlavors` NRE, `PerformanceWatcher` never writes profile) / 8 mod-hostile / 15 relook. Pass B: 0 uncertain / 1 relook (`SpritesheetManager` empty decompile); no issues filed; `SceneDB.DEFEAT` linked to V3 skip-fight invert. Pass C: 46 verified; `SpritesheetManager` empty body confirmed (relook dropped); 5 claims corrected; no new issues. |

### Global Assembly-CSharp glue (standard)

421 files at `docs/api/base-game/*.html` (no subfolder). Split by name:

| Id | A | B | C | ~N | Depth | Path |
|----|---|---|---|----:|-------|------|
| X1 | [x] 2026-08-15 | [x] 2026-08-15 | [x] 2026-08-15 | 40 | standard | `UI*.html`. Pass A: 0 TODO; 32 bug (`OnCancel` FindObjectOfType NRE, inverted `DelaySelect`, stacked demo-dialog listeners) / 12 mod-hostile / 8 relook / 1 uncertain. Pass B: 0 uncertain / 0 relook; no new issues; linked encyclopedia, party-reset, sandbox forfeit, and unknown-skill issues. Pass C: 40 verified; 6 claims corrected (`Init` early-out; language reload is not `SceneDB`; leave vs forfeit); no new issues. |
| X2 | [x] 2026-08-15 | [x] 2026-08-15 | [x] 2026-08-15 | 26 | standard | `Debug*.html`, `SR*.html`. Pass A: 0 TODO; 18 bug (`DebugPanel` vs `~DebugPanel` Find mismatch, `SRMath.Wrap(0)` hang, shipping builds hide debug button) / 4 mod-hostile / 5 relook / 2 uncertain. Pass B: 0 uncertain / 0 relook; no issues filed (Lab uses the working `GetDebugPanel` path; no plugin calls `SRMath.Wrap`). Pass C: 26 verified; 4 claims corrected (`WithA(0)` is the int overload; `GetService` returns null); no new issues. |
| X3 | [x] 2026-08-15 | [x] 2026-08-15 | [x] 2026-08-15 | 34 | standard | `Skill*.html`, `Portrait*.html`, `Unit*.html`, `Hero*.html`. Pass A: 0 TODO; 44 bug (`SkillsBar` five-slot cap, `UnitDetailWindow.Init` leaks 10 icons per open, `PortraitSkill.OnSelected` NRE) / 23 mod-hostile / 10 relook / 1 uncertain. Pass B: 0 uncertain / 0 relook; filed `skills-bar-five-slot-cap.md`; UnitDetailWindow leak and PortraitSkill NRE demoted as issues. Pass C: 34 verified; 5 claims corrected (`FirstSelected` is fight-nav; `showTooltipTitle` does not call `ShowTitle`); no new issues. |
| X4 | [x] 2026-08-15 | [x] 2026-08-15 | [x] 2026-08-15 | 37 | standard | `Encounter*.html`, `Level*.html`, `Map*.html`, `Range*.html`, `Cone*.html`, `Tunnel*.html`. Pass A: 0 TODO; 27 bug (inverted `InitializeMetaGameUnit` cinematicId, cone `materials[(int)kind]` unguarded) / 7 mod-hostile / 8 relook. Pass B: 0 uncertain / 0 relook; no new issues; linked RANGE_CONE, portrait-patches, and party-reset. Pass C: 37 verified; 5 claims corrected (`Random.Next(0,0)` returns 0; float `360/0` is Infinity); no new issues. |
| X5 | [x] 2026-08-15 | [x] 2026-08-15 | [x] 2026-08-15 | 60 | standard | Camera, Confirm, Dice, FX, HUD, Initiative, Localization, Node, Spline, Textured, Tooltip, tk2d prefixes. Pass A: 0 TODO; 61 bug (`CameraBase` empty-list divide-by-zero, `TooltipManager` hardcoded MapTooltip, tk2d layer off-by-one) / 17 mod-hostile / 20 relook / 2 uncertain. Pass B: 0 uncertain / 0 relook; no new issues; CameraBase / TooltipManager / tk2d demoted as Lab issues. Pass C: 60 verified; 4 claims corrected (`SetupTooltip` never wires `tooltipContent`; `KeepTargetsOnCamera` delay unused); no new issues. |
| X6 | [x] 2026-08-15 | [x] 2026-08-15 | [x] 2026-08-15 | 102 | standard | remaining root files A–I. Pass A: 0 TODO; 95 bug (`DataHelper.LoadAllFXMegaList` NRE, `DestroyGOOnEndClip` never destroys, `BasicTouchBehaviour` overwrites Find) / 23 mod-hostile / 15 relook / 1 uncertain. Pass B: 0 uncertain / 1 relook (`AchievementDebugPanel` coroutine body missing); no new issues; LoadAllFXMegaList / DestroyGOOnEndClip / BasicTouch demoted as Lab issues. Pass C: 102 verified (46 A–C + 56 D–I); `AchievementDebugPanel` coroutine body confirmed missing (relook dropped); 16 claims corrected; no new issues. |
| X7 | [x] 2026-08-15 | [x] 2026-08-15 | [x] 2026-08-15 | 56 | standard | remaining root files J–R. Pass A: 0 TODO; 58 bug (`ShowAlltext` inverted, lifebar `Refresh` ignores animate) / 17 mod-hostile / 1 relook. Pass B: 0 uncertain / 0 relook; no issues filed; `ShowAlltext` demoted to important; lifebar `Refresh` is test-wrapper only. Pass C: 56 verified; 6 claims corrected (`GetDefinition` fallback; `ArrayExtensions.Random` returns default; PowerBars postfix is `Init`); no new issues. |
| X8 | [x] 2026-08-15 | [x] 2026-08-15 | [x] 2026-08-15 | 66 | standard | remaining root files S–Z. Pass A: 0 TODO; 23 bug (`ShowUnitBehind` missing callback, `VictoryWindow` divide-by-zero / unknown-adventure NRE) / 7 mod-hostile / 3 relook / 1 uncertain. Pass B: 0 uncertain / 0 relook; no new issues; uniqueId NRE linked to `hero-progress-window-unknown-uniqueid-nre.md`; `SplashVideoController` patch-point confirmed. Pass C: 66 verified; 9 claims corrected (wasteland credits are any win; `ShowAlltext` is a no-op after Init); no new issues. |

If a root file is clearly a test-scene leftover, still document it (thin
one-liners are fine) and mark `DOC-MARK:relook` only if the decompile is
empty or broken.

### Third-party and test (thin)

| Id | A | B | C | ~N | Depth | Path |
|----|---|---|---|----:|-------|------|
| T1 | [x] 2026-08-15 | [x] 2026-08-15 | [x] 2026-08-15 | 72 | thin | `SRDebugger/` except `Services/`. Pass A: 0 TODO; 13 bug (`ProfilerFPSLabel` / `AverageFrameTime` divide-by-zero, `CircularBuffer.ToArray` empty, hardcoded version/opacity) / 2 mod-hostile / 5 relook / 1 uncertain. Pass B: 0 uncertain / 0 relook; no issues filed (vanilla debug; Lab/mods do not call these). Pass C: 72 verified (46 UI + 26 rest); `GetService` returns null; no new issues. |
| T2 | [x] 2026-08-15 | [x] 2026-08-15 | [x] 2026-08-15 | 32 | thin | `SRDebugger/Services/` + `Implementation/`. Pass A: 0 TODO; 6 bug (`Update` NRE, dead Timeout, `OpenTab` NRE, hardcoded IL2CPP=No) / 6 mod-hostile / 2 relook. Pass B: 0 uncertain / 0 relook; no issues filed; `KeyboardShortcutListenerService.Update` patch-point confirmed. Pass C: 32 verified; 3 claims corrected (`PinOption` pins every matching name); no new issues. |
| T3 | [x] 2026-08-15 | [x] 2026-08-15 | [x] 2026-08-15 | 53 | thin | `SRF/`. Pass A: 0 TODO; 19 bug (`AutoCreate` still throws, `LoadingCount` leak, `PeekChar` OverflowException) / 7 mod-hostile / 10 relook / 1 uncertain. Pass B: 0 uncertain / 3 relook (Json Parser/Serializer, VirtualVerticalLayoutGroup, FlowLayoutGroup); no issues filed. Pass C: 53 verified; 3 relooks dropped (public surface matches); 4 claims corrected; no new issues. |
| T4 | [x] 2026-08-15 | [x] 2026-08-15 | [x] 2026-08-15 | 53 | thin | `DG/Tweening/`, `SimpleJSON/`, `SpaceRush/`, `Tilemaps/`, `Lean/`, `Tiled2Unity/`, `UnityStandardAssets/`, `SharpNeatLib/`, `Utils/`, `DefaultNamespace/`, `AssemblyCSharp/`, `XXXXXXXXAssemblyCSharp/`. Pass A: 0 TODO; 17 bug (`JSONData.ToString` returns `NOT`, `AnimationLoader` missing first frame, `HexaTile` orthogonal refresh) / 5 mod-hostile / 15 relook / 2 uncertain. Pass B: 1 uncertain (`JSONNode.DeepChildren`, same ILSpy pattern as L1) / 3 relook (`SpriteCache`, `AnimationLoader`, `AbilitiesLoaderComponent`); no issues filed. Pass C: 53 verified; `DeepChildren` empty walk (uncertain dropped); 3 relooks dropped; 6 claims corrected; no new issues. |
| T5 | [x] 2026-08-15 | [x] 2026-08-15 | [x] 2026-08-15 | 25 | thin | `Scenes/`, `_LevelEditorStuff/`. Pass A: 0 TODO; 12 bug (`GetAssetsWithComponent` always null, `FightTestAddModifier` indexer throw) / 1 mod-hostile / 1 relook. Pass B: 1 uncertain (`KnockBack` vs HexBoard clamp) / 1 relook (empty `FightTesterCinematicPosition`); no issues filed. Pass C: 25 verified; KnockBack clamp confirmed (uncertain dropped); `FightTesterCinematicPosition` is empty (relook dropped); no new issues. |
| Z1 | [x] 2026-08-15 | [x] 2026-08-15 | [x] 2026-08-15 | 8 | mixed | Pages missed by folder splits: `Properties/AssemblyInfo.html`, `Ironhide/SpritesheetTools/SpritesheetConfig.html`, `Model/Common/Parsers/` (4), `Content/Extras/KrumAudience/` (2). Pass A: 0 TODO; 5 bug (`pointMagnitude` -> `FunctionPointMult` on `BaseLogicParser`, `ParseDebugContext.Exit` empty pop) / 3 mod-hostile / 2 relook. Pass B: 0 uncertain / 0 relook; no new issues; linked `ability-kv-pointmagnitude-constructs-pointmult.md`. Pass C: 8 verified; 3 claims tightened (`MergeWith` throws on collision); no new issues. |

---

## Progress (track-level)

| Milestone | When |
|-----------|------|
| Pass A complete (all batches) | 2026-08-15 — 1631 pages have `DOC-STATUS: initial`; content TODOs are gone (boilerplate comments only) |
| Pass B wave 1 complete | 2026-08-15 — A1/U1/E1/M1/C1/L1 at `DOC-STATUS: cleanup`. Wave leftover: 1 uncertain (`JSONNode.DeepChildren`), 1 relook (`OnFingerTap`). |
| Pass B wave 2 complete | 2026-08-15 — remaining Model/View at `DOC-STATUS: cleanup`. Wave leftover: 2 uncertain (`BaseActivityInterface` vs U1, `RoadAssignerHelper` View/Map shapes). |
| Pass B wave 3 complete | 2026-08-15 — S1/S2/B1/R1 at `DOC-STATUS: cleanup`. Wave leftover: 1 relook (`SpritesheetManager` empty decompile). |
| Pass B wave 4 complete | 2026-08-15 — X1–X8 at `DOC-STATUS: cleanup`. Wave leftover: 1 relook (`AchievementDebugPanel` coroutine body missing). |
| Pass B wave 5 complete | 2026-08-15 — T1–T5/Z1 at `DOC-STATUS: cleanup`. |
| Pass B complete | 2026-08-15 — 1631 pages `DOC-STATUS: cleanup`; 0 `initial` left. Leftover: 5 uncertain, 10 relook (listed in marker table). |
| Pass C wave 1 complete | 2026-08-15 — A1/U1/E1/M1/C1/L1 at `DOC-STATUS: verified` (174 pages). Wave leftover: 4 uncertain (`SimpleJSON.JSONNode.DeepChildren`, `BaseActivityInterface`, `RoadAssignerHelper`, `FXTestbedKnockBackAction`), 9 relook (OnFingerTap and IronJSON DeepChildren closed). |
| Pass C wave 2 complete | 2026-08-15 — U2/A2–A5/G1/M2–M4/C2/K1/P1/V1–V6 at `DOC-STATUS: verified` (639 pages; 813 total). Wave leftover: 2 uncertain (`SimpleJSON.JSONNode.DeepChildren`, `FXTestbedKnockBackAction`), 9 relook (`BaseActivityInterface` and `RoadAssignerHelper` closed). |
| Pass C wave 3 complete | 2026-08-15 — S1/S2/B1/R1 at `DOC-STATUS: verified` (154 pages; 967 total). Wave leftover: 2 uncertain (unchanged), 8 relook (`SpritesheetManager` closed). |
| Pass C wave 4 complete | 2026-08-15 — X1–X8 at `DOC-STATUS: verified` (421 pages; 1388 total). Wave leftover: 2 uncertain (unchanged), 7 relook (`AchievementDebugPanel` closed). |
| Pass C wave 5 complete | 2026-08-15 — T1–T5/Z1 at `DOC-STATUS: verified` (243 pages; 1631 total). Leftover uncertain/relook closed (`SimpleJSON.DeepChildren`, KnockBack clamp, SRF/SpaceRush relooks). |
| Pass C complete | 2026-08-15 — every page `DOC-STATUS: verified`; this roadmap moved to `roadmaps/completed/` |

Open marker counts (update at the end of each pass wave):

| Marker | Count | As of |
|--------|------:|-------|
| `DOC-MARK:uncertain` | 0 | 2026-08-15 Pass C complete |
| `DOC-MARK:bug` | 1041 | 2026-08-15 Pass C complete |
| `DOC-MARK:relook` | 0 | 2026-08-15 Pass C complete |
| `DOC-MARK:mod-hostile` | 591 | 2026-08-15 Pass C complete |

---

## Out of scope

- Rewriting plugin API HTML (`docs/api/classes/`). That already tracks
  `/// <summary>` via `generate_docs.py`.
- Re-decompiling the game or editing `ih-original/`.
- Implementing Harmony patches for every `mod-hostile` finding. Mark and
  (if it hurts us) file an issue; do not start a patch from this track.
- Regenerating `base-game/index.html` / `manifest.json`.
- Character File Reference (`docs/api/character-reference/`) — KV formats,
  already a separate site section.

---

## Relationship to the old checklist

[base-game-documentation-checklist.md](../../base-game-documentation-checklist.md)
stays as the **high-value spine** (load paths + the ~20 classes Character Lab
already needed). This roadmap is the **full tree**.

When a checklist class is in a batch:

- Honor its `done` / `spine` status.
- Finish member TODOs as part of that batch's Pass A.
- Do not delete hub markdown or `apply_spine_docs.py`. Those tools remain
  valid for the spine; they are not the workflow for the other ~1600 pages.

Tier 3 on the checklist (`AbilityParser`, expression functions, AI) is
batches A1, A3–A4, C2 here. Tier 4 (combat spawn) is C1 + parts of V1 / X4.
Do those at **full** depth even though the checklist called spawn `deferred`
— this track is documentation, not Sandbox implementation.
