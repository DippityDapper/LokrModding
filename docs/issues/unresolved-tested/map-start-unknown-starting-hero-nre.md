# Map Start NREs on an unknown StartingHeroes uniqueId

Area: docs/api/base-game (map scene start) and custom adventures
Status: unresolved-tested

As of 2026-08-15, `NewMapManagerComponent.Start` (empty-party fallback)
does `DefinitionsByUnique.GetValueOrDefault(heroe, null).id` for each
`HeroManager.StartingHeroes` uniqueId with no null check. An unknown id
null-derefs and aborts the map scene. This is the new-run path, not
save load — do not fold into
[`save-party-reset-to-vanilla-trio.md`](save-party-reset-to-vanilla-trio.md)
(`Sanitize` / `HeroRosterManager.Load` replace a bad party; this runs
only when `GetAllHeroes()` is already empty).

A custom adventure whose `StartingHeroes` lists a uniqueId that failed
to register (or a typo) never reaches the map. Character Lab / roster
mods that only add heroes to a non-empty party do not hit this.

Suggested fix: skip (and log) missing uniqueIds, or resolve through
`CharacterAPI` / `DefinitionsByUnique` and refuse to start the run
without a valid hero rather than NRE.

See
[`NewMapManagerComponent.html`](../../api/base-game/Ironhide/Legends/View/Map/NewMapManagerComponent.html).

## Proposed solution (Pass 1, 2026-08-15)

**Home:** LokrPatch
**Approach:** Harmony transpiler on `NewMapManagerComponent.Start` so the empty-party `StartingHeroes` ForEach null-checks `DefinitionsByUnique.GetValueOrDefault(heroe, null)` before `.id` / `AddHero`. Skip and log unknown uniqueIds. Do not mutate `HeroManager.startingHeroes` and do not fold this into Sanitize / party-reset (those run on load; this runs only when `GetAllHeroes()` is already empty).
**Exact change:** `MapStartStartingHeroPatch` in LokrPatch. `[HarmonyPatch(typeof(NewMapManagerComponent), "Start")]` transpiler on `private void Start()`. In the `HeroManager.StartingHeroes.ForEach` lambda, after `GetValueOrDefault(heroe, null)`, branch: if the `UnitDefinition` is null, `LogWarning` the uniqueId and skip `AddHero`; else `AddHero(def.id)` as vanilla. Leave the surrounding `if (GetAllHeroes().IsEmpty()) { ... Save(); }` intact so a successful spawn still saves, and so `startingHeroes` on disk is still the original list (unknown ids remain for when the pack is reinstalled). If every starting uniqueId is unknown, ForEach adds nobody and Save persists an empty live party with the original `startingHeroes` field — log that nothing spawnable was found; do not inject Gerald.
**Do not:** Prefix-replace all of `Start` (map prefab, quests, HUD, cinematics). Assign `CreateStartingPartyIds` or rewrite `startingHeroes`. Temporarily filter the property around `Save()` (that would drop unknown ids from the blob). Merge with [`save-party-reset-to-vanilla-trio.md`](save-party-reset-to-vanilla-trio.md).
**In-game verify:** 1. New custom adventure whose `StartingHeroes` are all registered: map starts with those heroes, same as today. 2. Same adventure with one uniqueId typo / unregistered: map still starts; known heroes are added; log names the missing uniqueId; save still lists the unknown id under `startingHeroes`. 3. All starting uniqueIds unknown: no NRE aborting the scene; log says none spawnable; do not silently become Gerald/Ranger/ArcaneMage. 4. Vanilla campaign new run: unchanged party spawn.
**Risk:** Empty live party if every starting id is unknown — later HUD may be barren, but the save is not rewritten to the vanilla trio. Combat balance unchanged. Transpiler must not skip the `Save()` when at least one hero was added.

## Unit tests (unresolved-tested)

Moved: 2026-08-15
Tests:
- LokrModding.Tests.Patch.PatchRulesTests.UnknownStartingHero_IsSkipped
Still needs in-game confirm. A passing unit test is not a running-game
confirm.
