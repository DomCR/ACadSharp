using ACadSharp.Classes;
using ACadSharp.IO.DWG.Tangent;
using CSMath;
using System;
using System.Collections.Generic;

namespace ACadSharp.Entities.Tangent
{
	/// <summary>
	/// A rectangular stair, as Tianzheng stores one.
	/// </summary>
	/// <remarks>
	/// <para>
	/// The class stores no outline. What it stores is a flight's worth of dimensions, and the drawing
	/// is worked out from them: two flights side by side with a well between them, a tread line every
	/// so often, and the lines that make up the well and the handrail.
	/// </para>
	/// <para>
	/// The values are the ones the drawn stair agrees with rather than the ones a name suggests. Which
	/// stored number is the tread depth is settled by the spacing of the tread lines, which is 270 and
	/// is the fourth value the class writes; which is the number of steps by there being nine of them,
	/// which is the last. A class whose fields are unnamed is read by measuring, and the measurement is
	/// the geometry the drawing already shows.
	/// </para>
	/// <para>
	/// The export draws this outline cut open where the stair is broken off, and adds the symbol that
	/// marks the break: two parallel lines across the flights with a jog between them. That symbol is
	/// not part of the stair's shape — it says the stair continues upwards — and it is left out here.
	/// Every straight line the export draws lies on this outline, and the pieces it leaves out are
	/// exactly where the break crosses.
	/// </para>
	/// </remarks>
	public sealed class TchRectStair : TchEntity
	{
		/// <summary>
		/// How many strings sit between the property block and the class's own fields.
		/// </summary>
		/// <remarks>
		/// A class identifier and two empty values, the same in the one stair available.
		/// </remarks>
		public const int StringsAfterPropertyBlock = 3;

		/// <summary>
		/// Which of the class's own fields each value it uses is.
		/// </summary>
		/// <remarks>
		/// Counted from the first value after the property block, in the order <c>dwgInFields</c> reads
		/// them. The class writes thirty and the rest describe what the stair looks like rather than
		/// where its lines go: the fill colour, the text height, the direction the flights run in.
		/// <para>
		/// The two stair classes put the same things at the same members and differ only in how many
		/// values they write before them, which is why the parallel stair's indices are these plus
		/// four. The tread lines are the fifth value rather than a count of steps further down: the
		/// class stores how many lines to draw, and a stair of ten of them has nine steps between.
		/// </para>
		/// </remarks>
		private const int RiserIndex = 0;

		/// <inheritdoc cref="RiserIndex"/>
		private const int TreadDepthIndex = 1;

		/// <inheritdoc cref="RiserIndex"/>
		private const int HandrailOffsetIndex = 2;

		/// <inheritdoc cref="RiserIndex"/>
		private const int FirstStepOffsetIndex = 3;

		/// <inheritdoc cref="RiserIndex"/>
		private const int TreadLineCountIndex = 4;

		/// <inheritdoc cref="RiserIndex"/>
		private const int WidthIndex = 6;

		/// <inheritdoc cref="RiserIndex"/>
		private const int HandrailBandIndex = 11;

		/// <summary>
		/// The most tread lines a stair is taken to have.
		/// </summary>
		/// <remarks>
		/// A guard rather than a fact about the format: the count is read from the object, and one
		/// that reads wrongly would otherwise emit an arbitrary number of lines.
		/// </remarks>
		public const int MaxTreadLines = 64;

		private readonly double[] _values = new double[HandrailBandIndex + 1];

		/// <summary>
		/// Initialises a stair from the class it was read as.
		/// </summary>
		internal TchRectStair(DxfClass dxfClass)
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
		/// <remarks>
		/// Kept because it is the stair's and a caller raising it into three dimensions needs it, and
		/// because it is what fixes where the break symbol goes: the export crosses the flights at the
		/// height a stored value names, which comes to a fraction of a step and is why the break does
		/// not land on a tread line.
		/// </remarks>
		public double Riser { get; private set; }

		/// <summary>
		/// Gets the total width of the stair, across both flights and the well.
		/// </summary>
		public double Width { get; private set; }

		/// <summary>
		/// Gets how far the handrail's line sits inside the stair's outer edge.
		/// </summary>
		public double HandrailOffset { get; private set; }

		/// <summary>
		/// Gets how far the edge of a flight sits inside the handrail's line.
		/// </summary>
		public double HandrailBand { get; private set; }

		/// <summary>
		/// Gets how far a flight's edge sits from the stair's outer edge.
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
		public static TchRectStair Parse(DxfClass dxfClass, byte[] payload, ACadVersion version)
		{
			TchRectStair stair = new TchRectStair(dxfClass);

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
			//The outline is worked out from the stair's dimensions rather than kept, so moving it would
			//mean recomputing the stored fields, which is not what a transform asks for.
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

			//A stair writes one point, last among the things it writes inline, and where it begins is
			//not recorded any more than a wall's baseline is.
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

			//Every one of these is a length the outline is built from, so a count that landed in the
			//wrong place gives numbers that are not lengths rather than an error.
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
