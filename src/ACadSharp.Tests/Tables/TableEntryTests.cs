using ACadSharp.Extensions;
using ACadSharp.Tables;
using ACadSharp.Tests.Common;
using System;
using System.Reflection;
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

		[Fact]
		public void ChangeNameCloned()
		{
			string initialName = "custom_layer";
			Layer layer = new Layer(initialName);

			CadDocument doc = new CadDocument();

			doc.Layers.Add(layer);

			layer.Name = "new_name";
			layer.OnNameChanged += (sender, args) =>
			{
				throw new Exception();
			};

			Layer clone = layer.CloneTyped();
			clone.Name = "test-event";

			Assert.ThrowsAny<Exception>(() => layer.Name = "Hello");

			var field = typeof(TableEntry)
				.GetField(nameof(TableEntry.OnNameChanged), BindingFlags.Instance | BindingFlags.NonPublic);

			var e = field.GetValue(layer) as EventHandler<OnNameChangedArgs>;
			Assert.NotEmpty(e.GetInvocationList());
			e = field.GetValue(clone) as EventHandler<OnNameChangedArgs>;
			Assert.Null(e);
		}

		[Theory]
		[MemberData(nameof(TableEntryTypes))]
		public void Clone(Type entryType)
		{
			TableEntry entry = TableEntryFactory.Create(entryType);
			TableEntry clone = (TableEntry)entry.Clone();

			CadObjectTestUtils.AssertTableEntryClone(entry, clone);
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