# ReloadData: missing sprite / source clip is a NRE, not a skip

Area: LokrCharacterLoader (`CustomRigLoader`) / LokrLab Character preview
Status: unresolved-tested

As of 2026-08-15: `ExoSkeletonDataAsset.ReloadData` logs
`Cant find sprite named: X` then immediately reads `sprite.vertices`.
A `rig.json` part name that does not match a packed PNG (case-insensitive;
`#suffix` stripped on the sprite side only) is a `NullReferenceException`,
not a skip. `LoadParts` (baked path) is the silent-skip variant.

Same method: a `subAnimationOverrides` source clip that `FirstOrDefault`
misses still reads `animation3.frames` after `LogError`. Character Lab and
`CustomRigLoader` do not set that list today.

Confirmed in `ih-original/Ironhide.Legends/Ironhide/ExoSkeleton/ExoSkeletonDataAsset.cs`
(`ReloadData`). `CustomRigLoader.Build` and Lab `BuildFromFolder` both call
it with no pre-check that every JSON part name exists in the atlas.

Related: a frame part name that `FindPartIndex` misses still writes `-1`
into `renderOrder` (crash later in `ExoSkeletonRenderer.LateUpdate`).
Miss value is `-1`. The party-token / `ReplacePart` hole is
[`find-part-index-unvalidated.md`](find-part-index-unvalidated.md) — do
not merge those; different call site.

Suggested fix: skip (or fail the build with a loader log) when
`FindSprite` / source clip is null, matching `LoadParts`. Do not invent
placeholder meshes.

## Proposed solution (Pass 1, 2026-08-15)

**Home:** LokrCharacterLoader
**Approach:** Do not Harmony-replace `ExoSkeletonDataAsset.ReloadData`. Pre-filter the JSON in `CustomRigLoader.Build` (the only runtime caller besides Lab `BuildFromFolder`, which already goes through `Build`) so vanilla `ReloadData` never dereferences a null sprite. Skip missing parts the way `LoadParts` does (`ih-original/.../ExoSkeletonDataAsset.cs:53–67`: `FindSprite` null → do not add the part). Fail the build if every part is missing. Do not invent placeholder meshes.
**Exact change:** In `LokrCharacterLoader/CustomRigs/CustomRigLoader.cs` `Build` (lines 113–136), after `PackSprites` (line 124) and before `asset.ReloadData` (line 128): `JSON.Parse(jsonText)` (same `SimpleJSON` ReloadData uses at line 103 of the decompile). For each `parts` child, resolve `name` against the packed sprite list with `FindSprite` rules (case-insensitive; `#` suffix stripped on the sprite name only — `ExoSkeletonDataAsset.cs:13–33`). `PackSprites` sets `sprite.name = entry.Key` (`LokrModAPI/Assets/ModAssetLoader.cs:39`). If no sprite matches, `LokrCharacterLoaderPlugin.Log.LogWarning` (`Cant find sprite named: …`, matching vanilla's log at ReloadData line 119) and omit that part object from the JSON array — do not add a dummy `Part` / quad. Collect kept names. For each animation frame's `parts` entries, omit any whose `name` is not in the kept set so `FindPartIndex` (`ExoSkeletonDataAsset.cs:160–172`) never writes `-1` into `renderOrder`. Pass `filtered.ToString()` into `ReloadData`. If the kept parts list is empty, log an error, do not call `ReloadData`, do not cache (`Resolve` at lines 98–100 must not store a null-failed build as success — return null without adding to `builtRigsById`, or remove a failed insert). `subAnimationOverrides`: `CreateInstance` leaves the list null (lines 126–128); CustomRigLoader and Lab do not assign it, so the source-clip NRE at ReloadData lines 228–244 is latent. Do not patch that loop.
**Do not:** Reimplement `ReloadData` as a Harmony prefix (`return false` + 200-line copy). Do not transpiler vanilla for this load path. Do not invent placeholder meshes or a 1×1 white sprite. Do not merge with [`find-part-index-unvalidated.md`](find-part-index-unvalidated.md) (party-token `ReplacePart` / banner). Do not set `subAnimationOverrides` in the loader just to exercise a skip.
**In-game verify:**
1. Launch through Steam. Character Lab: Preview a known-good rig (`rig.json` names match `sprites/*.png`). Preview still builds; map/fight still show the rig.
2. Duplicate a Lab character, rename one PNG so it no longer matches a `parts[].name` (or add a bogus part name). Preview / live reload: no `NullReferenceException` on `sprite.vertices`; `LogOutput.log` has the skip warning; other parts still draw.
3. A rig whose every part name misses: build fails with a loader error, Lab preview does not crash, `builtRigsById` does not cache a broken asset.
4. Confirm a frame that named only the missing part no longer crashes later in `ExoSkeletonRenderer.LateUpdate` via `renderOrder` `-1`.
**Risk:** None for save data or vanilla content (vanilla never calls `ReloadData` at runtime; baked assets use `LoadParts`). A mismatched PNG name loses that part's mesh instead of crashing — same visual gap as `LoadParts`. Combat balance unchanged.

## Unit tests (unresolved-tested)

Moved: 2026-08-15
Tests:
- LokrModding.Tests.CharacterLoader.ContentRulesTests.MissingPackedSprite_DoesNotMatchPart
Still needs in-game confirm. A passing unit test is not a running-game
confirm.
