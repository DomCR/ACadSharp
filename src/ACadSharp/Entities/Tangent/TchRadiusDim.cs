using ACadSharp.Classes;
using ACadSharp.IO.DWG.Tangent;
using CSMath;
using CSMath.Extensions;
using System;
using System.Collections.Generic;

namespace ACadSharp.Entities.Tangent
{
	/// <summary>
	/// A radius dimension, as Tianzheng stores one.
	/// </summary>
	/// <remarks>
	/// <para>
	/// The class stores three things: the centre of the circle it measures, a point on that circle,
	/// and how long the leader that points at it is. The radius is not stored at all — it is the
	/// distance between the two points, which is what the class itself works out when it is asked
	/// for one, and what the drawing shows.
	/// </para>
	/// <para>
	/// The arrow at the end of the leader and the number beside it are not stored either, and they
	/// are not constants that had to be guessed: they belong to the dimension style the object
	/// refers to, which is an object in the same drawing. The class keeps a reference to it, and the
	/// style holds the arrow's size, the text's height and how far the text sits from the line —
	/// 200, 297.5 and 126.25 for the one in the sample, all three of which the flattened export
	/// draws. That is why what this converts to is a dimension rather than a set of lines: the
	/// standard entity carries the same three quantities and a reader that has the style has
	/// everything the object had.
	/// </para>
	/// </remarks>
	public sealed class TchRadiusDim : TchEntity
	{
		/// <summary>
		/// Where the two points begin, as a bit offset into the payload.
		/// </summary>
		/// <remarks>
		/// The centre is written the way a position is, with a length prefix on each coordinate, and
		/// the point on the circle follows it as two plain doubles with none. Counting rather than
		/// searching is safe for the first of them: what precedes it is a version, a tag and a short,
		/// none of which varies, and what follows the pair is the string stream, which is where the
		/// second has to end.
		/// </remarks>
		public const long CenterOffset = 34L;

		/// <inheritdoc cref="CenterOffset"/>
		public const long CirclePointOffset = 168L;

		/// <summary>
		/// How many strings sit between the property block and the class's own fields.
		/// </summary>
		/// <remarks>
		/// A class identifier and one value of the reader's above it. The class writes two values of
		/// its own and they are the last two in the stream, which is what fixes the count.
		/// </remarks>
		public const int StringsAfterPropertyBlock = 2;

		/// <summary>
		/// Which bit of the object's flags says it measures a diameter.
		/// </summary>
		/// <remarks>
		/// Read from the decompiled reader, which tests this bit to choose between a radius and a
		/// diameter and reads the measurement's text accordingly. The object in the sample has it
		/// clear, which is what its own text says it is.
		/// </remarks>
		public const int DiameterFlag = 0x10;

		/// <summary>
		/// The index of the flags among the class's own values.
		/// </summary>
		/// <remarks>
		/// The reader takes them before anything else, ahead of the style it refers to, and they are
		/// the only two values the class writes: what follows them in the object is the style's
		/// reference, which is a handle and not a value.
		/// </remarks>
		private const int FlagsIndex = 0;

		/// <inheritdoc cref="FlagsIndex"/>
		private const int LeaderLengthIndex = 1;

		private readonly double[] _values = new double[LeaderLengthIndex + 1];

		/// <summary>
		/// Initialises a dimension from the class it was read as.
		/// </summary>
		internal TchRadiusDim(DxfClass dxfClass)
			: base(dxfClass)
		{
		}

		/// <summary>
		/// Gets the centre of the circle the dimension measures.
		/// </summary>
		public XYZ Center { get; private set; }

		/// <summary>
		/// Gets the point on the circle the leader points at.
		/// </summary>
		public XYZ CirclePoint { get; private set; }

		/// <summary>
		/// Gets how long the leader is, in drawing units.
		/// </summary>
		/// <remarks>
		/// Stored in the drawing's own scale rather than in units, so the scale the object records
		/// alongside it is what turns it into a length. The sign says which way the leader leaves the
		/// circle and is not part of the length.
		/// </remarks>
		public double LeaderLength { get; private set; }

