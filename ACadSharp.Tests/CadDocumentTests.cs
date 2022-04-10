using ACadSharp;
using System;
using System.Collections.Generic;
using System.Text;
using ACadSharp.Tables.Collections;
using ACadSharp.Tables;
using Xunit;
using ACadSharp.Tests.Common;
using ACadSharp.Entities;

namespace ACadSharp.Tests
{
	public class CadDocumentTests
	{
		[Fact]
		public void CadDocumentTest()
		{
			CadDocument doc = new CadDocument();

			DocumentIntegrity.AssertTableHirearchy(doc);
		}

		[Fact]
		public void CadDocumentDefaultTest()
		{
			CadDocument doc = new CadDocument();

			DocumentIntegrity.AssertDocumentDefaults(doc);
		}

		[Fact]
		public void AddCadObject()
		{
			Line line = new Line();
			CadDocument doc = new CadDocument();

			doc.BlockRecords[BlockRecord.ModelSpaceName].Entities.Add(line);

			CadObject l = doc.GetCadObject(line.Handle);
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