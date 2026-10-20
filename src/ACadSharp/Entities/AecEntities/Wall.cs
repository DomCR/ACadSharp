using ACadSharp.Attributes;
using ACadSharp.Objects;
using ACadSharp.Objects.AEC;
using CSMath;
using CSMath.Extensions;
using System.Collections.Generic;

namespace ACadSharp.Entities.AecEntities;

/// <summary>
/// Represents a wall entity in an AEC context.
/// </summary>
/// <remarks>
/// AEC Wall entities can only be stored in DWG files, not in DXF files.
/// DXF files may contain the class definition but not the actual entity instances.
/// </remarks>
[DxfName(DxfFileToken.EntityAecWall)]
[DxfSubClass(DxfSubclassMarker.AecWall)]
public class Wall : AecEntity, IOrientable
{
	/// <summary>
	/// Base height of the wall.
	/// </summary>
	public double BaseHeight { get; set; }

	/// <summary>
	/// Reference to the BinRecord containing wall-specific data
	/// </summary>
	public AecBinRecord BinRecord { get; internal set; }

	/// <summary>
	/// Handle reference to the AcAecBinRecord object containing the actual wall geometry data.
	/// </summary>
	/// <remarks>
	/// This handle points to a separate object in the DWG file that stores the wall's
	/// properties in a proprietary binary format.
	/// </remarks>
	public ulong BinRecordHandle { get; set; }

	/// <summary>
	/// Reference to the cleanup group that manages wall intersections and cleanup operations.
	/// </summary>
	public AecCleanupGroup CleanupGroup { get; internal set; }

	/// <summary>
	/// Handle reference to the cleanup group object.
	/// </summary>
	public ulong CleanupGroupHandle { get; set; }

	/// <summary>
	/// End position of the wall.
	/// </summary>
	public XYZ EndPoint { get; set; } = XYZ.Zero;

	public double Height { get; set; }

	/// <summary>
	/// Wall justification (e.g., Baseline, Left, Center, Right).
	/// </summary>
	public WallJustification Justification { get; set; }

	/// <summary>
	/// Length of the wall.
	/// </summary>
	public double Length { get; set; }

	/// <inheritdoc/>
	public XYZ Normal { get; set; } = XYZ.AxisZ;

	/// <inheritdoc/>
	public override string ObjectName => DxfFileToken.EntityAecWall;

	public override ObjectType ObjectType { get { return ObjectType.UNLISTED; } }

	public byte[] RawData { get; internal set; }

	/// <summary>
	/// Start position of the wall (insertion point).
	/// </summary>
	public XYZ StartPoint { get; set; } = XYZ.Zero;

	/// <summary>
	/// Reference to the wall style that defines this wall's appearance and properties.
	/// </summary>
	public AecWallStyle Style { get; internal set; }

	/// <inheritdoc/>
	public override string SubclassMarker => DxfSubclassMarker.AecWall;

	/// <summary>
	/// AEC object version number (if available from DWG).
	/// </summary>
	public int Version { get; set; }

	/// <summary>
	/// Wall width (thickness).
	/// </summary>
	public double Width { get; set; }


	public override void ApplyTransform(Transform transform)
	{
		XYZ rawDirection = this.EndPoint - this.StartPoint;
		XYZ direction = rawDirection.GetLength() == 0 ? XYZ.AxisX : rawDirection.Normalize();
		XYZ up = this.Normal.GetLength() == 0 ? XYZ.AxisZ : this.Normal.Normalize();
		XYZ lateral = XYZ.Cross(up, direction);
		lateral = lateral.GetLength() == 0 ? XYZ.AxisY : lateral.Normalize();
		XYZ origin = transform.ApplyTransform(XYZ.Zero);
		XYZ vector(XYZ value) => transform.ApplyTransform(value) - origin;
		XYZ transformedUp = vector(up);
		this.StartPoint = transform.ApplyTransform(this.StartPoint);
		this.EndPoint = transform.ApplyTransform(this.EndPoint);
		this.Length = rawDirection.GetLength() > 0 ? (this.EndPoint - this.StartPoint).GetLength() : this.Length * vector(direction).GetLength();
		this.Width *= vector(lateral).GetLength();
		this.Height *= transformedUp.GetLength();
		this.BaseHeight *= transformedUp.GetLength();
		this.Normal = transformedUp.GetLength() == 0 ? up : transformedUp.Normalize();
	}

	public override BoundingBox GetBoundingBox()
	{
		XYZ up = this.Normal.GetLength() == 0 ? XYZ.AxisZ : this.Normal.Normalize();
		XYZ rawDirection = this.EndPoint - this.StartPoint;
		XYZ direction = rawDirection.GetLength() == 0 ? XYZ.AxisX : rawDirection.Normalize();
		XYZ end = rawDirection.GetLength() == 0 ? this.StartPoint + direction * this.Length : this.EndPoint;
		XYZ lateral = XYZ.Cross(up, direction);
		lateral = lateral.GetLength() == 0 ? XYZ.AxisY : lateral.Normalize();
		double minimumWidth = -this.Width / 2, maximumWidth = this.Width / 2;
		if (this.Justification == WallJustification.Left) { minimumWidth = 0; maximumWidth = this.Width; }
		else if (this.Justification == WallJustification.Right) { minimumWidth = -this.Width; maximumWidth = 0; }
		var points = new List<XYZ>();
		foreach (var endpoint in new[] { this.StartPoint, end })
			foreach (var width in new[] { minimumWidth, maximumWidth })
				foreach (var height in new[] { this.BaseHeight, this.BaseHeight + this.Height })
					points.Add(endpoint + lateral * width + up * height);
		return BoundingBox.FromPoints(points);
	}
}
