# Save load resets a party that is not exactly 3 known uniqueIds to Gerald/Ranger/ArcaneMage

Area: docs/api/base-game (Metagame save load)
Status: unresolved-tested

As of 2026-08-15, two stacked checks wipe a custom party:

1. `SaveGameMetadata.Sanitize`: if `heroRosterData.party` contains any id
   not in `UnityDefinitionsParser.DefinitionsByUnique`, the entire party
   is replaced with `SaveGame.CreateStartingPartyIds()` (`Gerald`,
   `Ranger`, `ArcaneMage`). It does not drop just the unknown id, and it
   does not check `Count == 3`.
2. `HeroRosterManager.Load`: keeps only ids that exist in
   `DefinitionsByUnique`, then if `party.Count != 3` replaces the party
   with the same trio.

A 4-hero party of registered custom heroes survives Sanitize and is
reset on Load. A 3-hero party with one missing definition is fully
replaced on Sanitize. Character Lab / roster mods that add a fourth
member, or a custom-only trio after a definition fails to register, lose
the party. Vanilla map UI also assumes 3 slots (batch V5).

Hero-room / arena UI (batch V5) hardcodes the same 3:

- Party portraits: `UIHeroRoom` / `UIHeroRoomCurrentAdventurers`
  always touch indices 0, 1, 2. Extra roster heroes appear on the
  bar; they cannot occupy a fourth slot.
- Skill hexes: `UIHeroRoomHeroData` / `UIMapHeroRoomHeroData` /
  `UIArenaHeroItem` index `skillProgression[1]`, `[2]`, `[3]`.
  Missing keys KeyNotFound when viewing a custom hero.

Suggested fix: patch `HeroRosterManager.Load` (and optionally Sanitize's
party branch) to keep known ids and not force count == 3. 3-slot UI
compact/ghost portraits:
[`../resolved/party-stow-shifts-remaining-into-wrong-slots.md`](../resolved/party-stow-shifts-remaining-into-wrong-slots.md).

See
[`HeroRosterManager.html`](../../api/base-game/Ironhide/Legends/Model/Metagame/HeroRosterManager.html),
[`SaveGameMetadata.html`](../../api/base-game/Ironhide/Legends/Model/Metagame/SaveGameMetadata.html),
[`UIHeroRoom.html`](../../api/base-game/Ironhide/Legends/View/Metagame/Screens/HeroRoom/UIHeroRoom.html),
and
[`UIHeroRoomHeroData.html`](../../api/base-game/Ironhide/Legends/View/Metagame/Screens/HeroRoom/UIHeroRoomHeroData.html).

## Proposed solution (Pass 1, 2026-08-15)

**Home:** LokrPatch
**Approach:** Harmony prefix replacing `HeroRosterManager.Load` so the live party is the known uniqueIds in save order, with unknown ids stowed and written back on `Save` (mirror `stowawayItems`). Delete the `party.Count != 3` → `CreateStartingPartyIds()` branch. Sanitize must ship in the same build and must not assign the vanilla trio either — otherwise one patch undoes the other. Vanilla 3-slot hero-room / arena UI is [`../resolved/party-stow-shifts-remaining-into-wrong-slots.md`](../resolved/party-stow-shifts-remaining-into-wrong-slots.md); a 4-hero party is allowed to exist in roster state even if the room still paints indices 0–2.
**Exact change:** `HeroRosterPartyLoadPatches` in LokrPatch. `[HarmonyPatch(typeof(HeroRosterManager), nameof(HeroRosterManager.Load))]` prefix (`void Load(HeroRosterSave save)`): keep vanilla `stowawayItems` / `heroRosterState` behavior; build `party` from `save.party` ids that exist in `DefinitionsByUnique`; stow the rest in a new list (Traverse-backed or patch-static, cleared on Load). Do not run `if (this.party.Count != 3) this.party = SaveGame.CreateStartingPartyIds()`. Return false. `[HarmonyPatch(typeof(HeroRosterManager), nameof(HeroRosterManager.Save))]` postfix: append stowed party ids onto `HeroRosterSave.party` so unknown members round-trip. Coordinate with `SaveGameSanitizePatches`: that prefix leaves `heroRosterData.party` untouched (no trio write). Empty known-party (every id unknown) stays empty this session; do not fill Gerald/Ranger/ArcaneMage.
**Do not:** Patch `UIHeroRoom`, `UIHeroRoomCurrentAdventurers`, `UIHeroRoomHeroData`, `UIMapHeroRoomHeroData`, or `UIArenaHeroItem` in *this* issue (3-slot portraits, compact-into-legend-slot, and `skillProgression[1,2,3]` KeyNotFound are [`../resolved/party-stow-shifts-remaining-into-wrong-slots.md`](../resolved/party-stow-shifts-remaining-into-wrong-slots.md)). Force party count to 3 or 4. Drop unknown party ids on Save. Re-introduce `CreateStartingPartyIds` in Sanitize “just in case.”
**In-game verify:** 1. Four registered custom heroes in the party, save, quit, load: party still has those four uniqueIds (log has no trio reset). 2. Three custom heroes, disable one definition, load: the two known ids remain; the missing id is absent from the live party but returns after the pack is re-enabled and the slot is loaded again. 3. Vanilla Gerald/Ranger/ArcaneMage slot: still exactly that trio after load/save. 4. Confirm Sanitize-without-this-patch still resets a 4-hero party on Load, and this-patch-without-Sanitize still loses a 3-hero party that contains one unknown id — both must be in the same shipped build.
**Risk:** Save data: stow merge must not duplicate ids already in `party`. Hero-room UI still shows three portrait slots, so a fourth member is on the roster bar only until that follow-up. Combat balance unchanged. Vanilla 3-hero parties must not change.

## Unit tests (unresolved-tested)

Moved: 2026-08-15
Tests:
- LokrModding.Tests.Patch.PatchRulesTests.Party_KeepsKnownIds_NoVanillaTrioReset
Still needs in-game confirm. A passing unit test is not a running-game
confirm.
