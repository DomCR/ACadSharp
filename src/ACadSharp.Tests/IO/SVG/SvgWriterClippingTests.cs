using ACadSharp;
using ACadSharp.Entities;
using ACadSharp.IO;
using ACadSharp.Objects;
using ACadSharp.Tests.IO;
using ACadSharp.Tables;
using CSMath;
using System.IO;
using Xunit;
using Xunit.Abstractions;

namespace ACadSharp.Tests.IO.SVG
{
	public class SvgWriterClippingTests : IOTestsBase
	{
		public SvgWriterClippingTests(ITestOutputHelper output) : base(output)
		{
		}

		[Fact]
		public void WriteSingleWipeout()
		{
			CadDocument document = new CadDocument();

			document.Entities.Add(new Line(new XYZ(-10, 0, 0), new XYZ(110, 0, 0))
			{
				Color = Color.Red
			});

			Wipeout wipeout = new Wipeout
			{
				InsertPoint = XYZ.Zero,
				Size = new XY(100, 100),
				ClippingState = true
			};

			wipeout.ClipBoundaryVertices.Add(new XY(0, 0));
			wipeout.ClipBoundaryVertices.Add(new XY(0, 100));
			wipeout.ClipBoundaryVertices.Add(new XY(100, 100));
			wipeout.ClipBoundaryVertices.Add(new XY(100, 0));

			document.Entities.Add(wipeout);

			using SvgWriter writer = this.createWriter("wipeout.svg", document);
			writer.Write();
		}

		[Fact]
		public void WriteInsertWithSpatialFilter()
		{
			CadDocument document = new CadDocument();
			BlockRecord block = new BlockRecord("clip_block");

			block.Entities.Add(new Solid
			{
				FirstCorner = new XYZ(-30, -20, 0),
				SecondCorner = new XYZ(170, -20, 0),
				ThirdCorner = new XYZ(170, 110, 0),
				FourthCorner = new XYZ(-30, 110, 0),
				Color = Color.Black
			});

			block.Entities.Add(new Solid
			{
				FirstCorner = new XYZ(18, 18, 0),
				SecondCorner = new XYZ(102, 18, 0),
				ThirdCorner = new XYZ(102, 78, 0),
				FourthCorner = new XYZ(18, 78, 0),
				Color = Color.Default
			});

			block.Entities.Add(new Line(new XYZ(-40, 18, 0), new XYZ(160, 18, 0))
			{
				Color = Color.Blue
			});

			block.Entities.Add(new Line(new XYZ(12, -30, 0), new XYZ(12, 120, 0))
			{
				Color = Color.Green
			});

			block.Entities.Add(new Line(new XYZ(-20, 105, 0), new XYZ(140, -15, 0))
			{
				Color = Color.Red
			});

			block.Entities.Add(new Line(new XYZ(-35, -20, 0), new XYZ(160, 110, 0))
			{
				Color = Color.Magenta
			});

			block.Entities.Add(new Circle
			{
				Center = new XYZ(75, 52, 0),
				Radius = 26,
				Color = Color.Black
			});

			block.Entities.Add(new Circle
			{
				Center = new XYZ(132, 78, 0),
				Radius = 14,
				Color = Color.Cyan
			});

			Insert insert = new Insert(block);
			insert.InsertPoint = new XYZ(140, 90, 0);

			SpatialFilter filter = new SpatialFilter
			{
				DisplayBoundary = true
			};

			filter.BoundaryPoints.Add(new XY(20, 12));
			filter.BoundaryPoints.Add(new XY(96, 82));

			insert.SpatialFilter = filter;
			document.Entities.Add(insert);

			using SvgWriter writer = this.createWriter("spatial-filter.svg", document);
			writer.Write();
		}

		private SvgWriter createWriter(string filename, CadDocument doc)
		{
			string output = Path.Combine(TestVariables.OutputSvgFolder, filename);

			var writer = new SvgWriter(output, doc);
			writer.Configuration = this._svgConfiguration;
			writer.OnNotification += this.onNotification;
			return writer;
		}
	}
}
