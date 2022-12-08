using ACadSharp.Attributes;
using CSMath;
using System.Collections.Generic;

namespace ACadSharp.Entities
{
	/// <summary>
	/// Represents a <see cref="Polyline"/> entity
	/// </summary>
	[DxfName(DxfFileToken.EntityPolyline)]
	[DxfSubClass(null, true)]
	public abstract class Polyline : Entity, IPolyline
	{
		/// <inheritdoc/>
		public override string ObjectName => DxfFileToken.EntityPolyline;

		/// <summary>
		/// The current elevation of the object.
		/// </summary>
		[DxfCodeValue(30)]
		public double Elevation { get; set; } = 0.0;

		/// <summary>
		/// Specifies the distance a 2D AutoCAD object is extruded above or below its elevation.
		/// </summary>
		[DxfCodeValue(39)]
		public double Thickness { get; set; } = 0.0;

		/// <summary>
		/// Specifies the three-dimensional normal unit vector for the object.
		/// </summary>
		[DxfCodeValue(210, 220, 230)]
		public XYZ Normal { get; set; } = XYZ.AxisZ;

		/// <summary>
		/// Polyline flags
		/// </summary>
		[DxfCodeValue(70)]
		public PolylineFlags Flags { get; set; }

		/// <summary>
		/// Start width
		/// </summary>
		[DxfCodeValue(40)]
		public double StartWidth { get; set; } = 0.0;

		/// <summary>
		/// End width
		/// </summary>
		[DxfCodeValue(41)]
		public double EndWidth { get; set; } = 0.0;

		//71	Polygon mesh M vertex count(optional; default = 0)
		//72	Polygon mesh N vertex count(optional; default = 0)
		//73	Smooth surface M density(optional; default = 0)
		//74	Smooth surface N density(optional; default = 0)

		/// <summary>
		/// Curves and smooth surface type
		/// </summary>
		[DxfCodeValue(75)]
		public SmoothSurfaceType SmoothSurface { get; set; }

		/// <summary>
		/// Vertices that form this polyline.
		/// </summary>
		/// <remarks>
		/// Each <see cref="Vertex"/> has it's own unique handle.
		/// </remarks>
		public SeqendCollection<Vertex> Vertices { get; }

		IEnumerable<IVertex> IPolyline.Vertices { get { return this.Vertices; } }

		public Polyline() : base()
		{
			this.Vertices = new SeqendCollection<Vertex>(this);
		}
	}
}
