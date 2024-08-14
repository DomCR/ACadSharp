using ACadSharp.Attributes;

namespace ACadSharp.Objects
{
	/// <summary>
	/// Common base class for all underlay definitions, like <see cref="PdfUnderlayDefinition" />.
	/// </summary>
	[DxfSubClass(null, true)]
	public abstract class UnderlayDefinition : NonGraphicalObject
	{
		/// <inheritdoc/>
		public override ObjectType ObjectType => ObjectType.UNLISTED;

		/// <inheritdoc/>
		public override string SubclassMarker => DxfSubclassMarker.UnderlayDefinition;

		/// <summary>
		/// Gets or sets the underlay file.
		/// </summary>
		/// <remarks>
		/// The file extension must match the underlay type.
		/// </remarks>
		[DxfCodeValue(1)]
		public string File { get; set; }
	}
}
