# Dialog Start / HandleReply / HandleContinue First() has no fallback

Area: vanilla dialog graph (`Dialog.Start`, `HandleReply`,
`HandleContinue`). Hurts quest Lua `dialogs.CreateDialog` graphs where
every child can fail `CheckCondition`.
Status: unresolved-tested

As of 2026-08-15: `Dialog.cs` in
`ih-original/Ironhide.Legends/Ironhide/Legends/Model/Metagame/Dialogs/Dialog.cs`
picks the next node with `children.First(...)` and no
`FirstOrDefault` / exit path:

- `Start` — first root child whose `CheckCondition` is true
- `HandleReply` — first child of the chosen response whose condition is true
- `HandleContinue` — first child with `type == Dialog` and a true condition

If the filter matches nobody, LINQ throws `InvalidOperationException`
instead of ending the dialog. Vanilla graphs always keep a passing
child; a modded or Lab-authored graph that gates every child (or
forgets a Dialog-typed continue target) crashes mid-conversation.

`FillNodeMap` also returns early on a duplicate `node.id` and skips
that node's children, so a later `jumpTo` / `inheritFrom` can miss
nodes that exist in the tree (same file; keep on the class page).

Suggested fix: Harmony-prefix the three methods to `FirstOrDefault` and
`ExitDialog` when null. Do not start that patch from the HTML-docs
track.

See
[`Dialog.html`](../../api/base-game/Ironhide/Legends/Model/Metagame/Dialogs/Dialog.html).

## Proposed solution (Pass 1, 2026-08-15)

**Home:** LokrPatch
**Approach:** Harmony prefixes that fully replace `Dialog.Start`, `HandleReply`, and `HandleContinue` so each `children.First(...)` becomes `FirstOrDefault` and a null result calls private `ExitDialog` instead of throwing. Vanilla brittle assumption (LINQ `First` with no fallback) that crashes any graph where every child fails `CheckCondition`, including quest Lua `dialogs.CreateDialog`.
**Exact change:** New `LokrPatch/Patches/DialogFirstFallbackPatch.cs`. Three `[HarmonyPrefix]` methods, each return `false` after reimplementing the original body: `Start()` (`void`), `HandleReply(int index)`, `HandleContinue()` (`void`). Keep vanilla order (flags/data/`PrepareChildren`/`startAction` or reply `DoAction`/`After`, including the existing `dialogNode.exit` / `currentNode.exit` early `ExitDialog`). Replace only the `First` pick with `FirstOrDefault`; if the node is null, `AccessTools.Method(typeof(Dialog), "ExitDialog").Invoke(__instance, null)` (or Traverse), log a warning with dialog id / current node id, and return. On a match, invoke private `MoveToNode(DialogNode)` the same way. Do not patch `MoveToNode` itself.
**Do not:** Patch `FillNodeMap` duplicate-id early return (separate miss for `jumpTo`/`inheritFrom`; out of this issue). Wrap `CheckCondition` in extra try/catch. Change dialog UI. Harmony-patch `Enumerable.First`. Rewrite `Preprocess` / jump / inherit.
**In-game verify:** 1. Build and launch via Steam / Proton. 2. Play a vanilla conversation (root child always passes) and confirm it still advances and can exit normally. 3. Trigger a mod/Lab/quest graph whose Start children all fail `CheckCondition` — dialog should close (`Exit == true`) with a LokrPatch warning, not `InvalidOperationException`. 4. Same for a reply whose children all fail, and a continue node with no `type == Dialog` child that passes. 5. Confirm `LogOutput.log` has the warning and no dialog crash.
**Risk:** Vanilla graphs always keep a passing child, so they should be unchanged. Ending a gated-all-children graph instead of crashing can skip later quest flags that a throw would also have skipped; that is recover-and-continue, not a save rewrite. No combat balance impact.

## Unit tests (unresolved-tested)

Moved: 2026-08-15
Tests:
- LokrModding.Tests.Patch.PatchRulesTests.DialogExits_WhenNoChildPasses
Still needs in-game confirm. A passing unit test is not a running-game
confirm.
