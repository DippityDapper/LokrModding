#!/usr/bin/env python3
"""Write LokrAbilityLab picker name lists from the Phase 1/2 extracts.

Usage:
    python3 bepinex/docs/character-reference/generate_ability_picker_catalog.py
"""

from __future__ import annotations

import os
import re
import sys

SCRIPT_DIR = os.path.dirname(os.path.abspath(__file__))
sys.path.insert(0, SCRIPT_DIR)
from generate_skills_catalog import parse_abilities, kv_all_scalars  # noqa: E402

FXMEGA_LIST = os.path.join(SCRIPT_DIR, "_extracted/base-game/FXMega/FXMegaList.txt")
OUT = os.path.normpath(
    os.path.join(SCRIPT_DIR, "../../LokrAbilityLab/Editor/AbilityPickerCatalog.generated.cs")
)

# AbilityParser.expressionFunctions MergeWith + BaseLogicParser.expressionFunctions.
PARSER_FUNCTIONS = [
    "abilityCinematicContext",
    "abilityCooldown",
    "activeUnit",
    "ceil",
    "cinematicPosition",
    "color",
    "countFreeHexes",
    "currentContext",
    "customUnitFilter",
    "debugExpression",
    "encounterContext",
    "equal",
    "expr",
    "floor",
    "getUnitCinematicId",
    "getUnitId",
    "hasModifier",
    "hasModifierByTag",
    "hasTags",
    "hexDistance",
    "hexInLine",
    "hexNeighbour",
    "hexNeighbourOrNextFree",
    "hexPosition",
    "hitBrokenArmor",
    "hitChanceCalc",
    "hitConnected",
    "hitContext",
    "hitDamageOfType",
    "hitEffectiveDamage",
    "hitIsLegendary",
    "hitTags",
    "isAI",
    "isDiversifierActive",
    "isGridClear",
    "isNull",
    "isOnState",
    "lerp",
    "listCount",
    "matchesCinematicId",
    "matchesGroup",
    "matchesTeam",
    "max",
    "min",
    "modifierContext",
    "modifierTurnsRemaining",
    "newPoint",
    "not",
    "numberOfUnitsAffected",
    "objectList",
    "playingTutorial",
    "pointAdd",
    "pointMagnitude",
    "pointMult",
    "pointMultElements",
    "pointNormalize",
    "pointSub",
    "pointsInCircunferenceP",
    "positionHex",
    "power",
    "projectileContext",
    "projectilePosition",
    "randomBetween",
    "randomI",
    "roomContext",
    "round",
    "safeEquals",
    "stat",
    "stringConcat",
    "stringList",
    "stringWithIndex",
    "unitByCinematicId",
    "unitConfigString",
    "unitContext",
    "unitFacing",
    "unitGroup",
    "unitHex",
    "unitHexSide",
    "unitIsFlipped",
    "unitPosition",
    "wrapContext",
]

FUNCTION_TEMPLATES = [
    "stat(%CASTER, #baseDamage)",
    "stat(%CASTER, #attackDamage)",
    "stat(%CASTER, #rangedAttackRange)",
    "stat(%CASTER, #rangedAttackMinRange)",
    "stat(%CASTER, #meleeRange)",
    "stat(%CASTER, #attackCostAP)",
    "stat(%TARGET, #health)",
    "stat(%TARGET, #health_max)",
    "stat(%SOURCE, #actionsAvailable)",
    "unitPosition(%TARGET)",
    "unitPosition(%CASTER)",
    "unitHex(%TARGET)",
    "unitHex(%CASTER)",
    "hexPosition(%TARGET)",
    "hexDistance(%CASTER, %TARGET)",
    "unitGroup(%CASTER)",
    "isOnState(%TARGET, #STUN)",
    "hasModifier(%TARGET, #modifierName)",
    "not(isOnState(%TARGET, #BOSS))",
    "expr(%varName)",
    "ceil(stat(%CASTER, #attackDamage)/2)",
    "floor(stat(%CASTER, #attackDamage)/2)",
    "randomBetween(0.3, 1.2)",
    "stringList(#MELEE, #TARGETED)",
    "stringList(#PROJECTILE, #TARGETED)",
    "hitChanceCalc()",
    "activeUnit()",
]

