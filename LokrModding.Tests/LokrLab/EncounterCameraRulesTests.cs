using LokrLab.Encounter;
using Xunit;

namespace LokrModding.Tests.Lab
{
	public sealed class EncounterCameraRulesTests
	{
		[Fact]
		public void HasBounds_RequiresMinSpan()
		{
			Assert.False(EncounterCameraRules.HasBounds(null));
			Assert.False(EncounterCameraRules.HasBounds(new EncounterCameraModel
			{
				MinX = 0f,
				MinY = 0f,
				MaxX = 0.5f,
				MaxY = 4f
			}));
			Assert.True(EncounterCameraRules.HasBounds(EncounterCameraRules.FromCorners(0f, 0f, 4f, 3f)));
		}

		[Fact]
		public void Normalize_SwapsInvertedEdges()
		{
			EncounterCameraModel camera = new EncounterCameraModel
			{
				MinX = 8f,
				MaxX = 2f,
				MinY = 5f,
				MaxY = 1f
			};
			EncounterCameraRules.Normalize(camera);
			Assert.Equal(2f, camera.MinX);
			Assert.Equal(8f, camera.MaxX);
			Assert.Equal(1f, camera.MinY);
			Assert.Equal(5f, camera.MaxY);
		}

		[Fact]
		public void FitOrtho_UsesTheTighterAxis()
		{
			EncounterCameraModel camera = EncounterCameraRules.FromCorners(0f, 0f, 20f, 10f);
			Assert.Equal(5f, EncounterCameraRules.FitOrtho(camera, 1f));
			Assert.Equal(5f, EncounterCameraRules.FitOrtho(camera, 2f));
		}

		[Fact]
		public void PlayOrtho_ClampsAuthoredToFit()
		{
			EncounterCameraModel camera = EncounterCameraRules.FromCorners(0f, 0f, 20f, 10f);
			camera.OrthoSize = 20f;
			Assert.Equal(5f, EncounterCameraRules.PlayOrtho(camera, 1f));
			camera.OrthoSize = 2f;
			Assert.Equal(2f, EncounterCameraRules.PlayOrtho(camera, 1f));
		}

		[Fact]
		public void Hit_PrefersCornersThenEdgesThenInterior()
		{
			EncounterCameraModel camera = EncounterCameraRules.FromCorners(0f, 0f, 10f, 8f);
			Assert.Equal(EncounterCameraHandle.NorthWest, EncounterCameraRules.Hit(camera, 0f, 8f, 0.4f));
			Assert.Equal(EncounterCameraHandle.East, EncounterCameraRules.Hit(camera, 10f, 4f, 0.4f));
			Assert.Equal(EncounterCameraHandle.Interior, EncounterCameraRules.Hit(camera, 5f, 4f, 0.4f));
			Assert.Equal(EncounterCameraHandle.None, EncounterCameraRules.Hit(camera, 20f, 20f, 0.4f));
		}

		[Fact]
		public void ClampCenter_KeepsViewInsideRect()
		{
			EncounterCameraModel camera = EncounterCameraRules.FromCorners(0f, 0f, 20f, 10f);
			float x;
			float y;
			EncounterCameraRules.ClampCenter(camera, 100f, 100f, 2f, 1f, out x, out y);
			Assert.Equal(18f, x);
			Assert.Equal(8f, y);
			EncounterCameraRules.ClampCenter(camera, 10f, 5f, 2f, 1f, out x, out y);
			Assert.Equal(10f, x);
			Assert.Equal(5f, y);
		}

		[Fact]
		public void ClampCenter_PinsToMidWhenViewFillsTheRect()
		{
			EncounterCameraModel camera = EncounterCameraRules.FromCorners(0f, 0f, 20f, 10f);
			float x;
			float y;
			EncounterCameraRules.ClampCenter(camera, 0f, 0f, 5f, 2f, out x, out y);
			Assert.Equal(10f, x);
			Assert.Equal(5f, y);
		}

		[Fact]
		public void ApplyHandle_MovesAndResizes()
		{
			EncounterCameraModel origin = EncounterCameraRules.FromCorners(0f, 0f, 10f, 8f);
			EncounterCameraModel dest = new EncounterCameraModel();
			EncounterCameraRules.ApplyHandle(dest, origin, EncounterCameraHandle.Interior, 3f, 1f, 1f, 1f);
			Assert.Equal(2f, dest.MinX);
			Assert.Equal(12f, dest.MaxX);
			EncounterCameraRules.ApplyHandle(dest, origin, EncounterCameraHandle.East, 14f, 4f, 10f, 4f);
			Assert.Equal(14f, dest.MaxX);
			Assert.Equal(0f, dest.MinX);
		}
	}
}
