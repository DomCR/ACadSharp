using ACadSharp.Entities;
using CSMath;
using System.Collections.Generic;

namespace ACadSharp.Examples.Entities
{
	public static class LineExample
	{
		/// <summary>
		/// Get a Line object with some given properties
		/// </summary>
		public static Line CreateLine(XYZ startPoint, XYZ endPoint, short colorIndex)
		{
			var line = new Line();
			line.Color = new Color(colorIndex); //index color, from 0 to 255
			line.StartPoint = startPoint;
			line.EndPoint = endPoint;

			return line;
		}

		/// <summary>
		/// Create some lines and add it to the model
		/// </summary>
		public static void AddLinesIntoModel()
		{
			var lines = new List<Line>();
			lines.Add(CreateLine(new XYZ(0, 15, 0), new XYZ(5, -12, 0), 10));
			lines.Add(CreateLine(new XYZ(0, 15, 0), new XYZ(5, -12, 0), 25));
			lines.Add(CreateLine(new XYZ(0, 15, 0), new XYZ(5, -12, 0), 1));

			CadDocument doc = new CadDocument();
			doc.ModelSpace.Entities.AddRange(lines);

			//Once the file is saved you will have some lines added to its modelspace
		}
	}
}