using ACadSharp.Classes;
using ACadSharp.IO.DWG.Tangent;
using CSMath;
using System;
using System.Collections.Generic;

namespace ACadSharp.Entities.Tangent
{
	/// <summary>
	/// A door or a window, as Tianzheng stores one.
	/// </summary>
	/// <remarks>
	/// <para>
	/// Unlike the classes whose geometry is computed, an opening does not describe a shape at all: it
	/// records which library symbol it uses and how big it is, and the drawing is a block reference.
	/// So the conversion is a reference too, and the geometry comes from the block rather than from
	/// anything worked out here.
	/// </para>
	/// <para>
	/// Which symbol is not a field. It follows from the usage the object carries as a property, and
	/// the two agree in every instance in the sample: a door uses one library block and a window
	/// another. That is why the usage is read even though it is text and not a number.
	/// </para>
	/// </remarks>
	public sealed class TchOpening : TchEntity
	{
		/// <summary>
		/// The block a door is drawn with.
		/// </summary>
		public const string DoorBlock = "$DorLib2D$00000011";

		/// <summary>
		/// The block a window is drawn with.
		/// </summary>
		public const string WindowBlock = "$TCHSYS$WIN2D";

		/// <summary>
		/// The property that says whether this is a door or a window.
		/// </summary>
		public const string UsageProperty = "门窗用途";

		/// <summary>
		/// How many strings in the stream belong to readers above <see cref="TchOpening"/>.
		/// </summary>
		private const int SkippedStrings = 11;

		/// <summary>
		/// Gets where the opening sits on the wall's centre line.
		/// </summary>
		public XYZ Position { get; private set; }

		/// <summary>
		/// Gets how wide the opening is, along the wall.
		/// </summary>
		public double Width { get; private set; }

		/// <summary>
		/// Gets how tall the opening is.
		/// </summary>
		public double Height { get; private set; }

		/// <summary>
		/// Gets half the thickness of the wall it is set into.
		/// </summary>
		/// <remarks>
		/// Stored rather than measured from the wall, and named for what it is: the block is placed
		/// against the centre line, so half a thickness is the distance to either face.
		/// </remarks>
		public double HalfThickness { get; private set; }

		/// <summary>
		/// Gets the usage the object records, as it is written.
		/// </summary>
		public string Usage { get; private set; }

		/// <summary>
		/// Gets whether the object records itself as a door.
		/// </summary>
		public bool IsDoor => this.Usage != null && this.Usage.Contains("门");

		/// <summary>
		/// Gets whether the object records itself as a window.
		/// </summary>
		public bool IsWindow => this.Usage != null && this.Usage.Contains("窗");

		/// <summary>
		/// Gets the block the opening is drawn with.
		/// </summary>
		public string BlockName => this.IsWindow ? WindowBlock : DoorBlock;

		/// <summary>
		/// Gets how thick the wall is.
		/// </summary>
		public double Thickness => 2.0 * this.HalfThickness;

		/// <summary>
		/// Reads an opening from the bytes a class's own fields were written to.
		/// </summary>
		public static TchOpening Parse(DxfClass dxfClass, byte[] payload, ACadVersion version)
		{
			TchOpening opening = new TchOpening(dxfClass);

			opening.StringStreamOffset = FindStringStream(payload, payload.Length * 8L);
			opening.Properties = TchDataMap.ReadFromPayload(payload, version);
			opening.Fill(payload, version);

			return opening;
		}

		/// <inheritdoc/>
		public override IEnumerable<Entity> ToStandardEntities()
		{
			//A reference, not a shape: the symbol is a block in the drawing and the geometry lives
			//there. The scales are the opening's own size, which is what the block was drawn at unit
			//size for.
			//The two symbols are drawn at different scales for the same opening, because they are
			//built differently: the window block is a unit square and has to be stretched to the wall
			//it sits in, while the door block is already in the proportions it is drawn at and is
			//scaled evenly. Reading one rule for both gets one of them wrong by a factor of four.
			Insert insert = new Insert
			{
				InsertPoint = this.Position,
				XScale = -this.Width,
				YScale = this.IsDoor ? this.Width : this.Thickness,
				ZScale = -this.Width,
			};

			yield return insert;
		}

		/// <inheritdoc/>
		public override void ApplyTransform(Transform transform)
		{
		}

		/// <inheritdoc/>
		public override BoundingBox GetBoundingBox()
		{
			return BoundingBox.Null;
		}

		/// <inheritdoc/>
		internal override void Fill(byte[] payload, ACadVersion version)
		{
			if (payload == null || this.StringStreamOffset < 0)
			{
				return;
			}

			TchStreamReader reader = new TchStreamReader(payload, this.StringStreamOffset);

			//The usage is text and sits among the numbers, so it is picked out by shape rather than by
			//position: it is the string that is not ciphertext and not a marker.
			string usage = TchFieldMap.FindText(reader, SkippedStrings);

			reader = new TchStreamReader(payload, this.StringStreamOffset);

			for (int i = 0; i < SkippedStrings; i++)
			{
				reader = TchFieldMap.SkipString(reader);
			}

			this.Position = TchFieldMap.ReadInlinePosition(payload, this.StringStreamOffset);

			//A model code comes before the size, and it is not a size: it names which library symbol
			//the opening is, and the block the usage selects already says that.
			reader = TchFieldMap.SkipString(reader);

			this.Width = TchFieldMap.ReadDouble(reader, TgDataEncrypt.ObjectXor);
			this.HalfThickness = TchFieldMap.ReadDouble(reader, TgDataEncrypt.ObjectXor);
			this.Height = TchFieldMap.ReadDouble(reader, TgDataEncrypt.ObjectXor);

			this.Usage = usage;
		}

		/// <summary>
		/// Initialises an opening from the class it was read as.
		/// </summary>
		internal TchOpening(DxfClass dxfClass)
			: base(dxfClass)
		{
		}
	}
}
