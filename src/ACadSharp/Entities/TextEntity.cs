using ACadSharp.Attributes;
using ACadSharp.Tables;
using CSMath;
using CSUtilities.Extensions;
using System;
using System.Collections.Generic;

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
	public class TextEntity : Entity, IText
	{
		/// <summary>
		/// Second alignment point (in OCS)
		/// </summary>
		/// <remarks>
		/// This value is meaningful only if the value of a 72 or 73 group is nonzero (if the justification is anything other than baseline/left)
		/// </remarks>
		[DxfCodeValue(DxfReferenceType.Optional, 11, 21, 31)]
		public XYZ AlignmentPoint { get; set; }

		/// <inheritdoc/>
		[DxfCodeValue(40)]
		public double Height
		{
			get => _height;
			set
			{
				if (value < 0)
					throw new ArgumentOutOfRangeException("Height value cannot be negative.");
				else
					this._height = value;
			}
		}

		/// <summary>
		/// Horizontal text justification type.
		/// </summary>
		[DxfCodeValue(72)]
		public TextHorizontalAlignment HorizontalAlignment { get; set; } = TextHorizontalAlignment.Left;

		/// <summary>
		/// First alignment point(in OCS)
		/// </summary>
		[DxfCodeValue(10, 20, 30)]
		public XYZ InsertPoint { get; set; } = XYZ.Zero;

		/// <summary>
		/// Mirror flags.
		/// </summary>
		[DxfCodeValue(71)]
		public TextMirrorFlag Mirror { get => _mirror; set => _mirror = value; }
		/// <summary>
		/// Specifies the three-dimensional normal unit vector for the object.
		/// </summary>
		[DxfCodeValue(210, 220, 230)]
		public XYZ Normal { get; set; } = XYZ.AxisZ;

		/// <inheritdoc/>
		public override string ObjectName => DxfFileToken.EntityText;

		/// <inheritdoc/>
		public override ObjectType ObjectType => ObjectType.TEXT;

		/// <summary>
		/// Specifies the oblique angle of the object.
		/// </summary>
		/// <value>
		/// The angle in radians within the range of -85 to +85 degrees. A positive angle denotes a lean to the right; a negative value will have 2*PI added to it to convert it to its positive equivalent.
		/// </value>
		[DxfCodeValue(DxfReferenceType.IsAngle, 51)]
		public double ObliqueAngle { get; set; } = 0.0;

		/// <inheritdoc/>
		[DxfCodeValue(DxfReferenceType.IsAngle, 50)]
		public double Rotation { get; set; }

		/// <inheritdoc/>
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
					this._style = CadObject.updateCollection(value, this.Document.TextStyles);
				}
				else
				{
					this._style = value;
				}
			}
		}

		/// <inheritdoc/>
		public override string SubclassMarker => DxfSubclassMarker.Text;

		/// <summary>
		/// Specifies the distance a 2D object is extruded above or below its elevation.
		/// </summary>
		[DxfCodeValue(39)]
		public double Thickness { get; set; } = 0.0;

		/// <inheritdoc/>
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
		/// Vertical text justification type.
		/// </summary>
		[DxfCodeValue(DxfReferenceType.Optional, 73)]
		public virtual TextVerticalAlignmentType VerticalAlignment { get; set; } = TextVerticalAlignmentType.Baseline;

		/// <summary>
		/// Relative X scale factor—widt
		/// </summary>
		/// <remarks>
		/// This value is also adjusted when fit-type text is used
		/// </remarks>
		[DxfCodeValue(DxfReferenceType.Optional, 41)]
		public double WidthFactor { get; set; } = 1.0;

		private double _height = 0.0;
		private TextStyle _style = TextStyle.Default;
		private string _value = string.Empty;
		private TextMirrorFlag _mirror = TextMirrorFlag.None;

		public TextEntity() : base()
		{
		}

		/// <inheritdoc/>
		public override void ApplyTransform(Transform transform)
		{
			bool mirrText = this.Mirror.HasFlag(TextMirrorFlag.Backward);

			XYZ newInsert = transform.ApplyTransform(this.InsertPoint);
			XYZ newNormal = this.transformNormal(transform, this.Normal);

			var transformation = this.getWorldMatrix(transform, Normal, newNormal, out Matrix3 transOW, out Matrix3 transWO);

			List<XY> uv = applyRotation(
				new[]
				{
					this.WidthFactor * this.Height * XY.AxisX,
					new XY(this.Height * Math.Tan(this.ObliqueAngle), this.Height)
				},
				this.Rotation);

			XYZ v;
			v = transOW * new XYZ(uv[0].X, uv[0].Y, 0.0);
			v = transformation * v;
			v = transWO * v;
			XY newUvector = new XY(v.X, v.Y);

			v = transOW * new XYZ(uv[1].X, uv[1].Y, 0.0);
			v = transformation * v;
			v = transWO * v;
			XY newVvector = new XY(v.X, v.Y);

			double newRotation = newUvector.GetAngle();
			double newObliqueAngle = newVvector.GetAngle();

			if (mirrText)
			{
				if (XY.Cross(newUvector, newVvector) < 0)
				{
					newObliqueAngle = MathHelper.HalfPI - (newRotation - newObliqueAngle);
					if (!(this.HorizontalAlignment.HasFlag(TextHorizontalAlignment.Fit)
						|| this.HorizontalAlignment.HasFlag(TextHorizontalAlignment.Aligned)))
					{
						newRotation += Math.PI;
					}

					this._mirror.RemoveFlag(TextMirrorFlag.Backward);
				}
				else
				{
					newObliqueAngle = MathHelper.HalfPI + (newRotation - newObliqueAngle);
				}
			}
			else
			{
				if (XY.Cross(newUvector, newVvector) < 0.0)
				{
					newObliqueAngle = MathHelper.HalfPI - (newRotation - newObliqueAngle);

					if (newUvector.Dot(uv[0]) < 0.0)
					{
						newRotation += Math.PI;

						switch (this.HorizontalAlignment)
						{
							case TextHorizontalAlignment.Left:
								this.HorizontalAlignment = TextHorizontalAlignment.Right;
								break;
							case TextHorizontalAlignment.Right:
								this.HorizontalAlignment = TextHorizontalAlignment.Left;
								break;
						}
					}
					else
					{
						switch (this.VerticalAlignment)
						{
							case TextVerticalAlignmentType.Top:
								this.VerticalAlignment = TextVerticalAlignmentType.Bottom;
								break;
							case TextVerticalAlignmentType.Bottom:
								this.VerticalAlignment = TextVerticalAlignmentType.Top;
								break;
						}
					}
				}
				else
				{
					newObliqueAngle = MathHelper.HalfPI + (newRotation - newObliqueAngle);
				}
			}

			// the oblique angle is defined between -85 and 85 degrees
			double maxOblique = MathHelper.DegToRad(85);
			double minOblique = -maxOblique;
			if (newObliqueAngle > Math.PI)
			{
				newObliqueAngle = Math.PI - newObliqueAngle;
			}

			if (newObliqueAngle < minOblique)
			{
				newObliqueAngle = minOblique;
			}
			else if (newObliqueAngle > maxOblique)
			{
				newObliqueAngle = maxOblique;
			}

			// the height must be greater than zero, the cos is always positive between -85 and 85
			double newHeight = newVvector.GetLength() * Math.Cos(newObliqueAngle);
			newHeight = MathHelper.IsZero(newHeight) ? MathHelper.Epsilon : newHeight;

			// the width factor is defined between 0.01 and 100
			double newWidthFactor = newUvector.GetLength() / newHeight;
			if (newWidthFactor < 0.01)
			{
				newWidthFactor = 0.01;
			}
			else if (newWidthFactor > 100)
			{
				newWidthFactor = 100;
			}

			this.InsertPoint = newInsert;
			this.Normal = newNormal;
			this.Rotation = newRotation;
			this.Height = newHeight;
			this.WidthFactor = newWidthFactor;
			this.ObliqueAngle = newObliqueAngle;
		}

		/// <inheritdoc/>
		public override CadObject Clone()
		{
			TextEntity clone = (TextEntity)base.Clone();
			clone.Style = (TextStyle)this.Style.Clone();
			return clone;
		}

		/// <inheritdoc/>
		public override BoundingBox GetBoundingBox()
		{
			return new BoundingBox(this.InsertPoint);
		}

		internal override void AssignDocument(CadDocument doc)
		{
			base.AssignDocument(doc);

			this._style = CadObject.updateCollection(this.Style, doc.TextStyles);

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