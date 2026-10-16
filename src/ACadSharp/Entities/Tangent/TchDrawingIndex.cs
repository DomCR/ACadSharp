using ACadSharp.Classes;
using ACadSharp.IO.DWG.Tangent;
using CSMath;
using System;
using System.Collections.Generic;

namespace ACadSharp.Entities.Tangent
{
	/// <summary>
	/// A drawing index: the circle that names a drawing and the scale it is drawn at.
	/// </summary>
	/// <remarks>
	/// <para>
	/// The object stores the scale of the drawing it belongs to, the multiple of that scale its
	/// circle is drawn at, where it sits, and three pieces of text — the number inside the circle,
	/// the title block's label, and the scale written beside it. It does not store the circle's
	/// outline, because a circle is its centre and its radius, and both of those are here.
	/// </para>
	/// <para>
	/// The scale's text is placed from the centre in two steps, each a distance along a direction:
	/// across by the scale's own multiple, then down by the circle's. Every one of those quantities
	/// is a field of the object, so the position is derived rather than measured — and for the one
	/// object in the sample it lands on the export's own text to the last place.
	/// </para>
	/// <para>
	/// Two things this does not do, and neither is an omission that can be fixed here. The number
	/// inside the circle is not placed: its size and its position are solved from the outline of
	/// its glyph in the drawing's own font, which the object does not carry. And the text this does
	/// place is drawn smaller by the export than <see cref="TextHeight"/> reads; see that property
	/// for what the difference is and where it lives. The unplaced number is reported through
	/// <see cref="UnplacedText"/> rather than passed over.
	/// </para>
	/// </remarks>
	public sealed class TchDrawingIndex : TchEntity
	{
		/// <summary>
		/// How many bits the centre the object writes inline occupies.
		/// </summary>
		/// <remarks>
		/// The class reads it with the filer's own point reader, which writes two plain doubles with
		/// no length prefix, and it is the last thing written before the string stream. So it begins
		/// at the end of the stream minus this — and because that alone would read the two doubles
		/// before the stream whatever they were, they are checked for being coordinates.
		/// </remarks>
		public const long CenterBits = 128L;

		/// <summary>The multiple of the scale the circle's radius is.</summary>
		private const int RadiusMultipleIndex = 5;

		/// <summary>The number written inside the circle.</summary>
		private const int IndexLabelIndex = 7;

		/// <inheritdoc cref="IndexLabelIndex"/>
		private const int DrawingLabelIndex = 8;

		/// <inheritdoc cref="IndexLabelIndex"/>
		private const int ScaleTextIndex = 9;

		/// <inheritdoc cref="IndexLabelIndex"/>
		private const int ScaleTextHeightIndex = 10;

		/// <inheritdoc cref="IndexLabelIndex"/>
		private const int ScaleXDistanceIndex = 11;

		/// <summary>
		/// How many entries of the class's own follow the property block.
		/// </summary>
		/// <remarks>
		/// The class writes more than it reads, so this is where the reading stops rather than
		/// where the object does. What is counted is the order the class declares its fields in,
		/// which is the order they are written: entry zero is the class id, and the property block
		/// between it and the rest is a different length in every object, which is why the count
		/// runs from the block's closing marker.
		/// </remarks>
		private const int EntryCount = ScaleXDistanceIndex + 1;

		private readonly Entry[] _entries = new Entry[EntryCount];

		private struct Entry
		{
			public bool IsNumber;

			public double Number;

			public string Text;
		}

		/// <summary>
		/// Initialises a drawing index from the class it was read as.
		/// </summary>
		internal TchDrawingIndex(DxfClass dxfClass)
			: base(dxfClass)
		{
		}

		/// <summary>
		/// Gets the centre of the circle.
		/// </summary>
		public XYZ Center { get; private set; }

		/// <summary>
		/// Gets the drawing's own scale, which every length the object records is a multiple of.
		/// </summary>
		public double Scale { get; private set; }

		/// <summary>
		/// Gets the direction the whole index is turned to, in radians.
		/// </summary>
		public double Rotation { get; private set; }

		/// <summary>
		/// Gets the radius of the circle, in drawing units.
		/// </summary>
		public double Radius { get; private set; }

		/// <summary>
		/// Gets the number the circle holds.
		/// </summary>
		/// <remarks>
		/// Read, and not placed. See the remarks on this class.
		/// </remarks>
		public string IndexLabel { get; private set; }

