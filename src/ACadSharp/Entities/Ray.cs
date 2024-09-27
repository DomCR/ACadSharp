using ACadSharp.Attributes;
using CSMath;

namespace ACadSharp.Entities
{
	/// <summary>
	/// Represents a <see cref="Ray"/> entity.
	/// </summary>
	/// <remarks>
	/// Object name <see cref="DxfFileToken.EntityRay"/> <br/>
	/// Dxf class name <see cref="DxfSubclassMarker.Ray"/>
	/// </remarks>
	[DxfName(DxfFileToken.EntityRay)]
	[DxfSubClass(DxfSubclassMarker.Ray)]
	public class Ray : Entity
	{
		/// <inheritdoc/>
		public override ObjectType ObjectType => ObjectType.RAY;

		/// <inheritdoc/>
		public override string ObjectName => DxfFileToken.EntityRay;

		/// <inheritdoc/>
		public override string SubclassMarker => DxfSubclassMarker.Ray;

		/// <summary>
		/// Start point(in WCS).
		/// </summary>
		[DxfCodeValue(10, 20, 30)]
		public XYZ StartPoint { get; set; } = XYZ.Zero;

		/// <summary>
		/// Unit direction vector(in WCS).
		/// </summary>
		[DxfCodeValue(11, 21, 31)]
		public XYZ Direction { get; set; } = XYZ.Zero;

		public override void ApplyTransform(Transform transform)
		{
			throw new System.NotImplementedException();
		}

		/// <inheritdoc/>
		public override BoundingBox GetBoundingBox()
		{
			return BoundingBox.Infinite;
		}

		public override void Rotate(double rotation, XYZ axis)
		{
			throw new System.NotImplementedException();
		}

		public override void Scale(XYZ scale)
		{
			throw new System.NotImplementedException();
		}

		public override void Translate(XYZ translation)
		{
			throw new System.NotImplementedException();
		}
	}
}
