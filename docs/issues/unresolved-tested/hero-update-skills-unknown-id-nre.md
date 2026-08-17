# Hero.UpdateSkills NullRefs on an unknown skill id

Area: LokrCharacterLoader / Character Lab (custom hero skill lists)
Status: unresolved-tested

As of 2026-08-15: `Hero.UpdateSkills` in
`ih-original/Ironhide.Legends/Ironhide/Legends/Model/Metagame/Heroes/Hero.cs`
looks up each `heroDefinition.skills` id in `AbilitiesDefinitions.abilities`.
A miss logs `HERO: Can't find skill` then immediately reads
`valueOrDefault2.AbilityBehavior`, which NullReferenceException. The ctor,
`SetLevelUP`, and `HeroManager.CreateHero` / `Load` all call `UpdateSkills`,
so a custom hero whose skill failed to register (or is a typo) crashes at
recruit, load, or level-up — including Character Lab live-reload.

The same method indexes `int[] {1,2,3}` at `level-1` (and again for the
skill-pool branch). A level outside 1..3 IndexOutOfRangeException even
though that slot value is unused in the progression branch.

Suggested fix: Harmony-prefix or postfix `UpdateSkills` so a missing ability
is skipped after the log, and level is clamped to 1..3. Do not start that
patch from the HTML-docs track.

See
[`Hero.html`](../../api/base-game/Ironhide/Legends/Model/Metagame/Heroes/Hero.html).

## Proposed solution (Pass 1, 2026-08-15)

**Home:** LokrPatch
**Approach:** Harmony prefix that replaces `Hero.UpdateSkills` with the vanilla body plus two guards: a missing `AbilitiesDefinitions.abilities` entry is skipped after the existing log (predicate returns false instead of reading `AbilityBehavior`), and the unused `{1,2,3}` / skill-pool `{0,1,2,3}` indexers are clamped so a level outside the table cannot `IndexOutOfRangeException`. Vanilla throw: the log already admits the miss, then NRE. Resilience belongs here even when only a custom skill list triggers it.
**Exact change:** `HeroUpdateSkillsMissingPatch` in LokrPatch. `[HarmonyPatch(typeof(Hero), nameof(Hero.UpdateSkills))]` prefix, return false. Copy `UpdateSkills`: merge `unitDefinition.skills` into `heroDefinition.skills` as vanilla. In both `Where` lambdas, after `GetValueOrDefault(s, null)` and the `HERO: Can't find skill` log, `if (valueOrDefault2 == null) return false`. Progression branch: clamp `level` to 1..3 before `array[level - 1]` (`int[] {1,2,3}`). Skill-pool branch: clamp to 1..4 before indexing `int[] {0,1,2,3}`. Do not remove unknown ids from `heroDefinition.skills` (that would rewrite the save). Leave the `skillProgression` missing-key `throw` as vanilla (different bug).
**Do not:** Strip unknown skills from the saved hero blob. Clamp the hero’s `level` stat. Transpile the LINQ lambdas. Fold this into `HeroSkillSanitizer` (that helper dedupes before `RegenerateFakeUnit`; this NRE is inside `UpdateSkills` itself, including the ctor path before that sanitizer).
**In-game verify:** 1. Custom hero whose `skills` list contains a typo / unregistered ability: recruit or load without NRE; log shows `Can't find skill` plus a LokrPatch skip; other real skills still apply. 2. Character Lab live-reload that drops an ability the hero still lists: map/hero room still opens. 3. Level-up a vanilla hero 1→2→3: progression picks unchanged. 4. Vanilla Gerald load: no extra skip warnings.
**Risk:** A skipped unknown skill means that hero has fewer interactive skills than the author intended (safer than crashing recruit/load). Clamping the dead `array[level-1]` store does not change progression contents. Do not treat a missing progression table as skip-and-log in this patch.

## Unit tests (unresolved-tested)

Moved: 2026-08-15
Tests:
- LokrModding.Tests.Patch.PatchRulesTests.UnknownSkill_IsSkipped
Still needs in-game confirm. A passing unit test is not a running-game
confirm.
