# BepinEx Modular Plugin Architecture — Overview

This solution consists of **eight BepInEx plugins** (including SimpleUI, LokrLabApi, and the LokrLab suite), designed with clear separation of concerns: each plugin has a single responsibility, exposes a well-defined public API where applicable, and depends only on what it needs.

## Plugin dependency graph

```
Game (Ironhide.Legends)  ← Harmony patches
        ↑
   LokrModAPI  ← all plugins use ModAPI / GameInputPoll
        ↑
   LokrCharacterLoader  ← CharacterAPI, CustomRigLoader, live reload
        ↑
   LokrModMenu  ← ModMenuAPI, global hotkey (BackQuote / optional F3)
        ↑
   LokrLabApi  ← editor contracts (project types, session, menus)
        ↑
   LokrLab  ← editor suite (host + Character + Ability)
        ↑
   SimpleUI  ← shared UI widgets (referenced by labs)

Independent: LokrPatch (vanilla fixes), LokrEncyclopedia (menu unlock), SimpleUI / LokrLabApi (libraries)
```

**Plugin order** (guaranteed by BepInEx's `[BepInDependency]` system):
1. `LokrModAPI` — no dependencies
2. `LokrPatch`, `SimpleUI`, `LokrLabApi` — no plugin dependencies
3. `LokrEncyclopedia` — depends on `LokrModAPI`
4. `LokrCharacterLoader` — depends on `LokrModAPI`
5. `LokrModMenu` — depends on `LokrModAPI`
6. `LokrLab` — depends on `LokrLabApi`, `SimpleUI`, `LokrModAPI`, `LokrModMenu`, `LokrCharacterLoader`

## Per-plugin responsibilities

### `LokrModAPI` — Foundation

The **core shared utility layer**. Every other plugin depends on this.

**Provides:**
- Mod folder discovery and file scanning (`ModAPI.Files`)
- Texture/sprite/audio asset loading (`ModAPI.Assets`)
- Sound playback service (`ModAPI.Audio`)
- Configuration management via BepInEx `ConfigFile` (`ModAPI.Config`)
- Generic extension-data helper for side-tables (`AttachedData<K,V>`)
- Splash-screen skip patch

**Does NOT provide:**
- Gameplay features (this is pure infrastructure)
- Character/content domain logic

**Exposed via:** Static `ModAPI` facade (`ModAPI.Files`, `ModAPI.Assets`, `ModAPI.Audio`, `ModAPI.Config`)

**Used by:** Every other plugin in the solution

---

### `LokrCharacterLoader` — Character Content System

The **runtime character/content-loading layer**. Owns every Harmony patch that touches base-game character definitions, abilities, portraits, sounds, and Lua scripts.

**Provides:**
- `CharacterAPI` — public extension point for other plugins to add/override character content without re-patching base-game methods
- Built-in content sources: hero roster, unit definitions, abilities, portraits (6 slots), ability icons, sounds (3 event types), localization, Lua scripts, state-visual-effects
- Custom rig loading (`CustomRigLoader`) — builds mod skeleton/animation rigs from `rig/rig.json` + part PNGs at runtime
- **Live reload** (`CharacterAPI.ReloadLabContent`, `ContentReloader`, `MetagameHeroReloader`) — refresh mod content without restarting the game
- Resolver-chain pattern for all art assets (portraits, sounds, rigs)
- Duplicate hero-roster ids and duplicate Character Lab folders are
  skipped with a warning (first wins) instead of throwing

**Key design:**
- Uses **resolver chains** — higher-priority resolvers tried first, first non-null result wins. Applies to portraits, sounds, rigs, and sprite FXMega / projectiles.
- Uses **events** for content building (roster, unit definitions, abilities, localization, scripts) — allows other plugins to inject content at the right hook points.
- **Dogfoods its own API** — the built-in file-convention content source is registered through the same `CharacterAPI` interface any third-party plugin would use.
- **Full-method-replacement patches** where necessary (most of its patches) — for tight coupling with private fields, reimplements the full method body and skips the original.

**Does NOT provide:**
- In-game UI for creating characters (that's `LokrLab`'s Character module)
- Networking/multiplayer features
- Save-game integration (handled by base game transparently)

**Exposed via:** Static `CharacterAPI` facade + resolver registration methods

**Used by:** `LokrLab` (via `CustomRigLoader` and `CharacterAPI.ReloadLabContent`)

---

### `LokrLabApi` — Editor contracts

Passive **contracts library** for the editor shell. No Harmony patches and no rendering.

**Provides:** `LokrLabApi.RegisterProjectType`, `CurrentSession`, `Selection`, shell menus, `Host`, lab-scene events, `StartEmbeddedScene` / `StartEmbeddedFight`, `ProjectSession` / `LabNode` / `WorkspaceRegistration` / `PersistentInspectorRegistration`.

**Used by:** `LokrLab` (suite: host + Character + Ability + Encounter) and any future third-party editor plugin.

### `LokrLab` — Editor suite

The **in-game editor suite** — host shell plus Character, Ability, and Encounter authoring. A dedicated lab scene (real scene transition: the origin scene is unloaded while the lab is open). Registers `character`, `ability-library`, and `encounter` on `LokrLabApi`. Third-party editors still depend on LabApi, not this DLL.

**Provides:**
- Title-screen **Mods** button (`UIMainMenuPatches`) plus mod-menu **LokrLab** entry via `ModMenuAPI`
- **Project Browser** (empty state) and a **dockable shell** (`UiDockSpace`) once a project is open — workspace tabs, bottom panels, and inspector hosts come from the open project type. Left tabs: Node Tree / File Tree. Shell File / View / Help menus; project types add their own items. Node Tree writes `EditorSelection`. File Tree lists the project folder. Inspector dispatches registered drawers or persistent inspector hosts
- Assigns `LokrLabApi.Host` and raises `LabOpened` / `LabClosing` / `ShellShown`
- **Scene embed** — `StartEmbeddedScene` loads a bundle scene additively and crops its camera to a hole `RectTransform`. Character confirm canvases rebind to the hole camera after Stop→Start. `StartEmbeddedFight` is assigned from this assembly onto the API so other editors can start a fight.
- Character (Properties / Animator / Sandbox), Ability Library (nested action cards; overlay fallback), and Encounter (Setup + Sandbox). Content under `Mods/LokrLab/LokrCharacterLab/`, `Mods/LokrLab/LokrAbilityLab/`, and `Mods/LokrLab/LokrEncounterLab/`.

**Key design:**
- **Scene transition model** — lab scene built fresh on each open; origin scene unloaded; closing transitions back via `FadeScreen` + `TransitionSceneComponent`
- **Project types** — registered on `LokrLabApi`; `project.json` is the shared folder marker

**Uses:** `LokrLabApi`, `SimpleUI`, `LokrModMenu`, `LokrModAPI`, `LokrCharacterLoader`

---

### `LokrModMenu` — Global Mod Menu

**Hotkey-driven popup** listing registered mod tools. Primary binding: **BackQuote (`)** (Linux/Proton-friendly); optional bare **F3** via config.

**Provides:**
- `ModMenuAPI.RegisterButton` / `RegisterSubmenu` — extension points for other plugins
- `ModMenuAPI.RegisterBlockingOverlay` — LokrLab registers so the menu does not stack on top; hotkey closes overlay first
- `GameInputPoll` integration (via `LokrModAPI.Input`) for reliable key detection under Proton

**Depends on:** `LokrModAPI` only

---

### `SimpleUI` — UI Widget Library

A **passive shared-library plugin** (no Harmony patches, no gameplay logic) that provides reusable UI building blocks.

**Provides:**
- Fluent widget API for building responsive layouts
- Containers: `UiPanel` (background box), `UiStack` (row/column flow), `UiSplit` (weight-driven grid), `UiSplitter` (draggable divider)
- Docking: `UiDockSpace` (Left/Center/Right/Bottom zones), `UiDockPanel`, `UiTabGroup` (no floating panels; layout snapshot is consumer-owned)
- Screen switcher: `UiScreenSwitcher` (named full-stretch siblings)
- Controls: `UiButton`, `UiToggle`, `UiTextField`, `UiDropdown`, `UiComboBox`, `UiList<T>`, `UiTree`, `UiContextMenu`, `UiToolbar`, `UiStatusBar`, `UiLabel`, `UiImage`
- Modals: `UiModal` (centered overlay dialog)
- Theming: `UiTheme` for consistent colors/fonts/spacing

**Key design:**
- **Fluent CRTP pattern** — every widget inherits `UiElement<TSelf>`, so method chaining stays typed (e.g., `UiButton.Create(...).OnClick(...).FixedHeight(30)` returns a `UiButton`, not generic base).
- **Composition via `.Add()`** — explicitly nest widgets rather than hand-building hierarchies.
- **Sized via `LayoutElement` hints** — responsive to parent size, not hardcoded pixel positions (except for absolute-positioned modals).
- **No magic state** — does not auto-size panels, does not persist state, does not manage focus. Low-level primitives only; consumers build higher-level systems on top.

**Used by:** `LokrLab` (suite) and any future third-party editor plugin

**Could be used by:** Any future plugin that needs to build in-game UI

---

### `LokrEncyclopedia` — Encyclopedia Unlock

The **smallest plugin** — unlocks a single base-game button.

**Provides:**
- Patches the "Encyclopedia" button on the main menu to be visible and clickable (it ships in the game but is disabled by default)

**Design:**
- **Deliberately independent** — depends only on `LokrModAPI`, not on `LokrCharacterLoader`, because Encyclopedia has nothing to do with character content
- Demonstrates the modular architecture — a small, self-contained feature can be its own plugin with minimal code and minimal dependencies

**Click:** vanilla **Coming Soon!** popup (serialized on the button). Confirmed 2026-08-15 — see [`issues/resolved/encyclopedia-button-unverified-click.md`](issues/resolved/encyclopedia-button-unverified-click.md). Do not invent Encyclopedia UI.

---

### `LokrPatch` — Base-Game Bug Fixes

**Defensive patches on vanilla Ironhide code** — crash prevention, duplicate-key
guards, and other error-handling improvements that are not tied to mod content.

**Provides:**
- Harmony patches that make the stock game tolerate bad state instead of
  throwing (e.g. duplicate hero skill ids during save load)
- **`SuppressedUnityLogPatches`** — filters known-harmless Unity log spam
  (ability `#` debug actions, UNITDEFINITION migration warnings, AssetBundle
  missing-asset errors, MasterAudio voice exhaustion, LeanTouch NREs)
- **`LeanTouchUpdatePatch`** — swallows LeanTouch `NullReferenceException`
  when input stack is in a bad state (e.g. overlay mod menus)
- Shared normalization helpers used by multiple patches (`HeroSkillSanitizer`)
- `EndTurnClassIconPatch`, `ApplyModifierMissingPatch`, and
  `MetagameManagerInstanciatingPatch` (clears a stuck instantiating flag)

**Design:**
- **Dependency-free** — no `[BepInDependency]`, no reference to `LokrModAPI` or
  `LokrCharacterLoader`. Base-game fixes stay installable even if content
  plugins are removed.
- **Separate from content patches** — `LokrCharacterLoader` adds mod data;
  `LokrPatch` fixes how vanilla code behaves when data is wrong or edge cases
  appear.
- **Skip-and-log** over hard failure for recoverable collisions.

**Does NOT provide:**
- Mod content loading, editor UI, or public extension APIs
- Feature unlocks (see `LokrEncyclopedia`)

**Docs:** [`LokrPatch/docs/`](../LokrPatch/docs/overview.md)

---

## Architecture patterns used across plugins

### 1. Resolver chains (used by `LokrCharacterLoader`)

```csharp
void RegisterPortraitResolver(Func<string, string, Sprite> resolver, int priority = 0);
```

Higher priority first; ties broken by registration order; **first non-null result wins**. Enables:
- Built-in file-convention logic registers at `priority: 0`
- Other plugins register at higher priority to override, or lower priority to fallback

Examples:
- Portrait slots (MINI, BIG, BANNER, MAP, MAPMINI, CHALLENGE)
- Sounds (combat, promote, select-hero)
- State-visual-effects (Assassin invisibility)

### 2. Events (used by `LokrCharacterLoader`)

```csharp
event Action<RosterBuilder> BuildingHeroRoster;
event Action<UnitDefinitionsBuilder> BuildingUnitDefinitions;
```

Fired at specific hook points during content initialization. Differs from resolver chains — **all subscribers' changes are merged**, not "first-one-wins." Used for:
- Content builders (roster, unit definitions, abilities)
- Localization (all language files merged into one)
- Lua scripts (by name)

### 3. Service facade (used by `LokrModAPI` and `SimpleUI`)

```csharp
public static class ModAPI
{
    public static Files.ModFileSystem Files { get; internal set; }
    public static Assets.ModAssetLoader Assets { get; internal set; }
    // ...
}
```

Static facade with private `set` — initialized once at plugin startup, then read-only for all other consumers. Guarantees:
- Single global instance (no accidental duplication)
- Initialization order is explicit and controlled
- Consumers never deal with construction/wiring

### 4. Full-method-replacement patches (used by `LokrCharacterLoader`)

When a base-game method is too tightly coupled with private closures/fields to safely Harmony Prefix/Postfix around:

```csharp
[HarmonyPrefix]
private static bool Prefix(/* original params */)
{
    // Full reimplementation of the original method body
    // ...
    return false;  // skip the original
}
```

Trade-offs:
- **Pro**: complete control, no fragile assumptions about what the original does
- **Con**: must re-read the decompiled source every time the game updates; more likely to diverge from base-game behavior if not careful

Applies to: hero roster parsing, unit definitions parsing, abilities parsing, portrait resolution, sound resolution.

---

## Initialization order and guarantees

BepInEx loads plugins in dependency order:

1. **`LokrModAPI.Awake()`** runs:
   - Instantiates `ModFileSystem`, `ModAssetLoader`, `ModAudioService`, `ModConfig`
   - Exposes them via the static `ModAPI` facade
   - Patches the splash screen skip
   - **After this**: `ModAPI.*` is safe to call from any other plugin's `Awake()`

2. **`LokrPatch.Awake()`**, **`SimpleUI.Awake()`**, **`LokrEncyclopedia.Awake()`** run (no further dependencies beyond optional logging)

3. **`LokrCharacterLoader.Awake()`** runs:
   - Calls `DefaultContentSources.RegisterAll()` — wires up all built-in content resolvers/subscribers
   - Runs `Harmony.PatchAll()` — applies all patches
   - **After this**: patched game methods can fire safely; all built-in content sources are ready

4. **`LokrModMenu.Awake()`** runs:
   - Registers hotkeys via `GameInputPoll`
   - Builds persistent mod-menu overlay scene (initially hidden)

5. **`LokrLabApi.Awake()`** runs with the other library plugins (no further work beyond logging).

6. **`LokrLab.Awake()`** runs:
   - Binds jump/return/refresh and shell menus, and registers the mod-menu entry
   - Registers the Character project type and migrates `project.json` onto existing character folders
   - Registers the Ability Library project type (many named libraries)
   - Patch title screen (Mods button); builds its lab scene on first open
   - Ready to open on demand

**Critical constraint**: `DefaultContentSources.RegisterAll()` **must run before** `Harmony.PatchAll()` in the same plugin. If a patched game method fires before any resolvers are registered, it will find nothing and fall through to vanilla behavior — and later registration won't help (the method already fired). This is guaranteed by calling `RegisterAll()` explicitly in `Awake()` before invoking `Harmony.PatchAll()`.

---

## Extension points for third-party plugins

A third-party plugin (e.g., a new-game-mode mod) could depend on this architecture:

```csharp
[BepInPlugin("my.awesome.mod", "Awesome Mod", "1.0.0")]
[BepInDependency(LokrModAPIPlugin.Guid)]          // for asset loading, config
[BepInDependency(LokrCharacterLoaderPlugin.Guid)] // for CharacterAPI hooks
public class AwesomeModPlugin : BaseUnityPlugin
{
    private void Awake()
    {
        Logger.LogInfo("Awesome Mod loaded!");
        
        // Use ModAPI services
        if (ModAPI.Files.TryFindFile("MyContent", "data.json", out string path))
            Logger.LogInfo($"Found data at {path}");
        
        // Extend character content without re-patching base game
        CharacterAPI.RegisterPortraitResolver((heroId, slot) => 
        {
            // Return custom portrait or null to fall through
        }, priority: 100);  // higher priority = tried first
        
        CharacterAPI.BuildingHeroRoster += roster =>
        {
            roster.AddLegend(JsonConvert.DeserializeObject<Legend>(heroJson));
        };
    }
}
```

This plugin would be loaded after `LokrCharacterLoader` but before (or alongside) any other character-extending mods — guaranteed by BepInEx's `[BepInDependency]` ordering.

---

## Design decisions and trade-offs

### Q: Why are there so many little plugins instead of one monolithic DLL?

**A:** Separation of concerns and independent iteration.
- `LokrLab` can be rebuilt aggressively for editor polish without touching the stable `LokrCharacterLoader` patch set.
- `SimpleUI` can be evolved as a general UI library without tying it to character content.
- `LokrEncyclopedia` is truly independent — a player could uninstall it without affecting characters.
- Future mods can depend on just the pieces they need (e.g., a new map mod might use `LokrModAPI` + `SimpleUI` but not `LokrCharacterLoader`).

### Q: Why resolver chains instead of an event system for portraits/sounds?

**A:** Different semantics.
- Resolver chains: **first non-null result wins**. Good for "where do I load the portrait from?" — you want one canonical source.
- Events: **all subscribers' results merged**. Good for "what should go in the hero roster?" — you want all mods' heroes combined.

Using the right tool for each job makes the extension point's behavior obvious to the caller.

### Q: Why full-method-replacement patches for so much of `LokrCharacterLoader`?

**A:** The base-game methods are too tightly coupled with private state to safely prefix/postfix.
- Example: `HeroRosterManager.LoadHeroes()` uses local variables and private fields in ways a narrow prefix/postfix can't intercept without reimplementing the whole thing anyway.
- Tradeoff: requires re-reading decompiled source and carefully tracking when the game updates, but gains full control and clarity about what's happening.

### Q: Why is `SimpleUI` a full plugin instead of just a DLL?

**A:** Because BepInEx requires all plugin dependencies to be BepInEx plugins (or assemblies referenced with `Private="false"` in the .csproj). Making `SimpleUI` a plugin ensures `LokrLab` can reference it via `[BepInDependency]` and trust it's loaded first. The same reason `LokrLabApi` is a plugin.

---

See the individual plugin docs for more:
- [`LokrModAPI/docs/`](../LokrModAPI/docs/)
- [`LokrCharacterLoader/docs/`](../LokrCharacterLoader/docs/)
- [`LokrModMenu/docs/`](../LokrModMenu/docs/)
- [`LokrLabApi/docs/`](../LokrLabApi/docs/)
- [`LokrLab/docs/`](../LokrLab/docs/)
- [`LokrLab/docs/character/`](../LokrLab/docs/character/)
- [`LokrLab/docs/ability/`](../LokrLab/docs/ability/)
- [`SimpleUI/docs/`](../SimpleUI/docs/)
- [`LokrEncyclopedia/docs/`](../LokrEncyclopedia/docs/)
- [`LokrPatch/docs/`](../LokrPatch/docs/)

High-level plans and assessments:
- [`modapi-plan.md`](modapi-plan.md) — the original architecture proposal (still current)
- [`capabilities-and-gaps.md`](capabilities-and-gaps.md) — what works, what doesn't, next priorities
