using System.Collections.Generic;
using System.IO;
using LokrCharacterLoader;
using Xunit;

namespace LokrModding.Tests.CharacterLoader
{
	public sealed class LabAliasesTests
	{
		[Fact]
		[Trait("Issue", "lab-alias-loc-keys-not-expanded")]
		public void Expand_GreedyLocKey_KeepsNameSuffix()
		{
			Dictionary<string, string> map = new Dictionary<string, string>
			{
				{ "assassin", "assassin_z7v9v1" }
			};

			string expanded = LabAliases.Expand("UNIT_$assassin_NAME_0001", map);
			Assert.Equal("UNIT_assassin_z7v9v1_NAME_0001", expanded);
		}

		[Fact]
		public void Expand_UnknownAlias_LeftAlone()
		{
			Dictionary<string, string> map = new Dictionary<string, string>
			{
				{ "assassin", "assassin_z7v9v1" }
			};

			Assert.Equal("$other", LabAliases.Expand("$other", map));
		}

		[Fact]
		public void LoadSave_RoundTrip()
		{
			string folder = Path.Combine(Path.GetTempPath(), "lokr-alias-" + Path.GetRandomFileName());
			try
			{
				Dictionary<string, string> map = new Dictionary<string, string>
				{
					{ "assassin", "assassin_z7v9v1" }
				};
				LabAliases.Save(folder, map);
				Dictionary<string, string> loaded = LabAliases.Load(folder);
				Assert.Equal("assassin_z7v9v1", loaded["assassin"]);
			}
			finally
			{
				if (Directory.Exists(folder))
				{
					Directory.Delete(folder, true);
				}
			}
		}
	}

	public sealed class LabExpressionIdsTests
	{
		[Fact]
		[Trait("Issue", "alias-unitname-parsed-as-function")]
		public void RewriteAbilityText_BareUnitName_GetsHash()
		{
			string rewritten = LabExpressionIds.RewriteAbilityText("\"UnitName\" \"onagro_mine_6htjnq\"");
			Assert.Contains("\"UnitName\" \"#onagro_mine_6htjnq\"", rewritten);
		}

		[Fact]
		[Trait("Issue", "alias-unitname-parsed-as-function")]
		public void RewriteAbilityText_LeavesDollarAlias()
		{
			string source = "\"UnitName\" \"$assassin\"";
			Assert.Equal(source, LabExpressionIds.RewriteAbilityText(source));
		}

		[Fact]
		public void RewriteAbilityText_DigitHash_GetsCPrefix()
		{
			string rewritten = LabExpressionIds.RewriteAbilityText("\"UnitName\" \"#152912345678901234\"");
			Assert.Contains("#c152912345678901234", rewritten);
		}
	}

	public sealed class AbilityKvFixtureTests
	{
		[Fact]
		[Trait("Issue", "ability-kv-parse-empty-filename")]
		public void OfficialPackMalformedAoeTeamFilter_IsCorrupt()
		{
			const string malformed = "\"AbilityAOETeamFilter   \"\t\"TEAM_ALL\"\"";
			Assert.True(LokrLab.LabCatalogRules.LooksLikeCorruptAbilityKv(malformed));
		}

		[Fact]
		public void CleanAoeTeamFilter_IsNotCorrupt()
		{
			Assert.False(LokrLab.LabCatalogRules.LooksLikeCorruptAbilityKv("\"AbilityAOETeamFilter\"\t\"TEAM_ALL\""));
		}
	}

	public sealed class ContentRulesTests
	{
		[Fact]
		[Trait("Issue", "skills-bar-five-slot-cap")]
		public void TrimListToCap_DropsExtrasPastFive()
		{
			List<int> skills = new List<int> { 1, 2, 3, 4, 5, 6, 7 };
			ContentRules.TrimListToCap(skills, 5);
			Assert.Equal(new[] { 1, 2, 3, 4, 5 }, skills);
		}

		[Fact]
		[Trait("Issue", "skills-bar-five-slot-cap")]
		public void TrimListToCap_ZeroCap_DoesNotTrim()
		{
			List<int> skills = new List<int> { 1, 2 };
			ContentRules.TrimListToCap(skills, 0);
			Assert.Equal(2, skills.Count);
		}

		[Fact]
		[Trait("Issue", "invisibility-exit-fires-every-turn")]
		public void InvisibilityExit_OnlyOnTrueToFalseEdge()
		{
			Assert.False(ContentRules.ShouldRaiseInvisibilityExit(false, false));
			Assert.True(ContentRules.ShouldRaiseInvisibilityExit(true, false));
			Assert.False(ContentRules.ShouldRaiseInvisibilityExit(true, true));
			Assert.False(ContentRules.ShouldRaiseInvisibilityEnter(true, true));
			Assert.True(ContentRules.ShouldRaiseInvisibilityEnter(false, true));
		}

		[Fact]
		[Trait("Issue", "find-part-index-unvalidated")]
		public void ShouldWritePartAtIndex_SkipsMinusOne()
		{
			Assert.False(ContentRules.ShouldWritePartAtIndex(-1));
			Assert.True(ContentRules.ShouldWritePartAtIndex(0));
		}

