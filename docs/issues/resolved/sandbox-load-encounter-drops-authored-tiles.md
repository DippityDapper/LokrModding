# Sandbox Load Encounter keeps the host grass floor

Area: LokrLab Sandbox (`SandboxEncounterLoad`, `EncounterTileConstrainPatch`,
`EncounterHexaTileSpritePatch`)
Status: resolved

As of 2026-08-17: Character Lab Sandbox → Load Encounter places the open
character on the authored hero spawn point and spawns the other
combatants, props, and triggers, but the floor stays
`fighttesterempty`'s green grass. Encounter Setup for the same project
shows the painted water / island Tilemap.

Phase 12's tile patches only treated Setup and Play as "Encounter owns
the embed." A Sandbox-loaded Encounter is a third arm
(`SandboxEncounterLoad`). Vanilla `TileTestController.ConstrainMap`
then crops Lab-painted cells back to the template rect, and the
`GetTileData` hex-sprite fallback never runs.

## Attempted fix (LokrLab 0.12.99) — confirmed in-game

`EmbeddedFightHost.EncounterOwnsEmbed` is true for Setup, Play, or
Sandbox-loaded Encounter. Both tile patches use that gate.
`FinishFightControls` re-applies authored tiles and facing after
StartFight, matching Setup's second paint.

Resolved: 2026-08-17

Resolution: Confirmed in a running Sandbox load — the authored water /
island floor appears instead of `fighttesterempty` grass.
