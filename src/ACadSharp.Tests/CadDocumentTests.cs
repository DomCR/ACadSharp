using System;
using ACadSharp.Tables;
using Xunit;
using ACadSharp.Tests.Common;
using ACadSharp.Entities;
using Xunit.Abstractions;
using ACadSharp.Blocks;
using System.Linq;
using System.Diagnostics;
using ACadSharp.Objects;

namespace ACadSharp.Tests
{
	public class CadDocumentTests
	{
		public static readonly TheoryData<Type> EntityTypes;

		private readonly DocumentIntegrity _docIntegrity;

		private readonly ITestOutputHelper _output;

		static CadDocumentTests()
		{
			EntityTypes = new TheoryData<Type>();

			foreach (var item in DataFactory.GetTypes<Entity>())
			{
				if (item == typeof(Block)
					|| item == typeof(PdfUnderlay)
					|| item == typeof(BlockEnd)
					|| item == typeof(UnknownEntity))
					continue;

				EntityTypes.Add(item);
			}
		}

		public CadDocumentTests(ITestOutputHelper output)
		{
			this._output = output;
			this._docIntegrity = new DocumentIntegrity(output);
		}

		[Fact]
		public void AddCadObjectStressTest()
		{
			CadDocument doc = new CadDocument();

			Stopwatch stopwatch = new Stopwatch();
			this._output.WriteLine("StopWatch start");
			stopwatch.Start();

			for (int i = 0; i < 10000; i++)
			{
				Polyline3D polyline = new Polyline3D();
				for (int j = 0; j < 50; j++)
				{
					polyline.Vertices.Add(new Vertex3D() { Location = new CSMath.XYZ(i, j, 0) });
				}

				doc.Entities.Add(polyline);
			}

			stopwatch.Stop();
			this._output.WriteLine(stopwatch.Elapsed.TotalSeconds.ToString());

			if (TestVariables.LocalEnv)
			{
				Assert.True(stopwatch.Elapsed.TotalSeconds < 5);
			}
		}

		[Fact]
		public void AddCadObjectTest()
		{
			Line line = new Line();
			CadDocument doc = new CadDocument();

			doc.Entities.Add(line);

			CadObject l = doc.GetCadObject(line.Handle);

			//Assert existing element
			Assert.NotNull(l);
			Assert.Equal(line, l);
			Assert.False(0 == l.Handle);
			Assert.Equal(line.Handle, l.Handle);
		}

		[Fact]
		public void AddCadObjectWithNewLayer()
		{
			Line line = new Line();
			Layer layer = new Layer("test_layer");

			line.Layer = layer;

			CadDocument doc = new CadDocument();

			doc.Entities.Add(line);

			Line l = doc.GetCadObject<Line>(line.Handle);

			//Assert layer
			Assert.Equal(l.Layer, layer);
			Assert.False(0 == layer.Handle);
			Assert.NotNull(doc.Layers[layer.Name]);
			Assert.Equal(layer, doc.Layers[layer.Name]);
		}

		[Fact]
		public void CadDocumentDefaultTest()
		{
			CadDocument doc = new CadDocument();

			this._docIntegrity.AssertDocumentDefaults(doc);
			this._docIntegrity.AssertTableHierarchy(doc);
			this._docIntegrity.AssertBlockRecords(doc);

			Assert.Equal(2, doc.BlockRecords.Count);
			Assert.Equal(1, doc.Layers.Count);
			Assert.Equal(3, doc.LineTypes.Count);
			Assert.Equal(2, doc.Layouts.Count());
		}

		[Fact]
		public void CadDocumentTest()
		{
			CadDocument doc = new CadDocument();

			this._docIntegrity.AssertTableHierarchy(doc);
		}

		[Fact]
		public void ChangeEntityLayer()
		{
			Line line = new Line();
			Layer layer = new Layer("test_layer");

			CadDocument doc = new CadDocument();

			doc.Entities.Add(line);

			Line l = doc.GetCadObject<Line>(line.Handle);
			l.Layer = layer;

			//Assert layer
			Assert.Equal(l.Layer, layer);
			Assert.False(0 == layer.Handle);
			Assert.NotNull(doc.Layers[layer.Name]);
			Assert.Equal(layer, doc.Layers[layer.Name]);
		}

