# BepinEx Lokr Modding Documentation

Complete documentation for the modular BepInEx plugin architecture that powers mod support for Legends of Kingdom Rush.

## Start here

- **[ARCHITECTURE.md](ARCHITECTURE.md)** — High-level overview of the plugins, their responsibilities, dependency graph, and key design patterns used throughout
- **[modapi-plan.md](modapi-plan.md)** — The original architecture proposal (Section 1–10, still current) that explains *why* the codebase is structured this way
- **[capabilities-and-gaps.md](capabilities-and-gaps.md)** — What the current system supports and where it falls short; recommended priorities for future work
- **[roadmaps/README.md](roadmaps/README.md)** — Character Creator planning hub (completed / started / not-started)
- **[mods-folder-structure.md](mods-folder-structure.md)** — `Mods/` category layout (RLHeroes, Characters, Abilities, …)
- **[code-documentation-standards.md](code-documentation-standards.md)** — XML doc comment rules and how/when to rebake generated `docs/api/` HTML
- **[issues/](issues/README.md)** — open, unit-tested-but-unconfirmed, and resolved project issues (`unresolved/`, `unresolved-tested/`, `resolved/`)
- **[git-and-releases.md](git-and-releases.md)** — repo layout (hub + submodules), fresh-clone setup, and `scripts/release-plugin.sh`

## Plugin documentation

Each plugin has its own documentation folder with consistent sections:

- **`overview.md`** — one-paragraph summary and metadata
- **`architecture.md`** — major design decisions and patterns (or `classes.md` for simpler plugins)
- **`layout.md`** — file/folder structure and namespace organization
- **`classes.md`** or **`supporting-classes.md`** — every public class and its API
- **`conventions.md`** — naming, patterns, and style used throughout
- **`cross-references.md`** — base-game behavior dependencies and neighboring plugins

### [LokrModAPI](../LokrModAPI/docs/)

The foundational shared-utility layer — mod folder discovery, asset loading, audio playback, configuration. Every other plugin depends on this.

**Key topics:**
- [`architecture.md`](../LokrModAPI/docs/architecture.md) — `ModAPI` facade pattern, bootstrap order, initialization guarantees
- [`classes.md`](../LokrModAPI/docs/classes.md) — `ModFileSystem`, `ModAssetLoader`, `ModAudioService`, `ModConfig` (BepInEx `ConfigFile` integration), `AttachedData<K,V>`, `SplashVideoController` patch

### [LokrCharacterLoader](../LokrCharacterLoader/docs/)

Runtime character/content-loading system. Owns all Harmony patches for character definitions, abilities, portraits, sounds, Lua scripts. Exposes `CharacterAPI` for other plugins to extend character content.

**Key topics:**
- [`architecture.md`](../LokrCharacterLoader/docs/architecture.md) — resolver-chain pattern, full-method-replacement patches, `DefaultContentSources` wiring
- [`character-api.md`](../LokrCharacterLoader/docs/character-api.md) — `CharacterAPI` extension points: resolvers (portraits, sounds, rigs), events (roster, units, abilities, localization, scripts), state-visual-effects
- [`patches.md`](../LokrCharacterLoader/docs/patches.md) — every patch file (HeroRoster, UnityDefinitions, Abilities, Portraits, Sounds, etc.)
- [`custom-rig-loader.md`](../LokrCharacterLoader/docs/custom-rig-loader.md) — loading mod-provided skeleton/animation definitions from JSON

### [LokrLabApi](../LokrLabApi/docs/)

Editor **contracts** library — project types, `Host`, lab-scene events, `CurrentSession`, shell menus. No Harmony, no rendering. LokrLab and any third-party editor depend on it; they do not depend on each other.

### [LokrLab](../LokrLab/docs/)

In-game **editor suite** — host shell plus Character, Ability, and Encounter authoring. Real scene transition (origin scene unloaded while open), opened from the title-screen Mods button or the global mod menu. Opens on the **Project Browser**; a dockable shell appears once a project is open. Content lives under `Mods/LokrLab/`.

