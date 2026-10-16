using ACadSharp.Classes;
using ACadSharp.IO.DWG.Tangent;
using CSMath;
using System;
using System.Collections.Generic;

namespace ACadSharp.Entities.Tangent
{
	/// <summary>
	/// A parallel stair, as Tianzheng stores one.
	/// </summary>
	/// <remarks>
	/// <para>
	/// The same dimensions as the rectangular stair, put to a wider stair: two flights against the
	/// outer edges, a well inside each of them, and a third flight down the middle between the two
	/// wells. The outline itself is the shared one — see <see cref="TchStairOutline"/> — and what this
	/// class does beyond reading is decide nothing.
	/// </para>
	/// <para>
	/// The two classes lay their fields out differently and mean the same things by them. This one
	/// writes three more values before the flight's depth than the rectangular one does, so the same
	/// member sits four places further along the stream, which is why the two read their dimensions
	/// from different offsets rather than sharing one list.
	/// </para>
	/// </remarks>
	public sealed class TchParallelStair : TchEntity
	{
		/// <summary>
		/// How many strings sit between the property block and the class's own fields.
		/// </summary>
		/// <remarks>
		/// A class identifier and one empty value, against the rectangular stair's identifier and two.
		/// Both are measured, and what makes reading from the property block's closing marker worth
		/// doing is that these two classes carry different blocks — one lists its properties between
		/// the markers and the other does not — so a count taken from the start of the stream would
		/// not have survived either of them.
		/// </remarks>
		public const int StringsAfterPropertyBlock = 2;

		/// <summary>
		/// Which of the class's own fields each value it uses is.
		/// </summary>
		/// <remarks>
		/// Counted from the first value after the property block, in the order <c>dwgInFields</c> reads
		/// them. The names are the members the class keeps them in, which the decompiled reader gives:
		/// <c>0x158</c> for the riser, <c>0x160</c> for the tread, <c>0x168</c> for the handrail's
		/// offset, <c>0x188</c> for the flight's depth, <c>0x1b0</c> for the tread lines, <c>0x1b8</c>
		/// for the width and <c>0x1c8</c> for the band. What each one is for was still measured: the
		/// class stores ten tread lines and the export draws ten.
		/// </remarks>
		private const int RiserIndex = 1;

		/// <inheritdoc cref="RiserIndex"/>
		private const int TreadDepthIndex = 2;

		/// <inheritdoc cref="RiserIndex"/>
		private const int HandrailOffsetIndex = 3;

		/// <inheritdoc cref="RiserIndex"/>
		private const int FirstStepOffsetIndex = 7;

		/// <inheritdoc cref="RiserIndex"/>
		private const int TreadLineCountIndex = 8;

		/// <inheritdoc cref="RiserIndex"/>
		private const int WidthIndex = 10;

		/// <inheritdoc cref="RiserIndex"/>
		private const int HandrailBandIndex = 15;

		/// <summary>
		/// The most tread lines a stair is taken to have.
		/// </summary>
		public const int MaxTreadLines = 64;

		private readonly double[] _values = new double[HandrailBandIndex + 1];

		/// <summary>
		/// Initialises a stair from the class it was read as.
		/// </summary>
		internal TchParallelStair(DxfClass dxfClass)
			: base(dxfClass)
		{
		}

		/// <summary>
		/// Gets where the stair is placed, which is the corner of its top step.
		/// </summary>
		public XYZ Position { get; private set; }

		/// <summary>
		/// Gets how far the first tread line sits below the stair's position.
		/// </summary>
		public double FirstStepOffset { get; private set; }

		/// <summary>
		/// Gets the distance between one tread line and the next.
		/// </summary>
		public double TreadDepth { get; private set; }

		/// <summary>
		/// Gets how many tread lines the run holds, which is one more than the steps between them.
		/// </summary>
		public int TreadLineCount { get; private set; }

		/// <summary>
		/// Gets the height of one step, which a plan view does not show.
		/// </summary>
		public double Riser { get; private set; }

