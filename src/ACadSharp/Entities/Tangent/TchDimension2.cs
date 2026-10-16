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
	/// when that geometry moves. Only the object's own data is used here, which is enough for the
	/// run: the anchor, the direction and the distances describe every dimension in it without
	/// consulting anything the object points at. It is not enough for the angular form, which is
	/// why that one is refused rather than approximated.
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
		/// Where the dimension line's offset from the measured line begins, as a bit offset.
		/// </summary>
		/// <remarks>
		/// The one number this class writes as a plain double rather than as text, which is why it is
		/// found by position and the rest of the object's numbers are found in the stream.
		/// </remarks>
		public const long DimensionLineOffsetOffset = 30L;

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
		public IReadOnlyList<double> Segments => _segments;

		/// <summary>
		/// Gets a value indicating whether the object's own data describes a run this can convert.
		/// </summary>
		/// <remarks>
		/// False for a dimension whose shape is not decided by the object: the angular form records
		/// the angle between two directions it does not record, so its vertex cannot be recovered
		/// without the geometry it refers to. Conversion yields nothing for one of those rather than
		/// placing a dimension that would look plausible and be wrong.
		/// </remarks>
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
			this.DimensionLineOffset = TchFieldMap.ReadInlineRawDouble(payload, DimensionLineOffsetOffset);

			TchStreamReader reader = new TchStreamReader(payload, this.StringStreamOffset);

			//What comes before the class's own fields was written by a reader above it in the chain.
			for (int i = 0; i < TchFieldMap.DimensionSkippedStrings; i++)
			{
				reader = TchFieldMap.SkipString(reader);
			}

			this.Direction = TchFieldMap.ReadDouble(reader, TgDataEncrypt.ObjectXor);

			//The offset is written twice, once inline in drawing units and once here divided by the
			//drawing's dimension scale. The inline one is the one that places the dimension line, so
			//this second copy is stepped over rather than read.
			reader = TchFieldMap.SkipString(reader);

			double count = TchFieldMap.ReadDouble(reader, TgDataEncrypt.ObjectXor);

			if (double.IsNaN(count) || count < 1.0 || count > MaxSegments || count != Math.Floor(count))
			{
				this.Reason = $"the object records {count} measurements, which is not a run";
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

			//A run ends with a zero the class writes after its measurements. The angular form does
			//not: it writes three more numbers — the angle, the arc's radius and the distance to the
			//first point — in the same place, and a reader that took its single angle for a distance
			//would place a dimension a fraction of a unit long and draw it as though that were the
			//measurement. Checking for the zero is what tells the two apart, because nothing about
			//the count does.
			double terminator = TchFieldMap.ReadDouble(reader, TgDataEncrypt.ObjectXor);

			if (terminator != 0.0)
			{
				this.Reason = "the object is an angular dimension, whose vertex is not among its own fields";
				_segments.Clear();
				return;
			}

			this.IsConvertible = true;
		}
	}
}
