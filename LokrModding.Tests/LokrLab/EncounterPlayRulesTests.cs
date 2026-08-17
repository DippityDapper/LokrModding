using LokrLab.Encounter;
using Xunit;

namespace LokrModding.Tests.Lab
{
	public sealed class EncounterPlayRulesTests
	{
		[Fact]
		public void CanPlay_EmptyCombatants_Fails()
		{
			Assert.Equal("Add at least one GoodSide combatant or Hero Spawn Point before Sandbox.",
				EncounterPlayRules.CanPlay(EncounterFileModel.CreateEmpty()));
		}

		[Fact]
		public void CanPlay_NoGoodSide_Fails()
		{
			EncounterFileModel file = EncounterFileModel.CreateEmpty();
			file.Combatants.Add(Unit("raider_1", EncounterFileModel.BadSide));
			Assert.Contains("Sandbox needs at least one GoodSide combatant", EncounterPlayRules.CanPlay(file));
		}

		[Fact]
		public void CanPlay_OnlySpawnPointNoRealHero_FailsWithoutFill()
		{
			EncounterFileModel file = EncounterFileModel.CreateEmpty();
			file.Combatants.Add(Placed("raider_1", EncounterFileModel.BadSide, 4, 4));
			file.Combatants.Add(SpawnPoint("hero_spawn_1", 6, 10));
			Assert.Contains("Pick a character to fill the Hero Spawn Point", EncounterPlayRules.CanPlay(file));
		}

		[Fact]
		public void CanStart_OnlySpawnPointWithFill_Succeeds()
		{
			EncounterFileModel file = EncounterFileModel.CreateEmpty();
			file.Combatants.Add(Placed("raider_1", EncounterFileModel.BadSide, 4, 4));
			file.Combatants.Add(SpawnPoint("hero_spawn_1", 6, 10));
			Assert.Null(EncounterPlayRules.CanStart(file, true));
		}

		[Fact]
		public void CanPlay_SpawnPointAlongsideRealHero_Succeeds()
		{
			EncounterFileModel file = EncounterFileModel.CreateEmpty();
			file.Combatants.Add(Unit("gerald_1", EncounterFileModel.GoodSide));
			file.Combatants.Add(Unit("raider_1", EncounterFileModel.BadSide));
			file.Combatants.Add(SpawnPoint("hero_spawn_1", 6, 10));
			Assert.Null(EncounterPlayRules.CanPlay(file));
		}

		[Fact]
		public void CanPlay_UnplacedSpawnPoint_Fails()
		{
			EncounterFileModel file = EncounterFileModel.CreateEmpty();
			file.Combatants.Add(Unit("gerald_1", EncounterFileModel.GoodSide));
			EncounterCombatantModel spawnPoint = new EncounterCombatantModel
			{
				Id = "hero_spawn_1",
				Side = EncounterFileModel.GoodSide,
				Source = EncounterFileModel.SourceSpawn
			};
			file.Combatants.Add(spawnPoint);
			Assert.Contains("needs a hex", EncounterPlayRules.CanPlay(file));
		}

		[Fact]
		public void FirstGoodSide_SkipsSpawnPoints()
		{
			EncounterFileModel file = EncounterFileModel.CreateEmpty();
			file.Combatants.Add(SpawnPoint("hero_spawn_1", 6, 10));
			file.Combatants.Add(Unit("gerald_1", EncounterFileModel.GoodSide));
			Assert.Equal("gerald_1", EncounterPlayRules.FirstGoodSide(file).Id);
		}

		[Fact]
		public void FirstGoodSide_AllSpawnPoints_ReturnsNull()
		{
			EncounterFileModel file = EncounterFileModel.CreateEmpty();
			file.Combatants.Add(SpawnPoint("hero_spawn_1", 6, 10));
			Assert.Null(EncounterPlayRules.FirstGoodSide(file));
		}

