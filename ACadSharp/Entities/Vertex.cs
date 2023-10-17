﻿using ACadSharp.Attributes;
using CSMath;

namespace ACadSharp.Entities
{
	/// <summary>
	/// Represents a base type for Vertex entities
	/// </summary>
	[DxfSubClass(DxfSubclassMarker.Vertex, true)]
	public abstract class Vertex : Entity, IVertex
	{
		/// <inheritdoc/>
		public override string ObjectName => DxfFileToken.EntityVertex;

		/// <summary>
		/// Location point (in OCS when 2D, and WCS when 3D)
		/// </summary>
		[DxfCodeValue(10, 20, 30)]
		public XYZ Location { get; set; } = XYZ.Zero;

		/// <summary>
		/// Starting width
		/// </summary>
		[DxfCodeValue(DxfReferenceType.Optional, 40)]
		public double StartWidth { get; set; } = 0.0;

		/// <summary>
		/// Ending width
		/// </summary>
		[DxfCodeValue(DxfReferenceType.Optional, 41)]
		public double EndWidth { get; set; } = 0.0;

		/// <inheritdoc/>
		[DxfCodeValue(DxfReferenceType.Optional, 42)]
		public double Bulge { get; set; } = 0.0;

		/// <summary>
		/// Vertex flags
		/// </summary>
		[DxfCodeValue(70)]
		public VertexFlags Flags { get; set; }

		/// <summary>
		/// Curve fit tangent direction
		/// </summary>
		[DxfCodeValue(DxfReferenceType.IsAngle, 50)]
		public double CurveTangent { get; set; }

		/// <summary>
		/// Vertex identifier
		/// </summary>
		[DxfCodeValue(DxfReferenceType.Ignored, 91)]	//TODO: for some versions this code is invalid
		public int Id { get; set; }

		IVector IVertex.Location { get { return this.Location; } }

		public Vertex() : base() { }
	}
}
