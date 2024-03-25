using ACadSharp.IO;
using System.IO;
using Xunit;
using Xunit.Abstractions;

namespace ACadSharp.Tests.IO
{
	public class ColorTests : IOTestsBase
	{
		public static TheoryData<string> ColorSamplesFilePaths { get; }

		static ColorTests()
		{
			ColorSamplesFilePaths = new TheoryData<string>();
			foreach (string p in Directory.GetFiles(Path.Combine($"{samplesFolder}", "color_samples"), $"*.dwg"))
			{
				ColorSamplesFilePaths.Add(Path.GetFileName(p));
			}
		}

		public ColorTests(ITestOutputHelper output) : base(output)
		{
		}

		[Theory]
		[MemberData(nameof(ColorSamplesFilePaths))]
		public void ColorDwg(string path)
		{
			path = Path.Combine($"{samplesFolder}", "color_samples", path);

			CadDocument doc = DwgReader.Read(path);

			//CECOLOR R 155 : G 66 : B 236
			Color currentEntityColor = doc.Header.CurrentEntityColor;
			Assert.Equal(155, currentEntityColor.R);
			Assert.Equal(66, currentEntityColor.G);
			Assert.Equal(236, currentEntityColor.B);
		}
	}
}
