using ACadSharp.Entities;
using ACadSharp.IO;
using CSMath;
using System.IO;
using System.Linq;
using System.Text;
using Xunit;

namespace ACadSharp.Tests.IO.DXF;

public class DxfLongRunTests
{
	//Long enough to overflow the stack of any thread if a run is read with one level of recursion per item;
	//a stack overflow cannot be caught and ends the whole process.
	private const int RunLength = 100000;

	[Fact]
	public void ReadLongRunOfComments()
	{
		StringBuilder dxf = startEntities();
		for (int i = 0; i < RunLength; i++)
		{
			dxf.Append("999\ncomment\n");
		}
		appendLine(dxf);

		CadDocument doc = read(dxf);

		Assert.Single(doc.Entities.OfType<Line>());
	}

	[Fact]
	public void ReadLongRunOfExtendedDataApplications()
	{
		StringBuilder dxf = startEntities();
		appendLine(dxf);
		for (int i = 0; i < RunLength; i++)
		{
			dxf.Append($"1001\nAPP{i}\n1000\nvalue\n");
		}
		appendCircle(dxf);

		CadDocument doc = read(dxf);

		Assert.Single(doc.Entities.OfType<Line>());
		Assert.Equal(5, doc.Entities.OfType<Circle>().Single().Radius);
	}

	[Fact]
	public void ReadLongRunOfClipBoundaryGroups()
	{
		StringBuilder dxf = startEntities();
		dxf.Append("0\nWIPEOUT\n5\nB1\n100\nAcDbEntity\n8\n0\n100\nAcDbWipeout\n10\n0\n20\n0\n30\n0\n");
		for (int i = 0; i < RunLength; i++)
		{
			dxf.Append("91\n0\n");
		}
		appendCircle(dxf);

		CadDocument doc = read(dxf);

		Assert.Single(doc.Entities.OfType<Wipeout>());
		Assert.Equal(5, doc.Entities.OfType<Circle>().Single().Radius);
	}

	[Fact]
	public void ReadClipBoundaryThenFollowingCodes()
	{
		StringBuilder dxf = startEntities();
		dxf.Append("0\nWIPEOUT\n5\nB1\n100\nAcDbEntity\n8\n0\n100\nAcDbWipeout\n10\n0\n20\n0\n30\n0\n");
		dxf.Append("91\n2\n14\n1\n24\n2\n14\n3\n24\n4\n");
		dxf.Append("281\n60\n");
		appendCircle(dxf);

		CadDocument doc = read(dxf);

		Wipeout wipeout = doc.Entities.OfType<Wipeout>().Single();
		Assert.Equal(new[] { new XY(1, 2), new XY(3, 4) }, wipeout.ClipBoundaryVertices);
		Assert.Equal(60, wipeout.Brightness);
		Assert.Equal(5, doc.Entities.OfType<Circle>().Single().Radius);
	}

	private static StringBuilder startEntities()
	{
		StringBuilder dxf = new StringBuilder();
		dxf.Append("0\nSECTION\n2\nENTITIES\n");
		return dxf;
	}

	private static void appendLine(StringBuilder dxf)
	{
		dxf.Append("0\nLINE\n5\nA1\n100\nAcDbEntity\n8\n0\n100\nAcDbLine\n10\n0\n20\n0\n30\n0\n11\n1\n21\n1\n31\n0\n");
	}

	private static void appendCircle(StringBuilder dxf)
	{
		dxf.Append("0\nCIRCLE\n5\nC1\n100\nAcDbEntity\n8\n0\n100\nAcDbCircle\n10\n0\n20\n0\n30\n0\n40\n5\n");
	}

	private static CadDocument read(StringBuilder dxf)
	{
		dxf.Append("0\nENDSEC\n0\nEOF\n");

		using (MemoryStream stream = new MemoryStream(Encoding.ASCII.GetBytes(dxf.ToString())))
		using (DxfReader reader = new DxfReader(stream))
		{
			return reader.Read();
		}
	}
}
