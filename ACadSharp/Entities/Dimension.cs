using ACadSharp.Attributes;
using ACadSharp.Blocks;
using ACadSharp.Tables;
using CSMath;
using System;

namespace ACadSharp.Entities
{
	/// <summary>
	/// Represents a <see cref="Dimension"/> entity.
	/// </summary>
	/// <remarks>
	/// Object name <see cref="DxfFileToken.EntityDimension"/> <br/>
	/// Dxf class name <see cref="DxfSubclassMarker.Dimension"/>
	/// </remarks>
	[DxfName(DxfFileToken.EntityDimension)]
	[DxfSubClass(DxfSubclassMarker.Dimension)]
	public abstract class Dimension : Entity
	{
		/// <inheritdoc/>
		public override string SubclassMarker => DxfSubclassMarker.Dimension;

		/// <summary>
		/// Version number
		/// </summary>
		[DxfCodeValue(280)]
		public byte Version { get; set; }

		/// <summary>
		/// Block that contains the entities that make up the dimension picture
		/// </summary>
		[DxfCodeValue(DxfReferenceType.Name, 2)]
		public BlockRecord Block { get; set; }

		/// <summary>
		/// Definition point(in WCS)
		/// </summary>
		[DxfCodeValue(10, 20, 30)]
		public XYZ DefinitionPoint { get; set; }

		/// <summary>
		/// Middle point of dimension text(in OCS)
		/// </summary>
		[DxfCodeValue(11, 21, 31)]
		public XYZ TextMiddlePoint { get; set; }

		/// <summary>
		/// Insertion point for clones of a dimension-Baseline and Continue(in OCS)
		/// </summary>
		[DxfCodeValue(12, 22, 32)]
		public XYZ InsertionPoint { get; set; }

		/// <summary>
		/// Specifies the three-dimensional normal unit vector for the object.
		/// </summary>
		[DxfCodeValue(210, 220, 230)]
		public XYZ Normal { get; set; } = XYZ.AxisZ;

		/// <summary>
		/// Dimension type
		/// </summary>
		[DxfCodeValue(70)]
		public DimensionType Flags
		{
			get
			{
				var flags = this._flags | DimensionType.BlockReference;
				return flags;
			}
		}

		/// <summary>
		/// Attachment point
		/// </summary>
		[DxfCodeValue(71)]
		public AttachmentPointType AttachmentPoint { get; set; }

		/// <summary>
		/// Dimension text line-spacing style
		/// </summary>
		/// <remarks>
		/// optional
		/// </remarks>
		[DxfCodeValue(72)]
		public LineSpacingStyleType LineSpacingStyle { get; set; }

		/// <summary>
		/// Dimension text-line spacing factor
		/// </summary>
		/// <remarks>
		/// (optional) Percentage of default (3-on-5) line spacing to be applied.
		/// </remarks>
		/// <value>
		/// Valid values range from 0.25 to 4.00
		/// </value>
		[DxfCodeValue(41)]
		public double LineSpacingFactor { get; set; }

		/// <summary>
		/// Actual measurement
		/// </summary>
		/// <remarks>
		/// optional; read-only value
		/// </remarks>
		[DxfCodeValue(42)]
		public double Measurement { get; internal set; }

		/// <summary>
		/// Gets or sets a value indicating whether the first arrow
		/// is to be flipped.
		/// </summary>
		/// <value>
		/// <b>true</b> if the arrow is to be flipped; otherwise, <b>false</b>.
		/// </value>
		/// <remarks>
		/// Arrows are by default drawn inside the extension lines if there is enaugh
		/// space; otherwise, outside. This flag overrules the standard behaviour.
		/// </remarks>
		[DxfCodeValue(74)]
		public bool FlipArrow1 { get; set; }

		/// <summary>
		/// Gets or sets a value indicating whether the second arrow
		/// to be flipped.
		/// </summary>
		/// <value>
		/// <b>true</b> if the arrow is to be flipped; otherwise, <b>false</b>.
		/// </value>
		/// <remarks>
		/// Arrows are by default drawn inside the extension lines if there is enaugh
		/// space; otherwise, outside. This flag overrules the standard behaviour.
		/// </remarks>
		[DxfCodeValue(75)]
		public bool FlipArrow2 { get; set; }

		/// <summary>
		/// Dimension text explicitly entered by the user
		/// </summary>
		/// <remarks>
		/// Optional; default is the measurement.
		/// If null, the dimension measurement is drawn as the text, 
		/// if ““ (one blank space), the text is suppressed.Anything else is drawn as the text
		/// </remarks>
		[DxfCodeValue(DxfReferenceType.Optional, 1)]
		public string Text
		{
			get { return string.IsNullOrEmpty(_text) ? this.Measurement.ToString() : this._text; }
			set { this._text = value; }
		}

		/// <summary>
		/// rotation angle of the dimension text away from its default orientation (the direction of the dimension line)
		/// </summary>
		/// <remarks>
		/// Optional
		/// </remarks>
		[DxfCodeValue(DxfReferenceType.Optional | DxfReferenceType.IsAngle, 53)]
		public double TextRotation { get; set; }

		/// <summary>
		/// All dimension types have an optional 51 group code, which indicates the horizontal direction for the dimension entity.The dimension entity determines the orientation of dimension text and lines for horizontal, vertical, and rotated linear dimensions
		/// This group value is the negative of the angle between the OCS X axis and the UCS X axis. It is always in the XY plane of the OCS
		/// </summary>
		[DxfCodeValue(DxfReferenceType.Optional | DxfReferenceType.IsAngle, 51)]
		public double HorizontalDirection { get; set; }

		//This group value is the negative of the angle between the OCS X axis and the UCS X axis.It is always in the XY plane of the OCS

		/// <summary>
		/// Dimension style
		/// </summary>
		[DxfCodeValue(DxfReferenceType.Name, 3)]
		public DimensionStyle Style
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
					this._style = this.updateTable(value, this.Document.DimensionStyles);
				}
				else
				{
					this._style = value;
				}
			}
		}

		private string _text;

		private readonly DimensionType _flags;

		private DimensionStyle _style = DimensionStyle.Default;

		protected Dimension(DimensionType type)
		{
			this._flags = type;
		}

		public override CadObject Clone()
		{
			Dimension clone = (Dimension)base.Clone();

			clone.Style = (DimensionStyle)(this.Style.Clone());

			return clone;
		}

		internal override void AssignDocument(CadDocument doc)
		{
			base.AssignDocument(doc);

			this._style = this.updateTable(this.Style, doc.DimensionStyles);

			doc.DimensionStyles.OnRemove += this.tableOnRemove;
		}

		internal override void UnassignDocument()
		{
			this.Document.DimensionStyles.OnRemove -= this.tableOnRemove;

			base.UnassignDocument();

			this.Style = (DimensionStyle)this.Style.Clone();
		}

		protected override void tableOnRemove(object sender, CollectionChangedEventArgs e)
		{
			base.tableOnRemove(sender, e);

			if (e.Item.Equals(this.Style))
			{
				this.Style = this.Document.DimensionStyles[DimensionStyle.DefaultName];
			}
		}
	}
}