		/// <summary>
		/// Gets the total width of the stair, across every flight and well.
		/// </summary>
		public double Width { get; private set; }

		/// <summary>
		/// Gets how far the handrail's band sits inside the stair's outer edge.
		/// </summary>
		public double HandrailOffset { get; private set; }

		/// <summary>
		/// Gets how far the edge of a flight sits inside the handrail's band.
		/// </summary>
		public double HandrailBand { get; private set; }

		/// <summary>
		/// Gets how far an outer flight's edge sits from the stair's outer edge.
		/// </summary>
		public double FlightWidth => this.HandrailOffset - this.HandrailBand;

		/// <summary>
		/// Gets how deep the stair runs, from its position to the last tread line.
		/// </summary>
		public double Run => this.FirstStepOffset + ((this.TreadLineCount - 1) * this.TreadDepth);

		/// <summary>
		/// Gets a value indicating whether the object described a stair this can convert.
		/// </summary>
		public bool IsConvertible { get; private set; }

		/// <summary>
		/// Gets why <see cref="IsConvertible"/> is false, or null when it is true.
		/// </summary>
		public string Reason { get; private set; }

		/// <summary>
		/// Reads a stair from the bytes a class's own fields were written to.
		/// </summary>
		public static TchParallelStair Parse(DxfClass dxfClass, byte[] payload, ACadVersion version)
		{
			TchParallelStair stair = new TchParallelStair(dxfClass);

			stair.StringStreamOffset = FindStringStream(payload, payload.Length * 8L);
			stair.Fill(payload, version);

			return stair;
		}

		/// <inheritdoc/>
		public override IEnumerable<Entity> ToStandardEntities()
		{
			if (!this.IsConvertible)
			{
				return Array.Empty<Entity>();
			}

			return TchStairOutline.Lines(
				this,
				this.Position,
				this.Width,
				this.HandrailOffset,
				this.HandrailBand,
				this.FirstStepOffset,
				this.TreadDepth,
				this.TreadLineCount);
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
				this.Reason = "the object has no string stream, so its position cannot be placed";
				return;
			}

			if (!TchFieldMap.TryReadInlinePath(payload, 1, this.StringStreamOffset, out XYZ[] position))
			{
				this.Reason = "no point fills the space before the string stream, so the stair is not placed";
				return;
			}

			this.Position = position[0];

			TchStreamReader reader = TchFieldMap.SkipPropertyBlock(payload, this.StringStreamOffset, 0);

			for (int i = 0; i < StringsAfterPropertyBlock; i++)
			{
				reader = TchFieldMap.SkipString(reader);
			}

			for (int i = 0; i < _values.Length; i++)
			{
				_values[i] = TchFieldMap.ReadDouble(reader, TgDataEncrypt.ObjectXor);
			}

			this.Riser = _values[RiserIndex];
			this.TreadDepth = _values[TreadDepthIndex];
			this.HandrailOffset = _values[HandrailOffsetIndex];
			this.FirstStepOffset = _values[FirstStepOffsetIndex];
			this.Width = _values[WidthIndex];
			this.HandrailBand = _values[HandrailBandIndex];

			double lines = _values[TreadLineCountIndex];

			if (double.IsNaN(lines) || lines < 2.0 || lines > MaxTreadLines || lines != Math.Floor(lines))
			{
				this.Reason = $"the object records {lines} tread lines, which is not a stair";
				return;
			}

			this.TreadLineCount = (int)lines;

			if (!isUsable(this.Width) || !isUsable(this.TreadDepth) || !isUsable(this.HandrailOffset)
				|| !isUsable(this.Riser) || this.FirstStepOffset < 0.0 || this.HandrailBand < 0.0
				|| this.HandrailOffset >= (this.Width / 2.0) || this.HandrailBand >= this.HandrailOffset)
			{
				this.Reason = "the stair's dimensions do not read as the lengths its outline is built from";
				return;
			}

			this.IsConvertible = true;
		}

		private static bool isUsable(double value)
		{
			return !double.IsNaN(value) && value > 0.0 && value < 1e6;
		}
	}
}
