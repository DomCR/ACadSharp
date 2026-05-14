using ACadSharp.Extensions;
using ACadSharp.Tables;
using ACadSharp.Tables.Collections;
using System;
using Xunit;

namespace ACadSharp.Tests.Tables
{
	public abstract class TableEntryCommonTests<T>
		where T : TableEntry
	{
		public static string DefaultName = "table_entry";

		[Fact]
		public void ChangeName()
		{
			T entry = this.createEntry();

			CadDocument doc = new CadDocument();
			Table<T> table = this.getTable(doc);

			table.Add(entry);

			entry.Name = "new_name";

			Assert.NotNull(table[entry.Name]);
			Assert.False(table.TryGetValue(DefaultName, out _));
		}

		[Fact]
		public void DetachCloneEvents()
		{
			string initialName = "custom_layer";
			Layer layer = new Layer(initialName);

			CadDocument doc = new CadDocument();
			doc.Layers.Add(layer);

			Layer clone = layer.CloneTyped();
			string name = "new_name";
			clone.Name = name;

			Assert.False(doc.Layers.Contains(name));
		}

		[Fact]
		public void IsValidNameTest()
		{
			for (int i = 0; i < INamedCadObjectExtensions.InvalidCharacters.Length; i++)
			{
				var entry = this.createEntry($"{DefaultName}{INamedCadObjectExtensions.InvalidCharacters[i]}");

				Assert.False(entry.IsValidDxfName());
			}
		}

		[Fact]
		public void SetFlagUsingMapper()
		{
			T entry = this.createEntry();

			var map = DxfClassMap.Create<T>();
			map.DxfProperties[70].SetValue(entry, StandardFlags.XrefResolved);

			Assert.True(entry.Flags.HasFlag(StandardFlags.XrefResolved));
		}

		protected T createEntry()
		{
			return this.createEntry(DefaultName);
		}

		protected virtual T createEntry(string name)
		{
			return (T)Activator.CreateInstance(typeof(T), name);
		}

		protected abstract Table<T> getTable(CadDocument document);
	}
}