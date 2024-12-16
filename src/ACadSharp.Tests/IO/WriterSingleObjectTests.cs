using ACadSharp.Entities;
using ACadSharp.Objects;
using ACadSharp.Tables;
using CSMath;
using CSUtilities.Extensions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Xunit;
using Xunit.Abstractions;

namespace ACadSharp.Tests.IO
{
	public abstract class WriterSingleObjectTests : IOTestsBase
	{
		public class SingleCaseGenerator : IXunitSerializable
		{
			public string Name { get; private set; }

			public CadDocument Document { get; private set; } = new CadDocument();

			public SingleCaseGenerator()
			{
				this.Document.Header.ShowModelSpace = true;
			}

			public SingleCaseGenerator(string name) : this()
			{
				this.Name = name;
			}

			public override string ToString()
			{
				return this.Name;
			}

			public void Empty() { }

			public void DefaultLayer()
			{
				this.Document.Layers.Add(new Layer("default_layer"));
			}

			public void LayerTrueColor()
			{
				Layer layer = new Layer("Layer_true_color");
				layer.Color = Color.FromTrueColor(1151726);

				this.Document.Layers.Add(layer);
			}

			public void EntityColorByLayer()
			{
				Layer layer = new Layer("Test");
				layer.Color = new Color(25);
				this.Document.Layers.Add(layer);

				Circle c = new Circle();
				c.Center = new XYZ(0, 0, 0);
				c.Radius = 10;
				c.Layer = layer;
				c.Color = Color.ByLayer;

				this.Document.Entities.Add(c);
			}

			public void EntityColorTrueColor()
			{
				Circle c = new Circle();
				c.Center = new XYZ(0, 0, 0);
				c.Radius = 10;
				c.Color = Color.FromTrueColor(1151726);

				this.Document.Entities.Add(c);
			}

			public void EntityChangeNormal()
			{
				Circle c = new Circle();
				c.Center = new XYZ(0, 0, 0);
				c.Radius = 10;

				c.Normal = XYZ.AxisX;

				this.Document.Entities.Add(c);
			}

			public void EntityColorByIndex()
			{
				Circle c = new Circle();
				c.Center = new XYZ(0, 0, 0);
				c.Radius = 10;
				c.Color = new Color(11);

				this.Document.Entities.Add(c);
			}

			public void CurrentEntityColorTrueColor()
			{
				this.Document.Header.CurrentEntityColor = Color.FromTrueColor(1151726);
			}

			public void CurrentEntityByIndex()
			{
				this.Document.Header.CurrentEntityColor = new Color(11);
			}

			public void CurrentEntityByBlock()
			{
				this.Document.Header.CurrentEntityColor = Color.ByBlock;
			}

			public void SingleEllipse()
			{
				Ellipse ellipse = new Ellipse();
				ellipse.RadiusRatio = 0.5d;
				ellipse.StartParameter = 0.0d;
				ellipse.EndParameter = Math.PI * 2;

				this.Document.Entities.Add(ellipse);
			}

			public void SingleLine()
			{
				Line line = new Line(XYZ.Zero, new XYZ(100, 100, 0));

				this.Document.Entities.Add(line);
			}

			public void SingleMLine()
			{
				//It creates a valid dxf but the MLine is wrongly drawn

				MLine line = new MLine();

				line.StartPoint = XYZ.Zero;

				var v1 = new MLine.Vertex();
				v1.Position = XYZ.Zero;
				v1.Direction = XYZ.AxisY;

				v1.Segments.Add(new MLine.Vertex.Segment { Parameters = new List<double> { 0.75, 0 } });
				v1.Segments.Add(new MLine.Vertex.Segment { Parameters = new List<double> { -0.75, 0 } });

				var v2 = new MLine.Vertex();
				v2.Position = new XYZ(100, 100, 0);
				v2.Direction = XYZ.AxisY;

				v2.Segments.Add(new MLine.Vertex.Segment { Parameters = new List<double> { 0.75, 0 } });
				v2.Segments.Add(new MLine.Vertex.Segment { Parameters = new List<double> { -0.75, 0 } });

				line.Vertices.Add(v1);
				line.Vertices.Add(v2);

				this.Document.Entities.Add(line);
			}

			public void SingleMText()
			{
				MText mtext = new MText();

				mtext.Value = "HELLO I'm an MTEXT";

				this.Document.Entities.Add(mtext);
			}

