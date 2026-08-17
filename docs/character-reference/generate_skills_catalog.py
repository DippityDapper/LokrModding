#!/usr/bin/env python3
"""Generate the vanilla skills catalog HTML from extracted AbilityBehavior files.

Usage (from repo):
    python3 bepinex/docs/character-reference/generate_skills_catalog.py

Reads:
    _extracted/base-game/AbilitiesScript/
    _extracted/base-game/Localization/en_US.txt
    _extracted/base-game/resources/ (unit skill references)

Writes pages under docs/api/character-reference/ (index catalogs + skills/<id>.html).
"""

from __future__ import annotations

import os
import re
import sys
from collections import defaultdict

SCRIPT_DIR = os.path.dirname(os.path.abspath(__file__))
API_DIR = os.path.normpath(os.path.join(SCRIPT_DIR, "..", "api"))
CHAR_REF_DIR = os.path.join(API_DIR, "character-reference")
sys.path.insert(0, API_DIR)
sys.path.insert(0, CHAR_REF_DIR)

from markdown_to_html import convert_markdown_to_html  # noqa: E402
from sync_sidebar import build_sidebar, page_shell  # noqa: E402

ABILITY_ROOT = os.path.join(SCRIPT_DIR, "_extracted/base-game/AbilitiesScript")
LOC_PATH = os.path.join(SCRIPT_DIR, "_extracted/base-game/Localization/en_US.txt")
UNIT_ROOT = os.path.join(SCRIPT_DIR, "_extracted/base-game/resources")
SKILLS_DIR = os.path.join(CHAR_REF_DIR, "skills")

ENVELOPE_KEYS = (
    "AbilityBehavior",
    "AbilityTeamFilter",
    "AbilityCastRange",
    "AbilityCastMinRange",
    "AbilityCooldown",
    "AbilityPrewarmCooldown",
    "AbilityAPCost",
    "AbilityCanExecute",
    "AnimationID",
    "AnimationOverride",
    "CastFXId",
    "Icon",
    "LocalizationId",
    "HitChanceModifier",
    "AbilityAOEKind",
    "AbilityAOERange",
    "AbilityAOEMinRange",
    "AbilityAOEWidth",
    "AbilityAOETeamFilter",
    "AbilityAOECenterOnCaster",
    "AbilityAOEAffectsCaster",
    "AbilityAOECustomTargetFilter",
    "AbilityTargetFilterFlags",
    "AbilityCustomTargetFilter",
    "AbilityShowDetailFilter",
)

LOC_SUFFIXES = (
    "NAME",
    "DESCRIPTION",
    "DESCRIPTION_DATA",
    "DESCRIPTION_EPIC",
    "DESCRIPTION_EXTRA",
)

SKIP_TOP = {
    "01", "02", "03", "04", "05", "06", "07", "08", "09", "10", "100",
    "Actions", "Target", "AbilitySpecial", "OnAbilityAction", "Modifiers",
    "InitActions", "OnSpawn", "Params", "AIConfigB",
}


def read(path: str) -> str:
    try:
        with open(path, encoding="utf-8", errors="replace") as f:
            return f.read()
    except OSError:
        return ""


def collect_txt(root: str) -> list[str]:
    files = []
    if not os.path.isdir(root):
        return files
    for dirpath, _, filenames in os.walk(root):
        for fn in filenames:
            if fn.endswith(".txt"):
                files.append(os.path.join(dirpath, fn))
    return files


def split_top_level_blocks(text: str) -> list[tuple[str, str, str]]:
    """Return (id, inner_body, full_block) for each top-level quoted KV block."""
    blocks = []
    i = 0
    n = len(text)
    while i < n:
        while i < n and text[i] in " \t\r\n":
            i += 1
        if i >= n:
            break
        if text[i] != '"':
            nl = text.find("\n", i)
            i = n if nl < 0 else nl + 1
            continue
        j = text.find('"', i + 1)
        if j < 0:
            break
        key = text[i + 1 : j]
        k = j + 1
        while k < n and text[k] in " \t\r\n":
            k += 1
        if k >= n or text[k] != "{":
            i = k
            continue
        # Brace matching is quote-unaware: vanilla files contain typos such as
        # `"Target" %SOURCE"` that would desync a quote-tracking scanner.
        depth = 0
        p = k
        while p < n:
            if text[p] == "{":
                depth += 1
            elif text[p] == "}":
                depth -= 1
                if depth == 0:
                    inner = text[k + 1 : p]
                    full = text[i : p + 1]
                    blocks.append((key, inner, full))
                    i = p + 1
                    break
            p += 1
        else:
            last = text.rfind("}", k)
            if last > k:
                blocks.append((key, text[k + 1 : last], text[i : last + 1]))
                i = last + 1
            else:
                break
    return blocks


