using ACadSharp.Header;
using ACadSharp.Tables;
using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace ACadSharp.Tests.Header
{
	public class CadHeaderTests
	{
		[Fact]
		public void CadHeaderDefaultTest()
		{
			CadHeader header = new CadHeader();

			Assert.NotNull(header.CurrentLayer);
			Assert.True(header.CurrentLayer.Name == header.CurrentLayerName);
			Assert.True(header.CurrentLayer.Name == Layer.DefaultName);
			Assert.True(header.CurrentLayer.Handle == 0);
			Assert.Null(header.CurrentLayer.Owner);
		}

		[Fact]
		public void CadHeaderWithDocumentTest()
		{
			CadDocument document = new CadDocument();
			CadHeader header = new CadHeader(document);

			Assert.NotNull(header.CurrentLayer);
			Assert.True(header.CurrentLayer.Name == header.CurrentLayerName, "Name does not match");
			Assert.True(header.CurrentLayer.Name == Layer.DefaultName, "Name does not match");
			Assert.True(header.CurrentLayer.Handle == document.Layers[header.CurrentLayerName].Handle, "Handle does not match");
			Assert.Equal(document.Layers, header.CurrentLayer.Owner);
		}
	}
}
