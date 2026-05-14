using ACadSharp.Attributes;
using CSMath;

namespace ACadSharp.Entities
{
	/// <summary>
	/// Represents a <see cref="XLine"/> entity.
	/// </summary>
	/// <remarks>
	/// Object name <see cref="DxfFileToken.EntityXline"/> <br/>
	/// Dxf class name <see cref="DxfSubclassMarker.XLine"/>
	/// </remarks>
	[DxfName(DxfFileToken.EntityXline)]
	[DxfSubClass(DxfSubclassMarker.XLine)]
	public class XLine : Entity
	{
		/// <summary>
		/// Unit direction vector(in WCS).
		/// </summary>
		[DxfCodeValue(11, 21, 31)]
		public XYZ Direction { get; set; }

		/// <summary>
		/// First point(in WCS).
		/// </summary>
		[DxfCodeValue(10, 20, 30)]
		public XYZ FirstPoint { get; set; }

		/// <inheritdoc/>
		public override string ObjectName => DxfFileToken.EntityXline;

		/// <inheritdoc/>
		public override ObjectType ObjectType => ObjectType.XLINE;

		/// <inheritdoc/>
		public override string SubclassMarker => DxfSubclassMarker.XLine;

		/// <inheritdoc/>
		public override void ApplyTransform(Transform transform)
		{
			this.FirstPoint = transform.ApplyTransform(this.FirstPoint);
			this.Direction = transform.ApplyRotation(this.Direction);
		}

		/// <inheritdoc/>
		public override BoundingBox GetBoundingBox()
		{
			return BoundingBox.Infinite;
		}
	}
}