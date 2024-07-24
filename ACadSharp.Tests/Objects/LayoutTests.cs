using ACadSharp.Objects;
using System;
using System.Linq;
using Xunit;

namespace ACadSharp.Tests.Objects
{
	public class LayoutTests
	{
		[Fact]
		public void AddLayout()
		{
			CadDocument document = new CadDocument();

			string layoutName = "my_layout";
			Layout layout = new Layout(layoutName);

			document.Layouts.Add(layout);

			Assert.Equal(layoutName, layout.AssociatedBlock.Name);
			Assert.Equal(3, document.Layouts.Count());
			Assert.True(document.BlockRecords.Contains(layoutName));
		}

		[Fact]
		public void RemoveBlockRecordTest()
		{
			var document = new CadDocument();

			string layoutName = "my_layout";
			Layout layout = new Layout(layoutName);

			document.Layouts.Add(layout);

			document.BlockRecords.Remove(layoutName);

			Assert.Equal(2, document.Layouts.Count());
			Assert.False(document.Layouts.ContainsKey(layoutName));
			Assert.False(document.BlockRecords.Contains(layoutName));
		}

		[Fact]
		public void RemoveTest()
		{
			var document = new CadDocument();

			string layoutName = "my_layout";
			Layout layout = new Layout(layoutName);

			document.Layouts.Add(layout);

			Assert.True(document.Layouts.Remove(layoutName));
			Assert.Equal(2, document.Layouts.Count());
			Assert.True(document.BlockRecords.Contains(layoutName));
			Assert.NotEqual(document.BlockRecords[layoutName], layout.AssociatedBlock);
		}

		[Fact]
		public void CannotRemoveDefaultLayouts()
		{
			var document = new CadDocument();

			Assert.Throws<ArgumentException>(() => document.Layouts.Remove(Layout.ModelLayoutName));
			Assert.Throws<ArgumentException>(() => document.Layouts.Remove(Layout.PaperLayoutName));
		}
	}
}
