using LokrCharacterLoader;
using Xunit;

namespace LokrModding.Tests
{
	public sealed class SmokeTests
	{
		[Fact]
		public void LabAliases_FileName_IsAliasesJson()
		{
			Assert.Equal("aliases.json", LabAliases.FileName);
		}
	}
}