		[Fact]
		public void FirstSpawnPoint_ReturnsFirstSpawnRow()
		{
			EncounterFileModel file = EncounterFileModel.CreateEmpty();
			file.Combatants.Add(Unit("gerald_1", EncounterFileModel.GoodSide));
			file.Combatants.Add(SpawnPoint("hero_spawn_1", 6, 10));
			file.Combatants.Add(SpawnPoint("hero_spawn_2", 7, 10));
			Assert.Equal("hero_spawn_1", EncounterPlayRules.FirstSpawnPoint(file).Id);
		}

		[Fact]
		public void FirstSpawnPoint_None_ReturnsNull()
		{
			EncounterFileModel file = EncounterFileModel.CreateEmpty();
			file.Combatants.Add(Unit("gerald_1", EncounterFileModel.GoodSide));
			Assert.Null(EncounterPlayRules.FirstSpawnPoint(file));
		}

		[Fact]
		public void CanPlay_GoodSide_Succeeds()
		{
			EncounterFileModel file = EncounterFileModel.CreateEmpty();
			file.Combatants.Add(Unit("gerald_1", EncounterFileModel.GoodSide));
			file.Combatants.Add(Unit("raider_1", EncounterFileModel.BadSide));
			Assert.Null(EncounterPlayRules.CanPlay(file));
		}

		[Fact]
		public void CanPlay_DuplicateHex_Fails()
		{
			EncounterFileModel file = EncounterFileModel.CreateEmpty();
			file.Combatants.Add(Placed("gerald_1", EncounterFileModel.GoodSide, 6, 10));
			file.Combatants.Add(Placed("raider_1", EncounterFileModel.BadSide, 6, 10));
			Assert.Contains("share a hex", EncounterPlayRules.CanPlay(file));
		}

		[Fact]
		public void CanPlay_PartialPlacement_Fails()
		{
			EncounterFileModel file = EncounterFileModel.CreateEmpty();
			EncounterCombatantModel combatant = Unit("gerald_1", EncounterFileModel.GoodSide);
			combatant.Col = 6;
			file.Combatants.Add(combatant);
			Assert.Contains("only col or only row", EncounterPlayRules.CanPlay(file));
		}

		[Fact]
		public void CanPlay_HexOutsideLiveBoard_Fails()
		{
			EncounterFileModel file = EncounterFileModel.CreateEmpty();
			file.Combatants.Add(Placed("gerald_1", EncounterFileModel.GoodSide, 80, 10));
			Assert.Contains("outside the live", EncounterPlayRules.CanPlay(file));
		}

		[Fact]
		public void UnitId_UsesProjectIdForCharacter()
		{
			EncounterCombatantModel combatant = new EncounterCombatantModel
			{
				Id = "hero_1",
				Side = EncounterFileModel.GoodSide,
				Source = EncounterFileModel.SourceCharacter,
				ProjectId = "necromancer_ad8174"
			};
			Assert.Equal("necromancer_ad8174", EncounterPlayRules.UnitId(combatant));
		}

		[Fact]
		public void FirstGoodSide_ReturnsFirstFriendly()
		{
			EncounterFileModel file = EncounterFileModel.CreateEmpty();
			file.Combatants.Add(Unit("raider_1", EncounterFileModel.BadSide));
			file.Combatants.Add(Unit("gerald_1", EncounterFileModel.GoodSide));
			file.Combatants.Add(Unit("gerald_2", EncounterFileModel.GoodSide));
			Assert.Equal("gerald_1", EncounterPlayRules.FirstGoodSide(file).Id);
		}

		private static EncounterCombatantModel Unit(string id, string side)
		{
			return new EncounterCombatantModel
			{
				Id = id,
				Side = side,
				Source = EncounterFileModel.SourceUnit,
				UnitId = "BanditRaider"
			};
		}

		private static EncounterCombatantModel Placed(string id, string side, int col, int row)
		{
			EncounterCombatantModel combatant = Unit(id, side);
			combatant.Col = col;
			combatant.Row = row;
			return combatant;
		}

		private static EncounterCombatantModel SpawnPoint(string id, int col, int row)
		{
			return new EncounterCombatantModel
			{
				Id = id,
				Side = EncounterFileModel.GoodSide,
				Source = EncounterFileModel.SourceSpawn,
				Col = col,
				Row = row
			};
		}
	}
}
