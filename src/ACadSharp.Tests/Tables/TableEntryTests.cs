using ACadSharp.Tables;
using ACadSharp.Tests.Common;
using System;
using Xunit;

namespace ACadSharp.Tests.Tables
{
	public class TableEntryTests
	{
		public static readonly TheoryData<Type> TableEntryTypes = new TheoryData<Type>();

		static TableEntryTests()
		{
			foreach (var item in DataFactory.GetTypes<TableEntry>())
			{
				TableEntryTypes.Add(item);
			}
		}

		[Theory]
		[MemberData(nameof(TableEntryTypes))]
		public void Clone(Type entryType)
		{
			TableEntry entry = TableEntryFactory.Create(entryType);
			TableEntry clone = (TableEntry)entry.Clone();

			CadObjectTestUtils.AssertTableEntryClone(entry, clone);
		}

		[Theory]
		[MemberData(nameof(TableEntryTypes))]
		public void CloneUnattachEvent(Type t)
		{
			TableEntry entry = TableEntryFactory.Create(t);

			TableEntry clone = (TableEntry)entry.Clone();

			CadObjectTestUtils.AssertTableEntryClone(entry, clone);
		}

		[Fact()]
		public void ChangeName()
		{
			string initialName = "custom_layer";
			Layer layer = new Layer(initialName);

			CadDocument doc = new CadDocument();

			doc.Layers.Add(layer);

			layer.Name = "new_name";

			Assert.NotNull(doc.Layers[layer.Name]);
			Assert.False(doc.Layers.TryGetValue(initialName, out _));
		}

		[Fact()]
		public void SetFlagUsingMapper()
		{
			Layer layer = new Layer("My layer");

			var map = DxfClassMap.Create<Layer>();
			map.DxfProperties[70].SetValue(layer, LayerFlags.Frozen);

			Assert.True(layer.Flags.HasFlag(LayerFlags.Frozen));
		}
	}
}