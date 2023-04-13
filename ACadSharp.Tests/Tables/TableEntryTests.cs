using ACadSharp.Tests.Common;
using System;
using Xunit;

namespace ACadSharp.Tables.Tests
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