			public void SingleMTextSpecialCharacter()
			{
				MText mtext = new MText();

				mtext.Value = "∅45,6";

				this.Document.Entities.Add(mtext);
			}

			public void SingleMTextMultiline()
			{
				MText mtext = new MText();

				mtext.Value = "HELLO I'm an MTEXT\n and I have multiple lines";

				this.Document.Entities.Add(mtext);
			}

			public void SinglePoint()
			{
				this.Document.Entities.Add(new Point(XYZ.Zero));
			}

			public void SingleWipeout()
			{
				Wipeout wipeout = new Wipeout();

				wipeout.Size = new XY(1, 1);
				wipeout.ClippingState = true;

				wipeout.ClipBoundaryVertices.Add(new XY(0, 0));
				wipeout.ClipBoundaryVertices.Add(new XY(0, 1));
				wipeout.ClipBoundaryVertices.Add(new XY(1, 1));
				wipeout.ClipBoundaryVertices.Add(new XY(1, 0));

				this.Document.Entities.Add(wipeout);
			}

			public void SingleRasterImage()
			{
				ImageDefinition definition = new ImageDefinition();
				definition.Size = new XY(1, 1);
				definition.Name = "image";

				definition.FileName = "..\\..\\image.JPG";

				RasterImage raster = new RasterImage(definition);

				raster.ClipBoundaryVertices.Add(new XY(0, 0));
				raster.ClipBoundaryVertices.Add(new XY(0, 1));
				raster.ClipBoundaryVertices.Add(new XY(1, 1));
				raster.ClipBoundaryVertices.Add(new XY(1, 0));

				this.Document.Entities.Add(raster);
			}

			public void CreateLayout()
			{
				//Draw a cross in the model
				this.Document.Entities.Add(new Line(XYZ.Zero, new XYZ(100, 100, 0)));
				this.Document.Entities.Add(new Line(new XYZ(0, 100, 0), new XYZ(100, 0, 0)));

				Layout layout = new Layout("my_layout");

				this.Document.Layouts.Add(layout);
			}

			public void LineTypeWithSegments()
			{
				LineType lt = new LineType("segmented");
				lt.Description = "hello";

				LineType.Segment s1 = new LineType.Segment
				{
					Length = 12,
					//Style = this.Document.TextStyles[TextStyle.DefaultName]
				};

				LineType.Segment s2 = new LineType.Segment
				{
					Length = -3,
					//Style = this.Document.TextStyles[TextStyle.DefaultName]
				};

				lt.AddSegment(s1);
				lt.AddSegment(s2);

				this.Document.LineTypes.Add(lt);
			}

			public void ClosedLwPolyline()
			{
				List<LwPolyline.Vertex> vertices = new List<LwPolyline.Vertex>() {
				new LwPolyline.Vertex(new XY(0,0)),
				new LwPolyline.Vertex(new XY(1,0)),
				new LwPolyline.Vertex(new XY(2,1)),
				new LwPolyline.Vertex(new XY(3,1)),
				new LwPolyline.Vertex(new XY(4,4))
				};

				var lwPline = new LwPolyline();

				for (int i = 0; i < vertices.Count; i++)
					lwPline.Vertices.Add(vertices[i]);

				lwPline.IsClosed = true;

				lwPline.Vertices[2].Bulge = -0.5;
				this.Document.Entities.Add(lwPline);
			}

			public void ClosedPolyline2DTest()
			{
				List<Vertex2D> vector2d = new()
				{
					new Vertex2D() { Location = new XYZ(0, 0, 0) },
					new Vertex2D() { Location = new XYZ(1, 0, 0) },
					new Vertex2D() { Location = new XYZ(2, 1, 0) },
					new Vertex2D() { Location = new XYZ(3, 1, 0) },
					new Vertex2D() { Location = new XYZ(4, 4, 0) }
				};

				var pline = new Polyline2D();
				pline.Vertices.AddRange(vector2d);
				pline.IsClosed = true;
				pline.Vertices.ElementAt(3).Bulge = 1;

				this.Document.Entities.Add(pline);
			}

			public void EntityTransparency()
			{
				Line line = new Line(XYZ.Zero, new XYZ(100, 100, 0));

				line.Transparency = new Transparency(50);

				this.Document.Entities.Add(line);
			}