		[Fact]
		public void ChangeEntityLineType()
		{
			Line line = new Line();
			LineType lineType = new LineType("test_linetype");

			CadDocument doc = new CadDocument();

			doc.Entities.Add(line);

			Line l = doc.GetCadObject<Line>(line.Handle);
			l.LineType = lineType;

			//Assert layer
			Assert.Equal(l.LineType, lineType);
			Assert.False(0 == lineType.Handle);
			Assert.NotNull(doc.LineTypes[lineType.Name]);
			Assert.Equal(lineType, doc.LineTypes[lineType.Name]);
		}

		[Fact]
		public void ChangeEntityLineTypeNoDocument()
		{
			Line line = new Line();
			LineType lineType = new LineType("test_linetype");

			line.LineType = lineType;

			//Assert layer
			Assert.Equal(line.LineType, lineType);
			Assert.True(0 == lineType.Handle);
		}

		[Fact]
		public void CreateDefaultsExistingDocumentTest()
		{
			CadDocument doc = new CadDocument();

			ulong appIdsHandle = doc.AppIds.Handle;
			ulong blksHandle = doc.BlockRecords.Handle;
			ulong dimHandle = doc.DimensionStyles.Handle;
			ulong layersHandle = doc.Layers.Handle;
			ulong ltypesHandle = doc.LineTypes.Handle;
			ulong textStyleHandle = doc.TextStyles.Handle;
			ulong ucsHandle = doc.UCSs.Handle;
			ulong viewsHandle = doc.Views.Handle;
			ulong vportsHandle = doc.VPorts.Handle;

			doc.CreateDefaults();

			//Objects should not be replaced
			Assert.Equal(appIdsHandle, doc.AppIds.Handle);
			Assert.Equal(blksHandle, doc.BlockRecords.Handle);
			Assert.Equal(dimHandle, doc.DimensionStyles.Handle);
			Assert.Equal(layersHandle, doc.Layers.Handle);
			Assert.Equal(ltypesHandle, doc.LineTypes.Handle);
			Assert.Equal(textStyleHandle, doc.TextStyles.Handle);
			Assert.Equal(ucsHandle, doc.UCSs.Handle);
			Assert.Equal(viewsHandle, doc.Views.Handle);
			Assert.Equal(vportsHandle, doc.VPorts.Handle);

			this._docIntegrity.AssertDocumentDefaults(doc);
		}

		[Theory]
		[MemberData(nameof(EntityTypes))]
		public void DetachedEntityClone(Type entityType)
		{
			Entity entity = EntityFactory.Create(entityType);
			CadDocument doc = new CadDocument();

			doc.Entities.Add(entity);

			Entity clone = (Entity)doc.GetCadObject<Entity>(entity.Handle).Clone();

			//Assert clone
			Assert.NotEqual(clone, entity);
			Assert.True(0 == clone.Handle);
			Assert.Null(clone.Document);
			Assert.Null(clone.Owner);
			Assert.Null(clone.Layer.Document);
			Assert.Null(clone.LineType.Document);
		}

		[Fact]
		public void Get0HandleObject()
		{
			CadDocument doc = new CadDocument();

			Assert.Null(doc.GetCadObject(0));
			Assert.False(doc.TryGetCadObject(0, out CadObject cadObject));
			Assert.Null(cadObject);
		}

		[Fact]
		public void GetCurrentTest()
		{
			CadDocument doc = new CadDocument();

			Layer layer = doc.GetCurrent<Layer>();
			Assert.NotNull(layer);
			Assert.Equal(Layer.DefaultName, layer.Name);

			LineType lineType = doc.GetCurrent<LineType>();
			Assert.NotNull(lineType);
			Assert.Equal(LineType.ByLayerName, lineType.Name);

			TextStyle textStyle = doc.GetCurrent<TextStyle>();
			Assert.NotNull(textStyle);
			Assert.Equal(TextStyle.DefaultName, textStyle.Name);

			DimensionStyle dimStyle = doc.GetCurrent<DimensionStyle>();
			Assert.NotNull(dimStyle);
			Assert.Equal(DimensionStyle.DefaultName, dimStyle.Name);

			MLineStyle mlineStyle = doc.GetCurrent<MLineStyle>();
			Assert.NotNull(mlineStyle);
			Assert.Equal(MLineStyle.DefaultName, mlineStyle.Name);

			MultiLeaderStyle multiLeaderStyle = doc.GetCurrent<MultiLeaderStyle>();
			Assert.NotNull(multiLeaderStyle);
			Assert.Equal(MultiLeaderStyle.DefaultName, multiLeaderStyle.Name);
		}

