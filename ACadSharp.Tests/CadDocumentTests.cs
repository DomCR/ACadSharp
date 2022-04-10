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
			CadDocument doc = new CadDocument();

			DocumentIntegrity.AssertTableHirearchy(doc);
		}

		[Fact()]
		public void CadDocumentDefaultTest()
		{
			CadDocument doc = new CadDocument();

			DocumentIntegrity.AssertDocumentDefaults(doc);
		}
	}
}