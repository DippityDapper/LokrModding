using LokrLab.Encounter;
using Xunit;

namespace LokrModding.Tests.Lab
{
	public sealed class EncounterTileRulesTests
	{
		[Fact]
		public void Set_ReplacesSameHex()
		{
			EncounterFileModel file = EncounterFileModel.CreateEmpty();
			EncounterTileRules.Set(file, 4, 5, 1);
			EncounterTileRules.Set(file, 4, 5, 2);
			Assert.Single(file.Tiles);
			Assert.Equal(2, EncounterTileRules.Find(file, 4, 5).TerrainId);
		}

		[Fact]
		public void IsSameStamp_RequiresTemplateWhenIdsMatch()
		{
			EncounterFileModel file = EncounterFileModel.CreateEmpty();
			EncounterTileRules.Set(file, 4, 5, 1, "combat_bridge");
			EncounterHexTile tile = EncounterTileRules.Find(file, 4, 5);
			Assert.True(EncounterTileRules.IsSameStamp(file, tile, 1, "combat_bridge"));
			Assert.False(EncounterTileRules.IsSameStamp(file, tile, 1, file.Template));
			Assert.False(EncounterTileRules.IsSameStamp(file, tile, 2, "combat_bridge"));
		}

		[Fact]
		public void TilesRoundTrip_KeepsTerrainId()
		{
			EncounterFileModel model = EncounterFileModel.CreateEmpty();
			EncounterTileRules.Set(model, 3, 7, 4);
			Assert.True(EncounterFileModel.TryParse(model.ToJson(), out EncounterFileModel parsed, out string error));
			Assert.Null(error);
			Assert.Equal(EncounterFileModel.CurrentSchemaVersion, parsed.SchemaVersion);
			Assert.Single(parsed.Tiles);
			Assert.Equal(3, parsed.Tiles[0].Col);
			Assert.Equal(7, parsed.Tiles[0].Row);
			Assert.Equal(4, parsed.Tiles[0].TerrainId);
		}

		[Fact]
		public void Clear_RemovesOverride()
		{
			EncounterFileModel file = EncounterFileModel.CreateEmpty();
			EncounterTileRules.Set(file, 4, 5, 1);
			Assert.True(EncounterTileRules.Clear(file, 4, 5));
			Assert.Empty(file.Tiles);
			Assert.False(EncounterTileRules.Clear(file, 4, 5));
		}

		[Fact]
		public void HexToTileCell_OddRowShiftsX()
		{
			int cellX;
			int cellY;
			EncounterTileRules.HexToTileCell(0, 0, 1, -1, out cellX, out cellY);
			Assert.Equal(1, cellX);
			Assert.Equal(-1, cellY);
			EncounterTileRules.HexToTileCell(0, 1, 1, -1, out cellX, out cellY);
			Assert.Equal(2, cellX);
			Assert.Equal(-2, cellY);
			EncounterTileRules.HexToTileCell(1, 0, 1, -1, out cellX, out cellY);
			Assert.Equal(3, cellX);
			Assert.Equal(-1, cellY);
		}

		[Fact]
		public void HexToTileCells_RightIsLeftPlusOne()
		{
			int leftX;
			int rightX;
			int cellY;
			EncounterTileRules.HexToTileCells(2, 3, 1, -1, out leftX, out rightX, out cellY);
			Assert.Equal(leftX + 1, rightX);
			Assert.Equal(-4, cellY);
			int leftOnly;
			int yOnly;
			EncounterTileRules.HexToTileCell(2, 3, 1, -1, out leftOnly, out yOnly);
			Assert.Equal(leftOnly, leftX);
			Assert.Equal(yOnly, cellY);
		}

		[Fact]
		public void TileParityA_MatchesVanillaCheckerboard()
		{
			Assert.False(EncounterTileRules.TileParityA(0, 0));
			Assert.True(EncounterTileRules.TileParityA(1, 0));
			Assert.True(EncounterTileRules.TileParityA(0, 1));
			Assert.False(EncounterTileRules.TileParityA(1, 1));
			Assert.False(EncounterTileRules.TileParityA(-1, -1));
		}

		[Fact]
		public void TileOrigin_InvertsHexToTileCell()
		{
			int originX;
			int originY;
			EncounterTileRules.TileOrigin(2, 3, 8, -4, out originX, out originY);
			int cellX;
			int cellY;
			EncounterTileRules.HexToTileCell(2, 3, originX, originY, out cellX, out cellY);
			Assert.Equal(8, cellX);
			Assert.Equal(-4, cellY);
		}

		[Fact]
		public void TryParse_V3File_HasEmptyTiles()
		{
			string json = "{\n  \"schemaVersion\": 3,\n  \"template\": \"fighttesterempty\",\n  \"walkableDefault\": false,\n  \"overrides\": [],\n  \"combatants\": []\n}\n";
			Assert.True(EncounterFileModel.TryParse(json, out EncounterFileModel parsed, out string error));
			Assert.Null(error);
			Assert.Equal(3, parsed.SchemaVersion);
			Assert.Empty(parsed.Tiles);
			Assert.True(parsed.TilesDefault);
		}
	}
}
