using ACadSharp.Entities;
using ACadSharp.IO;
using ACadSharp.Tests.Common;
using CSMath;
using System.IO;
using System.Linq;
using Xunit;

namespace ACadSharp.Tests.IO.DXF;

public class DxfDimensionWriterTests
{
	[Fact]
	public void WriteDimensionPreservesInsertionPoint()
	{
		XYZ expected = new XYZ(12.5, -8.25, 3.5);
		CadDocument document = new CadDocument();
		document.Entities.Add(new DimensionLinear
		{
			FirstPoint = XYZ.Zero,
			SecondPoint = new XYZ(10, 0, 0),
			DefinitionPoint = new XYZ(0, 5, 0),
			TextMiddlePoint = new XYZ(5, 5, 0),
			InsertionPoint = expected,
		});

		using MemoryStream output = new MemoryStream();
		DxfWriter.Write(output, document);

		using MemoryStream input = new MemoryStream(output.ToArray());
		CadDocument result = DxfReader.Read(input);
		DimensionLinear dimension = Assert.Single(result.Entities.OfType<DimensionLinear>());

		AssertUtils.AreEqual(expected, dimension.InsertionPoint);
	}
}
