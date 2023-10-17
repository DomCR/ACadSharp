﻿using ACadSharp.Attributes;
using System;
using System.Collections.Generic;

namespace ACadSharp.Entities
{
	/// <summary>
	/// Represents a <see cref="Polyline2D"/> entity.
	/// </summary>
	/// <remarks>
	/// Object name <see cref="DxfFileToken.EntityPolyline"/> <br/>
	/// Dxf class name <see cref="DxfSubclassMarker.Polyline"/>
	/// </remarks>
	[DxfName(DxfFileToken.EntityPolyline)]
	[DxfSubClass(DxfSubclassMarker.Polyline)]
	public class Polyline2D : Polyline
	{
		/// <inheritdoc/>
		public override ObjectType ObjectType => ObjectType.POLYLINE_2D;

		/// <inheritdoc/>
		public override string SubclassMarker => DxfSubclassMarker.Polyline;

		public Polyline2D() : base()
		{
			this.Vertices.OnAdd += this.verticesOnAdd;
		}

		public override IEnumerable<Entity> Explode()
		{
			return Polyline.Explode(this);
		}

		private void verticesOnAdd(object sender, CollectionChangedEventArgs e)
		{
			if (e.Item is not Vertex2D)
			{
				this.Vertices.Remove((Vertex)e.Item);
				throw new ArgumentException($"Wrong vertex type for {DxfSubclassMarker.Polyline}");
			}
		}
	}
}
