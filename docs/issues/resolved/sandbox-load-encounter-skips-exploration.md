# Sandbox Load Encounter ignores exploration (pockets, triggers, aggro)

Area: LokrLab Sandbox (`EncounterPlay`, `EncounterRoster`,
`EncounterExploration`, `EncounterExplorationFightEndPatch`)
Status: resolved

As of 2026-08-17: Character Lab Sandbox → Load Encounter on a project
with `exploration: true` immediately puts every enemy into initiative.
Encounter Play for the same file parks BadSide pockets until aggro /
trigger, and the fight-end fence holds while a pocket is still parked.

`BeginExploration`, the per-frame `Tick`, and the `CheckFightEnd` prefix
were gated on `EncounterPlay.IsArmed` only. A third arm never parked
units, never scanned radius/trigger regions, and never fenced a false
instant win.

## Attempted fix (LokrLab 0.12.100) — confirmed in-game

`EncounterRoster.Spawn` parked pockets for Play or the Sandbox-load arm.
`EncounterExploration.Tick` runs whenever pockets are tracked.
The fight-end fence keys off `HasLivingParkedMembers`.

Resolved: 2026-08-17

Resolution: Confirmed in a running Sandbox load — enemies stay off
initiative until aggro / trigger. 0.12.101 then folded that arm into
`EncounterPlay` so this path cannot drift again.
