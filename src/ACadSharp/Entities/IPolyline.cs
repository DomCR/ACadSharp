﻿using CSMath;
using System.Collections.Generic;

namespace ACadSharp.Entities
{
	public interface IPolyline : IEntity
	{
		/// <summary>
		/// Indicates that the polyline is closed.
		/// </summary>
		bool IsClosed { get; set; }

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

		/// <summary>
		/// Explodes the polyline into a collection of entities formed by <see cref="Line"/> and <see cref="Arc"/>.
		/// </summary>
		/// <returns></returns>
		IEnumerable<Entity> Explode();

		/// <summary>
		/// Vertices that form the Polyline.
		/// </summary>
		IEnumerable<IVertex> Vertices { get; }
	}
}
