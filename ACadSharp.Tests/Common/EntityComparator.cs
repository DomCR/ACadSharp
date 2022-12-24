using ACadSharp.Entities;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Text;

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
			IsEqual((Circle)expected, (Circle)actual);

			Assert.IsTrue(expected.StartAngle == actual.StartAngle, "Different StartAngle");
			Assert.IsTrue(expected.EndAngle == actual.EndAngle, "Different EndAngle");
		}

		public static void IsEqual(Circle expected, Circle actual, bool compareLinks = false)
		{
			IsEqual((Entity)expected, (Entity)actual);

			Assert.IsTrue(expected.Normal == actual.Normal, "Different Normal");
			Assert.IsTrue(expected.Thickness == actual.Thickness, "Different Thickness");
			Assert.IsTrue(expected.Center == actual.Center, "Different Center");
			Assert.IsTrue(expected.Radius == actual.Radius, "Different Radius");
		}
	}
}
