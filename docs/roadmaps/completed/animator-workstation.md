# Animator workstation


*`LokrCharacterLab`'s Rig Editor, now a workstation inside the hub
(reached via Home's "Open Rig Editor" button, `CharacterLabScene.
SwitchToAnimator`), operating on the character document General
creates/loads rather than its own standalone folder-picker flow — the
"evolving into" from the original plan is done.*

### Status today

| Capability | Status |
|---|---|
| Load parts as individual PNGs | Done |
| Drag/select/rotate/scale parts (list-based selection) | Done |
| Rest pose + multiple named animation clips, each with keyframed frames | Done |
| Frame duration + easing, baked to real sub-frames at Save time | Done |
| Per-frame draw order (`RenderOrderIndex`) — a part's stacking can differ frame to frame | Done |
| A part drawn more than once per frame with different transforms (e.g. one arm sprite reused, mirrored, for both limbs) | Done |
| Import a real base-game/modded character's shipped rig into the editor | Done |
| Live Preview that mirrors the exact frame/clip/play-state being edited | Done |
| Non-uniform scale (independent X/Y) | Done — dedicated **Scale XY** tool |
| True shear/skew (matrix component Move/Rotate/Scale/Scale XY can't represent) | Displays and round-trips correctly (dedicated affine-mesh render path); still read-only, can't be *authored* by dragging |
| Custom pivots | Done — draggable pivot handle, offset persisted in `rig.pivots.json` sidecar |
| Load parts as an atlas/spritesheet | Done — grid slicing, plus an auto-detected "pixel island" picker (flood-fill) for non-uniform sheets, including merging multiple islands into one part |
| `attachPoints` (named sockets, e.g. for a held weapon) | Done |
| `events` (frame-triggered gameplay hooks, e.g. footstep sound) | Done |
| `rootMotions` (movement-along-a-path curves) | Done — Phase 4 of [animator-feel.md](animator-feel.md) (0.12.32) |
| Undo/redo | Done — snapshot-based (Ctrl+Z/Ctrl+Y), see [archive/animator-near-term-plan.md](archive/animator-near-term-plan.md) §9 |
| Third-party extension points (part-source importers, editing tools, rig validators) | Done — `AnimatorImportRegistry`/`AnimatorToolRegistry`/`AnimatorValidatorRegistry` |

### Near-term scope — complete

The five items above (custom pivots, atlas import, non-uniform-scale
authoring, `attachPoints`/`events`, undo/redo) plus the three extension
registries were all shipped; see
[`archive/animator-near-term-plan.md`](archive/animator-near-term-plan.md) for the design
each was built from and how the as-built version differs (e.g. atlas
import ended up with both grid slicing *and* the pixel-island auto-detect
called out there as a stretch goal).

Beyond that original scope, real hands-on use of the Animator (building
actual character rigs) surfaced several more workflow gaps, also now
fixed:

- **Multi-select** — Ctrl+click/Ctrl+A, group Move-dragging, and a
  "Center Selected" button (the motivating case: a rig authored off the
  Part Editor's origin didn't appear in Preview at all).
- **Background reference grid + bounded camera pan/zoom** on both the
  Part Editor and Preview viewports, so scale is legible and the camera
  can't wander into empty space or (via a large fixed world-space
  separation between the two viewports) into the other viewport's parts.
- **Mass Edit** — propagate one frame's pose edit for a part across every
  frame of a clip at once.
- A data-integrity fix ensuring an authored frame's easing/step-count
  survives a Save → close → reload round trip (`rig.animsource.json`
  sidecar) — previously baked sub-frames were mistaken for separately
  authored frames on load, silently expanding a small hand-authored
  easing setup into dozens of flat frames.

### Extensibility

Per §3's per-workstation model, the Animator should expose:

- **Part-source importers** — loose-PNG and atlas/spritesheet (above) are
  the two first-party sources, but a plugin should be able to register a
  third (e.g. "import from another modded rig," or a format specific to
  some external art tool) without touching this workstation's own code,
  the same way `CustomRigLoader`'s `ExoSkeletonResolver` registration
  lets a plugin supply a whole rig without this codebase knowing where it
  came from.
- **Editing tools** — Move/Rotate/Scale are built in; a plugin should be
  able to register an additional tool (a mesh-deform tool, or a custom
  pivot-editing tool ahead of it landing in core per the near-term scope
  above) that plugs into the same Q/W/E/R-style mode switch.
- **Rig validators** — `CustomRigLoader`'s required-animation-name check
  (`Stand`, `Portrait`/`StandStatic`) is hardcoded today; a plugin adding
  its own required-animation convention (e.g. for a new combat feature)
  should be able to register an additional Save-time validator instead of
  this workstation needing to know about every downstream consumer's
  requirements in advance. These same validators are what General's own
  readiness checklist (§4) surfaces to the user.

