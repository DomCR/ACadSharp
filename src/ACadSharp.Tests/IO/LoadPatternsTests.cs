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
			CadDocument doc = DxfReader.Read(Path.Combine(test.Folder, "hatch_pattern.dxf"), this.onNotification);

			Hatch h = doc.Entities.OfType<Hatch>().FirstOrDefault();

			IEnumerable<HatchPattern> patterns = HatchPattern.LoadFrom(test.Path);
		}
	}
}
