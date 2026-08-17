using LokrLab.Encounter;
using Xunit;

namespace LokrModding.Tests.Lab
{
	public sealed class EncounterTerrainRulesTests
	{
		[Fact]
		public void TerrainsRoundTrip_KeepsSourceAndTemplate()
		{
			EncounterFileModel model = EncounterFileModel.CreateEmpty();
			Assert.True(EncounterTerrainRules.Add(model, new EncounterTerrainModel
			{
				TerrainId = 1,
				Name = "Ice",
				Source = EncounterFileModel.TerrainSourceImport,
				Template = "combat_bridge"
			}));
			Assert.True(EncounterFileModel.TryParse(model.ToJson(), out EncounterFileModel parsed, out string error));
			Assert.Null(error);
			Assert.Equal(EncounterFileModel.CurrentSchemaVersion, parsed.SchemaVersion);
			Assert.Single(parsed.Terrains);
			Assert.Equal(1, parsed.Terrains[0].TerrainId);
			Assert.Equal("Ice", parsed.Terrains[0].Name);
			Assert.Equal(EncounterFileModel.TerrainSourceImport, parsed.Terrains[0].Source);
			Assert.Equal("combat_bridge", parsed.Terrains[0].Template);
		}

		[Fact]
		public void Add_SkipsSameIdAndTemplate()
		{
			EncounterFileModel file = EncounterFileModel.CreateEmpty();
			Assert.True(EncounterTerrainRules.Add(file, Row(1, "Ice", "combat_bridge")));
			Assert.False(EncounterTerrainRules.Add(file, Row(1, "Ice again", "combat_bridge")));
			Assert.True(EncounterTerrainRules.Add(file, Row(1, "Ice host", "fighttesterempty")));
			Assert.Equal(2, file.Terrains.Count);
		}

		[Fact]
		public void Remove_RefusesHostTemplateRow()
		{
			EncounterFileModel file = EncounterFileModel.CreateEmpty();
			Assert.True(EncounterTerrainRules.Add(file, new EncounterTerrainModel
			{
				TerrainId = 2,
				Name = "Snow",
				Source = EncounterFileModel.TerrainSourceTemplate,
				Template = "fighttesterempty"
			}));
			Assert.False(EncounterTerrainRules.Remove(file, 2, "fighttesterempty"));
			Assert.Single(file.Terrains);
		}

		[Fact]
		public void AddCustom_MintsIdAndName()
		{
			EncounterFileModel file = EncounterFileModel.CreateEmpty();
			EncounterTerrainModel first = EncounterTerrainRules.AddCustom(file);
			EncounterTerrainModel second = EncounterTerrainRules.AddCustom(file);
			Assert.Equal(EncounterTerrainRules.CustomIdStart, first.TerrainId);
			Assert.Equal(EncounterTerrainRules.CustomIdStart + 1, second.TerrainId);
			Assert.Equal("custom_1", first.Name);
			Assert.Equal("custom_2", second.Name);
			Assert.Equal(EncounterFileModel.TerrainSourceCustom, first.Source);
			Assert.True(EncounterTerrainRules.Remove(file, first.TerrainId, string.Empty));
			Assert.Single(file.Terrains);
		}

		[Fact]
		public void DropStaleHost_KeepsImport()
		{
			EncounterFileModel file = EncounterFileModel.CreateEmpty();
			EncounterTerrainRules.Add(file, new EncounterTerrainModel
			{
				TerrainId = 1,
				Name = "Old",
				Source = EncounterFileModel.TerrainSourceTemplate,
				Template = "combat_wip"
			});
			EncounterTerrainRules.Add(file, Row(4, "Imported", "combat_bridge"));
			EncounterTerrainRules.DropStaleHost(file, "fighttesterempty");
			Assert.Single(file.Terrains);
			Assert.Equal(EncounterFileModel.TerrainSourceImport, file.Terrains[0].Source);
		}

		[Fact]
		public void TileRoundTrip_KeepsImportTemplate()
		{
			EncounterFileModel model = EncounterFileModel.CreateEmpty();
			EncounterTileRules.Set(model, 3, 7, 4, "combat_bridge");
			Assert.True(EncounterFileModel.TryParse(model.ToJson(), out EncounterFileModel parsed, out string error));
			Assert.Null(error);
			Assert.Single(parsed.Tiles);
			Assert.Equal(4, parsed.Tiles[0].TerrainId);
			Assert.Equal("combat_bridge", parsed.Tiles[0].Template);
		}

		[Fact]
		public void Set_HostTemplate_StoresEmpty()
		{
			EncounterFileModel file = EncounterFileModel.CreateEmpty();
			EncounterTileRules.Set(file, 1, 2, 1, file.Template);
			Assert.Single(file.Tiles);
			Assert.Equal(string.Empty, file.Tiles[0].Template);
		}

		[Fact]
		public void TryParse_V4File_HasEmptyTerrains()
		{
			string json = "{\n  \"schemaVersion\": 4,\n  \"template\": \"fighttesterempty\",\n  \"walkableDefault\": false,\n  \"overrides\": [],\n  \"tiles\": [],\n  \"combatants\": []\n}\n";
			Assert.True(EncounterFileModel.TryParse(json, out EncounterFileModel parsed, out string error));
			Assert.Null(error);
			Assert.Equal(4, parsed.SchemaVersion);
			Assert.Empty(parsed.Terrains);
			Assert.True(parsed.TilesDefault);
		}

		private static EncounterTerrainModel Row(int id, string name, string template)
		{
			return new EncounterTerrainModel
			{
				TerrainId = id,
				Name = name,
				Source = EncounterFileModel.TerrainSourceImport,
				Template = template
			};
		}
	}
}
