# Vanilla scenario deco catalog

Working cache for [Encounter Creator](../../../../../roadmaps/started/encounter-creator.md)
Phase 14 (props). Regenerable. Do not dump the full `scenario` `assets.xml`
or per-asset text dumps back into git.

`AssetBundleManager.LoadAsset<GameObject>("scenario", name)` lowercases
the name. The rows in `deco-names.txt` are already lowercase container
names (1030) whose path or name contains `deco`.

## Files

| File | What |
|---|---|
| `deco-names.txt` | 1030 `scenario` containers with `deco` or `prop` in the name |

## Reproduce

```bash
GAME="$HOME/.local/share/Steam/steamapps/common/Legends of Kingdom Rush"
CLI="$HOME/dev/lokr-modding/lokr-modding/AssetStudioModCLI/AssetStudioModCLI_net9_linux64/AssetStudioModCLI"
DEST="$HOME/dev/lokr-modding/bepinex/docs/character-reference/_extracted/base-game/scenario"

DOTNET_ROLL_FORWARD=LatestMajor "$CLI" \
  "$GAME/legends_Data/StreamingAssets/AssetBundles/Windows/scenario" \
  -m dump --load-all --export-asset-list xml -o "$DEST"

# Parse unique Container values that contain deco or prop, then delete
# assets.xml and the per-asset dumps. Keep catalog/ only.
```

FXMega, Skill, Comic, and map-UI prefabs stay out of this list. The
`stuff` bundle is not dumped yet.
