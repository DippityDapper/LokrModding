using LokrLab.Encounter;
using Xunit;

namespace LokrModding.Tests.Lab
{
	public sealed class EncounterPlacementRulesTests
	{
		[Fact]
		public void LiveSize_FightTesterEmpty_Is24By24()
		{
			EncounterPlacementRules.LiveSize("fighttesterempty", out int width, out int height);
			Assert.Equal(24, width);
			Assert.Equal(24, height);
		}

		[Fact]
		public void ClampCoord_PinsToLiveBoard()
		{
			Assert.Equal(0, EncounterPlacementRules.ClampCoord(-3, 24));
			Assert.Equal(23, EncounterPlacementRules.ClampCoord(100, 24));
			Assert.Equal(6, EncounterPlacementRules.ClampCoord(6, 24));
		}

		[Fact]
		public void ClampToLiveBoard_ClampsAuthoredHex()
		{
			EncounterCombatantModel combatant = new EncounterCombatantModel
			{
				Id = "raider_1",
				Col = 100,
				Row = -2
			};
			EncounterPlacementRules.ClampToLiveBoard(combatant, "fighttesterempty");
			Assert.Equal(23, combatant.Col);
			Assert.Equal(0, combatant.Row);
		}

		[Fact]
		public void Warning_ReportsDuplicateHex()
		{
			EncounterFileModel file = EncounterFileModel.CreateEmpty();
			EncounterCombatantModel first = new EncounterCombatantModel
			{
				Id = "gerald_1",
				Col = 6,
				Row = 10
			};
			EncounterCombatantModel second = new EncounterCombatantModel
			{
				Id = "raider_1",
				Col = 6,
				Row = 10
			};
			file.Combatants.Add(first);
			file.Combatants.Add(second);
			Assert.Equal("raider_1", EncounterPlacementRules.DuplicateHexId(file, first));
			Assert.Contains("raider_1", EncounterPlacementRules.Warning(file, first));
		}

		[Fact]
		public void Warning_UnsetPlacement_IsSilent()
		{
			EncounterFileModel file = EncounterFileModel.CreateEmpty();
			EncounterCombatantModel combatant = new EncounterCombatantModel { Id = "raider_1" };
			file.Combatants.Add(combatant);
			Assert.Null(EncounterPlacementRules.Warning(file, combatant));
		}

		[Fact]
		public void TryParseCoord_EmptyClears()
		{
			Assert.True(EncounterPlacementRules.TryParseCoord("  ", out int? value));
			Assert.False(value.HasValue);
			Assert.False(EncounterPlacementRules.TryParseCoord("hex", out _));
		}
	}
}
