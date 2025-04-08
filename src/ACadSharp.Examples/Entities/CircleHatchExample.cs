using ACadSharp.Entities;
using CSMath;
using System.Collections.Generic;

namespace ACadSharp.Examples.Entities
{
	internal class CircleHatchExample
	{
		public static Hatch CreateCircleHatch(XY center, double radius)
		{
			Hatch hatch = new Hatch();
			hatch.IsSolid = true;
			hatch.SeedPoints.Add(new XY());
			hatch.Color = new Color(15);

			List<Hatch.BoundaryPath.Line> edges = new List<Hatch.BoundaryPath.Line>();

			Hatch.BoundaryPath.Arc arc = new();
			arc.Center = center;
			arc.CounterClockWise = true;
			arc.Radius = radius;
			arc.StartAngle = 0;
			arc.EndAngle = MathHelper.TwoPI;

			Hatch.BoundaryPath path = new Hatch.BoundaryPath();
			path.Edges.Add(arc);
			hatch.Paths.Add(path);

			return hatch;
		}

		/// <summary>
		/// Create some circle hatches and add them to the model
		/// </summary>
		public static void AddInsertIntoModel()
		{
			var hatches = new List<Hatch>();
			hatches.Add(CreateCircleHatch(new XY(0, 15), 10));
			hatches.Add(CreateCircleHatch(new XY(30, 12), 5));
			hatches.Add(CreateCircleHatch(new XY(50, 50), 25.3));
			hatches.Add(CreateCircleHatch(new XY(100, 50), 5));
			hatches.Add(CreateCircleHatch(new XY(-30, 10), 30));

			CadDocument doc = new CadDocument();
			doc.ModelSpace.Entities.AddRange(hatches);

			//Once the file is saved you will have some circular hatches added to its modelspace
		}
	}
}