#!/usr/bin/env python3
"""Regenerate appendices.html and _analysis.json from extracted base-game data.

Usage (from repo):
    python3 bepinex/docs/character-reference/generate_appendices.py

Enumerations use only files under _extracted/base-game/ (vanilla content). Mod-only
values from community packs are excluded — third-party plugins can extend Character
Lab comboboxes via CharacterLabOptionsAPI.

Also writes _analysis.json for generate_character_lab_known_options.py.
"""

import json
import os
import re
import sys
from collections import Counter

SCRIPT_DIR = os.path.dirname(os.path.abspath(__file__))
API_DIR = os.path.normpath(os.path.join(SCRIPT_DIR, "..", "api"))
CHAR_REF_DIR = os.path.join(API_DIR, "character-reference")
sys.path.insert(0, API_DIR)
sys.path.insert(0, CHAR_REF_DIR)

from markdown_to_html import convert_markdown_to_html  # noqa: E402
from sync_sidebar import build_sidebar, page_shell  # noqa: E402

BASE = os.path.join(SCRIPT_DIR, "_extracted/base-game/resources")
IH = os.path.expanduser("~/dev/lokr-modding/lokr-modding/ih-original/Ironhide.Legends")
OUT = os.path.join(CHAR_REF_DIR, "appendices.html")
ANALYSIS_OUT = os.path.join(SCRIPT_DIR, "_analysis.json")


def read(path):
    try:
        with open(path, encoding="utf-8", errors="replace") as f:
            return f.read()
    except OSError:
        return ""


def kv_scalar_values(content, key):
    return set(re.findall(rf'"{re.escape(key)}"\s+"([^"]*)"', content))


def collect_txt(*roots):
    files = []
    for root in roots:
        if not os.path.isdir(root):
            continue
        for dirpath, _, filenames in os.walk(root):
            for fn in filenames:
                if fn.endswith(".txt"):
                    files.append(os.path.join(dirpath, fn))
    return files


def enum_values(path, enum_name):
    text = read(path)
    match = re.search(rf"enum {enum_name}[^{{]*\{{([^}}]+)\}}", text, re.S)
    if not match:
        return []
    values = []
    for line in match.group(1).split("\n"):
        line = line.strip().rstrip(",")
        if not line or line.startswith("//"):
            continue
        values.append(line.split("=")[0].strip())
    return values


def counter_pairs(counter):
    return [[key, count] for key, count in counter.most_common()]


def scan_sound_events(content, sound_events):
    for match in re.finditer(r'"sounds"\s*\{([^}]*)\}', content, re.S):
        for sm in re.finditer(r'"([^"]+)"\s+"([^"]+)"', match.group(1)):
            sound_events[sm.group(1)] += 1


def scan_skill_ids(content, skill_ids):
    for match in re.finditer(r'"defaultSkill"\s*"([^"]+)"', content):
        skill_ids[match.group(1)] += 1
    for match in re.finditer(r'"skills"\s*\{([^}]*)\}', content, re.S):
        for sm in re.finditer(r'"([^"]+)"\s+"([^"]+)"', match.group(1)):
            skill_ids[sm.group(2)] += 1
    for match in re.finditer(
        r'"skillProgression"\s*\{([^}]*(?:\{[^}]*\}[^}]*)*)\}', content, re.S
    ):
        for sm in re.finditer(r'"([^"]+)"\s+"([^"]+)"', match.group(1)):
            if not sm.group(1).isdigit():
                continue
            skill_ids[sm.group(2)] += 1