		/// <summary>
		/// Gets the radius, which is the distance between the two points the object stores.
		/// </summary>
		public double Radius => this.Center.DistanceFrom(this.CirclePoint);

		/// <summary>
		/// Gets a value indicating whether the object described a dimension this can convert.
		/// </summary>
		public bool IsConvertible { get; private set; }

		/// <summary>
		/// Gets why <see cref="IsConvertible"/> is false, or null when it is true.
		/// </summary>
		public string Reason { get; private set; }

		/// <summary>
		/// Reads a dimension from the bytes a class's own fields were written to.
		/// </summary>
		public static TchRadiusDim Parse(DxfClass dxfClass, byte[] payload, ACadVersion version)
		{
			TchRadiusDim dimension = new TchRadiusDim(dxfClass);

			dimension.StringStreamOffset = FindStringStream(payload, payload.Length * 8L);
			dimension.Fill(payload, version);

			return dimension;
		}

		/// <inheritdoc/>
		public override IEnumerable<Entity> ToStandardEntities()
		{
			if (!this.IsConvertible)
			{
				return Array.Empty<Entity>();
			}

			//The three things the object stores are the three things the standard dimension carries:
			//the centre, the point on the circle, and the leader. Everything else about how it is
			//drawn — the arrow, the text, its size and its distance from the line — belongs to the
			//style, which this entity is left free to be given.
			return new[]
			{
				CopyCommonProperties(this, new DimensionRadius
				{
					AngleVertex = this.Center,
					DefinitionPoint = this.CirclePoint,
					LeaderLength = this.LeaderLength,
				}),
			};
		}

		/// <inheritdoc/>
		public override void ApplyTransform(Transform transform)
		{
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
				this.Reason = "the object has no string stream, so its leader cannot be sized";
				return;
			}

			this.Center = TchFieldMap.ReadInlinePosition(payload, CenterOffset);
			this.CirclePoint = TchFieldMap.ReadInlineRawPosition(payload, CirclePointOffset);

			double scale;

			//The scale is the second string of every object's stream, written before its property
			//block, and it is the same for all of them: the drawing's own.
			TchStreamReader head = new TchStreamReader(payload, this.StringStreamOffset);
			head = TchFieldMap.SkipString(head);
			scale = TchFieldMap.ReadDouble(head, TgDataEncrypt.ObjectXor);

			TchStreamReader reader = TchFieldMap.SkipPropertyBlock(payload, this.StringStreamOffset, 0);

			for (int i = 0; i < StringsAfterPropertyBlock; i++)
			{
				reader = TchFieldMap.SkipString(reader);
			}

			for (int i = 0; i < _values.Length; i++)
			{
				_values[i] = TchFieldMap.ReadDouble(reader, TgDataEncrypt.ObjectXor);
			}

			if (double.IsNaN(scale) || scale <= 0.0)
			{
				this.Reason = "the object records no drawing scale, so its leader has no length";
				return;
			}

			double flags = _values[FlagsIndex];

			if (double.IsNaN(flags))
			{
				this.Reason = "the object's flags do not read, so what it measures is not established";
				return;
			}

			if ((((int)flags) & DiameterFlag) != 0)
			{
				this.Reason = "the object measures a diameter, which is a different dimension from the one this reads";
				return;
			}

			double leader = _values[LeaderLengthIndex];

			if (double.IsNaN(leader) || leader == 0.0)
			{
				this.Reason = "the object records no leader, so the dimension cannot be placed";
				return;
			}

			this.LeaderLength = Math.Abs(leader) * scale;

			if (this.Radius <= 0.0)
			{
				this.Reason = "the two points the object stores are the same point, so there is no radius";
				return;
			}

			this.IsConvertible = true;
		}
	}
}
