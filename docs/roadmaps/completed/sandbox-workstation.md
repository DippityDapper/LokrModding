# Sandbox Encounter workstation

**Current (2026-08-14):** Sandbox Start sandbox embeds `fighttesterempty`
in a `SandboxHole` via `LabHost.StartEmbeddedFight` (same path as Ability
Lab Stage). The lab stays open. Fight-end does not call
`ReopenAfterFight`. The v1 scene-jump notes below are historical.

*A live, player-controlled test encounter for everything built in the
other workstations — animations, abilities, AI, and any other character
feature — running together in the actual game engine, not a mock.
Built 2026-08-12 as a workstation in the Character Creator hub (not a
separate plugin like `LokrAbilityLab`). **Verified in-game 2026-08-12**:
hero and one enemy spawn correctly positioned and facing each other, the
hex grid/turn order/debug panel all work as intended.*

- The player fully controls the encounter **live**: spawn enemies, objects,
  and obstacles on demand, at will, mid-session — this is a sandbox for
  *iterating*, not a scripted test scenario.
- Depends on the Animator (§5, in this hub) and `LokrAbilityLab` (§6, a
  separate plugin) already producing real, loadable content (a rig + at
  least one ability) — the Sandbox is where they get exercised together,
  not where they're authored.

### v1 implementation — far more tractable than originally assumed

Four research passes into decompiled source (nothing about combat/
encounter mechanics had ever been investigated anywhere in this project
before this — the only prior combat-adjacent code anywhere in this
solution was a defensive `MonoSingleton<LevelManager>.IsInstanceValid`
guard used to avoid touching state during a real fight, never to start
one) found the base game already ships almost everything needed:

