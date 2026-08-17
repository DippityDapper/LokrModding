# Unresolved-issue two-pass campaign (2026-08-15)

Backup of the tree before this work:

- Archive: `/home/dippity/dev/lokr-modding/backups/bepinex-20260815-172132.tar.gz`
- SHA-256: `9e7a4567f575ee39adeadd5877b8a3cb15114fb13b155e9ae8c1292936cd8433`

Restore: `tar -xzf` that file into a scratch directory, or replace `bepinex/` only after confirming you want to discard later edits.

## Rule that does not move

Do not mark an issue resolved, and do not move it to `resolved/`, until the
fix is confirmed in the running game. Shipping a build is not enough. Leave
the file in `unresolved/` until that confirmation. Process:
[`README.md`](README.md).

## Pass 1 — well-defined solution (no code)

For each file in `unresolved/` except this playbook and `README.md`, append
exactly one section:

```
## Proposed solution (Pass 1, 2026-08-15)

**Home:** LokrPatch | LokrCharacterLoader | LokrLab | LokrEncyclopedia | verify-only | do-not-patch
**Approach:** one paragraph: Harmony prefix/postfix/transpiler, Lab guard, or no code
**Exact change:** types, method signatures, prefix vs postfix, skip-and-log vs reimplement
**Do not:** what would be too broad, a second attempt of a failed approach, or a vanilla rewrite
**In-game verify:** numbered steps in the running game (Steam / Proton)
**Risk:** save data, combat balance, vanilla content
```

Home rules (from `LokrPatch/docs/overview.md` and `conventions.md`):

- Vanilla crash or brittle assumption that helps even with zero mods: `LokrPatch`.
- Mod content load / CharacterAPI / our existing patches: `LokrCharacterLoader`.
- Editor UI, templates, pickers, Lab-only chrome: `LokrLab`.
- Click-the-button / hear-the-clip / reopen-the-lab: `verify-only` until that
  session exists. Do not invent Encyclopedia UI.
- Full missing vanilla feature (for example RANGE_CONE hex fill): prefer Lab
  hide/warn plus a small, cited combat patch only if the decompile already
  has Width/MinT/MaxT and the three call sites can share one helper. If that
  is a rewrite, say `do-not-patch` for combat and Lab-warn only.

If the issue already has an **Attempted fix** block, propose the *next*
approach. Do not repeat sort-order promotion for the Forfeit dialog. Do not
repeat Inspector generation-gating for the pose leak unless you can name a
second leak path.

Do not implement C#. Do not edit plugin source. Do not start Pass 2 from
Pass 1.

## Pass 2 — attempt the fix

Started 2026-08-15 after review go-ahead. Implement the **Proposed solution**
in the named plugin. Follow
[`docs/code-documentation-standards.md`](../code-documentation-standards.md)
(`/// <summary>` on public/internal, `<remarks>` for why, no plain `//`).
Rebake `docs/api` only if a doc comment or new class is added.

Each agent must return numbered in-game test steps. The parent copies them
into [`docs/roadmaps/started/issue-resolution-in-game-tests.md`](../roadmaps/started/issue-resolution-in-game-tests.md)
as that agent finishes. Agents do not edit that file themselves.

Still do not move files to `resolved/`.

Pass 2 implementation finished 2026-08-15 (LokrPatch 1.0.6, LokrCharacterLoader
1.1.12, LokrLab 0.12.28). In-game checklist:
[`docs/roadmaps/started/issue-resolution-in-game-tests.md`](../roadmaps/started/issue-resolution-in-game-tests.md).

## Pass 1 complete (2026-08-15)

All 39 issue files have a `Proposed solution` section. Full text lives on
each file; this table is the review index.

**Coupled:** `save-sanitize-drops-unknown-ids` and
`save-party-reset-to-vanilla-trio` must ship together.

**Verify-only (no Pass 2 code):** Encyclopedia click, FXMega clips,
empty-filename log, alias UnitName Onagro fight, Island Atlas / File menu
after Close Lab.

**Lab hide/warn, no combat Harmony:** `RANGE_CONE`, never-fired AbilityEvents.

