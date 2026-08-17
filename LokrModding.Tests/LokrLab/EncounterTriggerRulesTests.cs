using System.Collections.Generic;
using LokrLab.Encounter;
using Xunit;

namespace LokrModding.Tests.Lab
{
	public sealed class EncounterTriggerRulesTests
	{
		[Fact]
		public void Add_MintsCatalogRow()
		{
			EncounterFileModel file = EncounterFileModel.CreateEmpty();
			Assert.True(EncounterTriggerRules.Add(file, "gate", "guards"));
			EncounterTriggerModel trigger = EncounterTriggerRules.Find(file, "gate");
			Assert.NotNull(trigger);
			Assert.Equal("guards", trigger.PocketKey);
		}

		[Fact]
		public void Add_RejectsDuplicateOrIllegalId()
		{
			EncounterFileModel file = EncounterFileModel.CreateEmpty();
			Assert.True(EncounterTriggerRules.Add(file, "gate", ""));
			Assert.False(EncounterTriggerRules.Add(file, "gate", ""));
			Assert.False(EncounterTriggerRules.Add(file, "1bad", ""));
		}

		[Fact]
		public void Rename_CascadesToCellsAndCombatants()
		{
			EncounterFileModel file = EncounterFileModel.CreateEmpty();
			EncounterTriggerRules.Add(file, "gate", "");
			EncounterTriggerRules.Set(file, 1, 1, "gate");
			file.Combatants.Add(new EncounterCombatantModel
			{
				Id = "orc_1",
				Side = EncounterFileModel.BadSide,
				Source = EncounterFileModel.SourceUnit,
				UnitId = "BanditRaider",
				TriggerId = "gate"
			});

			Assert.True(EncounterTriggerRules.Rename(file, "gate", "doorway"));
			Assert.Null(EncounterTriggerRules.Find(file, "gate"));
			Assert.NotNull(EncounterTriggerRules.Find(file, "doorway"));
			Assert.True(EncounterTriggerRules.HasCell(file, 1, 1, "doorway"));
			Assert.Equal("doorway", file.Combatants[0].TriggerId);
		}

		[Fact]
		public void Rename_RejectsIllegalOrCollidingId()
		{
			EncounterFileModel file = EncounterFileModel.CreateEmpty();
			EncounterTriggerRules.Add(file, "gate", "");
			EncounterTriggerRules.Add(file, "doorway", "");
			Assert.False(EncounterTriggerRules.Rename(file, "gate", "doorway"));
			Assert.False(EncounterTriggerRules.Rename(file, "gate", "1bad"));
			Assert.False(EncounterTriggerRules.Rename(file, "missing", "new_id"));
		}

		[Fact]
		public void RemoveDefinition_DropsCatalogCellsAndCombatantReferences()
		{
			EncounterFileModel file = EncounterFileModel.CreateEmpty();
			EncounterTriggerRules.Add(file, "gate", "");
			EncounterTriggerRules.Set(file, 1, 1, "gate");
			file.Combatants.Add(new EncounterCombatantModel
			{
				Id = "orc_1",
				Side = EncounterFileModel.BadSide,
				Source = EncounterFileModel.SourceUnit,
				UnitId = "BanditRaider",
				TriggerId = "gate"
			});

			Assert.True(EncounterTriggerRules.RemoveDefinition(file, "gate"));
			Assert.Null(EncounterTriggerRules.Find(file, "gate"));
			Assert.Empty(EncounterTriggerRules.HexesFor(file, "gate"));
			Assert.Equal(string.Empty, file.Combatants[0].TriggerId);
		}

		[Fact]
		public void CombatantsUsing_ReturnsIndividualOptInsOnly()
		{
			EncounterFileModel file = EncounterFileModel.CreateEmpty();
			file.Combatants.Add(new EncounterCombatantModel
			{
				Id = "orc_1", Side = EncounterFileModel.BadSide, Source = EncounterFileModel.SourceUnit,
				UnitId = "BanditRaider", TriggerId = "gate"
			});
			file.Combatants.Add(new EncounterCombatantModel
			{
				Id = "orc_2", Side = EncounterFileModel.BadSide, Source = EncounterFileModel.SourceUnit,
				UnitId = "BanditRaider"
			});

			List<EncounterCombatantModel> using_ = EncounterTriggerRules.CombatantsUsing(file, "gate");
			Assert.Single(using_);
			Assert.Equal("orc_1", using_[0].Id);
		}

		[Fact]
		public void PocketMembers_ReturnsBadSideCombatantsInTargetPocket()
		{
			EncounterFileModel file = EncounterFileModel.CreateEmpty();
			EncounterTriggerModel trigger = new EncounterTriggerModel { Id = "gate", PocketKey = "guards" };
			file.Combatants.Add(new EncounterCombatantModel
			{
				Id = "orc_1", Side = EncounterFileModel.BadSide, Source = EncounterFileModel.SourceUnit,
				UnitId = "BanditRaider", Pocket = "guards"
			});
			file.Combatants.Add(new EncounterCombatantModel
			{
				Id = "orc_2", Side = EncounterFileModel.BadSide, Source = EncounterFileModel.SourceUnit,
				UnitId = "BanditRaider", Pocket = "other"
			});
			file.Combatants.Add(new EncounterCombatantModel
			{
				Id = "hero_1", Side = EncounterFileModel.GoodSide, Source = EncounterFileModel.SourceUnit,
				UnitId = "Musketeer"
			});

			List<EncounterCombatantModel> members = EncounterTriggerRules.PocketMembers(file, trigger);
			Assert.Single(members);
			Assert.Equal("orc_1", members[0].Id);
		}

