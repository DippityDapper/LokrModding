# Sandbox Load Encounter: hero death does not end the fight

Area: LokrLab Sandbox (`EncounterSandbox`, `EncounterExplorationFightEndPatch`)
Status: resolved

As of 2026-08-17: after exploration parked correctly on Sandbox → Load
Encounter, the open character died and the fight stayed up. Sandbox
should treat living GoodSide == 0 as a loss (`Stage.CheckFightEnd`),
including during exploration (the fence must not block a party wipe).

A third Sandbox-load arm had been drifting from the live-fight path
(tiles, then exploration). Win/lose was the next split: the exploration
fence counted any non-DEAD GoodSide as still alive, which is wider than
vanilla (summons, `NO_CHECK_END_FIGHT`, parked-unless-`PREVENT_END_FIGHT`).

## Attempted fix (LokrLab 0.12.101 / 0.12.102) — confirmed in-game

Sandbox Load Encounter arms `EncounterSandbox` with a spawn fill and
the debug panel. Same runtime as Encounter Lab Sandbox: tiles, exploration,
camera bounds, enemy AI, `CheckFightEnd`. The exploration fence uses
vanilla's living-GoodSide filter so a dead hero ends the fight while
pockets are still parked. Character Sandbox's own 1v1 is unchanged.
0.12.102 removed the Encounter Play and Ability Stage tabs so this
path cannot drift again.

Resolved: 2026-08-17

Resolution: Confirmed in a running Sandbox — combat ends on hero death
(and the hole tears down) on the single `EncounterSandbox` arm.
