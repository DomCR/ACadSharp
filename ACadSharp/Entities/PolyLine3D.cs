﻿using ACadSharp.Attributes;
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

		/// <inheritdoc/>
		public override string SubclassMarker => DxfSubclassMarker.Polyline3d;

		public Polyline3D() : base()
		{
			this.Vertices.OnAdd += this.verticesOnAdd;
		}

		public override IEnumerable<Entity> Explode()
		{
			return Polyline.Explode(this);
		}

		private void verticesOnAdd(object sender, CollectionChangedEventArgs e)
		{
			if (e.Item is not Vertex3D)
			{
				this.Vertices.Remove((Vertex)e.Item);
				throw new ArgumentException($"Wrong vertex type for {DxfSubclassMarker.Polyline3d}");
			}
			else if (e.Item is Vertex3D v && v.Bulge != 0)
			{
				throw new ArgumentException($"Bulge value cannot be different than 0 for a Vertex3D in a 3D Polyline");
			}
		}
	}
}