def first_level_children(body: str) -> list[tuple[str, str | None, str | None]]:
    """First-level keys: (key, scalar_value or None, child_body or None)."""
    items = []
    i = 0
    n = len(body)
    while i < n:
        while i < n and body[i] in " \t\r\n":
            i += 1
        if i >= n:
            break
        if body[i] != '"':
            nl = body.find("\n", i)
            i = n if nl < 0 else nl + 1
            continue
        j = body.find('"', i + 1)
        if j < 0:
            break
        key = body[i + 1 : j]
        k = j + 1
        while k < n and body[k] in " \t\r\n":
            k += 1
        if k >= n:
            break
        if body[k] == '"':
            end = body.find('"', k + 1)
            if end < 0:
                break
            items.append((key, body[k + 1 : end], None))
            i = end + 1
            continue
        if body[k] == "{":
            depth = 0
            p = k
            while p < n:
                if body[p] == "{":
                    depth += 1
                elif body[p] == "}":
                    depth -= 1
                    if depth == 0:
                        items.append((key, None, body[k + 1 : p]))
                        i = p + 1
                        break
                p += 1
            else:
                break
            continue
        i = k
    return items


def kv_all_scalars(text: str, key: str) -> list[str]:
    return re.findall(rf'"{re.escape(key)}"\s+"([^"]*)"', text)


def safe_filename(ability_id: str) -> str:
    cleaned = re.sub(r"[^A-Za-z0-9_.-]", "_", ability_id)
    return cleaned or "unknown"


def parse_loc(path: str) -> dict[str, str]:
    out = {}
    for match in re.finditer(r'"([^"]+)"\s*=\s*"(.*)"\s*$', read(path), re.M):
        out[match.group(1)] = match.group(2)
    return out


def loc_for(ability_id: str, loc_id: str, loc: dict[str, str]) -> dict[str, str]:
    found = {}
    for suffix in LOC_SUFFIXES:
        for stem in (loc_id, ability_id):
            if not stem:
                continue
            key = f"SKILL_{stem}_{suffix}"
            if key in loc:
                found[key] = loc[key]
                break
    return found


def modifier_loc(modifier_id: str, loc: dict[str, str]) -> dict[str, str]:
    found = {}
    prefix = f"COMBAT_MODIFIER_{modifier_id}_"
    for key, value in loc.items():
        if key.startswith(prefix):
            found[key] = value
    return found


def scan_unit_skill_refs() -> dict[str, list[str]]:
    """ability_id -> list of 'role: unitId' references."""
    refs = defaultdict(list)
    files = [
        f
        for f in collect_txt(UNIT_ROOT)
        if "RLHeroes" in f
        or "EnemiesDefinitions" in f
        or "UnitDefinitions" in f
        or "HeroDefinitions" in f
        or "SkillUnitDefinitions" in f
    ]
    for path in files:
        content = read(path)
        for unit_id, unit_body, _full in split_top_level_blocks(content):
            if unit_id in ("units", "Units"):
                for inner_id, inner_body, _ in split_top_level_blocks(unit_body):
                    add_unit_refs(refs, inner_id, inner_body)
            else:
                add_unit_refs(refs, unit_id, unit_body)
    for key, items in list(refs.items()):
        seen = set()
        unique = []
        for item in items:
            if item not in seen:
                seen.add(item)
                unique.append(item)
        refs[key] = unique
    return refs


def add_unit_refs(refs: dict[str, list[str]], unit_id: str, body: str) -> None:
    for skill in kv_all_scalars(body, "defaultSkill"):
        refs[skill].append(f"defaultSkill: {unit_id}")
    children = {k: (v, b) for k, v, b in first_level_children(body)}
    if "skills" in children and children["skills"][1] is not None:
        for slot, skill, _ in first_level_children(children["skills"][1]):
            if skill:
                refs[skill].append(f"skills[{slot}]: {unit_id}")
    if "skillProgression" in children and children["skillProgression"][1] is not None:
        for rank, _sv, rank_body in first_level_children(children["skillProgression"][1]):
            if not rank_body:
                continue
            for slot, skill, _ in first_level_children(rank_body):
                if skill:
                    refs[skill].append(f"skillProgression[{rank}][{slot}]: {unit_id}")


