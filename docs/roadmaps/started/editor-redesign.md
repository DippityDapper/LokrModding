# LokrLab — unified extensible editor & project-type framework

**Status:** Phase 9 complete — Character and Ability live in the LokrLab
suite ([lab-suite-merge.md](../completed/lab-suite-merge.md), confirmed
2026-08-15). Phase 10 is
[encounter-creator.md](encounter-creator.md) (Phase 13 terrain catalog
in 0.12.64; Phase 12 floor-tile paint confirmed in-game 2026-08-16).
**Last updated:** 2026-08-16

**Goal, in one sentence:** turn `LokrCharacterLab` from "a hub that switches
between full-screen character-workstation scenes" into **`LokrLab`, one
persistent, dockable editor shell** — Godot-style menu bar, workspace tabs,
node tree, inspector, file tree, bottom panels, viewport — that hosts
pluggable **project types** (Character first, then Ability Library,
Encounter, Adventure, …) instead of being hardcoded to characters alone,
with every dock, registry, and project-type extension point reachable
through a public API.

This grew from a narrower "redesign the Character Lab's UI" plan into a
platform question mid-discussion: once the shell can host more than one
kind of thing to edit, character-specific plumbing (`CharacterSession`,
`CharacterCreatorAPI`) needs a project-agnostic layer underneath it. That
generalization is now the spine of this doc, not an add-on — §2 is new and
everything after it was revised to sit on top of it.

Every design question raised while planning this doc has now been
resolved — see §10 for the full decisions record, kept visible rather than
silently folded in so the reasoning behind each stays legible to whoever
picks this up next. Read
[vision-and-extensibility.md](../vision-and-extensibility.md) and
[character-lab-loader-pre-redesign-audit.md](character-lab-loader-pre-redesign-audit.md)
first — this doc assumes both, and treats
`vision-and-extensibility.md`'s "Character Creator" vision as describing
what becomes the **Character project type**'s own vision under this
broader shell, not something this doc overrides. That doc will need its
own companion pass once this direction is actually adopted — not attempted
here.

---

## 1. Where this sits relative to existing plans

- [phasing.md](../phasing.md) has the hub at "core v1 complete, Extensions
  next." Item 6 (Custom Scripting / Encounter Creator / Custom Adventures)
  reframes under this doc: **Encounter Creator and Custom Adventures become
  the second and third project types**, not just later features built on
  top of a still-character-shaped hub. That's a stronger tie-in than "this
  redesign makes them cheaper to build" — they're the actual validation
  that the project-type abstraction (§2) generalizes correctly, since
  Character alone can't prove that on its own. Recommend sequencing this
  whole doc **before** item 6 resumes in earnest (see §8) — building
  Encounter Creator once against a project-type framework beats building it
  against a character-shaped hub and re-platforming it later.
- [character-lab-loader-pre-redesign-audit.md](character-lab-loader-pre-redesign-audit.md)
  is Phase 0 of this roadmap, not a separate parallel track — see §4 for
  why it's a technical prerequisite, not just cleanup.
