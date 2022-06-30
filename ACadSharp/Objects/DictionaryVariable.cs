using ACadSharp.Attributes;

namespace ACadSharp.Objects
{
	/// <summary>
	/// Represents a <see cref="DictionaryVariable"/> object.
	/// </summary>
	/// <remarks>
	/// Object name <see cref="DxfFileToken.ObjectDictionaryVar"/> <br/>
	/// Dxf class name <see cref="DxfSubclassMarker.DictionaryVariables"/>
	/// </remarks>
	[DxfName(DxfFileToken.ObjectDictionaryVar)]
	[DxfSubClass(DxfSubclassMarker.DictionaryVariables)]
	public class DictionaryVariable : CadObject
	{
		/// <inheritdoc/>
		public override ObjectType ObjectType => ObjectType.UNLISTED;

		/// <inheritdoc/>
		public override string ObjectName => DxfFileToken.ObjectDictionaryVar;

		/// <summary>
		/// Value of variable
		/// </summary>
		[DxfCodeValue(1)]
		public string Value { get; set; }

		/// <summary>
		/// Object schema number (currently set to 0)
		/// </summary>
		[DxfCodeValue(280)]
		public int ObjectSchemaNumber { get; internal set; }
	}
}
