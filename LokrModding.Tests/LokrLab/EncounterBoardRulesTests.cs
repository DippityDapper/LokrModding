using LokrLab.Encounter;
using Xunit;

namespace LokrModding.Tests.Lab
{
	public sealed class EncounterBoardRulesTests
	{
		[Fact]
		public void EffectiveWalkable_NewEncounter_IgnoresTemplate()
		{
			EncounterFileModel file = EncounterFileModel.CreateEmpty();
			Assert.False(file.WalkableDefault);
			Assert.False(EncounterBoardRules.EffectiveWalkable(file, 8, 10, true));
		}

		[Fact]
		public void EffectiveWalkable_DefaultTrue_UsesTemplate()
		{
			EncounterFileModel file = EncounterFileModel.CreateEmpty();
			file.WalkableDefault = true;
			Assert.True(EncounterBoardRules.EffectiveWalkable(file, 8, 10, true));
			Assert.False(EncounterBoardRules.EffectiveWalkable(file, 8, 10, false));
		}

		[Fact]
		public void Set_ReplacesSameHex()
		{
			EncounterFileModel file = EncounterFileModel.CreateEmpty();
			EncounterBoardRules.SetOverride(file, 8, 10, false);
			EncounterBoardRules.SetOverride(file, 8, 10, true);
			Assert.Single(file.Overrides);
			Assert.True(EncounterBoardRules.EffectiveWalkable(file, 8, 10, false));
		}

		[Fact]
		public void OverridesRoundTrip_KeepsWalkableFlag()
		{
			EncounterFileModel model = EncounterFileModel.CreateEmpty();
			EncounterBoardRules.SetOverride(model, 3, 5, true);
			EncounterBoardRules.SetOverride(model, 8, 10, false);
			Assert.True(EncounterFileModel.TryParse(model.ToJson(), out EncounterFileModel parsed, out string error));
			Assert.Null(error);
			Assert.Equal(EncounterFileModel.CurrentSchemaVersion, parsed.SchemaVersion);
			Assert.False(parsed.WalkableDefault);
			Assert.Equal(2, parsed.Overrides.Count);
			Assert.True(EncounterBoardRules.EffectiveWalkable(parsed, 3, 5, false));
			Assert.False(EncounterBoardRules.EffectiveWalkable(parsed, 8, 10, true));
		}

		[Fact]
		public void TryParse_V1File_HasEmptyOverrides()
		{
			string json = "{\n  \"schemaVersion\": 1,\n  \"template\": \"fighttesterempty\",\n  \"combatants\": []\n}\n";
			Assert.True(EncounterFileModel.TryParse(json, out EncounterFileModel parsed, out string error));
			Assert.Null(error);
			Assert.Equal(1, parsed.SchemaVersion);
			Assert.True(parsed.WalkableDefault);
			Assert.Empty(parsed.Overrides);
		}

		[Fact]
		public void HasPlacementAt_MatchesAuthoredHex()
		{
			EncounterFileModel file = EncounterFileModel.CreateEmpty();
			file.Combatants.Add(new EncounterCombatantModel
			{
				Id = "gerald_1",
				Col = 6,
				Row = 10
			});
			Assert.True(EncounterBoardRules.HasPlacementAt(file, 6, 10));
			Assert.False(EncounterBoardRules.HasPlacementAt(file, 7, 10));
		}

		[Fact]
		public void EnsurePlacementsWalkable_OverwritesBlockedOverride()
		{
			EncounterFileModel file = EncounterFileModel.CreateEmpty();
			file.Combatants.Add(new EncounterCombatantModel
			{
				Id = "gerald_1",
				Col = 6,
				Row = 10
			});
			EncounterBoardRules.SetOverride(file, 6, 10, false);
			EncounterBoardRules.EnsurePlacementsWalkable(file);
			Assert.True(EncounterBoardRules.EffectiveWalkable(file, 6, 10, false));
		}

		[Fact]
		public void TryParse_OverridesWithoutCombatants()
		{
			string json = "{\n  \"schemaVersion\": 2,\n  \"template\": \"fighttesterempty\",\n  \"overrides\": [\n    { \"col\": 8, \"row\": 10, \"walkable\": false }\n  ]\n}\n";
			Assert.True(EncounterFileModel.TryParse(json, out EncounterFileModel parsed, out string error));
			Assert.Null(error);
			Assert.Single(parsed.Overrides);
			Assert.Equal(8, parsed.Overrides[0].Col);
			Assert.Equal(10, parsed.Overrides[0].Row);
			Assert.False(parsed.Overrides[0].Walkable);
			Assert.Empty(parsed.Combatants);
		}
	}
}
