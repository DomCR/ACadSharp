﻿using CSMath;
using System.Collections.Generic;

namespace ACadSharp.Entities
{
	public interface IPolyline : IEntity
	{
		/// <summary>
		/// Indicates that the polyline is closed
		/// </summary>
		bool IsClosed { get; set; }

		IEnumerable<IVertex> Vertices { get; }

		/// <summary>
		/// The current elevation of the object.
		/// </summary>
		double Elevation { get; set; }

		/// <summary>
		/// Specifies the three-dimensional normal unit vector for the object.
		/// </summary>
		XYZ Normal { get; set; }

		/// <summary>
		/// Specifies the distance a 2D object is extruded above or below its elevation.
		/// </summary>
		double Thickness { get; set; }

		IEnumerable<Entity> Explode();
	}
}
