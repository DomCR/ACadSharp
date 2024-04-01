using ACadSharp.Classes;

namespace ACadSharp.Entities
{
	public class UnknownEntity : Entity
	{
		/// <inheritdoc/>
		public override ObjectType ObjectType => ObjectType.UNDEFINED;

		/// <inheritdoc/>
		public override string ObjectName => this.DxfClass.DxfName;

		/// <inheritdoc/>
		public override string SubclassMarker => this.DxfClass.CppClassName;

		public DxfClass DxfClass { get; }

		internal UnknownEntity(DxfClass dxfClass)
		{
			this.DxfClass = dxfClass;
		}
	}
}
