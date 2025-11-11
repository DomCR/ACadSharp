using ACadSharp.Attributes;

namespace ACadSharp.Objects
{
	/// <summary>
	/// Represents a <see cref="DimensionAssociation"/> object.
	/// </summary>
	/// <remarks>
	/// Dxf class name <see cref="DxfSubclassMarker.DimensionAssociation"/>
	/// </remarks>
	[DxfSubClass(DxfSubclassMarker.DimensionAssociation)]
	public abstract class DimensionAssociation : NonGraphicalObject
	{
		/// <inheritdoc/>
		public override ObjectType ObjectType => ObjectType.UNLISTED;

		/// <inheritdoc/>
		public override string SubclassMarker => DxfSubclassMarker.DimensionAssociation;

		/// <inheritdoc/>
		public DimensionAssociation() : base()
		{
		}
	}
}