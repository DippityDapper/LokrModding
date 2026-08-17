# ModAPI & Modular Plugin Architecture — Plan

## 1. Objective

Split the current monolithic `LokrMods` plugin into four pieces:

1. **`LokrModAPI`** — a foundational BepInEx plugin *and* referenceable
   library (`LokrModAPI.dll`) that owns the generic, reusable modding
   infrastructure (mod-folder discovery, asset loading, sound playback,
   config, extension-data helper, and the one truly game-wide hook)
   behind a stable, documented `ModAPI` class. Other LoKR mod developers
   reference this DLL and build their own BepInEx plugins against it, the
   same way any BepInEx plugin references `BepInEx.dll`.
2. **`LokrCharacterLoader`** — everything about *creating a playable
   character* (heroes, companions, abilities, portraits, exoskeleton
   skins, sounds, localization, Lua scripts). Depends on `LokrModAPI`,
   built using its primitives, and additionally exposes **its own**
   extension API (`CharacterAPI`, §5) so *other* plugins can hook into
   the character-creation pipeline without re-patching the same base-game
   methods `LokrCharacterLoader` already patches.
3. **`LokrEncyclopedia`** — the main-menu Encyclopedia button unlock,
   split out as its own small plugin. Depends only on `LokrModAPI`, not
   on `LokrCharacterLoader` — it has nothing to do with characters.
4. *(retired)* `LokrMods` — deleted once the three plugins above reach
   parity with it (§8).

`ModManager` (today's ad hoc, do-everything class) goes away entirely —
its responsibilities move into `ModAPI`, decomposed into focused pieces
and exposed as an actual public API. `properties.txt` also goes away,
replaced by BepInEx's native config system (§4.4).

## 2. Why split it this way

Looking at what the 20 current patch files actually depend on:

- **Everything** duplicates the same three things: "loop over
  `ModManager.GetModsFolderPath()`, build a path, check `File.Exists`"
  (~15 near-identical occurrences); "`Texture2D` + `LoadImage` +
  `Sprite.Create`" (~8 occurrences); "`ConditionalWeakTable` side-table
  because Harmony can't add a field to a type it doesn't own" (the
  ExoSkeleton hero-ID plumbing). None of that is specific to *characters*
  — any future LoKR mod (new maps, new game rules, a UI overhaul) would
  need the same primitives. That's the `ModAPI` layer.
- **Almost everything else** — the hero-roster JSON splicing, the
  `_CHALLENGE.png`/`_MAP.png`/`_MAPMINI.png` naming conventions, the
  ability KV-file merging, the Assassin invisibility visual — *is*
  character/content domain knowledge. That's `LokrCharacterLoader`.
- **The Encyclopedia button** isn't infrastructure and isn't character
  content — it's an unrelated main-menu unlock that happened to ship in
  the same DLL as everything else purely because the original mod was
  one file. Splitting it out costs one small plugin and removes a
  permanent "why is this here" from `LokrCharacterLoader`.
- The **one** patch that's neither generic infrastructure nor character
  content nor Encyclopedia-related is the boot-time skip-splash hook
  (`SplashVideoController.Awake`). It stays in `LokrModAPI` since
  `ModAPI.Config` (which it reads `SkipSplashScreen` from) already lives
  there, and every plugin needs `LokrModAPI` loaded first regardless.

## 3. Project layout

