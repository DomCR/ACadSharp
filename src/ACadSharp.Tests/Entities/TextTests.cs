using ACadSharp.Entities;
using ACadSharp.Tests.Common;
using CSMath;
using Xunit;

namespace ACadSharp.Tests.Entities
{
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
			XYZ newLocation = _random.NextXYZ();
			TextEntity text = new TextEntity();
			Transform transform = Transform.CreateTranslation(newLocation);

			text.ApplyTransform(transform);
			AssertUtils.AreEqual(text.InsertPoint, newLocation);
		}
	}
}