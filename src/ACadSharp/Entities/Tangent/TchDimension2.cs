using ACadSharp.Classes;
using ACadSharp.IO.DWG.Tangent;
using CSMath;
using System;
using System.Collections.Generic;

namespace ACadSharp.Entities.Tangent
{
	/// <summary>
	/// A run of dimensions, as Tianzheng stores one.
	/// </summary>
	/// <remarks>
	/// <para>
	/// One object is not one dimension. The class records where a run of measured points starts, the
	/// direction the run advances, and the distance from each point to the next, and the drawing
	/// shows one dimension per distance. The sample's five-measurement object becomes five
	/// <see cref="DimensionAligned"/> entities, and the object that only carries the total of three
	/// becomes one. So the count of what a drawing contains cannot be read off the count of these
	/// objects, and neither can the reverse.
	/// </para>
	/// <para>
	/// The distances are the field that matters and the one that is least obvious: they are written
	/// as text into the object's string stream, like every other number a Tianzheng class writes, so
	/// they are not in the part of the object a reader searching for doubles would look at.
	/// </para>
	/// <para>
	/// A dimension is associative — it refers to the geometry it measures, and can be recomputed
	/// when that geometry moves. Only the object's own data is used here, and for the angular form
	/// that is enough: it was refused at first on the grounds that the angle's vertex was not among
	/// its fields, and that was wrong. The object records a point on the dimension arc, the
	/// direction the arc is swept from and the arc's radius, and the vertex is the first of those set
	/// back by the third.
	/// </para>
	/// </remarks>
	public sealed class TchDimension2 : TchEntity
	{
		/// <summary>
		/// Where the run's first measured point begins, as a bit offset into the payload.
		/// </summary>
		/// <remarks>
		/// Later than the position of every other class read so far, because this one's chain writes
		/// more before it: a plain double and two lengths sit between the tag and the point.
		/// </remarks>
		public const long AnchorOffset = 110L;

		/// <summary>
		/// Which bit of the object's flags says it measures an angle rather than a distance.
		/// </summary>
		/// <remarks>
		/// Read from the class's own <c>IsAngular</c>, which tests this bit and nothing else. The one
		/// angular object in the sample carries 144, whose bit this is, and the five runs carry nought.
		/// </remarks>
		public const int AngularFlag = 0x10;

		/// <summary>
		/// How far along its arc the angular form puts the point that places the dimension line.
		/// </summary>
		/// <remarks>
		/// A third. Not a preference: the export's own definition point sits at a third of the swept
		/// angle to thirteen places, and the class's text placement asks its curve segment for the
		/// point at a fraction of the segment's length.
		/// </remarks>
		public const double DimensionArcFraction = 1.0 / 3.0;

		/// <summary>
		/// The most measurements a single object is taken to carry.
		/// </summary>
		/// <remarks>
		/// A guard rather than a fact about the format: the count is read from the object, and one
		/// that reads wrongly would otherwise allocate and emit an arbitrary number of entities. No
		/// object in the sample carries more than five.
		/// </remarks>
		public const int MaxSegments = 64;

		private readonly List<double> _segments = new List<double>();

		/// <summary>
		/// Initialises a dimension run from the class it was read as.
		/// </summary>
		internal TchDimension2(DxfClass dxfClass)
			: base(dxfClass)
		{
		}

		/// <summary>
		/// Gets where the run's first measured point sits.
		/// </summary>
		public XYZ Anchor { get; private set; }

		/// <summary>
		/// Gets the direction the run advances, in radians.
		/// </summary>
		/// <remarks>
		/// Not reduced to a turn: the sample holds both nought and a full turn for runs that are
		/// both horizontal, so the value names a direction and not a angle in any particular range.
		/// </remarks>
		public double Direction { get; private set; }

		/// <summary>
		/// Gets the signed distance from the measured line to the dimension line.
		/// </summary>
		/// <remarks>
		/// Positive is to the left of the direction the run advances, which is the drawing's own
		/// convention and the reason the sign is kept rather than the magnitude.
		/// </remarks>
		public double DimensionLineOffset { get; private set; }

