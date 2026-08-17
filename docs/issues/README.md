# BepinEx issues

Process and format follow `~/agent-docs/issues/README.md`. This folder
holds problems that reproduce inside `~/dev/lokr-modding/bepinex` only.

```
docs/issues/
  README.md
  unresolved/          open problems (no passing unit test, or none possible yet)
  unresolved-tested/   unit test exists and passes; in-game confirm still required
  resolved/            confirmed in the running game
```

One file per issue, kebab-case, no ticket ids. Status is which folder
the file lives in: move it (do not copy). On resolve, set
`Status: resolved` and append a `Resolved` / `Resolution` block.

Do not mark an issue resolved, and do not move it to `resolved/`, until
the fix has been confirmed to work in the running game. Shipping a
build, writing the intended change, or a passing unit test is not
enough.

**Unit tests:** when an issue’s rule has a passing xUnit test, move the
file to `unresolved-tested/`, set `Status: unresolved-tested`, and
append a **Unit tests** block (test names + date). That folder means
“tests say the rule holds; the game can still fail.” Leave Unity-only
issues (Lab chrome, z-order, audio, Steam) in `unresolved/` until
in-game confirm. Suite plan:
[`../roadmaps/completed/test-suite.md`](../roadmaps/completed/test-suite.md).

If a `resolved/` entry was moved early, move it back and note that it
was not yet confirmed. If `unresolved-tested/` was used for an issue
with no test, move it back to `unresolved/`.

If another doc mentions a problem (capabilities, roadmaps, plugin
`docs/`), point at the issue file instead of restating it.

The 2026-08-15 two-pass campaign (design, then implement, still no move
until in-game confirm) is in [`two-pass-resolution.md`](two-pass-resolution.md).
A full-tree backup taken before that work is recorded there. Passing
unit tests use [`unresolved-tested/`](unresolved-tested/) per
[`../roadmaps/completed/test-suite.md`](../roadmaps/completed/test-suite.md);
that is not `resolved/`.

Host-wide or cross-project issues stay in `~/dev/agent-docs/issues/`.
