# Several AbilityEvents / ModifierEvents names never fire

Area: LokrLab (Ability Lab event lists) — vanilla whitelist, no C# fire site
Status: unresolved-tested

`AbilityParser.ParseEvents` accepts every name on `AbilityEvents` / `ModifierEvents`. These string constants have no `OnEvent` / `BroadcastEvent` / `SendEvent` call site in `ih-original`:

Ability: `OnAttackStart`, `OnAttackAction`, `OnAttacked`.

Modifier: those three plus `OnAbilityEnd`, `OnAttackEnd`, `OnPreAttack`, `OnPostAttack`, `OnAttack`, `OnUnitMoved`, `OnHitPreResultGlobal`.

A Lab or mod ability that implements them parses and then never runs. Combat uses `OnAbilityStart` / `OnAbilityAction` / `OnAbilityCustomEvent` (AbilityMeleeActivity), projectile `OnProjectile*`, hit-pipeline `OnPreHit` / `OnHit*` / `OnPostHit`, and the turn / spawn / fight globals instead.

Suggested fix: Ability Lab should warn (or hide) names with no known fire site. Do not add Harmony dispatch unless a vanilla animation or stripped assembly is later shown to raise them. Pass C should re-grep before treating this as a patch.

## Proposed solution (Pass 1, 2026-08-15)

**Home:** LokrLab (do-not-patch)
**Approach:** No Harmony dispatch. Ability Lab hides names with no fire site from the Add-event menus and warns if a loaded file already has them. Local ih-original grep (2026-08-15): `"OnAttackStart"`, `"OnAttackAction"`, `"OnAttacked"`, `"OnAbilityEnd"`, `"OnAttackEnd"`, `"OnPreAttack"`, `"OnPostAttack"`, `"OnAttack"`, `"OnUnitMoved"`, `"OnHitPreResultGlobal"` appear only as consts on `AbilityEvents` / `ModifierEvents`. Combat fires `OnAbilityStart` / `OnAbilityAction` / `OnAbilityCustomEvent` (`AbilityMeleeActivity`), projectile `OnProjectile*`, hit-pipeline `OnPreHit` / `OnHit*` / `OnPostHit` (including `OnHitPreResult` but not `OnHitPreResultGlobal`), and turn / spawn / fight globals (`Stage`, `LevelManager`, `Unit`). `OnUnitLeavingNode` / `OnUnitEnteredNode` fire; `OnUnitMoved` does not.
**Exact change:** In `AbilityEventNames`, add `FiredAbilityEvents` / `FiredModifierEvents` (All* minus the dead names above). `AbilityEditorCards` "Add event hat" / "Add modifier event" use Remaining(Fired*, present) so dead names are not offered. Keep All* for `TryFindIllegalEvent` — those names are parse-legal and must not become save errors. `AbilityValidation.CollectWarnings`: if an ability or modifier hat is in the dead set, note that the engine never dispatches it. Existing hats still render so authors can delete them.
**Do not:** Add Harmony `OnEvent` / `BroadcastEvent` / `SendEvent` for these names. Do not remove them from `AllAbilityEvents` / `AllModifierEvents` (parser would then treat them as illegal On*). Do not hide hats already on the file.
**In-game verify:** 1. Build, launch via Steam/Proton, Ability Lab. 2. Add event hat — OnAttackStart / OnAttackAction / OnAttacked are absent. 3. Add modifier event — the longer dead list is absent. 4. Paste KV with `OnAttackStart` — file loads, hat shows, status warns, sandbox combat never runs that hat. 5. OnAbilityAction still fires on a melee sandbox skill.
**Risk:** None to vanilla. Mods that already authored dead hats keep parsing; they only get a Lab warning. No save data.

## Unit tests (unresolved-tested)

Moved: 2026-08-15
Tests:
- LokrModding.Tests.Lab.LabCatalogRulesTests.DeadAbilityEvents_AreParseLegalButUnfired
Still needs in-game confirm. A passing unit test is not a running-game
confirm.