```
bepinex/
├── LokrModding.sln
├── LokrModAPI/
│   ├── LokrModAPI.csproj
│   ├── LokrModAPIPlugin.cs        ← BaseUnityPlugin entry point
│   ├── ModAPI.cs                  ← public static facade (§4)
│   ├── Files/
│   │   └── ModFileSystem.cs       ← was ModManager.GetModsFolderPath + the ~15 duplicated scan loops
│   ├── Assets/
│   │   └── ModAssetLoader.cs      ← LoadSprite/LoadTexture, LoadAudioClip
│   ├── Audio/
│   │   ├── ModAudioService.cs     ← was ModManager.PlaySound / ModdedSound
│   │   └── OpenWavParser.cs       ← moved here verbatim, unchanged
│   ├── Config/
│   │   └── ModConfig.cs           ← BepInEx ConfigFile-backed, replaces properties.txt (§4.4)
│   ├── ExtensionData/
│   │   └── AttachedData.cs        ← generic ConditionalWeakTable helper (§4.5)
│   └── Patches/
│       └── SplashVideoControllerPatches.cs
│
├── LokrCharacterLoader/
│   ├── LokrCharacterLoader.csproj    ← references LokrModAPI.dll + game assemblies
│   ├── LokrCharacterLoaderPlugin.cs  ← [BepInDependency(LokrModAPIPlugin.Guid)]
│   ├── CharacterAPI.cs                ← public extension-point facade for OTHER plugins (§5)
│   └── Patches/
│       ├── HeroRosterManagerPatches.cs
│       ├── UnityDefinitionsParserPatches.cs
│       ├── AbilitiesDefinitionsPatches.cs
│       ├── PortraitPatches.cs             ← was DataHelperPatches + MapHeroBarPortraitComponentPatches + the ×3 challenge-portrait patches, unified (§5.3)
│       ├── ExoSkeletonDataPatches.cs
│       ├── ExoSkeletonRendererPatches.cs
│       ├── ExoSkeletonUIGraphicPatches.cs
│       ├── PartyTokenComponentPatches.cs
│       ├── SoundPatches.cs                ← was UnitPatches(sound) + UIHeroManagePatches + UIHeroRoomPatches, unified (§5.4)
│       ├── InvisibilityPatches.cs         ← Assassin invisibility, rebuilt on the state-effect hook (§5.6)
│       ├── LocalizationManagerPatches.cs
│       └── IronhideScriptLoaderPatches.cs
│
└── LokrEncyclopedia/
    ├── LokrEncyclopedia.csproj     ← references LokrModAPI.dll only
    ├── LokrEncyclopediaPlugin.cs   ← [BepInDependency(LokrModAPIPlugin.Guid)]
    └── Patches/
        └── UIMainMenuPatches.cs
```

`LokrMods/` (today's monolithic project) is retired once the three new
plugins reach parity with it — not kept around as a compat shim, since
nothing else depends on its GUID yet (§8).

## 4. The `ModAPI` surface

A single static facade class, `LokrModAPI.ModAPI`, with small namespaced
sub-APIs underneath it — mirrors how `UnityEngine.Application` or
`BepInEx.Paths` read from calling code (`ModAPI.Files.GetModFolders()`,
`ModAPI.Assets.LoadSprite(path)`, etc.).

### 4.1 `ModAPI.Files` — mod discovery & file resolution

```csharp
IReadOnlyList<string> GetModFolders();
bool TryFindFile(string category, string relativePath, out string fullPath);
IEnumerable<(string modFolder, string filePath)> EnumerateCategoryFiles(string category, string searchPattern = "*");
bool TryFindSoundFile(string unitId, string eventNameSubstring, out string modFolder, out string filePath);
```

Unchanged from the previous pass of this plan — see there for the
per-method rationale. Replaces `ModManager.GetModsFolderPath()` and
every duplicated scan-loop.

### 4.2 `ModAPI.Assets` — asset loading

```csharp
Sprite LoadSprite(string path, TextureFormat format = TextureFormat.ARGB32);
Texture2D LoadTexture(string path, TextureFormat format = TextureFormat.ARGB32);
AudioClip LoadAudioClip(string path);   // wraps OpenWavParser
```

Collapses the `byte[] → Texture2D → LoadImage → Sprite.Create` block
copy-pasted ~8 times today.

### 4.3 `ModAPI.Audio` — sound playback

```csharp
void PlaySound(string eventName, string unitId, string modFolder);
```

Direct port of `ModManager.PlaySound` / `ModdedSound`, driven every
frame from `LokrModAPIPlugin.Update()`.

### 4.4 `ModAPI.Config` — now backed by BepInEx's native config system

**Changed from the previous pass**: `properties.txt` is retired.
`ModAPI.Config` is now three real `ConfigEntry<bool>`s bound through
`LokrModAPIPlugin`'s own `BaseUnityPlugin.Config` (BepInEx auto-creates
`BepInEx/config/com.lokrmodding.lokrmodapi.cfg`):