**Key topics:**
- [`architecture.md`](../LokrLab/docs/architecture.md) — scene transition lifecycle, Host binding
- [`character/overview.md`](../LokrLab/docs/character/overview.md) — Properties / Animator / Sandbox
- [`character/rig-editor-scene.md`](../LokrLab/docs/character/rig-editor-scene.md) — Animator orchestrator
- [`ability/overview.md`](../LokrLab/docs/ability/overview.md) — Ability Library + overlay fallback
- [`encounter/overview.md`](../LokrLab/docs/encounter/overview.md) — Encounter project type (Setup + Sandbox)

### [LokrModMenu](../LokrModMenu/docs/)

Global mod menu popup and shared hotkey entry point. Other plugins register buttons/submenus via `ModMenuAPI`.

**Key topics:**
- [`overview.md`](../LokrModMenu/docs/overview.md) — hotkeys (BackQuote primary, optional F3), blocking overlays
- [`classes.md`](../LokrModMenu/docs/classes.md) — `ModMenuAPI`, `ModMenuOverlay`, registration API

### [SimpleUI](../SimpleUI/docs/)

Passive UI widget library — fluent API for building responsive layouts without GameObject boilerplate.

**Key topics:**
- [`architecture.md`](../SimpleUI/docs/architecture.md) — CRTP pattern for fluent chaining, composition via `.Add()`, sizing via `LayoutElement` hints, theme inheritance
- [`classes.md`](../SimpleUI/docs/classes.md) — every widget: containers (`UiPanel`, `UiStack`, `UiSplit`, `UiSplitter`, `UiDockSpace`, `UiTabGroup`), controls (`UiButton`, `UiToggle`, `UiTextField`, `UiDropdown`, `UiComboBox`, `UiList<T>`, `UiTree`, `UiContextMenu`, `UiToolbar`, `UiStatusBar`), display (`UiLabel`, `UiImage`), theme (`UiTheme`)
- [`conventions.md`](../SimpleUI/docs/conventions.md) — naming (Ui prefix), factory methods, sizing (pixels vs. fractions), theming, interactivity

### [LokrEncyclopedia](../LokrEncyclopedia/docs/)

Small independent plugin that unlocks the base game's Encyclopedia button on the main menu.

**Key topics:**
- [`overview.md`](../LokrEncyclopedia/docs/overview.md) — why it's separate (no character-content involvement)
- [`classes.md`](../LokrEncyclopedia/docs/classes.md) — one patch: `UIMainMenuPatches`

### [LokrPatch](../LokrPatch/docs/)

Standalone base-game bug fixes and defensive error handling (no mod-content or API surface).

**Key topics:**
- [`overview.md`](../LokrPatch/docs/overview.md) — scope: vanilla bugs, crash prevention, when to add patches here
- [`classes.md`](../LokrPatch/docs/classes.md) — duplicate skill guards, Unity log filters, LeanTouch NRE suppression, End Turn icon, missing ApplyModifier, metagame ctor flag
- [`conventions.md`](../LokrPatch/docs/conventions.md) — LokrPatch vs LokrCharacterLoader responsibilities

## How the pieces fit together

```
Game (Ironhide.Legends)
        ↑ (Harmony patches)

LokrModAPI
        ↑ (all plugins depend on this)

┌───────┴────────┬────────────┬──────────────┐
│                │            │              │
LokrCharacter    SimpleUI     LokrEncyclopedia
Loader                        LokrPatch (independent)
        ↑
        │ (CharacterAPI, CustomRigLoader, live reload)
        │
┌───────┴────────┐
│ LokrModMenu    │  ← global hotkey + ModMenuAPI
└───────┬────────┘
        ↑ (BepInDependency)
        │
┌───────┴────────────────┐
│ LokrLabApi           │  editor contracts
│ LokrLab              │  suite: host + Character + Ability
└──────────────────────┘
(suite uses SimpleUI; registers with ModMenuAPI)
```

**Read this order:**
1. [ARCHITECTURE.md](ARCHITECTURE.md) — understand the big picture
2. `LokrModAPI/docs/` — foundation
3. `LokrCharacterLoader/docs/` — content system + live reload
4. `LokrModMenu/docs/` — global menu and hotkeys
5. `SimpleUI/docs/` — UI library
6. `LokrLabApi/docs/` — editor contracts
7. `LokrLab/docs/` — editor suite
8. `LokrLab/docs/character/` — Character project type
9. `LokrLab/docs/ability/` — Ability Library (optional until you touch abilities)

---

