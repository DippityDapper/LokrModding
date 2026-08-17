# FightStartedHandler NREs on an empty initiative list

Area: vanilla `NewInitiativeHandler.FightStartedHandler` / `Stage.StartFight`;
hurts LokrLab Sandbox / Ability Lab Stage (`EmbeddedFightHost` +
`fighttesterempty`).
Status: resolved

As of 2026-08-15 (Pass B, Game HTML): `FightStartedHandler` calls
`ActiveUnit.IncreaseTurnCount()` with no null check. `ActiveUnit` is
null when `units` is empty. `Events.Raise` is a multicast with no
per-listener try/catch, so that NRE aborts later `FightStartedEvent`
listeners — including `EmbeddedFightHost.OnFightStarted` (Sandbox
spawn). `Stage.StartFight` then also NREs on
`ActiveUnit.TurnStarted()` if the list is still empty after the event.

Lab's `fighttesterempty` template has zero encounter units. Spawn is
meant to happen in the later listener. That listener only runs if
something (leftover party heroes from `LevelManager.StartEncounter`)
is already on the board, or if `FightStartedHandler` is guarded.

As of 2026-08-15 in-game: Character Lab Sandbox and Ability Lab Stage
start with no FightStarted / StartFight NRE. A campaign fight-node
loading overlay in the same first session was a different bug
([campaign-fight-loading-stuck.md](campaign-fight-loading-stuck.md)),
now also confirmed clear. Progression-help Next was a separate bug
([`progression-help-popup-index-oor.md`](progression-help-popup-index-oor.md)),
now also confirmed.

Suggested fix: null-check `ActiveUnit` in `FightStartedHandler` (and
`StartFight`) via a Harmony prefix, or spawn the Lab roster before
`StartFight` instead of on `FightStartedEvent`.

## Proposed solution (Pass 1, 2026-08-15)

**Home:** LokrPatch
**Approach:** LokrPatch Harmony prefixes null-check `ActiveUnit` in `NewInitiativeHandler.FightStartedHandler` and `Stage.StartFight` so `Events.Raise` (multicast, no per-listener try/catch) can still reach later `FightStartedEvent` listeners such as `EmbeddedFightHost.OnFightStarted`. That guard alone is not enough for Lab: `FightStartedHandler` sets `fightStarted = true` before Lab spawns, and `EntityAddedEventHandler` then `Insert`s and increments `index`, leaving `ActiveUnit` null (`index >= units.Count`) so `StartFight` still NREs on `TurnStarted`. LokrLab must also spawn the Sandbox roster in a higher-priority prefix on `Stage.StartFight` (while `fightStarted` is still false, so units are `Add`ed) and keep OnFightStarted as a spawn fallback with a once-flag.
**Exact change:** (1) `LokrPatch/Patches/FightStartedEmptyInitiativePatch.cs`. Prefix `NewInitiativeHandler.FightStartedHandler(FightStartedEvent)` — reimplement (randomize unless `testSkipRandomize`, sort by initiative, `fightStarted = true`, `index = 0`) and call `ActiveUnit.IncreaseTurnCount()` only when `ActiveUnit != null`; else `LogWarning` and skip that call; return `false`. Prefix `Stage.StartFight()` — reimplement: `isFighting = true`; `Events.instance.Raise(new FightStartedEvent(__instance))`; if `initiative.ActiveUnit != null` then `TurnStarted(active)` and `active.TurnStarted()`, else log and skip both (vanilla `Stage.TurnStarted(Unit)` NREs on `activeUnit.states` too); still assign `dustParticlesPrefab` and `unitController`; return `false`. (2) LokrLab companion, required: Harmony prefix on `Stage.StartFight` with priority higher than the LokrPatch replacement (e.g. 600 vs default) so it runs first and returns `true`. When `EmbeddedFightHost.IsActive`, call `SandboxFightControls.Begin` + `SandboxRoster.SpawnHeroAndEnemy` once (static spawned flag), then let LokrPatch/vanilla continue. Move that spawn out of `OnFightStarted`; `OnFightStarted` only binds camera / `OnReady` / `Finish`, and calls the same spawn helper if the flag is still clear (fallback if StartFight was not the entry). Do not wrap `Events.Raise` in try/catch.
**Do not:** Patch `Events.Raise` globally. Reset `initiative.index` as the only Lab fix (hides the ordering bug). Spawn twice. Skip `StartFight` entirely in Lab. Change `fighttesterempty`. Touch `EndTurnNowCorroutine`'s later `ActiveUnit.TurnStarted()` (different empty-list case after combat has begun).
**In-game verify:** 1. Build and launch via Steam / Proton. 2. Character Lab → Sandbox → Start sandbox on `fighttesterempty` (no leftover party if possible). 3. Confirm `LogOutput.log` has no `FightStartedHandler` / `StartFight` NRE, hero and enemy appear, first turn HUD/skills work. 4. Ability Lab Stage with the same empty template: same. 5. Start a vanilla campaign fight and confirm initiative / first turn unchanged. 6. If a LokrPatch warning about empty ActiveUnit appears in Lab, spawn-before-StartFight did not run — fix Lab, do not ignore.
**Risk:** Vanilla encounters always have units, so the null path should not run in campaign. Lab spawn-before-StartFight changes when units enter initiative (Add vs Insert-after-fightStarted), which is the correct empty-board path and should not affect vanilla. No save data.

Resolved: 2026-08-15

Resolution: Confirmed in-game. Lab Sandbox and Ability Lab Stage start
with no FightStarted / StartFight NRE (LokrPatch empty-`ActiveUnit`
guard + LokrLab spawn-before-`StartFight`). Campaign fight overlay that
blocked the first campaign check is
[campaign-fight-loading-stuck.md](campaign-fight-loading-stuck.md)
(also confirmed). Progression-help Next is
[`progression-help-popup-index-oor.md`](progression-help-popup-index-oor.md)
(also confirmed).
