using ACadSharp.Classes;
using ACadSharp.IO.DWG.Tangent;
using CSMath;
using System;
using System.Collections.Generic;
using System.Globalization;

namespace ACadSharp.Entities.Tangent
{
	/// <summary>
	/// An elevation marker, as Tianzheng stores one.
	/// </summary>
	/// <remarks>
	/// <para>
	/// The marker is a filled triangle on a line with the level written beside it. The level is not a
	/// number the class stores as one: it is text, written plainly and not through the obfuscation
	/// layer, and it is what the reader has to find among the object's other strings.
	/// </para>
	/// <para>
	/// The triangle's proportions are fixed rather than stored: its height is half its base, so one
	/// length describes it. That single length is what the class records, and the rest follows.
	/// </para>
	/// </remarks>
	public sealed class TchElevation : TchEntity
	{
		/// <summary>
		/// The property that says which side of the marker the level is written on.
		/// </summary>
		public const string SideProperty = "标高注释在右侧";

		/// <summary>
		/// How many strings in the stream belong to readers above <see cref="TchElevation"/>.
		/// </summary>
		

		/// <summary>
		/// Gets where the marker's point sits.
		/// </summary>
		public XYZ Position { get; private set; }

		/// <summary>
		/// Gets or sets the width of the triangle's base.
		/// </summary>
		/// <remarks>
		/// Not a field: the object does not store its marker's size, and every marker in the sample
		/// is the same size however large the levels they record. The size belongs to the drawing's
		/// annotation settings, along with the text height, and is set here rather than invented.
		/// </remarks>
		public double BaseWidth { get; set; } = 600.0;

		/// <summary>
		/// Gets or sets the height of the level's text.
		/// </summary>
		/// <inheritdoc cref="BaseWidth"/>
		public double TextHeight { get; set; } = 297.5;

		/// <summary>
		/// Gets or sets how far above the marker's point the text sits.
		/// </summary>
		/// <inheritdoc cref="BaseWidth"/>
		public double TextRise { get; set; } = 405.0;

		/// <summary>
		/// Gets or sets the offset of the text along the marker.
		/// </summary>
		/// <inheritdoc cref="BaseWidth"/>
		public double TextOffset { get; set; } = 178.0;

		/// <summary>
		/// Gets the level, as the object writes it.
		/// </summary>
		/// <remarks>
		/// Kept as text rather than parsed: it is what the drawing shows, and converting it to a
		/// number would lose how many places it is written to.
		/// </remarks>
		public string Level { get; private set; }

		/// <summary>
		/// Gets or sets whether the level is written to the right of the marker.
		/// </summary>
		/// <remarks>
		/// The object records this as a property, by name and with no value, which is how a flag is
		/// written. Every marker in the sample is right handed, so that is the default; the property
		/// says so when the reader has found it.
		/// </remarks>
		public bool IsRightHanded { get; set; } = true;

		/// <summary>
		/// Gets the height of the triangle, which is half its base.
		/// </summary>
		public double MarkerHeight => this.BaseWidth / 2.0;

		/// <summary>
		/// Reads a marker from the bytes a class's own fields were written to.
		/// </summary>
		public static TchElevation Parse(DxfClass dxfClass, byte[] payload, ACadVersion version)
		{
			TchElevation elevation = new TchElevation(dxfClass);

			elevation.StringStreamOffset = FindStringStream(payload, payload.Length * 8L);
			elevation.Properties = TchDataMap.ReadFromPayload(payload, version);
			elevation.Fill(payload, version);

			return elevation;
		}

		/// <inheritdoc/>
		public override IEnumerable<Entity> ToStandardEntities()
		{
			//The triangle stands on its base with the point below: two sides from the point up to the
			//ends of the base, and the base itself across the top.
			XY point = new XY(this.Position.X, this.Position.Y);
			double half = this.BaseWidth / 2.0;
			double height = this.MarkerHeight;

			XY left = new XY(point.X - half, point.Y + height);
			XY right = new XY(point.X + half, point.Y + height);

			yield return new Line { StartPoint = new XYZ(point.X, point.Y, this.Position.Z), EndPoint = new XYZ(left.X, left.Y, this.Position.Z) };
			yield return new Line { StartPoint = new XYZ(point.X, point.Y, this.Position.Z), EndPoint = new XYZ(right.X, right.Y, this.Position.Z) };
			yield return new Line { StartPoint = new XYZ(left.X, left.Y, this.Position.Z), EndPoint = new XYZ(right.X, right.Y, this.Position.Z) };

			if (!string.IsNullOrEmpty(this.Level))
			{
				//The level is written beside the marker, on the side the object says, and a little
				//above the base.
				//Both offsets are measured from the marker's point, not from its base: the text sits
				//beside the triangle rather than above it, and adding the marker's height as well puts
				//it one triangle too high.
				double offset = this.IsRightHanded
					? Math.Abs(this.TextOffset)
					: -Math.Abs(this.TextOffset);

				yield return new TextEntity
				{
					Value = this.Level,
					Height = this.TextHeight,
					InsertPoint = new XYZ(point.X + offset, point.Y + this.TextRise, this.Position.Z),
				};
			}
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
				return;
			}

			TchStreamReader reader = new TchStreamReader(payload, this.StringStreamOffset);

			this.Position = TchFieldMap.ReadInlineRawPosition(payload, 78L);

			//The level is text among the numbers, so it is found by shape. It is also the only text in
			//the class's own fields, which is why nothing more specific is needed to identify it.
			this.Level = TchFieldMap.FindLevel(payload, this.StringStreamOffset);
			

			for (int i = 0; i < TchFieldMap.ElevationSkippedStrings; i++)
			{
				reader = TchFieldMap.SkipString(reader);
			}

			//The size the object stores as its own is not the marker's: it is the level's height in
			//the drawing, which is zero for every marker here. What the marker is drawn at comes from
			//the annotation settings, so it is left to them.
			_ = TchFieldMap.ReadDouble(reader, TgDataEncrypt.ObjectXor);

						if (this.Properties != null && this.Properties.Properties.ContainsKey(SideProperty))
			{
				this.IsRightHanded = true;
			}
		}

		/// <summary>
		/// Initialises a marker from the class it was read as.
		/// </summary>
		internal TchElevation(DxfClass dxfClass)
			: base(dxfClass)
		{
		}
	}
}
