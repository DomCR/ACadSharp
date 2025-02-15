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
		/// <inheritdoc/>
		public override ObjectType ObjectType => ObjectType.XLINE;

		/// <inheritdoc/>
		public override string ObjectName => DxfFileToken.EntityXline;

		/// <inheritdoc/>
		public override string SubclassMarker => DxfSubclassMarker.XLine;

		/// <summary>
		/// First point(in WCS).
		/// </summary>
		[DxfCodeValue(10, 20, 30)]
		public XYZ FirstPoint { get; set; }

		/// <summary>
		/// Unit direction vector(in WCS).
		/// </summary>
		[DxfCodeValue(11, 21, 31)]
		public XYZ Direction { get; set; }

		public override void ApplyTransform(Transform transform)
		{
			throw new System.NotImplementedException();
		}

		/// <inheritdoc/>
		public override BoundingBox GetBoundingBox()
		{
			return BoundingBox.Infinite;
		}

		public override void ApplyRotation(double rotation, XYZ axis)
		{
			throw new System.NotImplementedException();
		}

		public override void ApplyEscalation(XYZ scale)
		{
			throw new System.NotImplementedException();
		}

		public override void ApplyTranslation(XYZ translation)
		{
			throw new System.NotImplementedException();
		}
	}
}
