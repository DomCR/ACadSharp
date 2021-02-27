using System;

namespace ACadSharp.Entities
{
	/// <summary>
	/// Defines the boundary path type of the hatch.
	/// </summary>
	/// <remarks>Bit flag.</remarks>
	[Flags]
	public enum BoundaryPathFlags
	{
		/// <summary>
		/// Default.
		/// </summary>
		Default = 0,
		/// <summary>
		/// External.
		/// </summary>
		External = 1,
		/// <summary>
		/// Polyline.
		/// </summary>
		Polyline = 2,
		/// <summary>
		/// Derived.
		/// </summary>
		Derived = 4,
		/// <summary>
		/// Text box.
		/// </summary>
		Textbox = 8,
		/// <summary>
		/// Outermost.
		/// </summary>
		Outermost = 16,
		/// <summary>
		/// Loop is not closed.
		/// </summary>
		NotClosed = 32, 
		/// <summary>
		/// Self-intersecting loop.
		/// </summary>
		SelfIntersecting = 64, 
		/// <summary>
		/// Text loop surrounded by an even number of loops.
		/// </summary>
		TextIsland = 128, 
		/// <summary>
		/// Duplicate loop.
		/// </summary>
		Duplicate = 256,
		/// <summary>
		/// The bounding area is an annotative block.
		/// </summary>
		IsAnnotative = 512, 
		/// <summary>
		/// The bounding type does not support scaling.
		/// </summary>
		DoesNotSupportScale = 1024, 
		/// <summary>
		/// Forces all annotatives to be visible.
		/// </summary>
		ForceAnnoAllVisible = 2048, 
		/// <summary>
		/// Orients hatch loop to paper.
		/// </summary>
		OrientToPaper = 4096, 
		/// <summary>
		/// Describes if the hatch is an annotative block.
		/// </summary>
		IsAnnotativeBlock = 8192, 
	}
}
