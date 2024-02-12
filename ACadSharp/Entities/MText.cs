using ACadSharp.Attributes;
using ACadSharp.Tables;
using CSMath;
using System;

namespace ACadSharp.Entities
{
	/// <summary>
	/// Represents a <see cref="MText"/> entity.
	/// </summary>
	/// <remarks>
	/// Object name <see cref="DxfFileToken.EntityMText"/> <br/>
	/// Dxf class name <see cref="DxfSubclassMarker.MText"/>
	/// </remarks>
	[DxfName(DxfFileToken.EntityMText)]
	[DxfSubClass(DxfSubclassMarker.MText)]
	public partial class MText : Entity
	{
		/// <inheritdoc/>
		public override ObjectType ObjectType => ObjectType.MTEXT;

		/// <inheritdoc/>
		public override string ObjectName => DxfFileToken.EntityMText;

		/// <inheritdoc/>
		public override string SubclassMarker => DxfSubclassMarker.MText;

		/// <summary>
		/// A 3D WCS coordinate representing the insertion or origin point.
		/// </summary>
		[DxfCodeValue(10, 20, 30)]
		public XYZ InsertPoint { get; set; } = XYZ.Zero;

		/// <summary>
		/// Specifies the three-dimensional normal unit vector for the object.
		/// </summary>
		[DxfCodeValue(210, 220, 230)]
		public XYZ Normal { get; set; } = XYZ.AxisZ;

		/// <summary>
		/// Changes the height of the object.
		/// </summary>
		/// <value>
		/// This must be a positive, non-negative number.
		/// </value>
		[DxfCodeValue(40)]
		public double Height
		{
			get => _height; set
			{
				if (value < 0)
					throw new ArgumentOutOfRangeException("Height value cannot be negative.");
				else
					this._height = value;
			}
		}

		/// <summary>
		/// Reference rectangle width
		/// </summary>
		[DxfCodeValue(41)]
		public double RectangleWidth { get; set; }

		/// <summary>
		/// Reference rectangle height
		/// </summary>
		[DxfCodeValue(46)]
		public double RectangleHeight { get; set; }

		/// <summary>
		/// Attachment point
		/// </summary>
		[DxfCodeValue(71)]
		public AttachmentPointType AttachmentPoint { get; set; } = AttachmentPointType.TopLeft;

		/// <summary>
		/// Drawing direction
		/// </summary>
		[DxfCodeValue(72)]
		public DrawingDirectionType DrawingDirection { get; set; }

		/// <summary>
		/// Specifies the text string for the entity
		/// </summary>
		[DxfCodeValue(1)]
		public string Value { get; set; } = string.Empty;

		/// <summary>
		/// Style of this text entity.
		/// </summary>
		[DxfCodeValue(DxfReferenceType.Name | DxfReferenceType.Optional, 7)]
		public TextStyle Style
		{
			get { return this._style; }
			set
			{
				if (value == null)
				{
					throw new ArgumentNullException(nameof(value));
				}

				if (this.Document != null)
				{
					this._style = this.updateTable(value, this.Document.TextStyles);
				}
				else
				{
					this._style = value;
				}
			}
		}

		/// <summary>
		/// X-axis direction vector(in WCS)
		/// </summary>
		/// <remarks>
		/// A group code 50 (rotation angle in radians) passed as DXF input is converted to the equivalent direction vector (if both a code 50 and codes 11, 21, 31 are passed, the last one wins). This is provided as a convenience for conversions from text objects
		/// </remarks>
		[DxfCodeValue(11, 21, 31)]
		public XYZ AlignmentPoint
		{
			get => _alignmentPoint;
			set
			{
				_alignmentPoint = value;
				this._rotation = new XY(this._alignmentPoint.X, this._alignmentPoint.Y).GetAngle();
			}
		}

		/// <summary>
		/// Horizontal width of the characters that make up the mtext entity.
		/// This value will always be equal to or less than the value of group code 41 
		/// </summary>
		/// <remarks>
		/// read-only, ignored if supplied
		/// </remarks>
		[DxfCodeValue(DxfReferenceType.Ignored, 42)]
		public double HorizontalWidth { get; set; }

		/// <summary>
		/// Horizontal width of the characters that make up the mtext entity.
		/// This value will always be equal to or less than the value of group code 41 
		/// </summary>
		/// <remarks>
		/// read-only, ignored if supplied
		/// </remarks>
		[DxfCodeValue(DxfReferenceType.Ignored, 43)]
		public double VerticalWidth { get; set; }

		/// <summary>
		/// Specifies the rotation angle for the object.
		/// </summary>
		/// <value>
		/// The rotation angle in radians.
		/// </value>
		[DxfCodeValue(DxfReferenceType.IsAngle, 50)]
		public double Rotation
		{
			get => _rotation;
			set
			{
				_rotation = value;
				this.AlignmentPoint = new XYZ(Math.Cos(_rotation), Math.Sin(_rotation), 0.0);
			}
		}

		/// <summary>
		/// Mtext line spacing style 
		/// </summary>
		[DxfCodeValue(73)]
		public LineSpacingStyleType LineSpacingStyle { get; set; }

		/// <summary>
		/// Mtext line spacing factor 
		/// </summary>
		/// <remarks>
		/// Percentage of default (3-on-5) line spacing to be applied.Valid values range from 0.25 to 4.00
		/// </remarks>
		[DxfCodeValue(44)]
		public double LineSpacing { get; set; } = 1.0;

		/// <summary>
		/// Background fill setting
		/// </summary>
		[DxfCodeValue(90)]
		public BackgroundFillFlags BackgroundFillFlags { get; set; } = BackgroundFillFlags.None;

		/// <summary>
		/// Determines how much border there is around the text.
		/// </summary>
		[DxfCodeValue(45)]
		public double BackgroundScale { get; set; } = 1.5;

		/// <summary>
		/// Background fill color 
		/// </summary>
		/// <remarks>
		/// Color to use for background fill when group code 90 is 1.
		/// </remarks>
		[DxfCodeValue(63, 420, 430)]
		public Color BackgroundColor { get; set; }

		/// <summary>
		/// Transparency of background fill color
		/// </summary>
		[DxfCodeValue(441)]
		public Transparency BackgroundTransparency { get; set; }

		public TextColumn Column { get; set; } = new TextColumn();

		public bool IsAnnotative { get; set; } = false;

		private double _height = 1.0;

		private XYZ _alignmentPoint = XYZ.AxisX;

		private double _rotation = 0.0;

		private TextStyle _style = TextStyle.Default;

		public MText() : base() { }

		public override CadObject Clone()
		{
			MText clone = (MText)base.Clone();

			clone.Style = (TextStyle)(this.Style?.Clone());
			clone.Column = this.Column?.Clone();

			return clone;
		}

		internal override void AssignDocument(CadDocument doc)
		{
			base.AssignDocument(doc);

			this._style = this.updateTable(this.Style, doc.TextStyles);

			doc.DimensionStyles.OnRemove += this.tableOnRemove;
		}

		internal override void UnassignDocument()
		{
			this.Document.DimensionStyles.OnRemove -= this.tableOnRemove;

			base.UnassignDocument();

			this.Style = (TextStyle)this.Style.Clone();
		}

		protected override void tableOnRemove(object sender, CollectionChangedEventArgs e)
		{
			base.tableOnRemove(sender, e);

			if (e.Item.Equals(this.Style))
			{
				this.Style = this.Document.TextStyles[TextStyle.DefaultName];
			}
		}
	}
}