using ACadSharp.Entities;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ACadSharp.Tests.Common
{
	public static class EntityComparator
	{
		public static void IsEqual(CadObject expected, CadObject actual)
		{
			Assert.IsTrue(expected.Handle == actual.Handle, "Different handle");
		}

		/// <summary>
		/// Compares an entity with another
		/// </summary>
		/// <param name="expected"></param>
		/// <param name="actual"></param>
		/// <param name="compareLinks">Compare the links to other objects like layer or linetype</param>
		public static void IsEqual(Entity expected, Entity actual, bool compareLinks = false)
		{
			IsEqual((CadObject)expected, (CadObject)actual);

			//Assert.IsTrue(expected.Color.Equals(actual.Color), "Different color");
			Assert.IsTrue(expected.LineWeight == actual.LineWeight, "Different LineWeight");
			Assert.IsTrue(expected.LinetypeScale == actual.LinetypeScale, "Different LinetypeScale");
			Assert.IsTrue(expected.LinetypeScale == actual.LinetypeScale, "Different LinetypeScale");
			Assert.IsTrue(expected.IsInvisible == actual.IsInvisible, "Different IsInvisible");
			//Assert.IsTrue(expected.Transparency.Equals(actual.Transparency), "Different Transparency");

			if (compareLinks)
			{
				//To implement
			}
		}

		public static void IsEqual(Arc expected, Arc actual, bool compareLinks = false)
		{
			IsEqual((Circle)expected, (Circle)actual, compareLinks);

			AssertUtils.AreEqual(expected.StartAngle, actual.StartAngle, TestVariables.Delta, nameof(actual.StartAngle));
			AssertUtils.AreEqual(expected.EndAngle, actual.EndAngle, nameof(actual.EndAngle));
		}

		public static void IsEqual(Circle expected, Circle actual, bool compareLinks = false)
		{
			IsEqual((Entity)expected, (Entity)actual, compareLinks);

			Assert.IsTrue(expected.Normal == actual.Normal, "Different Normal");
			Assert.IsTrue(expected.Thickness == actual.Thickness, "Different Thickness");
			Assert.IsTrue(expected.Center == actual.Center, "Different Center");
			Assert.IsTrue(expected.Radius == actual.Radius, "Different Radius");
		}

		public static void IsEqual(Line expected, Line actual, bool compareLinks = false)
		{
			IsEqual((Entity)expected, (Entity)actual, compareLinks);

			Assert.IsTrue(expected.Normal == actual.Normal, "Different Normal");
			Assert.IsTrue(expected.Thickness == actual.Thickness, "Different Thickness");
			Assert.IsTrue(expected.StartPoint == actual.StartPoint, "Different StartPoint");
			Assert.IsTrue(expected.EndPoint == actual.EndPoint, "Different EndPoint");
		}

		public static void IsEqual(Point expected, Point actual, bool compareLinks = false)
		{
			IsEqual((Entity)expected, (Entity)actual, compareLinks);

			Assert.IsTrue(expected.Normal == actual.Normal, "Different Normal");
			Assert.IsTrue(expected.Thickness == actual.Thickness, "Different Thickness");
			Assert.IsTrue(expected.Location == actual.Location, "Different Location");
			Assert.IsTrue(expected.Rotation == actual.Rotation, "Different Rotation");
		}

		public static void IsEqual(TextEntity expected, TextEntity actual, bool compareLinks = false)
		{
			IsEqual((Entity)expected, (Entity)actual, compareLinks);

			Assert.IsTrue(expected.Normal == actual.Normal, "Different Normal");
			Assert.IsTrue(expected.Thickness == actual.Thickness, "Different Thickness");
			Assert.IsTrue(expected.InsertPoint == actual.InsertPoint, "Different InsertPoint");
			Assert.AreEqual(expected.AlignmentPoint, actual.AlignmentPoint, "Different AlignmentPoint");
			Assert.IsTrue(expected.Rotation == actual.Rotation, "Different Rotation");
			Assert.IsTrue(expected.Height == actual.Height, "Different Height");
			Assert.IsTrue(expected.Value == actual.Value, "Different Value");
			Assert.IsTrue(expected.WidthFactor == actual.WidthFactor, "Different WidthFactor");
			Assert.IsTrue(expected.ObliqueAngle == actual.ObliqueAngle, "Different ObliqueAngle");
			Assert.IsTrue(expected.Mirror == actual.Mirror, "Different Mirror");
			Assert.IsTrue(expected.HorizontalAlignment == actual.HorizontalAlignment, "Different HorizontalAlignment");
			Assert.IsTrue(expected.VerticalAlignment == actual.VerticalAlignment, "Different HorizontalAlignment");
		}
	}
}
