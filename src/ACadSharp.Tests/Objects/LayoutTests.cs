using ACadSharp.Objects;
using ACadSharp.Tables;
using System;
using System.Linq;
using Xunit;

namespace ACadSharp.Tests.Objects;

public class LayoutTests : NonGraphicalObjectTests<Layout>
{
	private const string _layoutName = "my_layout";

	[Fact]
	public void AddLayout()
	{
		CadDocument document = new CadDocument();

		Layout layout = new Layout(_layoutName);
		document.Layouts.Add(layout);

		Assert.Equal($"{BlockRecord.PaperSpaceName}0", layout.AssociatedBlock.Name);
		Assert.Equal(3, document.Layouts.Count());
		Assert.True(document.BlockRecords.Contains($"{BlockRecord.PaperSpaceName}0"));
		Assert.True(document.BlockRecords.Contains(BlockRecord.PaperSpaceName));
	}

	[Fact]
	public void CannotRemoveDefaultLayouts()
	{
		var document = new CadDocument();

		Assert.Throws<ArgumentException>(() => document.Layouts.Remove(Layout.ModelLayoutName));
		Assert.Throws<ArgumentException>(() => document.Layouts.Remove(Layout.PaperLayoutName));
	}

	[Fact]
	public void AddMultipleLayoutsAssignsConsecutivePaperSpaceNames()
	{
		CadDocument document = new CadDocument();

		Layout first = new Layout("first");
		Layout second = new Layout("second");
		Layout third = new Layout("third");

		document.Layouts.Add(first);
		document.Layouts.Add(second);
		document.Layouts.Add(third);

		Assert.Equal($"{BlockRecord.PaperSpaceName}0", first.AssociatedBlock.Name);
		Assert.Equal($"{BlockRecord.PaperSpaceName}1", second.AssociatedBlock.Name);
		Assert.Equal($"{BlockRecord.PaperSpaceName}2", third.AssociatedBlock.Name);
	}

	[Fact]
	public void CreateDefaultLayout()
	{
		Layout layout = new Layout(_layoutName);

		Assert.NotEmpty(layout.Viewports);
		Assert.NotNull(layout.AssociatedBlock);
		Assert.NotNull(layout.AssociatedBlock.Layout);
		Assert.True(layout.Viewports.First().RepresentsPaper);
		Assert.False(layout.AssociatedBlock.Entities.Remove(layout.Viewports.First()));
	}

	[Fact]
	public void RemoveBlockRecordTest()
	{
		var document = new CadDocument();
		Layout layout = new Layout(_layoutName);

		document.Layouts.Add(layout);

		document.BlockRecords.Remove(layout.AssociatedBlock.Name);

		Assert.Equal(2, document.Layouts.Count());
		Assert.False(document.Layouts.ContainsKey(_layoutName));
		Assert.False(document.BlockRecords.Contains(_layoutName));
	}

	[Fact]
	public void RemoveTest()
	{
		var document = new CadDocument();
		Layout layout = new Layout(_layoutName);

		document.Layouts.Add(layout);

		string associatedBlockName = layout.AssociatedBlock.Name;
		Assert.True(document.Layouts.Remove(_layoutName));
		Assert.Equal(2, document.Layouts.Count());
		Assert.False(document.BlockRecords.Contains(associatedBlockName));
	}

	[Fact]
	public void UpdatePaperViewportTest()
	{
		string layoutName = _layoutName;
		Layout layout = new Layout(layoutName);
		layout.PaperHeight = 10;
		layout.PaperWidth = 10;

		layout.UpdatePaperViewport();

		Assert.NotNull(layout.PaperViewport);
		Assert.Equal(layout.PaperViewport.Height, layout.PaperHeight);
		Assert.Equal(layout.PaperViewport.Width, layout.PaperWidth);
	}
}