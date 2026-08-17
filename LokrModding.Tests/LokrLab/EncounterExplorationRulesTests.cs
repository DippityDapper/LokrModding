using System.Collections.Generic;
using LokrLab.Encounter;
using Xunit;

namespace LokrModding.Tests.Lab
{
	public sealed class EncounterExplorationRulesTests
	{
		private static EncounterExplorationRules.HexPos Hex(int q, int r)
		{
			return new EncounterExplorationRules.HexPos(q, r, -q - r);
		}

		[Fact]
		public void PocketsToAggro_TriggersWhenGoodSideWithinRadius()
		{
			List<EncounterExplorationRules.HexPos> good = new List<EncounterExplorationRules.HexPos> { Hex(2, 0) };
			Dictionary<string, List<EncounterExplorationRules.PocketMember>> parked =
				new Dictionary<string, List<EncounterExplorationRules.PocketMember>>
				{
					["orc"] = new List<EncounterExplorationRules.PocketMember>
					{
						new EncounterExplorationRules.PocketMember(Hex(0, 0), 3)
					}
				};

			HashSet<string> aggro = EncounterExplorationRules.PocketsToAggro(good, parked);
			Assert.Contains("orc", aggro);
		}

		[Fact]
		public void PocketsToAggro_DoesNotTriggerWhenOutsideRadius()
		{
			List<EncounterExplorationRules.HexPos> good = new List<EncounterExplorationRules.HexPos> { Hex(5, 0) };
			Dictionary<string, List<EncounterExplorationRules.PocketMember>> parked =
				new Dictionary<string, List<EncounterExplorationRules.PocketMember>>
				{
					["orc"] = new List<EncounterExplorationRules.PocketMember>
					{
						new EncounterExplorationRules.PocketMember(Hex(0, 0), 3)
					}
				};

			HashSet<string> aggro = EncounterExplorationRules.PocketsToAggro(good, parked);
			Assert.Empty(aggro);
		}

		[Fact]
		public void PocketsToAggro_OnlyTriggersNearPocketAmongMultiple()
		{
			List<EncounterExplorationRules.HexPos> good = new List<EncounterExplorationRules.HexPos> { Hex(0, 0) };
			Dictionary<string, List<EncounterExplorationRules.PocketMember>> parked =
				new Dictionary<string, List<EncounterExplorationRules.PocketMember>>
				{
					["near"] = new List<EncounterExplorationRules.PocketMember>
					{
						new EncounterExplorationRules.PocketMember(Hex(1, 0), 2)
					},
					["far"] = new List<EncounterExplorationRules.PocketMember>
					{
						new EncounterExplorationRules.PocketMember(Hex(20, 0), 2)
					}
				};

			HashSet<string> aggro = EncounterExplorationRules.PocketsToAggro(good, parked);
			Assert.Contains("near", aggro);
			Assert.DoesNotContain("far", aggro);
		}

		[Fact]
		public void PocketsToAggro_PerMemberRadius_ShortRangeMemberDoesNotBlockLongRangeMember()
		{
			List<EncounterExplorationRules.HexPos> good = new List<EncounterExplorationRules.HexPos> { Hex(5, 0) };
			Dictionary<string, List<EncounterExplorationRules.PocketMember>> parked =
				new Dictionary<string, List<EncounterExplorationRules.PocketMember>>
				{
					["mixed"] = new List<EncounterExplorationRules.PocketMember>
					{
						new EncounterExplorationRules.PocketMember(Hex(0, 0), 1), // short-range, out of reach
						new EncounterExplorationRules.PocketMember(Hex(0, 0), 8) // long-range, in reach
					}
				};

			HashSet<string> aggro = EncounterExplorationRules.PocketsToAggro(good, parked);
			Assert.Contains("mixed", aggro);
		}

		[Fact]
		public void PocketsToAggro_NoGoodSideHexes_NeverTriggers()
		{
			Dictionary<string, List<EncounterExplorationRules.PocketMember>> parked =
				new Dictionary<string, List<EncounterExplorationRules.PocketMember>>
				{
					["orc"] = new List<EncounterExplorationRules.PocketMember>
					{
						new EncounterExplorationRules.PocketMember(Hex(0, 0), 99)
					}
				};

			HashSet<string> aggro = EncounterExplorationRules.PocketsToAggro(
				new List<EncounterExplorationRules.HexPos>(), parked);
			Assert.Empty(aggro);
		}

