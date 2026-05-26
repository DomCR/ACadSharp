using CSMath;
using System.Collections.Generic;

namespace ACadSharp.Entities
{
public abstract partial class ModelerGeometry
	{
		public class Wire
		{
			public int AcisIndex { get; set; }

			public bool ApplyTransformPresent { get; internal set; }

			public Color Color { get; set; }

			public bool HasRotation { get; internal set; }

			public bool HasShear { get; internal set; }

			public List<XYZ> Points { get; } = new List<XYZ>();

			public double Scale { get; internal set; }

			public int SelectionMarker { get; set; }

			public XYZ Translation { get; internal set; }

			public byte Type { get; set; }

			public XYZ XAxis { get; internal set; }

			public XYZ YAxis { get; internal set; }

			public XYZ ZAxis { get; internal set; }

			internal bool HasReflection;
		}
	}
}