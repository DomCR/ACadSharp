using ACadSharp.Objects;
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

		Assert.Equal(_layoutName, layout.AssociatedBlock.Name);
		Assert.Equal(3, document.Layouts.Count());
		Assert.True(document.BlockRecords.Contains(_layoutName));
	}

	[Fact]
	public void CannotRemoveDefaultLayouts()
	{
		var document = new CadDocument();

		Assert.Throws<ArgumentException>(() => document.Layouts.Remove(Layout.ModelLayoutName));
		Assert.Throws<ArgumentException>(() => document.Layouts.Remove(Layout.PaperLayoutName));
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

		document.BlockRecords.Remove(_layoutName);

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

		Assert.True(document.Layouts.Remove(_layoutName));
		Assert.Equal(2, document.Layouts.Count());
		Assert.True(document.BlockRecords.Contains(_layoutName));
		Assert.NotEqual(document.BlockRecords[_layoutName], layout.AssociatedBlock);
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