using ACadSharp.Entities;
using ACadSharp.IO;
using ACadSharp.Tests.TestModels;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Xunit;
using Xunit.Abstractions;

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
		public void LoadPatterns(FileModel test)
		{
			CadDocument doc;

			if (test.IsDxf)
			{
				doc = DxfReader.Read(Path.Combine(test.Folder, "hatch_pattern.dxf"), this.onNotification);
			}
			else
			{
				doc = DwgReader.Read(Path.Combine(test.Folder, "hatch_pattern.dwg"), this.onNotification);
			}

			var hatches = doc.Entities.OfType<Hatch>();

			IEnumerable<HatchPattern> patterns = HatchPattern.LoadFrom(test.Path);

			Assert.NotEmpty(patterns);

			foreach (HatchPattern pattern in patterns)
			{
				var p = hatches.FirstOrDefault(h => h.Pattern.Name.Equals(pattern.Name, System.StringComparison.OrdinalIgnoreCase)).Pattern;

				Assert.NotNull(p);
				Assert.Equal(pattern.Lines.Count, p.Lines.Count);
			}
		}
	}
}
