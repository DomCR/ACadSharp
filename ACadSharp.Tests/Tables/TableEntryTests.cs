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
			entry.OnReferenceChanged += this.tableEntry_OnReferenceChanged;

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

		private void tableEntry_OnReferenceChanged(object sender, ReferenceChangedEventArgs e)
		{
			//The clone must not have any attachment
			throw new InvalidOperationException();
		}
	}
}