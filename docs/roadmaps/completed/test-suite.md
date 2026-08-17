# Automated test suite (xUnit)

**Status:** Complete (Layer 1)
**Raised:** 2026-08-15
**Last updated:** 2026-08-15
**Owner:** solution-wide (`LokrModding.Tests`); issues process in
[`docs/issues/README.md`](../../issues/README.md)

Stand up a `dotnet test` suite for logic that does not need the Steam
player, then walk each plugin and actually use it. In-game confirm
remains the only path to `resolved/`. A passing unit test moves an
issue to `unresolved-tested/` — tests say the rule holds; the running
game can still fail.

**Shipped 2026-08-15:** `LokrModding.Tests` (`net10.0`, xUnit), Unity-free
helpers (`PatchRules`, `ContentRules`, `ModPathLookup`, `LabCatalogRules`,
`AnimatorFeelRules`, `AbilityPickerRules`, `AbilityLuaRules`,
`AbilityHoverCopy`), 97 passing tests, 29 issues in `unresolved-tested/`
(28 unit-testable plus the merge-by-id half of
`legacy-pack-and-lab-import-both-roster`). Five Unity-only issues stay
in `unresolved/`. After in-game confirms, `inventory-additem-never-sets-id`
and `progression-help-popup-index-oor` moved to `resolved/`. LokrPatch
**1.0.10** is current (Layer 1 originally noted 1.0.6).

