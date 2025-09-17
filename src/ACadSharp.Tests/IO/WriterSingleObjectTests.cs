using ACadSharp.Blocks;
using ACadSharp.Entities;
using ACadSharp.Extensions;
using ACadSharp.Objects;
using ACadSharp.Tables;
using ACadSharp.XData;
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
		public static readonly TheoryData<SingleCaseGenerator> Data;

		static WriterSingleObjectTests()
		{
			Data = new();
			if (!TestVariables.RunDwgWriterSingleCases)
			{
				Data.Add(new(nameof(SingleCaseGenerator.Empty)));
				return;
			}

			Data.Add(new(nameof(SingleCaseGenerator.Empty)));
			Data.Add(new(nameof(SingleCaseGenerator.ArcSegments)));
			Data.Add(new(nameof(SingleCaseGenerator.SingleEllipse)));
			Data.Add(new(nameof(SingleCaseGenerator.SingleLine)));
			Data.Add(new(nameof(SingleCaseGenerator.ViewZoom)));
			Data.Add(new(nameof(SingleCaseGenerator.SingleMLeader)));
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
			Data.Add(new(nameof(SingleCaseGenerator.SingleMTextRotation)));
			Data.Add(new(nameof(SingleCaseGenerator.SingleMTextSpecialCharacter)));
			Data.Add(new(nameof(SingleCaseGenerator.TextWithChineseCharacters)));
			Data.Add(new(nameof(SingleCaseGenerator.TextAlignment)));
			Data.Add(new(nameof(SingleCaseGenerator.CreateGroup)));
			Data.Add(new(nameof(SingleCaseGenerator.SingleMTextMultiline)));
			Data.Add(new(nameof(SingleCaseGenerator.SinglePoint)));
			Data.Add(new(nameof(SingleCaseGenerator.ClosedLwPolyline)));
			Data.Add(new(nameof(SingleCaseGenerator.ClosedPolyline2DTest)));
			Data.Add(new(nameof(SingleCaseGenerator.ClosedPolyline3DTest)));
			Data.Add(new(nameof(SingleCaseGenerator.SinglePdfUnderlay)));
			Data.Add(new(nameof(SingleCaseGenerator.SingleRasterImage)));
			Data.Add(new(nameof(SingleCaseGenerator.SingleWipeout)));
			Data.Add(new(nameof(SingleCaseGenerator.CreateLayout)));
			Data.Add(new(nameof(SingleCaseGenerator.EntityTransparency)));
			Data.Add(new(nameof(SingleCaseGenerator.LineTypeWithSegments)));
			Data.Add(new(nameof(SingleCaseGenerator.CreateInsertWithHatch)));
			Data.Add(new(nameof(SingleCaseGenerator.InsertWithSpatialFilter)));
			Data.Add(new(nameof(SingleCaseGenerator.CreateHatchPolyline)));
			Data.Add(new(nameof(SingleCaseGenerator.CreateHatch)));
			Data.Add(new(nameof(SingleCaseGenerator.CreateCircleHatch)));
			Data.Add(new(nameof(SingleCaseGenerator.ChangedEncoding)));
			Data.Add(new(nameof(SingleCaseGenerator.AddBlockWithAttributes)));
			Data.Add(new(nameof(SingleCaseGenerator.AddCustomScale)));
			Data.Add(new(nameof(SingleCaseGenerator.AddCustomBookColor)));
			Data.Add(new(nameof(SingleCaseGenerator.DimensionsInBlock)));
			Data.Add(new(nameof(SingleCaseGenerator.DimensionAligned)));
			Data.Add(new(nameof(SingleCaseGenerator.DimensionLinear)));
			Data.Add(new(nameof(SingleCaseGenerator.DimensionOrdinate)));
			Data.Add(new(nameof(SingleCaseGenerator.DimensionAngular2Line)));
			Data.Add(new(nameof(SingleCaseGenerator.DimensionAngular3Pt)));
			Data.Add(new(nameof(SingleCaseGenerator.DimensionDiameter)));
			Data.Add(new(nameof(SingleCaseGenerator.DimensionRadius)));
			Data.Add(new(nameof(SingleCaseGenerator.Dimensions)));
			Data.Add(new(nameof(SingleCaseGenerator.DimensionWithLineType)));
			Data.Add(new(nameof(SingleCaseGenerator.GeoData)));
			Data.Add(new(nameof(SingleCaseGenerator.TextAlignment)));
			Data.Add(new(nameof(SingleCaseGenerator.LineTypeInBlock)));
			Data.Add(new(nameof(SingleCaseGenerator.XData)));
			Data.Add(new(nameof(SingleCaseGenerator.XRef)));
			Data.Add(new(nameof(SingleCaseGenerator.SPlineCreation)));
			Data.Add(new(nameof(SingleCaseGenerator.CreateXRecords)));
		}

		public WriterSingleObjectTests(ITestOutputHelper output) : base(output)
		{
		}

		protected string getPath(string name, string ext, ACadVersion version)
		{
			return Path.Combine(TestVariables.OutputSingleCasesFolder, $"{name}_{version}.{ext}");
		}

		public class SingleCaseGenerator : IXunitSerializable
		{
			public CadDocument Document { get; private set; } = new CadDocument();

			public string Name { get; private set; }

			public SingleCaseGenerator()
			{
				this.Document.Header.ShowModelSpace = true;
			}

			public SingleCaseGenerator(string name) : this()
			{
				this.Name = name;
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

				record.Entities.Add(new AttributeDefinition()
				{
					InsertPoint = new XYZ(10, 10, 0),
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

				insert.Attributes.Add(new AttributeEntity()
				{
					InsertPoint = new XYZ(-10, -10, 0),
					Tag = "CIRCLE_NAME_ATT",
					Value = "Bla",
					HorizontalAlignment = TextHorizontalAlignment.Left,
					Height = 18,
					AttributeType = AttributeType.SingleLine,
				});

				this.Document.Entities.Add(insert);
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

				color = new BookColor("TEST");
				color.Color = new(226, 144, 0);

				line = new Line(new XYZ(100, 100, 0), XYZ.Zero);
				line.Color = Color.ByBlock;
				line.BookColor = color;

				this.Document.Colors.Add(color);
				this.Document.Entities.Add(line);
			}

			public void AddCustomScale()
			{
				this.Document.Scales.Add(new Scale("Hello"));
			}

			public void ChangedEncoding()
			{
				this.Document.Header.CodePage = "gb2312";
				this.Document.Layers.Add(new Layer("我的自定义层"));
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

			public void ClosedPolyline3DTest()
			{
				List<Vertex3D> vector2d = new()
				{
					new Vertex3D() { Location = new XYZ(0, 0, 0) },
					new Vertex3D() { Location = new XYZ(1, 0, 0) },
					new Vertex3D() { Location = new XYZ(2, 1, 0) },
					new Vertex3D() { Location = new XYZ(3, 1, 0) },
					new Vertex3D() { Location = new XYZ(4, 4, 0) }
				};

				var pline = new Polyline3D();
				pline.Vertices.AddRange(vector2d);
				pline.IsClosed = true;
				pline.Vertices.ElementAt(3).Bulge = 1;

				this.Document.Entities.Add(pline);
			}

			public void CreateCircleHatch()
			{
				Hatch hatch = new Hatch();
				hatch.IsSolid = true;

				hatch.SeedPoints.Add(new XY());

				List<Hatch.BoundaryPath.Line> edges = new List<Hatch.BoundaryPath.Line>();

				//Polyline circle
				Hatch.BoundaryPath.Polyline polyline = new Hatch.BoundaryPath.Polyline();
				polyline.IsClosed = true;
				polyline.Vertices.Add(new XYZ(0, 2.5, 1));
				polyline.Vertices.Add(new XYZ(10, 2.5, 1));

				//Arc circle
				Hatch.BoundaryPath.Arc arc = new();
				arc.Center = new XY(10, 10);
				arc.CounterClockWise = true;
				arc.Radius = 5;
				arc.StartAngle = 0;
				arc.EndAngle = MathHelper.TwoPI;

				Hatch.BoundaryPath path = new Hatch.BoundaryPath();
				path.Edges.Add(polyline);

				Hatch.BoundaryPath path1 = new Hatch.BoundaryPath();
				path1.Edges.Add(arc);

				hatch.Paths.Add(path);
				hatch.Paths.Add(path1);

				this.Document.Entities.Add(hatch);
			}

			public void ArcSegments()
			{
				Arc arc = new Arc()
				{
					Center = new XYZ(100, 0, 0),
					Radius = 50,
					StartAngle = MathHelper.HalfPI,
					EndAngle = Math.PI,
				};

				XYZ start = new XYZ(100, 50, 0);
				XYZ end = new XYZ(50, 0, 0);

				var v = arc.PolygonalVertexes(3);

				Polyline2D polyline = new Polyline2D(v.Select(a => new Vertex2D(a)), false);

				arc.GetEndVertices(out XYZ s, out XYZ e);

				this.Document.Entities.Add(arc);
				this.Document.Entities.Add(polyline);
			}

			public void CreateGroup()
			{
				Layer layer = new Layer("MyLayer");
				layer.Color = new Color(0, 153, 0);
				this.Document.Layers.Add(layer);

				Circle circle = new Circle();
				circle.Center = new CSMath.XYZ(1, 1, 0);
				circle.Radius = 1;
				circle.Normal = new CSMath.XYZ(0, 0, 1);

				Line line = new Line();
				line.StartPoint = new CSMath.XYZ(0, 0, 0);
				line.EndPoint = new CSMath.XYZ(2, 2, 0);

				circle.Layer = layer;
				line.Layer = layer;

				this.Document.Entities.Add(circle);
				this.Document.Entities.Add(line);

				//Group group = new Group();
				//group.Name = "MyGroup";
				//group.Add(circle);
				//group.Add(line);

				this.Document.Groups.CreateGroup("MyGroup", new List<Entity> { circle, line });

				TextEntity text = new TextEntity();
				text.Value = "Hello World!";
				text.Layer = layer;
				text.HorizontalAlignment = TextHorizontalAlignment.Center;
				text.VerticalAlignment = TextVerticalAlignmentType.Middle;
				text.InsertPoint = new CSMath.XYZ(1, 1, 0);
				text.AlignmentPoint = new CSMath.XYZ(10, 10, 0);

				this.Document.Entities.Add(text);
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
				path.Flags |= BoundaryPathFlags.Polyline;
				hatch.Paths.Add(path);

				this.Document.Entities.Add(hatch);
			}

			public void CreateInsertWithHatch()
			{
				CadDocument doc = this.Document;
				var modelSpace = doc.ModelSpace;

				string blockName = Guid.NewGuid().ToString();
				var blockRecord = new BlockRecord(blockName);
				var insert = new Insert(blockRecord);
				modelSpace.Entities.Add(insert);

				var hatch = new Hatch()
				{
					Pattern = HatchPattern.Solid,
					Color = new ACadSharp.Color(0, 0, 0),
					IsAssociative = false,
					IsSolid = true,
					PatternType = HatchPatternType.SolidFill,
					IsInvisible = false,
					Style = HatchStyleType.Normal,
				};

				var path = new Hatch.BoundaryPath
				{
					Flags = BoundaryPathFlags.External,
				};

				path.Edges.Add(new Hatch.BoundaryPath.Polyline()
				{
					Vertices = [new(0, 0, 0), new(0, 5, 0), new(5, 5, 0), new(5, 0, 0)],
					IsClosed = true,
				});

				hatch.Paths.Add(path);

				blockRecord.Entities.Add(hatch);
			}

			public void InsertWithSpatialFilter()
			{
				string blockName = Guid.NewGuid().ToString();
				var blockRecord = new BlockRecord("my_block");
				var insert = new Insert(blockRecord);

				SpatialFilter filter = new SpatialFilter();
				filter.BoundaryPoints.Add(XY.Zero);
				filter.BoundaryPoints.Add(new XY(50, 50));
				filter.DisplayBoundary = true;

				insert.SpatialFilter = filter;

				this.Document.Entities.Add(insert);

				Circle circle = new Circle
				{
					Radius = 20
				};
				blockRecord.Entities.Add(circle);
			}

			public void CreateLayout()
			{
				//Draw a cross in the model
				this.Document.Entities.Add(new Line(XYZ.Zero, new XYZ(100, 100, 0)));
				this.Document.Entities.Add(new Line(new XYZ(0, 100, 0), new XYZ(100, 0, 0)));

				Layout layout = new Layout("my_layout");

				this.Document.Layouts.Add(layout);
			}

			public void CreateXRecords()
			{
				Layer lay = new Layer("my_layer");

				//Extracted form a real case
				var dict = lay.CreateExtendedDictionary();
				var layerstates = new CadDictionary("ACAD_LAYERSTATES");
				dict.Add(layerstates);

				XRecord record = new XRecord("test");
				record.CreateEntry(90, 1);
				record.CreateEntry(330, Document.Layers);
				layerstates.Add(record);

				this.Document.Layers.Add(lay);
			}

			public void CurrentEntityByBlock()
			{
				this.Document.Header.CurrentEntityColor = Color.ByBlock;
			}

			public void CurrentEntityByIndex()
			{
				this.Document.Header.CurrentEntityColor = new Color(11);
			}

			public void CurrentEntityColorTrueColor()
			{
				this.Document.Header.CurrentEntityColor = Color.FromTrueColor(1151726);
			}

			public void DefaultLayer()
			{
				this.Document.Layers.Add(new Layer("default_layer"));
			}

			public void Deserialize(IXunitSerializationInfo info)
			{
				this.Name = info.GetValue<string>(nameof(this.Name));
				this.GetType().GetMethod(this.Name).Invoke(this, null);
			}

			public void DimensionAligned()
			{
				//DimensionAligned dim = new DimensionAligned
				//{
				//	SecondPoint = new XYZ(10, 0, 0),
				//	Offset = 0.5,
				//	//DefinitionPoint = new XYZ(10, 1, 0),
				//	//TextMiddlePoint = new XYZ(5, 1, 0)
				//};

				DimensionAligned dim1 = new DimensionAligned
				{
					SecondPoint = new XYZ(10, 0, 0),
					Offset = 2,
					TextMiddlePoint = new XYZ(5, 1, 0)
				};

				//this.Document.Entities.Add(dim);
				this.Document.Entities.Add(dim1);

				//dim.UpdateBlock();
				dim1.UpdateBlock();
			}

			public void DimensionAngular2Line()
			{
				DimensionAngular2Line dim = new DimensionAngular2Line();
				dim.FirstPoint = XYZ.AxisY;
				dim.SecondPoint = -XYZ.AxisY;

				dim.DefinitionPoint = -XYZ.AxisX;
				dim.AngleVertex = XYZ.AxisX;

				this.Document.Entities.Add(dim);

				dim.UpdateBlock();
			}

			public void DimensionAngular3Pt()
			{
				return;

				DimensionAngular3Pt dim = new DimensionAngular3Pt();
				dim.FirstPoint = XYZ.AxisY;
				dim.SecondPoint = XYZ.AxisX;

				dim.DefinitionPoint = XYZ.Zero;
				dim.AngleVertex = XYZ.AxisY;

				this.Document.Entities.Add(dim);

				dim.UpdateBlock();
			}

			public void DimensionDiameter()
			{
				DimensionDiameter dim = new DimensionDiameter
				{
					AngleVertex = new XYZ(10, 10, 0),
				};

				this.Document.Entities.Add(dim);

				dim.UpdateBlock();

				dim = new DimensionDiameter
				{
					DefinitionPoint = new XYZ(0, 0, 0),
					AngleVertex = new XYZ(10, 0, 0),
				};

				this.Document.Entities.Add(dim);

				dim.UpdateBlock();
			}

			public void DimensionLinear()
			{
				DimensionLinear dim = new DimensionLinear
				{
					SecondPoint = new XYZ(10, 10, 0),
					Offset = 0.5,
					//DefinitionPoint = new XYZ(10, 1, 0),
					//TextMiddlePoint = new XYZ(5, 1, 0)
				};

				DimensionLinear dim1 = new DimensionLinear
				{
					SecondPoint = new XYZ(10, 0, 0),
					Offset = 2,
					TextMiddlePoint = new XYZ(5, 1, 0)
				};

				this.Document.Entities.Add(dim);
				this.Document.Entities.Add(dim1);

				dim.UpdateBlock();
				dim1.UpdateBlock();
			}

			public void DimensionOrdinate()
			{
				DimensionOrdinate dim = new DimensionOrdinate
				{
					FeatureLocation = new XYZ(10, 10, 0),
				};

				this.Document.Entities.Add(dim);

				dim.UpdateBlock();
			}

			public void DimensionRadius()
			{
				DimensionRadius dim = new DimensionRadius
				{
					AngleVertex = new XYZ(10, 10, 0),
				};

				this.Document.Entities.Add(dim);

				dim.UpdateBlock();
			}

			public void Dimensions()
			{
				DimensionAligned dim = new DimensionAligned
				{
					SecondPoint = new XYZ(10)
				};

				dim.UpdateBlock();

				ACadSharp.Entities.Line line = new ACadSharp.Entities.Line
				{
					StartPoint = new CSMath.XYZ(1, 0, 0),
					EndPoint = new CSMath.XYZ(5, 5, 0)
				};

				dim.Text = "HELLO";
				dim.IsTextUserDefinedLocation = true;
				dim.TextMiddlePoint = new XYZ(10, 10, 0);

				this.Document.Entities.Add(dim);

				DimensionAligned dim1 = new DimensionAligned();

				dim1.SecondPoint = new XYZ(10, 0, 0);

				this.Document.Entities.Add(dim1);
			}

			public void DimensionsInBlock()
			{
				DimensionAligned dim = new DimensionAligned
				{
					SecondPoint = new XYZ(10, 0, 0)
				};

				ACadSharp.Entities.Line line = new ACadSharp.Entities.Line
				{
					StartPoint = new CSMath.XYZ(1, 0, 0),
					EndPoint = new CSMath.XYZ(5, 5, 0)
				};

				DimensionLinear dim1 = new DimensionLinear()
				{
					FirstPoint = line.StartPoint,
					SecondPoint = line.EndPoint
				};

				BlockRecord record = new BlockRecord("dim_block");
				record.Entities.Add(dim);
				record.Entities.Add(line);
				record.Entities.Add(dim1);

				this.Document.Entities.Add(new Insert(record));

				DimensionAligned c = (DimensionAligned)dim.Clone();
				Document.Entities.Add(c);

				dim.UpdateBlock();
				dim1.UpdateBlock();
				c.UpdateBlock();
			}

			public void DimensionWithLineType()
			{
				LineType linetype = new LineType("LTYPE:PAINT");
				linetype.AddSegment(new LineType.Segment() { Length = 1 });
				linetype.AddSegment(new LineType.Segment() { Length = -1 });

				DimensionStyle style = new DimensionStyle("my_style");
				style.LineType = linetype;

				DimensionAligned dim = new DimensionAligned();
				dim.Style = style;

				dim.SecondPoint = new XYZ(10);

				this.Document.Entities.Add(dim);
			}

			public void Empty()
			{ }

			public void EntityChangeNormal()
			{
				Circle c = new Circle();
				c.Center = new XYZ(0, 0, 0);
				c.Radius = 10;

				c.Normal = XYZ.AxisX;

				this.Document.Entities.Add(c);

				var arc = new Arc()
				{
					StartAngle = 0,
					EndAngle = Math.PI / (2),
					Radius = 20,
					Normal = XYZ.AxisX
				};

				foreach (XYZ item in arc.PolygonalVertexes(100))
				{
					this.Document.Entities.Add(new Circle()
					{
						Center = item,
						Radius = 0.1,
						Color = new Color(255, 0, 0)
					});
				}

				this.Document.Entities.Add(arc);
			}

			public void EntityColorByIndex()
			{
				Circle c = new Circle();
				c.Center = new XYZ(0, 0, 0);
				c.Radius = 10;
				c.Color = new Color(11);

				this.Document.Entities.Add(c);
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

			public void EntityTransparency()
			{
				Line line = new Line(XYZ.Zero, new XYZ(100, 100, 0));

				line.Transparency = new Transparency(50);

				this.Document.Entities.Add(line);
			}

			public void GeoData()
			{
				this.Document.ModelSpace.CreateExtendedDictionary();

				var geodata = new GeoData();
				geodata.HostBlock = this.Document.ModelSpace;

				this.Document.ModelSpace.XDictionary.Add(CadDictionary.GeographicData, geodata);
			}

			public void LayerTrueColor()
			{
				Layer layer = new Layer("Layer_true_color");
				layer.Color = Color.FromTrueColor(1151726);

				this.Document.Layers.Add(layer);
			}

			public void LineTypeInBlock()
			{
				BlockRecord block = new BlockRecord("block1");

				LineType linetype = new LineType("LTYPE:PAINT");
				linetype.AddSegment(new LineType.Segment() { Length = 1 });
				linetype.AddSegment(new LineType.Segment() { Length = -1 });

				ACadSharp.Entities.Line line = new ACadSharp.Entities.Line
				{
					StartPoint = new CSMath.XYZ(1, 0, 0),
					EndPoint = new CSMath.XYZ(5, 5, 0),
					LineType = linetype
				};

				ACadSharp.Entities.Line line1 = new ACadSharp.Entities.Line
				{
					StartPoint = new CSMath.XYZ(1, 0, 0),
					EndPoint = new CSMath.XYZ(5, 5, 0),
					LineType = linetype
				};
				// dashed line shows up fine when added directly to the document
				this.Document.Entities.Add(line1);

				block.Entities.Add(line);

				this.Document.BlockRecords.Add(block);
				Insert blockinsert = new Insert(block)
				{
					InsertPoint = new XYZ(10, 10, 0)
				};

				this.Document.Entities.Add(blockinsert);
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

			public void Serialize(IXunitSerializationInfo info)
			{
				info.AddValue(nameof(this.Name), this.Name);
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

			public void SingleMLeader()
			{
				MultiLeader mleader = new MultiLeader();
				mleader.PathType = MultiLeaderPathType.StraightLineSegments;
				mleader.PropertyOverrideFlags = MultiLeaderPropertyOverrideFlags.ContentType | MultiLeaderPropertyOverrideFlags.TextAlignment | MultiLeaderPropertyOverrideFlags.EnableUseDefaultMText;

				mleader.ContextData.ContentBasePoint = new XYZ(1.8599999999999999, 1.5, 0);
				mleader.ContextData.BasePoint = new XYZ(0, 0, 0);
				mleader.ContextData.TextLabel = "This is my test MLEader";

				var root = new MultiLeaderObjectContextData.LeaderRoot
				{
					ConnectionPoint = new XYZ(1.5, 1.5, 0),
					ContentValid = true,
					Direction = XYZ.AxisX,
					LandingDistance = 0.36,
				};
				MultiLeaderObjectContextData.LeaderLine leaderLine = new MultiLeaderObjectContextData.LeaderLine();
				leaderLine.PathType = MultiLeaderPathType.StraightLineSegments;
				leaderLine.Points.Add(XYZ.Zero);
				root.Lines.Add(leaderLine);
				mleader.ContextData.LeaderRoots.Add(root);

				this.Document.Entities.Add(mleader);
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

			public void SingleMTextMultiline()
			{
				MText mtext = new MText();

				mtext.Value = "HELLO I'm an MTEXT\n and I have multiple lines";

				this.Document.Entities.Add(mtext);
			}

			public void SingleMTextRotation()
			{
				MText mtext = new MText();
				mtext.Value = "HELLO I'm a rotated MTEXT";
				mtext.AlignmentPoint = new XYZ(Math.Cos(MathHelper.DegToRad(-30)), Math.Sin(MathHelper.DegToRad(-30)), 0);

				this.Document.Entities.Add(mtext);

				mtext = new MText();
				mtext.Value = "normal changed";
				mtext.AlignmentPoint = new XYZ(Math.PI / 4, Math.PI / 4, 0);
				mtext.Normal = XYZ.AxisX;

				this.Document.Entities.Add(mtext);

				mtext = new MText();
				mtext.Value = "Bla bla bla";
				mtext.ApplyRotation(XYZ.AxisZ, Math.PI / 4);

				this.Document.Entities.Add(mtext);
			}

			public void SingleMTextSpecialCharacter()
			{
				MText mtext = new MText();

				mtext.Value = "∅45,6";

				this.Document.Entities.Add(mtext);
			}

			public void SinglePoint()
			{
				this.Document.Entities.Add(new Point(XYZ.Zero));
			}

			public void SinglePdfUnderlay()
			{
				var definition = new PdfUnderlayDefinition();
				definition.Page = "1";
				definition.File = "..\\..\\pdf-definition.pdf";

				definition.Name = $"{definition.File} {definition.Page}";

				PdfUnderlay raster = new PdfUnderlay(definition);

				raster.ClipBoundaryVertices.Add(new XY(0, 0));
				raster.ClipBoundaryVertices.Add(new XY(1, 1));

				this.Document.Entities.Add(raster);

				var clone = raster.CloneTyped();
				clone.InsertPoint = new XYZ(10, 10, 0);

				this.Document.Entities.Add(clone);
			}

			public void SingleRasterImage()
			{
				ImageDefinition definition = new ImageDefinition();
				definition.Size = new XY(1, 1);
				definition.Name = "image";
				definition.IsLoaded = true;

				definition.FileName = "..\\..\\image.JPG";

				RasterImage raster = new RasterImage(definition);

				raster.Flags = ImageDisplayFlags.ShowImage;

				raster.ClipBoundaryVertices.Add(new XY(0, 0));
				raster.ClipBoundaryVertices.Add(new XY(0, 1));
				raster.ClipBoundaryVertices.Add(new XY(1, 1));
				raster.ClipBoundaryVertices.Add(new XY(1, 0));

				this.Document.Entities.Add(raster);
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

			public void SPlineCreation()
			{
				Spline spline = new Spline();

				spline.ControlPoints.Add(new XYZ(0, 0, 0));
				spline.ControlPoints.Add(new XYZ(10, 10, 0));
				spline.ControlPoints.Add(new XYZ(20, 10, 0));
				spline.ControlPoints.Add(new XYZ(50, 30, 0));

				spline.Degree = 3;

				spline.Knots.Add(0);
				spline.Knots.Add(0);
				spline.Knots.Add(0);
				spline.Knots.Add(0);

				spline.Knots.Add(1);
				spline.Knots.Add(1);
				spline.Knots.Add(1);
				spline.Knots.Add(1);

				Polyline3D polyline = new Polyline3D(spline.PolygonalVertexes(255));

				this.Document.Entities.Add(spline);
				this.Document.Entities.Add(polyline);

				List<XYZ> fitPoints = new()
				{
					new XYZ(0, 0, 0),
					new XYZ(5, 5, 0),
					new XYZ(10, 0, 0),
					new XYZ(15, -5, 0),
					new XYZ(20, 0, 0)
				};

				spline = new Spline();
				spline.FitPoints.AddRange(fitPoints);

				spline.UpdateFromFitPoints();

				this.Document.Entities.Add(spline);
			}

			public void TextAlignment()
			{
				XYZ insert = new XYZ(0, 0, 0);

				foreach (var item in Enum.GetValues(typeof(TextHorizontalAlignment)).Cast<TextHorizontalAlignment>())
				{
					TextEntity textEntity = new TextEntity();
					textEntity.Value = item.ToString();
					textEntity.HorizontalAlignment = item;
					textEntity.InsertPoint = insert;
					textEntity.Height = 0.5;

					this.Document.Entities.Add(textEntity);

					insert = new XYZ(insert.X + 2, 0, 0);
				}
			}

			public void TextWithChineseCharacters()
			{
				//this.Document.Header.CodePage = "GB2312";

				TextStyle style = new TextStyle("custom");
				style.Filename = "romans.shx";
				style.BigFontFilename = "chineset.shx";

				MText mtext = new MText();
				//mtext.AlignmentPoint = XYZ.Zero;
				//mtext.HorizontalWidth = 1;
				mtext.Value = "我的短信";
				mtext.Style = style;

				TextEntity text = new TextEntity();
				text.Value = "我的短信";
				text.Style = style;

				this.Document.Entities.Add(mtext);
				this.Document.Entities.Add(text);
			}

			public override string ToString()
			{
				return this.Name;
			}

			public void ViewZoom()
			{
				Line line = new Line(XYZ.Zero, new XYZ(100, 100, 0));
				Line line1 = new Line(new XYZ(0, 100, 0), new XYZ(100, 0, 0));

				this.Document.Entities.Add(line);
				this.Document.Entities.Add(line1);

				var box = line.GetBoundingBox();

				VPort active = this.Document.VPorts[VPort.DefaultName];
				active.Center = (XY)box.Center;
				//active.BottomLeft = (XY)box.Min;
				//active.TopRight = (XY)box.Max;
				active.ViewHeight = 100;
			}

			public void XData()
			{
				AppId app = new AppId("my_app");
				Layer layer = new Layer("my_layer");
				this.Document.AppIds.Add(app);
				this.Document.Layers.Add(layer);

				Line line = new Line(XYZ.Zero, new XYZ(100, 100, 0));

				List<ExtendedDataRecord> records = new();
				records.Add(new ExtendedDataControlString(false));
				records.Add(new ExtendedDataInteger16(5));
				records.Add(new ExtendedDataInteger32(33));
				records.Add(new ExtendedDataString("my extended data string"));
				records.Add(new ExtendedDataHandle(5));
				records.Add(new ExtendedDataReal(25.35));
				records.Add(new ExtendedDataScale(0.66));
				records.Add(new ExtendedDataDistance(481.48));
				records.Add(new ExtendedDataDirection(new XYZ(4, 3, 2)));
				records.Add(new ExtendedDataCoordinate(new XYZ(8, 7, 4)));
				records.Add(new ExtendedDataWorldCoordinate(new XYZ(85, 74, 47)));
				records.Add(new ExtendedDataLayer(layer.Handle));
				records.Add(new ExtendedDataBinaryChunk(new byte[] { 1, 2, 3, 4 }));
				records.Add(new ExtendedDataControlString(true));

				line.ExtendedData.Add(app, records);

				this.Document.Entities.Add(line);
			}

			public void XRef()
			{
				BlockRecord record = new BlockRecord("my_xref");
				record.Flags = BlockTypeFlags.XRef | BlockTypeFlags.XRefResolved;
				record.BlockEntity.XRefPath = "./SinglePoint_AC1032.dwg";

				this.Document.BlockRecords.Add(record);

				this.Document.Entities.Add(new Insert(record));
			}
		}
	}
}