using System.Collections.Generic;
using LokrLab;
using Xunit;

namespace LokrModding.Tests.Lab
{
	public sealed class AnimatorFeelRulesTests
	{
		[Fact]
		[Trait("Issue", "animator-feel")]
		public void CompensateClipDelta_KeepsWorldPosition()
		{
			float deltaX = 2f;
			float deltaY = -1f;
			AnimatorFeelRules.CompensateClipDelta(ref deltaX, ref deltaY, 0.5f, 0.25f);
			Assert.Equal(1.5f, deltaX, 5);
			Assert.Equal(-1.25f, deltaY, 5);
		}

		[Fact]
		[Trait("Issue", "animator-feel")]
		public void UseTemporaryGroupPivot_RequiresMultiSelectAndFlag()
		{
			Assert.False(AnimatorFeelRules.UseTemporaryGroupPivot(1, true));
			Assert.False(AnimatorFeelRules.UseTemporaryGroupPivot(3, false));
			Assert.True(AnimatorFeelRules.UseTemporaryGroupPivot(2, true));
		}

		[Fact]
		[Trait("Issue", "animator-feel")]
		public void EnsureRootMotionLength_IgnoresEmptyList()
		{
			List<float> empty = new List<float>();
			AnimatorFeelRules.EnsureRootMotionLength(empty, 4);
			Assert.Empty(empty);
		}

		[Fact]
		[Trait("Issue", "animator-feel")]
		public void EnsureRootMotionLength_GrowsAndTrimsAuthoredList()
		{
			List<float> samples = new List<float> { 0f, 10f };
			AnimatorFeelRules.EnsureRootMotionLength(samples, 4);
			Assert.Equal(new[] { 0f, 10f, 0f, 0f }, samples);
			AnimatorFeelRules.EnsureRootMotionLength(samples, 1);
			Assert.Equal(new[] { 0f }, samples);
		}

		[Fact]
		[Trait("Issue", "animator-feel")]
		public void InsertAndMoveRootMotionSample_PreserveAuthoredCurve()
		{
			List<float> samples = new List<float> { 0f, 10f };
			AnimatorFeelRules.InsertRootMotionSample(samples, 1);
			Assert.Equal(new[] { 0f, 0f, 10f }, samples);
			AnimatorFeelRules.MoveRootMotionSample(samples, 2, 0);
			Assert.Equal(new[] { 10f, 0f, 0f }, samples);
			AnimatorFeelRules.RemoveRootMotionSample(samples, 0);
			Assert.Equal(new[] { 0f, 0f }, samples);
		}

		[Fact]
		[Trait("Issue", "animator-feel")]
		public void ExpandRootMotionPositions_EmitsAtLeastTwoSamples()
		{
			float[] expanded = AnimatorFeelRules.ExpandRootMotionPositions(
				new[] { 0f, 30f },
				new[] { 0.1f, 0.1f });
			Assert.True(expanded.Length >= 2);
			Assert.Equal(0f, expanded[0], 3);
			Assert.Equal(30f, expanded[expanded.Length - 1], 3);
		}

		[Fact]
		[Trait("Issue", "animator-feel")]
		public void EvaluateRootMotionAtTime_LerpsAcrossFrames()
		{
			float mid = AnimatorFeelRules.EvaluateRootMotionAtTime(
				new[] { 0f, 10f },
				new[] { 1f, 1f },
				0.5f);
			Assert.Equal(5f, mid, 3);
		}

		[Fact]
		[Trait("Issue", "animator-feel")]
		public void SampleRootMotionAtFrameStarts_RoundTripsLinearRamp()
		{
			float[] dense = AnimatorFeelRules.ExpandRootMotionPositions(
				new[] { 0f, 20f },
				new[] { 0.15f, 0.15f });
			float[] sampled = AnimatorFeelRules.SampleRootMotionAtFrameStarts(
				dense,
				new[] { 0.15f, 0.15f });
			Assert.Equal(2, sampled.Length);
			Assert.Equal(0f, sampled[0], 1);
			Assert.InRange(sampled[1], 8f, 20f);
		}
	}
}
