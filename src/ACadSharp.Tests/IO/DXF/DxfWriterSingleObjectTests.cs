using ACadSharp.Entities;
using ACadSharp.IO;
using CSMath;
using System.IO;
using System.Linq;
using Xunit;
using Xunit.Abstractions;

namespace ACadSharp.Tests.IO.DXF;

public class DxfWriterSingleObjectTests : WriterSingleObjectTests
{
	public DxfWriterSingleObjectTests(ITestOutputHelper output) : base(output) { }

	[Theory()]
	[MemberData(nameof(Data))]
	public void WriteCasesAC1015(SingleCaseGenerator data)
	{
		this.writeDxfFile(data, ACadVersion.AC1015);
	}

	[Theory()]
	[MemberData(nameof(Data))]
	public void WriteCasesAC1018(SingleCaseGenerator data)
	{
		this.writeDxfFile(data, ACadVersion.AC1018);
	}

	[Theory()]
	[MemberData(nameof(Data))]
	public void WriteCasesAC1021(SingleCaseGenerator data)
	{
		this.writeDxfFile(data, ACadVersion.AC1021);
	}

	[Theory()]
	[MemberData(nameof(Data))]
	public void WriteCasesAC1024(SingleCaseGenerator data)
	{
		this.writeDxfFile(data, ACadVersion.AC1024);
	}

	[Theory()]
	[MemberData(nameof(Data))]
	public void WriteCasesAC1027(SingleCaseGenerator data)
	{
		this.writeDxfFile(data, ACadVersion.AC1027);
	}

	[Theory()]
	[MemberData(nameof(Data))]
	public void WriteCasesAC1032(SingleCaseGenerator data)
	{
		this.writeDxfFile(data, ACadVersion.AC1032);
	}

	[Theory]
	[InlineData(-90, 270, -90, 270)]
	[InlineData(180, 540, -180, 180)]
	[InlineData(-540, -180, -180, 180)]
	[InlineData(540, 180, 180, -180)]
	[InlineData(-180, -540, 180, -180)]
	[InlineData(0, 360, 0, 360)]
	[InlineData(0, -360, 0, -360)]
	[InlineData(360, 720, 0, 360)]
	[InlineData(90, 90, 90, 90)]
	[InlineData(0, 90, 0, 90)]
	[InlineData(450, 540, 90, 180)]
	[InlineData(-540, -450, -180, -90)]
	public void WriteHatchNormalizesBoundaryAngles(double startDegrees, double endDegrees, double expectedStartDegrees, double expectedEndDegrees)
	{
		double startAngle = MathHelper.DegToRad(startDegrees);
		double endAngle = MathHelper.DegToRad(endDegrees);
		double expectedStartAngle = MathHelper.DegToRad(expectedStartDegrees);
		double expectedEndAngle = MathHelper.DegToRad(expectedEndDegrees);

		SingleCaseGenerator data = new SingleCaseGenerator();
		data.CreateHatchFullSweeps();

		Hatch hatch = Assert.Single(data.Document.Entities.OfType<Hatch>());
		Hatch.BoundaryPath.Arc arc = Assert.Single(
			hatch.Paths.SelectMany(p => p.Edges).OfType<Hatch.BoundaryPath.Arc>());
		Hatch.BoundaryPath.Ellipse ellipse = Assert.Single(
			hatch.Paths.SelectMany(p => p.Edges).OfType<Hatch.BoundaryPath.Ellipse>());
		arc.StartAngle = ellipse.StartAngle = startAngle;
		arc.EndAngle = ellipse.EndAngle = endAngle;
		ellipse.CounterClockWise = false;

		using MemoryStream output = new MemoryStream();
		DxfWriter.Write(output, data.Document);

		using MemoryStream input = new MemoryStream(output.ToArray());
		CadDocument result = DxfReader.Read(input);
		Hatch writtenHatch = Assert.Single(result.Entities.OfType<Hatch>());
		arc = Assert.Single(writtenHatch.Paths.SelectMany(p => p.Edges).OfType<Hatch.BoundaryPath.Arc>());
		ellipse = Assert.Single(writtenHatch.Paths.SelectMany(p => p.Edges).OfType<Hatch.BoundaryPath.Ellipse>());

		Assert.Equal(expectedStartAngle, arc.StartAngle, 12);
		Assert.Equal(expectedEndAngle, arc.EndAngle, 12);
		Assert.Equal(expectedStartAngle, ellipse.StartAngle, 12);
		Assert.Equal(expectedEndAngle, ellipse.EndAngle, 12);
		Assert.InRange(arc.StartAngle, -MathHelper.TwoPI, MathHelper.TwoPI);
		Assert.InRange(arc.EndAngle, -MathHelper.TwoPI, MathHelper.TwoPI);
		Assert.InRange(ellipse.StartAngle, -MathHelper.TwoPI, MathHelper.TwoPI);
		Assert.InRange(ellipse.EndAngle, -MathHelper.TwoPI, MathHelper.TwoPI);
		Assert.True(arc.CounterClockWise);
		Assert.False(ellipse.CounterClockWise);
	}

	protected void writeDxfFile(SingleCaseGenerator data, ACadVersion version)
	{
		Assert.True(data.HasExecuted, $"The writer has failed during it's execution.");

		string path = this.getPath(data.Name, "dxf", version);
		data.Document.Header.Version = version;

		if (TestVariables.SaveOutputInStream)
		{
			MemoryStream ms = new MemoryStream();
			DxfWriter.Write(ms, data.Document, false, notification: this.onNotification);
			data.Stream = new MemoryStream(ms.ToArray());
		}
		else
		{
			DxfWriter.Write(path, data.Document, false, notification: this.onNotification);
		}

		if (TestVariables.SelfCheckOutput)
		{
			this._output.WriteLine("--- starting read ---");

			CadDocument doc = null;
			if (TestVariables.SaveOutputInStream)
			{
				doc = DxfReader.Read(data.Stream, this.onNotification);
			}
			else
			{
				doc = DxfReader.Read(path, this.onNotification);
			}
		}
	}
}
