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

			CadObject l = doc.GetCadObject(line.Handle);

			//Assert layer
			Assert.Equal(line.Layer, layer);
			Assert.False(0 == layer.Handle);
			Assert.Equal(layer, doc.Layers[layer.Name]);
		}

		[Fact]
		public void RemoveCadObject()
		{
			Line line = new Line();
			CadDocument doc = new CadDocument();

			doc.Entities.Add(line);

			var l = doc.Entities.Remove(line);

			//Assert removed element
			Assert.NotNull(l);
			Assert.Equal(line, l);
			Assert.False(0 == l.Handle);
			Assert.Equal(line.Handle, l.Handle);
		}

		[Fact]
		public void NotAllowDuplicate()
		{
			Line line = new Line();
			CadDocument doc = new CadDocument();

			doc.BlockRecords[BlockRecord.ModelSpaceName].Entities.Add(line);

			Assert.Throws<ArgumentException>(() => doc.BlockRecords[BlockRecord.ModelSpaceName].Entities.Add(line));
		}
	}
}