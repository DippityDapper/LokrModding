using LokrLab;
using Xunit;

namespace LokrModding.Tests.Lab
{
	public sealed class ExoImportRulesTests
	{
		[Fact]
		public void InferModelFromMetaExo_StripsPrefixAndSuffix()
		{
			Assert.Equal(
				"HumanGeraldLightSeeker",
				ExoImportRules.InferModelFromMetaExo("ExoSkeletonHumanGeraldLightSeeker_MetaDataAsset"));
			Assert.Equal(string.Empty, ExoImportRules.InferModelFromMetaExo("HumanGeraldLightSeeker"));
			Assert.Equal(string.Empty, ExoImportRules.InferModelFromMetaExo(null));
		}

		[Fact]
		public void PreferPrefabExo_WhenItHasAtLeastAsManyClips()
		{
			Assert.True(ExoImportRules.PreferPrefabExo(22, 5));
			Assert.True(ExoImportRules.PreferPrefabExo(5, 5));
			Assert.False(ExoImportRules.PreferPrefabExo(0, 5));
			Assert.False(ExoImportRules.PreferPrefabExo(4, 5));
		}

		[Fact]
		public void JsonHasCombatClip_IgnoresMapOnlyRigs()
		{
			Assert.False(ExoImportRules.JsonHasCombatClip(
				"{\"parts\":[],\"animations\":[{\"name\":\"Stand\"},{\"name\":\"Portrait\"},{\"name\":\"Victory\"}]}"));
			Assert.True(ExoImportRules.JsonHasCombatClip(
				"{\"animations\":[{\"name\":\"Stand\"},{\"name\":\"Walk\"}]}"));
			Assert.True(ExoImportRules.JsonHasCombatClip(
				"{\"animations\":[{\"name\":\"Attack\"}]}"));
			Assert.False(ExoImportRules.JsonHasCombatClip(""));
		}
	}
}
