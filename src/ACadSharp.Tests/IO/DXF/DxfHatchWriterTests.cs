using ACadSharp.Entities;
using ACadSharp.IO;
using CSMath;
using System.IO;
using System.Linq;
using Xunit;

namespace ACadSharp.Tests.IO.DXF;

public class DxfHatchWriterTests
{
	[Fact]
	public void WriteHatchPreservesFullArcAndEllipseSweeps()
	{
		Hatch hatch = new Hatch { IsSolid = true };
		hatch.SeedPoints.Add(XY.Zero);

		Hatch.BoundaryPath arcPath = new Hatch.BoundaryPath();
		arcPath.Edges.Add(new Hatch.BoundaryPath.Arc
		{
			Center = XY.Zero,
			Radius = 2,
			StartAngle = -MathHelper.HalfPI,
			EndAngle = MathHelper.ThreeHalfPI,
			CounterClockWise = true,
		});
		hatch.Paths.Add(arcPath);

		Hatch.BoundaryPath ellipsePath = new Hatch.BoundaryPath();
		ellipsePath.Edges.Add(new Hatch.BoundaryPath.Ellipse
		{
			Center = new XY(5, 0),
			MajorAxisEndPoint = new XY(2, 0),
			RadiusRatio = 0.5,
			StartAngle = -MathHelper.HalfPI,
			EndAngle = MathHelper.ThreeHalfPI,
			CounterClockWise = true,
		});
		hatch.Paths.Add(ellipsePath);

		CadDocument document = new CadDocument();
		document.Entities.Add(hatch);
		using MemoryStream output = new MemoryStream();
		DxfWriter.Write(output, document);

		using MemoryStream input = new MemoryStream(output.ToArray());
		CadDocument result = DxfReader.Read(input);
		Hatch writtenHatch = Assert.Single(result.Entities.OfType<Hatch>());
		Hatch.BoundaryPath.Arc arc = Assert.Single(
			writtenHatch.Paths.SelectMany(p => p.Edges).OfType<Hatch.BoundaryPath.Arc>()
		);
		Hatch.BoundaryPath.Ellipse ellipse = Assert.Single(
			writtenHatch.Paths.SelectMany(p => p.Edges).OfType<Hatch.BoundaryPath.Ellipse>()
		);

		Assert.Equal(-MathHelper.HalfPI, arc.StartAngle, 12);
		Assert.Equal(MathHelper.ThreeHalfPI, arc.EndAngle, 12);
		Assert.Equal(-MathHelper.HalfPI, ellipse.StartAngle, 12);
		Assert.Equal(MathHelper.ThreeHalfPI, ellipse.EndAngle, 12);
	}
}
