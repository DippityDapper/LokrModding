# Party stow compacts remaining heroes into the wrong slots

Area: LokrPatch (`HeroRosterLoadPartyPatch`) + hero-room / adventure-brief party UI
Status: resolved

As of 2026-08-15, in-game confirm of
[`save-sanitize-drops-unknown-ids.md`](../unresolved-tested/save-sanitize-drops-unknown-ids.md)
and
[`save-party-reset-to-vanilla-trio.md`](../unresolved-tested/save-party-reset-to-vanilla-trio.md)
kept the Krum'thak run and restored Onagro, but the live party list is
**compacted**. Unknown uniqueIds are dropped from the middle; remaining
ids shift left. Vanilla party chrome is **index**, not
`cinematicTags.Contains("Legend")`:

- `UIAdventureBrief.SetupParty` and `UIHeroRoom.LoadData` write
  `party[i]` into portrait slot `i`. Slot 0 is the gold legend frame.
- `UIAdventureBrief.AdventurerSelected` maps index 0 → Legend picker,
  1 → LeftCompanion, 2 → RightCompanion.

So a companion (Ranger) occupies the gold slot after a legend (Onagro)
is unregistered. Combat still uses the unit definition; Ranger is not
promoted to a legend. Players cannot tell.

`HeroRosterLoadPartyPatch.Save` then **appends** stowed ids, so a save
while the pack is missing rewrites `[Onagro, Ranger, ArcaneMage]` to
`[Ranger, ArcaneMage, Onagro]`. After restore the legend sits in a
companion slot.

Hero-room `LoadData` only `SetHero`s `i < party.Count`. Unlike
`UIAdventureBrief.SetupParty` (which clears unused slots with
`SetHero(j, null)`), leftover portrait widgets stay active with the
prefab (or previous) sprite and a **null/empty** `heroId`. Observed:
third party slot showed a fiery portrait that is not on the roster
grid; log `CurrentAdventured Selected: 2 -` (empty id). Click:

1. `UIHeroRoomCurrentAdventurers.RefreshRankedUp` →
   `CheckRankedUpState` → `GetHeroLevel` / `GetHeroMaxLevel` on null.
2. `UIHeroRoomHeroData.SetHero` → `DataHelper.LoadBigPortrait` →
   `PortraitPatches.RegisterDefaults` `Path.Combine(..., heroId, ...)`
   `ArgumentNullException` (`path2`).

`UIHeroRoom.OnDone` copies `GetParty()` (every portrait `HeroId`,
including nulls) back onto `HeroRosterManager.Party` and saves, so an
empty slot can enter the blob.

This is the 3-slot UI follow-up that both save issues deferred. Same
root: compact-and-append instead of holes at the original indices.

Suggested fix: remember each stowed uniqueId's original index and splice
it back on Save; paint legend vs companion by roster role or remembered
index; `SetHero(i, null)` unused hero-room slots; do not write null ids
from `GetParty`; skip portrait resolve when `heroId` is null/empty.

## Proposed solution (Pass 1, 2026-08-15)

**Home:** LokrPatch (slot order + hero-room clear / OnDone filter). One
guard in LokrCharacterLoader `PortraitPatches.RegisterDefaults`.
**Approach:** Stop treating the live party as a compacted list for UI
and for Save. Stow `(index, uniqueId)`. Save inserts at those indices
instead of `List.Add`. Prefix `UIHeroRoom.LoadData` (or postfix after
the `party.Count` loop) to `SetHero(i, null)` on unused portrait
indices, matching `UIAdventureBrief.SetupParty`. Prefix `OnDone` so
null/empty `GetParty()` entries never reach `HeroRosterManager.Party`.
Place known members by role when the live list is still compact:
uniqueIds in `HeroRosterConfig.legends` (or
`cinematicTags.Contains("Legend")`) → slot 0; companions → 1, 2. Empty
legend slot stays empty (gold chrome, no companion). Portrait resolver:
`if (string.IsNullOrEmpty(heroId)) return null` before `Path.Combine`.
**Exact change:** `HeroRosterLoadPartyPatch`: replace `StowedPartyIds`
with a list of index+id; Load records `i` when
`!DefinitionsByUnique.ContainsKey`; skip null/empty ids (do not stow
them). Save postfix: start from the live `party` list, `Insert` each
stowed id at its recorded index (clamp to `Count`), skip duplicates.
New `UIHeroRoom` patches in LokrPatch: after LoadData's party loop,
for `i` from `party.Count` to `portraits.Count - 1` call
`currentAdventurers.SetHero(i, null, null, false, false)`. OnDone
prefix: build the written list from `GetParty()` where
`!string.IsNullOrEmpty`. Optional LoadData prefix: assign slot 0 / 1 / 2
from legend vs companion membership instead of enumeration order.
`PortraitPatches.RegisterDefaults` lambda: empty `heroId` returns null.
**Do not:** Force `party.Count == 3` or write Gerald/Ranger/ArcaneMage
(that fights the party-reset patch). Put unknown ids into
`DefinitionsByUnique`. Treat Ranger as a legend (`cinematicTags` /
roster row stay companion). Reimplement `UIHeroRoom` / a fourth party
slot. Swallow `CheckRankedUpState` globally.
**In-game verify:**
1. Party Onagro (legend) + Ranger + ArcaneMage; start Krum'thak; quit.
2. Hide the Onagro folder; load: run intact; Ranger and ArcaneMage stay
   in companion slots; legend slot empty (no gold-framed Ranger); no
   third ghost portrait; clicking empty/disabled slots does not NRE.
3. Restore the folder (save while missing is fine); load: Onagro back
   in the **legend** slot, companions unmoved.
4. Click each party portrait: detail panel opens; log has no
   `ArgumentNullException` from `PortraitPatches` / `CheckRankedUpState`.
5. Vanilla Gerald/Ranger/ArcaneMage slot: gold Gerald, two companion
   frames, load/save unchanged.
**Risk:** Save order: splice-at-index must not duplicate Onagro if he is
already in `party`. Combat still cannot spawn an unregistered legend
(empty slot 0 is correct). Vanilla 3-known parties must stay bit-identical
aside from normal session fields.

## Attempted fix (2026-08-15, LokrPatch 1.0.10 / LokrCharacterLoader 1.1.15)

Empty core slots (0–2) show Official Pack `DEFAULT_MINI` instead of a white
`Image`. The in-adventure fourth recruit slot is hidden until a fourth hero
exists. Adventure brief with a run still paints `HeroManager`, not the roster.

Resolved: 2026-08-15

Resolution: Confirmed in-game (LokrPatch 1.0.10, Character Loader 1.1.15).
Hide Onagro: gold slot shows `DEFAULT_MINI`, companions unmoved, unused
fourth slot hidden, title-screen save card matches, run brief does not
show a roster legend that is not in the run. Restore Onagro: he returns
in the legend slot.