- **On-disk schema — not a blanket freeze.** `rig.json`'s flat shape is
  genuinely bounded (`CustomRigLoader` feeds it straight into the base
  game's own `ExoSkeletonDataAsset.ReloadData`), and
  [conventions.md](../../../LokrLab/docs/conventions.md)'s "no
  schema changes, ever" rule is about *that* constraint specifically — why
  new rig-editor data has always gone into sidecars instead of extending
  `rig.json`. It is not a rule against changing anything Lab-owned.
  `character.json`, `roster.json`, every sidecar, and the new `project.json`
  marker this doc introduces (§2.3) are Lab-owned formats with no base-game
  parser reading them — they can change wherever it's a net improvement,
  paid for with a **one-time migration** (convert existing folders the
  first time they're loaded post-upgrade), not permanent dual-format
  support. `CharacterIdentityRekey` already does exactly this kind of
  "detect legacy shape, rewrite once" migration for old named-folder
  characters — direct precedent, not a new pattern being invented.

---

## 2. The bigger shift: project types

### 2.1 Why generalize now

The trigger was a real design problem, not abstraction for its own sake:
under the original (Character-only) plan, editing an Encounter would have
meant *loading a character* to get into the editor at all — Encounter
Creator doesn't have a character, it has a roster of references to several
characters plus placement/trigger data. Forcing every project type through
"first, load one character" doesn't fit, and special-casing Encounter
Creator as "the one workstation that's weird about loading" would have
been exactly the kind of thing this whole redesign exists to avoid.

The fix generalizes one level up: **`LokrLab` is a shell that opens
*projects*; "Character" is one project type among several the shell can
host, not the thing the shell is fundamentally about.** Concretely, this
also cleanly resolves something §5.1 was already circling: whether
`LokrAbilityLab` bolts an "Abilities" tab onto Character's shell, or is
itself a first-class peer. Under this model it's obviously the latter — an
Ability Library is just another project type (§2.5.1), registered the same
way Character or Encounter are, no special-casing required.

### 2.2 Project type contract

```csharp
public sealed class ProjectTypeRegistration
{
    public string Id;                       // "character", "encounter", "ability-library", …
    public string DisplayName;
    public string IconKey;
    public string FolderRoot;                // e.g. Mods/LokrLab/Characters — where projects of this type live on disk
    public bool IsSingleton;                 // true for e.g. Ability Library — one project, not many; see §2.5.1
    public string[] ReferenceableProjectTypes; // which other types this one may cross-reference (§2.5) — e.g. Encounter references Character

    public Func<ProjectSession> CreateNew;   // "New Project" flow, type-specific UI
    public Func<string, ProjectSession> Load; // folder -> session

    // Registries scoped to *this* project type only:
    public void RegisterWorkspace(WorkspaceRegistration ws);
    public void RegisterNodeTreeContributor(NodeTreeContributor c, int priority = 0);
    public void RegisterNodeFactory(string kind, string[] validParentKinds, NodeFactory f);
    public void RegisterInspectorDrawer(string kind, InspectorDrawer d, int priority = 0);
    public void RegisterInspectorSection(string kind, InspectorDrawer s, int priority = 0);
    public void RegisterBottomPanel(string name, string iconKey, Func<Transform, GameObject> builder, BottomPanelIsRelevant isRelevant = null);
}

public static class LokrLabApi
{
    public static ProjectTypeRegistration RegisterProjectType(string id, string displayName, string iconKey, string folderRoot);
    public static ProjectSession CurrentSession { get; }   // whatever project is open, whichever type it is
    public static EditorSelection Selection { get; }
    public static ProjectReference PickProjectReference(string projectTypeId); // shared cross-project picker, §2.5
}
```

Every per-project-type registry here is the **same** registry §5 (core
concepts) describes — `WorkspaceRegistration`, `NodeTreeContributor`,
`InspectorDrawer`, etc. don't change shape, they just hang off a specific
`ProjectTypeRegistration` instance instead of one flat global surface. A
node kind, a workspace tab, an inspector drawer are always scoped to the
project type that registered them — the Character project type's `Part`
node kind means nothing to the Encounter project type and vice versa.

`ProjectSession` generalizes today's implicit "loaded character" state:

```csharp
public abstract class ProjectSession
{
    public string ProjectTypeId;
    public string Id;
    public string FolderPath;
    public string DisplayName;
    public bool IsDirty;
}

public sealed class CharacterSession : ProjectSession { /* today's CharacterProfile-backed fields, per §4 */ }
```

**`CharacterCreatorAPI` doesn't disappear** — it becomes a thin,
Character-specific convenience façade over `LokrLabApi`'s generic surface
(`CharacterCreatorAPI.RegisterWorkstation(...)` internally calls
`LokrLabApi.GetProjectType("character").RegisterWorkspace(...)`), so
existing call sites and this doc's earlier §5 content (written before this
generalization) don't need a rewrite — only a thin forwarding layer
underneath them changes. See §6.

### 2.3 Project discovery — the `project.json` marker

Every project folder, regardless of type, gets one new tiny universal
file at its root:

```json
{ "projectType": "character", "schemaVersion": 1 }
```

`LokrLab`'s Project Browser (§2.4) reads this **before** dispatching to any
project type's own `Load(folder)` — it's the one piece of format every
project type shares, exactly so the shell can route without needing to
understand any type's actual content. Existing character folders don't
have this today; a one-time migration pass (mirroring
`CharacterIdentityRekey`'s existing shape) writes it the first time
`LokrLab` boots post-upgrade, inferring `"projectType": "character"` since
that's the only type that exists yet.

**Folder layout — Decided: per-type root folders (A).** `Mods/LokrLab/Characters/`,
`Mods/LokrLab/Encounters/`, `Mods/LokrAbilityLab/Abilities/` (Ability
Library keeps its own existing root, since it's a different plugin) —
each `ProjectTypeRegistration.FolderRoot` points at its own convention,
matching `CharacterLabPaths.CharactersRoot` today. Human-browsable in a
plain file manager without needing to open every folder to learn its type;
no migration beyond adding the marker file. The alternative (one unified
`Mods/LokrLab/Projects/<id>/` root, type resolved purely from the marker)
would force moving every existing character folder for no gain the marker
file doesn't already provide.

### 2.4 Project Browser (generalized Load screen)

Today's Load workstation (Create / Load Existing / Import + recent list)
generalizes into a **Project Browser**: still the mandatory pre-gate — the
shell renders only this until a project is open, `LokrLabApi.CurrentSession
== null` — but "New Project" now asks *which project type* first (skipped
entirely if only one is registered, so today's exact flow is what a
single-project-type install still sees), then hands off to that type's own
`CreateNew`.

**Decided: the browser actively scans, not just recalls.** Beyond
surfacing `recent.json`'s remembered list, the Project Browser walks every
registered `ProjectTypeRegistration.FolderRoot` looking for `project.json`
markers (§2.3) each time it opens, so a project that was never explicitly
opened through this shell before — copied in by hand, shared by another
modder, restored from backup — still shows up without the user needing to
know to "Import" it first. `RecentFilesStore` stays as the ordering/pin
signal (most-recent-first, matches today), the directory scan is what
guarantees completeness on top of it. Every row is tagged with its type's
icon regardless of which of the two sources surfaced it.

### 2.5 Cross-project references — the actual answer to §2.1's problem

An Encounter project does not *load* a Character project as its active
session — it holds **read-only references** to one or more Character
projects by id, resolved lazily, the same established pattern
`vision-and-extensibility.md` already uses for abilities ("referenced by
id from a character's own `skillProgression`, the same way a character
references a shared sound-group"). Concretely:

- `EncounterSession` holds `{ role: "hero"/"enemy", characterProjectId }`
  entries. Its Node Tree contribution (§5.2) shows a `Combatants` node
  with one reference-child per entry; that reference node's Inspector
  drawer shows read-only summary (name, portrait) plus an **"Open
  Character project"** jump action.
- `LokrLabApi.PickProjectReference(projectTypeId)` is the one shared
  picker UI (same modal/row-list shape `MetaExoPickerPanel`/
  `ReplacePartPickerPanel` already establish) any project type's "Add
  Node" flow uses to pick a reference — Encounter's "Add Combatant" calls
  `PickProjectReference("character")`, nothing Encounter-specific about
  the picker itself.

**Jump navigation — Decided: switch, not split-view, for v1.** Opening a
referenced project **switches** the active session (closes Encounter,
opens Character, with a "back to Encounter" breadcrumb) rather than
opening a second, pinned session alongside it. Godot has no direct
analogue here (scene-instancing stays fully embedded, never a session
switch) — this is the simplest shape to build, matches "open scene
replaces the edited scene," and pairs naturally with §2.4's Project
Browser auto-scan: since the browser already discovers every project
without needing an explicit prior "open," switching back into the
Encounter after visiting a referenced Character is just as cheap as
switching into it was. True multi-project split-view is a real later
possibility, deliberately deferred rather than built alongside v1.

#### 2.5.1 Not every project type is "many small projects"

`ProjectTypeRegistration.IsSingleton` exists because Ability Library
doesn't fit the Character/Encounter shape of "many folders, pick one to
open." The whole point of a shared ability library is browsing everything
together — so it's **one project**, and each ability is a **node** inside
it, not a separate project of its own. A singleton project type skips the
New/Open picker in the Project Browser entirely; it's just always there as
one entry. See §6.1 for how this plays out for `LokrAbilityLab`
specifically.

### 2.6 Renaming `LokrCharacterLab` → `LokrLab`

Once the shell is genuinely project-type-agnostic, keeping the plugin
named "Character Lab" is actively misleading in exactly the way
`ability-lab.md` already flagged for the old `LokrCharacterCreator` →
`LokrCharacterLoader` rename ("the name should say what it does").
Concretely:

- `LokrCharacterLab` → **`LokrLab`**: GUID
  `com.lokrmodding.lab` → `com.lokrmodding.lab`, class
  `LokrLabPlugin` → `LokrLabPlugin`, folder/assembly rename.
  `LokrLab` shipped the Character project type built in through Phase 9
  (no reason to split yet — see §7.1). **Phase 9.5 split it**: Character
  is now `LokrCharacterLab` (`com.lokrmodding.characterlab`), a sibling
  of Ability Lab that depends on `LokrLabApi` only.
- **New plugin: `LokrLabApi`** — pure contracts (§2.2's types +
  delegates), no Harmony patches, no rendering — mirrors `SimpleUI`'s
  existing shape as "a passive shared library, no gameplay logic of its
  own." `LokrLab` and `LokrAbilityLab` both depend on it; see §6.1 for why
  this replaces this doc's earlier answer of "put contracts in
  `LokrCharacterLoader`."
- Ripple cost: every doc across this solution referencing
  `LokrCharacterLab` by name (dozens of files — top-level `CLAUDE.md`s,
  every plugin's `cross-references.md`, `ARCHITECTURE.md`'s dependency
  graph) needs updating. **Decided: rename exactly once, at Phase 2**
  (§8) when the shell skeleton + project-type framework land — not before
  (renaming mid-refactor just means doing the doc-sync twice) and not
  deferred past it (every doc written after Phase 2 should already say
  `LokrLab`).

### 2.7 Runtime content ownership belongs to `LokrCharacterLoader`, not the authoring plugins

**Done, 2026-08-12** — ahead of the rest of this roadmap, exactly as
flagged below as independently shippable. `AbilitiesContribution` moved
from `LokrAbilityLab` into `LokrCharacterLoader` as
`CustomRigs/AbilityLabContentLoader.cs`, alongside a new shared
`CustomRigs/LocaleFileSuffixes.cs` deduplicating the two locale-suffix
tables that would otherwise sit identically side-by-side in the same
assembly. Verified with a full solution build. See
[character-lab-loader-pre-redesign-audit.md](character-lab-loader-pre-redesign-audit.md)'s
own 2026-08-12 update for the audit-side record. Rest of this section kept
as-written for the reasoning/context.

A play-only user shouldn't need `LokrLab` or `LokrAbilityLab` installed just
to use character/ability content someone else authored and shared — only
`LokrCharacterLoader` (the plugin that actually patches the live game)
should be required. Checked against the real source rather than assumed,
this is **already true for characters and not yet true for abilities** —
two different states, two different fixes:

- **Characters already work this way.** `ModAPI.Files.EnumerateCategorySubfolders`/
  `EnumerateCategoryFiles` (`LokrModAPI/Files/ModFileSystem.cs`) scan
  **every** mod folder generically by category name (`"Characters"`,
  `"NewAbilities"`, …) — not by a specific plugin's folder name — and
  `CharacterLabContentLoader`, the bridge that turns a `Characters/<id>/`
  folder's files into real `CharacterAPI` content, already lives inside
  `LokrCharacterLoader` itself, not the Lab plugin. A character folder
  dropped under `Mods/AnyName/Characters/<id>/` loads today with **zero**
  Lab plugins installed. `Mods/LokrLab/Characters/` is only where
  the Lab *defaults to writing* — never a load-time requirement. This
  already matches the audit's own "Shared domain (Loader ↔ Lab boundary)"
  recommendation (M-04): the contract is Loader-owned, Lab is just a
  writer into it.
- **Abilities don't, yet.** `AbilitiesContribution.cs` — the equivalent
  bridge that turns a nested `Abilities/<id>/ability.txt` (+ `icons/` +
  `localization_*.txt`) folder into `CharacterAPI.BuildingAbilities`
  content — lives inside `LokrAbilityLab` itself, not
  `LokrCharacterLoader`. Without `LokrAbilityLab` installed, that
  subscriber never registers, so a hand-copied ability folder is silently
  ignored even though the generic flat `Mods/*/NewAbilities/*.txt`
  convention (read directly by `LokrCharacterLoader`'s own
  `AbilitiesDefinitionsPatches`, regardless of any Lab plugin) already
  works fine. Ability icon resolution is unaffected — `PortraitPatches`
  already lives in `LokrCharacterLoader` and resolves nested `icons/`
  generically, same as it does for hero portraits.

**Fix:** move `AbilitiesContribution`'s reading logic from `LokrAbilityLab`
into `LokrCharacterLoader`, mirroring `CharacterLabContentLoader`'s
existing shape exactly — it only needs to know the on-disk convention
(folder name `"Abilities"`, nested `ability.txt`/`icons/`/
`localization_*.txt` per id), not anything about `LokrAbilityLab.dll`
itself, the same way `CharacterLabContentLoader` needs no compile-time
knowledge of `LokrLab.dll`. No architecture-layering violation — this is
pure data/file-format knowledge moving to sit next to the character
equivalent that already lives there.

**This is a real, general design rule worth stating explicitly, not just a
one-off fix:** a project type's *runtime content-loading bridge* belongs
wherever that content's actual runtime consumer lives — `LokrCharacterLoader`
for anything the live game reads — independent of where the *authoring*
UI lives. `ProjectTypeRegistration.FolderRoot` (§2.2) for the `character`
and `ability-library` types should point at `LokrCharacterLoader`-owned
paths (`Mods/LokrCharacterLoader/Characters/`, `Mods/LokrCharacterLoader/Abilities/`)
rather than plugin-branded ones — purely for player-facing clarity, since
the scan is already folder-name-agnostic; **no migration is required**
even for existing characters under the old `Mods/LokrLab/`
path — they keep loading forever regardless of the folder's name, so only
the *default write target* for newly-created content needs to point at
the new location. Encounter/Adventure project types don't have this
question yet — there's no runtime encounter-loading system to own that
content today, so their `FolderRoot` stays wherever their own future
runtime consumer ends up living.

**This was decoupled from the rest of this roadmap and shipped on its
own, independent of Phase timing** (see "Done" note above) — moving
`AbilitiesContribution` was a small, low-risk, already-well-understood
change (an existing, working class relocating to sit next to its
character equivalent) that didn't need `UiDockSpace`, node trees, or any
registry from §5 to exist first. Folded into Phase 0's scope (§8) since
it's squarely the same "Shared domain" cleanup the audit already scoped
there, but landed ahead of the rest of Phase 0 rather than gated behind
it.

---

## 3. Target experience

Still one persistent shell, one scene, built once per session — the
diagram below shows the **Character** project type's own workspace/panel
set specifically; a different project type fills the same chrome (top
menu, node tree dock, inspector dock, file tree dock, bottom panel dock)
with its own tabs and nodes instead:

```
┌─────────────────────────────────────────────────────────────────────────┐
│ File   Edit   View   Help                        Onagro (Character) *   │
├───────────────────────────────────────────────────────────────────────┤
│ [Character] [Sandbox] [Abilities]                          [workspaces] │
├───────────────────────────────────────────────────────────────────────┤
│ [Select][Move][Rotate][Scale][ScaleXY][Pivot]   Mass Edit  [toolbar]    │
├───────────┬───────────────────────────────────────────┬─────────────────┤
│ Node Tree │                                             │  Inspector    │
│  ▸ Rig    │                                             │  (selection-  │
│  ▾ Animator│              VIEWPORT                      │   driven,     │
│    ▸ Walk │        (workspace-owned content)            │   registry-   │
│  ▾ Abilities (references, jump to Ability Library)       │   dispatched) │
│    • dash │                                             │               │
│───────────┤                                             │               │
│ File Tree │                                             │               │
│  rig/     │                                             │               │
│  sounds/  │                                             │               │
├───────────┴───────────────────────────────────────────┴─────────────────┤
│ [Timeline] [Checklist] [History]                        [bottom panels] │
├───────────────────────────────────────────────────────────────────────┤
│ status text                                          project id / type │
└─────────────────────────────────────────────────────────────────────────┘
```

Mapped to Godot's own editor by analogy (unchanged from before — this part
of the shape didn't change, only what fills it did):

| This redesign | Godot equivalent |
|---|---|
| Top menu (File/Edit/View/Help) | Scene/Project/Debug/Editor/Help menu bar |
| Workspace tabs (per active project type) | 2D / 3D / Script / AssetLib main-screen tabs |
| Node Tree dock | Scene dock |
| Inspector dock | Inspector dock |
| File Tree dock | FileSystem dock |
| Bottom panels | Output/Debugger/Animation/Audio bottom panels |
| Viewport | 2D/3D viewport |
| `ProjectTypeRegistration` (§2.2) | *(no direct analogue — closer to how VS Code or a JetBrains IDE support different project/language modes via extensions than to anything Godot itself does; Godot is single-purpose per editor instance)* |

The Project Browser (§2.4) is the shell's own empty state — nothing else
renders until a project is open, generalizing the existing "only General
available with no character loaded" hub rule to "only the Project Browser
is available with no project open."

---

## 4. Why Phase 0 (the pre-redesign audit) is still a hard prerequisite

Unchanged in substance from before this generalization — if anything, more
true now that state has to be swappable across project types, not just
across workspace tabs within one:

- An **Inspector drawer registry** needs something stable to bind to — a
  selected node's *data*, independent of whichever panel or project type
  built the UI showing it. Today, `InspectorPanel` reads directly off
  `RigEditorScene`'s static fields. A registry-dispatched drawer can't be
  written against "whatever `RigEditorScene` currently has selected" — it
  needs a `ProjectSession`/`EditorSelection` object it can read from
  regardless of which project type or plugin populated it.
- A **Node Tree** spanning a whole project needs one place holding "the
  current session," not several workstations each owning their own slice
  of static state that happens to agree today only because just one is
  ever visible at a time.
- **C-03**/**C-UI-02** (static session/panel state not reset) get *more*
  dangerous under a shell that swaps between project types at runtime —
  "close one project, open a different type" now exercises the same reset
  path that used to only run on a full lab close/reopen.

**Recommendation unchanged:** audit P0 items C-01–C-04 plus
`CharacterSession`/`RigEditorScene.ResetSession()` extraction are Phase 0,
before any registry work starts. See §8.

---

## 5. Core concepts (as introduced by the Character project type)

Everything below is written the way it was originally scoped — against
Character specifically — because Character is still the first, and for a
long while the only real, consumer proving these registries out (§2.1).
Read each registration call as "on the Character `ProjectTypeRegistration`"
per §2.2; nothing here is Character-exclusive machinery, just
Character-first usage of it.

**Terminology, settled: "workspace" for the new concept, "workstation" for
the old one.** This doc uses **workspace** consistently for the new,
registered-through-`WorkspaceRegistration` top-tab concept (§5.4), and
**workstation** only where referring to today's actual pre-redesign
classes/screens (`HomeWorkstationScene`, "the Load workstation," etc.) —
i.e. the thing each workspace *replaces*, not a synonym for it. Kept
deliberately distinct rather than merged into one word so "port the Home
workstation into a workspace" stays readable as a migration, not a
tautology.

### 5.1 Dockable shell — SimpleUI additions

SimpleUI's current widget set (`UiPanel`, `UiStack`, `UiSplit`,
`UiScreenSwitcher`, `UiButton`, `UiToggle`, `UiTextField`, `UiDropdown`,
`UiComboBox`, `UiList<T>`, `UiLabel`, `UiImage`, `UiModal`) has **no
docking, no drag-to-resize, and no drag-and-drop** — `UiSplit` divides
space by fixed weights set at construction, and every "list"
(`SceneTreePanel`, `InspectorPanel`'s parts container, `EditHistoryPanel`)
is a hand-rolled `ScrollRect` reimplementation local to `LokrCharacterLab`,
not a shared SimpleUI type. New widgets needed, in rough dependency order:

| New widget | Purpose | Fills a gap because... |
|---|---|---|
| `UiSplitter` | draggable divider between two regions; updates sibling weights live | `UiSplit`'s weights are fixed at `.Create()` today — no interactive resize exists anywhere in SimpleUI |
| `UiTabGroup` | a visible, reorderable tab strip over one shared content area | generalizes `UiScreenSwitcher` (already built, explicitly unused: "available for future modal/tab scenarios" per [classes.md](../../../SimpleUI/docs/classes.md)) — this redesign is that future scenario |
| `UiDockPanel` | wraps arbitrary content with a draggable title bar, close/pin, context menu | nothing like this exists; every panel today is placed once at scene-build time and never moves |
| `UiDockSpace` | root container owning named zones (Left/Right/Bottom/Center) that hold `UiTabGroup`s of `UiDockPanel`s; handles dragging a panel from one zone/tab group into another | the actual "dockable container" primitive the original request calls for by name |
| `UiTree` | generic indented tree list: expand/collapse, icons, drag-reorder, multi-select | promotes the row-list pattern currently duplicated across `SceneTreePanel`, `InspectorPanel`'s parts container, and `EditHistoryPanel` into one shared widget — directly reusable for the Node Tree (§5.2) and File Tree (§5.7) |
| `UiContextMenu` | right-click popup menu | doesn't exist; `MenuBarPanel`'s File/Edit/Help are hand-built click-toggle panels, not a reusable context-menu primitive, and "Add Node" (§5.2) needs one |
| `UiToolbar` | horizontal icon-button strip with grouping/separators | generalizes `ToolbarPanel`'s hand-built tool-mode button row so other workspaces can build a toolbar without re-deriving it |
| `UiStatusBar` | thin strip for status text + right-aligned indicators | promotes `ToolbarPanel`'s ad hoc status `Text` reference into something workspace-agnostic |

**Design constraints carried over from SimpleUI's existing philosophy**
([architecture.md](../../../SimpleUI/docs/architecture.md) "No magic, no
hidden state"): `UiDockSpace` should not auto-persist layout — it exposes a
serializable snapshot type (zone → panel-id → size) that the *consumer*
reads/writes, the same way SimpleUI never owns persistence for anything
else today.

**Doc-sync side note found while researching this:** [SimpleUI/docs/classes.md](../../../SimpleUI/docs/classes.md)
documents `UiList` as non-generic with a `UserData` field; the actual
`UiList.cs` is generic (`UiList<T>`, confirmed via
`LokrLab/Editor/InspectorPanel.cs`'s usage), and `UiComboBox`
exists in code but isn't documented in `classes.md` at all. Worth a small
doc-sync pass independent of this redesign — flagging here so it doesn't
get lost.

**Decided — no floating panels.** Panels are always docked into one of the
shell's zones; dragging a panel's tab redocks it into a different
zone/tab group. There is no undocked/floating state at all in v1, not even
as a same-canvas overlay — simpler than this doc originally proposed, and
sidesteps the multi-window-vs-overlay question entirely by not needing
either.

**Layout persistence** is a **per-user editor preference**, not part of
any project's own data — stored once under `LokrLab`'s own editor-data
root (where `recent.json` already lives) as `layout.json`, independent of
which project is currently open. Mirrors Godot's own `editor_layout.cfg`.
Persisting per-project instead was considered and rejected: a brand-new
project starting from whatever arrangement was last used for an unrelated
one is surprising, not helpful.

### 5.2 Node tree & document model

Today's `SceneTreePanel` lists only rig parts — a leaf of one workstation,
not a tree of the project. The original request explicitly wants a
**generic node tree** where "nodes can be defined and created for a
multitude of purposes, like a part node, animator node, ability node."

```csharp
public sealed class LabNode
{
    public string Id;                 // stable within a session
    public string DisplayName;
    public string Kind;               // extensible string, not a closed enum — "Part", "AnimationClip", "AbilityRef", …
    public string IconKey;
    public List<LabNode> Children;
    public object Payload;            // the real object this node represents (DraggablePart, AnimationClip, a project reference, …)
}
```

`Kind` is a string for the same reason `CharacterAPI`'s resolver chains key
by string rather than a closed type — a third-party plugin needs to
introduce a wholly new node kind without this codebase needing a
recompile to add an enum case.

```csharp
public delegate IEnumerable<LabNode> NodeTreeContributor(ProjectSession session);
// registered via a specific ProjectTypeRegistration.RegisterNodeTreeContributor — §2.2
```

Each contributor returns the top-level node(s) it owns, scoped to
whichever project type registered it — Character's Rig contributes a
`Rig` node with `Part` children; Character's Animator contributes an
`Animator` node with `AnimationClip` children; Encounter contributes a
`Combatants` node of reference children (§2.5). The Node Tree panel
concatenates every contributor *for the currently open project's type*, in
priority order, and renders one `UiTree`.

**Node creation** replaces today's scattered ad hoc buttons ("+ Add
Animation" modal, "Add Reference," "Create Character") with one consistent
flow: right-click a parent node (or the tree background) → `UiContextMenu`
→ "Add Node" → filtered by what's valid under that parent.

```csharp
public delegate LabNode NodeFactory(LabNode parent, ProjectSession session);
// registered via ProjectTypeRegistration.RegisterNodeFactory(kind, validParentKinds, factory)
```

**Selection model** generalizes `RigEditorScene.SelectedPart`/
`MultiSelection` into a shell-level selection every panel reads from one
place:

```csharp
public sealed class EditorSelection
{
    public LabNode Primary;
    public IReadOnlyList<LabNode> All;   // multi-select, primary always a member — same invariant RigEditorScene.MultiSelection already keeps
}
```

#### 5.2.1 Schema, not just a session view — Decided

Two real options were on the table:

- **(A) Presentation-layer only** — `LabNode` built fresh each session
  from existing on-disk files; nothing is itself persisted as a node.
  Lower risk, keeps the on-disk document format off this redesign's
  critical path.
- **(B) A real persisted node/document schema** — `character.json` and
  sidecars restructured so the tree's own shape is the source of truth on
  disk, not a derived view. More genuinely Godot-native (real
  drag-to-reparent, authoritative ordering), at the cost of a one-time
  migration pass across every existing project folder.

**Decided: start with (A), revisit (B) once the node tree has real mileage
on it (post-Phase 3/4).** Not ruling out (B) — the opposite: it's worth
doing once it's clear what the ideal document shape actually is, which is
easier to see correctly after building against the tree for a while than
to guess up front.

### 5.3 Inspector extensibility

Today's `InspectorPanel` dispatches on a **closed** `enum
InspectorTarget { None, Part, Animation, Frame, Reference }`. Same
registry shape as §5.2, keyed by `LabNode.Kind`:

```csharp
public delegate void InspectorDrawer(LabNode node, ProjectSession session, UiElement contentParent);
// ProjectTypeRegistration.RegisterInspectorDrawer(kind, drawer, priority)   — one primary drawer per kind, highest priority wins
// ProjectTypeRegistration.RegisterInspectorSection(kind, section, priority) — many contributed sections stack under the primary drawer
```

Two-tier on purpose, mirroring Godot's own `EditorInspectorPlugin` model
(one plugin `can_handle`s an object and builds its main properties; any
number of others can each contribute an extra section without owning it).
Migrating today's four sections is mechanical once the registry exists —
the actual field-editing code barely changes, only the dispatch that
decides *which* section to build.

### 5.4 Workspace framework (top tabs)

```csharp
public sealed class WorkspaceRegistration
{
    public string Name;
    public string IconKey;
    public int Priority;
    public bool RequiresProjectOpen = true;            // generalized from RequiresCharacterLoaded
    public Func<UiElement, UiElement> BuildToolbar;
    public Func<Transform, GameObject> BuildViewport;   // in-shell viewport content, OR:
    public Func<ProjectSession, bool> RequiresSceneTransition; // unused by Sandbox setup; fight still CloseTo — see §6.2
}
// registered via a specific ProjectTypeRegistration.RegisterWorkspace(registration)
```

**Home's role dissolves, deliberately.** Session-state ownership moves to
`CharacterSession`/`ProjectSession`; the checklist becomes a bottom panel
(§5.5) available from every Character workspace rather than its own tab.
The project's name/id/dirty-state instead lives in the shell's own status
bar, always visible (§3's diagram). Load/Project Browser stays a real
pre-gate, not a workspace.

### 5.5 Bottom panel framework

```csharp
public delegate void BottomPanelIsRelevant(WorkspaceRegistration activeWorkspace, EditorSelection selection); // optional filter, not gating
// ProjectTypeRegistration.RegisterBottomPanel(name, iconKey, builder, isRelevant)
```

`Timeline` is Animator-specific; `Checklist` and `History` are global
within the Character project type. Registration is always
dockable/selectable; an optional `isRelevant` predicate only controls
auto-focus, not visibility — avoids panels appearing/disappearing outright
by workspace, which would make "where did that dock go" a recurring
confusion.

### 5.6 Top menu framework

```csharp
LokrLabApi.RegisterMenu(string name, int priority);
LokrLabApi.RegisterMenuItem(string menuName, string label, Action onClick, int priority, Func<bool> isEnabled = null, Func<bool> isVisible = null);
```

Unlike workspace/node/inspector/bottom-panel registries, the top menu bar
is genuinely **shell-level, not per-project-type** — File/Edit/View/Help
exist regardless of which project type is open; individual menu *items*
are what a project type (or a specific workspace) contributes into them.
Replaces `MenuBarPanel`'s hardcoded File/Edit/Help with registration;
today's own entries become the first (built-in) registrants — dogfooding
the same "no special-cased fast path for built-in content" principle
`CharacterAPI`'s default resolvers already follow.

### 5.7 File tree dock

Promotes file browsing from `FileBrowserPanel`'s modal-only role
(Save-As, atlas-import picking — unchanged) to a permanent left-dock tab
showing the **currently open project's own folder**, using `UiTree`.
Double-clicking a row can select the corresponding Node Tree entry where
one exists — a convenience link, not a merge; File Tree stays "what's on
disk," Node Tree stays "what this project logically contains," the Godot
FileSystem-dock/Scene-dock split.

### 5.8 Viewport ownership

The center viewport's content is workspace-owned
(`WorkspaceRegistration.BuildViewport`) — Character's own workspace keeps
today's dual-camera Main/Preview split unchanged; other workspaces (and
other project types entirely) can put whatever they need there. See §6.2
for why Sandbox specifically doesn't try to put a live combat scene inside
it.

---

## 6. Cross-plugin integration

### 6.1 Ability Lab — as its own project type, deeply integrated (Decided)

[ability-lab.md](../completed/ability-lab.md) made a deliberate call:
`LokrAbilityLab` is a **sibling** plugin, not a `LokrCharacterLab`
workstation, because its data model (a shared, mod-wide library) doesn't
fit "one open character." The project-type generalization (§2) actually
*validates* that original call rather than overturning it — under this
model, Ability Lab was never going to be a workspace tab bolted onto
Character; it's a **peer project type**, exactly the shape sibling-ness
already argued for.

**Shape:** `LokrAbilityLab` registers `ProjectTypeRegistration { Id =
"ability-library", IsSingleton = true, FolderRoot =
"Mods/LokrAbilityLab/Abilities" }` (§2.5.1 — one project, not many; each
ability is a **node**, not a separate project). It builds real Node Tree
contributions (one node per ability) and Inspector drawers (today's
envelope form + raw KV body becomes a drawer on an `Ability` node kind) —
genuine "deep" integration on the authoring side, not a launched-in
overlay.

A Character project's own tree separately contributes an `Abilities`
branch (§5.2) of lightweight reference nodes for whatever the character's
`skillProgression` actually references, using the same cross-project
reference/jump mechanic §2.5 defines for Encounter → Character. Clicking
one switches into the Ability Library project with that ability
pre-selected. This is what makes the integration "deep" on *both* ends —
Ability Lab gets first-class node/inspector treatment for authoring, and
Character projects get live, accurate, clickable references instead of an
opaque id string — without either plugin needing special-case knowledge
of the other's internals; both go through `LokrLabApi` the same way.

**Worth separating explicitly: authoring integration (above) is a
different concern from runtime-loading independence (§2.7).** §2.7's
`AbilitiesContribution` relocation is what actually lets a *player* use
someone else's authored abilities without `LokrAbilityLab` installed at
all — it has nothing to do with whether the Ability Library is a
project type, a workspace, or anything else in the authoring shell, and
doesn't need to wait for any of it.

**Contract placement — updated from this doc's earlier answer.** Before
the project-type generalization, this doc recommended extracting shell
contracts into `LokrCharacterLoader` (already shared upstream of both
plugins) rather than having `LokrAbilityLab` depend on
`LokrCharacterLab.dll` directly. That reasoning doesn't fully carry over:
`LokrCharacterLoader` is a **runtime** content-loading plugin (patches the
live game); housing editor-authoring contracts (`ProjectTypeRegistration`,
`LabNode`, workspace/inspector delegates) there would conflate two
concerns this codebase has always kept cleanly split (`LokrCharacterLoader`
= runtime, `LokrCharacterLab`/`LokrLab` = authoring). **Updated
recommendation: give the contracts their own new plugin, `LokrLabApi`**
(§2.6) — pure interfaces/delegates, no rendering, the same "passive shared
library" shape `SimpleUI` already proves out. `LokrLab` (the shell +
Character project type) and `LokrAbilityLab` both depend on it as true
peers; neither depends on the other's implementation assembly.

### 6.2 Sandbox — scene jump; Ability Lab Stage researches embed

A literal "everything, including a live 2D/3D view, renders inside one
editor" would mean Sandbox's combat encounter rendering inside a dockable
viewport panel. Sandbox's fight scene
(`KRLegendsFightGameplay02`) is a real, separate Unity scene with its own
hex-grid rendering, camera rig, and gameplay systems never designed to run
additively alongside another scene's UI/canvas stack.

**Sandbox Enter Fight** uses the same additive hole embed as Ability Lab
Stage (`LabHost.StartEmbeddedFight`, `SandboxHole`). The lab stays open.
Fight-end does not call `ReopenAfterFight`.

**Ability Lab Stage (2026-08-13):** additive embed of the same
`fighttesterempty` arena into the Stage camera hole. LokrLab owns
generic scene-in-hole embed (`LabHost.StartEmbeddedScene`: additive
load, `Camera.rect`, HUD fit). Character Lab's `StartEmbeddedFight`
validates the unit, sets up the quest, and spawns. Session 1 chose
`LoadSceneMode.Additive` + `Camera.rect` (not render-to-texture).
Fight-end must not call `ReopenAfterFight`. Embedded fights pan by
right/middle-drag only (no screen-edge scroll). If embed fails at
runtime, Stage Play reports the error (no mannequin viewer). See
[ability-lab-overhaul.md](../completed/ability-lab-overhaul.md) Phase 8 embed.

### 6.3 Future extensions (Encounter Creator, Custom Adventures, Custom Scripting)

Per §2.1/§1, Encounter Creator and Custom Adventures are now the second
and third project types rather than features layered onto a
character-shaped hub — this is where the generalization actually gets
proven, not just theorized. Encounter's own phases, data model, and
out-of-scope list live in
[encounter-creator.md](encounter-creator.md). Custom
Adventures stays on [extensions.md](../not-started/extensions.md) until
Encounter v1 exists.

---

## 7. Public API surface (sketch)

Two layers now, not one flat `CharacterCreatorAPI`:

```csharp
// LokrLabApi — project-type-agnostic, lives in the new contracts plugin (§2.6)
public static class LokrLabApi
{
    ProjectTypeRegistration RegisterProjectType(string id, string displayName, string iconKey, string folderRoot);
    ProjectSession CurrentSession { get; }
    EditorSelection Selection { get; }
    ProjectReference PickProjectReference(string projectTypeId);
    void RegisterMenu(string name, int priority = 0);                 // shell-level — §5.6
    void RegisterMenuItem(string menuName, string label, Action onClick, int priority = 0, Func<bool> isEnabled = null, Func<bool> isVisible = null);
}

// Per project type (Character shown; Encounter/Ability Library register the same shape)
public sealed class ProjectTypeRegistration
{
    void RegisterWorkspace(WorkspaceRegistration ws);
    void RegisterNodeTreeContributor(NodeTreeContributor c, int priority = 0);
    void RegisterNodeFactory(string kind, string[] validParentKinds, NodeFactory f);
    void RegisterInspectorDrawer(string kind, InspectorDrawer d, int priority = 0);
    void RegisterInspectorSection(string kind, InspectorDrawer s, int priority = 0);
    void RegisterBottomPanel(string name, string iconKey, Func<Transform, GameObject> builder, BottomPanelIsRelevant isRelevant = null);
}

// Character-specific convenience façade, thin, forwards into the above:
public static class CharacterCreatorAPI
{
    void RegisterWorkstation(WorkspaceRegistration registration); // => characterProjectType.RegisterWorkspace(...)
    // … one forwarding method per ProjectTypeRegistration member, scoped to "character" automatically
}
```

Every registration method follows the same priority-ordered shape already
proven by `CharacterAPI`'s resolvers and the Animator's tool/importer/
validator registries — this doc introduces new extension *surfaces* and one
new *layer* (project types), not a new extension *pattern*.

---

## 8. Phased rollout

**Phase 0 — De-risk the internals** (§4, unchanged): audit P0 items
C-01–C-04; extract `CharacterSession`, `RigLoadService`/`RigSaveService`/
`RigPreviewService`, `CharacterProfileService`; consolidate the three
existing UI-construction paradigms onto SimpleUI. **Complete as of
2026-08-13** — see the dated sub-bullets below for what each item
actually turned out to involve.
- **Done, 2026-08-12:** moving `AbilitiesContribution` from
  `LokrAbilityLab` into `LokrCharacterLoader` (§2.7) — shipped standalone
  since it didn't depend on anything else in this phase.
- **Done, 2026-08-12:** all four audit P0 items —
  [C-01](character-lab-loader-pre-redesign-audit.md) (shared
  `LokrModAPI.Serialization.TextEscaping`, applied to every hand-built
  JSON/KV writer in `RLHeroesGenerator`, `RigEditorScene`,
  `CharacterImporter`, plus `CharacterProfileSidecar`'s own escaper
  consolidated onto it), C-02 (`RigEditorScene.WriteAllTextAtomic` —
  temp-file-plus-rename for `rig.json` and both sidecars), C-03 (new
  `RigEditorScene.ResetSession()`, called from `Build()`'s start,
  `CharacterLabScene.CloseTo`, and `CharacterLabScene.ForceClose`), C-04
  (`SwitchToWorkstation` now enforces `RequiresCharacterLoaded`
  server-side instead of relying only on the nav button being hidden).
  See the audit doc's own updated rows for full detail per item.
- **Done, 2026-08-12:** `CharacterSession` extracted from
  `HomeWorkstationScene` — folder/profile/editing-level state now has its
  own home (`LokrLab/Editor/General/CharacterSession.cs`),
  deliberately scoped as a maximally mechanical move (`HomeWorkstationScene`'s
  own properties became thin forwards, so no other file's call sites
  needed to change) rather than bundled with the much riskier
  `RigLoadService`/`RigSaveService`/`RigPreviewService`/
  `CharacterProfileService` extraction below — see the audit doc's own
  updated P2 section for why those were deliberately kept separate rather
  than attempted in the same pass.
- **Done, 2026-08-12:** `CharacterProfileService` extracted from
  `HomeWorkstationScene`'s own `PersistAndSync` + ~40 `SetX`/`AddX`/`RemoveX`
  mutators, same forwarding pattern as `CharacterSession` — every Properties
  panel still calls `HomeWorkstationScene.SetName`/`AddSkill`/etc.
  unchanged (~15 panel files, ~60 call sites untouched).
- **Done, 2026-08-12:** `RigPreviewService`/`RigSaveService`/
  `RigLoadService` extracted from `RigEditorScene`, in that order (most
  self-contained first, riskiest last). `RigEditorScene.cs` ~3,370 → ~2,820
  lines. Real, discovered-not-assumed scope: `OnLoadClicked`/`OnSaveClicked`
  themselves stayed in `RigEditorScene` — both spawn/mutate live editing
  state (`DraggablePart` GameObjects, active-clip/selection state) genuinely
  coupled to the rest of the file, not to file I/O; splitting them further
  would have meant inventing new method boundaries inside intricate,
  hard-to-verify logic. See the audit doc's own P2 section for the full
  per-service account. Full solution build verified clean after each of
  the four extractions individually, not just once at the end.
- **Done, 2026-08-13:** consolidated the "three UI-construction
  paradigms" (C-UI-01). Investigation found the premise was stale by the
  time this phase reached it: `EditorUiHelpers.cs` had zero real callers
  anywhere — every panel `conventions.md`/`supporting-classes.md`
  described it serving had already migrated onto real `SimpleUI` widgets
  independently, and `CharacterLabScene.CreateInputField` was likewise
  dead. Only 5 real call sites remained (`CreateLabel`/`CreateButton`),
  all canvas-level chrome needing an absolute anchor point rather than
  layout-group placement. Migrated all 5 to `UiLabel.Create`/
  `UiButton.Create` with `RectTransform.anchorMin`/`anchorMax`/
  `sizeDelta` set directly afterward — zero new SimpleUI API surface, and
  theme colors/fonts already matched the old hardcoded values exactly, so
  no visual drift risk despite this being unverifiable without in-game
  playtesting. Deleted `EditorUiHelpers.cs` and the three dead
  `CharacterLabScene` methods. Full solution build verified clean. **Phase
  0 is now complete.**

**Phase 1 — SimpleUI docking primitives** (§5.1): `UiSplitter`,
`UiTabGroup`, `UiDockPanel`, `UiDockSpace`, `UiTree`, `UiContextMenu`,
`UiToolbar`, `UiStatusBar`. Buildable and testable independent of any
project-type work — pure SimpleUI additions with their own smoke-test
scene.
- **Done, 2026-08-13:** all eight widgets plus `DockLayoutSnapshot` /
  `DockZoneSnapshot` / `DockZone` live in SimpleUI 1.1.0. `UiDockSpace`
  does not auto-persist — `CaptureLayout` / `ApplyLayout` expose the
  snapshot the consumer will write to `layout.json` in Phase 2. No
  floating panels (redock-only), matching §5.1. `DockingSmokeTest` is an
  additive overlay (Ability Lab shape): F8 when
  `SimpleUI.Debug.EnableDockingSmokeTest` is true (default), or the
  **SimpleUI Docking Test** mod-menu button. Full solution build
  verified clean. Docs updated across SimpleUI, `ARCHITECTURE.md`, this
  roadmap, and `docs/api/classes.json`.

**Phase 2 — Project-type framework + shell skeleton + rename** (done
2026-08-13):
- `LokrLabApi` plugin (`com.lokrmodding.labapi`) with
  `ProjectTypeRegistration` / `ProjectSession` / `LokrLabApi.RegisterProjectType`.
  No Harmony, no SimpleUI reference; builders take `Transform`.
- `project.json` marker (`ProjectMarker`) + one-time migration of every
  `Mods/*/Characters/` folder on first boot (§2.3).
- Rename `LokrCharacterLab` → `LokrLab` (§2.6): folder/assembly/namespace,
  GUID `com.lokrmodding.lab`, class `LokrLabPlugin`, write root
  `Mods/LokrLab/Characters/`. Old `Mods/LokrCharacterLab/Characters/`
  folders still appear in the browser via category scan.
- `UiDockSpace` wired into `Shell/LabShell.cs` (placeholder Node Tree /
  Viewport / Inspector / Timeline). Project Browser is the empty state
  when `CurrentSession == null`. File menu (registered on LokrLabApi)
  reaches Close Project / Home / Properties / Animator / Sandbox / Close Lab.
- Character project type registered in `LokrLabPlugin.Awake`.
  `CharacterCreatorAPI.RegisterWorkstation` also forwards a
  `WorkspaceRegistration` onto that type. No Node Tree/Inspector content
  yet.

**Phase 3 — Node Tree + selection model** (done 2026-08-13):
- `NodeTreePanel` concatenates the open type's `NodeTreeContributor`s into
  one `UiTree` and writes `LokrLabApi.Selection` (`EditorSelection.Set` /
  `Clear`; Primary is always a member of All).
- Character registers three contributors: the character itself, **Rig**
  with `Part` children (the `SceneTreePanel` port, read from `rig.json`
  without opening the Animator), and **Animator** with `AnimationClip`
  children.
- Right-click a parent → `UiContextMenu` → "Add \<Kind\>" for factories
  whose `validParentKinds` match. Part / AnimationClip factories surgically
  insert into `rig.json` when the file has no authored frames; otherwise
  they refuse and tell the user to use File → Animator.
- Inspector dock shows a selection summary only. Real drawers are Phase 4.
- Drag-reparent on the tree is in-memory only (schema A); a refresh reloads
  from disk.

**Phase 4 — Inspector registry** (done 2026-08-13):
- `InspectorDock` rebuilds only when selection identity changes, then
  calls `FindInspectorDrawer` + `FindInspectorSections` for the primary
  node's kind.
- Character registers drawers for Character / Rig / Part / Animator /
  AnimationClip / Frame / Reference. Part and AnimationClip show name +
  `rig.json` offsets/frame count; live pivot/visibility/events/poses stay
  in the Animator (those widgets refresh on every playback tick).
- `InspectorPanel.Refresh` maps `InspectorTarget` onto the same kind
  strings. Built-in sections stay persistent (row reuse, focus-skip,
  in-place playback refresh unchanged). Extra sections stack underneath
  and rebuild only when kind+id changes.
- `CharacterCreatorAPI.RegisterInspectorDrawer` /
  `RegisterInspectorSection` forward onto the Character type.
- LokrLab **0.4.0**. Full solution build verified clean.

**Phase 5 — Workspace framework + remaining workstations** (done 2026-08-13):
- Shell workspace tab strip (`Properties` / `Animator` / `Sandbox`).
  Sandbox setup is in-shell (`BuildViewport`); only Enter Sandbox Fight
  leaves the lab.
- Properties: category nodes under Character; `PropertiesCategoryHost`
  builds every category once and show/hides so PersistAndSync refresh
  stays in place. Viewport is a short prompt.
- Animator: `EnsureShellRuntime` + `ViewportCameraBinder` put Main/Preview
  cameras in the center dock; `ToolbarPanel.BuildInto` fills the workspace
  toolbar. Node Tree Part/Clip selection calls `SelectPartByName` /
  `SelectClipByName`. Live `InspectorPanel` hosts in the shell inspector
  while this workspace is active.
- Home retired: `SwitchToHome` aliases `SwitchToShell`. File menu is
  Close Project / Save Rig / Import / Slice Atlas (Animator-live) /
  Sandbox / Close Lab.
- `ProjectTypeRegistration.FindWorkspace`. LokrLab **0.5.0**, LokrLabApi
  **1.2.0**. Full solution build verified clean.

**Phase 6 — Bottom panels + top menu** (done 2026-08-13):
- Character registers Timeline / Checklist / History via
  `RegisterBottomPanel`. `isRelevant` auto-focuses Timeline on Animator
  and Checklist on Properties; it never hides a tab.
- Timeline hosts `AnimationsPanel` + `AnimationTimelinePanel` in the
  bottom dock (BuildInto). Checklist is the readiness list. History is
  the live undo list (modal remains as fallback).
- File / Edit / View / Help via `LokrLabApi.RegisterMenu`. Edit carries
  undo/redo/frame ops; View focuses bottom tabs + Refresh Preview; Help
  opens About.
- `UiDockSpace.SelectPanel`. `CharacterCreatorAPI.RegisterBottomPanel`.
  LokrLab **0.6.0**, SimpleUI **1.1.3**. Full solution build verified
  clean.

**Phase 7 — File Tree dock** (done 2026-08-13):
- Left-dock File Tree tab lists the open project's folder via `UiTree`.
  Double-click a file selects the matching Node Tree row when one exists
  (`project.json`/`character.json` → Character, `rig.json` → Rig,
  `sprites/<name>.png` → Part, portrait paths → Portraits category,
  else display-name match). Folders toggle expand. Drag-reparent is off
  (`UiTree.SetReorderable(false)`).
- File Tree is disk; Node Tree is logical. `FileBrowserPanel` stays the
  modal for Save/Import/Atlas.
- View → File Tree focuses the tab (`LabShell.FocusPanel`).
- `UiTree.OnRowActivated` / `SetReorderable`. LokrLab **0.7.0**,
  SimpleUI **1.1.6**. Full solution build verified clean.

**Phase 8 — Sandbox fight round-trip** (done 2026-08-13):
- Setup tab was already in-shell (Phase 5). Sandbox Enter Fight still uses
  `CloseTo("fight")`. Ability Lab Stage later embeds the same arena
  additively in the dock hole (§6.2).
- `OnFightEnded` calls `CharacterLabScene.ReopenAfterFight` instead of
  `TransitionToNextScene` back to the original origin. The fight scene
  is unloaded; the lab is rebuilt; `CurrentSession` is unchanged so the
  shell (Node Tree, File Tree, docks) comes back. Lands on the Sandbox
  workspace. Close Lab still returns to the pre-lab origin
  (`SandboxSession.ReturnScene`).
- LokrLab **0.8.0**. Full solution build verified clean.

**Phase 9 — Ability Library as its own project type** (done 2026-08-13):
- `LokrAbilityLab` depends on `LokrLabApi` (not `LokrLab.dll`). Registers
  singleton `ability-library` (`IsSingleton`, FolderRoot =
  `Mods/LokrAbilityLab/Abilities`). Node Tree is one root plus one
  Ability node per folder; Inspector hosts the existing envelope + raw
  KV form (`AbilityEditorPanel.BuildInto`). Library workspace is a short
  prompt. File → New Ability.
- `LokrLabApi.JumpToProject` / `ReturnToPreviousProject` / `RequestRefresh`
  (shell-assigned). File → Back to Previous Project. Workspace tabs
  rebuild when the project type changes (`UiToolbar.Clear`).
- Character Node Tree gains an Abilities branch from skills /
  defaultSkill / skillProgression. Inspector Open (or double-click)
  jumps into the library with that ability selected.
- Mod-menu Ability Lab opens the shell library when the jump hook is
  assigned; overlay remains the fallback. `RegisterMenu` is idempotent
  so both plugins can contribute File items.
- LokrLab **0.9.0**, LokrLabApi **1.3.0**, LokrAbilityLab **0.2.0**,
  SimpleUI **1.1.7**. Full solution build verified clean.
- Follow-up (2026-08-13): bottom panels rebuild with the project type.
  Ability Library no longer keeps Character's Timeline / Checklist /
  History. LokrLab **0.9.1**.

**Phase 9.5 — Character as its own plugin** (done 2026-08-13):
- `LokrCharacterLab` (`com.lokrmodding.characterlab`, **0.9.10**) owns
  `Editor/`, `Projects/`, `CharacterCreatorAPI`, sandbox fight hooks.
  No `ProjectReference` to `LokrLab.dll`.
- `LokrLabApi` **1.4.0**: `LabHost`, `LabSceneContext`,
  `PersistentInspectorRegistration`, `LabOpened` / `LabClosing` /
  `ShellShown` / `ScreenShown`, `PromptLegacyImport`, `ScanCategory`,
  `OnSelectionChanged`, `OnNodeActivated`, bottom-panel Refresh/Unbind.
- `LokrLab` **0.10.0** is host-only: scene, Project Browser, docks,
  shell File/View/Help menus. Inspector loops persistent hosts.
- Public Character APIs stay in namespace `LokrLab`; third parties
  change their project reference / `[BepInDependency]` to
  `LokrCharacterLabPlugin.Guid`.

**Phase 10 — Encounter Creator as the second "many-projects" project
type** (§6.3): the actual validation of §2's generalization — a project
type that is neither Character's "many folders" shape exactly unmodified
nor Ability Library's singleton shape, and that genuinely needs
cross-project references (§2.5) rather than only benefiting from them
incidentally. **Implementation plan:**
[encounter-creator.md](encounter-creator.md) (Phase 3 type + save in
0.12.36). Corresponds to [phasing.md](../phasing.md) item 7. Unblocked:
Phase 9, Sandbox embed, suite merge, lab save UX.

---

## 9. Risks

| Risk | Mitigation |
|---|---|
| Phase 0's SRP extraction touches the largest, most load-bearing file in the plugin (`RigEditorScene`, ~3,370 lines) before any user-visible payoff exists | Its own mergeable unit with its own verification pass — existing Animator behavior provably unchanged before Phase 1 starts |
| A dispatch-based Inspector regresses `InspectorPanel`'s already carefully-tuned playback-tick refresh behavior | Phase 4 scoped as an explicit port-with-regression-pass, not a rewrite |
| The project-type abstraction (§2) is designed against only one real consumer (Character) until Phase 9/10 — risk of over-fitting its shape to Character's needs without noticing | Ability Library (Phase 9, singleton shape) and Encounter Creator (Phase 10, cross-referencing many-projects shape) are deliberately structurally different from Character and from each other — treat *both* as required validation before calling `ProjectTypeRegistration`'s shape stable, not just Ability Library |
| Renaming `LokrCharacterLab` → `LokrLab` mid-solution touches a large number of cross-linked docs and every `[BepInDependency]` reference to its GUID | Do it exactly once, at Phase 2, per §2.6 — not before the shell exists to justify the new name, not deferred past it |
| Drag-and-drop docking is new interaction surface with no precedent in this codebase | Build and prove `UiDockSpace`'s drag-and-drop in a minimal SimpleUI-only test scene (Phase 1) before any real panel depends on it |

---

## 10. Discussion — decisions record

Every discussion point raised while planning this doc is now resolved.
Kept as a record (not deleted) so the reasoning behind each stays visible
to anyone picking this up later — per this repo's own documentation
culture of citing *why*, not just *what*.

1. **Floating panels** (§5.1) — **Decided: no floating panels at all**,
   redock-only. Simpler than this doc's original overlay-vs-multi-window
   framing — sidesteps the question entirely.
2. **Node tree schema** (§5.2.1) — **Decided: start with (A)
   presentation-layer-only**, revisit a real persisted schema (B) once the
   tree has real usage behind it.
3. **Ability Lab contract placement** (§6.1) — **Decided**, and updated in
   shape given the project-type generalization: a new dedicated
   `LokrLabApi` contracts plugin, not `LokrCharacterLoader`.
4. **Ability Lab integration depth** (§6.1) — **Decided: deep** — resolved
   into "its own project type" under the new model, not "a node-tree
   branch grafted into Character's tree."
5. **Sandbox embedding** (§6.2) — **Sandbox Enter Fight stays a scene
   jump.** Ability Lab Stage now researches an additive embed of the same
   fight scene in a workspace hole (`LabHost` contract; Sandbox and Stage
   are both embed-only).
6. **Project folder layout** (§2.3) — **Decided: per-type root folders**
   (`Characters/`, `Encounters/`, …), not one unified `Projects/<id>/`
   root.
7. **Project discovery + cross-project navigation** (§2.4, §2.5) —
   **Decided: the Project Browser actively scans every registered type's
   folder root for `project.json` markers** (not just a recalled
   `recent.json` list), and jumping into a cross-referenced project
   **switches** the active session rather than opening a second pinned
   one. Multi-project split-view is explicitly deferred to later, not
   built alongside v1 — the auto-scanning browser makes switching back and
   forth cheap enough that v1 doesn't need it.
8. **Rename timing** (§2.6) — **Decided: at Phase 2**, as recommended.
9. **"Workspace" vs. "workstation"** — **Decided: keep both, deliberately
   distinct** (§5's terminology note) — "workspace" for the new concept,
   "workstation" only for today's pre-redesign classes/screens being
   migrated away from.
10. **Runtime content ownership** (§2.7) — **Done, 2026-08-12.** Moved
    `AbilitiesContribution` from `LokrAbilityLab` into
    `LokrCharacterLoader` as `CustomRigs/AbilityLabContentLoader.cs`,
    mirroring `CharacterLabContentLoader`'s existing shape, so ability
    content loads for play-only users without `LokrAbilityLab`
    installed — matching what was already true for characters. Shipped
    standalone, ahead of the rest of Phase 0, exactly as flagged.

Nothing in this doc currently blocks starting Phase 10.

---

## Revision log

| Date | Change |
|---|---|
| 2026-08-12 | Initial roadmap, written against the completed hub v1 + pre-redesign audit |
| 2026-08-12 | Clarified "no schema changes" scope: specifically about `rig.json`'s base-game-parser constraint, not a blanket freeze on Lab-owned formats |
| 2026-08-12 | Major revision: generalized from a Character-specific redesign into `LokrLab`, a project-type framework (§2) with Character as the first project type; recorded resolved discussions (floating panels, node-tree schema sequencing, Ability Lab contract placement/depth, Sandbox embedding); updated Ability Lab's contract-placement recommendation from `LokrCharacterLoader` to a new dedicated `LokrLabApi` plugin given the generalization; added Encounter Creator as Phase 10, the model's actual validation |
| 2026-08-12 | Resolved remaining open discussion points (§10): per-type folder roots, Project Browser auto-scan + switch-not-split-view navigation, rename at Phase 2, and settled "workspace" (new concept) vs. "workstation" (pre-redesign classes) terminology — every decision this doc raised is now recorded, none left open |
| 2026-08-12 | Added §2.7: runtime content (characters, abilities) should be loadable by players with only `LokrCharacterLoader` installed, not the authoring plugins. Verified against actual source: characters already work this way (`CharacterLabContentLoader` already lives in `LokrCharacterLoader`); abilities don't (`AbilitiesContribution` still lives in `LokrAbilityLab`) — real gap, fix scoped and folded into Phase 0, flagged as independently shippable |
| 2026-08-12 | **Phase 0 started:** shipped §2.7's fix — `AbilitiesContribution` moved from `LokrAbilityLab` to `LokrCharacterLoader/CustomRigs/AbilityLabContentLoader.cs`, plus a new shared `LocaleFileSuffixes.cs` deduplicating what would've been two identical locale tables in one assembly. Solution builds clean. Docs updated across `LokrCharacterLoader`, `LokrAbilityLab`, `mods-folder-structure.md`, and the pre-redesign audit doc |
| 2026-08-12 | **All four audit P0 items (C-01–C-04) fixed** — new `LokrModAPI.Serialization.TextEscaping`, `RigEditorScene.WriteAllTextAtomic`/`ResetSession()`, and a server-side `RequiresCharacterLoaded` guard in `SwitchToWorkstation`. Full solution build verified clean throughout. Docs updated: the audit doc's own C-01–C-04 rows, checklist, and revision log |
| 2026-08-12 | `CharacterSession` extracted from `HomeWorkstationScene` (P2 target split, item 1 of 4) — deliberately scoped separately from the much larger/riskier `RigLoadService`/`RigSaveService`/`RigPreviewService`/`CharacterProfileService` extraction, per user direction to keep this pass to what's verifiable by compiling alone. `docs/api/classes.json` and the generated API docs site also brought back in sync (new classes added, stale `AbilitiesContribution` page removed, descriptions re-synced) — first time this session that step was needed |
| 2026-08-12 | **Remaining P2 extraction items completed, at user direction to proceed despite the compile-only-verification risk** — `CharacterProfileService` (HomeWorkstationScene), then `RigPreviewService`/`RigSaveService`/`RigLoadService` (RigEditorScene, most-self-contained-first order). `RigEditorScene.cs` ~3,370 → ~2,820 lines; `OnLoadClicked`/`OnSaveClicked` deliberately stayed in place (see the audit doc's own P2 section for why). Phase 0 now complete except UI-paradigm consolidation. `docs/api/classes.json` + generated site kept in sync after each extraction |
| 2026-08-13 | **C-UI-01 (three UI paradigms) fixed — Phase 0 now fully complete.** The "three paradigms" premise itself was stale: `EditorUiHelpers.cs` (209 lines) had zero real callers anywhere in the solution, and `CharacterLabScene.CreateInputField` was likewise dead — both fully deleted. Only 5 real call sites remained for `CreateLabel`/`CreateButton` (canvas-level chrome needing an absolute anchor point); migrated all 5 to `UiLabel.Create`/`UiButton.Create` with `RectTransform` set directly afterward, matching the old theme colors/fonts exactly. Full solution build verified clean. Docs updated: `architecture.md`, `conventions.md`, `supporting-classes.md`, `animation-data-model.md` (LokrCharacterLab), `SimpleUI/docs/cross-references.md`, `docs/api/classes.json` (removed stale `EditorUiHelpers` entry + orphaned HTML page), the pre-redesign audit doc's C-UI-01 row/checklist/revision log, and `docs/roadmaps/README.md` |
| 2026-08-13 | **Phase 1 complete — SimpleUI docking primitives.** Added `UiSplitter`, `UiTabGroup`, `UiDockPanel`, `UiDockSpace` (+ `DockZone` / `DockLayoutSnapshot` / `DockZoneSnapshot`), `UiTree` / `UiTreeItem`, `UiContextMenu`, `UiToolbar`, `UiStatusBar`. `DockingSmokeTest` overlay (F8 + mod-menu button) proves splitter resize, tab reorder, drag-to-redock, tree multi-select/reparent, and context menus independently of any Lab workstation. Also used the pass to sync stale SimpleUI docs (`UiList<T>` is generic; `UiComboBox` exists; widgets use `Text` not TMP). SimpleUI version 1.1.0. Full solution build verified clean. |
| 2026-08-13 | **Phase 2 complete — project-type framework + shell + rename.** New `LokrLabApi` plugin; `LokrCharacterLab` renamed to `LokrLab` (GUID `com.lokrmodding.lab`, write root `Mods/LokrLab/`); `project.json` marker + boot migration; Project Browser empty state; `UiDockSpace` shell with placeholder panels; Character registered as the first project type. Legacy Home/Properties/Animator/Sandbox stay on File. Full solution build verified clean. Docs updated across `LokrLab`, `LokrLabApi`, `ARCHITECTURE.md`, `CLAUDE.md`, `docs/README.md`, `mods-folder-structure.md`, and `docs/api/classes.json`. |
| 2026-08-13 | **Phase 3 complete — Node Tree + selection.** Character contributors populate Rig/Parts (SceneTreePanel port) and Animator/Clips from `rig.json`; `EditorSelection` is written from the tree; Add Node factories are registered and insert into empty/unauthored rigs only. Inspector still a selection summary. `LokrLabApi` 1.1.0 (`Set`/`Clear`, `FactoriesForParent`). Full solution build verified clean. |
| 2026-08-13 | **Phase 4 complete — Inspector registry.** `InspectorDock` dispatches registered drawers by `LabNode.Kind` (rebuild on selection identity only). Character drawers port Part / AnimationClip / Frame / Reference plus Character / Rig / Animator summaries; live Animator fields stay on `InspectorPanel` so playback-tick row reuse and focus-skip are unregressed. `InspectorPanel` maps `InspectorTarget` to the same kind strings and stacks extra sections only when kind+id changes. `CharacterCreatorAPI` forwards drawer/section registration. LokrLab 0.4.0. Full solution build verified clean. |
| 2026-08-13 | **Phase 5 complete — workspaces.** Properties and Animator are shell tabs (`WorkspaceRegistration` + `BuildViewport` / `BuildToolbar`). Home is retired (`SwitchToHome` → shell). Properties categories are Character children with a persistent inspector host. Animator cameras bind to the center dock; live InspectorPanel + tool strip stay in-shell. `FindWorkspace` on `ProjectTypeRegistration`. LokrLab 0.5.0, LokrLabApi 1.2.0. Full solution build verified clean. |
| 2026-08-13 | **Phase 6 complete — bottom panels + top menu.** Timeline / Checklist / History registered on Character and hosted in the bottom dock (`isRelevant` auto-focuses, never hides). File / Edit / View / Help via `LokrLabApi` menus. `UiDockSpace.SelectPanel`. LokrLab 0.6.0, SimpleUI 1.1.3. Full solution build verified clean. |
| 2026-08-13 | **Pin/close chrome (Phase 6 follow-up).** Closable docks use right-click Pin/Unpin + Close and middle-click close; pinned tabs get a left accent, not a P button. Lab panels stay `closable: false, pinnable: false`. SimpleUI 1.1.5. |
| 2026-08-13 | **Phase 7 complete — File Tree dock.** Left-dock File Tree lists the open project folder; double-click selects a matching Node Tree row. `UiTree.OnRowActivated` / `SetReorderable(false)`. View → File Tree. LokrLab 0.7.0, SimpleUI 1.1.6. Full solution build verified clean. |
| 2026-08-13 | **Phase 8 complete — Sandbox fight round-trip.** Fight end reopens the lab shell on the Sandbox tab with the same project (`ReopenAfterFight`); Close Lab still returns to the pre-lab origin. LokrLab 0.8.0. Full solution build verified clean. |
| 2026-08-13 | **Phase 9 complete — Ability Library project type.** Singleton `ability-library` in the shell (Node Tree + inspector form). Character Abilities refs jump via `JumpToProject`. Overlay is fallback. LokrLab 0.9.0, LokrLabApi 1.3.0, LokrAbilityLab 0.2.0, SimpleUI 1.1.7. Full solution build verified clean. |
| 2026-08-13 | **Phase 9 follow-up — per-type bottom panels.** `LabShell` rebuilds the bottom dock when `ProjectTypeId` changes. Ability Library hosts none (zone collapses); Character keeps Timeline / Checklist / History. Character-only View/Edit items gated. LokrLab 0.9.1. |
| 2026-08-13 | **Animator viewport + inspector.** Viewport dock background is cleared so Main/Preview cameras show through. Live `InspectorPanel` uses `BuildInto` (no nested scroll). LokrLab 0.9.2. |
| 2026-08-13 | **Animator viewports via RenderTexture.** Overlay canvas still hid `Camera.rect`; each slot now shows a `RawImage` of that camera. Timeline clip picks sync the Node Tree. LokrLab 0.9.3. |
| 2026-08-13 | **Viewport host no longer a UiStack.** ContentSizeFitter was collapsing the camera split (RawImage had 0 height). Stretch `RectTransform` host + growing split. LokrLab 0.9.4. |
| 2026-08-13 | **Preview as PIP.** In-engine preview is a small bottom-right overlay on the edit viewport, toggleable from the toolbar and View → Preview. LokrLab 0.9.5. |
| 2026-08-13 | **Camera.rect viewports again.** Stretch host + cleared LabBackdrop lets cameras show through; click-to-select works. Node Tree multi-select tints every Part. LokrLab 0.9.7. |
| 2026-08-13 | **Viewport veil.** Shell root / dock / Viewport panel Images are cleared so cameras are not seen through a 94% dark parent. LokrLab 0.9.8. |
| 2026-08-13 | **Properties inspector fields.** Properties host is a Grow() scroll (no nested ScrollRect); `Show` matches category Name or DisplayLabel. LokrLab 0.9.9. |
| 2026-08-13 | **Nested-scroll collapse documented.** SimpleUI + LokrLab conventions: one ScrollRect on the sized host. Audit of every scrollable stack/list; atlas picker split now relaxes its parent fitter. LokrLab 0.9.10. |
| 2026-08-13 | **Phase 9.5 — Character split.** New `LokrCharacterLab` sibling plugin (0.9.10). LokrLab 0.10.0 is host-only. LokrLabApi 1.4.0 adds Host, lab-scene events, persistent inspectors. Full solution build verified clean. |
| 2026-08-13 | **Menu context.** `RegisterMenuItem` gained `isVisible`; File/Edit/View items appear only in their session/workspace. LokrLabApi 1.4.1, LokrLab 0.10.1, LokrCharacterLab 0.9.11, LokrAbilityLab 0.2.2. |
| 2026-08-13 | **New Project wizard.** `ProjectTypeRegistration.BuildCreateSheet` / `CommitCreateSheet`; Project Browser always prompts for type + that type's init sheet. New characters are a blank slate (no Ranger/HumanArcher defaults); Properties categories register at plugin Awake so the Node Tree lists them immediately. LokrLabApi 1.4.2, LokrLab 0.10.3, LokrCharacterLab 0.9.15. |
| 2026-08-13 | **Delete + Ability in New Project.** Project Browser delete (confirm) and File → Delete Project for non-singleton rows. Ability Library appears in the New Project type list with an ability-id sheet. LokrLabApi 1.4.3, LokrLab 0.10.4, LokrCharacterLab 0.9.16, LokrAbilityLab 0.2.3. |
| 2026-08-13 | **Close Project then reopen NRE.** Close Project destroyed InspectorDock persistent hosts but left Properties list refs; Load refreshed them and `UiList.SetItems` NRE'd. Reset hosts on Browser; rebuild if destroyed. LokrLab 0.10.5, LokrCharacterLab 0.9.17. |
| 2026-08-13 | **Close Lab on Project Browser.** The browser screen has no shell File menu; it now has a Close Lab button that returns to the origin scene. LokrLab 0.10.6. |
| 2026-08-13 | **Removed docking smoke-test.** Deleted `DockingSmokeTest`, the F8 / config toggle, and the **SimpleUI Docking Test** mod-menu button. SimpleUI 1.2.1, LokrLab 0.10.7. |
| 2026-08-13 | **Removed Ability Lab mod-menu button.** Open the library from LokrLab (Project Browser / New Project). Overlay remains a fallback if the shell jump hook is missing. LokrAbilityLab 0.2.4. |
| 2026-08-13 | **Project Browser sections + type filter.** List is grouped by project type; Show toggles hide types (`HiddenProjectTypes` config). LokrLab 0.10.8. |
| 2026-08-13 | **Recent section + multi-library abilities + plugin-named folders.** Project Browser lists Recent first. Ability Lab is many named libraries (`LokrAbilityLab/<libraryId>/`), not a singleton. Character/ability loader scans use `LokrCharacterLab` / `LokrAbilityLab` instead of generic `Characters` / `Abilities`. LokrLab 0.10.9, LokrAbilityLab 0.3.0, LokrCharacterLab 0.9.18, LokrCharacterLoader 1.0.1. |
| 2026-08-13 | **Ability Lab Stage embed.** Additive `fighttesterempty` fight in the Stage hole via `LabHost.StartEmbeddedFight` (Character Lab). Isolation off while live; mannequin Play is the fallback. LokrLabApi 1.4.4, LokrCharacterLab 0.9.23, LokrAbilityLab 0.8.9. |
| 2026-08-13 | **Stage hole HUD fit.** Fight HUD is refit every LateUpdate to the hole (`ConstantPixelSize`); mannequin Stage controls hide while the fight is live. LokrCharacterLab 0.9.28, LokrAbilityLab 0.8.13. |
| 2026-08-13 | **Stage hole crop + turn recovery.** HUD fitter no longer stomps `Camera.rect` / canvas enable; binder writes the hole last; `AspectUtility` skipped; skills/End Turn reset on `FightStartTurn`. LokrCharacterLab 0.9.29, LokrAbilityLab 0.8.14. |
| 2026-08-13 | **Stage hole height.** Hiding mannequin chrome collapsed the empty hole `UiStack` to 0px so `Camera.rect` never cropped. Hole is now a `StageHole` panel. LokrAbilityLab 0.8.16. |
| 2026-08-13 | **Embed dump removed.** Temporary `embedded-fight-dump.log` instrumentation is gone. LokrCharacterLab 0.9.31, LokrAbilityLab 0.8.17. |
| 2026-08-13 | **LokrLab scene embed.** Generic `StartEmbeddedScene` (hole crop, HUD fit, AspectUtility skip) lives on LokrLab. Character Lab fight spawn calls it. Embedded fight pan is right/middle-drag only. LokrLabApi 1.5.0, LokrLab 0.11.0, LokrCharacterLab 0.9.32, LokrAbilityLab 0.8.18. |
| 2026-08-13 | **Mannequin Stage removed.** Ability Lab Play is embed-only; dummy C/T standees, hex board, and isolation Harmony are gone. Failures stay as status text. LokrAbilityLab 0.8.20, LokrLab 0.11.5. |
| 2026-08-13 | **Embed camera reset.** Stop waits for unload before the next Start; `CameraBase.mainCamera` is the hole camera so hex auto-pan does not keep the previous offset. LokrLab 0.11.6, LokrCharacterLab 0.9.39. |
| 2026-08-13 | **Sandbox uses fight embed.** Character Lab Sandbox Start sandbox loads the additive fight in a `SandboxHole` (same path as Ability Lab Stage). Scene-jump `CloseTo("fight")` / `ReopenAfterFight` / `SandboxFightHooks` are gone. LokrCharacterLab 0.9.40. |
| 2026-08-13 | **Sandbox / Stage hero level.** `EmbeddedFightRequest.CasterLevel` walks `nextLevelArchetype` and grants that rank's skillProgression options. LokrLabApi 1.5.1, LokrCharacterLab 0.9.41, LokrAbilityLab 0.8.22. |
| 2026-08-14 | **Confirm / cast boot after Stop.** Character Lab rebinds WorldSpace confirm canvases to the hole camera (`BindConfirmCanvases`). LokrCharacterLab 0.9.45. LokrLab 0.11.8, LokrCharacterLoader 1.1.4 (duplicate roster ids skipped), LokrPatch 1.0.4 (metagame instantiating flag). |
| 2026-08-16 | **Phase 10 plan extracted.** Encounter Creator is its own roadmap ([encounter-creator.md](encounter-creator.md)); this file keeps the shell/project-type rationale only. |
