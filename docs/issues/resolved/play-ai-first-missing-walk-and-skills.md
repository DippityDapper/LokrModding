# Play: no walk hexes or skills bar after an AI-first turn

Area: LokrLab Encounter Play / embedded fight (`SandboxFightControls`, `EmbeddedFightStagePatch`)
Status: resolved

As of 2026-08-16: in Encounter Play, when the enemy wins initiative and
takes a turn, the following player turn sometimes has no movement-range
hexes and no skills / End Turn chrome. Portraits and the turn banner
still show. Intermittent; not every AI-first fight.

Vanilla starts player interaction from `UnitController.LateUpdate` →
`CheckUserInteraction` → `StartUserInteraction` (`Calculate` +
`SetSelectedSkill` + `ShowPowerBarButtons`). `FightStartTurn` fires at
the *end* of the current unit's `StartUserInteraction`, so after an AI
turn Lab's `OnFightStartTurn` / `EnsureFightInput` sees an AI unit and
must not start interaction (that re-opens the `EndTurn` petition loop).
The player handoff then depends entirely on `CheckUserInteraction`,
which no-ops while `InCinematicMode`, while `processingLogic` is stuck,
or after it consumes `CanHandleInputOrAI` on a frame
`TurnActionFinished` is still false.

## Attempted fix (LokrLab 0.12.52)

`Stage.TurnStarted` postfix calls `BeginPlayerTurnHandoff` for a living
player unit. That coroutine waits until end-turn / hide-tween /
`processingLogic` are clear, then `RecoverTurnChrome` + `EnsureFightHud`
+ `EnsureFightInput`. Last-chance after 45 frames clears a stuck
`hidingHUD` / `barTweening` and a stuck `processingLogic` only when the
player still has no activity and `CanAcceptInput` is false. Still does
not start `StartUserInteraction` for AI.

Resolved: 2026-08-16

Resolution: Confirmed in-game. After an AI-first turn the hero turn
shows walk hexes and the skills bar (LokrLab 0.12.52).