		[Fact]
		public void NotAllowDuplicate()
		{
			Line line = new Line();
			CadDocument doc = new CadDocument();

			doc.Entities.Add(line);

			Assert.Throws<ArgumentException>(() => doc.Entities.Add(line));
		}

		[Fact]
		public void RemoveCadObject()
		{
			Line line = new Line();
			CadDocument doc = new CadDocument();

			doc.Entities.Add(line);

			Entity l = doc.Entities.Remove(line);

			//Assert removed element
			Assert.NotNull(l);
			Assert.Equal(line, l);
			Assert.True(0 == l.Handle);
			Assert.Equal(line.Handle, l.Handle);

			Assert.True(0 == l.Layer.Handle);
			Assert.Null(l.Layer.Document);
			Assert.True(0 == l.LineType.Handle);
			Assert.Null(l.LineType.Document);
		}

		[Fact]
		public void RemoveLayer()
		{
			string layerName = "custom_layer";
			Line line = new Line();
			line.Layer = new Layer(layerName);
			CadDocument doc = new CadDocument();

			doc.Entities.Add(line);

			Layer l = doc.Layers.Remove(layerName);

			//Assert removed element
			Assert.False(doc.Layers.Contains(layerName));
			Assert.Null(l.Document);
			Assert.True(l.Handle == 0);
			Assert.Equal(doc.Layers[Layer.DefaultName], line.Layer);
		}

		[Fact]
		public void RemoveLineType()
		{
			string ltypeName = "custom_ltype";
			Line line = new Line();
			line.LineType = new LineType(ltypeName);
			CadDocument doc = new CadDocument();

			doc.Entities.Add(line);

			LineType ltype = doc.LineTypes.Remove(ltypeName);

			//Assert removed element
			Assert.False(doc.Layers.Contains(ltypeName));
			Assert.Null(ltype.Document);
			Assert.True(ltype.Handle == 0);
			Assert.Equal(doc.LineTypes[LineType.ByLayerName], line.LineType);
		}

		[Fact]
		public void RestoreHandlesTest()
		{
			ulong bigHandle = 10000;
			Line line = new Line();
			line.Handle = bigHandle;

			CadDocument doc = new CadDocument();

			doc.Entities.Add(line);

			doc.RestoreHandles();

			CadObject l = doc.GetCadObject(line.Handle);

			//Assert existing element
			Assert.NotNull(l);
			Assert.Equal(line, l);
			Assert.False(0 == l.Handle);
			Assert.Equal(line.Handle, l.Handle);
			Assert.True(line.Handle < bigHandle);
		}

		[Fact]
		public void SetCurrentTest()
		{
			CadDocument doc = new CadDocument();

			string layerName = "my_layer";
			doc.SetCurrent(new Layer(layerName));
			Assert.True(doc.Layers.Contains(layerName));
			Assert.Equal(layerName, doc.Header.CurrentLayerName);

			string lineTypeName = "my_linetype";
			doc.SetCurrent(new LineType(lineTypeName));
			Assert.True(doc.LineTypes.Contains(lineTypeName));
			Assert.Equal(lineTypeName, doc.Header.CurrentLineTypeName);

			string textStyleName = "my_textstyle";
			doc.SetCurrent(new TextStyle(textStyleName));
			Assert.True(doc.TextStyles.Contains(textStyleName));
			Assert.Equal(textStyleName, doc.Header.CurrentTextStyleName);

			string dimStyleName = "my_dimstyle";
			doc.SetCurrent(new DimensionStyle(dimStyleName));
			Assert.True(doc.DimensionStyles.Contains(dimStyleName));
			Assert.Equal(dimStyleName, doc.Header.CurrentDimensionStyleName);

			string mlineStyleName = "my_mlinestyle";
			doc.SetCurrent(new MLineStyle(mlineStyleName));
			Assert.True(doc.MLineStyles.ContainsKey(mlineStyleName));
			Assert.Equal(mlineStyleName, doc.Header.CurrentMLineStyleName);

			string multiLeaderStyleName = "my_multileaderstyle";
			doc.SetCurrent(new MultiLeaderStyle(multiLeaderStyleName));
			Assert.True(doc.MLeaderStyles.ContainsKey(multiLeaderStyleName));
		}
	}
}