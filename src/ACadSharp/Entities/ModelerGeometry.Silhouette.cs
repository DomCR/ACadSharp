using CSMath;
using System.Collections.Generic;

namespace ACadSharp.Entities
{
public abstract partial class ModelerGeometry
	{
		public class Silhouette
		{
			public XYZ ViewportDirectionFromTarget { get; internal set; }

			public long ViewportId { get; internal set; }

			public bool ViewportPerspective { get; internal set; }

			public XYZ ViewportTarget { get; internal set; }

			public XYZ ViewportUpDirection { get; internal set; }

			public List<Wire> Wires { get; } = new();
		}
	}
}