def outline_tree(body: str, indent: int = 0) -> list[str]:
    lines = []
    pad = "  " * indent
    for key, scalar, child in first_level_children(body):
        if scalar is not None:
            lines.append(f"{pad}- `{key}`: `{scalar}`")
        else:
            lines.append(f"{pad}- `{key}`")
            if child and indent < 8:
                lines.extend(outline_tree(child, indent + 1))
    return lines


def parse_abilities() -> list[dict]:
    abilities = []
    for path in collect_txt(ABILITY_ROOT):
        rel = os.path.relpath(path, ABILITY_ROOT)
        text = read(path)
        for ability_id, body, full in split_top_level_blocks(text):
            if ability_id in SKIP_TOP or ability_id.isdigit():
                continue
            if '"AbilityBehavior"' not in full and "AbilityBehavior" not in body:
                continue
            children = first_level_children(body)
            scalars = {k: v for k, v, b in children if v is not None}
            blocks = {k: b for k, v, b in children if b is not None}
            modifiers = []
            if "Modifiers" in blocks:
                for mid, _sv, mbody in first_level_children(blocks["Modifiers"]):
                    modifiers.append({"id": mid, "body": mbody or "", "passive": '"Passive"' in (mbody or "")})
            events = [k for k in blocks if k.startswith("On") or k.startswith("AI")]
            abilities.append(
                {
                    "id": ability_id,
                    "file": rel,
                    "body": body,
                    "full": full,
                    "scalars": scalars,
                    "blocks": blocks,
                    "modifiers": modifiers,
                    "events": events,
                    "behavior": scalars.get("AbilityBehavior", ""),
                }
            )
    abilities.sort(key=lambda a: a["id"].lower())
    return abilities


def md_cell(text: str) -> str:
    return str(text).replace("|", " · ").strip() or "(empty)"


def ability_href(ability_id: str) -> str:
    return f"skills/{safe_filename(ability_id)}.html"


def write_page(filename: str, title: str, breadcrumb: str, markdown: str, *, nested: bool = False) -> None:
    converted = convert_markdown_to_html(markdown)
    prefix = "../" if nested else ""
    html_page = page_shell(
        title_suffix=title,
        breadcrumb_label=breadcrumb,
        sidebar=build_sidebar("skills-catalog.html" if nested else filename, href_prefix=prefix),
        body_html=converted["body"],
        comment="Auto-generated by generate_skills_catalog.py — re-run that script after re-extracting abilities.",
        rel_prefix=prefix,
    )
    path = os.path.join(SKILLS_DIR, os.path.basename(filename)) if nested else os.path.join(CHAR_REF_DIR, filename)
    os.makedirs(os.path.dirname(path), exist_ok=True)
    with open(path, "w", encoding="utf-8") as f:
        f.write(html_page)


def collect_index_maps(abilities: list[dict]) -> dict[str, dict[str, list[str]]]:
    maps = {
        "fx": defaultdict(list),
        "animation": defaultdict(list),
        "icon": defaultdict(list),
        "sound": defaultdict(list),
        "callfunction": defaultdict(list),
        "expression": defaultdict(list),
        "spawn": defaultdict(list),
        "trigger": defaultdict(list),
    }
    for ability in abilities:
        aid = ability["id"]
        full = ability["full"]
        scalars = ability["scalars"]
        if scalars.get("CastFXId"):
            maps["fx"][scalars["CastFXId"]].append(f"{aid} (CastFXId)")
        for name in kv_all_scalars(full, "EffectName"):
            maps["fx"][name].append(f"{aid} (EffectName)")
        for name in kv_all_scalars(full, "ModifierFXName"):
            maps["fx"][name].append(f"{aid} (ModifierFXName)")
        for name in kv_all_scalars(full, "Model"):
            maps["fx"][name].append(f"{aid} (Model)")
        if scalars.get("AnimationID"):
            maps["animation"][scalars["AnimationID"]].append(f"{aid} (AnimationID)")
        for name in kv_all_scalars(full, "Animation"):
            maps["animation"][name].append(f"{aid} (PlayAnimation/Override)")
        if scalars.get("Icon"):
            maps["icon"][scalars["Icon"]].append(aid)
        for name in kv_all_scalars(full, "Sound"):
            maps["sound"][name].append(aid)
        for name in kv_all_scalars(full, "Function"):
            maps["callfunction"][name].append(aid)
        for key in (
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
        ):
            for name in kv_all_scalars(full, key):
                if "(" in name or name.startswith("%") or name.startswith("#"):
                    maps["expression"][name].append(f"{aid} ({key})")
        for name in kv_all_scalars(full, "UnitName"):
            maps["spawn"][name].append(aid)
        for name in kv_all_scalars(full, "Skill"):
            maps["trigger"][name].append(aid)
    return maps


