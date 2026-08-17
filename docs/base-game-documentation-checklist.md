# Base game documentation checklist

Tracked plan for hand-filling the [Base Game Reference](api/base-game/index.html)
class pages that matter for **complete custom characters** (content loading,
rigs, abilities, later combat spawn). This is the curated spine — not the
whole tree.

**Full coverage** of all ~1631 pages (every class, mod-friendliness, bugs,
agent-parallel three-pass workflow) is
[roadmaps/completed/base-game-html-docs.md](roadmaps/completed/base-game-html-docs.md).
Do not treat a class as out of scope for that roadmap just because this
checklist deprioritized it.

**Companion docs (already exist — don't duplicate here):**

| Layer | Doc | Covers |
|-------|-----|--------|
| Txt/KV formats | [Character File Reference](api/character-reference/index.html) | RLHeroes, abilities, roster, appendices |
| Namespace map | [base-game-namespaces.md](base-game-namespaces.md) | All 155 declared namespaces + file counts |
| Mod hooks | [LokrCharacterLoader patches](../LokrCharacterLoader/docs/patches.md) | Which base-game methods are Harmony-patched |
| Roadmap alignment | [roadmaps/phasing.md](roadmaps/phasing.md) | Phasing: General → AbilityLab → Sandbox → Extensions |

**Load-path hubs:**

| Hub | Doc |
|-----|-----|
| Unit definitions | [unit-load-path.md](unit-load-path.md) |
| Hero roster | [roster-load-path.md](roster-load-path.md) |
| Abilities (loader) | [ability-load-path.md](ability-load-path.md) |
| ExoSkeleton / rigs | [exoskeleton-pipeline.md](exoskeleton-pipeline.md) |

---

## Status key (read this before the tables)

| Status | Meaning |
|--------|---------|
| `done` | **Fully documented** on that HTML page: Description, Remarks, Usage Examples, **and** every member section — zero `TODO` placeholders left (except the boilerplate generator comment in `<!-- ... -->`). |
| `spine` | **Class-level only:** Description, Remarks, and Usage Examples are filled, but the page still has member-level `TODO`s (properties, methods, fields). **Not finished.** |
| `todo` | Spine sections still contain `TODO` — not started or reverted. |
| `deferred` | Intentionally out of scope until a later roadmap phase. |

**Member TODOs** column = count of remaining `TODO` strings on the page (excluding
the auto-generated boilerplate comment). Re-check with:

```bash
python3 docs/api/audit_spine_docs.py
```

**Spine prose** (Description / Remarks / Examples for curated classes) can be
re-applied from `apply_spine_docs.py`; that script does **not** mark a page `done`.

---

## Hub pages

| Status | Hub | Purpose |
|--------|-----|---------|
| done | **Unit load path** | [unit-load-path.md](unit-load-path.md) |
| done | **Roster load path** | [roster-load-path.md](roster-load-path.md) |
| done | **Ability load path** | [ability-load-path.md](ability-load-path.md) (loader only) |
| done | **ExoSkeleton pipeline** | [exoskeleton-pipeline.md](exoskeleton-pipeline.md) |
| todo | **Expression & action model** | Appendices §Q, §T — Tier 3 |
| deferred | **Combat spawn path** | Sandbox prerequisite |

Hub markdown files are `done` when the hub doc itself is written. That does **not**
imply linked class pages are `done`.

---

## Tier 1 — Content injection pipeline

### Unit definitions

| Status | Member TODOs | Class | Base-game page |
|--------|-------------:|-------|----------------|
| done | 0 | `UnityDefinitionsParser` | [page](api/base-game/Ironhide/Legends/Model/Game/Units/UnityDefinitionsParser.html) |
| done | 0 | `UnitDefinition` | [page](api/base-game/Ironhide/Legends/Model/Game/Units/UnitDefinition.html) |
| spine | 185 | `Unit` | [page](api/base-game/Ironhide/Legends/Model/Game/Units/Unit.html) |
| spine | 16 | `Stat` | [page](api/base-game/Ironhide/Legends/Model/Game/Units/Stat.html) |
| done | 0 | `StatHelper` | [page](api/base-game/Ironhide/Legends/Model/Game/Units/StatHelper.html) |

### Hero roster

| Status | Member TODOs | Class | Base-game page |
|--------|-------------:|-------|----------------|
| done | 0 | `HeroRosterManager` | [page](api/base-game/Ironhide/Legends/Model/Metagame/HeroRosterManager.html) |
| spine | 5 | `HeroRosterConfig` | [page](api/base-game/Ironhide/Legends/View/Metagame/Screens/Logic/HeroRosterConfig.html) |
| spine | 34 | `Hero` (metagame; includes `exoSkeletonDataAsset`) | [page](api/base-game/Ironhide/Legends/Model/Metagame/Heroes/Hero.html) |

### Abilities (loader only — parser depth is Tier 3)

| Status | Member TODOs | Class | Base-game page |
|--------|-------------:|-------|----------------|
| done | 0 | `AbilitiesDefinitions` | [page](api/base-game/Ironhide/Legends/Model/Game/Units/Abilities/AbilitiesDefinitions.html) |
| spine | 36 | `Ability` (runtime model) | [page](api/base-game/Ironhide/Legends/Model/Game/Units/Abilities/Ability.html) |

### Localization & scripts

| Status | Member TODOs | Class | Base-game page |
|--------|-------------:|-------|----------------|
| spine | 25 | `LocalizationManager` | [page](api/base-game/Ironhide/Localization/LocalizationManager.html) |
| spine | 8 | `IronhideScriptLoader` | [page](api/base-game/Ironhide/Legends/Model/Metagame/Scripts/IronhideScriptLoader.html) |

### Assets (portraits & icons)

| Status | Member TODOs | Class | Base-game page |
|--------|-------------:|-------|----------------|
| spine | 1 | `DataHelper` | [page](api/base-game/DataHelper.html) — properties section TODO |
| done | 0 | `MapHeroBarPortraitComponent` | [page](api/base-game/MapHeroBarPortraitComponent.html) |

**Tier 1 summary (2026-08-12 audit):** 7 `done`, 8 `spine`, 0 `todo` among spine
classes. Hubs for unit/roster/ability load paths are written; most large runtime
classes still need member documentation.

---

## Tier 2 — ExoSkeleton / custom rigs

| Status | Member TODOs | Class | Base-game page |
|--------|-------------:|-------|----------------|
| spine | 22 | `ExoSkeletonDataAsset` (`ReloadData` schema in Remarks) | [page](api/base-game/Ironhide/ExoSkeleton/ExoSkeletonDataAsset.html) |
| done | 0 | `ExoSkeletonData` | [page](api/base-game/Ironhide/ExoSkeleton/ExoSkeletonData.html) |
| spine | 5 | `ExoSkeletonRenderer` | [page](api/base-game/Ironhide/ExoSkeleton/ExoSkeletonRenderer.html) |
| spine | 3 | `ExoSkeletonUIGraphic` | [page](api/base-game/ExoSkeleton/Code/ExoSkeletonUIGraphic.html) |
| spine | 15 | `AssetBundleManager` | [page](api/base-game/Ironhide/AssetBundles/AssetBundleManager.html) |
| spine | 20 | `ExternalLoaderController` | [page](api/base-game/Ironhide/ExoSkeleton/ExternalLoaderController.html) |

**Tier 2 summary:** 1 `done`, 5 `spine`. Hub [exoskeleton-pipeline.md](exoskeleton-pipeline.md)
is written; `ExoSkeletonDataAsset` and `ExternalLoaderController` still have the
most member TODOs. `Hero.exoSkeletonDataAsset` is covered on the Tier 1 `Hero`
page (also `spine`).

**Mod cross-ref:** [CustomRigLoader](../LokrCharacterLoader/docs/custom-rig-loader.md),
[HeroExoSkeletonPatches](../LokrCharacterLoader/Patches/HeroExoSkeletonPatches.cs).

---

## Tier 3 — Ability runtime (parallel with LokrAbilityLab v1)

| Status | Item | Base-game page |
|--------|------|----------------|
| todo | `AbilityParser` | [page](api/base-game/Ironhide/Legends/Model/Game/Units/Abilities/AbilityParser.html) |
| todo | `AbilityEvents` | constants class |
| todo | `ModifierEvents` | constants class |
| todo | Action type registry | `AbilityParser.genericClassConfigs` |
| todo | Worked action examples (3) | `Damage`, `ApplyModifier`, `ActOnTargets` |
| todo | Expression functions | evaluator + function table |
| todo | AI selection (light) | `AIBrain` + consideration types |

---

## Tier 4 — Combat spawn (defer until Sandbox Encounter v1)

| Status | Item |
|--------|------|
| deferred | Encounter / spawn entry points |
| deferred | Turn / initiative owner |
| deferred | Fight lifecycle |

---

## Explicitly deprioritized

- `SRDebugger.*`, `SRF.*`, `DG.Tweening.*`, most map UI, achievements (except roster gating), level editor, 422 `(global)` root classes — see [base-game-namespaces.md](base-game-namespaces.md).

---

## Suggested order of work

1. [ ] Tier 3: **Expression & action model** hub + **`AbilityParser`**
2. [x] Tier 1/2 **hubs** and **spine sections** for curated classes (`apply_spine_docs.py`)
3. [ ] Tier 1/2 **member docs** — prioritize by mod touch frequency:
   - `ExoSkeletonDataAsset.ReloadData` + `ExternalLoaderController` (custom rigs)
   - `Unit` + `Ability` (combat debugging)
   - `LocalizationManager`, `AssetBundleManager`, `Hero`, `Stat`
4. deferred — **Combat spawn path** hub when Sandbox v1 begins

---

## Maintenance

| Task | Command |
|------|---------|
| Audit spine status + TODO counts | `python3 docs/api/audit_spine_docs.py` |
| Re-apply spine Description/Remarks/Examples | `python3 docs/api/apply_spine_docs.py` |
| Regenerate sitewide search index | `python3 docs/api/generate_search_index.py` |
| Refresh namespace index | `python3 docs/list_base_game_namespaces.py` |

**Last reviewed:** 2026-08-12 (`audit_spine_docs.py`)
