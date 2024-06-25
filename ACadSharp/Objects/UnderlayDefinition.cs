namespace ACadSharp.Objects
{
	public abstract class UnderlayDefinition : NonGraphicalObject
	{
		/// <inheritdoc/>
		public override ObjectType ObjectType => ObjectType.UNLISTED;

		/// <inheritdoc/>
		public override string SubclassMarker => DxfSubclassMarker.Underlay;

		/// <summary>
		/// Gets or sets the underlay file.
		/// </summary>
		/// <remarks>
		/// The file extension must match the underlay type.
		/// </remarks>
		public string File { get; set; }
	}
}
