using System.Collections.Generic;
using LokrLab.Encounter;
using Xunit;

namespace LokrModding.Tests.Lab
{
	public sealed class EncounterFileModelTests
	{
		[Fact]
		public void EmptyRoundTrip_KeepsDefaultTemplate()
		{
			EncounterFileModel empty = EncounterFileModel.CreateEmpty();
			string json = empty.ToJson();
			Assert.True(EncounterFileModel.TryParse(json, out EncounterFileModel parsed, out string error));
			Assert.Null(error);
			Assert.Equal(EncounterFileModel.CurrentSchemaVersion, parsed.SchemaVersion);
			Assert.Equal(EncounterFileModel.DefaultTemplate, parsed.Template);
			Assert.Empty(parsed.Combatants);
			Assert.Empty(parsed.Overrides);
			Assert.Empty(parsed.Tiles);
			Assert.Empty(parsed.Terrains);
			Assert.Empty(parsed.Props);
			Assert.Empty(parsed.Decorations);
			Assert.Null(parsed.Camera);
			Assert.DoesNotContain("\"camera\"", json);
			Assert.False(parsed.WalkableDefault);
			Assert.False(parsed.TilesDefault);
			Assert.Contains("\"overrides\": []", json);
			Assert.Contains("\"tiles\": []", json);
			Assert.Contains("\"terrains\": []", json);
			Assert.Contains("\"props\": []", json);
			Assert.Contains("\"decorations\": []", json);
			Assert.Contains("\"tilesDefault\": false", json);
		}

		[Fact]
		public void DecorationsRoundTrip_KeepsUnitIdAndPlacement()
		{
			EncounterFileModel model = EncounterFileModel.CreateEmpty();
			model.Template = "combat_goblinraid";
			model.Decorations.Add(new EncounterDecorationModel
			{
				Id = "farmer_1",
				UnitId = "Farmer",
				Col = 8,
				Row = 12,
				Flipped = true
			});
			model.Decorations.Add(new EncounterDecorationModel
			{
				Id = "farmer_2",
				UnitId = string.Empty
			});

			Assert.True(EncounterFileModel.TryParse(model.ToJson(), out EncounterFileModel parsed, out string error));
			Assert.Null(error);
			Assert.Equal(2, parsed.Decorations.Count);
			Assert.Equal("farmer_1", parsed.Decorations[0].Id);
			Assert.Equal("Farmer", parsed.Decorations[0].UnitId);
			Assert.Equal(8, parsed.Decorations[0].Col);
			Assert.Equal(12, parsed.Decorations[0].Row);
			Assert.True(parsed.Decorations[0].Flipped);
			Assert.Equal("farmer_2", parsed.Decorations[1].Id);
			Assert.Equal(string.Empty, parsed.Decorations[1].UnitId);
			Assert.Null(parsed.Decorations[1].Col);
			Assert.Null(parsed.Decorations[1].Row);
		}

		[Fact]
		public void ValidateDecoration_RejectsBadIdButAllowsEmptyUnitId()
		{
			Assert.Null(EncounterFileModel.ValidateDecoration(
				new EncounterDecorationModel { Id = "farmer_1", UnitId = string.Empty },
				new HashSet<string>()));
			Assert.NotNull(EncounterFileModel.ValidateDecoration(
				new EncounterDecorationModel { Id = "Bad Id!", UnitId = "Farmer" },
				new HashSet<string>()));
			Assert.NotNull(EncounterFileModel.ValidateDecoration(
				new EncounterDecorationModel { Id = "farmer_1", UnitId = "Farmer" },
				new HashSet<string> { "farmer_1" }));
		}

