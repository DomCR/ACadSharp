using ACadSharp.Entities;
using ACadSharp.Tests.Common;
using CSMath;
using Xunit;

namespace ACadSharp.Tests.Entities;

public class TextTests : CommonEntityTests<TextEntity>
{
	public override void GetBoundingBoxTest()
	{
		TextEntity text = new TextEntity();
		text.InsertPoint = new XYZ(5, 5, 5);

		BoundingBox b = text.GetBoundingBox();
		Assert.Equal(new BoundingBox(new XYZ(5, 5, 5)), b);
	}

	[Fact]
	public void TranslationTest()
	{
		XYZ newLocation = this._random.NextXYZ();
		TextEntity text = new TextEntity();
		Transform transform = Transform.CreateTranslation(newLocation);

		text.ApplyTransform(transform);
		AssertUtils.AreEqual(text.InsertPoint, newLocation);
	}

	[Theory]
	[InlineData(TextHorizontalAlignment.Center, TextVerticalAlignmentType.Baseline)]
	[InlineData(TextHorizontalAlignment.Left, TextVerticalAlignmentType.Top)]
	public void TranslationTransformsOriginAlignmentPointForJustifiedText(
		TextHorizontalAlignment horizontalAlignment,
		TextVerticalAlignmentType verticalAlignment)
	{
		XYZ translation = new XYZ(10, 20, 0);
		TextEntity text = new TextEntity
		{
			HorizontalAlignment = horizontalAlignment,
			VerticalAlignment = verticalAlignment,
			AlignmentPoint = XYZ.Zero,
		};

		text.ApplyTransform(Transform.CreateTranslation(translation));

		AssertUtils.AreEqual(text.AlignmentPoint, translation);
	}

	[Fact]
	public void TranslationLeavesUnusedOriginAlignmentPointUnchanged()
	{
		TextEntity text = new TextEntity();

		text.ApplyTransform(Transform.CreateTranslation(new XYZ(10, 20, 0)));

		AssertUtils.AreEqual(text.AlignmentPoint, XYZ.Zero);
	}
}