			public void CreateHatchPolyline()
			{
				Hatch hatch = new Hatch();
				hatch.IsSolid = true;

				Hatch.BoundaryPath path = new Hatch.BoundaryPath();

				Hatch.BoundaryPath.Polyline pline = new Hatch.BoundaryPath.Polyline();
				pline.Vertices.Add(new XYZ(0, 0, 0));
				pline.Vertices.Add(new XYZ(1, 0, 0));
				pline.Vertices.Add(new XYZ(1, 1, 0));
				pline.Vertices.Add(new XYZ(0, 1, 0));
				pline.Vertices.Add(new XYZ(0, 0, 0));

				path.Edges.Add(pline);
				path.Flags = path.Flags.AddFlag(BoundaryPathFlags.Polyline);
				hatch.Paths.Add(path);

				this.Document.Entities.Add(hatch);
			}

			public void CreateHatch()
			{
				Hatch hatch = new Hatch();
				hatch.IsSolid = true;

				hatch.SeedPoints.Add(new XY());

				List<Hatch.BoundaryPath.Line> edges = new List<Hatch.BoundaryPath.Line>();

				//edges
				Hatch.BoundaryPath.Line edge1 = new Hatch.BoundaryPath.Line
				{
					Start = new CSMath.XY(0, 0),
					End = new CSMath.XY(1, 0)
				};
				edges.Add(edge1);

				Hatch.BoundaryPath.Line edge2 = new Hatch.BoundaryPath.Line
				{
					Start = new CSMath.XY(1, 0),
					End = new CSMath.XY(1, 1)
				};
				edges.Add(edge2);

				Hatch.BoundaryPath.Line edge3 = new Hatch.BoundaryPath.Line
				{
					Start = new CSMath.XY(1, 1),
					End = new CSMath.XY(0, 1)
				};
				edges.Add(edge3);

				Hatch.BoundaryPath.Line edge4 = new Hatch.BoundaryPath.Line
				{
					Start = new CSMath.XY(0, 1),
					End = new CSMath.XY(0, 0)
				};
				edges.Add(edge4);


				Hatch.BoundaryPath path = new Hatch.BoundaryPath();
				foreach (var item in edges)
				{
					path.Edges.Add(item);
				}

				hatch.Paths.Add(path);

				this.Document.Entities.Add(hatch);
			}

			public void CreateCircleHatch()
			{
				Hatch hatch = new Hatch();
				hatch.IsSolid = true;

				hatch.SeedPoints.Add(new XY());

				List<Hatch.BoundaryPath.Line> edges = new List<Hatch.BoundaryPath.Line>();

				//edges
				Hatch.BoundaryPath.Polyline polyline = new Hatch.BoundaryPath.Polyline();
				polyline.IsClosed = true;
				polyline.Vertices.Add(new XYZ(0, 2.5, 1));
				polyline.Vertices.Add(new XYZ(10, 2.5, 1));

				Hatch.BoundaryPath path = new Hatch.BoundaryPath();
				foreach (var item in edges)
				{
					path.Edges.Add(item);
				}

				hatch.Paths.Add(path);

				this.Document.Entities.Add(hatch);
			}

			public void ChangedEncoding()
			{
				this.Document.Header.CodePage = "gb2312";
				this.Document.Layers.Add(new Layer("我的自定义层"));
			}

			public void AddCustomScale()
			{
				this.Document.Scales.Add(new Scale("Hello"));
			}

			public void Dimensions()
			{
				DimensionAligned dim = new DimensionAligned();

				dim.SecondPoint = new XYZ(10);

				this.Document.Entities.Add(dim);
			}

			public void AddCustomBookColor()
			{
				//var color = new BookColor("RAL CLASSIC$RAL 1006");
				var color = new BookColor("TEST BOOK$MY COLOR");
				color.Color = new(226, 144, 0);

				Line line = new Line(XYZ.Zero, new XYZ(100, 100, 0));
				line.Color = Color.ByBlock;
				line.BookColor = color;

				this.Document.Colors.Add(color);
				this.Document.Entities.Add(line);
			}

