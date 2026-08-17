using LokrAbilityLab.Editor;
using LokrLab;
using Xunit;

namespace LokrModding.Tests.Lab
{
	public sealed class LabCatalogRulesTests
	{
		[Fact]
		[Trait("Issue", "ability-hit-closed-tag-whitelist")]
		public void ProjectileIsLegal_RangedIsNot()
		{
			Assert.True(LabCatalogRules.IsLegalHitTag("#PROJECTILE"));
			Assert.True(LabCatalogRules.IsLegalHitTag("PROJECTILE"));
			Assert.False(LabCatalogRules.IsLegalHitTag("#RANGED"));
			Assert.False(LabCatalogRules.IsLegalHitTag("#SKULL"));
		}

		[Fact]
		[Trait("Issue", "ability-aoe-range-cone-empty")]
		public void RangeCone_IsNotSelectable_AndWarns()
		{
			Assert.False(LabCatalogRules.IsSelectableAoeKind("RANGE_CONE"));
			Assert.True(LabCatalogRules.IsSelectableAoeKind("RANGE_CIRCLE"));
			Assert.True(LabCatalogRules.ShouldWarnRangeCone("RANGE_CONE"));
			Assert.False(LabCatalogRules.ShouldWarnRangeCone("RANGE_TUNNEL"));
		}

		[Fact]
		[Trait("Issue", "ability-events-never-dispatched")]
		public void DeadAbilityEvents_AreParseLegalButUnfired()
		{
			Assert.True(AbilityEventNames.IsAbilityEvent("OnAttackStart"));
			Assert.True(AbilityEventNames.IsDeadAbilityEvent("OnAttackStart"));
			Assert.False(AbilityEventNames.IsDeadAbilityEvent("OnAbilityStart"));
			Assert.Contains("OnAttackStart", AbilityEventNames.DeadAbilityEvents);
			Assert.DoesNotContain("OnAttackStart", AbilityEventNames.FiredAbilityEvents);
		}

		[Fact]
		[Trait("Issue", "lab-alias-loc-keys-not-expanded")]
		public void LocStems_UseUniqueIdNotAlias()
		{
			Assert.Equal("UNIT_assassin_z7v9v1_NAME_0001", LabCatalogRules.UnitNameLocKey("assassin_z7v9v1"));
			Assert.Equal("UNIT_assassin_z7v9v1_LORE", LabCatalogRules.UnitLoreLocKey("assassin_z7v9v1"));
			Assert.DoesNotContain("$assassin", LabCatalogRules.UnitNameLocKey("assassin_z7v9v1"));
		}
	}
}
