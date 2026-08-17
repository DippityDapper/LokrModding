using LokrLab.Encounter;
using Xunit;

namespace LokrModding.Tests.Lab
{
	public sealed class EncounterPropRulesTests
	{
		[Fact]
		public void PropsRoundTrip_KeepsPrefabAndHex()
		{
			EncounterFileModel model = EncounterFileModel.CreateEmpty();
			EncounterPropModel added = EncounterPropRules.Add(model, "forest_deco_generic_bush_1x1_02");
			Assert.NotNull(added);
			added.Col = 8;
			added.Row = 10;
			added.Flipped = true;
			Assert.True(EncounterFileModel.TryParse(model.ToJson(), out EncounterFileModel parsed, out string error));
			Assert.Null(error);
			Assert.Equal(EncounterFileModel.CurrentSchemaVersion, parsed.SchemaVersion);
			Assert.Single(parsed.Props);
			Assert.Equal(added.Id, parsed.Props[0].Id);
			Assert.Equal("forest_deco_generic_bush_1x1_02", parsed.Props[0].PrefabName);
			Assert.Equal(8, parsed.Props[0].Col);
			Assert.Equal(10, parsed.Props[0].Row);
			Assert.True(parsed.Props[0].Flipped);
			Assert.True(parsed.Props[0].Snap);
			Assert.Null(parsed.Props[0].X);
			Assert.Null(parsed.Props[0].Y);
		}

		[Fact]
		public void PropsRoundTrip_KeepsFreeWorld()
		{
			EncounterFileModel model = EncounterFileModel.CreateEmpty();
			EncounterPropModel added = EncounterPropRules.Add(model, "forest_deco_generic_bush_1x1_02");
			Assert.NotNull(added);
			added.Snap = false;
			added.X = 12.4f;
			added.Y = -3.25f;
			Assert.True(EncounterPropRules.HasPlacement(added));
			Assert.False(EncounterPropRules.SnapsToGrid(added));
			Assert.True(EncounterFileModel.TryParse(model.ToJson(), out EncounterFileModel parsed, out string error));
			Assert.Null(error);
			Assert.Equal(EncounterFileModel.CurrentSchemaVersion, parsed.SchemaVersion);
			Assert.False(parsed.Props[0].Snap);
			Assert.Equal(12.4f, parsed.Props[0].X.Value, 3);
			Assert.Equal(-3.25f, parsed.Props[0].Y.Value, 3);
			Assert.Null(parsed.Props[0].Col);
			Assert.Null(parsed.Props[0].Row);
		}

		[Fact]
		public void V8File_MissingSnap_LoadsTrue()
		{
			const string json = @"{
  ""schemaVersion"": 8,
  ""template"": ""fighttesterempty"",
  ""walkableDefault"": false,
  ""tilesDefault"": false,
  ""overrides"": [],
  ""tiles"": [],
  ""terrains"": [],
  ""props"": [
    { ""id"": ""bush_1"", ""prefabName"": ""forest_deco_generic_bush_1x1_02"", ""col"": 8, ""row"": 10, ""flipped"": false }
  ],
  ""combatants"": []
}
";
			Assert.True(EncounterFileModel.TryParse(json, out EncounterFileModel parsed, out string error));
			Assert.Null(error);
			Assert.Equal(8, parsed.SchemaVersion);
			Assert.True(parsed.Props[0].Snap);
			Assert.Equal(8, parsed.Props[0].Col);
			Assert.Equal(10, parsed.Props[0].Row);
			Assert.Null(parsed.Props[0].X);
			Assert.Null(parsed.Props[0].Y);
		}

		[Fact]
		public void V6File_MissingProps_LoadsEmpty()
		{
			const string json = @"{
  ""schemaVersion"": 6,
  ""template"": ""fighttesterempty"",
  ""walkableDefault"": false,
  ""tilesDefault"": false,
  ""overrides"": [],
  ""tiles"": [],
  ""terrains"": [],
  ""combatants"": []
}
";
			Assert.True(EncounterFileModel.TryParse(json, out EncounterFileModel parsed, out string error));
			Assert.Null(error);
			Assert.Equal(6, parsed.SchemaVersion);
			Assert.Empty(parsed.Props);
		}

		[Fact]
		public void CanPlace_RefusesOffBoardAndDuplicateHex()
		{
			EncounterFileModel file = EncounterFileModel.CreateEmpty();
			EncounterPropModel first = EncounterPropRules.Add(file, "a_deco");
			EncounterPropModel second = EncounterPropRules.Add(file, "b_deco");
			Assert.Null(EncounterPropRules.CanPlace(file, first, 2, 3, 8, 8));
			first.Col = 2;
			first.Row = 3;
			Assert.Equal("Unblock to grow the board, then place props.",
				EncounterPropRules.CanPlace(file, second, 9, 0, 8, 8));
			Assert.Contains("already uses hex", EncounterPropRules.CanPlace(file, second, 2, 3, 8, 8));
		}

		[Fact]
		public void CanPlace_IgnoresFreeMoveOnSameHex()
		{
			EncounterFileModel file = EncounterFileModel.CreateEmpty();
			EncounterPropModel snapped = EncounterPropRules.Add(file, "a_deco");
			EncounterPropModel free = EncounterPropRules.Add(file, "b_deco");
			snapped.Col = 2;
			snapped.Row = 3;
			free.Snap = false;
			free.X = 4.5f;
			free.Y = 1.25f;
			EncounterPropModel third = EncounterPropRules.Add(file, "c_deco");
			Assert.Contains("already uses hex", EncounterPropRules.CanPlace(file, third, 2, 3, 8, 8));
			Assert.Equal(snapped.Id, EncounterPropRules.OccupantIdAt(file, 2, 3));
			Assert.Equal(free.Id, EncounterPropRules.NearestFreeId(file, 4.5f, 1.25f, 0.01f));
			Assert.Null(EncounterPropRules.NearestFreeId(file, 0f, 0f, 0.01f));
			EncounterPropRules.ClearPlacement(free);
			Assert.False(EncounterPropRules.HasPlacement(free));
		}

		[Fact]
		public void Add_MintsIdAwayFromCombatants()
		{
			EncounterFileModel file = EncounterFileModel.CreateEmpty();
			file.Combatants.Add(new EncounterCombatantModel
			{
				Id = "forest_deco_generic_bush_1x1_02_1",
				Side = EncounterFileModel.GoodSide,
				Source = EncounterFileModel.SourceUnit,
				UnitId = "BanditRaider"
			});
			EncounterPropModel prop = EncounterPropRules.Add(file, "forest_deco_generic_bush_1x1_02");
			Assert.NotNull(prop);
			Assert.Equal("forest_deco_generic_bush_1x1_02_2", prop.Id);
			Assert.True(EncounterPropRules.Remove(file, prop.Id));
			Assert.Empty(file.Props);
		}
	}
}
