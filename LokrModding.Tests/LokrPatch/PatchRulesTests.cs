using System.Collections.Generic;
using LokrPatch;
using Xunit;

namespace LokrModding.Tests.Patch
{
	public sealed class PatchRulesTests
	{
		[Theory]
		[InlineData(1f, 0f, true)]
		[InlineData(0f, 0f, false)]
		[InlineData(0.3f, 0.1f, true)]
		[InlineData(0.3f, 0.3f, false)]
		[Trait("Issue", "loot-anyof-chance-always-fires")]
		public void LootChildFires_UsesFloatComparison(float chance, float roll, bool expected)
		{
			Assert.Equal(expected, PatchRules.LootChildFires(chance, roll));
		}

		[Fact]
		[Trait("Issue", "dialog-first-no-fallback")]
		public void DialogExits_WhenNoChildPasses()
		{
			Assert.True(PatchRules.ShouldExitDialogWhenNoPassingChild(false));
			Assert.False(PatchRules.ShouldExitDialogWhenNoPassingChild(true));
		}

		[Fact]
		[Trait("Issue", "ability-kv-pointmagnitude-constructs-pointmult")]
		public void PointMagnitude_MapsToFunctionPointMagnitude()
		{
			Dictionary<string, string> map = new Dictionary<string, string>
			{
				{ PatchRules.PointMagnitudeKvKey, "FunctionPointMult" }
			};
			PatchRules.ApplyPointMagnitudeMapping(map);
			Assert.Equal(PatchRules.PointMagnitudeTypeName, map[PatchRules.PointMagnitudeKvKey]);
		}

		[Theory]
		[InlineData("AOE", true)]
		[InlineData("UNIT_TARGET | AOE", true)]
		[InlineData("UNIT_TARGET", false)]
		[InlineData("AOE_LIKE", false)]
		[Trait("Issue", "ability-aoe-missing-center-keys-nre")]
		public void HasAoeToken_StandaloneFlag(string behavior, bool expected)
		{
			Assert.Equal(expected, PatchRules.HasAoeToken(behavior));
		}

		[Fact]
		[Trait("Issue", "ability-ai-retreat-if-week-typo")]
		public void RetreatIfWeak_CorrectKeyDiffersFromTypo()
		{
			Assert.Equal("RetreatIfWeakAI", PatchRules.RetreatIfWeakCorrectKey);
			Assert.Equal("RetreatIfWeekAI", PatchRules.RetreatIfWeakTypoKey);
			Assert.NotEqual(PatchRules.RetreatIfWeakCorrectKey, PatchRules.RetreatIfWeakTypoKey);
		}

		[Fact]
		[Trait("Issue", "ability-ai-per-affected-not-action")]
		public void PerAffectedAi_IsSkippedActionKey()
		{
			Assert.True(PatchRules.IsSkippedActionKey("PerAffectedAI"));
			Assert.False(PatchRules.IsSkippedActionKey("GetInRangeAI"));
		}

		[Fact]
		[Trait("Issue", "ability-callfunction-empty-filter-throws")]
		public void EmptyCallFunctionFilter_IsSkipped()
		{
			Assert.True(PatchRules.ShouldSkipEmptyUnitFilter(0));
			Assert.False(PatchRules.ShouldSkipEmptyUnitFilter(1));
		}

		[Fact]
		[Trait("Issue", "ability-ai-empty-brain-divide-by-zero")]
		public void EmptyConsiderations_ReturnsZero()
		{
			Assert.True(PatchRules.EmptyConsiderationsReturnsZero(0));
			Assert.False(PatchRules.EmptyConsiderationsReturnsZero(2));
		}

		[Fact]
		[Trait("Issue", "ability-equal-null-lhs-nre")]
		public void NullSafeEquals_NullLhs()
		{
			Assert.True(PatchRules.NullSafeEquals(null, null));
			Assert.False(PatchRules.NullSafeEquals(null, "x"));
			Assert.True(PatchRules.NullSafeEquals("x", "x"));
		}

		[Fact]
		[Trait("Issue", "ability-each-in-list-actions-if-empty-inverted")]
		public void ActionsIfEmpty_OnlyWhenCountIsZero()
		{
			Assert.True(PatchRules.ActionsIfEmptyShouldRun(0));
			Assert.False(PatchRules.ActionsIfEmptyShouldRun(3));
		}

		[Fact]
		[Trait("Issue", "ability-tooltip-missing-var-returns-999")]
		public void MissingVariable_ReturnsZeroNot999()
		{
			Assert.True(PatchRules.MissingVariableReturnsZero(null));
			Assert.False(PatchRules.MissingVariableReturnsZero(999f));
			Assert.Equal(0f, PatchRules.MissingVariableFallback);
		}