def main():
    unit_files = [
        f
        for f in collect_txt(BASE)
        if "/RLHeroes/" in f
        or "/EnemiesDefinitions/" in f
        or "RLHeroes" in f
        or "EnemiesDefinitions" in f
        or "UnitDefinitions" in f
        or "HeroDefinitions" in f
        or "SkillUnitDefinitions" in f
    ]
    ability_root = os.path.join(SCRIPT_DIR, "_extracted/base-game/AbilitiesScript")
    ability_files = collect_txt(ability_root)
    if not ability_files:
        ability_files = [
            f
            for f in collect_txt(BASE)
            if "/NewAbilities/" in f
            or "/filter-Abilities/" in f
            or "/filter-Enemy/" in f
            or "/filter-Skill/" in f
        ]
    roster_files = [
        f
        for f in collect_txt(BASE)
        if "/HeroRoster/" in f or f.endswith("HeroRoster.txt")
    ]

    stat_names = Counter()
    state_names = Counter()
    cinematic_tags = Counter()
    model_values = Counter()
    attack_types = Counter()
    meta_exo = Counter()
    inherit_from = Counter()
    sound_events = Counter()
    sound_asset_ids = Counter()
    skill_ids = Counter()
    animation_ids = Counter()
    ability_behaviors = Counter()
    team_filters = Counter()
    aoe_kinds = Counter()
    var_types = Counter()
    ability_special_vars = Counter()
    modifier_props = Counter()
    action_types = Counter()
    ability_ids = Counter()
    achievement_ids = Counter()
    localization_prefixes = Counter()

    for path in unit_files:
        content = read(path)
        for match in re.finditer(r'"stats"\s*\{([^}]*(?:\{[^}]*\}[^}]*)*)\}', content, re.S):
            for sm in re.finditer(r'"([^"]+)"\s+"', match.group(1)):
                stat_names[sm.group(1)] += 1
        for match in re.finditer(r'"states"\s*\{([^}]*)\}', content, re.S):
            for sm in re.finditer(r'"([^"]+)"\s+"', match.group(1)):
                state_names[sm.group(1)] += 1
        for match in re.finditer(r'"cinematicTags"\s*"([^"]*)"', content):
            for tag in match.group(1).split("|"):
                cinematic_tags[tag.strip()] += 1
        for value in kv_scalar_values(content, "Model"):
            model_values[value] += 1
        for value in kv_scalar_values(content, "AttackType"):
            attack_types[value] += 1
        for value in kv_scalar_values(content, "MetaExo"):
            meta_exo[value] += 1
        for value in kv_scalar_values(content, "InheritsFrom"):
            inherit_from[value] += 1
        for match in re.finditer(r'"soundConfig"\s*\{([^}]*(?:\{[^}]*\}[^}]*)*)\}', content, re.S):
            for am in re.finditer(r'"assetId"\s*"([^"]+)"', match.group(1)):
                sound_asset_ids[am.group(1)] += 1
        scan_sound_events(content, sound_events)
        scan_skill_ids(content, skill_ids)

    for event in (
        "receiveDamage",
        "receiveArmorDamage",
        "receiveHeal",
        "death",
        "finalDeath",
        "useSkill",
        "startTurn",
        "walk",
        "promote",
        "victory",
        "selectHero",
    ):
        if event not in sound_events:
            sound_events[event] = 0

    skip_top = {
        "01", "02", "03", "04", "05", "06", "07", "08", "09", "10", "100",
        "Actions", "Target", "sounds", "Params", "Teams", "Center", "Radius",
        "IteratorName", "Considerations", "PropertiesAdd", "ModifierTag",
        "PropertiesRemove", "Duration", "Refresh", "Sound", "Tags", "Type",
        "Curve", "Weight", "MaxRange", "MinRange", "Source", "Condition",
        "Consideration", "CustomFilter", "LocalizationId", "Unit", "damage",
        "damageReduction", "damageRetributed", "HealAmount", "EffectName",
        "Damage", "TargetPos", "SourcePos", "Model", "Stat", "Value",
        "ModifierName",
    }

    for path in ability_files:
        content = read(path)
        for match in re.finditer(r'^"([^"]+)"\s*\{', content, re.M):
            name = match.group(1)
            if name not in skip_top and not name.isdigit():
                ability_ids[name] += 1
        for value in kv_scalar_values(content, "AbilityBehavior"):
            ability_behaviors[value] += 1
        for value in kv_scalar_values(content, "AbilityTeamFilter"):
            team_filters[value] += 1
        for value in kv_scalar_values(content, "AbilityAOETeamFilter"):
            team_filters[value] += 1
        for value in kv_scalar_values(content, "AbilityAOEKind"):
            aoe_kinds[value] += 1
        for value in kv_scalar_values(content, "var_type"):
            var_types[value] += 1
        for value in kv_scalar_values(content, "AnimationID"):
            animation_ids[value] += 1
        for match in re.finditer(r'\n\s*"([A-Z][A-Za-z0-9]+)"\s*\{', content):
            action_types[match.group(1)] += 1
        for match in re.finditer(r'"PropertiesAdd"\s*\{([^}]*)\}', content, re.S):
            for pm in re.finditer(r'"([^"]+)"\s+"', match.group(1)):
                modifier_props[pm.group(1)] += 1
        for match in re.finditer(r'"AbilitySpecial"\s*\{([^}]*(?:\{[^}]*\}[^}]*)*)\}', content, re.S):
            for blk in re.finditer(r"\{([^}]*)\}", match.group(1)):
                for vm in re.finditer(r'"([a-zA-Z][^"]*)"\s+"', blk.group(1)):
                    if vm.group(1) != "var_type":
                        ability_special_vars[vm.group(1)] += 1

    for path in roster_files:
        content = read(path)
        for match in re.finditer(r'"unlockAchievement"\s*:\s*"([^"]+)"', content):
            if match.group(1):
                achievement_ids[match.group(1)] += 1

    ap_path = os.path.join(IH, "Ironhide/Legends/Model/Game/Units/Abilities/AbilityParser.cs")
    ap = read(ap_path)
    parser_actions = sorted(
        set(
            re.findall(r'\{\s*"([A-Z][A-Za-z0-9]+)"\s*,\s*\w+Action', ap)
            + re.findall(r'\{\s*"([A-Z][A-Za-z0-9]+)"\s*,\s*\w+AI', ap)
            + re.findall(r'\{\s*"([A-Z][A-Za-z0-9]+)"\s*,\s*\w+Evaluator', ap)
            + re.findall(r'\{\s*"([A-Z][A-Za-z0-9]+)"\s*,\s*\w+Helper', ap)
            + re.findall(r'\{\s*"([A-Z][A-Za-z0-9]+)"\s*,\s*CallFunction', ap)
            + re.findall(r'\{\s*"([A-Z][A-Za-z0-9]+)"\s*,\s*ModifyHexPassable', ap)
            + re.findall(r'\{\s*"([A-Z][A-Za-z0-9]+)"\s*,\s*GolemizeAction', ap)
            + re.findall(r'\{\s*"([A-Z][A-Za-z0-9]+)"\s*,\s*KeepProjectileGoing', ap)
            + re.findall(r'\{\s*"([A-Z][A-Za-z0-9]+)"\s*,\s*LuaAction', ap)
            + re.findall(r'\{\s*"([A-Z][A-Za-z0-9]+)"\s*,\s*PauseUnityAction', ap)
        )
    )
    expr_funcs = sorted(set(re.findall(r'\{\s*"([a-z][a-zA-Z0-9_]*)"\s*,\s*typeof\(Function', ap)))
    ai_considerations = sorted(set(re.findall(r'"([A-Z][A-Za-z0-9]+)"\s*,\s*typeof\(Consideration', ap)))

    ability_events = re.findall(
        r'public const string \w+ = "([^"]+)"',
        read(os.path.join(IH, "Ironhide/Legends/Model/Game/Units/Abilities/AbilityEvents.cs")),
    )
    modifier_events = re.findall(
        r'public const string \w+ = "([^"]+)"',
        read(os.path.join(IH, "Ironhide/Legends/Model/Game/Units/Abilities/ModifierEvents.cs")),
    )
    unit_cs = read(os.path.join(IH, "Ironhide/Legends/Model/Game/Units/Unit.cs"))
    engine_states = sorted(set(m[1] for m in re.findall(r'public const string STATE_(\w+) = "(\w+)"', unit_cs)))

    def md_table_cell(text):
        # Pipe characters break GFM table parsing.
        s = str(text).replace("|", " · ").strip()
        return s if s else "(empty)"

    def table_rows(counter):
        return "\n".join(
            f"| `{md_table_cell(k)}` | {v} |" for k, v in counter.most_common()
        )

    def bullets(items):
        return "\n".join(f"- `{x}`" for x in items)

    lines = [
        "# Appendices — full enumerations",
        "",
        "Auto-generated by `generate_appendices.py` from extracted **base-game**",
        "files and decompiled engine enums (no mod-pack content). Re-run after data changes:",
        "",
        "```bash",
        "python3 bepinex/docs/character-reference/generate_appendices.py",
        "```",
        "",
        "---",
        "",
        f"## A. Stat field names ({len(stat_names)} total)",
        "",
        "| Stat | Occurrences |",
        "|------|-------------:|",
        table_rows(stat_names),
        "",
        f"## B. Unit state flags ({len(state_names)} in data)",
        "",
        "### B.1 Observed in KV data",
        "",
        "| State | Occurrences |",
        "|-------|-------------:|",
        table_rows(state_names),
        "",
        "### B.2 Engine constants (`Unit.cs`)",
        "",
        bullets(engine_states),
        "",
        f"## C. Cinematic tags ({len(cinematic_tags)} total)",
        "",
        "| Tag | Occurrences |",
        "|-----|-------------:|",
        table_rows(cinematic_tags),
        "",
        f"## D. Model values ({len(model_values)} total)",
        "",
        "| Model | Occurrences |",
        "|-------|-------------:|",
        table_rows(model_values),
        "",
        "## E. Attack types",
        "",
        "| AttackType | Occurrences |",
        "|------------|-------------:|",
        table_rows(attack_types),
        "",
        "## F. InheritsFrom parents",
        "",
        "| Parent | Occurrences |",
        "|--------|-------------:|",
        table_rows(inherit_from),
        "",
        f"## G. MetaExo values ({len(meta_exo)} total)",
        "",
        "| MetaExo | Occurrences |",
        "|---------|-------------:|",
        table_rows(meta_exo),
        "",
        "## H. Sound config events",
        "",
        "| Event | Occurrences |",
        "|-------|-------------:|",
        table_rows(sound_events),
        "",
        "## I. Sound assetIds",
        "",
        "| assetId | Occurrences |",
        "|---------|-------------:|",
        table_rows(sound_asset_ids),
        "",
        "## J. AbilityBehavior (observed combinations)",
        "",
        "| AbilityBehavior | Count |",
        "|-----------------|------:|",
        table_rows(ability_behaviors),
        "",
        "### J.1 Engine enum flags",
        "",
        bullets(
            enum_values(
                os.path.join(IH, "Ironhide/Legends/Model/Game/Units/Abilities/AbilityBehavior.cs"),
                "AbilityBehavior",
            )
        ),
        "",
        "## K. TeamFilter",
        "",
        "| TeamFilter | Count |",
        "|------------|------:|",
        table_rows(team_filters),
        "",
        "Engine enum: "
        + ", ".join(
            f"`{x}`"
            for x in enum_values(
                os.path.join(IH, "Ironhide/Legends/Model/Game/Units/Abilities/TeamFilter.cs"),
                "TeamFilter",
            )
        ),
        "",
        "## L. AOEKind",
        "",
        "| AOEKind | Count |",
        "|---------|------:|",
        table_rows(aoe_kinds),
        "",
        "Engine enum: "
        + ", ".join(
            f"`{x}`"
            for x in enum_values(
                os.path.join(IH, "Ironhide/Legends/Model/Game/Units/Abilities/AOEKind.cs"),
                "AOEKind",
            )
        ),
        "",
        "## M. AbilitySpecial var_type",
        "",
        "| var_type | Count |",
        "|----------|------:|",
        table_rows(var_types),
        "",
        f"## N. AbilitySpecial variables ({len(ability_special_vars)} total)",
        "",
        "| Variable | Count |",
        "|----------|------:|",
        table_rows(ability_special_vars),
        "",
        "## O. Modifier PropertiesAdd keys",
        "",
        "| Property | Count |",
        "|----------|------:|",
        table_rows(modifier_props),
        "",
        "## P. AnimationID values",
        "",
        "| AnimationID | Count |",
        "|-------------|------:|",
        table_rows(animation_ids),
        "",
        f"## Q. Ability action types ({len(parser_actions)} engine-registered)",
        "",
        bullets(parser_actions),
        "",
        f"### Q.1 Observed action block names ({len(action_types)} total)",
        "",
        "| Action block | Count |",
        "|--------------|------:|",
        table_rows(action_types),
        "",
        "## R. Ability event handlers",
        "",
        bullets(ability_events),
        "",
        "## S. Modifier event handlers",
        "",
        bullets(modifier_events),
        "",
        f"## T. Expression functions ({len(expr_funcs)} total)",
        "",
        bullets(expr_funcs),
        "",
        f"## U. AI consideration types ({len(ai_considerations)} total)",
        "",
        bullets(ai_considerations),
        "",
        "## V. unlockAchievement ids",
        "",
        "| Achievement id | Count |",
        "|----------------|------:|",
        table_rows(achievement_ids),
        "",
        "## W. Localization key prefixes",
        "",
        "| Prefix | Keys |",
        "|--------|-----:|",
        table_rows(localization_prefixes),
        "",
        f"## X. Ability ids ({len(ability_ids)} total, alphabetical)",
        "",
    ]
    lines.extend(f"- `{aid}`" for aid in sorted(ability_ids.keys()))
    lines.append("")

    markdown = "\n".join(lines)
    converted = convert_markdown_to_html(markdown)
    body_html = converted["body"]
    html = page_shell(
        title_suffix="Appendices",
        breadcrumb_label="Appendices",
        sidebar=build_sidebar("appendices.html"),
        body_html=body_html,
        comment="Auto-generated by generate_appendices.py — re-run that script after data changes.",
    )
    with open(OUT, "w", encoding="utf-8") as f:
        f.write(html)

    analysis = {
        "stat_names": counter_pairs(stat_names),
        "state_names": counter_pairs(state_names),
        "cinematic_tags": counter_pairs(cinematic_tags),
        "model_values": counter_pairs(model_values),
        "attack_types": counter_pairs(attack_types),
        "meta_exo_sample": counter_pairs(meta_exo),
        "sound_events": counter_pairs(sound_events),
        "sound_asset_ids": counter_pairs(sound_asset_ids),
        "skill_ids": counter_pairs(skill_ids),
        "ability_ids_sample": counter_pairs(ability_ids),
        "base_game_counts": {
            "unit_files": len(unit_files),
            "ability_files": len(ability_files),
            "roster_files": len(roster_files),
        },
    }
    with open(ANALYSIS_OUT, "w", encoding="utf-8") as f:
        json.dump(analysis, f, indent=2)
        f.write("\n")

    print(f"Wrote {OUT}")
    print(f"Wrote {ANALYSIS_OUT}")
    print(
        f"  stats={len(stat_names)} states={len(state_names)} tags={len(cinematic_tags)} "
        f"skills={len(skill_ids)} sound_asset_ids={len(sound_asset_ids)} "
        f"actions={len(parser_actions)} abilities={len(ability_ids)}"
    )


if __name__ == "__main__":
    main()
