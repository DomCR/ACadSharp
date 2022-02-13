using ACadSharp;
using System;
using System.Collections.Generic;
using System.Text;
using ACadSharp.Tables.Collections;
using ACadSharp.Tables;
using Xunit;

namespace ACadSharp.Tests
{
	public class CadDocumentTests
	{
		[Fact()]
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
		}

		private void assertTable<T>(CadDocument doc, Table<T> table)
			where T : TableEntry
		{
			Assert.NotNull(table);

			Assert.NotNull(table.Document);
			Assert.Equal(doc, table.Document);
			Assert.Equal(doc, table.Owner);

			Assert.True(table.Handle != 0);

			CadObject t = doc.GetCadObject(table.Handle);
			Assert.Equal(t, table);
		}
	}
}