## Reference materials

- [`reference/README.md`](reference/README.md) — decompiled base-game location, ExoSkeleton rig dump, asset extraction tooling
- [`base-game-namespaces.md`](base-game-namespaces.md) — all C# namespaces in the decompiled game source (regenerate with `list_base_game_namespaces.py`)
- [`base-game-documentation-checklist.md`](base-game-documentation-checklist.md) — prioritized spine classes and hub pages (subset of the full-coverage track)
- [`roadmaps/completed/base-game-html-docs.md`](roadmaps/completed/base-game-html-docs.md) — full-coverage Base Game Reference HTML (all 1631 pages verified)
- [`unit-load-path.md`](unit-load-path.md) — RLHeroes/EnemiesDefinitions/Character Lab `Characters/` through `UnityDefinitionsParser` to `UnitDefinition`
- [`roster-load-path.md`](roster-load-path.md) — hero roster JSON through `HeroRosterManager`
- [`ability-load-path.md`](ability-load-path.md) — ability KV through `AbilitiesDefinitions` to runtime `Ability`
- [`exoskeleton-pipeline.md`](exoskeleton-pipeline.md) — bundle vs `ReloadData` rigs, world vs UI renderers
- [Character File Reference](api/character-reference/index.html) — Official Pack + base-game txt formats (RLHeroes, abilities, roster, localization, …)
- [`roadmaps/README.md`](roadmaps/README.md) — planning hub: completed, started, and not-started tracks
- [`roadmaps/phasing.md`](roadmaps/phasing.md) — what's done and what's next
- [`roadmaps/started/live-reload.md`](roadmaps/started/live-reload.md) — hot-reload Lab edits (Phase 1–2 done)
- [`roadmaps/completed/archive/`](roadmaps/completed/archive/) — historical implementation plans (shipped)

---

## For developers extending these plugins

If you're writing a mod that uses this architecture:

1. **To add character content** (heroes, abilities, sounds):
   - Reference `LokrCharacterLoader.dll`
   - Declare `[BepInDependency(LokrCharacterLoaderPlugin.Guid)]` in your plugin
   - Call `CharacterAPI.RegisterPortraitResolver(...)`, `CharacterAPI.BuildingHeroRoster += ...`, etc.
   - See [`LokrCharacterLoader/docs/character-api.md`](../LokrCharacterLoader/docs/character-api.md)

2. **To add in-game UI**:
   - Reference `SimpleUI.dll`
   - Use `UiPanel.Create(...)`, `UiButton.Create(...)`, etc.
   - See [`SimpleUI/docs/classes.md`](../SimpleUI/docs/classes.md)

3. **To access assets/config**:
   - Reference `LokrModAPI.dll`
   - Use `ModAPI.Files`, `ModAPI.Assets`, `ModAPI.Config`
   - See [`LokrModAPI/docs/overview.md`](../LokrModAPI/docs/overview.md)

---

## Key architectural patterns

### Resolver chains
Priority-ordered, first-non-null-wins. Used for portraits, sounds, rigs.
```csharp
CharacterAPI.RegisterPortraitResolver((heroId, slot) => portrait, priority: 100);
```

### Events
All subscribers' results merged. Used for roster, units, abilities, localization.
```csharp
CharacterAPI.BuildingHeroRoster += roster => roster.AddLegend(...);
```

### Service facade
Static, initialized once, read-only thereafter. Used by `ModAPI` and (implicitly) UI theme.
```csharp
ModAPI.Files.TryFindFile("Portraits", "Hero1/Hero1_MINI.png", out path);
```

### Full-method-replacement patches
For tightly-coupled base-game methods. Used extensively in `LokrCharacterLoader`.
```csharp
[HarmonyPrefix]
private static bool Prefix() { /* full reimplementation */ return false; }
```

See [ARCHITECTURE.md](ARCHITECTURE.md) for detailed explanations and trade-offs.

---

## Questions or issues?

- Check the plugin docs for the relevant component — each folder is self-contained
- Read [`cross-references.md`](../LokrCharacterLoader/docs/cross-references.md) in any plugin to understand base-game dependencies
- See [`issues/`](issues/README.md) for open and resolved problems
- See [`capabilities-and-gaps.md`](capabilities-and-gaps.md) for platform limits and suggested next steps
