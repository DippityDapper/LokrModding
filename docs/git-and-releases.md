# Git repo structure and releases

This solution is split across 10 private GitHub repos (all under the
`DippityDapper` account) rather than one monorepo. This doc covers why,
how to set up a fresh clone, and how to cut a release.

---

## Repo layout

| Repo | Contents |
|---|---|
| `LokrModding` | Hub: `LokrModding.sln`, `Directory.Build.props`/`.targets`, `LokrModding.Tests/`, this `docs/` tree (minus `docs/api`), `CLAUDE.md` — plus the 9 submodules below |
| `LokrModAPI`, `LokrCharacterLoader`, `LokrLabApi`, `LokrLab`, `LokrEncyclopedia`, `LokrModMenu`, `LokrPatch`, `SimpleUI` | Each plugin's own source, independently versioned and pushable |
| `lokr-modding-docs` | The generated `docs/api/` HTML tree (base-game reference + class docs), meant to eventually be hosted on dippnet |

The hub repo holds each plugin as a **git submodule**, checked out flat as
siblings — exactly the same on-disk layout the solution already needs for
its `ProjectReference`s to resolve (`LokrLab.csproj` references
`..\LokrModAPI\LokrModAPI.csproj`, etc.). Nothing about those paths changed;
a submodule is just a folder whose content is tracked by a separate repo.

### Why this shape, not one repo per plugin with no hub

Two things don't have a single-plugin owner: the shared build scaffolding
(`Directory.Build.props`/`.targets`, the `.sln`, `LokrModding.Tests`, which
itself spans four plugins) and this top-level `docs/` tree. The hub exists
to give those a home.

### Why submodules, not duplicated dependency copies

The alternative (each dependent plugin repo vendoring its own copies of
what it needs) would require restructuring every `ProjectReference` path
and duplicating `Directory.Build.props` per plugin, plus a manual
commit-and-bump step every time a shared plugin like `LokrModAPI` changes
— which happens constantly, since this codebase is written for you to edit
across plugin boundaries in one sitting (see `cross-references.md` in any
plugin's docs). Submodules at the hub level avoid all of that: editing and
pushing inside any plugin folder is just normal git in that plugin's own
repo, with zero interaction with the hub required. The hub's submodule
pointers only matter at clone time (see below) — they don't gate day-to-day
work.

### Why no GitHub Actions

`Directory.Build.props` references the game's own assemblies
(`BepInEx.dll`, `0Harmony.dll`, `Ironhide.Legends.dll`,
`UnityEngine*.dll`, …) via `$(GameDir)`, pointed at a local Steam install.
Those are the game's copyrighted DLLs — they can't be committed to a repo
or fetched by a cloud-hosted Actions runner. A self-hosted runner on this
machine would work (it already has the game installed), but that's new
standing infrastructure reacting to pushes; releases are cut manually
instead via `scripts/release-plugin.sh` (below).

---

## Fresh clone setup

```bash
git clone --recurse-submodules https://github.com/DippityDapper/LokrModding.git
```

A plain `git clone` without `--recurse-submodules` leaves the 9 submodule
folders present but empty — run `git submodule update --init --recursive`
afterward if that happens.

`GameDir` is machine-specific and deliberately not committed. Copy the
example and point it at your own Steam install:

```bash
cp Directory.Build.local.props.example Directory.Build.local.props
# edit GameDir inside it
```

`Directory.Build.props` imports `Directory.Build.local.props` automatically
when present (see `Directory.Build.props` itself for the `<Import>`).

---

## Keeping the hub's submodule pointers current

The hub only needs a push when you want to record "these are the plugin
versions that make up the workspace right now" — day-to-day edits inside
a plugin never require touching the hub. To bring every submodule up to
its own repo's latest `main` in one shot:

```bash
git submodule update --remote
git commit -am "Sync submodule pointers"
git push
```

Not required for anything to function locally; it's only relevant if you
re-clone the hub later and want that clone to reflect current work rather
than whatever commit each plugin was on when the hub was last pushed.

---

## Cutting a release

`scripts/release-plugin.sh` (hub repo) builds one plugin in Release
config, packages it into the same `dll`/`pdb`/`Placeholders`/`Sidecars`
layout `DeployToBepInEx` writes into `BepInEx/plugins/<name>/`, and
publishes it as a GitHub Release on that plugin's own repo.

```bash
scripts/release-plugin.sh LokrLab
scripts/release-plugin.sh LokrLab 0.12.111   # override the auto-detected version
```

What it does, in order:

1. Checks the plugin's repo has no uncommitted changes and pushes it if
   the local branch is ahead of its upstream — aborts otherwise.
2. Reads the version from the `Version` constant in
   `<Plugin>/<Plugin>Plugin.cs` (or uses the explicit override arg).
3. `dotnet build <Plugin>/<Plugin>.csproj -c Release`. This also runs the
   normal `DeployToBepInEx` post-build step, so your live
   `BepInEx/plugins/` install gets refreshed the same as any other build.
4. Zips the deploy layout into `dist/<Plugin>-v<version>.zip` (gitignored,
   not committed).
5. `gh release create` on `DippityDapper/<Plugin>` at tag `v<version>`,
   attaching the zip, with `--generate-notes` for the release body.

Valid plugin names: `LokrModAPI`, `LokrCharacterLoader`, `LokrLabApi`,
`LokrLab`, `LokrEncyclopedia`, `LokrModMenu`, `LokrPatch`, `SimpleUI`.

Requires `dotnet`, `gh` (already authenticated), and `python3` (used to
zip — `zip` itself isn't installed on this machine).
