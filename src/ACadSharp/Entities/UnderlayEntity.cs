using ACadSharp.Attributes;
using ACadSharp.Objects;
using CSMath;
using System;

namespace ACadSharp.Entities
{
	/// <summary>
	/// Common base class for all underlay entities, like <see cref="PdfUnderlay" />.
	/// </summary>
	[DxfSubClass(null, true)]
	public abstract class UnderlayEntity : Entity
	{
		/// <inheritdoc/>
		public override string SubclassMarker => DxfSubclassMarker.Underlay;

		/// <summary>
		/// Specifies the three-dimensional normal unit vector for the object.
		/// </summary>
		[DxfCodeValue(210, 220, 230)]
		public XYZ Normal { get; set; } = XYZ.AxisZ;

		/// <summary>
		/// Insertion point(in WCS)
		/// </summary>
		[DxfCodeValue(10, 20, 30)]
		public XYZ InsertPoint { get; set; }

		/// <summary>
		/// X scale factor 
		/// </summary>
		[DxfCodeValue(41)]
		public double XScale { get; set; } = 1;

		/// <summary>
		/// Y scale factor 
		/// </summary>
		[DxfCodeValue(42)]
		public double YScale { get; set; } = 1;

		/// <summary>
		/// Z scale factor 
		/// </summary>
		[DxfCodeValue(43)]
		public double ZScale { get; set; } = 1;

		/// <summary>
		/// Specifies the rotation angle for the object.
		/// </summary>
		/// <value>
		/// The rotation angle in radians.
		/// </value>
		[DxfCodeValue(DxfReferenceType.IsAngle, 50)]
		public double Rotation { get; set; } = 0.0;

		/// <summary>
		/// Underlay display options.
		/// </summary>
		[DxfCodeValue(280)]
		public UnderlayDisplayFlags Flags { get; set; }

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

		[DxfCodeValue(DxfReferenceType.Handle, 340)]
		public UnderlayDefinition Definition { get; set; }

		private byte _contrast = 50;
		private byte _fade = 0;

		/// <inheritdoc/>
		public override BoundingBox GetBoundingBox()
		{
			return BoundingBox.Null;
		}
	}
}
