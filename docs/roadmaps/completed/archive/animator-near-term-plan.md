# Animator — Near-Term Scope Plan

## Status: Implemented

Every item in this plan has shipped. See [animator-workstation.md](../animator-workstation.md) for
current status and [`../LokrLab/docs/`](../LokrLab/docs/) for how the as-built code is structured.

- **Atlas import (§4)** shipped both the planned v1 grid slicer *and* the
  v1.5-stretch auto-detect: `PixelIslandDetector.cs` (flood-fill
  connected-component detection) plus an interactive
  `IslandAtlasPickerPanel.cs` for selecting/merging/renaming/excluding
  detected islands and choosing a per-character export folder, rather
  than landing as a lower-priority follow-up.
- **True shear/skew authoring (§5's "further stretch")** stayed out of
  scope as anticipated — still read-only/`Approximate` for genuinely
  sheared poses, only independent-axis scale became authorable.

## 1. Objective

Implement the five items called out as the Animator workstation's
near-term scope in [animator-workstation.md](../animator-workstation.md)
§4 — custom pivots, atlas/spritesheet import, authoring non-uniform
scale/shear, `attachPoints`/`events`, undo/redo — plus the workstation's
own extensibility surface (part-source importers, editing tools, rig
validators) from roadmap §3/§4, landing **alongside** each capability
rather than bolted on afterward per the roadmap's phasing note (§8).

This plan is **implemented** — see status note at top.

## 2. Where things stand today

Current architecture, per [`LokrLab/docs/`](../../../LokrLab/docs/):

- `RigEditorScene` (static class) owns all editor state; `DraggablePart`
  (one per part/duplicate-instance), `PartsListPanel`, `EditorInputController`,
  `AnimationPlaybackController` are thin `MonoBehaviour` plumbing that
  delegate into it. See   [`architecture.md`](../../../../LokrLab/docs/character/architecture.md),
  [`rig-editor-scene.md`](../../../../LokrLab/docs/character/rig-editor-scene.md),
  [`supporting-classes.md`](../../../../LokrLab/docs/character/supporting-classes.md).
- `AnimationClip.cs`: `PartPose` (delta position/rotation/uniform `Scale`
  relative to `RestPose`, `Included`, `Approximate`+raw matrix fields,
  `RenderOrderIndex`); `PoseFrame` (`Duration`, `Poses`, baked `Easing`);
  `RestPose` (the one shared-across-every-animation baseline). See
  [`animation-data-model.md`](../../../../LokrLab/docs/character/animation-data-model.md).
- Import today is **loose-PNG only**: `OnLoadClicked` reads every `*.png`
  in a folder. `CharacterImporter.cs` (a *different* entry point, for
  pulling a real shipped rig out of the game's own atlas) already proves
  the atlas-crop math this plan's atlas-import feature needs — see
  [`character-importer.md`](../../../../LokrLab/docs/character/character-importer.md).
- Rotation/scale tools are Move/Rotate/Scale (Q/W/E/R), uniform scale
  only. A part's rotation/scale visually pivots around its **rest
  position** — `ComputeFrameMatrix` solves a compensating translation to
  make that true, since the schema's own matrix rotation happens around
  the world origin. This makes "rest position" and "pivot" the same
  point today, with no way to separate them.
- `DraggablePart.SetAffinePose`/`IsAffinePose` already render shear/
  non-uniform-scale poses read-only (for imported real rigs that have
  them) via a dedicated child-mesh path — `EditorInputController`
  explicitly refuses to drag an affine pose today.
- No `attachPoints`/`events` handling anywhere in this plugin — the
  schema supports both (per `ExoSkeletonDataAsset.ReloadData`) but
  nothing here reads, writes, or edits them.
- No undo/redo — a mis-drag or accidental delete is unrecoverable within
  a session short of reloading from the last Save.
- No registry/extension-point mechanism anywhere in `LokrCharacterLab` —
  every capability is hardcoded into `RigEditorScene`. `LokrCharacterLoader`'s
  `CharacterAPI` resolver chain (`ResolverChain<TResolver>`, priority-ordered,
  first-non-null-wins — see `../LokrCharacterLoader/docs/architecture.md`)
  is the established pattern this plan follows for the three new registries.

## 3. Custom pivots

### Problem

The schema has no explicit pivot field; "pivot" is implicitly always the
part's rest position, baked into `ComputeFrameMatrix`'s `Q` term. A true
pivot editor needs to let `Q` be a chosen point *offset from* rest
position, without changing what the exported matrix values mean to the
game (which doesn't know or care what "pivot" is — it only ever sees the
final `a,b,c,d,tx,ty`).

### Design

- Add `PivotOffset` (`Vector2`, part-local units, default `(0,0)` =
  today's behavior) to the in-memory part model, alongside `RestPose`.
- `ComputeFrameMatrix`/`DecodeFrameMatrix` both take `Q = RestPose +
  PivotOffset` instead of `Q = RestPose`. Because the exported matrix is
  fully general (any pivot choice round-trips through the same
  `a,b,c,d,tx,ty` fields losslessly), **no schema change is needed for
  the game to load a pivoted rig correctly.**
- `PivotOffset` itself has nowhere to live in `rig.json` (the game never
  needs it) but this editor needs it to persist across sessions for
  continued editing — store it in a small editor-only sidecar file next
  to `rig.json` (e.g. `rig.pivots.json`, part name → offset), read by
  `LoadSavedRig` if present, defaulting every part to `(0,0)` if absent
  (so every rig saved before this feature loads unchanged).
- UI: a new draggable pivot-handle gizmo, shown only for the selected
  part, in a new `EditMode` (or a modifier-held state within Select) —
  dragging it edits `PivotOffset` directly; Rotate/Scale continue to
  operate exactly as today but around `RestPose + PivotOffset`.

## 4. Atlas/spritesheet import

### Problem

Today, adding parts to a rig means one PNG per part in a folder. An
atlas workflow needs to slice one sheet into named per-part regions that
converge on the exact same internal representation loose-PNG import
already produces, so nothing downstream cares which source was used.

### Design

- v1: **grid slicing** — user provides one atlas PNG plus rows/columns
  (or fixed cell width/height); the tool crops each non-empty cell,
  trims to its alpha bounding box (reusing `CharacterImporter`'s
  existing crop/resize helpers), and prompts for a name per non-empty
  cell (defaulting to `Part_01`, `Part_02`, ... or a filename pattern).
  Confirmed regions get written out as real per-part PNGs into the
  working folder — **the same folder-of-PNGs shape `OnLoadClicked`
  already reads** — so atlas import is a one-time conversion step, not a
  parallel code path through the rest of the editor.
- v1.5 (stretch): **auto-detect** regions via connected-component
  detection on the alpha channel (flood-fill isolated non-transparent
  blobs) as a "suggest slices" helper the user can accept/adjust/rename
  before confirming, for atlases that aren't a clean grid.
- Both variants go through the new **part-source importer registry**
  (§7) as two registered importers (`GridAtlasImporter`,
  `LoosePngImporter` wrapping today's existing behavior) rather than
  special-cased branches in `OnLoadClicked`.

## 5. Authoring non-uniform scale/shear

### Problem

`DraggablePart.Scale`/the Scale tool are uniform-only; `SetAffinePose`
exists but is explicitly read-only (`IsAffinePose` blocks dragging in
`EditorInputController`) because there's no rotation/uniform-scale pair
that represents an arbitrary sheared matrix, and no UI to edit one
directly.

### Design

- Extend `PartPose` with independent `ScaleX`/`ScaleY` (replacing the
  single uniform `Scale` as the general case; existing rigs load with
  `ScaleX = ScaleY = Scale` unchanged). True shear (skew) beyond
  non-uniform scale is a further stretch, not required for v1 — most
  real non-uniform cases (e.g. a stretched cape) are axis-scale, not
  skew.
- New tool: **Scale XY**, a corner/edge-handle gizmo (two independent
  edge handles instead of Scale's single uniform handle) bound to its
  own hotkey, registered through the new **editing-tool registry** (§7)
  alongside Move/Rotate/Scale rather than hardcoded as a fifth
  hardcoded `EditMode` case.
- `ComputeFrameMatrix`/`DecodeFrameMatrix` generalize to independent
  X/Y scale factors (already algebraically closer to what
  `DecodeFrameMatrix`'s shear-tolerance check computes internally than
  the uniform-scale path is). Once decomposition accepts non-uniform
  scale as an exact (non-`Approximate`) case, previously-`Approximate`
  imported poses that were *only* non-uniform-scaled (no true shear)
  become editable instead of read-only — a real reduction in the
  read-only surface, not just new authoring capability.
- Poses with genuine shear (off-diagonal skew, not reducible to
  independent X/Y scale) stay `Approximate`/read-only — true skew
  authoring is out of scope for this plan.

## 6. `attachPoints` / `events`

### Problem

Schema supports both per `ExoSkeletonDataAsset.ReloadData`; nothing in
this editor reads, writes, or edits them today. First real consumer is
the planned Ability Creator workstation (roadmap §5), which needs an
attach point to anchor a held weapon or VFX spawn location.

### Design

- **Attach points**: named sockets, position (+ likely rotation) per
  frame, modeled the same shape as a `PartPose` but with no sprite —
  render as a small distinct gizmo (e.g. a cross-hair icon) in the
  viewport, draggable via the same global-drag mechanism
  `EditorInputController` already uses for parts (`RigEditorScene.SelectedPart`
  generalizes to "selected part or attach point"). Listed in a new
  "Attach Points" section of `PartsListPanel` (or a sibling panel),
  add/rename/delete.
- **Events**: a named string tag fired at a specific frame — no spatial
  component, so no viewport gizmo. Authored as a small list attached to
  a `PoseFrame` (add/remove/rename) in `AnimationTimelinePanel`, next to
  the existing duration/easing controls for the selected frame.
- Both round-trip through `rig.json`'s existing schema fields — no
  format change, purely new UI + new fields on the in-memory
  `PoseFrame`/document model, following the "no schema changes, ever"
  convention already established for every prior feature in this
  plugin.

## 7. Extensibility surface

Per roadmap §3/§4, three registries, each following the same
priority-ordered pattern `CharacterAPI.ResolverChain<T>` already
establishes elsewhere in this solution (see
`../LokrCharacterLoader/docs/architecture.md`) — register a handler at
a priority, first-registered-at-highest-priority wins where relevant,
without the workstation needing to know what registered:

- **Part-source importers.** `AnimatorImportRegistry.RegisterImporter(name, priority, importerFn)`.
  Ship two first-party entries built as part of this plan: loose-PNG
  (today's existing behavior, wrapped rather than rewritten) and grid
  atlas (§4). A third-party plugin registers a third the same way — the
  import UI just lists whatever's registered.
- **Editing tools.** `AnimatorToolRegistry.RegisterTool(name, hotkey, gizmoFn, dragHandlerFn)`.
  Move/Rotate/Scale (existing) and Scale XY (§5) become the first-party
  registered set instead of a hardcoded `EditMode` enum; a plugin can add
  a tool (e.g. a future mesh-deform or true-pivot-drag tool) without
  `RigEditorScene`/`EditorInputController` needing a new case per tool.
- **Rig validators.** `AnimatorValidatorRegistry.RegisterValidator(Func<RigDocument, IEnumerable<string>> validateFn)`,
  run at Save time, results shown as warnings (non-blocking, matching
  `CustomRigLoader`'s existing required-animation-name check today).
  That existing hardcoded check (`Stand`/`Portrait`/`StandStatic`)
  becomes this registry's first built-in entry rather than a special
  case — see [`cross-references.md`](../../../LokrLab/docs/cross-references.md)
  for the check being replaced.

Building these as real registries with two-or-more first-party
participants each (not a speculative single-implementor interface) is
why §7 isn't first in the implementation order below — each registry is
designed opposite its first concrete second-user, per §8.

## 8. Explicitly out of scope for this plan

Carried over from the roadmap (§4/§9) and not re-litigated here:

- **True shear/skew authoring** (only non-uniform axis-scale, §5).
- **`rootMotions`** (movement-along-a-path curves) — still untouched.
- **Multi-part selection / group transform** — one selected part (or
  attach point) at a time, matching the existing tool model.
- **Combat-required animation names** — still a data-driven, not
  fixed-list, requirement; still deferred pending the Sandbox workstation
  actually needing it (roadmap §9).
- **Command-pattern (inverse-operation) undo.** See §9 below — this plan
  deliberately chooses the cheaper snapshot approach instead.

## 9. Undo/redo

### Design choice

`RigEditorScene` is a static class with direct field/dictionary mutation
throughout, no central mutable "document" object, and no existing
indirection layer any command pattern could hook into without touching
every mutating method. Retrofitting a full command-pattern (paired
do/undo operations per action) would mean rewriting most of the file.

Instead: **snapshot-based undo** — serialize the whole in-memory document
(parts, poses, clips, pivots, attach points/events once those exist) to
an in-memory stack entry before each user-initiated mutating action
(drag-end, add/delete frame, add/delete clip, delete part, etc.), bounded
to a reasonable depth (e.g. 50), Ctrl+Z/Ctrl+Y pop/restore a full
snapshot. More memory and less "surgical" than command objects, but a
small fraction of the implementation cost, and correct by construction
(a restored snapshot is exactly a prior real state, not a hand-maintained
inverse operation that can drift from what its forward operation
actually did).

### Why last

Placed last in the implementation order (§10) deliberately — it needs to
wrap whatever the full mutation surface looks like once pivots, atlas
import, non-uniform scale, and attach points/events all exist, so it's
implemented once against the final shape of the document rather than
rewritten as each earlier feature lands.

## 10. Suggested implementation order

1. **Custom pivots** (§3) — self-contained, no dependency on the
   registries below, and the roadmap's most-requested "everything the
   exoskeleton system allows" gap.
2. **Part-source importer registry + atlas import** (§7 importers, §4) —
   built together so the registry has two real implementors from day
   one instead of a speculative single-implementor interface.
3. **Editing-tool registry + non-uniform scale/shear** (§7 tools, §5) —
   same reasoning: Move/Rotate/Scale become registered entries alongside
   the new Scale XY tool, not retrofitted after.
4. **`attachPoints`/`events`** (§6) — depends on nothing above, but
   ordered after atlas import/non-uniform scale since it's the most
   directly useful to the *next* workstation (Ability Creator, roadmap
   §5), so later is fine but not urgent to front-load.
5. **Rig-validator registry** (§7 validators) — wraps the existing
   required-animation-name check as its first entry; small and
   low-risk, sequenced here mainly because it has no other dependents.
6. **Undo/redo** (§9) — last, per §9's "why last."
