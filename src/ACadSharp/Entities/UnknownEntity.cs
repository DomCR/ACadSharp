using ACadSharp.Classes;
using CSMath;
using System;

namespace ACadSharp.Entities
{
	/// <summary>
	/// Class that holds the basic information for an unknown <see cref="Entity"/>.
	/// </summary>
	/// <remarks>
	/// Unknown entities may appear in the <see cref="CadDocument"/> if the cad file contains proxies or entities not yet supported by ACadSharp.
	/// </remarks>
	public class UnknownEntity : Entity
	{
		/// <inheritdoc/>
		public override ObjectType ObjectType => ObjectType.UNDEFINED;

		/// <inheritdoc/>
		public override string ObjectName
		{
			get
			{
				if (this.DxfClass == null)
				{
					return "UNKNOWN";
				}
				else
				{
					return this.DxfClass.DxfName;
				}
			}
		}

		/// <inheritdoc/>
		public override string SubclassMarker
		{
			get
			{
				if (this.DxfClass == null)
				{
					return DxfSubclassMarker.Entity;
				}
				else
				{
					return this.DxfClass.CppClassName;
				}
			}
		}

		/// <summary>
		/// Dxf class linked to this entity.
		/// </summary>
		public DxfClass DxfClass { get; }

		internal UnknownEntity(DxfClass dxfClass)
		{
			this.DxfClass = dxfClass;
		}

		public override void ApplyTransform(Transform transform)
		{
			throw new NotImplementedException();
		}

		public override void ApplyRotation(double rotation, XYZ axis)
		{
			throw new NotImplementedException();
		}

		public override void ApplyEscalation(XYZ scale)
		{
			throw new NotImplementedException();
		}

		public override void ApplyTranslation(XYZ translation)
		{
			throw new NotImplementedException();
		}

		/// <inheritdoc/>
		/// <remarks>
		/// An Unknown Entity does not have any geometric shape, therfore it's bounding box will be always 0
		/// </remarks>
		public override BoundingBox GetBoundingBox()
		{
			return BoundingBox.Null;
		}
	}
}
