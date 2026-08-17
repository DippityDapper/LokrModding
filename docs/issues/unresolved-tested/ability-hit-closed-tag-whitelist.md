# HitAction.ValidateTags rejects Lab ranged template and picker tags

Area: LokrLab (Ability Lab New ranged template, Hit Tags picker) and vanilla
`HitAction.ValidateTags`
Status: unresolved-tested

As of 2026-08-15: `HitAction.ValidateTags` (parse-time) throws
`HitAction is using invalid Tags: ...` unless every tag is in a closed
whitelist: `MELEE`, `TARGETED`, `PROJECTILE`, `MAGICAL`, `AOE`,
`MODIFIER`, `ENVIRONMENTAL`, `RAY`, `REFLECTED`, `INTERNAL`,
`FREEZEDAMAGE`, `BURNDAMAGE`, `CANTBEBLOCKED`, `CANTBEDODGED`,
`CANTBESHIELDED`, `GLARE_SUPER_RAY`, `GLARE_TOWER_OR_CRYSTAL`,
`HEX_BLAST_FIRST_TIME`. `BACKSTAB` is injected only at Execute and is
not a legal KV tag.

Ability Lab's ranged template writes `stringList(#RANGED, #TARGETED)`.
`#RANGED` is not on that list, so a New ranged ability fails to parse
on reload. The Hit Tags picker also offers `#SKULL`, `#NON_TARGETABLE`,
and `#TowerCultist1`–`4`, which `ValidateTags` likewise rejects.

Suggested fix: change the ranged template to `#PROJECTILE` (vanilla
ranged hits) and filter the picker to the whitelist; or Harmony-prefix
`ValidateTags` to allow extra tags. Confirm a New ranged Lab ability
loads and the Hit card still classifies as a projectile hit where
combat cares.

## Proposed solution (Pass 1, 2026-08-15)

**Home:** LokrLab
**Approach:** Do not open the vanilla `HitAction.ValidateTags` whitelist. Change the New ranged template to `stringList(#PROJECTILE, #TARGETED)` (what vanilla ranged hits and `hasTags(hitTags(%HIT), #PROJECTILE, #TARGETED)` actually test). Drive the Hit Tags picker from that whitelist, not from dump-wide `stringList` hashes (`#RANGED`, `#SKULL`, `#NON_TARGETABLE`, `#TowerCultist*`). Block save when a Hit `Tags` token is outside the list. `#RANGED` would not classify as a projectile hit even if ValidateTags allowed it.
**Exact change:** `AbilityTemplates.RangedModel`: `Tags` = `stringList(#PROJECTILE, #TARGETED)`. `AbilityExpressionField.OptionsFor(ArgKind.Tag)`: return a curated list matching ValidateTags (`MELEE`, `TARGETED`, `PROJECTILE`, `MAGICAL`, `AOE`, `MODIFIER`, `ENVIRONMENTAL`, `RAY`, `REFLECTED`, `INTERNAL`, `FREEZEDAMAGE`, `BURNDAMAGE`, `CANTBEBLOCKED`, `CANTBEDODGED`, `CANTBESHIELDED`, `GLARE_SUPER_RAY`, `GLARE_TOWER_OR_CRYSTAL`, `HEX_BLAST_FIRST_TIME`) with a leading `#` for display — including `HEX_BLAST_FIRST_TIME`, which the dump-based `HitTags` array currently omits. `AbilityValidation.TryValidate`: for each Hit card `Tags` value, strip `#` and reject any token not on that list (blocking error). In `generate_ability_picker_catalog.py` `FUNCTION_TEMPLATES`, replace `stringList(#RANGED, #TARGETED)` with the PROJECTILE form (regenerate or patch that one snippet). Optional load rewrite: if a Hit `Tags` field contains `#RANGED`, replace with `#PROJECTILE` so already-created New ranged files parse without opening the whitelist.
**Do not:** Harmony-prefix `ValidateTags` to allow `#RANGED` / unit tags / `#BACKSTAB` (BACKSTAB is Execute-only). Do not use dump `HitTags` as the picker source. Do not change melee template `#MELEE, #TARGETED`. Do not treat `#skull` / `#TowerCultist*` as hit tags.
**In-game verify:** 1. Build LokrLab. 2. Launch through Steam / Proton, open Ability Lab. 3. File → New Ability → Ranged projectile, save, sandbox-reload. 4. Confirm LogOutput has no `HitAction is using invalid Tags` and the ability registers. 5. Confirm the Hit Tags picker lists `#PROJECTILE` / `#TARGETED` / `#MELEE` / … and does not list `#RANGED`, `#SKULL`, `#NON_TARGETABLE`, or `#TowerCultist1`. 6. Type `#RANGED` into Tags and confirm Save is blocked. 7. In sandbox, fire the new ranged skill and confirm a `hasTags(..., #PROJECTILE, #TARGETED)` listener (or vanilla projectile-hit VFX / block rules) still treats it as a projectile hit.
**Risk:** Existing Lab ranged files that already saved `#RANGED` stay unloadable until the optional load rewrite or a manual edit; new files are fine. Vanilla hits unchanged. Combat classification matches vanilla ranged (`PROJECTILE`), which is the point. No save-schema change beyond the Tags string.

## Unit tests (unresolved-tested)

Moved: 2026-08-15
Tests:
- LokrModding.Tests.Lab.LabCatalogRulesTests.ProjectileIsLegal_RangedIsNot
Still needs in-game confirm. A passing unit test is not a running-game
confirm.
