using ACadSharp.Tables;
using ACadSharp.Tables.Collections;
using ACadSharp.Tests.TestCases;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using Xunit;
using Xunit.Abstractions;

namespace ACadSharp.Tests.Common
{
	public class DocumentIntegrity
	{
		public ITestOutputHelper Output { get; set; }

		private const string _documentTree = "../../../../ACadSharp.Tests/Data/document_tree.json";

		private CadDocument _document;

		public DocumentIntegrity(ITestOutputHelper output)
		{
			this.Output = output;
		}

		public void AssertTableHirearchy(CadDocument doc)
		{
			//Assert all the tables in the doc
			this.assertTable(doc, doc.AppIds);
			this.assertTable(doc, doc.BlockRecords);
			this.assertTable(doc, doc.DimensionStyles);
			this.assertTable(doc, doc.Layers);
			this.assertTable(doc, doc.LineTypes);
			this.assertTable(doc, doc.TextStyles);
			this.assertTable(doc, doc.UCSs);
			this.assertTable(doc, doc.Views);
			this.assertTable(doc, doc.VPorts);
		}

		public void AssertDocumentDefaults(CadDocument doc)
		{
			//Assert the default values for the document
			this.entryNotNull(doc.BlockRecords, "*Model_Space");
			this.entryNotNull(doc.BlockRecords, "*Paper_Space");

			this.entryNotNull(doc.LineTypes, "ByLayer");
			this.entryNotNull(doc.LineTypes, "ByBlock");
			this.entryNotNull(doc.LineTypes, "Continuous");

			this.entryNotNull(doc.Layers, "0");

			this.entryNotNull(doc.TextStyles, "Standard");

			this.entryNotNull(doc.AppIds, "ACAD");

			this.entryNotNull(doc.DimensionStyles, "Standard");

			this.entryNotNull(doc.VPorts, "*Active");

			//Assert Model layout
			var layout = doc.Layouts.FirstOrDefault(l => l.Name == "Model");
			Assert.NotNull(layout);
			Assert.True(layout.AssociatedBlock == doc.ModelSpace);
		}

		public void AssertBlockRecords(CadDocument doc)
		{
			foreach (BlockRecord br in doc.BlockRecords)
			{
				Assert.Equal(br.Name, br.BlockEntity.Name);

				this.documentObjectNotNull(doc, br.BlockEntity);

				Assert.True(br.Handle == br.BlockEntity.Owner.Handle, "Block entity owner doesn't mach");

				this.documentObjectNotNull(doc, br.BlockEnd);

				foreach (Entities.Entity e in br.Entities)
				{
					this.documentObjectNotNull(doc, e);
				}
			}
		}

		public void AssertDocumentTree(CadDocument doc)
		{
			this._document = doc;
			CadDocumentTree tree = System.Text.Json.JsonSerializer.Deserialize<CadDocumentTree>(File.ReadAllText(_documentTree));

			this.assertTable(doc.BlockRecords, tree.BlocksTable);
		}

		private void assertTable<T>(CadDocument doc, Table<T> table)
			where T : TableEntry
		{
			Assert.NotNull(table);

			this.notNull(table.Document, $"Document not assigned to table {table}");
			Assert.Equal(doc, table.Document);
			Assert.Equal(doc, table.Owner);

			Assert.True(table.Handle != 0, "Table does not have a handle assigned");

			CadObject t = doc.GetCadObject(table.Handle);
			Assert.Equal(t, table);

			foreach (T entry in table)
			{
				Assert.Equal(entry.Owner.Handle, table.Handle);
				Assert.Equal(entry.Owner, table);

				this.documentObjectNotNull(doc, entry);
			}
		}

		private void assertTable<T>(Table<T> table, TableNode node)
			where T : TableEntry
		{
			this.assertObject(table, node);

			//By handle
			foreach (T entry in table)
			{
				TableEntryNode child = node.GetEntry(entry.Handle);
				if (child == null)
				{
					this.Output.WriteLine($"[{node.ACadName}] entry not found in the tree {entry.Handle} | {entry.Name}");
					continue;
				}

				this.assertObject(entry, child);
			}

			foreach (T entry in table)
			{
				TableEntryNode child = node.GetEntry(entry.Name);
				if (child == null)
				{
					this.Output.WriteLine($"[{node.ACadName}] entry not found in the tree {entry.Handle} | {entry.Name}");
					continue;
				}

				this.assertObject(entry, child);
			}

			foreach (Node c in node.Children)
			{

			}
		}
		private void assertObject(CadObject co, Node node)
		{
			Assert.True(co.Handle == node.Handle, $"[{co.GetType().FullName}] handle does not match, expected : {node.Handle} but was : {co.Handle}");
			Assert.True(co.Owner.Handle == node.OwnerHandle);

			if (co.XDictionary != null && this._document.Header.Version >= ACadVersion.AC1021)
			{
				Assert.True(co.XDictionary.Handle == node.DictionaryHandle);
				Assert.True(co.XDictionary.Owner == co);

				this.notNull<CadDocument>(co.XDictionary.Document, "Dictionary is not assigned to a document");
				Assert.Equal(co.Document, co.XDictionary.Document);
			}

			switch (co)
			{
				case BlockRecord record:
					this.assertCollection(record.Entities, node);
					break;
				default:
					break;
			}
		}

		private void assertCollection(IEnumerable<CadObject> collection, Node node)
		{
			//Check the actual elements in the collection
			foreach (CadObject entry in collection)
			{
				Node child = node.GetChild(entry.Handle);

				if (child == null)
				{
					this.Output.WriteLine($"Handle not found in collection : {entry.Handle}");
					continue;
				}

				this.assertObject(entry, child);
			}

			//Look for missing elements
			foreach (Node n in node.Children)
			{
				var o = collection.FirstOrDefault(x => x.Handle == n.Handle);
				if (o == null)
					this.Output.WriteLine($"Missing object in collection with handle with handle : {n.Handle} | owner handle : {n.OwnerHandle}");
			}
		}

		private void documentObjectNotNull<T>(CadDocument doc, T o)
			where T : CadObject
		{
			Assert.True(doc.GetCadObject(o.Handle) != null, $"Object of type {typeof(T)} | {o.Handle} not found in the doucment");

		}

		private void notNull<T>(T o, string info)
		{
			Assert.True(o != null, $"Object of type {typeof(T)} should not be null:  {info}");
		}

		private void entryNotNull<T>(Table<T> table, string entry)
			where T : TableEntry
		{
			Assert.True(table[entry] != null, $"Entry with name {entry} is null for thable {table}");
		}
	}
}
