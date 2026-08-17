# EachInList ActionsIfEmpty runs when the list is not empty

Area: vanilla ability scripting (`EachInListAction.Execute`). Hurts hand-authored
and Lab-opaque `EachInList` blocks that set `ActionsIfEmpty`.
Status: unresolved-tested

As of 2026-08-15: `EachInListAction.Execute` runs the `ActionsIfEmpty` list when
`list.Count > 0`. An actually empty list never runs it. Vanilla and Official Pack
`EachInList` blocks omit the key, so shipped content is unaffected. A mod that
writes the named fallback gets the opposite of the parse-key name.

Ability Lab has no dedicated EachInList card today; the type is still in
`AbilityParser.genericClassConfigs` and can appear as an Advanced/opaque card or
in pasted KV.

Suggested fix: Harmony-prefix `Execute` so `ActionsIfEmpty` runs when
`Count == 0`. Confirm a sandbox skill whose EachInList has an empty List and a
non-empty ActionsIfEmpty actually fires the fallback, and a non-empty List does
not.

## Proposed solution (Pass 1, 2026-08-15)

**Home:** LokrPatch
**Approach:** Harmony prefix reimplements `EachInListAction.Execute` with the parse-key meaning: `ActionsIfEmpty` runs only when `list.Count == 0`. Vanilla and Official Pack omit `ActionsIfEmpty`, so `list3` is null and the branch never runs either way.
**Exact change:** New `LokrPatch/Patches/EachInListActionsIfEmptyPatch.cs`: `[HarmonyPatch(typeof(EachInListAction), nameof(EachInListAction.Execute))]` prefix `bool Prefix(EachInListAction __instance, AbilityContext context)`. Copy vanilla `Execute` (Traverse private `attributes`), keep the `Actions` loop as-is, change only `if (list3 != null && list.Count > 0)` to `if (list3 != null && list.Count == 0)`, then return `false`. If `List` is null, return `true` and let vanilla throw.
**Do not:** Transpile a single `>` to `==` (prefix reimplementation is the LokrPatch style). Do not add an EachInList Lab card. Do not invert `ActionsIfFound` on `ActOnHexasAction` (that one already uses `Count == 0` correctly).
**In-game verify:** 1. Build, launch via Steam/Proton, Ability Lab Advanced/opaque EachInList. 2. Empty List + non-empty ActionsIfEmpty (e.g. PlaySound) — fallback fires, no Actions loop. 3. Non-empty List + ActionsIfEmpty — iterator Actions run, fallback does not. 4. Confirm a vanilla skill that uses EachInList without ActionsIfEmpty is unchanged.
**Risk:** If any unpublished mod relied on the inverted condition, fallback timing flips. Shipped KV omits the key. No save data.

## Unit tests (unresolved-tested)

Moved: 2026-08-15
Tests:
- LokrModding.Tests.Patch.PatchRulesTests.ActionsIfEmpty_OnlyWhenCountIsZero
Still needs in-game confirm. A passing unit test is not a running-game
confirm.
