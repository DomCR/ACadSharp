using ACadSharp.Attributes;
using System;
using System.Collections.Generic;

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
	public class Polyline3D : Polyline
	{
		/// <inheritdoc/>
		public override ObjectType ObjectType => ObjectType.POLYLINE_3D;

		public Polyline3D() : base()
		{
			this.Vertices.OnAdd += this.verticesOnAdd;
		}

#if NET48
		public override IEnumerable<Entity> Explode()
		{
			throw new NotImplementedException();
		}
#endif

		private void verticesOnAdd(object sender, ReferenceChangedEventArgs e)
		{
			if (e.Current is not Vertex3D)
			{
				this.Vertices.Remove((Vertex)e.Current);
				throw new ArgumentException($"Wrong vertex type for {DxfSubclassMarker.Polyline3d}");
			}
		}
	}
}