		[Fact]
		[Trait("Issue", "activity-interface-point-target-nre")]
		public void NullTargetFilter_IsSkipped()
		{
			Assert.True(PatchRules.ShouldSkipNullTargetFilter(true));
			Assert.False(PatchRules.ShouldSkipNullTargetFilter(false));
		}

		[Fact]
		[Trait("Issue", "stats-apply-modifier-missing-stat-throws")]
		public void MissingStat_IsSkipped()
		{
			Assert.True(PatchRules.ShouldSkipMissingStat(false));
			Assert.False(PatchRules.ShouldSkipMissingStat(true));
		}

		[Fact]
		[Trait("Issue", "save-sanitize-drops-unknown-ids")]
		public void Sanitize_DoesNotDiscardRun()
		{
			Assert.False(PatchRules.SanitizeDiscardsRunOnUnknownIds);
		}

		[Fact]
		[Trait("Issue", "save-party-reset-to-vanilla-trio")]
		public void Party_KeepsKnownIds_NoVanillaTrioReset()
		{
			Assert.False(PatchRules.ShouldResetPartyToVanillaTrio(1));
			Assert.False(PatchRules.ShouldResetPartyToVanillaTrio(4));
			HashSet<string> registered = new HashSet<string> { "Gerald", "Assassin" };
			List<string> kept = PatchRules.FilterKnownIds(
				new[] { "Gerald", "MissingModHero", "Assassin" },
				registered);
			Assert.Equal(new[] { "Gerald", "Assassin" }, kept);
		}

		[Fact]
		[Trait("Issue", "party-stow-shifts-remaining-into-wrong-slots")]
		public void ShouldOmitPartyId_NullOrEmpty()
		{
			Assert.True(PatchRules.ShouldOmitPartyId(null));
			Assert.True(PatchRules.ShouldOmitPartyId(""));
			Assert.False(PatchRules.ShouldOmitPartyId("Ranger"));
		}

		[Fact]
		[Trait("Issue", "party-stow-shifts-remaining-into-wrong-slots")]
		public void IsCorePartySlot_FirstThreeOnly()
		{
			Assert.True(PatchRules.IsCorePartySlot(0));
			Assert.True(PatchRules.IsCorePartySlot(2));
			Assert.False(PatchRules.IsCorePartySlot(3));
			Assert.False(PatchRules.IsCorePartySlot(-1));
		}

		[Fact]
		[Trait("Issue", "party-stow-shifts-remaining-into-wrong-slots")]
		public void SplitSaveParty_HolesUnknownAndSkipsEmpty()
		{
			HashSet<string> registered = new HashSet<string> { "Ranger", "ArcaneMage" };
			List<string> slots = new List<string>();
			List<StowedPartyMember> stowed = new List<StowedPartyMember>();
			PatchRules.SplitSaveParty(
				new[] { "Onagro", "Ranger", null, "ArcaneMage" },
				registered,
				slots,
				stowed);
			Assert.Equal(new string[] { null, "Ranger", null, "ArcaneMage" }, slots);
			Assert.Single(stowed);
			Assert.Equal(0, stowed[0].Index);
			Assert.Equal("Onagro", stowed[0].UniqueId);
		}

		[Fact]
		[Trait("Issue", "party-stow-shifts-remaining-into-wrong-slots")]
		public void AlignPartySlotsByRole_LeavesLegendHole()
		{
			HashSet<string> legends = new HashSet<string> { "Onagro", "Gerald" };
			List<string> aligned = PatchRules.AlignPartySlotsByRole(
				new[] { "Ranger", "ArcaneMage" },
				legends);
			Assert.Equal(new string[] { null, "Ranger", "ArcaneMage" }, aligned);
		}

		[Fact]
		[Trait("Issue", "party-stow-shifts-remaining-into-wrong-slots")]
		public void AlignPartySlotsByRole_RepairsCompactedLegendAtEnd()
		{
			HashSet<string> legends = new HashSet<string> { "Onagro" };
			List<string> aligned = PatchRules.AlignPartySlotsByRole(
				new[] { "Ranger", "ArcaneMage", "Onagro" },
				legends);
			Assert.Equal(new[] { "Onagro", "Ranger", "ArcaneMage" }, aligned);
		}

