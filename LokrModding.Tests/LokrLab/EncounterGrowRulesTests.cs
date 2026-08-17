using LokrLab.Encounter;
using Xunit;

namespace LokrModding.Tests.Lab
{
	public sealed class EncounterGrowRulesTests
	{
		[Fact]
		public void EffectiveLiveSize_Unset_IsTemplateLive()
		{
			EncounterFileModel file = EncounterFileModel.CreateEmpty();
			EncounterGrowRules.EffectiveLiveSize(file, out int width, out int height);
			Assert.Equal(24, width);
			Assert.Equal(24, height);
			Assert.False(EncounterGrowRules.HasGrown(file));
		}

		[Fact]
		public void EffectiveLiveSize_WalkableOverride_GrowsBoard()
		{
			EncounterFileModel file = EncounterFileModel.CreateEmpty();
			EncounterBoardRules.SetOverride(file, 30, 10, true);
			EncounterGrowRules.EffectiveLiveSize(file, out int width, out int height);
			Assert.Equal(31, width);
			Assert.Equal(24, height);
			Assert.True(EncounterGrowRules.HasGrown(file));
		}

		[Fact]
		public void EffectiveLiveSize_BlockedOverride_DoesNotGrow()
		{
			EncounterFileModel file = EncounterFileModel.CreateEmpty();
			EncounterBoardRules.SetOverride(file, 30, 10, false);
			EncounterGrowRules.EffectiveLiveSize(file, out int width, out int height);
			Assert.Equal(24, width);
			Assert.Equal(24, height);
		}

		[Fact]
		public void EffectiveLiveSize_Placement_GrowsBoard()
		{
			EncounterFileModel file = EncounterFileModel.CreateEmpty();
			file.Combatants.Add(new EncounterCombatantModel
			{
				Id = "raider_1",
				Col = 28,
				Row = 5
			});
			EncounterGrowRules.EffectiveLiveSize(file, out int width, out int height);
			Assert.Equal(29, width);
			Assert.Equal(24, height);
		}

		[Fact]
		public void SetupLiveSize_AddsHalo()
		{
			EncounterFileModel file = EncounterFileModel.CreateEmpty();
			EncounterGrowRules.SetupLiveSize(file, out int width, out int height);
			Assert.Equal(25, width);
			Assert.Equal(25, height);
		}

		[Fact]
		public void Normalize_LegacySize_BecomesWalkableOverrides()
		{
			EncounterFileModel file = EncounterFileModel.CreateEmpty();
			file.Width = 26;
			file.Height = 24;
			Assert.True(EncounterFileModel.TryParse(file.ToJson(), out EncounterFileModel parsed, out string error));
			Assert.Null(error);
			Assert.Equal(EncounterFileModel.CurrentSchemaVersion, parsed.SchemaVersion);
			Assert.False(parsed.Width.HasValue);
			Assert.False(parsed.Height.HasValue);
			EncounterGrowRules.EffectiveLiveSize(parsed, out int width, out int height);
			Assert.Equal(26, width);
			Assert.Equal(24, height);
			Assert.NotNull(EncounterBoardRules.FindOverride(parsed, 24, 0));
			Assert.True(EncounterBoardRules.FindOverride(parsed, 24, 0).Walkable);
		}

		[Fact]
		public void TryParse_V2File_HasNullSize()
		{
			string json = "{\n  \"schemaVersion\": 2,\n  \"template\": \"fighttesterempty\",\n  \"overrides\": [],\n  \"combatants\": []\n}\n";
			Assert.True(EncounterFileModel.TryParse(json, out EncounterFileModel parsed, out string error));
			Assert.Null(error);
			Assert.Equal(2, parsed.SchemaVersion);
			Assert.False(parsed.Width.HasValue);
			Assert.False(parsed.Height.HasValue);
		}

		[Fact]
		public void EffectiveLiveSize_CapsAtMaxLive()
		{
			EncounterFileModel file = EncounterFileModel.CreateEmpty();
			EncounterBoardRules.SetOverride(file, 80, 80, true);
			EncounterGrowRules.EffectiveLiveSize(file, out int width, out int height);
			Assert.Equal(EncounterGrowRules.MaxLive, width);
			Assert.Equal(EncounterGrowRules.MaxLive, height);
		}
	}
}
