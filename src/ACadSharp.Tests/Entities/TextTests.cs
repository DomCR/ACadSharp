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
		XYZ ip = new XYZ(5, 5, 5);
		text.InsertPoint = ip;

		// [MERGE] Upstream returned a degenerate 0x0 point box (insertion point only).
		// The patched GetBoundingBox now returns a real extent computed from the font
		// metrics via TextBoundsHelper, so the box is finite and has a non-zero span.
		BoundingBox b = text.GetBoundingBox();
		Assert.Equal(BoundingBoxExtent.Finite, b.Extent);
		Assert.True(b.LengthX > 0 || b.LengthY > 0);
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
}