using ACadSharp.Classes;
using CSMath;
using System;
using System.Collections.Generic;

namespace ACadSharp.Entities.Tangent
{
	/// <summary>
	/// A Tianzheng entity whose class has no reader of its own yet.
	/// </summary>
	/// <remarks>
	/// <para>
	/// It holds what any Tianzheng object holds — the raw class data and the property blocks — and
	/// converts to nothing, because the shape it stands for has not been worked out. That is not the
	/// same as the object being empty: several of these classes store an outline that is still to be
	/// decoded, and the property blocks are readable today.
	/// </para>
	/// <para>
	/// A class is promoted out of this type by giving it a reader, not by changing what it derives
	/// from, so callers that only want to know whether something is Tianzheng do not have to change.
	/// </para>
	/// </remarks>
	public class TchUnlistedEntity : TchEntity
	{
		internal TchUnlistedEntity(DxfClass dxfClass)
			: base(dxfClass)
		{
		}

		/// <inheritdoc/>
		public override IEnumerable<Entity> ToStandardEntities()
		{
			return Array.Empty<Entity>();
		}

		/// <inheritdoc/>
		public override void ApplyTransform(Transform transform)
		{
		}

		/// <inheritdoc/>
		public override BoundingBox GetBoundingBox()
		{
			return BoundingBox.Null;
		}
	}
}
