# Animator feel

**Status:** Complete (Phases 1–4 in LokrLab 0.12.32; Copy/Override Rest
Pose in 0.12.33). Phase 5 is a standing process, not a leftover feature
list.
**Raised:** 2026-08-14
**Last updated:** 2026-08-15
**Owner:** LokrLab Character (Animator)

The Animator is the keystone of the Lab: it is what the old system
could not do. v1 shipped in
[animator-workstation.md](animator-workstation.md) (parts, clips,
pivots, mass edit, undo). This track is “it must feel good and not do
anything weird.” Do not move that completed doc out of `completed/`.

See also [roadmaps/README.md](../README.md).

Unity-free rules live in `LokrLab/AnimatorFeelRules.cs` (xUnit:
`AnimatorFeelRulesTests`, trait `animator-feel`).

---

## Phase 1 — Pose leak (bug) — code complete, awaiting in-game confirm

Tracked in
[`../../issues/unresolved/animator-pose-leaks-across-frames.md`](../../issues/unresolved/animator-pose-leaks-across-frames.md).
Move / rotate / scale with Mass Edit off must not change another frame
or clip.

**Shipped:** `CancelViewportDragAfterContextSwitch` after
`CommitCurrentPoseToActiveContext` on clip/frame switches (SelectClip,
SelectRestPose, Scrub, MoveActiveFrame, CreateNewClip, AddFrame,
DeleteActiveFrame, DeleteActiveClip). Mouse-up skips commit when
`PoseContextGeneration` no longer matches the drag-start value.
Inspector `BindPoseField` already dropped stale `OnEndEdit` in 0.12.7.

Do not move the issue to `resolved/` until a running Animator session
confirms it.

---

## Phase 2 — Rest Pose seeds new clips only — done

**Wanted:** Rest Pose is the template copied into frame 0 when the
user creates a new clip. After that, editing Rest Pose must not move
Walk / Attack / etc.

**Shipped:**

- `CreateNewClip` snapshots current Rest Pose into frame 0
  (`DefaultPoseFor` for every loaded part) instead of an empty frame
  that live-falls-back to rest.
- `CommitCurrentPoseToActiveContext` when editing Rest Pose compensates
  existing clip deltas (`CompensateClipDeltasForPart` /
  `AnimatorFeelRules.CompensateClipDelta`) so included, non-approximate
  poses keep their world positions. Rotation/scale on `PartPose` were
  already absolute.
- Existing rigs get the same compensation on the next rest-position
  edit (no separate bake pass).
- Inspector copy: Rest Pose is “default for new clips.” Later Rest
  Pose edits do not move Walk / Attack / other clips.
- Save-time `EnsureRequiredClip` stubs (Stand / combat) still use an
  empty frame so those follow live rest. That is intentional.

Pivot stays rest-wide in the schema (`RestPose.PivotOffset`). Group
rotate/scale uses a session temp pivot (Phase 3), not a rewrite of
each part’s rest pivot.

---

## Phase 3 — Temporary pivot for multi-select — done

Group rotate / scale uses a session temp pivot for the current
multi-selection. The Pivot tool with more than one part selected moves
that session pivot only; it does not rewrite `RestPose.PivotOffset`.

**Shipped:**

- `RigEditorScene.GetGroupPivotWorld` / `SetTemporaryGroupPivotWorld`.
  Membership change in `ApplySelection` resets the temp pivot to the
  selection centroid (`ResetTemporaryGroupPivotFromSelection`).
- `PivotTool.SupportsGroupDrag = true`: group drag writes the session
  pivot, not each part’s rest pivot.
- Rotate / Scale / Scale XY `BeginGroupDrag` use `GetGroupPivotWorld`
  (temp pivot when set, otherwise `AnimatorGroupMath.AveragePivotWorld`).
- Cleared on `ResetSession`, load, and clearing part selection.

---

## Phase 4 — `rootMotions` — done

Root motion is a curve that moves the **unit’s origin** during a clip
(walk-cycle displacement, knockback travel), not parts wiggling in
place.

Vanilla `ExoSkeletonDataAsset.ReloadData` reads top-level
`rootMotions[]` with `{ name, positions[] }` (pixel cumulative X).
Consecutive samples become `Animation.moveCurve` speeds at `1/30s`.
Combat uses `moveCurve`; `ExoSkeletonAnimator` does not apply it.
`CustomRigLoader` already passes `rig.json` through `ReloadData`.

**Shipped:**

- `AnimationClip.RootMotionPositions` — one sample per authored frame,
  pixels, empty = none.
- Frame inspector **Root X (px)**; blank clears the whole clip curve.
- Save expands via `AnimatorFeelRules.ExpandRootMotionPositions` into
  dense 30fps `positions` on `rig.json`. Sidecar
  `rig.animsource.json` stores optional `"rootMotion"` per clip.
- Load: sidecar first; else downsample `rig.json` `rootMotions` with
  `SampleRootMotionAtFrameStarts`.
- Frame add/paste/delete/move keep the list in sync.
- No live editor viewport offset (would slide the grid). Preview after
  Save uses vanilla `moveCurve`.

---

## Phase 5 — Ongoing feel pass (standing)

File further “weird” Animator behavior as issues and point them here.
Do not grow a second v1 feature list inside
[animator-workstation.md](animator-workstation.md).

**Shipped follow-ups (not a new phase list):**

- **Copy / Override Rest Pose** (LokrLab 0.12.33) — `CopyActiveFrame`
  snapshots rest; `OverrideActiveFrame` writes clipboard poses onto
  rest and compensates clip deltas. Paste as New still needs a real
  clip.
