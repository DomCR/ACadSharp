using ACadSharp.Attributes;

namespace ACadSharp.Objects
{
	/// <summary>
	/// Represents a <see cref="Filter"/> object.
	/// </summary>
	/// <remarks>
	/// Dxf class name <see cref="DxfSubclassMarker.Filter"/>
	/// </remarks>
	[DxfSubClass(DxfSubclassMarker.Filter)]
	public abstract class Filter : NonGraphicalObject
	{
		public const string FilterEntryName = "ACAD_FILTER";

		/// <inheritdoc/>
		public override ObjectType ObjectType => ObjectType.UNLISTED;

		/// <inheritdoc/>
		public override string SubclassMarker => DxfSubclassMarker.Filter;

		/// <inheritdoc/>
		public Filter() : base()
		{
		}

		/// <inheritdoc/>
		public Filter(string name) : base(name)
		{
		}
	}
}