using ACadSharp.Classes;

namespace ACadSharp.Objects
{
	/// <summary>
	/// Class that holds the basic information for an unknown <see cref="NonGraphicalObject"/>.
	/// </summary>
	/// <remarks>
	/// Unknown entities may appear in the <see cref="CadDocument"/> if the cad file contains proxies or objects not yet supported by ACadSharp.
	/// </remarks>
	public class UnknownNonGraphicalObject : NonGraphicalObject
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

		internal UnknownNonGraphicalObject(DxfClass dxfClass)
		{
			this.DxfClass = dxfClass;
		}
	}
}
