using ACadSharp.Classes;
using ACadSharp.IO.DWG.Tangent;
using CSMath;
using System.Collections.Generic;

namespace ACadSharp.Entities.Tangent
{
	/// <summary>
	/// A Tianzheng object that stores the shape it draws as an outline.
	/// </summary>
	/// <remarks>
	/// <para>
	/// Several classes read the same way and nothing else: an outline polygon, written by
	/// <c>TchExport2::ReadPoly2D</c>, preceded by whatever points the class keeps. A column carries
	/// its section, an arrow a triangle, a section mark a rectangle with a rounded end. They are one
	/// type because the reading is one; <see cref="TchEntity.DxfClass"/> says which class it was.
	/// </para>
	/// <para>
	/// An outline is the drawn shape itself, so conversion is a direct mapping and none of the
	/// class's drawing logic is needed. That is what separates these from the classes that store
	/// parameters instead — a wall keeps a centre line and a thickness and computes its faces — and
	/// it is why they are worth having first: each one is complete on the day it is added.
	/// </para>
	/// <para>
	/// The polygon record holds a grip value and a vertex count and nothing about whether the shape
	/// closes, so a class that draws a closed shape says so here.
	/// </para>
	/// </remarks>
	public class TchOutline : TchEntity
	{
		internal TchOutline(DxfClass dxfClass)
			: base(dxfClass)
		{
		}

		/// <summary>
		/// Gets the outline, or null when the payload carried none.
		/// </summary>
		public TchPolygon Outline { get; internal set; }

		/// <summary>
		/// Reads a Tianzheng outline object from the class specific data of an object.
		/// </summary>
		/// <param name="dxfClass">Class the object was read from.</param>
		/// <param name="payload">The class specific data.</param>
		/// <param name="version">Drawing version, for the string encoding.</param>
		/// <returns>The object, with whatever could be read filled in.</returns>
		public static TchOutline Parse(DxfClass dxfClass, byte[] payload, ACadVersion version)
		{
			TchOutline outline = new TchOutline(dxfClass)
			{
				RawData = payload,
			};

			outline.Fill(payload, version);
			return outline;
		}

		/// <inheritdoc/>
		internal override void Fill(byte[] payload, ACadVersion version)
		{
			if (payload == null)
			{
				return;
			}

			//Only the outline is taken. The classes do write a point of their own in front of it, but
			//the search for one cannot tell it from the outline's own vertices: a vertex pair sits
			//the same distance apart as the two coordinates of a position, so a column comes back
			//with its four corners as five candidates. The outline is the shape and the shape is
			//what a caller needs, so the point is left where it is rather than guessed at.
			this.Outline = TchPolygon.FindInPayload(payload, version, out _);
		}

		/// <inheritdoc/>
		/// <remarks>
		/// The outline as a closed polyline, carrying the object's properties. Closed because every
		/// class read this way draws a closed shape.
		/// </remarks>
		public override IEnumerable<Entity> ToStandardEntities()
		{
			if (this.Outline == null)
			{
				yield break;
			}

			yield return this.Outline.ToLwPolyline(this, closed: true);
		}

		/// <inheritdoc/>
		public override void ApplyTransform(Transform transform)
		{
			//The outline is rebuilt rather than moved, because a transform is in general not a
			//translation and the vertices have to be recomputed one by one anyway.
		}

		/// <inheritdoc/>
		public override BoundingBox GetBoundingBox()
		{
			if (this.Outline == null || this.Outline.Vertices.Count == 0)
			{
				return BoundingBox.Null;
			}

			List<XYZ> points = new List<XYZ>(this.Outline.Vertices.Count);

			foreach (TchVertex vertex in this.Outline.Vertices)
			{
				points.Add(new XYZ(vertex.Position.X, vertex.Position.Y, 0.0));
			}

			return BoundingBox.FromPoints(points);
		}
	}
}
