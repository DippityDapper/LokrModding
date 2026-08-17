# SkillsBar hard-caps five interactive skills and throws on a sixth

Area: LokrLab (sandbox / Ability Lab Stage) and LokrCharacterLoader custom heroes
Status: unresolved-tested

As of 2026-08-15: `SkillsBar.AddSkillsBar` always Instantiates five
`scenario/Skill` slots (`while i < 5`), then records every
`IsInteractive` skill on the unit into `MatchSkillsBarUnit.skills`.
`SetSelectedUnit` (the private `UnitViewComponent` overload),
`GetSelectedSkillIcon`, and `NotDefaultSkillSelected` then index
`skillsList[i]` for `i < match.skills.Count`. A sixth interactive skill
throws `ArgumentOutOfRangeException`.

Character Lab already papers this over in two places, both incomplete
for campaign play:

1. `SandboxRoster.GrantProgressionSkills` grants at most five
   interactive skills (vanilla level-up picks one per rank). A hero
   whose base `skills` list already has six is not trimmed.
2. `SkillsBarSlotCap` in
   `LokrLab/Character/Patches/SkillsBarTurnMarkerPatch.cs` drops extras,
   but only when `EmbeddedFightHost.IsActive`. A campaign fight with a
   custom hero that has more than five interactive skills still throws.

Suggested fix: Harmony-prefix `AddSkillsBar` / `SetSelectedUnit` in
LokrCharacterLoader (not only Lab) to cap `match.skills` at
`skillsList.Count`, or build extra hex slots. Do not start that patch
from the HTML-docs track.

See
[`SkillsBar.html`](../../api/base-game/SkillsBar.html)
and
[`MatchSkillsBarUnit.html`](../../api/base-game/MatchSkillsBarUnit.html).

## Proposed solution (Pass 1, 2026-08-15)

**Home:** LokrCharacterLoader
**Approach:** Campaign-wide Harmony cap in LokrCharacterLoader that trims `MatchSkillsBarUnit.skills` to `skillsList.Count` (always 5 hexes, `SkillsButtons` length 5). Not LokrPatch: vanilla `Hero.UpdateSkills` grants one non-passive pick per rank (decompile ranks 1–3 plus base skills) and the HUD is built for five slots, so a sixth interactive skill is a custom-hero / Lab case, not a vanilla throw. Lab's `SkillsBarSlotCap` already trims but only when `EmbeddedFightHost.IsActive`, so a campaign fight with a custom hero still indexes `skillsList[i]` past 4 in `SetSelectedUnit(UnitViewComponent)`, `GetSelectedSkillIcon`, `SetSelectedSkill`, and `NotDefaultSkillSelected`.
**Exact change:** New `LokrCharacterLoader/Patches/SkillsBarSlotCapPatches.cs` (same trim helper shape as Lab's `SkillsBarSlotCap.Trim`). Postfix `SkillsBar.AddSkillsBar(UnitViewComponent)` — after vanilla copies every `IsInteractive` skill, `RemoveRange` extras beyond `skillsList.Count`; log a warning once with unit id and dropped count. Prefix the private `SetSelectedUnit(UnitViewComponent)`, `SetSelectedSkill(UnitViewComponent, Activity, bool)`, `GetSelectedSkillIcon(UnitViewComponent)`, and `NotDefaultSkillSelected(Unit)` — trim then return `true` so later `AddSkill` during the fight cannot grow past the hex list. No `EmbeddedFightHost.IsActive` gate. Leave Lab `GrantProgressionSkills` (still grants at most five interactive) and the existing Lab trim patches in place this pass (idempotent; Lab already `[BepInDependency]` on CharacterLoader). Do not instantiate extra `scenario/Skill` slots.
**Do not:** Build a sixth hex / extend `SkillsButtons`. Put this only in LokrLab. Put it in LokrPatch (vanilla does not throw). Delete Lab `SkillsBarTurnMarkerPatch` (that skip is a different missing-key bug). Change `Arrange`. Rewrite `AddSkillsBar`'s instantiate loop.
**In-game verify:** 1. Build and launch via Steam / Proton. 2. Campaign fight with a custom hero whose base `skills` list has six or more interactive abilities — skills bar shows five, no `ArgumentOutOfRangeException`, extras omitted (warning in `LogOutput.log`). 3. Same hero in Character Lab Sandbox (base list of six, not only progression) — bar still five, fight playable. 4. Vanilla hero at max level: all five slots behave as before (default + progression). 5. Click / hotkeys Skill1–Skill5 still match visible icons.
**Risk:** Extra interactive skills on custom heroes become unreachable in combat (authors must pick which five live in the first five `IsInteractive` dictionary entries). Vanilla HUD and saves unchanged. Dictionary iteration order of `unit.skills` decides which extras drop — same as today's Lab trim.

## Unit tests (unresolved-tested)

Moved: 2026-08-15
Tests:
- LokrModding.Tests.CharacterLoader.ContentRulesTests.TrimListToCap_DropsExtrasPastFive
- LokrModding.Tests.CharacterLoader.ContentRulesTests.TrimListToCap_ZeroCap_DoesNotTrim
Still needs in-game confirm. A passing unit test is not a running-game
confirm.