def index_markdown(title: str, intro: str, mapping: dict[str, list[str]], abilities_by_id: dict[str, dict]) -> str:
    lines = [f"# {title}", "", intro, "", f"**{len(mapping)}** distinct names.", "", "| Name | Used by |", "|------|---------|"]
    for name in sorted(mapping, key=str.lower):
        users = []
        seen = set()
        for entry in mapping[name]:
            aid = entry.split(" ", 1)[0]
            label = f"[`{aid}`]({ability_href(aid)})" if aid in abilities_by_id else f"`{aid}`"
            extra = entry[len(aid) :].strip()
            cell = f"{label} {extra}" if extra else label
            if cell not in seen:
                seen.add(cell)
                users.append(cell)
        lines.append(f"| `{md_cell(name)}` | {', '.join(users)} |")
    lines.append("")
    return "\n".join(lines)


def ability_page_markdown(ability: dict, loc: dict[str, str], refs: dict[str, list[str]]) -> str:
    aid = ability["id"]
    scalars = ability["scalars"]
    loc_id = scalars.get("LocalizationId") or aid
    loc_pairs = loc_for(aid, loc_id, loc)
    display = loc_pairs.get(f"SKILL_{loc_id}_NAME") or loc_pairs.get(f"SKILL_{aid}_NAME") or aid
    behavior = scalars.get("AbilityBehavior", "")
    kind = "Passive" if "PASSIVE" in behavior else "Active"
    lines = [
        f"# `{aid}`",
        "",
        f"**{display}** — {kind}. Source file: `{ability['file']}`. Schema: [Abilities](../abilities.html).",
        "",
        "## Envelope",
        "",
        "| Field | Value |",
        "|-------|-------|",
    ]
    for key in ENVELOPE_KEYS:
        if key in scalars:
            lines.append(f"| `{key}` | `{md_cell(scalars[key])}` |")
    lines += ["", "## Localization", ""]
    if loc_pairs:
        lines += ["| Key | en_US |", "|-----|-------|"]
        for key, value in loc_pairs.items():
            lines.append(f"| `{key}` | {md_cell(value)} |")
    else:
        lines.append("No `SKILL_*` keys found in extracted `en_US.txt` for this id.")
    lines += ["", "## Referenced by units", ""]
    unit_refs = refs.get(aid, [])
    if unit_refs:
        for item in unit_refs:
            lines.append(f"- `{item}`")
    else:
        lines.append("Not referenced from extracted `defaultSkill` / `skills` / `skillProgression` (shared attack, cinematic, or enemy-only helper).")
    lines += ["", "## FX, animation, icon, sounds", ""]
    fx = []
    if scalars.get("CastFXId"):
        fx.append(f"CastFXId: `{scalars['CastFXId']}`")
    for name in sorted(set(kv_all_scalars(ability["full"], "EffectName"))):
        fx.append(f"EffectName: `{name}`")
    for name in sorted(set(kv_all_scalars(ability["full"], "ModifierFXName"))):
        fx.append(f"ModifierFXName: `{name}`")
    for name in sorted(set(kv_all_scalars(ability["full"], "Model"))):
        fx.append(f"Model: `{name}`")
    if scalars.get("AnimationID"):
        fx.append(f"AnimationID: `{scalars['AnimationID']}`")
    for name in sorted(set(kv_all_scalars(ability["full"], "Animation"))):
        fx.append(f"Animation: `{name}`")
    if scalars.get("Icon"):
        fx.append(f"Icon: `{scalars['Icon']}`")
    for name in sorted(set(kv_all_scalars(ability["full"], "Sound"))):
        fx.append(f"Sound: `{name}`")
    if fx:
        lines.extend(f"- {item}" for item in fx)
    else:
        lines.append("None named.")
    lines += ["", "## Nested modifiers", ""]
    if ability["modifiers"]:
        for modifier in ability["modifiers"]:
            mid = modifier["id"]
            flag = " (Passive auto-apply)" if modifier["passive"] else ""
            lines.append(f"### `{mid}`{flag}")
            mloc = modifier_loc(mid, loc)
            if mloc:
                lines += ["", "| Key | en_US |", "|-----|-------|"]
                for key, value in mloc.items():
                    lines.append(f"| `{key}` | {md_cell(value)} |")
            lines.append("")
    else:
        lines.append("No nested `Modifiers` block.")
    lines += ["", "## Event / action tree", ""]
    if ability["events"] or any(k.startswith("On") or k == "AbilitySpecial" or k.startswith("AI") for k in ability["blocks"]):
        for key in ("AbilitySpecial", *sorted(k for k in ability["blocks"] if k.startswith("On") or k.startswith("AI"))):
            if key not in ability["blocks"]:
                continue
            lines.append(f"### `{key}`")
            lines.append("")
            tree = outline_tree(ability["blocks"][key])
            lines.extend(tree or ["*(empty)*"])
            lines.append("")
    else:
        lines.append("No event, AbilitySpecial, or AI blocks.")
    lines += [
        "",
        "## Raw KV",
        "",
        "```kv",
        ability["full"].rstrip(),
        "```",
        "",
    ]
    return "\n".join(lines)


