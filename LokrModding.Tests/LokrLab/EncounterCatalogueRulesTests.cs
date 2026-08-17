using System.Collections.Generic;
using LokrLab.Encounter;
using Xunit;

namespace LokrModding.Tests.Lab
{
	public sealed class EncounterCatalogueRulesTests
	{
		[Fact]
		public void EmptyQueryMatchesEverything()
		{
			Assert.True(EncounterCatalogueRules.Matches("gerald_1", "Gerald", null));
			Assert.True(EncounterCatalogueRules.Matches("gerald_1", "Gerald", "   "));
		}

		[Fact]
		public void QueryMatchesIdOrName()
		{
			Assert.True(EncounterCatalogueRules.Matches("forest_deco_bush_01", "Bush", "bush"));
			Assert.True(EncounterCatalogueRules.Matches("forest_deco_bush_01", "Bush", "FOREST"));
			Assert.True(EncounterCatalogueRules.Matches("gerald_1", "Sir Gerald", "sir"));
			Assert.False(EncounterCatalogueRules.Matches("gerald_1", "Sir Gerald", "raider"));
		}

		[Fact]
		public void NullIdStillMatchesName()
		{
			Assert.True(EncounterCatalogueRules.Matches(null, "Sir Gerald", "gerald"));
			Assert.False(EncounterCatalogueRules.Matches(null, "Sir Gerald", "bush"));
		}

		[Fact]
		public void IsLikelyVisualUnit_DropsDumpAndControlIds()
		{
			Assert.True(EncounterCatalogueRules.IsLikelyVisualUnit("gerald_1"));
			Assert.True(EncounterCatalogueRules.IsLikelyVisualUnit("BanditRaider"));
			Assert.False(EncounterCatalogueRules.IsLikelyVisualUnit(null));
			Assert.False(EncounterCatalogueRules.IsLikelyVisualUnit("#ArcaneSentry"));
			Assert.False(EncounterCatalogueRules.IsLikelyVisualUnit("DummyUnit"));
			Assert.False(EncounterCatalogueRules.IsLikelyVisualUnit("DiversifierControlUnitBase"));
			Assert.False(EncounterCatalogueRules.IsLikelyVisualUnit("SomeControlUnit"));
		}

		[Fact]
		public void SpritesheetPrefix_UsesFirstToken()
		{
			Assert.Equal("Arena", EncounterCatalogueRules.SpritesheetPrefix("Arena_Deco_Generic_PileOfTrash_01"));
			Assert.Equal("arena", EncounterCatalogueRules.SpritesheetPrefix("arena_deco_generic_pileoftrash_01"));
			Assert.Equal("Forest", EncounterCatalogueRules.SpritesheetPrefix("Forest_Deco_Bush_01"));
			Assert.Null(EncounterCatalogueRules.SpritesheetPrefix("Arena"));
			Assert.Null(EncounterCatalogueRules.SpritesheetPrefix(null));
		}

		[Fact]
		public void TexturePackerRect_FindsNamedCell()
		{
			const string packer = "w 256\nh 256\n~\nn path/Arena_Deco_Generic_PileOfTrash_01.png\ns 10 20 32 48\n~\nn Forest_Deco_Bush_01\ns 40 8 16 16\n~";
			Assert.True(EncounterCatalogueRules.TryGetTexturePackerRect(
				packer, "Arena_Deco_Generic_PileOfTrash_01", out EncounterPackerRect trash));
			Assert.Equal(10, trash.X);
			Assert.Equal(20, trash.Y);
			Assert.Equal(32, trash.Width);
			Assert.Equal(48, trash.Height);
			Assert.True(EncounterCatalogueRules.TryGetTexturePackerRect(
				packer, "forest_deco_bush_01", out EncounterPackerRect bush));
			Assert.Equal(16, bush.Width);
			Assert.False(EncounterCatalogueRules.TryGetTexturePackerRect(packer, "missing", out _));
		}

		[Fact]
		public void AddTexturePackerRects_SkipsDuplicates()
		{
			const string packer = "n One\ns 0 0 8 8\n~\nn One\ns 1 1 4 4\n~";
			Dictionary<string, EncounterPackerRect> dest = new Dictionary<string, EncounterPackerRect>();
			Assert.Equal(1, EncounterCatalogueRules.AddTexturePackerRects(packer, dest));
			Assert.Equal(8, dest["One"].Width);
		}

		[Fact]
		public void NextBatchEnd_ClampsAndSteps()
		{
			Assert.Equal(24, EncounterCatalogueRules.NextBatchEnd(0, 1000, 24));
			Assert.Equal(48, EncounterCatalogueRules.NextBatchEnd(24, 1000, 24));
			Assert.Equal(1000, EncounterCatalogueRules.NextBatchEnd(984, 1000, 24));
			Assert.Equal(5, EncounterCatalogueRules.NextBatchEnd(0, 5, 24));
			Assert.Equal(10, EncounterCatalogueRules.NextBatchEnd(10, 10, 24));
			Assert.Equal(1, EncounterCatalogueRules.NextBatchEnd(0, 10, 0));
		}
	}
}
