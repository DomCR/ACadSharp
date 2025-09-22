using ACadSharp.Entities;
using ACadSharp.Objects;
using ACadSharp.Tables;
using ACadSharp.Tables.Collections;
using ACadSharp.Tests.TestModels;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Xunit;
using Xunit.Abstractions;

namespace ACadSharp.Tests.Common
{
	public class DocumentIntegrity
	{
		public bool IsDxf { get; set; }

		public ITestOutputHelper Output { get; set; }

		private const string _folder = "../../../../ACadSharp.Tests/Data/";

		private CadDocument _document;

		public DocumentIntegrity(ITestOutputHelper output)
		{
			this.Output = output;
		}

		public void AssertTableHierarchy(CadDocument doc)
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
			var layout = doc.Layouts.FirstOrDefault(l => l.Name == Layout.ModelLayoutName);

			this.notNull(layout, "Layout Model is null");

			Assert.True(layout.AssociatedBlock == doc.ModelSpace);
		}

		public void AssertBlockRecords(CadDocument doc)
		{
			foreach (BlockRecord br in doc.BlockRecords)
			{
				Assert.Equal(br.Name, br.BlockEntity.Name);
				Assert.NotNull(br.BlockEntity.Document);
				this.documentObjectNotNull(doc, br.BlockEntity);

				Assert.True(br.Handle == br.BlockEntity.Owner.Handle, "Block entity owner doesn't match");
				Assert.NotNull(br.BlockEntity.Document);

				Assert.NotNull(br.BlockEnd.Document);
				this.documentObjectNotNull(doc, br.BlockEnd);

				foreach (Entity e in br.Entities)
				{
					this.documentObjectNotNull(doc, e);
				}
			}
		}

		public void AssertDocumentContent(CadDocument doc)
		{
#if !NETFRAMEWORK
			this._document = doc;
			CadDocumentTree tree = System.Text.Json.JsonSerializer.Deserialize<CadDocumentTree>(
				File.ReadAllText(Path.Combine(_folder, $"sample_{doc.Header.Version}_tree.json"))
				);

			if (doc.Header.Version > ACadVersion.AC1021)
			{
				this.assertTableContent(doc.AppIds, tree.AppIdsTable);
			}
			this.assertTableContent(doc.BlockRecords, tree.BlocksTable);
			this.assertTableContent(doc.DimensionStyles, tree.DimensionStylesTable);
			this.assertTableContent(doc.Layers, tree.LayersTable);
			this.assertTableContent(doc.LineTypes, tree.LineTypesTable);
			this.assertTableContent(doc.TextStyles, tree.TextStylesTable);
			this.assertTableContent(doc.UCSs, tree.UCSsTable);
			this.assertTableContent(doc.Views, tree.ViewsTable);
			this.assertTableContent(doc.VPorts, tree.VPortsTable);
#endif
		}

