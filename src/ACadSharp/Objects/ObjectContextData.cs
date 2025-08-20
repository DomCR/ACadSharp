using ACadSharp.Attributes;


namespace ACadSharp.Objects
{
	[DxfSubClass(DxfSubclassMarker.ObjectContextData)]
	public abstract class ObjectContextData : NonGraphicalObject
	{
		/// <inheritdoc/>
		public override string SubclassMarker => DxfSubclassMarker.ObjectContextData;

		//R2010

		//BS	70	Version (default value is 3).
		[DxfCodeValue(70)]
		public short Version { get; set; } = 3;

		//B	-	Has file to extension dictionary (default value is true).
		//[DxfCodeValue()]
		public bool HasFileToExtensionDictionary { get; set; } = true;

		//B	290	Default flag (default value is false).
		[DxfCodeValue(290)]
		public bool Default { get; set; } = false;
	}
}
