using System.Collections.Generic;
using LokrLab.Encounter;
using Xunit;

namespace LokrModding.Tests.Lab
{
	public sealed class EncounterPaintRulesTests
	{
		[Fact]
		public void TryClampCell_Negative_IsRejected()
		{
			Assert.False(EncounterPaintRules.TryClampCell(-1, 10, out _, out _));
			Assert.False(EncounterPaintRules.TryClampCell(10, -1, out _, out _));
		}

		[Fact]
		public void TryClampCell_PastCap_PinsToMaxLive()
		{
			Assert.True(EncounterPaintRules.TryClampCell(80, 80, out int col, out int row));
			Assert.Equal(EncounterGrowRules.MaxLive - 1, col);
			Assert.Equal(EncounterGrowRules.MaxLive - 1, row);
		}

		[Fact]
		public void ForEachOnLine_SameCell_VisitsOnce()
		{
			List<string> cells = new List<string>();
			EncounterPaintRules.ForEachOnLine(8, 10, 8, 10, (col, row) => cells.Add(col + "," + row));
			Assert.Equal(new[] { "8,10" }, cells);
		}

		[Fact]
		public void ForEachOnLine_FiveRowsDown_IncludesEnds()
		{
			List<string> cells = new List<string>();
			EncounterPaintRules.ForEachOnLine(10, 24, 10, 28, (col, row) => cells.Add(col + "," + row));
			Assert.Equal("10,24", cells[0]);
			Assert.Equal("10,28", cells[cells.Count - 1]);
			Assert.Equal(5, cells.Count);
		}

		[Fact]
		public void UnblockFarBelow_GrowsEffectiveHeight()
		{
			EncounterFileModel file = EncounterFileModel.CreateEmpty();
			Assert.True(EncounterPaintRules.TryClampCell(10, 28, out int col, out int row));
			EncounterBoardRules.SetOverride(file, col, row, true);
			EncounterGrowRules.EffectiveLiveSize(file, out int width, out int height);
			Assert.Equal(24, width);
			Assert.Equal(29, height);
		}
	}
}
