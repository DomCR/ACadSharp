using ACadSharp.Attributes;
using ACadSharp.Objects;
using CSMath;
using System;
using System.Collections.Generic;

namespace ACadSharp.Entities
{
	/// <summary>
	/// Common base class for all underlay entities, like <see cref="PdfUnderlay" />.
	/// </summary>
	[DxfSubClass(null, true)]
	public abstract class UnderlayEntity : Entity
	{
		/// <summary>
		/// Contrast
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

		[DxfCodeValue(DxfReferenceType.Handle, 340)]
		public UnderlayDefinition Definition { get; set; }

		/// <summary>
		/// Fade
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
		public UnderlayDisplayFlags Flags { get; set; }

		/// <summary>
		/// Insertion point(in WCS)
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

		private byte _contrast = 50;

		private byte _fade = 0;

		private double _xscale = 1;

		private double _yscale = 1;

		private double _zscale = 1;

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

			int sign = Math.Sign(transformation.m00 * transformation.m11 * transformation.m22) < 0 ? -1 : 1;

			double newRotation = (sign * newUvector).GetAngle();

			this.InsertPoint = newPosition;
			this.Normal = newNormal;
			this.Rotation = newRotation;
			this.XScale = transform.Scale.X * this.XScale;
			this.YScale = transform.Scale.Y * this.YScale;
			this.ZScale = transform.Scale.Z * this.ZScale;
		}

		/// <inheritdoc/>
		public override BoundingBox GetBoundingBox()
		{
			return BoundingBox.Null;
		}
	}
}