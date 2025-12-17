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

namespace ACadSharp.Tests.IO
{
	public class LoadPatternsTests : IOTestsBase
	{
		public static string AppPatterns { get { return Path.Combine(TestVariables.SamplesFolder, "patterns", "ALL.pat"); } }

		public static TheoryData<FileModel> DwgFiles { get; } = new();

		public static TheoryData<FileModel> DxfFiles { get; } = new();

		public static TheoryData<FileModel> PatternFilesPaths { get; } = new();

		static LoadPatternsTests()
		{
			loadSamples("./", "dwg", DwgFiles);
			loadSamples("./", "dxf", DxfFiles);
			loadSamples("patterns", "pat", PatternFilesPaths);
		}

		public LoadPatternsTests(ITestOutputHelper output) : base(output)
		{
		}

		[Theory]
		[MemberData(nameof(DwgFiles))]
		[MemberData(nameof(DxfFiles))]
		public void LoadPatternsCadComparison(FileModel test)
		{
			CadDocument doc =this.readDocument(test);

			if(doc.Header.Version <= ACadVersion.AC1009)
			{
				return;
			}

			this.assertPatterns(doc, test);
		}

		[Theory]
		[MemberData(nameof(PatternFilesPaths))]
		public void LoadPattern(FileModel test)
		{
			IEnumerable<HatchPattern> patterns = HatchPattern.LoadFrom(test.Path);
		}

		[Fact]
		public void SavePattern()
		{
			HatchPattern p = new HatchPattern("my pattern");
			p.Description = "my custom description";

			p.Lines.Add(new HatchPattern.Line
			{
				Angle = MathHelper.DegToRad(30),
				BasePoint = XY.Zero,
				Offset = XY.AxisX
			});

			p.Lines.Add(new HatchPattern.Line
			{
				Angle = MathHelper.DegToRad(40),
				BasePoint = XY.AxisY,
				Offset = XY.AxisY,
				DashLengths = new List<double> { 3.34, 32.3, 44.5 }
			});

			HatchPattern.SavePatterns(Path.Combine(TestVariables.OutputSamplesFolder, "patterns.pat"), p);
		}

		private void assertPatterns(CadDocument doc, FileModel test)
		{
			var hatches = doc.Entities.OfType<Hatch>();

			IEnumerable<HatchPattern> patterns = HatchPattern.LoadFrom(AppPatterns);

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
					//AssertUtils.AreEqual(l1.BasePoint.Round(5) / h.PatternScale, l2.BasePoint.Round(5));
					Assert.Equal(l1.DashLengths.Count, l2.DashLengths.Count);
					Assert.Equal(l1.Offset.Round(5) / h.PatternScale, l2.Offset.Round(5));
				}
			}
		}
	}
}
