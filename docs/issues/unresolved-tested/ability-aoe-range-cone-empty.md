# Ability AOE RANGE_CONE never fills hexes

Area: vanilla combat targeting (`MoveAndSkillController.CalculateAOE`) plus
model twins (`ActOnHexasAction.Execute`, `UnitFilter.PassesFilter`).
Hurts Ability Lab (`AbilityEnvelopeOptions.AOEKinds` includes `RANGE_CONE`).
Status: unresolved-tested

As of 2026-08-15: `AOEKind.RANGE_CONE` (2) is a declared KV / Lab value, but
the three runtime consumers leave it empty or match nobody:

- `MoveAndSkillController.CalculateAOE` case `2U` is an empty branch
  (`aoeAffectedHexs` is never assigned)
- `ActOnHexasAction.Execute` has no cone arm (batch A2)
- `UnitFilter.PassesFilter` returns false for cone (batch A5)

Ability Lab offers `RANGE_CONE` in the AOE-kind dropdown. A Lab or mod cone
ability previews and hits nobody. Overlay classes (`ConeRangeIndicator`)
can still draw a cone the combat filter will not honor.

Suggested fix: implement cone hex enumeration (Width / MinT / MaxT already
exist on `UnitFilter` and are unread) in `CalculateAOE`, `ActOnHexas`, and
`PassesFilter` together. Until then, hide or warn on `RANGE_CONE` in Ability
Lab.

## Proposed solution (Pass 1, 2026-08-15)

**Home:** LokrLab (do-not-patch combat)
**Approach:** Do not Harmony-patch `CalculateAOE`, `ActOnHexasAction.Execute`, or `UnitFilter.PassesFilter`. Ability Lab hides `RANGE_CONE` from new AOE-kind picks and warns if a loaded file already has it.
**Exact change:** Decompile check: `MoveAndSkillController.CalculateAOE` case `2U` is an empty `break`; `ActOnHexasAction.Execute` has no cone arm; `UnitFilter.PassesFilter` case `2U` returns `false`. `UnitFilter.Width` is assigned from `AbilityAOEWidth` / `UnitTargetHelper` but never read in those three methods. `MinT`/`MaxT` are written by `Projectile.UpdateTransformData` for travel, not cone fill — `PassesFilter` never reads them. `ConeRangeIndicator` only draws a mesh. There is no unused cone helper to share. In LokrLab: keep `RANGE_CONE` out of the Envelope dropdown options used for *new* selection (`AbilityEnvelopeOptions.AOEKinds` filtered, or a `SelectableAOEKinds` without cone). If `current.AOEKind == "RANGE_CONE"`, still bind that value so save does not silently rewrite to `RANGE_CIRCLE`. `AbilityValidation.CollectWarnings`: if `AOEKind` is `RANGE_CONE`, note that combat never fills cone hexes.
**Do not:** Invent a cone hex algorithm in Pass 2. Do not patch the three combat sites. Do not strip `RANGE_CONE` from files on save. Do not treat `Width`/`MinT`/`MaxT` as a ready-made fill.
**In-game verify:** 1. Build, launch via Steam/Proton, Ability Lab Envelope with AOE on. 2. AOE kind dropdown does not offer RANGE_CONE for a new/circle ability. 3. Hand-set or load a file with `AbilityAOEKind RANGE_CONE` — editor keeps the value, status warns, sandbox preview/hits stay empty (vanilla). 4. RANGE_CIRCLE / RANGE_TUNNEL still fill hexes.
**Risk:** None to vanilla combat (no patch). Lab authors can still paste RANGE_CONE in Advanced KV; they get a warning instead of a fake cone.

## Unit tests (unresolved-tested)

Moved: 2026-08-15
Tests:
- LokrModding.Tests.Lab.LabCatalogRulesTests.RangeCone_IsNotSelectable_AndWarns
Still needs in-game confirm. A passing unit test is not a running-game
confirm.
