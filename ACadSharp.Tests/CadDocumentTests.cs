using ACadSharp;
using System;
using System.Collections.Generic;
using System.Text;
using ACadSharp.Tables.Collections;
using ACadSharp.Tables;
using Xunit;
using ACadSharp.Tests.Common;

namespace ACadSharp.Tests
{
	public class CadDocumentTests
	{
		[Fact()]
		public void CadDocumentTest()
		{
			CadDocument document = new CadDocument();

			DocumentIntegrity.AssertTableHirearchy(document);
		}

		[Fact()]
		public void CadDocumentDefaultTest()
		{
			CadDocument doc = new CadDocument();

			Assert.NotNull(doc.BlockRecords["*Model_Space"]);
			Assert.NotNull(doc.BlockRecords["*Paper_Space"]);
		}
	}
}