		/// <summary>
		/// Gets the distances between the run's measured points, in order.
		/// </summary>
		/// <remarks>
		/// For the angular form this holds the one angle it measures, in radians, and it is the only
		/// entry: an angle is not a run.
		/// </remarks>
		public IReadOnlyList<double> Segments => _segments;

		/// <summary>
		/// Gets a value indicating whether the object measures an angle rather than a run of lengths.
		/// </summary>
		public bool IsAngular { get; private set; }

		/// <summary>
		/// Gets the radius of the arc the angular form draws its dimension line on.
		/// </summary>
		public double DimensionArcRadius { get; private set; }

		/// <summary>
		/// Gets the angle the angular form measures, in radians.
		/// </summary>
		public double Angle => _segments.Count == 1 ? _segments[0] : double.NaN;

		/// <summary>
		/// Gets a value indicating whether the object's own data describes a dimension this can convert.
		/// </summary>
		public bool IsConvertible { get; private set; }

		/// <summary>
		/// Gets why <see cref="IsConvertible"/> is false, or null when it is true.
		/// </summary>
		/// <remarks>
		/// Kept as text so a caller converting a drawing can report what it could not convert and
		/// why. A conversion that quietly drops an object is indistinguishable from one that had
		/// nothing to drop.
		/// </remarks>
		public string Reason { get; private set; }

		/// <summary>
		/// Reads a dimension run from the bytes a class's own fields were written to.
		/// </summary>
		public static TchDimension2 Parse(DxfClass dxfClass, byte[] payload, ACadVersion version)
		{
			TchDimension2 dimension = new TchDimension2(dxfClass);

			dimension.StringStreamOffset = FindStringStream(payload, payload.Length * 8L);
			dimension.Fill(payload, version);

			return dimension;
		}

		/// <inheritdoc/>
		public override IEnumerable<Entity> ToStandardEntities()
		{
			if (!this.IsConvertible)
			{
				yield break;
			}

			if (this.IsAngular)
			{
				yield return CopyCommonProperties(this, this.toAngularDimension());
				yield break;
			}

			double cos = Math.Cos(this.Direction);
			double sin = Math.Sin(this.Direction);

			XY from = new XY(this.Anchor.X, this.Anchor.Y);

			foreach (double length in _segments)
			{
				XY to = new XY(from.X + (length * cos), from.Y + (length * sin));

				//The dimension line runs alongside the measured one, so the point that defines it is
				//the second measured point moved out by the offset, across the direction of the run.
				//That is where the flattened export puts it, to the last place, and it is the whole of
				//what the offset decides.
				yield return CopyCommonProperties(this, new DimensionAligned
				{
					FirstPoint = new XYZ(from.X, from.Y, this.Anchor.Z),
					SecondPoint = new XYZ(to.X, to.Y, this.Anchor.Z),
					DefinitionPoint = new XYZ(
						to.X - (this.DimensionLineOffset * sin),
						to.Y + (this.DimensionLineOffset * cos),
						this.Anchor.Z),
				});

				from = to;
			}
		}

