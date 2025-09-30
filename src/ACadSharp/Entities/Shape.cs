using ACadSharp.Attributes;
using ACadSharp.Tables;
using CSMath;
using System;

namespace ACadSharp.Entities
{
	/// <summary>
	/// Represents a <see cref="Shape"/> entity.
	/// </summary>
	/// <remarks>
	/// Object name <see cref="DxfFileToken.EntityShape"/> <br/>
	/// Dxf class name <see cref="DxfSubclassMarker.Shape"/>
	/// </remarks>
	[DxfName(DxfFileToken.EntityShape)]
	[DxfSubClass(DxfSubclassMarker.Shape)]
	public class Shape : Entity
	{
		/// <inheritdoc/>
		public override ObjectType ObjectType => ObjectType.SHAPE;

		/// <inheritdoc/>
		public override string ObjectName => DxfFileToken.EntityShape;

		/// <inheritdoc/>
		public override string SubclassMarker => DxfSubclassMarker.Shape;

		/// <summary>
		/// Thickness.
		/// </summary>
		[DxfCodeValue(39)]
		public double Thickness { get; set; } = 0.0;

		/// <summary>
		/// Insertion point (in WCS).
		/// </summary>
		[DxfCodeValue(10, 20, 30)]
		public XYZ InsertionPoint { get; set; }

		/// <summary>
		/// Size.
		/// </summary>
		[DxfCodeValue(40)]
		public double Size { get; set; } = 1.0;

		/// <summary>
		/// Shape name.
		/// </summary>
		[DxfCodeValue(DxfReferenceType.Name, 2)]
		public TextStyle ShapeStyle
		{
			get { return this._style; }
			set
			{
				if (value == null || !value.IsShapeFile)
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

		/// <summary>
		/// Rotation angle.
		/// </summary>
		[DxfCodeValue(DxfReferenceType.IsAngle, 50)]
		public double Rotation { get; set; } = 0;

		/// <summary>
		/// Relative X scale factor.
		/// </summary>
		[DxfCodeValue(41)]
		public double RelativeXScale { get; set; } = 1;

		/// <summary>
		/// Oblique angle.
		/// </summary>
		[DxfCodeValue(DxfReferenceType.IsAngle, 51)]
		public double ObliqueAngle { get; set; } = 0;

		/// <summary>
		/// Extrusion direction.
		/// </summary>
		[DxfCodeValue(210, 220, 230)]
		public XYZ Normal { get; set; } = XYZ.AxisZ;

		internal ushort ShapeIndex { get; set; }

		private TextStyle _style;

		internal Shape() : base() { }

		/// <summary>
		/// Initializes a shape by the <see cref="TextStyle"/>
		/// </summary>
		/// <param name="textStyle">Text style with the flag <see cref="TextStyle.IsShapeFile"/></param>
		public Shape(TextStyle textStyle)
		{
			this.ShapeStyle = textStyle;
		}

		/// <inheritdoc/>
		public override CadObject Clone()
		{
			Shape clone = (Shape)base.Clone();

			clone.ShapeStyle = (TextStyle)(this.ShapeStyle?.Clone());

			return clone;
		}

		/// <inheritdoc/>
		public override BoundingBox GetBoundingBox()
		{
			return new BoundingBox(this.InsertionPoint);
		}

		/// <inheritdoc/>
		public override void ApplyTransform(Transform transform)
		{
			this.Normal = this.transformNormal(transform, this.Normal);
			this.InsertionPoint = transform.ApplyTranslation(this.InsertionPoint);
		}
	}
}
