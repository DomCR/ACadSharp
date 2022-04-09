using ACadSharp.Tables;
using ACadSharp.Tables.Collections;
using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace ACadSharp.Tests.Common
{
	public static class DocumentIntegrity
	{
		public static void AssertTableHirearchy(CadDocument doc)
		{
			//Assert all the tables in the doc
			assertTable(doc, doc.AppIds);
			assertTable(doc, doc.BlockRecords);
			assertTable(doc, doc.DimensionStyles);
			assertTable(doc, doc.Layers);
			assertTable(doc, doc.LineTypes);
			assertTable(doc, doc.TextStyles);
			assertTable(doc, doc.UCSs);
			assertTable(doc, doc.Views);
			assertTable(doc, doc.VPorts);
		}

		public static void AssertDocumentDefaults(CadDocument doc)
		{
			//Assert the default values for the document
			Assert.NotNull(doc.BlockRecords["*Model_Space"]);
			Assert.NotNull(doc.BlockRecords["*Paper_Space"]);

			//Assert linetypes
			Assert.NotNull(doc.LineTypes["ByLayer"]);
			Assert.NotNull(doc.LineTypes["ByBlock"]);
			Assert.NotNull(doc.LineTypes["Continuous"]);

		}

		private static void assertTable<T>(CadDocument doc, Table<T> table)
			where T : TableEntry
		{
			Assert.NotNull(table);

			Assert.NotNull(table.Document);
			Assert.Equal(doc, table.Document);
			Assert.Equal(doc, table.Owner);

			Assert.True(table.Handle != 0);

			CadObject t = doc.GetCadObject(table.Handle);
			Assert.Equal(t, table);

			foreach (T entry in table)
			{
				Assert.Equal(entry.Owner.Handle, table.Handle);
				Assert.Equal(entry.Owner, table);

				Assert.NotNull(doc.GetCadObject(entry.Handle));
			}
		}
	}
}
