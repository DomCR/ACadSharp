using Microsoft.VisualStudio.TestTools.UnitTesting;
using ACadSharp;
using System;
using System.Collections.Generic;
using System.Text;
using ACadSharp.Tables.Collections;
using ACadSharp.Tables;

namespace ACadSharp.Tests
{
	[TestClass()]
	public class CadDocumentTests
	{
		[TestMethod()]
		public void CadDocumentTest()
		{
			CadDocument document = new CadDocument();

			this.assertTable(document, document.AppIds);
			this.assertTable(document, document.BlockRecords);
			this.assertTable(document, document.DimensionStyles);
			this.assertTable(document, document.Layers);
			this.assertTable(document, document.LineTypes);
			this.assertTable(document, document.TextStyles);
			this.assertTable(document, document.UCSs);
			this.assertTable(document, document.Views);
			this.assertTable(document, document.VPorts);
			this.assertTable(document, document.Layouts);
		}

		private void assertTable<T>(CadDocument doc, Table<T> table)
			where T : TableEntry
		{
			Assert.IsNotNull(table);

			Assert.IsNotNull(table.Document);
			Assert.AreEqual(doc, table.Document);
			Assert.AreEqual(doc, table.Owner);

			Assert.IsTrue(table.Handle != 0);

			CadObject t = doc.GetCadObject(table.Handle);
			Assert.AreEqual(t, table);
		}
	}
}