CONTEXT_TOKENS = [
    "%CASTER",
    "%SOURCE",
    "%TARGET",
    "%UNIT",
    "%ATTACKER",
    "%ATTACKED",
    "%HITSOURCE",
    "%newTarget",
]

DAMAGE_TYPES = [
    "DAMAGE_PHYSICAL",
    "DAMAGE_MAGICAL",
    "DAMAGE_TRUE",
    "DAMAGE_ARMOR",
]

EXPR_KEYS = (
    "AbilityCastRange",
    "AbilityCastMinRange",
    "AbilityCooldown",
    "AbilityPrewarmCooldown",
    "AbilityAPCost",
    "AbilityCanExecute",
    "HitChanceModifier",
    "AbilityAOERange",
    "AbilityAOEMinRange",
    "AbilityAOEWidth",
    "Damage",
    "HealAmount",
    "Condition",
    "Time",
    "Duration",
    "Value",
    "Position",
    "Strength",
    "ArmorAmount",
    "Offset",
    "Times",
    "Refresh",
    "Enqueue",
    "Backstab",
)

UNIT_REF_KEYS = ("Target", "Unit", "Source", "Center", "SourcePos", "TargetPos")

RANGE_KEYS = (
    "AbilityCastRange",
    "AbilityCastMinRange",
    "AbilityAOERange",
    "AbilityAOEMinRange",
    "AbilityAOEWidth",
)
NUMBER_KEYS = (
    "AbilityCooldown",
    "AbilityPrewarmCooldown",
    "AbilityAPCost",
    "HitChanceModifier",
    "Damage",
    "HealAmount",
    "Duration",
    "Time",
    "Strength",
    "ArmorAmount",
    "Offset",
    "Times",
    "Refresh",
    "Enqueue",
    "Backstab",
)
POSITION_KEYS = ("SourcePos", "TargetPos", "Position")
UNIT_KEYS = ("Target", "Unit", "Source", "Center")
CONDITION_KEYS = ("AbilityCanExecute", "Condition")
TAG_KEYS = ("Tags",)
GROUP_KEYS = ("UnitGroup",)
GENERAL_KEYS = ("Value",)

COMMON_NUMBERS = ("0", "1", "2", "3", "4", "5", "0.5", "999", "1000")
RANGE_STATS_SEED = (
    "#rangedAttackRange",
    "#meleeRange",
    "#rangedAttackMinRange",
    "#walkSpeedAvailable",
)
NUMBER_STATS_SEED = (
    "#baseDamage",
    "#attackDamage",
    "#attackCostAP",
    "#health",
    "#health_max",
)

ATTACH_SEED = ("#Chest", "#CastPoint", "#RayPoint", "#Base")
UNITPOS_ATTACH = re.compile(
    r"unitPosition\s*\(\s*[^,()]+\s*,\s*(#[A-Za-z_][A-Za-z0-9_]*)\s*\)"
)
STAT_CALL = re.compile(r"stat\s*\(\s*[^,]+,\s*(#[A-Za-z_][A-Za-z0-9_]*)")
STATE_CALL = re.compile(r"isOnState\s*\(\s*[^,]+,\s*(#[A-Za-z_][A-Za-z0-9_]*)")
STRINGLIST_ARG = re.compile(r"stringList\s*\(([^)]*)\)")
HASH_REF = re.compile(r"#[A-Za-z_][A-Za-z0-9_]*")


def unique_sorted(values) -> list[str]:
    return sorted({v for v in values if v and str(v).strip()}, key=str.lower)


