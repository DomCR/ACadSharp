using ACadSharp.IO.DWG.Tangent;
using CSMath;
using System;
using System.Collections.Generic;

namespace ACadSharp.Entities.Tangent
{
	/// <summary>
	/// A roof drawn against a wall, as Tianzheng stores one.
	/// </summary>
	/// <remarks>
	/// <para>
	/// The class stores the dimensions of a rectangle, an angle, and two heights, and works the
	/// outline out when the object is drawn. So the conversion works it out too, following the same
	/// steps in the same order, which is the only way the result agrees with what the drawing shows.
	/// </para>
	/// <para>
	/// Two lengths are derived rather than stored, and both matter. The pitch follows from the
	/// height of the roof and the distance it spans; the length along the slope follows from the
	/// pitch and the step, and overrides the stored length whenever the roof rises at all. An object
	/// whose stored length is zero — every one in the sample — still draws at its full slope.
	/// </para>
	/// <para>
	/// The fields are read in the order the class reads them, which is not the order its members sit
	/// in: the step comes before the perpendicular distance, and the roof height before the wall
	/// height. Reading them in member order gives eight numbers that are individually plausible and
	/// together wrong.
	/// </para>
	/// </remarks>
	public sealed class TchMountRoof : TchEntity
	{
		/// <summary>
		/// The smallest difference that counts as one.
		/// </summary>
		private const double Tolerance = 1e-9;

		/// <summary>
		/// The pitch a roof of no span falls back to, in radians.
		/// </summary>
		private const double DefaultPitch = Math.PI / 6.0;

		/// <summary>
		/// Reads a roof from the bytes a class's own fields were written to.
		/// </summary>
		/// <param name="dxfClass">The class the object belongs to.</param>
		/// <param name="payload">The object's bytes.</param>
		/// <param name="version">The version of the file they came from.</param>
		public static TchMountRoof Parse(ACadSharp.Classes.DxfClass dxfClass, byte[] payload, ACadVersion version)
		{
			TchMountRoof roof = new TchMountRoof(dxfClass);

			roof.StringStreamOffset = FindStringStream(payload, payload.Length * 8L);
			roof.Fill(payload, version);

			return roof;
		}

		/// <summary>
		/// Initialises a roof from the class it was read as.
		/// </summary>
		internal TchMountRoof(ACadSharp.Classes.DxfClass dxfClass)
			: base(dxfClass)
		{
		}

		/// <summary>
		/// Gets where the roof meets the wall.
		/// </summary>
		/// <remarks>
		/// The one field the class reads inline rather than from the string stream, which is why the
		/// stream has to be found before anything else can be read.
		/// </remarks>
		public XYZ Position { get; private set; }

		/// <summary>
		/// Gets the angle the rectangle is turned to, in radians.
		/// </summary>
		/// <remarks>
		/// Any real value, not one reduced to a turn: the angles a drawing stores accumulate, so a
		/// value in the hundreds of thousands is ordinary and only the direction it names matters.
		/// </remarks>
		public double Angle { get; private set; }

		/// <summary>
		/// Gets the distance the roof runs along the wall.
		/// </summary>
		public double AxialLength { get; private set; }

		/// <summary>
		/// Gets the distance the roof spans away from the wall.
		/// </summary>
		public double Span { get; private set; }

		/// <summary>
		/// Gets the distance from the wall to the far edge of the roof.
		/// </summary>
		public double EdgeDistance { get; private set; }

		/// <summary>
		/// Gets the thickness the roof is stepped by, in radians as the object stores it.
		/// </summary>
		public double Step { get; private set; }

		/// <summary>
		/// Gets the height of the roof above the wall.
		/// </summary>
		public double Height { get; private set; }

		/// <summary>
		/// Gets the height of the wall the roof sits on.
		/// </summary>
		public double WallHeight { get; private set; }

		/// <summary>
		/// Gets the length the object stores, which the pitch usually overrides.
		/// </summary>
		public double StoredLength { get; private set; }

		/// <summary>
		/// Gets the pitch of the roof, in radians.
		/// </summary>
		public double Pitch => this.Span > Tolerance
			? Math.Atan(2.0 * this.Height / this.Span)
			: DefaultPitch;

