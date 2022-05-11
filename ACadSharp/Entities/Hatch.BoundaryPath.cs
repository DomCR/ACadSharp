using ACadSharp.Attributes;
using CSMath;
using System.Collections.Generic;

namespace ACadSharp.Entities
{
	public partial class Hatch
	{
		public partial class BoundaryPath
		{
			/// <summary>
			/// Boundary path type flag
			/// </summary>
			[DxfCodeValue(92)]
			public BoundaryPathFlags Flags { get; set; }

			/// <summary>
			/// Number of edges in this boundary path
			/// </summary>
			/// <remarks>
			/// only if boundary is not a polyline
			/// </remarks>
			[DxfCodeValue(DxfReferenceType.Count, 93)]
			public List<Edge> Edges { get; set; } = new List<Edge>();

			/// <summary>
			/// Source boundary objects
			/// </summary>
			[DxfCodeValue(DxfReferenceType.Count, 97)]
			public List<Entity> Entities { get; set; } = new List<Entity>();
		}
	}
}
