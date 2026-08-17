# Vision, principles, and extensibility


A single in-game tool — **the Character Creator** — where a player can build
a complete custom hero without writing code: import or draw art, rig and
animate it, give it abilities, and test all of it together in a live
encounter they control. It's organized as a hub with multiple
**workstations**, each a focused editor for one part of "what makes a
character" (general identity, art/animation, abilities, live testing),
sharing one underlying character document so work in one workstation is
immediately usable in the next.

Everything the base game's own ExoSkeleton system is capable of — pivots,
scale, rotation, translation, per-frame draw order, multi-instance parts,
the works — should be reachable through the UI. No hidden ceiling where "the
engine can do it but the tool can't."

Just as important: **the hub and every workstation in it are meant to be
extended by plugins other than this one** — not just used as a finished
product. A third-party plugin should be able to add an entirely new
workstation to the hub, or add to/change the behavior of an existing one,
without forking this plugin's code. See §3 for how.

Power-user extensions (fully custom scripting, a dedicated encounter
builder, fully custom adventures) are explicitly **later, separate**
plugins layered on top of the core workstations, not blockers for them —
see §8. They're also the first concrete proof of the extensibility model
in §3, not a special case of it.

## 2. Guiding principles

- **Intuitive first, powerful always.** The default path through any
  workstation should require no coding and no schema knowledge. Advanced
  capability lives behind progressive disclosure (an "advanced" toggle, a
  later extension plugin) rather than being cut to keep things simple.
- **One document, many workstations.** A character being built is one
  coherent unit of data (identity + rig + animations + eventually
  AI/stats) that every workstation reads and writes, not separate
  disconnected tools that happen to share a hub screen. It's created or
  loaded exactly once, in the General workstation (§4) — every other
  workstation opens already pointed at it, the same way a non-linear video
  editor's separate tools (color, edit, audio) all operate on one open
  project rather than each importing their own copy of the media.
  Abilities are a deliberate exception, not an oversight: they're
  authored in a separate plugin (§6) and only *referenced* by id from a
  character's own `skillProgression`, the same way a character references
  a shared sound-group or a base-game asset name today — see §6 for why
  they don't belong inside "one character, one document" at all.
- **No feature ceiling below what the engine already supports.** If
  `ExoSkeletonDataAsset`/`AnimationFrame` can represent it (pivots,
  non-uniform transforms, per-frame draw order, attach points, events), the
  Animator workstation should eventually be able to author it — the current
  Rig Editor already had to solve several of these the hard way when
  *importing* real game rigs (see §5's status table), which is exactly the
  knowledge this roadmap builds forward from.
- **Built to be extended, not just used.** Every workstation's own
  registration/extension surface (§3) should follow the same
  resolver-chain/event shape `CharacterAPI` already established for
  runtime content — register a handler at a priority, let the framework
  pick the right one, never require forking core code to add or change
  behavior. This applies to the hub itself (adding a whole new
  workstation) and to each workstation individually (adding to or
  changing what it can do).
- **Extensions build on stable cores, not around gaps.** Custom scripting,
  the Encounter Creator, and Custom Adventures (§8) are all designed to
  plug into workstations that already work standalone — not to paper over
  a workstation that doesn't.

## 3. Extensibility model

Two distinct, deliberately separate extension shapes — both expected to
exist from early on, not bolted on after the fact (see §10's phasing note):

1. **Adding a new workstation.** The hub itself should expose a
   registration point — e.g. `CharacterCreatorAPI.RegisterWorkstation(...)`
   — modeled directly on `CharacterAPI`'s own resolver-chain/event pattern
   (see `LokrCharacterLoader/docs/character-api.md` for the pattern this
   should follow: `RegisterPortraitResolver`, `RegisterSoundResolver`, etc.,
   all sharing one priority-ordered, first-match-wins chain). A
   third-party plugin should be able to add an entirely new tab to the hub
   (say, a Stats workstation, or a Voice/Dialogue workstation) purely by
   registering a name/icon and a panel-builder callback — the hub doesn't
   need to know anything about what's inside a workstation it didn't
   author.
2. **Extending or changing an existing workstation.** Each workstation
   should expose its own, narrower extension points for the specific
   things plugins are likely to want to add without needing a whole new
   workstation — new part-source formats in the Animator, new spawnable
   entity types in the Sandbox, new readiness-checklist items in General.
   Same resolver-chain/event shape as above, scoped to that one
   workstation instead of the whole hub. See each workstation's own
   "Extensibility" subsection below (§4–§7) for what's expected there.
   The same philosophy extends one level further, to plugins that aren't
   part of this hub at all — new condition/effect/targeting types in the
   Ability Lab (§6) follow the identical resolver-chain shape, just scoped
   to that plugin's own extension points instead of a workstation's.

This is a deliberate continuation of an already-proven pattern in this
codebase, not a new idea: `CharacterAPI` (in `LokrCharacterLoader`) is
already the extension point for portraits, sounds, ability icons, unit
definitions, localization, and rigs, and its own default file-convention
logic is registered through that exact same surface as an ordinary,
lowest-priority participant — no special-cased fast path for "built-in"
content. The Character Creator hub and its workstations should hold
themselves to the same standard: nothing a first-party workstation does
should require a capability a third-party plugin couldn't also reach.

Section 8's three planned extensions (custom scripting, the Encounter
Creator, Custom Adventures) are the **first-party** examples of this model
in action — proof it works, not the limit of what's extensible. Any
plugin author should be able to do the same thing for their own purposes,
at either the hub level or the single-workstation level.

### The shared character document and hub gating

- **One live document, not a copy per workstation.** The character
  created or loaded in General is the same in-memory (and on-disk) document
  every other workstation reads and writes — a rig edited in the Animator
  is editing a field of the exact object General's own identity fields
  live on, not a synced copy of it. This mirrors how a non-linear video
  editor's tools all operate on one open project.
- **Only General is available with no character loaded.** Opening the hub
  with nothing loaded shows just the General tab — every other
  workstation tab is hidden or disabled until a character has been
  created or loaded, since none of them have anything meaningful to
  operate on before that point. This is a hub-level rule, not something
  each workstation opts into individually.
- **`LokrCharacterLab` hub shell is live (2026-08-11+).** Load / Home /
  Properties / Animator screen switcher, `CharacterCreatorAPI.RegisterWorkstation`,
  and gating (workstations require a loaded character) are implemented. See
  [phasing.md](phasing.md).