			public void AddBlockWithAttributes()
			{
				BlockRecord record = new("my_block");

				record.Entities.Add(new Circle
				{
					Radius = 10,
					Center = XYZ.Zero
				});

				record.Entities.Add(new AttributeDefinition()
				{
					InsertPoint = XYZ.Zero,
					Prompt = "Name_custom",
					Tag = "CIRCLE_NAME",
					Value = "Circilla",
					HorizontalAlignment = TextHorizontalAlignment.Left,
					Height = 18,
					AttributeType = AttributeType.SingleLine,
				});

				this.Document.BlockRecords.Add(record);

				var insert = new Insert(record)
				{
					InsertPoint = new XYZ(0, 0, 0),
					XScale = 0.8,
					YScale = 0.8,
				};

				this.Document.Entities.Add(insert);
			}

			public void Deserialize(IXunitSerializationInfo info)
			{
				this.Name = info.GetValue<string>(nameof(this.Name));
				this.GetType().GetMethod(this.Name).Invoke(this, null);
			}

			public void Serialize(IXunitSerializationInfo info)
			{
				info.AddValue(nameof(this.Name), this.Name);
			}
		}

		public static readonly TheoryData<SingleCaseGenerator> Data;

		public WriterSingleObjectTests(ITestOutputHelper output) : base(output) { }

		static WriterSingleObjectTests()
		{
			Data = new();
			if (!TestVariables.RunDwgWriterSingleCases)
			{
				Data.Add(new(nameof(SingleCaseGenerator.Empty)));
				return;
			}

			Data.Add(new(nameof(SingleCaseGenerator.Empty)));
			Data.Add(new(nameof(SingleCaseGenerator.SingleEllipse)));
			Data.Add(new(nameof(SingleCaseGenerator.SingleLine)));
			Data.Add(new(nameof(SingleCaseGenerator.SingleMLine)));
			Data.Add(new(nameof(SingleCaseGenerator.EntityColorByLayer)));
			Data.Add(new(nameof(SingleCaseGenerator.EntityColorTrueColor)));
			Data.Add(new(nameof(SingleCaseGenerator.EntityChangeNormal)));
			Data.Add(new(nameof(SingleCaseGenerator.EntityColorByIndex)));
			Data.Add(new(nameof(SingleCaseGenerator.CurrentEntityColorTrueColor)));
			Data.Add(new(nameof(SingleCaseGenerator.CurrentEntityByIndex)));
			Data.Add(new(nameof(SingleCaseGenerator.CurrentEntityByBlock)));
			Data.Add(new(nameof(SingleCaseGenerator.DefaultLayer)));
			Data.Add(new(nameof(SingleCaseGenerator.LayerTrueColor)));
			Data.Add(new(nameof(SingleCaseGenerator.SingleMText)));
			Data.Add(new(nameof(SingleCaseGenerator.SingleMTextSpecialCharacter)));
			Data.Add(new(nameof(SingleCaseGenerator.SingleMTextMultiline)));
			Data.Add(new(nameof(SingleCaseGenerator.SinglePoint)));
			Data.Add(new(nameof(SingleCaseGenerator.ClosedLwPolyline)));
			Data.Add(new(nameof(SingleCaseGenerator.ClosedPolyline2DTest)));
			Data.Add(new(nameof(SingleCaseGenerator.SingleRasterImage)));
			Data.Add(new(nameof(SingleCaseGenerator.SingleWipeout)));
			Data.Add(new(nameof(SingleCaseGenerator.CreateLayout)));
			Data.Add(new(nameof(SingleCaseGenerator.EntityTransparency)));
			Data.Add(new(nameof(SingleCaseGenerator.LineTypeWithSegments)));
			Data.Add(new(nameof(SingleCaseGenerator.CreateHatchPolyline)));
			Data.Add(new(nameof(SingleCaseGenerator.CreateHatch)));
			Data.Add(new(nameof(SingleCaseGenerator.CreateCircleHatch)));
			Data.Add(new(nameof(SingleCaseGenerator.ChangedEncoding)));
			Data.Add(new(nameof(SingleCaseGenerator.AddBlockWithAttributes)));
			Data.Add(new(nameof(SingleCaseGenerator.AddCustomScale)));
			Data.Add(new(nameof(SingleCaseGenerator.AddCustomBookColor)));
			Data.Add(new(nameof(SingleCaseGenerator.Dimensions)));
		}

		protected string getPath(string name, string ext, ACadVersion version)
		{
			return Path.Combine(TestVariables.OutputSingleCasesFolder, $"{name}_{version}.{ext}");
		}
	}
}