		[Fact]
		public void PocketsToAggro_MultipleGoodSideUnits_AnyWithinRadiusTriggers()
		{
			List<EncounterExplorationRules.HexPos> good = new List<EncounterExplorationRules.HexPos>
			{
				Hex(20, 0), // far
				Hex(1, 0) // near
			};
			Dictionary<string, List<EncounterExplorationRules.PocketMember>> parked =
				new Dictionary<string, List<EncounterExplorationRules.PocketMember>>
				{
					["orc"] = new List<EncounterExplorationRules.PocketMember>
					{
						new EncounterExplorationRules.PocketMember(Hex(0, 0), 2)
					}
				};

			HashSet<string> aggro = EncounterExplorationRules.PocketsToAggro(good, parked);
			Assert.Contains("orc", aggro);
		}

		[Fact]
		public void PocketsToAggro_EmptyParkedPockets_ReturnsEmpty()
		{
			List<EncounterExplorationRules.HexPos> good = new List<EncounterExplorationRules.HexPos> { Hex(0, 0) };
			HashSet<string> aggro = EncounterExplorationRules.PocketsToAggro(
				good, new Dictionary<string, List<EncounterExplorationRules.PocketMember>>());
			Assert.Empty(aggro);
		}

		[Fact]
		public void PocketsToAggro_RegionMember_TriggersWhenGoodSideStepsInside()
		{
			HashSet<EncounterExplorationRules.HexPos> region = new HashSet<EncounterExplorationRules.HexPos>
			{
				Hex(5, 5),
				Hex(5, 6)
			};
			List<EncounterExplorationRules.HexPos> good = new List<EncounterExplorationRules.HexPos> { Hex(5, 6) };
			Dictionary<string, List<EncounterExplorationRules.PocketMember>> parked =
				new Dictionary<string, List<EncounterExplorationRules.PocketMember>>
				{
					["gate"] = new List<EncounterExplorationRules.PocketMember>
					{
						new EncounterExplorationRules.PocketMember(Hex(0, 0), region)
					}
				};

			HashSet<string> aggro = EncounterExplorationRules.PocketsToAggro(good, parked);
			Assert.Contains("gate", aggro);
		}

		[Fact]
		public void PocketsToAggro_RegionMember_IgnoresDistanceToOwnHex()
		{
			// Member stands far from the region (col/row 0,0), but the region itself is what matters.
			HashSet<EncounterExplorationRules.HexPos> region = new HashSet<EncounterExplorationRules.HexPos> { Hex(50, 50) };
			List<EncounterExplorationRules.HexPos> good = new List<EncounterExplorationRules.HexPos> { Hex(50, 50) };
			Dictionary<string, List<EncounterExplorationRules.PocketMember>> parked =
				new Dictionary<string, List<EncounterExplorationRules.PocketMember>>
				{
					["gate"] = new List<EncounterExplorationRules.PocketMember>
					{
						new EncounterExplorationRules.PocketMember(Hex(0, 0), region)
					}
				};

			HashSet<string> aggro = EncounterExplorationRules.PocketsToAggro(good, parked);
			Assert.Contains("gate", aggro);
		}

		[Fact]
		public void PocketsToAggro_RegionMember_NoTriggerWhenGoodSideOutsideRegion()
		{
			HashSet<EncounterExplorationRules.HexPos> region = new HashSet<EncounterExplorationRules.HexPos> { Hex(5, 5) };
			List<EncounterExplorationRules.HexPos> good = new List<EncounterExplorationRules.HexPos> { Hex(5, 6) };
			Dictionary<string, List<EncounterExplorationRules.PocketMember>> parked =
				new Dictionary<string, List<EncounterExplorationRules.PocketMember>>
				{
					["gate"] = new List<EncounterExplorationRules.PocketMember>
					{
						new EncounterExplorationRules.PocketMember(Hex(5, 5), region)
					}
				};

			HashSet<string> aggro = EncounterExplorationRules.PocketsToAggro(good, parked);
			Assert.Empty(aggro);
		}

