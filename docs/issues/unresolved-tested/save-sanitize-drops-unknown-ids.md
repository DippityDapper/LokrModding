# SaveGameMetadata.Sanitize discards the run on unknown hero, item, or quest ids

Area: docs/api/base-game (Metagame save load)
Status: unresolved-tested

As of 2026-08-15: `SaveGameMetadata.Sanitize` in
`ih-original/Ironhide.Legends/Ironhide/Legends/Model/Metagame/SaveGameMetadata.cs`
calls `DiscardRun()` (replaces `run` with `SaveGameRun.CreateEmpty()`) and
still returns true if any of these fail:

- `run.gameData.currentAdventure` is non-empty and missing from
  `AdventureManager.AdventuresConfig.adventuresById`
- any `run.heroesData.heroes[].archetype` missing from
  `UnityDefinitionsParser.Definitions`
- any `run.heroesData.startingHeroes` uniqueId missing from
  `DefinitionsByUnique`
- any quest id in `questsInAdventure` / `questsInGame` /
  `ephemeralQuests` / `questStatuses.questName` missing from
  `MapManager.GetMapQuestDefinition`
- any `run.inventoryData.inventory[].itemArchetype` missing from
  `InventoryManager.GetAllItemDefinitions()`

The slot stays `VALID` with an empty run. Uninstalling a hero, item, or
quest mod — or loading a save before those definitions are registered —
wipes the in-progress adventure. Character Lab live-reload and
community-pack uninstalls hit this.

Suggested fix: Harmony-prefix `Sanitize` to keep or stow unknown ids
(roster XP already uses `HeroRosterManager` stowaways) instead of
`DiscardRun`. Do not start that patch from the HTML-docs track.

See
[`SaveGameMetadata.html`](../../api/base-game/Ironhide/Legends/Model/Metagame/SaveGameMetadata.html).

## Proposed solution (Pass 1, 2026-08-15)

**Home:** LokrPatch
**Approach:** Harmony prefix that replaces `SaveGameMetadata.Sanitize` so unknown adventure / hero / starting-hero / quest / item ids are logged and left in the parsed save (or stowed on the matching manager Save/Load, same pattern as `HeroRosterManager.stowawayItems` and `AdventureManager.stowawayAdventures`). Never call `DiscardRun`. Party-list mutation is owned by the party-reset patch — this prefix must not assign `SaveGame.CreateStartingPartyIds()`. Companion prefixes on `ExtractInfo` plus `HeroManager` / `InventoryManager` / `MapManager` Load/Save keep vanilla Load from throwing once Sanitize no longer empties the run.
**Exact change:** `SaveGameSanitizePatches` in LokrPatch. `[HarmonyPatch(typeof(SaveGameMetadata), nameof(SaveGameMetadata.Sanitize))]` prefix: copy the null guards and the achievement-state merge; on each failing All/ContainsKey check log the missing ids via `LokrPatchPlugin.Log.LogWarning` and continue; return true (skip vanilla). Do not write `CreateStartingPartyIds` into `heroRosterData.party`. `[HarmonyPatch(typeof(SaveGameMetadata), nameof(SaveGameMetadata.ExtractInfo))]` prefix: same as vanilla except `heroParty` is built only from archetypes present in `UnityDefinitionsParser.Definitions` (do not call `GetDefinition`, whose `MissingUnitDefinition` fallback is null and would throw, marking the slot `INVALID`). Prefix `HeroManager.Load(HeroesSave)` / postfix `Save()`: construct only heroes whose `archetype` is in `Definitions`; stow the rest (`HeroDefinition` clones, including `oldHeroes`) and append them on Save so the blob is unchanged. Copy `startingHeroes` verbatim (unknown uniqueIds stay on the field; map-start skip-and-log spawns only known ones). Prefix `InventoryManager.Load(InventorySave)` / postfix `Save()`: skip `GetItemDefinition` for unknown `itemArchetype` (vanilla throws); stow `InventoryItemSave` rows and append on Save. Prefix `MapManager.Load(MapSave)` / postfix `Save()`: keep the quest id string lists as stored; live `questStatuses` only for ids where `GetMapQuestDefinition` is non-null; stow the other `MapQuestStatus` clones and append on Save. Clear all stow bags at the start of each Load. Unknown `currentAdventure` stays on `GameManager` as a string (reinstalling the pack restores the run); do not substitute another adventure.
**Do not:** Call `DiscardRun` or `SaveGameRun.CreateEmpty`. Silently drop unknown ids on the next `SaveGameManager.Save`. Replace the party with Gerald/Ranger/ArcaneMage (that fights [`save-party-reset-to-vanilla-trio.md`](save-party-reset-to-vanilla-trio.md)). Patch `UIHeroRoom` / 3-slot hero-room UI (that follow-up is [`../resolved/party-stow-shifts-remaining-into-wrong-slots.md`](../resolved/party-stow-shifts-remaining-into-wrong-slots.md)). Reimplement `SaveGameManager.Load` / `LoadRun`. Invent a new-adventure fallback when `LoadedAdventureConfig` is null.
**In-game verify:** 1. Start an adventure with a custom hero, a custom item in inventory, and (if available) a custom quest still on the map; save and quit to desktop. 2. Disable or uninstall that content pack (or Lab-reload so the ids are unregistered), launch, load the slot: the run must still be in progress (not an empty `CreateEmpty` run), BepInEx log lists the missing ids, slot stays `VALID`. 3. Re-enable the pack, load again: custom hero, item, and quest ids are still in the save. 4. Repeat with only a custom `currentAdventure` missing: run is not discarded. 5. Vanilla-only slot (Gerald/Ranger/ArcaneMage, stock items): load and save with no extra warnings and no party/inventory change.
**Risk:** High for save data — a buggy stow merge could duplicate or drop rows; keep stow append-only and never rewrite ids in place. Unknown adventure/quest ids can still NRE later on the map HUD until the pack is restored; that is preferred to wiping the run. Combat balance unchanged. Vanilla saves must be bit-identical aside from normal session fields (`revGuid` / timestamp).

## Unit tests (unresolved-tested)

Moved: 2026-08-15
Tests:
- LokrModding.Tests.Patch.PatchRulesTests.Sanitize_DoesNotDiscardRun
Still needs in-game confirm. A passing unit test is not a running-game
confirm.
