using ACadSharp.IO;
using System.IO;
using Xunit;

namespace ACadSharp.Tests.IO
{
	public class ShapeFileTests
	{
		[Fact]
		public void OpenTest()
		{
			ShapeFile.Open(Path.Combine(TestVariables.SamplesFolder, ShapeFile.DefaultShapeFile));
			ShapeFile.Open(Path.Combine(TestVariables.SamplesFolder, "line.shx"));
		}
	}
}
