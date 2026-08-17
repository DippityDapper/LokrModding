# Reference material

## Decompiled base-game source

The full decompiled `Ironhide.Legends` C# source tree lives at
`~/dev/lokr-modding/lokr-modding/ih-original/Ironhide.Legends/` — outside
this repo (it's a sibling of `~/dev/lokr-modding/bepinex`, not a subfolder
of it). This is the
actual game logic, not extracted data — use it to read the real parsing/
game-behavior code directly instead of reverse-engineering behavior from
Harmony-patched decompiled snippets or guesswork. Every "per decompiled
source" claim throughout this docs suite that cites a specific class/method
(e.g. `ExoSkeletonDataAsset.ReloadData`, `UnityDefinitionsParser.ParseText`)
is readable in full here. The dump below is a *data* reference (an actual
shipped asset instance) — this is the *code* that reads data like it.

## `ExoSkeletonHumanRanger_MetaDataAsset.dump.txt`

A full field-by-field JSON-tree dump of a **real, shipped** character rig
(`ExoSkeletonHumanRanger_MetaDataAsset`, the skeleton every "Ranger"-based
modded hero in the community pack reuses — see
[capabilities-and-gaps.md](../capabilities-and-gaps.md) §2.1), extracted
directly from the game's `units` asset bundle
(`legends_Data/StreamingAssets/AssetBundles/Windows/units`).

This is the deserialized/baked form (Unity's internal representation),
**not** the original JSON that `ExoSkeletonDataAsset.ReloadData(string
jsonText, List<Sprite> partSprites)` was originally fed at art-import
time — that JSON isn't stored anywhere at runtime. But the field
structure is identical (`Part.name`/`vertices`/`uvs`/`color`/`triangles`,
`Animation.name`/`frames`, `AnimationFrame.time`/`matrices`/`renderOrder`/
`attachPoints`/`events`/`alphas`), so this is a reliable ground-truth
reference for reverse-engineering the JSON schema `ReloadData` expects,
cross-referenced against the parser itself in
`ih-original/Ironhide.Legends/Ironhide/ExoSkeleton/ExoSkeletonDataAsset.cs`.

Headline facts from this dump, at a glance:
- 32 named `Part`s (`Asst_Arm01`, `Asst_Arrow`, `Asst_Body01`, ...), each
  a simple 4-vertex quad (2 triangles, indices `0,1,2,2,1,3`) with its own
  UV rect into a shared texture atlas.
- 5 named `Animation`s (first one dumped: `"Vanilla"`), each with
  per-frame `MatrixFlash` (`a,b,c,d,tx,ty` — a 2D affine transform,
  Flash/Animate-style) for every visible part that frame, plus a
  `renderOrder` array giving draw order by part index.
- `pixelsToUnits = 100`.

## Tooling used

Extracted with **[AssetStudioModCLI](https://github.com/aelurum/AssetStudioMod)**
(a CLI-only fork of [AssetStudio](https://github.com/Perfare/AssetStudio)).
Kept persistently (not re-downloaded per session anymore) at
`~/dev/lokr-modding/lokr-modding/AssetStudioModCLI/AssetStudioModCLI_net9_linux64/AssetStudioModCLI`
— `lokr-modding/lokr-modding` isn't a git repo, so there's nothing to
gitignore, but it's still a third-party binary, not project source; don't
expect it in a fresh checkout of this repo (`bepinex/`) itself, only in
this dev machine's own `~/dev/lokr-modding/lokr-modding` working copy.
Version installed is v0.19.0
(`github.com/aelurum/AssetStudioMod/releases`), the `net9_linux64` asset
— a self-contained native Linux build (confirmed working against this
project's own `units` bundle — 3687 assets, Unity 2020.3.18f1). If it's
ever missing (a different machine, a fresh clone), re-grab the same
`AssetStudioModCLI_net9_linux64.zip` asset, or the `net8`/`net9_portable`
build run via `dotnet AssetStudioModCLI.dll` if a native build isn't
available for that platform (the `.exe` build is Windows/.NET-Framework-only).

**Gotchas**:
- The `net9_linux64` native build is framework-*dependent* on .NET 9
  specifically, not self-contained the way the name suggests — on a
  machine with only a newer major version installed (.NET 10 here) it
  refuses to launch ("You must install or update .NET to run this
  application") unless roll-forward is explicitly allowed:
  ```
  DOTNET_ROLL_FORWARD=LatestMajor ./AssetStudioModCLI ...
  ```
- The positional `<input path>` argument must come *before* the `-m`/etc.
  flags, not after — `AssetStudioModCLI -m info "<path>"` silently prints
  the help text instead of running (no error), while
  `AssetStudioModCLI "<path>" -m info` works.

To reproduce or pull additional assets:

```
AssetStudioModCLI "<game>/legends_Data/StreamingAssets/AssetBundles/Windows/units" -m dump --filter-by-name "<AssetName>" -o <outputDir> -r
```
(prefix with `dotnet` and point at the `.dll` instead if using a portable
build; prefix with `DOTNET_ROLL_FORWARD=LatestMajor` for the native build
per the gotcha above, if needed.)

`-m info` (no `-o` needed) is useful first to confirm an asset exists and
what type it is before dumping/exporting it. `-m export -t tex2d,sprite`
exports textures/sprites as PNG instead of dumping JSON — useful for
pulling the actual spritesheet art, though sprite/texture asset names
don't necessarily match the character name (they're per-part, e.g.
`Asst_Arm01`, and may be shared across characters), so name-filtering
those requires knowing the part names first (visible in a `dump` of the
`MetaDataAsset` itself) or exporting the whole bundle and searching by
`--filter-by-container`.

Other bundles worth knowing about (`legends_Data/StreamingAssets/AssetBundles/Windows/`):
`images`, `scenario`, `scenes`, `sounds`, `spritesheets`, `stuff`,
`templates`, `units`.
