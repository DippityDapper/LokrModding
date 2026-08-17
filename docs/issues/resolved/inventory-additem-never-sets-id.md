# InventoryManager.AddItem never assigns ItemInstance.id

Area: docs/api/base-game (Metagame inventory / custom items)
Status: resolved

As of 2026-08-15: `InventoryManager.AddItem` in
`ih-original/Ironhide.Legends/Ironhide/Legends/Model/Metagame/Inventory/InventoryManager.cs`
builds a new `ItemInstance` with only `itemDefinition` and `quantity = 1`.
Nothing in the Inventory folder assigns `ItemInstance.id` except `Load`
copying `itemSave.id`. Mid-run grants (loot, shop, Lua `item` results,
custom items) therefore `Save` with a null instance id. Runtime lookups
use `itemDefinition.id`, so stacking/consume still work; the save row is
what is wrong. `SaveGameMetadata.Sanitize` keys inventory by
`itemArchetype`, so a null instance id does not by itself `DiscardRun`
(that path is
[`save-sanitize-drops-unknown-ids.md`](../unresolved-tested/save-sanitize-drops-unknown-ids.md)).

Suggested fix: Harmony-postfix `AddItem` (or the new-instance branch) to
set `id = Guid.NewGuid().ToString()` when it is null. Do not start that
patch from the HTML-docs track.

See
[`InventoryManager.html`](../../api/base-game/Ironhide/Legends/Model/Metagame/Inventory/InventoryManager.html).

## Proposed solution (Pass 1, 2026-08-15)

**Home:** LokrPatch
**Approach:** Harmony postfix on `InventoryManager.AddItem(string)` that assigns `ItemInstance.id` when it is null or empty after vanilla runs. Mid-run grants (loot, shop, Lua `item`, custom items) then serialize a real instance id. Do not walk the inventory on load or rewrite existing save rows in bulk.
**Exact change:** `InventoryAddItemIdPatch` in LokrPatch. `[HarmonyPatch(typeof(InventoryManager), nameof(InventoryManager.AddItem))]` postfix (`void AddItem(string itemDefinitionId)`). After the original method: `GetItem(itemDefinitionId)`; if the instance is non-null and `string.IsNullOrEmpty(instance.id)`, set `id = Guid.NewGuid().ToString()`. Covers both the new-instance branch and a stacked row that was loaded with a null id (repaired on the next grant of that archetype). Auto-consume items never enter `inventory`; `GetItem` stays null and the postfix no-ops. Leave `Load` copying `itemSave.id` as-is.
**Do not:** Prefix-replace `AddItem`. Backfill every null id during `InventoryManager.Load` (that is a silent save rewrite). Key Sanitize off instance id (it already keys `itemArchetype`; see [`save-sanitize-drops-unknown-ids.md`](../unresolved-tested/save-sanitize-drops-unknown-ids.md)). Change stacking / `GetItem` to look up by instance id.
**In-game verify:** 1. New run, buy or loot an item that was not in the starting inventory; save; inspect the slot JSON (or a subsequent load) so that row’s `id` is a non-empty GUID. 2. Grant a second copy of the same archetype: quantity increments and the existing id is kept. 3. Auto-consume item: no inventory row, no error. 4. Vanilla starting-inventory load: ids from the save file unchanged.
**Risk:** Low. New GUIDs only when vanilla left `id` null; does not remap loaded ids. Combat/inventory balance unchanged. Worst case a stacked dirty row gets an id on the next grant, which is a repair not a wipe.

## Unit tests (unresolved-tested)

Moved: 2026-08-15
Tests:
- LokrModding.Tests.Patch.PatchRulesTests.EmptyItemId_NeedsGuid
Still needs in-game confirm. A passing unit test is not a running-game
confirm.

Resolved: 2026-08-15

Resolution: Confirmed in-game (LokrPatch 1.0.10). Mystery elixir (new
archetype, not in the starting kit) saved with a non-empty GUID. Tent
stack 2→3 kept the existing instance id and did not mint a new one.
Starting-kit ids were unchanged on that stack grant.
