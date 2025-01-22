using ACadSharp.IO;
using ACadSharp.Entities;
using ACadSharp.Tables;
using System.IO;
using System.Linq;
using Xunit;
using Xunit.Abstractions;
using ACadSharp.Tests.TestModels;
using ACadSharp.Objects;

namespace ACadSharp.Tests.IO
{
	public class ColorTests : IOTestsBase
	{
		public static TheoryData<FileModel> ColorSamplesFilePaths { get; } = new();

		static ColorTests()
		{
			loadSamples("color_samples", "*", ColorSamplesFilePaths);
		}

		public ColorTests(ITestOutputHelper output) : base(output)
		{
		}

		[Theory]
		[MemberData(nameof(ColorSamplesFilePaths))]
		public void BasicColorTest(FileModel test)
		{
			bool isDxf = Path.GetExtension(test.FileName).Equals(".dxf");
			CadDocument doc = this.readDocument(test);

			if (doc.Header.Version <= ACadVersion.AC1015)
			{
				return;
			}

			//Entity Line: R 52 : G 201 : B 24
			Line line = doc.Entities.OfType<Line>().FirstOrDefault();
			Assert.NotNull(line);
			Color lcolor = line.Color;
			Assert.Equal(52, lcolor.R);
			Assert.Equal(201, lcolor.G);
			Assert.Equal(24, lcolor.B);

			//Layer: color_125_33_79
			Color layerColor = doc.Layers["color_125_33_79"].Color;
			Assert.Equal(125, layerColor.R);
			Assert.Equal(33, layerColor.G);
			Assert.Equal(79, layerColor.B);

			if (isDxf)
			{
				//True color for dimension style is not supported in dxf
				return;
			}

			//CECOLOR R 155 : G 66 : B 236
			Color currentEntityColor = doc.Header.CurrentEntityColor;
			Assert.Equal(155, currentEntityColor.R);
			Assert.Equal(66, currentEntityColor.G);
			Assert.Equal(236, currentEntityColor.B);

			//DimStyle: custom_dim_style
			DimensionStyle dimStyle = doc.DimensionStyles["custom_dim_style"];
			//DimensionLines: 44,136,27
			Color linesColor = dimStyle.DimensionLineColor;
			Assert.Equal(44, linesColor.R);
			Assert.Equal(136, linesColor.G);
			Assert.Equal(27, linesColor.B);
			//ExtensionLines: 128,61,194
			Color extensionLinesColor = dimStyle.ExtensionLineColor;
			Assert.Equal(128, extensionLinesColor.R);
			Assert.Equal(61, extensionLinesColor.G);
			Assert.Equal(194, extensionLinesColor.B);
			//TextColor: 80,179,255
			Color textColor = dimStyle.TextColor;
			Assert.Equal(80, textColor.R);
			Assert.Equal(179, textColor.G);
			Assert.Equal(255, textColor.B);

			if (doc.Header.Version >= ACadVersion.AC1021)
			{
				//FillColor: 54,117,66
				Color fillColor = dimStyle.TextBackgroundColor;
				Assert.Equal(54, fillColor.R);
				Assert.Equal(117, fillColor.G);
				Assert.Equal(66, fillColor.B);
			}
		}

		[Theory]
		[MemberData(nameof(ColorSamplesFilePaths))]
		public void BookColor(FileModel test)
		{
			CadDocument doc = this.readDocument(test);

			Circle circle = doc.GetCadObject<Circle>(649);

			Assert.True(doc.Colors.ContainsKey("RAL CLASSIC$RAL 1006"));

			if (doc.Header.Version <= ACadVersion.AC1015)
			{
				return;
			}

			Assert.NotNull(circle.BookColor);

			BookColor color = circle.BookColor;

			Assert.Equal("RAL 1006", color.ColorName);
			Assert.Equal("RAL CLASSIC", color.BookName);
		}
	}
}
