using LokrLab.Encounter;
using Xunit;

namespace LokrModding.Tests.Lab
{
	public sealed class EncounterTemplateRulesTests
	{
		[Fact]
		public void EmptyEnough_IsDefaultAndBridge()
		{
			Assert.Equal(2, EncounterTemplateRules.EmptyEnough.Length);
			Assert.Equal(EncounterFileModel.DefaultTemplate, EncounterTemplateRules.EmptyEnough[0]);
			Assert.Equal(EncounterTemplateRules.CombatBridge, EncounterTemplateRules.EmptyEnough[1]);
		}

		[Fact]
		public void Normalize_Empty_IsDefault()
		{
			Assert.Equal(EncounterFileModel.DefaultTemplate, EncounterTemplateRules.Normalize("  "));
			Assert.Equal(EncounterFileModel.DefaultTemplate, EncounterTemplateRules.Normalize(null));
		}

		[Fact]
		public void Canonical_MatchesKnownCasing()
		{
			Assert.Equal(EncounterTemplateRules.CombatBridge, EncounterTemplateRules.Canonical("Combat_Bridge"));
			Assert.Equal(EncounterTemplateRules.CombatWip, EncounterTemplateRules.Canonical("Combat_WIP"));
			Assert.Equal("combat_blank", EncounterTemplateRules.Canonical("combat_blank"));
		}

		[Fact]
		public void IsEmptyEnough_AllowsWipRejectsBlank()
		{
			Assert.True(EncounterTemplateRules.IsEmptyEnough("fighttesterempty"));
			Assert.True(EncounterTemplateRules.IsEmptyEnough("combat_bridge"));
			Assert.True(EncounterTemplateRules.IsEmptyEnough("combat_wip"));
			Assert.False(EncounterTemplateRules.IsEmptyEnough("combat_blank"));
		}

		[Fact]
		public void Label_DescribesOpenFieldAndBridge()
		{
			Assert.Contains("open field", EncounterTemplateRules.Label("fighttesterempty"));
			Assert.Contains("bridge", EncounterTemplateRules.Label("combat_bridge"));
		}

		[Fact]
		public void Options_AppendsUnknownCurrent()
		{
			string[] options = EncounterTemplateRules.Options("combat_blank");
			Assert.Equal(3, options.Length);
			Assert.Equal("combat_blank", options[2]);
			Assert.Equal(2, EncounterTemplateRules.IndexOf(options, "combat_blank"));
		}

		[Fact]
		public void Options_KeepsSavedWip()
		{
			string[] options = EncounterTemplateRules.Options("combat_wip");
			Assert.Equal(3, options.Length);
			Assert.Equal(EncounterTemplateRules.CombatWip, options[2]);
		}

		[Fact]
		public void Options_DoesNotDuplicateDefault()
		{
			string[] options = EncounterTemplateRules.Options("FightTesterEmpty");
			Assert.Equal(2, options.Length);
			Assert.Equal(0, EncounterTemplateRules.IndexOf(options, "FightTesterEmpty"));
		}
	}
}