		[Fact]
		public void PocketsToAggro_MixedPocket_RegionMemberWakesEvenWhenRadiusMemberDoesNot()
		{
			HashSet<EncounterExplorationRules.HexPos> region = new HashSet<EncounterExplorationRules.HexPos> { Hex(9, 9) };
			List<EncounterExplorationRules.HexPos> good = new List<EncounterExplorationRules.HexPos> { Hex(9, 9) };
			Dictionary<string, List<EncounterExplorationRules.PocketMember>> parked =
				new Dictionary<string, List<EncounterExplorationRules.PocketMember>>
				{
					["mixed"] = new List<EncounterExplorationRules.PocketMember>
					{
						new EncounterExplorationRules.PocketMember(Hex(0, 0), 1), // radius member, out of reach
						new EncounterExplorationRules.PocketMember(Hex(0, 0), region) // region member, in reach
					}
				};

			HashSet<string> aggro = EncounterExplorationRules.PocketsToAggro(good, parked);
			Assert.Contains("mixed", aggro);
		}

		[Fact]
		public void PocketsToAggro_PocketRegion_WakesWholePocketRegardlessOfMemberConditions()
		{
			// Members have no chance of triggering on their own (radius 0, far away); only the
			// trigger's own pocket-targeted region should wake them.
			HashSet<EncounterExplorationRules.HexPos> pocketRegion = new HashSet<EncounterExplorationRules.HexPos> { Hex(20, 20) };
			List<EncounterExplorationRules.HexPos> good = new List<EncounterExplorationRules.HexPos> { Hex(20, 20) };
			Dictionary<string, List<EncounterExplorationRules.PocketMember>> parked =
				new Dictionary<string, List<EncounterExplorationRules.PocketMember>>
				{
					["guards"] = new List<EncounterExplorationRules.PocketMember>
					{
						new EncounterExplorationRules.PocketMember(Hex(0, 0), 0)
					}
				};
			Dictionary<string, List<HashSet<EncounterExplorationRules.HexPos>>> pocketRegions =
				new Dictionary<string, List<HashSet<EncounterExplorationRules.HexPos>>>
				{
					["guards"] = new List<HashSet<EncounterExplorationRules.HexPos>> { pocketRegion }
				};

			HashSet<string> aggro = EncounterExplorationRules.PocketsToAggro(good, parked, pocketRegions);
			Assert.Contains("guards", aggro);
		}

		[Fact]
		public void PocketsToAggro_PocketRegion_NoTriggerWhenGoodSideOutsideRegion()
		{
			// GoodSide sits away from both the region (20,20) and the member's own hex (0,0), so
			// neither the direct pocket-region check nor the member's own radius=0 check can fire.
			HashSet<EncounterExplorationRules.HexPos> pocketRegion = new HashSet<EncounterExplorationRules.HexPos> { Hex(20, 20) };
			List<EncounterExplorationRules.HexPos> good = new List<EncounterExplorationRules.HexPos> { Hex(9, 9) };
			Dictionary<string, List<EncounterExplorationRules.PocketMember>> parked =
				new Dictionary<string, List<EncounterExplorationRules.PocketMember>>
				{
					["guards"] = new List<EncounterExplorationRules.PocketMember>
					{
						new EncounterExplorationRules.PocketMember(Hex(0, 0), 0)
					}
				};
			Dictionary<string, List<HashSet<EncounterExplorationRules.HexPos>>> pocketRegions =
				new Dictionary<string, List<HashSet<EncounterExplorationRules.HexPos>>>
				{
					["guards"] = new List<HashSet<EncounterExplorationRules.HexPos>> { pocketRegion }
				};

			HashSet<string> aggro = EncounterExplorationRules.PocketsToAggro(good, parked, pocketRegions);
			Assert.Empty(aggro);
		}

		[Fact]
		public void PocketsToAggro_PocketRegion_IgnoredForUntrackedPocket()
		{
			// A trigger names a pocket that isn't (or is no longer) tracked as parked — no-op, no crash.
			HashSet<EncounterExplorationRules.HexPos> pocketRegion = new HashSet<EncounterExplorationRules.HexPos> { Hex(0, 0) };
			List<EncounterExplorationRules.HexPos> good = new List<EncounterExplorationRules.HexPos> { Hex(0, 0) };
			Dictionary<string, List<HashSet<EncounterExplorationRules.HexPos>>> pocketRegions =
				new Dictionary<string, List<HashSet<EncounterExplorationRules.HexPos>>>
				{
					["ghost_pocket"] = new List<HashSet<EncounterExplorationRules.HexPos>> { pocketRegion }
				};

			HashSet<string> aggro = EncounterExplorationRules.PocketsToAggro(
				good, new Dictionary<string, List<EncounterExplorationRules.PocketMember>>(), pocketRegions);
			Assert.Empty(aggro);
		}
	}
}
