# Animator Play is laggy, and gets worse with more frames

Area: LokrLab Character Animator (`RigEditorScene`, `AnimationTimelinePanel`)
Status: resolved

As of 2026-08-17: pressing Play on a clip in the Animator workstation is
noticeably laggy, and the lag scales with the clip's frame count — even a
clip with only 50-60 frames is already bad.

## Root cause

`AnimationPlaybackController` calls `RigEditorScene.TickPlayback(deltaTime)`
every `Update()`. `TickPlayback` (`RigEditorScene.cs:2513-2546`) is cheap
most ticks, but every time a baked sub-frame's `Duration` elapses — i.e.
periodically throughout playback, not once per loop — it does two
operations whose cost scales with frame count, run synchronously on the
main thread:

**1. A full non-incremental rebake of every clip in the rig, twice, on every
rollover.** `TickPlayback` calls `RebakeAllClips()` directly
(`RigEditorScene.cs:2537`), then calls `RefreshTimeline()`
(`RigEditorScene.cs:2544`), which calls `RebakeAllClips()` a second time
(`RigEditorScene.cs:2556`) — a redundant double rebake per tick.
`RebakeAllClips` (`RigEditorScene.cs:3169-3175`) loops every clip in the
rig; `RebakeClip` (`RigEditorScene.cs:3160-3166`) loops every `PoseFrame`
in that clip; `RebakeFrame` (`RigEditorScene.cs:3132-3157`) `Clear()`s and
rebuilds that frame's `BakedFrames` from scratch, deep-cloning via
`RigSnapshotCloner.Clone` and, for eased frames, calling `InterpolateFrame`
(`RigEditorScene.cs:3179-3207`) once per easing sub-step — each call
allocating a new `HashSet<string>` (union of both frames' part names) plus
a new `PoseFrame`/`PartPose` per part. Total cost is O(total frames across
every clip in the rig × parts × easing steps), redone from scratch on every
tick that advances a sub-frame. `RebakeFrame`'s own doc comment
(`RigEditorScene.cs:3131`) acknowledges the design is "deliberately
non-incremental" on the assumption that "a rig's clips total at most a few
hundred Lerp calls combined" — that assumption is what breaks down as
authored frame count grows past the range it was sized for.

**2. A full UI rebuild of the frame-chip strip, every rollover — the
larger cost.** `RefreshTimeline()` also calls `AnimationTimelinePanel.Refresh`
(`RigEditorScene.cs:2558`), which (`AnimationTimelinePanel.cs:94-114`) does
`chipRow.Clear()` (line 101) then rebuilds **every** frame chip from
scratch via a loop over `activeClip.PoseFrames.Count` (lines 105-108),
calling `BuildFrameGroup` per frame. `BuildFrameGroup`
(`AnimationTimelinePanel.cs:130-155`) does one real `UiButton.Create(...)`
(a Unity `Instantiate` + layout pass) per frame chip, plus one more per
baked sub-chip (lines 144-154) — real GameObject churn scaling with frame
count (and multiplied further by `EasingSteps`), repeated several times per
second throughout playback. `UiStack.Clear()`
(`SimpleUI/UiStack.cs:208-222`) is the destroy-and-rebuild-everything
primitive; its own doc comment contrasts it with `UiList<T>`'s diff-by-key
approach, which every *other* per-tick-refreshed panel (`SceneTreePanel`,
`InspectorPanel`) already migrated to for exactly this reason.
`AnimationTimelinePanel` was never migrated off `Clear()`.

At 50-60 frames (times `EasingSteps` for the chip strip, times rig clip
count for the rebake), this is almost certainly the dominant cost — real
GameObject instantiate/destroy/layout work, not just allocation.

This is a distinct issue from
[`animator-pose-leaks-across-frames.md`](animator-pose-leaks-across-frames.md)
(a correctness bug in commit/apply timing, not performance).

## Likely fix

- Drop the redundant second `RebakeAllClips()` call: `TickPlayback` already
  rebakes at line 2537 before calling `RefreshTimeline()`, which rebakes
  again at line 2556. `RefreshTimeline` could take a `skipRebake` flag, or
  playback could call a lighter refresh that skips it.
- During playback specifically, avoid rebaking every clip in the rig on
  every tick — only the active clip's active frame region needs a fresh
  bake to keep editing responsive; the rest didn't change.
- `AnimationTimelinePanel.Refresh` should not destroy/recreate every chip
  every tick. Either skip the timeline refresh during Play (chip contents
  don't change while playing, only which chip is highlighted as active —
  that's a color/state update, not a rebuild), or migrate it to `UiList<T>`
  the way `SceneTreePanel`/`InspectorPanel` already did, diffing by frame
  index instead of clearing and rebuilding.

## Reproduce

1. Open the Animator workstation on a rig with a clip of 50+ frames.
2. Press Play.
3. Observe stutter/lag during playback; compare against a short clip
   (5-10 frames) on the same rig, which plays smoothly.

Do not mark resolved until Play on a 50+ frame clip runs smoothly in the
running game.

## Fix implemented — in-game confirm pending

- `RigEditorScene.TickPlayback` (`LokrLab/Character/Editor/RigEditorScene.cs:2513`)
  no longer calls `RebakeAllClips()` on sub-frame rollover, and no longer
  calls the full `RefreshTimeline()`. `BakedFrames` is already fresh from
  the moment Play starts (`TogglePlayback`'s own `RefreshTimeline()` call),
  and nothing mutates `PoseFrame` data during a pure playback tick — an
  edit action pauses playback first (`PausePlayback`) — so re-deriving the
  bake or rebuilding every dependent panel on every rollover was pure
  waste.
- Added `AnimationTimelinePanel.RefreshActiveHighlight`
  (`LokrLab/Character/Editor/Animation/AnimationTimelinePanel.cs`), which
  recolors the already-built frame/baked-sub-chip buttons in place
  (`UiButton.SetColor` — a plain `Image.color` set, no allocation, no
  `Instantiate`/`Destroy`) instead of `Refresh`'s `Clear()`+full-rebuild.
  `TickPlayback` now calls this instead of `RefreshTimeline()`. The chip
  strip's structure (frame count, baked-sub-chip count) never changes
  mid-play, only which chip is highlighted, so nothing but color needs to
  update per tick.
- `RefreshTimeline()` itself (used by every actual edit path — add/delete/
  move frame, clip switch, Save, etc.) is unchanged; only the playback
  tick path was narrowed.
- Solution builds clean; full `LokrModding.Tests` suite passes (242/242,
  unaffected — this code is Unity-editor-only, not covered by that suite).

**In-game verify:** Play a clip with 50+ frames in the Animator
workstation; confirm it runs smoothly with no stutter, and that editing
(add/delete/move frame, switching clips, Copy/Paste/Override frame) still
updates the chip strip correctly afterward.

## Resolved (2026-08-17)

Confirmed in the running game: framerate during Play is far better.
