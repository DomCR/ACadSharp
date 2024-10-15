using ACadSharp.Entities;
using ACadSharp.Tests.Common;
using CSMath;
using System.Collections.Generic;
using Xunit;

namespace ACadSharp.Tests.Entities
{
	public class ViewportTests
	{
		[Fact]
		public void GetBoundingBoxTest()
		{
			Viewport viewport = new Viewport();
			viewport.Width = 100;
			viewport.Height = 50;
			viewport.Center = new XYZ(10, 10, 0);

			BoundingBox boundingBox = viewport.GetBoundingBox();

			AssertUtils.AreEqual(viewport.Center, boundingBox.Center);
			AssertUtils.AreEqual(new XYZ(-40, -15, 0), boundingBox.Min);
			AssertUtils.AreEqual(new XYZ(60, 35, 0), boundingBox.Max);
		}

		[Fact]
		public void GetModelBoundingBoxTest()
		{
			Viewport viewport = new Viewport();
			viewport.Width = 100;
			viewport.Height = 50;
			viewport.ViewHeight = 50;
			viewport.ViewCenter = new XY(10, 10);

			BoundingBox boundingBox = viewport.GetModelBoundingBox();

			Assert.Equal(100, viewport.ViewWidth);
			AssertUtils.AreEqual(viewport.ViewCenter, ((XY)boundingBox.Center));
			AssertUtils.AreEqual(new XYZ(-40, -15, 0), boundingBox.Min);
			AssertUtils.AreEqual(new XYZ(60, 35, 0), boundingBox.Max);
		}

		[Fact]
		public void SelectEntitiesTest()
		{
			CadDocument doc = new CadDocument();

			Viewport viewport = new Viewport();
			viewport.Width = 100;
			viewport.Height = 50;
			viewport.ViewHeight = 50;
			viewport.ViewCenter = new XY(10, 10);

			//Viewbox
			//min: -40, -15
			//max: 60, 35

			doc.PaperSpace.Entities.Add(viewport);

			//Entities in the view
			Point ptIn = new Point(new XYZ(0, 0, 0));

			List<Entity> inView = new List<Entity>
			{
				ptIn
			};
			doc.Entities.AddRange(inView);

			//Entities out the view
			Point ptOut = new Point(new XYZ(100, 100, 0));

			List<Entity> outView = new List<Entity>
			{
				ptOut
			};
			doc.Entities.AddRange(outView);

			var selected = viewport.SelectEntities();

			Assert.NotEmpty(selected);

			foreach (Entity e in selected)
			{
				Assert.Contains(e, inView);
				Assert.DoesNotContain(e, outView);
			}
		}
	}
}
