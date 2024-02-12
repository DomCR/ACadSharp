using ACadSharp.Attributes;
using ACadSharp.Tables;
using CSMath;
using System;

namespace ACadSharp.Entities
{
	/// <summary>
	/// Represents a <see cref="TextEntity"/>
	/// </summary>
	/// <remarks>
	/// Object name <see cref="DxfFileToken.EntityText"/> <br/>
	/// Dxf class name <see cref="DxfSubclassMarker.Text"/>
	/// </remarks>
	[DxfName(DxfFileToken.EntityText)]
	[DxfSubClass(DxfSubclassMarker.Text)]
	public class TextEntity : Entity
	{
		/// <inheritdoc/>
		public override ObjectType ObjectType => ObjectType.TEXT;

		/// <inheritdoc/>
		public override string ObjectName => DxfFileToken.EntityText;

		/// <inheritdoc/>
		public override string SubclassMarker => DxfSubclassMarker.Text;

		/// <summary>
		/// Specifies the distance a 2D object is extruded above or below its elevation.
		/// </summary>
		[DxfCodeValue(39)]
		public double Thickness { get; set; } = 0.0;

		/// <summary>
		/// First alignment point(in OCS)
		/// </summary>
		[DxfCodeValue(10, 20, 30)]
		public XYZ InsertPoint { get; set; } = XYZ.Zero;

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
		/// Specifies the text string for the entity.
		/// </summary>
		/// <value>
		/// The maximum length is 256 characters.
		/// </value>
		[DxfCodeValue(1)]
		public string Value
		{
			get
			{
				return _value;
			}
			set
			{
				if (value.Length > 256)
					throw new ArgumentException($"Text length cannot be supiror than 256, current: {value.Length}");
				else
					this._value = value;
			}
		}

		/// <summary>
		/// Specifies the rotation angle for the object.
		/// </summary>
		/// <value>
		/// The rotation angle in radians.
		/// </value>
		[DxfCodeValue(DxfReferenceType.IsAngle, 50)]
		public double Rotation { get; set; }

		/// <summary>
		/// Relative X scale factor—widt
		/// </summary>
		/// <remarks>
		/// This value is also adjusted when fit-type text is used (optional)
		/// </remarks>
		[DxfCodeValue(41)]
		public double WidthFactor { get; set; } = 1.0;

		/// <summary>
		/// Specifies the oblique angle of the object.
		/// </summary>
		/// <value>
		/// The angle in radians within the range of -85 to +85 degrees. A positive angle denotes a lean to the right; a negative value will have 2*PI added to it to convert it to its positive equivalent.
		/// </value>
		[DxfCodeValue(DxfReferenceType.IsAngle, 51)]
		public double ObliqueAngle { get; set; } = 0.0;

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
		/// Mirror flags.
		/// </summary>
		[DxfCodeValue(71)]
		public TextMirrorFlag Mirror { get; set; } = TextMirrorFlag.None;

		/// <summary>
		/// Horizontal text justification type.
		/// </summary>
		[DxfCodeValue(72)]
		public TextHorizontalAlignment HorizontalAlignment { get; set; } = TextHorizontalAlignment.Left;

		/// <summary>
		/// Second alignment point (in OCS) 
		/// </summary>
		/// <remarks>
		/// This value is meaningful only if the value of a 72 or 73 group is nonzero (if the justification is anything other than baseline/left)
		/// </remarks>
		[DxfCodeValue(DxfReferenceType.Optional, 11, 21, 31)]
		public XYZ AlignmentPoint { get; set; }

		/// <summary>
		/// Specifies the three-dimensional normal unit vector for the object.
		/// </summary>
		[DxfCodeValue(210, 220, 230)]
		public XYZ Normal { get; set; } = XYZ.AxisZ;

		/// <summary>
		/// Vertical text justification type.
		/// </summary>
		[DxfCodeValue(DxfReferenceType.Optional, 73)]
		public virtual TextVerticalAlignmentType VerticalAlignment { get; set; } = TextVerticalAlignmentType.Baseline;

		private string _value = string.Empty;

		private double _height = 0.0;

		private TextStyle _style = TextStyle.Default;

		public TextEntity() : base() { }

		public override CadObject Clone()
		{
			TextEntity clone = (TextEntity)base.Clone();
			clone.Style = (TextStyle)this.Style.Clone();
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
