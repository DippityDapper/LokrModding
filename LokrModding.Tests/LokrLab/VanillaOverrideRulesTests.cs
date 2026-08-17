using System.Collections.Generic;
using LokrLab;
using Xunit;

namespace LokrModding.Tests.Lab
{
	public sealed class VanillaOverrideRulesTests
	{
		[Fact]
		public void IsOverride_RequiresUniqueId()
		{
			Assert.False(VanillaOverrideRules.IsOverride(null));
			Assert.False(VanillaOverrideRules.IsOverride(""));
			Assert.True(VanillaOverrideRules.IsOverride("Gerald"));
		}

		[Fact]
		public void LocStem_PrefersShippedNameField()
		{
			Assert.Equal("GERALD_LIGHTSEEKER", VanillaOverrideRules.LocStem("GERALD_LIGHTSEEKER", "Gerald"));
			Assert.Equal("Gerald", VanillaOverrideRules.LocStem("", "Gerald"));
		}

		[Fact]
		public void BlockKeyAt_UsesVanillaListThenFallback()
		{
			List<string> keys = new List<string>
			{
				"RLHumanGeraldLightSeekerLvl1",
				"RLHumanGeraldLightSeekerLvl2"
			};
			Assert.Equal("RLHumanGeraldLightSeekerLvl1", VanillaOverrideRules.BlockKeyAt(keys, 0, "Gerald"));
			Assert.Equal("RLHumanGeraldLightSeekerLvl2", VanillaOverrideRules.BlockKeyAt(keys, 1, "Gerald"));
			Assert.Equal("Gerald_Lvl3", VanillaOverrideRules.BlockKeyAt(keys, 2, "Gerald"));
		}

		[Fact]
		public void EngineIds_StayVanillaOnOverride()
		{
			Assert.Equal("Gerald", VanillaOverrideRules.EngineUniqueId("Gerald", "gerald_ab12cd"));
			Assert.Equal(
				"ExoSkeletonHumanGeraldLightSeeker_MetaDataAsset",
				VanillaOverrideRules.EngineMetaExo(
					"Gerald",
					"ExoSkeletonHumanGeraldLightSeeker_MetaDataAsset",
					"gerald_ab12cd",
					labRigPresent: false));
			Assert.Equal("gerald_ab12cd", VanillaOverrideRules.EngineUniqueId("", "gerald_ab12cd"));
		}

		[Fact]
		public void EngineMetaExo_UsesFolderWhenLabRigPresent()
		{
			Assert.Equal(
				"gerald_ab12cd",
				VanillaOverrideRules.EngineMetaExo(
					"Gerald",
					"ExoSkeletonHumanGeraldLightSeeker_MetaDataAsset",
					"gerald_ab12cd",
					labRigPresent: true));
		}

		[Fact]
		public void FolderClaimsUniqueId_MatchesAnyOverrideSignal()
		{
			Assert.True(VanillaOverrideRules.FolderClaimsUniqueId("Gerald", "gerald_lab_override", null, "Gerald"));
			Assert.True(VanillaOverrideRules.FolderClaimsUniqueId(null, "Gerald", null, "Gerald"));
			Assert.True(VanillaOverrideRules.FolderClaimsUniqueId(null, null, "Gerald", "Gerald"));
			Assert.False(VanillaOverrideRules.FolderClaimsUniqueId(null, "asra_ab12cd", "Asra", "Gerald"));
		}
	}
}
