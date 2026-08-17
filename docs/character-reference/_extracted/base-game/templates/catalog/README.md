# Vanilla encounter templates catalog

Working cache for [Encounter Creator](../../../../../roadmaps/started/encounter-creator.md)
Phase 1a/1b. Not a second Base Game HTML pass. Regenerable; do not dump
the 610 `EncounterBkgDefinition` files back into git.

## Files

| File | What |
|---|---|
| `template-names.txt` | 610 prefab container names in the `templates` bundle |
| `board-sizes.csv` | `hexWidth`, `hexHeight`, `boardState` count, `EncounterDefinition` byte size |
| `fighttesterempty-EncounterDefinition.txt` | Empty spawn lists (v1 host) |
| `fighttesterempty-boardMetadata-head.txt` | 16x20 + first `boardState` cells |

## Reproduce

```bash
GAME="$HOME/.local/share/Steam/steamapps/common/Legends of Kingdom Rush"
CLI="$HOME/dev/lokr-modding/lokr-modding/AssetStudioModCLI/AssetStudioModCLI_net9_linux64/AssetStudioModCLI"
BUNDLE="$GAME/legends_Data/StreamingAssets/AssetBundles/Windows/templates"
DEST="$HOME/dev/lokr-modding/bepinex/docs/character-reference/_extracted/base-game/templates"

DOTNET_ROLL_FORWARD=LatestMajor "$CLI" "$BUNDLE" \
  -m export --load-all --export-asset-list xml -o "$DEST"

# Optional MonoBehaviour dumps (large; summarize then delete):
# DOTNET_ROLL_FORWARD=LatestMajor "$CLI" "$BUNDLE" \
#   -m export -t monobehaviour --filter-by-name EncounterBkgDefinition -o "$DEST/bkg"
```

`AssetBundleManager.LoadAsset<GameObject>("templates", name)` loads a
room by the names in `template-names.txt`.
