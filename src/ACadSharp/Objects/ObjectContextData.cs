using ACadSharp.Attributes;


namespace ACadSharp.Objects;

[DxfSubClass(DxfSubclassMarker.ObjectContextData)]
public abstract class ObjectContextData : NonGraphicalObject
{
	/// <inheritdoc/>
	public override string SubclassMarker => DxfSubclassMarker.ObjectContextData;

	/// <summary>
	/// Gets or sets the version number of the entity definition.
	/// </summary>
	[DxfCodeValue(70)]
	public short Version { get; set; } = 3;

	//B	-	Has file to extension dictionary (default value is true).
	//[DxfCodeValue()]
	public bool HasFileToExtensionDictionary { get; set; } = true;

	/// <summary>
	/// Gets or sets a value indicating whether this instance uses the default setting.
	/// </summary>
	[DxfCodeValue(290)]
	public bool Default { get; set; } = false;
}
