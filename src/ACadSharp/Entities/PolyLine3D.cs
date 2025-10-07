using ACadSharp.Attributes;
using CSMath;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ACadSharp.Entities
{
	/// <summary>
	/// Represents a <see cref="Polyline3D"/> entity.
	/// </summary>
	/// <remarks>
	/// Object name <see cref="DxfFileToken.EntityPolyline"/> <br/>
	/// Dxf class name <see cref="DxfSubclassMarker.Polyline"/>
	/// </remarks>
	[DxfName(DxfFileToken.EntityPolyline)]
	[DxfSubClass(DxfSubclassMarker.Polyline3d)]
	public class Polyline3D : Polyline<Vertex3D>
	{
		/// <inheritdoc/>
		public override ObjectType ObjectType => ObjectType.POLYLINE_3D;

		/// <inheritdoc/>
		public override string SubclassMarker => DxfSubclassMarker.Polyline3d;

		public Polyline3D() : base()
		{
		}

		public Polyline3D(IEnumerable<XYZ> vertices, bool isClosed = false) : base(vertices.Select(v => new Vertex3D(v)), isClosed)
		{
		}

		public Polyline3D(IEnumerable<Vertex3D> vertices, bool isClosed = false) : base(vertices, isClosed)
		{
		}

		public Polyline3D(params IEnumerable<XYZ> vertices) : base(vertices.Select(v => new Vertex3D(v)), false)
		{
		}
	}
}
