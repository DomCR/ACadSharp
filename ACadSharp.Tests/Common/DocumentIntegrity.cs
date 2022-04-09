using ACadSharp.Tables;
using ACadSharp.Tables.Collections;
using System;
using System.Collections.Generic;
using System.Linq;
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

			Assert.NotNull(doc.LineTypes["ByLayer"]);
			Assert.NotNull(doc.LineTypes["ByBlock"]);
			Assert.NotNull(doc.LineTypes["Continuous"]);

			Assert.NotNull(doc.Layers["0"]);

			Assert.NotNull(doc.TextStyles["Standard"]);

			Assert.NotNull(doc.AppIds["ACAD"]);

			Assert.NotNull(doc.DimensionStyles["Standard"]);

			Assert.NotNull(doc.VPorts["*Active"]);

			//TODO: Change layout list to an observable collection
			Assert.NotNull(doc.Layouts.FirstOrDefault(l => l.Name == "Model"));
		}

		public static void AssertBlockRecords(CadDocument doc)
		{
			foreach (BlockRecord br in doc.BlockRecords)
			{
				Assert.Equal(br.Name, br.BlockEntity.Name);

				Assert.NotNull(doc.GetCadObject(br.BlockEntity.Handle));
				Assert.NotNull(doc.GetCadObject(br.BlockEnd.Handle));

				foreach (Entities.Entity e in br.Entities)
				{
					Assert.NotNull(doc.GetCadObject(e.Handle));
				}
			}
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