		/// <summary>
		/// Gets the length the roof runs up the slope.
		/// </summary>
		/// <remarks>
		/// The stored length is the answer only when the two heights are equal, or when the step is
		/// too shallow to divide by. Otherwise the length follows from the pitch and the step, and the
		/// stored value is not used at all — which is what the object's own check comes to.
		/// </remarks>
		public double SlopeLength
		{
			get
			{
				if (this.Height - this.WallHeight >= Tolerance)
				{
					double step = Math.Tan(this.Step);

					if (Math.Abs(step) > Tolerance)
					{
						return this.EdgeDistance * Math.Tan(this.Pitch) / step;
					}
				}

				return this.StoredLength;
			}
		}

		/// <inheritdoc/>
		public override void ApplyTransform(Transform transform)
		{
			//The outline is worked out from the position and the angle rather than kept, so moving it
			//would mean recomputing the stored fields, which is not what a transform asks for.
		}

		/// <inheritdoc/>
		public override BoundingBox GetBoundingBox()
		{
			BoundingBox box = BoundingBox.Null;

			foreach (Entity entity in this.ToStandardEntities())
			{
				box = box.Merge(entity.GetBoundingBox());
			}

			return box;
		}

		/// <inheritdoc/>
		public override IEnumerable<Entity> ToStandardEntities()
		{
			//Four corners, each reached by moving from the last: back along the slope, out to the far
			//edge, along the wall, and back across. The distances that are not stored are the ones the
			//slope length and the edge distance add to.
			double length = this.SlopeLength;
			double along = this.AxialLength + (2.0 * length);

			XY point = new XY(this.Position.X, this.Position.Y);
			point = polar(point, this.Angle + Math.PI, length);
			point = polar(point, this.Angle - (Math.PI / 2.0), this.EdgeDistance);

			LwPolyline outline = new LwPolyline();

			outline.Vertices.Add(new LwPolyline.Vertex(point));

			point = polar(point, this.Angle, along);
			outline.Vertices.Add(new LwPolyline.Vertex(point));

			point = polar(point, this.Angle + (Math.PI / 2.0), this.Span + (2.0 * this.EdgeDistance));
			outline.Vertices.Add(new LwPolyline.Vertex(point));

			point = polar(point, this.Angle + Math.PI, along);
			outline.Vertices.Add(new LwPolyline.Vertex(point));

			outline.IsClosed = true;

			yield return outline;
		}

		/// <inheritdoc/>
		internal override void Fill(byte[] payload, ACadVersion version)
		{
			if (payload == null || this.StringStreamOffset < 0)
			{
				return;
			}

			TchStreamReader reader = new TchStreamReader(payload, this.StringStreamOffset);

			//What comes before the class's own fields was written by a reader above it in the chain.
			//Stepping over it is what lets the fields be found without knowing a single offset.
			for (int i = 0; i < TchFieldMap.MountRoofSkippedStrings; i++)
			{
				reader = TchFieldMap.SkipString(reader);
			}

			this.Position = TchFieldMap.ReadInlinePosition(payload);

			this.Angle = TchFieldMap.ReadDouble(reader, TgDataEncrypt.ObjectXor);
			this.AxialLength = TchFieldMap.ReadDouble(reader, TgDataEncrypt.ObjectXor);
			this.Span = TchFieldMap.ReadDouble(reader, TgDataEncrypt.ObjectXor);
			this.Step = TchFieldMap.ReadDouble(reader, TgDataEncrypt.ObjectXor);
			this.EdgeDistance = TchFieldMap.ReadDouble(reader, TgDataEncrypt.ObjectXor);
			this.StoredLength = TchFieldMap.ReadDouble(reader, TgDataEncrypt.ObjectXor);
			this.Height = TchFieldMap.ReadDouble(reader, TgDataEncrypt.ObjectXor);
			this.WallHeight = TchFieldMap.ReadDouble(reader, TgDataEncrypt.ObjectXor);
		}

		private static XY polar(XY from, double angle, double distance)
		{
			return new XY(
				from.X + (distance * Math.Cos(angle)),
				from.Y + (distance * Math.Sin(angle)));
		}
	}
}
