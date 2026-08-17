#!/usr/bin/env bash
# Build, package, and publish a GitHub Release for one LokrModding plugin.
#
# Usage:
#   scripts/release-plugin.sh <PluginName> [version]
#
#   PluginName  One of: LokrModAPI, LokrCharacterLoader, LokrLabApi, LokrLab,
#               LokrEncyclopedia, LokrModMenu, LokrPatch, SimpleUI
#   version     Optional. Defaults to the Version constant in
#               <PluginName>/<PluginName>Plugin.cs.
#
# What it does:
#   1. Verifies the plugin's own repo has no uncommitted changes, and pushes
#      it to its remote if the local branch is ahead.
#   2. Builds <PluginName>/<PluginName>.csproj in Release config. This also
#      runs DeployToBepInEx (Directory.Build.targets), same as any other
#      build, so your live BepInEx/plugins/ install gets refreshed too.
#   3. Packages the same dll/pdb/Placeholders/Sidecars layout the deploy step
#      writes into dist/<PluginName>-v<version>.zip.
#   4. Creates a GitHub Release on DippityDapper/<PluginName> at tag
#      v<version>, attaching the zip, with auto-generated release notes.
#
# Requires: dotnet, gh (already authenticated), python3.

set -euo pipefail

VALID_PLUGINS=(LokrModAPI LokrCharacterLoader LokrLabApi LokrLab LokrEncyclopedia LokrModMenu LokrPatch SimpleUI)
GITHUB_OWNER="DippityDapper"

usage() {
  echo "Usage: $0 <PluginName> [version]"
  echo "  PluginName: ${VALID_PLUGINS[*]}"
  exit 1
}

[ $# -ge 1 ] || usage
PLUGIN="$1"
VERSION_OVERRIDE="${2:-}"

valid=false
for p in "${VALID_PLUGINS[@]}"; do
  if [ "$p" == "$PLUGIN" ]; then
    valid=true
    break
  fi
done
if [ "$valid" != "true" ]; then
  echo "Unknown plugin: $PLUGIN"
  usage
fi

ROOT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
PLUGIN_DIR="$ROOT_DIR/$PLUGIN"
CSPROJ="$PLUGIN_DIR/$PLUGIN.csproj"
PLUGIN_CS="$PLUGIN_DIR/${PLUGIN}Plugin.cs"

[ -f "$CSPROJ" ] || { echo "ERROR: cannot find $CSPROJ"; exit 1; }
[ -f "$PLUGIN_CS" ] || { echo "ERROR: cannot find $PLUGIN_CS"; exit 1; }

echo "==> Checking $PLUGIN repo state"
if [ -n "$(git -C "$PLUGIN_DIR" status --porcelain)" ]; then
  echo "ERROR: $PLUGIN has uncommitted changes. Commit or stash before releasing."
  exit 1
fi

git -C "$PLUGIN_DIR" fetch origin -q
LOCAL_SHA="$(git -C "$PLUGIN_DIR" rev-parse HEAD)"
UPSTREAM_SHA="$(git -C "$PLUGIN_DIR" rev-parse '@{u}' 2>/dev/null || echo "")"
if [ -z "$UPSTREAM_SHA" ]; then
  echo "ERROR: $PLUGIN's current branch has no upstream configured."
  exit 1
fi
if [ "$LOCAL_SHA" != "$UPSTREAM_SHA" ]; then
  echo "==> Pushing $PLUGIN (local was ahead of its upstream)"
  git -C "$PLUGIN_DIR" push
fi

if [ -n "$VERSION_OVERRIDE" ]; then
  VERSION="$VERSION_OVERRIDE"
else
  VERSION="$(sed -n 's/.*Version = "\([^"]*\)".*/\1/p' "$PLUGIN_CS" | head -1)"
fi
[ -n "$VERSION" ] || { echo "ERROR: could not determine version from $PLUGIN_CS. Pass it explicitly: $0 $PLUGIN <version>"; exit 1; }
echo "==> Releasing $PLUGIN v$VERSION"

echo "==> Building $PLUGIN (Release)"
dotnet build "$CSPROJ" -c Release

OUT_DIR="$PLUGIN_DIR/bin/Release"
DLL="$OUT_DIR/$PLUGIN.dll"
PDB="$OUT_DIR/$PLUGIN.pdb"
[ -f "$DLL" ] || { echo "ERROR: build did not produce $DLL"; exit 1; }

STAGE_DIR="$(mktemp -d)"
trap 'rm -rf "$STAGE_DIR"' EXIT
PKG_DIR="$STAGE_DIR/$PLUGIN"
mkdir -p "$PKG_DIR"
cp "$DLL" "$PKG_DIR/"
[ -f "$PDB" ] && cp "$PDB" "$PKG_DIR/"
[ -d "$PLUGIN_DIR/Placeholders" ] && cp -r "$PLUGIN_DIR/Placeholders" "$PKG_DIR/"
[ -d "$PLUGIN_DIR/Sidecars" ] && cp -r "$PLUGIN_DIR/Sidecars" "$PKG_DIR/"

DIST_DIR="$ROOT_DIR/dist"
mkdir -p "$DIST_DIR"
ZIP_BASE="$DIST_DIR/$PLUGIN-v$VERSION"
rm -f "$ZIP_BASE.zip"
python3 -c "import shutil,sys; shutil.make_archive(sys.argv[1], 'zip', sys.argv[2])" "$ZIP_BASE" "$STAGE_DIR"
echo "==> Packaged $ZIP_BASE.zip"

TAG="v$VERSION"
echo "==> Creating GitHub release $GITHUB_OWNER/$PLUGIN@$TAG"
gh release create "$TAG" "$ZIP_BASE.zip" \
  --repo "$GITHUB_OWNER/$PLUGIN" \
  --title "$PLUGIN $VERSION" \
  --generate-notes

echo "==> Done: https://github.com/$GITHUB_OWNER/$PLUGIN/releases/tag/$TAG"