		/// <inheritdoc/>
		public override void ApplyTransform(Transform transform)
		{
			//The run is worked out from the anchor and the direction rather than kept, so moving it
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
		internal override void Fill(byte[] payload, ACadVersion version)
		{
			if (payload == null || this.StringStreamOffset < 0)
			{
				this.Reason = "the object has no string stream, so its distances are not readable";
				return;
			}

			this.Anchor = TchFieldMap.ReadInlinePosition(payload, AnchorOffset);

			double scale = TchFieldMap.ReadDouble(
				TchFieldMap.SkipString(new TchStreamReader(payload, this.StringStreamOffset)),
				TgDataEncrypt.ObjectXor);

			if (double.IsNaN(scale) || scale <= 0.0)
			{
				this.Reason = "the object records no drawing scale, so its distances are not lengths";
				return;
			}

			TchStreamReader reader = new TchStreamReader(payload, this.StringStreamOffset);

			//What comes before the class's own fields was written by a reader above it in the chain.
			for (int i = 0; i < TchFieldMap.DimensionSkippedStrings; i++)
			{
				reader = TchFieldMap.SkipString(reader);
			}

			this.Direction = TchFieldMap.ReadDouble(reader, TgDataEncrypt.ObjectXor);

			//The distance from the measured line to the dimension line is written in the drawing's
			//own scale, so it is the scale that turns it into a length. The object also writes it a
			//second time inline, and for a run of lengths the two agree — but not for the angular
			//form, whose inline copy is left unscaled, so the value read here is the one used.
			this.DimensionLineOffset = TchFieldMap.ReadDouble(reader, TgDataEncrypt.ObjectXor) * scale;

			double count = TchFieldMap.ReadDouble(reader, TgDataEncrypt.ObjectXor);

			if (double.IsNaN(count) || count < 1.0 || count > MaxSegments || count != Math.Floor(count))
			{
				this.Reason = $"the object records {count} measurements, which is not a dimension";
				return;
			}

			for (int i = 0; i < (int)count; i++)
			{
				double length = TchFieldMap.ReadDouble(reader, TgDataEncrypt.ObjectXor);

				if (double.IsNaN(length))
				{
					this.Reason = $"measurement {i} of {count} does not read as a number";
					_segments.Clear();
					return;
				}

				_segments.Add(length);
			}

			//What follows the measurements is the object's flags, and the bit that says whether it
			//measures an angle is in them. An earlier reading of this class took the flags for a
			//terminator and refused anything but zero, which refused the angular form on the grounds
			//that its vertex was not among the object's fields. It is: the class's own reader derives
			//it from the anchor, the direction and the two radii that follow the flags.
			double flags = TchFieldMap.ReadDouble(reader, TgDataEncrypt.ObjectXor);

			if (double.IsNaN(flags))
			{
				this.Reason = "the object's flags do not read, so what it measures is not established";
				_segments.Clear();
				return;
			}

			this.IsAngular = (((int)flags) & AngularFlag) != 0;

			if (!this.IsAngular)
			{
				this.IsConvertible = true;
				return;
			}

			this.DimensionArcRadius = TchFieldMap.ReadDouble(reader, TgDataEncrypt.ObjectXor);

			if (double.IsNaN(this.DimensionArcRadius) || this.DimensionArcRadius <= 0.0)
			{
				this.Reason = "the object records no radius for its dimension arc";
				_segments.Clear();
				return;
			}

			if (_segments.Count != 1)
			{
				this.Reason = $"an angular dimension measures one angle, and this one records {_segments.Count}";
				_segments.Clear();
				return;
			}

			this.IsConvertible = true;
		}

		/// <summary>
		/// Builds the angular dimension this object stands for.
		/// </summary>
		/// <remarks>
		/// The vertex is not stored, and it does not have to be: the object records a point on the
		/// dimension arc and the direction the arc is swept from, and the vertex is that point set
		/// back by the arc's radius along that direction. The sample's vertex comes out of it to
		/// thirteen places, as do both ends of the arc and the point that places the dimension line.
		/// </remarks>
		private DimensionAngular3Pt toAngularDimension()
		{
			//The arc is swept about a direction square to the one the object names, and the anchor is
			//where it starts. So the vertex is the anchor set back by the offset and the radius.
			double perpendicular = this.Direction + (Math.PI / 2.0);
			double reach = this.DimensionLineOffset + this.DimensionArcRadius;

			XYZ vertex = polar(this.Anchor, perpendicular, reach);

			//The arc starts on the far side of the vertex from the direction it was reached by, which
			//is a half turn from it, and sweeps by the angle the object measures.
			double sweep = perpendicular + Math.PI;

			return new DimensionAngular3Pt
			{
				AngleVertex = vertex,
				FirstPoint = polar(vertex, sweep, reach),
				SecondPoint = polar(vertex, sweep + this.Angle, reach),
				DefinitionPoint = polar(vertex, sweep + (this.Angle * DimensionArcFraction), this.DimensionArcRadius),
			};
		}

		/// <summary>
		/// Moves a point a distance along a direction.
		/// </summary>
		private static XYZ polar(XYZ from, double angle, double distance)
		{
			return new XYZ(
				from.X + (distance * Math.Cos(angle)),
				from.Y + (distance * Math.Sin(angle)),
				from.Z);
		}
	}
}
