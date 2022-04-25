using ACadSharp.Attributes;
using CSMath;
using System.Collections.Generic;

namespace ACadSharp.Entities
{
	public partial class Hatch
	{
		public partial class BoundaryPath
		{
			public BoundaryPathFlags Flags { get; set; }

			/// <summary>
			/// Number of edges in this boundary path
			/// </summary>
			/// <remarks>
			/// only if boundary is not a polyline
			/// </remarks>
			[DxfCodeValue(93)]
			public List<Edge> Edges { get; set; } = new List<Edge>();

			/// <summary>
			/// 
			/// </summary>
			public List<Entity> Entities { get; set; } = new List<Entity>();
		}
	}
}
