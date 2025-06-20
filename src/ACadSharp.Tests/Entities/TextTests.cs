using ACadSharp.Entities;
using ACadSharp.Tests.Common;
using CSMath;
using Xunit;

namespace ACadSharp.Tests.Entities
{
	public class TextTests : CommonEntityTests<TextEntity>
	{
		private CSMathRandom _random = new CSMathRandom();

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