		[Fact]
		public void CombatantsRoundTrip_KeepsCharacterAndUnit()
		{
			EncounterFileModel model = EncounterFileModel.CreateEmpty();
			model.Template = "combat_wip";
			model.Combatants.Add(new EncounterCombatantModel
			{
				Id = "gerald_1",
				Side = EncounterFileModel.GoodSide,
				Source = EncounterFileModel.SourceCharacter,
				ProjectId = "necromancer_ad8174",
				Level = 2,
				Col = 6,
				Row = 10,
				Flipped = false
			});
			model.Combatants.Add(new EncounterCombatantModel
			{
				Id = "banditraider_1",
				Side = EncounterFileModel.BadSide,
				Source = EncounterFileModel.SourceUnit,
				UnitId = "BanditRaider",
				Flipped = true
			});

			Assert.True(EncounterFileModel.TryParse(model.ToJson(), out EncounterFileModel parsed, out string error));
			Assert.Null(error);
			Assert.Equal("combat_wip", parsed.Template);
			Assert.Equal(2, parsed.Combatants.Count);
			Assert.Equal("gerald_1", parsed.Combatants[0].Id);
			Assert.Equal("necromancer_ad8174", parsed.Combatants[0].ProjectId);
			Assert.Equal(2, parsed.Combatants[0].Level);
			Assert.Equal(6, parsed.Combatants[0].Col);
			Assert.Equal(10, parsed.Combatants[0].Row);
			Assert.Equal("BanditRaider", parsed.Combatants[1].UnitId);
			Assert.True(parsed.Combatants[1].Flipped);
			Assert.Equal(1, parsed.CountSide(EncounterFileModel.GoodSide));
			Assert.Equal(1, parsed.CountSide(EncounterFileModel.BadSide));
		}

		[Fact]
		public void ValidateCombatant_RejectsUnknownSideAndBadId()
		{
			HashSet<string> used = new HashSet<string>();
			Assert.Equal("Combatant id must be a legal slug.",
				EncounterFileModel.ValidateCombatant(new EncounterCombatantModel
				{
					Id = "1bad",
					Side = EncounterFileModel.GoodSide,
					Source = EncounterFileModel.SourceUnit,
					UnitId = "BanditRaider"
				}, used));
			Assert.Equal("Combatant side must be GoodSide or BadSide.",
				EncounterFileModel.ValidateCombatant(new EncounterCombatantModel
				{
					Id = "raider_1",
					Side = "OwnSide",
					Source = EncounterFileModel.SourceUnit,
					UnitId = "BanditRaider"
				}, used));
		}

		[Fact]
		public void ValidateCombatant_RejectsSpawnPointOnBadSide()
		{
			Assert.Equal("Hero spawn points must be GoodSide.",
				EncounterFileModel.ValidateCombatant(new EncounterCombatantModel
				{
					Id = "hero_spawn_1",
					Side = EncounterFileModel.BadSide,
					Source = EncounterFileModel.SourceSpawn
				}, null));
		}

		[Fact]
		public void ValidateCombatant_AcceptsSpawnPointOnGoodSide()
		{
			Assert.Null(EncounterFileModel.ValidateCombatant(new EncounterCombatantModel
			{
				Id = "hero_spawn_1",
				Side = EncounterFileModel.GoodSide,
				Source = EncounterFileModel.SourceSpawn
			}, null));
		}

		[Fact]
		public void SpawnPointRoundTrip_KeepsPlacementAndNoIdentity()
		{
			EncounterFileModel model = EncounterFileModel.CreateEmpty();
			model.Combatants.Add(new EncounterCombatantModel
			{
				Id = "hero_spawn_1",
				Side = EncounterFileModel.GoodSide,
				Source = EncounterFileModel.SourceSpawn,
				Col = 6,
				Row = 10,
				Flipped = true
			});

			string json = model.ToJson();
			Assert.DoesNotContain("\"projectId\"", json);
			Assert.DoesNotContain("\"unitId\"", json);
			Assert.True(EncounterFileModel.TryParse(json, out EncounterFileModel parsed, out string error));
			Assert.Null(error);
			Assert.Single(parsed.Combatants);
			EncounterCombatantModel spawnPoint = parsed.Combatants[0];
			Assert.Equal(EncounterFileModel.SourceSpawn, spawnPoint.Source);
			Assert.Equal(EncounterFileModel.GoodSide, spawnPoint.Side);
			Assert.Equal(6, spawnPoint.Col);
			Assert.Equal(10, spawnPoint.Row);
			Assert.True(spawnPoint.Flipped);
			Assert.Equal(string.Empty, spawnPoint.ProjectId);
			Assert.Equal(string.Empty, spawnPoint.UnitId);
		}

