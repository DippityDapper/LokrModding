# ~/dev/lokr-modding/bepinex — LokrModding BepinEx Plugin Solution

## Overview

**Status**: Production-ready, fully documented

This is the **active modding platform** for Legends of Kingdom Rush, built as a modular BepInEx plugin architecture. Instead of recompiling the game DLL (like the original mod), this uses Harmony to patch the original unmodified game at runtime.

**Platform**: Developed and run on Linux. The game itself is Windows-only and runs
through Steam Proton; BepInEx (via Doorstop) hooks into it the same way it would on
Windows, and the install directory is a normal native filesystem path (no `compatdata`/
wine-prefix path juggling needed — see "Linux/Proton notes" below).

**Consists of**:
- 8 BepInEx plugins (including SimpleUI and LokrLabApi) + the LokrLab suite + complete documentation
- Complete documentation (architecture, design decisions, extension points)
- Ready for community mod development

## Key Projects

### Main Plugins

| Plugin | Responsibility | Status |
|--------|---|---|
| **LokrModAPI** | Shared foundation (assets, config, audio, file system) | Stable |
| **LokrCharacterLoader** | Runtime character/content system + CharacterAPI | Stable (1.1.17) |
| **LokrLabApi** | Editor contracts (project types, session, Host, menus) | Stable (1.5.3) |
| **LokrLab** | Editor suite: host + Character + Ability + Encounter (scene, Project Browser, docks) | 0.12.102 |
| **LokrEncyclopedia** | Encyclopedia button unlock | Stable |
| **LokrModMenu** | Global mod menu popup (BackQuote `` ` `` primary; optional F3) | Stable |
| **LokrPatch** | Base-game bug fixes (duplicate skills, missing modifiers, metagame ctor, save/ability guards, progression-help clamp, party slot stow) | Stable (1.0.10) |
| **SimpleUI** | Reusable UI widget library + docking primitives | Stable (1.2.11) |

### Solution File

- `LokrModding.sln` — Open this in JetBrains Rider (or VS Code with the C# Dev Kit) to work on plugins; build with the `dotnet` CLI (see "Building Plugins" below)

## How to Navigate

### First Time Here?
1. **Read**: `docs/README.md` — Documentation index
2. **Read**: `docs/ARCHITECTURE.md` — System design, dependency graph, patterns
3. **Pick a plugin** and read its `docs/overview.md`
4. **Dive into code** as needed

### Working on a Specific Plugin?
1. Open `LokrModding.sln`
2. Read `<PluginName>/docs/overview.md` and `architecture.md`
3. Check `<PluginName>/docs/cross-references.md` for base-game dependencies
4. Code against the public API (documented in `classes.md`)

### Adding Character Content?
1. Read `LokrCharacterLoader/docs/overview.md` and `character-api.md`
2. Create a new BepInEx plugin in `../bepinex/` (or outside this folder)
3. Declare `[BepInDependency(LokrCharacterLoaderPlugin.Guid)]`
4. Register your content via `CharacterAPI` (don't re-patch the same methods)

### Building UI in a Plugin?
1. Reference `SimpleUI.dll`
2. Read `SimpleUI/docs/overview.md` and `classes.md`
3. Use `UiPanel`, `UiStack`, `UiButton`, etc. for responsive layouts
4. See `LokrLab/docs/architecture.md` for an example of combining SimpleUI + hand-built panels

## Documentation Structure

Every plugin folder has a `docs/` directory with consistent sections:

- **`overview.md`** — What is this plugin, what does it do, why does it exist?
- **`architecture.md`** — Major design decisions, patterns, state machine, initialization order
- **`layout.md`** — File/folder structure and namespace organization
- **`classes.md`** or **`supporting-classes.md`** — Every public class, method, and property
- **`conventions.md`** — Naming patterns, coding style, design patterns used
- **`cross-references.md`** — Base-game classes/methods this plugin depends on, neighboring plugin dependencies

**Top-level docs** (in `docs/` folder):
- **`README.md`** — Navigation hub, quick-start guide
- **`ARCHITECTURE.md`** — System-wide design
- **`modapi-plan.md`** — Original architecture proposal
- **`roadmaps/`** — Planning docs grouped by status (`completed/`, `started/`, `not-started/`)
- **`capabilities-and-gaps.md`** — What works, what doesn't
- **`mods-folder-structure.md`** — On-disk layout
- **`git-and-releases.md`** — Repo layout (hub + submodules), fresh-clone setup, `scripts/release-plugin.sh`
- **`issues/`** — Project issues (`unresolved/`, `unresolved-tested/`,
 `resolved/`); process in `docs/issues/README.md`. Do not move an issue
 to `resolved/` until the fix is confirmed in the running game. A
 passing unit test moves the file to `unresolved-tested/` only
 ([`docs/roadmaps/completed/test-suite.md`](docs/roadmaps/completed/test-suite.md)).

**Documentation style:** Do not use emojis in docs or code. Use plain text for status
labels (`done`, `partial`, `todo`, `deferred`) and markdown checkboxes (`[ ]`) where
needed.

## Key Architectural Patterns

All documented in `docs/ARCHITECTURE.md`. Quick reference:

### 1. Resolver Chains
```csharp
// Higher priority tried first; first non-null result wins
CharacterAPI.RegisterPortraitResolver((heroId, slot) => LoadPortrait(...), priority: 100);
```
**Used by**: Portraits (6 slots), sounds, rigs, sprite FXMega / projectiles  
**Why**: "Where do I load the portrait from?" — you want one canonical source

### 2. Events
```csharp
// Fired at hook points; all subscribers' results merged
CharacterAPI.BuildingHeroRoster += roster => roster.AddLegend(...);
```
**Used by**: Roster, unit definitions, abilities, localization, scripts  
**Why**: "What should go in the hero roster?" — you want all mods' content combined

### 3. Service Facades
```csharp
// Static, initialized once, read-only thereafter
ModAPI.Files.TryFindFile("Portraits", path, out fullPath);
ModAPI.Assets.LoadSprite(fullPath);
```
**Used by**: LokrModAPI (ModAPI), UI theming  
**Why**: Single global instance, predictable initialization

### 4. Full-Method-Replacement Patches
```csharp
[HarmonyPrefix]
private static bool Prefix(/* original params */) { /* full reimplementation */ return false; }
```
**Used by**: LokrCharacterLoader (most patches)  
**Why**: Base-game methods are too tightly coupled to patch with Prefix/Postfix alone

## Important Files

### Build & Solution
- `LokrModding.sln` — Open this in JetBrains Rider or VS Code
- `Directory.Build.props` — Shared build configuration for all projects, including `GameDir` (the Steam install path each project's game-DLL `HintPath`s and the post-build deploy step resolve against)
- `Directory.Build.targets` — Shared build targets

### Documentation
- `docs/README.md` — Start here
- `docs/ARCHITECTURE.md` — System design
- `docs/ARCHITECTURE.md` → "Plugin extension points" for third-party mods
- `<PluginName>/docs/` — Per-plugin documentation

### Plugins (alphabetical)
- `LokrCharacterLoader/` — Character/content system
  - Public API: `CharacterAPI.cs`
  - Patches: `Patches/` folder
  - Docs: See `docs/character-api.md` for extension points
  
- `LokrLabApi/` — Editor contracts (project types, session, menus)
  - Public facade: `LokrLabApi.cs`
  - Docs: See `docs/overview.md`

- `LokrLab/` — Editor suite (host + Character + Ability + Encounter)
 - Entry: title-screen Mods button + `LokrModMenu` (BackQuote default)
 - Core: `CharacterLabScene.cs` (scene transition + Host) + Project Browser / dockable shell
 - Modules: `Character/` (Properties / Animator / Sandbox), `Ability/` (library + overlay fallback), `Encounter/` (Setup + Play; combatants + placement)
 - Writes `Mods/LokrLab/LokrCharacterLab/<id>/`, `Mods/LokrLab/LokrAbilityLab/<libraryId>/`, and `Mods/LokrLab/LokrEncounterLab/<id>/`
 - Docs: See `docs/architecture.md`; Character/Ability/Encounter chapters under `docs/character/`, `docs/ability/`, and `docs/encounter/`
  
- `LokrModMenu/` — Global mod menu
  - Entry: `LokrModMenuPlugin.cs`, `ModMenuAPI.cs`
  - Hotkey: BackQuote (`` ` ``) primary; optional bare F3
  
