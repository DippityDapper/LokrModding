# Code Documentation Standards

The rule, in one sentence: **every `public`/`internal` type, property, field,
event, and method gets an XML doc comment; nothing gets a plain `//`
comment.** This formalizes a convention the codebase already follows almost
everywhere (as of 2026-08-11, a full scan found zero plain `//` comment
lines across all five plugins) — it isn't a new departure, just written down
so it stays that way as new plugins/agents touch this code.

## The rule

- **`/// <summary>` on everything.** Classes, structs, enums, interfaces,
  properties, fields, events, methods, constructors — if it's `public` or
  `internal`, it gets a one-line (occasionally two-line) `<summary>`. No
  exceptions for "obvious" one-liners like `internal static ManualLogSource
  Log;` — the summary can be short, but it still exists.
- **No plain `//` comments, anywhere.** If something needs explaining
  beyond what the `<summary>` says, that explanation goes in `<remarks>`,
  not a floating `//` line above or beside the code. This isn't just
  style — a `//` comment is invisible to the generated HTML API docs (see
  below), so information left in one effectively doesn't exist for anyone
  reading the docs site instead of the raw source.
- **Keep both short.** `<summary>` is one sentence, occasionally two — this
  is a signature-plus-one-liner reference, not an essay (same guidance
  `docs/api/GENERATE_CLASS_DOCS.md` already gives for how these get
  rendered). `<remarks>` should be as short as the actual problem allows:
  enough to capture a hidden constraint, a workaround for a specific bug, a
  non-obvious invariant, or "why this instead of the obvious alternative" —
  and not a word more. If it needs paragraphs, it probably belongs in that
  plugin's own `docs/*.md` instead, with `<remarks>` just linking there.

## `<remarks>` is this project's "why" comment, not "what"

This lines up with the root `~/dev/CLAUDE.md`/repo-wide instruction to
default to no comments and only explain non-obvious *why*: `<remarks>` is
exactly that — always expressed as XML doc instead of an ad hoc `//`, so it
survives into the generated docs site automatically. Don't restate what the
signature already makes obvious.

```csharp
// Bad — restates the signature, and it's a // comment, so it's invisible
// to the generated docs entirely.
internal static void SwitchToAnimator(string characterFolder) { ... }

/// <summary>Lazily builds the Animator (once) the first time it's needed, then always OnLoadClicked's the given character folder.</summary>
/// <remarks>RigEditorScene.Build() is expensive, hence "once" — building it eagerly on every Lab.Open() would cost a frame spike nobody asked for.</remarks>
internal static void SwitchToAnimator(string characterFolder) { ... }
```

`<summary>` alone is enough for anything self-explanatory from its name and
signature; only add `<remarks>` when there's a real *why* to capture.

## Current coverage

**Total, as of 2026-08-11.** An initial audit found about 156 of ~565
`public`/`internal` members across the solution (~28%) missing a doc
comment; all were backfilled the same day (plus a handful of
implicit-`public` interface members — e.g. `IAnimatorTool`'s own members
don't write the `public` keyword, since interfaces make that implicit —
that a first, keyword-literal audit pass missed and a second pass caught).
A solution-wide scan (including implicit-public interface members) now
finds zero undocumented `public`/`internal` declarations. Keep it that
way: **new code must always include a doc comment**, not rely on a future
backfill pass.

## Keeping the generated HTML docs in sync

The pages under `docs/api/classes/**/*.html` are generated output, not
hand-authored — see `docs/api/GENERATE_CLASS_DOCS.md` for the full
mechanics. The part that matters for this doc:

- Every class's and member's one-line description in the generated HTML is
  sourced directly from that declaration's own `/// <summary>` — not from
  `classes.json` (which only supplies name/plugin/namespace/location, plus
  a fallback description if no doc comment exists yet).
- Each page splits its members into a **Public API** section and an
  **Internal** section (each with its own Properties/Methods
  subsections), matching the `public`/`internal` keyword the source
  actually used — not just `public`. This matters because this codebase's
  real, intended-to-be-documented surface is overwhelmingly `internal`
  (BepInEx plugin assemblies aren't consumed like a public NuGet package),
  so a page that only showed `public` members would be blank for most
  classes here. A section with nothing in it renders "None." rather than
  a `TODO` — an empty Public API section on a fully-internal class is
  expected, not a gap.
- **Whenever a `/// <summary>` (class-level or member-level) changes,
  rebake the HTML** so the docs site actually reflects it:
  ```bash
  cd docs/api
  python3 generate_docs.py --sync-descriptions
  ```
  This rewrites only the `<!-- AUTO-DOC:... -->` blocks the generator
  owns; any hand-written Remarks/Usage Examples prose already on a page is
  left untouched. Treat this the same as "regenerate after changing
  architecture" in the root `CLAUDE.md`'s "Updating Documentation"
  workflow — it's the code-comment equivalent of that same rule.
- Adding a **brand-new** class/struct/enum/interface that's meant to be
  part of the documented surface also needs an entry in
  `docs/api/classes.json` (`name`/`plugin`/`namespace`/`sourceFile`) —
  only then will a plain `python3 generate_docs.py` (no flag) create its
  page. See the next section for why this step doesn't happen
  automatically.
- If a member you expect to see on a page is missing even though its
  class *is* in `classes.json`, and it isn't simply undocumented (see
  "Current coverage" above), check that it's actually `public` or
  `internal` — `private`/`protected` members are deliberately excluded,
  same as before.

## `classes.json` is curated, not exhaustive

`docs/api/classes.json` is a hand-maintained list of "classes worth
documenting," not a mirror of every type in the solution. Per
`GENERATE_CLASS_DOCS.md`, individual Harmony patch classes with no tie to
a documented `CharacterAPI` extension point, and other narrow
implementation detail, are deliberately left out. Practically, this
means:

- The generated site's class list will always lag the actual codebase
  somewhat by design — a missing page for a class you expect to find is
  likely a stale manifest, not a generation bug; check `classes.json` for
  an entry with that class's `name` before assuming the tooling is
  broken.
- It had lagged *badly* as of 2026-08-11 (33 entries vs. roughly 101
  actual source files solution-wide, worst in `LokrCharacterLab` at
  9/53) — fixed the same day with a one-time pass adding the 80 missing,
  in-scope classes across all five plugins (now 113 entries). Keep it
  current going forward: add an entry whenever a new class is meant to be
  part of the documented surface, per the previous section.