- **Unit spawning is a solved, already-proven operation**:
  `Stage.instance.AddUnit(new Unit(pos, flipped, "", UnitClass.Generic,
  group, definition))` is called live, mid-fight, by three independent
  base-game systems (hero revive, the turn marker, and the base game's
  own hidden debug panel's own "SPAWN" tool). Turn-order registration is
  automatic — `NewInitiativeHandler` listens for `EntityAddedEvent` and
  self-inserts the new unit, no manual initiative bookkeeping needed.
- **Exactly one combat scene exists** (`KRLegendsFightGameplay02`,
  `SceneDB.FIGHT`) for the entire game — every quest/arena/boss fight
  reuses it, with per-encounter content assembled at runtime from data
  (a `MapQuestStatus.encounters` list of room-template names loaded by
  string from the shared `"templates"` asset bundle), not baked into
  unique scene content. Nothing structurally prevents a synthetic
  "sandbox" encounter.
- **The base game's own debug tooling already does almost exactly
  this**: `MapDebugPanel`'s "Random Fight"/"COOL FIGHTS" tools start an
  arbitrary quest via `CinematicMapHelper.EnqueueEphemeralQuest(questName)`
  + `TransitionSceneComponent.TransitionToNextScene(...)`, bypassing the
  normal map-node flow entirely — and `FightDebugPanel` provides a
  complete, working "spawn any unit at the cursor," take-over-AI,
  heal/kill-active, reset-cooldowns toolset on top of the same
  `Stage.AddUnit` primitive. Both panels are **fully compiled into the
  shipped game**, not stripped — the only thing hiding them is
  `CheatDebugController.DEBUG_PANEL_ENABLED`, a public static `bool`
  that defaults `false` and is never set `true` anywhere in shipped
  code.
- **User-approved scope, given the above**: bypass the base game's real
  party/roster system entirely (enter with an empty party, spawn only
  the Lab hero directly), and **reuse the base game's own hidden debug
  panel wholesale** for the actual spawn-enemies/test-tools UI instead of
  building a new one — force-enabled
  (`CheatDebugController.DEBUG_PANEL_ENABLED = true` +
  `DebugPanelController.GetDebugPanel().rootContent.SetActive(true)`)
  by `SandboxFightHooks.OnFightStarted` once the fight actually starts.
  This closes the "spawn menu (enemy type / object / obstacle picker)"
  v1 scope this section originally called for — not with new UI, with
  the base game's own equivalent tooling, already built and proven.
- **The arena is a genuinely empty template, not a borrowed real quest's
  roster** — `SandboxWorkstationScene.OnEnterClicked` still needs *some*
  real `MapQuestDefinition` to anchor the ephemeral `MapQuestStatus.quest`
  reference (several downstream reads dereference it unconditionally),
  but picks it arbitrarily now (`FindHostQuestDefinition`, first result)
  since its own `encounters` list is discarded. `MapQuestStatus.encounters`
  is instead hardcoded to `"fighttesterempty"` — a real, shipped template
  found by inspecting the game's own `templates` asset bundle with
  [AssetStudioModCLI](reference/README.md) (see that doc's own "Tooling
  used" section) and confirmed via a JSON-tree dump to have zero heroes/
  enemies/cinematic units in its `EncounterDefinition`, on a normal,
  fully-walkable 16x20 hex board — apparently the base game's own arena
  for its internal `FightTesterOverride` dev tool. `SandboxFightHooks.
  OnFightStarted` spawns the Lab hero plus one hardcoded `"BanditRaider"`
  enemy (also verified to exist via the same tooling, from a real quest's
  spawn data) a couple of hex steps apart on either side of the board's
  center, facing each other.
  - **Earlier approach and why it was replaced**: v1 first tried borrowing
    a real single-encounter quest (the `.encounters.Count == 1` filter the
    base game's own "Random Fight" debug button uses) and stripping its
    roster down to one enemy after the fact. This surfaced two real bugs
    before landing on the empty-template approach above: (1) removing the
    quest's real enemies inside `OnFightStarted` left a correct
    `Stage.units` list but a stale/duplicated initiative bar, because
    `NewInitiativeHandler` registers its own `FightStartedEvent` listener
    the moment `Stage` is constructed — well before Sandbox's own
    listeners — so it always snapshots and sorts the original roster
    first; and (2) even after moving the stripping earlier (an
    `EntityAddedEvent` listener removing enemies as they spawned, before
    the fight-start sort), the same listener stayed registered when
    `OnFightStarted` added the sandbox's own hero and enemy, immediately
    stripping those right back out too and triggering an instant
    `FightEnded` (both sides at 0 units). Switching to a template with
    nothing to strip in the first place sidesteps both failure modes
    entirely, rather than requiring ever-more-careful listener sequencing.
- **Real correctness gotcha, load-bearing, not optional**:
  `Stage.CheckFightEnd()` runs every `Stage.Update()` tick and triggers
  an **instant automatic Defeat** if the `GoodSide` unit count is ever 0
  while fighting is active. The Lab hero is spawned **synchronously
  inside the `FightStartedEvent` handler** (`SandboxFightHooks.
  OnFightStarted`, which fires during `Stage.StartFight()`, strictly
  before the next `Update()` pass) — not deferred to a coroutine or next
  frame, or the sandbox would auto-lose the instant it opens.
- **Built on top of Character Lab's own new real-scene-transition model**
  (see §11's own entry on that): entering Sandbox calls
  `CharacterLabScene.CloseTo(SceneDB.GetScene("fight"))` instead of the
  lab's own normal `Close()`. `SandboxSession` carries the real origin
  scene (`CharacterLabScene.OriginScene`) so Close Lab after the fight
  still returns there. `OnFightEnded` calls
  `CharacterLabScene.ReopenAfterFight` — unloads the fight, rebuilds the
  dockable shell on the Sandbox tab, same open project (editor-redesign
  Phase 8). Not an embedded fight viewport.
-   No save/replay of a configured encounter for v1 — that's the
  [Encounter Creator](../started/encounter-creator.md). Encounter is
  a separate project type; Sandbox may later load those projects to
  play them, but does not author them.

### Extensibility

Deferred, not designed yet: since v1 reuses the base game's own hidden
debug panel wholesale rather than building a first-party spawn menu, the
"a plugin should be able to register additional spawnable enemy/object/
obstacle types" extension point this section originally called for has
no first-party UI to extend yet. Revisit once/if a dedicated Sandbox UI
replaces the reused debug panel — see
[encounter-creator.md](../started/encounter-creator.md). Encounter
Lab is a separate project type. Sandbox may load an authored encounter
to play it; it does not grow an encounter editor of its own.

