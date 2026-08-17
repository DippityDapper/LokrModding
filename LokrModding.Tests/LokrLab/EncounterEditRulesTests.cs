using LokrLab.Encounter;
using Xunit;

namespace LokrModding.Tests.Lab
{
	public sealed class EncounterEditRulesTests
	{
		[Fact]
		public void CanPlace_LegalHex_Succeeds()
		{
			EncounterFileModel file = EncounterFileModel.CreateEmpty();
			EncounterCombatantModel combatant = Placed("gerald_1", 6, 10);
			file.Combatants.Add(combatant);
			Assert.Null(EncounterEditRules.CanPlace(file, combatant, 7, 11, 24, 24));
		}

		[Fact]
		public void CanPlace_OwnHex_Succeeds()
		{
			EncounterFileModel file = EncounterFileModel.CreateEmpty();
			EncounterCombatantModel combatant = Placed("gerald_1", 6, 10);
			file.Combatants.Add(combatant);
			Assert.Null(EncounterEditRules.CanPlace(file, combatant, 6, 10, 24, 24));
		}

		[Fact]
		public void CanPlace_OutOfBoard_Fails()
		{
			EncounterFileModel file = EncounterFileModel.CreateEmpty();
			EncounterCombatantModel combatant = Placed("gerald_1", 6, 10);
			file.Combatants.Add(combatant);
			Assert.Contains("outside the live", EncounterEditRules.CanPlace(file, combatant, 40, 10, 24, 24));
			Assert.Contains("outside the live", EncounterEditRules.CanPlace(file, combatant, -1, 0, 24, 24));
		}

		[Fact]
		public void CanPlace_DuplicateHex_Fails()
		{
			EncounterFileModel file = EncounterFileModel.CreateEmpty();
			EncounterCombatantModel first = Placed("gerald_1", 6, 10);
			EncounterCombatantModel second = Placed("raider_1", 8, 8);
			file.Combatants.Add(first);
			file.Combatants.Add(second);
			Assert.Contains("raider_1", EncounterEditRules.CanPlace(file, first, 8, 8, 24, 24));
		}

		[Fact]
		public void CanPlace_NullCombatant_AsksToSelect()
		{
			Assert.Contains("Select a combatant", EncounterEditRules.CanPlace(
				EncounterFileModel.CreateEmpty(), null, 6, 10, 24, 24));
		}

		[Fact]
		public void CanPlace_ImpassableHex_Fails()
		{
			EncounterFileModel file = EncounterFileModel.CreateEmpty();
			EncounterCombatantModel combatant = Placed("gerald_1", 6, 10);
			file.Combatants.Add(combatant);
			Assert.Equal("Hex is not walkable.",
				EncounterEditRules.CanPlace(file, combatant, 7, 11, 24, 24, isPassable: false));
		}

		private static EncounterCombatantModel Placed(string id, int col, int row)
		{
			return new EncounterCombatantModel
			{
				Id = id,
				Side = EncounterFileModel.GoodSide,
				Source = EncounterFileModel.SourceUnit,
				UnitId = id,
				Col = col,
				Row = row
			};
		}
	}
}