		[Fact]
		public void MintCombatantId_IncrementsUntilUnused()
		{
			HashSet<string> used = new HashSet<string> { "banditraider_1" };
			Assert.Equal("banditraider_2", EncounterFileModel.MintCombatantId("BanditRaider", used));
			Assert.Equal("combatant_1", EncounterFileModel.MintCombatantId(" ", null));
			Assert.Equal("c1bad_1", EncounterFileModel.MintCombatantId("1bad", used));
		}

		[Fact]
		public void CameraRoundTrip_KeepsBoundsAndLock()
		{
			EncounterFileModel model = EncounterFileModel.CreateEmpty();
			model.Camera = EncounterCameraRules.FromCorners(1.5f, -2f, 20f, 14f);
			model.Camera.LockZoom = true;
			model.Camera.OrthoSize = 6.25f;
			Assert.True(EncounterFileModel.TryParse(model.ToJson(), out EncounterFileModel parsed, out string error));
			Assert.Null(error);
			Assert.NotNull(parsed.Camera);
			Assert.Equal(1.5f, parsed.Camera.MinX);
			Assert.Equal(-2f, parsed.Camera.MinY);
			Assert.Equal(20f, parsed.Camera.MaxX);
			Assert.Equal(14f, parsed.Camera.MaxY);
			Assert.True(parsed.Camera.LockZoom);
			Assert.Equal(6.25f, parsed.Camera.OrthoSize);
		}

		[Fact]
		public void TryParse_V7WithoutCamera_StaysUnclamped()
		{
			string json = "{\n  \"schemaVersion\": 7,\n  \"template\": \"fighttesterempty\",\n  \"props\": [],\n  \"combatants\": []\n}\n";
			Assert.True(EncounterFileModel.TryParse(json, out EncounterFileModel parsed, out string error));
			Assert.Null(error);
			Assert.Null(parsed.Camera);
		}

		[Fact]
		public void EmptyRoundTrip_DefaultsExplorationOff()
		{
			EncounterFileModel empty = EncounterFileModel.CreateEmpty();
			string json = empty.ToJson();
			Assert.True(EncounterFileModel.TryParse(json, out EncounterFileModel parsed, out string error));
			Assert.Null(error);
			Assert.Equal(14, EncounterFileModel.CurrentSchemaVersion);
			Assert.False(parsed.Exploration);
			Assert.Equal(EncounterFileModel.DefaultAggroRadiusValue, parsed.DefaultAggroRadius);
			Assert.Contains("\"exploration\": false", json);
			Assert.Contains("\"defaultAggroRadius\": " + EncounterFileModel.DefaultAggroRadiusValue, json);
			Assert.Empty(parsed.Triggers);
			Assert.Empty(parsed.TriggerCells);
			Assert.Contains("\"triggers\": []", json);
			Assert.Contains("\"triggerCells\": []", json);
		}

		[Fact]
		public void TriggersRoundTrip_KeepsCatalogPaintedCellsAndCombatantReference()
		{
			EncounterFileModel model = EncounterFileModel.CreateEmpty();
			model.Exploration = true;
			model.Triggers.Add(new EncounterTriggerModel { Id = "gate_trigger", PocketKey = "guards" });
			model.TriggerCells.Add(new EncounterHexTrigger { Col = 3, Row = 4, TriggerId = "gate_trigger" });
			model.TriggerCells.Add(new EncounterHexTrigger { Col = 3, Row = 5, TriggerId = "gate_trigger" });
			model.Combatants.Add(new EncounterCombatantModel
			{
				Id = "orc_1",
				Side = EncounterFileModel.BadSide,
				Source = EncounterFileModel.SourceUnit,
				UnitId = "BanditRaider",
				TriggerId = "gate_trigger"
			});

			Assert.True(EncounterFileModel.TryParse(model.ToJson(), out EncounterFileModel parsed, out string error));
			Assert.Null(error);
			Assert.Single(parsed.Triggers);
			Assert.Equal("gate_trigger", parsed.Triggers[0].Id);
			Assert.Equal("guards", parsed.Triggers[0].PocketKey);
			Assert.Equal(2, parsed.TriggerCells.Count);
			Assert.Equal("gate_trigger", parsed.TriggerCells[0].TriggerId);
			Assert.Equal(3, parsed.TriggerCells[0].Col);
			Assert.Equal(4, parsed.TriggerCells[0].Row);
			Assert.Equal("gate_trigger", parsed.Combatants[0].TriggerId);
		}