		/// <summary>
		/// Gets the title block's label, which the sample drawing leaves empty.
		/// </summary>
		public string DrawingLabel { get; private set; }

		/// <summary>
		/// Gets the scale, written as the drawing states it — <c>1:20</c> in the sample.
		/// </summary>
		public string ScaleText { get; private set; }

		/// <summary>
		/// Gets where the scale's text sits.
		/// </summary>
		public XYZ ScaleTextPoint { get; private set; }

		/// <summary>
		/// Gets the height the object records for its texts, in drawing units.
		/// </summary>
		/// <remarks>
		/// The export draws these texts at 297.5 where this reads 350, and the difference is not a
		/// misreading. The object records a height in the drawing's scale, as every class here does
		/// — 3.5, with the drawing at 1:100 — and this class then draws its texts smaller by a ratio
		/// it takes from the drawing's configuration object. That object is not this one, so the
		/// ratio is not available here, and a caller that has the drawing can apply it where one
		/// that does not gets a text of the right shape in the right place and a sixth too large.
		/// </remarks>
		public double TextHeight { get; private set; }

		/// <summary>
		/// Gets the texts this object holds but does not place, with why.
		/// </summary>
		/// <remarks>
		/// Empty is the ordinary case. The number inside the circle is the one that appears here,
		/// and it is listed rather than dropped so that a conversion can say what it left out.
		/// </remarks>
		public IEnumerable<string> UnplacedText
		{
			get
			{
				if (!string.IsNullOrEmpty(this.IndexLabel))
				{
					yield return $"the number '{this.IndexLabel}' inside the circle, whose size and "
						+ "position come from the outline of its glyph in the drawing's font";
				}
			}
		}

		/// <summary>
		/// Gets a value indicating whether the object described an index this can convert.
		/// </summary>
		public bool IsConvertible { get; private set; }

		/// <summary>
		/// Gets why <see cref="IsConvertible"/> is false, or null when it is true.
		/// </summary>
		public string Reason { get; private set; }

		/// <summary>
		/// Reads a drawing index from the bytes a class's own fields were written to.
		/// </summary>
		public static TchDrawingIndex Parse(DxfClass dxfClass, byte[] payload, ACadVersion version)
		{
			TchDrawingIndex index = new TchDrawingIndex(dxfClass);

			index.StringStreamOffset = FindStringStream(payload, payload.Length * 8L);
			index.Fill(payload, version);

			return index;
		}

		/// <inheritdoc/>
		public override IEnumerable<Entity> ToStandardEntities()
		{
			if (!this.IsConvertible)
			{
				return Array.Empty<Entity>();
			}

			//A circle is drawn as a closed polyline whose two vertices are the ends of a diameter
			//and whose bulge is one on each, which is two half circles. That is what the export
			//writes, and it is the one shape here that carries no font and no configuration.
			LwPolyline circle = new LwPolyline();

			circle.Vertices.Add(new LwPolyline.Vertex(new XY(this.Center.X + this.Radius, this.Center.Y))
			{
				Bulge = 1.0,
			});

			circle.Vertices.Add(new LwPolyline.Vertex(new XY(this.Center.X - this.Radius, this.Center.Y))
			{
				Bulge = 1.0,
			});

			circle.IsClosed = true;

			return new Entity[]
			{
				CopyCommonProperties(this, circle),
				CopyCommonProperties(this, new TextEntity
				{
					InsertPoint = this.ScaleTextPoint,
					Value = this.ScaleText ?? string.Empty,
					Height = this.TextHeight,
				}),
			};
		}

		/// <inheritdoc/>
		public override void ApplyTransform(Transform transform)
		{
			this.Center = transform.ApplyTransform(this.Center);
			this.ScaleTextPoint = transform.ApplyTransform(this.ScaleTextPoint);
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
				this.Reason = "the object has no string stream, so its scale cannot be read";
				return;
			}

			if (!this.readCircle(payload))
			{
				return;
			}

			if (!this.readHeader(payload))
			{
				return;
			}

			if (!this.readEntries(payload))
			{
				return;
			}

			this.IndexLabel = _entries[IndexLabelIndex].Text;
			this.DrawingLabel = _entries[DrawingLabelIndex].Text;
			this.ScaleText = _entries[ScaleTextIndex].Text;

