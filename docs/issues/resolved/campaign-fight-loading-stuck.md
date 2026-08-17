# Campaign fight node leaves an infinite loading screen

Area: vanilla campaign map → fight transition (`krlegendsmap_roguelike`
→ `krlegendsfightgameplay02`); session had Lab open earlier the same
process. Hurts adventure fight nodes, not Lab Sandbox / Stage.
Status: resolved

As of 2026-08-15 (in-game Pass 2 confirm): Lab Sandbox and Ability Lab
Stage fights start with no FightStarted NRE. The same session then
Continue → start adventure → fight node: combat scene loads (`mode=Single`),
quest `OrcAmbush` / `combat_orcAmbush_4` units add, a voiceline plays,
`UIFightNavController` goes `NONE` → `DEFAULT` — and the loading overlay
never dismisses.

This is not
[`fight-started-empty-initiative-nre.md`](fight-started-empty-initiative-nre.md)
(empty `ActiveUnit` on `fighttesterempty`). Campaign fight is a Single
load, not Lab additive embed. Do not fold the two.

Likely `FadeScreen` / `transitionscene` never dismissed after the map
→ fight handoff. Same class of overlay as the resolved Lab reopen stuck
load ([lab-reopen-loading-screen-stuck.md](lab-reopen-loading-screen-stuck.md)),
but that fix is Lab Close → title. Confirm whether Lab left FadeScreen
state, or vanilla map→fight FadeScreen fails on Proton after Lab.

Related same session: progression-help Next threw before the map
([`progression-help-popup-index-oor.md`](progression-help-popup-index-oor.md);
later confirmed clamped).
Cloud `KREQ_ERR_CLOUD_REMOTE_CHANGED` is Steam/Proton, not this overlay.

Suggested fix: after a cold start with no Lab, enter a fight node. If
the overlay still sticks, it is vanilla/Proton FadeScreen. If it only
sticks after Lab was opened in the process, Lab Close must also reset
FadeScreen / `wasTransitioning` the way reopen-loading did. Do not
disable campaign Single loads.

## Proposed solution (Pass 1, 2026-08-15)

**Home:** unknown until the no-Lab control run
**Approach:** Reproduce with two process starts: (A) title → Continue → adventure → fight node, never open Lab; (B) Lab sandbox once, Close Lab, then the same fight node. Compare `LogOutput.log` for `FadeScreen`, `transitionscene`, `wasTransitioning`, and `UIFightNavController`.
**Exact change:** None until A vs B. If only B sticks, extend Lab Close FadeScreen reset. If A also sticks, LokrPatch / FadeScreen dismiss after `krlegendsfightgameplay02` Single load when nav is DEFAULT and units exist.
**Do not:** Treat Lab sandbox success as campaign confirm. Do not patch `FightStartedHandler` again for this overlay. Do not blame `cant find UIManager` on the fight scene (that miss is normal for some fight canvases).
**In-game verify:** 1. Cold start, no Lab, Forest fight node — does the overlay clear? 2. Repeat after Lab Close in the same process. 3. Confirm combat is playable (skills, end turn) once the overlay is gone. 4. Confirm Lab sandbox still starts after this fix.
**Risk:** Dismissing FadeScreen too early can flash the fight before units exist. Gate on nav DEFAULT + at least one unit, or on the same vanilla signal map fights already use.

Resolved: 2026-08-15

Resolution: Confirmed in-game: the loading overlay over a campaign fight
node dismisses and combat is reachable. No extra FadeScreen patch this
confirm; Lab Close already resets transition state (same family as
[lab-reopen-loading-screen-stuck.md](lab-reopen-loading-screen-stuck.md)).
Progression-help Next is confirmed clamped —
[`progression-help-popup-index-oor.md`](progression-help-popup-index-oor.md).