| Issue | Home | One-line |
|---|---|---|
| `portrait-patches-self-parent` | LokrCharacterLoader | Delete no-op `SetParent(self)` |
| `portrait-patches-hardcoded-hierarchy` | LokrCharacterLoader | Use `portraitData`; skip if null |
| `portrait-patches-buff-store-index` | LokrCharacterLoader | Bounds-check `GetAllHeroes()` |
| `find-part-index-unvalidated` | LokrCharacterLoader | Skip `parts[-1]` on miss |
| `exo-skeleton-null-unitdefinition` | LokrCharacterLoader | Null-guard `unitDefinition` / `metaExo` |
| `reload-data-missing-sprite-nre` | LokrCharacterLoader | Pre-filter JSON in `CustomRigLoader.Build` |
| `invisibility-exit-fires-every-turn` | LokrCharacterLoader | Raise exit only on on→off edge |
| `lab-static-panels-not-reset-on-close` | verify-only | Re-check File / Island Atlas after Close |
| `animator-pose-leaks-across-frames` | LokrLab | Cancel viewport drag after frame/clip switch |
| `sandbox-forfeit-confirm-behind-settings` | LokrLab | Mute `UIOptions` while confirm is up |
| `ability-kv-parse-empty-filename` | verify-only | Next named log, then fix or skip fragments |
| `ability-kv-pointmagnitude-constructs-pointmult` | LokrPatch | Map `pointMagnitude` → `FunctionPointMagnitude` |
| `ability-aoe-missing-center-keys-nre` | LokrPatch | Missing AOE keys default to `0` |
| `ability-ai-retreat-if-week-typo` | LokrPatch | Alias `RetreatIfWeakAI`; keep `RetreatIfWeekAI` |
| `ability-ai-per-affected-not-action` | LokrPatch + Lab | Skip in `ParseActionList`; Lab blocks save |
| `alias-unitname-parsed-as-function` | LokrCharacterLoader | Existing `#` prefix; confirm Onagro Stage |
| `ability-hit-closed-tag-whitelist` | LokrLab | Template `#PROJECTILE`; filter picker |
| `ability-callfunction-empty-filter-throws` | LokrPatch + Lab | Empty-filter skip on six `Execute`s |
| `ability-ai-empty-brain-divide-by-zero` | LokrPatch + Lab | `Eval` returns 0 on empty considerations |
| `ability-equal-null-lhs-nre` | LokrPatch | Null-safe `object.Equals` |
| `ability-each-in-list-actions-if-empty-inverted` | LokrPatch | `ActionsIfEmpty` only when `Count == 0` |
| `ability-tooltip-missing-var-returns-999` | LokrPatch | Missing key returns 0, not 999 |
| `ability-aoe-range-cone-empty` | Lab only | Hide/warn `RANGE_CONE`; no combat patch |
| `ability-events-never-dispatched` | Lab only | Hide/warn names with no fire site |
| `activity-interface-point-target-nre` | LokrPatch + Lab | Null-check `targetFilter` |
| `stats-apply-modifier-missing-stat-throws` | LokrPatch + Lab | Skip missing *stat keys* |
| `save-sanitize-drops-unknown-ids` | LokrPatch | Never `DiscardRun`; stow unknown ids |
| `save-party-reset-to-vanilla-trio` | LokrPatch | Keep known ids; drop `Count != 3` reset |
| `inventory-additem-never-sets-id` | LokrPatch | Guid when `ItemInstance.id` is null |
| `hero-update-skills-unknown-id-nre` | LokrPatch | Skip missing abilities |
| `hero-progress-window-unknown-uniqueid-nre` | LokrPatch | Drop unknown uniqueIds |
| `map-start-unknown-starting-hero-nre` | LokrPatch | Skip-and-log unknown `StartingHeroes` |
| `map-hud-unknown-modifier-config-nre` | LokrPatch | Skip null `GetConfigByKey` |
| `loot-anyof-chance-always-fires` | LokrPatch | Float `Random.Range` in `AddItems` |
| `dialog-first-no-fallback` | LokrPatch | `FirstOrDefault` then `ExitDialog` |
| `fight-started-empty-initiative-nre` | LokrPatch + Lab | Null-check `ActiveUnit`; spawn before `StartFight` |
| `skills-bar-five-slot-cap` | LokrCharacterLoader | Cap `match.skills` at five in campaign |
| `encyclopedia-button-unverified-click` | verify-only | Click Encyclopedia on main-menu hub |
| `fxmega-sounds-need-source-hero-group` | verify-only | Confirm Assassin sandbox clips |

## Batches (no overlapping files)

| Pass 1 agent | Issues |
|---|---|
| A portraits/exo | `portrait-patches-*`, `find-part-index-unvalidated`, `exo-skeleton-null-unitdefinition`, `reload-data-missing-sprite-nre`, `invisibility-exit-fires-every-turn` |
| B Lab UI | `lab-static-panels-not-reset-on-close`, `animator-pose-leaks-across-frames`, `sandbox-forfeit-confirm-behind-settings` |
| C Ability parse | `ability-kv-parse-empty-filename`, `ability-kv-pointmagnitude-constructs-pointmult`, `ability-aoe-missing-center-keys-nre`, `ability-ai-retreat-if-week-typo`, `ability-ai-per-affected-not-action`, `alias-unitname-parsed-as-function`, `ability-hit-closed-tag-whitelist` |
| D Ability runtime | `ability-callfunction-empty-filter-throws`, `ability-ai-empty-brain-divide-by-zero`, `ability-equal-null-lhs-nre`, `ability-each-in-list-actions-if-empty-inverted`, `ability-tooltip-missing-var-returns-999`, `ability-aoe-range-cone-empty`, `ability-events-never-dispatched`, `activity-interface-point-target-nre`, `stats-apply-modifier-missing-stat-throws` |
| E Save/metagame | `save-sanitize-drops-unknown-ids`, `save-party-reset-to-vanilla-trio`, `inventory-additem-never-sets-id`, `hero-update-skills-unknown-id-nre`, `hero-progress-window-unknown-uniqueid-nre`, `map-start-unknown-starting-hero-nre`, `map-hud-unknown-modifier-config-nre` |
| F Combat/map | `loot-anyof-chance-always-fires`, `dialog-first-no-fallback`, `fight-started-empty-initiative-nre`, `skills-bar-five-slot-cap` |
| G Remainder | `encyclopedia-button-unverified-click`, `fxmega-sounds-need-source-hero-group` |

## In-game confirm (2026-08-15)

Moved to `resolved/` after the test checklist notes: `encyclopedia-button-unverified-click` (Coming Soon popup), `fxmega-sounds-need-source-hero-group` (Assassin clips heard), `alias-unitname-parsed-as-function` (Onagro mine), `lab-static-panels-not-reset-on-close` (Slice Atlas after Close Lab), `ability-kv-parse-empty-filename` (Assassin `ERROR PARSING` gone), `fight-started-empty-initiative-nre` (Lab sandbox/Stage), `campaign-fight-loading-stuck` (fight overlay dismisses), `progression-help-popup-index-oor` (Next on help popup, LokrPatch 1.0.6).

`loot-anyof-chance-always-fires` and `dialog-first-no-fallback` were not fully tested.

