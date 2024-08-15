using ACadSharp.Entities;
using ACadSharp.Svg;
using CSMath;
using System.IO;
using Xunit;

namespace ACadSharp.Tests.Svg
{
	public class SvgConverterTests
	{
		protected const string _svgOutFolder = "../../../../../samples/out/svg";

		public static SvgConverter Instance = new SvgConverter();

		static SvgConverterTests()
		{
			if (!Directory.Exists(_svgOutFolder))
			{
				Directory.CreateDirectory(_svgOutFolder);
			}
		}

		[Fact]
		public void LineTest()
		{
			Line line = new Line(new XYZ(0, 0, 0), new XYZ(10, 10, 0));

			string filename = Path.Combine(_svgOutFolder, $"line.svg");
			string svg = Instance.Convert(line);

			this.writeSvg(filename, svg);
		}

		private void writeSvg(string filename, string svg)
		{
			using (StreamWriter sw = new StreamWriter(filename))
			{
				sw.Write(svg);
			}
		}
	}
}