def csharp_escape(value: str) -> str:
    return (
        value.replace("\\", "\\\\")
        .replace("\"", "\\\"")
        .replace("\r", "\\r")
        .replace("\n", "\\n")
        .replace("\t", "\\t")
    )


def csharp_array(name: str, values: list[str]) -> str:
    lines = [f"\t\tinternal static readonly string[] {name} ="]
    lines.append("\t\t{")
    for value in values:
        lines.append(f'\t\t\t"{csharp_escape(value)}",')
    lines.append("\t\t};")
    return "\n".join(lines)


def looks_like_expression(value: str) -> bool:
    if not value or len(value) > 160:
        return False
    if "\n" in value or "\r" in value:
        return False
    if value.startswith("%") or value.startswith("#"):
        return True
    if "(" in value and ")" in value:
        return True
    if value.replace(".", "", 1).isdigit():
        return True
    return False


def collect_field_values(full: str, keys: tuple[str, ...], dest: set[str]) -> None:
    for key in keys:
        for value in kv_all_scalars(full, key):
            if looks_like_expression(value):
                dest.add(value)


def main() -> int:
    abilities = parse_abilities()
    fx = set()
    if os.path.isfile(FXMEGA_LIST):
        with open(FXMEGA_LIST, encoding="utf-8") as handle:
            for line in handle:
                name = line.strip()
                if name:
                    fx.add(name)
    projectiles = set()
    animations = set()
    icons = set()
    sounds = set()
    call_fns = set()
    units = set()
    snippets = set()
    tokens = set(CONTEXT_TOKENS)
    stats = set()
    unit_refs = set(CONTEXT_TOKENS)
    attach = set(ATTACH_SEED)
    range_snips = set(COMMON_NUMBERS)
    number_snips = set(COMMON_NUMBERS)
    position_snips = set(CONTEXT_TOKENS)
    unit_snips = set(CONTEXT_TOKENS)
    condition_snips = set()
    tag_snips = set()
    group_snips = set()
    range_stats = set(RANGE_STATS_SEED)
    number_stats = set(NUMBER_STATS_SEED)
    condition_stats = set()
    states = {"#STUN", "#DEAD", "#BOSS"}
    hit_tags = set()
    for ability in abilities:
        scalars = ability.get("scalars") or {}
        for key in ("CastFXId",):
            if scalars.get(key):
                fx.add(scalars[key])
        if scalars.get("AnimationID"):
            animations.add(scalars["AnimationID"])
        if scalars.get("Icon"):
            icons.add(scalars["Icon"])
        full = ability["full"]
        for key in ("EffectName", "ModifierFXName"):
            fx.update(kv_all_scalars(full, key))
        projectiles.update(kv_all_scalars(full, "Model"))
        animations.update(kv_all_scalars(full, "AnimationID"))
        animations.update(kv_all_scalars(full, "Animation"))
        sounds.update(kv_all_scalars(full, "Sound"))
        call_fns.update(kv_all_scalars(full, "Function"))
        units.update(kv_all_scalars(full, "UnitName"))
        units.update(kv_all_scalars(full, "Unit"))
        for key in EXPR_KEYS:
            for value in kv_all_scalars(full, key):
                if looks_like_expression(value):
                    snippets.add(value)
                for token in re.findall(r"%[A-Za-z_][A-Za-z0-9_]*", value):
                    tokens.add(token)
        collect_field_values(full, RANGE_KEYS, range_snips)
        collect_field_values(full, NUMBER_KEYS, number_snips)
        collect_field_values(full, POSITION_KEYS, position_snips)
        collect_field_values(full, UNIT_KEYS, unit_snips)
        collect_field_values(full, CONDITION_KEYS, condition_snips)
        collect_field_values(full, TAG_KEYS, tag_snips)
        collect_field_values(full, GROUP_KEYS, group_snips)
        for key in UNIT_REF_KEYS:
            for value in kv_all_scalars(full, key):
                if "\n" in value or "\r" in value:
                    continue
                unit_refs.add(value)
                if looks_like_expression(value):
                    snippets.add(value)
        for value in kv_all_scalars(full, "Stat"):
            if value.startswith("#"):
                stats.add(value)
            else:
                stats.add("#" + value if value else value)
        attach.update(UNITPOS_ATTACH.findall(full))
        range_stats.update(STAT_CALL.findall(" ".join(v for k in RANGE_KEYS for v in kv_all_scalars(full, k))))
        number_stats.update(STAT_CALL.findall(" ".join(v for k in NUMBER_KEYS for v in kv_all_scalars(full, k))))
        condition_stats.update(STAT_CALL.findall(" ".join(v for k in CONDITION_KEYS for v in kv_all_scalars(full, k))))
        states.update(STATE_CALL.findall(full))
        for args in STRINGLIST_ARG.findall(full):
            hit_tags.update(HASH_REF.findall(args))

    animations.add("NOANIMATION")
    snippets.update(FUNCTION_TEMPLATES)
    for template in FUNCTION_TEMPLATES:
        for stat in STAT_CALL.findall(template):
            stats.add(stat)
            number_stats.add(stat)
    parts = [
        "// <auto-generated> generate_ability_picker_catalog.py — do not edit by hand.",
        "namespace LokrAbilityLab.Editor",
        "{",
        "\t/// <summary>Vanilla name lists for envelope and action-card comboboxes.</summary>",
        "\tinternal static class AbilityPickerCatalog",
        "\t{",
        csharp_array("FxMegaNames", unique_sorted(fx)),
        csharp_array("ProjectileModels", unique_sorted(projectiles)),
        csharp_array("AnimationIds", unique_sorted(animations)),
        csharp_array("IconStems", unique_sorted(icons)),
        csharp_array("SoundNames", unique_sorted(sounds)),
        csharp_array("CallFunctions", unique_sorted(call_fns)),
        csharp_array("SpawnUnitIds", unique_sorted(units)),
        csharp_array("ExpressionFunctions", PARSER_FUNCTIONS),
        csharp_array("ContextTokens", CONTEXT_TOKENS),
        csharp_array("StatRefs", unique_sorted(s for s in stats if s and s != "#")),
        csharp_array("ExpressionSnippets", unique_sorted(snippets)),
        csharp_array("RangeSnippets", unique_sorted(range_snips)),
        csharp_array("NumberSnippets", unique_sorted(number_snips)),
        csharp_array("PositionSnippets", unique_sorted(position_snips)),
        csharp_array("UnitSnippets", unique_sorted(unit_snips)),
        csharp_array("ConditionSnippets", unique_sorted(condition_snips)),
        csharp_array("TagSnippets", unique_sorted(tag_snips)),
        csharp_array("GroupSnippets", unique_sorted(group_snips)),
        csharp_array("RangeStats", unique_sorted(range_stats)),
        csharp_array("NumberStats", unique_sorted(number_stats)),
        csharp_array("ConditionStats", unique_sorted(condition_stats)),
        csharp_array("StateRefs", unique_sorted(states)),
        csharp_array("HitTags", unique_sorted(hit_tags)),
        csharp_array("UnitRefs", unique_sorted(unit_refs)),
        csharp_array("AttachPoints", unique_sorted(attach)),
        csharp_array("DamageTypes", DAMAGE_TYPES),
        "\t}",
        "}",
        "",
    ]
    os.makedirs(os.path.dirname(OUT), exist_ok=True)
    with open(OUT, "w", encoding="utf-8") as handle:
        handle.write("\n".join(parts))
    print("Wrote", OUT)
    print(
        f"  functions={len(PARSER_FUNCTIONS)} tokens={len(tokens)} "
        f"stats={len(stats)} snippets={len(snippets)} unitRefs={len(unit_refs)} "
        f"attach={len(attach)} range={len(range_snips)} number={len(number_snips)} "
        f"pos={len(position_snips)} cond={len(condition_snips)}"
    )
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
