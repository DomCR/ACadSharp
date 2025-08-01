using ACadSharp.Classes;
using CSMath;
using System;

namespace ACadSharp.Entities
{
	public class ProxyEntity : Entity
	{
		/// <inheritdoc/>
		public override string ObjectName => DxfFileToken.EntityProxyEntity;

		/// <inheritdoc/>
		public override ObjectType ObjectType => ObjectType.ACAD_PROXY_ENTITY;

		public DxfClass DxfClass { get; }

		public override void ApplyTransform(Transform transform)
		{
			throw new NotImplementedException();
		}

		public override BoundingBox GetBoundingBox()
		{
			throw new NotImplementedException();
		}
	}
}
