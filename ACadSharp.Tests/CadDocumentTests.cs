using ACadSharp;
using System;
using System.Collections.Generic;
using System.Text;
using ACadSharp.Tables.Collections;
using ACadSharp.Tables;
using Xunit;
using ACadSharp.Tests.Common;
using ACadSharp.Entities;
using Xunit.Abstractions;

namespace ACadSharp.Tests
{
	public class CadDocumentTests
	{
		protected readonly DocumentIntegrity _docIntegrity;

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
		public void DetachedEntityClone()
		{
			Line line = new Line();
			CadDocument doc = new CadDocument();

			doc.Entities.Add(line);

			Line clone = (Line)doc.GetCadObject<Line>(line.Handle).Clone();

			//Assert clone
			Assert.NotEqual(clone, line);
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