using ACadSharp.Entities;
using ACadSharp.Tables;
using CSMath;
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

			public SingleCaseGenerator() { }

			public SingleCaseGenerator(string name)
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


			public void SingleLine()
			{
				Line line = new Line(XYZ.Zero, new XYZ(100, 100, 0));

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

				var Pline = new Polyline2D();
				Pline.Vertices.AddRange(vector2d);
				Pline.IsClosed = true;
				Pline.Vertices.ElementAt(3).Bulge = 1;

				this.Document.Entities.Add(Pline);
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
			Data.Add(new(nameof(SingleCaseGenerator.SingleLine)));
			Data.Add(new(nameof(SingleCaseGenerator.EntityColorByLayer)));
			Data.Add(new(nameof(SingleCaseGenerator.EntityColorTrueColor)));
			Data.Add(new(nameof(SingleCaseGenerator.DefaultLayer)));
			Data.Add(new(nameof(SingleCaseGenerator.SingleMText)));
			Data.Add(new(nameof(SingleCaseGenerator.SingleMTextSpecialCharacter)));
			Data.Add(new(nameof(SingleCaseGenerator.SingleMTextMultiline)));
			Data.Add(new(nameof(SingleCaseGenerator.SinglePoint)));
			Data.Add(new(nameof(SingleCaseGenerator.ClosedLwPolyline)));
			Data.Add(new(nameof(SingleCaseGenerator.ClosedPolyline2DTest)));
		}

		protected string getPath(string name, string ext, ACadVersion version)
		{
			return Path.Combine(singleCasesOutFolder, $"{name}_{version}.{ext}");
		}
	}
}