		public void AssertDocumentTree(CadDocument doc)
		{
#if !NETFRAMEWORK
			this._document = doc;
			CadDocumentTree tree = System.Text.Json.JsonSerializer.Deserialize<CadDocumentTree>(
						File.ReadAllText(Path.Combine(_folder, $"sample_{doc.Header.Version}_tree.json"))
						);

			this.assertTableTree(doc.BlockRecords, tree.BlocksTable);
			this.assertTableTree(doc.Layers, tree.LayersTable);
#endif
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

		private void assertTableContent<T, R>(Table<T> table, TableNode<R> node)
			where T : TableEntry
			where R : TableEntryNode
		{
			this.assertObject(table, node);

			foreach (T entry in table)
			{
				if (entry.Name.Contains("__") || entry.Name.Contains(" @ "))
					continue;

				TableEntryNode child = node.GetEntry(entry.Handle);
				if (child == null && (entry.Name.StartsWith("*U") || entry.Name.StartsWith("*T")))
				{
					return;
				}

				this.notNull(child, $"[{table}] Entry name: {entry.Name}");

				this.assertObject(entry, child);

				switch (entry)
				{
					case BlockRecord record when child is BlockRecordNode blockRecordNode:
						this.assertCollectionContent(record.Entities, blockRecordNode.Entities);
						break;
					case Layer layer when child is LayerNode layerNode:
						this.assertLayer(layer, layerNode);
						break;
					default:
						break;
				}
			}
		}

		private void assertTableTree<T, R>(Table<T> table, TableNode<R> node)
			where T : TableEntry
			where R : TableEntryNode
		{
			this.assertObject(table, node);

			foreach (R child in node.Entries)
			{
				//Blocks are not saved in the dwg file
				if (child.Name == "*D22" || child.Name == "*D23")
				{
					continue;
				}

				if (this._document.Header.Version < ACadVersion.AC1024 &&
					child is BlockRecordNode tmp &&
					tmp.IsDynamic)
				{
					continue;
				}

				Assert.True(table.TryGetValue(child.Name, out T entry), $"Entry not found: {child.Name}");
				this.assertObject(entry, child);

				switch (entry)
				{
					case BlockRecord record when child is BlockRecordNode blockRecordNode:
						if (record.Name.StartsWith("*T"))
						{
							//The dynamic block instance for tables are generated on the spot and not saved.
							break;
						}

						this.assertCollectionTree(record.Entities, blockRecordNode.Entities);
						break;
					case Layer layer when child is LayerNode layerNode:
						this.assertLayer(layer, layerNode);
						break;
					default:
						break;
				}
			}
		}

		private void assertObject(CadObject co, Node node)
		{
			Assert.True(co.Handle == node.Handle, $"[{co.GetType().FullName}] handle doesn't match;  actual : {co.Handle} | expected : {node.Handle}");
			Assert.True(co.Owner.Handle == node.OwnerHandle);

			if (co.XDictionary != null && this._document.Header.Version >= ACadVersion.AC1021)
			{
				// Some versions do not add dictionaries to some entities
				if (node.DictionaryHandle != 0 && false)    //TODO: handles does not match for the different versions, the export script for DocumentTree should handle that
				{
					Assert.True(co.XDictionary.Handle == node.DictionaryHandle, $"Dictionary handle doesn't match; actual: {co.XDictionary.Handle} | expected {node.DictionaryHandle}");
					Assert.True(co.XDictionary.Owner == co);
				}

				this.notNull<CadDocument>(co.XDictionary.Document, "Dictionary is not assigned to a document");
				Assert.Equal(co.Document, co.XDictionary.Document);
			}
		}

		private void assertEntity(Entity entity, EntityNode node)
		{
			this.assertObject(entity, node);

			if (this._document.Header.Version > ACadVersion.AC1021) //TODO: For TextEntity the default layer is changed for "0 @ 1"
				Assert.Equal(entity.Layer.Name, node.LayerName);

			//Assert.Equal(entity.Transparency, node.Transparency);
			Assert.Equal(entity.LineType.Name, node.LinetypeName, ignoreCase: true);
			Assert.Equal(entity.LineTypeScale, node.LinetypeScale);

			if (this._document.Header.Version > ACadVersion.AC1014)
			{
				Assert.Equal(entity.IsInvisible, node.IsInvisible);
				Assert.Equal(entity.LineWeight, node.LineWeight);
			}

			switch (entity)
			{
				case Dimension dim:
					assertDimensionProperties(dim, node);
					break;
			}
		}

		private void assertLayer(Layer layer, LayerNode node)
		{
			//Assert.Equal(entity.Transparency, node.Transparency);
			Assert.Equal(layer.LineType.Name, node.LinetypeName, ignoreCase: true);

			if (this._document.Header.Version > ACadVersion.AC1014)
			{
				Assert.Equal(layer.LineWeight, node.LineWeight);
			}
		}

		private void assertCollectionContent(IEnumerable<Entity> collection, IEnumerable<EntityNode> node)
		{
			foreach (Entity e in collection)
			{
				EntityNode child = node.FirstOrDefault(x => x.Handle == e.Handle);
				this.notNull(child, $"Entity: {e}");

				this.assertEntity(e, child);
			}
		}

		private void assertCollectionTree(IEnumerable<Entity> collection, IEnumerable<EntityNode> node)
		{
			//Look for missing elements
			foreach (EntityNode n in node)
			{
				var e = collection.FirstOrDefault(x => x.Handle == n.Handle);
				this.notNull(e, $"Entity: {n}");

				this.assertEntity(e, n);
			}
		}

		private void documentObjectNotNull<T>(CadDocument doc, T o)
			where T : CadObject
		{
			CadObject cobj = doc.GetCadObject(o.Handle);
			this.notNull(cobj, $"Object of type {typeof(T)} | {o.Handle} not found in the document");
			this.notNull(cobj.Document, $"Document is null for object with handle: {cobj.Handle}");
		}

		private void assertDimensionProperties(Dimension dimension, EntityNode node)
		{
#if !NETFRAMEWORK
			if (node.Properties.TryGetValue(nameof(dimension.Measurement), out object measurement))
			{
				Assert.Equal(((System.Text.Json.JsonElement)measurement).GetDouble(), dimension.Measurement, 4);
			}
#endif
		}

		private void notNull<T>(T o, string info = null)
		{
			Assert.True(o != null, $"Object of type {typeof(T)} should not be null: {info}");
		}

		private void entryNotNull<T>(Table<T> table, string entry)
			where T : TableEntry
		{
			var record = table[entry];
			Assert.True(record != null, $"Entry with name {entry} is null for table {table}");
			Assert.NotNull(record.Document);
		}
	}
}
