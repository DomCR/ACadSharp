using ACadSharp.Classes;

namespace ACadSharp.Entities
{
	/// <summary>
	/// Class that holds the basic information for an unknown entity
	/// </summary>
	/// <remarks>
	/// Unknown entities may appear in the <see cref="CadDocument"/> if the dwg file contains proxies or entities not yet supported by ACadSharp
	/// </remarks>
	public class UnknownEntity : Entity
	{
		/// <inheritdoc/>
		public override ObjectType ObjectType => ObjectType.UNDEFINED;

		/// <inheritdoc/>
		public override string ObjectName => this.DxfClass.DxfName;

		/// <inheritdoc/>
		public override string SubclassMarker => this.DxfClass.CppClassName;

		/// <summary>
		/// Dxf class linked to this entity
		/// </summary>
		public DxfClass DxfClass { get; }

		internal UnknownEntity(DxfClass dxfClass)
		{
			this.DxfClass = dxfClass;
		}
	}
}