		[Fact]
		[Trait("Issue", "exo-skeleton-null-unitdefinition")]
		public void ShouldSkipExoResolve_WhenDefinitionNull()
		{
			Assert.True(ContentRules.ShouldSkipExoResolveForNullDefinition(true));
			Assert.False(ContentRules.ShouldSkipExoResolveForNullDefinition(false));
		}

		[Fact]
		[Trait("Issue", "reload-data-missing-sprite-nre")]
		public void MissingPackedSprite_DoesNotMatchPart()
		{
			Assert.False(ContentRules.AnyPackedSpriteMatchesPart(new[] { "Head", "Arm" }, "MissingPart"));
			Assert.True(ContentRules.PackedSpriteNameMatchesPart("atlas#Head", "Head"));
			Assert.True(ContentRules.AnyPackedSpriteMatchesPart(new[] { "atlas#Head" }, "Head"));
		}

		[Fact]
		[Trait("Issue", "portrait-patches-buff-store-index")]
		public void BuffStoreHeroPosition_OutOfRange()
		{
			Assert.False(ContentRules.IsHeroPositionInRange(3, 3));
			Assert.False(ContentRules.IsHeroPositionInRange(0, 0));
			Assert.True(ContentRules.IsHeroPositionInRange(2, 3));
		}

		[Fact]
		[Trait("Issue", "party-stow-shifts-remaining-into-wrong-slots")]
		public void ShouldSkipPortraitResolve_NullOrEmpty()
		{
			Assert.True(ContentRules.ShouldSkipPortraitResolve(null));
			Assert.True(ContentRules.ShouldSkipPortraitResolve(""));
			Assert.False(ContentRules.ShouldSkipPortraitResolve("Ranger"));
		}

		[Fact]
		[Trait("Issue", "legacy-pack-and-lab-import-both-roster")]
		public void MergeUniqueIds_KeepsBothSources()
		{
			List<string> merged = ContentRules.MergeUniqueIds(
				new[] { "Gerald", "Assassin" },
				new[] { "onagro_mine_6htjnq" });
			Assert.Equal(new[] { "Gerald", "Assassin", "onagro_mine_6htjnq" }, merged);
		}

		[Fact]
		public void AssignLastWins_ReplacesExistingKey()
		{
			Dictionary<string, string> dest = new Dictionary<string, string>
			{
				{ "RLHumanGeraldLightSeekerLvl1", "vanilla" }
			};

			Assert.True(ContentRules.AssignLastWins(dest, "RLHumanGeraldLightSeekerLvl1", "lab"));
			Assert.Equal("lab", dest["RLHumanGeraldLightSeekerLvl1"]);
			Assert.False(ContentRules.AssignLastWins(dest, "RLHumanGeraldLightSeekerLvl2", "lab"));
			Assert.Equal("lab", dest["RLHumanGeraldLightSeekerLvl2"]);
		}

		[Fact]
		public void AssignLevel1UniqueIdLastWins_IgnoresNonLevel1()
		{
			Dictionary<string, string> index = new Dictionary<string, string>();
			Assert.False(ContentRules.AssignLevel1UniqueIdLastWins(
				index, "Gerald", "RLHumanGeraldLightSeekerLvl2", 2, out string previous));
			Assert.Null(previous);
			Assert.Empty(index);
		}

		[Fact]
		public void AssignLevel1UniqueIdLastWins_LaterBlockReplaces()
		{
			Dictionary<string, string> index = new Dictionary<string, string>();
			Assert.False(ContentRules.AssignLevel1UniqueIdLastWins(
				index, "Gerald", "RLHumanGeraldLightSeekerLvl1", 1, out _));
			Assert.True(ContentRules.AssignLevel1UniqueIdLastWins(
				index, "Gerald", "RLHumanGeraldLightSeekerLvl1_lab", 1, out string previous));
			Assert.Equal("RLHumanGeraldLightSeekerLvl1", previous);
			Assert.Equal("RLHumanGeraldLightSeekerLvl1_lab", index["Gerald"]);
		}

		[Fact]
		public void MergeRosterArray_ReplacesVanillaGeraldAndAppendsNew()
		{
			const string vanilla = @"[
		{
			""id"" : ""Gerald""
		},
		{
			""id"" : ""Asra"",
			""locked"" : true
		}
	]";
			string merged = ContentRules.MergeRosterArray(
				vanilla,
				new[] { "{\"id\":\"Gerald\",\"locked\":false}", "{\"id\":\"onagro_mine_6htjnq\"}" });

			Assert.Contains("\"id\":\"Gerald\"", merged);
			Assert.Contains("\"locked\":false", merged);
			Assert.DoesNotContain("\"id\" : \"Gerald\"", merged);
			Assert.Contains("Asra", merged);
			Assert.Contains("onagro_mine_6htjnq", merged);
			Assert.Equal("Gerald", ContentRules.ReadRosterId("{\"id\":\"Gerald\",\"locked\":false}"));
		}

		[Fact]
		public void ExtractJsonObjects_SkipsBracesInsideStrings()
		{
			List<string> objects = ContentRules.ExtractJsonObjects(
				"[ {\"id\":\"A\",\"note\":\"{not an object}\"}, {\"id\":\"B\"} ]");
			Assert.Equal(2, objects.Count);
			Assert.Contains("A", objects[0]);
			Assert.Contains("B", objects[1]);
		}
	}
}