- `LokrEncyclopedia/` — Encyclopedia unlock
  - One patch: `Patches/UIMainMenuPatches.cs`
  - Smallest plugin (demonstrates modularity)

- `LokrPatch/` — Base-game bug fixes
  - No plugin dependencies; patches vanilla crash/error paths
  - Docs: [`docs/overview.md`](LokrPatch/docs/overview.md) — scope and when to add fixes here
  
- `LokrModAPI/` — Foundation
  - Public facade: `ModAPI.cs`
  - Sub-services: `Files/`, `Assets/`, `Audio/`, `Config/`, `ExtensionData/`
  - Patch: `Patches/SplashVideoControllerPatches.cs`
  
- `SimpleUI/` — UI library
  - Public classes: `Ui*.cs` (UiPanel, UiButton, UiStack, etc.)
  - Theme: `UiTheme.cs`
  - No Harmony patches (pure library)

## Development Workflows

### Building Plugins
```bash
# Build all
dotnet build LokrModding.sln

# Build specific project
dotnet build LokrCharacterLoader/LokrCharacterLoader.csproj

# Clean before rebuild
dotnet clean LokrModding.sln

# Unit tests (Unity-free helpers; not an in-game confirm)
dotnet test LokrModding.sln
```

### Cutting a release

```bash
scripts/release-plugin.sh <PluginName>
```

