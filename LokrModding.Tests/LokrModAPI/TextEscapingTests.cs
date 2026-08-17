using System.IO;
using LokrModAPI.Files;
using LokrModAPI.Serialization;
using Xunit;

namespace LokrModding.Tests.ModApi
{
	public sealed class TextEscapingTests
	{
		[Fact]
		public void JsonEscape_Null_ReturnsEmpty()
		{
			Assert.Equal(string.Empty, TextEscaping.JsonEscape(null));
		}

		[Fact]
		public void JsonEscape_Empty_ReturnsEmpty()
		{
			Assert.Equal(string.Empty, TextEscaping.JsonEscape(string.Empty));
		}

		[Fact]
		public void JsonEscape_QuotesAndBackslashes()
		{
			Assert.Equal("say \\\"hi\\\" and \\\\path", TextEscaping.JsonEscape("say \"hi\" and \\path"));
		}

		[Fact]
		public void KvEscape_QuotesAndBackslashes()
		{
			Assert.Equal("say \\\"hi\\\" and \\\\path", TextEscaping.KvEscape("say \"hi\" and \\path"));
		}

		[Fact]
		public void KvEscape_Null_ReturnsEmpty()
		{
			Assert.Equal(string.Empty, TextEscaping.KvEscape(null));
		}
	}

	public sealed class ModPathLookupTests
	{
		[Fact]
		public void TryFindFile_FindsFirstModMatch()
		{
			string root = Path.Combine(Path.GetTempPath(), "lokr-modpath-" + Path.GetRandomFileName());
			try
			{
				string portraits = Path.Combine(root, "PackA", "Portraits");
				Directory.CreateDirectory(portraits);
				string expected = Path.Combine(portraits, "hero_MINI.png");
				File.WriteAllText(expected, "x");

				bool found = ModPathLookup.TryFindFile(root, "Portraits", "hero_MINI.png", out string fullPath);
				Assert.True(found);
				Assert.Equal(expected, fullPath);
			}
			finally
			{
				if (Directory.Exists(root))
				{
					Directory.Delete(root, true);
				}
			}
		}

		[Fact]
		public void TryFindFile_Missing_ReturnsFalse()
		{
			string root = Path.Combine(Path.GetTempPath(), "lokr-modpath-missing-" + Path.GetRandomFileName());
			Directory.CreateDirectory(root);
			try
			{
				Assert.False(ModPathLookup.TryFindFile(root, "Portraits", "nope.png", out string fullPath));
				Assert.Null(fullPath);
			}
			finally
			{
				Directory.Delete(root, true);
			}
		}
	}
}
