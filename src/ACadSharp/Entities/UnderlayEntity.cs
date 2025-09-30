using ACadSharp.Attributes;
using ACadSharp.Extensions;
using ACadSharp.Objects;
using ACadSharp.Objects.Collections;
using CSMath;
using System;
using System.Collections.Generic;

namespace ACadSharp.Entities
{
	/// <summary>
	/// Common base class for all underlay entities, like <see cref="PdfUnderlay" />.
	/// </summary>
	[DxfSubClass(null, true)]
	public abstract class UnderlayEntity<T> : Entity
		where T : UnderlayDefinition
	{
		/// <summary>
		/// 2d points in OCS/ECS.
		/// </summary>
		/// <remarks>
		/// If only two, then they are the lower left and upper right corner points of a clip rectangle. <br/>
		/// If more than two, then they are the vertices of a clipping polygon.
		/// </remarks>
		[DxfCollectionCodeValue(11, 21)]
		public List<XY> ClipBoundaryVertices { get; set; } = new List<XY>();

		/// <summary>
		/// Contrast.
		/// </summary>
		/// <remarks>
		/// 0-100; default = 50
		/// </remarks>
		[DxfCodeValue(281)]
		public byte Contrast
		{
			get { return this._contrast; }
			set
			{
				if (value < 0 || value > 100)
				{
					throw new ArgumentException($"Invalid Brightness value: {value}, must be in range 0-100");
				}

				this._contrast = value;
			}
		}

		/// <summary>
		/// The AcDbUnderlayDefinition object.
		/// </summary>
		[DxfCodeValue(DxfReferenceType.Handle, 340)]
		public T Definition
		{
			get { return this._definition; }
			set
			{
				if (value == null)
				{
					throw new ArgumentNullException(nameof(value));
				}

				if (this.Document != null)
				{
					this._definition = updateCollection(value, this.getDocumentCollection(this.Document));
				}
				else
				{
					this._definition = value;
				}
			}
		}

		/// <summary>
		/// Fade.
		/// </summary>
		/// <value>
		/// Range: 0 - 100 <br/>
		/// Default: 0
		/// </value>
		[DxfCodeValue(282)]
		public byte Fade
		{
			get { return this._fade; }
			set
			{
				if (value < 0 || value > 100)
				{
					throw new ArgumentException($"Invalid Brightness value: {value}, must be in range 0-100");
				}

				this._fade = value;
			}
		}

		/// <summary>
		/// Underlay display options.
		/// </summary>
		[DxfCodeValue(280)]
		public UnderlayDisplayFlags Flags { get; set; } = UnderlayDisplayFlags.Default;

		/// <summary>
		/// Insertion point(in WCS).
		/// </summary>
		[DxfCodeValue(10, 20, 30)]
		public XYZ InsertPoint { get; set; }

		/// <summary>
		/// Specifies the three-dimensional normal unit vector for the object.
		/// </summary>
		[DxfCodeValue(210, 220, 230)]
		public XYZ Normal { get; set; } = XYZ.AxisZ;

		/// <summary>
		/// Specifies the rotation angle for the object.
		/// </summary>
		/// <value>
		/// The rotation angle in radians.
		/// </value>
		[DxfCodeValue(DxfReferenceType.IsAngle, 50)]
		public double Rotation { get; set; } = 0.0;

		/// <inheritdoc/>
		public override string SubclassMarker => DxfSubclassMarker.Underlay;

		/// <summary>
		/// X scale factor.
		/// </summary>
		[DxfCodeValue(41)]
		public double XScale
		{
			get
			{
				return this._xscale;
			}
			set
			{
				if (value.Equals(0))
				{
					string name = nameof(this.XScale);
					throw new ArgumentOutOfRangeException(name, value, $"{name} value must be none zero.");
				}
				this._xscale = value;
			}
		}

		/// <summary>
		/// Y scale factor.
		/// </summary>
		[DxfCodeValue(42)]
		public double YScale
		{
			get
			{
				return this._yscale;
			}
			set
			{
				if (value.Equals(0))
				{
					string name = nameof(this.YScale);
					throw new ArgumentOutOfRangeException(name, value, $"{name} value must be none zero.");
				}
				this._yscale = value;
			}
		}

		/// <summary>
		/// Z scale factor.
		/// </summary>
		[DxfCodeValue(43)]
		public double ZScale
		{
			get
			{
				return this._zscale;
			}
			set
			{
				if (value.Equals(0))
				{
					string name = nameof(this.ZScale);
					throw new ArgumentOutOfRangeException(name, value, $"{name} value must be none zero.");
				}
				this._zscale = value;
			}
		}

		private byte _contrast = 100;

		private T _definition;

		private byte _fade = 0;

		private double _xscale = 1;

		private double _yscale = 1;

		private double _zscale = 1;

		/// <summary>
		/// Initializes a new instance of the <see cref="UnderlayEntity{T}" /> class.
		/// </summary>
		/// <param name="definition"></param>
		public UnderlayEntity(T definition)
		{
			this.Definition = definition;
		}

		internal UnderlayEntity()
		{
		}

		/// <inheritdoc/>
		public override void ApplyTransform(Transform transform)
		{
			XYZ newPosition = transform.ApplyTransform(this.InsertPoint);
			XYZ newNormal = this.transformNormal(transform, Normal);

			var transformation = getWorldMatrix(transform, newNormal, this.Normal, out Matrix3 transOW, out Matrix3 transWO);

			List<XY> uv = applyRotation(
				new[]
				{
					XY.AxisX, XY.AxisY
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

			int sign = Math.Sign(transformation.M00 * transformation.M11 * transformation.M22) < 0 ? -1 : 1;

			double newRotation = (sign * newUvector).GetAngle();

			this.InsertPoint = newPosition;
			this.Normal = newNormal;
			this.Rotation = newRotation;
			this.XScale = transform.Scale.X * this.XScale;
			this.YScale = transform.Scale.Y * this.YScale;
			this.ZScale = transform.Scale.Z * this.ZScale;
		}

		/// <inheritdoc/>
		public override CadObject Clone()
		{
			UnderlayEntity<T> clone = (UnderlayEntity<T>)base.Clone();

			clone.Definition = this.Definition?.CloneTyped();
			clone.ClipBoundaryVertices = new List<XY>(this.ClipBoundaryVertices);

			return clone;
		}

		/// <inheritdoc/>
		public override BoundingBox GetBoundingBox()
		{
			return BoundingBox.Null;
		}

		internal override void AssignDocument(CadDocument doc)
		{
			base.AssignDocument(doc);

			this._definition = updateCollection(this.Definition, getDocumentCollection(doc));

			this.Document.PdfDefinitions.OnRemove += this.imageDefinitionsOnRemove;
		}

		internal override void UnassignDocument()
		{
			this.Document.ImageDefinitions.OnRemove -= this.imageDefinitionsOnRemove;

			base.UnassignDocument();

			this.Definition = (T)this.Definition?.Clone();
		}

		protected abstract ObjectDictionaryCollection<T> getDocumentCollection(CadDocument document);

		private void imageDefinitionsOnRemove(object sender, CollectionChangedEventArgs e)
		{
			if (e.Item.Equals(this.Definition))
			{
				this.Definition = null;
			}
		}
	}
}