Builds that plugin in Release config, packages the deploy layout into
`dist/<PluginName>-v<version>.zip`, and publishes a GitHub Release on that
plugin's own repo. See [`docs/git-and-releases.md`](docs/git-and-releases.md)
for the repo layout (this solution is 10 private repos — a hub with 9
submodules, one per plugin plus `docs/api`) and full release-script details.

### Running/Testing
1. `dotnet build` already deploys each plugin's DLL/PDB straight into
   `$(GameDir)/BepInEx/plugins/<AssemblyName>/` (the `DeployToBepInEx` target in
   `Directory.Build.targets`) — no manual copy step needed.
2. Launch the game through Steam.
3. Check `$(GameDir)/BepInEx/LogOutput.log` for load order + any errors.
4. Test with the installed community pack (use reference implementation if available).

### Linux/Proton notes
- `GameDir` in `Directory.Build.props` points at the native Steam library path, e.g.
  `~/.local/share/Steam/steamapps/common/Legends of Kingdom Rush/`. BepInEx/Doorstop
  hooks the game the same way it does on Windows; nothing here needs a wine-prefix
  (`compatdata`) path — the game's own install directory is a plain Linux path.
- `LogOutput.log` lands directly under `$(GameDir)/BepInEx/`, same as on Windows.
- MSBuild on Linux normalizes the backslash-separated `HintPath`/deploy paths in
  `Directory.Build.props`/`.targets` fine — they don't need to be rewritten to forward
  slashes.

### Updating Documentation
1. Every code change that affects architecture should update relevant `docs/` files
2. Use consistent section names across plugins (overview → architecture → classes → conventions → cross-references)
3. Include examples and cross-links
4. When you change one plugin's API, update references in other plugins' `cross-references.md`
5. When you change or add a `/// <summary>`/`<remarks>` doc comment, rebake the generated API docs: `cd docs/api && python3 generate_docs.py --sync-descriptions` (see [`docs/code-documentation-standards.md`](docs/code-documentation-standards.md)). Adding a brand-new class also needs a `docs/api/classes.json` entry before a plain `generate_docs.py` run will create its page.

