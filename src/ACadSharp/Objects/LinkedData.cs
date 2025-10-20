using ACadSharp.Attributes;

namespace ACadSharp.Objects
{
	/// <summary>
	/// Represents a <see cref="LinkedData"/> object.
	/// </summary>
	/// <remarks>
	/// Dxf class name <see cref="DxfSubclassMarker.LinkedData"/>
	/// </remarks>
	[DxfSubClass(DxfSubclassMarker.LinkedData)]
	public abstract class LinkedData : NonGraphicalObject
	{
		/// <inheritdoc/>
		public override string SubclassMarker => DxfSubclassMarker.LinkedData;

		/// <summary>
		/// Gets or sets the name of the object.
		/// </summary>
		[DxfCodeValue(1)]
		public override string Name { get => base.Name; set => base.Name = value; }

		/// <summary>
		/// Gets or sets the description associated with the object.
		/// </summary>
		[DxfCodeValue(300)]
		public string Description { get; set; }
	}
}
