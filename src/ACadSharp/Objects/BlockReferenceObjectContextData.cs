using ACadSharp.Attributes;
using CSMath;
using System;

namespace ACadSharp.Objects;

/// <summary>
/// Represents object context data for a block reference entity, including insertion point, rotation, and scale factors
/// in world coordinates.
/// </summary>
/// <remarks>This type provides context-specific information for block references, such as position and
/// transformation parameters, which are used when applying annotation scaling or other context-dependent behaviors. It
/// is typically used in scenarios where block references must adapt to different annotation scales or contexts within a
/// drawing. All scale factor properties must be non-zero; setting a scale factor to zero will result in an
/// exception.</remarks>
[DxfName(DxfFileToken.BlkRefObjectContextData)]
[DxfSubClass(DxfSubclassMarker.AnnotScaleObjectContextData)]
public class BlockReferenceObjectContextData : AnnotScaleObjectContextData
{
	/// <summary>
	/// Gets or sets the insertion point of the entity in world coordinates.
	/// </summary>
	[DxfCodeValue(10, 20, 30)]
	public XYZ InsertionPoint { get; set; }

	/// <inheritdoc/>
	public override string ObjectName => DxfFileToken.BlkRefObjectContextData;

	/// <summary>
	/// Gets or sets the rotation angle of the entity, in degrees.
	/// </summary>
	[DxfCodeValue(DxfReferenceType.IsAngle, 50)]
	public double Rotation { get; set; }

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

	private double _xscale = 1;

	private double _yscale = 1;

	private double _zscale = 1;
}