using ACadSharp.Attributes;
using ACadSharp.Entities;
using CSMath;
using System;
using System.Collections.Generic;

namespace ACadSharp.Entities
{
public abstract partial class ModelerGeometry
	{

		[DxfCodeValue(2)]
		internal Guid Guid { get; set; }
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