def main() -> None:
    if not os.path.isdir(ABILITY_ROOT):
        raise SystemExit(f"Missing {ABILITY_ROOT}. Extract vanilla TextAssets first.")

    abilities = parse_abilities()
    loc = parse_loc(LOC_PATH)
    refs = scan_unit_skill_refs()
    by_id = {a["id"]: a for a in abilities}
    maps = collect_index_maps(abilities)

    referenced = set(refs)
    defined = set(by_id)
    missing = sorted(referenced - defined)
    unused = sorted(defined - referenced)

    os.makedirs(SKILLS_DIR, exist_ok=True)
    for old in os.listdir(SKILLS_DIR):
        if old.endswith(".html"):
            os.remove(os.path.join(SKILLS_DIR, old))

    for ability in abilities:
        write_page(
            f"{safe_filename(ability['id'])}.html",
            ability["id"],
            ability["id"],
            ability_page_markdown(ability, loc, refs),
            nested=True,
        )

    rows = []
    for ability in abilities:
        aid = ability["id"]
        loc_id = ability["scalars"].get("LocalizationId") or aid
        name = loc.get(f"SKILL_{loc_id}_NAME") or loc.get(f"SKILL_{aid}_NAME") or ""
        kind = "passive" if "PASSIVE" in ability["behavior"] else "active"
        flags = [part.strip() for part in ability["behavior"].split("|") if part.strip()]
        behavior_cell = " ".join(f"`{md_cell(flag)}`" for flag in flags) or "(empty)"
        rows.append(
            f"| [`{aid}`]({ability_href(aid)}) | {md_cell(name)} | {behavior_cell} | {kind} | `{md_cell(ability['scalars'].get('Icon', ''))}` |"
        )

    missing_md = "\n".join(f"- `{mid}` — referenced by: {', '.join(f'`{r}`' for r in refs[mid])}" for mid in missing) or "None."
    catalog = "\n".join(
        [
            "# Skills catalog",
            "",
            "Auto-generated from vanilla `TextAsset`s that contain `AbilityBehavior`",
            f"({len(abilities)} abilities in {len(collect_txt(ABILITY_ROOT))} files).",
            "Schema reference: [Abilities](abilities.html). Re-run:",
            "",
            "```bash",
            "python3 bepinex/docs/character-reference/generate_skills_catalog.py",
            "```",
            "",
            "Related indexes: [Ability rules](ability-rules.html) · [VFX / cast animation](ability-vfx-animation.html) · [FX](skills-fx.html) · [Animations](skills-animations.html) · [Icons](skills-icons.html) · [Sounds](skills-sounds.html) · [CallFunction](skills-callfunctions.html) · [Expressions](skills-expressions.html) · [Spawns / chains](skills-spawns.html).",
            "",
            "## Coverage",
            "",
            f"- Defined abilities: **{len(abilities)}**",
            f"- Distinct ids referenced from unit `defaultSkill` / `skills` / `skillProgression`: **{len(referenced)}**",
            f"- Referenced but missing a definition page: **{len(missing)}**",
            f"- Defined but not referenced from those unit fields: **{len(unused)}** (shared attacks, cinematics, helpers)",
            "",
            "## Referenced ids with no extracted definition",
            "",
            missing_md,
            "",
            "## All abilities",
            "",
            "| Id | Name | AbilityBehavior | Kind | Icon |",
            "|----|------|-----------------|------|------|",
            *rows,
            "",
        ]
    )
    write_page("skills-catalog.html", "Skills catalog", "Skills catalog", catalog)

    write_page(
        "skills-fx.html",
        "Ability FX catalog",
        "FX catalog",
        index_markdown(
            "FX catalog",
            "Every `CastFXId`, `EffectName`, `ModifierFXName`, and projectile `Model` string in the vanilla ability dump. Pipeline rules: [VFX / cast animation](ability-vfx-animation.html).",
            maps["fx"],
            by_id,
        ),
    )
    write_page(
        "skills-animations.html",
        "Ability animation catalog",
        "Animation catalog",
        index_markdown(
            "Animation catalog",
            "Envelope `AnimationID` plus `PlayAnimation` / `OverrideAnimation` `Animation` values. The clip must exist on the caster's exo-skeleton with `AbilityAction` / `AbilityEnd` events.",
            maps["animation"],
            by_id,
        ),
    )
    write_page(
        "skills-icons.html",
        "Ability icon catalog",
        "Icon catalog",
        index_markdown(
            "Icon catalog",
            "Every `Icon` stem. Vanilla lookup is `AbilityIcons/<Icon>.png`. Traits with a null icon do not show in the hero room.",
            maps["icon"],
            by_id,
        ),
    )
    write_page(
        "skills-sounds.html",
        "Ability sound catalog",
        "Sound catalog",
        index_markdown(
            "Sound catalog",
            "`PlaySound` / `StopSound` `Sound` values. Separate from unit `soundConfig` `useSkill` and from audio inside FXMega prefabs.",
            maps["sound"],
            by_id,
        ),
    )
    write_page(
        "skills-callfunctions.html",
        "CallFunction catalog",
        "CallFunction catalog",
        index_markdown(
            "CallFunction catalog",
            "Every `Function` string the dump actually calls (usually a shipped C# type under `Ironhide.Legends.Content.Abilities`). Ability Lab's Call Function card lists these in a combobox.",
            maps["callfunction"],
            by_id,
        ),
    )
    write_page(
        "skills-expressions.html",
        "Expression catalog",
        "Expression catalog",
        index_markdown(
            "Expression catalog",
            "Function-style values the dump actually writes (`stat(...)`, `unitPosition(...)`, `%CASTER`, …). Parser function names live on [Ability rules](ability-rules.html). Ability Lab expression fields are a one-level function composer (function + arguments) when the value is a single call — you can still type a custom expression.",
            maps["expression"],
            by_id,
        ),
    )
    spawn_md = index_markdown(
        "Spawn / chain catalog",
        "`SpawnUnit` `UnitName` values and `TriggerSkill` `Skill` ids.",
        maps["spawn"],
        by_id,
    )
    spawn_md += "\n## TriggerSkill targets\n\n"
    spawn_md += index_markdown(
        "TriggerSkill `Skill` ids",
        "Abilities that fire another ability via `TriggerSkill`.",
        maps["trigger"],
        by_id,
    ).split("\n", 3)[-1]
    write_page("skills-spawns.html", "Spawn and chain catalog", "Spawn / chain catalog", spawn_md)

    print(f"Wrote {len(abilities)} ability pages + 8 index pages")
    print(f"  defined={len(defined)} referenced={len(referenced)} missing={len(missing)}")
    if missing:
        print("  missing ids: " + ", ".join(missing[:20]) + (" ..." if len(missing) > 20 else ""))


if __name__ == "__main__":
    main()
