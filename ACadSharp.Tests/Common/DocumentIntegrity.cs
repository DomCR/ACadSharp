using ACadSharp.Tables;
using ACadSharp.Tables.Collections;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
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
			entryNotNull(doc.BlockRecords, "*Model_Space");
			entryNotNull(doc.BlockRecords, "*Paper_Space");

			entryNotNull(doc.LineTypes, "ByLayer");
			entryNotNull(doc.LineTypes, "ByBlock");
			entryNotNull(doc.LineTypes, "Continuous");

			entryNotNull(doc.Layers, "0");

			entryNotNull(doc.TextStyles, "Standard");

			entryNotNull(doc.AppIds, "ACAD");

			entryNotNull(doc.DimensionStyles, "Standard");

			entryNotNull(doc.VPorts, "*Active");

			//TODO: Change layout list to an observable collection
			//notNull(doc.Layouts.FirstOrDefault(l => l.Name == "Model"), "Model");
		}

		public static void AssertBlockRecords(CadDocument doc)
		{
			foreach (BlockRecord br in doc.BlockRecords)
			{
				Assert.Equal(br.Name, br.BlockEntity.Name);

				documentObjectNotNull(doc, br.BlockEntity);
				documentObjectNotNull(doc, br.BlockEnd);

				foreach (Entities.Entity e in br.Entities)
				{
					documentObjectNotNull(doc, e);
				}
			}
		}

		private static void assertTable<T>(CadDocument doc, Table<T> table)
			where T : TableEntry
		{
			Assert.NotNull(table);

			notNull(table.Document, $"Document not assigned to table {table}");
			Assert.Equal(doc, table.Document);
			Assert.Equal(doc, table.Owner);

			Assert.True(table.Handle != 0);

			CadObject t = doc.GetCadObject(table.Handle);
			Assert.Equal(t, table);

			foreach (T entry in table)
			{
				Assert.Equal(entry.Owner.Handle, table.Handle);
				Assert.Equal(entry.Owner, table);

				documentObjectNotNull(doc, entry);
			}
		}

		private static void documentObjectNotNull<T>(CadDocument doc, T o)
			where T : CadObject
		{
			Assert.True(doc.GetCadObject(o.Handle) != null, $"Object of type {typeof(T)} | {o.Handle} not found in the doucment");

		}

		private static void notNull<T>(T o, string info)
		{
			Assert.True(o != null, $"Object of type {typeof(T)} should not be null:  {info}");
		}

		private static void entryNotNull<T>(Table<T> table, string entry)
			where T : TableEntry
		{
			Assert.True(table[entry] != null, $"Entry with name {entry} is null for thable {table}");
		}
	}
}
