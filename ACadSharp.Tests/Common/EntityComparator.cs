using ACadSharp.Entities;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ACadSharp.Tests.Common
{
	public static class EntityComparator
	{
		public static void IsEqual(CadObject expected, CadObject actual)
		{
			AssertUtils.AreEqual(expected.Handle, actual.Handle, nameof(actual.Handle));
		}

		/// <summary>
		/// Compares an entity with another
		/// </summary>
		/// <param name=)expected)></param>
		/// <param name=)actual)></param>
		/// <param name=)compareLinks)>Compare the links to other objects like layer or linetype</param>
		public static void IsEqual(Entity expected, Entity actual, bool compareLinks = false)
		{
			IsEqual((CadObject)expected, (CadObject)actual);

			//AssertUtils.AreEqual(expected.Color.Equals(actual.Color), nameof(actual. color));
			AssertUtils.AreEqual(expected.LineWeight, actual.LineWeight, nameof(actual.LineWeight));
			AssertUtils.AreEqual(expected.LinetypeScale, actual.LinetypeScale, nameof(actual.LinetypeScale));
			AssertUtils.AreEqual(expected.IsInvisible, actual.IsInvisible, nameof(actual.IsInvisible));
			//AssertUtils.AreEqual(expected.Transparency.Equals(actual.Transparency), nameof(actual. Transparency));

			if (compareLinks)
			{
				//To implement
			}
		}

		public static void IsEqual(Arc expected, Arc actual, bool compareLinks = false)
		{
			IsEqual((Circle)expected, (Circle)actual, compareLinks);

			AssertUtils.AreEqual(expected.StartAngle, actual.StartAngle, nameof(actual.StartAngle));
			AssertUtils.AreEqual(expected.EndAngle, actual.EndAngle, nameof(actual.EndAngle));
		}

		public static void IsEqual(Circle expected, Circle actual, bool compareLinks = false)
		{
			IsEqual((Entity)expected, (Entity)actual, compareLinks);

			AssertUtils.AreEqual(expected.Normal, actual.Normal, nameof(actual.Normal));
			AssertUtils.AreEqual(expected.Thickness, actual.Thickness, nameof(actual.Thickness));
			AssertUtils.AreEqual(expected.Center, actual.Center, nameof(actual.Center));
			AssertUtils.AreEqual(expected.Radius, actual.Radius, nameof(actual.Radius));
		}

		public static void IsEqual(Line expected, Line actual, bool compareLinks = false)
		{
			IsEqual((Entity)expected, (Entity)actual, compareLinks);

			AssertUtils.AreEqual(expected.Normal, actual.Normal, nameof(actual.Normal));
			AssertUtils.AreEqual(expected.Thickness, actual.Thickness, nameof(actual.Thickness));
			AssertUtils.AreEqual(expected.StartPoint, actual.StartPoint, nameof(actual.StartPoint));
			AssertUtils.AreEqual(expected.EndPoint, actual.EndPoint, nameof(actual.EndPoint));
		}

		public static void IsEqual(Point expected, Point actual, bool compareLinks = false)
		{
			IsEqual((Entity)expected, (Entity)actual, compareLinks);

			AssertUtils.AreEqual(expected.Normal, actual.Normal, nameof(actual.Normal));
			AssertUtils.AreEqual(expected.Thickness, actual.Thickness, nameof(actual.Thickness));
			AssertUtils.AreEqual(expected.Location, actual.Location, nameof(actual.Location));
			AssertUtils.AreEqual(expected.Rotation, actual.Rotation, nameof(actual.Rotation));
		}

		public static void IsEqual(TextEntity expected, TextEntity actual, bool compareLinks = false)
		{
			IsEqual((Entity)expected, (Entity)actual, compareLinks);

			AssertUtils.AreEqual(expected.Normal, actual.Normal, nameof(actual.Normal));
			AssertUtils.AreEqual(expected.Thickness, actual.Thickness, nameof(actual.Thickness));
			AssertUtils.AreEqual(expected.InsertPoint, actual.InsertPoint, nameof(actual.InsertPoint));
			Assert.AreEqual(expected.AlignmentPoint, actual.AlignmentPoint, nameof(actual.AlignmentPoint));
			AssertUtils.AreEqual(expected.Rotation, actual.Rotation, nameof(actual.Rotation));
			AssertUtils.AreEqual(expected.Height, actual.Height, nameof(actual.Height));
			AssertUtils.AreEqual(expected.Value, actual.Value, nameof(actual.Value));
			AssertUtils.AreEqual(expected.WidthFactor, actual.WidthFactor, nameof(actual.WidthFactor));
			AssertUtils.AreEqual(expected.ObliqueAngle, actual.ObliqueAngle, nameof(actual.ObliqueAngle));
			AssertUtils.AreEqual(expected.Mirror, actual.Mirror, nameof(actual.Mirror));
			AssertUtils.AreEqual(expected.HorizontalAlignment, actual.HorizontalAlignment, nameof(actual.HorizontalAlignment));
			AssertUtils.AreEqual(expected.VerticalAlignment, actual.VerticalAlignment, nameof(actual.HorizontalAlignment));
		}
	}
}
