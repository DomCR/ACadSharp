using ACadSharp.Attributes;
using ACadSharp.Objects;
using CSMath;

namespace ACadSharp.Entities.AecObjects
{
	/// <summary>
	/// Represents an AEC (Architecture, Engineering & Construction) Wall entity.
	/// </summary>
	/// <remarks>
	/// AEC Wall entities can only be stored in DWG files, not in DXF files.
	/// DXF files may contain the class definition but not the actual entity instances.
	/// </remarks>
	[DxfName(DxfFileToken.EntityAecWall)]
	[DxfSubClass(DxfSubclassMarker.AecWall)]
	public class Wall : Entity
	{
		public override ObjectType ObjectType { get { return ObjectType.UNLISTED; } }

		/// <inheritdoc/>
		public override string ObjectName => DxfFileToken.EntityAecWall;

		/// <inheritdoc/>
		public override string SubclassMarker => DxfSubclassMarker.AecWall;

		/// <summary>
		/// AEC object version number (if available from DWG).
		/// </summary>
		public int Version { get; set; }

		/// <summary>
		/// Handle reference to the AcAecBinRecord object containing the actual wall geometry data.
		/// </summary>
		/// <remarks>
		/// This handle points to a separate object in the DWG file that stores the wall's
		/// properties in a proprietary binary format.
		/// </remarks>
		public ulong BinRecordHandle { get; set; }

		/// <summary>
		/// Reference to the BinRecord containing wall-specific data
		/// </summary>
		public AecBinRecord BinRecord { get; internal set; }

		/// <summary>
		/// Handle reference to the wall style object.
		/// </summary>
		public ulong StyleHandle { get; set; }

		/// <summary>
		/// Reference to the wall style that defines this wall's appearance and properties.
		/// </summary>
		public AecWallStyle Style { get; internal set; }

		/// <summary>
		/// Handle reference to the cleanup group object.
		/// </summary>
		public ulong CleanupGroupHandle { get; set; }

		/// <summary>
		/// Reference to the cleanup group that manages wall intersections and cleanup operations.
		/// </summary>
		public AecCleanupGroup CleanupGroup { get; internal set; }

		/// <summary>
		/// Wall width (thickness).
		/// </summary>
		public double Width { get; set; }

		/// <summary>
		/// Base height of the wall.
		/// </summary>
		public double BaseHeight { get; set; }

		/// <summary>
		/// Length of the wall.
		/// </summary>
		public double Length { get; set; }

		/// <summary>
		/// Wall justification (e.g., Baseline, Left, Center, Right).
		/// </summary>
		public WallJustification Justification { get; set; }

		/// <summary>
		/// Start position of the wall (insertion point).
		/// </summary>
		public XYZ StartPoint { get; set; } = XYZ.Zero;

		/// <summary>
		/// End position of the wall.
		/// </summary>
		public XYZ EndPoint { get; set; } = XYZ.Zero;
		public XYZ Normal { get; set; }
		public double Height { get; set; }
		public byte[] RawData { get; internal set; }

		public override void ApplyTransform(Transform transform)
		{
			// Transform the start and end points
			StartPoint = transform.ApplyTransform(StartPoint);
			EndPoint = transform.ApplyTransform(EndPoint);
		}

		public override BoundingBox GetBoundingBox()
		{
			throw new System.NotImplementedException();
		}
	}
}
