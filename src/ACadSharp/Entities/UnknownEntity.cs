using ACadSharp.Classes;
using CSMath;

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

		/// <summary>
		/// Gets the part of the object data that lays beyond the common entity data, as it is
		/// stored in the file.
		/// </summary>
		/// <remarks>
		/// ACadSharp has no typed representation for this object, so the bytes written by the
		/// class's own <c>dwgOutFields</c> are kept here verbatim. They are the only source of the
		/// class specific fields, and they are what an application that knows the class reads back.
		/// <para>
		/// Only populated when the reader is configured to keep unknown entities.
		/// </para>
		/// </remarks>
		public byte[] RawData { get; internal set; }

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

		/// <inheritdoc/>
		public override void ApplyTransform(Transform transform)
		{
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