This is **not** a Unity Editor project, an AssetRipper playthrough, or
a BepInEx in-player harness. Those stay out of scope here (see
[Deferred](#deferred)).

See also [roadmaps/README.md](../README.md).

---

## Why this track exists

Most remaining open issues are Harmony skip/clamp/map/parse rules.
Those can regress without anyone launching Steam. Recent in-game
confirms (`$alias` expand, loc stems, Assassin KV) were exactly this
class. Lab chrome, FadeScreen, audio, and portrait GameObjects are
not this class — they stay human.

Do not wait for a “full” player suite. Ship Layer 1 (xUnit + extracted
helpers) and use it plugin by plugin.

---

## Constraints

- Plugins are `netstandard2.0` and reference player `UnityEngine` /
  `Ironhide.Legends` from `GameDir`. The test project must **not**
  inherit `DeployToBepInEx` and must not copy Unity DLLs into output.
- Tests compile Unity-free helpers as **linked source** (`Compile Include`
  of `PatchRules.cs`, `ContentRules.cs`, `LabAliases.cs`, …) instead of
  ProjectReferencing plugin DLLs. Loading player `UnityEngine.dll` into
  `net10.0` testhost is unreliable on this host; the roadmap said to
  extract helpers rather than mock Unity. `InternalsVisibleTo` is still
  on LokrModAPI, LokrCharacterLoader, LokrPatch, and LokrLab for a later
  in-process pass.
- `Directory.Build.props` sets `IsTestProject` when
  `$(MSBuildProjectName.EndsWith('.Tests'))` and skips Unity/BepInEx
  refs. `DeployToBepInEx` is gated the same way.
- CI without a Steam install is a later nice-to-have. Plugin builds still
  need `GameDir`; the test project does not.
- Moving an issue to `resolved/` still requires a running-game confirm
  ([`docs/issues/README.md`](../../issues/README.md)). A green test is
  not that confirm.

---

## Issue folder: `unresolved-tested/`

```
docs/issues/
  unresolved/          open; no passing unit test for the rule (or none possible)
  unresolved-tested/   unit test exists and passes; in-game confirm still required
  resolved/            confirmed in the running game
```

**Move to `unresolved-tested/` only when all of these are true:**

1. The issue’s rule is implemented (helper or existing pure function).
2. At least one xUnit test names that issue (see naming below) and
   `dotnet test` is green for it.
3. The issue file `Status:` line is `unresolved-tested`, and a
   **Unit tests** block lists the test names.

**Do not** move Unity-only issues here (pose leak, forfeit z-order,
achievements atlas, portrait GameObject hierarchy, Encyclopedia click).
Leave those in `unresolved/` until in-game confirm, then `resolved/`.

**Do not** skip in-game testing because the file sits in
`unresolved-tested/`. The next human pass for that issue is still the
checklist in
[`started/issue-resolution-in-game-tests.md`](../started/issue-resolution-in-game-tests.md)
(or a new section there). On confirm, move to `resolved/` as today.

When moving, update links in other docs that pointed at
`issues/unresolved/<file>.md`.

### Issue file block

```
Status: unresolved-tested

## Unit tests (unresolved-tested)

Moved: YYYY-MM-DD
Tests:
- LokrModding.Tests.<Class>.<Method>
Still needs in-game confirm. A passing unit test is not a running-game
confirm.
```

### Test naming

Each test that backs an issue includes the kebab-case issue id in the
method name or a `[Trait("Issue", "loot-anyof-chance-always-fires")]`
so a sweep can find coverage. Prefer both when the test is the reason
for the move.

---

## Phase 0 — Scaffold

**Done** (2026-08-15). `dotnet test LokrModding.sln` discovers tests;
`SmokeTests.LabAliases_FileName_IsAliasesJson` plus the rest of the suite
pass. Target framework is `net10.0` (this host has no `net8.0` runtime).

Helpers are linked into the test project rather than ProjectReferenced
(see Constraints). `InternalsVisibleTo` is on LokrModAPI,
LokrCharacterLoader, LokrPatch, and LokrLab.

1. Add `LokrModding.Tests/LokrModding.Tests.csproj`:
   - `net8.0` (or current SDK default that can reference `netstandard2.0`)
   - xUnit + `Microsoft.NET.Test.Sdk`
   - `IsTestProject=true`
   - ProjectReferences to plugins as they gain tests (start with
     `LokrModAPI` + `LokrCharacterLoader` only)
2. Gate `Directory.Build.props` Unity/BepInEx `ItemGroup` and
   `Directory.Build.targets` `DeployToBepInEx` on
   `'$(IsTestProject)' != 'true'`.
3. Add the test project to `LokrModding.sln`.
4. `InternalsVisibleTo("LokrModding.Tests")` on plugins that will be
   referenced (start: `LokrModAPI`, `LokrCharacterLoader`; add others
   in later phases). Put it in a small `Properties/InternalsVisibleTo.cs`
   (no plain `//` comments; a one-line `/// <summary>` on the file is
   not required for assembly attributes — keep the file to the
   attribute plus an existing plugin-level doc if the standards require
   a type; assembly attributes can live next to the plugin class file
   if that is cleaner).
5. One smoke test that does not touch Unity (e.g. `LabAliases.FileName`
   equals `aliases.json`, or `TextEscaping` if that is the first
   ModAPI test).
6. Document `dotnet test` in this file and a one-liner in
   [`CLAUDE.md`](../../CLAUDE.md) Development Workflows.

**Do not:** Unity Test Framework, a second solution, shipping tests
into `BepInEx/plugins/`.

---

## Phase 1 — LokrModAPI

**Done** (2026-08-15). `TextEscaping` quotes/backslashes/empty/null;
`ModPathLookup.TryFindFile` against a temp `Mods/` tree.

Use it on:

- `TextEscaping.JsonEscape` (round-trip / quotes / backslashes)
- Any `TryFindFile` / folder-layout helper that can run against a temp
  directory (create a throwaway `Mods/` tree in the test output dir)

Skip: splash-video patch, `LoadSprite`, audio. No issues in
`unresolved/` are ModAPI-only today; this phase is so later plugins
can reference a proven test project.

---

## Phase 2 — LokrCharacterLoader

**Done** (2026-08-15). Alias expand, expression rewrite, KV corrupt
fixture, `ContentRules` (skills cap, invisibility edge, FindPartIndex,
null definition, packed-sprite match, buff-store index, roster merge).
Matching open issues are in `unresolved-tested/`. Resolved `$alias` /
UnitName cases stay as regression tests.

Use it on (existing types, little or no extract):

| Target | Issue / regression |
|--------|-------------------|
| `LabAliases.Expand` / `Load` / `Save` | resolved `lab-alias-loc-keys-not-expanded` (greedy `$assassin_NAME`); keep as regression |
| `LabExpressionIds.RewriteAbilityText` | resolved `alias-unitname-parsed-as-function`; keep as regression |
| Ability KV **string** fixtures (parse preview / fail) | resolved `ability-kv-parse-empty-filename` (corrupt `AbilityAOETeamFilter`); fixture of the Official Pack malformed line so a re-import would fail the test |
| `SkillsBarSlotCapPatches.Trim` (or extracted list helper) | `skills-bar-five-slot-cap` → `unresolved-tested/` |
| Invisibility on→off edge helper | `invisibility-exit-fires-every-turn` → `unresolved-tested/` |
| `FindPartIndex` miss → skip `-1` | `find-part-index-unvalidated` → `unresolved-tested/` |
| Null `unitDefinition` / `metaExo` guard | `exo-skeleton-null-unitdefinition` → `unresolved-tested/` |
| JSON pre-filter missing sprite | `reload-data-missing-sprite-nre` → `unresolved-tested/` |
| Buff-store index vs `GetAllHeroes` count | `portrait-patches-buff-store-index` → `unresolved-tested/` |

Skip in this phase: `CustomRigLoader` preview, MasterAudio,
`HeroRosterManager` live splice, portrait `SetParent` / hierarchy
(those stay `unresolved/` until in-game).

`legacy-pack-and-lab-import-both-roster`: optional test that a merge of
two sources with **different** ids keeps both. That can move the issue
to `unresolved-tested/` only for the merge rule; the two-card roster
still needs in-game confirm. If the test is only the merge, say so in
the Unit tests block.

---

## Phase 3 — LokrPatch

**Done** (2026-08-15). `PatchRules` covers every skip/clamp/map in the
table below. `ProgressionHelpPopupPatch` (1.0.6) clamps `ShowPage` /
`Next` using those helpers. Matching issues are in `unresolved-tested/`.

Harmony prefixes stay thin: `Prefix` reads vanilla fields, calls the
helper, sets `__result` / returns `false`. Tests call the helper only.

| Helper (extract from) | Issue → `unresolved-tested/` |
|-----------------------|------------------------------|
| Loot `anyOf` chance vs roll | `loot-anyof-chance-always-fires` |
| Dialog no passing child → exit | `dialog-first-no-fallback` |
| `pointMagnitude` → `FunctionPointMagnitude` | `ability-kv-pointmagnitude-constructs-pointmult` |
| Missing AOE center keys → `0` | `ability-aoe-missing-center-keys-nre` |
| `RetreatIfWeekAI` alias | `ability-ai-retreat-if-week-typo` |
| Per-affected not an action (parse skip) | `ability-ai-per-affected-not-action` |
| Empty CallFunction filter → skip | `ability-callfunction-empty-filter-throws` |
| Empty AI considerations → `0` | `ability-ai-empty-brain-divide-by-zero` |
| Null-safe `Equals` | `ability-equal-null-lhs-nre` |
| `ActionsIfEmpty` only when `Count == 0` | `ability-each-in-list-actions-if-empty-inverted` |
| Missing tooltip var → `0` not `999` | `ability-tooltip-missing-var-returns-999` |
| Null `targetFilter` | `activity-interface-point-target-nre` |
| Missing stat key skip | `stats-apply-modifier-missing-stat-throws` |
| Sanitize: do not `DiscardRun`; stow unknown ids | `save-sanitize-drops-unknown-ids` |
| Party: keep known ids; no reset on `Count != 3` | `save-party-reset-to-vanilla-trio` |
| `ItemInstance.id` null → Guid | `inventory-additem-never-sets-id` |
| Unknown skill id skip | `hero-update-skills-unknown-id-nre` |
| Unknown uniqueId drop | `hero-progress-window-unknown-uniqueid-nre` |
| Unknown `StartingHeroes` skip | `map-start-unknown-starting-hero-nre` |
| Null `GetConfigByKey` skip | `map-hud-unknown-modifier-config-nre` |
| Progression-help index clamp / Finished | `progression-help-popup-index-oor` (implement the helper + patch in this phase if not already shipped) |

Resolved regression (optional, same pattern): empty `ActiveUnit` skip
from `fight-started-empty-initiative-nre`.

Skip: `SuppressedUnityLogPatches`, LeanTouch, anything that only
filters log spam.

---

## Phase 4 — LokrLab (string and disk only)

**Done** (2026-08-15). Hit-tag whitelist, RANGE_CONE hide/warn, dead
AbilityEvent names, `UNIT_<uniqueId>_` loc stems, corrupt-KV fixture.
`AbilityKvIO` / KVParser.KV1 is not loaded in xUnit (game DLL); the
corrupt-line detector covers the Official Pack extra-quote case.

Use it on:

| Target | Issue |
|--------|--------|
| `AbilityKvIO` round-trip / reject extra quotes | fixtures; Official Pack malformed `AbilityAOETeamFilter` line |
| Hit closed-tag whitelist (`#PROJECTILE`) | `ability-hit-closed-tag-whitelist` |
| Hide/warn `RANGE_CONE` | `ability-aoe-range-cone-empty` |
| Hide/warn never-fired AbilityEvent names | `ability-events-never-dispatched` |
| `RLHeroesGenerator` loc stems `UNIT_<uniqueId>_` | regression for resolved loc-key issue |
| Rest-delta compensate, temp group pivot, root-motion sample/expand | `animator-feel` (`AnimatorFeelRulesTests`; 0.12.32) |

Skip: `EditHistoryPanel`, `MenuBarPanel.EnsurePopups`, Animator
cameras, Island atlas clicks, FadeScreen. Those need Unity or the
player (Deferred).

---

## Phase 5 — Remaining plugins (use or explicitly skip)

Walk each leftover plugin in one pass. Either add tests or write a
**Skip** subsection here with why.

### LokrLabApi

**Skip for this track.** Contracts are `Host` / `LabSceneContext` /
menu registration. No Unity-free table (menu visibility predicates,
project-type id lists without Host) was worth extracting.

### SimpleUI

**Skip for this track.** `UiModal.Show` / fake-null needs a Unity
2020.3 Play Mode project (Deferred). Do not pretend xUnit covers it.

### LokrModMenu

**Skip.** Overlay + BackQuote stay in-game. No isolated hotkey predicate
was extracted.

### LokrEncyclopedia

**Skip.** The postfix is two flags; click is a serialized Coming Soon
popup. Already `resolved/` via in-game confirm.

### LokrPatch / Loader / Lab

Already covered in phases 1–4. This phase is only the leftovers.

---

## Phase 6 — Issue sweep and checklist

**Done** (2026-08-15). Every unit-testable id in the [Open-issue map](#open-issue-map)
is in `unresolved-tested/` (or already `resolved/` for regressions).
Five Unity-only issues remain in `unresolved/`. Human in-game pass is a
**separate** session.

1. Grep `Trait("Issue"` / issue ids in `LokrModding.Tests`.
2. Move files; fix links.
3. Human in-game pass is a **separate** session, not this phase.

---

## Open-issue map (as of 2026-08-15, after Layer 1)

Snapshot of the Layer 1 sweep. **Current folders (2026-08-15 evening):**
29 files in `unresolved-tested/` (README excluded), 5 in `unresolved/`.
In-game confirms since this map: `inventory-additem-never-sets-id` and
`progression-help-popup-index-oor` are `resolved/` (remove them from the
list below when using it as a live inventory). `party-stow-shifts-remaining-into-wrong-slots`
was filed during Pass 2 and is also `resolved/`.

Unit-testable (31 at Layer 1) — then in `unresolved-tested/`:

`loot-anyof-chance-always-fires`, `dialog-first-no-fallback`,
`ability-kv-pointmagnitude-constructs-pointmult`,
`ability-aoe-missing-center-keys-nre`, `ability-ai-retreat-if-week-typo`,
`ability-ai-per-affected-not-action`,
`ability-callfunction-empty-filter-throws`,
`ability-ai-empty-brain-divide-by-zero`, `ability-equal-null-lhs-nre`,
`ability-each-in-list-actions-if-empty-inverted`,
`ability-tooltip-missing-var-returns-999`,
`activity-interface-point-target-nre`,
`stats-apply-modifier-missing-stat-throws`,
`save-sanitize-drops-unknown-ids`, `save-party-reset-to-vanilla-trio`,
`inventory-additem-never-sets-id`, `hero-update-skills-unknown-id-nre`,
`hero-progress-window-unknown-uniqueid-nre`,
`map-start-unknown-starting-hero-nre`,
`map-hud-unknown-modifier-config-nre`, `skills-bar-five-slot-cap`,
`invisibility-exit-fires-every-turn`, `find-part-index-unvalidated`,
`exo-skeleton-null-unitdefinition`, `reload-data-missing-sprite-nre`,
`portrait-patches-buff-store-index`, `progression-help-popup-index-oor`,
`ability-hit-closed-tag-whitelist`, `ability-aoe-range-cone-empty`,
`ability-events-never-dispatched`, plus merge-by-id for
`legacy-pack-and-lab-import-both-roster` (two roster cards still need
in-game confirm).

Stay in `unresolved/` until in-game (5):

`animator-pose-leaks-across-frames`,
`sandbox-forfeit-confirm-behind-settings`,
`achievements-nre-on-atlas-load`, `portrait-patches-self-parent`,
`portrait-patches-hardcoded-hierarchy`.

---

## Habit after the suite exists

On any new helper or LokrPatch prefix: add the test on the **same
change** as the helper. If it backs an issue, move the issue to
`unresolved-tested/` in that same change. Do not “backfill later.”

---

## Deferred (not this roadmap)

- Unity 2020.3 Play Mode project for SimpleUI destroy/rebuild (Slice
  Atlas / EditHistory class of bugs).
- BepInEx in-player scenario runner (Lab open/close, sandbox start).
- Ripping the game into an Editor project.
- Recompiling `Ironhide.Legends.dll` as a test host.
- CI without `GameDir`.

Revisit only after Phase 6.

---

## How to run

```bash
dotnet test LokrModding.sln
dotnet test LokrModding.Tests/LokrModding.Tests.csproj
```

Do not `dotnet test` as a substitute for Steam/Proton when marking
`resolved/`.
