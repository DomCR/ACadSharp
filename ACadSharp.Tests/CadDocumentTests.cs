using System;
using ACadSharp.Tables;
using Xunit;
using ACadSharp.Tests.Common;
using ACadSharp.Entities;
using Xunit.Abstractions;

namespace ACadSharp.Tests
{
	public class CadDocumentTests
	{
		public static readonly TheoryData<Type> EntityTypes;

		protected readonly DocumentIntegrity _docIntegrity;

		static CadDocumentTests()
		{
			EntityTypes = new TheoryData<Type>();

			foreach (var item in DataFactory.GetTypes<Entity>())
			{
				EntityTypes.Add(item);
			}
		}

		public CadDocumentTests(ITestOutputHelper output)
		{
			this._docIntegrity = new DocumentIntegrity(output);
		}

		[Fact]
		public void CadDocumentTest()
		{
			CadDocument doc = new CadDocument();

			this._docIntegrity.AssertTableHirearchy(doc);
		}

		[Fact]
		public void CadDocumentDefaultTest()
		{
			CadDocument doc = new CadDocument();

			this._docIntegrity.AssertDocumentDefaults(doc);
			this._docIntegrity.AssertTableHirearchy(doc);
			this._docIntegrity.AssertBlockRecords(doc);
		}

		[Fact]
		public void AddCadObject()
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

		[Theory]
		[MemberData(nameof(EntityTypes))]
		public void DetachedEntityClone(Type entityType)
		{
			Entity entity = (Entity)Activator.CreateInstance(entityType);
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

		[Fact(Skip = "Implementation in branch : table-operations")]
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

		[Fact(Skip = "Implementation in branch : table-operations")]
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
			Assert.Equal(doc.LineTypes[LineType.ByBlockName], line.LineType);
		}

		[Fact]
		public void NotAllowDuplicate()
		{
			Line line = new Line();
			CadDocument doc = new CadDocument();

			doc.Entities.Add(line);

			Assert.Throws<ArgumentException>(() => doc.Entities.Add(line));
		}
	}
}