### Adding a New Feature
1. **Decide where it belongs**: Is it infrastructure (`LokrModAPI`), character content (`LokrCharacterLoader`), editor contracts (`LokrLabApi`), editor shell (`LokrLab`), or UI (`SimpleUI`)?
2. **Declare its API**: How will other plugins use this feature? (resolver chain, event, static method?)
3. **Implement**: Write code + Harmony patches (if needed)
4. **Document**: Add/update `docs/` files in that plugin
5. **Test**: Verify with existing mods if possible

### Extending from Outside
Third-party plugins should:
1. Reference the `.dll` files they need (e.g., `LokrCharacterLoader.dll` for `CharacterAPI`)
2. Declare `[BepInDependency(PluginGuid)]` for load ordering
3. Use published APIs only (documented in plugin docs)
4. **NOT** patch the same base-game methods (use extension points instead)

See `docs/ARCHITECTURE.md` → "Extension points for third-party plugins" for a code example.

## Known Limitations & Future Work

Actionable bugs live in `docs/issues/`. Platform limits and future work
are in `docs/capabilities-and-gaps.md`:

**What works:**
- Character re-skinning (texture/portrait replacement)
- Hero roster injection (new playable heroes/companions)
- Custom rigs and animations (`CustomRigLoader` + Character Lab)
- Ability injection (new spells/abilities with full scripting)
- Live reload (`CharacterAPI.ReloadLabContent`) from Character Lab
- Sound replacement, localization, Lua script override

**What doesn't (or is partial):**
- Ability VFX overrides (asset-bundle ceiling)
- Roster card art overrides (same ceiling)
- MAP/MAPMINI/CHALLENGE portraits (flat-image workaround)

**Unverified:**
- Encyclopedia button click — [`docs/issues/resolved/encyclopedia-button-unverified-click.md`](docs/issues/resolved/encyclopedia-button-unverified-click.md) (vanilla Coming Soon)
- Save-game compatibility when mods are uninstalled — run is no longer discarded (Sanitize/party stow). Party slot holes confirmed in-game: [`docs/issues/resolved/party-stow-shifts-remaining-into-wrong-slots.md`](docs/issues/resolved/party-stow-shifts-remaining-into-wrong-slots.md). See also [`docs/issues/unresolved-tested/save-sanitize-drops-unknown-ids.md`](docs/issues/unresolved-tested/save-sanitize-drops-unknown-ids.md) and [`docs/issues/unresolved-tested/save-party-reset-to-vanilla-trio.md`](docs/issues/unresolved-tested/save-party-reset-to-vanilla-trio.md)
- Achievement system integration (same)

**Recommended priority**: See `docs/capabilities-and-gaps.md` § 3.

## Conventions

### Plugin Naming
- GUID: `com.lokrmodding.<name>` (all lowercase, no underscores)
- Class: `<Name>Plugin` (e.g., `LokrCharacterLoaderPlugin`)
- Namespace: `LokrCharacterLoader` (matches folder)

### Code Organization
- `<PluginName>.csproj` — Project file
- `<PluginName>Plugin.cs` — BepInEx entry point
- `Patches/` — Harmony patch classes
- `docs/` — Always maintain documentation

### Patch Naming
- Class: `<TargetClassName>Patches` or `<TargetClassName>_<MethodName>_Patch`
- Files: One patch class per file (or group related patches in one file if small)
- Documentation: Always document WHY the patch exists, not just WHAT it does — as a `<remarks>` on the patch class/method, per "Code Documentation Standards" below, never a plain `//` comment

### Code Documentation Standards

**Full standard**: [`docs/code-documentation-standards.md`](docs/code-documentation-standards.md) — read this before writing or reviewing any C# in this solution.

The short version: every `public`/`internal` member gets a `/// <summary>`;
nothing gets a plain `//` comment; use `<remarks>` for the non-obvious
*why* instead. Keep both short — one sentence for `<summary>`, only as much
as the problem actually needs for `<remarks>`. Generated class pages split
members into a **Public API** section and an **Internal** section (this
codebase's real, deliberate surface is overwhelmingly `internal`, not
`public`, so both are documented — not just `public`). Whenever a doc
comment changes, rebake the generated HTML docs:
```bash
cd docs/api && python3 generate_docs.py --sync-descriptions
```
See the standards doc for why `docs/api/classes.json` being a *curated*
manifest (not auto-discovered) means new classes need a manifest entry
too. Coverage is complete as of the 2026-08-11 backfill; new members
need a comment on the same commit, not a later pass.

