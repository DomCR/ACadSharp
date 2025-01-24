using ACadSharp.Entities;
using ACadSharp.IO;
using ACadSharp.Tests.Common;
using ACadSharp.Tests.TestModels;
using CSMath;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Xunit;
using Xunit.Abstractions;
using static System.Net.Mime.MediaTypeNames;

namespace ACadSharp.Tests.IO
{
	public class LoadPatternsTests : IOTestsBase
	{
		public static TheoryData<FileModel> PatternFilesPaths { get; } = new();

		static LoadPatternsTests()
		{
			loadSamples("patterns", "pat", PatternFilesPaths);
		}

		public LoadPatternsTests(ITestOutputHelper output) : base(output)
		{
		}

		[Theory]
		[MemberData(nameof(PatternFilesPaths))]
		public void LoadPatternsDwgComparison(FileModel test)
		{
			CadDocument doc = DwgReader.Read(Path.Combine(test.Folder, "hatch_pattern.dwg"), this.onNotification);

			this.assertPatterns(doc, test);
		}

		[Theory]
		[MemberData(nameof(PatternFilesPaths))]
		public void LoadPatternsDxfComparison(FileModel test)
		{
			CadDocument doc = DxfReader.Read(Path.Combine(test.Folder, "hatch_pattern.dxf"), this.onNotification);

			this.assertPatterns(doc, test);
		}

		[Fact]
		public void SavePattern()
		{
		}

		private void assertPatterns(CadDocument doc, FileModel test)
		{
			var hatches = doc.Entities.OfType<Hatch>();

			IEnumerable<HatchPattern> patterns = HatchPattern.LoadFrom(test.Path);

			Assert.NotEmpty(patterns);

			foreach (HatchPattern pattern in patterns)
			{
				var h = hatches.FirstOrDefault(h => h.Pattern.Name.Equals(pattern.Name, System.StringComparison.OrdinalIgnoreCase));
				var p = hatches.FirstOrDefault(h => h.Pattern.Name.Equals(pattern.Name, System.StringComparison.OrdinalIgnoreCase)).Pattern;

				Assert.NotNull(p);
				Assert.Equal(pattern.Lines.Count, p.Lines.Count);

				for (int i = 0; i < pattern.Lines.Count; i++)
				{
					HatchPattern.Line l1 = p.Lines[i];
					HatchPattern.Line l2 = pattern.Lines[i];

					//Assert.Equal(l1.Angle, l2.Angle, 0.001);
					AssertUtils.AreEqual(l1.BasePoint.Round(5) / h.PatternScale, l2.BasePoint.Round(5));
					Assert.Equal(l1.DashLengths.Count, l2.DashLengths.Count);
					Assert.Equal(l1.Offset.Round(5) / h.PatternScale, l2.Offset.Round(5));
				}
			}
		}
	}
}
