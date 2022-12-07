using ACadSharp.Attributes;
using CSMath;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Numerics;

namespace ACadSharp.Entities
{
	/// <summary>
	/// Represents a <see cref="Polyline"/> entity
	/// </summary>
	[DxfName(DxfFileToken.EntityPolyline)]
	[DxfSubClass(null, true)]
	public abstract class Polyline : Entity
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

		public Polyline() : base()
		{
			this.Vertices = new SeqendCollection<Vertex>(this);
		}
	}

	public interface IPolyline : IEntity
	{
		bool IsClosed { get; }

		IEnumerable<IVertex> Vertices { get; }

		double Elevation { get; }

		XYZ Normal { get; }

		double Thickness { get; }

		IEnumerable<Entity> Explode()
		{
			List<Entity> entities = new List<Entity>();

			for (int i = 0; i < Vertices.Count(); i++)
			{
				IVertex curr = this.Vertices.ElementAt(i);
				IVertex next = this.Vertices.ElementAtOrDefault(i + 1);

				if (next == null && this.IsClosed)
				{
					next = Vertices.First();
				}
				else if (next == null)
				{
					break;
				}

				Entity e = null;
				if (curr.Bulge == 0)
				{
					//Is a line
					e = new Line
					{
						StartPoint = new XYZ(curr.Location.GetComponents()),
						EndPoint = new XYZ(next.Location.GetComponents())
					};
				}
				else
				{
					XY p1 = new XY(curr.Location.GetComponents());
					XY p2 = new XY(next.Location.GetComponents());

					XY center = MathUtils.GetCenter(p1, p2, curr.Bulge, out double r);

					double startAngle;
					double endAngle;
					if (curr.Bulge > 0)
					{
						startAngle = p2.Substract(center).GetAngle();
						endAngle = p1.Substract(center).GetAngle();
					}
					else
					{
						startAngle = p1.Substract(center).GetAngle();
						endAngle = p2.Substract(center).GetAngle();
					}

					//Is an arc
					e = new Arc
					{
						Center = new XYZ(center.X, center.Y, this.Elevation),
						Normal = this.Normal,
						Radius = r,
						StartAngle = startAngle,
						EndAngle = endAngle,
						Thickness = this.Thickness,
					};
				}

				this.MatchProperties(e);

				entities.Add(e);
			}

			return entities;
		}
	}

	public interface IVertex
	{
		IVector Location { get; }

		double Bulge { get; }
	}
}