		[Fact]
		[Trait("Issue", "party-stow-shifts-remaining-into-wrong-slots")]
		public void AlignPartySlotsByRole_VanillaTrioUnchanged()
		{
			HashSet<string> legends = new HashSet<string> { "Gerald" };
			List<string> aligned = PatchRules.AlignPartySlotsByRole(
				new[] { "Gerald", "Ranger", "ArcaneMage" },
				legends);
			Assert.Equal(new[] { "Gerald", "Ranger", "ArcaneMage" }, aligned);
		}

		[Fact]
		[Trait("Issue", "party-stow-shifts-remaining-into-wrong-slots")]
		public void MergeStowedPartyIds_FillsLegendHole()
		{
			List<StowedPartyMember> stowed = new List<StowedPartyMember>
			{
				new StowedPartyMember(0, "Onagro")
			};
			List<string> merged = PatchRules.MergeStowedPartyIds(
				new string[] { null, "Ranger", "ArcaneMage" },
				stowed);
			Assert.Equal(new[] { "Onagro", "Ranger", "ArcaneMage" }, merged);
		}

		[Fact]
		[Trait("Issue", "party-stow-shifts-remaining-into-wrong-slots")]
		public void MergeStowedPartyIds_OccupiedIndexUsesFirstHole()
		{
			List<StowedPartyMember> stowed = new List<StowedPartyMember>
			{
				new StowedPartyMember(2, "Onagro")
			};
			List<string> merged = PatchRules.MergeStowedPartyIds(
				new string[] { null, "Ranger", "ArcaneMage" },
				stowed);
			Assert.Equal(new[] { "Onagro", "Ranger", "ArcaneMage" }, merged);
		}

		[Fact]
		[Trait("Issue", "party-stow-shifts-remaining-into-wrong-slots")]
		public void CompactPartyIds_DropsHoles()
		{
			Assert.Equal(
				new[] { "Ranger", "ArcaneMage" },
				PatchRules.CompactPartyIds(new string[] { null, "Ranger", "ArcaneMage" }));
		}

		[Fact]
		[Trait("Issue", "inventory-additem-never-sets-id")]
		public void EmptyItemId_NeedsGuid()
		{
			Assert.True(PatchRules.ShouldAssignItemInstanceId(null));
			Assert.True(PatchRules.ShouldAssignItemInstanceId(""));
			Assert.False(PatchRules.ShouldAssignItemInstanceId("already-set"));
		}

		[Fact]
		[Trait("Issue", "hero-update-skills-unknown-id-nre")]
		public void UnknownSkill_IsSkipped()
		{
			Assert.True(PatchRules.IsUnknownSkill(false));
			Assert.False(PatchRules.IsUnknownSkill(true));
		}

		[Fact]
		[Trait("Issue", "hero-progress-window-unknown-uniqueid-nre")]
		public void UnknownUniqueId_IsDropped()
		{
			Assert.True(PatchRules.ShouldDropUnknownUniqueId(false));
			Assert.False(PatchRules.ShouldDropUnknownUniqueId(true));
		}

		[Fact]
		[Trait("Issue", "map-start-unknown-starting-hero-nre")]
		public void UnknownStartingHero_IsSkipped()
		{
			Assert.True(PatchRules.ShouldSkipUnknownStartingHero(false));
			Assert.False(PatchRules.ShouldSkipUnknownStartingHero(true));
		}

		[Fact]
		[Trait("Issue", "map-hud-unknown-modifier-config-nre")]
		public void NullModifierConfig_IsSkipped()
		{
			Assert.True(PatchRules.ShouldSkipNullModifierConfig(true));
			Assert.False(PatchRules.ShouldSkipNullModifierConfig(false));
		}

		[Fact]
		[Trait("Issue", "progression-help-popup-index-oor")]
		public void ProgressionHelp_ClampsAndFinishesOffEitherList()
		{
			Assert.True(PatchRules.IsProgressionHelpIndexInRange(0, 2, 3));
			Assert.False(PatchRules.IsProgressionHelpIndexInRange(2, 2, 3));
			Assert.True(PatchRules.ProgressionHelpNextShouldFinish(1, 2, 3));
			Assert.False(PatchRules.ProgressionHelpNextShouldFinish(0, 3, 3));
			Assert.True(PatchRules.ProgressionHelpNextShouldFinish(2, 3, 3));
		}

		[Fact]
		[Trait("Issue", "fight-started-empty-initiative-nre")]
		public void NullActiveUnit_IsSkipped()
		{
			Assert.True(PatchRules.ShouldSkipNullActiveUnit(true));
			Assert.False(PatchRules.ShouldSkipNullActiveUnit(false));
		}
	}
}
