using LokrAbilityLab.Editor;
using Xunit;

namespace LokrModding.Tests.Lab
{
	public sealed class AbilityPickerRulesTests
	{
		[Fact]
		public void HitTarget_OmitsBossTokens_KeepsCore()
		{
			string[] dump =
			{
				"%TARGET",
				"%CASTER",
				"%drainSource",
				"%eye",
				"hexNeighbour(unitHex(%CASTER), unitHex(%TARGET), 1)",
				"activeUnit()",
			};

			string[] filtered = AbilityPickerRules.FilterUnitRefs(dump, "Hit", "Target", null);

			Assert.Contains("%TARGET", filtered);
			Assert.Contains("%CASTER", filtered);
			Assert.Contains("activeUnit()", filtered);
			Assert.DoesNotContain("%drainSource", filtered);
			Assert.DoesNotContain("%eye", filtered);
			Assert.DoesNotContain("hexNeighbour(unitHex(%CASTER), unitHex(%TARGET), 1)", filtered);
		}

		[Fact]
		public void HitTarget_KeepsLoadedBossToken()
		{
			string[] dump = { "%TARGET", "%drainSource" };
			string[] filtered = AbilityPickerRules.FilterUnitRefs(dump, "Hit", "Target", "%drainSource");
			Assert.Contains("%drainSource", filtered);
			Assert.Equal("%drainSource", filtered[0]);
		}

		[Fact]
		public void KnockbackCenter_AllowsKnockbackCenterToken()
		{
			string[] dump = { "%TARGET", "%knockbackCenter", "%drainSource" };
			string[] filtered = AbilityPickerRules.FilterUnitRefs(dump, "Knockback", "Center", null);
			Assert.Contains("%knockbackCenter", filtered);
			Assert.Contains("%TARGET", filtered);
			Assert.DoesNotContain("%drainSource", filtered);
		}

		[Fact]
		public void KeepCurrent_PrependsMissingValue()
		{
			string[] kept = AbilityPickerRules.KeepCurrent(new[] { "%TARGET" }, "%CASTER");
			Assert.Equal(new[] { "%CASTER", "%TARGET" }, kept);
		}
	}

	public sealed class AbilityLuaRulesTests
	{
		[Fact]
		public void FlattenForKv_CollapsesNewlines()
		{
			string flat = AbilityLuaRules.FlattenForKv("return function(ctx)\n  ctx.GetObject('SOURCE')\nend");
			Assert.DoesNotContain("\n", flat);
			Assert.Contains("return function(ctx)", flat);
			Assert.Contains("end", flat);
		}

		[Fact]
		public void ContainsDoubleQuote_DetectsIllegalKv()
		{
			Assert.True(AbilityLuaRules.ContainsDoubleQuote("return \"nope\""));
			Assert.False(AbilityLuaRules.ContainsDoubleQuote(AbilityLuaRules.DefaultAction));
		}
	}

	public sealed class AbilityHoverCopyTests
	{
		[Fact]
		public void ParseMarkdown_ReadsTitleAndBody()
		{
			string markdown = "## field.Hit.Target\nHit Target\nUnit that receives this hit.\n";
			System.Collections.Generic.Dictionary<string, AbilityHoverCopy.HoverEntry> parsed =
				AbilityHoverCopy.ParseMarkdown(markdown);
			Assert.True(parsed.ContainsKey("field.Hit.Target"));
			Assert.Equal("Hit Target", parsed["field.Hit.Target"].Title);
			Assert.Contains("receives this hit", parsed["field.Hit.Target"].Body);
		}

		[Fact]
		public void Format_AppendsTokenCopy()
		{
			AbilityHoverCopy.Reload();
			string body = AbilityHoverCopy.Format("field.Hit.Target", "%CASTER", out string title);
			Assert.Equal("Hit Target", title);
			Assert.Contains("%CASTER", body);
			Assert.Contains("cast this ability", body);
		}

		[Fact]
		public void ApplyMarkdown_OverridesDefault()
		{
			AbilityHoverCopy.Reload();
			AbilityHoverCopy.ApplyMarkdown("## field.Hit.Target\nHit Target\nOverlay body.\n");
			string body = AbilityHoverCopy.Format("field.Hit.Target", null, out _);
			Assert.Equal("Overlay body.", body);
		}

		[Fact]
		public void Format_UsesCharacterFallback()
		{
			AbilityHoverCopy.Reload();
			string body = AbilityHoverCopy.Format("character.roster.Tier", null, out string title);
			Assert.Equal("Legend (roster)", title);
			Assert.Contains("LEGEND", body);
		}

		[Fact]
		public void Format_DeadEventSaysUnfired()
		{
			AbilityHoverCopy.Reload();
			string body = AbilityHoverCopy.Format("event.OnAttackStart", null, out _);
			Assert.Contains("never fires", body);
		}

		[Fact]
		public void Format_RootMotionEmptyClearsClip()
		{
			AbilityHoverCopy.Reload();
			string body = AbilityHoverCopy.Format("animator.frame.RootMotionX", null, out _);
			Assert.Contains("whole clip", body);
		}

		[Fact]
		public void Format_UsesEncounterFallback()
		{
			AbilityHoverCopy.Reload();
			string body = AbilityHoverCopy.Format("encounter.template", null, out string title);
			Assert.Equal("Template", title);
			Assert.Contains("combat_bridge", body);
			Assert.Contains("GoodSide", AbilityHoverCopy.Format("encounter.combatant.Side", null, out _));
		}
	}
}
