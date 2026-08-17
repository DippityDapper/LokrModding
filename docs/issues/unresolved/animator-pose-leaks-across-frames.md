# Animator: pose edits leak to another frame or clip

Area: LokrLab Animator (`RigEditorScene`)
Status: unresolved

As of 2026-08-14: moving, rotating, scaling, or otherwise altering a
part sometimes changes that part on another frame or another clip, even
when Mass Edit is off.

Commit should write only the active context
(`CommitCurrentPoseToActiveContext` / `ApplyContextPoseToParts` in
`RigEditorScene`). Rest Pose moving every clip because clip poses are
live deltas from rest is a separate feel item — see
[`../../roadmaps/completed/animator-feel.md`](../../roadmaps/completed/animator-feel.md).
This issue is the leak with Mass Edit off.

Do not mark resolved until a running Animator session can edit one
frame of one clip without changing other frames or clips (Mass Edit
off).

## Attempted fix (LokrLab 0.12.7) — awaiting in-game confirm

Tried 2026-08-14. Do not mark resolved until confirmed in the running
Animator.

Hypothesis: a focused Inspector pose field fires `onEndEdit` *after*
a timeline chip or Node Tree clip click has already switched
`activeClip` / `activeFrameIndex`. `SetPartPosition` (and rotate /
scale / shear) then writes the old field into the new frame or clip.
Viewport drags commit on mouse-up to the context that is still active,
so this path is Inspector-led.

What 0.12.7 shipped:

- `RigEditorScene.PoseContextGeneration` increments on clip/frame
  switches (and playback frame advance).
- `InspectorPanel.BindPoseField` records that generation on focus and
  drops `OnEndEdit` when it no longer matches.

Not covered here: Rest Pose moving every clip because poses are live
deltas from rest ([animator-feel.md](../../roadmaps/completed/animator-feel.md));
Pivot is rest-wide. If the leak is a viewport drag with Mass Edit off
and no Inspector field involved, this will not be enough.

## Proposed solution (Pass 1, 2026-08-15)

**Home:** LokrLab
**Approach:** Lab guard on the viewport-drag path that 0.12.7 left open. EventSystem runs before `EditorInputController.Update`, so releasing a Move/Rotate/Scale drag on a timeline chip or Node Tree clip button fires `ScrubToFrame` / `SelectClip` first (commit to the old frame, then `ApplyContextPoseToParts` skips `ActivelyDraggingPart`). The same-frame mouse-up then `CommitCurrentPoseToActiveContext` into the *new* frame. `[` / `]` during a held drag is the same leak without a chip. Do not treat Inspector `PoseContextGeneration` as the whole fix.
**Exact change:** In `RigEditorScene.SelectClip` (when the clip actually changes), `SelectRestPose`, `ScrubToFrame` / `ScrubToBakedFrame` (when the index changes): after the leading `CommitCurrentPoseToActiveContext`, clear `ActivelyDraggingPart` and `ClearActivelyDraggingGroup` so the following `ApplyContextPoseToParts` applies the new context to the dragged part. Add `EditorInputController.CancelActiveDrag()` (clear `isDragging` / group / reference flags, no commit) and call it from those switch sites. On mouse-up, skip `CommitCurrentPoseToActiveContext` when `PoseContextGeneration` differs from the value recorded in `TryBeginDrag`. Leave `TickPlayback` + Mass Edit alone — that path must keep the drag skip so playback can run through a mass edit.
**Do not:** Do not ship another Inspector `BindPoseField` generation gate as the fix (0.12.7 already did that; `generation >= 0` is only a residual hole if the Inspector path still fails with no drag). Do not rewrite clip poses as absolute (rest-delta feel is `animator-feel.md`). Do not make `SelectedPart` leave `multiSelection` (Mass Edit off already skips `PropagateMassEdit`). Do not cancel drags from `BumpPoseContext` itself (playback frame advance would stomp Mass Edit).
**In-game verify:** 1. Animator, Mass Edit off, two frames in one clip plus a second clip. 2. Move tool: drag a part on frame 1 and release on the frame 2 chip; frame 1 should keep the edit, frame 2 should not. 3. Same drag, release on a different clip in the Node Tree. 4. Hold a drag and tap `]` / `[`; edit must stay on the frame where the drag began. 5. Repeat with Inspector Pos fields (0.12.7 path) and confirm Pivot still rest-wide. 6. Mass Edit on: one drag still propagates across the clip while playback can run.
**Risk:** Lab-only authored poses. No save-game or vanilla combat. Wrongly clearing `ActivelyDraggingPart` during Mass Edit playback would snap the part every baked tick — that is why TickPlayback is excluded.