### Documentation
- Write for the reader 2 months from now (yourself included)
- Include code examples, not just descriptions
- Document trade-offs and "why this instead of that"
- Link liberally between related docs

## Debugging Tips

### Plugin not loading?
1. Check `BepInEx/LogOutput.log` for load errors
2. Verify `[BepInPlugin]` attributes are correct
3. Verify `[BepInDependency]` for all dependencies
4. Ensure DLL is in `BepInEx/plugins/`

### Patch not firing?
1. Verify target class/method name (use decompiled source as reference)
2. Check method signature matches (parameters, return type)
3. Look for `OnSceneLoaded` guards (some methods only exist in certain scenes)
4. Verify Harmony prefix/postfix/transpiler syntax

### Content not loading?
1. Check mod folder structure (should match `docs/mods-folder-structure.md`)
2. Verify `ModAPI.Files.TryFindFile()` finds the file (add debug logging)
3. Check file extension and format (`.txt` for KV files, `.png` for images, `.wav` for audio)
4. Look for load-order issues (does dependency exist before trying to use it?)

## Quick Links

| | |
|---|---|
| **Start here** | `docs/README.md` |
| **System architecture** | `docs/ARCHITECTURE.md` |
| **Extending with new content** | `LokrCharacterLoader/docs/character-api.md` |
| **Building UI** | `SimpleUI/docs/overview.md` |
| **Base-game reference** | Any plugin's `docs/cross-references.md` |
| **Capabilities & gaps** | `docs/capabilities-and-gaps.md` |
| **Code documentation standards** | `docs/code-documentation-standards.md` |
| **Git repo layout & releases** | `docs/git-and-releases.md` |
| **Original mod analysis** | `../lokr-modding/docs/README.md` (historical reference) |

## Project Status & Stability

| Component | Status | Notes |
|-----------|--------|-------|
| LokrModAPI | Stable | Foundation; changes rarely |
| LokrCharacterLoader | Stable (1.1.17) | Core system; CharacterAPI is frozen public API; unit-def / roster last-wins (vanilla override); Lab scan dedupes by folder name |
| LokrLabApi | Stable (1.5.3) | Editor contracts; Host + StartEmbeddedScene / StartEmbeddedFight; `EncounterTypeId`; persistent `Scrollable` |
| **LokrLab** | Evolving (0.12.110) | Suite: host + Character + Ability + Encounter; File → Edit Vanilla Hero (slug_token folder + Model-prefab combat rig); Close Lab flushes Description loc and always reloads content; Sandbox Stop restores camera ortho; File → Save / Ctrl+S / dirty `*` / close prompt; Animator feel; Ability overhaul; hover coverage; Encounter visual catalogues + camera bounds; Setup auto-load + Restart board + catalogue drag-to-place; prop snap or free-move. [Encounter Creator](docs/roadmaps/started/encounter-creator.md) Phases 1-16b all confirmed in-game as of 2026-08-16. Phase 17: one Sandbox workspace; Sandbox Level is always 1-3 |
| SimpleUI | Stable (1.2.11) | UI library + docking + UiFileBrowser + UiCatalogue (scroll batches + drag-out drop + cursor ghost) |
| LokrEncyclopedia | Stable | One patch; no active changes |
| LokrModMenu | Stable (1.1.1) | BackQuote (`` ` ``) opens mod menu; blocked on loading screens |
| LokrPatch | Stable (1.0.11) | Base-game bug fixes; dependency-free; party slot stow; progression-help clamp; EventSystem LateUpdate / achievement NRE guards |
| Documentation | Comprehensive | Update when architecture changes |

Breaking changes to public APIs (`CharacterAPI`, `ModAPI`, plugin GUIDs) will be documented in release notes. Patch changes are internal and don't break compatibility.

---

**Last updated**: 2026-08-17 (Sandbox Stop restores camera ortho 0.12.110)