		[Fact]
		public void TryParse_V10WithoutTriggers_DefaultsEmpty()
		{
			string json = "{\n  \"schemaVersion\": 10,\n  \"template\": \"fighttesterempty\",\n  \"combatants\": []\n}\n";
			Assert.True(EncounterFileModel.TryParse(json, out EncounterFileModel parsed, out string error));
			Assert.Null(error);
			Assert.Empty(parsed.Triggers);
		}

		[Fact]
		public void ExplorationRoundTrip_KeepsPocketAndPerUnitRadius()
		{
			EncounterFileModel model = EncounterFileModel.CreateEmpty();
			model.Exploration = true;
			model.DefaultAggroRadius = 6;
			model.Combatants.Add(new EncounterCombatantModel
			{
				Id = "hero_1",
				Side = EncounterFileModel.GoodSide,
				Source = EncounterFileModel.SourceUnit,
				UnitId = "Musketeer"
			});
			model.Combatants.Add(new EncounterCombatantModel
			{
				Id = "orc_1",
				Side = EncounterFileModel.BadSide,
				Source = EncounterFileModel.SourceUnit,
				UnitId = "BanditRaider",
				Pocket = "gate_guards",
				AggroRadius = 2
			});
			model.Combatants.Add(new EncounterCombatantModel
			{
				Id = "orc_2",
				Side = EncounterFileModel.BadSide,
				Source = EncounterFileModel.SourceUnit,
				UnitId = "BanditRaider",
				Pocket = "gate_guards"
			});
			model.Combatants.Add(new EncounterCombatantModel
			{
				Id = "sniper_1",
				Side = EncounterFileModel.BadSide,
				Source = EncounterFileModel.SourceUnit,
				UnitId = "BanditRaider"
			});

			Assert.True(EncounterFileModel.TryParse(model.ToJson(), out EncounterFileModel parsed, out string error));
			Assert.Null(error);
			Assert.True(parsed.Exploration);
			Assert.Equal(6, parsed.DefaultAggroRadius);
			Assert.Equal("gate_guards", parsed.Combatants[1].Pocket);
			Assert.Equal(2, parsed.Combatants[1].AggroRadius);
			Assert.Equal("gate_guards", parsed.Combatants[2].Pocket);
			Assert.Null(parsed.Combatants[2].AggroRadius);
			Assert.Equal(string.Empty, parsed.Combatants[3].Pocket);
			Assert.Null(parsed.Combatants[3].AggroRadius);
			Assert.Equal(string.Empty, parsed.Combatants[0].Pocket);
		}

		[Fact]
		public void TryParse_V9WithoutExploration_DefaultsOff()
		{
			string json = "{\n  \"schemaVersion\": 9,\n  \"template\": \"fighttesterempty\",\n  \"combatants\": []\n}\n";
			Assert.True(EncounterFileModel.TryParse(json, out EncounterFileModel parsed, out string error));
			Assert.Null(error);
			Assert.False(parsed.Exploration);
			Assert.Equal(EncounterFileModel.DefaultAggroRadiusValue, parsed.DefaultAggroRadius);
		}

		[Fact]
		public void TryParse_SkipsInvalidCombatantRows()
		{
			string json = "{\n  \"schemaVersion\": 1,\n  \"template\": \"fighttesterempty\",\n  \"combatants\": [\n    { \"id\": \"ok_1\", \"side\": \"GoodSide\", \"source\": \"unit\", \"unitId\": \"BanditRaider\" },\n    { \"id\": \"bad_1\", \"side\": \"OwnSide\", \"source\": \"unit\", \"unitId\": \"BanditRaider\" }\n  ]\n}\n";
			Assert.True(EncounterFileModel.TryParse(json, out EncounterFileModel parsed, out string error));
			Assert.Null(error);
			Assert.Single(parsed.Combatants);
			Assert.Equal("ok_1", parsed.Combatants[0].Id);
		}
	}
}
