# Extracted base-game data

Working cache for Character File Reference generators. Not hand-edited.

## AbilitiesScript/

Vanilla ability KV (`"AbilityBehavior"` present), exported from
`legends_Data/resources.assets` TextAssets (409 files, 431 ability ids).

Re-extract:

```bash
GAME="$HOME/.local/share/Steam/steamapps/common/Legends of Kingdom Rush"
CLI="$HOME/dev/lokr-modding/lokr-modding/AssetStudioModCLI/AssetStudioModCLI_net9_linux64/AssetStudioModCLI"
TMP="$HOME/dev/lokr-modding/bepinex/docs/character-reference/_extracted/base-game/_textassets_all"
DEST="$HOME/dev/lokr-modding/bepinex/docs/character-reference/_extracted/base-game/AbilitiesScript"

DOTNET_ROLL_FORWARD=LatestMajor "$CLI" "$GAME/legends_Data/resources.assets" \
  -m export -t textasset -o "$TMP" -r

# Keep AbilityBehavior files; copy en_US.txt into Localization/; then remove $TMP.
```

Then:

```bash
python3 bepinex/docs/character-reference/generate_skills_catalog.py
python3 bepinex/docs/character-reference/generate_appendices.py
python3 bepinex/docs/api/character-reference/sync_sidebar.py
```

## FXMega/

`FXMegaList.txt` from the `scenario` AssetBundle (460 prefab names).
`FXManager.Preload` loads each line as `AssetBundleManager.LoadAsset<GameObject>("scenario", name)`.

```bash
GAME="$HOME/.local/share/Steam/steamapps/common/Legends of Kingdom Rush"
CLI="$HOME/dev/lokr-modding/lokr-modding/AssetStudioModCLI/AssetStudioModCLI_net9_linux64/AssetStudioModCLI"
BUNDLE="$GAME/legends_Data/StreamingAssets/AssetBundles/Windows/scenario"
DEST="$HOME/dev/lokr-modding/bepinex/docs/character-reference/_extracted/base-game/FXMega"

DOTNET_ROLL_FORWARD=LatestMajor "$CLI" "$BUNDLE" \
  -m export -t textasset --filter-by-name FXMegaList -o "$DEST" -r
```

See [ability-vfx-animation.html](../../../api/character-reference/ability-vfx-animation.html).

## Localization/

`en_US.txt` from the same TextAsset export, used to pair `SKILL_*` /
`COMBAT_MODIFIER_*` keys on catalog pages.

## templates/

Catalog of the `templates` AssetBundle (610 encounter prefabs) for
Encounter Creator Phase 1a/1b. Keep `catalog/` only — do not commit the
full `assets.xml` or per-prefab `EncounterBkgDefinition` dumps.

See [templates/catalog/README.md](templates/catalog/README.md) and
[encounter-creator.md](../../../roadmaps/started/encounter-creator.md).

## scenario/

Catalog of deco prefab names in the `scenario` AssetBundle for Encounter
Creator Phase 14. Keep `catalog/` only — do not commit `assets.xml` or
per-asset dumps.

See [scenario/catalog/README.md](scenario/catalog/README.md).

## resources/

Earlier filtered extracts (RLHeroes, enemies, unit stats). Still the source
for unit → skill id cross-checks.