```csharp
// LokrModAPIPlugin.Awake()
DebugMode = Config.Bind("General", "DebugMode", false,
    "Enables debug logging and the in-game debug panel.");
SkipSplashScreen = Config.Bind("General", "SkipSplashScreen", false,
    "Skip the intro splash video and go straight to the main menu.");
TakeOverAI = Config.Bind("General", "TakeOverAI", false,
    "Enables the built-in fight-tester AI take-over cheat.");
```

exposed read-only through `ModAPI.Config.DebugMode` /
`.SkipSplashScreen` / `.TakeOverAI` (all `ConfigEntry<bool>`, so callers
get `.Value` plus BepInEx's change-notification event for free if they
want to react live).

**What this buys over the hand-rolled parser:**
- Players get BepInEx's standard config UX — hand-edit the generated
  `.cfg` file (with auto-written comments/defaults), or use
  [ConfigurationManager](https://github.com/BepInEx/BepInEx.ConfigurationManager)
  for an in-game F1 settings menu, for free, no code on our side.
- Per-entry descriptions, typed values, and change notification instead
  of a flat string dictionary.
- One less hand-rolled parser (`ModManager.LoadProperties`) and one less
  thing that can throw `FileNotFoundException` at boot if a file is
  missing — `Config.Bind` always succeeds and just uses the default.

**One-time migration from the existing `properties.txt`:** the official
community pack already ships `Mods/Resources/properties.txt`, so a clean
cut would silently reset those three settings to their defaults for
anyone upgrading. `LokrModAPIPlugin.Awake()` checks, *before* calling
`Config.Bind`, whether its own `.cfg` file doesn't exist yet (first run
for this plugin) **and** `Mods/Resources/properties.txt` does exist —
if so, it parses the old file (reusing `ModManager.LoadProperties`'s
tiny `key = value` parser one last time, purely as a one-shot importer)
and passes those values in as the `Config.Bind` defaults instead of the
hardcoded `false`s. `properties.txt` itself is left on disk untouched
(not deleted — see the standing rule about not being destructive with
files we didn't create); it's simply never read again after that first
run. Worth a one-line log message on import (`"Imported settings from
properties.txt — edit BepInEx/config/com.lokrmodding.lokrmodapi.cfg from
now on"`) so it's not a silent, surprising switch.

`Mods/` folder *content* layout (`Portraits/`, `Sounds/`, `HeroRoster/`,
etc.) is unrelated to this and is completely unaffected — only the one
global `properties.txt` file moves.

### 4.5 `ModAPI.ExtensionData<TKey, TValue>` — the "add a field to a type
you don't own" helper

```csharp
public sealed class AttachedData<TKey, TValue> where TKey : class
{
    public bool TryGet(TKey key, out TValue value);
    public void Set(TKey key, TValue value);          // add-or-replace
    public TValue GetOrAdd(TKey key, Func<TValue> factory);
}
```

Generalizes the `ConditionalWeakTable`-based side-table pattern from
today's `ExoSkeletonModData`. `LokrCharacterLoader`'s exoskeleton
patches become two instances of this instead of hand-rolled
`ConditionalWeakTable` boilerplate.

### 4.6 Logging

Unchanged: each plugin uses its own BepInEx-provided `Logger`, no shared
logging service.

## 5. `CharacterAPI` — letting other plugins extend character creation
without repatching

This is the direct answer to *"for other plugins trying to patch methods
that are patched already, make the patches good enough that plugins have
a lot of control over these systems without needing to patch/repatch
them."*

`LokrCharacterLoader` already owns the only Harmony patches on
`HeroRosterManager`, `UnityDefinitionsParser`, `AbilitiesDefinitions`,
`DataHelper`/portrait components, `Unit`/sound components,
`LocalizationManager`, and `IronhideScriptLoader`. Instead of a second
plugin needing to *also* patch one of those same methods (which Harmony
would allow but gives no control over ordering, conflicts, or
interaction with `LokrCharacterLoader`'s own logic), `LokrCharacterLoader`
exposes a second public facade — `LokrCharacterLoader.CharacterAPI` —
with a **resolver-chain** and **event** at each meaningful extension
point. A plugin that wants to add or override character content
references `LokrModAPI.dll` *and* `LokrCharacterLoader.dll`, declares
`[BepInDependency]` on both GUIDs, and calls into `CharacterAPI` instead
of Harmony.

**Design note that shrank the API surface:** the six separate
`_MINI`/`_BIG`/`_BANNER`/`_MAP`/`_MAPMINI`/`_CHALLENGE` portrait hooks
and the three separate sound hooks (`Unit.PlaySound`,
`UIHeroManage.PromoteHero`, `UIHeroRoom.PlayHeroSelectedSound`) are all,
structurally, the same operation with a different "slot"/event name.
Rather than expose six portrait-resolver methods and three
sound-resolver methods, `CharacterAPI` exposes **one** of each,
parameterized by slot/event name — which also collapses the
corresponding patch files in `LokrCharacterLoader` itself (§3: six
files become `PortraitPatches.cs`, three become `SoundPatches.cs`).

### 5.1 Resolver-chain pattern (used throughout)

```csharp
public delegate Sprite PortraitResolver(string heroId, string slot);

// Higher priority tried first; first non-null result wins.
// Ties broken by registration order (first-registered tried first).
void RegisterPortraitResolver(PortraitResolver resolver, int priority = 0);
```

`LokrCharacterLoader` registers its **own** file-convention lookup
(today's `Mods/*/Portraits/<heroId>/<heroId>_<slot>.png` scan) as an
ordinary resolver at `priority: 0` during its own `Awake()` — it does
not get special treatment. A plugin wanting to *override* file-based
portraits (e.g. procedurally-generated ones) registers at a higher
priority; a plugin wanting to provide a *fallback* registers lower.
This means the existing community pack keeps working unmodified (its
resolver is just the first one in the chain), while code-based plugins
get equal footing through the same mechanism instead of a separate,
special-cased path.

### 5.2 Character/roster/ability content

```csharp
// Fired once, while the roster TextAsset is being assembled, before parsing.
// RosterBuilder exposes AddLegend(json) / AddCompanion(json) so plugins don't
// hand-splice JSON strings themselves (today's ReplaceTextAssetWithModFile-
// style raw-string surgery stays an internal implementation detail).
event Action<RosterBuilder> BuildingHeroRoster;

// Same idea for RLHeroes/EnemiesDefinitions unit-stat text before it's parsed.
event Action<UnitDefinitionsBuilder> BuildingUnitDefinitions;

// Fired once per parsed UnitDefinition, after parsing — for programmatic
// tweaks that don't want to hand-write KV text (balance patches, etc.).
event Action<UnitDefinition> UnitDefinitionLoaded;

// Ability content: file-based (today's NewAbilities/*.txt) keeps working via
// LokrCharacterLoader's own registered contributor; plugins can add more KV
// text the same way, or register parsed Ability objects directly.
event Action<AbilitiesBuilder> BuildingAbilities;
void RegisterAbility(Ability ability);
```

`LokrCharacterLoader`'s `HeroRosterManagerPatches`,
`UnityDefinitionsParserPatches`, and `AbilitiesDefinitionsPatches` fire
these events at the appropriate point in their existing (already-written)
full-method-replacement patches, and register their own file-convention
scanning as the default subscriber — same "dogfood the extension point"
principle as §5.1.

The duplicate-key tolerance already built into `UnityDefinitionsParser.ParseText`
and `AbilitiesDefinitions.Load` (§3/§4 of an earlier planning pass for this
document) means a later
subscriber can override an earlier one's unit/ability by reusing its ID
— that behavior extends naturally to `UnitDefinitionLoaded` subscribers
and `RegisterAbility` callers too, no extra work needed.

### 5.3 Portraits (replaces `PortraitPatches.cs`'s six old patch files)

```csharp
void RegisterPortraitResolver(PortraitResolver resolver, int priority = 0);
// slot ∈ "MINI" | "BIG" | "BANNER" | "MAP" | "MAPMINI" | "CHALLENGE"
```

One resolver chain, reused by `DataHelper` (MINI/BIG/BANNER),
`MapHeroBarPortraitComponent` (MAP), `ExoSkeletonData.ReplacePart`
(MAPMINI), and the three challenge-portrait call sites
(`DialogViewManagerMap`/`RewardViewComponent`/`UIBuffStoreItem`, all
CHALLENGE). The exoskeleton-teardown mechanics (destroying
`ExoSkeletonUIGraphic`/`ExoSkeletonData`, adding a plain `Image`) stay
internal to `LokrCharacterLoader` — a resolver just returns a `Sprite`,
it doesn't need to know how that sprite ends up on screen.

### 5.4 Sounds (replaces `SoundPatches.cs`'s three old patch files)

```csharp
public delegate AudioClip SoundResolver(string unitId, string eventName);
void RegisterSoundResolver(SoundResolver resolver, int priority = 0);
```

Covers combat sound events (`Unit.PlaySound`), `"promote"`
(`UIHeroManage`), and `"selectHero"` (`UIHeroRoom`) — the event name
already disambiguates them, same as today.

### 5.5 Localization & Lua scripts

```csharp
event Func<LocalizationManager.LanguageCode, IDictionary<string, string>> ContributingLocalization;
event Func<string, string> ResolvingScript;   // scriptName -> Lua source, or null to fall through
```

File-based contributors (today's `Mods/*/Localization/*_<lang>.txt` and
`Mods/*/Lua/*.lua`) are `LokrCharacterLoader`'s own default
subscribers, same pattern as everywhere else in this section.

**Bug fix included in this rewrite** (decided — no longer open): the
current recompiled-DLL `LocalizationManager` behavior has the
JA-hardcoding bug documented in
[content-systems.md](../../lokr-modding/docs/content-systems.md) §6 — five of
six `LoadKVText` call sites hardcode `LanguageCode.JA` instead of the
actual selected language, so `ContributingLocalization` subscribers only
ever see `*_ja.txt`-suffixed content requested for those five, regardless
of the player's real language. Since `LocalizationManagerPatches` is
being rewritten anyway to fire the new event, each call site is fixed to
pass the language code it actually represents (`EN` for the English
reference merge, the player's `currLanguageCode` for the AUTO/QUEST/
override-path merges, matching what the sixth, already-correct call site
does today) instead of the hardcoded `JA`. This is a genuine behavior
change from the current shipped mod — worth calling out in release notes
once this ships, since players on non-Japanese locales will start seeing
their own language's mod localization files picked up where previously
only Japanese ones were (if present at all).

### 5.6 State-visual-effect hook (generalizes Assassin invisibility)

Today's Assassin invisibility effect is two `Unit` patches
(`AddModifier`/`TurnEnded`) with `unitDefinition.uniqueId == "Assassin"`
hardcoded directly in the condition — the least "modular" piece of the
current design, and not something another plugin could reasonably extend
(it's baked to one specific hero). Rebuilt as a generic hook other
content (including `LokrCharacterLoader`'s own Assassin effect,
registered as its first/only caller) can use instead of hardcoding a
unique ID check:

```csharp
// action(unit, isEntering) — isEntering=true when the state was just applied,
// false when a per-turn check determines it should no longer be considered active.
void RegisterStateVisualEffect(string stateName, Action<Unit, bool> action);
```

`LokrCharacterLoader` calls
`CharacterAPI.RegisterStateVisualEffect("INVISIBLE", AssassinInvisibilityEffect)`
once at startup, where `AssassinInvisibilityEffect` contains today's
exact logic (still gated on `unit.isHero && unit.unitDefinition.uniqueId == "Assassin"`
internally — the hook generalizes *where the check lives*, not the
Assassin-specific behavior itself, which stays exactly as-is). Any
other plugin wanting a visual effect tied to a different state name (or
a different hero) now has a real extension point instead of needing to
Harmony-patch `Unit.AddModifier`/`TurnEnded` itself.

## 6. Plugin bootstrapping & load order

- `LokrCharacterLoaderPlugin` and `LokrEncyclopediaPlugin` both declare
  `[BepInDependency(LokrModAPIPlugin.Guid)]`. `LokrEncyclopediaPlugin`
  depends on nothing else — it doesn't touch `CharacterAPI`.
- A hypothetical future plugin that wants to *extend* character content
  additionally declares `[BepInDependency(LokrCharacterLoaderPlugin.Guid)]`
  and references `LokrCharacterLoader.dll` to see `CharacterAPI`.
- Each plugin owns its own `Harmony` instance keyed to its own GUID —
  unchanged from the previous pass of this plan.
- `ModAPI` initialization (binding config, resolving `Mods/` folders)
  happens in `LokrModAPIPlugin.Awake()`.
- `CharacterAPI`'s default (file-convention) resolvers/subscribers are
  registered in `LokrCharacterLoaderPlugin.Awake()`, *before*
  `Harmony.PatchAll()` — so even if a Harmony-patched game method fires
  during the same frame BepInEx finishes loading plugins, the default
  content sources are already wired up.

## 7. Migration mapping (old → new)

| Old (`LokrMods`) | New home | Notes |
|---|---|---|
| `ModManager.GetModsFolderPath()` | `ModAPI.Files.GetModFolders()` | |
| `ModManager.instance.PlaySound(...)` | `ModAPI.Audio.PlaySound(...)` | |
| `ModManager.isDebugMode` / `.skipSplashScreen` | `ModAPI.Config.DebugMode.Value` / `.SkipSplashScreen.Value` | now `ConfigEntry<bool>`, see §4.4 |
| `properties.txt` | `BepInEx/config/com.lokrmodding.lokrmodapi.cfg` | one-time auto-import, §4.4 |
| `ModManager.ModdedSound` | `ModAPI.Audio` internals | implementation detail |
| `OpenWavParser` | `LokrModAPI/Audio/OpenWavParser.cs` | moved, unchanged |
| `SplashVideoControllerPatches.cs` | stays a `LokrModAPI` patch | reads `ModAPI.Config.SkipSplashScreen` |
| `ExoSkeletonModData.cs` | two `ModAPI.ExtensionData<,>` instances inside `LokrCharacterLoader` | pattern moves to ModAPI, data stays character-domain |
| `DataHelperPatches.cs`, `MapHeroBarPortraitComponentPatches.cs`, the ×3 challenge-portrait patches | `LokrCharacterLoader/Patches/PortraitPatches.cs` | unified onto `CharacterAPI.RegisterPortraitResolver` (§5.3) |
| `UnitPatches.cs` (sound half), `UIHeroManagePatches.cs`, `UIHeroRoomPatches.cs` | `LokrCharacterLoader/Patches/SoundPatches.cs` | unified onto `CharacterAPI.RegisterSoundResolver` (§5.4) |
| `UnitPatches.cs` (Assassin invisibility half) | `LokrCharacterLoader/Patches/InvisibilityPatches.cs` | rebuilt on `CharacterAPI.RegisterStateVisualEffect` (§5.6) |
| `HeroRosterManagerPatches.cs`, `UnityDefinitionsParserPatches.cs`, `AbilitiesDefinitionsPatches.cs` | unchanged filenames, `LokrCharacterLoader/Patches/` | now fire `CharacterAPI` events (§5.2) in addition to existing behavior |
| `LocalizationManagerPatches.cs`, `IronhideScriptLoaderPatches.cs` | unchanged filenames, `LokrCharacterLoader/Patches/` | now fire `CharacterAPI` events (§5.5) |
| `UIMainMenuPatches.cs` | `LokrEncyclopedia/Patches/UIMainMenuPatches.cs` | own plugin now, unchanged logic |
| `ExoSkeletonDataPatches.cs`, `ExoSkeletonRendererPatches.cs`, `ExoSkeletonUIGraphicPatches.cs`, `PartyTokenComponentPatches.cs` | unchanged filenames, `LokrCharacterLoader/Patches/` | internal plumbing for the portrait/texture resolver chain, not directly plugin-facing |

## 8. Suggested implementation order

1. Scaffold `LokrModAPI` project + `LokrModding.sln`; wire up
   `ModAPI.Config` against BepInEx's `ConfigFile`, including the
   one-time `properties.txt` import; confirm settings survive a restart
   and land in the right `.cfg` file.
2. Port `ModAPI.Assets` + `ModAPI.Audio` (+ `OpenWavParser`) — pure
   ports, low risk.
3. Port `ModAPI.ExtensionData<,>`.
4. Move `SplashVideoControllerPatches` into `LokrModAPI`; confirm
   skip-splash still works against the new config.
5. Scaffold `LokrEncyclopedia` (simplest of the three remaining
   plugins — one patch, no `CharacterAPI` needed) to prove out the
   `[BepInDependency(LokrModAPIPlugin.Guid)]` pattern before tackling
   the bigger `LokrCharacterLoader` project.
6. Scaffold `LokrCharacterLoader` + `CharacterAPI.cs` (events/resolver
   chains declared, no subscribers yet).
7. Move the roster/unit-definition/ability patches over, wiring them to
   fire the new `CharacterAPI` events while registering
   `LokrCharacterLoader`'s own file-convention logic as the default
   subscriber (§5.2) — this is the trickiest step since it changes the
   *shape* of those patches, not just their location.
8. Move + unify the six portrait patches into `PortraitPatches.cs`
   around `CharacterAPI.RegisterPortraitResolver` (§5.3).
9. Move + unify the three sound patches into `SoundPatches.cs` around
   `CharacterAPI.RegisterSoundResolver` (§5.4).
10. Move localization + Lua patches, wiring the new events (§5.5) and
    fixing the JA-hardcoding bug in the same pass (§5.5) — worth its own
    explicit before/after test (switch the game to a non-English,
    non-Japanese locale and confirm the right mod localization files get
    picked up) rather than folding into the general smoke test in step 13.
11. Rebuild Assassin invisibility on `CharacterAPI.RegisterStateVisualEffect`
    (§5.6).
12. Move the exoskeleton/party-token plumbing (internal, no `CharacterAPI`
    surface of its own).
13. Full smoke test against the installed community pack (same test
    plan as used for the original migration effort) —
    this is the point where the resolver-chain refactor either proves
    itself behaviorally identical to today or doesn't, so budget real
    testing time here rather than treating it as a formality.
14. Delete `LokrMods/`.

## 9. Compatibility

`Mods/` folder *content* layout is completely unchanged — the installed
community pack keeps working with zero changes to any mod files. The one
compatibility-sensitive change is `properties.txt` → BepInEx config,
handled by the one-time import in §4.4 rather than a silent reset.
Nothing external depends on the current `LokrMods` GUID, so no compat
shim is needed for the plugin split itself.

## 10. Open questions / deferred scope

- **Manifest/mod.json**: still out of scope. No mod metadata
  (name/version/author/declared dependencies) exists today beyond the
  folder name itself. Matters more once there are enough independent
  mods/plugins that load-order or conflict detection becomes a real
  problem — `CharacterAPI`'s priority-ordered resolver chains (§5.1)
  cover the *code-level* version of this for now; a manifest would be
  about *content-level* conflicts between two data-only mod folders,
  which is a separate, still-unsolved problem (documented as
  "first-match-wins" folder-enumeration order today).
- **Plugin metadata** (decided now): `LokrModAPI` → GUID
  `com.lokrmodding.lokrmodapi`, name `"LoKR Mod API"`, version `1.0.0`.
  `LokrCharacterLoader` → GUID `com.lokrmodding.characterloader`, name
  `"LoKR Character Loader"`, version `1.0.0`. `LokrEncyclopedia` → GUID
  `com.lokrmodding.encyclopedia`, name `"LoKR Encyclopedia"`, version
  `1.0.0`.