		[Fact]
		public void PocketMembers_SoloMemberMatchesByOwnId()
		{
			EncounterFileModel file = EncounterFileModel.CreateEmpty();
			EncounterTriggerModel trigger = new EncounterTriggerModel { Id = "gate", PocketKey = "orc_1" };
			file.Combatants.Add(new EncounterCombatantModel
			{
				Id = "orc_1", Side = EncounterFileModel.BadSide, Source = EncounterFileModel.SourceUnit,
				UnitId = "BanditRaider"
			});

			List<EncounterCombatantModel> members = EncounterTriggerRules.PocketMembers(file, trigger);
			Assert.Single(members);
		}

		[Fact]
		public void MintTriggerId_IncrementsUntilUnused()
		{
			EncounterFileModel file = EncounterFileModel.CreateEmpty();
			EncounterTriggerRules.Add(file, "trigger_1", "");
			Assert.Equal("trigger_2", EncounterTriggerRules.MintTriggerId(file));
		}

		[Fact]
		public void Cell_SetIsIdempotentForSameTrigger()
		{
			EncounterFileModel file = EncounterFileModel.CreateEmpty();
			EncounterTriggerRules.Set(file, 4, 5, "gate");
			EncounterTriggerRules.Set(file, 4, 5, "gate");
			Assert.Single(file.TriggerCells);
			Assert.True(EncounterTriggerRules.HasCell(file, 4, 5, "gate"));
		}

		[Fact]
		public void Cell_DifferentTriggersOnSameHexOverlapWithoutDisturbingEachOther()
		{
			EncounterFileModel file = EncounterFileModel.CreateEmpty();
			EncounterTriggerRules.Set(file, 4, 5, "gate");
			EncounterTriggerRules.Set(file, 4, 5, "other");
			Assert.Equal(2, file.TriggerCells.Count);
			Assert.True(EncounterTriggerRules.HasCell(file, 4, 5, "gate"));
			Assert.True(EncounterTriggerRules.HasCell(file, 4, 5, "other"));

			Assert.True(EncounterTriggerRules.Clear(file, 4, 5, "gate"));
			Assert.False(EncounterTriggerRules.HasCell(file, 4, 5, "gate"));
			Assert.True(EncounterTriggerRules.HasCell(file, 4, 5, "other"));
		}

		[Fact]
		public void Cell_ClearRemovesOnlyThatHexAndThatTrigger()
		{
			EncounterFileModel file = EncounterFileModel.CreateEmpty();
			EncounterTriggerRules.Set(file, 4, 5, "gate");
			EncounterTriggerRules.Set(file, 4, 6, "gate");
			Assert.True(EncounterTriggerRules.Clear(file, 4, 5, "gate"));
			Assert.False(EncounterTriggerRules.HasCell(file, 4, 5, "gate"));
			Assert.True(EncounterTriggerRules.HasCell(file, 4, 6, "gate"));
			Assert.False(EncounterTriggerRules.Clear(file, 4, 5, "gate"));
		}

		[Fact]
		public void Cell_ClearDoesNotTouchAnotherTriggersCellOnSameHex()
		{
			EncounterFileModel file = EncounterFileModel.CreateEmpty();
			EncounterTriggerRules.Set(file, 4, 5, "gate");
			EncounterTriggerRules.Set(file, 4, 5, "other");
			Assert.False(EncounterTriggerRules.Clear(file, 4, 5, "missing_trigger"));
			Assert.True(EncounterTriggerRules.HasCell(file, 4, 5, "gate"));
			Assert.True(EncounterTriggerRules.HasCell(file, 4, 5, "other"));
		}

		[Fact]
		public void Cell_HexesForReturnsOnlyMatchingId()
		{
			EncounterFileModel file = EncounterFileModel.CreateEmpty();
			EncounterTriggerRules.Set(file, 1, 1, "gate");
			EncounterTriggerRules.Set(file, 2, 2, "gate");
			EncounterTriggerRules.Set(file, 3, 3, "other");
			List<(int Col, int Row)> hexes = EncounterTriggerRules.HexesFor(file, "gate");
			Assert.Equal(2, hexes.Count);
			Assert.Contains((1, 1), hexes);
			Assert.Contains((2, 2), hexes);
		}

		[Fact]
		public void CatalogAndCellsRoundTrip()
		{
			EncounterFileModel model = EncounterFileModel.CreateEmpty();
			EncounterTriggerRules.Add(model, "doorway", "guards");
			EncounterTriggerRules.Set(model, 3, 7, "doorway");
			Assert.True(EncounterFileModel.TryParse(model.ToJson(), out EncounterFileModel parsed, out string error));
			Assert.Null(error);
			Assert.Single(parsed.Triggers);
			Assert.Equal("doorway", parsed.Triggers[0].Id);
			Assert.Equal("guards", parsed.Triggers[0].PocketKey);
			Assert.Single(parsed.TriggerCells);
			Assert.Equal(3, parsed.TriggerCells[0].Col);
			Assert.Equal(7, parsed.TriggerCells[0].Row);
			Assert.Equal("doorway", parsed.TriggerCells[0].TriggerId);
		}
	}
}
