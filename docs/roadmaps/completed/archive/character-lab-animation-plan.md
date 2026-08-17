# Character Lab — Animation System Plan

**Status: Implemented.** Kept as historical design record. See
[animator-workstation.md](../animator-workstation.md) and
[`animation-data-model.md`](../../../../LokrLab/docs/character/animation-data-model.md).

## 1. Objective

Extend `LokrCharacterLab`'s rig editor (`RigEditorScene`, `DraggablePart`,
`PartsListPanel`) — currently a **single static pose** editor — into a real
keyframed animation authoring tool: multiple named animation clips per rig,
each with its own sequence of poses over time, built from the same
loaded-sprite-pieces workflow that already exists.

This plan is **implemented** — see status note at top.

## 2. Where things stand today

- `rig.json` uses `ExoSkeletonDataAsset.ReloadData`'s own schema directly —
  a `"parts"` list (name/offsetX/offsetY, the *rest pose* translation) and
  an `"animations"` list (name + frames, each frame a per-part 2x3 affine
  matrix). This was reverse-engineered and proven against real game art in
  `LokrCharacterLab`'s original `RealCharacterRig`/`PosedRealCharacterRig`
  tests and is not changing.
- `RigEditorScene.OnSaveClicked` always bakes **one** pose (whatever the
  parts' current position/rotation/scale are) into **three** animations —
  `"Stand"`, `"Portrait"`, `"StandStatic"` — as a single identity-relative
  frame each. Those three names are hard-required: several base-game
  systems (`AdventureMetagameManager`, `MapHeroBarPortraitComponent`,
  `UIBuffStoreItem`, `RewardViewComponent`, `DialogViewManagerMap`) throw
  an uncaught exception if a hero's rig is missing them — this was the
  cause of a real adventure-map crash earlier in this project and is why
  `CustomRigLoader` validates for them today.
- `DraggablePart` already tracks a single absolute pose per part —
  position (`transform.position`), `RotationDegrees`, `Scale`, plus
  `Layer` (draw order) and `Visible`. The Q/W/E/R state machine
  (`RigEditorScene.EditMode`) already governs what click-dragging a part
  does (select / move / rotate / scale).
- `RigEditorScene.ComputeFrameMatrix` already solves the one genuinely
  tricky piece of this format: a per-frame matrix is applied to vertices
  that already have the rest-pose translation baked in, and matrix
  rotation happens around the world origin, not the part's own position —
  so rotating/scaling a part *in place* requires a compensating
  translation term. That derivation carries over unchanged; animation
  keyframes reuse the exact same math, just evaluated once per keyframe
  instead of once per save.
- The underlying format is **not tweened**. `ExoSkeletonAnimator.Update()`
  holds a frame for its `duration` and then switches wholesale to the
  next frame's raw matrices — no interpolation between them. This is a
  flipbook format, not a curve/keyframe-interpolation format. It shapes
  the whole plan below: the "timeline" is a discrete list of poses, not a
  curve editor, and playback in the editor can just jump between stored
  poses exactly like the game does.

## 3. Data model

### 3.1 Rest pose vs. animation poses

The rest pose (`offsetX`/`offsetY` in the `"parts"` list) is **one fixed
value per part, shared by every animation** — the schema has no per-frame
equivalent. A part's position while a specific animation frame is playing
is always *rest pose, transformed by that frame's matrix*. So a part that
visibly moves across a walk cycle isn't getting a different rest position
per frame; each frame's matrix encodes a translation *relative to* the
one shared rest position.

Consequence for the data model: every keyframe stores a pose **relative
to rest** (`deltaPosition`, `deltaRotationDegrees`, `deltaScale`), not an
absolute one. Concretely:

```
RigDocument
├── Parts: List<PartDefinition>
│     name, spritePath, restOffset (Vector2, pixels), layer (int)
└── Animations: List<AnimationClip>
      name, List<Keyframe>
        Keyframe: duration (seconds), List<PartPose>
          PartPose: partName, deltaPosition (Vector2, world units),
                    deltaRotationDegrees (float), deltaScale (float, 1 = unchanged)
```

`layer` (draw order) is modeled as **constant across every frame of every
animation** — not itself per-frame, even though the schema's
`AnimationFrame.renderOrder` technically allows it to vary. Per-frame
draw-order changes are a real but rare need (e.g. a hand passing in front
of vs. behind the body mid-swing); deferred (§6) rather than adding a
second axis of complexity to the very first version of this.

### 3.2 Editor state additions

- `RigEditorScene` gains an *active editing context*: which
  `AnimationClip` is being edited (nullable — null means "editing rest
  pose," today's existing behavior, unchanged) and which `Keyframe` index
  within it is the current scrub position.
- While editing a specific keyframe, `DraggablePart`'s visible
  position/rotation/scale in the flat editor view show **rest pose +
  delta** (so the user always sees the actual resulting pose, not the
  delta numbers) — dragging a part in this mode computes the new delta as
  `(new absolute pose) - (rest pose)` and writes that into the active
  keyframe's `PartPose` for that part. This is a thin layer on top of the
  existing Move/Rotate/Scale tools, not a new interaction model.
- Switching keyframes (scrubbing) snaps every part to
  `rest + delta(part, frame)` for the newly selected frame — a direct
  "SetPose," matching the non-interpolated nature of the format, so
  scrubbing looks exactly like what the game will actually play.

## 4. File format

No schema changes. `rig.json`'s `"animations"` array already supports
arbitrary named clips with arbitrary frame counts — today's code just
never uses more than one frame per clip. Saving becomes:

- For each `AnimationClip` in the document, for each `Keyframe`, for each
  part: compute `ComputeFrameMatrix`-style output using `Q = restOffset`
  (not the live editor position) and `M` built from
  `deltaRotationDegrees`/`deltaScale`, with `Q`'s translation additionally
  offset by `deltaPosition`. This generalizes the existing single-frame
  bake — when a clip has exactly one keyframe with all-zero deltas, the
  output is byte-identical to what `OnSaveClicked` produces today.
- The three required names (§2) are handled by convention rather than a
  special code path: if the document has no clip named `"Stand"`, Save
  auto-generates one (a single keyframe, all-zero deltas — i.e. the rest
  pose) so a rig is never missing it. Same for `"Portrait"`/`"StandStatic"`
  — auto-generate `"StandStatic"` from the rest pose if neither
  `"Portrait"` nor `"StandStatic"` exists. `CustomRigLoader`'s existing
  validation warning stays as a safety net for rigs edited outside this
  tool (hand-written JSON, a future non-Lab authoring path, etc.).

### 4.1 Backward compatibility

Existing saved rigs (one baked pose in three identically-shaped
animations, from every rig this tool has produced so far, including the
one already tested on a real hero) load as: three `AnimationClip`s
(`Stand`/`Portrait`/`StandStatic`), each with one keyframe, all deltas
zero. No migration step needed — Load's decomposition math
(§ current `LoadSavedParts`) already extracts rotation/scale per part per
frame; it's generalized to run per-clip-per-frame instead of "first
animation's first frame" only.

## 5. Editor UI

New panel: **Animation timeline**, bottom of screen (the two side panels
— parts list and mode buttons — stay where they are).

- **Clip selector**: dropdown or button row of existing clip names, plus
  "+ New Clip" (prompts for a name) and "Delete Clip" (with the required
  three exempted from deletion, or at least a confirmation that explains
  why deleting all of them will trigger the auto-generation fallback).
  Selecting a clip enters "editing this clip" mode; a "Rest Pose" entry
  (or simply deselecting) returns to editing the shared rest pose, today's
  existing behavior.
- **Frame strip**: one button per keyframe in the selected clip, showing
  its index and duration; click to scrub to it. "+ Add Frame" duplicates
  the current frame's poses as a starting point (so the common case —
  small adjustment from the previous pose — doesn't start from a blank
  rest pose every time); "Delete Frame"; a duration field (seconds) for
  the currently-selected frame.
- **Playback**: Play/Pause toggle that steps through frames in the editor
  at their real durations (a simple `Update()`-driven scrub, no need to
  reuse `ExoSkeletonAnimator` for this — the flat SpriteRenderer parts
  already show the right thing once each frame just calls the same
  "snap to rest+delta" used for scrubbing). The existing **Preview**
  button (builds the real asset via `CustomRigLoader.BuildFromFolder` and
  renders with the actual `ExoSkeletonRenderer`/`ExoSkeletonAnimator`)
  gets a small addition: play the *currently selected* clip instead of
  always `animations[0]`, so there's still a "does this match the real
  in-engine result" check independent of the editor's own flat playback.

No on-canvas timeline scrubber, no drag-to-retime keyframes, no curve
view — all deferred (§6). A flat list of frame buttons plus a duration
field is enough to author a flipbook-style animation and matches the
format's own non-interpolated nature.

## 6. Explicitly out of scope for v1

Each of these is a real feature this format could eventually support, but
adds a second axis of complexity on top of "multiple keyframed poses,"
which is already the core lift here:

- **Per-frame draw-order changes.** Layer stays constant across an
  entire rig for now (§3.1).
- **`attachPoints`** (named sockets, e.g. for held weapons) — the schema
  supports them; this plan doesn't touch them.
- **`events`** (frame-triggered gameplay hooks, e.g. footstep sounds) —
  same, schema supports them, not covered here.
- **`rootMotions`** (movement-along-a-path curves layered on an
  animation) — same.
- **Non-uniform scale / shear.** `DraggablePart.Scale` is uniform today;
  staying that way.
- **Multi-part selection / group transform.** One selected part at a
  time, matching the existing Select/Move/Rotate/Scale tools.
- **Undo/redo.** Worth having eventually; real complexity (needs a
  command history over the whole `RigDocument`, not just transform
  tweaks) that doesn't block a first working timeline.
- **Combat-required animation names.** Established earlier in this
  project that combat pulls animation names from data-driven ability/unit
  config rather than a fixed list like the map screen's — out of scope
  until something actually needs a combat-ready custom hero.

## 7. Suggested implementation order

1. `RigDocument`/`AnimationClip`/`Keyframe`/`PartPose` data model +
   generalize `ComputeFrameMatrix` and `LoadSavedParts` to operate over
   it (no UI yet — Save/Load round-trip should still produce today's
   exact single-clip output when there's only one keyframe per clip).
2. Clip selector + frame strip UI, wired to the existing Move/Rotate/Scale
   tools via the rest+delta scrubbing behavior (§3.2).
3. Add-frame/delete-frame/duration editing.
4. In-editor Play/Pause scrub playback.
5. Preview button plays the selected clip instead of always the first
   animation.
6. Required-clip auto-generation on Save (§4) + confirmation UX around
   deleting a required clip.