			double radiusMultiple = _entries[RadiusMultipleIndex].Number;
			double xDistance = _entries[ScaleXDistanceIndex].Number * this.Scale;

			this.Radius = radiusMultiple * this.Scale;

			if (this.Radius <= 0.0)
			{
				this.Reason = "the object records no radius, so there is no circle to draw";
				return;
			}

			//The scale's text is placed from the centre in two steps, each a distance along a
			//direction: across by the scale's own multiple, then down by the circle's. The second
			//distance is the radius and it points the other way, which is what puts the text below
			//the circle rather than through it.
			this.ScaleTextPoint = polar(
				polar(this.Center, this.Rotation, xDistance),
				this.Rotation + (Math.PI / 2.0),
				-this.Radius);

			this.TextHeight = _entries[ScaleTextHeightIndex].Number * this.Scale;

			this.IsConvertible = true;
		}

		/// <summary>
		/// Reads the scale and the direction, which every object of every class carries.
		/// </summary>
		/// <remarks>
		/// They are the two values after the first string of the stream, and they are written
		/// before the property block, so they are not among the entries counted from the block's
		/// closing marker. The scale is the drawing's own and is the same in every object; it is
		/// what turns the multiples the class records into lengths.
		/// </remarks>
		private bool readHeader(byte[] payload)
		{
			TchStreamReader afterId =
				TchFieldMap.SkipString(new TchStreamReader(payload, this.StringStreamOffset));

			this.Scale = TchFieldMap.ReadDouble(afterId, TgDataEncrypt.ObjectXor);
			this.Rotation = TchFieldMap.ReadDouble(afterId, TgDataEncrypt.ObjectXor);

			if (double.IsNaN(this.Scale) || this.Scale <= 0.0)
			{
				this.Reason = "the object records no drawing scale, so its lengths are not lengths";
				return false;
			}

			return true;
		}

		/// <summary>
		/// Reads the centre, which is the last thing the object writes inline.
		/// </summary>
		private bool readCircle(byte[] payload)
		{
			long offset = this.StringStreamOffset - CenterBits;

			if (offset <= 0 || this.StringStreamOffset >= (long)payload.Length * 8L)
			{
				this.Reason = "the object is too short to hold the point it is placed at";
				return false;
			}

			this.Center = TchFieldMap.ReadInlineRawPosition(payload, offset);

			if (!isCoordinate(this.Center.X) || !isCoordinate(this.Center.Y))
			{
				this.Reason = "the values before the string stream are not a position";
				return false;
			}

			return true;
		}

		/// <summary>
		/// Reads the class's own fields, in the order it declares them.
		/// </summary>
		/// <remarks>
		/// A value and a string sit in the same stream, and which is which is a matter of shape: a
		/// number goes through the obfuscation layer and its characters say so, and everything else
		/// is text. The class writes both in one order, so the entry's place says what it is and the
		/// shape only says how to read it.
		/// </remarks>
		private bool readEntries(byte[] payload)
		{
			TchStreamReader reader = TchFieldMap.SkipPropertyBlock(payload, this.StringStreamOffset, 0);

			for (int i = 0; i < EntryCount; i++)
			{
				string entry;

				try
				{
					entry = reader.ReadCipherString();
				}
				catch (InvalidOperationException)
				{
					this.Reason = "the object's stream ends before its fields do, so they cannot be placed";
					return false;
				}

				if (TgDataEncrypt.IsCiphertext(entry))
				{
					this._entries[i].IsNumber = true;
					this._entries[i].Number = TgDataEncrypt.TryDecode(entry, out double value, TgDataEncrypt.ObjectXor)
						? value
						: double.NaN;
				}
				else
				{
					//A string the class writes plainly is text even when it reads as a number: the
					//number inside the circle is the character 1, and taking it for a value would
					//leave the label it is empty and the count of values one short.
					this._entries[i].Text = entry;
				}
			}

			if (double.IsNaN(this._entries[RadiusMultipleIndex].Number)
				|| double.IsNaN(this._entries[ScaleXDistanceIndex].Number)
				|| double.IsNaN(this._entries[ScaleTextHeightIndex].Number))
			{
				this.Reason = "the object's circle and its scale's offsets do not read";
				return false;
			}

			return true;
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

		private static bool isCoordinate(double value)
		{
			return !double.IsNaN(value) && !double.IsInfinity(value) && Math.Abs(value) < 1e8;
		}
	}
}
