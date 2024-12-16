using ACadSharp.Attributes;

namespace ACadSharp.Objects
{
	/// <summary>
	/// Represents a <see cref="CadDictionaryWithDefault"/> object.
	/// </summary>
	/// <remarks>
	/// Object name <see cref="DxfFileToken.ObjectDictionaryWithDefault"/> <br/>
	/// Dxf class name <see cref="DxfSubclassMarker.DictionaryWithDefault"/>
	/// </remarks>
	[DxfName(DxfFileToken.ObjectDictionaryWithDefault)]
	[DxfSubClass(DxfSubclassMarker.DictionaryWithDefault)]
	public class CadDictionaryWithDefault : CadDictionary
	{
		/// <inheritdoc/>
		public override ObjectType ObjectType { get { return ObjectType.UNLISTED; } }

		/// <inheritdoc/>
		public override string ObjectName => DxfFileToken.ObjectDictionaryWithDefault;

		/// <inheritdoc/>
		public override string SubclassMarker => DxfSubclassMarker.DictionaryWithDefault;

		/// <summary>
		/// Default object
		/// </summary>
		/// <remarks>
		/// Currently only used for plot style dictionary's default entry, named “Normal”
		/// </remarks>
		[DxfCodeValue(DxfReferenceType.Handle, 340)]
		public CadObject DefaultEntry { get; set; }
	}
}
